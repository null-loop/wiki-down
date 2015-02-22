using NUnit.Framework;
using wiki_down.core.storage;

namespace wiki_down.testing.unit.storage
{
    [SetUpFixture]
    public class TestSetup
    {
        [SetUp]
        public void RunBeforeAnyTests()
        {
            MongoDataStore.Initialise("mongodb://localhost", "wd-unit-tests");
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            
        }


    }
}