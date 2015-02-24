namespace wiki_down.core.storage
{
    public class MongoSystemConfigurationStore : MongoStorage<MongoConfigurationArticleData>, ISystemConfigurationService
    {
        public MongoSystemConfigurationStore() : base("sys-config")
        {
            RequiresHistory = true;
            RequiresAudit = true;
        }
    }
}