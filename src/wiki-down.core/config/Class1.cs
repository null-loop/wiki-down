using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using wiki_down.core.storage;

namespace wiki_down.core.config
{
    public interface IDraftArticlesConfiguration
    {
        bool SaveHistory { get; set; }
    }

    public interface ILoggingConfiguration
    {
        long MaximumDataStoreSize { get; set; }

        LoggingLevel MinimumLoggingLevel { get; set; }
    }

    public interface ISiteConfiguration
    {
        string SiteName { get; set; }

        List<string> Domains { get; set; }

        Dictionary<string, string> PathMappings { get; set; }
    }

    public static class SystemConfigBootstrap
    {
        private static string _systemName;

        public static string SystemName
        {
            get { return _systemName; }
        }

        public static void Initialise()
        {
            var configuredSystemName = ConfigurationManager.AppSettings["wikidown.system.name"];

            if (!string.IsNullOrEmpty(configuredSystemName))
            {
                _systemName = configuredSystemName;
            }
            else
            {
                _systemName = AutoDetermineSystemName();
            }
        }

        private static string AutoDetermineSystemName()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            return assembly.GetName().Name;
        }
    }
}
