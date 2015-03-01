using System.Collections.Generic;

namespace wiki_down.core.config
{
    public interface ISiteConfiguration
    {
        string RootPath { get; set; }

        string SiteName { get; set; }

        List<string> Domains { get; set; }

        Dictionary<string, string> PathMappings { get; set; }
    }
}