using ActiproSoftware.Windows.DocumentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPA_Workbench.Views
{
    /// <summary>
    /// Interaction logic for RecentDocMeny.xaml
    /// </summary>
    public partial class RecentDocMeny : IDocumentReference
    {
        public RecentDocMeny()
        {
            InitializeComponent();
        }

        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ImageSource ImageSourceLarge { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ImageSource ImageSourceSmall { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsPinnedRecentDocument { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime LastOpenedDateTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Uri Location { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
