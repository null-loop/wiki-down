using System.Collections.Generic;
using wiki_down.core.config;

namespace wiki_down.core.storage
{
    public class MongoSiteConfigurationData : MongoConfigurationArticleData, ISiteConfiguration
    {
        public string RootPath { get; set; }

        public string SiteName { get; set; }
        public List<string> Domains { get; set; }

        public Dictionary<string, string> PathMappings { get; set; }
    }
}