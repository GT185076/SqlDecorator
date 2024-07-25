using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace SqlDecTest.MsssqlDBTables
{
    internal class MsSqlMigration
    {
        private SqlConnection sqlConnection;

        public MsSqlMigration(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        internal void Run()
        {
            sqlConnection.Open();
            Ver1();
        }

        private void Ver1()
        {
            var Fullstatment = File.ReadAllText("MsssqlDBTables\\NorthWind.sql"); 
            var statment = Fullstatment.Split("GO");
           
            foreach (var stat in statment)
            {
                var ss = Fullstatment.Split("goO ");
                foreach(var s in ss)
                        if (!s.IsNullOrEmpty())
                            RunSql(s);
            }                        
        }

        private string RunSql(string statment)
        {
            Console.WriteLine(statment);

            var sf = new StringBuilder();
            using (SqlCommand command = new SqlCommand(statment.ToString(), sqlConnection))
            {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sf.Append(reader.ToString());
                }
            }
            return sf.ToString();
        }
    }
}
