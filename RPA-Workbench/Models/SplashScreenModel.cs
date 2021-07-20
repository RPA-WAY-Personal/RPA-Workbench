using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA_Workbench.Models
{
    public class SplashScreenModel: INotifyPropertyChanged
    {
        private int progress;

        public int Progress 
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
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
