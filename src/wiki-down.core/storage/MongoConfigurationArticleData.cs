using System;
using System.Collections.Generic;
using MongoDB.Bson;
using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoSiteConfigurationData : MongoConfigurationArticleData, ISiteConfiguration
    {
        public string SiteName { get; set; }
        public List<string> Domains { get; set; }

        public Dictionary<string, string> PathMappings { get; set; }
    }

    public class MongoLoggingConfigurationData : MongoConfigurationArticleData, ILoggingConfiguration
    {
        public long MaximumDataStoreSize { get; set; }

        public LoggingLevel MinimumLoggingLevel { get; set; }
    }

    public class MongoConfigurationArticleData : IMongoData
    {
        public ObjectId Id { get; set; }

        public string System { get; set; }

        public string ConfigurationSetName { get; set; }

        public int Revision { get; set; }

        public DateTime RevisedOn { get; set; }

        public string RevisedBy { get; set; }
    }

    public class MongoArticlesDraftsConfigurationData : MongoConfigurationArticleData, IDraftArticlesConfiguration
    {
        public bool SaveHistory { get; set; }


    }
}