using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDC.Plugins.ChromeZone.Core.Interfaces;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class SettingsWrapper : Interfaces.ISettingsWrapper
    {
        public string WebView2FolderLocation { get; set; }

        public List<IWebTab> WebTabs { get; set; } = new List<IWebTab>(new List<WebTab>());
    }
}
