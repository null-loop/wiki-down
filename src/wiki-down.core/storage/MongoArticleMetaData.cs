using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoArticleMetaData : IMongoData, IArticleMetaData
    {
        public ObjectId Id { get; set; }

        public string GlobalId { get; set; }

        public string ParentArticlePath { get; set; }

        public string Path { get; set; }

        public int Revision { get; set; }

        public string Title { get; set; }

    }
}