using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using wiki_down.core;
using wiki_down.core.storage;

namespace wiki_down.tools.config
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                WriteUsage();
                return;
            }
            if (args.First() == "-install")
            {
                Install(args.Skip(1).ToArray());
            }
            else
            {
                WriteUsage();
                return;
            }

        }

        private static void WriteUsage()
        {
            Console.WriteLine("-install -mongo:{mongodb-uri} -db:{db-name} [-clean]");
        }

        private static void Install(string[] args)
        {
            var isDefault = args.First() == "-defaults";
            var mongo = isDefault ? "-mongo://localhost" : args.FirstOrDefault(a => a.StartsWith("-mongo:"));
            var db = isDefault ? "-db:wiki-down" : args.FirstOrDefault(a => a.StartsWith("-db:"));
            var clean = isDefault || args.Any(a => a == "-clean");

            if (string.IsNullOrEmpty(mongo) ||
                string.IsNullOrEmpty(db))
            {
                WriteUsage();
                return;
            }

            mongo = mongo.Substring(7);
            db = db.Substring(4);

            var connectionString = "mongodb:" + mongo;
            Console.WriteLine("Connecting to " + connectionString + " - using database name '" + db + "'");
            try
            {


                MongoDataStore.Initialise(connectionString, db);
                if (clean)
                {
                    var client = new MongoClient(connectionString);
                    var server = client.GetServer();
                    var database = server.GetDatabase(db);

                    CleanDB(database);
                }
                InitDB();
                Console.WriteLine("===============");
                Console.WriteLine("FIN!");
                Console.WriteLine("===============");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR::" + ex.Message);
                throw;
            }

        }

        private static void InitDB()
        {
            MongoDataStore.SystemAuditStore.Configure();
            MongoDataStore.SystemConfigurationStore.Configure();
            MongoDataStore.SystemLoggingStore.Configure();

            var articleStore = MongoDataStore.CreateStore<MongoArticleStore>();
            articleStore.Configure();

            const string wikidownConfigExe = "wiki-down.config.exe";

            articleStore.CreateDraftArticle("Home", "", "Home", "Welcome to Wiki.Down", File.ReadAllText("home.txt"), true, true, Environment.UserName,new []{"Article","Content","Default"}, wikidownConfigExe);
            articleStore.PublishDraft("Home", 1, Environment.UserName);

            articleStore.CreateDraftArticle("Markdown-Example", "Home", "Home.Markdown-Example", "A Markdown Example", File.ReadAllText("markdown-example.txt"), true, true, Environment.UserName, new[] { "Article", "Content", "Default" }, wikidownConfigExe);
            articleStore.PublishDraft("Home.Markdown-Example", 1, Environment.UserName);

            articleStore.CreateDraftArticle("Deleted", "Home", "Home.Deleted", "A Deleted Article", File.ReadAllText("deleted.txt"), true, true, Environment.UserName, new[] { "Article", "Content", "Default" }, wikidownConfigExe);
            //articleStore.PublishDraft("Home.Deleted", 1, Environment.UserName);
            articleStore.DeleteArticle("Home.Deleted", Environment.UserName);

            articleStore.CreateDraftArticle("Draft", "Home", "Home.Draft", "A Draft Article", File.ReadAllText("draft.txt"), true, true, Environment.UserName, new[] { "Article", "Content", "Default" }, wikidownConfigExe);
            
            Console.WriteLine("Created initial articles");

            
        }

        private static void CleanDB(MongoDatabase database)
        {
            Console.WriteLine("Cleaning db");
            var collections = database.GetCollectionNames().Where(ns => !ns.StartsWith("system."));
            foreach (var collection in collections)
            {
                Console.WriteLine("Dropping collection '" + collection + "'");
                database.DropCollection(collection);
            }
        }
    }

}
