using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoArticleTrashData : IMongoData
    {
        public string GlobalId { get; set; }

        public string Path { get; set; }

        public DateTime TrashedOn { get; set; }

        public string TrashedBy { get; set; }

        public List<MongoArticleData> ArticleHistory { get; set; }
        public List<MongoArticleData> Drafts { get; set; }

        public ObjectId Id { get; set; }
    }
}