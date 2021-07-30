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
using ActiproSoftware.Windows.Controls.Ribbon;
using ActiproSoftware.Windows.Themes;
using ActiproSoftware.Windows.Serialization;
using ActiproSoftware.Windows.Controls.Docking.Serialization;
using RPA_Workbench.Properties;

namespace RPA_Workbench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public ViewModels.WorkflowStudioIntegration.MainWindowViewModel mainWindowViewModel;
        public Views.BackstageMenu MainBackStageMenu = null;
        public MainWindow(String CurrentProjectPath = null, Views.BackstageMenu backstageMenuOptionalAssign = null)
        {
			LoadLayoutSerializer();
			InitializeComponent();
            MainBackStageMenu = backstageMenuOptionalAssign;
            // ViewModel.MainWindowViewModel mainWindowViewModel = new ViewModel.MainWindowViewModel();
            mainWindowViewModel = new ViewModels.WorkflowStudioIntegration.MainWindowViewModel(PropertiesToolWindow,OutlineToolwindow,OutputToolWindow,ErrorListToolWindow,DebuggerToolWindow, MainDockSite,MDIHost,DebuggerToolWindow,this, backstageMenuOptionalAssign, CurrentProjectPath);
            DataContext = mainWindowViewModel;
			//mainWindowViewModel.AddAllAddReferencesToFileToToolBox(CurrentProjectPath);
			//ThemeManager.CurrentTheme = ThemeNames.MetroDark;
			//ThemeManager.ApplyTheme();
			//ThemeManager.BeginUpdate(); 
			//ThemeManager.EndUpdate();
			SerializeSetupOnLoad();
			SetToolWindowOptions();
		}

        public void ReloadMainWindowModel(String CurrentProjectPath, Views.BackstageMenu backstageMenuOptionalAssign = null)
        {
            mainWindowViewModel = new ViewModels.WorkflowStudioIntegration.MainWindowViewModel(PropertiesToolWindow, OutlineToolwindow, OutputToolWindow, ErrorListToolWindow, DebuggerToolWindow, MainDockSite, MDIHost, DebuggerToolWindow, this, backstageMenuOptionalAssign, CurrentProjectPath);
            DataContext = mainWindowViewModel;
            mainWindowViewModel.AddAllAddReferencesToFileToToolBox(CurrentProjectPath);
			SetToolWindowOptions();

		}

		public void SetToolWindowOptions()
        {
			PropertiesToolWindow.CanClose = false;
			ProjectToolWindow.CanClose = false;
			ActivitiesToolWindow.CanClose = false;
			OutlineToolwindow.CanClose = false;
			OutputToolWindow.CanClose = false;
			ErrorListToolWindow.CanClose = false;
			DebuggerToolWindow.CanClose = false;
		}

		#region Layout Serializer
		//LAYOUT SERIALIZER

		void SerializeSetupOnLoad()
        {
			if (string.IsNullOrEmpty(Properties.Settings.Default.IDE_Serialization) == false)
			{
				//MessageBox.Show("Layout Setting Not Empty");
				this.LoadLayout(false);
			}

			else
			{
				this.LoadLayout(true);
				this.SaveLayout(false);
			}
		}
		private static DockSiteSerializationBehavior layoutSerializationBehavior = DockSiteSerializationBehavior.ToolWindowsOnly;
		private static string layoutXml = string.Empty;
		private static DockingWindowDeserializationBehavior windowDeserializationBehavior = DockingWindowDeserializationBehavior.Discard;

		private string defaultLayoutXml = string.Empty;
		private DockSiteLayoutSerializer layoutSerializer;


		private void LoadLayoutSerializer()
		{
			layoutSerializer = new DockSiteLayoutSerializer();
			layoutSerializer.DocumentWindowDeserializationBehavior = windowDeserializationBehavior;
			layoutSerializer.SerializationBehavior = layoutSerializationBehavior;
			layoutSerializer.ToolWindowDeserializationBehavior = windowDeserializationBehavior;
			// layoutSerializer.DockingWindowDeserializing += this.OnLayoutSerializerDockingWindowDeserializing;
		}
		private void LoadLayout(bool loadDefaultLayout)
		{
			try
			{
				if (loadDefaultLayout)
				{
					if (!string.IsNullOrEmpty(defaultLayoutXml))
					{
						layoutSerializer.LoadFromString(defaultLayoutXml, MainDockSite);
					}

				}
				else if (!string.IsNullOrEmpty(Properties.Settings.Default.IDE_Serialization))
				{
					layoutSerializer.LoadFromString(Properties.Settings.Default.IDE_Serialization, MainDockSite);
				}

			}
			catch (Exception)
			{
			}

		}
		private void SaveLayout(bool saveDefaultLayout)
		{
			layoutSerializationBehavior = (true ? DockSiteSerializationBehavior.ToolWindowsOnly : DockSiteSerializationBehavior.ToolWindowsOnly);
			layoutSerializer.SerializationBehavior = layoutSerializationBehavior;

			if (saveDefaultLayout)
			{
				defaultLayoutXml = layoutSerializer.SaveToString(MainDockSite);
			}
			else
			{
				layoutXml = layoutSerializer.SaveToString(MainDockSite);
				Properties.Settings.Default.IDE_Serialization = layoutXml;
			}
			Settings.Default.Save();

			//MessageBox.Show(Properties.Settings.Default.IDE_Serialization);
		}

        #endregion

        #region Window Events
        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
			SaveLayout(false);
		}

        private void MainDockSite_WindowsDragged(object sender, ActiproSoftware.Windows.Controls.Docking.DockingWindowsEventArgs e)
        {
			SaveLayout(false);
		}

        #endregion
    }
}
