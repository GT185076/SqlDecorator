using CommonInfra.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SQLDecorator.WebQL
{
    public class WebQLReq : JsonFactory<WebQLReq>
    {            
        public List<string> Select { get; set; }          
    }
    
}
