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
	/// Summary description for FileDetailsForm.
	/// </summary>
	public class FileDetailsForm : System.Windows.Forms.Form
	{
		private AppItem m_CurApp;
		private FileItem m_CurFile;
		private ProgramConditionChecker m_Checker;
		private System.Windows.Forms.GroupBox gbFileDetails;
		private System.Windows.Forms.Label lblImageFile;
		private System.Windows.Forms.TextBox txtTitle;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblFilename;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button btnImageFile;
		private System.Windows.Forms.Button btnFilename;
		private System.Windows.Forms.TextBox txtGenre;
		private System.Windows.Forms.TextBox txtManufacturer;
		private System.Windows.Forms.TextBox txtYear;
		private System.Windows.Forms.TextBox txtCountry;
		private System.Windows.Forms.TextBox txtSystem;
		private System.Windows.Forms.TextBox txtOverview;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label lblOverview;
		private System.Windows.Forms.Label lblSystem;
		private System.Windows.Forms.Label lblCountry;
		private System.Windows.Forms.Label lblRating;
		private System.Windows.Forms.Label lblYear;
		private System.Windows.Forms.Label lblManufacturer;
		private System.Windows.Forms.Label lblGenre;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.TextBox txtImageFile;
		private System.Windows.Forms.ComboBox cbRating;
		private System.Windows.Forms.TextBox txtFilepath;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components;

		public AppItem CurApp
		{
			get{ return m_CurApp; }
			set{ m_CurApp = value; }
		}

		public FileItem CurFile
		{
			get{ return m_CurFile; }
			set{ m_CurFile = value; }
		}

		public FileDetailsForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_Checker = new ProgramConditionChecker();

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
			this.components = new System.ComponentModel.Container();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.gbFileDetails = new System.Windows.Forms.GroupBox();
			this.txtFilepath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbRating = new System.Windows.Forms.ComboBox();
			this.txtOverview = new System.Windows.Forms.TextBox();
			this.lblOverview = new System.Windows.Forms.Label();
			this.txtSystem = new System.Windows.Forms.TextBox();
			this.lblSystem = new System.Windows.Forms.Label();
			this.txtCountry = new System.Windows.Forms.TextBox();
			this.lblCountry = new System.Windows.Forms.Label();
			this.lblRating = new System.Windows.Forms.Label();
			this.txtYear = new System.Windows.Forms.TextBox();
			this.lblYear = new System.Windows.Forms.Label();
			this.txtManufacturer = new System.Windows.Forms.TextBox();
			this.lblManufacturer = new System.Windows.Forms.Label();
			this.txtGenre = new System.Windows.Forms.TextBox();
			this.lblGenre = new System.Windows.Forms.Label();
			this.lblImageFile = new System.Windows.Forms.Label();
			this.btnImageFile = new System.Windows.Forms.Button();
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.txtImageFile = new System.Windows.Forms.TextBox();
			this.txtTitle = new System.Windows.Forms.TextBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblFilename = new System.Windows.Forms.Label();
			this.btnFilename = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.gbFileDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(328, 456);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(80, 23);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(416, 456);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			// 
			// gbFileDetails
			// 
			this.gbFileDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.gbFileDetails.Controls.Add(this.txtFilepath);
			this.gbFileDetails.Controls.Add(this.label1);
			this.gbFileDetails.Controls.Add(this.cbRating);
			this.gbFileDetails.Controls.Add(this.txtOverview);
			this.gbFileDetails.Controls.Add(this.lblOverview);
			this.gbFileDetails.Controls.Add(this.txtSystem);
			this.gbFileDetails.Controls.Add(this.lblSystem);
			this.gbFileDetails.Controls.Add(this.txtCountry);
			this.gbFileDetails.Controls.Add(this.lblCountry);
			this.gbFileDetails.Controls.Add(this.lblRating);
			this.gbFileDetails.Controls.Add(this.txtYear);
			this.gbFileDetails.Controls.Add(this.lblYear);
			this.gbFileDetails.Controls.Add(this.txtManufacturer);
			this.gbFileDetails.Controls.Add(this.lblManufacturer);
			this.gbFileDetails.Controls.Add(this.txtGenre);
			this.gbFileDetails.Controls.Add(this.lblGenre);
			this.gbFileDetails.Controls.Add(this.lblImageFile);
			this.gbFileDetails.Controls.Add(this.btnImageFile);
			this.gbFileDetails.Controls.Add(this.txtFilename);
			this.gbFileDetails.Controls.Add(this.txtImageFile);
			this.gbFileDetails.Controls.Add(this.txtTitle);
			this.gbFileDetails.Controls.Add(this.lblTitle);
			this.gbFileDetails.Controls.Add(this.lblFilename);
			this.gbFileDetails.Controls.Add(this.btnFilename);
			this.gbFileDetails.Location = new System.Drawing.Point(8, 8);
			this.gbFileDetails.Name = "gbFileDetails";
			this.gbFileDetails.Size = new System.Drawing.Size(498, 442);
			this.gbFileDetails.TabIndex = 0;
			this.gbFileDetails.TabStop = false;
			// 
			// txtFilepath
			// 
			this.txtFilepath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilepath.Location = new System.Drawing.Point(88, 72);
			this.txtFilepath.Name = "txtFilepath";
			this.txtFilepath.ReadOnly = true;
			this.txtFilepath.Size = new System.Drawing.Size(366, 20);
			this.txtFilepath.TabIndex = 62;
			this.txtFilepath.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 74);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 63;
			this.label1.Text = "Filepath:";
			// 
			// cbRating
			// 
			this.cbRating.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbRating.Items.AddRange(new object[] {
																									"0 - poor",
																									"1",
																									"2",
																									"3",
																									"4",
																									"5 - average",
																									"6",
																									"7",
																									"8",
																									"9",
																									"10 - perfect"});
			this.cbRating.Location = new System.Drawing.Point(88, 192);
			this.cbRating.Name = "cbRating";
			this.cbRating.Size = new System.Drawing.Size(192, 21);
			this.cbRating.TabIndex = 8;
			// 
			// txtOverview
			// 
			this.txtOverview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtOverview.Location = new System.Drawing.Point(8, 288);
			this.txtOverview.Multiline = true;
			this.txtOverview.Name = "txtOverview";
			this.txtOverview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOverview.Size = new System.Drawing.Size(482, 144);
			this.txtOverview.TabIndex = 11;
			this.txtOverview.Text = "txtOverview";
			// 
			// lblOverview
			// 
			this.lblOverview.Location = new System.Drawing.Point(8, 272);
			this.lblOverview.Name = "lblOverview";
			this.lblOverview.Size = new System.Drawing.Size(100, 16);
			this.lblOverview.TabIndex = 61;
			this.lblOverview.Text = "Overview:";
			// 
			// txtSystem
			// 
			this.txtSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtSystem.Location = new System.Drawing.Point(88, 240);
			this.txtSystem.Name = "txtSystem";
			this.txtSystem.Size = new System.Drawing.Size(366, 20);
			this.txtSystem.TabIndex = 10;
			this.txtSystem.Text = "";
			// 
			// lblSystem
			// 
			this.lblSystem.Location = new System.Drawing.Point(8, 244);
			this.lblSystem.Name = "lblSystem";
			this.lblSystem.Size = new System.Drawing.Size(64, 16);
			this.lblSystem.TabIndex = 59;
			this.lblSystem.Text = "System:";
			this.lblSystem.Click += new System.EventHandler(this.label6_Click);
			// 
			// txtCountry
			// 
			this.txtCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtCountry.Location = new System.Drawing.Point(88, 216);
			this.txtCountry.Name = "txtCountry";
			this.txtCountry.Size = new System.Drawing.Size(366, 20);
			this.txtCountry.TabIndex = 9;
			this.txtCountry.Text = "";
			// 
			// lblCountry
			// 
			this.lblCountry.Location = new System.Drawing.Point(8, 220);
			this.lblCountry.Name = "lblCountry";
			this.lblCountry.Size = new System.Drawing.Size(72, 16);
			this.lblCountry.TabIndex = 57;
			this.lblCountry.Text = "Country:";
			// 
			// lblRating
			// 
			this.lblRating.Location = new System.Drawing.Point(8, 196);
			this.lblRating.Name = "lblRating";
			this.lblRating.Size = new System.Drawing.Size(72, 16);
			this.lblRating.TabIndex = 55;
			this.lblRating.Text = "Rating:";
			// 
			// txtYear
			// 
			this.txtYear.Location = new System.Drawing.Point(88, 168);
			this.txtYear.MaxLength = 4;
			this.txtYear.Name = "txtYear";
			this.txtYear.Size = new System.Drawing.Size(48, 20);
			this.txtYear.TabIndex = 7;
			this.txtYear.Text = "";
			// 
			// lblYear
			// 
			this.lblYear.Location = new System.Drawing.Point(8, 172);
			this.lblYear.Name = "lblYear";
			this.lblYear.Size = new System.Drawing.Size(64, 16);
			this.lblYear.TabIndex = 53;
			this.lblYear.Text = "Year:";
			// 
			// txtManufacturer
			// 
			this.txtManufacturer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtManufacturer.Location = new System.Drawing.Point(88, 144);
			this.txtManufacturer.Name = "txtManufacturer";
			this.txtManufacturer.Size = new System.Drawing.Size(366, 20);
			this.txtManufacturer.TabIndex = 6;
			this.txtManufacturer.Text = "";
			// 
			// lblManufacturer
			// 
			this.lblManufacturer.Location = new System.Drawing.Point(8, 148);
			this.lblManufacturer.Name = "lblManufacturer";
			this.lblManufacturer.Size = new System.Drawing.Size(80, 16);
			this.lblManufacturer.TabIndex = 51;
			this.lblManufacturer.Text = "Manufacturer:";
			// 
			// txtGenre
			// 
			this.txtGenre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtGenre.Location = new System.Drawing.Point(88, 120);
			this.txtGenre.Name = "txtGenre";
			this.txtGenre.Size = new System.Drawing.Size(366, 20);
			this.txtGenre.TabIndex = 5;
			this.txtGenre.Text = "";
			// 
			// lblGenre
			// 
			this.lblGenre.Location = new System.Drawing.Point(8, 124);
			this.lblGenre.Name = "lblGenre";
			this.lblGenre.Size = new System.Drawing.Size(64, 16);
			this.lblGenre.TabIndex = 49;
			this.lblGenre.Text = "Genre:";
			// 
			// lblImageFile
			// 
			this.lblImageFile.Location = new System.Drawing.Point(8, 100);
			this.lblImageFile.Name = "lblImageFile";
			this.lblImageFile.Size = new System.Drawing.Size(64, 16);
			this.lblImageFile.TabIndex = 47;
			this.lblImageFile.Text = "Imagefile:";
			// 
			// btnImageFile
			// 
			this.btnImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnImageFile.Location = new System.Drawing.Point(466, 96);
			this.btnImageFile.Name = "btnImageFile";
			this.btnImageFile.Size = new System.Drawing.Size(20, 20);
			this.btnImageFile.TabIndex = 4;
			this.btnImageFile.Text = "...";
			this.btnImageFile.Click += new System.EventHandler(this.btnImageFile_Click);
			// 
			// txtFilename
			// 
			this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilename.Location = new System.Drawing.Point(88, 48);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(366, 20);
			this.txtFilename.TabIndex = 1;
			this.txtFilename.Text = "";
			// 
			// txtImageFile
			// 
			this.txtImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtImageFile.Location = new System.Drawing.Point(88, 96);
			this.txtImageFile.Name = "txtImageFile";
			this.txtImageFile.Size = new System.Drawing.Size(366, 20);
			this.txtImageFile.TabIndex = 3;
			this.txtImageFile.Text = "";
			// 
			// txtTitle
			// 
			this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtTitle.Location = new System.Drawing.Point(88, 24);
			this.txtTitle.Name = "txtTitle";
			this.txtTitle.Size = new System.Drawing.Size(366, 20);
			this.txtTitle.TabIndex = 0;
			this.txtTitle.Text = "";
			// 
			// lblTitle
			// 
			this.lblTitle.Location = new System.Drawing.Point(8, 28);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(56, 16);
			this.lblTitle.TabIndex = 46;
			this.lblTitle.Text = "Title:";
			// 
			// lblFilename
			// 
			this.lblFilename.Location = new System.Drawing.Point(8, 50);
			this.lblFilename.Name = "lblFilename";
			this.lblFilename.Size = new System.Drawing.Size(64, 16);
			this.lblFilename.TabIndex = 45;
			this.lblFilename.Text = "Filename:";
			// 
			// btnFilename
			// 
			this.btnFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFilename.Location = new System.Drawing.Point(466, 48);
			this.btnFilename.Name = "btnFilename";
			this.btnFilename.Size = new System.Drawing.Size(20, 20);
			this.btnFilename.TabIndex = 2;
			this.btnFilename.Text = "...";
			this.btnFilename.Click += new System.EventHandler(this.btnFilename_Click);
			// 
			// FileDetailsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(514, 488);
			this.Controls.Add(this.gbFileDetails);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FileDetailsForm";
			this.Text = "File-Details";
			this.Load += new System.EventHandler(this.FileDetailsForm_Load);
			this.gbFileDetails.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void updateDisplay()
		{
			gbFileDetails.Text = CurApp.Title + ": " + CurFile.Title;
			txtTitle.Text = m_CurFile.Title;
			txtFilename.Text = m_CurFile.Filename;
			txtFilepath.Text = m_CurFile.Filepath;
			txtImageFile.Text = m_CurFile.Imagefile;
			txtGenre.Text = m_CurFile.Genre;
			txtManufacturer.Text = m_CurFile.Manufacturer;
			if (m_CurFile.Year > 1900)
			{
				txtYear.Text = m_CurFile.Year.ToString();
			}
			else
			{
				txtYear.Text = "";
			}
			cbRating.SelectedIndex = m_CurFile.Rating;
			txtCountry.Text = m_CurFile.Country;
			txtSystem.Text = m_CurFile.System_;
			txtOverview.Text = m_CurFile.Overview;
		}

		private void FileDetailsForm_Load(object sender, System.EventArgs e)
		{
			updateDisplay();
		}

		private void label6_Click(object sender, System.EventArgs e)
		{
		
		}

		private void btnFilename_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = txtFilename.Text;
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog(null) == DialogResult.OK )
			{
				txtFilename.Text = openFileDialog1.FileName;
			}
		}

		private void btnImageFile_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = txtImageFile.Text;
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog(null) == DialogResult.OK )
			{
				txtImageFile.Text = openFileDialog1.FileName;
			}
		}


		private bool EntriesOK()
		{
			m_Checker.Clear();
			m_Checker.DoCheck(CurFile.Title != "", "No title entered!");
			m_Checker.DoCheck(CurFile.Filename != "", "No filename entered!");
			if (!m_Checker.IsOk)
			{
				string strHeader = "The following entries are invalid: \r\n\r\n";
				System.Windows.Forms.MessageBox.Show(strHeader + m_Checker.Problems, "Invalid Entries");
			}
			return m_Checker.IsOk;
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			CurFile.Title = txtTitle.Text;
			CurFile.Filename = txtFilename.Text;
			CurFile.Filepath = txtFilepath.Text;
			CurFile.Imagefile = txtImageFile.Text;
			CurFile.Genre = txtGenre.Text;
			CurFile.Manufacturer = txtManufacturer.Text;
			CurFile.Year = ProgramUtils.StrToIntDef(txtYear.Text, -1);
			CurFile.Rating = cbRating.SelectedIndex;
			CurFile.Country = txtCountry.Text;
			CurFile.System_ = txtSystem.Text;
			CurFile.Overview = txtOverview.Text;
			if (EntriesOK())
			{
				this.DialogResult = DialogResult.OK; 
				this.Close();
			}
		
		}


//		private void button1_Click(object sender, System.EventArgs e)
//		{
//			CurFile.FindFileInfo(myProgScraperType.ALLGAME);
//		}
	}
}
