using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace wiki_down.core.storage
{

    public class MongoArticleData
    {
        public ObjectId Id { get; set; }

        public string GlobalId { get; set; }

        public string ParentArticlePath { get; set; }

        public string Path { get; set; }

        public int Revision { get; set; }

        public DateTime RevisedOn { get; set; }

        public string RevisedBy { get; set; }

        public string Title { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsAllowedChildren { get; set; }

        public List<string> Keywords { get; set; }

        public List<MongoArticleContentData> Content { get; set; }
    }

    public class MongoArticleTrashData
    {
        public string GlobalId { get; set; }

        public string Path { get; set; }

        public DateTime TrashedOn { get; set; }

        public string TrashedBy { get; set; }

        public List<MongoArticleData> ArticleHistory { get; set; }
    }
}