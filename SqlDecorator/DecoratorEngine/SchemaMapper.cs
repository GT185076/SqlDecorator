using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommonInfra.Serialization;
using SQLDecorator.Providers;
using SQLDecorator.WebQL;

namespace SQLDecorator
{
    public enum BooleanOperator
    {      
        [Description("and")]
        And,
        [JsonPropertyName("or")]
        Or,
        [JsonPropertyName("not")]
        Not,
        [JsonPropertyName("andNot")]
        AndNot,
        [JsonPropertyName("orNot")]
        OrNot       
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
    public enum ComparisonOperator
    {      
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
        IsNot        
    }
    public enum OrderBy
    {
        Asc,
        Desc        
    }
    public abstract class Record 
    {
        [JsonPropertyName("Record")]
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
    public class Select 
    {
        string _compliedSentes;
        bool _compileDone;
        bool _isDistict;
        int  _Top;
        int  _fieldCount;
        internal bool IsOne { get; private set;}
        internal DbConnectionManager dbConnectionManager;              
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

        [JsonIgnore]
        public List<TableColumn> Columns { get; private set; }

        [JsonIgnore]
        public List<DbParameter> Parameters { get; internal set ;}

        [JsonPropertyName("Result")]
        public List<ResultRecord> Result { get; internal set;}
        public Select(DbConnectionManager DbconnectionManager)
        {
            dbConnectionManager = DbconnectionManager;
            Columns   = new List<TableColumn>();
            SelectedTables   = new List<DBTable>();
            WhereConditions  = new List<Condition>();
            HavingConditions = new List<Condition>();
            FieldsDictionary = new Dictionary<string, int>();
            GroupBy          = new List<TableColumn>();
            OrderBy          = new Dictionary<TableColumn,OrderBy>();
            Result           = new List<ResultRecord>();
            Parameters       = new List<DbParameter>();
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
            if (_Top ==1 ) IsOne = true;
            return this;
        }
        public Select One()
        {
            IsOne = true;
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
            string suffix = string.Empty;
            if (Table.OrdinalNumber > 0)
                suffix = $"_{Table.OrdinalNumber}";

            if (!string.IsNullOrEmpty(Caption) )
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}{suffix}";

            foreach (var f in fa)
            {
                var tc = f.GetValue(Table) as TableColumn;                
                tc.ParentTable = Table;
                if (ColumnsSelection== ColumnsSelection.All ) ColumnAdd(tc,$"{tc.ColumnCaption}{suffix}");
            }                     

            SelectedTables.Add(Table);

            return this;
        }

        public Select TableAdd<T>(T Table, string Caption = null,params string[] ColumnNames) where T : DBTable
        {
            _compileDone = false;
            var t = Table.GetType();
            var fa = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Table.OrdinalNumber = SelectedTables.Count;
            string suffix = string.Empty;
            if (Table.OrdinalNumber > 0)
                suffix = $"_{Table.OrdinalNumber}";

            if (!string.IsNullOrEmpty(Caption))
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}{suffix}";

            foreach (var cn in ColumnNames)
            {
                var f = fa.FirstOrDefault(x => x.Name == cn);
                if (f != null)
                {
                    var tc = f.GetValue(Table) as TableColumn;
                    tc.ParentTable = Table;
                    ColumnAdd(tc, $"{tc.ColumnCaption}{suffix}");
                }
                else
                    throw new Exception("Column not found :"+cn);
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

            string suffix = string.Empty;
            if (Table.OrdinalNumber > 0)
                suffix = $"_{Table.OrdinalNumber}";

            if (!string.IsNullOrEmpty(Caption))
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}{suffix}";

            foreach (var f in fa)
            {
                var tc = f.GetValue(Table) as TableColumn;
                tc.ParentTable = Table;
                if (ColumnsSelection == ColumnsSelection.All) ColumnAdd(tc, $"{tc.FieldName}{suffix}");            
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
            string suffix = string.Empty;
            if (Table.OrdinalNumber > 0)
                suffix = $"_{Table.OrdinalNumber}";

            if (!string.IsNullOrEmpty(Caption))
                Table.TableCaption = Caption;
            else
                Table.TableCaption = $"{Table.TableName}{suffix}";

            foreach (var f in fa)
            {
                var tc = f.GetValue(Table) as TableColumn;
                tc.ParentTable = Table;
                if (ColumnsSelection == ColumnsSelection.All) ColumnAdd(tc, $"{tc.FieldName}{suffix}");
            }

            Table.JoinType = JoinType.Inner;
            Table.JoinCondition = JoinCondition;         
            SelectedTables.Add(Table);

            return this;
        }
        public Select Where(Condition FilterCondition)
        {
            _compileDone = false;
            WhereAnd(FilterCondition);
            return this;
        }
        public Select Where(WebQLCondition[] Conditions)
        {
            _compileDone = false;
            foreach (var condition in Conditions)
            {
                var c = WhereWebConvert(condition);
                if (c.MainOperator == BooleanOperator.And) WhereAnd(c);
                else
                if (c.MainOperator == BooleanOperator.Or) WhereOr(c);        
                else
                if (c.MainOperator == BooleanOperator.AndNot) WhereAndNot(c);
                else
                throw new Exception(c.MainOperator.ToString() + "Logical operator is Not supported Yet in this context");
            }
            return this;
        }
        Condition WhereWebConvert(WebQLCondition Condition)
        {
            var c = new Condition();
            c.MainOperator = Condition.MainOperator;
            c.Operator = Condition.ComparisonOperator;
            c.parentSelect = this;

            if (Condition.FirstOperand != null)
                if (FieldsDictionary.ContainsKey(Condition.FirstOperand))
                c.FirstOperand = this.Columns[FieldsDictionary[Condition.FirstOperand]];

            if (Condition.SecondOperand != null)
                if (FieldsDictionary.ContainsKey(Condition.SecondOperand))
                    c.SecondOperand = this.Columns[FieldsDictionary[Condition.SecondOperand]];
                else
                {
                    if (c.FirstOperand.columnType == ColumnType.Text)
                        c.SecondOperand = new StringColumn() { Value = Condition.SecondOperand };
                    if (c.FirstOperand.columnType == ColumnType.Integer)
                        c.SecondOperand = new IntegerColumn() { Value = int.Parse(Condition.SecondOperand) };
                    if (c.FirstOperand.columnType == ColumnType.Number)
                        c.SecondOperand = new NumberColumn() { Value = decimal.Parse(Condition.SecondOperand) };
                    if (c.FirstOperand.columnType == ColumnType.DateTime)
                        c.SecondOperand = new DateTimeColumn() { Value = DateTime.Parse(Condition.SecondOperand) };
                    if (c.FirstOperand.columnType == ColumnType.Logical)
                        c.SecondOperand = new LogicalColumn() { Value = bool.Parse(Condition.SecondOperand) };
                }

            if (Condition.ThirdOperand != null)
                if (FieldsDictionary.ContainsKey(Condition.ThirdOperand))
                    c.SecondOperand = this.Columns[FieldsDictionary[Condition.ThirdOperand]];
                else
                {
                    if (c.FirstOperand.columnType == ColumnType.Text)
                        c.ThirdOperand = new StringColumn() { Value = Condition.ThirdOperand };
                    if (c.FirstOperand.columnType == ColumnType.Integer)
                        c.ThirdOperand = new IntegerColumn() { Value = int.Parse(Condition.ThirdOperand) };
                    if (c.FirstOperand.columnType == ColumnType.Number)
                        c.ThirdOperand = new NumberColumn() { Value = decimal.Parse(Condition.ThirdOperand) };
                    if (c.FirstOperand.columnType == ColumnType.DateTime)
                        c.ThirdOperand = new DateTimeColumn() { Value = DateTime.Parse(Condition.ThirdOperand) };
                    if (c.FirstOperand.columnType == ColumnType.Logical)
                        c.ThirdOperand = new LogicalColumn() { Value = bool.Parse(Condition.ThirdOperand) };
                }

            if (Condition.NextCondition != null)
                WhereWebConvert(Condition.NextCondition).prev = c;

            return c;
        }
        public Select Having(Condition HavingCondition)
        {
            _compileDone = false;
            HavingCondition.GetFirst().MainOperator = BooleanOperator.And;
            HavingCondition.parentSelect = this;
            HavingConditions.Add(HavingCondition);
            return this;
        }
        public Select WhereAnd(Condition FilterCondition)
        {
            _compileDone = false;
            FilterCondition.GetFirst().MainOperator = BooleanOperator.And;
            FilterCondition.parentSelect = this;
            WhereConditions.Add(FilterCondition);
            return this;
        }
        public Select WhereOr(Condition FilterCondition)
        {
            _compileDone = false;
            FilterCondition.GetFirst().MainOperator = BooleanOperator.Or;
            FilterCondition.parentSelect = this;
            WhereConditions.Add(FilterCondition);
            return this;
        }
        public Select WhereAndNot(Condition FilterCondition)
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
            return dbConnectionManager.Run(this, Parameters);            
        }
        public Task<IEnumerable<ResultRecord>> RunAsync()
        {
            if (!_compileDone) Compile();
            return dbConnectionManager.RunAsync(this,Parameters);
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
            string GROUPBY = "GROUP BY";
            string ORDERBY = "ORDER BY";
            string HAVING = "HAVING";

            StringBuilder sf = new StringBuilder();

            if (Columns.Count > 0) sf.Append(SELECT);
            if (_isDistict) sf.Append($" {DISTINCT}");
            if (_Top > 0)   sf.Append(dbConnectionManager .runner.Syntax.Top(_Top)) ;
            sf.Append("\n");
            bool isFirst = true;

            foreach (var f in Columns.FindAll((c)=> c.columnType!= ColumnType.Complex))
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
                        if (string.IsNullOrEmpty(t.Schema))
                            st.Append($" \"{t.TableName}\" ");
                        else
                            st.Append($" \"{t.Schema}\".\"{t.TableName}\" ");
                    }
                    else
                        st.Append(",\n").Append($" \"{t.Schema}\".\"{t.TableName}\" ");

                    if (!string.IsNullOrEmpty(t._caption))
                        st.Append($"\"{t._caption}\" ");
                }

                if (t.JoinType == JoinType.Inner)
                {
                    if (string.IsNullOrEmpty(t.Schema))
                        st.Append($"\n JOIN \"{t.TableName}\" ");
                    else
                        st.Append("\n").Append($" JOIN \"{t.Schema}\".\"{t.TableName}\" ");

                    if (!string.IsNullOrEmpty(t._caption))
                        st.Append($"\"{t._caption}\" ");

                    var OnCondition = t.JoinCondition.ToString();

                    st.Append($" ON {OnCondition} ");
                }

                if (t.JoinType == JoinType.Left)
                {
                    if (string.IsNullOrEmpty(t.Schema))
                        st.Append($"\n LEFT JOIN \"{t.TableName}\" ");
                    else
                        st.Append("\n").Append($" LEFT JOIN \"{t.Schema}\".\"{t.TableName}\" ");
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

            

            StringBuilder sfx = new StringBuilder();
            if (_Top > 0) sfx.Append("\n").Append(dbConnectionManager.runner.Syntax.Limit(_Top));

            _compliedSentes = sf.ToString() +
                              st.ToString() +
                              sw.ToString() +
                              gb.ToString() +
                              sh.ToString() +
                              ob.ToString() +
                              sfx.ToString();
            _compileDone = true;
            return _compliedSentes;
        }
        public string ToJson()
        {
            return Result.ToJson(IsOne);
        }
    }
    public class Condition 
    {
        internal Select parentSelect { get; set; }
        internal Condition prev;
        public BooleanOperator MainOperator { get; set; }
        public ComparisonOperator Operator { get; set; }
        public TableColumn FirstOperand { get; set; }
        public TableColumn SecondOperand { get; set; }
        public TableColumn ThirdOperand { get; set; }
        public Condition And(Condition NextCondition)
        {
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition And(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComparisonOperator.Is;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition.parentSelect = this.parentSelect;
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            return NextCondition;
        }
        public Condition Or(Condition NextCondition)
        {
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.Or;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition Not(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComparisonOperator.IsNot;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition Is(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComparisonOperator.Is;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition AndNot(Condition NextCondition)
        {
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.AndNot;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition AndNot(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComparisonOperator.IsNot;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.And;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition OrNot(Condition NextCondition)
        {
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.OrNot;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition OrNot(LogicalColumn logicalColumn)
        {
            Condition NextCondition = new Condition();
            NextCondition.Operator = ComparisonOperator.IsNot;
            NextCondition.FirstOperand = logicalColumn;
            NextCondition.prev = this;
            NextCondition.MainOperator = BooleanOperator.Or;
            NextCondition.parentSelect = this.parentSelect;
            return NextCondition;
        }
        public Condition GetFirst()
        {
            if (prev == null)
                return this;
            else
                return prev.GetFirst();
        }
        public string ToString(StringBuilder sentes = null)
        {
            string firstOperandName =   FirstOperand?.FieldFullName;

            if (sentes == null)
            {
                sentes = new StringBuilder();
                sentes.Append(")");
            }           

            if (Operator == ComparisonOperator.Is)
            {
                sentes.Insert(0, $"{firstOperandName}");
            }

            if (Operator == ComparisonOperator.IsNot)
            {
                sentes.Insert(0, $"not {firstOperandName}");
            }

            if (Operator == ComparisonOperator.IsNull)
            {
                sentes.Insert(0, $"{firstOperandName} is null");
            }

            if (Operator == ComparisonOperator.NotNull)
            {
                sentes.Insert(0, $"{firstOperandName} is not null");
            }

            if (Operator == ComparisonOperator.Equal)
            {
                sentes.Insert(0, $"{firstOperandName}={SecondOperand.ToNameOrParameter(parentSelect)}");
            }

            if (Operator == ComparisonOperator.NotEqual)
            {
                sentes.Insert(0, $"{firstOperandName}!={SecondOperand.ToNameOrParameter(parentSelect)}");
            }

            if (Operator == ComparisonOperator.GreaterThan)
            {
                sentes.Insert(0, $"{firstOperandName}>{SecondOperand.ToNameOrParameter(parentSelect)}");
            }

            if (Operator == ComparisonOperator.GreaterEqualThan)
            {
                    sentes.Insert(0, $"{firstOperandName}>={SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComparisonOperator.LessThan)
            {                
                    sentes.Insert(0, $"{firstOperandName}<{SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComparisonOperator.LessEqualThan)
            {                
                    sentes.Insert(0, $"{firstOperandName}<={SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComparisonOperator.Like)
            {                
                    sentes.Insert(0, $"{firstOperandName} like {SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComparisonOperator.NotLike)
            {                
                    sentes.Insert(0, $"{firstOperandName} not like {SecondOperand.ToNameOrParameter(parentSelect)}");             
            }

            if (Operator == ComparisonOperator.In)
            {
                sentes.Insert(0, $"{firstOperandName} in ({SecondOperand.ToNameOrParameter(parentSelect)})");
            }

            if (Operator == ComparisonOperator.NotIn)
            {
                sentes.Insert(0, $"{firstOperandName} not in ({SecondOperand.ToNameOrParameter(parentSelect)})");
            }

            if (Operator == ComparisonOperator.Between)
            {
                string statment = string.Empty;                
                statment = $"{firstOperandName} between {SecondOperand.ToNameOrParameter(parentSelect)}";                
                statment += $" and {ThirdOperand.ToNameOrParameter(parentSelect)}";                
                sentes.Insert(0, statment);
            }

            if (Operator == ComparisonOperator.NotBetween)
            {
                string statment = string.Empty;
                statment = $"{firstOperandName}\" not between {SecondOperand.ToNameOrParameter(parentSelect)}";
                statment += $" and {ThirdOperand.ToNameOrParameter(parentSelect)}";
                sentes.Insert(0, statment);
            }

            if (prev == null)
            {
                sentes.Insert(0, "(");
                if (prev == null &&
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
                return prev.ToString(sentes);
            }
        }
       
    }
    public static class ConditionExtention
    {
        static public Condition Equal(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Equal,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Equal,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Equal,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this LogicalColumn Lc, bool value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Equal,
                FirstOperand = Lc,
                SecondOperand = new LogicalColumn { Value = value }
            };
            return c;
        }
        static public Condition Equal(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Equal,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }       
        static public Condition Equal<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Equal,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition NotEqual(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotEqual,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotEqual,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotEqual,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this LogicalColumn Lc, bool value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotEqual,
                FirstOperand = Lc,
                SecondOperand = new LogicalColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotEqual,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition NotEqual<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotEqual,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition LessThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition LessThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition LessEqualThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessEqualThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessEqualThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessEqualThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessEqualThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition LessEqualThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.LessEqualThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition GreaterThan(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition Between(this StringColumn Sc, string FirstValue, string SecondValue)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Between,
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
                Operator = ComparisonOperator.NotBetween,
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
                Operator = ComparisonOperator.Between,
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
                Operator = ComparisonOperator.NotBetween,
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
                Operator = ComparisonOperator.Between,
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
                Operator = ComparisonOperator.NotBetween,
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
                Operator = ComparisonOperator.Between,
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
                Operator = ComparisonOperator.NotBetween,
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
                Operator = ComparisonOperator.GreaterEqualThan,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this NumberColumn Nc, decimal value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterEqualThan,
                FirstOperand = Nc,
                SecondOperand = new NumberColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this IntegerColumn Ic, int value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterEqualThan,
                FirstOperand = Ic,
                SecondOperand = new IntegerColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan(this DateTimeColumn dtc, DateTime value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterEqualThan,
                FirstOperand = dtc,
                SecondOperand = new DateTimeColumn { Value = value }
            };
            return c;
        }
        static public Condition GreaterEqualThan<T>(this T Tc, T SecondTc) where T : TableColumn
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.GreaterEqualThan,
                FirstOperand = Tc,
                SecondOperand = SecondTc
            };
            return c;
        }
        static public Condition Like(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Like,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }
        static public Condition NotLike(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotLike,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { Value = value }
            };
            return c;
        }

        static public Condition In(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.In,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { ColumnCaption="Array" ,VirtualValue = value }
            };
            return c;
        }
        static public Condition NotIn(this StringColumn Sc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotIn,
                FirstOperand = Sc,
                SecondOperand = new StringColumn { ColumnCaption="Array", VirtualValue = value }
            };
            return c;
        }

        static public Condition In(this IntegerColumn Ic, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.In,
                FirstOperand = Ic,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }
        static public Condition NotIn(this IntegerColumn Ic, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotIn,
                FirstOperand = Ic,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }

        static public Condition In(this NumberColumn Nc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.In,
                FirstOperand = Nc,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }
        static public Condition NotIn(this NumberColumn Nc, string value)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotIn,
                FirstOperand = Nc,
                SecondOperand = new StringColumn { ColumnCaption = "Array", VirtualValue = value }
            };
            return c;
        }

        static public Condition Not(this LogicalColumn Lc)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.IsNot,
                FirstOperand = Lc
            };
            return c;
        }
        static public Condition Is(this LogicalColumn Lc)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.Is,
                FirstOperand = Lc
            };

            return c;
        }
        static public Condition IsNull(this TableColumn Tc)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.IsNull,
                FirstOperand = Tc
            };
            return c;
        }
        static public Condition NotNull(this TableColumn Tc)
        {
            var c = new Condition
            {
                Operator = ComparisonOperator.NotNull,
                FirstOperand = Tc
            };
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
   
}
