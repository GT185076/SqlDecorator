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
            if (!NorthWind.DbConnectionsEstablisher.Connect())
                throw new Exception("Connection Not started");
        }

        [HttpGet("{resourceName}",Name = "NorthWind")]
        public string GetMeny([FromRoute] string resourceName,[FromBody] WebQLReq WebQL)
        {            
            var Resource = WebQLManager.WebQLDirectory[resourceName];
            return Resource.GetMeny(WebQL.Select.ToArray());
        }

        [HttpGet("{resourceName}/{identifier?}", Name = "NorthWind By Id")]
        public string Get([FromRoute] string resourceName,[FromRoute] string identifier, [FromBody] WebQLReq WebQL)
        {
            var Resource = WebQLManager.WebQLDirectory[resourceName];
            return Resource.Get(identifier, WebQL.Select.ToArray());
        }

    }
}