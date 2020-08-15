using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Config
{
    public class UHubOptions
    {
        public int SyncRate { get; set; }

        public int TaskMaxExecCount { get; set; }

        public int TaskExpireAfter { get; set; }
    }
}
