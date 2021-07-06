using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Objects
{
    public class URLRule : Interfaces.IURLRule
    {

        public string Field { get; set; }
        public string Match { get; set; }
        public string URL { get; set; }

        public List<Interfaces.IFieldRule> FieldRules { get; set; } = new List<Interfaces.IFieldRule>();
        public List<Interfaces.IFieldConversion>FieldConversions { get; set; } = new List<Interfaces.IFieldConversion>();
    }
}
