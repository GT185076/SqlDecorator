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
    public class WebQL : JsonFactory<WebQL>
    {
        [JsonIgnore]
        public string Schema { get; set; }
        [JsonIgnore]
        public string From   { get; set; }
        public List<string> Select { get; set; }        
        public List<Condition> Where { get; set; }
        public Dictionary<string,OrderBy> OrderBy { get; set; }

    }
    
}
