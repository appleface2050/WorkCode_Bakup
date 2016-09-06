using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        private System.Windows.Forms.DialogResult result;
        public static bool sIsCloseMessageBoxUp = false;

        public CustomMessageBox()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public static System.Windows.Forms.DialogResult ShowMessageBox(object sender, string title, string message, string leftBtnLbl,
            string rightBtnLbl, string cancelBtnLbl, string checkBoxLbl, bool showRememberChoice)
        {
            sIsCloseMessageBoxUp = true;
            CustomMessageBox msgBox = new CustomMessageBox();
            msgBox.Title = title;
            msgBox.textBox1.Text = message;
            msgBox.mYesLbl.Content = leftBtnLbl;
            msgBox.mNoLbl.Content = rightBtnLbl;
            msgBox.mCancelLbl.Content = cancelBtnLbl;
            msgBox.mChkBoxLbl.Text = checkBoxLbl;

            if (!showRememberChoice)
            {
                msgBox.mRememberChoiceChkbox.Visibility = Visibility.Hidden;
            }

            msgBox.Owner = (Window)sender;

            msgBox.ShowDialog();
            sIsCloseMessageBoxUp = false;

            return msgBox.result;
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            switch (lbl.Name)
            {
                case "mYesLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55e0cc"));
                    break;
                case "mNoLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f66a4c"));
                    break;
                case "mCancelLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88a3b3"));
                    break;
                default: break;
            }
        }

        private void Label_MouseDown(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            switch (lbl.Name)
            {
                case "mYesLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a9efe1"));
                    break;
                case "mNoLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fbb5a6"));
                    break;
                case "mCancelLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c4d1d9"));
                    break;
                default: break;
            }
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            switch (lbl.Name)
            {
                case "mYesLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8b9ba3"));
                    break;
                case "mNoLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f64c4c"));
                    break;
                case "mCancelLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c1cfd8"));
                    break;
                default: break;
            }
        }

        private void Label_MouseUp(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            switch (lbl.Name)
            {
                case "mYesLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55e0cc"));
                    break;
                case "mNoLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f66a4c"));
                    break;
                case "mCancelLbl": lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88a3b3"));
                    break;
                default: break;
            }
        }

        private void mYesLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.result = System.Windows.Forms.DialogResult.Yes;
            this.Close();
        }

        private void mNoLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.result = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        private void mCancelLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.result = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        void Handle(CheckBox checkBox)
        {
            bool flag = checkBox.IsChecked.Value;
            GameManagerUtilities.sRememberClosingPopupChoice = flag;
        }

    }
}
