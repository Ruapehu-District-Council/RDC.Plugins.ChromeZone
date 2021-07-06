using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Interfaces
{
    public interface IURLRule
    {
        string Field { get; set; }
        string Match { get; set; }
        string URL { get; set; }

        List<IFieldRule> FieldRules { get; set; }
        List<IFieldConversion> FieldConversions { get; set; }
    }
}
