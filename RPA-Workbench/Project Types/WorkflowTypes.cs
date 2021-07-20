using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA_Workbench.Project_Types
{
    public static class WorkflowTypes
    {
        public static string CleanProjectView(string filename)
        {
            var sb = new System.Text.StringBuilder(1310);
            sb.AppendLine($@"<p:Activity x:Class=""{filename}""  ");
            sb.AppendLine(@"          xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" ");
            sb.AppendLine(@"          xmlns:p=""http://schemas.microsoft.com/netfx/2009/xaml/activities""");
            sb.AppendLine(@"          xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""> ");
            sb.AppendLine(@"    <p:TextExpression.NamespacesForImplementation>");
            sb.AppendLine(@"        <sco:Collection x:TypeArguments=""x:String"">");
            sb.AppendLine(@"            <x:String>System</x:String>");
            sb.AppendLine(@"            <x:String>System.Collections.Generic</x:String>");
            sb.AppendLine(@"            <x:String>System.Data</x:String>");
            sb.AppendLine(@"            <x:String>System.Linq</x:String>");
            sb.AppendLine(@"            <x:String>System.Text</x:String>");
            sb.AppendLine(@"        </sco:Collection>");
            sb.AppendLine(@"    </p:TextExpression.NamespacesForImplementation>");
            sb.AppendLine(@"    <p:TextExpression.ReferencesForImplementation>");
            sb.AppendLine(@"        <sco:Collection x:TypeArguments=""p:AssemblyReference"">");
            sb.AppendLine(@"            <p:AssemblyReference>mscorlib</p:AssemblyReference>");
            sb.AppendLine(@"            <p:AssemblyReference>System</p:AssemblyReference>");
            sb.AppendLine(@"            <p:AssemblyReference>System.Core</p:AssemblyReference>");
            sb.AppendLine(@"            <p:AssemblyReference>System.Data</p:AssemblyReference>");
            sb.AppendLine(@"            <p:AssemblyReference>System.ServiceModel</p:AssemblyReference>");
            sb.AppendLine(@"            <p:AssemblyReference>System.Xml</p:AssemblyReference>");
            sb.AppendLine(@"        </sco:Collection>");
            sb.AppendLine(@"    </p:TextExpression.ReferencesForImplementation>");
            sb.AppendLine(@"</p:Activity>");

            return sb.ToString();
        }
    }
}
