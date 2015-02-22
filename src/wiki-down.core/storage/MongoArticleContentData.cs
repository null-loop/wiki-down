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
}