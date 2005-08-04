using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using UtilityLibrary.WinControls;

namespace SkinEditor.Forms
{
	public enum StatusBarMessageType { Debug = 0, Info = 1, Warning = 2, Error = 3 }
	/// <summary>
	/// Summary description for EditorStatusBar.
	/// </summary>
	public class EditorStatusBar : System.Windows.Forms.StatusBar
	{
		//private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.PictureBox icon;
		private ProgressBarEx progressBar;
		private System.ComponentModel.Container components = null;
		ImageList imageList;

		public EditorStatusBar(ImageList statusImageList) {
			imageList = statusImageList;
			Console.WriteLine("There are " + imageList.Images.Count + " images");
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			imageList = new ImageList();
			imageList.ColorDepth = ColorDepth.Depth24Bit;
			Height = 20;
			progressBar = new ProgressBarEx();
			progressBar.Smooth = true;
			progressBar.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top)));
			progressBar.Size = new Size(150,16);
			progressBar.Location = new Point(Width - 165,2);
			progressBar.Enable3DBorder = true;
			progressBar.Border3D = Border3DStyle.Flat;
			progressBar.BorderColor = Color.FromKnownColor(KnownColor.ControlDark);
			Controls.Add(progressBar);
		}

		public void SetStatus(StatusBarMessageType type, string message, int progress) {
			if (progress <= 0)
				progressBar.Hide();
			else
				progressBar.Show();
			progressBar.Value = progress;
			Text = "      " + message;
			icon.Image = imageList.Images[0];
			/*if (type == StatusBarMessageType.Info)
				icon.Image = imageList.Images[
			else
				icon.Image = imageList.Images[0];*/
		}

		public void InitStatus(int min, int max) {
			progressBar.Minimum = min;
			progressBar.Maximum = max;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EditorStatusBar));
			this.icon = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// icon
			// 
			this.icon.Image = ((System.Drawing.Image)(resources.GetObject("icon.Image")));
			this.icon.Location = new System.Drawing.Point(193, 17);
			this.icon.Name = "icon";
			this.icon.Size = new System.Drawing.Size(16, 16);
			this.icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.icon.TabIndex = 1;
			this.icon.TabStop = false;
			// 
			// EditorStatusBar
			// 
			this.Controls.Add(this.icon);
			this.Size = new System.Drawing.Size(536, 22);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
