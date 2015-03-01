using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoArticleStore : MongoStorage<MongoArticleData>, IArticleService
    {
        public MongoArticleStore() : base("articles")
        {
            RequiresAudit = true;
            RequiresHistory = true;
            RequiresDrafts = true;
        }

        public IArticle GetArticleByGlobalId(string globalId)
        {
            var globalIdQuery = Query.EQ("GlobalId", globalId);
            var articles = GetCollection();

            var article = articles.Find(globalIdQuery).FirstOrDefault();

            if (article == null) throw new MissingArticleException("Cannot find article at article://" + globalId);

            return article;
        }

        public IArticle GetArticleByPath(string path)
        {
            var pathQuery = PathEQ(path);
            var articles = GetCollection();

            var article = articles.Find(pathQuery).FirstOrDefault();

            if (article == null) throw new MissingArticleException("Cannot find article at articlePath://" + path);

            return article;
        }

        public bool HasArticleByGlobalId(string globalId)
        {
            var globalIdQuery = Query.EQ("GlobalId", globalId);
            var articles = GetCollection();

            return articles.Find(globalIdQuery).Any();
        }

        public bool HasArticleByPath(string path)
        {
            var pathQuery = PathEQ(path);
            var articles = GetCollection();

            return articles.Find(pathQuery).Any();
        }

        public IArticle GetDraft(string path, string author)
        {
            var pathQuery = PathEQ(path);
            var revisedByQuery = RevisedByEQ(author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisedByQuery);
            var drafts = GetDraftsCollection();
            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();
            return draft;
        }

        public IArticle CreateDraft(string globalId, string parentArticlePath, string path, string title,
            string markdown, bool isIndexed, bool isAllowedChildren, string author, string[] keywords,
            string generator = null, int revision = 1)
        {
            Ids.ValidateGlobalId(globalId);
            Ids.ValidatePath(path);

            var collection = GetDraftsCollection();
            var draftsConfig = SystemConfiguration.GetConfiguration<IDraftArticlesConfiguration>();

            var article = CreateArticleData(globalId, parentArticlePath, path, title, markdown, isIndexed,
                isAllowedChildren, author, keywords, generator ?? author, revision);

            collection.Insert(article);

            if (draftsConfig.SaveHistory)
            {
                var draftsHistory = GetCollection<MongoArticleData>("drafts-history");
                draftsHistory.Insert(History(article));
            }

            Audit(AuditAction.Create, path, revision, author);

            Info("articles", "Created draft article at articlePath://" + article.Path + ", article://" +
                             article.GlobalId + ", revision " + revision);

            return article;
        }

        public IArticle CreateDraftFromArticle(string path, string author)
        {
            // create a draft from an existing published article
            var pathQuery = PathEQ(path);
            var articles = GetCollection();

            var article = articles.Find(pathQuery).FirstOrDefault();

            if (article == null)
            {
                var message = "Cannot find article at articlePath://" + path;
                Error("articles", message);
                throw new MissingArticleException(message);
            }
            var newDraftRevision = article.Revision + 1;

            return CreateDraft(article.GlobalId, article.ParentArticlePath, article.Path, article.Title,
                article.Markdown, article.ShowInIndex, article.IsAllowedChildren, author,
                article.Keywords.ToArray(), author, newDraftRevision);
        }

        public bool ArticleHasChildren(string path)
        {
            var parentPathQuery = Query.EQ("ParentArticlePath", path);
            if (GetCollection().Find(parentPathQuery).Any())
            {
                return true;
            }
            return false;
        }

        public string TrashArticle(string path, string author)
        {
            // move between collections
            var pathQuery = Query.EQ("Path", path);
            var collection = GetCollection();
            var trashCollection = GetTrashCollection();
            var historyCollection = GetHistoryCollection();
            var draftCollection = GetDraftsCollection();
            var generatedCollection = GetCollection<MongoGeneratedArticleContentData>("generated");

            // do we have children?
            if (ArticleHasChildren(path))
            {
                // if so we can't delete
                var message = "Article at articlePath://" + path + " cannot be trashed as it has child articles";
                Error("articles", message);
                throw new InvalidArticleStateException(message);
            }

            var articleHistory = historyCollection.Find(pathQuery);
            var drafts = draftCollection.Find(pathQuery);
            var uniqueness = GetUniqueness();
            var trashData = new MongoArticleTrashData
            {
                ArticleHistory = new List<MongoArticleData>(articleHistory),
                Drafts = new List<MongoArticleData>(drafts),
                Path = path + "::" + uniqueness,
                TrashedBy = author,
                TrashedOn = DateTime.UtcNow
            };

            trashCollection.Insert(trashData);

            var lastRevision = trashData.ArticleHistory.Any() ? trashData.ArticleHistory.Max(ah => ah.Revision) : 1;

            collection.Remove(pathQuery);
            historyCollection.Remove(pathQuery);
            draftCollection.Remove(pathQuery);
            generatedCollection.Remove(Query.EQ("Path", path));
            Audit(AuditAction.Delete, path, lastRevision, author);

            Info("articles", "Trashed article at articlePath://" + path + ", with " + trashData.ArticleHistory.Count + " revisions");

            return trashData.Path;
        }

        public void RecoverArticle(string path, string author)
        {
            var pathQuery = Query.EQ("Path", path);

            var collection = GetCollection();
            var trashCollection = GetTrashCollection();
            var historyCollection = GetHistoryCollection();
            var draftCollection = GetDraftsCollection();

            var trashData = trashCollection.FindOne(pathQuery);

            historyCollection.InsertBatch(trashData.ArticleHistory);
            draftCollection.InsertBatch(trashData.Drafts);

            var maxRevision = trashData.ArticleHistory.Max(ah => ah.Revision);
            var latestRevision = trashData.ArticleHistory.First(ah => ah.Revision == maxRevision);

            collection.Insert(latestRevision);
            trashCollection.Remove(pathQuery);

            GenerateArticleContent(path, latestRevision.GlobalId);

            Audit(AuditAction.Recover, path, maxRevision, author);

            Info("articles", "Recovered article at articlePath://" + path + ", " + trashData.ArticleHistory.Count +
                             " revisions");
        }

        public IArticle ReviseDraft(string path, string title, string markdown, bool isIndexed,
            bool isAllowedChildren, string[] keywords, string author, int revision)
        {
            var pathQuery = Query.EQ("Path", path);
            var revisionQuery = Query.EQ("Revision", revision);
            var revisedByQuery = Query.EQ("RevisedBy", author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);

            var drafts = GetDraftsCollection();
            var draftsConfig = SystemConfiguration.GetConfiguration<IDraftArticlesConfiguration>();

            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();

            if (draft == null)
            {
                var message = "Cannot find draft article for " + pathRevisionAndAuthorQuery;
                Error("articles", message);
                throw new MissingDraftException(message);
            }

            draft.Title = title;
            draft.Markdown = markdown;
            draft.ShowInIndex = isIndexed;
            draft.IsAllowedChildren = isAllowedChildren;
            draft.Keywords = new List<string>(keywords);
            drafts.Save(draft);

            if (draftsConfig.SaveHistory)
            {
                var draftsHistory = GetCollection<MongoArticleData>("drafts-history");
                draftsHistory.Insert(History(draft));
            }


            Audit(AuditAction.Revise, path, revision, author);

            Info("articles", "Revised draft article at articlePath://" + path + ", revision " + draft.Revision);

            return draft;
        }

        public IExtendedArticleMetaData GetDraftMetaData(string home, string author)
        {
            throw new NotImplementedException();
        }

        public override void InitialiseDatabase()
        {
            var collection = GetCollection();
            collection.CreateIndex(new IndexKeysBuilder().Ascending("GlobalId"), IndexOptions.SetUnique(true));
            collection.CreateIndex(new IndexKeysBuilder().Ascending("Path"), IndexOptions.SetUnique(true));
            collection.CreateIndex(new IndexKeysBuilder().Ascending("ParentArticlePath"), IndexOptions.SetUnique(false));
            collection.CreateIndex(new IndexKeysBuilder().Ascending("Title"), IndexOptions.SetUnique(false));

            var history = GetHistoryCollection();
            history.CreateIndex(new IndexKeysBuilder().Ascending("GlobalId"), IndexOptions.SetUnique(false));
            history.CreateIndex(new IndexKeysBuilder().Ascending("Path"), IndexOptions.SetUnique(false));
            history.CreateIndex(new IndexKeysBuilder().Ascending("ParentArticlePath"), IndexOptions.SetUnique(false));

            var trash = GetTrashCollection();
            trash.CreateIndex(new IndexKeysBuilder().Ascending("GlobalId"), IndexOptions.SetUnique(false));
            trash.CreateIndex(new IndexKeysBuilder().Ascending("Path"), IndexOptions.SetUnique(false));

            var drafts = GetDraftsCollection();
            drafts.CreateIndex(new IndexKeysBuilder().Ascending("GlobalId"), IndexOptions.SetUnique(false));
            drafts.CreateIndex(new IndexKeysBuilder().Ascending("Path"), IndexOptions.SetUnique(false));

            var draftsHistory = GetCollection<MongoArticleData>("drafts-history");
            draftsHistory.CreateIndex(new IndexKeysBuilder().Ascending("GlobalId"), IndexOptions.SetUnique(false));
            draftsHistory.CreateIndex(new IndexKeysBuilder().Ascending("Path"), IndexOptions.SetUnique(false));
        }

        public IEnumerable<MongoArticleMetaData> SearchArticleTitlesIndex(string searchTerm, bool ignoreCase = false, int batchSize = 50)
        {
            var titleSearch = TitleMatchesStart(searchTerm);
            var collection = GetCollection<MongoArticleMetaData>();

            return collection.Find(titleSearch).SetSortOrder(SortBy.Ascending("Path")).Take(batchSize);
        }

        private static IMongoQuery TitleMatchesStart(string searchTerm)
        {
            return Query.Matches("Title", "/^" + searchTerm + "/");
        }

        public IEnumerable<MongoArticleMetaData> SearchArticleTitles(string searchTerm, bool ignoreCase = false, int batchSize = 50)
        {
            var titleSearch = TitleMatchesAny(searchTerm);
            var collection = GetCollection<MongoArticleMetaData>();

            return collection.Find(titleSearch).SetSortOrder(SortBy.Ascending("Path")).Take(batchSize);
        }

        private static IMongoQuery TitleMatchesAny(string searchTerm)
        {
            return Query.Matches("Title", "/" + searchTerm + "/");
        }

        private IEnumerable<MongoArticleData> GetArticlesByGlobalIds(params string[] globalIds)
        {
            throw new Exception();
        }

        public void PublishDraft(string path, int revision, string author, string publisher = null)
        {
            var pathQuery = PathEQ(path);
            var revisionQuery = RevisionEQ(revision);
            var revisedByQuery = RevisedByEQ(author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);

            var coreConfig = SystemConfiguration.GetConfiguration<ICoreConfiguration>();

            var articles = GetCollection();
            var drafts = GetDraftsCollection();
            var history = GetHistoryCollection();

            // find the draft
            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();
            if (draft == null)
            {
                var message = "Cannot find draft article for " + pathRevisionAndAuthorQuery;
                Error("articles", message);
                throw new MissingDraftException(message);
            }

            // find the current article (if any)
            var article = articles.Find(pathQuery).FirstOrDefault();
            if (article != null)
            {
                // make sure it's revision is one we're based on
                var expectedRevision = revision - 1;

                if (article.Revision != expectedRevision)
                {
                    var message = "Expected revision of " + expectedRevision + " for exisiting article, has " + article.Revision;
                    Error("articles", message);
                    throw new RevisionMismatchException(message);
                }

                // if we're changing allowed children check we don't have any already
                if (article.IsAllowedChildren && !draft.IsAllowedChildren)
                {
                    var childArticleQuery = Query.EQ("ParentArticlePath", path);
                    if (articles.Find(childArticleQuery).Any())
                    {
                        var message = "Article at articlePath://" + path + " cannot have IsAllowedChildren set to false as it has child articles";
                        Error("articles", message);
                        throw new InvalidArticleStateException(message);
                    }
                }

                article.Revision = revision;
                article.RevisedOn = DateTime.UtcNow;
                article.RevisedBy = author;
                article.Title = draft.Title;
                article.Markdown = draft.Markdown;
                article.IsAllowedChildren = draft.IsAllowedChildren;
                article.ShowInIndex = draft.ShowInIndex;
                article.Keywords = draft.Keywords;

                articles.Save(article);
                history.Insert(History(article));
                drafts.Remove(pathRevisionAndAuthorQuery);
                Audit(AuditAction.Publish, article.Path, article.Revision, publisher ?? author);

                Info("articles", "Published draft article at articlePath://" + article.Path + ", article://" + article.GlobalId + " to revision " + article.Revision);

                GenerateArticleContent(article.Path, article.GlobalId);
            }
            else
            {
                // make sure we're 1
                if (revision != 1)
                {
                    var message = "Expected revision of 1 for new article, received " + revision;
                    Error("articles", message);
                    throw new RevisionMismatchException(message);
                }


                if (string.IsNullOrEmpty(draft.ParentArticlePath) && !coreConfig.AllowMultipleRoots)
                {
                    // make sure we're the FIRST with an empty parent
                    if (articles.Find(Query.EQ("ParentArticlePath", string.Empty)).Any())
                    {
                        var message = "There is already a root article. There can be only one!";
                        Error("articles", message);
                        throw new InvalidArticleStateException(message);
                    }
                }


                // check global id and parent article path
                var hasArticleByGlobalId = articles.Find(Query.EQ("GlobalId", draft.GlobalId)).Any();

                if (hasArticleByGlobalId)
                {
                    var message = "There is already a published article with the global id of " + draft.GlobalId;
                    Error("articles", message);
                    throw new InvalidArticleStateException(
                        message);
                }

                if (!string.IsNullOrEmpty(draft.ParentArticlePath))
                {
                    var parentArticleQuery = Query.And(Query.EQ("Path", draft.ParentArticlePath), Query.EQ("IsAllowedChildren", true));
                    var hasParentArticle = articles.Find(parentArticleQuery).Any();

                    if (!hasParentArticle)
                    {
                        var message = "There is no article at the articlePath://" + draft.ParentArticlePath + " or it is not allowed children";
                        Error("articles", message);
                        throw new InvalidArticleStateException(message);
                    }
                }

                article = new MongoArticleData
                {
                    GlobalId = draft.GlobalId,
                    Path = draft.Path,
                    ParentArticlePath = draft.ParentArticlePath,
                    Revision = revision,
                    RevisedOn = DateTime.UtcNow,
                    RevisedBy = author,
                    Title = draft.Title,
                    Markdown = draft.Markdown,
                    IsAllowedChildren = draft.IsAllowedChildren,
                    ShowInIndex = draft.ShowInIndex,
                    Keywords = draft.Keywords
                };

                articles.Insert(article);
                history.Insert(article);
                drafts.Remove(pathRevisionAndAuthorQuery);
                Audit(AuditAction.Publish, article.Path, article.Revision, publisher ?? author);

                Info("articles", "Published draft article at articlePath://" + article.Path + ", article://" + article.GlobalId + " to revision " + article.Revision);

                GenerateArticleContent(article.Path, article.GlobalId);
            }
        }

        private void GenerateArticleContent(string path, string globalId)
        {
            MongoGeneratedArticleContentStore.RunGenerate(path, globalId, Database);
            Debug("articles-generated", "Regenerating article content for articlePath://" + path + ", article://" + globalId);
        }

        private static IMongoQuery RevisedByEQ(string author)
        {
            return Query.EQ("RevisedBy", author);
        }

        private static IMongoQuery RevisionEQ(int revision)
        {
            return Query.EQ("Revision", revision);
        }

        private static IMongoQuery PathEQ(string path)
        {
            return Query.EQ("Path", path);
        }

        private static MongoArticleData History(MongoArticleData article)
        {
            return new MongoArticleData
            {
                GlobalId = article.GlobalId,
                Path = article.Path,
                ParentArticlePath = article.ParentArticlePath,
                Markdown = article.Markdown,
                Keywords = article.Keywords,
                IsAllowedChildren = article.IsAllowedChildren,
                ShowInIndex = article.ShowInIndex,
                RevisedBy = article.RevisedBy,
                Revision = article.Revision,
                RevisedOn = article.RevisedOn,
                Title = article.Title
            };
        }

        private static MongoArticleData CreateArticleData(string globalId, string parentArticlePath, string path,
            string title,
            string markdown, bool isIndexed, bool isAllowedChildren, string author, string[] keywords, string generator,
            int revision)
        {
            var articleData = new MongoArticleData
            {
                GlobalId = globalId,
                ParentArticlePath = parentArticlePath,
                Path = path,
                Title = title,
                ShowInIndex = isIndexed,
                IsAllowedChildren = isAllowedChildren,
                RevisedBy = author,
                RevisedOn = DateTime.UtcNow,
                Revision = revision,
                Keywords = new List<string>(keywords),
                Markdown = markdown
            };
            return articleData;
        }

        private void Audit(AuditAction action, string path, int revision, string actionedBy)
        {
            StoreAudit("articles", action, path, actionedBy, revision);
        }

        private static string GetUniqueness()
        {
            //TODO:Improve this
            return Guid.NewGuid().ToString();
        }

        private MongoCollection<MongoArticleTrashData> GetTrashCollection()
        {
            return GetCollection<MongoArticleTrashData>("trash");
        }
    }
}