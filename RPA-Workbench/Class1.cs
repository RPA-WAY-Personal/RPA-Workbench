//using Newtonsoft.Json.Linq;
//using RPA.Workbench.Input;
//using RPA.Workbench.Interfaces;
//using RPA.Workbench.Interfaces.entity;
//using System;
//using System.Activities;
//using System.Activities.Presentation;
//using System.Activities.Presentation.Model;
//using System.Activities.Statements;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace RPA_Workbench
//{
//    class Class1
//    {
//        ViewModels.WorkflowStudioIntegration.WorkflowViewModel WorkflowViewModelLocal;
//        ActiproSoftware.Windows.Controls.Docking.DockSite dockSiteLocal;
//        public WorkflowDesigner WorkflowDesigner { get; private set; }
//        public Class1(ViewModels.WorkflowStudioIntegration.WorkflowViewModel WorkflowViewModel = null, ActiproSoftware.Windows.Controls.Docking.DockSite dockSite)
//        {
//            WorkflowViewModelLocal = WorkflowViewModel;
//            dockSiteLocal = dockSite;
//        }
//        bool isRecording;
//        RPA.Workbench.Interfaces.Overlay.OverlayWindow _overlayWindow = null;

//        public object SelectedContent
//        {
//            get
//            {
//                if (dockSiteLocal == null) return null;
//                var b = dockSiteLocal.ActiveWindow;
//                return b;
//            }
//        }
//        private void StartRecordPlugins(bool all)
//        {
//            Log.FunctionIndent("MainWindow", "StartRecordPlugins");
//            try
//            {
//                isRecording = true;
//                var p = Plugins.recordPlugins.Where(x => x.Name == "Windows").First();
//                p.OnUserAction += OnUserAction;
//                if (Config.local.record_overlay) p.OnMouseMove += OnMouseMove;
//                p.Start();
//                if (_overlayWindow == null && Config.local.record_overlay)
//                {
//                    _overlayWindow = new RPA.Workbench.Interfaces.Overlay.OverlayWindow(true)
//                    {
//                        BackColor = System.Drawing.Color.PaleGreen,
//                        Visible = true,
//                        TopMost = true
//                    };
//                }

//                p = Plugins.recordPlugins.Where(x => x.Name == "SAP").FirstOrDefault();
//                if (p != null && (all == true || all == false))
//                {
//                    p.OnUserAction += OnUserAction;
//                    if (Config.local.record_overlay) p.OnMouseMove += OnMouseMove;
//                    p.Start();
//                }

//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex.ToString());
//            }
//            Log.FunctionOutdent("MainWindow", "StartRecordPlugins");
//        }
//        private void StopRecordPlugins(bool all)
//        {
//            Log.FunctionIndent("MainWindow", "StopRecordPlugins");
//            try
//            {
//                isRecording = false;
//                var p = Plugins.recordPlugins.Where(x => x.Name == "Windows").First();
//                p.OnUserAction -= OnUserAction;
//                if (Config.local.record_overlay) p.OnMouseMove -= OnMouseMove;
//                p.Stop();

//                p = Plugins.recordPlugins.Where(x => x.Name == "SAP").FirstOrDefault();
//                if (p != null && (all == true || all == false))
//                {
//                    p.OnUserAction -= OnUserAction;
//                    p.Stop();
//                }

//                if (_overlayWindow != null)
//                {
//                    GenericTools.RunUI(_overlayWindow, () =>
//                    {
//                        try
//                        {
//                            _overlayWindow.Visible = true;
//                            _overlayWindow.Dispose();
//                        }
//                        catch (Exception ex)
//                        {
//                            Log.Error(ex.ToString());
//                        }
//                    });
//                }
//                _overlayWindow = null;
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex.ToString());
//            }
//            Log.FunctionOutdent("MainWindow", "StopRecordPlugins");
//        }
//        public void OnMouseMove(IRecordPlugin sender, IRecordEvent e)
//        {
//            if (!Config.local.record_overlay) return;
//            foreach (var p in Plugins.recordPlugins)
//            {
//                if (p.Name != sender.Name)
//                {
//                    if (p.ParseMouseMoveAction(ref e)) break;
//                }
//            }

//            // e.Element.Highlight(false, System.Drawing.Color.PaleGreen, TimeSpan.FromSeconds(1));
//            if (e.Element != null && _overlayWindow != null)
//            {

//                GenericTools.RunUI(_overlayWindow, () =>
//                {
//                    try
//                    {
//                        if (_overlayWindow != null)
//                        {
//                            _overlayWindow.Visible = true;
//                            _overlayWindow.Bounds = e.Element.Rectangle;
//                        }
//                    }
//                    catch (Exception)
//                    {
//                    }
//                });
//            }
//            else if (_overlayWindow != null)
//            {
//                GenericTools.RunUI(_overlayWindow, () =>
//                {
//                    try
//                    {
//                        _overlayWindow.Visible = false;
//                    }
//                    catch (Exception)
//                    {
//                    }
//                });
//            }
//        }
//        public void OnUserAction(IRecordPlugin sender, IRecordEvent e)
//        {
//            Log.FunctionIndent("MainWindow", "OnUserAction");
//            if (sender.Name == "Windows") StopRecordPlugins(false);
//            AutomationHelper.syncContext.Post(o =>
//            {
//                IPlugin plugin = sender;
//                try
//                {
//                    if (sender.Name == "Windows")
//                    {
//                        // TODO: Add priotrity, we could create an ordered list in config ?
//                        foreach (var p in Plugins.recordPlugins)
//                        {
//                            if (p.Name != sender.Name)
//                            {
//                                try
//                                {
//                                    if (p.ParseUserAction(ref e))
//                                    {
//                                        plugin = p;
//                                        break;
//                                    }
//                                }
//                                catch (Exception ex)
//                                {
//                                    Log.Error(ex.ToString());
//                                }
//                            }
//                        }
//                    }
//                    if (e.a == null)
//                    {
//                        if (sender.Name == "Windows") StartRecordPlugins(false);
//                        if (e.ClickHandled == false)
//                        {
//                            NativeMethods.SetCursorPos(e.X, e.Y);
//                            InputDriver.Click(e.Button);
//                        }
//                        Log.Function("MainWindow", "OnUserAction", "Action is null");
//                        return;
//                    }
//                    if (SelectedContent is WorkflowDesigner view)
//                    {

//                        var VirtualClick = Config.local.use_virtual_click;
//                        if (!e.SupportVirtualClick) VirtualClick = false;
//                        e.a.AddActivity(new RPA.Workbench.Windows.ClickElement
//                        {
//                            Element = new System.Activities.InArgument<IElement>()
//                            {
//                                Expression = new Microsoft.VisualBasic.Activities.VisualBasicValue<IElement>("item")
//                            },
//                            OffsetX = e.OffsetX,
//                            OffsetY = e.OffsetY,
//                            Button = (int)e.Button,
//                            VirtualClick = VirtualClick,
//                            AnimateMouse = Config.local.use_animate_mouse
//                        }, "item");
//                        if (e.SupportSelect)
//                        {
//                            var win = new Views.InsertSelect(e.Element)
//                            {
//                                Topmost = true
//                            };
//                            isRecording = false;
//                            InputDriver.Instance.CallNext = true;
//                            win.Owner = this;
//                            if (win.ShowDialog() == true)
//                            {
//                                e.ClickHandled = true;
//                                if (!string.IsNullOrEmpty(win.SelectedItem.Value))
//                                {
//                                    e.a.AddInput(win.SelectedItem.Value, e.Element);
//                                }
//                                else
//                                {
//                                    e.a.AddInput(win.SelectedItem.Name, e.Element);
//                                }

//                            }
//                            else
//                            {
//                                e.SupportSelect = false;
//                            }
//                            InputDriver.Instance.CallNext = false;
//                            isRecording = true;
//                        }
//                        else if (e.SupportInput)
//                        {
//                            //var win = new Views.InsertText
//                            //{
//                            //    Topmost = true
//                            //};
//                            //isRecording = false;
//                            //win.Owner = this;
//                            //if (win.ShowDialog() == true)
//                            //{
//                            //    e.a.AddInput(win.Text, e.Element);
//                            //}
//                            //else { e.SupportInput = false; }
//                            //isRecording = true;
//                        }

//                        //view.ReadOnly = false;
//                        //view.Lastinserted = e.a.Activity;
//                        //view.Lastinsertedmodel = view.AddRecordingActivity(e.a.Activity, plugin);
//                        //view.ReadOnly = true;
//                        if (e.ClickHandled == false && e.SupportInput == false)
//                        {
//                            NativeMethods.SetCursorPos(e.X, e.Y);
//                            InputDriver.Click(e.Button);
//                        }
//                        System.Threading.Thread.Sleep(500);
//                    }
//                    if (sender.Name == "Windows") StartRecordPlugins(false);
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show(ex.Message);
//                    Log.Error(ex.ToString());
//                }
//            }, null);
//            Log.FunctionOutdent("MainWindow", "OnUserAction");
//        }
//        internal void OnRecord(object _item)
//        {
//            //if (!(SelectedContent is Views.WFDesigner)) return;
//            //Log.FunctionIndent("MainWindow", "OnRecord");
//            //try
//            //{
//            //    WorkflowViewModelLocal
//            //    var designer = WorkflowViewModelLocal.WorkflowDesigner;
//            //    designer.ReadOnly = true;
//            //    designer.Lastinserted = null;
//            //    designer.Lastinsertedmodel = null;
//            //    StopDetectorPlugins();
//            //    InputDriver.Instance.OnKeyDown += OnKeyDown;
//            //    InputDriver.Instance.OnKeyUp += OnKeyUp;
//            //    StartRecordPlugins(true);
//            //    InputDriver.Instance.CallNext = false;
//            //    if (this.Minimize) GenericTools.Minimize();
//            //}
//            //catch (Exception ex)
//            //{
//            //    Log.Error(ex.ToString());
//            //}
//            //Log.FunctionOutdent("MainWindow", "OnRecord");
//        }
//        public void OnDetector(IDetectorPlugin plugin, IDetectorEvent detector, EventArgs e)
//        {
//            Log.FunctionIndent("MainWindow", "OnDetector");
//            try
//            {
//                Log.Information("Detector " + plugin.Entity.name + " was triggered, with id " + plugin.Entity._id);
//                foreach (var wi in WorkflowInstance.Instances.ToList())
//                {
//                    if (wi.isCompleted) continue;
//                    if (wi.Bookmarks != null)
//                    {
//                        foreach (var b in wi.Bookmarks)
//                        {
//                            var _id = (plugin.Entity as Detector)._id;
//                            Log.Debug(b.Key + " -> " + "detector_" + _id);
//                            if (b.Key == "detector_" + _id)
//                            {
//                                wi.ResumeBookmark(b.Key, detector);
//                            }
//                        }
//                    }
//                }
//                if (!global.isConnected)
//                {
//                    Log.FunctionOutdent("MainWindow", "OnDetector", "isConnected is false");
//                    return;
//                }
//                RPA.Workbench.Interfaces.mq.RobotCommand command = new RPA.Workbench.Interfaces.mq.RobotCommand();
//                // detector.user = global.webSocketClient.user;
//                var data = JObject.FromObject(detector);
//                var Entity = (plugin.Entity as Detector);
//                command.command = "detector";
//                command.detectorid = Entity._id;
//                if (string.IsNullOrEmpty(Entity._id))
//                {
//                    Log.FunctionOutdent("MainWindow", "OnDetector", "Entity._id is null");
//                    return;
//                }
//                command.data = data;
//                Task.Run(async () =>
//                {
//                    try
//                    {
//                        await global.webSocketClient.QueueMessage(Entity._id, command, null, null, 0);
//                    }
//                    catch (Exception ex)
//                    {
//                        Log.Debug(ex.Message);
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex, "");
//                MessageBox.Show(ex.Message);
//            }
//            Log.FunctionOutdent("MainWindow", "OnDetector");
//        }

//        public ModelItem AddRecordingActivity(Activity a, IPlugin plugin)
//        {
//            if (plugin != null)
//            {
//                WFHelper.AddVBNamespaceSettings(WorkflowDesigner, new Type[] { plugin.GetType() });
//                Type t = plugin.GetType();
//                WFHelper.DynamicAssemblyMonitor(WorkflowDesigner, t.Assembly.GetName().Name, t.Assembly, true);
//            }
//            //DynamicAssemblyMonitor(t.Assembly.GetName().Name, t.Assembly, true);
//            if (Config.local.recording_add_to_designer)
//            {
//                return AddActivity(a);
//            }
//            if (recording == null) BeginRecording();
//            recording.Add(a);
//            if (!recordingplugins.Contains(plugin)) recordingplugins.Add(plugin);
//            return null;
//        }
//        List<Activity> recording = null;
//        List<IPlugin> recordingplugins = null;
//        public void BeginRecording()
//        {
//            recording = new List<Activity>();
//            recordingplugins = new List<IPlugin>();
//        }
//        public ModelItem AddActivity(Activity a)
//        {
//            ModelItem newItem = null;
//            ModelService modelService = WorkflowDesigner.Context.Services.GetService<ModelService>();
//            using (ModelEditingScope editingScope = modelService.Root.BeginEdit("Implementation"))
//            {
//                var lastSequence = GetSequence(SelectedActivity);
//                if (lastSequence == null && SelectedActivity != null) lastSequence = GetActivitiesScope(SelectedActivity.Parent);
//                if (lastSequence != null)
//                {
//                    ModelItemCollection Activities = null;
//                    if (lastSequence.Properties["Activities"] != null)
//                    {
//                        Activities = lastSequence.Properties["Activities"].Collection;
//                    }
//                    else
//                    {
//                        Activities = lastSequence.Properties["Nodes"].Collection;
//                    }

//                    var insertAt = Activities.Count;
//                    for (var i = 0; i < Activities.Count; i++)
//                    {
//                        if (Activities[i].Equals(SelectedActivity))
//                        {
//                            insertAt = (i + 1);
//                        }
//                    }
//                    if (lastSequence.Properties["Activities"] != null)
//                    {
//                        if (string.IsNullOrEmpty(a.DisplayName)) a.DisplayName = "Activity";
//                        newItem = Activities.Insert(insertAt, a);
//                    }
//                    else
//                    {
//                        FlowStep step = new FlowStep
//                        {
//                            Action = a
//                        };
//                        newItem = Activities.Insert(insertAt, step);
//                    }
//                    //Selection.Select(wfDesigner.Context, selectedActivity);
//                    //ModelItemExtensions.Focus(selectedActivity);
//                }
//                editingScope.Complete();
//                //WorkflowInspectionServices.CacheMetadata(a);
//            }
//            if (newItem != null)
//            {
//                SelectedActivity = newItem;
//                newItem.Focus(20);
//                Selection.SelectOnly(WorkflowDesigner.Context, newItem);
//            }
//            return newItem;
//        }

//        private ModelItem GetSequence(ModelItem from)
//        {
//            ModelItem parent = from;
//            while (parent != null && !parent.ItemType.Equals(typeof(Sequence)))
//            {
//                parent = parent.Parent;
//            }
//            return parent;
//        }
//        private ModelItem GetVariableScope(ModelItem from)
//        {
//            ModelItem parent = from;

//            while (parent != null && parent.Properties["Variables"] == null)
//            {
//                parent = parent.Parent;
//            }
//            return parent;
//        }
//        private ModelItem GetActivitiesScope(ModelItem from)
//        {
//            ModelItem parent = from;

//            while (parent != null && parent.Properties["Activities"] == null && parent.Properties["Handler"] == null && parent.Properties["Nodes"] == null)
//            {
//                parent = parent.Parent;
//            }
//            return parent;
//        }

//    }
//}
