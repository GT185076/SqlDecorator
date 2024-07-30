using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

namespace SqlDecTest.MsssqlDBTables
{
    internal class MsSqlMigration
    {       
        private static MsSqlMigration _MsSqlMigration;

        private SqlConnection sqlConnection;
        private SqlDataReader reader;
        private bool            isLog;

        public static MsSqlMigration CheckVersionAndRun(SqlConnection SqlConnection,bool IsLog=false)
        {            
            if (_MsSqlMigration == null) 
                _MsSqlMigration = new MsSqlMigration();

            _MsSqlMigration.sqlConnection = SqlConnection;
            _MsSqlMigration.isLog = IsLog;
            _MsSqlMigration.Ver1();

            return _MsSqlMigration;
        }

        private void Ver1()
        {
            var check = "select count(*) from sysobjects where id = object_id('dbo.orders')";
            var exists = RunSql(check,false).Trim();
            if (exists=="1") return;

            var NorthWindSeedFile = "MsssqlDBTables\\NorthWind.sql";            
            var lines = File.ReadAllLines(NorthWindSeedFile);
            var statement =  new StringBuilder();

            foreach (var line in lines) 
            { 
                if (line.Trim().ToUpper()=="GO")                    
                {
                    if (statement.Length>0) RunSql(statement.ToString(),isLog);
                    continue;
                }                
                statement.Append(line);            
            }            
        }

        private string RunSql(string statment,bool IsLog)
        {
            if (IsLog)
            {
                Console.WriteLine("Run Sql:");
                Console.WriteLine(statment);
            }

            var sf = new StringBuilder();

            using (SqlCommand command = new SqlCommand(statment, sqlConnection))
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                       if (reader.FieldCount>0) 
                        sf.Append(reader[0].ToString());                                    
            }

            reader.Close();

            if (IsLog)
            {
                Console.WriteLine("");
                Console.WriteLine(sf.ToString());
            }

            return sf.ToString();
            
        }
    }
}
