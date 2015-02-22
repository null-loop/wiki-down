using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoSystemConfigurationStore : MongoStorage<MongoConfigurationArticleData>, ISystemConfigurationService
    {
        public MongoSystemConfigurationStore() : base("sys-config")
        {
            RequiresHistory = true;
            RequiresAudit = true;
        }
    }

    public class MongoConfigurationArticleData : IMongoData
    {
        public ObjectId Id { get; set; }
    }
}