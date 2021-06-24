using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class SettingsWrapper
    {
        public string WebView2FolderLocation { get; set; }

        public List<WebTab> WebTabs { get; set; } = new List<WebTab>();
    }
}
