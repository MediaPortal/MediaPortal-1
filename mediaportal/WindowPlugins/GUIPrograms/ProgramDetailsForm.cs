using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using MediaPortal.GUI.Library;
using ProgramsDatabase;
using Programs.Utils;

namespace GUIPrograms
{
	/// <summary>
	/// Summary description for SetupForum.
	/// </summary>
	public class ProgramShareDetailsForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog3;
		private System.Windows.Forms.OpenFileDialog openFileDialog2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.OpenFileDialog openFileDialog3;

		private AppItem m_NewApp;
		private System.Windows.Forms.GroupBox gbApp;
		private System.Windows.Forms.CheckBox chkbEnabled;
		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.TextBox txtTitle;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblFilename;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label lblImageFile;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.TextBox txtImageFile;
		private System.Windows.Forms.CheckBox chkbUseShellExecute;
		private System.Windows.Forms.CheckBox chkbUseQuotes;
		private System.Windows.Forms.Button btnStartup;
		private System.Windows.Forms.TextBox txtStartupDir;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbWindowStyle;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtArguments;
		private System.Windows.Forms.Label lblArg;
		private System.Windows.Forms.ComboBox cbSourceType;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.GroupBox gbFiles;
		private System.Windows.Forms.TabControl tcFiles;
		private System.Windows.Forms.TabPage tpBrowse;
		private System.Windows.Forms.TabPage tpImport;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TextBox txtExtensions;
		private System.Windows.Forms.TextBox txtFiles;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox chkbValidImagesOnly;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.TextBox txtSource;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtImageDirs;
		private System.Windows.Forms.Label lblImgDirectories;
		private System.Windows.Forms.Button btnImageDirs;
		private ProgramConditionChecker m_Checker;

		public AppItem NewApp
		{
			get{ return m_NewApp; }
			set{ m_NewApp = value; }
		}
	

		public ProgramShareDetailsForm()
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
			this.button1 = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.folderBrowserDialog2 = new System.Windows.Forms.FolderBrowserDialog();
			this.folderBrowserDialog3 = new System.Windows.Forms.FolderBrowserDialog();
			this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
			this.btnOk = new System.Windows.Forms.Button();
			this.openFileDialog3 = new System.Windows.Forms.OpenFileDialog();
			this.gbApp = new System.Windows.Forms.GroupBox();
			this.cbSourceType = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.chkbUseShellExecute = new System.Windows.Forms.CheckBox();
			this.chkbUseQuotes = new System.Windows.Forms.CheckBox();
			this.btnStartup = new System.Windows.Forms.Button();
			this.txtStartupDir = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbWindowStyle = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtArguments = new System.Windows.Forms.TextBox();
			this.lblArg = new System.Windows.Forms.Label();
			this.lblImageFile = new System.Windows.Forms.Label();
			this.button6 = new System.Windows.Forms.Button();
			this.txtImageFile = new System.Windows.Forms.TextBox();
			this.chkbEnabled = new System.Windows.Forms.CheckBox();
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.txtTitle = new System.Windows.Forms.TextBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblFilename = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.gbFiles = new System.Windows.Forms.GroupBox();
			this.txtImageDirs = new System.Windows.Forms.TextBox();
			this.btnImageDirs = new System.Windows.Forms.Button();
			this.lblImgDirectories = new System.Windows.Forms.Label();
			this.tcFiles = new System.Windows.Forms.TabControl();
			this.tpBrowse = new System.Windows.Forms.TabPage();
			this.button3 = new System.Windows.Forms.Button();
			this.txtExtensions = new System.Windows.Forms.TextBox();
			this.txtFiles = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tpImport = new System.Windows.Forms.TabPage();
			this.chkbValidImagesOnly = new System.Windows.Forms.CheckBox();
			this.button5 = new System.Windows.Forms.Button();
			this.txtSource = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.gbApp.SuspendLayout();
			this.gbFiles.SuspendLayout();
			this.tcFiles.SuspendLayout();
			this.tpBrowse.SuspendLayout();
			this.tpImport.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(264, 488);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 23);
			this.button1.TabIndex = 20;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOk.Location = new System.Drawing.Point(352, 488);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 21;
			this.btnOk.Text = "Cancel";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// gbApp
			// 
			this.gbApp.Controls.Add(this.cbSourceType);
			this.gbApp.Controls.Add(this.label7);
			this.gbApp.Controls.Add(this.chkbUseShellExecute);
			this.gbApp.Controls.Add(this.chkbUseQuotes);
			this.gbApp.Controls.Add(this.btnStartup);
			this.gbApp.Controls.Add(this.txtStartupDir);
			this.gbApp.Controls.Add(this.label5);
			this.gbApp.Controls.Add(this.cbWindowStyle);
			this.gbApp.Controls.Add(this.label1);
			this.gbApp.Controls.Add(this.txtArguments);
			this.gbApp.Controls.Add(this.lblArg);
			this.gbApp.Controls.Add(this.lblImageFile);
			this.gbApp.Controls.Add(this.button6);
			this.gbApp.Controls.Add(this.txtImageFile);
			this.gbApp.Controls.Add(this.chkbEnabled);
			this.gbApp.Controls.Add(this.txtFilename);
			this.gbApp.Controls.Add(this.txtTitle);
			this.gbApp.Controls.Add(this.lblTitle);
			this.gbApp.Controls.Add(this.lblFilename);
			this.gbApp.Controls.Add(this.button2);
			this.gbApp.Location = new System.Drawing.Point(8, 8);
			this.gbApp.Name = "gbApp";
			this.gbApp.Size = new System.Drawing.Size(424, 272);
			this.gbApp.TabIndex = 0;
			this.gbApp.TabStop = false;
			this.gbApp.Text = "Application";
			// 
			// cbSourceType
			// 
			this.cbSourceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbSourceType.Items.AddRange(new object[] {
															  "Directory-Browse",
															  "Directory-Cache",
															  "MyGames Meedio output (*.mlf)",
															  "MyGames myHTPC output (*.my)",
															  "Filelauncher (manually edited fileitems)"});
			this.cbSourceType.Location = new System.Drawing.Point(128, 240);
			this.cbSourceType.Name = "cbSourceType";
			this.cbSourceType.Size = new System.Drawing.Size(250, 21);
			this.cbSourceType.TabIndex = 11;
			this.cbSourceType.SelectedIndexChanged += new System.EventHandler(this.cbSourceType_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 242);
			this.label7.Name = "label7";
			this.label7.TabIndex = 50;
			this.label7.Text = "Source-Type:";
			// 
			// chkbUseShellExecute
			// 
			this.chkbUseShellExecute.Location = new System.Drawing.Point(128, 216);
			this.chkbUseShellExecute.Name = "chkbUseShellExecute";
			this.chkbUseShellExecute.Size = new System.Drawing.Size(176, 24);
			this.chkbUseShellExecute.TabIndex = 10;
			this.chkbUseShellExecute.Text = "Startup using ShellExecute";
			// 
			// chkbUseQuotes
			// 
			this.chkbUseQuotes.Checked = true;
			this.chkbUseQuotes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkbUseQuotes.Location = new System.Drawing.Point(128, 192);
			this.chkbUseQuotes.Name = "chkbUseQuotes";
			this.chkbUseQuotes.Size = new System.Drawing.Size(184, 24);
			this.chkbUseQuotes.TabIndex = 9;
			this.chkbUseQuotes.Text = "Quotes around Filenames";
			// 
			// btnStartup
			// 
			this.btnStartup.Location = new System.Drawing.Point(392, 168);
			this.btnStartup.Name = "btnStartup";
			this.btnStartup.Size = new System.Drawing.Size(20, 20);
			this.btnStartup.TabIndex = 8;
			this.btnStartup.Text = "...";
			this.btnStartup.Click += new System.EventHandler(this.btnStartup_Click_1);
			// 
			// txtStartupDir
			// 
			this.txtStartupDir.Location = new System.Drawing.Point(128, 168);
			this.txtStartupDir.Name = "txtStartupDir";
			this.txtStartupDir.Size = new System.Drawing.Size(250, 20);
			this.txtStartupDir.TabIndex = 7;
			this.txtStartupDir.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 171);
			this.label5.Name = "label5";
			this.label5.TabIndex = 48;
			this.label5.Text = "Startup Directory:";
			// 
			// cbWindowStyle
			// 
			this.cbWindowStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbWindowStyle.Items.AddRange(new object[] {
															   "Normal",
															   "Minimized",
															   "Maximized",
															   "Hidden"});
			this.cbWindowStyle.Location = new System.Drawing.Point(128, 144);
			this.cbWindowStyle.Name = "cbWindowStyle";
			this.cbWindowStyle.Size = new System.Drawing.Size(250, 21);
			this.cbWindowStyle.TabIndex = 6;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 148);
			this.label1.Name = "label1";
			this.label1.TabIndex = 47;
			this.label1.Text = "Window-Style:";
			// 
			// txtArguments
			// 
			this.txtArguments.Location = new System.Drawing.Point(128, 120);
			this.txtArguments.Name = "txtArguments";
			this.txtArguments.Size = new System.Drawing.Size(250, 20);
			this.txtArguments.TabIndex = 5;
			this.txtArguments.Text = "";
			// 
			// lblArg
			// 
			this.lblArg.Location = new System.Drawing.Point(8, 124);
			this.lblArg.Name = "lblArg";
			this.lblArg.TabIndex = 46;
			this.lblArg.Text = "Arguments:";
			// 
			// lblImageFile
			// 
			this.lblImageFile.Location = new System.Drawing.Point(8, 100);
			this.lblImageFile.Name = "lblImageFile";
			this.lblImageFile.Size = new System.Drawing.Size(80, 20);
			this.lblImageFile.TabIndex = 39;
			this.lblImageFile.Text = "Imagefile:";
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(392, 96);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(20, 20);
			this.button6.TabIndex = 4;
			this.button6.Text = "...";
			this.button6.Click += new System.EventHandler(this.button6_Click_1);
			// 
			// txtImageFile
			// 
			this.txtImageFile.Location = new System.Drawing.Point(128, 96);
			this.txtImageFile.Name = "txtImageFile";
			this.txtImageFile.Size = new System.Drawing.Size(250, 20);
			this.txtImageFile.TabIndex = 3;
			this.txtImageFile.Text = "";
			// 
			// chkbEnabled
			// 
			this.chkbEnabled.Checked = true;
			this.chkbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkbEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkbEnabled.Location = new System.Drawing.Point(16, 16);
			this.chkbEnabled.Name = "chkbEnabled";
			this.chkbEnabled.TabIndex = 12;
			this.chkbEnabled.Text = "Enabled";
			// 
			// txtFilename
			// 
			this.txtFilename.Location = new System.Drawing.Point(128, 72);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(250, 20);
			this.txtFilename.TabIndex = 1;
			this.txtFilename.Text = "";
			// 
			// txtTitle
			// 
			this.txtTitle.Location = new System.Drawing.Point(128, 48);
			this.txtTitle.Name = "txtTitle";
			this.txtTitle.Size = new System.Drawing.Size(250, 20);
			this.txtTitle.TabIndex = 0;
			this.txtTitle.Text = "";
			// 
			// lblTitle
			// 
			this.lblTitle.Location = new System.Drawing.Point(8, 53);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.TabIndex = 35;
			this.lblTitle.Text = "Title:";
			// 
			// lblFilename
			// 
			this.lblFilename.Location = new System.Drawing.Point(8, 76);
			this.lblFilename.Name = "lblFilename";
			this.lblFilename.Size = new System.Drawing.Size(120, 20);
			this.lblFilename.TabIndex = 33;
			this.lblFilename.Text = "Launching Application:";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(392, 72);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(20, 20);
			this.button2.TabIndex = 2;
			this.button2.Text = "...";
			this.button2.Click += new System.EventHandler(this.button2_Click_1);
			// 
			// gbFiles
			// 
			this.gbFiles.Controls.Add(this.txtImageDirs);
			this.gbFiles.Controls.Add(this.btnImageDirs);
			this.gbFiles.Controls.Add(this.lblImgDirectories);
			this.gbFiles.Controls.Add(this.tcFiles);
			this.gbFiles.Location = new System.Drawing.Point(8, 288);
			this.gbFiles.Name = "gbFiles";
			this.gbFiles.Size = new System.Drawing.Size(424, 192);
			this.gbFiles.TabIndex = 23;
			this.gbFiles.TabStop = false;
			this.gbFiles.Text = "Files";
			// 
			// txtImageDirs
			// 
			this.txtImageDirs.Location = new System.Drawing.Point(123, 120);
			this.txtImageDirs.Multiline = true;
			this.txtImageDirs.Name = "txtImageDirs";
			this.txtImageDirs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtImageDirs.Size = new System.Drawing.Size(250, 64);
			this.txtImageDirs.TabIndex = 1;
			this.txtImageDirs.Text = "txtImageDirs";
			// 
			// btnImageDirs
			// 
			this.btnImageDirs.Location = new System.Drawing.Point(388, 120);
			this.btnImageDirs.Name = "btnImageDirs";
			this.btnImageDirs.Size = new System.Drawing.Size(20, 20);
			this.btnImageDirs.TabIndex = 2;
			this.btnImageDirs.Text = "...";
			this.btnImageDirs.Click += new System.EventHandler(this.btnImageClick);
			// 
			// lblImgDirectories
			// 
			this.lblImgDirectories.Location = new System.Drawing.Point(11, 120);
			this.lblImgDirectories.Name = "lblImgDirectories";
			this.lblImgDirectories.TabIndex = 39;
			this.lblImgDirectories.Text = "Image directories:";
			// 
			// tcFiles
			// 
			this.tcFiles.Controls.Add(this.tpBrowse);
			this.tcFiles.Controls.Add(this.tpImport);
			this.tcFiles.Location = new System.Drawing.Point(8, 16);
			this.tcFiles.Name = "tcFiles";
			this.tcFiles.SelectedIndex = 0;
			this.tcFiles.Size = new System.Drawing.Size(408, 96);
			this.tcFiles.TabIndex = 0;
			// 
			// tpBrowse
			// 
			this.tpBrowse.Controls.Add(this.button3);
			this.tpBrowse.Controls.Add(this.txtExtensions);
			this.tpBrowse.Controls.Add(this.txtFiles);
			this.tpBrowse.Controls.Add(this.label4);
			this.tpBrowse.Controls.Add(this.label2);
			this.tpBrowse.Location = new System.Drawing.Point(4, 22);
			this.tpBrowse.Name = "tpBrowse";
			this.tpBrowse.Size = new System.Drawing.Size(400, 70);
			this.tpBrowse.TabIndex = 0;
			this.tpBrowse.Text = "Browse";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(373, 11);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(20, 20);
			this.button3.TabIndex = 1;
			this.button3.Text = "...";
			this.button3.Click += new System.EventHandler(this.button3_Click_1);
			// 
			// txtExtensions
			// 
			this.txtExtensions.Location = new System.Drawing.Point(115, 40);
			this.txtExtensions.Name = "txtExtensions";
			this.txtExtensions.Size = new System.Drawing.Size(250, 20);
			this.txtExtensions.TabIndex = 2;
			this.txtExtensions.Text = "";
			// 
			// txtFiles
			// 
			this.txtFiles.Location = new System.Drawing.Point(115, 12);
			this.txtFiles.Name = "txtFiles";
			this.txtFiles.Size = new System.Drawing.Size(250, 20);
			this.txtFiles.TabIndex = 0;
			this.txtFiles.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 40);
			this.label4.Name = "label4";
			this.label4.TabIndex = 18;
			this.label4.Text = "File-Extensions:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 12);
			this.label2.Name = "label2";
			this.label2.TabIndex = 16;
			this.label2.Text = "File Directory:";
			// 
			// tpImport
			// 
			this.tpImport.Controls.Add(this.chkbValidImagesOnly);
			this.tpImport.Controls.Add(this.button5);
			this.tpImport.Controls.Add(this.txtSource);
			this.tpImport.Controls.Add(this.label6);
			this.tpImport.Location = new System.Drawing.Point(4, 22);
			this.tpImport.Name = "tpImport";
			this.tpImport.Size = new System.Drawing.Size(400, 70);
			this.tpImport.TabIndex = 1;
			this.tpImport.Text = "Import";
			// 
			// chkbValidImagesOnly
			// 
			this.chkbValidImagesOnly.Location = new System.Drawing.Point(112, 36);
			this.chkbValidImagesOnly.Name = "chkbValidImagesOnly";
			this.chkbValidImagesOnly.Size = new System.Drawing.Size(224, 24);
			this.chkbValidImagesOnly.TabIndex = 2;
			this.chkbValidImagesOnly.Text = "Only import files with valid images";
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(376, 12);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(20, 20);
			this.button5.TabIndex = 1;
			this.button5.Text = "...";
			this.button5.Click += new System.EventHandler(this.button5_Click_2);
			// 
			// txtSource
			// 
			this.txtSource.Location = new System.Drawing.Point(112, 12);
			this.txtSource.Name = "txtSource";
			this.txtSource.Size = new System.Drawing.Size(250, 20);
			this.txtSource.TabIndex = 0;
			this.txtSource.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 14);
			this.label6.Name = "label6";
			this.label6.TabIndex = 34;
			this.label6.Text = "Source:";
			// 
			// ProgramShareDetailsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(442, 520);
			this.Controls.Add(this.gbFiles);
			this.Controls.Add(this.gbApp);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ProgramShareDetailsForm";
			this.Text = "Program Details";
			this.Load += new System.EventHandler(this.SetupForum_Load);
			this.gbApp.ResumeLayout(false);
			this.gbFiles.ResumeLayout(false);
			this.tcFiles.ResumeLayout(false);
			this.tpBrowse.ResumeLayout(false);
			this.tpImport.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void SetWindowStyle(ProcessWindowStyle val) 
		{
			switch (val)
			{
				case ProcessWindowStyle.Normal:
					cbWindowStyle.SelectedIndex = 0;
					break;
				case ProcessWindowStyle.Minimized:
					cbWindowStyle.SelectedIndex = 1;
					break;
				case ProcessWindowStyle.Maximized:
					cbWindowStyle.SelectedIndex = 2;
					break;
				case ProcessWindowStyle.Hidden:
					cbWindowStyle.SelectedIndex = 3;
					break;
			}

		}
		
		private ProcessWindowStyle GetSelectedWindowStyle()
		{
			if (this.cbWindowStyle.SelectedIndex == 1)
			{
				return ProcessWindowStyle.Minimized;
			}
			else if (this.cbWindowStyle.SelectedIndex == 2)
			{
				return ProcessWindowStyle.Maximized;
			} 
			else if (this.cbWindowStyle.SelectedIndex == 3)
			{
				return ProcessWindowStyle.Hidden;
			}
			else
				return ProcessWindowStyle.Normal;

		}

		private void SetSourceType(myProgSourceType val)
		{
			switch (val)
			{
				case myProgSourceType.DIRBROWSE:
					cbSourceType.SelectedIndex = 0;
					break;
				case myProgSourceType.DIRCACHE:
					cbSourceType.SelectedIndex = 1;
					break;
				case myProgSourceType.MYFILEMEEDIO:
					cbSourceType.SelectedIndex = 2;
					break;
				case myProgSourceType.MYFILEINI:
					cbSourceType.SelectedIndex = 3;
					break;
				case myProgSourceType.FILELAUNCHER:
					cbSourceType.SelectedIndex = 4;
					break;
//				case myProgSourceType.MYGAMESDIRECT:
//					cbSourceType.SelectedIndex = 3;
//					break;
			}

		}

		private myProgSourceType GetSelectedSourceType()
		{
			//Directory (Browse-Mode)
			//Directory (DBCache-Mode)
			//MyGames myHTPC output 
			//MyGames Meedio output
			//MyGames Direct-Link
			if (this.cbSourceType.SelectedIndex == 0)
			{
				return myProgSourceType.DIRBROWSE;
			}
			else if (this.cbSourceType.SelectedIndex == 1)
			{
				return myProgSourceType.DIRCACHE;
			} 
			else if (this.cbSourceType.SelectedIndex == 2)
			{
				return myProgSourceType.MYFILEMEEDIO;
			}
			else if (this.cbSourceType.SelectedIndex == 3)
			{
				return myProgSourceType.MYFILEINI;
			}
			else if (cbSourceType.SelectedIndex == 4)
			{
				return myProgSourceType.FILELAUNCHER;
			}
			//			else if (this.cbSourceType.SelectedIndex == 5)
			//			{
			//				return myProgSourceType.MYGAMESDIRECT;
			//			}
			else
				return myProgSourceType.UNKNOWN;
		}

		private bool EntriesOK()
		{
			m_Checker.Clear();
			m_Checker.DoCheck(NewApp.Title != "", "No title entered!");
			if (NewApp.Filename == "") 
			{
				m_Checker.DoCheck(NewApp.UseShellExecute, "No launching filename entered!");
			}
			switch (NewApp.SourceType)
			{
				case myProgSourceType.UNKNOWN:
					m_Checker.DoCheck(false, "No sourcetype entered!");
					break;
				case myProgSourceType.MYFILEMEEDIO:
					m_Checker.DoCheck(NewApp.Source != "", "No sourcefile entered!");
					break;
				case myProgSourceType.MYFILEINI:
					m_Checker.DoCheck(NewApp.Source != "", "No sourcefile entered!");
					break;
				case myProgSourceType.MYGAMESDIRECT:
					break;
				case myProgSourceType.FILELAUNCHER:
					break;
				case myProgSourceType.DIRBROWSE:
					m_Checker.DoCheck(NewApp.FileDirectory != "", "No filedirectory entered!");
					m_Checker.DoCheck(NewApp.ValidExtensions != "", "No file extensions entered!");
					break;
				case myProgSourceType.DIRCACHE:
					m_Checker.DoCheck(NewApp.FileDirectory != "", "No filedirectory entered!");
					m_Checker.DoCheck(NewApp.ValidExtensions != "", "No file extensions entered!");
					break;
			}
									

			if (!m_Checker.IsOk)
			{
				System.Windows.Forms.MessageBox.Show(m_Checker.Problems);
			}
			return m_Checker.IsOk;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			NewApp.Enabled = this.chkbEnabled.Checked;
			NewApp.Title = this.txtTitle.Text;
			NewApp.Filename = this.txtFilename.Text;
			NewApp.Arguments = this.txtArguments.Text;
			NewApp.WindowStyle = GetSelectedWindowStyle();
			NewApp.Startupdir = this.txtStartupDir.Text;
			NewApp.UseShellExecute = (this.chkbUseShellExecute.Checked);
			NewApp.UseQuotes = (this.chkbUseQuotes.Checked);
			NewApp.SourceType = GetSelectedSourceType();
			NewApp.Source = this.txtSource.Text;
			NewApp.Imagefile = this.txtImageFile.Text;
			NewApp.FileDirectory = this.txtFiles.Text;
			NewApp.ImageDirectory = this.txtImageDirs.Text;
			NewApp.ValidExtensions = this.txtExtensions.Text;
			NewApp.ImportValidImagesOnly = this.chkbValidImagesOnly.Checked;
			if (EntriesOK())
			{
				this.DialogResult = DialogResult.OK; 
				this.Close();
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.openFileDialog1.RestoreDirectory = true;
			if( this.openFileDialog1.ShowDialog(null) == DialogResult.OK )
			{
				this.txtFilename.Text = this.openFileDialog1.FileName;
			}
		}

		private void SetupForum_Load(object sender, System.EventArgs e)
		{
			this.chkbEnabled.Checked = m_NewApp.Enabled;
			this.txtTitle.Text = m_NewApp.Title;
			this.txtFilename.Text = m_NewApp.Filename;
			this.txtArguments.Text = m_NewApp.Arguments;
			SetWindowStyle(m_NewApp.WindowStyle);
			this.txtStartupDir.Text = m_NewApp.Startupdir;
			this.chkbUseShellExecute.Checked = (m_NewApp.UseShellExecute);
			this.chkbUseQuotes.Checked = (m_NewApp.UseQuotes);
			SetSourceType(m_NewApp.SourceType);
			this.txtSource.Text = m_NewApp.Source;
			this.txtImageFile.Text = m_NewApp.Imagefile;
			this.txtFiles.Text = m_NewApp.FileDirectory;
			this.txtImageDirs.Text = m_NewApp.ImageDirectory;
			this.txtExtensions.Text = m_NewApp.ValidExtensions;
			this.chkbValidImagesOnly.Checked = m_NewApp.ImportValidImagesOnly;
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			if( this.folderBrowserDialog1.ShowDialog( null ) == DialogResult.OK )
			{
				this.txtFiles.Text = this.folderBrowserDialog1.SelectedPath;
			}
		}


		private void btnStartup_Click(object sender, System.EventArgs e)
		{
			if( this.folderBrowserDialog3.ShowDialog( null ) == DialogResult.OK )
			{
				this.txtStartupDir.Text = this.folderBrowserDialog3.SelectedPath;
			}
		
		}

		private void button5_Click(object sender, System.EventArgs e)
		{
			this.txtStartupDir.Text = "";
		}

		private void button6_Click(object sender, System.EventArgs e)
		{
			this.openFileDialog2.RestoreDirectory = true;
			if( this.openFileDialog2.ShowDialog(null) == DialogResult.OK )
			{
				this.txtImageFile.Text = this.openFileDialog2.FileName;
			}
		}

		private void button7_Click(object sender, System.EventArgs e)
		{
			this.txtImageFile.Text = "";
		}


		private void button5_Click_1(object sender, System.EventArgs e)
		{
			this.openFileDialog3.RestoreDirectory = true;
			if( this.openFileDialog3.ShowDialog(null) == DialogResult.OK )
			{
				this.txtSource.Text = this.openFileDialog3.FileName;
			}
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel; 		
			this.Close();
		}

		private void cbSourceType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// if sourcetype changes activate the corresponding tab
			if (this.cbSourceType.SelectedIndex == 0)
			{
				tcFiles.SelectedTab = tpBrowse;
			}
			else if (this.cbSourceType.SelectedIndex == 1)
			{
				tcFiles.SelectedTab = tpBrowse;
			}
			else if (this.cbSourceType.SelectedIndex == 2)
			{
				tcFiles.SelectedTab = tpImport;
			}
			else if (this.cbSourceType.SelectedIndex == 3)
			{
				tcFiles.SelectedTab = tpImport;
			}
			else
			{
				tcFiles.SelectedTab = tpBrowse; // is not really necessary....
			}

		}

		private void button2_Click_1(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = txtFilename.Text;
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog(null) == DialogResult.OK )
			{
				txtFilename.Text = openFileDialog1.FileName;
			}
		}

		private void button6_Click_1(object sender, System.EventArgs e)
		{
			openFileDialog2.FileName = txtImageFile.Text;
			this.openFileDialog2.RestoreDirectory = true;
			if( this.openFileDialog2.ShowDialog(null) == DialogResult.OK )
			{
				this.txtImageFile.Text = this.openFileDialog2.FileName;
			}
		}

		private void btnStartup_Click_1(object sender, System.EventArgs e)
		{
			folderBrowserDialog3.SelectedPath = txtStartupDir.Text;
			if( this.folderBrowserDialog3.ShowDialog( null ) == DialogResult.OK )
			{
				this.txtStartupDir.Text = this.folderBrowserDialog3.SelectedPath;
			}
		}

		private void button5_Click_2(object sender, System.EventArgs e)
		{
			openFileDialog3.FileName = txtSource.Text;
			this.openFileDialog3.RestoreDirectory = true;
			if( this.openFileDialog3.ShowDialog(null) == DialogResult.OK )
			{
				this.txtSource.Text = this.openFileDialog3.FileName;
			}
		}

		private void button3_Click_1(object sender, System.EventArgs e)
		{
			folderBrowserDialog1.SelectedPath = txtFiles.Text;
			if( this.folderBrowserDialog1.ShowDialog( null ) == DialogResult.OK )
			{
				this.txtFiles.Text = this.folderBrowserDialog1.SelectedPath;
			}
		}

		private void btnImageClick(object sender, System.EventArgs e)
		{
			if (txtImageDirs.Text != "")
			{
				folderBrowserDialog2.SelectedPath = txtImageDirs.Lines[0];
			}
			if( this.folderBrowserDialog2.ShowDialog( null ) == DialogResult.OK )
			{
				txtImageDirs.Text = txtImageDirs.Text + "\r\n" + folderBrowserDialog2.SelectedPath;
			}
		}


	}
}
