using Npgsql;
using Microsoft.Data.SqlClient;
using SQLDecorator;

namespace Composer
{
    static public class Installer
    {
        static public void Init()
        {
            Resolver<DbProviderRunner>.Register<MsssqlSelectRunner>(typeof(SqlConnection).Name,true);
            Resolver<DbProviderRunner>.Register<PostGressSelectRunner>(typeof(NpgsqlConnection).Name,true);
        }
    }
}
