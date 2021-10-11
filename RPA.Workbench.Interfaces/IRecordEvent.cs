using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Workbench.Interfaces
{
    public interface IRecordEvent
    {
        // AutomationElement Element { get; set; }
        UIElement UIElement { get; set; }
        IElement Element { get; set; }
        Selector.Selector Selector { get; set; }
        IBodyActivity a { get; set; }
        bool SupportInput { get; set; }
        bool SupportSelect { get; set; }
        RPA.Workbench.Input.MouseButton Button { get; set; }
        bool ClickHandled { get; set; }
        bool SupportVirtualClick { get; set; }
        int X { get; set; }
        int Y { get; set; }
        int OffsetX { get; set; }
        int OffsetY { get; set; }
    }
}
