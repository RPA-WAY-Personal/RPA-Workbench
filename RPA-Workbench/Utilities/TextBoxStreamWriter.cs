//-----------------------------------------------------------------------
// <copyright file="TextBoxStreamWriter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
//-----------------------------------------------------------------------
namespace RPA_Workbench.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ActiproSoftware.Windows.Controls.Grids;
    using ActiproSoftware.Windows.Controls.Ribbon.Controls;
    using ActiproSoftware.Windows.Controls.Ribbon.Controls.Primitives;
    using RibbonControls = ActiproSoftware.Windows.Controls.Ribbon.Controls;
    public class TextBoxStreamWriter : TextWriter
    {
        private TreeListBox output = null;
        private TraceSource traceSource;
        private TraceSource allTraceSource;
        private string workflowName;
        ContextMenu contextMenu = new ContextMenu();

        public TextBoxStreamWriter(TreeListBox output, string workflowName)
        {
            this.output = output;
            this.workflowName = workflowName;
            this.traceSource = new TraceSource(workflowName + "Output", SourceLevels.Verbose);
            this.allTraceSource = new TraceSource("AllOutput", SourceLevels.Verbose);
            ContextMenuSetup();

        }

        void ContextMenuSetup()
        {
            output.ContextMenu = contextMenu;
            RibbonControls.Button ClearListBtn = new RibbonControls.Button();
            BitmapImage ClearListImage = new BitmapImage(new Uri("/RPA-Workbench;component/1. Resources/MainWindow Images/ToolWindow Images/ClearListImage.png", UriKind.Relative));
            ClearListBtn.ImageSourceSmall = ClearListImage;
            ClearListBtn.Label = "Clear list";
            ClearListBtn.Click += ClearList_Click;



            contextMenu.Items.Add(ClearListBtn);

        }

        private void ClearList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            output.Items.Clear();
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        public override void WriteLine(string value)
        {
            if (value != null)
            {
                base.WriteLine(value);
                this.traceSource.TraceData(TraceEventType.Verbose, 0, value.ToString());
                this.allTraceSource.TraceData(TraceEventType.Verbose, 0, this.workflowName, value.ToString());

               
                this.output.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.output.Items.Add(value);
                    // this.output.AppendText(value.ToString());
                }));
               
            }
        }

        public override void Write(char value)
        {
            base.Write(value);
            this.output.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //this.output.Items.Add(value.ToString());
                   // this.output.AppendText(value.ToString());
                }));
        }
    }
}