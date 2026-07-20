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
using System.Windows.Shapes;

namespace TonyaRFApp
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel; //result property, keeping messageboxresult so i dont have to change existing code

        //private constructor, cant write "new CustomMessageBox()" outside this class, have to use the static methods
        private CustomMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;

            //configuring message icon based on message type
            switch (icon)
            {
                case MessageBoxImage.Warning: 
                    txtIcon.Text = "⚠";
                    iconCircle.Background = new SolidColorBrush(Color.FromRgb(0xE6, 0x9B, 0x3A));
                    break;

                case MessageBoxImage.Error:
                    txtIcon.Text = "✕";
                    iconCircle.Background = new SolidColorBrush(Color.FromRgb(0xC0, 0x55, 0x55));
                    break;

                case MessageBoxImage.Question:
                    txtIcon.Text = "?";
                    iconCircle.Background = new SolidColorBrush(Color.FromRgb(0x9B, 0x8D, 0xB0));
                    break;

                default: //info / success
                    txtIcon.Text = "✓";
                    iconCircle.Background = new SolidColorBrush(Color.FromRgb(0xC9, 0x74, 0x8F));
                    break;
            }
            //configuring buttons based on mode
            if (buttons == MessageBoxButton.OK)
            {
                btnOk.Content = "OK"; // single centered button, cancel border is already collapsed
            }
            else //YesNo
            {
                //showing cancel button and label
                btnCancelBorder.Visibility = Visibility.Visible;
                btnCancel.Content = "No";
                btnOk.Content = "OK";
            }
        }

        // button click handlers

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        //static show methods -- used to call from by the rest of app, replacing MessageBox.Show()
        public static void Show(string message, string title = "Tonya RF", MessageBoxImage icon = MessageBoxImage.Information)
        {
            var box = new CustomMessageBox(message, title, MessageBoxButton.OK, icon);

            box.Owner = Application.Current.MainWindow; //so that the custom box knows the owner, for windowstartuplocation="centerowner"
            box.ShowDialog();   // shows window AND blocks executions of the calling code until window is closed
        }
        public static MessageBoxResult ShowConfirm(string message, string title = "Confirm", MessageBoxImage icon = MessageBoxImage.Question)
        {
            var box = new CustomMessageBox(message, title, MessageBoxButton.YesNo, icon);

            box.Owner = Application.Current.MainWindow;
            box.ShowDialog();
            return box.Result;
        }
        public static void ShowError(string message, string title = "Error")
        {
            Show(message, title, MessageBoxImage.Error);
        }
        public static void ShowWarning(string message, string title = "Warning")
        {
            Show(message, title, MessageBoxImage.Warning);
        }
    }
}
