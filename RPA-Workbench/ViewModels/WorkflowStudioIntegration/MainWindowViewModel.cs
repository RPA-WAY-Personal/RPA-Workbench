namespace RPA_Workbench.ViewModels.WorkflowStudioIntegration
{
    using System;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Toolbox;
    using System.Activities.Statements;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Presentation.Factories;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using ActiproSoftware.Windows.Controls.Docking;
    using RPA_Workbench.Properties;
    using RPA_Workbench.Utilities;
    using RPA_Workbench.ViewModels.WorkflowStudioIntegration;
    using RPA_Workbench.ViewModels;
    using Microsoft.Win32;
    using System.Activities.Core.Presentation;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Activities.Presentation.View;
    using System.Linq;
    using RPA.Workbench.Interfaces;
    using System.Activities.Expressions;

    public class MainWindowViewModel : ViewModelBase
    {
        private static List<string> namespacesToIgnore = new List<string>
            {
                "Microsoft.VisualBasic.Activities",
                "System.Activities.Expressions",
                "System.Activities.Statements",
                "System.ServiceModel.Activities",
                "System.ServiceModel.Activities.Presentation.Factories",
                "System.Activities.Presentation"
            };

        private DockSite dockingManager;
        private ICommand openWorkflowCommand;
        private ICommand newWorkflowCommand;
        private ICommand newServiceCommand;
        private ICommand closeWorkflowCommand;
        private ICommand closeAllWorkflowsCommand;
        private ICommand saveWorkflowCommand;
        private ICommand saveAsWorkflowCommand;
        private ICommand startWithoutDebuggingCommand;
        private ICommand startWithDebuggingCommand;
        private ICommand viewToolboxCommand;
        private ICommand viewPropertyInspectorCommand;
        private ICommand viewOutputCommand;
        private ICommand viewErrorsCommand;
        private ICommand viewDebugCommand;
        private ICommand viewOutlineCommand;
        private ICommand abortCommand;
        private ICommand addReferenceCommand;
        private ICommand removeReferenceCommand;
        private ICommand exitCommand;
        private ICommand saveAllWorkflowsCommand;
        private ICommand aboutCommand;
        private ICommand _BackstageTabStart;
        private ICommand _CloseProject;
        private ICommand _RunProjectFromJson;
        private ICommand _OpenBackstage;
        private ICommand _OpenSettingsTab;
        private ICommand _ShowUiExplorerFromRibbon;
        private TabbedMdiHost tabsPane;
        private IDictionary<ContentTypes, DocumentWindow> dockableContents;
        private ToolboxControl toolboxControl;
        private IDictionary<ToolboxCategory, IList<string>> loadedToolboxActivities;
        private IDictionary<string, ToolboxCategory> toolboxCategoryMap;
        private bool disableDebugViewOutput;
        WorkflowDesigner designer; //Used At OpenCommand
        public WorkflowViewModel workspaceViewModel;
        public WorkflowDocumentContent content;

        public string Filename; //Used on Open File
        MainWindow mainWindowLocal;
        Views.BackstageMenu backstageMenuLocal = new Views.BackstageMenu();
        string _currentProjectPath;
        public string CurrentProjectPath;

        ToolWindow _PropertiesToolWindow;
        ToolWindow _OutlineToolWindow;
        ToolWindow _OutputToolWindow;
        ToolWindow _ErrorsToolWindow;
        ToolWindow _DebugToolWindow;


        private MenuItem comment;
        private MenuItem uncomment;
        public ModelItem SelectedActivity { get; private set; }
        private Selection selection = null;



        public MainWindowViewModel(ToolWindow PropertiesToolwindow, ToolWindow OutlineToolWindow, ToolWindow OutputToolWindow, ToolWindow ErrorsToolWindow,
             ToolWindow DebugToolWindow, DockSite dockingManager = null, TabbedMdiHost tabsPane = null, ToolWindow DebuggerWindow = null,  MainWindow mainWindow = null,
            Views.BackstageMenu backstageMenu = null, String CurrentProjectPath = null)
        {
            var metaData = new DesignerMetadata();
            metaData.Register();
            RPAWorkbench.UiAutomation.Activities.TestAutomationMetadata.RegisterAll();

            this.CurrentProjectPath = CurrentProjectPath;

            this.dockingManager = dockingManager;
            mainWindowLocal = mainWindow;
            backstageMenuLocal = backstageMenu;
            dockingManager.WindowActivated += delegate (object sender, DockingWindowEventArgs e)
            {
                this.UpdateViews();
            };

            this._PropertiesToolWindow = PropertiesToolwindow;
            this._OutlineToolWindow = OutlineToolWindow;
            this._OutputToolWindow = OutputToolWindow;
            this._ErrorsToolWindow = ErrorsToolWindow;
            this._DebugToolWindow = DebugToolWindow;
            this.toolboxControl = new ToolboxControl();
            this.InitialiseToolbox();

            this.tabsPane = tabsPane;
          

            this.dockableContents = new Dictionary<ContentTypes, DocumentWindow>();
            this.ViewToolbox();

            string disableDebugViewOutputValue = ConfigurationManager.AppSettings["DisableDebugViewOutput"];
            if (!string.IsNullOrEmpty(disableDebugViewOutputValue))
            {
                this.disableDebugViewOutput = bool.Parse(disableDebugViewOutputValue);
            }
            disableDebugViewOutput = false;
            dockingManager.PrimaryDocumentChanged += DockingManager_PrimaryDocumentChanged;
            tabsPane.PrimaryWindowChanged += TabsPane_PrimaryWindowChanged;

            this.ViewOutput();
            this.ViewErrors();
            this._OutputToolWindow.Content = OutputView;
            this._ErrorsToolWindow.Content = ValidationErrorsView;
            this.ViewOutput();
            this.ViewErrors();
            this.UpdateViews();
            RelaySetup();
            LoadBackstageTabs();


        }

       

        void RelaySetup()
        {
            //OpenWorkflowCommand = new RelayCommand(new Action<object>(OpenWorkflow)); //Not Needed as a command
            OpenStartPageCommand = new RelayCommand(new Action<object>(OpenStartPage));
            CloseProjectCommand = new RelayCommand(new Action<object>(CloseProject));
            RunProjectFromJsonCommand = new RelayCommand(new Action<object>(StartWithoutDebuggingFromJson));
            OpenBackstageCommand = new RelayCommand(new Action<object>(StartWithoutDebuggingFromJson));
            ShowUiExplorerFromRibbonCommand = new RelayCommand(new Action<object>(ShowUiExplorerFromRibbon));
        }

        void LoadBackstageTabs()
        {
            try
            {
                //Settings
                Views.BackstageMenu_Tabs.SettingsPanel settingsPanel = new Views.BackstageMenu_Tabs.SettingsPanel();
                backstageMenuLocal.SettingsTabContent.Children.Add(settingsPanel);
            }
            catch (NullReferenceException)
            {
            }
          
        }
        private void DockingManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SaveAll();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.F5)
            {
                StartWithoutDebugging();
            }
            else if (e.Key == Key.F5)
            {
                StartWithDebugging();
            }






        }
      
        #region Presentation Properties

        public bool HasValidationErrors
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                if (model != null)
                {
                    return model.HasValidationErrors;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CurrentProjectPath) == false)
                    {
                        string json;
                        dynamic jsonObj;
                        model = new WorkflowViewModel(false);
                        json = File.ReadAllText(CurrentProjectPath + "\\project.json");
                        jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        string ProjectCurrentDirectory = jsonObj["ProjectPath"];
                        model.FullFilePath = ProjectCurrentDirectory + "\\" + jsonObj["Main"];
                        model.WorkflowDesigner.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
                        if (model != null)
                        {
                            this.ViewOutput();
                        }

                        return model.HasValidationErrors;
                    }
                    else
                    {
                        return true;
                    }
                }
               
            }
        }

        public UIElement ValidationErrorsView
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                if (model != null)
                {
                    return model.ValidationErrorsView;
                }
                else
                {  //This is to always show the ValidationErrorsView, based on the Main workflow
                    if (string.IsNullOrWhiteSpace(CurrentProjectPath) == false)
                    {
                        string json;
                        dynamic jsonObj;
                        model = new WorkflowViewModel(false);
                        json = File.ReadAllText(CurrentProjectPath + "\\project.json");
                        jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        string ProjectCurrentDirectory = jsonObj["ProjectPath"];
                        model.FullFilePath = ProjectCurrentDirectory + "\\" + jsonObj["Main"];
                        model.WorkflowDesigner.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
                        if (model != null)
                        {
                            this.ViewErrors();
                        }
                        return model.ValidationErrorsView;
                    }
                    else
                    {
                        return null;
                    }
                }
                
            }
        }

        public UIElement DebugView
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                return model == null ? null : model.DebugView;
            }
        }

        public UIElement OutlineView
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                return model == null ? null : model.OutlineView;
            }
        }

        public UIElement OutputView
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                if (model != null)
                {
                    model.outputTextBox.Items.Clear();
                    Console.SetOut(model.Output);
                    return model.OutputView;
                }
                else
                {
                    //This is to always show the Outputwindow, based on the Main workflow
                    if (string.IsNullOrWhiteSpace(CurrentProjectPath) == false)
                    {
                        string json;
                        dynamic jsonObj;
                        model = new WorkflowViewModel(false);
                        model.outputTextBox.Items.Clear();
                        json = File.ReadAllText(CurrentProjectPath + "\\project.json");
                        jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        string ProjectCurrentDirectory = jsonObj["ProjectPath"];
                        model.FullFilePath = ProjectCurrentDirectory + "\\" + jsonObj["Main"];
                        model.WorkflowDesigner.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
                        if (model != null)
                        {
                            this.ViewOutput();
                        }
                        Console.SetOut(model.Output);
                        return model.OutputView;
                    }
                    else
                    {
                        return null;
                    }
                   
                }
            }
        }

        public UIElement ToolboxView
        {
            get
            {
                return this.toolboxControl;
            }
        }

        public UIElement PropertyInspectorView
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                return model == null ? null : model.WorkflowDesigner.PropertyInspectorView;
            }
        }
        public ToolboxControl ActivitiesView
        {
            get
            {
                return toolboxControl;
            }
            set
            {
                toolboxControl = value;
            }
        }

        #endregion

        #region Custom Commands
        public ICommand OpenStartPageCommand
        {
            get
            {
                return _BackstageTabStart;
            }
            set
            {
                _BackstageTabStart = value;
            }
        }

        public ICommand CloseProjectCommand
        {
            get { return _CloseProject; }
            set { _CloseProject = value; }
        }

        public ICommand RunProjectFromJsonCommand
        {
            get { return _RunProjectFromJson; }
            set { _RunProjectFromJson = value; }
        }

        public ICommand OpenBackstageCommand
        {
            get { return _OpenBackstage; }
            set { _OpenBackstage = value; }
        }

        public ICommand OpenSettingsCommand
        {
            get { return _OpenSettingsTab; }
            set { _OpenSettingsTab = value; }
        }

        public ICommand ShowUiExplorerFromRibbonCommand
        {
            //CHECK GETELEMENT DESIGNER
            get { return _ShowUiExplorerFromRibbon; }
            set { _ShowUiExplorerFromRibbon = value; }
        }

        #endregion

        #region Commands

        public ICommand NewWorkflowCommand
        {
            get
            {
                if (this.newWorkflowCommand == null)
                {
                    this.newWorkflowCommand = new RelayCommand(
                        param => this.NewWorkflow(WorkflowTypes.Activity),
                        param => this.CanNew);
                }

                return this.newWorkflowCommand;
            }
        }

        public ICommand CloseWorkflowCommand
        {
            get
            {
                if (this.closeWorkflowCommand == null)
                {
                    this.closeWorkflowCommand = new RelayCommand(
                        param => this.CloseWorkflow(),
                        param => this.CanClose);
                }

                return this.closeWorkflowCommand;
            }
        }

        public ICommand CloseAllWorkflowsCommand
        {
            get
            {
                if (this.closeAllWorkflowsCommand == null)
                {
                    this.closeAllWorkflowsCommand = new RelayCommand(
                        param => this.CloseAllWorkflows(),
                        param => this.CanCloseAll);
                }

                return this.closeAllWorkflowsCommand;
            }
        }

        public ICommand SaveWorkflowCommand
        {
            get
            {
                if (this.saveWorkflowCommand == null)
                {
                    //this.saveWorkflowCommand = new RelayCommand(
                    //    param => this.SaveWorkflow());
                    this.saveWorkflowCommand = new RelayCommand(
                          param => this.SaveWorkflow(),
                          param => true);
                }

                return this.saveWorkflowCommand;
            }
            set
            {
                this.saveWorkflowCommand = value;
            }
        }

        public ICommand SaveAsWorkflowCommand
        {
            get
            {
                if (this.saveAsWorkflowCommand == null)
                {
                    this.saveAsWorkflowCommand = new RelayCommand(
                        param => this.SaveAsWorkflow(),
                        param => this.CanSaveAs);
                }

                return this.saveAsWorkflowCommand;
            }
        }

        public ICommand StartWithoutDebuggingCommand
        {
            get
            {
                if (this.startWithoutDebuggingCommand == null)
                {
                    this.startWithoutDebuggingCommand = new RelayCommand(
                        param => this.StartWithoutDebugging(),
                        param => this.CanStartWithoutDebugging);
                }

                return this.startWithoutDebuggingCommand;
            }
        }

        public ICommand StartWithDebuggingCommand
        {
            get
            {
                if (this.startWithDebuggingCommand == null)
                {
                    this.startWithDebuggingCommand = new RelayCommand(
                        param => this.StartWithDebugging(),
                        param => this.CanStartWithDebugging);
                }

                return this.startWithDebuggingCommand;
            }
        }

        public ICommand AbortCommand
        {
            get
            {
                if (this.abortCommand == null)
                {
                    this.abortCommand = new RelayCommand(
                        param => this.Abort(),
                        param => this.CanAbort);
                }

                return this.abortCommand;
            }
        }

        public ICommand ViewToolboxCommand
        {
            get
            {
                if (this.viewToolboxCommand == null)
                {
                    this.viewToolboxCommand = new RelayCommand(
                        param => this.ViewToolbox(),
                        param => this.CanViewToolbox);
                }

                return this.viewToolboxCommand;
            }
        }

        public ICommand ViewPropertyInspectorCommand
        {
            get
            {
                if (this.viewPropertyInspectorCommand == null)
                {
                    this.viewPropertyInspectorCommand = new RelayCommand(
                        param => this.ViewPropertyInspector(),
                        param => this.CanViewPropertyInspector);
                }

                return this.viewPropertyInspectorCommand;
            }
        }

        public ICommand ViewOutputCommand
        {
            get
            {
                if (this.viewOutputCommand == null)
                {
                    this.viewOutputCommand = new RelayCommand(
                        param => this.ViewOutput(),
                        param => this.CanViewOutput);
                }

                return this.viewOutputCommand;
            }
        }

        public ICommand ViewErrorsCommand
        {
            get
            {
                if (this.viewErrorsCommand == null)
                {
                    this.viewErrorsCommand = new RelayCommand(
                        param => this.ViewErrors(),
                        param => this.CanViewErrors);
                }

                return this.viewErrorsCommand;
            }
        }

        public ICommand ViewOutlineCommand
        {
            get
            {
                if (this.viewOutlineCommand == null)
                {
                    this.viewOutlineCommand = new RelayCommand(
                        param => this.ViewOutline(),
                        param => this.CanViewOutline);
                }

                return this.viewOutlineCommand;
            }
        }

        public ICommand ViewDebugCommand
        {
            get
            {
                if (this.viewDebugCommand == null)
                {
                    this.viewDebugCommand = new RelayCommand(
                        param => this.ViewDebug(),
                        param => this.CanViewDebug);
                }

                return this.viewDebugCommand;
            }
        }

        public ICommand AddReferenceCommand
        {
            get
            {
                if (this.addReferenceCommand == null)
                {
                    this.addReferenceCommand = new RelayCommand(
                        param => this.AddReference(),
                        param => this.CanAddReference);
                }

                return this.addReferenceCommand;
            }
        }
        public string CategoryName;
        public List<string> SelectedDependencyPath = new List<string>();
        public ICommand RemoveReferenceCommand
        {
            get
            {
                if (this.removeReferenceCommand == null)
                {
                    
                    this.removeReferenceCommand = new RelayCommand(
                        param => this.RemoveActivitiesFromAssemblies(SelectedDependencyPath),
                        param => this.CanAddReference);
                }
                
                return this.removeReferenceCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (this.exitCommand == null)
                {
                    this.exitCommand = new RelayCommand(
                        param => this.Exit(),
                        param => this.CanExit);
                }

                return this.exitCommand;
            }
        }

        public ICommand SaveAllWorkflowsCommand
        {
            get
            {
                if (this.saveAllWorkflowsCommand == null)
                {
                    this.saveAllWorkflowsCommand = new RelayCommand(
                        param => this.SaveAll(),
                        param => true);
                }

                return this.saveAllWorkflowsCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if (this.aboutCommand == null)
                {
                    this.aboutCommand = new RelayCommand(
                        param => this.About(),
                        param => this.CanAbout);
                }

                return this.aboutCommand;
            }
        }

        #region Properties
        private bool CanNew
        {
            get { return true; }
        }

        private bool CanOpen
        {
            get { return true; }
        }

        private bool CanClose
        {
            get { return this.dockingManager.ActiveWindow != null; }
        }

        private bool CanCloseAll
        {
            get { return this.dockingManager.Documents.Count > 1; }
        }

        private bool CanSave
        {
            get
            {
                WorkflowDocumentContent document = this.dockingManager.ActiveWindow as WorkflowDocumentContent;
                if (document != null)
                {
                    WorkflowViewModel model = document.DataContext as WorkflowViewModel;
                    return model.IsModified;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool CanSaveAs
        {
            get { return this.dockingManager.ActiveWindow != null; }
        }

        private bool CanStartWithoutDebugging
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                bool running = model == null ? false : model.IsRunning;
                return this.dockingManager.ActiveWindow != null && !running;
            }
        }

        private bool CanAbort
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                return model == null ? false : model.IsRunning;
            }
        }

        private bool CanStartWithDebugging
        {
            get
            {
                WorkflowViewModel model = this.ActiveWorkflowViewModel;
                bool running = model == null ? false : model.IsRunning;
                return this.dockingManager.ActiveWindow != null && !running;
            }
        }

        private bool CanViewToolbox
        {
            get { return true; }
        }

        private bool CanViewPropertyInspector
        {
            get { return true; }
        }

        private bool CanViewOutput
        {
            get { return true; }
        }

        private bool CanViewErrors
        {
            get { return true; }
        }

        private bool CanViewDebug
        {
            get { return !this.disableDebugViewOutput; }
        }

        private bool CanViewOutline
        {
            get { return true; }
        }

        public bool CanAddReference
        {
            get { return true; }
        }

        private bool CanExit
        {
            get { return true; }
        }

        private bool CanSaveAll
        {
            get
            {
                IList<WorkflowViewModel> modifiedWorkflows = this.GetModifiedWorkflows();
                return modifiedWorkflows.Count > 1;
            }
        }

        private bool CanAbout
        {
            get { return true; }
        }
        #endregion


        #endregion

        private WorkflowViewModel ActiveWorkflowViewModel
        {
            get
            {
                WorkflowDocumentContent content = this.dockingManager.PrimaryDocument as WorkflowDocumentContent;
                if (content != null)
                {
                    WorkflowViewModel model = content.DataContext as WorkflowViewModel;
                    if (model != null)
                    {
                        return model;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                   return null;
                }
            }
        }

        private bool HasRunningWorkflows
        {
            get
            {
                foreach (DocumentWindow document in this.dockingManager.Documents)
                {
                    if (document is WorkflowDocumentContent)
                    {
                        WorkflowViewModel model = document.DataContext as WorkflowViewModel;
                        if (model.IsRunning)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = this.CancelCloseAllWorkflows();
        }

        #region Custom Helpers 
        private void OpenStartPage(object parameter)
        {
            mainWindowLocal.MainRibbon.ApplicationMenu.IsEnabled = false;
            mainWindowLocal.Opacity = 0.2f;

            Views.StartupMenu.ParentStartupWindow parentStartupWindow = new Views.StartupMenu.ParentStartupWindow(mainWindowLocal);
            parentStartupWindow.Visibility = System.Windows.Visibility.Visible;
            parentStartupWindow.ShowDialog();

            mainWindowLocal.MainRibbon.ApplicationMenu.IsEnabled = true;
            mainWindowLocal.Opacity = 100;
        }

        private void CloseProject(object parameter)
        {
            //backstageMenuLocal.CanClose = false;
            mainWindowLocal.MainBackStageMenu.CanClose = false;
            mainWindowLocal.ActivitiesToolWindow.Content = null;
            mainWindowLocal.PropertiesToolWindow.Content = null;
            mainWindowLocal.ProjectToolWindow.Content = null;
            mainWindowLocal.OutputToolWindow.Content = null;
            mainWindowLocal.ErrorListToolWindow.Content = null;
            mainWindowLocal.DebuggerToolWindow.Content = null;
            mainWindowLocal.OutlineToolwindow.Content = null;
            try
            {
                foreach (var item in mainWindowLocal.MainDockSite.DocumentWindows)
                {
                    mainWindowLocal.MainDockSite.DocumentWindows.Remove(item);
                }
            }
            catch (Exception)
            {
                //To catch Collection (mainWindowLocal.MainDockSite.DocumentWindows) modified exception
            }


        }

        private void StartWithoutDebuggingFromJson(object parameter)
        {
            string json;
            dynamic jsonObj;
            WorkflowViewModel model = new WorkflowViewModel(false);
          //  MessageBox.Show(CurrentProjectPath);
            //WorkflowViewModel model = this.ActiveWorkflowViewModel;
            json = File.ReadAllText(CurrentProjectPath + "\\project.json");
            jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string ProjectCurrentDirectory = jsonObj["ProjectPath"];
            // model.FullFilePath = ProjectCurrentDirectory + "\\" + jsonObj["Main"] + ".xaml";
            //  model.Designer.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"] + ".xaml");
            model.FullFilePath = ProjectCurrentDirectory + "\\" + jsonObj["Main"];
            model.WorkflowDesigner.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
            if (model != null)
            {
                if (this.HasValidationErrors)
                {
                    Console.WriteLine($"Error : There was an error in '{jsonObj["Main"]}', can't run");
                    this.ViewErrors();
                    this.SetSelectedTab(ContentTypes.Errors);
                }
                else
                {
                    if (RPA_Workbench.Properties.Settings.Default.Setting_MinimizeOnRun == true)
                    {
                        mainWindowLocal.WindowState = WindowState.Minimized;
                        model.RunWorkflow(mainWindowLocal);
                    }
                    else
                    {
                        model.RunWorkflow();
                    }
                    this.ViewOutput();
                    this.SetSelectedTab(ContentTypes.Output);
                }
                // model.RunWorkflow();
            }
        }
        System.Activities.Presentation.WorkflowViewElement WorkflowViewElement = new WorkflowViewElement();
        private void ShowUiExplorerFromRibbon(object parameter)
        {
            //RPA.Workbench.Interfaces.Selector.SelectorWindow selectorWindow = new RPA.Workbench.Interfaces.Selector.SelectorWindow("s",1);
            //selectorWindow.Show();
           
            //string SelectorString = WorkflowViewElement.ModelItem.GetValue<string>("Selector");
            //int maxresult = 1;

            //if (string.IsNullOrEmpty(SelectorString)) SelectorString = "[{Selector: 'Windows'}]";
            //var selector = new RPA.Workbench.Interfaces.Selector.Selector(SelectorString);
            //var pluginname = selector.First().Selector;
            //var selectors = new RPA.Workbench.Interfaces.Selector.SelectorWindow(pluginname, selector, maxresult);
            //selectors.Owner = RPA.Workbench.Interfaces.GenericTools.MainWindow;
            //if (selectors.ShowDialog() == true)
            //{
            //    WorkflowViewElement.ModelItem.Properties["Selector"].SetValue(new InArgument<string>() { Expression = new Literal<string>(selectors.vm.json) });
            //}

            //TODO: Show Ui Explror when in MainWindow (ViewModel)
        }
        #endregion

        #region Private helpers

        private void NewWorkflow(WorkflowTypes workflowType)
        {
            WorkflowViewModel workspaceViewModel = new WorkflowViewModel(this.disableDebugViewOutput);
            WorkflowDocumentContent content = new WorkflowDocumentContent(workspaceViewModel, workflowType);
            content.Title = workspaceViewModel.DisplayNameWithModifiedIndicator;

            content.Activate();
            this.dockingManager.PrimaryDocument.Content = content;
            //dockingManager.WindowsClosing += new ActiproSoftware.Windows.Controls.Docking.DockingWindowEventArgs<ActiproSoftware.Windows.Controls.Docking.DockingWindowEventArgs>(workspaceViewModel.Closing);
            this.ViewPropertyInspector();
            this.ViewErrors();
            this.SetSelectedTab(ContentTypes.Errors);
        }
        public void OpenWorkflow()
        {
            //OpenFileDialog fileDialog = WorkflowFileDialogFactory.CreateOpenFileDialog();
            //if (fileDialog.ShowDialog() == true)
            //  {

           
            workspaceViewModel = new WorkflowViewModel(false);
            //workspaceViewModel.FullFilePath = fileDialog.FileName;
            workspaceViewModel.FullFilePath = Filename;
            content = new WorkflowDocumentContent(workspaceViewModel);

            designer = workspaceViewModel.WorkflowDesigner;

            ModelService modelService = workspaceViewModel.WorkflowDesigner.Context.Services.GetService<ModelService>();
            if (modelService != null)
            {
                List<Type> referencedActivities = this.GetReferencedActivities(modelService);
                this.AddActivitiesToToolbox(referencedActivities);
               // AddAllAddReferencesToFileToToolBox();
            }
            content.Title = workspaceViewModel.DisplayNameWithModifiedIndicator;
            dockingManager.DocumentWindows.Add(content);
            content.Activate();
            content.KeyDown += DockingManager_KeyDown;
            SetupDesignerCustomContextMenuItems();


            //  dockingManager.ActiveWindow.Content = content;
            //content.Closing += new EventHandler<CancelEventArgs>(workspaceViewModel.Closing);
            //   }

            //this.ViewPropertyInspector();
            //if (this.HasValidationErrors)
            //{
            //    this.ViewErrors();
            //    this.SetSelectedTab(ContentTypes.Errors);
            //}
        }
        private string SelectedVariableName = null;
        private void SetupDesignerCustomContextMenuItems()
        {
            designer.Context.Items.Subscribe(new SubscribeContextCallback<Selection>(SelectionChanged));
            comment = new MenuItem() { Header = "Comment" };
            uncomment = new MenuItem() { Header = "Uncomment" };

            comment.Click += OnComment;
            uncomment.Click += OnUncomment;

            designer.ContextMenu.Items.Add(comment);
            designer.ContextMenu.Items.Add(uncomment);
        }

        public void OnUncomment(object sender, RoutedEventArgs e)
        {
            var thisselection = selection;
            var comment = SelectedActivity;
            var currentSequence = SelectedActivity.Properties["Body"].Value;
            if (currentSequence == null) return;
            //var newSequence = GetActivitiesScope(SelectedActivity.Parent.Parent);
            var newSequence = GetActivitiesScope(SelectedActivity.Parent, designer);
            ModelItemCollection currentActivities = null;
            if (currentSequence.Properties["Activities"] != null)
            {
                currentActivities = currentSequence.Properties["Activities"].Collection;
            }
            else if (currentSequence.Properties["Nodes"] != null)
            {
                currentActivities = currentSequence.Properties["Nodes"].Collection;
            }
            ModelItemCollection newActivities = null;
            if (newSequence.Properties["Activities"] != null)
            {
                newActivities = newSequence.Properties["Activities"].Collection;
            }
            else if (newSequence.Properties["Nodes"] != null)
            {
                newActivities = newSequence.Properties["Nodes"].Collection;
                var next = thisselection.PrimarySelection.Parent.Properties["Next"];

                newActivities.Remove(thisselection.PrimarySelection.Parent);

                FlowStep step = new FlowStep
                {
                    Action = new Sequence()
                };
                var newStep = newActivities.Add(step);
                newStep.Properties["Action"].SetValue(comment.Properties["Body"].Value);
                newStep.Properties["Next"].SetValue(next.Value);

                if (newSequence.Properties["StartNode"].Value == thisselection.PrimarySelection.Parent)
                {
                    newSequence.Properties["StartNode"].SetValue(newStep);
                }
                foreach (var node in newActivities)
                {
                    if (node.Properties["Next"] != null && node.Properties["Next"].Value != null)
                    {
                        if (node.Properties["Next"].Value == thisselection.PrimarySelection.Parent)
                        {
                            node.Properties["Next"].SetValue(newStep);
                        }
                    }
                }
                return;
            }

            if (currentActivities != null && newActivities != null)
            {
                var index = newActivities.IndexOf(comment);
                foreach (var sel in currentActivities.ToList())
                {
                    currentActivities.Remove(sel);
                    index++;
                    newActivities.Insert(index, sel);
                    //newActivities.Add(sel);
                }
                newActivities.Remove(comment);
            }
            if (currentActivities != null && currentActivities.Count == 1 && comment.Parent.Properties["Handler"] != null)
            {
                var handler = comment.Parent.Properties["Handler"];
                handler.SetValue(currentActivities.First());
            }
            else if (currentActivities == null && newActivities != null)
            {
                var index = newActivities.IndexOf(comment);
                var movethis = comment.Properties["Body"].Value;
                newActivities.Insert(index, movethis);
                newActivities.Remove(comment);
            }
            else if (currentActivities == null && newActivities == null)
            {
                if (newSequence.Properties["Body"] != null)
                {
                    var body = newSequence.Properties["Body"];
                    var handler = body.Value.Properties["Handler"];
                    handler.SetValue(handler.Value.Properties["Body"].Value);
                }
                else if (newSequence.Properties["Handler"] != null)
                {
                    var handler = newSequence.Properties["Handler"];
                    handler.SetValue(currentSequence);
                }
            }
            else if (currentSequence != null && comment.Parent != null && comment.Parent.Properties["Handler"] != null)
            {
                var handler = comment.Parent.Properties["Handler"];
                handler.SetValue(currentSequence);
            }
        }
        private void OnComment(object sender, RoutedEventArgs e)
        {
            var thisselection = selection;
            var pri = thisselection.PrimarySelection;
            if (pri == null) return;
            //var movethis = selectedActivity;

            var lastSequence = GetActivitiesScope(SelectedActivity.Parent, designer);
            if (lastSequence == null) lastSequence = GetActivitiesScope(SelectedActivity, designer);
            ModelItemCollection Activities = null;
            if (lastSequence.Properties["Activities"] != null)
            {
                Activities = lastSequence.Properties["Activities"].Collection;
            }
            else if (lastSequence.Properties["Nodes"] != null)
            {
                Activities = lastSequence.Properties["Nodes"].Collection;
            }

            if (SelectedActivity.ItemType == typeof(Sequence))
            {

                var parent = SelectedActivity.Parent;
                if (SelectedActivity.Parent.ItemType == typeof(ActivityBuilder)) return;
                if (parent.Properties["Activities"] != null)
                {
                    Activities = parent.Properties["Activities"].Collection;
                }
                var co = new BuiltIn.Activities.CommentOut
                {
                    Body = SelectedActivity.GetCurrentValue() as Activity
                };
                if (Activities == null)
                {
                    var item = thisselection.PrimarySelection.Parent.Properties["Handler"].SetValue(co);
                }
                else
                {
                    try
                    {
                        Activities.Remove(SelectedActivity);
                        Activities.Add(co);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                   
                }
            }
            else if (thisselection.SelectionCount > 1 || thisselection.PrimarySelection.ItemType == typeof(Sequence))
            {
                if (lastSequence.Properties["Nodes"] != null) return;
                var co = new BuiltIn.Activities.CommentOut
                {
                    Body = new Sequence()
                };
                if (Activities == null)
                {
                    var item = thisselection.PrimarySelection.Parent.Properties["Handler"].SetValue(co);
                    var newActivities = item.Properties["Body"].Value.Properties["Activities"].Collection;
                    foreach (var sel in thisselection.SelectedObjects)
                    {
                        if (Activities != null) Activities.Remove(sel);
                        var index = newActivities.Count;
                        Log.Debug("insert at " + index);
                        newActivities.Insert(0, sel);
                        //newActivities.Add(sel);
                    }
                }
                else
                {
                    AddActivity(co, designer);
                    var newActivities = SelectedActivity.Properties["Body"].Value.Properties["Activities"].Collection;
                    foreach (var sel in thisselection.SelectedObjects)
                    {
                        if (Activities != null) Activities.Remove(sel);
                        var index = newActivities.Count;
                        Log.Debug("insert at " + index);
                        newActivities.Insert(0, sel);
                        //newActivities.Add(sel);
                    }
                }

            }
            else
            {
                var parentparent = thisselection.PrimarySelection.Parent.Parent;
                var parent = thisselection.PrimarySelection.Parent;

                if (parentparent == lastSequence)
                {
                    var co = new BuiltIn.Activities.CommentOut();
                    AddActivity(co, designer);
                    Activities.Remove(thisselection.PrimarySelection);
                    SelectedActivity.Properties["Body"].SetValue(thisselection.PrimarySelection);
                }
                else
                {
                    try
                    {
                        if (parentparent.Properties["Body"] != null)
                        {
                            var body = parentparent.Properties["Body"];
                            if (body.Value == null)
                            {
                                var aa = (dynamic)Activator.CreateInstance(body.PropertyType);
                                //aa.Handler = new CommentOut();
                                SelectedActivity = parentparent.Properties["Body"].SetValue(aa);
                            }
                            var handler = body.Value.Properties["Handler"];
                            var comment = handler.SetValue(new BuiltIn.Activities.CommentOut());
                            comment.Properties["Body"].SetValue(thisselection.PrimarySelection);

                            //p.Properties["Body"].Value.Properties["Handler"].Value.Properties["Body"].SetValue(thisselection.PrimarySelection);
                        }
                        else if (parent.Properties["Action"] != null)
                        {
                            var next = thisselection.PrimarySelection.Parent.Properties["Next"];
                            var co = new BuiltIn.Activities.CommentOut();
                            var comment = AddActivity(co, designer);
                            Activities.Remove(thisselection.PrimarySelection.Parent);

                            if (lastSequence.Properties["StartNode"].Value == thisselection.PrimarySelection.Parent)
                            {
                                lastSequence.Properties["StartNode"].SetValue(comment);
                            }
                            foreach (var node in Activities)
                            {
                                if (node.Properties["Next"] != null && node.Properties["Next"].Value != null)
                                {
                                    if (node.Properties["Next"].Value == thisselection.PrimarySelection.Parent)
                                    {
                                        node.Properties["Next"].SetValue(comment);
                                    }
                                }
                            }


                            if (comment.Properties["Body"] != null)
                            {
                                comment.Properties["Body"].SetValue(thisselection.PrimarySelection);
                            }
                            else if (comment.Properties["Action"] != null)
                            {
                                comment.Properties["Action"].Value.Properties["Body"].SetValue(thisselection.PrimarySelection);
                                comment.Properties["Next"].SetValue(next.Value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
        }
        public ModelItem AddActivity(Activity a, WorkflowDesigner workflowDesigner)
        {
            ModelItem newItem = null;
            ModelService modelService = workflowDesigner.Context.Services.GetService<ModelService>();
            using (ModelEditingScope editingScope = modelService.Root.BeginEdit("Implementation"))
            {
                var lastSequence = GetSequence(SelectedActivity, designer);
                if (lastSequence == null && SelectedActivity != null) lastSequence = GetActivitiesScope(SelectedActivity.Parent, designer);
                if (lastSequence != null)
                {
                    ModelItemCollection Activities = null;
                    if (lastSequence.Properties["Activities"] != null)
                    {
                        Activities = lastSequence.Properties["Activities"].Collection;
                    }
                    else
                    {
                        Activities = lastSequence.Properties["Nodes"].Collection;
                    }

                    var insertAt = Activities.Count;
                    for (var i = 0; i < Activities.Count; i++)
                    {
                        if (Activities[i].Equals(SelectedActivity))
                        {
                            insertAt = (i + 1);
                        }
                    }
                    if (lastSequence.Properties["Activities"] != null)
                    {
                        if (string.IsNullOrEmpty(a.DisplayName)) a.DisplayName = "Activity";
                        newItem = Activities.Insert(insertAt, a);
                    }
                    else
                    {
                        FlowStep step = new FlowStep
                        {
                            Action = a
                        };
                        newItem = Activities.Insert(insertAt, step);
                    }
                    //Selection.Select(wfDesigner.Context, selectedActivity);
                    //ModelItemExtensions.Focus(selectedActivity);
                }
                editingScope.Complete();
                //WorkflowInspectionServices.CacheMetadata(a);
            }
            if (newItem != null)
            {
                SelectedActivity = newItem;
                newItem.Focus(20);
                Selection.SelectOnly(workflowDesigner.Context, newItem);
            }
            return newItem;
        }
        private ModelItem GetSequence(ModelItem from, WorkflowDesigner workflowDesigner)
        {
            ModelItem parent = from;
            while (parent != null && !parent.ItemType.Equals(typeof(Sequence)))
            {
                parent = parent.Parent;
            }
            return parent;
        }
        private ModelItem GetVariableScope(ModelItem from, WorkflowDesigner workflowDesigner)
        {
            ModelItem parent = from;

            while (parent != null && parent.Properties["Variables"] == null)
            {
                parent = parent.Parent;
            }
            return parent;
        }
        private ModelItem GetActivitiesScope(ModelItem from, WorkflowDesigner workflowDesigner)
        {
            ModelItem parent = from;

            while (parent != null && parent.Properties["Activities"] == null && parent.Properties["Handler"] == null && parent.Properties["Nodes"] == null)
            {
                parent = parent.Parent;
            }
            return parent;
        }
        private void SelectionChanged(Selection item)
        {
            selection = item;
            SelectedActivity = selection.PrimarySelection;
            if (SelectedActivity == null) return;
            SelectedVariableName = SelectedActivity.GetCurrentValue().ToString();

            try
            {
                if (designer.ContextMenu.Items.Contains(comment)) designer.ContextMenu.Items.Remove(comment);
                if (designer.ContextMenu.Items.Contains(uncomment)) designer.ContextMenu.Items.Remove(uncomment);
                var lastSequence = GetActivitiesScope(SelectedActivity.Parent, designer);
                if (lastSequence == null) lastSequence = GetActivitiesScope(SelectedActivity, designer);
                if (lastSequence == null) return;
                if (SelectedActivity.ItemType == typeof(BuiltIn.Activities.CommentOut))
                {
                    designer.ContextMenu.Items.Add(uncomment);
                }
                else if (lastSequence.ItemType != typeof(Flowchart))
                {
                    if (selection.SelectionCount > 1)
                    {
                        if (lastSequence.Properties["Nodes"] == null)
                        {
                            designer.ContextMenu.Items.Add(comment);
                        }
                    }
                    else
                    {
                        designer.ContextMenu.Items.Add(comment);
                    }
                }
                else if (lastSequence.ItemType != typeof(Flowchart))
                {
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        private void DockingManager_PrimaryDocumentChanged(object sender, DockingWindowEventArgs e)
        {
            //try
            //{
            //    //System.Windows.Forms.MessageBox.Show("Tab Changed");

            //    UpdateViews();
            //    //designer = workspaceViewModel.Designer;
            //    // UpdateViews();
            //    if (e.Window != null)
            //    {
                   // workspaceViewModel = new WorkflowViewModel(false);
            //        // if (e.Window.Title.Contains(".xaml") && workspaceViewModel != null)
            //        //if (workspaceViewModel != null)
            //        //{
                   //UpdateViews();
                   this.ViewPropertyInspector();
                    this.ViewOutput();
                    if (this.HasValidationErrors)
                  {
                        this.ViewErrors();
                      this.SetSelectedTab(ContentTypes.Errors);
                   }
            //      }
            //    ModelService modelService = workspaceViewModel.Designer.Context.Services.GetService<ModelService>();
            //    if (modelService != null)
            //    {
            //        List<Type> referencedActivities = this.GetReferencedActivities(modelService);
            //        this.AddActivitiesToToolbox(referencedActivities);
            //    }
          
                    //_ErrorsToolWindow.Content = workspaceViewModel.ValidationErrorsView;
                    //_OutputToolWindow.Content = workspaceViewModel.OutputView;
                   // _DebuggerWindow.Content = workspaceViewModel.DebugView;
            //  //  ViewToolbox();

            //    //}

            //    //    ViewToolbox();
            UpdateViews();
            //}
            //catch (Exception)
            //{

            //   // throw;
            //}
        }

        private void TabsPane_PrimaryWindowChanged(object sender, DockingWindowEventArgs e)
        {
            //ViewToolbox();
            //UpdateViews();
        }

        //private void Model_PropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName == "DisplayNameWithModifiedIndicator" || args.PropertyName == "DisplayName")
        //    {
        //        WorkflowViewModel model = documentWindow.DataContext as WorkflowViewModel;
        //        if (model != null)
        //        {
        //            documentWindow.Title = model.DisplayNameWithModifiedIndicator;
        //        }
        //    }
        //}

        private void CloseWorkflow()
        {
            this.dockingManager.ActiveWindow.Close();
        }

        private void CloseAllWorkflows()
        {
            if (!this.CancelCloseAllWorkflows())
            {
                foreach (DocumentWindow documentContent in new List<DocumentWindow>(this.dockingManager.Documents.Count))
                {
                    WorkflowViewModel vm = documentContent.DataContext as WorkflowViewModel;
                  //  dockingManager.WindowsClosing -= vm.Closing;
                    documentContent.Close();
                }
            }
        }

        private bool CancelCloseAllWorkflows()
        {
            bool cancel = false;

            if (this.HasRunningWorkflows)
            {
                MessageBoxResult closingResult = MessageBox.Show(Resources.ConfirmCloseWhenRunningWorkflowsDialogMessage, Resources.ConfirmCloseWhenRunningWorkflowsDialogTitle, MessageBoxButton.YesNo);
                switch (closingResult)
                {
                    case MessageBoxResult.No:
                        cancel = true;
                        break;
                    case MessageBoxResult.Yes:
                        cancel = false;
                        break;
                    case MessageBoxResult.Cancel:
                        cancel = true;
                        break;
                }
            }

            if (cancel == false)
            {
                IList<WorkflowViewModel> modifiedWorkflows = this.GetModifiedWorkflows();
                if (modifiedWorkflows.Count > 0)
                {
                    MessageBoxResult closingResult = MessageBox.Show(string.Format(Resources.SaveChangesDialogMessage, this.FormatUnsavedWorkflowNames(modifiedWorkflows)), Resources.SaveChangesDialogTitle, MessageBoxButton.YesNoCancel);
                    switch (closingResult)
                    {
                        case MessageBoxResult.No:
                            cancel = false;
                            break;
                        case MessageBoxResult.Yes:
                            cancel = !this.SaveAllWorkflows();
                            break;
                        case MessageBoxResult.Cancel:
                            cancel = true;
                            break;
                    }
                }
            }

            return cancel;
        }

        private void SaveWorkflow()
        {
            WorkflowViewModel model = this.ActiveWorkflowViewModel;
            if (model != null)
            {
                model.SaveWorkflow();
            }
        }

        private void SaveAsWorkflow()
        {
            WorkflowViewModel model = this.ActiveWorkflowViewModel;
            if (model != null)
            {
                model.SaveAsWorkflow();
            }
        }

        private void StartWithoutDebugging()
        {
            WorkflowViewModel model = this.ActiveWorkflowViewModel;
            if (model != null)
            {
                this.ViewOutput();
                this.SetSelectedTab(ContentTypes.Output);
                if (this.HasValidationErrors)
                {
                    Console.WriteLine($"There was an error in '{ActiveWorkflowViewModel.DisplayName}', can't run");
                    this.ViewErrors();
                    this.SetSelectedTab(ContentTypes.Errors);
                }
                else
                {
                    if (RPA_Workbench.Properties.Settings.Default.Setting_MinimizeOnRun == true)
                    {
                        mainWindowLocal.WindowState = WindowState.Minimized;
                        model.RunWorkflow(mainWindowLocal);
                    }
                    else
                    {
                        model.RunWorkflow();
                    }
                    
                }
            }

        }

        private void Abort()
        {
            WorkflowViewModel model = this.ActiveWorkflowViewModel;
            if (model != null)
            {
                model.Abort();
            }
        }

        private void StartWithDebugging()
        {
            WorkflowViewModel model = this.ActiveWorkflowViewModel;
            if (model != null)
            {
                this.ViewOutput();
                this.ViewDebug();
                this.SetSelectedTab(ContentTypes.Debug);
                if (this.HasValidationErrors)
                {
                    this.ViewErrors();
                    this.SetSelectedTab(ContentTypes.Errors);
                }

                model.DebugWorkflow();
                this.OnPropertyChanged("DebugView");
            }
        }

        private void ViewToolbox()
        {
            this.CreateOrUnhideDockableContent(ContentTypes.Toolbox, "Toolbox", "ToolboxView");
        }

        private void ViewPropertyInspector()
        {
           this.CreateOrUnhideDockableContent(ContentTypes.PropertyInspector, "Properties", "PropertyInspectorView", this._PropertiesToolWindow);
        }

        private void ViewOutput()
        {
            this.CreateOrUnhideDockableContent(ContentTypes.Output, "Output", "OutputView" );
        }

        private void ViewErrors()
        {
            this.CreateOrUnhideDockableContent(ContentTypes.Errors, "Errors", "ValidationErrorsView");
        }

        private void ViewDebug()
        {
               this.CreateOrUnhideDockableContent(ContentTypes.Debug, "Debug", "DebugView");
        }

        private void ViewOutline()
        {
            this.CreateOrUnhideDockableContent(ContentTypes.Outline, "Outline", "OutlineView");
        }

        public void RefreshToolbox()
        {
            ActivitiesView.Categories.Clear();
            toolboxControl.Categories.Clear();
          //  InitialiseToolbox();
            UpdateViews();
        }
        public void AddReference()
        {
            try
            {
                OpenFileDialog fileDialog = WorkflowFileDialogFactory.CreateAddReferenceDialog();
                fileDialog.Multiselect = true;
                if (fileDialog.ShowDialog() == true)
                {
                    //foreach (var filename in fileDialog.SafeFileNames)
                    //{
                    //    File.Copy(fileDialog.FileName, Environment.CurrentDirectory + "\\" + filename, true);
                    //}
                    this.AddActivitiesFromAssemblies(fileDialog.FileNames);
                    this.AddAllAddReferencesToFileToToolBox(CurrentProjectPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
          
        }

        public void AddReference(object parameters)
        {
            try
            {
                OpenFileDialog fileDialog = WorkflowFileDialogFactory.CreateAddReferenceDialog();
                fileDialog.Multiselect = true;
                if (fileDialog.ShowDialog() == true)
                {
                    //foreach (var filename in fileDialog.SafeFileNames)
                    //{
                    //    File.Copy(fileDialog.FileName, Environment.CurrentDirectory + "\\" + filename, true);
                    //}
                    this.AddActivitiesFromAssemblies(fileDialog.FileNames);
                    this.AddAllAddReferencesToFileToToolBox(CurrentProjectPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }


        private void Exit()
        {
            if (!this.CancelCloseAllWorkflows())
            {
                foreach (DocumentWindow documentContent in new List<DocumentWindow>(this.dockingManager.Documents.Count))
                {
                    WorkflowViewModel vm = documentContent.DataContext as WorkflowViewModel;
                    //documentContent.Closing -= vm.Closing;
                    documentContent.Close();
                }

                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Application.Current.Shutdown();
            }
        }

        private void SaveAll()
        {
            this.SaveAllWorkflows();
        }

        private void About()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show(string.Format(CultureInfo.InvariantCulture, Resources.AboutDialogMessage, version.ToString(4)), Resources.AboutDialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InitialiseToolbox()
        {
            WorkflowDesignerIcons.UseWindowsStoreAppStyleIcons();
            this.loadedToolboxActivities = new Dictionary<ToolboxCategory, IList<string>>();
            this.toolboxCategoryMap = new Dictionary<string, ToolboxCategory>();

            this.AddCategoryToToolbox(
                Resources.ControlFlowCategoryToToolbox,
                new List<Type>
                {
                    typeof(ForEach<>),
                    typeof(If),
                    typeof(Parallel),
                    typeof(ParallelForEach<>),
                    typeof(DoWhile),
                    typeof(Pick),
                    typeof(PickBranch),
                    typeof(Sequence),
                    typeof(Switch<>),
                    typeof(While)
                });

            this.AddCategoryToToolbox(
                Resources.FlowchartCategoryToToolbox,
                new List<Type>
                {
                    typeof(Flowchart),
                    typeof(FlowDecision),
                    typeof(FlowSwitch<>)
                });

            this.AddCategoryToToolbox(
              "Test",
               new List<Type>
               {
                    typeof(RPAWorkbench.UiAutomation.Activities.TestAutomationDesigner),
               });

            //this.AddCategoryToToolbox(
            //    Resources.MessagingCategoryToToolbox,
            //    new List<Type>
            //    {
            //        typeof(CorrelationScope),
            //        typeof(InitializeCorrelation),
            //        typeof(Receive),
            //        typeof(ReceiveAndSendReplyFactory),
            //        typeof(Send),
            //        typeof(SendAndReceiveReplyFactory),
            //        typeof(TransactedReceiveScope)
            //    });

            this.AddCategoryToToolbox(
                Resources.RuntimeCategoryToToolbox,
                new List<Type>
                {
                    typeof(Persist),
                    typeof(TerminateWorkflow),
                });

            this.AddCategoryToToolbox(
                Resources.PrimitivesCategoryToToolbox,
                new List<Type>
                {
                    typeof(Assign),
                    typeof(Delay),
                    typeof(InvokeMethod),
                    typeof(WriteLine),
                });

            this.AddCategoryToToolbox(
                Resources.TransactionCategoryToToolbox,
                new List<Type>
                {
                    typeof(CancellationScope),
                    typeof(CompensableActivity),
                    typeof(Compensate),
                    typeof(Confirm),
                    typeof(TransactionScope),
                });

            this.AddCategoryToToolbox(
                Resources.CollectionCategoryToToolbox,
                new List<Type>
                {
                    typeof(AddToCollection<>),
                    typeof(ClearCollection<>),
                    typeof(ExistsInCollection<>),
                    typeof(RemoveFromCollection<>),
                });

            this.AddCategoryToToolbox(
                Resources.ErrorHandlingCategoryToToolbox,
                new List<Type>
                {
                    typeof(Rethrow),
                    typeof(Throw),
                    typeof(TryCatch),
                });

        }

        private void AddActivitiesFromAssemblies(IEnumerable<string> assemblyFiles)
        {
            var assemblies = new List<Assembly>();
            foreach (string fileName in assemblyFiles)
            {
                Assembly assembly = Assembly.LoadFrom(fileName);
                assemblies.Add(assembly);
                this.AddCategoryToToolbox(assemblies);
            }

            for (int i = 0; i < assemblies.Count; i++)
            {
                AddReferencesToFile(assemblies[i]);
            }
        }
        private void AddActivitiesFromAssemblies(IEnumerable<Reference> assemblyFiles)
        {
            var assemblies = new List<Assembly>();
            if (assemblyFiles != null)
            {

                foreach (Reference fileName in assemblyFiles)
                {
                    Assembly assembly = Assembly.LoadFrom(fileName.Location);
                    assemblies.Add(assembly);
                    this.AddCategoryToToolbox(assemblies);
                }
            }
        }

        private void RemoveActivitiesFromAssemblies(IEnumerable<string> assemblyFiles)
        {
            foreach (var item in SelectedDependencyPath)
            {
                MessageBox.Show(item);
            }
            var assemblies = new List<Assembly>();
            foreach (string fileName in assemblyFiles)
            {
                Assembly assembly = Assembly.LoadFrom(fileName);
                assemblies.Add(assembly);
                this.RemoveCategoryFromToolbox(assemblies);
            }
            toolboxControl = null;
            toolboxControl = new ToolboxControl();
            InitialiseToolbox();
            ActivitiesView = toolboxControl;
            //Check if there are any dependencies, then add them to the toolbox as well
            this.AddAllAddReferencesToFileToToolBox(CurrentProjectPath);
            UpdateViews();
            //SelectedDependencyPath.Clear();
        }
        //private void AddActivitiesFromAssemblies(List<Reference> assemblyFiles)
        //{
        //    var assemblies = new List<Assembly>();
        //    foreach (var fileName in assemblyFiles)
        //    {
        //        MessageBox.Show(fileName.Location);
        //        Assembly assembly = Assembly.LoadFrom(fileName.Location);
        //        assemblies.Add(assembly);
        //        this.AddCategoryToToolbox(assemblies);
        //    }
        //}


        private class Reference
        {
            string _name;
            public string Name {
                get { return _name; }
                set { _name = value; }
            }


            string _location;
            public string Location
            {
                get { return _location; }
                set { _location = value; }
            }

            string _fullname;
            public string FullName
            {
                get { return _fullname; }
                set { _fullname = value; }
            }

            //Version _version;
            //public Version Version
            //{
            //    get { return _version; }
            //    set { _version = value; }
            //}
        }
        List<Reference> references;
        Reference reference;
        private void AddReferencesToFile(Assembly assembly)
        {
            //Step 1. Create .root folder if it does not exist.
            if (Directory.Exists(CurrentProjectPath + "//.root") == false)
            {
                Directory.CreateDirectory(CurrentProjectPath + "//.root");
            }

            #region Different way to add to json
            //Reference reference = new Reference
            //{
            //    Name = assembly.GetName().Name,
            //    Location = assembly.Location,
            //    FullName = assembly.FullName//,
            //   // Version = assembly.GetName().Version
            //};
            #endregion

            //Step 2. Read the Dependencies.json File and get any dependencies.
            StreamReader streamReader = new StreamReader(CurrentProjectPath + "\\.root" + "\\Dependencies.json");
            string DependecyFileContents = streamReader.ReadToEnd();

            //Step 3. If Dependencies.json is empty start a new reference json array/list and add the depedency.
            if (string.IsNullOrEmpty(DependecyFileContents))
            {
                references = new List<Reference>();
                reference = new Reference
                {
                    Name = assembly.GetName().Name,
                    Location = assembly.Location,
                    FullName = assembly.FullName//,
                   // Version = assembly.GetName().Version
                };
                references.Add(reference);
            }
            else //If Dep file contains depedencies already, make a copy of them, add new one and add the old ones
            {
                references = new List<Reference>();
                List<Reference> originalReferences = JsonConvert.DeserializeObject<List<Reference>>(DependecyFileContents);
                reference = new Reference
                {
                    Name = assembly.GetName().Name,
                    Location = assembly.Location,
                    FullName = assembly.FullName
                };
                bool exists = false;
                //Add Original References, that was in the file again.
                foreach (var item in originalReferences)
                {
                    references.Add(item);
                }
                //Check if reference exists in dependencies already, if it does DO NOT ADD, Else ADD
                foreach (var item in originalReferences)
                {
                    if (item.FullName == reference.FullName)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists == false)
                {
                    references.Add(reference);
                }
            }

            //Step 4. Close the reader, otherwise it will say the Dependencies.json, is being used by another process
            streamReader.Close();

            //Step 5. Serialize the references to json array
            string json = JsonConvert.SerializeObject(references, Formatting.Indented);

            //Step 6. write the json to the Dependencies.json File.
            TextWriter tsw = new StreamWriter(CurrentProjectPath + "\\.root" + "\\Dependencies.json");
            tsw.Write(json);
            tsw.Close();

        }

        public void AddAllAddReferencesToFileToToolBox()
        {
            string json;
            dynamic jsonObj;
            json = File.ReadAllText(CurrentProjectPath + "\\project.json");
            jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string ProjectCurrentDirectory = jsonObj["ProjectPath"];


            if (ProjectCurrentDirectory != null)
            {
                string JsonFile = ProjectCurrentDirectory + "\\.root" + "\\Dependencies.json";
                JsonControls jsonControls = new JsonControls();
                jsonControls.ReadJsonFile(JsonFile);
                jsonControls.DeserializeJsonObject();

                List<Reference> products = JsonConvert.DeserializeObject<List<Reference>>(File.ReadAllText(JsonFile));
                AddActivitiesFromAssemblies(products);
                UpdateViews();
            }

        }
        public void AddAllAddReferencesToFileToToolBox(string ProjectPath)
        {
            string json;
            dynamic jsonObj;
           
            //MessageBox.Show(ProjectPath);
            json = File.ReadAllText(ProjectPath + "\\project.json");
            jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string ProjectCurrentDirectory = jsonObj["ProjectPath"];


            if (ProjectCurrentDirectory != null)
            {
                string JsonFile = ProjectCurrentDirectory + "\\.root" + "\\Dependencies.json";
                JsonControls jsonControls = new JsonControls();
                jsonControls.ReadJsonFile(JsonFile);
                jsonControls.DeserializeJsonObject();

                List<Reference> products = JsonConvert.DeserializeObject<List<Reference>>(File.ReadAllText(JsonFile));
                AddActivitiesFromAssemblies(products);
                UpdateViews();
            }

        }
        private bool IsValidToolboxActivity(Type activityType)
        {
            return activityType.IsPublic && !activityType.IsNested && !activityType.IsAbstract
                && (typeof(Activity).IsAssignableFrom(activityType) || typeof(IActivityTemplateFactory).IsAssignableFrom(activityType) || typeof(FlowNode).IsAssignableFrom(activityType));
        }

        private void AddCategoryToToolbox(List<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type activityType in assembly.GetTypes())
                {
                    if (this.IsValidToolboxActivity(activityType))
                    {
                        ToolboxCategory category = this.GetToolboxCategory(activityType.Namespace);

                        if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
                        {
                            this.loadedToolboxActivities[category].Add(activityType.FullName);
                            category.Add(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, activityType.Name));
                        }
                    }
                }
            }
        }

        private void AddCategoryToToolbox(string categoryName, List<Type> activities)
        {
            foreach (Type activityType in activities)
            {
                if (this.IsValidToolboxActivity(activityType))
                {
                    ToolboxCategory category = this.GetToolboxCategory(categoryName);

                    if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
                    {
                        string displayName;
                        string[] splitName = activityType.Name.Split('`');
                        if (splitName.Length == 1)
                        {
                            displayName = activityType.Name;
                        }
                        else
                        {
                            displayName = string.Format("{0}<>", splitName[0]);
                        }

                        this.loadedToolboxActivities[category].Add(activityType.FullName);
                        category.Add(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, displayName));
                    }
                }
            }
        }

        private void AddActivitiesToToolbox(List<Type> activities)
        {
            foreach (Type activityType in activities)
            {
                if (this.IsValidToolboxActivity(activityType))
                {
                    ToolboxCategory category = this.GetToolboxCategory(activityType.Namespace);

                    if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
                    {
                        this.loadedToolboxActivities[category].Add(activityType.FullName);
                        category.Add(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, activityType.Name));
                    }
                }
            }
        }

        private void RemoveCategoryFromToolbox(List<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type activityType in assembly.GetTypes())
                {
                    if (this.IsValidToolboxActivity(activityType))
                    {
                        ToolboxCategory category = this.GetToolboxCategory(activityType.Namespace);

                        if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
                        {
                            this.loadedToolboxActivities[category].Remove(activityType.FullName);
                            category.Remove(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, activityType.Name));
                        }
                    }
                }
            }
        }

        private List<Type> GetReferencedActivities(ModelService modelService)
        {
            IEnumerable<ModelItem> items = modelService.Find(modelService.Root, typeof(Activity));
            List<Type> activities = new List<Type>();
            foreach (ModelItem item in items)
            {
                if (!namespacesToIgnore.Contains(item.ItemType.Namespace))
                {
                    activities.Add(item.ItemType);
                }
            }

            return activities;
        }

        private ToolboxCategory GetToolboxCategory(string name)
        {
            if (this.toolboxCategoryMap.ContainsKey(name))
            {
                return this.toolboxCategoryMap[name];
            }
            else
            {
                ToolboxCategory category = new ToolboxCategory(name);
                this.toolboxCategoryMap[name] = category;
                this.loadedToolboxActivities.Add(category, new List<string>());
                this.toolboxControl.Categories.Add(category);
                return category;
            }
        }

        private void CreateOrUnhideDockableContent(ContentTypes contentType, string title, string viewPropertyName, object toolwindow = null)
        {
            if (!this.dockableContents.Keys.Contains(contentType))
            {
                ToolWindow dockableContent = new ToolWindow();

                ContentControl contentControl = new ContentControl();

                dockableContent.CanClose = true;
              //  dockableContent.HideOnClose = false;

                dockableContent.Title = title;

                dockableContent.Content = contentControl;

                //if (parent is DockSite)
                //{
                //    dockingManager.ToolWindows.Add(dockableContent);
                //    ResizingPanel resizingPanel = parent as ResizingPanel;
                ViewModels.WorkflowStudioIntegration.WorkflowViewModel workflowViewModel = new WorkflowViewModel(false); // RUWE TEST
                if (designer != null)
                {
                    switch (contentType)
                    {
                        case ContentTypes.PropertyInspector:
                            _PropertiesToolWindow.Content = designer.PropertyInspectorView;
                        break;
                        case ContentTypes.Outline:
                            _OutlineToolWindow.Content = OutlineView;
                            break;
                        case ContentTypes.Toolbox:
                            ActivitiesView = toolboxControl;
                            break;
                        case ContentTypes.Errors:
                            _ErrorsToolWindow.Content = workflowViewModel.ValidationErrorsView;
                            break;
                        case ContentTypes.Debug:
                            _DebugToolWindow.Content = workflowViewModel.DebugView;
                            break;
                    }
                }
                UpdateViews();
                //}
                //else if (parent is DockablePane)
                //{
                //    DockablePane dockablePane = parent as DockablePane;
                //    dockablePane.Items.Add(dockableContent);
                //    if (dockablePane.Parent == null)
                //    {
                //        this.verticalResizingPanel.Children.Add(dockablePane);
                //    }
                //}

                //Binding dataContextBinding = new Binding(viewPropertyName);
                //dockableContent.SetBinding(DockableContent.DataContextProperty, dataContextBinding);

                //Binding contentBinding = new Binding(".");
                //contentControl.SetBinding(ContentControl.ContentProperty, contentBinding);

                //this.dockableContents[contentType] = dockableContent;

                //dockableContent.Closed += delegate (object sender, EventArgs args)
                //{
                //    contentControl.Content = null;
                //    this.dockableContents[contentType].DataContext = null;
                //    this.dockableContents.Remove(contentType);
                //};
            }
            else
            {
                //if (this.dockableContents[contentType].State == DockableContentState.Hidden)
                //{
                //    this.dockableContents[contentType].Show();
                //}
            }
        }

        private void SetSelectedTab(ContentTypes contentType)
        {
            if (this.dockableContents.Keys.Contains(contentType))
            {
                this.tabsPane.Child = this.dockableContents[contentType];
            }
        }

        private IList<WorkflowViewModel> GetModifiedWorkflows()
        {
            List<WorkflowViewModel> modifiedWorkflows = new List<WorkflowViewModel>();

            foreach (DocumentWindow document in this.dockingManager.Documents)
            {
                if (document is WorkflowDocumentContent)
                {
                    WorkflowViewModel model = document.DataContext as WorkflowViewModel;
                    if (model.IsModified)
                    {
                        modifiedWorkflows.Add(model);
                    }
                }
            }

            return modifiedWorkflows;
        }

        private bool SaveAllWorkflows()
        {
            foreach (WorkflowViewModel model in this.GetModifiedWorkflows())
            {
                if (!model.SaveWorkflow())
                {
                    return false;
                }
            }

            return true;
        }

        private string FormatUnsavedWorkflowNames(IList<WorkflowViewModel> workflowViewModels)
        {
            StringBuilder sb = new StringBuilder();

            foreach (WorkflowViewModel model in this.GetModifiedWorkflows())
            {
                sb.Append(model.DisplayName);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        private void UpdateViews()
        {
            try
            {
                if (ActiveWorkflowViewModel != null)
                {
                    //ActivitiesToolWindow.Content = toolboxControl;
                    _PropertiesToolWindow.Content = PropertyInspectorView;
                    _OutlineToolWindow.Content = OutlineView;
                    //_OutputToolWindow.Content = OutputView;
                    _ErrorsToolWindow.Content = ValidationErrorsView;
                    _DebugToolWindow.Content = DebugView;
                }

                if (dockingManager.DocumentWindows.Count <= 0)
                {
                    _PropertiesToolWindow.Content = null;
                    _OutlineToolWindow.Content = null;
                }
                

                this.OnPropertyChanged("PropertyInspectorView");
                this.OnPropertyChanged("ToolboxView");
                this.OnPropertyChanged("ActivitiesView");
                this.OnPropertyChanged("ValidationErrorsView");
                this.OnPropertyChanged("DebugView");
                this.OnPropertyChanged("OutputView");
                this.OnPropertyChanged("OutlineView");
            }
            catch (NullReferenceException)
            {

            }
         
        }
        #endregion
    }
}
