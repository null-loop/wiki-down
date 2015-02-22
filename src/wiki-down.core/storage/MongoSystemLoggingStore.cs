using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    public class MongoSystemLoggingStore : MongoStorage<MongoSystemLoggingEventData>, ISystemLoggingService
    {
        public MongoSystemLoggingStore() : base("sys-logging")
        {
        }

        public override void Configure()
        {
            //TODO:Get the max size from config
            Database.CreateCollection(CollectionNameRoot, CollectionOptions
                .SetCapped(true)
                .SetMaxSize(100000000)
            );
        }

        private void Log(LoggingLevel level, string system, string area, string type, string message)
        {
            GetCollection().Insert(new MongoSystemLoggingEventData()
            {
                Level = level,
                SourceSystem = system,
                SourceArea = area,
                SourceType = type,
                Message = message,
                Occurred = DateTime.UtcNow
            });
        }

        public void Debug(string system, string area, string type, string message)
        {
            Log(LoggingLevel.Debug, system,area,type,message);
        }

        public void Info(string system, string area, string type, string message)
        {
            Log(LoggingLevel.Info, system, area, type, message);
        }

        public void Warn(string system, string area, string type, string message)
        {
            Log(LoggingLevel.Warn, system, area, type, message);
        }

        public void Error(string system, string area, string type, string message)
        {
            Log(LoggingLevel.Error, system, area, type, message);
        }

        public void Fatal(string system, string area, string type, string message)
        {
            Log(LoggingLevel.Fatal, system, area, type, message);
        }
    }

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

    public enum LoggingLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4
    }
}