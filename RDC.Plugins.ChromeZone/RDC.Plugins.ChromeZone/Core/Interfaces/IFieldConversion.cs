using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDC.Plugins.ChromeZone.Core.Interfaces
{
    public interface IFieldConversion
    {
        string FieldName { get; set; }
        string Type { get; set; }
        string Format { get; set; }
        string OldValue { get; set; }
        string NewValue { get; set; }
    }
}
