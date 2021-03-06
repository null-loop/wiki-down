using System;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoSystemLoggingStore : MongoStorage<MongoSystemLoggingEventData>, ISystemLoggingService
    {
        public MongoSystemLoggingStore() : base("sys-logging")
        {
        }

        public override void InitialiseDatabase()
        {
            var config = SystemConfiguration.GetConfiguration<ILoggingConfiguration>();

            Database.DropCollection(CollectionNameRoot);
            Database.CreateCollection(CollectionNameRoot, CollectionOptions
                .SetCapped(true)
                .SetMaxSize(config.MaximumDataStoreSize)
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
}