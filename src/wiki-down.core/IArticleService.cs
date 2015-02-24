namespace wiki_down.core
{
    public interface IArticleService
    {
        IArticle GetArticleByPath(string path);

        IArticle GetArticleByGlobalId(string globalId);

        bool HasArticleByGlobalId(string globalId);

        bool HasArticleByPath(string path);

        bool ArticleHasChildren(string path);

        string TrashArticle(string path, string author);

        void RecoverArticle(string path, string author);

        string GetArticleContentByPath(string path, ArticleContentFormat format);

        string GetArticleContentByGlobalId(string globalId, ArticleContentFormat format);

        IArticle GetDraft(string path, string author, int revision);

        IArticle CreateDraft(string globalId, string parentArticlePath, string path, string title,
            string markdown, bool isIndexed, bool isAllowedChildren, string author, string[] keywords,
            string generator = null, int revision = 1);

        IArticle CreateDraftFromArticle(string path, string author);

        IArticle ReviseDraft(string path, string title, string markdown, bool isIndexed,
            bool isAllowedChildren, string[] keywords, string author, int revision);
    }

    public interface IGeneratedArticleContentService
    {
        void RegenerateArticleContent(string path);

        IArticleContent GetGeneratedArticleCotent(string path, ArticleContentFormat format);
    }
}