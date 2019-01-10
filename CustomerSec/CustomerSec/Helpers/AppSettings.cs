using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerSec.Helpers
{
    public class AppSettings
    {
        public string JwtKey { get; set; }
        public string JwtIssue { get; set; }
        public string JwtExpireDays { get; set; }
    }
}
