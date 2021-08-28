//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// Third Party Code: This file is based on or incorporates material from the projects listed below (collectively, “Third Party Code”).
// Microsoft is not the original author of the Third Party Code. The original copyright notice, as well as the license under which 
// Microsoft received such Third Party Code, are set forth below. Such licenses and notices are provided for informational purposes only.
// Microsoft, not the third party, licenses the Third Party Code to you under the terms set forth in the EULA for AvalonDock.
// Unless applicable law gives you more rights, Microsoft reserves all other rights not expressly granted under this agreement,
// whether by implication, estoppel or otherwise.  
//
// AvalonDock project, available at http://avalondock.codeplex.com. Copyright (c) 2007-2009, Adolfo Marinucci. All rights reserved.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
// OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
//-----------------------------------------------------------------------
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
        Views.BackstageMenu backstageMenuLocal;
        string _currentProjectPath;
        public string CurrentProjectPath;

        ToolWindow _PropertiesToolWindow;
        ToolWindow _OutlineToolWindow;
        ToolWindow _OutputToolWindow;
        ToolWindow _ErrorsToolWindow;
        ToolWindow _DebugToolWindow;
        public MainWindowViewModel(ToolWindow PropertiesToolwindow, ToolWindow OutlineToolWindow, ToolWindow OutputToolWindow, ToolWindow ErrorsToolWindow,
             ToolWindow DebugToolWindow, DockSite dockingManager = null, TabbedMdiHost tabsPane = null, ToolWindow DebuggerWindow = null,  MainWindow mainWindow = null,
            Views.BackstageMenu backstageMenu = null, String CurrentProjectPath = null)
        {
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

          
        }

       

        void RelaySetup()
        {
            //OpenWorkflowCommand = new RelayCommand(new Action<object>(OpenWorkflow)); //Not Needed as a command
            OpenStartPageCommand = new RelayCommand(new Action<object>(OpenStartPage));
            CloseProjectCommand = new RelayCommand(new Action<object>(CloseProject));
            RunProjectFromJsonCommand = new RelayCommand(new Action<object>(StartWithoutDebuggingFromJson));
            OpenBackstageCommand = new RelayCommand(new Action<object>(StartWithoutDebuggingFromJson));
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
                        model.Designer.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
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
                        model.Designer.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
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
                        json = File.ReadAllText(CurrentProjectPath + "\\project.json");
                        jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        string ProjectCurrentDirectory = jsonObj["ProjectPath"];
                        model.FullFilePath = ProjectCurrentDirectory + "\\" + jsonObj["Main"];
                        model.Designer.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
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
                return model == null ? null : model.Designer.PropertyInspectorView;
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
            model.Designer.Load(ProjectCurrentDirectory + "\\" + jsonObj["Main"]);
            if (model != null)
            {
                if (this.HasValidationErrors)
                {
                    Console.WriteLine($"There was an error in '{jsonObj["Main"]}', can't run");
                    this.ViewErrors();
                    this.SetSelectedTab(ContentTypes.Errors);
                }
                else
                {
                    model.RunWorkflow();
                    this.ViewOutput();
                    this.SetSelectedTab(ContentTypes.Output);
                }
                // model.RunWorkflow();
            }
        }

        private void OpenBackstage(object parameter)
        {
            
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

            designer = workspaceViewModel.Designer;
            ModelService modelService = workspaceViewModel.Designer.Context.Services.GetService<ModelService>();
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
                    model.RunWorkflow();
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
