using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class FieldRule
    {
        public string FieldName { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }
    }
}
