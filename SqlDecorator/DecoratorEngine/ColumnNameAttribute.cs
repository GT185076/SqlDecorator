using System;

namespace SQLDecorator
{
    public class ColumnNameAttribute : Attribute
    {
        private string v;

        public ColumnNameAttribute() { }

        public ColumnNameAttribute(string ColumnName)
        {
            v = ColumnName;
        }
        public string ColumnName { get { return v; } }
    }
}