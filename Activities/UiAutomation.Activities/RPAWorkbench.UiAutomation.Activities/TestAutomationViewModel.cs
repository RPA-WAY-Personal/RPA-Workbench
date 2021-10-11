using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RPAWorkbench.UiAutomation.Activities.Properties;
using System.Activities.Presentation.Metadata;

namespace RPAWorkbench.UiAutomation.Activities
{
 
    [ToolboxBitmap(typeof(TestAutomation), "icons8-open-file-folder-32.png")]
    // [ToolboxBitmap(typeof(TestAutomationDesigner), "RPAWorkbench.UiAutomation.Activities.Resources.icons8-open-file-folder-32.png")]

    [DisplayName(nameof(Resources.TestAutomation_DisplayName))]
    public class TestAutomation : CodeActivity
    {
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> FileName { get; set; }


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (FileName == null) metadata.AddValidationError("Please Provide a Filename");
            base.CacheMetadata(metadata);
        }

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
       
        public TestAutomation()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                string filename = context.GetValue(this.FileName);
                //System.Diagnostics.Process.Start(filename);
                Process p = new Process();
                p.StartInfo.FileName = filename;
                p.Start();
                if (p != null)
                {
                    IntPtr h = p.MainWindowHandle;
                    SetForegroundWindow(h);
                }

                Console.WriteLine("File: " + filename + " opened");
            }
            catch (Exception e)
            {
                throw;
            }

        }
    }
}
