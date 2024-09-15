using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SQLDecorator
{
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
                            if (c.IsVirtualValueOnly && c.ParentTable.TableName == mapRecord.TableName)
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

        public static IEnumerable<T> Export<T>(this IEnumerable<ResultRecord> result, int TableOrdinalNumber) where T : DBTable, new()
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
                            if (c.ParentTable.OrdinalNumber == TableOrdinalNumber && mc.FieldName == c.FieldName)
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

        public static string ToJson(this IEnumerable<Record> result)
        {
            var sba = new StringBuilder();
            foreach (ResultRecord resultRecord in result)
            {
                if (sba.Length > 0) sba.Append(",");
                sba.Append("{");
                var sbr = new StringBuilder();
                foreach ( var c in resultRecord.Columns)
                {
                    if (sbr.Length > 0) sbr.Append(", ");
                    sbr.Append("\"").Append(c.ColumnCaption).Append("\":").Append(JsonSerializer.Serialize(c.Value));
                }
                sba.Append(sbr).Append("}");                                
            }                         
            string serialResult = "{["+sba.ToString()+"]}";
            return serialResult;
        }

    }
}
