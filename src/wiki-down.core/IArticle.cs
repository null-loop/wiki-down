using System.Collections.Generic;
using wiki_down.core.storage;

namespace wiki_down.core
{
    public interface IArticle : IExistsInArticleTree, IRevisable, ITitlable, IIndexable
    {
        List<string> Keywords { get; }

        IArticleContent Markdown { get; }

        string GetArticleContent(ArticleContentFormat format);
    }

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


    public interface ISystemAuditService
    {
        void Audit(string area, AuditAction action, string path, string actionedBy, int revision);
    }

    public interface ISystemStatisticsService
    {
        
    }

    public interface ISystemLoggingService
    {
        void Debug(string system, string area, string type, string message);
        void Info(string system, string area, string type, string message);
        void Warn(string system, string area, string type, string message);
        void Error(string system, string area, string type, string message);
        void Fatal(string system, string area, string type, string message);
    }

    public interface ISystemConfigurationService
    {
        
    }
}