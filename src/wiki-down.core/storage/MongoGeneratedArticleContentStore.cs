using System;
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

        public void RegenerateArticleContent(string path)
        {
            RunGenerate(path, Database);
        }

        internal static void RunGenerate(string path, MongoDatabase mongoDatabase)
        {
            mongoDatabase.Eval(new EvalArgs()
            {
                Code = new BsonJavaScript("function(a){generate_all_article_content(a);}"),
                Args = new BsonValue[] {new BsonString(path)}
            });
        }

        private void RegenerateArticleContent(string path, ArticleContentFormat html)
        {
            throw new NotImplementedException();
        }

        public IArticleContent GetGeneratedArticleCotent(string path, ArticleContentFormat format)
        {
            throw new NotImplementedException();
        }

        public override void InitialiseDatabase()
        {
            var collection = GetCollection();
            collection.CreateIndex(new IndexKeysBuilder().Ascending("ArticlePath").Ascending("Format"), IndexOptions.SetUnique(true));
        }
    }
}