using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWebApi.Models
{
    public class TransferModelReq
    {
        public string txnbrn { get; set; }
        public string txnacc { get; set; }
        public string txnccy { get; set; }
        public decimal txnamt { get; set; }
        public string txntrn { get; set; }
        public string offsetbrn { get; set; }
        public string offsetacc { get; set; }
        public string narrative { get; set; }
        public decimal chgamt { get; set; }
    }
}
