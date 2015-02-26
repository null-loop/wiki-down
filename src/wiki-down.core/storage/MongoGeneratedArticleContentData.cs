using System;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoGeneratedArticleContentData : IMongoData, IArticleContent
    {
        public ObjectId Id { get; set; }

        public string Path { get; set; }

        public string GlobalId { get; set; }

        public ArticleContentFormat Format { get; set; }

        public string GeneratedBy { get; set; }

        public DateTime GeneratedOn { get; set; }

        public string Content { get; set; }
    }
}