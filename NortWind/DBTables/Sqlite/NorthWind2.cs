using System.IO;
using System.Text;
using SQLDecorator.Providers;

namespace DBTables.Sqlite
{
    public class NorthWind2 : SqliteConnectionManager
    {
        public NorthWind2(string Alias,string ConnectionString, bool IsLog = false) : base(Alias,ConnectionString, IsLog)
        {
            migrationActions.Add(Ver1);
            RunMigrationList();
        }
        protected override void Ver1()
        {
            var check = "select count(*) from sqlite_master where tbl_name = 'Orders'";
            var exists =  RunDMLSql(check).Trim();
            if (exists == "1") return;
            var localDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // var NorthWindSeedFile =  Path.Combine("bin","debug","net6.0","DBTables","sqlite","NorthWind2.sql");
            var NorthWindSeedFile =  Path.Combine(localDirectory,"DBTables", "sqlite", "NorthWind2.sql");
            var lines = File.ReadAllLines(NorthWindSeedFile);
            var statement = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Trim().ToUpper() == "GO")
                {
                    if (statement.Length > 0) RunDMLSql(statement.ToString());
                    statement = new StringBuilder();
                    continue;
                }
                statement.Append(line).Append("\n");
            }
        }
    }
}
