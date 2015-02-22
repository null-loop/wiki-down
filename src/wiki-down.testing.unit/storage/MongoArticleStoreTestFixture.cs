using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;
using wiki_down.core;
using wiki_down.core.storage;

namespace wiki_down.testing.unit.storage
{
    [TestFixture]
    public class MongoArticleStoreTestFixture
    {
        [SetUp]
        public void TestSetup()
        {
            MongoDataStore.EmptyCollections();
        }

        [Test]
        public void SearchArticleTitle_Finds_Live_Article()
        {
            var id = Guid.NewGuid();
            var store = MongoDataStore.CreateStore<MongoArticleStore>();

            var idString = id.ToString();

            store.CreateDraftArticle(idString, "", idString, "My weird title", idString, true, true, "UnitTesting", new string[0]);
            store.PublishDraft(idString, 1, "UnitTesting");

            var searchResults = store.SearchArticleTitlesIndex("My");
            searchResults.Count().Should().Be(1);

            searchResults = store.SearchArticleTitlesIndex("weird");
            searchResults.Count().Should().Be(0);

            searchResults = store.SearchArticleTitlesIndex("title");
            searchResults.Count().Should().Be(0);

            searchResults = store.SearchArticleTitlesIndex("My weird title");
            searchResults.Count().Should().Be(1);

            searchResults = store.SearchArticleTitles("My");
            searchResults.Count().Should().Be(1);

            searchResults = store.SearchArticleTitles("weird");
            searchResults.Count().Should().Be(1);

            searchResults = store.SearchArticleTitles("title");
            searchResults.Count().Should().Be(1);

            searchResults = store.SearchArticleTitles("My weird title");
            searchResults.Count().Should().Be(1);
        }

        [Test]
        public void Delete_And_Recover_Restores_Article_ArticleDrafts_And_History()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Create_DraftArticle_Adds_Draft_Article_No_History_Entry_And_No_Main()
        {
            var id = Guid.NewGuid();
            var store = MongoDataStore.CreateStore<MongoArticleStore>();

            var idString = id.ToString();

            store.CreateDraftArticle(idString, "", idString, idString, idString, true, true, "UnitTesting", new string[0]);

            var mongoQuery = Query.EQ("Path", idString);

            store.Find(mongoQuery).Count().Should().Be(0);
            store.FindHistory(mongoQuery).Count().Should().Be(0);
            store.FindDrafts(mongoQuery).Count().Should().Be(1);

            //store.GetArticlesCollection().Find(mongoQuery).Count().Should().Be(0);
            //store.GetArticlesHistoryCollection().Find(mongoQuery).Count().Should().Be(0);
            //store.GetArticlesDraftsCollection().Find(mongoQuery).Count().Should().Be(1);
        }

        [Test]
        public void Revise_Draft_Updates_Draft()
        {
            var id = Guid.NewGuid();
            var store = MongoDataStore.CreateStore<MongoArticleStore>();

            var idString = id.ToString();
            var mongoQuery = Query.EQ("Path", idString);

            store.CreateDraftArticle(idString, "", idString, idString, idString, true, true, "UnitTesting", new string[0]);

            store.FindDrafts(mongoQuery).Count().Should().Be(1);
            store.Find(mongoQuery).Count().Should().Be(0);
            store.FindHistory(mongoQuery).Count().Should().Be(0);

            var revisedContent = idString + " with revisions";

            store.ReviseDraft(idString, idString, revisedContent, true, true, new []{"New"},"UnitTesting", 1);

            store.FindDrafts(mongoQuery).Count().Should().Be(1);
            store.Find(mongoQuery).Count().Should().Be(0);
            store.FindHistory(mongoQuery).Count().Should().Be(0);

            var article = store.GetDraft(idString, "UnitTesting", 1);

            article.Markdown.Content.Should().Be(revisedContent);
            article.Revision.Should().Be(1);
        }



        [Test]
        public void Create_Draft_And_Publish_Creates_Live_Article_And_History_And_Removes_Draft()
        {
            var id = Guid.NewGuid();
            var store = MongoDataStore.CreateStore<MongoArticleStore>();

            var idString = id.ToString();
            var mongoQuery = Query.EQ("Path", idString);

            store.CreateDraftArticle(idString, "", idString, idString, idString, true, true, "UnitTesting", new string[0]);

            store.FindDrafts(mongoQuery).Count().Should().Be(1);
            store.Find(mongoQuery).Count().Should().Be(0);
            store.FindHistory(mongoQuery).Count().Should().Be(0);

            store.PublishDraft(idString, 1, "UnitTesting");

            store.FindDrafts(mongoQuery).Count().Should().Be(0);
            store.Find(mongoQuery).Count().Should().Be(1);
            store.FindHistory(mongoQuery).Count().Should().Be(1);

            var article = store.GetArticle(idString);

            article.Markdown.Content.Should().Be(idString);
        }

        [Test]
        public void Live_Article_Receives_Draft_Publish_Revisions()
        {
            var id = Guid.NewGuid();
            var store = MongoDataStore.CreateStore<MongoArticleStore>();

            var idString = id.ToString();
            var mongoQuery = Query.EQ("Path", idString);

            store.CreateDraftArticle(idString, "", idString, idString, idString, true, true, "UnitTesting", new string[0]);

            store.FindDrafts(mongoQuery).Count().Should().Be(1);
            store.Find(mongoQuery).Count().Should().Be(0);
            store.FindHistory(mongoQuery).Count().Should().Be(0);

            store.PublishDraft(idString, 1, "UnitTesting");

            store.FindDrafts(mongoQuery).Count().Should().Be(0);
            store.Find(mongoQuery).Count().Should().Be(1);
            store.FindHistory(mongoQuery).Count().Should().Be(1);

            var article = store.GetArticle(idString);

            article.Markdown.Content.Should().Be(idString);

            store.CreateDraftArticle(idString, "UnitTesting");
            store.FindDrafts(mongoQuery).Count().Should().Be(1);
            store.ReviseDraft(idString, "UnitTesting", "NEW CONTENT", true, true, new []{"New","Keywords"},"UnitTesting", 2);
            store.FindDrafts(mongoQuery).Count().Should().Be(1);
            store.PublishDraft(idString, 2, "UnitTesting");
            store.FindDrafts(mongoQuery).Count().Should().Be(0);
            store.FindHistory(mongoQuery).Count().Should().Be(2);

            article = store.GetArticle(idString);
            article.Markdown.Content.Should().Be("NEW CONTENT");
        }

        [Test]
        public void Create_And_Publish_Draft_From_Existing_Article_Moves_Revision_On_By_One()
        {
            var id = Guid.NewGuid();
            var store = MongoDataStore.CreateStore<MongoArticleStore>();

            var idString = id.ToString();

            store.CreateDraftArticle(idString, "", idString, idString, idString, true, true, "UnitTesting", new string[0]);
            store.PublishDraft(idString, 1, "UnitTesting");
            var secondDraft = store.CreateDraftArticle(idString, "UnitTesting");
            secondDraft.Revision.Should().Be(2);
            store.ReviseDraft(idString, "UnitTesting", "NEW CONTENT", true, true, new[] { "New", "Keywords" }, "UnitTesting", 2);
            store.PublishDraft(idString, 2, "UnitTesting");
            var publishedSecond = store.GetArticle(idString);
            publishedSecond.Revision.Should().Be(2);
        }
    }
}
