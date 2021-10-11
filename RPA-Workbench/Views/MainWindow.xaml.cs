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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActiproSoftware.Windows.Controls.Ribbon;
using ActiproSoftware.Windows.Themes;
using ActiproSoftware.Windows.Serialization;
using ActiproSoftware.Windows.Controls.Docking.Serialization;
using RPA_Workbench.Properties;
using RPA.Workbench.Input;
using RPA.Workbench.Interfaces;

namespace RPA_Workbench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
		internal static MainWindow instance;
		public ViewModels.WorkflowStudioIntegration.MainWindowViewModel mainWindowViewModel;
        public Views.BackstageMenu MainBackStageMenu = null;
		SolidColorBrush NormalBGColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF217346"));
		SolidColorBrush NormalFGColor = new SolidColorBrush(Colors.White);
		SolidColorBrush SelectedBGColor = new SolidColorBrush(Colors.White);
		SolidColorBrush SelectedFGColor = new SolidColorBrush(Colors.Black);
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
            MainRibbon.SelectedTabChanged += MainRibbon_SelectedTabChanged;


			foreach (var item in MainRibbon.Tabs)
			{
				if (MainRibbon.SelectedTab.Label == item.Label)
				{
					MainRibbon.SelectedTab.Background = SelectedBGColor;
					MainRibbon.SelectedTab.Foreground = SelectedFGColor;
					MainRibbon.SelectedTab.BorderBrush = new SolidColorBrush(Colors.White);
					
				}
				else
				{
					item.Background = NormalBGColor;
					item.Foreground = NormalFGColor;
				}
			}
			//}
			//foreach (var item in MainRibbon.Tabs)
			//         {
			//	MessageBox.Show(item.Label);
			//         }
		}


        private void MainRibbon_SelectedTabChanged(object sender, ActiproSoftware.Windows.Controls.Ribbon.Controls.TabPropertyChangedRoutedEventArgs e)
        {
			//SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Red);
			//MainRibbon.SelectedTab.Background = solidColorBrush;
			//MainRibbon.SelectedTab.Foreground = solidColorBrush;

		

            //MessageBox.Show(MainRibbon.SelectedTab.Label);
            foreach (var item in MainRibbon.Tabs)
            {
                if (MainRibbon.SelectedTab.Label == item.Label)
                {
					MainRibbon.SelectedTab.Background = SelectedBGColor;
					MainRibbon.SelectedTab.Foreground = SelectedFGColor;
					MainRibbon.SelectedTab.BorderBrush = new SolidColorBrush(Colors.White);
				}
                else
                {
					item.Background = NormalBGColor;
					item.Foreground = NormalFGColor;
				}
            }
			//if (MainRibbon.SelectedTab.Label == "Design")
			//{
				
			//}
			//else if (MainRibbon.SelectedTab.Label == "THISTAB")
   //         {
			//	MainRibbon.SelectedTab.Background = SelectedBGColor;
			//	MainRibbon.SelectedTab.Foreground = SelectedFGColor;

			//	MainRibbon.Tabs[0].Background = NormalBGColor;
			//	SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Black);
			//	MainRibbon.SelectedTab.Foreground = solidColorBrush2;
			//}
		}

        public void ReloadMainWindowModel(String CurrentProjectPath, Views.BackstageMenu backstageMenuOptionalAssign = null)
        {
            mainWindowViewModel = new ViewModels.WorkflowStudioIntegration.MainWindowViewModel(PropertiesToolWindow, OutlineToolwindow, OutputToolWindow, ErrorListToolWindow, DebuggerToolWindow, MainDockSite, MDIHost, DebuggerToolWindow, this, backstageMenuOptionalAssign, CurrentProjectPath);
            DataContext = mainWindowViewModel;
            mainWindowViewModel.AddAllAddReferencesToFileToToolBox(CurrentProjectPath);
			SetToolWindowOptions();
			MainRibbon.SelectedTabChanged += MainRibbon_SelectedTabChanged;
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
		//public object SelectedContent
		//{
		//	get
		//	{
		//		if (this.MainDockSite == null) return null;
		//		var b = MainDockSite.ActiveWindow;
		//		return b;
		//	}
		//}

		//internal void OnRecord(object _item)
		//{
		//	//if (!(SelectedContent is Views.WFDesigner.WFDesigner)) return;
		//	//Log.FunctionIndent("MainWindow", "OnRecord");
		//	//try
		//	//{
		//	//	var designer = (Views.WFDesigner.WFDesigner)SelectedContent;
		//	//	designer.ReadOnly = true;
		//	//	designer.Lastinserted = null;
		//	//	designer.Lastinsertedmodel = null;
		//	//	StopDetectorPlugins();
		//	//	InputDriver.Instance.OnKeyDown += OnKeyDown;
		//	//	InputDriver.Instance.OnKeyUp += OnKeyUp;
		//	//	StartRecordPlugins(true);
		//	//	InputDriver.Instance.CallNext = false;
		//	//	if (this.Minimize) GenericTools.Minimize();
		//	//}
		//	//catch (Exception ex)
		//	//{
		//	//	Log.Error(ex.ToString());
		//	//}
		//	//Log.FunctionOutdent("MainWindow", "OnRecord");
		//}
		//bool isRecording;
		//RPA.Workbench.Interfaces.Overlay.OverlayWindow _overlayWindow = null;
		//private void StartRecordPlugins(bool all)
		//{
		//	Log.FunctionIndent("MainWindow", "StartRecordPlugins");
		//	try
		//	{
		//		isRecording = true;
		//		var p = Plugins.recordPlugins.Where(x => x.Name == "Windows").First();
		//		p.OnUserAction += OnUserAction;
		//		if (Config.local.record_overlay) p.OnMouseMove += OnMouseMove;
		//		p.Start();
		//		if (_overlayWindow == null && Config.local.record_overlay)
		//		{
		//			_overlayWindow = new RPA.Workbench.Interfaces.Overlay.OverlayWindow(true)
		//			{
		//				BackColor = System.Drawing.Color.PaleGreen,
		//				Visible = true,
		//				TopMost = true
		//			};
		//		}

		//		p = Plugins.recordPlugins.Where(x => x.Name == "SAP").FirstOrDefault();
		//		if (p != null && (all == true || all == false))
		//		{
		//			p.OnUserAction += OnUserAction;
		//			if (Config.local.record_overlay) p.OnMouseMove += OnMouseMove;
		//			p.Start();
		//		}

		//	}
		//	catch (Exception ex)
		//	{
		//		Log.Error(ex.ToString());
		//	}
		//	Log.FunctionOutdent("MainWindow", "StartRecordPlugins");
		//}
		//private void StopRecordPlugins(bool all)
		//{
		//	Log.FunctionIndent("MainWindow", "StopRecordPlugins");
		//	try
		//	{
		//		isRecording = false;
		//		var p = Plugins.recordPlugins.Where(x => x.Name == "Windows").First();
		//		p.OnUserAction -= OnUserAction;
		//		if (Config.local.record_overlay) p.OnMouseMove -= OnMouseMove;
		//		p.Stop();

		//		p = Plugins.recordPlugins.Where(x => x.Name == "SAP").FirstOrDefault();
		//		if (p != null && (all == true || all == false))
		//		{
		//			p.OnUserAction -= OnUserAction;
		//			p.Stop();
		//		}

		//		if (_overlayWindow != null)
		//		{
		//			GenericTools.RunUI(_overlayWindow, () =>
		//			{
		//				try
		//				{
		//					_overlayWindow.Visible = true;
		//					_overlayWindow.Dispose();
		//				}
		//				catch (Exception ex)
		//				{
		//					Log.Error(ex.ToString());
		//				}
		//			});
		//		}
		//		_overlayWindow = null;
		//	}
		//	catch (Exception ex)
		//	{
		//		Log.Error(ex.ToString());
		//	}
		//	Log.FunctionOutdent("MainWindow", "StopRecordPlugins");
		//}
		//public void OnMouseMove(IRecordPlugin sender, IRecordEvent e)
		//{
		//	if (!Config.local.record_overlay) return;
		//	foreach (var p in Plugins.recordPlugins)
		//	{
		//		if (p.Name != sender.Name)
		//		{
		//			if (p.ParseMouseMoveAction(ref e)) break;
		//		}
		//	}

		//	// e.Element.Highlight(false, System.Drawing.Color.PaleGreen, TimeSpan.FromSeconds(1));
		//	if (e.Element != null && _overlayWindow != null)
		//	{

		//		GenericTools.RunUI(_overlayWindow, () =>
		//		{
		//			try
		//			{
		//				if (_overlayWindow != null)
		//				{
		//					_overlayWindow.Visible = true;
		//					_overlayWindow.Bounds = e.Element.Rectangle;
		//				}
		//			}
		//			catch (Exception)
		//			{
		//			}
		//		});
		//	}
		//	else if (_overlayWindow != null)
		//	{
		//		GenericTools.RunUI(_overlayWindow, () =>
		//		{
		//			try
		//			{
		//				_overlayWindow.Visible = false;
		//			}
		//			catch (Exception)
		//			{
		//			}
		//		});
		//	}
		//}
		//public void OnUserAction(IRecordPlugin sender, IRecordEvent e)
		//{
		//	Log.FunctionIndent("MainWindow", "OnUserAction");
		//	if (sender.Name == "Windows") StopRecordPlugins(false);
		//	AutomationHelper.syncContext.Post(o =>
		//	{
		//		IPlugin plugin = sender;
		//		try
		//		{
		//			if (sender.Name == "Windows")
		//			{
		//				// TODO: Add priotrity, we could create an ordered list in config ?
		//				foreach (var p in Plugins.recordPlugins)
		//				{
		//					if (p.Name != sender.Name)
		//					{
		//						try
		//						{
		//							if (p.ParseUserAction(ref e))
		//							{
		//								plugin = p;
		//								break;
		//							}
		//						}
		//						catch (Exception ex)
		//						{
		//							Log.Error(ex.ToString());
		//						}
		//					}
		//				}
		//			}
		//			if (e.a == null)
		//			{
		//				if (sender.Name == "Windows") StartRecordPlugins(false);
		//				if (e.ClickHandled == false)
		//				{
		//					NativeMethods.SetCursorPos(e.X, e.Y);
		//					InputDriver.Click(e.Button);
		//				}
		//				Log.Function("MainWindow", "OnUserAction", "Action is null");
		//				return;
		//			}
		//			if (SelectedContent is MainWindowViewModel.workspaceViewModel.WorkflowDesigner view)
		//			{

		//				var VirtualClick = Config.local.use_virtual_click;
		//				if (!e.SupportVirtualClick) VirtualClick = false;
		//				e.a.AddActivity(new RPA.Workbench.Windows.ClickElement
		//				{
		//					Element = new System.Activities.InArgument<IElement>()
		//					{
		//						Expression = new Microsoft.VisualBasic.Activities.VisualBasicValue<IElement>("item")
		//					},
		//					OffsetX = e.OffsetX,
		//					OffsetY = e.OffsetY,
		//					Button = (int)e.Button,
		//					VirtualClick = VirtualClick,
		//					AnimateMouse = Config.local.use_animate_mouse
		//				}, "item");
		//				if (e.SupportSelect)
		//				{
		//					var win = new Views.InsertSelect(e.Element)
		//					{
		//						Topmost = true
		//					};
		//					isRecording = false;
		//					InputDriver.Instance.CallNext = true;
		//					win.Owner = this;
		//					if (win.ShowDialog() == true)
		//					{
		//						e.ClickHandled = true;
		//						if (!string.IsNullOrEmpty(win.SelectedItem.Value))
		//						{
		//							e.a.AddInput(win.SelectedItem.Value, e.Element);
		//						}
		//						else
		//						{
		//							e.a.AddInput(win.SelectedItem.Name, e.Element);
		//						}

		//					}
		//					else
		//					{
		//						e.SupportSelect = false;
		//					}
		//					InputDriver.Instance.CallNext = false;
		//					isRecording = true;
		//				}
		//				else if (e.SupportInput)
		//				{
		//					var win = new Views.InsertText
		//					{
		//						Topmost = true
		//					};
		//					isRecording = false;
		//					win.Owner = this;
		//					if (win.ShowDialog() == true)
		//					{
		//						e.a.AddInput(win.Text, e.Element);
		//					}
		//					else { e.SupportInput = false; }
		//					isRecording = true;
		//				}

		//				view.ReadOnly = false;
		//				view.Lastinserted = e.a.Activity;
		//				view.Lastinsertedmodel = view.AddRecordingActivity(e.a.Activity, plugin);
		//				view.ReadOnly = true;
		//				if (e.ClickHandled == false && e.SupportInput == false)
		//				{
		//					NativeMethods.SetCursorPos(e.X, e.Y);
		//					InputDriver.Click(e.Button);
		//				}
		//				System.Threading.Thread.Sleep(500);
		//			}
		//			if (sender.Name == "Windows") StartRecordPlugins(false);
		//		}
		//		catch (Exception ex)
		//		{
		//			MessageBox.Show(ex.Message);
		//			Show();
		//			Log.Error(ex.ToString());
		//		}
		//	}, null);
		//	Log.FunctionOutdent("MainWindow", "OnUserAction");
		//}

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
