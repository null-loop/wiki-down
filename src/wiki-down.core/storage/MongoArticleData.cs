using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoArticleData : IMongoData
    {
        public ObjectId Id { get; set; }

        public string GlobalId { get; set; }

        public string ParentArticlePath { get; set; }

        public string Path { get; set; }

        public int Revision { get; set; }

        public string Title { get; set; }

        public DateTime RevisedOn { get; set; }

        public string RevisedBy { get; set; }

        public bool ShowInIndex { get; set; }

        public bool IsAllowedChildren { get; set; }

        public List<string> Keywords { get; set; }

        public string Markdown { get; set; }
    }
}