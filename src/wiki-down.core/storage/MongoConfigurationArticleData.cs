using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoConfigurationArticleData : IMongoData
    {
        public ObjectId Id { get; set; }
    }
}