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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPA_Workbench.Views.StartupMenu
{
    /// <summary>
    /// Interaction logic for StartPageTemplateDialog.xaml
    /// </summary>
    public partial class StartPageTemplateDialog : UserControl
    {
        public StartPageTemplateDialog()
        {
            InitializeComponent();
            ViewModels.StartupMenu.StartPageViewModel startPageViewModel = new ViewModels.StartupMenu.StartPageViewModel();
            this.DataContext = startPageViewModel;
        }
    }
}
