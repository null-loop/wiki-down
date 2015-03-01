using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoSystemConfigurationStore : MongoStorage<MongoConfigurationArticleData>, ISystemConfigurationService
    {
        private class ConfigurationDataTypeMapping
        {
            public string SetName { get; set; }
            public Type InterfaceType { get; set; }
            public Type InstanceType { get; set; }
        }

        private static readonly List<ConfigurationDataTypeMapping> DataTypeMappings = new List<ConfigurationDataTypeMapping>();
        static MongoSystemConfigurationStore()
        {
            AddDataTypeMapping<IDraftArticlesConfiguration, MongoArticlesDraftsConfigurationData>("articles-draft");
            AddDataTypeMapping<ISiteConfiguration, MongoSiteConfigurationData>("sys-site");
            AddDataTypeMapping<ILoggingConfiguration, MongoLoggingConfigurationData>("sys-logging");
        }

        public static List<Type> GetInstanceTypes()
        {
            return DataTypeMappings.Select(m => m.InstanceType).ToList();
        }

        private static void AddDataTypeMapping<T, T1>(string configurationSetName) where T1:T
        {
            DataTypeMappings.Add(new ConfigurationDataTypeMapping()
            {
                InterfaceType = typeof(T),
                InstanceType = typeof(T1),
                SetName = configurationSetName
            });
        }

        public MongoSystemConfigurationStore() : base("sys-config")
        {
            RequiresHistory = true;
            RequiresAudit = true;
        }

        public TConfiguration GetConfiguration<TConfiguration>() where TConfiguration : class
        {
            var mapping = GetMapping<TConfiguration>();
            var collection = GetCollection();
            var configuration = collection.FindOneAs(mapping.InstanceType, Query.And(Query.EQ("ConfigurationSetName", mapping.SetName), Query.EQ("System", SystemConfigBootstrap.SystemName))) as TConfiguration;

            if (configuration == null)
            {
                configuration = collection.FindOneAs(mapping.InstanceType, Query.And(Query.EQ("ConfigurationSetName", mapping.SetName), Query.EQ("System", "*"))) as TConfiguration;
            }

            return configuration;
        }

        public TConfiguration GetConfiguration<TConfiguration>(string systemName) where TConfiguration : class
        {
            throw new System.NotImplementedException();
        }

        public override void InitialiseDatabase()
        {
            var configuration = GetCollection();

            configuration.CreateIndex(new IndexKeysBuilder().Ascending("System").Ascending("ConfigurationSetName").Descending("Revision"), IndexOptions.SetUnique(true));

            var history = GetHistoryCollection();

            history.CreateIndex(new IndexKeysBuilder().Ascending("System").Ascending("ConfigurationSetName").Descending("Revision"), IndexOptions.SetUnique(true));
            
            base.InitialiseDatabase();
        }

        public void SetDefaultConfiguration<T>(Action<T> action, string author) where T : class
        {
            SetConfiguration(action,"*",author);
        }

        public void SetConfiguration<T>(Action<T> action, string system, string author) where T : class
        {
            var mapping = GetMapping<T>();

            // check for an existing data doc.
            T doc = null;

            var collection = GetCollection();
            var history = GetHistoryCollection();
            var name = mapping.SetName;
            var revision = 1;

            doc = collection.FindOneAs(mapping.InstanceType, Query.And(Query.EQ("ConfigurationSetName", name), Query.EQ("System", system))) as T;

            if (doc == null)
            {
                doc = Activator.CreateInstance(mapping.InstanceType) as T;

                if (doc == null) throw new InvalidOperationException("Failed to create instance of " + mapping.InstanceType.FullName + " as " + mapping.InterfaceType.Name);

                // perform our config
                action(doc);

                // set metadata
                var configBase = doc as MongoConfigurationArticleData;

                configBase.ConfigurationSetName = mapping.SetName;
                configBase.System = system;
                configBase.Revision = 1;
                configBase.RevisedOn = DateTime.UtcNow;
                configBase.RevisedBy = author;

                collection.Insert(doc);

                revision = configBase.Revision;
            }
            else
            {
                action(doc);
                var configBase = doc as MongoConfigurationArticleData;

                configBase.Revision = configBase.Revision + 1;
                configBase.RevisedOn = DateTime.UtcNow;
                configBase.RevisedBy = author;
                collection.Save(doc);

                configBase.Id = ObjectId.Empty;

                revision = configBase.Revision;
            }

            history.Insert(doc);

            Audit(AuditAction.Create, name + "." + system, revision, author);
            Info("sys-config", "Set " + system + " configuration for set '" + name + "' of type " + typeof(T).FullName);
        }

        private void Audit(AuditAction action, string path, int revision, string actionedBy)
        {
            StoreAudit("sys-config", action, path, actionedBy, revision);
        }

        private static ConfigurationDataTypeMapping GetMapping<T>() where T : class
        {
            var mapping = DataTypeMappings.FirstOrDefault(m => m.InterfaceType == typeof (T));

            if (mapping == null) throw new InvalidOperationException("Unknown configuration type " + typeof (T).FullName);
            return mapping;
        }
    }
}