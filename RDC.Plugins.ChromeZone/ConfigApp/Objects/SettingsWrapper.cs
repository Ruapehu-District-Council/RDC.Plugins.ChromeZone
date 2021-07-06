using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFormGenerator.Object;
using RDC.Plugins.ChromeZone.Core.Interfaces;

namespace ConfigApp.Objects
{
    public class SettingsWrapper
    {
        [FormField(DisplayName = "Resources Folder")]
        public string WebView2FolderLocation { get; set; }

        [FormField(Type = Types.NestedList)]
        public List<WebTab> WebTabs { get; set; } = new List<WebTab>();
    }
}
