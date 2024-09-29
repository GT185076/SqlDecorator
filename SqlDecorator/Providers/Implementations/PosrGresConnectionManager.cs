using SQLDecorator.Composer;

namespace SQLDecorator.Providers
{
  
    public abstract class PostGresConnectionManager : DbConnectionManager
    {
        public PostGresConnectionManager(string Alias, string ConnectionString, bool IsLog = false) : base(Alias, ConnectionString, IsLog)
        {           
            runner = Resolver<DbProviderRunner>.Resolve(DBProvider.PostGres.ToString());
            DbConnection = runner.CreateDbConnection(ConnectionString);
        }              
        protected abstract void Ver1();
    }
}
