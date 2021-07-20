using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace RPA_Workbench.Models
{
    public class BackstageMenuModel : INotifyPropertyChanged
    {
        private bool BackstageClose;
        private bool BackStageOpen;
        public bool CanBackstageClose
        {
            get
            {
                return BackstageClose;
            }
            set
            {
                BackstageClose = value;
                OnPropertyChanged("BackstageClose");
            }
        }

        public bool IsBackStageOpen
        {
            get
            {
                return BackStageOpen;
            }
            set
            {
                BackStageOpen = value;
                OnPropertyChanged("IsBackStageOpen");
            }
        }


        #region INotifyPropertyChanged Members  

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
