using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using AutoFormGenerator.Object;
using RDC.Plugins.ChromeZone.Core.Interfaces;

namespace ConfigApp.Objects
{
    [FormClass(DisplayName = "Web Tab")]
    public class WebTab
    {
        [FormField]
        public string Name { get; set; }
        [FormField(DisplayName = "Refresh On Screen Change")]
        public bool RefreshOnScreenChange { get; set; }
        [FormField(DisplayName = "Refresh On Record Load")]
        public bool RefreshOnRecordLoad { get; set; }
        [FormField(DisplayName = "Block Refresh After NavigateA way")]
        public bool BlockRefreshAfterNavigateAway { get; set; }
        [FormField(DisplayName = "Default URL")]
        public string DefaultURL { get; set; }
        [FormField(Type = Types.NestedList, DisplayName = "URL Rules")]
        public List<URLRule> URLRules { get; set; } = new List<URLRule>();
        
    }
}
