using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RPA_Workbench.Views.StartupMenu;
using System.Windows;
using ActiproSoftware.Windows.Themes;
using ActiproSoftware.Windows.Themes.Generation;

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

            ThemeManager.BeginUpdate();
            try
            {
                // Register the theme definitions for your application
                ThemeManager.RegisterThemeDefinition(new ThemeDefinition("StartupWindowTheme")
                {
                    ColorPaletteKind = ColorPaletteKind.Office,
                    ArrowKind = ArrowKind.FilledTriangle,
                    DockGuideColorFamilyName = ColorFamilyName.Green,
                    PreviewTabColorFamilyName = ColorFamilyName.Gray,
                    PrimaryAccentColorFamilyName = ColorFamilyName.Green,
                    WindowColorFamilyName = ColorFamilyName.Green,
                    Intent = ThemeIntent.White,
                    WindowTitleBarBackgroundKind = WindowTitleBarBackgroundKind.Window

                });

                // Use the Actipro styles for native WPF controls that look great with Actipro's control products
                ThemeManager.AreNativeThemesEnabled = true;
                ThemeManager.SetTheme(parentStartupWindow, "StartupWindowTheme");
                // Set the current app theme via a registered theme definition name
                //ThemeManager.CurrentTheme = "Custom";
            }
            finally
            {
                ThemeManager.EndUpdate();
            }
        }

        public bool PageControlsVisibility
        {
            get { return PageControlsVisible; }
            set { PageControlsVisible = value; }
        }


      
    }
}
