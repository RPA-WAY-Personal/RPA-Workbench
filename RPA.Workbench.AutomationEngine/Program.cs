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
        public static async Task Main(string[] args)
        {

            string FileName;
         
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in args)
            {
                stringBuilder.Append(item);
            }
            
            if (stringBuilder.ToString().Contains("Run"))
            {
                FileName = stringBuilder.ToString().Remove(0,3);
                //Console.WriteLine($"File Name: {FileName}");
                Console.WriteLine($"Starting Flow ");
                Console.WriteLine($"Flow running: {FileName}");
                runner = new RPA.Workbench.AutomationEngine.Execution.WorkflowRunner(FileName);
                runner.Run();
            }
            //do
            //{

            //} while (Console.ReadLine().Contains("Exit"));



            //FileName = Console.ReadLine();
            //Console.WriteLine("FileName: " + FileName);
            //do
            //{
            //    if (FileName.Contains("Run \"" + FileName + "\""))
            //    {
            //        FileName = FileName.Remove(FileName.Length - 4);
            //        Console.WriteLine($"Running: {FileName}");
            //        runner = new RPA.Workbench.AutomationEngine.Execution.WorkflowRunner(FileName);
            //        runner.Run();
            //    }
            //} while (Console.ReadLine().Contains("Exit"));




            Console.ReadLine();
        }
    }
}
