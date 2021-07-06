using AutoFormGenerator.Object;
using RDC.Plugins.ChromeZone.Core.Interfaces;

namespace ConfigApp.Objects
{
    public class FieldConversion
    {
        [FormField]
        public string FieldName { get; set; }
        [FormField]
        public string Type { get; set; }
        [FormField]
        public string Format { get; set; }
        [FormField]
        public string OldValue { get; set; }
        [FormField]
        public string NewValue { get; set; }

    }
}
