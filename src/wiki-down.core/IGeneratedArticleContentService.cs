namespace wiki_down.core
{
    public interface IGeneratedArticleContentService
    {
        void RegenerateArticleContent(string path, string globalId);

        IArticleContent GetGeneratedArticleContentByPath(string path, ArticleContentFormat format);
        IArticleContent GetGeneratedArticleContentByGlobalId(string path, ArticleContentFormat format);
    }
}