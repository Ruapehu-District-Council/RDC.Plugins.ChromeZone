using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Interfaces
{
    public interface IFieldRule
    {
        string FieldName { get; set; }
        string Operator { get; set; }
        string Value { get; set; }
        string Result { get; set; }
    }
}
