using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace wiki_down.core.config
{
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
