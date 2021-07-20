using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RPA_Workbench.Views.StartupMenu;
using System.Windows;
namespace RPA_Workbench.ViewModels.StartupMenu
{
    public class ParentStartupViewModel
    {
        private StartPageViewModel startPageViewModel = new StartPageViewModel(); // Page0 VIEWMODEL
        
        private bool PageControlsVisible;
        //Start Pages 
        static private MainWindow localMainwindow;
        static private ParentStartupWindow localParentStartupWindow = new ParentStartupWindow(localMainwindow);
        static public Models.ParentStartupWindowModel parentStartupWindowModel = new Models.ParentStartupWindowModel(); 
        static private StartPage startPage; //Page 0
        static public StartPageTemplateDialog startPageTemplateDialog = new StartPageTemplateDialog(); //Page 1
        static public StartPageCreateFromTemplateDialog startPageCreateFromTemplateDialog = new StartPageCreateFromTemplateDialog(); //Page2
        public void LoadStartUpPage(Views.StartupMenu.ParentStartupWindow parentStartupWindow, MainWindow mainWindow)
        {
            PageControlsVisibility = true; // Hides the Next/Back buttons, as they are not needed at this stage
            
            localMainwindow = mainWindow;
            localParentStartupWindow = parentStartupWindow;
            startPageViewModel.LoadStartPage(parentStartupWindow, mainWindow);
        }

        public bool PageControlsVisibility
        {
            get { return PageControlsVisible; }
            set { PageControlsVisible = value; }
        }


      
    }
}
