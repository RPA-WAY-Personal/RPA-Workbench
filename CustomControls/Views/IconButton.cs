using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace CustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfIconButtonControlLibrary"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfIconButtonControlLibrary;assembly=WpfIconButtonControlLibrary"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:IconButton/>
    ///
    /// </summary>
    public partial class IconButton : Button
    {
            public IconButton()
            {
                InitializeComponent();
            }

            public string Title
            {
                get { return (string)GetValue(TitleProperty); }
                set { SetValue(TitleProperty, value); }
            }

            public Brush TitleColor
            {
                get { return (Brush)GetValue(TitlePropertyColor); }
                set { SetValue(TitlePropertyColor, value); }
            }

            public static readonly DependencyProperty TitleProperty =
                DependencyProperty.Register("Title", typeof(string), typeof(IconButton), new FrameworkPropertyMetadata("Title", FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty TitlePropertyColor =
                DependencyProperty.Register("TitleColor", typeof(Brush), typeof(IconButton), new FrameworkPropertyMetadata(null));

            public string SubTitle
            {
                get { return (string)GetValue(SubTitleProperty); }
                set { SetValue(SubTitleProperty, value); }
            }

            public Brush SubtitleColor
            {
                get { return (Brush)GetValue(SubTitlePropertyPropertyColor); }
                set { SetValue(SubTitlePropertyPropertyColor, value); }
            }


        public static readonly DependencyProperty SubTitleProperty =
                DependencyProperty.Register("SubTitle", typeof(string), typeof(IconButton), new FrameworkPropertyMetadata("SubTitle", FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SubTitlePropertyPropertyColor =
              DependencyProperty.Register("SubTitleColor", typeof(Brush), typeof(IconButton), new FrameworkPropertyMetadata(null));


        public Brush BackgroundGridColor
        {
            get { return (Brush)GetValue(BackgroundGridPropertyColor); }
            set { SetValue(BackgroundGridPropertyColor, value); }
        }

        public static readonly DependencyProperty BackgroundGridPropertyColor =
            DependencyProperty.Register("BackgroundGridColor", typeof(Brush), typeof(IconButton), new FrameworkPropertyMetadata(null));
        public FrameworkElement Image
            {
                get { return (FrameworkElement)GetValue(ImageProperty); }
                set { SetValue(ImageProperty, value); }
            }

            public static readonly DependencyProperty ImageProperty =
                DependencyProperty.Register("Image", typeof(FrameworkElement), typeof(IconButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    }
}

