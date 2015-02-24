using System;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoSystemLoggingEventData : IMongoData
    {
        public ObjectId Id { get; set; }

        public LoggingLevel Level { get; set; }

        public string SourceSystem { get; set; }

        public string SourceArea { get; set; }

        public string SourceType { get; set; }

        public DateTime Occurred { get; set; }

        public string Message { get; set; }
    }
}