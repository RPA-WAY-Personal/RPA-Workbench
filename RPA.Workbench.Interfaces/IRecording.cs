using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Workbench.Interfaces
{
    public interface IRecording
    {
        string Name { get; }
        void Initialize();
        void Start();
        void Stop();
        event Action<IRecording, IRecordEvent> OnUserAction;
        bool parseUserAction(ref IRecordEvent e);
        Selector.treeelement[] GetRootEelements();
    }
}
