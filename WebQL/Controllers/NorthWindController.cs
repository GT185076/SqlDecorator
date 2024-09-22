using Microsoft.AspNetCore.Mvc;
using WebQL.Models;

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
        public IEnumerable<string> Get([FromBody] List<string> reqBudy)
        {
            return reqBudy;            
        }
       
    }
}