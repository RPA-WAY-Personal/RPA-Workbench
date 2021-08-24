using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RPA_Workbench.Views.StartupMenu;
namespace RPA_Workbench.ViewModels
{
    public class SplashScreenViewModel
    {
        Models.SplashScreenModel splashScreenModel = new Models.SplashScreenModel();
        private ParentStartupWindow localParentStartupWindow;
        private Views.SplashScreen localSplashScreen;
        private System.Windows.Forms.Timer SplashScreenLoader;
        public void InitializeApplication(Views.SplashScreen splashScreen, ParentStartupWindow parentStartupWindow)
        {
            SplashScreenLoader = new System.Windows.Forms.Timer();
            localParentStartupWindow = parentStartupWindow;
            localSplashScreen = splashScreen;
            StartupCheck();
            SplashScreenLoader.Tick += SplashScreenLoader_Tick;
            SplashScreenLoader.Interval = 25;
            SplashScreenLoader.Start();
        }

        private void SplashScreenLoader_Tick(object sender, EventArgs e)
        {
           
            if (splashScreenModel.Progress == 100)
            {
                SplashScreenLoader.Stop();
                localSplashScreen.Close();
                localParentStartupWindow.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                splashScreenModel.Progress++;
            }
        }

        private void StartupCheck()
        {
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RPA-Workbench")) == false)
            {

                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RPA-Workbench"));

            }
        }
    }
}
