using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace wiki_down.content.server.controllers
{
    public class UtilityController : Controller
    {
        private static string _robotsTxt;

        public ActionResult Robots()
        {
            return Content(_robotsTxt,"text/plain");
        }

        public static void ConfigureRobots(Action<RobotsBuilder> builderAction)
        {
            var b = new RobotsBuilder();
            builderAction(b);
            _robotsTxt = b.ToString();
        }
    }

    public class RobotsBuilder
    {
        private class RobotsSection
        {
            public string UserAgent { get; private set; }
            public List<Tuple<string,string>> Pairs { get; private set; }

            public RobotsSection(string userAgent)
            {
                UserAgent = userAgent;
                Pairs = new List<Tuple<string, string>>();
            }
        }

        private readonly List<RobotsSection> _sections = new List<RobotsSection>();
        private RobotsSection _currentSection;

        public override string ToString()
        {
            var b = new StringBuilder();
            foreach (var section in _sections)
            {
                var wroteLines = false;
                b.AppendLine("User-Agent: " + section.UserAgent);
                foreach (var d in section.Pairs)
                {
                    b.AppendLine(d.Item1 + ": " + d.Item2);
                    wroteLines = true;
                }
                if (wroteLines)
                {
                    b.AppendLine();
                }
            }
            return b.ToString();
        }

        public void Exclude(string uri)
        {
            EnsureCurrentSection();
            ValidateUri(uri);
            AddPair("Disallow", uri);
        }

        private void ValidateUri(string uri)
        {
            
        }

        private void EnsureCurrentSection()
        {
            if (_currentSection == null)
            {
                CreateCurrent();
            }
        }

        public void StartSection(string useragent = "*")
        {
            CreateCurrent(useragent);
        }

        public void StartSection(params string[] useragents)
        {
            foreach (var agent in useragents)
            {
                CreateCurrent(agent);
            }
        }

        private void CreateCurrent(string useragent = "*")
        {
            if (_sections.Any(u => u.UserAgent == useragent))
            {
                throw new InvalidOperationException("Duplicate user agent registration for " + useragent);
            }

            _currentSection = new RobotsSection(useragent);
            _sections.Add(_currentSection);
        }

        public void SiteMap(string siteMapUri)
        {
            AddPair("Sitemap", siteMapUri);
        }

        private void AddPair(string key, string value)
        {
            _currentSection.Pairs.Add(new Tuple<string, string>(key, value));
        }
    }
}