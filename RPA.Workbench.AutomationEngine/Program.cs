using RPA.Workbench.AutomationEngine.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Workbench.AutomationEngine
{
    public class Program
    {
        static IWorkflowRunner runner;
        [STAThread]
        public static void Main(string[] args)
        {

            string FileName = "";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in args)
            {
                stringBuilder.Append(item);
            }
            FileName = stringBuilder.ToString();
            Console.WriteLine($"Running: {FileName}");
            runner = new RPA.Workbench.AutomationEngine.Execution.WorkflowRunner(FileName);
            runner.Run();
            Console.ReadLine();
        }
    }
}
