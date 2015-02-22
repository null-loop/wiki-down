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
        IArticle RetrieveArticle(string path);

        IArticle RetrieveArticleDraft(string path, int revision, string author);

        void TrashArticle(string path);

        void RecoverArticle(string path);
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