using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class URLRule
    {
        public string URL { get; set; }

        public List<Rule> Rules { get; set; } = new List<Rule>();
        public List<FieldRule> FieldRules { get; set; } = new List<FieldRule>();
        public List<FieldConversion> FieldConversions { get; set; } = new List<FieldConversion>();
    }
}
