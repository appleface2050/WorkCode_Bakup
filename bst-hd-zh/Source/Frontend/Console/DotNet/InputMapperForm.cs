using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{

	public class InputMapperForm : Form
	{

		public delegate void EditHandler(String package);
		public delegate void ManageHandler(String package);

		private const int WIDTH = 400;
		private const int HEIGHT = 250;
		private const int PADDING = 10;
		private const int TEXT_HEIGHT = 50;

		private String mPackage;
		private EditHandler mEditHandler;
		private ManageHandler mManageHandler;

		public InputMapperForm(String package, EditHandler editHandler,
			ManageHandler manageHandler)
		{
			mPackage = package;
			mEditHandler = editHandler;
			mManageHandler = manageHandler;

			Text = "Input Mapper Tool";

			CreateLayout();
		}

		private void CreateLayout()
		{
			Size = new Size(WIDTH, HEIGHT);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MinimizeBox = false;
			MaximizeBox = false;

			/*
			 * Current App
			 */

			Label desc = new Label();
			desc.Text = "Current app: " +
				(mPackage != null ? mPackage : "none");

			desc.Location = new Point(PADDING, PADDING);
			desc.Width = ClientSize.Width - PADDING;

			/*
			 * Edit Button
			 */

			Button edit = new Button();
			edit.Text = "Edit";

			edit.Location = new Point(
				PADDING,
				desc.Bottom + PADDING);

			if (mPackage == null)
				edit.Enabled = false;

			edit.Click += delegate (Object obj, EventArgs evt)
			{
				mEditHandler(mPackage);
				Close();
			};

			Label editDesc = new Label();
			editDesc.Text = "Edit the input mapper configuration " +
				"for the current app.  If the current app does not " +
				"yet have a configuration file, then create one " +
				"from a template.";

			editDesc.Location = new Point(
				edit.Right + PADDING,
				edit.Top);

			editDesc.Size = new Size(
				ClientSize.Width - editDesc.Left - PADDING,
				TEXT_HEIGHT);

			/*
			 * Manage Button
			 */

			Button manage = new Button();
			manage.Text = "Manage";

			manage.Location = new Point(
				PADDING,
				editDesc.Bottom + PADDING);

			manage.Click += delegate (Object obj, EventArgs evt)
			{
				mManageHandler(mPackage);
				Close();
			};

			Label manageDesc = new Label();
			manageDesc.Text = "Manage all the existing input " +
				"mapper configurations.  Opens the input mapper " +
				"folder in Windows Explorer.";

			manageDesc.Location = new Point(
				manage.Right + PADDING,
				manage.Top);

			manageDesc.Size = new Size(
				ClientSize.Width - manageDesc.Left - PADDING,
				TEXT_HEIGHT);

			/*
			 * Cancel Button
			 */

			Button cancel = new Button();
			cancel.Text = "Cancel";

			cancel.Location = new Point(
				PADDING,
				manageDesc.Bottom + PADDING);

			cancel.Click += delegate (Object obj, EventArgs evt)
			{
				Close();
			};

			Label cancelDesc = new Label();
			cancelDesc.Text = "Close this window.";

			cancelDesc.Location = new Point(
				cancel.Right + PADDING,
				cancel.Top);

			cancelDesc.Size = new Size(
				ClientSize.Width - cancelDesc.Left - PADDING,
				TEXT_HEIGHT);

			/*
			 * Add Controls
			 */

			Controls.AddRange(new Control[] {
			desc,
			edit,
			editDesc,
			manage,
			manageDesc,
			cancel,
			cancelDesc,
		});
		}
	}

}
