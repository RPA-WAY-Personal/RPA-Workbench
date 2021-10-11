using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPA.Workbench.Interfaces.entity;

namespace RPA.Workbench.Interfaces
{
    public delegate void DetectorDelegate(IDetectorPlugin plugin, IDetectorEvent detector, EventArgs e);
    public interface IDetectorPlugin : INotifyPropertyChanged, IPlugin
    {
        void Initialize(IOpenRPAClient client, Detector Entity);
        Detector Entity { get; set; }
        event DetectorDelegate OnDetector;
        void Start();
        void Stop();
    }
}
