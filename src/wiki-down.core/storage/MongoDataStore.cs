using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace wiki_down.core.storage
{
    public static class MongoDataStore
    {
        private static string _connectionString;
        private static string _dbName;

        private static bool NotInitialised()
        {
            return string.IsNullOrEmpty(_connectionString);
        }

        public static void EmptyCollections()
        {
            var db = GetDatabase();
            var collections = db.GetCollectionNames().Where(ns => !ns.StartsWith("system."));
            foreach (var collection in collections)
            {
                var collectionObj = db.GetCollection<BsonDocument>(collection);
                collectionObj.RemoveAll();
            }
            Console.WriteLine("All collections empty");
        }

        public static T CreateStore<T>() where T : MongoStorage
        {
            if (NotInitialised()) throw new InvalidOperationException("Must call Initialise(connectionString, dbName) first!");

            var type = typeof(T);

            if (Stores.ContainsKey(type)) return Stores[type] as T;

            var instance = Activator.CreateInstance<T>();

            instance.ConfigureDatabase(_database);
            instance.ConfigureServices(_configService,_auditService,_loggingService);

            Stores.Add(type, instance);

            return instance;
        }

        private static readonly Dictionary<Type, MongoStorage> Stores = new Dictionary<Type, MongoStorage>();
        private static MongoSystemLoggingStore _loggingService;
        private static MongoSystemAuditStore _auditService;
        private static MongoSystemConfigurationStore _configService;
        private static MongoDatabase _database;
        private static MongoServer _server;

        public static MongoSystemLoggingStore SystemLoggingStore { get {  return _loggingService; } }
        public static MongoSystemAuditStore SystemAuditStore { get { return _auditService; } }
        public static MongoSystemConfigurationStore SystemConfigurationStore { get { return _configService; } }

        private static MongoDatabase GetDatabase()
        {
            if (_server == null)
            {
                _server = new MongoClient(_connectionString.StartsWith("mongodb:") ? _connectionString : "mongodb:" + _connectionString).GetServer();
            }
            return _server.GetDatabase(_dbName);
        }

        public static void Initialise(string connectionString, string dbName)
        {
            BsonClassMap.RegisterClassMap<MongoArticleData>();
            BsonClassMap.RegisterClassMap<MongoGeneratedArticleContentData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Format).SetRepresentation(BsonType.String);
            });
            BsonClassMap.RegisterClassMap<MongoSystemAuditEventData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Action).SetRepresentation(BsonType.String);
            });
            BsonClassMap.RegisterClassMap<MongoArticleTrashData>();
            BsonClassMap.RegisterClassMap<MongoArticleMetaData>(cm =>
            {
                cm.MapProperty(md => md.Id);
                cm.MapProperty(md => md.GlobalId);
                cm.MapProperty(md => md.ParentArticlePath);
                cm.MapProperty(md => md.Path);
                cm.MapProperty(md => md.Revision);
                cm.MapProperty(md => md.Title);
            });
            BsonClassMap.RegisterClassMap<MongoExtendedArticleMetaData>(cm =>
            {
                cm.MapProperty(emd => emd.RevisedOn);
                cm.MapProperty(emd => emd.RevisedBy);
                cm.MapProperty(emd => emd.ShowInIndex);
                cm.MapProperty(emd => emd.IsAllowedChildren);
                cm.MapProperty(emd => emd.Keywords);
            });
            BsonClassMap.RegisterClassMap<MongoSystemLoggingEventData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Level).SetRepresentation(BsonType.String);
            });
            BsonClassMap.RegisterClassMap<MongoJavascriptFunctionData>(cm =>
            {
                cm.MapIdProperty(c => c.Id);
                cm.MapProperty(c => c.Value).SetElementName("value");
            });
            BsonClassMap.RegisterClassMap<MongoConfigurationArticleData>();
            BsonClassMap.RegisterClassMap<MongoArticlesDraftsConfigurationData>();
            BsonClassMap.RegisterClassMap<MongoSiteConfigurationData>();
            BsonClassMap.RegisterClassMap<MongoLoggingConfigurationData>();

            _connectionString = connectionString;
            _dbName = dbName;
            _database = GetDatabase();

            // create the services
            _loggingService = new MongoSystemLoggingStore();
            _auditService = new MongoSystemAuditStore();
            _configService = new MongoSystemConfigurationStore();

            _loggingService.ConfigureDatabase(_database);
            _auditService.ConfigureDatabase(_database);
            _configService.ConfigureDatabase(_database);

            _loggingService.ConfigureServices(_configService, _auditService, _loggingService);
            _auditService.ConfigureServices(_configService, _auditService, _loggingService);
            _configService.ConfigureServices(_configService, _auditService, _loggingService);
  
        }
    }
}