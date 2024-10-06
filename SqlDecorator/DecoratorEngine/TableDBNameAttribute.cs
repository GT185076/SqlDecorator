using System;

namespace SQLDecorator
{
    public class TableDBNameAttribute : Attribute
    {

        private string v;

        public TableDBNameAttribute() { }

        public TableDBNameAttribute(string TableName)
        {
            v = TableName;
        }
        public string TableName { get { return v; } }
    }
}
