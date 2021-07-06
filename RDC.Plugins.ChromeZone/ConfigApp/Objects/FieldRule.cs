using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFormGenerator.Object;
using RDC.Plugins.ChromeZone.Core.Interfaces;

namespace ConfigApp.Objects
{
    public class FieldRule
    {
        [FormField]
        public string FieldName { get; set; }
        [FormField(ObjectTypeName = ObjectTypes.SpecialDropdown)]
        public string Operator { get; set; }
        [FormField]
        public string Value { get; set; }
        [FormField]
        public string Result { get; set; }
    }
}
