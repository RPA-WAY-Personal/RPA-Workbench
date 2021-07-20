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
using RPA_Workbench.ViewModels;
namespace RPA_Workbench.Views
{
    /// <summary>
    /// Interaction logic for ProjectView.xaml
    /// </summary>
    public partial class ProjectView : UserControl
    {
        public ProjectView(ProjectWindowViewModel projectWindowViewModel, MainWindow mainWindow)
        {
            InitializeComponent();
            ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu contextMenu = new ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu();
            projectWindowViewModel.ProjectWindowHookup(SolutionTreeViewBox, mainWindow, contextMenu,mainWindow.mainWindowViewModel);
            this.DataContext = projectWindowViewModel;
        }
    }
}
