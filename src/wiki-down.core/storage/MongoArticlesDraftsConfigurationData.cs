using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoArticlesDraftsConfigurationData : MongoConfigurationArticleData, IDraftArticlesConfiguration
    {
        public bool SaveHistory { get; set; }


    }
}