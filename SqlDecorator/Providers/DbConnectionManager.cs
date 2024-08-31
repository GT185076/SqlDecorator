﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SQLDecorator.Providers
{
    public abstract class DbConnectionManager
    {
        public DbConnection DbConnection { get; protected set; }
        protected bool isLog;
        protected List<Action> migrationActions = new List<Action>();
        protected DbProviderRunner runner { get; set; }
        static DbConnectionManager()
        {
            Installer.Init();
        }
        protected DbConnectionManager(string ConnectionString, bool IsLog = false)
        {
            if (ConnectionString == null) throw new ArgumentNullException();
            else
            {
                isLog = IsLog;
            }
        }
        protected void RunMigrationList()
        {
            foreach (var action in migrationActions) action.Invoke();
        }
        public string RunDMLSql(string statment)
        {
            return runner.RunDMLSql(statment, DbConnection, isLog);
        }
        public IEnumerable<ResultRecord> Run(Select statment, List<DbParameter> parameters)
        {
           foreach (var r in runner.Run(statment, DbConnection, parameters))
                   yield return r;
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, List<DbParameter> parameters)
        {
            return await runner.RunAsync(statment,DbConnection,parameters);
        }
        public DbParameter CreateParameter(string paramName, object value)
        {
            return runner.CreateParameter(paramName, value);
        }
    }

}
