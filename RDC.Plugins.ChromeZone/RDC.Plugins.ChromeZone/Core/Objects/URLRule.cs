using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class URLRule
    {

        public string Field { get; set; }
        public string Match { get; set; }
        public string URL { get; set; }

        public List<Objects.FieldRule> FieldRules { get; set; } = new List<FieldRule>();
        public List<FieldConversion>FieldConversions { get; set; } = new List<FieldConversion>();
    }
}
