using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    public class MongoGeneratedArticleContentStore : MongoStorage<MongoGeneratedArticleContentData>, IGeneratedArticleContentService
    {
        public MongoGeneratedArticleContentStore() : base("articles-generated")
        {
        }

        public void RegenerateArticleContent(string path, string globalId)
        {
            Debug("articles-generated", "Regenerating article content for articlePath://" + path + ", article://" + globalId);
            RunGenerate(path, globalId, Database);
        }

        internal static void RunGenerate(string path, string globalId, MongoDatabase mongoDatabase)
        {
            mongoDatabase.Eval(new EvalArgs()
            {
                Code = new BsonJavaScript("function(a,b){generate_all_article_content(a,b);}"),
                Args = new BsonValue[] { new BsonString(path), new BsonString(globalId) },
                Lock = false
            });
        }


        public IArticleContent GetGeneratedArticleContentByPath(string path, ArticleContentFormat format)
        {
            var collection = GetCollection();
            var articleQuery = Query.And(Query.EQ("Path", path), Query.EQ("Format", format.ToString()));
            var articleContent = collection.FindOne(articleQuery);

            return articleContent;
        }

        public IArticleContent GetGeneratedArticleContentByGlobalId(string globalId, ArticleContentFormat format)
        {
            var collection = GetCollection();
            var articleQuery = Query.And(Query.EQ("GlobalId", globalId), Query.EQ("Format", format.ToString()));
            var articleContent = collection.FindOne(articleQuery);

            return articleContent;
        }

        public override void InitialiseDatabase()
        {
            var collection = GetCollection();
            collection.CreateIndex(new IndexKeysBuilder().Ascending("Path").Ascending("Format"), IndexOptions.SetUnique(true));
        }
    }
}