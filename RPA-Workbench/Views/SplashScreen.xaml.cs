using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RPA_Workbench.Views;
namespace RPA_Workbench.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen(StartupMenu.ParentStartupWindow parentStartupWindow)
        {
            InitializeComponent();
            ViewModels.SplashScreenViewModel splashScreenViewModel = new ViewModels.SplashScreenViewModel();
            splashScreenViewModel.InitializeApplication(this, parentStartupWindow);
            this.DataContext = splashScreenViewModel;
        }
    }
}
