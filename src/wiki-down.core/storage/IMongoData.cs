using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public interface IMongoData
    {
        ObjectId Id { get; set; }
    }
}