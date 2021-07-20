using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace RPA_Workbench.Models
{
    public class StartPageModel : INotifyPropertyChanged
    {

        private bool continueWithoutCodeVisibilty;
        private bool boolStartPageFirstOpen;
        public bool IsWithoutCodeVisible
        {
            get
            {
                return continueWithoutCodeVisibilty;
            }
            set
            {
                continueWithoutCodeVisibilty = value;
                OnPropertyChanged("BackstageClose");
            }
        }

        public bool StartPageFirstOpen
        {
            get
            {
                return boolStartPageFirstOpen;
            }
            set
            {
                boolStartPageFirstOpen = value;
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
