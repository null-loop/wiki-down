using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;
using wiki_down.core.storage;

namespace wiki_down.testing.unit.storage
{
    [SetUpFixture]
    public class MySetUpClass
    {
        [SetUp]
        public void RunBeforeAnyTests()
        {
            MongoArticleStore.Init("mongodb://localhost", "unit-test");
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            
        }
    }

    [TestFixture]
    public class MongoArticleStoreTestFixture
    {

        [Test]
        public void Create_Article_Adds_Article_And_History_Entry_But_No_Draft()
        {
            var id = Guid.NewGuid();
            var store = new MongoArticleStore();

            var idString = id.ToString();

            store.CreateArticle(idString, "", idString, idString, idString, true, false, true, "UnitTesting",new string[0]);

            var mongoQuery = Query.EQ("Path", idString);

            store.GetArticlesCollection().Find(mongoQuery).Count().Should().Be(1);
            store.GetArticlesHistoryCollection().Find(mongoQuery).Count().Should().Be(1);
            store.GetArticlesDraftsCollection().Find(mongoQuery).Count().Should().Be(0);
        }

        [Test]
        public void Create_Draft_Article_Adds_Draft_Article_And_History_Entry_But_No_Main()
        {
            var id = Guid.NewGuid();
            var store = new MongoArticleStore();

            var idString = id.ToString();

            store.CreateArticle(idString, "", idString, idString, idString, true, true, true, "UnitTesting", new string[0]);

            var mongoQuery = Query.EQ("Path", idString);

            store.GetArticlesCollection().Find(mongoQuery).Count().Should().Be(0);
            store.GetArticlesHistoryCollection().Find(mongoQuery).Count().Should().Be(1);
            store.GetArticlesDraftsCollection().Find(mongoQuery).Count().Should().Be(1);
        }

        [Test]
        public void Revise_Article_Adds_New_History_Entry_And_Updates_Main_Article()
        {
            var id = Guid.NewGuid();
            var store = new MongoArticleStore();

            var idString = id.ToString();
            var mongoQuery = Query.EQ("Path", idString);

            store.CreateArticle(idString, "", idString, idString, idString, true, false, true, "UnitTesting", new string[0]);

            store.GetArticlesCollection().Find(mongoQuery).Count().Should().Be(1);
            store.GetArticlesHistoryCollection().Find(mongoQuery).Count().Should().Be(1);

            var revisedContent = idString + " with revisions";

            store.ReviseArticle(idString, idString, revisedContent, true, false, true, "UnitTesting", 1);

            store.GetArticlesCollection().Find(mongoQuery).Count().Should().Be(1);
            store.GetArticlesHistoryCollection().Find(mongoQuery).Count().Should().Be(1);

            var article = store.GetArticleByPath(idString);

            article.Markdown.Content.Should().Be(revisedContent);
            article.Revision.Should().Be(2);
        }

        [Test]
        public void Delete_And_Recover_Restores_Article_ArticleDrafts_And_History()
        {
            throw new NotImplementedException();
        }
    }
}
