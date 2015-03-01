namespace wiki_down.core
{
    public interface IArticleMetaDataService
    {
        IArticleHistoryPage GetHistoryPageByGlobalId(string globalId, int page, int pageSize);
        IArticleExtendedMetaData GetCompleteMetaDataByGlobalId(string globalId);
        IArticleNavigationStructure GetNavigationStructureByGlobalId(string globalId);
        IArticleStatistics GetStatisticsDataByGlobalId(string globalId);
    }
}