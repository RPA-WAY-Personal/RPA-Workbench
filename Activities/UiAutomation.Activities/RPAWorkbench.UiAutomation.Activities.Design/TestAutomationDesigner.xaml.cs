﻿using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPAWorkbench.UiAutomation.Activities
{
    // Interaction logic for ActivityDesigner1.xaml
    public partial class TestAutomationDesigner
    {
        public TestAutomationDesigner()
        {
            InitializeComponent();
        }
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(TestAutomation), new DesignerAttribute(typeof(TestAutomationDesigner)));
            builder.AddCustomAttributes(typeof(TestAutomation), new DescriptionAttribute("My sample activity"));
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Still testing");
        }
    }
}
