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
using ActiproSoftware.Windows.Controls.Ribbon;
using static RPA_Workbench.ViewModels.StartupMenu.StartPageViewModel;

namespace RPA_Workbench.Views.StartupMenu
{
    /// <summary>
    /// Interaction logic for ParentStartupWindow.xaml
    /// </summary>
    public partial class ParentStartupWindow : RibbonWindow
    {
        public ParentStartupWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            ViewModels.StartupMenu.ParentStartupViewModel parentStartupViewModel = new ViewModels.StartupMenu.ParentStartupViewModel();
            DataContext = parentStartupViewModel;
            parentStartupViewModel.LoadStartUpPage(this, mainWindow);
        }
    }
}
