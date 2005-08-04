using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mpe.Forms
{
	/// <summary>
	/// Summary description for PreferencesForm.
	/// </summary>
	public class MpePreferenceForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.FolderBrowserDialog folderBrowser;
		private System.Windows.Forms.PropertyGrid grid;
		private System.ComponentModel.Container components = null;

		private MpePreferences preferences;

		public MpePreferenceForm() {
			InitializeComponent();
			preferences = MediaPortalEditor.Global.Preferences;
		}

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpePreferenceForm));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.grid = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(282, 278);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(362, 278);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// folderBrowser
			// 
			this.folderBrowser.ShowNewFolderButton = false;
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grid.CommandsVisibleIfAvailable = true;
			this.grid.LargeButtons = false;
			this.grid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.grid.Location = new System.Drawing.Point(8, 8);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(432, 264);
			this.grid.TabIndex = 2;
			this.grid.Text = "PropertyGrid";
			this.grid.ToolbarVisible = false;
			this.grid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.grid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// MpePreferenceForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(450, 312);
			this.Controls.Add(this.grid);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MpePreferenceForm";
			this.Text = "Preferences";
			this.Load += new System.EventHandler(this.PreferencesForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void cancelButton_Click(object sender, System.EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void PreferencesForm_Load(object sender, System.EventArgs e) {
			CenterToScreen();
			grid.SelectedObject = preferences;
		}

		private void okButton_Click(object sender, System.EventArgs e) {
			try {
				preferences.Save();
			} catch (Exception ee) {
				MpeLog.Error(ee);
				return;
			}
			DialogResult = DialogResult.OK;
			Close();
		}

	}

	public class DirectoryEditor : UITypeEditor {
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			return UITypeEditorEditStyle.Modal;
		}
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null)
				MpeLog.Info("Context: " + context.Instance.GetType().ToString());
			if (value != null)
				MpeLog.Info("Value: " + value.ToString());
			FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
			folderBrowser.ShowDialog();
			return folderBrowser.SelectedPath;
		}
	}
}