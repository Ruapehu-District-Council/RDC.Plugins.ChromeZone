using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class FieldConversion : Interfaces.IFieldConversion
    {
        public string FieldName { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

    }
}
