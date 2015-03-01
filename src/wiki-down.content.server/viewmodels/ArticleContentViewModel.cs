using System;
using System.Collections.Generic;
using wiki_down.core;

namespace wiki_down.content.server.viewmodels
{
    public class ArticleContentViewModel
    {
        public string Content { get; set; }
        public ArticleContentFormat Format { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string Path { get; set; }
        public string GlobalId { get; set; }
        public string Title { get; set; }
        public int Revision { get; set; }
        public string RevisedBy { get; set; }
        public DateTime RevisedOn { get; set; }
        public List<string> Keywords { get; set; }
        public string ParentArticlePath { get; set; }
    }
}