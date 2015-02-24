using System;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoGeneratedArticleContentData : IMongoData
    {
        public ObjectId Id { get; set; }

        public string ArticlePath { get; set; }

        public ArticleContentFormat Format { get; set; }

        public string GeneratedBy { get; set; }

        public DateTime GeneratedOn { get; set; }

        public string Content { get; set; }
    }
}