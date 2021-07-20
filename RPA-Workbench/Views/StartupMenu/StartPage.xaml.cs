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
using ActiproSoftware.Windows.Controls.Docking;
using ActiproSoftware.Windows.Controls.Docking.Serialization;

namespace RPA_Workbench.Views.StartupMenu
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {

        public StartPage(ViewModels.StartupMenu.StartPageViewModel startPageViewModel)
        {
            InitializeComponent();

            DataContext = startPageViewModel;
            startPageViewModel.SetupRecentDocuments(RecentDocManager, RecentDocMenu);
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.ApplicationCommands.Open, startPageViewModel.OnOpenExecute));
        }

     
    }
}
