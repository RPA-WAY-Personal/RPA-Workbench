using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;

namespace RPA_Workbench.Utilities.TreeNodeClasses
{

        #region HeaderToImageConverter

        [ValueConversion(typeof(string), typeof(bool))]
        public class HeaderToImageConverter : IValueConverter
        {
            public static HeaderToImageConverter Instance = new HeaderToImageConverter();
   
        ExtractIcon ExtractIcon = new ExtractIcon();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage source = null;

            if ((value as string).Contains(@".xaml"))
            {
                Uri uri = new Uri("pack://application:,,,/RPA-Workbench-Revision2;component//1. Resources/ProjectWindow Images/Xaml File - 32.png");
                 source = new BitmapImage(uri);
                source.UriSource = uri;
                source.DecodePixelHeight = 10;
                source.DecodePixelWidth = 10;

                return source;
            }
            //if (Properties.Settings.Default.ThemeType == 0)
            //{
                if ((value as string).Contains(@"\"))
                {
                    // Uri uri = new Uri("pack://application:,,,/RPA Workbench;component//Resources/FolderIcon_ExcelGreen.png");
                     source = new BitmapImage(new Uri("pack://application:,,,/RPA-Workbench-Revision2;component//1. Resources/ProjectWindow Images/Folder Dark -32.png"));
                    return source;
                }
                else
                {
                    //System.Windows.Forms.MessageBox.Show @value.ToString()))'
                    //Icon IEIcon = Icon.ExtractAssociatedIcon(@value.ToString());
                    //System.Drawing.Image source = IEIcon.ToBitmap();

                    //Uri uri = new Uri("pack://application:,,,/RPA Workbench;component//Resources/FolderIcon_ExcelGreen.png");
                    //BitmapImage source = new BitmapImage(uri);
                     source = new BitmapImage(new Uri("pack://application:,,,/RPA-Workbench-Revision2;component//1. Resources/ProjectWindow Images/Folder Dark -32.png"));
                    return source;
                }
            //}
            //else if (Properties.Settings.Default.ThemeType == 1)
            //{
            //    if ((value as string).Contains(@"\"))
            //    {
            //        // Uri uri = new Uri("pack://application:,,,/RPA Workbench;component//Resources/FolderIcon_ExcelGreen.png");
            //         source = new BitmapImage(new Uri("pack://application:,,,/RPA Workbench;component//Resources/Toolwindow Icons/FolderLight.png"));
            //      //  return source;
            //    }
            //    else
            //    {
            //        //System.Windows.Forms.MessageBox.Show @value.ToString()))'
            //        //Icon IEIcon = Icon.ExtractAssociatedIcon(@value.ToString());
            //        //System.Drawing.Image source = IEIcon.ToBitmap();

            //        //Uri uri = new Uri("pack://application:,,,/RPA Workbench;component//Resources/FolderIcon_ExcelGreen.png");
            //        //BitmapImage source = new BitmapImage(uri);
                   
            //        source = new BitmapImage(new Uri("pack://application:,,,/RPA Workbench;component/Resources/Toolwindow Icons/FolderLight.png"));

            //    }

            //}
            return source;
        }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException("Cannot convert back");
            }
        }

    #endregion // DoubleToIntegerConverter

  
}

