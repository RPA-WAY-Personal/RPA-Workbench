//-----------------------------------------------------------------------
// <copyright file="WorkflowRunner.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
//-----------------------------------------------------------------------
namespace RPA.Workbench.AutomationEngine.Execution
{
    using System;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.XamlIntegration;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Drawing;
    using RPA.Workbench.AutomationEngine.Properties;
    using RPA.Workbench.AutomationEngine.Utilities;

    public class WorkflowRunner : IWorkflowRunner
    {
        private TextWriter output;
        private WorkflowApplication workflowApplication;
        private bool running;
        private WorkflowDesigner workflowDesigner;

        private string workflowName;
        string WorkFlowFile;

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);
        private int hWnd;
        /*
         Hide Window = 0
         Minimize Window = 1
         Maxmize Window = 2
         Restore Window = 9

        ex:
        private const int SW_HIDE = 0;
         */

        enum States
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11
        }
        public WorkflowRunner(TextWriter output, string workflowName, WorkflowDesigner workflowDesigner)
        {
            this.output = output;
            this.workflowName = workflowName;
            this.workflowDesigner = workflowDesigner;
        }

        public WorkflowRunner(string WorkFlowFile)
        {
            this.WorkFlowFile = WorkFlowFile;
        }

        public bool IsRunning
        {
            get
            {
                return this.running;
            }
        }

        public void Abort()
        {
            if (this.running && this.workflowApplication != null)
            {
              //  StatusViewModel.SetStatusText(Resources.AbortingStatus, this.workflowName);
                this.workflowApplication.Abort();
            }
        }
        [STAThread]
        public void Run()
        {
            //
            //MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(this.workflowDesigner.Text));
         
            try
            {
                //string xaml = File.ReadAllText(@"C:\Users\ruwek\Documents\RPA-Workbench\Blank Processesdrfsfsfsf\Main.xaml");
                //workflowDesigner = new WorkflowDesigner();
                //this.workflowDesigner.Load(@"C:\Users\ruwek\Documents\RPA-Workbench\Blank Processesdrfsfsfsf\Main.xaml");
                //this.workflowDesigner.Flush();

                //MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(xaml));
                //Console.WriteLine(xaml);
                //Activity activityToRun = ActivityXamlServices.Load(@"C:\Users\ruwek\Documents\RPA-Workbench\Blank Processesdrfsfsfsf\Main.xaml") as Activity;
                //Console.WriteLine(activityToRun.DisplayName);
                //this.workflowApplication = new WorkflowApplication(activityToRun);

                //this.workflowApplication.Extensions.Add(this.output);
                //this.workflowApplication.Completed = this.WorkflowCompleted;
                //this.workflowApplication.Aborted = this.WorkflowAborted;
                //this.workflowApplication.OnUnhandledException = this.WorkflowUnhandledException;

                // StatusViewModel.SetStatusText(Resources.RunningStatus, this.workflowName);
                string filePath = WorkFlowFile;
                string tempString = "";
                StringBuilder xamlWFString = new StringBuilder();
                StreamReader xamlStreamReader =
                    new StreamReader(filePath);
                while (tempString != null)
                {
                    tempString = xamlStreamReader.ReadLine();
                    if (tempString != null)
                    {
                        xamlWFString.Append(tempString);
                    }
                }
                Activity wfInstance = ActivityXamlServices.Load(
                    new StringReader(xamlWFString.ToString()));

                this.workflowApplication = new WorkflowApplication(wfInstance);

                //this.workflowApplication.Extensions.Add(this.output);
                this.workflowApplication.Completed = this.WorkflowCompleted;
                this.workflowApplication.Aborted = this.WorkflowAborted;
                this.workflowApplication.OnUnhandledException = this.WorkflowUnhandledException;
                //WorkflowInvoker.Invoke(wfInstance);


                try
                {
                    this.running = true;
                    this.workflowApplication.Run();
                }
                catch (Exception e)
                {
                    this.output.WriteLine(ExceptionHelper.FormatStackTrace(e));
                    //StatusViewModel.SetStatusText(Resources.ExceptionStatus, this.workflowName);
                    this.running = false;
                }
                Console.ReadKey();
            }
            catch (Exception)
            {

                throw;
            }
           
           // MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(this.WorkFlowFile));

           // DynamicActivity activityToRun = ActivityXamlServices.Load(ms) as DynamicActivity;

           // this.workflowApplication = new WorkflowApplication(activityToRun);

           // this.workflowApplication.Extensions.Add(this.output);
           // this.workflowApplication.Completed = this.WorkflowCompleted;
           // this.workflowApplication.Aborted = this.WorkflowAborted;
           // this.workflowApplication.OnUnhandledException = this.WorkflowUnhandledException;
           //// StatusViewModel.SetStatusText(Resources.RunningStatus, this.workflowName);

           // try
           // {
           //     this.running = true;
           //     this.workflowApplication.Run();
           // }
           // catch (Exception e)
           // {
           //     this.output.WriteLine(ExceptionHelper.FormatStackTrace(e));
           //     //StatusViewModel.SetStatusText(Resources.ExceptionStatus, this.workflowName);
           //     this.running = false;
           // }
        }
        bool Completed = false;
        private void WorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {
            this.running = false;
            Completed = true;
            Console.WriteLine("Workflow Complete");

            Process[] p = Process.GetProcessesByName("RPA-Workbench");
            hWnd = (int)p[0].MainWindowHandle; // 

            //If RPA-Workbench window is minimized, then restore it when workflow is done
            var placement = GetPlacement(p[0].MainWindowHandle);
            if (placement.showCmd.ToString() == "Minimized")
            {
                ShowWindow(hWnd, (int)States.SW_RESTORE);
            }

            System.Threading.Thread.Sleep(1000);
            Process[] automationEngineProcess = Process.GetProcessesByName("AutomationEngine");
            automationEngineProcess[0].Kill();


            //StatusViewModel.SetStatusText(string.Format(Resources.CompletedStatus, e.CompletionState.ToString()), this.workflowName);
        }

        #region RPA-WORKBENCH Windows states
        private static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(
            IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }
        #endregion

        private void WorkflowAborted(WorkflowApplicationAbortedEventArgs e)
        {
            if (Completed == false)
            {
                this.running = false;
                Console.WriteLine("Workflow Aborted");
                foreach (var process in Process.GetProcessesByName("AutomationEngine"))
                {
                    process.Kill();
                }
            }
            
            //StatusViewModel.SetStatusText(Resources.AbortedStatus, this.workflowName);
        }

        private UnhandledExceptionAction WorkflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(ExceptionHelper.FormatStackTrace(e.UnhandledException));
            return UnhandledExceptionAction.Terminate;
        }
    }
}
