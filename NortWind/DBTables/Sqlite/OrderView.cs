using SQLDecorator.WebQL;
using SQLDecorator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBTables.Sqlite
{
    [TableDBName("OrderView")]
    public class OrderView : DBTable
    {
        [ColumnDBName("OrderDetailID")]
        public IntegerColumn OrderDetailID;

        [ColumnDBName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnDBName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnDBName("Quantity")]
        public IntegerColumn Quantity;

        public OrderView() : base("OrderView")
        { }

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { OrderDetailID };
        }
    }

}
