using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
    public class ProgressBarControl : UserControl
    {
        private Label mLblLoadingMsg;

        System.Windows.Forms.Timer mTimer = new System.Windows.Forms.Timer();

        public List<string> mLstMessages;
        private ColorProgressBar progressBar1;
        public bool IsVisible = false;
        public ProgressBarControl()
        {
            try
            {
                Logger.Info("Setting Up Progress Bar Control");
                InitializeComponent();
                SetUpUI();
                this.IsVisible = true;
                mTimer.Interval = 30000;
                mTimer.Tick += TimerElapsed;
                mLstMessages = new List<string>(Locale.Strings.GetLocalizedString("Wait_Message_String_Comma_Seprated").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                if (mLstMessages.Count > 0)
                {
                    mLblLoadingMsg.Text = mLstMessages[0];
                }
                mTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
            }
        }
        public ProgressBarStyle Style
        {
            get { return progressBar1.Style; }
            set
            {
                if (progressBar1.Style == ProgressBarStyle.Marquee && value != ProgressBarStyle.Marquee)
                {
                    mTimer.Stop();
                }
                else if (progressBar1.Style != ProgressBarStyle.Marquee && value == progressBar1.Style)
                {
                    mTimer.Start();
                }
                progressBar1.Style = value;

            }
        }
        public int Step
        {
            set
            {
                progressBar1.Value = value;
                mLblLoadingMsg.Text = value.ToString() + " %";
            }
        }

        public bool ShowLabel
        {
            get { return mLblLoadingMsg.Visible; }
            set { mLblLoadingMsg.Visible = value; }
        }

        private void SetUpUI()
        {
            this.mLblLoadingMsg.Font = new Font(GameManagerUtilities.GetFont(), 18, FontStyle.Regular,
               GraphicsUnit.Point, ((byte)(0)));
            this.Size = new System.Drawing.Size(100, 100);
            this.progressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void InitializeComponent()
        {
            this.mLblLoadingMsg = new System.Windows.Forms.Label();
            this.progressBar1 = new BlueStacks.hyperDroid.GameManager.ColorProgressBar();
            this.SuspendLayout();
            // 
            // mLblLoadingMsg
            // 
            this.mLblLoadingMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mLblLoadingMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mLblLoadingMsg.Location = new System.Drawing.Point(3, 45);
            this.mLblLoadingMsg.Name = "mLblLoadingMsg";
            this.mLblLoadingMsg.Size = new System.Drawing.Size(900, 83);
            this.mLblLoadingMsg.TabIndex = 1;
            this.mLblLoadingMsg.Text = "Please Wait...";
            this.mLblLoadingMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(105, 150);
            this.progressBar1.Maximum = 100;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(691, 20);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 2;
            this.progressBar1.Value = 0;
            // 
            // ProgressBarControl
            // 
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.mLblLoadingMsg);
            this.Name = "ProgressBarControl";
            this.Size = new System.Drawing.Size(900, 253);
            this.ResumeLayout(false);

        }

        void TimerElapsed(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    if (!progressBar1.IsDisposed)
                    {
                        if (mLstMessages != null && mLstMessages.Count > 0)
                        {
                            int index = 0;
                            if (mLstMessages.Contains(mLblLoadingMsg.Text))
                            {
                                index = mLstMessages.IndexOf(mLblLoadingMsg.Text) + 1;
                            }
                            if (index == mLstMessages.Count)
                            {
                                index = 0;
                            }
                            mLblLoadingMsg.Text = mLstMessages[index];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            mTimer.Dispose();
            base.Dispose(disposing);
        }
    }

    public class ColorProgressBar : UserControl
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        int min = 0;	// Minimum value for progress range
        int max = 100;	// Maximum value for progress range
        int val = 0;		// Current progress
        int lastValue = 0;
        Rectangle lastBoxRect = new Rectangle();
        int progressWidth = 144;
        public Brush BarColor = new SolidBrush(Color.FromArgb(194, 205, 212));
        public Brush boxColor = new SolidBrush(Color.FromArgb(186, 236, 255));
        Brush _progressBarBackColor = new SolidBrush(Color.FromArgb(255, 255, 255));
        public Brush ProgressBarBackColor
        {
            set
            {
                _progressBarBackColor = value;
                this.BackColor = System.Drawing.Color.DimGray;
            }
        }

        public ColorProgressBar()
        {
            InitializeComponent();
            timer.Tick += timer_Tick;
            this.timer.Interval = 1;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(977, 58);
            this.ResumeLayout(false);

        }
        ProgressBarStyle _style = ProgressBarStyle.Marquee;
        public ProgressBarStyle Style
        {
            get { return _style; }
            set
            {
                Reset();
                if (value == ProgressBarStyle.Marquee)
                {
                    timer.Enabled = true;
                }
                else
                {
                    timer.Enabled = false;
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.Value++;
        }

        protected override void OnResize(EventArgs e)
        {
            // Invalidate the control to get a repaint.
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            float percent = (float)(val - min) / (float)(max - min);
            Rectangle rect = this.ClientRectangle;
            //rect.X = rect.X - 200;
            // Calculate area for drawing the progress.
            rect.Width = (int)((float)rect.Width * percent);
            // System.Drawing.Drawing2D.HatchBrush brush =new HatchBrush(HatchStyle.BackwardDiagonal,Color.SkyBlue )

            // Draw the progress meter.
            g.FillRectangle(Brushes.WhiteSmoke, rect);

            // Draw a three-dimensional border around the control.
            Reset();
            //Draw3DBorder(g);

            // Clean up.
            //b//rush.Dispose();
            g.Dispose();
        }

        public int Minimum
        {
            get
            {
                return min;
            }

            set
            {
                // Prevent a negative value.
                if (value < 0)
                {
                    min = 0;
                }

                // Make sure that the minimum value is never set higher than the maximum value.
                if (value > max)
                {
                    min = value;
                    min = value;
                }

                // Ensure value is still in range
                if (val < min)
                {
                    val = min;
                }

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        public int Maximum
        {
            get
            {
                return max;
            }

            set
            {
                // Make sure that the maximum value is never set lower than the minimum value.
                if (value < min)
                {
                    min = value;
                }

                max = value;

                // Make sure that value is still in range.
                if (val > max)
                {
                    val = max;
                }

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        public int Value
        {
            get
            {
                return val;
            }

            set
            {
                int oldValue = val;

                // Make sure that the value does not stray outside the valid range.
                if (value < min)
                {
                    val = min;
                }
                else if (value > max)
                {
                    if (this.timer.Enabled)
                    {
                        val = min;
                    }
                    else
                    {
                        val = max;
                    }
                }
                else
                {
                    val = value;
                }

                // Invalidate only the changed area.
                float percent;

                Rectangle newValueRect = this.ClientRectangle;
                Rectangle oldValueRect = this.ClientRectangle;

                // Use a new value to calculate the rectangle for progress.
                percent = (float)(val - min) / (float)(max - min);
                newValueRect.Width = (int)((float)newValueRect.Width * percent);

                // Use an old value to calculate the rectangle for progress.
                percent = (float)(oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)((float)oldValueRect.Width * percent);

                Rectangle updateRect = new Rectangle();

                // Find only the part of the screen that must be updated.
                if (newValueRect.Width > oldValueRect.Width)
                {
                    updateRect.X = oldValueRect.Size.Width;
                    updateRect.Width = newValueRect.Width - oldValueRect.Width;
                }
                else
                {
                    updateRect.X = newValueRect.Size.Width;
                    updateRect.Width = oldValueRect.Width - newValueRect.Width;
                }


                updateRect.Height = this.Height - 6;
                updateRect.Y = 3;

                if (Style == ProgressBarStyle.Marquee)
                {
                    // Invalidate the intersection region only.
                    if (updateRect.X != 0 || updateRect.Width != this.Width)
                    {
                        FillRect(BarColor, updateRect);

                        FillRect(BarColor, lastBoxRect);
                        lastBoxRect.Width = 9;
                        lastBoxRect = updateRect;
                        lastBoxRect.X = (int)(updateRect.X * 1.1 - 55);
                        FillRect(boxColor, lastBoxRect);

                        if (updateRect.X > progressWidth)
                        {
                            updateRect.X = updateRect.X - progressWidth;
                            if (lastValue > 0)
                            {
                                lastValue = 0;
                                updateRect.Width += updateRect.X;
                                updateRect.X = 1;
                            }
                        }
                        else
                        {
                            updateRect.X = this.Width - progressWidth + updateRect.X;
                            lastValue = updateRect.X;
                        }
                        FillRect(_progressBarBackColor, updateRect);
                    }
                }
                else
                {
                    FillRect(BarColor, updateRect);
                    if (updateRect.X < lastValue)
                    {
                        Reset();
                    }
                    lastValue = updateRect.X;
                }
            }
        }

        private void FillRect(Brush brush, Rectangle updateRect)
        {
            if (updateRect.X < 1)
            {
                updateRect.X = 1;
            }
            else if (updateRect.X == this.Width)
            {
                updateRect.X = 0;
            }
            else if (updateRect.X + updateRect.Width >= this.Width)
            {
                updateRect.Width = this.Width - (updateRect.X + 1);
            }
            this.CreateGraphics().FillRectangle(brush, updateRect);
        }

        private void Reset()
        {
            lastValue = 0;
            this.Value = 0;
            this.CreateGraphics().FillRectangle(_progressBarBackColor, this.ClientRectangle);
            Draw3DBorder(this.CreateGraphics());
        }

        public Brush ProgressBarColor
        {
            get
            {
                return BarColor;
            }

            set
            {
                BarColor = value;

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            int PenWidth = (int)Pens.White.Width;

            g.DrawLine(Pens.Gray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
            g.DrawLine(Pens.Gray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
        }

        protected override void Dispose(bool disposing)
        {
            timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
