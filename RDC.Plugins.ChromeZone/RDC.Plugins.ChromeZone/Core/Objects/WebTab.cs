using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class WebTab
    {

        public string Name { get; set; }
        public bool RefreshOnScreenChange { get; set; }
        public bool RefreshOnRecordLoad { get; set; }
        public bool BlockRefreshAfterNavigateAway { get; set; }

        public string DefaultURL { get; set; }

        public List<URLRule> URLRules { get; set; } = new List<URLRule>();
        
    }
}
