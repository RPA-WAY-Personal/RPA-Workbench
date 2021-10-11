using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPA.Workbench.Interfaces;

namespace RPA.Workbench.Interfaces
{
    public interface IDetectorEvent
    {
        IElement element { get; set; }
        string host { get; set; }
        string fqdn { get; set; }
        string result { get; set; }
        // TokenUser user { get; set; }
    }
}
