using Microsoft.AspNetCore.Mvc;
using SQLDecorator.WebQL;
using SQLDecorator;

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
            if (!NorthWind.ConnectionsInstaller.IsStart)
                throw new Exception("Connection Not started");
        }

        [HttpGet(Name = "NorthWind")]
        public IEnumerable<Record> Get([FromBody] WebQLReq WebQL)
        {
            return new NorthWind.Views.Orders().GetOrders();
        }
       
    }
}