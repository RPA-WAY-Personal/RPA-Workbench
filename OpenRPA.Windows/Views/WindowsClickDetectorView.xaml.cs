using RPA.Workbench.Interfaces;
using RPA.Workbench.Interfaces.entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPA.Workbench.Windows.Views
{
    /// <summary>
    /// Interaction logic for WindowsClickDetectorView.xaml
    /// </summary>
    public partial class WindowsClickDetectorView : UserControl, INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public WindowsClickDetectorView(IDetectorPlugin plugin)
        {
            InitializeComponent();
            this.plugin = plugin;
            HighlightImage.Source = RPA.Workbench.Interfaces.Extensions.GetImageSourceFromResource("search.png");
            DataContext = this;
        }
        private IDetectorPlugin plugin;
        public Detector Entity
        {
            get
            {
                return plugin.Entity as Detector;
            }
        }
        public string EntityName
        {
            get
            {
                if (Entity == null) return string.Empty;
                return Entity.name;
            }
            set
            {
                Entity.name = value;
                NotifyPropertyChanged("EntityName");
            }
        }
        public string Selector
        {
            get
            {
                if (Entity == null) return null;
                if(!Entity.Properties.ContainsKey("Selector")) return null;
                var _val = Entity.Properties["Selector"];
                if (_val == null) return null;
                return _val.ToString().Replace(Environment.NewLine, "");
            }
            set
            {
                Entity.Properties["Selector"] = value;
            }
        }
        private void Open_Selector_Click(object sender, RoutedEventArgs e)
        {
            string SelectorString = Selector;
            RPA.Workbench.Interfaces.Selector.SelectorWindow selectors;
            if (!string.IsNullOrEmpty(SelectorString))
            {
                var selector = new WindowsSelector(SelectorString);
                selectors = new RPA.Workbench.Interfaces.Selector.SelectorWindow("Windows", selector, null, 10);
            }
            else
            {
                var selector = new WindowsSelector("[{Selector: 'Windows'}]");
                selectors = new RPA.Workbench.Interfaces.Selector.SelectorWindow("Windows", selector, null, 10);
            }
            // selectors.Owner = GenericTools.MainWindow;  -- Locks up and never returns ?
            if (selectors.ShowDialog() == true)
            {
                Selector = selectors.vm.json;
                NotifyPropertyChanged("EntityName");
                NotifyPropertyChanged("Selector");
            }
        }
        private void Highlight_Click(object sender, RoutedEventArgs e)
        {
            HighlightImage.Source = RPA.Workbench.Interfaces.Extensions.GetImageSourceFromResource("search.png");
            string SelectorString = Selector;
            var selector = new WindowsSelector(SelectorString);
            var elements = WindowsSelector.GetElementsWithuiSelector(selector, null, 10, null);
            if (elements.Count() > 0)
            {
                HighlightImage.Source = RPA.Workbench.Interfaces.Extensions.GetImageSourceFromResource("check.png");
            } else
            {
                HighlightImage.Source = RPA.Workbench.Interfaces.Extensions.GetImageSourceFromResource(".x.png");
            }
            foreach (var ele in elements) ele.Highlight(false, System.Drawing.Color.Red, TimeSpan.FromSeconds(1));
        }
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            RPA.Workbench.Interfaces.GenericTools.Minimize();
            StartRecordPlugins();
        }
        private void StartRecordPlugins()
        {
            var p = RPA.Workbench.Interfaces.Plugins.recordPlugins.Where(x => x.Name == "Windows").First();
            p.OnUserAction += OnUserAction;
            p.Start();
        }
        private void StopRecordPlugins()
        {
            var p = RPA.Workbench.Interfaces.Plugins.recordPlugins.Where(x => x.Name == "Windows").First();
            p.OnUserAction -= OnUserAction;
            p.Stop();
        }
        public void OnUserAction(RPA.Workbench.Interfaces.IRecordPlugin sender, RPA.Workbench.Interfaces.IRecordEvent e)
        {
            StopRecordPlugins();
            AutomationHelper.syncContext.Post(o =>
            {
                RPA.Workbench.Interfaces.GenericTools.Restore();
                foreach (var p in RPA.Workbench.Interfaces.Plugins.recordPlugins)
                {
                    if (p.Name != sender.Name)
                    {
                        if (p.ParseUserAction(ref e)) continue;
                    }
                }
                Selector = e.Selector.ToString();
                EntityName = e.UIElement.ToString();
                NotifyPropertyChanged("EntityName");
                NotifyPropertyChanged("Selector");
            }, null);
        }
    }
}
