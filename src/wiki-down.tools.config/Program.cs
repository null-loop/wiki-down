using System;
using System.IO;
using System.Linq;
using MongoDB.Driver;
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
            MongoDataStore.SystemAuditStore.InitialiseDatabase();
            MongoDataStore.SystemConfigurationStore.InitialiseDatabase();
            MongoDataStore.SystemLoggingStore.InitialiseDatabase();

            var javascriptStore = MongoDataStore.CreateStore<MongoJavascriptFunctionStore>();

            javascriptStore.InitialiseDatabase();
            javascriptStore.StoreFunction("markdown_to_html", File.ReadAllText("javascript/marked.js"));
            javascriptStore.StoreFunction("generate_all_article_content", File.ReadAllText("javascript/generate_all_article_content.js"));
            javascriptStore.StoreFunction("generate_article_content", File.ReadAllText("javascript/generate_article_content.js"));

            var generatedStore = MongoDataStore.CreateStore<MongoGeneratedArticleContentStore>();
            generatedStore.InitialiseDatabase();

            var articleStore = MongoDataStore.CreateStore<MongoArticleStore>();
            articleStore.InitialiseDatabase();

            const string wikidownConfigExe = "wiki-down.config.exe";

            articleStore.CreateDraft("Home", "", "Home", "Welcome to Wiki.Down", File.ReadAllText("home.txt"), true, true, Environment.UserName,new []{"Article","Content","Default"}, wikidownConfigExe);
            articleStore.PublishDraft("Home", 1, Environment.UserName);

            articleStore.CreateDraft("Markdown-Example", "Home", "Home.Markdown-Example", "A Markdown Example", File.ReadAllText("markdown-example.txt"), true, true, Environment.UserName, new[] { "Article", "Content", "Default" }, wikidownConfigExe);
            articleStore.PublishDraft("Home.Markdown-Example", 1, Environment.UserName);

            articleStore.CreateDraft("Deleted", "Home", "Home.Deleted", "A Deleted Article", File.ReadAllText("deleted.txt"), true, true, Environment.UserName, new[] { "Article", "Content", "Default" }, wikidownConfigExe);
            //articleStore.PublishDraft("Home.Deleted", 1, Environment.UserName);
            articleStore.TrashArticle("Home.Deleted", Environment.UserName);

            articleStore.CreateDraft("Draft", "Home", "Home.Draft", "A Draft Article", File.ReadAllText("draft.txt"), true, true, Environment.UserName, new[] { "Article", "Content", "Default" }, wikidownConfigExe);
            
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
