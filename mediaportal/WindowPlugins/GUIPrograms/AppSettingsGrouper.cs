using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
	public class AppSettingsGrouper : WindowPlugins.GUIPrograms.AppSettings
	{
		private System.Windows.Forms.ListView listApps;
		private System.Windows.Forms.ColumnHeader TitleColumn;
		private System.ComponentModel.IContainer components = null;

		public AppSettingsGrouper()
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
			this.listApps = new System.Windows.Forms.ListView();
			this.TitleColumn = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// listApps
			// 
			this.listApps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listApps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.TitleColumn});
			this.listApps.Location = new System.Drawing.Point(0, 0);
			this.listApps.Name = "listApps";
			this.listApps.Size = new System.Drawing.Size(456, 320);
			this.listApps.TabIndex = 0;
			this.listApps.View = System.Windows.Forms.View.Details;
			// 
			// TitleColumn
			// 
			this.TitleColumn.Text = "Title";
			this.TitleColumn.Width = 300;
			// 
			// AppSettingsGrouper
			// 
			this.Controls.Add(this.listApps);
			this.Name = "AppSettingsGrouper";
			this.Size = new System.Drawing.Size(464, 320);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnUp_Click(object sender, System.EventArgs e)
		{
			UpButtonClicked();
		}

		private void btnDown_Click(object sender, System.EventArgs e)
		{
			DownButtonClicked();
		}

		private void SyncListView(Applist apps)
		{
			listApps.BeginUpdate();
			try
			{
				listApps.Items.Clear();
				foreach(AppItem app in apps)
				{
					ListViewItem curItem = new ListViewItem(app.Title);
					curItem.Tag = app;
					listApps.Items.Add(curItem);
				}
			}
			finally
			{
				listApps.EndUpdate();
			}
		}

		public override bool Applist2Form(Applist apps)
		{
			SyncListView(apps);
			return true;
		}

	}
}

