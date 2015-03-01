namespace wiki_down.core
{
    public interface IArticleMetaDataService
    {
        IArticleHistoryPage GetHistoryPageByGlobalId(string globalId, int page, int pageSize);
        ICompleteArticleMetaData GetCompleteMetaDataByGlobalId(string globalId);
        IArticleNavigationStructure GetNavigationStructureByGlobalId(string globalId);
        IArticleStatistics GetStatisticsByGlobalId(string globalId);
    }
}