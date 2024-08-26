using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SQLDecorator.Composer;

namespace SQLDecorator
{
    public enum BooleanOperator
    {
        None,
        And,
        Or,
        Not,
        AndNot,
        OrNot       
    }
    public enum ColumnType
    {
        Same,
        Text,
        Integer,
        Number,
        DateTime,
        Logical
    }
    public enum AggregateFunction
    {
        None,
        Count,
        Sum,
        Max,
        Min,
        Avg
    }
    public enum JoinType
    {
        None,
        Inner,
        Left        
    }
    public enum ColumnsSelection
    {
        None,
        All
    }
    public enum ComperationOperator
    {
        None,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterEqualThan,
        LessThan,
        LessEqualThan,
        Between,
        NotBetween,
        Like,
        NotLike,
        In,
        NotIn,
        IsNull ,
        NotNull,
        Is,
        Not        
    }
    public enum OrderBy
    {
        Asc,
        Desc        
    }
    public abstract class Record
    {
        public TableColumn[] Columns { get; internal set; }
        public TableColumn this[string ColumnName]
        {
            get
            {
                return Columns.Single<TableColumn>(r => r.ColumnCaption == ColumnName);
            }
        }
        public TableColumn this[int ColumnIndex]
        {
            get
            {
                return Columns[ColumnIndex];
            }
        }
        public override string ToString()
        {
            var fieldsValues = new StringBuilder();
            foreach (var c in Columns)
                fieldsValues.Append(c.ToString()).Append("\t");
            return fieldsValues.ToString();
        }
    }
    public abstract class DBTable : Record
    {
        public abstract TableColumn[] GetPrimaryKey();
        internal string _caption { get; set; }
        public string Schema { private set; get; }
        public string TableName { private set; get; }      
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
                    var columnName = field.GetCustomAttributes(typeof(ColumnNameAttribute), true).FirstOrDefault() as ColumnNameAttribute;
                    var constractor = field.FieldType.GetConstructor(new Type[] { typeof(string) });
                    var newColumn = constractor.Invoke(new string[] { columnName.ColumnName }) as TableColumn;
                    if (newColumn != null)
                        field.SetValue(dBTable, newColumn);
                    Columns[fieldIndex] = newColumn as TableColumn;
                }
                fieldIndex++;
            }
            
        }
        public JoinType JoinType { internal set; get; }
        public Condition JoinCondition { set; get; }        
        public int? OrdinalNumber { set; get; }
       
    }
    public abstract class TableColumn
    {
        internal bool IsValueOnly { get { return String.IsNullOrWhiteSpace(FieldName) && String.IsNullOrWhiteSpace(VirtualValue); } }
        internal bool IsVirtualValueOnly { get { return !String.IsNullOrWhiteSpace(VirtualValue); } }
        internal string _caption { get; set; }
        internal AggregateFunction _aggregateFunction { get; set; }
        public DBTable ParentTable { get; internal set; }
        public string FieldName { get; set; }
        public string VirtualValue { get; set; }
        public string FieldFullName { get
            {
                var aggregatefunction = _aggregateFunction.ToString() + "({0})";
                string fullName;

                if (IsVirtualValueOnly)
                    fullName = VirtualValue;
                else
                {
                    if (ParentTable != null)
                        fullName = $"\"{ParentTable.TableCaption}\".\"{FieldName}\"";
                    else
                        fullName = $"\"{FieldName}\"";
                }

                if (_aggregateFunction> AggregateFunction.None)
                     return  fullName= string.Format(aggregatefunction, fullName);
                else return  fullName;
            }
        }
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
                var parameter = new SqlParameter(paramName, _value);
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
        public int? OrdinalNumber { get; set; }
        internal SqlParameter parameter { get; set; }
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
            columnType = ColumnType.Number;
        }
        public IntegerColumn(string Caption ,string VirtualValue)
        {
            this.ColumnCaption = Caption;
            columnType = ColumnType.Number;
            this.VirtualValue = VirtualValue;
        }

    }
    public class DateTimeColumn : TableColumn
    {
        public new DateTime Value
        {
            get
            {
                DateTime iv = (DateTime)_value;
                return iv;
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
    public class Select
    {
        string _compliedSentes;
        bool _compileDone;
        bool _isDistict;
        int  _Top;
        int  _fieldCount;
        DbConnectionManager _dbConnectionManager;              
        List<TableColumn> GroupBy { get; set; }
        Dictionary<TableColumn,OrderBy> OrderBy { get; set; }
        Dictionary<string,int> FieldsDictionary { get; set; }
        internal ResultRecord FeatchNextRecord(DbDataReader reader)
        {
            var record = new TableColumn[_fieldCount];
            int i = 0;
            foreach (var f in Columns)
            {
                record[i] = (f.Clone());
                i++;
            }
            
            var resultRecord = new ResultRecord(reader,record);
            Result.Add(resultRecord);
            return resultRecord;
        }
        List<DBTable> SelectedTables { get; set; }
        List<Condition> WhereConditions { get; set; }
        List<Condition> HavingConditions { get; set; }
        public List<TableColumn> Columns { get; private set; }
        public List<SqlParameter> Parameters { get; internal set ;}
        public List<ResultRecord> Result { get; internal set;}
        public Select(DbConnectionManager DbconnectionManager)
        {
            _dbConnectionManager = DbconnectionManager;
            Columns   = new List<TableColumn>();
            SelectedTables   = new List<DBTable>();
            WhereConditions  = new List<Condition>();
            HavingConditions = new List<Condition>();
            FieldsDictionary = new Dictionary<string, int>();
            GroupBy          = new List<TableColumn>();
            OrderBy          = new Dictionary<TableColumn,OrderBy>();
            Result           = new List<ResultRecord>();
            Parameters       = new List<SqlParameter>();
        }
        public Select Distict()
        {
            _compileDone = false;
            _isDistict = true;
            return this;
        }
        public Select Top(int RowsCount)
        {
            _compileDone = false;
            _Top = RowsCount;
            return this;
        }
        public Select ColumnAdd(TableColumn tc, string Caption = null)
        {
            _compileDone = false;
            if (!string.IsNullOrEmpty(Caption)) 
                tc.ColumnCaption = Caption;
           
            Columns.Add(tc);
            FieldsDictionary.Add(tc.ColumnCaption, _fieldCount++);
            return this;
        }
        public Select ColumnAdd(params TableColumn[] tc)
        {
            _compileDone = false;
            foreach (var c in tc)
            {
                Columns.Add(c);
                FieldsDictionary.Add(c.ColumnCaption, _fieldCount++);
            }
            return this;
        }
        public Select OrderByAdd(TableColumn tc, OrderBy By= SQLDecorator.OrderBy.Asc)
        {
            _compileDone = false;
            OrderBy.Add(tc,By);
            return this;
        }
        public Select OrderByAdd(OrderBy By,params TableColumn[] tc)
        {
            _compileDone = false;
            foreach ( var c in tc)
                    OrderBy.Add(c, By);
            return this;
        }
        public Select GroupByAdd(params TableColumn[] tc)
        {
            _compileDone = false;
            foreach (var c  in tc)
                    GroupBy.Add(c);
            return this;
        }
        public Select TableAdd<T>(T Table, string Caption = null , ColumnsSelection ColumnsSelection = ColumnsSelection.None) where T : DBTable
        {
            _compileDone = false;
            var t = Table.GetType();
            var fa = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Table.OrdinalNumber = SelectedTables.Count;

            if (!string.IsNullOrEmpty(Caption))
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}_{Table.OrdinalNumber}";

            foreach (var f in fa)
            {
                var tc = f.GetValue(Table) as TableColumn;                
                tc.ParentTable = Table;
                if (ColumnsSelection== ColumnsSelection.All ) ColumnAdd(tc,$"{tc.ColumnCaption}_{Table.OrdinalNumber}");
            }                     

            SelectedTables.Add(Table);

            return this;
        }
        public Select TableLeftJoin<T>(T Table,string Caption, Condition JoinCondition,ColumnsSelection ColumnsSelection = ColumnsSelection.None) where T : DBTable
        {
            _compileDone = false;
            var t = Table.GetType();
            var fa = t.GetFields();
            Table.OrdinalNumber = SelectedTables.Count;

            if (!string.IsNullOrEmpty(Caption))
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}_{Table.OrdinalNumber}";

            foreach (var f in fa)
            {
                var tc = f.GetValue(Table) as TableColumn;
                tc.ParentTable = Table;
                if (ColumnsSelection == ColumnsSelection.All) ColumnAdd(tc, $"{tc.FieldName}_{Table.OrdinalNumber}");            
            }
           
            Table.JoinType = JoinType.Left;
            Table.JoinCondition = JoinCondition;
            SelectedTables.Add(Table);
            return this;
        }
        public Select TableJoin<T>(T Table, string Caption, Condition JoinCondition, ColumnsSelection ColumnsSelection = ColumnsSelection.None) where T : DBTable
        {
            _compileDone = false;
            var t = Table.GetType();
            var fa = t.GetFields();
            Table.OrdinalNumber = SelectedTables.Count;

            if (!string.IsNullOrEmpty(Caption))
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}_{Table.OrdinalNumber}";

            foreach (var f in fa)
            {
                var tc = f.GetValue(Table) as TableColumn;
                tc.ParentTable = Table;
                if (ColumnsSelection == ColumnsSelection.All) ColumnAdd(tc, $"{tc.FieldName}_{Table.OrdinalNumber}");
            }

            Table.JoinType = JoinType.Inner;
            Table.JoinCondition = JoinCondition;         
            SelectedTables.Add(Table);

            return this;
        }
        public Select Where(Condition FilterCondition)
        {
            _compileDone = false;
            And(FilterCondition);
            return this;
        }
        public Select Having(Condition HavingCondition)
        {
            _compileDone = false;
            HavingCondition.GetFirst().MainOperator = BooleanOperator.And;
            HavingCondition.parentSelect = this;
            HavingConditions.Add(HavingCondition);
            return this;
        }
        public Select And(Condition FilterCondition)
        {
            _compileDone = false;
            FilterCondition.GetFirst().MainOperator = BooleanOperator.And;
            FilterCondition.parentSelect = this;
            WhereConditions.Add(FilterCondition);
            return this;
        }
        public Select Or(Condition FilterCondition)
        {
            _compileDone = false;
            FilterCondition.GetFirst().MainOperator = BooleanOperator.Or;
            FilterCondition.parentSelect = this;
            WhereConditions.Add(FilterCondition);
            return this;
        }
        public Select AndNot(Condition FilterCondition)
        {
            _compileDone = false;
            FilterCondition.GetFirst().MainOperator = BooleanOperator.Not;
            FilterCondition.parentSelect = this;
            WhereConditions.Add(FilterCondition);
            return this;
        }
        public override string ToString()
        {
            if (!_compileDone) Compile();
            return _compliedSentes;
        }    
        public IEnumerable<ResultRecord> Run()
        {
            if (!_compileDone) Compile();
            return _dbConnectionManager.runner.Run(this,_dbConnectionManager.DbConnection,Parameters);            
        }
        public Task<IEnumerable<ResultRecord>> RunAsync()
        {
            if (!_compileDone) Compile();
            return _dbConnectionManager.runner.RunAsync(this, _dbConnectionManager.DbConnection,Parameters);
        }   
        public string CaptionsToString()
        {
            var captions = new StringBuilder();
            foreach (var c in Columns)
                captions.Append(c.ColumnCaption).Append("\t");
            captions.AppendLine();
            foreach (var c in Columns)
                captions.Append(string.Empty.PadLeft(c.ColumnCaption.Length, '-')).Append("\t");
            return captions.ToString();
        }
        public string ParametersToString()
        {
            if (!_compileDone) Compile();
            var param = new StringBuilder();
            foreach (var p in Parameters)
                param.Append(p.ToString()).Append(" = ").Append(p.Value).Append("\n");            
            return param.ToString();
        }
        string Compile()
        {
            string SELECT = "SELECT";
            string DISTINCT = "DISTINCT";
            string FROM = "FROM";
            string WHERE = "WHERE";
            string TOP = "TOP";
            string GROUPBY = "GROUP BY";
            string ORDERBY = "ORDER BY";
            string HAVING = "HAVING";

            StringBuilder sf = new StringBuilder();

            if (Columns.Count > 0) sf.Append(SELECT);
            if (_isDistict) sf.Append($" {DISTINCT}");
            if (_Top > 0) sf.Append($" {TOP}({_Top})");
            sf.Append("\n");
            bool isFirst = true;

            foreach (var f in Columns)
            {
                if (isFirst == true)
                    sf.Append($" {f.FieldFullName} ");
                else
                    sf.Append(",\n").Append($" {f.FieldFullName} ");

                if (!string.IsNullOrEmpty(f.ColumnCaption))
                    sf.Append($"\"{f.ColumnCaption}\" ");

                isFirst = false;
            }

            StringBuilder st = new StringBuilder();
            foreach (var t in SelectedTables.OrderBy((x) => x.JoinType))
            {
                if (t.JoinType == JoinType.None)
                {
                    if (st.Length == 0)
                    {
                        st.Append("\n").Append(FROM);
                        st.Append($" \"{t.Schema}\".\"{t.TableName}\" ");
                    }
                    else
                        st.Append(",\n").Append($" \"{t.Schema}\".\"{t.TableName}\" ");

                    if (!string.IsNullOrEmpty(t._caption))
                        st.Append($"\"{t._caption}\" ");
                }

                if (t.JoinType == JoinType.Inner)
                {
                    st.Append("\n").Append($"JOIN \"{t.Schema}\".\"{t.TableName}\" ");
                    if (!string.IsNullOrEmpty(t._caption))
                        st.Append($"\"{t._caption}\" ");

                    var OnCondition = t.JoinCondition.ToString();

                    st.Append($" ON {OnCondition} ");
                }

                if (t.JoinType == JoinType.Left)
                {
                    st.Append("\n").Append($"LEFT JOIN \"{t.Schema}\".\"{t.TableName}\" ");
                    if (!string.IsNullOrEmpty(t._caption))
                        st.Append($"\"{t._caption}\" ");

                    var OnCondition = t.JoinCondition.ToString();

                    st.Append($" ON {OnCondition} ");
                }
            }

            StringBuilder sw = new StringBuilder();
            foreach (var wc in WhereConditions)
            {
                if (sw.Length == 0)
                {
                    sw.Append("\n").Append(WHERE);
                    sw.Append($" {wc.ToString()}");
                }
                else
                    sw.Append($" {wc.MainOperator.ToString()} {wc.ToString()}");
            }

            StringBuilder sh = new StringBuilder();
            foreach (var hc in HavingConditions)
            {
                if (sh.Length == 0)
                {
                    sh.Append("\n").Append(HAVING);
                    sh.Append($" {hc.ToString()}");
                }
                else
                    sh.Append($" {hc.MainOperator.ToString()} {hc.ToString()}");
            }

            StringBuilder gb = new StringBuilder();
            foreach (var g in GroupBy)
            {
                if (gb.Length == 0)
                {
                    gb.Append("\n").Append(GROUPBY);
                    gb.Append($" {g.FieldFullName}");
                }
                else
                    gb.Append($",{g.FieldFullName}");
            }

            StringBuilder ob = new StringBuilder();
            foreach (var o in OrderBy)
            {
                if (ob.Length == 0)
                {
                    ob.Append("\n").Append(ORDERBY);
                    ob.Append($" {o.Key.FieldFullName} {o.Value.ToString()}");
                }
                else
                    ob.Append($",{o.Key.FieldFullName} {o.Value.ToString()}");
            }

            _compliedSentes = sf.ToString() +
                              st.ToString() +
                              sw.ToString() +
                              gb.ToString() +
                              sh.ToString() +
                              ob.ToString();
            _compileDone = true;
            return _compliedSentes;
        }
    }
    public class Condition
    {
        internal Select parentSelect { get; set; }
        Condition _prev;
        public BooleanOperator MainOperator { get; set; }
        public ComperationOperator Operator { get; set; }
        public TableColumn FirstOperand { get; set; }
        public TableColumn SecondOperand { get; set; }
        public TableColumn ThirdOperand { get; set; }
        public Condition And(Condition NextCondition)
        {
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition And(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComperationOperator.Is;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition.parentSelect = this.parentSelect;
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            return NextCondition;
        }
        public Condition Or(Condition NextCondition)
        {
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.Or;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition Not(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComperationOperator.Not;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition Is(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComperationOperator.Is;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition AndNot(Condition NextCondition)
        {
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.AndNot;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition AndNot(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComperationOperator.Not;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition OrNot(Condition NextCondition)
        {
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.OrNot;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition OrNot(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComperationOperator.Not;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition._prev = this;
            NextCondition.MainOperator = BooleanOperator.Or;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition GetFirst()
        {
            if (_prev == null)
                return this;
            else
                return _prev.GetFirst();
        }
        public string ToString(StringBuilder sentes = null)
        {
            string firstOperandName =   FirstOperand?.FieldFullName;

            if (sentes == null)
            {
                sentes = new StringBuilder();
                sentes.Append(")");
            }

            if (Operator == ComperationOperator.None)
            {
                sentes.Insert(0, $"{firstOperandName}");
            }

            if (Operator == ComperationOperator.Is)
            {
                sentes.Insert(0, $"{firstOperandName}");
            }

            if (Operator == ComperationOperator.Not)
            {
                sentes.Insert(0, $"not {firstOperandName}");
            }

            if (Operator == ComperationOperator.IsNull)
            {
                sentes.Insert(0, $"{firstOperandName} is null");
            }

            if (Operator == ComperationOperator.NotNull)
            {
                sentes.Insert(0, $"{firstOperandName} is not null");
            }

            if (Operator == ComperationOperator.Equal)
            {
                sentes.Insert(0, $"{firstOperandName}={SecondOperand.ToNameOrParameter(parentSelect)}");
            }

            if (Operator == ComperationOperator.NotEqual)
            {
                sentes.Insert(0, $"{firstOperandName}!={SecondOperand.ToNameOrParameter(parentSelect)}");
            }

            if (Operator == ComperationOperator.GreaterThan)
            {
                sentes.Insert(0, $"{firstOperandName}>{SecondOperand.ToNameOrParameter(parentSelect)}");
            }

            if (Operator == ComperationOperator.GreaterEqualThan)
            {
                    sentes.Insert(0, $"{firstOperandName}>={SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComperationOperator.LessThan)
            {                
                    sentes.Insert(0, $"{firstOperandName}<{SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComperationOperator.LessEqualThan)
            {                
                    sentes.Insert(0, $"{firstOperandName}<={SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComperationOperator.Like)
            {                
                    sentes.Insert(0, $"{firstOperandName} like {SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComperationOperator.NotLike)
            {                
                    sentes.Insert(0, $"{firstOperandName} not like {SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComperationOperator.In)
            {
                sentes.Insert(0, $"{firstOperandName} in ({SecondOperand.ToNameOrParameter(parentSelect)})");
            }

            if (Operator == ComperationOperator.NotIn)
            {
                sentes.Insert(0, $"{firstOperandName} not in ({SecondOperand.ToNameOrParameter(parentSelect)})");
            }

            if (Operator == ComperationOperator.Between)
            {
                string statment = string.Empty;                
                statment = $"{firstOperandName} between {SecondOperand.ToNameOrParameter(parentSelect)}";                
                statment += $" and {ThirdOperand.ToNameOrParameter(parentSelect)}";                
                sentes.Insert(0, statment);
            }

            if (Operator == ComperationOperator.NotBetween)
            {
                string statment = string.Empty;
                statment = $"{firstOperandName}\" not between {SecondOperand.ToNameOrParameter(parentSelect)}";
                statment += $" and {ThirdOperand.ToNameOrParameter(parentSelect)}";
                sentes.Insert(0, statment);
            }

            if (_prev == null)
            {
                sentes.Insert(0, "(");
                if (_prev == null &&
                    MainOperator == BooleanOperator.Not) sentes.Insert(0, " not ");
                return sentes.ToString();
            }

            else
            {
                if (MainOperator == BooleanOperator.And) sentes.Insert(0, " and ");
                if (MainOperator == BooleanOperator.Or) sentes.Insert(0, " or ");
                if (MainOperator == BooleanOperator.Not) sentes.Insert(0, " not ");
                if (MainOperator == BooleanOperator.AndNot) sentes.Insert(0, " and not ");
                if (MainOperator == BooleanOperator.OrNot) sentes.Insert(0, " or not ");
                return _prev.ToString(sentes);
            }
        }
       
    }
    public static class ConditionExtention
    {
        static public Condition Equal(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Equal,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Equal,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Equal,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this LogicalColumn Lc, bool value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Equal,
                FirstOperand = Lc,
                SecondOperand = new LogicalColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Equal,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }       
        static public Condition Equal<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Equal,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition NotEqual(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotEqual,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotEqual,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotEqual,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this LogicalColumn Lc, bool value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotEqual,
                FirstOperand = Lc,
                SecondOperand = new LogicalColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotEqual,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotEqual,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition LessThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition LessEqualThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessEqualThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessEqualThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessEqualThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessEqualThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComperationOperator.LessEqualThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition GreaterThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition Between(this StringColumn Sc, string FirstValue, string SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Between,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = FirstValue },
                ThirdOperand = new StringColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition NotBetween(this StringColumn Sc, string FirstValue, string SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotBetween,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = FirstValue },
                ThirdOperand = new StringColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition Between(this NumberColumn Nc, decimal FirstValue, decimal SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Between,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = FirstValue },
                ThirdOperand = new NumberColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition NotBetween(this NumberColumn Nc, decimal FirstValue, decimal SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotBetween,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = FirstValue },
                ThirdOperand = new NumberColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition Between(this IntegerColumn Ic, int FirstValue, int SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Between,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = FirstValue },
                ThirdOperand = new IntegerColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition NotBetween(this IntegerColumn Ic, int FirstValue, int SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotBetween,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = FirstValue },
                ThirdOperand = new IntegerColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition Between(this DateTimeColumn dtc, DateTime FirstValue, DateTime SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Between,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = FirstValue },
                ThirdOperand = new DateTimeColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition NotBetween(this DateTimeColumn dtc, DateTime FirstValue, DateTime SecondValue)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotBetween,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = FirstValue },
                ThirdOperand = new DateTimeColumn { Value = SecondValue }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterEqualThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterEqualThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterEqualThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterEqualThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComperationOperator.GreaterEqualThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition Like(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Like,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition NotLike(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotLike,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }

        static public Condition In(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.In,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { ColumnCaption="Array" ,VirtualValue = value }
            };
            return c;
        }
        static public Condition NotIn(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotIn,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { ColumnCaption="Array", VirtualValue = value }
            };
            return c;
        }

        static public Condition In(this IntegerColumn Ic, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.In,
                FirstOperand = Ic,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }
        static public Condition NotIn(this IntegerColumn Ic, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotIn,
                FirstOperand = Ic,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }

        static public Condition In(this NumberColumn Nc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.In,
                FirstOperand = Nc,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }
        static public Condition NotIn(this NumberColumn Nc, string value)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotIn,
                FirstOperand = Nc,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }

        static public Condition Not(this LogicalColumn Lc)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Not,
                FirstOperand = Lc
            };
            return c;
        }
        static public Condition Is(this LogicalColumn Lc)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.Is,
                FirstOperand = Lc
            };

            return c;
        }
        static public Condition IsNull(this TableColumn Tc)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.IsNull,
                FirstOperand = Tc
            };
            return c;
        }
        static public Condition NotNull(this TableColumn Tc)
        {
            var c = new Condition
            {
                Operator = ComperationOperator.NotNull,
                FirstOperand = Tc
            };
            return c;
        }
    }
    static public class ColumnExtention
    {
        static public TableColumn Clone(this TableColumn original,ColumnType TargetType= ColumnType.Same)
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
    public class ResultRecord : Record
    {
        internal ResultRecord(DbDataReader Reader, TableColumn[] Columns)
        {
            this.Columns = Columns;
            foreach (var f in Columns)
            {

                if (f.columnType == ColumnType.Text)
                {
                    var sc = f as StringColumn;
                    sc.Value = Reader[f.ColumnCaption].ToString();
                }
                if (f.columnType == ColumnType.Number)
                {
                    var nc = f as NumberColumn;
                    decimal d = 0m;
                    decimal.TryParse(Reader[f.ColumnCaption].ToString(), out d);
                    nc.Value = d;
                }
                if (f.columnType == ColumnType.Integer)
                {
                    var ic = f as IntegerColumn;
                    int i = 0;
                    int.TryParse(Reader[f.ColumnCaption].ToString(), out i);
                    ic.Value = i;
                }
                if (f.columnType == ColumnType.Logical)
                {
                    var lc = f as LogicalColumn;
                    lc.Value = Reader[f.ColumnCaption].ToString() == "True";
                }
                if (f.columnType == ColumnType.DateTime)
                {
                    var dtc = f as DateTimeColumn;
                    DateTime dt;
                    DateTime.TryParse(Reader[f.ColumnCaption].ToString(), out dt);
                    dtc.Value = dt;
                }
            }
        }
    }
    public static class ResultExtention
    {
        public static IEnumerable<T> Export<T>(this IEnumerable<ResultRecord> result) where T : DBTable, new()
        {
            var mapRecord = new T();
            var mt = mapRecord.GetType();
            var ft = mt.GetFields();

            foreach (ResultRecord resultRecord in result)
            {
                var newRecord = new T();
                foreach (var f in ft)
                {
                    var mc = f.GetValue(mapRecord) as TableColumn;
                    var nc = f.GetValue(newRecord) as TableColumn;

                    if (mc.OrdinalNumber != null)
                        nc.Value = resultRecord.Columns[(int)mc.OrdinalNumber].Value;
                    else
                    {
                        int i = 0;
                        foreach (var c in resultRecord.Columns)
                        {
                            if (!c.IsVirtualValueOnly && c.ParentTable.TableName == mapRecord.TableName && mc.FieldName == c.FieldName)
                            {
                                nc._value = c._value;
                                mc.OrdinalNumber = i;
                            }
                            if (c.IsVirtualValueOnly && c.ParentTable.TableName == mapRecord.TableName )
                            {
                                if (mc.FieldFullName == c.FieldFullName)
                                {
                                    nc._value = c._value;
                                    mc.OrdinalNumber = i;
                                }
                            }
                            i++;
                        }
                    }
                }
                yield return newRecord;
            }
        }

        public static IEnumerable<T> Export<T>(this IEnumerable<ResultRecord> result,int TableOrdinalNumber) where T : DBTable, new()
        {
            var mapRecord = new T();
            var mt = mapRecord.GetType();
            var ft = mt.GetFields();

            foreach (ResultRecord resultRecord in result)
            {
                var newRecord = new T();
                foreach (var f in ft)
                {
                    var mc = f.GetValue(mapRecord) as TableColumn;
                    var nc = f.GetValue(newRecord) as TableColumn;

                    if (mc.OrdinalNumber != null)
                        nc.Value = resultRecord.Columns[(int)mc.OrdinalNumber].Value;
                    else
                    {
                        int i = 0;
                        foreach (var c in resultRecord.Columns)
                        {
                            if (c.ParentTable.OrdinalNumber==TableOrdinalNumber && mc.FieldName == c.FieldName)
                            {
                                nc._value = c._value;
                                mc.OrdinalNumber = i;
                            }
                            i++;
                        }
                    }
                }
                yield return newRecord;
            }
        }
    }

}
