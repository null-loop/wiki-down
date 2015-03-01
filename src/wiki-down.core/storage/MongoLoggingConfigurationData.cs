using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoLoggingConfigurationData : MongoConfigurationArticleData, ILoggingConfiguration
    {
        public long MaximumDataStoreSize { get; set; }

        public LoggingLevel MinimumLoggingLevel { get; set; }
    }

    public class MongoCoreConfigurationData : MongoConfigurationArticleData, ICoreConfiguration
    {
        public bool AllowMultipleRoots { get; set; }
    }
}