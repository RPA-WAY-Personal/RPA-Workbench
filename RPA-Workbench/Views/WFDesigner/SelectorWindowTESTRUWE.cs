using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RPA_Workbench.Views.WFDesigner;
using RPA_Workbench;
namespace RPA_Workbench.Views.WFDesigner
{
    class SelectorWindowTESTRUWE
    {
        private void Open_Selector(object sender, RoutedEventArgs e)
        {
            //string SelectorString = ModelItem.GetValue<string>("Selector");
            //int maxresult = 1;

            //if (string.IsNullOrEmpty(SelectorString)) SelectorString = "[{Selector: 'Windows'}]";
            //var selector = new RPA.Workbench.Interfaces.Selector.Selector(SelectorString);
            //var pluginname = selector.First().Selector;
            //var selectors = new RPA.Workbench.Interfaces.Selector.SelectorWindow(pluginname, selector, maxresult);
            //selectors.Owner = RPA.Workbench.Interfaces.GenericTools.MainWindow;
            //if (selectors.ShowDialog() == true)
            //{
            //    ModelItem.Properties["Selector"].SetValue(new InArgument<string>() { Expression = new Literal<string>(selectors.vm.json) });
            //}
        }
    }
}
