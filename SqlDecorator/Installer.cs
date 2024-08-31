using Npgsql;
using Microsoft.Data.SqlClient;
using SQLDecorator.Providers;
using SQLDecorator.Composer;

namespace SQLDecorator
{
    public enum DBProvider
    {
       MsSql,
       PostGres,
       SqlLite
    }
    static public class Installer
    {
        static public void Init()
        {
            Resolver<DbProviderRunner>.Register<MsssqlSelectRunner>(DBProvider.MsSql.ToString(), true);
            Resolver<DbProviderRunner>.Register<PostGresSelectRunner>(DBProvider.PostGres.ToString(), true);
            Resolver<DbProviderRunner>.Register<SqliteSelectRunner>(DBProvider.SqlLite.ToString(), true);
        }
    }
}
