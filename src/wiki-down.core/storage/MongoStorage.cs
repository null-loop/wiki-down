using System.Collections.Generic;
using System.Reflection;
using MongoDB.Driver;
using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public abstract class MongoStorage
    {
        private MongoDatabase _database;
        private readonly string _collectionNameRoot;

        protected MongoStorage(string collectionNameRoot)
        {
            _collectionNameRoot = collectionNameRoot;
        }

        internal void ConfigureDatabase(MongoDatabase database)
        {
            _database = database;
        }

        protected MongoDatabase Database {get { return _database; }}

        protected ISystemConfigurationService SystemConfiguration { get; private set; }
        protected ISystemAuditService SystemAudit { get; private set; }
        protected ISystemLoggingService SystemLoggingService { get; private set; }

        internal void ConfigureServices(ISystemConfigurationService configurationService,
                                        ISystemAuditService systemAuditService, 
                                        ISystemLoggingService systemLoggingService)
        {
            SystemConfiguration = configurationService;
            SystemAudit = systemAuditService;
            SystemLoggingService = systemLoggingService;
        }


        protected MongoCollection<T> GetCollection<T>(string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                return Database.GetCollection<T>(_collectionNameRoot);
            }
            return Database.GetCollection<T>(_collectionNameRoot + "-" + collectionName);
        }

        protected void StoreAudit(string area, AuditAction action, string path, string actionedBy, int revision)
        {
            SystemAudit.Audit(area, action, path, actionedBy, revision);
        }

        protected string CollectionNameRoot
        {
            get {  return _collectionNameRoot;}
        }

        public virtual void InitialiseDatabase()
        {
            // perform any config on datastore init
        }
    }

    public abstract class MongoStorage<TDefault> : MongoStorage where TDefault : IMongoData
    {
        private static string _system;
        protected bool RequiresHistory { get; set; }
        protected bool RequiresAudit { get; set; }

        protected bool RequiresDrafts { get; set; }

        protected string SystemArea { get; set; }

        protected void Debug(string area, string message)
        {
            SystemLoggingService.Debug(_system,area,GetType().FullName,message);
        }

        protected void Info(string area, string message)
        {
            SystemLoggingService.Info(_system, area, GetType().FullName, message);
        }

        protected void Warn(string area, string message)
        {
            SystemLoggingService.Warn(_system, area, GetType().FullName, message);
        }

        protected void Error(string area, string message)
        {
            SystemLoggingService.Error(_system, area, GetType().FullName, message);
        }

        protected void Fatal(string area, string message)
        {
            SystemLoggingService.Fatal(_system, area, GetType().FullName, message);
        }

        protected MongoStorage(string collectionNameRoot) : base(collectionNameRoot)
        {
            _system = SystemConfigBootstrap.SystemName;
        }

        public MongoCursor<TDefault> Find(IMongoQuery mongoQuery)
        {
            return GetCollection().Find(mongoQuery);
        }

        public MongoCursor<TDefault> FindHistory(IMongoQuery mongoQuery)
        {
            return GetHistoryCollection().Find(mongoQuery);
        }

        public MongoCursor<TDefault> FindDrafts(IMongoQuery mongoQuery)
        {
            return GetDraftsCollection().Find(mongoQuery);
        }

        protected MongoCollection<T> GetCollection<T>()
        {
            return GetCollection<T>(string.Empty);
        }

        protected MongoCollection<TDefault> GetCollection()
        {
            return GetCollection<TDefault>();
        }

        protected MongoCollection<TDefault> GetHistoryCollection()
        {
            EnsureRequiresHistory();
            return GetCollection<TDefault>("history");
        }

        private void EnsureRequiresDrafts()
        {
            if (!RequiresDrafts)
            {
                throw new CollectionUnavailableException(CollectionNameRoot + " is not configured for drafts");
            }
        }

        private void EnsureRequiresHistory()
        {
            if (!RequiresHistory)
            {
                throw new CollectionUnavailableException(CollectionNameRoot + " is not configured for history");
            }
        }

        protected MongoCollection<TDefault> GetDraftsCollection()
        {
            EnsureRequiresDrafts();
            return GetCollection<TDefault>("drafts");
        }

        
    }
}