using SQLDecorator;
using SQLDecorator.Providers;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;


namespace SQLDecorator
{
    public enum ColumnType
    {
        Same,
        Text,
        Integer,
        Number,
        DateTime,
        Logical,
        Complex
    }
    public abstract class DBTable : Record
    {
        [JsonIgnore]
        internal string _caption { get; set; }
        [JsonIgnore]
        public string Schema { private set; get; }
        [JsonIgnore]
        public string TableName { private set; get; }
        [JsonIgnore]
        public string TableCaption
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_caption))
                    return TableName;
                else
                    return _caption;
            }
            set
            {
                _caption = value;
            }
        }
        public DBTable(string TableName)
        {
            this.TableName = TableName;
            createColumnsInstance(this);
        }
        public DBTable(string TableName, string SchemaName)
        {
            this.TableName = TableName;
            this.Schema = SchemaName;
            createColumnsInstance(this);
        }
        private void createColumnsInstance(DBTable dBTable)
        {
            var tc = dBTable.GetType();
            var AllFields = tc.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Columns = new TableColumn[AllFields.Length];
            var fieldIndex = 0;

            foreach (var field in AllFields)
            {
                var actualfield = field.GetValue(dBTable);
                if (actualfield != null)
                {
                    Columns[fieldIndex] = actualfield as TableColumn;
                }
                else
                {
                    var columnName = field.GetCustomAttributes(typeof(ColumnDBNameAttribute), true).FirstOrDefault() as ColumnDBNameAttribute;
                    var constractor = field.FieldType.GetConstructor(new Type[] { typeof(string) });
                    var newColumn = constractor.Invoke(new string[] { columnName.ColumnName }) as TableColumn;
                    if (newColumn != null)
                        field.SetValue(dBTable, newColumn);
                    Columns[fieldIndex] = newColumn as TableColumn;
                }
                fieldIndex++;
            }

        }
        [JsonIgnore]
        public JoinType JoinType { internal set; get; }
        [JsonIgnore]
        public Condition JoinCondition { set; get; }
        [JsonIgnore]
        public int? OrdinalNumber { set; get; }
        public abstract TableColumn[] GetPrimaryKey();
        public static T GetById<T>(DbConnectionManager DbconnectionManager, params object[] KeysValues) where T : DBTable, new()
        {
            var t = new T();
            var selectOrder = new Select(DbconnectionManager)
                                        .One()
                                        .TableAdd(t, null, ColumnsSelection.All);

            int i = 0;
            foreach (var c in t.GetPrimaryKey())
            {
                switch (c.columnType)
                {
                    case ColumnType.Integer:
                        {
                            var key = c as IntegerColumn;
                            var value = int.Parse(KeysValues[i].ToString());
                            selectOrder.WhereAnd(key.Equal(value));
                            break;
                        }
                    case ColumnType.Number:
                        {
                            var key = c as NumberColumn;
                            var value = decimal.Parse(KeysValues[i].ToString());
                            selectOrder.WhereAnd(key.Equal(value));
                            break;
                        }
                    case ColumnType.Text:
                        {
                            var key = c as StringColumn;
                            var value = KeysValues[i].ToString();
                            selectOrder.WhereAnd(key.Equal(value));
                            break;
                        }
                    case ColumnType.DateTime:
                        {
                            var key = c as DateTimeColumn;
                            var value = DateTime.Parse(KeysValues[i].ToString());
                            selectOrder.WhereAnd(key.Equal(value));
                            break;
                        }
                    case ColumnType.Logical:
                        {
                            var key = c as LogicalColumn;
                            var value = bool.Parse(KeysValues[i].ToString());
                            selectOrder.WhereAnd(key.Equal(value));
                            break;
                        }
                }
                i++;
            }
            if (i == KeysValues.Length)
                return selectOrder.Run().Export<T>().FirstOrDefault();
            else
                return null;
        }
    }
    public abstract class TableColumn
    {
        internal bool IsValueOnly { get { return String.IsNullOrWhiteSpace(FieldName) && String.IsNullOrWhiteSpace(VirtualValue); } }
        internal bool IsVirtualValueOnly { get { return !String.IsNullOrWhiteSpace(VirtualValue); } }
        internal string _caption { get; set; }
        internal AggregateFunction _aggregateFunction { get; set; }

        [JsonIgnore]
        public DBTable ParentTable { get; internal set; }
        [JsonIgnore]
        public string FieldName { get; set; }
        [JsonIgnore]
        public string VirtualValue { get; set; }

        [JsonIgnore]
        public string TableName { get { return ParentTable?.TableName; } }
        [JsonIgnore]
        public string FieldFullName
        {
            get
            {
                var aggregatefunction = _aggregateFunction.ToString() + "({0})";
                string fullName;

                if (IsVirtualValueOnly)
                    fullName = VirtualValue;
                else
                {
                    if (ParentTable != null && !string.IsNullOrWhiteSpace(ParentTable.TableCaption))
                        fullName = $"\"{ParentTable.TableCaption}\".\"{FieldName}\"";
                    else
                        fullName = $"\"{FieldName}\"";
                }

                if (_aggregateFunction > AggregateFunction.None)
                    return fullName = string.Format(aggregatefunction, fullName);
                else return fullName;
            }
        }
        [JsonPropertyName("Column")]
        public string ColumnCaption
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_caption))
                    return FieldName;
                else
                    return _caption;
            }
            set
            {
                _caption = value;
            }
        }
        internal object _value { get; set; }
        internal ColumnType columnType { get; set; }
        public string ToNameOrParameter(Select select)
        {
            if (IsValueOnly)
            {
                int paramIndex = select.Parameters.Count() + 1;
                var paramName = "@" + paramIndex.ToString();
                var parameter = select.dbConnectionManager.CreateParameter(paramName, _value);
                select.Parameters.Add(parameter);
                return $"{paramName}";
            }
            else
            {
                if (FieldFullName != null)
                    return FieldFullName;
                else
                    return string.Empty;
            }
        }
        public override string ToString()
        {
            if (_value != null)
            {
                if (columnType == ColumnType.Text)
                    return $"'{_value.ToString()}'";

                if (columnType == ColumnType.Number)
                    return $"{((decimal)_value).ToString()}";

                if (columnType == ColumnType.Logical)
                    return $"{((bool)_value).ToString()}";

                if (columnType == ColumnType.Integer)
                    return $"{((int)_value).ToString()}";

                if (columnType == ColumnType.DateTime)
                    return $"'{((DateTime)_value).ToString("s")}'";
            }
            return null;
        }
        static public TableColumn Create(object Value)
        {
            var t = Value.GetType();
            if (t == typeof(string))
                return new StringColumn { Value = Value as string };
            if (t == typeof(decimal))
                return new NumberColumn { Value = (decimal)Value };
            if (t == typeof(bool))
                return new LogicalColumn { Value = (bool)Value };
            if (t == typeof(int))
                return new IntegerColumn { Value = (int)Value };
            if (t == typeof(DateTime))
                return new DateTimeColumn { Value = (DateTime)Value };
            return null;
        }

        [JsonPropertyName("Value")]
        public Object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        [JsonIgnore]
        public int? OrdinalNumber { get; set; }

    }
    public class StringColumn : TableColumn
    {
        public new string Value
        {
            get
            {
                return _value.ToString();
            }

            set
            {
                _value = value;
            }
        }
        public StringColumn(string caption = null)
        {
            if (!string.IsNullOrEmpty(caption))
            {
                FieldName = caption;
                ColumnCaption = caption;
            }
            columnType = ColumnType.Text;
        }

        public StringColumn(string Caption, string VirtualValue)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.Text;
            this.VirtualValue = VirtualValue;
        }

    }
    public class LogicalColumn : TableColumn
    {
        public new bool Value
        {
            get
            {
                bool bv = (bool)_value;
                return bv;
            }
            set
            {
                _value = value;
            }
        }
        public LogicalColumn(string caption = null)
        {
            if (!string.IsNullOrEmpty(caption))
            {
                FieldName = caption;
                ColumnCaption = caption;
            }
            columnType = ColumnType.Logical;
        }
        public LogicalColumn(string Caption, string VirtualValue)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.Logical;
            this.VirtualValue = VirtualValue;
        }
    }
    public class NumberColumn : TableColumn
    {
        public new decimal Value
        {
            get
            {
                decimal dv = (decimal)_value;
                return dv;
            }
            set
            {
                _value = value;
            }
        }
        public NumberColumn(string caption = null)
        {
            if (!string.IsNullOrEmpty(caption))
            {
                FieldName = caption;
                ColumnCaption = caption;
            }
            columnType = ColumnType.Number;
        }
        public NumberColumn(string Caption, string VirtualValue)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.Number;
            this.VirtualValue = VirtualValue;
        }
    }
    public class IntegerColumn : TableColumn
    {
        [JsonPropertyName("Value")]
        public new int Value
        {
            get
            {
                int iv = (int)_value;
                return iv;
            }
            set
            {
                _value = value;
            }
        }
        public IntegerColumn(string Caption = null)
        {
            if (!string.IsNullOrEmpty(Caption))
            {
                FieldName = Caption;
                this.ColumnCaption = Caption;
            }
            columnType = ColumnType.Integer;
        }
        public IntegerColumn(string Caption, string VirtualValue)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.Integer;
            this.VirtualValue = VirtualValue;
        }

    }
    public class DateTimeColumn : TableColumn
    {
        public new DateTime Value
        {
            get
            {
                DateTime dv = (DateTime)_value;
                return dv;
            }
            set
            {
                _value = value;
            }
        }
        public DateTimeColumn(string caption = null)
        {
            if (!string.IsNullOrEmpty(caption))
            {
                FieldName = caption;
                ColumnCaption = caption;
            }
            columnType = ColumnType.DateTime;
        }
        public DateTimeColumn(string Caption, string VirtualValue)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.DateTime;
            this.VirtualValue = VirtualValue;
        }
    }
    public class ComplexColumn : TableColumn
    {
        public new Select Value
        {
            get
            {
                Select sv = (Select)_value;
                return sv;
            }
            set
            {
                _value = value;
            }
        }
        public ComplexColumn(string Caption, Select Value)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.Complex;
            this.Value = Value;
        }

    }
    public static class ColumnExtention
    {
        static public TableColumn Clone(this TableColumn original, ColumnType TargetType = ColumnType.Same)
        {
            if (TargetType == ColumnType.Text ||
                TargetType == ColumnType.Same && original.columnType == ColumnType.Text)
                return new StringColumn(original.ColumnCaption)
                {
                    FieldName = original.FieldName,
                    ParentTable = original.ParentTable,
                    OrdinalNumber = original.OrdinalNumber,
                    VirtualValue = original.VirtualValue,
                    _value = original._value
                };

            if (TargetType == ColumnType.Number ||
                TargetType == ColumnType.Same && original.columnType == ColumnType.Number)
                return new NumberColumn(original.ColumnCaption)
                {
                    FieldName = original.FieldName,
                    ParentTable = original.ParentTable,
                    OrdinalNumber = original.OrdinalNumber,
                    VirtualValue = original.VirtualValue,
                    _value = original._value
                };

            if (TargetType == ColumnType.Integer ||
                TargetType == ColumnType.Same && original.columnType == ColumnType.Integer)
                return new IntegerColumn(original.ColumnCaption)
                {
                    FieldName = original.FieldName,
                    ParentTable = original.ParentTable,
                    OrdinalNumber = original.OrdinalNumber,
                    VirtualValue = original.VirtualValue,
                    _value = original._value
                };

            if (TargetType == ColumnType.Logical ||
                TargetType == ColumnType.Same && original.columnType == ColumnType.Logical)
                return new LogicalColumn(original.ColumnCaption)
                {
                    FieldName = original.FieldName,
                    ParentTable = original.ParentTable,
                    OrdinalNumber = original.OrdinalNumber,
                    VirtualValue = original.VirtualValue,
                    _value = original._value
                };

            if (TargetType == ColumnType.DateTime ||
                TargetType == ColumnType.Same && original.columnType == ColumnType.DateTime)
                return new DateTimeColumn(original.ColumnCaption)
                {
                    FieldName = original.FieldName,
                    ParentTable = original.ParentTable,
                    OrdinalNumber = original.OrdinalNumber,
                    VirtualValue = original.VirtualValue,
                    _value = original._value
                };

            return null;
        }

        static public IntegerColumn Count(this TableColumn original)
        {
            var c = original.Clone(ColumnType.Integer) as IntegerColumn;
            c._aggregateFunction = AggregateFunction.Count;
            return c;
        }

        static public NumberColumn Sum(this TableColumn original)
        {
            var c = original.Clone(ColumnType.Number) as NumberColumn;
            c._aggregateFunction = AggregateFunction.Sum;
            return c;
        }
        static public NumberColumn Max(this TableColumn original)
        {
            var c = original.Clone(ColumnType.Number) as NumberColumn;
            c._aggregateFunction = AggregateFunction.Max;
            return c;
        }
        static public NumberColumn Min(this TableColumn original)
        {
            var c = original.Clone(ColumnType.Number) as NumberColumn;
            c._aggregateFunction = AggregateFunction.Min;
            return c;
        }
        static public NumberColumn Avg(this TableColumn original)
        {
            var c = original.Clone(ColumnType.Number) as NumberColumn;
            c._aggregateFunction = AggregateFunction.Avg;
            return c;
        }
    }

    public static class TableExtention
    {
    }
}
