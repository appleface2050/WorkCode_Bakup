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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar : UserControl
    {
        List<String> progressMsg = new List<string>(Locale.Strings.GetLocalizedString("Wait_Message_String_Comma_Seprated").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        int mProgressTextIndex = 0;
        DispatcherTimer mProgressTimer;

        DispatcherTimer mTextBlockTimer;

        public ProgressBar()
        {
            InitializeComponent();
            mProgressTimer = new DispatcherTimer();
            mProgressTimer.Tick += ProgressTimerTick;
            mProgressTimer.Interval = TimeSpan.FromMilliseconds(10);
            mProgressTimer.Start();

            mTextBlockTimer = new DispatcherTimer();
            mTextBlockTimer.Tick += mTextBlockTimer_Tick;
            mTextBlockTimer.Interval = TimeSpan.FromSeconds(3);
            mTextBlockTimer.Start();

            TextBlockProgressText.Text = progressMsg[mProgressTextIndex];
            mProgressTextIndex++;
        }

        void mTextBlockTimer_Tick(object sender, EventArgs e)
        {
            if (mProgressTextIndex == progressMsg.Count)
            {
                mProgressTextIndex = 0;
            }

            TextBlockProgressText.Text = progressMsg[mProgressTextIndex];
            mProgressTextIndex++;
        }


        private void ProgressTimerTick(object sender, EventArgs e)
        {
            if (ProgressIndicatorTransform.X > gridProgress.ActualWidth + ProgressIndicator.ActualWidth)
            {
                ProgressIndicatorTransform.X = 0;
                BlueBoxTrasform.X = 0;
            }

            ProgressIndicatorTransform.X = ProgressIndicatorTransform.X + 4;
            BlueBoxTrasform.X = Math.Min(BlueBoxTrasform.X + 5, ProgressGrid.ActualWidth - ProgressBarBlueBox.ActualWidth - 1);
        }

        internal void Reset()
        {
            //TODO : To be implemented
        }
    }
}
