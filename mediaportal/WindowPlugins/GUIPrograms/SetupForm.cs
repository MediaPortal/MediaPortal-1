using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using ProgramsDatabase;

namespace GUIPrograms
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form, ISetupForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.GroupBox gbApps;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnNew;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnUp;
		private System.Windows.Forms.Button btnDown;
		private System.Windows.Forms.CheckedListBox lbApps;
		private System.Windows.Forms.Button btnFiles;

		private Applist apps = ProgamDatabase.AppList;

		public SetupForm()
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

		public bool	CanEnable()		// Indicates whether plugin can be enabled/disabled
		{
			return true;
		}
		
    public bool HasSetup()
    {
      return true;
    }
		public int GetWindowId()
		{
			return (int)GUIWindow.Window.WINDOW_FILES;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText=GUILocalizeStrings.Get(0);;
			strButtonImage="";
			strButtonImageFocus="";
			strPictureImage="";
			return true;
		}

		public string PluginName() 
		{
			return "My Programs";
		}
		public string Description()
		{
			return "A Program Launching Plugin";
		}
		public string Author()
		{
			return "waeberd/Domi_Fan";
		}
		public void ShowPlugin()
		{
			ShowDialog();
		}
		public bool DefaultEnabled()
		{
			return false;
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.gbApps = new System.Windows.Forms.GroupBox();
			this.btnFiles = new System.Windows.Forms.Button();
			this.lbApps = new System.Windows.Forms.CheckedListBox();
			this.btnDown = new System.Windows.Forms.Button();
			this.btnUp = new System.Windows.Forms.Button();
			this.btnEdit = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnNew = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.gbApps.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbApps
			// 
			this.gbApps.Controls.Add(this.btnFiles);
			this.gbApps.Controls.Add(this.lbApps);
			this.gbApps.Controls.Add(this.btnDown);
			this.gbApps.Controls.Add(this.btnUp);
			this.gbApps.Controls.Add(this.btnEdit);
			this.gbApps.Controls.Add(this.btnDelete);
			this.gbApps.Controls.Add(this.btnNew);
			this.gbApps.Location = new System.Drawing.Point(8, 8);
			this.gbApps.Name = "gbApps";
			this.gbApps.Size = new System.Drawing.Size(312, 272);
			this.gbApps.TabIndex = 0;
			this.gbApps.TabStop = false;
			this.gbApps.Text = "Applications";
			// 
			// btnFiles
			// 
			this.btnFiles.Location = new System.Drawing.Point(224, 168);
			this.btnFiles.Name = "btnFiles";
			this.btnFiles.TabIndex = 7;
			this.btnFiles.Text = "&Files...";
			this.btnFiles.Click += new System.EventHandler(this.btnFiles_Click);
			// 
			// lbApps
			// 
			this.lbApps.Location = new System.Drawing.Point(8, 16);
			this.lbApps.Name = "lbApps";
			this.lbApps.Size = new System.Drawing.Size(208, 244);
			this.lbApps.TabIndex = 6;
			this.lbApps.DoubleClick += new System.EventHandler(this.lbApps_DoubleClick_1);
			this.lbApps.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbApps_ItemCheck);
			// 
			// btnDown
			// 
			this.btnDown.Location = new System.Drawing.Point(224, 128);
			this.btnDown.Name = "btnDown";
			this.btnDown.TabIndex = 5;
			this.btnDown.Text = "&Down";
			this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
			// 
			// btnUp
			// 
			this.btnUp.Location = new System.Drawing.Point(224, 104);
			this.btnUp.Name = "btnUp";
			this.btnUp.TabIndex = 4;
			this.btnUp.Text = "&Up";
			this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
			// 
			// btnEdit
			// 
			this.btnEdit.Location = new System.Drawing.Point(224, 40);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.TabIndex = 3;
			this.btnEdit.Text = "&Edit...";
			this.btnEdit.Click += new System.EventHandler(this.button4_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Location = new System.Drawing.Point(224, 64);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.TabIndex = 2;
			this.btnDelete.Text = "&Delete...";
			this.btnDelete.Click += new System.EventHandler(this.button3_Click);
			// 
			// btnNew
			// 
			this.btnNew.Location = new System.Drawing.Point(224, 16);
			this.btnNew.Name = "btnNew";
			this.btnNew.TabIndex = 1;
			this.btnNew.Text = "&New...";
			this.btnNew.Click += new System.EventHandler(this.button2_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(240, 288);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.button1_Click);
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(330, 320);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.gbApps);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "SetupForm";
			this.Text = "SetupForm";
			this.Load += new System.EventHandler(this.SetupForm_Load);
			this.gbApps.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void button2_Click(object sender, System.EventArgs e)
		{
			ProgramShareDetailsForm frmDetails = new ProgramShareDetailsForm();
			frmDetails.NewApp = new AppItem(ProgamDatabase.m_db);
			DialogResult dialogResult = frmDetails.ShowDialog( this );
			if (dialogResult == DialogResult.OK)
			{
				apps.Add(frmDetails.NewApp);
				frmDetails.NewApp.Position = apps.GetMaxPosition() + 10;
				frmDetails.NewApp.Write();
				apps.LoadAll(); // poor man's refresh.... Load all and display all.....
				updateList();
			}
		}

		private void updateList()
		{
			lbApps.Items.Clear();

			foreach( AppItem app in apps )
			{
				lbApps.Items.Add( app.Title );
				lbApps.SetItemChecked(lbApps.Items.Count - 1, app.Enabled);
			}
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this application item?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if(dialogResult == DialogResult.Yes)
			{
				if (lbApps.SelectedIndex >= 0)
				{
					AppItem app = (AppItem)apps[lbApps.SelectedIndex];
					if (app != null)
					{
						app.Delete();
					}
					apps.LoadAll();
					updateList();
				}
			}
		}

		private void EditApp()
		{
			if (lbApps.SelectedIndex >= 0)
			{
				AppItem app = (AppItem)apps[lbApps.SelectedIndex];
				if (app != null)
				{
					ProgramShareDetailsForm frmDetails = new ProgramShareDetailsForm();
					frmDetails.NewApp = app;
					DialogResult dialogResult = frmDetails.ShowDialog( this );
					if (dialogResult == DialogResult.OK)
					{
						app.Write();
						updateList();
					}
				}
			}
		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			EditApp();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void SetupForm_Load(object sender, System.EventArgs e)
		{
			apps.LoadAll(); // we need all apps, whether enabled or not => reload with LoadALL
			updateList();
		}


		private void btnUp_Click(object sender, System.EventArgs e)
		{
			AppItem curApp = null;
			AppItem prevApp = null;
			int n = -1;
			if (lbApps.SelectedIndex >= 1)
			{
				curApp = (AppItem)apps[lbApps.SelectedIndex];
				prevApp = (AppItem)apps[lbApps.SelectedIndex-1];
				if ((curApp != null) && (prevApp != null))
				{
					int nOldIndex = lbApps.SelectedIndex;
					n = curApp.Position;
					curApp.Position = prevApp.Position;
					prevApp.Position = n;
					curApp.Write();
					prevApp.Write();
					apps.LoadAll(); // poor man's refresh.... Load all and display all.....
					updateList();
					lbApps.SelectedIndex = nOldIndex - 1;
				}
			}
		}

		private void btnDown_Click(object sender, System.EventArgs e)
		{
			AppItem curApp = null;
			AppItem nextApp = null;
			int n = -1;
			if (lbApps.SelectedIndex <= lbApps.Items.Count - 2)
			{
				curApp = (AppItem)apps[lbApps.SelectedIndex];
				nextApp = (AppItem)apps[lbApps.SelectedIndex+1];
				if ((curApp != null) && (nextApp != null))
				{
					int nOldIndex = lbApps.SelectedIndex;
					n = curApp.Position;
					curApp.Position = nextApp.Position;
					nextApp.Position = n;
					curApp.Write();
					nextApp.Write();
					apps.LoadAll(); // poor man's refresh.... Load all and display all.....
					updateList();
					lbApps.SelectedIndex = nOldIndex + 1;
				}
			}
		}

		private void lbApps_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (lbApps.SelectedIndex >= 0)
			{
				AppItem app = (AppItem)apps[lbApps.SelectedIndex];
				app.Enabled = (e.NewValue == CheckState.Checked);
				app.Write();
			}
		}

		private void lbApps_DoubleClick_1(object sender, System.EventArgs e)
		{
			EditApp();
		}

		private void btnFiles_Click(object sender, System.EventArgs e)
		{
			if (lbApps.SelectedIndex >= 0)
			{
				AppItem app = (AppItem)apps[lbApps.SelectedIndex];
				if (app != null)
				{
					if (app.FileEditorAllowed())
					{
						FileEditor frmFiles = new FileEditor();
						frmFiles.CurApp = app;
						frmFiles.ShowDialog( this );
					}
					else
					{
						System.Windows.Forms.MessageBox.Show("File-editing is not possible for this application (wrong mode)!");
					}
				}
			}
		}
	}
}
