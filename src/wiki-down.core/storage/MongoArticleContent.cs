namespace wiki_down.core.storage
{
    public class MongoArticleContent : IArticleContent
    {
        private readonly MongoArticleContentData _markdownData;

        private MongoArticleContent(MongoArticleContentData markdownData)
        {
            _markdownData = markdownData;
            throw new System.NotImplementedException();
        }

        public static IArticleContent Create(MongoArticleContentData markdownData)
        {
            return new MongoArticleContent(markdownData);
        }

        public ArticleContentFormat Format
        {
            get { return _markdownData.Format; }
        }

        public string GeneratedBy
        {
            get { return _markdownData.GeneratedBy; }
        }

        public string Content
        {
            get { return _markdownData.Content; }
            set { _markdownData.Content = value; }
        }
    }
}