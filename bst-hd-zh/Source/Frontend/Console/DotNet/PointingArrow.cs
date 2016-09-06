using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Frontend
{
	public class PointingArrow : Form
	{
		private Color colorKey = Color.FromArgb(255, 128, 128, 128);

		public static Rectangle s_ScreenDimesnion = Screen.PrimaryScreen.WorkingArea;
		public static int s_xPos = 0;
		public static int s_yPos = 0;
		public static int s_xMovement = 0;
		public static int s_yMovement = 0;

		private static int s_ArrowDisplacement = 50;
		private static RotateFlipType s_RotationAngel = RotateFlipType.RotateNoneFlipNone;
		private static Form s_ArrowForm;

		private static int s_DockHeight = 300;

		private const int s_DockPosTop = 0;
		private const int s_DockPosBottom = 1;
		private const int s_DockPosLeft = 2;
		private const int s_DockPosRight = 3;

		public static System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();

		public PointingArrow()
		{
			InitSettings();

			TransparencyKey = colorKey;

			this.SuspendLayout();

			this.StartPosition = FormStartPosition.Manual;
			this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			this.SizeGripStyle = SizeGripStyle.Hide;
			this.ShowIcon = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.FormBorderStyle = FormBorderStyle.None;
			this.ClientSize = new System.Drawing.Size(180, 180);

			animationTimer.Tick += new EventHandler(AnimateArrow);

			animationTimer.Interval = 200;

			TopMost = true;

			SetStyle(ControlStyles.Opaque, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			this.ResumeLayout(false);

			this.Location = new Point(s_xPos, s_yPos);
			PerformLayout();
			s_ArrowForm = this;
			animationTimer.Start();
		}

		private static void AnimateArrow(Object myObject, EventArgs myEventArgs)
		{
			animationTimer.Stop();
			bool movementDirection = true;
			for (int i = 0; i < 15; i++)
			{
				Thread.Sleep(200);
				if (movementDirection)
				{
					s_ArrowForm.Location = new Point(s_xPos + s_xMovement, s_yPos + s_yMovement);
					movementDirection = false;
				}
				else
				{
					s_ArrowForm.Location = new Point(s_xPos - s_xMovement, s_yPos - s_yMovement);
					movementDirection = true;
				}
			}

			s_ArrowForm.Hide();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			DoPaint(g);
		}

		private void DoPaint(Graphics g)
		{
			g.CompositingMode = CompositingMode.SourceCopy;
			g.FillRectangle(new SolidBrush(colorKey), 0, 0, Width, Height);

			g.CompositingMode = CompositingMode.SourceOver;

			RegistryKey HKLMregistry = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)HKLMregistry.GetValue("InstallDir");
			string imagePath = Path.Combine(installDir, "PointingArrow.png");

			Image bgImage = Image.FromFile(imagePath);
			bgImage.RotateFlip(s_RotationAngel);

			g.DrawImage(bgImage, new Point(0, 0));
		}

		public static void InitSettings()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\RocketDock");

			int dockPos = -1;
			try
			{
				dockPos = Convert.ToInt32((string)key.GetValue("Side", "3"));
			}
			catch (Exception e)
			{
				// Ignore
				return;
			}

			switch (dockPos)
			{
				case s_DockPosTop:
					s_xPos = s_ScreenDimesnion.Width / 2;
					s_yPos = s_DockHeight;
					s_xMovement = 0;
					s_yMovement = s_ArrowDisplacement;
					s_RotationAngel = RotateFlipType.Rotate270FlipNone;
					break;

				case s_DockPosBottom:
					s_xPos = s_ScreenDimesnion.Width / 2;
					s_yPos = s_ScreenDimesnion.Height - s_DockHeight;
					s_xMovement = 0;
					s_yMovement = -1 * s_ArrowDisplacement;
					s_RotationAngel = RotateFlipType.Rotate90FlipNone;
					break;

				case s_DockPosLeft:
					s_xPos = s_DockHeight;
					s_yPos = s_ScreenDimesnion.Height / 2;
					s_xMovement = -1 * s_ArrowDisplacement;
					s_yMovement = 0;
					s_RotationAngel = RotateFlipType.Rotate180FlipNone;
					break;

				case s_DockPosRight:
					s_xPos = s_ScreenDimesnion.Width - s_DockHeight;
					s_yPos = s_ScreenDimesnion.Height / 2;
					s_xMovement = s_ArrowDisplacement;
					s_yMovement = 0;
					s_RotationAngel = RotateFlipType.RotateNoneFlipNone;
					break;

				default:
					return;
			}

			/*
			   Application.EnableVisualStyles();
			   Application.SetCompatibleTextRenderingDefault(false);
			   Application.Run(new PointingArrow());
			   */
		}
	}
}
