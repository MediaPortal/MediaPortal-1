using System.ComponentModel;
using System.Windows.Forms;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
	public class AppFilesImportProgress : AppSettings
	{
		private Label label3;
		private TextBox ProgressText;
		private IContainer components = null;
		private AppItem m_CurApp;
		private bool m_ImportRunning = false;

		public AppFilesImportProgress()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label3 = new System.Windows.Forms.Label();
			this.ProgressText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(0, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(248, 32);
			this.label3.TabIndex = 1;
			this.label3.Text = "Import running....";
			// 
			// ProgressText
			// 
			this.ProgressText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressText.Location = new System.Drawing.Point(8, 32);
			this.ProgressText.Multiline = true;
			this.ProgressText.Name = "ProgressText";
			this.ProgressText.ReadOnly = true;
			this.ProgressText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ProgressText.Size = new System.Drawing.Size(336, 368);
			this.ProgressText.TabIndex = 2;
			this.ProgressText.Text = "";
			// 
			// AppFilesImportProgress
			// 
			this.Controls.Add(this.ProgressText);
			this.Controls.Add(this.label3);
			this.Name = "AppFilesImportProgress";
			this.Size = new System.Drawing.Size(360, 408);
			this.ResumeLayout(false);

		}
		#endregion


		public void RunImport()
		{
			if (CurApp != null)
			{
				ProgressText.Text = "";
				Application.DoEvents(); // make sure the title caption appears....
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

		public AppItem CurApp
		{
			get{ return m_CurApp; }
			set{ m_CurApp = value; }
		}

		public bool ImportRunning
		{
			get{ return m_ImportRunning; }
			set{ m_ImportRunning = value; }
		}

	}
}

