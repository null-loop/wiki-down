using System;
using System.Collections.Generic;

namespace wiki_down.core.storage
{
    public class MongoExtendedArticleMetaData : MongoArticleMetaData
    {
        public DateTime RevisedOn { get; set; }

        public string RevisedBy { get; set; }      

        public bool ShowInIndex { get; set; }

        public bool IsAllowedChildren { get; set; }

        public List<string> Keywords { get; set; }
    }
}