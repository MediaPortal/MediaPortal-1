using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Mpe.Forms
{
	public class MpeSkinBrowserDialog : System.Windows.Forms.Form {
	
		#region Variables
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label3;
		private System.ComponentModel.Container components = null;
		private MpePreferences preferences;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView skinList;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.PictureBox previewBox;
		private System.Windows.Forms.TextBox nameTextBox;
		private Hashtable previews;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.Label listLabel;
		private MpeSkinBrowserMode mode;
		private DirectoryInfo skinDir;
		#endregion

		#region Constructors
		public MpeSkinBrowserDialog() : this(MpeSkinBrowserMode.New) {
			//
		}
		public MpeSkinBrowserDialog(MpeSkinBrowserMode browserMode) {
			InitializeComponent();
			mode = browserMode;
			preferences = MediaPortalEditor.Global.Preferences;
			previews = new Hashtable();
			if (preferences == null)
				throw new Exception("Could not load preferences");
		}
		#endregion

		#region Properties
		public Image SelectedSkinPreview {
			get {
				if (skinList.SelectedItems.Count > 0) {
					return (Image)previews[skinList.SelectedItems[0].Text];
				}
				return null;
			}
		}
		public DirectoryInfo SelectedSkinDir {
			get {
				if (skinList.SelectedItems.Count > 0) {
					return (DirectoryInfo)skinList.SelectedItems[0].Tag;
				}
				return null;
			}
		}
		public DirectoryInfo NewSkinDir {
			get {
				return skinDir;
			}
		}
		public string SkinName {
			get {
				return nameTextBox.Text.Trim();
			}
		}
		#endregion

		#region Methods
		
		#endregion

		#region Event Handlers
		private void OnLoad(object sender, System.EventArgs e) {
			CenterToParent();
			if (mode == MpeSkinBrowserMode.Open) {
				listLabel.Text = "Skins";
				Height = 336;
				Text = "Open Skin";
			} else {
				listLabel.Text = "Templates";
				Height = 368;
				Text = "New Skin";
			}
			DirectoryInfo[] skins = preferences.MediaPortalSkins;
			for (int i = 0; i < skins.Length; i++) {
				ListViewItem item = skinList.Items.Add(skins[i].Name);
				item.Tag = skins[i];
				FileInfo[] f = skins[i].GetFiles("preview.jpg");
				if (f != null && f.Length > 0) {
					Bitmap b = new Bitmap(f[0].FullName);
					previews.Add(skins[i].Name, b);
				}
			}
		}
		private void OnCancelClick(object sender, System.EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}
		private void OnOkClick(object sender, System.EventArgs e) {
			if (mode == MpeSkinBrowserMode.New) {
				if (SelectedSkinDir == null) {
					MessageBox.Show(this, "Please select the skin that will be used a template for your new skin.", "Skin Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				if (SkinName.Length == 0) {
					MessageBox.Show(this, "Please specify a name for the new skin.", "Name Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
					nameTextBox.Focus();
					return;
				}
				if (SkinName.IndexOfAny(new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' }) >= 0) {
					MessageBox.Show(this, "The skin name cannot contain any of the following characters:" + Environment.NewLine + "/ \\ : * ? \" < > | ", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
					nameTextBox.Focus();
					return;
				}
				try {
					skinDir = new DirectoryInfo(preferences.MediaPortalSkinDir.FullName + "\\" + SkinName);
					if (skinDir.Exists) {
						MessageBox.Show(this, "A skin with the specified name already exists.", "Invalid Skin Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
						nameTextBox.Focus();
						return;
					}
				} catch (Exception ee) {
					MessageBox.Show(this, ee.Message, "Invalid Skin Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
					nameTextBox.Focus();
					return;
				}
			} else {
				if (SelectedSkinDir == null) {
					MessageBox.Show(this, "Please select the skin that you want to open.", "Skin Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
			}
			DialogResult = DialogResult.OK;
			Close();
		}
		private void OnIndexChanged(object sender, System.EventArgs e) {
			previewBox.Image = SelectedSkinPreview;
		}
		private void OnDoubleClick(object sender, System.EventArgs e) {
			if (mode == MpeSkinBrowserMode.Open) {
				OnOkClick(sender, e);
			}
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
				if (previews != null) {
					ICollection c = previews.Values;
					IEnumerator e = c.GetEnumerator();
					while (e.MoveNext()) {
						Bitmap b = (Bitmap)e.Current;
						b.Dispose();
					}
				}
			}
			base.Dispose( disposing );
		}
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpeSkinBrowserDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.listLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.skinList = new System.Windows.Forms.ListView();
			this.colName = new System.Windows.Forms.ColumnHeader();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(304, 304);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.OnOkClick);
			// 
			// nameLabel
			// 
			this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(8, 267);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(88, 16);
			this.nameLabel.TabIndex = 3;
			this.nameLabel.Text = "New Skin Name:";
			// 
			// nameTextBox
			// 
			this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.nameTextBox.Location = new System.Drawing.Point(96, 264);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(368, 20);
			this.nameTextBox.TabIndex = 4;
			this.nameTextBox.Text = "";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(384, 304);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.OnCancelClick);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Location = new System.Drawing.Point(8, 296);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(456, 3);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			// 
			// listLabel
			// 
			this.listLabel.AutoSize = true;
			this.listLabel.Location = new System.Drawing.Point(8, 8);
			this.listLabel.Name = "listLabel";
			this.listLabel.Size = new System.Drawing.Size(57, 16);
			this.listLabel.TabIndex = 7;
			this.listLabel.Text = "Templates";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(416, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(45, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "Preview";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.previewBox);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.skinList);
			this.panel1.Location = new System.Drawing.Point(8, 24);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(456, 232);
			this.panel1.TabIndex = 10;
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.White;
			this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.previewBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.previewBox.Location = new System.Drawing.Point(163, 0);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(293, 232);
			this.previewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.previewBox.TabIndex = 12;
			this.previewBox.TabStop = false;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(160, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 232);
			this.splitter1.TabIndex = 11;
			this.splitter1.TabStop = false;
			// 
			// skinList
			// 
			this.skinList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																											  this.colName});
			this.skinList.Dock = System.Windows.Forms.DockStyle.Left;
			this.skinList.FullRowSelect = true;
			this.skinList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.skinList.HideSelection = false;
			this.skinList.Location = new System.Drawing.Point(0, 0);
			this.skinList.MultiSelect = false;
			this.skinList.Name = "skinList";
			this.skinList.Size = new System.Drawing.Size(160, 232);
			this.skinList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.skinList.TabIndex = 10;
			this.skinList.View = System.Windows.Forms.View.Details;
			this.skinList.DoubleClick += new System.EventHandler(this.OnDoubleClick);
			this.skinList.SelectedIndexChanged += new System.EventHandler(this.OnIndexChanged);
			// 
			// colName
			// 
			this.colName.Text = "Name";
			this.colName.Width = 120;
			// 
			// MpeSkinBrowserDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(474, 336);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.listLabel);
			this.Controls.Add(this.nameTextBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MpeSkinBrowserDialog";
			this.ShowInTaskbar = false;
			this.Text = "New Skin";
			this.Load += new System.EventHandler(this.OnLoad);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		
	}

	public enum MpeSkinBrowserMode {	
		New, 
		Open 
	};
}
