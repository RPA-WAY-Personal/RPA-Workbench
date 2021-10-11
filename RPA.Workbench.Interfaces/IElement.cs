using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Workbench.Interfaces
{
    public interface IElement
    {
        object RawElement { get; set; }
        System.Drawing.Rectangle Rectangle { get; set; }
        string Value { get; set; }
        string Name { get; set; }
        void Focus();
        void Click(bool VirtualClick, RPA.Workbench.Input.MouseButton Button, int OffsetX, int OffsetY, bool DoubleClick, bool AnimateMouse);
        Task Highlight(bool Blocking, System.Drawing.Color Color, TimeSpan Duration);
        string ImageString();
        IElement[] Items { get; }
    }
}
