using SQLDecorator;

namespace DBTables
{ 
    public class cacheReady : DBTable
    {
        public StringColumn PKey = new StringColumn { FieldName = "Key" };

        public StringColumn PValue = new StringColumn { FieldName = "value" };

        public NumberColumn Height = new  NumberColumn { FieldName = "height" };

        public LogicalColumn IsActive = new LogicalColumn { FieldName = "isactive" };

        public IntegerColumn proirity = new IntegerColumn { FieldName = "priority" };

        public DateTimeColumn createTS = new DateTimeColumn { FieldName = "create_ts" };
        public cacheReady() : base("cacheready",  "public")
        {            
            SetPrimaryKey(PKey);
        }        
    }
}
