using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Interfaces
{
    public interface IWebTab
    {
        string Name { get; set; }
        bool RefreshOnScreenChange { get; set; }
        bool RefreshOnRecordLoad { get; set; }
        bool BlockRefreshAfterNavigateAway { get; set; }

        string DefaultURL { get; set; }

        List<IURLRule> URLRules { get; set; }
    }
}
