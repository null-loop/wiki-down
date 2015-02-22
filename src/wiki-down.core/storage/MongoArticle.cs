using System;
using System.Collections.Generic;
using System.Linq;

namespace wiki_down.core.storage
{
    public class MongoArticle : IArticle
    {

        private MongoArticle(MongoArticleData data)
        {
            _data = data;
        }

        internal static MongoArticle Create(MongoArticleData data)
        {
            return new MongoArticle(data);
        }

        public string GlobalId
        {
            get { return _data.GlobalId; }
        }

        public string ParentArticlePath
        {
            get { return _data.ParentArticlePath; }
        }

        public string Path
        {
            get { return _data.Path; }
        }

        public int Revision
        {
            get { return _data.Revision; }
        }

        public string RevisedBy
        {
            get { return _data.RevisedBy; }
        }

        public DateTime RevisedOn
        {
            get { return _data.RevisedOn; }
        }

        public string Title
        {
            get { return _data.Title; }
            set { _data.Title = value; }
        }

        public bool ShowInIndex
        {
            get { return _data.ShowInIndex; }
            set { _data.ShowInIndex = value; }
        }

        public bool IsAllowedChildren
        {
            get { return _data.IsAllowedChildren; }
            set { _data.IsAllowedChildren = value; }
        }

        private readonly MongoArticleData _data;

        private Dictionary<ArticleContentFormat,IArticleContent> _contentMap = new Dictionary<ArticleContentFormat,IArticleContent>();

        public List<string> Keywords
        {
            get { return _data.Keywords; }
        }

        public IArticleContent Markdown
        {
            get
            {
                if (!_contentMap.ContainsKey(ArticleContentFormat.Markdown))
                {
                    var markdownData = GetContent(ArticleContentFormat.Markdown);
                    if (markdownData==null) throw new InvalidOperationException("Missing markdown data!");
                    _contentMap[ArticleContentFormat.Markdown] = MongoArticleContent.Create(markdownData);
                }
                return _contentMap[ArticleContentFormat.Markdown];
            }
        }

        private MongoArticleContentData GetContent(ArticleContentFormat articleContentFormat)
        {
            return _data.Content.FirstOrDefault(f => f.Format == articleContentFormat);
        }

        public string GetArticleContent(ArticleContentFormat format)
        {
            if (format == ArticleContentFormat.Markdown) return Markdown.Content;
            var data = GetContent(format);
            return data == null ? null : data.Content;
        }
    }
}