using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v2rayN.Mode
{
    class SubResponse
    {
        public string status { set; get; } = "";

        public string msg { set; get; } = "";

        public ResponseData data { set; get; } = new ResponseData();

        public class ResponseData
        {
            public string data { set; get; } = "";
        }
    }

}
