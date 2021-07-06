using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Interfaces
{
    public interface ISettingsWrapper
    {
        string WebView2FolderLocation { get; set; }

        List<IWebTab> WebTabs { get; set; }
    }
}
