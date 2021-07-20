using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CustomControls.Views
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml.
    /// </summary>
    public partial class CustomMessageBox : ActiproSoftware.Windows.Controls.Ribbon.RibbonWindow
    {

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        static CustomMessageBox _messageBox;
        static MessageBoxResults _result = MessageBoxResults.No;
        public enum MessageBoxType
        {
            ConfirmationWithYesNo = 0,
            ConfirmationWithYesNoCancel,
            Information,
            Error,
            Warning
        }

        public enum MessageBoxImage
        {
            Warning = 0,
            Question,
            Information,
            Error,
            None
        }

        public enum MessageBoxButtons
        {
            OKCancel = 0,
            YesNo,
            YesNoBrowse,
            OK
        }

        public enum MessageBoxResults
        {
           OK,
           Yes,
           No,
           Cancel,
           Browse,
           None
        }

        public static MessageBoxResults Show
        (string caption, string msg, MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.ConfirmationWithYesNo:
                    return (MessageBoxResults)Show(caption, msg, MessageBoxButtons.YesNo,
                    MessageBoxImage.Question);
                //case MessageBoxType.ConfirmationWithYesNoCancel:
                //    return Show(caption, msg, MessageBoxButtons.YesNoCancel,
                //    MessageBoxImage.Question);
                case MessageBoxType.Information:
                    return (MessageBoxResults)Show(caption, msg, MessageBoxButtons.OKCancel,
                    MessageBoxImage.Information);
                case MessageBoxType.Error:
                    return (MessageBoxResults)Show(caption, msg, MessageBoxButtons.OKCancel,
                    MessageBoxImage.Error);
                case MessageBoxType.Warning:
                    return (MessageBoxResults)Show(caption, msg, MessageBoxButtons.OKCancel,
                    MessageBoxImage.Warning);
                default:
                    return MessageBoxResults.No;
            }
        }
        public static MessageBoxResults Show(string msg, MessageBoxType type)
        {
            return Show(string.Empty, msg, type);
        }
        public static MessageBoxResults Show(string msg)
        {
            return Show(string.Empty, msg,
            MessageBoxButtons.OKCancel, MessageBoxImage.None);
        }
        public static MessageBoxResults Show
        (string caption, string text)
        {
            return Show(caption, text,
            MessageBoxButtons.OKCancel, MessageBoxImage.None);
        }
        public static MessageBoxResults Show
        (string caption, string text, MessageBoxButtons buttons)
        {
            return Show(caption, text, buttons,
            MessageBoxImage.None);
        }
        public static MessageBoxResults Show
        (string caption, string text,
        MessageBoxButtons buttons, MessageBoxImage image)
        {
            _messageBox = new CustomMessageBox
            { txtMsg = { Text = text }, MessageTitle = { Text = caption } };
            SetVisibilityOfButtons(buttons);
            SetImageOfMessageBox(image);
            _messageBox.ShowDialog();
            return _result;
        }
        private static void SetVisibilityOfButtons(MessageBoxButtons button)
        {
            switch (button)
            {
                case MessageBoxButtons.OKCancel:
                    _messageBox.btnNo.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Visibility = Visibility.Collapsed;
                    _messageBox.btnBrowse.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButtons.YesNo:
                    _messageBox.btnNo.Visibility = Visibility.Visible;
                    _messageBox.btnYes.Visibility = Visibility.Visible;
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnBrowse.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButtons.YesNoBrowse:
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnNo.Visibility = Visibility.Visible;
                    _messageBox.btnYes.Visibility = Visibility.Visible;
                    _messageBox.btnBrowse.Visibility = Visibility.Visible;
                    break;

                case MessageBoxButtons.OK:
                    _messageBox.btnOk.Visibility = Visibility.Visible;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnNo.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Visibility = Visibility.Collapsed;
                    _messageBox.btnBrowse.Visibility = Visibility.Collapsed;

                    break;
                default:
                    break;
            }
        }
        private static void SetImageOfMessageBox(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Warning:
                    _messageBox.SetImage("Warning.png");
                    break;
                case MessageBoxImage.Question:
                    _messageBox.SetImage("Info.png");
                    break;
                case MessageBoxImage.Information:
                    _messageBox.SetImage("Information.png");
                    break;
                case MessageBoxImage.Error:
                    _messageBox.SetImage("/CustomControls;component/Resources/Error.png");
                    break;
                default:
                    _messageBox.img.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
            {
                _result = MessageBoxResults.OK;
                
                Close();
            }
            else if (sender == btnYes)
            {
                _result = MessageBoxResults.Yes;
                Close();
            }
            else if (sender == btnNo)
            {
                _result = MessageBoxResults.No;
                Close();
            }
            else if (sender == btnCancel)
            {
                _result = MessageBoxResults.Cancel;
                Close();
            }
            else if (sender == btnBrowse)
            {
                _result = MessageBoxResults.Browse;
            }
            else
                _result = MessageBoxResults.None;
            _messageBox.Close();
            _messageBox = null;
        }
        private void SetImage(string imageName)
        {
            string uri = string.Format(imageName);
            var uriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            img.Source = new BitmapImage(uriSource);
        }
    }
}
