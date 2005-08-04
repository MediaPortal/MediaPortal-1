using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Mpe.Designers;

namespace Mpe.Forms
{
	/// <summary>
	/// Summary description for SaveChangesForm.
	/// </summary>
	public class MpeSaveForm : System.Windows.Forms.Form {
		#region Variables

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button noButton;
		private System.Windows.Forms.Button yesButton;
		private System.Windows.Forms.Label title;
		private System.Windows.Forms.ColumnHeader nameCol;
		private System.Windows.Forms.ColumnHeader typeCol;
		private System.Windows.Forms.ListView changeList;
		#endregion

		private System.ComponentModel.Container components = null;

		#region Constructor
		public MpeSaveForm(MpeDesigner[] designers, ImageList imageList) {
			InitializeComponent();
			for (int i = 0; designers != null && i < designers.Length; i++) {
				ListViewItem item = changeList.Items.Add(designers[i].ResourceName);
				item.Tag = designers[i];
				item.Selected = true;
				item.SubItems.Add(designers[i].GetType().Name.Replace("Designer","").Replace("Mpe",""));
			}
			DialogResult = DialogResult.Cancel;
		}
		#endregion

		#region Properties
		public MpeDesigner[] SelectedDesigners {
			get {
				ArrayList array = new ArrayList();
				for (int i = 0; changeList.SelectedItems != null && i < changeList.SelectedItems.Count; i++) {
					ListViewItem item = changeList.SelectedItems[i];
					array.Add(item.Tag);
				}
				return (MpeDesigner[])array.ToArray(typeof(MpeDesigner));
			}
		}
		#endregion
		
		#region Event Handlers
		private void SaveChangesForm_Load(object sender, System.EventArgs e) {
			CenterToParent();
		}

		private void yesButton_Click(object sender, System.EventArgs e) {
			DialogResult = DialogResult.Yes;
			Close();
		}

		private void noButton_Click(object sender, System.EventArgs e) {
			DialogResult = DialogResult.No;
			Close();
		}

		private void cancelButton_Click(object sender, System.EventArgs e) {
			DialogResult = DialogResult.Cancel;
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
		/// Required method for Designer support - Do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.cancelButton = new System.Windows.Forms.Button();
			this.title = new System.Windows.Forms.Label();
			this.noButton = new System.Windows.Forms.Button();
			this.yesButton = new System.Windows.Forms.Button();
			this.changeList = new System.Windows.Forms.ListView();
			this.nameCol = new System.Windows.Forms.ColumnHeader();
			this.typeCol = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(304, 192);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 24);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// title
			// 
			this.title.AutoSize = true;
			this.title.Location = new System.Drawing.Point(7, 6);
			this.title.Name = "title";
			this.title.Size = new System.Drawing.Size(213, 16);
			this.title.TabIndex = 4;
			this.title.Text = "Save changes to the following resources?";
			// 
			// noButton
			// 
			this.noButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.noButton.Location = new System.Drawing.Point(224, 192);
			this.noButton.Name = "noButton";
			this.noButton.Size = new System.Drawing.Size(75, 24);
			this.noButton.TabIndex = 2;
			this.noButton.Text = "No";
			this.noButton.Click += new System.EventHandler(this.noButton_Click);
			// 
			// yesButton
			// 
			this.yesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.yesButton.Location = new System.Drawing.Point(144, 192);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = new System.Drawing.Size(75, 24);
			this.yesButton.TabIndex = 1;
			this.yesButton.Text = "Yes";
			this.yesButton.Click += new System.EventHandler(this.yesButton_Click);
			// 
			// changeList
			// 
			this.changeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																												 this.nameCol,
																												 this.typeCol});
			this.changeList.FullRowSelect = true;
			this.changeList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.changeList.HideSelection = false;
			this.changeList.Location = new System.Drawing.Point(8, 24);
			this.changeList.Name = "changeList";
			this.changeList.Size = new System.Drawing.Size(376, 160);
			this.changeList.TabIndex = 0;
			this.changeList.View = System.Windows.Forms.View.Details;
			// 
			// nameCol
			// 
			this.nameCol.Text = "Name";
			this.nameCol.Width = 280;
			// 
			// typeCol
			// 
			this.typeCol.Text = "Type";
			this.typeCol.Width = 72;
			// 
			// MpeSaveForm
			// 
			this.AcceptButton = this.yesButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(394, 224);
			this.Controls.Add(this.changeList);
			this.Controls.Add(this.yesButton);
			this.Controls.Add(this.noButton);
			this.Controls.Add(this.title);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MpeSaveForm";
			this.ShowInTaskbar = false;
			this.Text = "Save Confirmation";
			this.Load += new System.EventHandler(this.SaveChangesForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

	}
}
