using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteAPI
{
    public class MessageWrapper
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        public string AdditionalComment { get; set; }
    }
}
