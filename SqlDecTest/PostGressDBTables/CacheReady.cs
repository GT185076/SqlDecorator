using System;
using System.Collections.Generic;
using System.Text;
using SQLDecorator;

namespace DBTables
{ 
    public class cacheReady : DBTable
    {
        public StringColumn PKey = new StringColumn { Name = "Key" };

        public StringColumn PValue = new StringColumn { Name = "value" };

        public NumberColumn Height = new  NumberColumn { Name = "height" };

        public LogicalColumn IsActive = new LogicalColumn { Name = "isactive" };

        public IntegerColumn proirity = new IntegerColumn { Name = "priority" };

        public DateTimeColumn createTS = new DateTimeColumn { Name = "create_ts" };
        public cacheReady() : base("cacheready",  "public")
        {            
            SetPrimaryKey(PKey);
        }        
    }
}
