using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using ProgramsDatabase;
using Programs.Utils;

namespace GUIPrograms
{
	/// <summary>
	/// Summary description for FileEditor.
	/// </summary>
	public class FileEditor : System.Windows.Forms.Form
	{

		private AppItem m_CurApp;
		private ProgramConditionChecker m_Checker;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.GroupBox gbFileList;
		private System.Windows.Forms.ListBox lbFiles;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnNew;


		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AppItem CurApp
		{
			get{ return m_CurApp; }
			set{ m_CurApp = value; }
		}

		public FileEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_Checker = new ProgramConditionChecker();

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
			this.btnOk = new System.Windows.Forms.Button();
			this.gbFileList = new System.Windows.Forms.GroupBox();
			this.lbFiles = new System.Windows.Forms.ListBox();
			this.btnEdit = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnNew = new System.Windows.Forms.Button();
			this.gbFileList.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(360, 380);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(80, 24);
			this.btnOk.TabIndex = 0;
			this.btnOk.Text = "OK";
			// 
			// gbFileList
			// 
			this.gbFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.gbFileList.Controls.Add(this.btnEdit);
			this.gbFileList.Controls.Add(this.btnDelete);
			this.gbFileList.Controls.Add(this.btnNew);
			this.gbFileList.Controls.Add(this.lbFiles);
			this.gbFileList.Location = new System.Drawing.Point(8, 8);
			this.gbFileList.Name = "gbFileList";
			this.gbFileList.Size = new System.Drawing.Size(440, 364);
			this.gbFileList.TabIndex = 1;
			this.gbFileList.TabStop = false;
			// 
			// lbFiles
			// 
			this.lbFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lbFiles.Location = new System.Drawing.Point(8, 16);
			this.lbFiles.Name = "lbFiles";
			this.lbFiles.Size = new System.Drawing.Size(336, 342);
			this.lbFiles.TabIndex = 0;
			this.lbFiles.DoubleClick += new System.EventHandler(this.lbFiles_DoubleClick);
			// 
			// btnEdit
			// 
			this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnEdit.Location = new System.Drawing.Point(352, 40);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.TabIndex = 6;
			this.btnEdit.Text = "&Edit...";
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDelete.Location = new System.Drawing.Point(352, 96);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.TabIndex = 5;
			this.btnDelete.Text = "&Delete...";
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnNew
			// 
			this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNew.Location = new System.Drawing.Point(352, 16);
			this.btnNew.Name = "btnNew";
			this.btnNew.TabIndex = 4;
			this.btnNew.Text = "&New...";
			this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
			// 
			// FileEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 414);
			this.Controls.Add(this.gbFileList);
			this.Controls.Add(this.btnOk);
			this.MinimizeBox = false;
			this.Name = "FileEditor";
			this.Text = "Filelist";
			this.Load += new System.EventHandler(this.FileEditor_Load);
			this.gbFileList.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		private void updateList()
		{
			int nPrevIndex = lbFiles.SelectedIndex;
			this.gbFileList.Text = CurApp.Title;
			lbFiles.BeginUpdate();
			try
			{
				lbFiles.Items.Clear();
				foreach( FileItem file in CurApp.Files )
				{
					lbFiles.Items.Add( file.Title );
				}
			}
			finally
			{
				lbFiles.EndUpdate();
				if (nPrevIndex != -1)
				{
					if (nPrevIndex <= lbFiles.Items.Count - 1)
					{
						lbFiles.SelectedIndex = nPrevIndex;
					}
					else
					{
						lbFiles.SelectedIndex = lbFiles.Items.Count - 1;
					}
				}
			}
		}

		private void FileEditor_Load(object sender, System.EventArgs e)
		{
			updateList();
		}

		private void lbFiles_DoubleClick(object sender, System.EventArgs e)
		{
			EditFile();
		}

		private void btnNew_Click(object sender, System.EventArgs e)
		{
			NewFile();
		}

		private void btnEdit_Click(object sender, System.EventArgs e)
		{
			EditFile();
		}

		private void btnDelete_Click(object sender, System.EventArgs e)
		{
			DeleteFile();
		}

		private void NewFile()
		{
			FileItem file = new FileItem(CurApp.db);
			file.AppID = CurApp.AppID; // CRUCIAL!! :-)
			FileDetailsForm frmFileDetails = new FileDetailsForm();
			frmFileDetails.CurApp = this.CurApp;
			frmFileDetails.CurFile = file;
			DialogResult dialogResult = frmFileDetails.ShowDialog( this );
			if (dialogResult == DialogResult.OK)
			{
				file.Write();
				CurApp.Files.Load(CurApp.AppID);
				updateList();
			}
		}
		
		private void EditFile()
		{
			if (lbFiles.SelectedIndex >= 0)
			{
				FileItem file = (FileItem)CurApp.Files[lbFiles.SelectedIndex];
				if (file != null)
				{
					FileDetailsForm frmFileDetails = new FileDetailsForm();
					frmFileDetails.CurApp = this.CurApp;
					frmFileDetails.CurFile = file;
					DialogResult dialogResult = frmFileDetails.ShowDialog( this );
					if (dialogResult == DialogResult.OK)
					{
						file.Write();
						CurApp.Files.Load(CurApp.AppID);
						updateList();
					}
				}

			}
		}
		
		private void DeleteFile()
		{
			DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this file item?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
			{
				if (lbFiles.SelectedIndex >= 0)
				{
					FileItem file = (FileItem)CurApp.Files[lbFiles.SelectedIndex];
					if (file != null)
					{
						file.Delete();
					}
				    file = null;
					CurApp.Files.Load(CurApp.AppID);
					updateList();
				}
			}
		}
	}
}
