using System.Collections.Generic;

namespace SQLDecorator.WebQL
{
    public class WebQLReq
    {
        public List<string> Select { get; set; }
        public int? Top { get; set; }
        public List<WebQLCondition> Where{ get; set;}
      
    }
    
}
