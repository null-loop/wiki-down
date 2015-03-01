using System;

namespace wiki_down.core.storage
{
    public class MongoArticleMetaDataStore : MongoStorage, IArticleMetaDataService
    {
        public MongoArticleMetaDataStore() : base("articles")
        {
        }

        public IArticleHistoryPage GetHistoryPageByGlobalId(string globalId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public IArticleExtendedMetaData GetCompleteMetaDataByGlobalId(string globalId)
        {
            throw new NotImplementedException();
        }

        public IArticleNavigationStructure GetNavigationStructureByGlobalId(string globalId)
        {
            throw new NotImplementedException();
        }

        public IArticleStatistics GetStatisticsDataByGlobalId(string globalId)
        {
            throw new NotImplementedException();
        }
    }
}