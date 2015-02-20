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

        public IArticle GetArticleByGlobalId(string globalId, bool draft=false, int revision = -1)
        {
            throw new NotImplementedException();
        }
        
        public IArticle GetArticleByPath(string path, bool draft=false, int revision = -1)
        {
            throw new NotImplementedException();
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

        public IArticle CreateArticle(string globalId, string parentArticlePath, string path, string title, string markdown,
            bool isIndexed, bool isDraft, bool isAllowedChildren, string author, string[] keywords, string generator = "Author")
        {
            //TODO:isDraft decides which collection to add to?

            // check if there's already an article at that globalId... or path
            var collection = isDraft ? GetArticleCollection("drafts") : GetArticleCollection();
            var historyCollection = GetArticleCollection("history");

            var articleData = new MongoArticleData()
            {
                GlobalId = globalId,
                ParentArticlePath = parentArticlePath,
                Path = path,
                Title = title,
                IsIndexed = isIndexed,
                IsAllowedChildren = isAllowedChildren,
                RevisedBy = author,
                RevisedOn = DateTime.UtcNow,
                Revision = 1,
                Keywords = new List<string>(keywords),
                Content = new List<MongoArticleContentData>()
                {
                    new MongoArticleContentData()
                    {
                        Format = ArticleContentFormat.Markdown,
                        Content = markdown,
                        GeneratedBy = generator,
                        GeneratedOn = DateTime.UtcNow
                    }
                }
            };


            // since it's a new article we just insert the same data into current and history
            collection.Insert(articleData);
            historyCollection.Insert(articleData);

            Audit(MongoArticleAction.Create, path, 1, author);

            var article = MongoArticle.Create(articleData);

            Console.WriteLine("Created article at articlePath://" + article.Path + ", article://" + article.GlobalId);

            return article;
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
            var audit = new MongoArticleAuditData()
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

        public string DeleteArticle(string path, string author)
        {
            // move between collections
            var pathQuery = Query.EQ("Path", path);
            var collection = GetArticleCollection();
            var trashCollection = GetArticleTrashCollection();
            var historyCollection = GetArticleCollection("history");
            var draftCollection = GetArticleCollection("drafts");
            var articleHistory = historyCollection.Find(pathQuery);
            var drafts = draftCollection.Find(pathQuery);
            var trashData = new MongoArticleTrashData()
            {
                ArticleHistory = new List<MongoArticleData>(articleHistory),
                Drafts = new List<MongoArticleData>(drafts),
                Path = path + "::" + Guid.NewGuid(),
                TrashedBy = author,
                TrashedOn = DateTime.UtcNow
            };

            trashCollection.Insert(trashData);

            var lastRevision = trashData.ArticleHistory.Max(ah => ah.Revision);

            collection.Remove(pathQuery);
            historyCollection.Remove(pathQuery);
            draftCollection.Remove(pathQuery);
            Audit(MongoArticleAction.Delete, path, lastRevision, author);

            Console.WriteLine("Trashed article at articlePath://" + path + ", " + trashData.ArticleHistory.Count + " revisions");

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


        public IArticle ReviseArticle(string path, string title, string markdown, bool isIndexed, bool isDraft,
            bool isAllowedChildren, string author, int expectedCurrentRevision)
        {
            throw new NotImplementedException();
        }

        public static void Init(string connectionString, string dbName)
        {
            BsonClassMap.RegisterClassMap<MongoArticleData>();
            BsonClassMap.RegisterClassMap<MongoArticleContentData>();
            BsonClassMap.RegisterClassMap<MongoArticleAuditData>();
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
    }

    public class ArticleBatchCreate
    {
        //TODO:Flesh this out
    }
}
