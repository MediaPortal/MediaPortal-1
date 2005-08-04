using System;
using System.IO;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Mpe.Controls;
using Mpe.Controls.Properties;

namespace Mpe.Controls.Design
{
	#region MpeImageEditorForm
	public class MpeImageEditorForm : System.Windows.Forms.UserControl {
		
		#region Variables
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.ListBox imageList;
		private MpeParser skinParser;
		private string selectedImageName;
		private System.Windows.Forms.Panel thumbPanel;
		private System.Windows.Forms.PictureBox thumbPictureBox;
		private IWindowsFormsEditorService editorService;
		#endregion
			
		#region Constructors
		public MpeImageEditorForm(System.IO.FileInfo currentValue, MpeParser skinParser, IWindowsFormsEditorService editorService) {
			InitializeComponent();
			this.skinParser = skinParser;
			this.editorService = editorService;
			imageList.SelectionMode = SelectionMode.One;
			imageList.Items.Add("(none)");
         for (int i = 0; i < skinParser.ImageFiles.Length; i++) {
				imageList.Items.Add(skinParser.ImageFiles[i]);
				if (skinParser.ImageFiles[i].Equals(currentValue)) {
					imageList.SelectedIndex = (i+1);
				}
			}
			MpeScreen window = (MpeScreen)skinParser.GetControl(MpeControlType.Screen);
			if (window.TextureBack != null)
				thumbPanel.BackgroundImage = new Bitmap(window.TextureBack.FullName);
			imageList.MouseWheel += new MouseEventHandler(imageList_MouseWheel);
		}
		#endregion

		#region Properties
		public string SelectedImageName {
			get {
				return selectedImageName;
			}
		}
		#endregion

		#region Methods
		public void Close() {
			if (editorService != null)
				editorService.CloseDropDown();
		}
		#endregion
		
		#region Event Handlers
		private void imageList_SelectedIndexChanged(object sender, System.EventArgs e) {
			if (imageList.SelectedIndex >= 0) {
				selectedImageName = imageList.SelectedItem.ToString(); 
				if (selectedImageName.Equals("(none)"))
					selectedImageName = null;
				else 
					thumbPictureBox.Image = skinParser.GetImageThumbnail(selectedImageName);
			}
		}
		private void imageList_MouseWheel(object sender, MouseEventArgs e) {
			int i = 0;
			if (e.Delta > 0)
				i = imageList.SelectedIndex - 1;
			else
				i = imageList.SelectedIndex + 1;
			if (i < 0)
				i = 0;
			else if (i >= imageList.Items.Count)
				i = imageList.Items.Count - 1;
			imageList.SelectedIndex = i;
		}
		private void imageList_DoubleClick(object sender, System.EventArgs e) {
			Close();
		}
		private void thumbPictureBox_Click(object sender, System.EventArgs e) {
			Close();
		}
		#endregion

		#region Windows Form Designer Generated Code
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.imageList = new System.Windows.Forms.ListBox();
			this.thumbPanel = new System.Windows.Forms.Panel();
			this.thumbPictureBox = new System.Windows.Forms.PictureBox();
			this.thumbPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList
			// 
			this.imageList.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.imageList.Location = new System.Drawing.Point(140, 4);
			this.imageList.Name = "imageList";
			this.imageList.Size = new System.Drawing.Size(256, 130);
			this.imageList.Sorted = true;
			this.imageList.TabIndex = 5;
			this.imageList.DoubleClick += new System.EventHandler(this.imageList_DoubleClick);
			this.imageList.SelectedIndexChanged += new System.EventHandler(this.imageList_SelectedIndexChanged);
			// 
			// thumbPanel
			// 
			this.thumbPanel.BackColor = System.Drawing.Color.Transparent;
			this.thumbPanel.Controls.Add(this.thumbPictureBox);
			this.thumbPanel.Location = new System.Drawing.Point(4, 4);
			this.thumbPanel.Name = "thumbPanel";
			this.thumbPanel.Size = new System.Drawing.Size(132, 132);
			this.thumbPanel.TabIndex = 6;
			// 
			// thumbPictureBox
			// 
			this.thumbPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.thumbPictureBox.Location = new System.Drawing.Point(2, 2);
			this.thumbPictureBox.Name = "thumbPictureBox";
			this.thumbPictureBox.Size = new System.Drawing.Size(128, 128);
			this.thumbPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.thumbPictureBox.TabIndex = 4;
			this.thumbPictureBox.TabStop = false;
			this.thumbPictureBox.Click += new System.EventHandler(this.thumbPictureBox_Click);
			// 
			// ImageSelector
			// 
			this.Controls.Add(this.thumbPanel);
			this.Controls.Add(this.imageList);
			this.Name = "ImageSelector";
			this.Size = new System.Drawing.Size(400, 140);
			this.thumbPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		
	}
	#endregion

	#region MpeImageEditor
	public class MpeImageEditor : UITypeEditor {
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			return UITypeEditorEditStyle.DropDown;
		}
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context.Instance is MpeControl) {
				try {
					MpeControl mpc = (MpeControl)context.Instance;
					IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					MpeImageEditorForm selector = new MpeImageEditorForm((System.IO.FileInfo)value, mpc.Parser, editorService);
					editorService.DropDownControl(selector);
					if (selector.SelectedImageName == null) {
						MpeLog.Info("Clearing Image...");
						return null;
					} 
					MpeLog.Info("Changing texture to [" + selector.SelectedImageName + "]");
					return mpc.Parser.GetImageFile(selector.SelectedImageName);
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Error(ee);
				}
			}
			return base.EditValue (context, provider, value);
		}
	}
	#endregion	
}
