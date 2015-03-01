using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    public class MongoArticleMetaDataStore : MongoStorage<MongoArticleData>, IArticleMetaDataService
    {
        public MongoArticleMetaDataStore() : base("articles")
        {
        }

        public IArticleHistoryPage GetHistoryPageByGlobalId(string globalId, int page, int pageSize)
        {
            var historyCollection = GetCollection<MongoExtendedArticleMetaData>("history");
            var query = Query.EQ("GlobalId", globalId);
            var skip = page*pageSize;
            var mongoArticleMetaDatas = historyCollection.Find(query).SetSkip(skip).SetBatchSize(pageSize).SetSortOrder(SortBy.Descending("Revision"));
            var results = mongoArticleMetaDatas.OfType<IExtendedArticleMetaData>().ToArray();

            return new MongoArticleHistoryPageData()
            {
                Page = page,
                PageSize = pageSize,
                GlobalId = globalId,
                Results = results
            };
        }

        public ICompleteArticleMetaData GetCompleteMetaDataByGlobalId(string globalId)
        {
            var collection = GetCollection<MongoExtendedArticleMetaData>();
            var query = Query.EQ("GlobalId", globalId);
            var data = collection.Find(query).FirstOrDefault();

            return new MongoCompleteArticleMetaData()
            {

            };
        }

        public IArticleNavigationStructure GetNavigationStructureByGlobalId(string globalId)
        {
            return new MongoArticleNavigationStructureData();
        }

        public IArticleStatistics GetStatisticsByGlobalId(string globalId)
        {
            return new MongoArticleStatisticsData();
        }
    }

    public class MongoCompleteArticleMetaData : ICompleteArticleMetaData
    {
        public string GlobalId { get; set; }
        public string ParentArticlePath { get; set; }
        public string Path { get; set; }
        public int Revision { get; set; }
        public string Title { get; set; }
        public string RevisedBy { get; set; }
        public DateTime RevisedOn { get; set; }
        public List<string> Keywords { get; set; }
        public bool IsAllowedChildren { get; set; }
        public bool ShowInIndex { get; set; }
    }

    public class MongoArticleStatisticsData : IArticleStatistics
    {
    }

    public class MongoArticleNavigationStructureData : IArticleNavigationStructure
    {
    }


    public class MongoArticleHistoryPageData : IArticleHistoryPage
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string GlobalId { get; set; }
        public IExtendedArticleMetaData[] Results { get; set; }
    }

    public interface IArticleMetaData
    {
        string GlobalId { get; }

        string ParentArticlePath { get; }

        string Path { get; }

        int Revision { get; }

        string Title { get; }
    }
}