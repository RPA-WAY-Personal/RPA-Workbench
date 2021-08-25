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
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
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

        private void WorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {
            this.running = false;
            Console.WriteLine("Workflow Complete");
            //StatusViewModel.SetStatusText(string.Format(Resources.CompletedStatus, e.CompletionState.ToString()), this.workflowName);
        }

        private void WorkflowAborted(WorkflowApplicationAbortedEventArgs e)
        {
            this.running = false;
            Console.WriteLine("Workflow Aborted");
            //StatusViewModel.SetStatusText(Resources.AbortedStatus, this.workflowName);
        }

        private UnhandledExceptionAction WorkflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(ExceptionHelper.FormatStackTrace(e.UnhandledException));
            return UnhandledExceptionAction.Terminate;
        }
    }
}
