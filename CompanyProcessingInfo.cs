using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiService
{
    internal class CompanyProcessingInfo
    {
        public int CompanyId { get; set; }
        public DateTimeOffset NextProcessingTime { get; set; }
    }
}
