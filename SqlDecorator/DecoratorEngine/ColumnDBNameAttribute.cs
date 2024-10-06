using System;

namespace SQLDecorator
{
    public class ColumnDBNameAttribute : Attribute
    {
        private string v;

        public ColumnDBNameAttribute() { }

        public ColumnDBNameAttribute(string ColumnName)
        {
            v = ColumnName;
        }
        public string ColumnName { get { return v; } }
    }
}