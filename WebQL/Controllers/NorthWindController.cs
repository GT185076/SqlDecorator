using Microsoft.AspNetCore.Mvc;
using SQLDecorator.WebQL;
using SQLDecorator;
using NorthWind;

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
            NorthWind.DbConnectionsEstablisher.Connect();            
        }

        [HttpGet("{resourceName}",Name = "NorthWind")]
        public string GetMeny([FromRoute] NWResources NWR,[FromBody] WebQLReq WebQL)
        {            
            var Resource = WebQLDao.WebQLDirectory[NWR.ToString()];
            return Resource.GetMeny(WebQL.Select?.ToArray(),WebQL.Top,WebQL.Where?.ToArray());
        }

        [HttpGet("{resourceName}/{identifier?}", Name = "NorthWind By Id")]
        public string Get([FromRoute] NWResources NWR, [FromRoute] string identifier, [FromBody] WebQLReq WebQL)
        {
            var Resource = WebQLDao.WebQLDirectory[NWR.ToString()];
            return Resource.Get(identifier, WebQL.Select?.ToArray());
        }

    }
}