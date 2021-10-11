//-----------------------------------------------------------------------
// <copyright file="WorkflowViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
//-----------------------------------------------------------------------
namespace RPA_Workbench.ViewModels.WorkflowStudioIntegration
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.ServiceModel.Activities;
    using System.Windows;
    using System.Windows.Controls;
    using RPA_Workbench.Execution;
    using RPA_Workbench.Properties;
    using RPA_Workbench.Utilities;
    using RPA_Workbench.Views;
    using RPA.Workbench.AutomationEngine;
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Threading;
    using System.Xml;
    using System.Collections;
    using System.Xaml;
    using ActiproSoftware.Windows.Controls.Grids;
    using CustomControls.CustomToolbox;

    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Activities;
    using System.Linq;
    using RPA.Workbench.Interfaces;
    using System.Activities.Debugger;
    using System.Activities.Presentation.Debug;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Reflection;
    using Newtonsoft.Json.Linq;
    using RPA_Workbench.Views.WFDesigner;
    using System.Text;
    using System.Activities.Core.Presentation;

    public class WorkflowViewModel : ViewModelBase, System.ComponentModel.INotifyPropertyChanged, IDesigner
    {
        private static int designerCount;
        private WorkflowDesigner workflowDesigner;
        private IList<ValidationErrorInfo> validationErrors;
        private ValidationErrorService validationErrorService;
        private ValidationErrorsUserControl validationErrorsView;
        private bool modelChanged;
        public TextWriter output;
        public TreeListBox outputTextBox;
        private RPA.Workbench.AutomationEngine.Execution.IWorkflowRunner runner;
        private int id;
        private string fullFilePath;
        private bool disableDebugViewOutput;
        private MenuItem comment;
        private MenuItem uncomment;
        private string SelectedVariableName = null;
        public ModelItem SelectedActivity { get; private set; }
        public Dictionary<ModelItem, SourceLocation> _modelLocationMapping = new Dictionary<ModelItem, SourceLocation>();
        public Dictionary<string, SourceLocation> _sourceLocationMapping = new Dictionary<string, SourceLocation>();
        public Dictionary<string, Activity> _activityIdMapping = new Dictionary<string, Activity>();
        public Dictionary<Activity, SourceLocation> _activitysourceLocationMapping = new Dictionary<Activity, SourceLocation>();
        public Dictionary<string, ModelItem> _activityIdModelItemMapping = new Dictionary<string, ModelItem>();

        private Selection selection = null;

        public Action<WorkflowDesigner> OnChanged { get; set; }
        public WorkflowViewModel(bool disableDebugViewOutput)
        {
            var metaData = new DesignerMetadata();
            metaData.Register();
            RPAWorkbench.UiAutomation.Activities.TestAutomationMetadata.RegisterAll();

            this.workflowDesigner = new WorkflowDesigner();

            metaData.Register();
            RPAWorkbench.UiAutomation.Activities.TestAutomationMetadata.RegisterAll();

            LoadTheme(workflowDesigner);
            this.id = ++designerCount;
            this.validationErrors = new List<ValidationErrorInfo>();
            this.validationErrorService = new ValidationErrorService(this.validationErrors);
            this.workflowDesigner.Context.Services.Publish<IValidationErrorService>(this.validationErrorService);
     
            this.workflowDesigner.ModelChanged += delegate (object sender, EventArgs args)
            {
                this.modelChanged = true;
                this.OnPropertyChanged("DisplayNameWithModifiedIndicator");
            };

            this.validationErrorsView = new ValidationErrorsUserControl();

            this.outputTextBox = new TreeListBox();
            this.output = new TextBoxStreamWriter(this.outputTextBox, this.DisplayName);
            this.disableDebugViewOutput = disableDebugViewOutput;


            //this.workflowDesigner.Context.Services.GetService<DesignerConfigurationService>().TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5));
            //this.workflowDesigner.Context.Services.GetService<DesignerConfigurationService>().LoadingFromUntrustedSourceEnabled = true;

            DesignerConfigurationService configService = workflowDesigner.Context.Services.GetRequiredService<DesignerConfigurationService>();
            configService.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5));
            configService.AnnotationEnabled = true;
            configService.AutoConnectEnabled = true;
            configService.AutoSplitEnabled = true;
            configService.AutoSurroundWithSequenceEnabled = true;
            configService.BackgroundValidationEnabled = true;
            configService.MultipleItemsContextMenuEnabled = true;
            configService.MultipleItemsDragDropEnabled = true;
            configService.NamespaceConversionEnabled = true;
            configService.PanModeEnabled = true;
            configService.RubberBandSelectionEnabled = true;
            configService.LoadingFromUntrustedSourceEnabled = true;
            WorkflowDesigner.Context.Services.Publish<IExpressionEditorService>(new RPA.Workbench.CodeEditor.EditorService(this));
            //SetupDesignerCustomContextMenuItems();
            //workflowDesigner.Context.Services.Publish<IExpressionEditorService>(new EditorService(this));
            this.validationErrorService.ErrorsChangedEvent += delegate (object sender, EventArgs args)
            {
                DispatcherService.Dispatch(() =>
                {
                    this.validationErrorsView.ErrorsDataGrid.ItemsSource = this.validationErrors;
                    this.validationErrorsView.ErrorsDataGrid.Items.Refresh();
                });
            };
        }
        public bool ReadOnly
        {
            get
            {
                try
                {
                    return workflowDesigner.Context.Items.GetValue<System.Activities.Presentation.Hosting.ReadOnlyState>().IsReadOnly;
                }
                catch (Exception ex)
                {
                    Log.Error("WFDesigner:ReadOnly: " + ex.ToString());
                    return false;
                }
            }
            set
            {
                try
                {
                    workflowDesigner.Context.Items.GetValue<System.Activities.Presentation.Hosting.ReadOnlyState>().IsReadOnly = value;
                }
                catch (Exception ex)
                {
                    Log.Error("WFDesigner:Set.ReadOnly: " + ex.ToString());
                }
            }
        }


        public void OnUncomment(object sender, RoutedEventArgs e)
        {
            var thisselection = selection;
            var comment = SelectedActivity;
            var currentSequence = SelectedActivity.Properties["Body"].Value;
            if (currentSequence == null) return;
            //var newSequence = GetActivitiesScope(SelectedActivity.Parent.Parent);
            var newSequence = GetActivitiesScope(SelectedActivity.Parent);
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

            var lastSequence = GetActivitiesScope(SelectedActivity.Parent);
            if (lastSequence == null) lastSequence = GetActivitiesScope(SelectedActivity);
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
                    Activities.Remove(SelectedActivity);
                    Activities.Add(co);
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
                    AddActivity(co);
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
                    AddActivity(co);
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
                            var comment = AddActivity(co);
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


        public ModelItem AddActivity(Activity a)
        {
            ModelItem newItem = null;
            ModelService modelService = workflowDesigner.Context.Services.GetService<ModelService>();
            using (ModelEditingScope editingScope = modelService.Root.BeginEdit("Implementation"))
            {
                var lastSequence = GetSequence(SelectedActivity);
                if (lastSequence == null && SelectedActivity != null) lastSequence = GetActivitiesScope(SelectedActivity.Parent);
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
        private ModelItem GetSequence(ModelItem from)
        {
            ModelItem parent = from;
            while (parent != null && !parent.ItemType.Equals(typeof(Sequence)))
            {
                parent = parent.Parent;
            }
            return parent;
        }
        private ModelItem GetVariableScope(ModelItem from)
        {
            ModelItem parent = from;

            while (parent != null && parent.Properties["Variables"] == null)
            {
                parent = parent.Parent;
            }
            return parent;
        }
        private ModelItem GetActivitiesScope(ModelItem from)
        {
            ModelItem parent = from;

            while (parent != null && parent.Properties["Activities"] == null && parent.Properties["Handler"] == null && parent.Properties["Nodes"] == null)
            {
                parent = parent.Parent;
            }
            return parent;
        }
        public Workflow Workflow { get; private set; }
        #region Presentation Properties

        public UIElement DebugView
        {
            get
            {
                IWorkflowDebugger debugger = this.runner as IWorkflowDebugger;
                if (debugger != null)
                {
                    return debugger.GetDebugView();
                }
                else
                {
                    return null;
                }
            }
        }

        public UIElement OutlineView
        {
            get
            {
                return this.workflowDesigner.OutlineView;
            }
        }

        public UIElement ValidationErrorsView
        {
            get
            {
                return this.validationErrorsView;
            }
        }

        public bool HasValidationErrors
        {
            get
            {
                return this.validationErrors.Count > 0;
            }
        }

        public UIElement OutputView
        {
            get
            {
                return this.outputTextBox;
            }
        }

        public TextWriter Output
        {
            get
            {
                return this.output;
            }
        }

        public WorkflowDesigner WorkflowDesigner
        {
            get
            {
                return this.workflowDesigner;
            }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.FullFilePath))
                {
                    return string.Format(Resources.NewWorkflowTabTitle, this.id);
                }
                else
                {
                    return Path.GetFileName(this.FullFilePath);
                }
            }
        }

        public string DisplayNameWithModifiedIndicator
        {
            get
            {
                string modifiedIndicator = this.modelChanged ? "*" : string.Empty;
                if (string.IsNullOrEmpty(this.FullFilePath))
                {
                    return string.Format(Resources.NewWorkflowWithModifierTabTitle, this.id, modifiedIndicator);
                }
                else
                {
                    return string.Format("{0} {1}", Path.GetFileName(this.FullFilePath), modifiedIndicator);
                }
            }
        }

        public string FullFilePath
        {
            get
            {
                return this.fullFilePath;
            }

            set
            {
                this.fullFilePath = value;
                this.output = new TextBoxStreamWriter(this.outputTextBox, Path.GetFileNameWithoutExtension(this.fullFilePath));
            }
        }

        public bool IsRunning
        {
            get
            {
                return this.runner == null ? false : this.runner.IsRunning;
            }
        }

        public bool IsModified
        {
            get
            {
                return this.modelChanged;
            }
        }

        public bool HasChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public bool BreakPointhit { get; set; }
        public bool Singlestep { get; set; }
        public bool SlowMotion { get; set; }
        public bool VisualTracking { get; set; }
        public bool IsRunnning
        {
            get
            {
                foreach (var i in WorkflowInstance.Instances.ToList())
                {
                    if (!string.IsNullOrEmpty(Workflow._id) && i.WorkflowId == Workflow._id)
                    {
                        if (i.state != "completed" && i.state != "aborted" && i.state != "failed")
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        private object _Properties;
        public object Properties
        {
            get
            {
                return _Properties;
            }
            set
            {
                _Properties = value;
                OnPropertyChanged("Properties");
            }
        }

        public System.Threading.AutoResetEvent ResumeRuntimeFromHost { get; set; }
        public Activity Lastinserted { get; set; }
        public ModelItem Lastinsertedmodel { get; set; }


        IWorkflow IDesigner.Workflow { get => Workflow; set { } }
        public IDictionary<SourceLocation, System.Activities.Presentation.Debug.BreakpointTypes> BreakpointLocations { get; set; }

        #endregion

        public void Abort()
        {
            if (this.runner != null)
            {
                this.runner.Abort();
            }
        }

        public void Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            if (this.IsRunning)
            {
                MessageBoxResult closingResult = MessageBox.Show(Resources.ConfirmCloseWhenRunningWorkflowDialogMessage, Resources.ConfirmCloseWhenRunningWorkflowDialogTitle, MessageBoxButton.YesNo);
                switch (closingResult)
                {
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        break;
                    case MessageBoxResult.Yes:
                        e.Cancel = false;
                        this.Abort();
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }

            if (e.Cancel == false)
            {
                if (this.modelChanged)
                {
                    MessageBoxResult closingResult = MessageBox.Show(string.Format(Resources.SaveChangesDialogMessage, this.DisplayName), Resources.SaveChangesDialogTitle, MessageBoxButton.YesNoCancel);
                    switch (closingResult)
                    {
                        case MessageBoxResult.No:
                            e.Cancel = false;
                            break;
                        case MessageBoxResult.Yes:
                            e.Cancel = !this.SaveWorkflow();
                            break;
                        case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }
        }

        public Type GetRootType()
        {
            ModelService modelService = this.workflowDesigner.Context.Services.GetService<ModelService>();
            if (modelService != null)
            {
                return modelService.Root.GetCurrentValue().GetType();
            }
            else
            {
                return null;
            }
        }

        public bool SaveWorkflow()
        {
            if (!string.IsNullOrEmpty(this.FullFilePath))
            {
                this.SaveWorkflow(this.FullFilePath);
                return true;
            }
            else
            {
                SaveFileDialog fileDialog = WorkflowFileDialogFactory.CreateSaveFileDialog(this.DisplayName);

                if (fileDialog.ShowDialog() == true)
                {
                    this.SaveWorkflow(fileDialog.FileName);
                    return true;
                }

                return false;
            }
        }

        public void SaveAsWorkflow()
        {
            SaveFileDialog fileDialog = WorkflowFileDialogFactory.CreateSaveFileDialog(this.DisplayName);

            if (fileDialog.ShowDialog() == true)
            {
                this.SaveWorkflow(fileDialog.FileName);
            }
        }

        public void RunWorkflow()
        {
            outputTextBox.Items.Clear();
            // System.Diagnostics.Process process = new System.Diagnostics.Process();
            if (this.GetRootType() == typeof(WorkflowService))
            {
                //this.runner = new WorkflowServiceHostRunner(this.output, this.DisplayName, this.workflowDesigner);
            }
            else
            {
                
               // process.StartInfo.FileName = "AutomationEngine.exe";
               // process.StartInfo.Arguments = "Run " + "\"" + fullFilePath + "\"";
               
                //this.runner = new WorkflowRunner(this.output, this.DisplayName, this.workflowDesigner);
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = "AutomationEngine.exe",
                CreateNoWindow = true,
                UseShellExecute = false, // Required to use RedirectStandardOutput
                RedirectStandardOutput = true, //Required to be able to read StandardOutput
                Arguments = "Run " + "\"" + fullFilePath + "\"" // Skip this if you don't use Arguments
            };

            using (var process = new Process { StartInfo = startInfo })
            {
               
                process.Start();
                
                process.OutputDataReceived += (sender, line) =>
                {
  
                    if (line.Data != null)
                    {
                        Console.WriteLine(line.Data);
                    }
                      
                   
                };

                process.BeginOutputReadLine();

                //process.WaitForExit();
                if (this.outputTextBox.Items.Contains("Workflow Complete"))
                {
                    process.Kill();
                }
                
            }
            try
            {
                //this.outputTextBox.Clear();
                //process.StartInfo.UseShellExecute = false;
           
                //process.StartInfo.RedirectStandardOutput = true;

                //process.Start();
                //process.BeginOutputReadLine();
                //this.outputTextBox.Text += process.StandardOutput.ReadToEnd();
               // this.runner.Run();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Resources.ErrorRunningDialogMessage, ExceptionHelper.FormatStackTrace(e)), Resources.ErrorRunningDialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool Completed;
        Process[] automationEngineProcess = Process.GetProcesses();
        public void RunWorkflow(MainWindow mainWindow)
        {
            outputTextBox.Items.Clear();
            var startInfo = new ProcessStartInfo
            {
                FileName = "AutomationEngine.exe",
                CreateNoWindow = true,
                UseShellExecute = false, // Required to use RedirectStandardOutput
                RedirectStandardOutput = true, //Required to be able to read StandardOutput
                Arguments = "Run " + "\"" + fullFilePath + "\"" // Skip this if you don't use Arguments
            };
         
            using (var process = new Process() { StartInfo = startInfo })
            {
                process.Start();
                
                //process.OutputDataReceived += (sender, line) =>
                //{
                //    if (line.Data != null)
                //    {
                //        Console.WriteLine(line.Data);
                //    }
                //};
               // process.BeginOutputReadLine();
                string standard_output;
                while ((standard_output = process.StandardOutput.ReadLine()) != null)
                {
                    Console.WriteLine(standard_output);
                    if (standard_output.Contains("Workflow Completed"))
                    {
                        mainWindow.WindowState = WindowState.Maximized;
                        process.Kill();
                        break;
                    }
                }
            }
            try
            {
                //this.outputTextBox.Clear();
                //process.StartInfo.UseShellExecute = false;
           
                //process.StartInfo.RedirectStandardOutput = true;

                //process.Start();
                //process.BeginOutputReadLine();
                //this.outputTextBox.Text += process.StandardOutput.ReadToEnd();
               // this.runner.Run();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Resources.ErrorRunningDialogMessage, ExceptionHelper.FormatStackTrace(e)), Resources.ErrorRunningDialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        RPA.Workbench.AutomationEngine.Execution.IWorkflowDebugger debugger;
        public void DebugWorkflow()
        {
            if (this.GetRootType() == typeof(WorkflowService))
            {
               // this.runner = new WorkflowServiceHostDebugger(this.output, this.DisplayName, this.workflowDesigner, this.disableDebugViewOutput);
            }
            else
            {
              
                debugger = new RPA.Workbench.AutomationEngine.Execution.WorkflowDebugger(this.output, this.DisplayName, this.workflowDesigner, this.disableDebugViewOutput);
                //this.runner = new RPA.Workbench.AutomationEngine.Execution.WorkflowDebugger(this.output, this.DisplayName, this.workflowDesigner, this.disableDebugViewOutput);
            }

            try
            {
                this.outputTextBox.Items.Clear();
                this.debugger.Run();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Resources.ErrorLoadingInDebugDialogMessage, ExceptionHelper.FormatStackTrace(e)), Resources.ErrorLoadingInDebugDialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveWorkflow(string fullFilePath)
        {
            StatusViewModel.SetStatusText(Resources.SavingStatus, this.DisplayName);

            this.FullFilePath = fullFilePath;
            this.workflowDesigner.Save(this.FullFilePath);
            this.modelChanged = false;
            this.OnPropertyChanged("DisplayName");
            this.OnPropertyChanged("DisplayNameWithModifiedIndicator");

            StatusViewModel.SetStatusText(Resources.SaveSuccessfulStatus, this.DisplayName);
        }

        private void LoadTheme(WorkflowDesigner designer)
        {

            StringReader reader = new StringReader(RPA_Workbench.Properties.Resources.LightTheme);

            XmlReader xmlReader = XmlReader.Create(reader);

            ResourceDictionary fontAndColorDictionary = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(xmlReader);

            Hashtable hashTable = new Hashtable();

            foreach (var key in fontAndColorDictionary.Keys)
            {
                hashTable.Add(key, fontAndColorDictionary[key]);
            }

            designer.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);
            HidePropertyWindowToolbar(designer);

        }

        private void HidePropertyWindowToolbar(WorkflowDesigner designer)
        {
            try
            {
                FrameworkElement fe = (FrameworkElement)designer.PropertyInspectorView;
                fe = (FrameworkElement)fe.FindName("_propertyToolBar");
                if (fe.Visibility == null)
                {
                    fe.Visibility = Visibility.Collapsed;
                }
                else
                {
                    fe.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {


            }

        }

        public Argument GetArgument(string Name, bool add, Type type)
        {
            throw new NotImplementedException();
        }

        public DynamicActivityProperty GetArgumentOf<T>(string Name, bool add)
        {
            throw new NotImplementedException();
        }

        public Variable GetVariable(string Name, Type type)
        {
            try
            {
                MethodInfo method = typeof(WorkflowDesigner).GetMethod("GetVariableOf");
                MethodInfo generic = method.MakeGenericMethod(type);
                var res = generic.Invoke(this, new object[] { Name });
                return (Variable)res;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public Variable<T> GetVariableOf<T>(string Name)
        {
            if (SelectedActivity == null) throw new Exception("Cannot get variable when no activity has been selected");
            var seq = GetVariableScope(SelectedActivity);
            if (seq == null) throw new Exception("Cannot add variables to root activity!");
            Variable<T> result = GetVariableModel<T>(Name, SelectedActivity);
            if (result == null)
            {
                ModelService modelService = workflowDesigner.Context.Services.GetService<ModelService>();
                using (ModelEditingScope editingScope = modelService.Root.BeginEdit("Implementation"))
                {
                    var Variables = seq.Properties["Variables"].Collection;
                    result = new Variable<T>() { Name = Name };
                    Variables.Add(result);
                    editingScope.Complete();
                }
            }
            return result;
        }

        public Variable<T> GetVariableModel<T>(string Name, ModelItem model)
        {
            Variable<T> result = null;

            if (model.Properties["Variables"] != null)
            {
                var Variables = model.Properties["Variables"].Collection;
                foreach (var _v in Variables)
                {
                    var nameprop = (string)_v.Properties["Name"].ComputedValue;
                    if (Name == nameprop) return _v.GetCurrentValue() as Variable<T>;
                }
            }
            if (model.Parent != null)
            {
                result = GetVariableModel<T>(Name, model.Parent);
            }
            return result;
        }

        private void EnsureSourceLocationUpdated()
        {
            var debugView = workflowDesigner.DebugManagerView;

            var nonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            var debuggerServiceType = typeof(System.Activities.Presentation.Debug.DebuggerService);
            var ensureMappingMethodName = "EnsureSourceLocationUpdated";
            var ensureMappingMethod = debuggerServiceType.GetMethod(ensureMappingMethodName, nonPublicInstance);
            _ = ensureMappingMethod.Invoke(debugView, new object[0]);
        }

        private Dictionary<Activity, SourceLocation> CreateSourceLocationMapping(ModelService modelService)
        {
            var debugView = workflowDesigner.DebugManagerView;

            var nonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            var debuggerServiceType = typeof(System.Activities.Presentation.Debug.DebuggerService);
            var mappingFieldName = "instanceToSourceLocationMapping";
            var mappingField = debuggerServiceType.GetField(mappingFieldName, nonPublicInstance);
            if (mappingField == null)
                throw new MissingFieldException(debuggerServiceType.FullName, mappingFieldName);

            if (!(modelService.Root.GetCurrentValue() is Activity rootActivity))
            {
                workflowDesigner.Flush();
                System.Activities.XamlIntegration.ActivityXamlServicesSettings activitySettings = new System.Activities.XamlIntegration.ActivityXamlServicesSettings
                {
                    CompileExpressions = true
                };
                var xamlReaderSettings = new System.Xaml.XamlXmlReaderSettings { LocalAssembly = typeof(WorkflowDesigner).Assembly };
                var xamlReader = new System.Xaml.XamlXmlReader(new System.IO.StringReader(workflowDesigner.Text), xamlReaderSettings);
                rootActivity = System.Activities.XamlIntegration.ActivityXamlServices.Load(xamlReader, activitySettings);
            }
            WorkflowInspectionServices.CacheMetadata(rootActivity);

            EnsureSourceLocationUpdated();
            var mapping = (Dictionary<object, System.Activities.Debugger.SourceLocation>)mappingField.GetValue(debugView);
            var result = new Dictionary<Activity, System.Activities.Debugger.SourceLocation>();
            foreach (var m in mapping)
            {
                try
                {
                    if (m.Key is Activity a) result.Add(a, m.Value);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex.ToString());
                }
            }
            return result;
        }
        public void forceHasChanged(bool value) { HasChanged = value; }

        private void SelectionChanged(Selection item)
        {
            selection = item;
            SelectedActivity = selection.PrimarySelection;
            if (SelectedActivity == null) return;
            SelectedVariableName = SelectedActivity.GetCurrentValue().ToString();

            try
            {
                if (workflowDesigner.ContextMenu.Items.Contains(comment)) workflowDesigner.ContextMenu.Items.Remove(comment);
                if (workflowDesigner.ContextMenu.Items.Contains(uncomment)) workflowDesigner.ContextMenu.Items.Remove(uncomment);
                var lastSequence = GetActivitiesScope(SelectedActivity.Parent);
                if (lastSequence == null) lastSequence = GetActivitiesScope(SelectedActivity);
                if (lastSequence == null) return;
                if (SelectedActivity.ItemType == typeof(BuiltIn.Activities.CommentOut))
                {
                    workflowDesigner.ContextMenu.Items.Add(uncomment);
                }
                else if (lastSequence.ItemType != typeof(Flowchart))
                {
                    if (selection.SelectionCount > 1)
                    {
                        if (lastSequence.Properties["Nodes"] == null)
                        {
                            workflowDesigner.ContextMenu.Items.Add(comment);
                        }
                    }
                    else
                    {
                        workflowDesigner.ContextMenu.Items.Add(comment);
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

        public KeyedCollection<string, DynamicActivityProperty> GetParameters()
        {
            ActivityBuilder ab2;

            using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(Workflow.Xaml)))
            {
                ab2 = System.Xaml.XamlServices.Load(
                    System.Activities.XamlIntegration.ActivityXamlServices.CreateBuilderReader(
                    new System.Xaml.XamlXmlReader(stream))) as ActivityBuilder;
            }
            return ab2.Properties;
        }
        public List<ModelItem> GetWorkflowActivities()
        {
            List<ModelItem> list = new List<ModelItem>();

            ModelService modelService = workflowDesigner.Context.Services.GetService<ModelService>();
            list = modelService.Find(modelService.Root, typeof(Activity)).ToList<ModelItem>();

            list.AddRange(modelService.Find(modelService.Root, (Predicate<Type>)(type => (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(FlowSwitch<>))))));
            list.AddRange(modelService.Find(modelService.Root, typeof(FlowDecision)));
            return list;
        }
        private static List<ModelItem> GetWorkflowActivities(WorkflowDesigner wfDesigner)
        {
            List<ModelItem> list = new List<ModelItem>();

            ModelService modelService = wfDesigner.Context.Services.GetService<ModelService>();
            list = modelService.Find(modelService.Root, typeof(Activity)).ToList<ModelItem>();

            list.AddRange(modelService.Find(modelService.Root, (Predicate<Type>)(type => (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(FlowSwitch<>))))));
            list.AddRange(modelService.Find(modelService.Root, typeof(FlowDecision)));
            return list;
        }

        public Task<bool> SaveAsync()
        {
            throw new NotImplementedException();
        }
        private SourceLocation GetSourceLocationFromModelItem(ModelItem modelItem)
        {
            var debugView = workflowDesigner.DebugManagerView;
            var nonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            var debuggerServiceType = typeof(System.Activities.Presentation.Debug.DebuggerService);
            var ensureMappingMethodName = "GetSourceLocationFromModelItem";
            var ensureMappingMethod = debuggerServiceType.GetMethod(ensureMappingMethodName, nonPublicInstance);
            var res = ensureMappingMethod.Invoke(debugView, new object[] { modelItem });
            return res as System.Activities.Debugger.SourceLocation;
        }
        public void NavigateTo(ModelItem item)
        {
            var validation = workflowDesigner.Context.Services.GetService<System.Activities.Presentation.Validation.ValidationService>();
            //private ModelSearchServiceImpl modelSearchService;

            var modelSearchService = typeof(System.Activities.Presentation.Validation.ValidationService).GetField("modelSearchService", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(validation);
            //this.modelSearchService.NavigateTo(itemToFocus);
            var methods = modelSearchService.GetType().GetMethods().Where(x => x.Name == "NavigateTo");
            foreach (var methodInfo in methods)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length == 1)
                {
                    if (parameters[0].Name == "itemToFocus")
                    {
                        methodInfo.Invoke(modelSearchService, new Object[] { item });
                    }
                }
            }

        }
        public void InitializeStateEnvironment()
        {
            Log.Debug("InitializeStateEnvironment");
            GenericTools.RunUI(() =>
            {
                try
                {
                    var modelService = workflowDesigner.Context.Services.GetService<ModelService>();
                    //IEnumerable<ModelItem> wfElements = modelService.Find(modelService.Root, typeof(Activity)).
                    //Union(modelService.Find(modelService.Root, typeof(System.Activities.Debugger.State)));
                    var wfElements = modelService.Find(modelService.Root, typeof(Activity)).
                    Union(modelService.Find(modelService.Root, typeof(System.Activities.Debugger.State))).ToList();
                    wfElements.Add(modelService.Root);


                    var map = CreateSourceLocationMapping(modelService);
                    _sourceLocationMapping.Clear();
                    _activityIdMapping.Clear();
                    _activitysourceLocationMapping.Clear();
                    _activityIdModelItemMapping.Clear();
                    _modelLocationMapping.Clear();

                    foreach (var modelItem in wfElements)
                    {
                        var loc = GetSourceLocationFromModelItem(modelItem);
                        var activity = modelItem.GetCurrentValue() as Activity;
                        if (activity == null)
                        {
                            var builder = modelItem.GetCurrentValue() as ActivityBuilder;
                            continue;
                        }
                        var id = activity.Id;
                        if (string.IsNullOrEmpty(id)) continue;
                        if (_sourceLocationMapping.ContainsKey(id)) continue;
                        _activitysourceLocationMapping.Add(activity, loc);
                        _sourceLocationMapping.Add(id, loc);
                        _activityIdMapping.Add(id, activity);
                        _activityIdModelItemMapping.Add(id, modelItem);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            });
        }
        private void ShowVariables(IDictionary<string, WorkflowInstanceValueType> Variables)
        {
            GenericTools.RunUI(() =>
            {
                var form = new showVariables
                {
                    variables = new System.Collections.ObjectModel.ObservableCollection<variable>()
                };
                Variables?.ForEach(x =>
                {
                    form.addVariable(x.Key, x.Value.value, x.Value.type);
                });
                Properties = form;
            });
        }
        public void OnVisualTracking(IWorkflowInstance Instance, string ActivityId, string ChildActivityId, string State)
        {
            try
            {
                if (SlowMotion) System.Threading.Thread.Sleep(500);

                //if (_activityIdMapping == null || ActivityId == "1") return;
                //if (!_activityIdMapping.ContainsKey(ActivityId))
                //{
                //    // Log.Debug("Failed locating ActivityId : " + ActivityId);
                //    return;
                //}
                //if (!_sourceLocationMapping.ContainsKey(ActivityId)) return;
                //if (!_sourceLocationMapping.ContainsKey(ChildActivityId)) return;


                System.Activities.Debugger.SourceLocation location;

                //location = _sourceLocationMapping[ActivityId];
                //BreakPointhit = wfDesigner.DebugManagerView.GetBreakpointLocations().ContainsKey(location);

                if (!_sourceLocationMapping.ContainsKey(ChildActivityId) || !_sourceLocationMapping.ContainsKey(ActivityId))
                {
                    InitializeStateEnvironment();
                }

                // InitializeStateEnvironment();
                if (!_sourceLocationMapping.ContainsKey(ChildActivityId)) return;
                location = _sourceLocationMapping[ChildActivityId];
                if (location == null) return;
                if (!BreakPointhit)
                {
                    if (BreakpointLocations == null) BreakpointLocations = workflowDesigner.DebugManagerView.GetBreakpointLocations();
                    BreakPointhit = BreakpointLocations.ContainsKey(location);
                }
                ModelItem model = _activityIdModelItemMapping[ChildActivityId];
                if (VisualTracking || BreakPointhit || Singlestep)
                {
                    GenericTools.RunUI(() =>
                    {
                        GenericTools.Restore();
                        NavigateTo(model);
                        SetDebugLocation(location);

                    });
                }
                if (BreakPointhit || Singlestep)
                {
                    using (ResumeRuntimeFromHost = new System.Threading.AutoResetEvent(false))
                    {
                        BreakPointhit = true;
                        ShowVariables(Instance.Variables);
                        GenericTools.Restore();
                        ResumeRuntimeFromHost.WaitOne();
                    }
                    ResumeRuntimeFromHost = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public void SetDebugLocation(SourceLocation location)
        {
            workflowDesigner.DebugManagerView.CurrentLocation = location;
        }
        public void IdleOrComplete(IWorkflowInstance instance, EventArgs e)
        {
            if (!string.IsNullOrEmpty(instance.queuename) && !string.IsNullOrEmpty(instance.correlationId))
            {
                RPA.Workbench.Interfaces.mq.RobotCommand command = new RPA.Workbench.Interfaces.mq.RobotCommand();
                var data = JObject.FromObject(instance.Parameters);
                command.command = "invoke" + instance.state;
                command.workflowid = instance.WorkflowId;
                command.data = data;
                if ((instance.state == "failed" || instance.state == "aborted") && instance.Exception != null)
                {
                    command.data = JObject.FromObject(instance.Exception);
                }
                Task.Run(async () =>
                {
                    try
                    {
                        await global.webSocketClient.QueueMessage(instance.queuename, command, null, instance.correlationId, 0);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex.Message);
                    }
                });
                OnChanged?.Invoke(WorkflowDesigner);
            }
            if (instance.state == "idle" && Singlestep == true)
            {
                GenericTools.Minimize();
                //GenericTools.RunUI(() =>
                //{
                //    SetDebugLocation(null);
                //    Properties = wfDesigner.PropertyInspectorView;
                //});
            }
            if (instance.state == "completed")
            {
                GenericTools.RunUI(() =>
                {
                    SetDebugLocation(null);
                    Properties = workflowDesigner.PropertyInspectorView;
                });
            }
            if ((string.IsNullOrEmpty(instance.queuename) && string.IsNullOrEmpty(instance.correlationId)) && string.IsNullOrEmpty(instance.caller) && instance.isCompleted && Config.local.minimize)
            {
                GenericTools.RunUI(() =>
                {
                    GenericTools.Restore();
                });
            }

            if (instance.state != "idle")
            {
                GenericTools.RunUI(() =>
                {
                    Properties = workflowDesigner.PropertyInspectorView;
                    if (global.isConnected)
                    {
                        ReadOnly = !Workflow.hasRight(global.webSocketClient.user, RPA.Workbench.Interfaces.entity.ace_right.update);
                    }
                    else
                    {
                        ReadOnly = false;
                    }
                });

                BreakPointhit = false; Singlestep = false;
                bool isRemote = true;
                if (string.IsNullOrEmpty(instance.queuename) && string.IsNullOrEmpty(instance.correlationId))
                {
                    isRemote = false;
                    if (instance.state != "completed")
                    {
                        System.Activities.Debugger.SourceLocation location;
                        if (instance.errorsource != null && !_sourceLocationMapping.ContainsKey(instance.errorsource))
                        {
                            InitializeStateEnvironment();
                        }

                        if (instance.errorsource != null && _sourceLocationMapping.ContainsKey(instance.errorsource))
                        {
                            GenericTools.RunUI(() =>
                            {
                                location = _sourceLocationMapping[instance.errorsource];
                                SetDebugLocation(location);
                                if (_activityIdModelItemMapping.ContainsKey(instance.errorsource))
                                {
                                    ModelItem model = _activityIdModelItemMapping[instance.errorsource];
                                    NavigateTo(model);
                                }
                            });
                        }
                        else
                        {
                            GenericTools.RunUI(() =>
                            {
                                SetDebugLocation(null);
                            });

                        }
                    }
                }
                //string message = "#*****************************#" + Environment.NewLine;
                //if (instance.runWatch != null)
                //{
                //    message += ("# " + instance.Workflow.name + " " + instance.state + " in " + string.Format("{0:mm\\:ss\\.fff}", instance.runWatch.Elapsed));
                //}
                //else
                //{
                //    message += ("# " + instance.Workflow.name + " " + instance.state);
                //}
                //if (!string.IsNullOrEmpty(instance.errormessage)) message += (Environment.NewLine + "# " + instance.errormessage);
                string message = "";
                if (instance.runWatch != null)
                {
                    message += (instance.Workflow.name + " " + instance.state + " in " + string.Format("{0:mm\\:ss\\.fff}", instance.runWatch.Elapsed));
                }
                else
                {
                    message += (instance.Workflow.name + " " + instance.state);
                }
                if (!string.IsNullOrEmpty(instance.errormessage)) message += (Environment.NewLine + "# " + instance.errormessage);
                Log.Output(message);
                if (instance.hasError || instance.isCompleted)
                {
                    if ((Config.local.notify_on_workflow_end && !isRemote) || (Config.local.notify_on_workflow_remote_end && isRemote))
                    {
                        if (instance.state == "completed")
                        {
                            // App.notifyIcon.ShowBalloonTip(1000, instance.Workflow.name + " " + instance.state, message, System.Windows.Forms.ToolTipIcon.Info);
                          //  App.notifyIcon.ShowBalloonTip(1000, "", message, System.Windows.Forms.ToolTipIcon.Info);
                        }
                        else
                        {
                            // App.notifyIcon.ShowBalloonTip(1000, instance.Workflow.name + " " + instance.state, message, System.Windows.Forms.ToolTipIcon.Error);
                           // App.notifyIcon.ShowBalloonTip(1000, "", message, System.Windows.Forms.ToolTipIcon.Error);
                        }
                    }
                }


                OnChanged?.Invoke(workflowDesigner);
            }
            if (instance.hasError || instance.isCompleted)
            {
                _ = Task.Run(() =>
                {
                    var sw = new System.Diagnostics.Stopwatch(); sw.Start();
                    while (sw.Elapsed < TimeSpan.FromSeconds(1))
                    {
                        lock (WorkflowInstance.Instances)
                        {
                            foreach (var wi in WorkflowInstance.Instances.ToList())
                            {
                                if (wi.isCompleted) continue;
                                if (wi.Bookmarks == null) continue;
                                foreach (var b in wi.Bookmarks)
                                {
                                    if (b.Key == instance._id)
                                    {
                                        wi.ResumeBookmark(b.Key, instance);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        }

        public void Run(bool VisualTracking, bool SlowMotion, IWorkflowInstance instance)
        {
            GenericTools.RunUI(() =>
            {
                this.VisualTracking = VisualTracking; this.SlowMotion = SlowMotion;
                if (BreakPointhit)
                {
                    SetDebugLocation(null);
                    Properties = workflowDesigner.PropertyInspectorView;
                    Singlestep = false;
                    BreakPointhit = false;
                    if (!VisualTracking && Config.local.minimize) GenericTools.Minimize();
                    if (ResumeRuntimeFromHost != null) ResumeRuntimeFromHost.Set();
                    return;
                }
                workflowDesigner.Flush();
                // InitializeStateEnvironment();
                if (global.isConnected)
                {
                    if (!Workflow.hasRight(global.webSocketClient.user, RPA.Workbench.Interfaces.entity.ace_right.invoke))
                    {
                        Log.Error("Access denied, " + global.webSocketClient.user.username + " does not have invoke permission");
                        return;
                    }
                }
                //GenericTools.RunUI(() =>
                //{
                //    if (_activityIdMapping.Count == 0)
                //    {
                //        int failCounter = 0;
                //        while (_activityIdMapping.Count == 0 && failCounter < 3)
                //        {
                //            System.Windows.Forms.Application.DoEvents();
                //            InitializeStateEnvironment(true);
                //            System.Threading.Thread.Sleep(500);
                //            failCounter++;
                //        }
                //    }
                //    if (_activityIdMapping.Count == 0)
                //    {
                //        _ = Save();
                //        // ReloadDesigner();
                //    }
                //    if (_activityIdMapping.Count == 0)
                //    {
                //        int failCounter = 0;
                //        while (_activityIdMapping.Count == 0 && failCounter < 3)
                //        {
                //            System.Windows.Forms.Application.DoEvents();
                //            InitializeStateEnvironment(true);
                //            System.Threading.Thread.Sleep(500);
                //            failCounter++;
                //        }
                //    }

                //});
                //if (_activityIdMapping.Count == 0)
                //{
                //    Log.Error("Failed mapping activites!!!!!");
                //    throw new Exception("Failed mapping activites!!!!!");
                //}
                if (instance == null)
                {
                    var param = new Dictionary<string, object>();
                    BreakpointLocations = workflowDesigner.DebugManagerView.GetBreakpointLocations();
                    if (SlowMotion || VisualTracking || BreakpointLocations.Count > 0 || Singlestep == true)
                    {
                        instance = Workflow.CreateInstance(param, null, null, IdleOrComplete, OnVisualTracking, null, null);
                    }
                    else
                    {
                        instance = Workflow.CreateInstance(param, null, null, IdleOrComplete, null, null, null);
                    }
                }
                ReadOnly = true;
                if (!VisualTracking && Config.local.minimize) GenericTools.Minimize();
            });
            if (instance != null) instance.Run();
        }
    }
}