using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    //TODO:Make use of services - logging


    public class MongoArticleStore : MongoStorage<MongoArticleData>
    {
        public MongoArticleStore() : base("articles")
        {
            RequiresAudit = true;
            RequiresHistory = true;
            RequiresDrafts = true;
        }

        public override void Configure()
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

        public IArticle GetArticleByGlobalId(string globalId, bool draft = false, int revision = -1)
        {
            throw new NotImplementedException();
        }

        public IArticle GetArticle(string path)
        {
            var pathQuery = PathEQ(path);
            var articles = GetCollection();

            var article = articles.Find(pathQuery).FirstOrDefault();

            if (article == null) throw new MissingArticleException("Cannot find article at articlePath://" + path);

            return MongoArticle.Create(article);
        }

        public IArticleContent GetArticleContent(string path, ArticleContentFormat format, bool draft = false,
            int revision = -1)
        {
            throw new NotImplementedException();
        }

        public int BatchCreateArticle(ArticleBatchCreate[] articles)
        {
            throw new NotImplementedException();
        }

        public IArticle GetDraft(string path, string author, int revision)
        {
            var pathQuery = PathEQ(path);
            var revisionQuery = RevisionEQ(revision);
            var revisedByQuery = RevisedByEQ(author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);
            var drafts = GetDraftsCollection();
            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();
            if (draft == null)
            {
                var message = "Cannot find draft article for " + pathRevisionAndAuthorQuery;
                Error("articles", message);
                throw new MissingDraftException(message);
            }
            return MongoArticle.Create(draft);
        }

        public void PublishDraft(string path, int revision, string author, string publisher = null)
        {
            var pathQuery = PathEQ(path);
            var revisionQuery = RevisionEQ(revision);
            var revisedByQuery = RevisedByEQ(author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);

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
                article.Content = draft.Content;
                article.IsAllowedChildren = draft.IsAllowedChildren;
                article.IsIndexed = draft.IsIndexed;
                article.Keywords = draft.Keywords;

                articles.Save(article);
                history.Insert(History(article));
                drafts.Remove(pathRevisionAndAuthorQuery);
                Audit(AuditAction.Publish, article.Path, article.Revision, publisher ?? author);


                Info("articles", "Published draft article at articlePath://" + article.Path + ", article://" + article.GlobalId + " to revision " + article.Revision);
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

                if (string.IsNullOrEmpty(draft.ParentArticlePath))
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
                    var parentArticleQuery = Query.And(Query.EQ("Path", draft.ParentArticlePath),
                        Query.EQ("IsAllowedChildren", true));
                    var hasParentArticle = articles.Find(parentArticleQuery).Any();

                    if (!hasParentArticle)
                    {
                        var message = "There is no article at the articlePath://" +
                                      draft.ParentArticlePath +
                                      " or it is not allowed children";
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
                    Content = draft.Content,
                    IsAllowedChildren = draft.IsAllowedChildren,
                    IsIndexed = draft.IsIndexed,
                    Keywords = draft.Keywords
                };

                articles.Insert(article);
                history.Insert(article);
                drafts.Remove(pathRevisionAndAuthorQuery);
                Audit(AuditAction.Publish, article.Path, article.Revision, publisher ?? author);

                Info("articles", "Published draft article at articlePath://" + article.Path + ", article://" +
                                 article.GlobalId + " to revision " + article.Revision);
            }
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
                Content = article.Content,
                Keywords = article.Keywords,
                IsAllowedChildren = article.IsAllowedChildren,
                IsIndexed = article.IsIndexed,
                RevisedBy = article.RevisedBy,
                Revision = article.Revision,
                RevisedOn = article.RevisedOn,
                Title = article.Title
            };
        }

        public IArticle CreateDraftArticle(string globalId, string parentArticlePath, string path, string title,
            string markdown, bool isIndexed, bool isAllowedChildren, string author, string[] keywords,
            string generator = null, int revision = 1)
        {
            var collection = GetDraftsCollection();

            var articleData = CreateArticleData(globalId, parentArticlePath, path, title, markdown, isIndexed,
                isAllowedChildren, author, keywords, generator ?? author, revision);

            collection.Insert(articleData);

            Audit(AuditAction.Create, path, revision, author);

            var article = MongoArticle.Create(articleData);

            Info("articles", "Created draft article at articlePath://" + article.Path + ", article://" +
                             article.GlobalId + ", revision " + revision);

            return article;
        }

        public IArticle CreateDraftArticle(string path, string author)
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
            var contentData = article.Content.FirstOrDefault(c => c.Format == ArticleContentFormat.Markdown);

            var newDraftRevision = article.Revision + 1;

            return CreateDraftArticle(article.GlobalId, article.ParentArticlePath, article.Path, article.Title,
                contentData == null ? "" : contentData.Content, article.IsIndexed, article.IsAllowedChildren, author,
                article.Keywords.ToArray(), author, newDraftRevision);
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
                IsIndexed = isIndexed,
                IsAllowedChildren = isAllowedChildren,
                RevisedBy = author,
                RevisedOn = DateTime.UtcNow,
                Revision = revision,
                Keywords = new List<string>(keywords),
                Content = new List<MongoArticleContentData>
                {
                    new MongoArticleContentData
                    {
                        Format = ArticleContentFormat.Markdown,
                        Content = markdown,
                        GeneratedBy = generator,
                        GeneratedOn = DateTime.UtcNow
                    }
                }
            };
            return articleData;
        }

        private void Audit(AuditAction action, string path, int revision, string actionedBy)
        {
            StoreAudit("articles", action, path, actionedBy, revision);
        }

        public bool ArticleHasChildren(string path)
        {
            var parentPathQuery = Query.EQ("ParentArticlePath", path);
            // do we have children?
            if (GetCollection().Find(parentPathQuery).Any())
            {
                return true;
            }
            return false;
        }

        public string DeleteArticle(string path, string author)
        {
            // move between collections
            var pathQuery = Query.EQ("Path", path);
            var collection = GetCollection();
            var trashCollection = GetTrashCollection();
            var historyCollection = GetHistoryCollection();
            var draftCollection = GetDraftsCollection();

            // do we have children?
            if (ArticleHasChildren(path))
            {
                // if so we can't delete
                var message = "Article at articlePath://" + path +
                              " cannot be deleted as it has child articles";
                Error("articles", message);
                throw new InvalidArticleStateException(message);
            }

            var articleHistory = historyCollection.Find(pathQuery);
            var drafts = draftCollection.Find(pathQuery);
            var trashData = new MongoArticleTrashData
            {
                ArticleHistory = new List<MongoArticleData>(articleHistory),
                Drafts = new List<MongoArticleData>(drafts),
                Path = path + "::" + Guid.NewGuid(),
                TrashedBy = author,
                TrashedOn = DateTime.UtcNow
            };

            trashCollection.Insert(trashData);

            var lastRevision = trashData.ArticleHistory.Any() ? trashData.ArticleHistory.Max(ah => ah.Revision) : 1;

            collection.Remove(pathQuery);
            historyCollection.Remove(pathQuery);
            draftCollection.Remove(pathQuery);
            Audit(AuditAction.Delete, path, lastRevision, author);

            Info("articles", "Trashed article at articlePath://" + path + ", " + trashData.ArticleHistory.Count +
                             " revisions");

            return trashData.Path;
        }

        private MongoCollection<MongoArticleTrashData> GetTrashCollection()
        {
            return GetCollection<MongoArticleTrashData>("trash");
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

            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();

            if (draft == null)
            {
                var message = "Cannot find draft article for " + pathRevisionAndAuthorQuery;
                Error("articles", message);
                throw new MissingDraftException(message);
            }

            draft.Title = title;
            var contentData = draft.Content.FirstOrDefault(c => c.Format == ArticleContentFormat.Markdown);

            if (contentData == null)
            {
                contentData = new MongoArticleContentData
                {
                    Format = ArticleContentFormat.Markdown
                };
                draft.Content.Add(contentData);
                Warn("articles", "Adding missing content data for " + draft.Id);
            }

            contentData.Content = markdown;
            contentData.GeneratedOn = DateTime.UtcNow;
            contentData.GeneratedBy = author;

            draft.IsIndexed = isIndexed;
            draft.IsAllowedChildren = isAllowedChildren;
            draft.Keywords = new List<string>(keywords);
            drafts.Save(draft);

            //TODO:Save draft in history if configured so (once sys config is in place)

            Audit(AuditAction.Revise, path, revision, author);

            Info("articles", "Revised draft article at articlePath://" + path + ", revision " + draft.Revision);

            return MongoArticle.Create(draft);
        }
    }

    public class ArticleBatchCreate
    {
    }
}