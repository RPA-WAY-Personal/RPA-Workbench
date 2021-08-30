using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPA_Workbench.Views.StartupMenu;
using System.Windows.Input;
using System.Windows;
using Newtonsoft.Json.Linq;
using RPA_Workbench.Utilities;
using System.IO;
using System.ComponentModel;
using RPA_Workbench.Project_Types;
using static RPA_Workbench.Project_Types.WorkflowTypes;
using System.Windows.Media.Imaging;
using ActiproSoftware.Windows.DocumentManagement;
using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using System.Windows.Forms;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using RPA_Workbench.ViewModels;
using RPA_Workbench.Properties;
using System.Windows.Media;

namespace RPA_Workbench.ViewModels.BackstageMenuTabs
{
    public class SettingsMenuViewModel : INotifyPropertyChanged
    {
        #region Variables
        bool _minimizeOnRun;
        #endregion

        #region Constructors
        //Constructor

        public SettingsMenuViewModel()
        {
            //Set checkbox value to saved setting value
            MinimizeOnRunBool = RPA_Workbench.Properties.Settings.Default.Setting_MinimizeOnRun;
            RelaySetup();
        }

        void RelaySetup()
        {
            MinimizeOnRunCommand = new RelayCommand(new Action<object>(MinimizeOnRun));
        }

        #endregion

        #region Properties
        
        public bool MinimizeOnRunBool
        {
            get { return _minimizeOnRun; }
            set { _minimizeOnRun = value; }
        }

        #endregion

        #region ICommands


        private ICommand iMinimizeOnRun;
        public ICommand MinimizeOnRunCommand
        {
            get
            {
                return iMinimizeOnRun;
            }
            set
            {
                iMinimizeOnRun = value;
            }
        }

        #endregion

        #region ICommand Functions/Methods
        public void MinimizeOnRun(object parameter)
        {
            if (MinimizeOnRunBool == true)
            {
                RPA_Workbench.Properties.Settings.Default.Setting_MinimizeOnRun = true;
            }
            else
            {
                RPA_Workbench.Properties.Settings.Default.Setting_MinimizeOnRun = false;
            }
            RPA_Workbench.Properties.Settings.Default.Save();
            // 
        }
        #endregion

        #region Events

        #endregion

        #region Methods/Functions

        #endregion

        #region PropertyChangedEvents
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}

