using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using ProgramsDatabase;
using Programs.Utils;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for ImportStatusForm.
	/// </summary>
	public class ImportStatusForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox ProgressText;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private AppItem m_CurApp;
		private bool m_ImportRunning = false;

		public ImportStatusForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProgressText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// ProgressText
			// 
			this.ProgressText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressText.Location = new System.Drawing.Point(4, 8);
			this.ProgressText.Multiline = true;
			this.ProgressText.Name = "ProgressText";
			this.ProgressText.ReadOnly = true;
			this.ProgressText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ProgressText.Size = new System.Drawing.Size(444, 328);
			this.ProgressText.TabIndex = 0;
			this.ProgressText.Text = "";
			// 
			// ImportStatusForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 342);
			this.Controls.Add(this.ProgressText);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportStatusForm";
			this.ShowInTaskbar = false;
			this.Text = "Import running...";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ImportStatusForm_Closing);
			this.ResumeLayout(false);

		}
		#endregion


		public void RunImport()
		{
			if (CurApp != null)
			{
				CurApp.OnRefreshInfo += new AppItem.RefreshInfoEventHandler(RefreshInfo);
				try
				{
					ProgressText.Text = "=== import started...";
					m_ImportRunning = true;
					CurApp.Refresh(false);
				}
				finally
				{
					CurApp.OnRefreshInfo -= new AppItem.RefreshInfoEventHandler(RefreshInfo);
					this.Text = "Import finished.";
					ProgressText.Text = ProgressText.Text + "\r\n" + "=== import finished.";
					m_ImportRunning = false;
				}
			}
		}


		private void RefreshInfo(string Message)
		{
			ProgressText.Text = ProgressText.Text + "\r\n" + Message;
			ProgressText.SelectionStart = ProgressText.Text.Length;
			ProgressText.ScrollToCaret();
			Application.DoEvents();
		}

		private void ImportStatusForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = m_ImportRunning;
		}

		public AppItem CurApp
		{
			get{ return m_CurApp; }
			set{ m_CurApp = value; }
		}

	}
}
