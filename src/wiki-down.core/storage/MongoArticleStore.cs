using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    public class MongoArticleStore
    {
        private const string ArticlesCollectionName = "articles";
        private const string MediaCollectionName = "media";
        private static string _connectionString;
        private static string _dbName;
        private readonly MongoDatabase _database;

        public MongoArticleStore()
        {
            if (NotInitialised()) throw new InvalidOperationException("Must call init first!");

            _database = GetDatabase();
        }

        private static bool NotInitialised()
        {
            return string.IsNullOrEmpty(_connectionString);
        }

        public IArticle GetArticleByGlobalId(string globalId, bool draft = false, int revision = -1)
        {
            throw new NotImplementedException();
        }

        public IArticle GetArticle(string path)
        {
            var pathQuery = Query.EQ("Path", path);
            var articles = GetArticleCollection();

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
            var pathQuery = Query.EQ("Path", path);
            var revisionQuery = Query.EQ("Revision", revision);
            var revisedByQuery = Query.EQ("RevisedBy", author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);
            var drafts = GetArticleCollection("drafts");
            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();
            if (draft == null) throw new MissingDraftException("Cannot find draft article for " + pathRevisionAndAuthorQuery);
            return MongoArticle.Create(draft);
        }

        public void PublishDraft(string path, int revision, string author, string publisher = null)
        {
            var pathQuery = Query.EQ("Path", path);
            var revisionQuery = Query.EQ("Revision", revision);
            var revisedByQuery = Query.EQ("RevisedBy", author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);
            var articles = GetArticleCollection();
            var drafts = GetArticleCollection("drafts");
            var history = GetArticleCollection("history");

            // find the draft
            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();
            if (draft == null) throw new MissingDraftException("Cannot find draft article for " + pathRevisionAndAuthorQuery);

            // find the current article (if any)
            var article = articles.Find(pathQuery).FirstOrDefault();
            if (article != null)
            {
                // make sure it's revision is one we're based on
                var expectedRevision = revision - 1;

                if (article.Revision!=expectedRevision)
                {
                    throw new RevisionMismatchException("Expected revision of " + expectedRevision + " for exisiting article, has " + article.Revision);
                }

                // if we're changing allowed children check we don't have any already
                if (article.IsAllowedChildren && !draft.IsAllowedChildren)
                {
                    var childArticleQuery = Query.EQ("ParentArticlePath", path);
                    if (articles.Find(childArticleQuery).Any())
                    {
                        throw new InvalidArticleStateException("Article at articlePath://" + path + " cannot have IsAllowedChildren set to false as it has child articles");
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
                Audit(MongoArticleAction.Publish, article.Path, article.Revision, publisher ?? author);

                Console.WriteLine("Published draft article at articlePath://" + article.Path + ", article://" + article.GlobalId + " to revision " + article.Revision);
            }
            else
            {
                // make sure we're 1
                if (revision != 1)
                {
                    throw new RevisionMismatchException("Expected revision of 1 for new article, received " + revision);
                }

                if (string.IsNullOrEmpty(draft.ParentArticlePath))
                {
                    // make sure we're the FIRST with an empty parent
                    if (articles.Find(Query.EQ("ParentArticlePath", string.Empty)).Any())
                    {
                        throw new InvalidArticleStateException("There is already a root article. There can be only one!");
                    }
                }

                // check global id and parent article path
                var hasArticleByGlobalId = articles.Find(Query.EQ("GlobalId", draft.GlobalId)).Any();

                if (hasArticleByGlobalId)
                {
                    throw new InvalidArticleStateException("There is already a published article with the global id of " + draft.GlobalId);
                }

                if (!string.IsNullOrEmpty(draft.ParentArticlePath))
                {
                    var parentArticleQuery = Query.And(Query.EQ("Path", draft.ParentArticlePath), Query.EQ("IsAllowedChildren", true));
                    var hasParentArticle = articles.Find(parentArticleQuery).Any();

                    if (!hasParentArticle)
                    {
                        throw new InvalidArticleStateException("There is no article at the articlePath://" + draft.ParentArticlePath + " or it is not allowed children");
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
                Audit(MongoArticleAction.Publish, article.Path, article.Revision, publisher ?? author);

                Console.WriteLine("Published draft article at articlePath://" + article.Path + ", article://" + article.GlobalId + " to revision " + article.Revision);
            }
        }

        private static MongoArticleData History(MongoArticleData article)
        {
            return new MongoArticleData()
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
            string markdown, bool isIndexed, bool isAllowedChildren, string author, string[] keywords, string generator = null, int revision = 1)
        {
            var collection = GetArticleCollection("drafts");

            var articleData = CreateArticleData(globalId, parentArticlePath, path, title, markdown, isIndexed,
                                                isAllowedChildren, author, keywords, generator ?? author, revision);

            collection.Insert(articleData);

            Audit(MongoArticleAction.Create, path, revision, author);

            var article = MongoArticle.Create(articleData);

            Console.WriteLine("Created draft article at articlePath://" + article.Path + ", article://" + article.GlobalId);

            return article;
        }

        public IArticle CreateDraftArticle(string path, string author)
        {
            // create a draft from an existing published article
            var pathQuery = Query.EQ("Path", path);
            var articles = GetArticleCollection();

            var article = articles.Find(pathQuery).FirstOrDefault();

            if (article == null) throw new MissingArticleException("Cannot find article at articlePath://" + path);
            var contentData = article.Content.FirstOrDefault(c => c.Format == ArticleContentFormat.Markdown);

            var newDraftRevision = article.Revision + 1;

            return CreateDraftArticle(article.GlobalId, article.ParentArticlePath, article.Path, article.Title,
                contentData == null ? "" : contentData.Content, article.IsIndexed, article.IsAllowedChildren, author,
                article.Keywords.ToArray(), author, newDraftRevision);
        }

        private static MongoArticleData CreateArticleData(string globalId, string parentArticlePath, string path, string title,
            string markdown, bool isIndexed, bool isAllowedChildren, string author, string[] keywords, string generator, int revision)
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

        private MongoCollection<MongoArticleData> GetArticleCollection(string subName)
        {
            return _database.GetCollection<MongoArticleData>(ArticlesCollectionName + "-" + subName);
        }

        private MongoCollection<MongoArticleData> GetArticleCollection()
        {
            return _database.GetCollection<MongoArticleData>(ArticlesCollectionName);
        }

        private MongoCollection<MongoArticleAuditData> GetArticleAuditCollection()
        {
            return _database.GetCollection<MongoArticleAuditData>(ArticlesCollectionName + "-audit");
        }

        private void Audit(MongoArticleAction action, string path, int revision, string actionedBy)
        {
            var audit = new MongoArticleAuditData
            {
                Action = action,
                ActionedBy = actionedBy,
                Path = path,
                ActionedOn = DateTime.UtcNow,
                Revision = revision
            };

            var collection = GetArticleAuditCollection();
            collection.Insert(audit);
        }

        public bool ArticleHasChildren(string path)
        {
            var parentPathQuery = Query.EQ("ParentArticlePath", path);
            // do we have children?
            if (GetArticleCollection().Find(parentPathQuery).Any())
            {
                return true;
            }
            return false;
        }

        public string DeleteArticle(string path, string author)
        {
            // move between collections
            var pathQuery = Query.EQ("Path", path);
            var collection = GetArticleCollection();
            var trashCollection = GetArticleTrashCollection();
            var historyCollection = GetArticleCollection("history");
            var draftCollection = GetArticleCollection("drafts");

            // do we have children?
            if (ArticleHasChildren(path))
            {
                // if so we can't delete
                throw new InvalidArticleStateException("Article at articlePath://" + path + " cannot be deleted as it has child articles");
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
            Audit(MongoArticleAction.Delete, path, lastRevision, author);

            Console.WriteLine("Trashed article at articlePath://" + path + ", " + trashData.ArticleHistory.Count +
                              " revisions");

            return trashData.Path;
        }

        private MongoCollection<MongoArticleTrashData> GetArticleTrashCollection()
        {
            return _database.GetCollection<MongoArticleTrashData>(ArticlesCollectionName + "-trash");
        }

        public void RecoverArticle(string path, string author)
        {
            var pathQuery = Query.EQ("Path", path);

            var collection = GetArticleCollection();
            var trashCollection = GetArticleTrashCollection();
            var historyCollection = GetArticleCollection("history");
            var draftCollection = GetArticleCollection("drafts");

            var trashData = trashCollection.FindOne(pathQuery);

            historyCollection.InsertBatch(trashData.ArticleHistory);
            draftCollection.InsertBatch(trashData.Drafts);

            var maxRevision = trashData.ArticleHistory.Max(ah => ah.Revision);
            var latestRevision = trashData.ArticleHistory.First(ah => ah.Revision == maxRevision);

            collection.Insert(latestRevision);
            trashCollection.Remove(pathQuery);

            Audit(MongoArticleAction.Recover, path, maxRevision, author);
        }

        public IArticle ReviseDraft(string path, string title, string markdown, bool isIndexed,
            bool isAllowedChildren, string[] keywords, string author, int revision)
        {
            var pathQuery = Query.EQ("Path", path);
            var revisionQuery = Query.EQ("Revision", revision);
            var revisedByQuery = Query.EQ("RevisedBy", author);
            var pathRevisionAndAuthorQuery = Query.And(pathQuery, revisionQuery, revisedByQuery);

            var drafts = GetArticleCollection("drafts");

            var draft = drafts.Find(pathRevisionAndAuthorQuery).FirstOrDefault();

            if (draft == null) throw new MissingDraftException("Cannot find draft article for " + pathRevisionAndAuthorQuery);

            draft.Title = title;
            var contentData = draft.Content.FirstOrDefault(c => c.Format == ArticleContentFormat.Markdown);

            if (contentData == null)
            {
                contentData = new MongoArticleContentData
                {
                    Format = ArticleContentFormat.Markdown
                };
                draft.Content.Add(contentData);
                Console.WriteLine("Adding missing content data...");
            }

            contentData.Content = markdown;
            contentData.GeneratedOn = DateTime.UtcNow;
            contentData.GeneratedBy = author;

            draft.IsIndexed = isIndexed;
            draft.IsAllowedChildren = isAllowedChildren;
            draft.Keywords = new List<string>(keywords);
            drafts.Save(draft);

            //TODO:Save draft in history if configured so (once sys config is in place)

            Audit(MongoArticleAction.Revise, path, revision, author);

            return MongoArticle.Create(draft);
        }

        public static void Init(string connectionString, string dbName)
        {
            BsonClassMap.RegisterClassMap<MongoArticleData>();
            BsonClassMap.RegisterClassMap<MongoArticleContentData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Format).SetRepresentation(BsonType.String);
            });
            BsonClassMap.RegisterClassMap<MongoArticleAuditData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Action).SetRepresentation(BsonType.String);
            });
            BsonClassMap.RegisterClassMap<MongoArticleTrashData>();

            //TODO:Map the MongoArticleMetaData & MongoExtendedArticleMetaData types!?

            _connectionString = connectionString;
            _dbName = dbName;
        }

        public MongoCollection<BsonDocument> GetArticlesCollection()
        {
            return _database.GetCollection<BsonDocument>(ArticlesCollectionName);
        }

        public MongoCollection<BsonDocument> GetArticlesHistoryCollection()
        {
            return _database.GetCollection<BsonDocument>(ArticlesCollectionName + "-history");
        }

        public MongoCollection<BsonDocument> GetArticlesTrashCollection()
        {
            return _database.GetCollection<BsonDocument>(ArticlesCollectionName + "-trash");
        }

        public MongoCollection<BsonDocument> GetArticlesDraftsCollection()
        {
            return _database.GetCollection<BsonDocument>(ArticlesCollectionName + "-drafts");
        }

        public MongoCollection<BsonDocument> GetArticlesAuditCollection()
        {
            return _database.GetCollection<BsonDocument>(ArticlesCollectionName + "-audit");
        }

        public MongoCollection<BsonDocument> GetMediaCollection()
        {
            return _database.GetCollection<BsonDocument>(MediaCollectionName);
        }

        private static MongoDatabase GetDatabase()
        {
            var client = new MongoClient(_connectionString);
            return client.GetServer().GetDatabase(_dbName);
        }

        public static void EmptyCollections()
        {
            var db = GetDatabase();
            var collections = db.GetCollectionNames().Where(ns => !ns.StartsWith("system."));
            foreach (var collection in collections)
            {
                var collectionObj = db.GetCollection<BsonDocument>(collection);
                collectionObj.RemoveAll();
            }
            Console.WriteLine("All collections empty");
        }
    }

    public class ArticleBatchCreate
    {
        
    }
}