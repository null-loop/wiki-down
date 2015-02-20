using System;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoArticleContentData
    {
        public ArticleContentFormat Format { get; set; }

        public string GeneratedBy { get; set; }

        public DateTime GeneratedOn { get; set; }

        public string Content { get; set; }
    }

    public class MongoArticleAuditData
    {
        public ObjectId Id { get; set; }

        public MongoArticleAction Action { get; set; }

        public string Path { get; set; }

        public string ActionedBy { get; set; }

        public DateTime ActionedOn { get; set; }
        public int Revision { get; set; }
    }

    public enum MongoArticleAction
    {
        Create = 0,
        Revise = 1,
        Delete = 2,
        Recover = 3,
        Publish = 4
    }
}