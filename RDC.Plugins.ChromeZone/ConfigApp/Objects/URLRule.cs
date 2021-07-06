using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFormGenerator.Object;
using RDC.Plugins.ChromeZone.Core.Interfaces;

namespace ConfigApp.Objects
{
    [FormClass(DisplayName = "URL Rule", FormValueWidth = 200)]
    public class URLRule
    {
        [FormField(ObjectTypeName = ObjectTypes.SpecialDropdown)]
        public string Field { get; set; }
        [FormField]
        public string Match { get; set; }
        [FormField]
        public string URL { get; set; }

        [FormField(Type = Types.NestedList, DisplayName = "Field Rules")]
        public List<FieldRule> FieldRules { get; set; } = new List<FieldRule>();
        [FormField(Type = Types.NestedList, DisplayName = "Field Conversions")]
        public List<FieldConversion> FieldConversions { get; set; } = new List<FieldConversion>();
    }
}
