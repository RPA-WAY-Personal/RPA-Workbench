using ActiproSoftware.Windows.Themes;
using ActiproSoftware.Windows.Themes.Generation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RPA_Workbench
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void ThemeSetup()
        {
            // Configure the Actipro theme manager
            ThemeManager.BeginUpdate();
            try
            {
                // Register the theme definitions for your application
                ThemeManager.RegisterThemeDefinition(new ThemeDefinition("Custom")
                {
                    ColorPaletteKind = ColorPaletteKind.Office,
                    ArrowKind = ArrowKind.FilledTriangle,
                    DockGuideColorFamilyName = ColorFamilyName.Green,
                    PreviewTabColorFamilyName = ColorFamilyName.Gray,
                    PrimaryAccentColorFamilyName = ColorFamilyName.Green,
                    StatusBarBackgroundKind = StatusBarBackgroundKind.Accent,
                    WindowColorFamilyName = ColorFamilyName.Green,
                    Intent = ThemeIntent.White,
                    WindowTitleBarBackgroundKind = WindowTitleBarBackgroundKind.Accent
                    
                });

                // Use the Actipro styles for native WPF controls that look great with Actipro's control products
                ThemeManager.AreNativeThemesEnabled = true;

                // Set the current app theme via a registered theme definition name
                ThemeManager.CurrentTheme = "Custom";
            }
            finally
            {
                ThemeManager.EndUpdate();
            }

            // Your other app startup code here
        }

        Views.BackstageMenu BackstageMenu = new Views.BackstageMenu();
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeSetup();
            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow("",BackstageMenu);
            
            //mainWindow.MainRibbon.IsApplicationMenuOpen = true;
            mainWindow.Visibility = Visibility.Hidden;
            mainWindow.Hide();
         
            Views.StartupMenu.ParentStartupWindow parentStartupWindow = new Views.StartupMenu.ParentStartupWindow(mainWindow);
            RPA_Workbench.App.Current.MainWindow = parentStartupWindow;
            parentStartupWindow.Visibility = Visibility.Hidden;
            parentStartupWindow.Hide();
            Views.SplashScreen splashScreen = new Views.SplashScreen(parentStartupWindow);
            splashScreen.Show();
        }
    }
}
