using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Mpe.Designers
{
	/// <summary>
	/// Summary description for MpeHelpBrowser.
	/// </summary>
	public class MpeHelpBrowser : UserControl, MpeDesigner {
		#region Variables
		private System.ComponentModel.Container components = null;
		private MediaPortalEditor mpe;
		private AxSHDocVw.AxWebBrowser browser;
		#endregion

		#region Contructors
		public MpeHelpBrowser(MediaPortalEditor mpe) {
			this.mpe = mpe;
			InitializeComponent();
		}
		#endregion
		
		#region Methods
		public void ShowHelp(FileInfo file) {
			if (file == null || file.Exists == false) {
				throw new DesignerException("Invalid help file");
			}
			try {
				browser.Navigate(FileToUrl(file));
			} catch (Exception e) {
				MpeLog.Error(e);
				throw new DesignerException(e.Message);
			}
		}
		public void Initialize() {
			//
		}
		public void Save() {
			//
		}
		public void Cancel() {
			//
		}
		public void Destroy() {
			browser.Dispose();
		}
		public void Pause() {
			mpe.PropertyManager.SelectedResource = null;
			mpe.PropertyManager.HideResourceList();
		}
		public void Resume() {
			mpe.PropertyManager.SelectedResource = null;
			mpe.PropertyManager.HideResourceList();
		}
		private string FileToUrl(FileInfo file) {
			if (file == null)
				throw new Exception("File cannot be null!");
			return "file:///" + file.FullName.Replace("\\","/");

		}
		private string FileToUrl(DirectoryInfo file) {
			if (file == null)
				throw new Exception("File cannot be null!");
			return "file:///" + file.FullName.Replace("\\","/");

		}
		private string FileToUrl(string path) {
			return "file:///" + path.Replace("\\","/");
		}
		#endregion

		#region Properties
		public string ResourceName {
			get {
				return "Help";
			}
		}
		public bool AllowAdditions {
			get {
				return false;
			}
		}
		public bool AllowDeletions {
			get {
				return false;
			}
		}
		#endregion	

		#region Component Designer Generated Code
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
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpeHelpBrowser));
			this.browser = new AxSHDocVw.AxWebBrowser();
			((System.ComponentModel.ISupportInitialize)(this.browser)).BeginInit();
			this.SuspendLayout();
			// 
			// browser
			// 
			this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.browser.Enabled = true;
			this.browser.Location = new System.Drawing.Point(0, 0);
			this.browser.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("browser.OcxState")));
			this.browser.Size = new System.Drawing.Size(368, 224);
			this.browser.TabIndex = 0;
			// 
			// MpeHelpBrowser
			// 
			this.Controls.Add(this.browser);
			this.Name = "MpeHelpBrowser";
			this.Size = new System.Drawing.Size(368, 224);
			((System.ComponentModel.ISupportInitialize)(this.browser)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

	}
}
