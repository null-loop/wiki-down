using wiki_down.core.storage;

namespace wiki_down.core
{
    public interface IArticleHistoryPage
    {
        string GlobalId { get; }

        int Page { get; }

        int PageSize { get; }

        IExtendedArticleMetaData[] Results { get; }
    }
}