using Microsoft.AspNetCore.Mvc;
using SQLDecorator.WebQL;
using DBTables.Sqlite;
using SQLDecorator;
using DBTables.MsSql;
using Microsoft.Data.Sqlite;

namespace WebQL.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NorthWindController : ControllerBase
    {
      
        private readonly ILogger<NorthWindController> _logger;

        public NorthWindController(ILogger<NorthWindController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "NorthWind")]
        public IEnumerable<Record> Get([FromBody] WebQLReq WebQL)
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "Nortwind_Sqlight.db";
            builder.DataSource = ":memory:";
            var northWind2 = new DBTables.Sqlite.NorthWind2(builder.ConnectionString);

            var View1 = new DBTables.Sqlite.View1();
            var selectOrder = new Select(northWind2).TableAdd(View1, null, ColumnsSelection.All);
            
            var viewRecord = selectOrder.Run().Export<DBTables.Sqlite.View1>();
            return viewRecord;
        }
       
    }
}