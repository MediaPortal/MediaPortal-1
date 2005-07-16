using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using MediaPortal.GUI.Library;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Video.Database;
using MediaPortal.Util;


namespace MediaPortal.Configuration.Sections
{
	public class MovieDatabase : MediaPortal.Configuration.SectionSettings, IMDB.IProgress
	{
		private class MovieTitleComparer: IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				IMDBMovie movie1=x as IMDBMovie;
				IMDBMovie movie2=y as IMDBMovie;
				return movie1.Title.CompareTo(movie2.Title); 
			}

			#endregion

		}

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckedListBox sharesListBox;
    private System.Windows.Forms.Button startButton;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label countLabel;
    private System.Windows.Forms.Label fileLabel;
    private System.Windows.Forms.Button cancelButton;
		private System.ComponentModel.IContainer components = null;


    internal class ComboBoxItemMovie
    {
      public string		Title;
			public IMDBMovie Movie;

      public ComboBoxItemMovie(string title, IMDBMovie movie)
      {
        this.Title = title;
        this.Movie = movie;
      }
			public override string ToString()
			{
				return Title;
			}
    }
		internal class ComboBoxArt
		{
			public string		Title;
			public string   URL;

			public ComboBoxArt(string title, string url)
			{
				this.Title = title;
				this.URL   = url;
			}
			public override string ToString()
			{
				return Title;
			}
		}

    bool stopRebuild = false;
    ArrayList extractedTags;
		private System.Windows.Forms.Label labelLine1;
		private System.Windows.Forms.Label labelLine2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabControl tabControl2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.TabPage tabPage7;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Button buttonImport;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.TextBox tbWritingCredits;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.TextBox tbPlotOutline;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.TextBox tbVotes;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Button buttonLookupMovie;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.TextBox tbTagline;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox cbWatched;
    private System.Windows.Forms.TextBox tbDescription;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.ComboBox cbTitle;
    private System.Windows.Forms.TextBox tbMPAARating;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox tbYear;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox tbDirector;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox tbDuration;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox tbRating;
    private System.Windows.Forms.TextBox tbTitle;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.TextBox textBoxNewGenre;
    private System.Windows.Forms.Button btnDeleteGenre;
    private System.Windows.Forms.Button buttonNewGenre;
    private System.Windows.Forms.Button buttonUnmapGenre;
    private System.Windows.Forms.Button buttonMapGenre;
    private System.Windows.Forms.ListView listViewGenres;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ListView listViewAllGenres;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.GroupBox groupBox5;
    private System.Windows.Forms.TextBox textBoxNewActor;
    private System.Windows.Forms.Button buttonDeleteActor;
    private System.Windows.Forms.Button buttonNewActor;
    private System.Windows.Forms.Button buttonUnmapActors;
    private System.Windows.Forms.Button buttonMapActors;
    private System.Windows.Forms.ListView listViewMovieActors;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ListView listViewAllActors;
    private System.Windows.Forms.ColumnHeader chName;
    private System.Windows.Forms.GroupBox groupBox6;
    private System.Windows.Forms.Button buttonRemoveFile;
    private System.Windows.Forms.Button buttonAddFile;
    private System.Windows.Forms.ListView listViewFiles;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.GroupBox groupBox7;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.ComboBox comboBoxPictures;
    private System.Windows.Forms.Button btnLookupImage;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.TextBox textBoxPictureURL;
    private System.Windows.Forms.PictureBox pictureBox1;
    ArrayList availableFiles;

    public MovieDatabase() :  this("Movie Database")
    {
    }

    public MovieDatabase(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			cbTitle.DropDownStyle = ComboBoxStyle.DropDownList;
      groupBox2.Enabled = false;
		}

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }
    string[] extensions = new string[] { ".avi" };

    public override void OnSectionActivated()
    {
			labelLine1.Text="";
			labelLine2.Text="";
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load selected shares
      //
      SectionSettings section = SectionSettings.GetSection("MovieShares");

      if(section != null)
      {
        ArrayList shares = (ArrayList)section.GetSetting("shares");

        foreach(string share in shares)
        {
          //
          // Add to share to list box and default to selected
          //
          sharesListBox.Items.Add(share, CheckState.Checked);
        }
      }

      //
      // Fetch extensions
      //
      section = SectionSettings.GetSection("MovieExtensions");

      if(section != null)
      {
        string extensions = (string)section.GetSetting("extensions");
        Extensions = extensions.Split(new char[] { ',' });
      }

			UpdateControlStatus();
			LoadMovies();
			if (cbTitle.Items.Count>0)
				cbTitle.SelectedIndex=0;
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.startButton = new System.Windows.Forms.Button();
      this.sharesListBox = new System.Windows.Forms.CheckedListBox();
      this.cancelButton = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.labelLine2 = new System.Windows.Forms.Label();
      this.labelLine1 = new System.Windows.Forms.Label();
      this.fileLabel = new System.Windows.Forms.Label();
      this.countLabel = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabControl2 = new System.Windows.Forms.TabControl();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.buttonImport = new System.Windows.Forms.Button();
      this.btnDelete = new System.Windows.Forms.Button();
      this.tbWritingCredits = new System.Windows.Forms.TextBox();
      this.label18 = new System.Windows.Forms.Label();
      this.tbPlotOutline = new System.Windows.Forms.TextBox();
      this.label17 = new System.Windows.Forms.Label();
      this.tbVotes = new System.Windows.Forms.TextBox();
      this.label16 = new System.Windows.Forms.Label();
      this.buttonLookupMovie = new System.Windows.Forms.Button();
      this.btnSave = new System.Windows.Forms.Button();
      this.tbTagline = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cbWatched = new System.Windows.Forms.CheckBox();
      this.tbDescription = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label13 = new System.Windows.Forms.Label();
      this.cbTitle = new System.Windows.Forms.ComboBox();
      this.tbMPAARating = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.tbYear = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this.tbDirector = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.tbDuration = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.tbRating = new System.Windows.Forms.TextBox();
      this.tbTitle = new System.Windows.Forms.TextBox();
      this.tabPage6 = new System.Windows.Forms.TabPage();
      this.groupBox6 = new System.Windows.Forms.GroupBox();
      this.buttonRemoveFile = new System.Windows.Forms.Button();
      this.buttonAddFile = new System.Windows.Forms.Button();
      this.listViewFiles = new System.Windows.Forms.ListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.tabPage5 = new System.Windows.Forms.TabPage();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.textBoxNewActor = new System.Windows.Forms.TextBox();
      this.buttonDeleteActor = new System.Windows.Forms.Button();
      this.buttonNewActor = new System.Windows.Forms.Button();
      this.buttonUnmapActors = new System.Windows.Forms.Button();
      this.buttonMapActors = new System.Windows.Forms.Button();
      this.listViewMovieActors = new System.Windows.Forms.ListView();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.listViewAllActors = new System.Windows.Forms.ListView();
      this.chName = new System.Windows.Forms.ColumnHeader();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.textBoxNewGenre = new System.Windows.Forms.TextBox();
      this.btnDeleteGenre = new System.Windows.Forms.Button();
      this.buttonNewGenre = new System.Windows.Forms.Button();
      this.buttonUnmapGenre = new System.Windows.Forms.Button();
      this.buttonMapGenre = new System.Windows.Forms.Button();
      this.listViewGenres = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.listViewAllGenres = new System.Windows.Forms.ListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.tabPage7 = new System.Windows.Forms.TabPage();
      this.groupBox7 = new System.Windows.Forms.GroupBox();
      this.label19 = new System.Windows.Forms.Label();
      this.comboBoxPictures = new System.Windows.Forms.ComboBox();
      this.btnLookupImage = new System.Windows.Forms.Button();
      this.label15 = new System.Windows.Forms.Label();
      this.textBoxPictureURL = new System.Windows.Forms.TextBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabControl2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tabPage6.SuspendLayout();
      this.groupBox6.SuspendLayout();
      this.tabPage5.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.tabPage7.SuspendLayout();
      this.groupBox7.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.startButton);
      this.groupBox1.Controls.Add(this.sharesListBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(448, 160);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Scan Movie Folders";
      // 
      // startButton
      // 
      this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.startButton.Location = new System.Drawing.Point(216, 128);
      this.startButton.Name = "startButton";
      this.startButton.Size = new System.Drawing.Size(216, 22);
      this.startButton.TabIndex = 0;
      this.startButton.Text = "Update database from selected shares";
      this.startButton.Click += new System.EventHandler(this.startButton_Click);
      // 
      // sharesListBox
      // 
      this.sharesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.sharesListBox.CheckOnClick = true;
      this.sharesListBox.Location = new System.Drawing.Point(16, 24);
      this.sharesListBox.Name = "sharesListBox";
      this.sharesListBox.Size = new System.Drawing.Size(416, 94);
      this.sharesListBox.TabIndex = 2;
      this.sharesListBox.SelectedIndexChanged += new System.EventHandler(this.sharesListBox_SelectedIndexChanged);
      this.sharesListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListBox_ItemCheck);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(360, 120);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(72, 22);
      this.cancelButton.TabIndex = 0;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.labelLine2);
      this.groupBox2.Controls.Add(this.labelLine1);
      this.groupBox2.Controls.Add(this.fileLabel);
      this.groupBox2.Controls.Add(this.countLabel);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.progressBar);
      this.groupBox2.Controls.Add(this.cancelButton);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(8, 176);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(448, 152);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Progress";
      // 
      // labelLine2
      // 
      this.labelLine2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelLine2.Location = new System.Drawing.Point(16, 72);
      this.labelLine2.Name = "labelLine2";
      this.labelLine2.Size = new System.Drawing.Size(416, 16);
      this.labelLine2.TabIndex = 5;
      // 
      // labelLine1
      // 
      this.labelLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelLine1.Location = new System.Drawing.Point(16, 56);
      this.labelLine1.Name = "labelLine1";
      this.labelLine1.Size = new System.Drawing.Size(416, 16);
      this.labelLine1.TabIndex = 4;
      // 
      // fileLabel
      // 
      this.fileLabel.Location = new System.Drawing.Point(16, 40);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(416, 16);
      this.fileLabel.TabIndex = 3;
      // 
      // countLabel
      // 
      this.countLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.countLabel.Location = new System.Drawing.Point(96, 24);
      this.countLabel.Name = "countLabel";
      this.countLabel.Size = new System.Drawing.Size(336, 16);
      this.countLabel.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.Location = new System.Drawing.Point(16, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 16);
      this.label2.TabIndex = 1;
      this.label2.Text = "Scanned files:";
      // 
      // progressBar
      // 
      this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar.Location = new System.Drawing.Point(16, 96);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(416, 16);
      this.progressBar.TabIndex = 0;
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 2;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.AutoScroll = true;
      this.tabPage1.Controls.Add(this.tabControl2);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Editor";
      this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
      // 
      // tabControl2
      // 
      this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl2.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
      this.tabControl2.Controls.Add(this.tabPage3);
      this.tabControl2.Controls.Add(this.tabPage6);
      this.tabControl2.Controls.Add(this.tabPage5);
      this.tabControl2.Controls.Add(this.tabPage4);
      this.tabControl2.Controls.Add(this.tabPage7);
      this.tabControl2.Location = new System.Drawing.Point(8, 8);
      this.tabControl2.Name = "tabControl2";
      this.tabControl2.SelectedIndex = 0;
      this.tabControl2.Size = new System.Drawing.Size(448, 368);
      this.tabControl2.TabIndex = 31;
      this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.groupBox3);
      this.tabPage3.Location = new System.Drawing.Point(4, 25);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(440, 339);
      this.tabPage3.TabIndex = 0;
      this.tabPage3.Text = "Title";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.buttonImport);
      this.groupBox3.Controls.Add(this.btnDelete);
      this.groupBox3.Controls.Add(this.tbWritingCredits);
      this.groupBox3.Controls.Add(this.label18);
      this.groupBox3.Controls.Add(this.tbPlotOutline);
      this.groupBox3.Controls.Add(this.label17);
      this.groupBox3.Controls.Add(this.tbVotes);
      this.groupBox3.Controls.Add(this.label16);
      this.groupBox3.Controls.Add(this.buttonLookupMovie);
      this.groupBox3.Controls.Add(this.btnSave);
      this.groupBox3.Controls.Add(this.tbTagline);
      this.groupBox3.Controls.Add(this.label4);
      this.groupBox3.Controls.Add(this.cbWatched);
      this.groupBox3.Controls.Add(this.tbDescription);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.label13);
      this.groupBox3.Controls.Add(this.cbTitle);
      this.groupBox3.Controls.Add(this.tbMPAARating);
      this.groupBox3.Controls.Add(this.label11);
      this.groupBox3.Controls.Add(this.label6);
      this.groupBox3.Controls.Add(this.tbYear);
      this.groupBox3.Controls.Add(this.label10);
      this.groupBox3.Controls.Add(this.tbDirector);
      this.groupBox3.Controls.Add(this.label9);
      this.groupBox3.Controls.Add(this.tbDuration);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Controls.Add(this.label8);
      this.groupBox3.Controls.Add(this.tbRating);
      this.groupBox3.Controls.Add(this.tbTitle);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(440, 336);
      this.groupBox3.TabIndex = 40;
      this.groupBox3.TabStop = false;
      // 
      // buttonImport
      // 
      this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonImport.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonImport.Location = new System.Drawing.Point(200, 312);
      this.buttonImport.Name = "buttonImport";
      this.buttonImport.Size = new System.Drawing.Size(72, 17);
      this.buttonImport.TabIndex = 14;
      this.buttonImport.Text = "Import";
      this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnDelete.Location = new System.Drawing.Point(360, 312);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(72, 17);
      this.btnDelete.TabIndex = 16;
      this.btnDelete.Text = "Delete";
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // tbWritingCredits
      // 
      this.tbWritingCredits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbWritingCredits.Location = new System.Drawing.Point(88, 140);
      this.tbWritingCredits.Name = "tbWritingCredits";
      this.tbWritingCredits.Size = new System.Drawing.Size(336, 20);
      this.tbWritingCredits.TabIndex = 6;
      this.tbWritingCredits.Text = "";
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(8, 144);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(88, 16);
      this.label18.TabIndex = 67;
      this.label18.Text = "Writing Credits:";
      // 
      // tbPlotOutline
      // 
      this.tbPlotOutline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPlotOutline.Location = new System.Drawing.Point(88, 68);
      this.tbPlotOutline.Name = "tbPlotOutline";
      this.tbPlotOutline.Size = new System.Drawing.Size(336, 20);
      this.tbPlotOutline.TabIndex = 3;
      this.tbPlotOutline.Text = "";
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(8, 72);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(64, 16);
      this.label17.TabIndex = 66;
      this.label17.Text = "Plot outline:";
      // 
      // tbVotes
      // 
      this.tbVotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbVotes.Location = new System.Drawing.Point(88, 276);
      this.tbVotes.Name = "tbVotes";
      this.tbVotes.Size = new System.Drawing.Size(96, 20);
      this.tbVotes.TabIndex = 12;
      this.tbVotes.Text = "";
      // 
      // label16
      // 
      this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label16.Location = new System.Drawing.Point(8, 280);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(40, 16);
      this.label16.TabIndex = 65;
      this.label16.Text = "Votes:";
      // 
      // buttonLookupMovie
      // 
      this.buttonLookupMovie.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookupMovie.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonLookupMovie.Location = new System.Drawing.Point(352, 43);
      this.buttonLookupMovie.Name = "buttonLookupMovie";
      this.buttonLookupMovie.Size = new System.Drawing.Size(72, 22);
      this.buttonLookupMovie.TabIndex = 2;
      this.buttonLookupMovie.Text = "Lookup";
      this.buttonLookupMovie.Click += new System.EventHandler(this.buttonLookupMovie_Click);
      // 
      // btnSave
      // 
      this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnSave.Location = new System.Drawing.Point(280, 312);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(72, 17);
      this.btnSave.TabIndex = 15;
      this.btnSave.Text = "Save";
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // tbTagline
      // 
      this.tbTagline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbTagline.Location = new System.Drawing.Point(88, 92);
      this.tbTagline.Name = "tbTagline";
      this.tbTagline.Size = new System.Drawing.Size(336, 20);
      this.tbTagline.TabIndex = 4;
      this.tbTagline.Text = "";
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label4.Location = new System.Drawing.Point(8, 232);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(32, 16);
      this.label4.TabIndex = 47;
      this.label4.Text = "Year:";
      // 
      // cbWatched
      // 
      this.cbWatched.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cbWatched.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbWatched.Location = new System.Drawing.Point(245, 280);
      this.cbWatched.Name = "cbWatched";
      this.cbWatched.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.cbWatched.Size = new System.Drawing.Size(96, 16);
      this.cbWatched.TabIndex = 13;
      this.cbWatched.Text = "Watched          ";
      // 
      // tbDescription
      // 
      this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbDescription.Location = new System.Drawing.Point(88, 164);
      this.tbDescription.Multiline = true;
      this.tbDescription.Name = "tbDescription";
      this.tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tbDescription.Size = new System.Drawing.Size(336, 60);
      this.tbDescription.TabIndex = 7;
      this.tbDescription.Text = "";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(32, 16);
      this.label3.TabIndex = 44;
      this.label3.Text = "Title:";
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(8, 168);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(64, 16);
      this.label13.TabIndex = 64;
      this.label13.Text = "Description:";
      // 
      // cbTitle
      // 
      this.cbTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTitle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTitle.Location = new System.Drawing.Point(88, 19);
      this.cbTitle.Name = "cbTitle";
      this.cbTitle.Size = new System.Drawing.Size(336, 21);
      this.cbTitle.TabIndex = 0;
      this.cbTitle.SelectedIndexChanged += new System.EventHandler(this.cbTitle_SelectedIndexChanged);
      // 
      // tbMPAARating
      // 
      this.tbMPAARating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.tbMPAARating.Location = new System.Drawing.Point(328, 252);
      this.tbMPAARating.Name = "tbMPAARating";
      this.tbMPAARating.Size = new System.Drawing.Size(96, 20);
      this.tbMPAARating.TabIndex = 11;
      this.tbMPAARating.Text = "";
      // 
      // label11
      // 
      this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label11.Location = new System.Drawing.Point(248, 256);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(80, 16);
      this.label11.TabIndex = 63;
      this.label11.Text = "MPAA Rating:";
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.Location = new System.Drawing.Point(248, 232);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(40, 16);
      this.label6.TabIndex = 51;
      this.label6.Text = "Rating:";
      // 
      // tbYear
      // 
      this.tbYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbYear.Location = new System.Drawing.Point(88, 228);
      this.tbYear.Name = "tbYear";
      this.tbYear.Size = new System.Drawing.Size(96, 20);
      this.tbYear.TabIndex = 8;
      this.tbYear.Text = "";
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(8, 96);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(48, 16);
      this.label10.TabIndex = 62;
      this.label10.Text = "Tagline:";
      // 
      // tbDirector
      // 
      this.tbDirector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbDirector.Location = new System.Drawing.Point(88, 116);
      this.tbDirector.Name = "tbDirector";
      this.tbDirector.Size = new System.Drawing.Size(336, 20);
      this.tbDirector.TabIndex = 5;
      this.tbDirector.Text = "";
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(8, 120);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(48, 16);
      this.label9.TabIndex = 59;
      this.label9.Text = "Director:";
      // 
      // tbDuration
      // 
      this.tbDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbDuration.Location = new System.Drawing.Point(88, 252);
      this.tbDuration.Name = "tbDuration";
      this.tbDuration.Size = new System.Drawing.Size(96, 20);
      this.tbDuration.TabIndex = 10;
      this.tbDuration.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(56, 16);
      this.label1.TabIndex = 42;
      this.label1.Text = "Title:";
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label8.Location = new System.Drawing.Point(8, 256);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(56, 16);
      this.label8.TabIndex = 56;
      this.label8.Text = "Duration:";
      // 
      // tbRating
      // 
      this.tbRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRating.Location = new System.Drawing.Point(328, 228);
      this.tbRating.Name = "tbRating";
      this.tbRating.Size = new System.Drawing.Size(96, 20);
      this.tbRating.TabIndex = 9;
      this.tbRating.Text = "";
      // 
      // tbTitle
      // 
      this.tbTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbTitle.Location = new System.Drawing.Point(88, 44);
      this.tbTitle.Name = "tbTitle";
      this.tbTitle.Size = new System.Drawing.Size(256, 20);
      this.tbTitle.TabIndex = 1;
      this.tbTitle.Text = "";
      // 
      // tabPage6
      // 
      this.tabPage6.Controls.Add(this.groupBox6);
      this.tabPage6.Location = new System.Drawing.Point(4, 25);
      this.tabPage6.Name = "tabPage6";
      this.tabPage6.Size = new System.Drawing.Size(440, 339);
      this.tabPage6.TabIndex = 3;
      this.tabPage6.Text = "Files";
      // 
      // groupBox6
      // 
      this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox6.Controls.Add(this.buttonRemoveFile);
      this.groupBox6.Controls.Add(this.buttonAddFile);
      this.groupBox6.Controls.Add(this.listViewFiles);
      this.groupBox6.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox6.Location = new System.Drawing.Point(0, 0);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(440, 336);
      this.groupBox6.TabIndex = 0;
      this.groupBox6.TabStop = false;
      // 
      // buttonRemoveFile
      // 
      this.buttonRemoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonRemoveFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonRemoveFile.Location = new System.Drawing.Point(352, 304);
      this.buttonRemoveFile.Name = "buttonRemoveFile";
      this.buttonRemoveFile.Size = new System.Drawing.Size(72, 22);
      this.buttonRemoveFile.TabIndex = 5;
      this.buttonRemoveFile.Text = "Remove";
      this.buttonRemoveFile.Click += new System.EventHandler(this.buttonRemoveFile_Click);
      // 
      // buttonAddFile
      // 
      this.buttonAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonAddFile.Location = new System.Drawing.Point(272, 304);
      this.buttonAddFile.Name = "buttonAddFile";
      this.buttonAddFile.Size = new System.Drawing.Size(72, 22);
      this.buttonAddFile.TabIndex = 4;
      this.buttonAddFile.Text = "Add";
      this.buttonAddFile.Click += new System.EventHandler(this.buttonAddFile_Click);
      // 
      // listViewFiles
      // 
      this.listViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                    this.columnHeader4});
      this.listViewFiles.Location = new System.Drawing.Point(16, 24);
      this.listViewFiles.Name = "listViewFiles";
      this.listViewFiles.Size = new System.Drawing.Size(408, 272);
      this.listViewFiles.TabIndex = 3;
      this.listViewFiles.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Filename";
      this.columnHeader4.Width = 404;
      // 
      // tabPage5
      // 
      this.tabPage5.Controls.Add(this.groupBox5);
      this.tabPage5.Location = new System.Drawing.Point(4, 25);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Size = new System.Drawing.Size(440, 339);
      this.tabPage5.TabIndex = 2;
      this.tabPage5.Text = "Actors";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.textBoxNewActor);
      this.groupBox5.Controls.Add(this.buttonDeleteActor);
      this.groupBox5.Controls.Add(this.buttonNewActor);
      this.groupBox5.Controls.Add(this.buttonUnmapActors);
      this.groupBox5.Controls.Add(this.buttonMapActors);
      this.groupBox5.Controls.Add(this.listViewMovieActors);
      this.groupBox5.Controls.Add(this.listViewAllActors);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox5.Location = new System.Drawing.Point(0, 0);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(440, 336);
      this.groupBox5.TabIndex = 0;
      this.groupBox5.TabStop = false;
      // 
      // textBoxNewActor
      // 
      this.textBoxNewActor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.textBoxNewActor.Location = new System.Drawing.Point(16, 270);
      this.textBoxNewActor.Name = "textBoxNewActor";
      this.textBoxNewActor.Size = new System.Drawing.Size(152, 20);
      this.textBoxNewActor.TabIndex = 25;
      this.textBoxNewActor.Text = "";
      // 
      // buttonDeleteActor
      // 
      this.buttonDeleteActor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDeleteActor.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonDeleteActor.Location = new System.Drawing.Point(96, 296);
      this.buttonDeleteActor.Name = "buttonDeleteActor";
      this.buttonDeleteActor.Size = new System.Drawing.Size(72, 22);
      this.buttonDeleteActor.TabIndex = 24;
      this.buttonDeleteActor.Text = "Remove";
      this.buttonDeleteActor.Click += new System.EventHandler(this.buttonDeleteActor_Click);
      // 
      // buttonNewActor
      // 
      this.buttonNewActor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNewActor.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonNewActor.Location = new System.Drawing.Point(16, 296);
      this.buttonNewActor.Name = "buttonNewActor";
      this.buttonNewActor.Size = new System.Drawing.Size(72, 22);
      this.buttonNewActor.TabIndex = 23;
      this.buttonNewActor.Text = "Add";
      this.buttonNewActor.Click += new System.EventHandler(this.buttonNewActor_Click);
      // 
      // buttonUnmapActors
      // 
      this.buttonUnmapActors.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonUnmapActors.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonUnmapActors.Location = new System.Drawing.Point(184, 152);
      this.buttonUnmapActors.Name = "buttonUnmapActors";
      this.buttonUnmapActors.Size = new System.Drawing.Size(40, 22);
      this.buttonUnmapActors.TabIndex = 22;
      this.buttonUnmapActors.Text = "<<";
      this.buttonUnmapActors.Click += new System.EventHandler(this.buttonUnmapActors_Click);
      // 
      // buttonMapActors
      // 
      this.buttonMapActors.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonMapActors.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonMapActors.Location = new System.Drawing.Point(184, 120);
      this.buttonMapActors.Name = "buttonMapActors";
      this.buttonMapActors.Size = new System.Drawing.Size(40, 22);
      this.buttonMapActors.TabIndex = 20;
      this.buttonMapActors.Text = ">>";
      this.buttonMapActors.Click += new System.EventHandler(this.buttonMapActors_Click);
      // 
      // listViewMovieActors
      // 
      this.listViewMovieActors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewMovieActors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                          this.columnHeader3,
                                                                                          this.columnHeader5});
      this.listViewMovieActors.Location = new System.Drawing.Point(240, 24);
      this.listViewMovieActors.Name = "listViewMovieActors";
      this.listViewMovieActors.Size = new System.Drawing.Size(184, 240);
      this.listViewMovieActors.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewMovieActors.TabIndex = 18;
      this.listViewMovieActors.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Actors for this Movie";
      this.columnHeader3.Width = 116;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "as";
      this.columnHeader5.Width = 46;
      // 
      // listViewAllActors
      // 
      this.listViewAllActors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewAllActors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                        this.chName});
      this.listViewAllActors.Location = new System.Drawing.Point(16, 24);
      this.listViewAllActors.Name = "listViewAllActors";
      this.listViewAllActors.Size = new System.Drawing.Size(152, 240);
      this.listViewAllActors.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewAllActors.TabIndex = 17;
      this.listViewAllActors.View = System.Windows.Forms.View.Details;
      // 
      // chName
      // 
      this.chName.Text = "Available Actors";
      this.chName.Width = 130;
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.groupBox4);
      this.tabPage4.Location = new System.Drawing.Point(4, 25);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(440, 339);
      this.tabPage4.TabIndex = 1;
      this.tabPage4.Text = "Genres";
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.textBoxNewGenre);
      this.groupBox4.Controls.Add(this.btnDeleteGenre);
      this.groupBox4.Controls.Add(this.buttonNewGenre);
      this.groupBox4.Controls.Add(this.buttonUnmapGenre);
      this.groupBox4.Controls.Add(this.buttonMapGenre);
      this.groupBox4.Controls.Add(this.listViewGenres);
      this.groupBox4.Controls.Add(this.listViewAllGenres);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox4.Location = new System.Drawing.Point(0, 0);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(440, 336);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      // 
      // textBoxNewGenre
      // 
      this.textBoxNewGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.textBoxNewGenre.Location = new System.Drawing.Point(16, 270);
      this.textBoxNewGenre.Name = "textBoxNewGenre";
      this.textBoxNewGenre.Size = new System.Drawing.Size(152, 20);
      this.textBoxNewGenre.TabIndex = 17;
      this.textBoxNewGenre.Text = "";
      // 
      // btnDeleteGenre
      // 
      this.btnDeleteGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnDeleteGenre.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnDeleteGenre.Location = new System.Drawing.Point(96, 296);
      this.btnDeleteGenre.Name = "btnDeleteGenre";
      this.btnDeleteGenre.Size = new System.Drawing.Size(72, 22);
      this.btnDeleteGenre.TabIndex = 16;
      this.btnDeleteGenre.Text = "Remove";
      this.btnDeleteGenre.Click += new System.EventHandler(this.btnDeleteGenre_Click);
      // 
      // buttonNewGenre
      // 
      this.buttonNewGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNewGenre.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonNewGenre.Location = new System.Drawing.Point(16, 296);
      this.buttonNewGenre.Name = "buttonNewGenre";
      this.buttonNewGenre.Size = new System.Drawing.Size(72, 22);
      this.buttonNewGenre.TabIndex = 15;
      this.buttonNewGenre.Text = "Add";
      this.buttonNewGenre.Click += new System.EventHandler(this.buttonNewGenre_Click);
      // 
      // buttonUnmapGenre
      // 
      this.buttonUnmapGenre.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonUnmapGenre.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonUnmapGenre.Location = new System.Drawing.Point(184, 152);
      this.buttonUnmapGenre.Name = "buttonUnmapGenre";
      this.buttonUnmapGenre.Size = new System.Drawing.Size(40, 22);
      this.buttonUnmapGenre.TabIndex = 14;
      this.buttonUnmapGenre.Text = "<<";
      this.buttonUnmapGenre.Click += new System.EventHandler(this.buttonUnmapGenre_Click);
      // 
      // buttonMapGenre
      // 
      this.buttonMapGenre.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonMapGenre.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonMapGenre.Location = new System.Drawing.Point(184, 120);
      this.buttonMapGenre.Name = "buttonMapGenre";
      this.buttonMapGenre.Size = new System.Drawing.Size(40, 22);
      this.buttonMapGenre.TabIndex = 12;
      this.buttonMapGenre.Text = ">>";
      this.buttonMapGenre.Click += new System.EventHandler(this.buttonMapGenre_Click);
      // 
      // listViewGenres
      // 
      this.listViewGenres.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                     this.columnHeader1});
      this.listViewGenres.Location = new System.Drawing.Point(240, 24);
      this.listViewGenres.Name = "listViewGenres";
      this.listViewGenres.Size = new System.Drawing.Size(184, 240);
      this.listViewGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewGenres.TabIndex = 10;
      this.listViewGenres.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Genres for this Movie";
      this.columnHeader1.Width = 180;
      // 
      // listViewAllGenres
      // 
      this.listViewAllGenres.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewAllGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                        this.columnHeader2});
      this.listViewAllGenres.Location = new System.Drawing.Point(16, 24);
      this.listViewAllGenres.Name = "listViewAllGenres";
      this.listViewAllGenres.Size = new System.Drawing.Size(152, 240);
      this.listViewAllGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewAllGenres.TabIndex = 9;
      this.listViewAllGenres.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Available Genres";
      this.columnHeader2.Width = 148;
      // 
      // tabPage7
      // 
      this.tabPage7.Controls.Add(this.groupBox7);
      this.tabPage7.Location = new System.Drawing.Point(4, 25);
      this.tabPage7.Name = "tabPage7";
      this.tabPage7.Size = new System.Drawing.Size(440, 339);
      this.tabPage7.TabIndex = 4;
      this.tabPage7.Text = "Coverart";
      // 
      // groupBox7
      // 
      this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox7.Controls.Add(this.label19);
      this.groupBox7.Controls.Add(this.comboBoxPictures);
      this.groupBox7.Controls.Add(this.btnLookupImage);
      this.groupBox7.Controls.Add(this.label15);
      this.groupBox7.Controls.Add(this.textBoxPictureURL);
      this.groupBox7.Controls.Add(this.pictureBox1);
      this.groupBox7.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox7.Location = new System.Drawing.Point(0, 0);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(440, 336);
      this.groupBox7.TabIndex = 0;
      this.groupBox7.TabStop = false;
      // 
      // label19
      // 
      this.label19.Location = new System.Drawing.Point(16, 48);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(96, 16);
      this.label19.TabIndex = 38;
      this.label19.Text = "Available Images:";
      // 
      // comboBoxPictures
      // 
      this.comboBoxPictures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxPictures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPictures.Location = new System.Drawing.Point(136, 44);
      this.comboBoxPictures.Name = "comboBoxPictures";
      this.comboBoxPictures.Size = new System.Drawing.Size(288, 21);
      this.comboBoxPictures.TabIndex = 37;
      this.comboBoxPictures.SelectedIndexChanged += new System.EventHandler(this.comboBoxPictures_SelectedIndexChanged);
      // 
      // btnLookupImage
      // 
      this.btnLookupImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLookupImage.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLookupImage.Location = new System.Drawing.Point(352, 19);
      this.btnLookupImage.Name = "btnLookupImage";
      this.btnLookupImage.Size = new System.Drawing.Size(72, 22);
      this.btnLookupImage.TabIndex = 36;
      this.btnLookupImage.Text = "Lookup";
      this.btnLookupImage.Click += new System.EventHandler(this.btnLookupImage_Click);
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(16, 24);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(80, 16);
      this.label15.TabIndex = 35;
      this.label15.Text = "Image-URL:";
      // 
      // textBoxPictureURL
      // 
      this.textBoxPictureURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxPictureURL.Location = new System.Drawing.Point(136, 20);
      this.textBoxPictureURL.Name = "textBoxPictureURL";
      this.textBoxPictureURL.Size = new System.Drawing.Size(208, 20);
      this.textBoxPictureURL.TabIndex = 34;
      this.textBoxPictureURL.Text = "";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox1.Location = new System.Drawing.Point(136, 80);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(170, 240);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 33;
      this.pictureBox1.TabStop = false;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.groupBox2);
      this.tabPage2.Controls.Add(this.groupBox1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(464, 382);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Scan";
      // 
      // MovieDatabase
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "MovieDatabase";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.tabPage6.ResumeLayout(false);
      this.groupBox6.ResumeLayout(false);
      this.tabPage5.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.tabPage4.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.tabPage7.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

    private void sharesListBox_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateControlStatus()
    {
      startButton.Enabled = sharesListBox.CheckedItems.Count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void sharesListBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void startButton_Click(object sender, System.EventArgs e)
    {
      groupBox2.Enabled = true;
      groupBox1.Enabled = false;

      RebuildDatabase();
    }

    private enum RebuildState
    {
      None,
      Counting,
      Scanning,
      Updating,
      Done
    }

    /// <summary>
    /// 
    /// </summary>
    private void RebuildDatabase()
    {
      int totalFiles = 0;

      //
      // Start by counting files
      //
      RebuildState rebuildState = RebuildState.None;

      while(stopRebuild != true)
      {
        switch(rebuildState)
        {
          case RebuildState.None:
            rebuildState = RebuildState.Counting;
            break;

          case RebuildState.Counting:
          {
            SetStatus("Counting files in selected folders");

            //
            // Count files
            //
            availableFiles = new ArrayList();
            totalFiles = CountFiles();
      
            //
            // Initialize progress bar
            //
            progressBar.Value = 0;
            progressBar.Maximum = totalFiles;
            progressBar.Step = 1;
            rebuildState = RebuildState.Scanning;
            break;
          }

          case RebuildState.Scanning:
          {
            SetStatus("Scanning movie files...");
            extractedTags = new ArrayList(totalFiles);
            ScanFiles(totalFiles);
            rebuildState = RebuildState.Updating; 
            break;
          }

          case RebuildState.Updating:
          {
            rebuildState = RebuildState.Done; 
            break;
          }

          case RebuildState.Done:
            SetStatus("Database has been successfully rebuilt");
            stopRebuild = true;
            rebuildState = RebuildState.None;

						
						labelLine1.Text="";
						labelLine2.Text="";
            break;
        }
      }
      
      stopRebuild = false;
      groupBox1.Enabled = true;
      groupBox2.Enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int CountFiles()
    {
      int totalFiles = 0;

      for(int index = 0; index < sharesListBox.CheckedIndices.Count; index++)
      {
        string path = sharesListBox.Items[(int)sharesListBox.CheckedIndices[index]].ToString();

        //
        // Make sure the path exists
        //
        if(Directory.Exists(path))
        {
          CountFiles(path, ref totalFiles);
        }
      }

      return totalFiles;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="totalFiles"></param>
    private void CountFiles(string path, ref int totalFiles)
    {
      //
      // Exit counting if we requested so
      //
      if(stopRebuild) 
        return;

      SetCount(0, totalFiles);

      //
      // Count the files in the current directory
      //
      try
      {
        foreach(string extension in Extensions)
        {
          string[] files = Directory.GetFiles(path, String.Format("*{0}", extension));
          availableFiles.AddRange(files);
          totalFiles += files.Length;
        }
      }
      catch
      {
        // Ignore
      }

      //
      // Count files in subdirectories
      //
      try
      {
        string[] directories = Directory.GetDirectories(path);

        foreach(string directory in directories)
        {
          CountFiles(directory, ref totalFiles);
        }
      }
      catch
      {
        // Ignore
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    private void SetStatus(string status)
    {
      fileLabel.Text = status;
      Application.DoEvents();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="total"></param>
    private void SetCount(int count, int total)
    {
      countLabel.Text = String.Format("{0} of {1}", count.ToString(), total.ToString());
      Application.DoEvents();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cancelButton_Click(object sender, System.EventArgs e)
    {
      stopRebuild = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="totalFiles"></param>
    private void ScanFiles(int totalFiles)
    {
      int currentCount = 0;

      foreach(string file in availableFiles)
      {
        ScanFile(file,-1);

        //
        // Update stats
        //
        SetCount(++currentCount, totalFiles);

        progressBar.PerformStep();

        //
        // Exit counting if we requested so
        //
        if(stopRebuild) 
          return;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    private IMDBMovie ScanFile(string file, int ID)
		{
			if(stopRebuild)  return null;

			string ext=System.IO.Path.GetExtension(file.ToLower());
			if (ext==".ifo") return null;
			if (ext==".vob") return null;
			IMDBMovie movieDetails=new IMDBMovie();
			IMDB imdb = new IMDB(this);
			int selectedItem=0;
			int id=VideoDatabase.GetMovieInfo(file,ref movieDetails);
			if (id<0) 
			{
				Application.DoEvents();
				imdb.Find( Utils.GetFilename(file));
				Application.DoEvents();
				if(stopRebuild)  return null;
				if (imdb.Count<=0) return null;
				if (imdb.Count>0)
				{
					DlgMovieList dlg = new DlgMovieList();
					dlg.imdb = imdb;
					dlg.Filename=file;
					for (int i=0; i < imdb.Count;++i)
						dlg.AddMovie(imdb[i].Title);
					if (dlg.ShowDialog() == DialogResult.Cancel) return null;
					selectedItem=dlg.SelectedItem;
				}

				if (stopRebuild) return null;
				if ( imdb.GetDetails(imdb[selectedItem],ref movieDetails))
				{
					if(stopRebuild)  return null;
					Application.DoEvents();
					if (ID < 0)
					{
						string path,filename;
						Utils.Split(file,out path, out filename);
						VirtualDirectory dir = new VirtualDirectory();
						dir.SetExtensions(Utils.VideoExtensions);
						ArrayList items = dir.GetDirectory(path);
						foreach (GUIListItem item in items)
						{
							if (item.IsFolder) continue;
							if (Utils.ShouldStack(item.Path, file)||item.Path==file)
							{
								VideoDatabase.AddMovieFile(item.Path);
							}
						}
						id=VideoDatabase.AddMovie(file,false);
						movieDetails.ID=id;
					}
					else
					{
						id=ID;
						movieDetails.ID=ID;
					}

					AmazonImageSearch search = new AmazonImageSearch();
					search.Search(movieDetails.Title);
					if (search.Count>0)
					{
						movieDetails.ThumbURL=search[0];
					}
					VideoDatabase.SetMovieInfoById(movieDetails.ID,ref movieDetails);
					if (stopRebuild) return null;
					//download thumbnail
					DownloadThumnail(Thumbs.MovieTitle,movieDetails.ThumbURL,movieDetails.Title);
					
					if (stopRebuild) return null;
					Application.DoEvents();
				}
			}
			if (id>=0)
			{
				//"Cast overview:\nNaomi Watts as Rachel Keller\nMartin Henderson as Noah Clay\nDavid Dorfman as Aidan Keller\nBrian Cox as Richard Morgan\nJane Alexander as Dr. Grasnik\nLindsay Frost as Ruth Embry\nAmber Tamblyn as Katie Embry\nRachael Bella as Rebecca ''Becca'' Kotler\nDaveigh Chase as Samara Morgan\nShannon Cochran as Anna Morgan\nSandra Thigpen as Teacher\nRichard Lineback as Innkeeper\nSasha Barrese as Girl Teen #1\nTess Hall as Girl Teen #2\nAdam Brody as Kellen, Male Teen #1"
				DownloadActors(movieDetails);
				DownloadDirector(movieDetails);
			}
			return movieDetails;
    }

		void DownloadThumnail(string folder,string url, string name)
		{
			if(stopRebuild)  return;
			if (url==null) return;
			if (url.Length==0) return;
			Application.DoEvents();
			string strThumb = Utils.GetCoverArtName(folder,name);
			string LargeThumb = Utils.GetLargeCoverArtName(folder,name);
			if (!System.IO.File.Exists(strThumb))
			{
				string strExtension;
				strExtension = System.IO.Path.GetExtension(url);
				if (strExtension.Length > 0)
				{
					string strTemp = "temp";
					strTemp += strExtension;
					Utils.FileDelete(strTemp);
             
					Utils.DownLoadImage(url, strTemp);
					if (System.IO.File.Exists(strTemp))
					{
						MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
						MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
					}
					else Log.Write("Unable to download {0}->{1}", url,strTemp);
					Utils.FileDelete(strTemp);
				}
			}
		}
		void DownloadDirector(IMDBMovie movieDetails)
		{
			if(stopRebuild)  return;
			IMDB imdb = new IMDB(this);
			string actor=movieDetails.Director;
			string strThumb = Utils.GetCoverArtName(Thumbs.MovieActors,actor);
			if (!System.IO.File.Exists(strThumb))
			{
				imdb.FindActor(actor);
				IMDBActor imdbActor=new IMDBActor();
				for (int x=0; x < imdb.Count;++x)
				{
					imdb.GetActorDetails(imdb[x],out imdbActor);
					if (imdbActor.ThumbnailUrl!=null && imdbActor.ThumbnailUrl.Length>0) break;
				}
				if (imdbActor.ThumbnailUrl!=null)
				{
					if (imdbActor.ThumbnailUrl.Length!=0)
					{
						DownloadThumnail(Thumbs.MovieActors,imdbActor.ThumbnailUrl,actor);
					}
					else Log.Write("url=empty for actor {0}", actor);
				}
				else Log.Write("url=null for actor {0}", actor);
			}
		}
		void DownloadActors(IMDBMovie movieDetails)
		{	
			IMDB imdb = new IMDB(this);
			string[] actors=movieDetails.Cast.Split('\n');
			if (actors.Length>1)
			{
				for (int i=1; i < actors.Length;++i)
				{
					if(stopRebuild)  return;
					int pos =actors[i].IndexOf(" as ");
					if (pos <0) continue;
					string actor=actors[i].Substring(0,pos);
					string strThumb = Utils.GetCoverArtName(Thumbs.MovieActors,actor);
					if (!System.IO.File.Exists(strThumb))
					{
						imdb.FindActor(actor);
						if(stopRebuild)  return;
						IMDBActor imdbActor=new IMDBActor();
						for (int x=0; x < imdb.Count;++x)
						{
							if(stopRebuild)  return;
							imdb.GetActorDetails(imdb[x],out imdbActor);
							if (imdbActor.ThumbnailUrl!=null && imdbActor.ThumbnailUrl.Length>0) break;
						}
						if(stopRebuild)  return;
						if (imdbActor.ThumbnailUrl!=null)
						{
							if (imdbActor.ThumbnailUrl.Length!=0)
							{
								DownloadThumnail(Thumbs.MovieActors,imdbActor.ThumbnailUrl,actor);
							}
							else Log.Write("url=empty for actor {0}", actor);
						}
						else Log.Write("url=null for actor {0}", actor);
					}
				}
			}
		}
    private void clearButton_Click(object sender, System.EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the entire video database?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if(dialogResult == DialogResult.Yes)
      {
        string database = @"database\VideoDatabaseV5.db3";
        if(File.Exists(database))
        {
          File.Delete(database);
        }

        MessageBox.Show("Music database has been cleared");
      }
    }
		public void OnProgress(string line1, string line2, string line3, int percent)
		{
			labelLine1.Text=line1;
			labelLine2.Text=line2;
			Application.DoEvents();
		}

		private void tabPage1_Click(object sender, System.EventArgs e)
		{
		}

		void LoadMovies()
		{
			cbTitle.Items.Clear();
			ArrayList movies=new ArrayList();
			VideoDatabase.GetMovies(ref movies);
			movies.Sort(new MovieTitleComparer() );
			int i=0;
			foreach(IMDBMovie movie in movies)
			{
				ComboBoxItemMovie newItem = new ComboBoxItemMovie(movie.Title,movie);
				cbTitle.Items.Add(newItem);
				if (i==0) UpdateEdit(movie);
				++i;
			}
			

			IMDBMovie movieNew = new IMDBMovie();
			movieNew.Title="New...";
			ComboBoxItemMovie emptyItem = new ComboBoxItemMovie("New...",movieNew);
			cbTitle.Items.Add(emptyItem);
		}
		void UpdateEdit(IMDBMovie movie)
		{	
			tbTitle.Text=movie.Title;
			tbTagline.Text=movie.TagLine;
			tbYear.Text=movie.Year.ToString();
			tbVotes.Text=movie.Votes;
			tbRating.Text=movie.Rating.ToString();
			tbDirector.Text=movie.Director;
			tbWritingCredits.Text=movie.WritingCredits;
			tbDescription.Text=movie.Plot;
			textBoxPictureURL.Text=movie.ThumbURL;
			tbPlotOutline.Text=movie.PlotOutline;
			tbMPAARating.Text=movie.MPARating;
			tbDuration.Text=movie.RunTime.ToString();
			if (movie.Watched>0) cbWatched.Checked=true;
			else cbWatched.Checked=false;
			if (pictureBox1.Image!=null)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image=null;
			}
			string file=Utils.GetLargeCoverArtName(Thumbs.MovieTitle,movie.Title);
			if (System.IO.File.Exists(file))
			{
				using (Image img = Image.FromFile(file))
				{
					Bitmap result= new Bitmap(img.Width,img.Height);
					using (Graphics g = Graphics.FromImage(result))
					{
						g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
						g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
						g.DrawImage(img, new Rectangle(0,0,img.Width,img.Height) );
					}
					pictureBox1.Image=result;
				}
			}
			listViewMovieActors.Items.Clear();
			string[] actors=movie.Cast.Split('\n');
			if (actors.Length>1)
			{
				for (int i=1; i < actors.Length;++i)
				{
					string actor;
					string role="";
					int pos =actors[i].IndexOf(" as ");
					if (pos >=0)
					{
						actor=actors[i].Substring(0,pos);
						role=actors[i].Substring(pos+4);
					}
					else
						actor=actors[i];
					
					ListViewItem item = new ListViewItem(actor);
					item.SubItems.Add(role);
					listViewMovieActors.Items.Add(item);
				}
			}
			listViewMovieActors.Sort();
			
			listViewGenres.Items.Clear();
			string szGenres=movie.Genre;
			ArrayList vecGenres=new ArrayList();
			if ( szGenres.IndexOf("/")>=0 )
			{
				Tokens f = new Tokens(szGenres, new char[] {'/'} );
				foreach (string strGenre in f)
				{ 
					listViewGenres.Items.Add(strGenre.Trim());
				}
			}
			else
			{
				string strGenre=movie.Genre; 
				listViewGenres.Items.Add(strGenre.Trim());
			}
			
			listViewGenres.Sort();


			listViewAllGenres.Items.Clear();
			ArrayList genres = new ArrayList();
			VideoDatabase.GetGenres(genres);
			foreach (string genre in genres)
			{
				bool add=true;
				foreach (ListViewItem item in listViewGenres.Items)
				{
					if (item.Text==genre)
					{
						add=false;
						break;
					}
				}
				if (add)
					listViewAllGenres.Items.Add(genre);
			}
			listViewAllGenres.Sort();
			
			
			listViewAllActors.Items.Clear();
			ArrayList listActors = new ArrayList();
			VideoDatabase.GetActors( listActors);
			foreach (string actor in listActors)
			{
				bool add=true;
				foreach (ListViewItem item in listViewMovieActors.Items)
				{
					if (item.Text==actor)
					{
						add=false;
						break;
					}
				}
				if (add)
					listViewAllActors.Items.Add(actor);
			}
			listViewAllActors.Sort();

			listViewFiles.Items.Clear();
			ArrayList filenames = new ArrayList();
			VideoDatabase.GetFiles(movie.ID,ref filenames);
			foreach (string filename in filenames)
			{
				listViewFiles.Items.Add(filename);
			}
		}

		private void cbTitle_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (cbTitle.SelectedItem==null) return;
			ComboBoxItemMovie item = (ComboBoxItemMovie)cbTitle.SelectedItem;
			UpdateEdit(item.Movie);
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (tabControl1.SelectedTab==tabPage1)
			{
				LoadMovies();
			}
		}

		private void buttonMapGenre_Click(object sender, System.EventArgs e)
		{
			if (listViewAllGenres.SelectedItems==null) return;
			
			for(int i=0; i < listViewAllGenres.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewAllGenres.SelectedItems[i];
				
				listViewGenres.Items.Add(listItem.Text);
			}
			
			for(int i=listViewAllGenres.SelectedItems.Count-1; i >=0 ;i--)
			{
				ListViewItem listItem=listViewAllGenres.SelectedItems[i];

				listViewAllGenres.Items.Remove(listItem);
			}		
		}

		private void buttonUnmapGenre_Click(object sender, System.EventArgs e)
		{
			if (listViewAllGenres.SelectedItems==null) return;
			for(int i=0; i < listViewGenres.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewGenres.SelectedItems[i];
				listViewAllGenres.Items.Add(listItem.Text);
			}		

			for(int i=listViewGenres.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listViewGenres.SelectedItems[i];
				listViewGenres.Items.Remove(listItem);
			}
		}

		private void buttonMapActors_Click(object sender, System.EventArgs e)
		{
			if (listViewAllActors.SelectedItems==null) return;
			
			for(int i=0; i < listViewAllActors.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewAllActors.SelectedItems[i];
				
				ListViewItem newItem=new ListViewItem(listItem.Text);
				newItem.SubItems.Add("");
				listViewMovieActors.Items.Add(newItem);
			}
			
			for(int i=listViewAllActors.SelectedItems.Count-1; i >=0 ;i--)
			{
				ListViewItem listItem=listViewAllActors.SelectedItems[i];

				listViewAllActors.Items.Remove(listItem);
			}		
		}

		private void buttonUnmapActors_Click(object sender, System.EventArgs e)
		{
			if (listViewMovieActors.SelectedItems==null) return;
			for(int i=0; i < listViewMovieActors.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewMovieActors.SelectedItems[i];
				listViewAllActors.Items.Add(listItem.Text);
			}		

			for(int i=listViewMovieActors.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listViewMovieActors.SelectedItems[i];
				listViewMovieActors.Items.Remove(listItem);
			}
		}

		private void buttonAddFile_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog find_file = new OpenFileDialog();
      find_file.RestoreDirectory = true;
			find_file.DefaultExt = "avi";
			find_file.Filter = "Avi Files|*.avi|Recordings|*.dvr-ms|Mpeg files|*.mpeg|Mpeg files|*.mpg|Windows Media|*.wmv|All files|*.*";
			find_file.InitialDirectory = ".";
			find_file.Title= "Find files for "+ tbTitle.Text;
			if(find_file.ShowDialog(this)==DialogResult.OK)
			{
				listViewFiles.Items.Add(find_file.FileName);

			}
		}

		private void buttonRemoveFile_Click(object sender, System.EventArgs e)
		{
			if (listViewFiles.SelectedItems==null) return;
			for(int i=listViewFiles.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listViewFiles.SelectedItems[i];
				listViewFiles.Items.Remove(listItem);
			}
		}

		private void buttonDeleteActor_Click(object sender, System.EventArgs e)
		{
			if (listViewAllActors.SelectedItems==null) return;
			if(MessageBox.Show("Are you sure you want to delete the selected actors?","Are you sure?",MessageBoxButtons.YesNo)==DialogResult.Yes)
			{
				for(int i=listViewAllActors.SelectedItems.Count-1; i>=0;--i)
				{
					ListViewItem listItem=listViewAllActors.SelectedItems[i];
					VideoDatabase.DeleteActor(listItem.Text);
					listViewAllActors.Items.Remove(listItem);
				}
			}
		}

		private void buttonNewActor_Click(object sender, System.EventArgs e)
		{
			if (textBoxNewActor.Text.Length==0) return;
			VideoDatabase.AddActor(textBoxNewActor.Text);
			listViewAllActors.Items.Add(textBoxNewActor.Text);
		
		}

		private void btnDeleteGenre_Click(object sender, System.EventArgs e)
		{
			if (listViewAllGenres.SelectedItems==null) return;
			if(MessageBox.Show("Are you sure you want to delete the selected genres?","Are you sure?",MessageBoxButtons.YesNo)==DialogResult.Yes)
			{
				for(int i=listViewAllGenres.SelectedItems.Count-1; i>=0;--i)
				{
					ListViewItem listItem=listViewAllGenres.SelectedItems[i];
					VideoDatabase.DeleteGenre(listItem.Text);
					listViewAllGenres.Items.Remove(listItem);
				}					
			}
		}

		private void buttonNewGenre_Click(object sender, System.EventArgs e)
		{
			if (textBoxNewGenre.Text.Length==0) return;
			VideoDatabase.AddGenre(textBoxNewGenre.Text);
			listViewAllGenres.Items.Add(textBoxNewGenre.Text);
		}

		private void buttonNewMovie_Click(object sender, System.EventArgs e)
		{
			cbTitle.SelectedItem=null;
			IMDBMovie details = new IMDBMovie();
			UpdateEdit(details);
		}

		private void buttonLookupMovie_Click(object sender, System.EventArgs e)
		{
			if (tbTitle.Text==String.Empty)
			{
				MessageBox.Show("Please enter a movie title", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);;
			}
			buttonLookupMovie.Enabled=false;
			btnSave.Enabled=false;
			tabControl2.Enabled=false;
			tabControl1.Enabled=false;
			int id=CurrentMovie.ID;
			IMDBMovie movie=ScanFile(tbTitle.Text, id);
			if (movie!=null) 
			{
				LoadMovies();
				foreach (ComboBoxItemMovie item in cbTitle.Items)
				{
					if (item.Title==movie.Title)
					{
						cbTitle.SelectedItem=item;
						break;
					}
				}
				if (cbTitle.SelectedItem==null)
				{
					foreach (ComboBoxItemMovie item in cbTitle.Items)
					{
						if (item.Movie.ID==id)
						{
							cbTitle.SelectedItem=item;
							break;
						}
					}
				}
			}
			else
			{
				MessageBox.Show("Movie details could not be found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);;
			}
			if (cbTitle.SelectedItem!=null)
			{
				ComboBoxItemMovie item=(ComboBoxItemMovie )cbTitle.SelectedItem;
				UpdateEdit(item.Movie);
			}
			buttonLookupMovie.Enabled=true;
			btnSave.Enabled=true;
			tabControl2.Enabled=true;
			tabControl1.Enabled=true;
		}

		private void btnSave_Click(object sender, System.EventArgs e)
		{
			IMDBMovie details = CurrentMovie;
			if (details.ID>=0)
			{
				VideoDatabase.RemoveGenresForMovie(details.ID);
				VideoDatabase.RemoveActorsForMovie(details.ID);
				VideoDatabase.RemoveFilesForMovie(details.ID);
			}

			VideoDatabase.SetMovieInfoById(details.ID,ref details);

			//add files to movie
			foreach (ListViewItem item in listViewFiles.Items)
			{
				string strPath, strFileName;

				MediaPortal.Database.DatabaseUtility.Split(item.Text,out strPath, out strFileName); 
				MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strPath);
				MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strFileName);

				int pathId = VideoDatabase.AddPath(strPath);
				VideoDatabase.AddFile(details.ID,pathId,strFileName);
			}
		}

		private void btnLookupImage_Click(object sender, System.EventArgs e)
		{
			if (textBoxPictureURL.Text==String.Empty) return;
			if (pictureBox1.Image!=null)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image=null;
			}			
			string strThumb = Utils.GetCoverArtName(Thumbs.MovieTitle,tbTitle.Text);
			string LargeThumb = Utils.GetLargeCoverArtName(Thumbs.MovieTitle,tbTitle.Text);
			Utils.FileDelete(strThumb);
			Utils.FileDelete(LargeThumb);
			DownloadThumnail(Thumbs.MovieTitle,textBoxPictureURL.Text, tbTitle.Text);

			string file=Utils.GetLargeCoverArtName(Thumbs.MovieTitle,tbTitle.Text);
			if (System.IO.File.Exists(file))
			{
				using (Image img = Image.FromFile(file))
				{
					Bitmap result= new Bitmap(img.Width,img.Height);
					using (Graphics g = Graphics.FromImage(result))
					{
						g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
						g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
						g.DrawImage(img, new Rectangle(0,0,img.Width,img.Height) );
					}
					pictureBox1.Image=result;
				}
			}
		}

		private void btnDelete_Click(object sender, System.EventArgs e)
		{
			if (CurrentMovie.ID < 0)
			{
				return;
			}
			DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this movie?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult== DialogResult.Yes)
			{
				VideoDatabase.DeleteMovieInfoById(CurrentMovie.ID);
				LoadMovies();
			}
		}

		private void comboBoxPictures_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ComboBoxArt art = comboBoxPictures.SelectedItem as ComboBoxArt;
			if (art!=null)
			{
				textBoxPictureURL.Text=art.URL;
			}
		}

		private void tabControl2_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (tabControl2.SelectedTab==tabPage7)
			{
				comboBoxPictures.Items.Clear();
				AmazonImageSearch search = new AmazonImageSearch();
				search.Search(CurrentMovie.Title);
				if (search.Count>0)
				{
					for (int i=0; i < search.Count;++i)
					{
						ComboBoxArt art = new ComboBoxArt(String.Format("{0}", (i+1)),search[i]);
						comboBoxPictures.Items.Add(art);
					}
				}
			}
		}

		private void buttonImport_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog find_file = new OpenFileDialog();
			find_file.RestoreDirectory = true;
			find_file.DefaultExt = "xml";
			find_file.Filter = "DVD Profile|*.xml";
			find_file.InitialDirectory = ".";
			find_file.Title= "Select DVD Profiler database"+ tbTitle.Text;
			if(find_file.ShowDialog(this)!=DialogResult.OK) return;
			XmlDocument doc = new XmlDocument();
			doc.Load(find_file.FileName);
			XmlNodeList dvdList = doc.DocumentElement.SelectNodes("/Collection/DVD");
			foreach (XmlNode nodeDVD in dvdList)
			{
				XmlNode nodeTitle=nodeDVD.SelectSingleNode("Title");
				XmlNode nodeRating=nodeDVD.SelectSingleNode("Rating");
				XmlNode nodeYear=nodeDVD.SelectSingleNode("ProductionYear");
				XmlNode nodeDuration=nodeDVD.SelectSingleNode("RunningTime");
				XmlNode nodeOverview=nodeDVD.SelectSingleNode("Overview");

				string genre=String.Empty;
				XmlNodeList  genreList = nodeDVD.SelectNodes("Genres/Genre");
				foreach (XmlNode nodeGenre in genreList)
				{
					if (genre.Length>0) genre +=" / ";
					genre+=nodeGenre.InnerText;
				}
				string cast="Cast overview:";
				XmlNodeList  actorsList = nodeDVD.SelectNodes("Actors/Actor");
				foreach (XmlNode nodeActor in actorsList)
				{
					string firstname=String.Empty;
					string lastname=String.Empty;
					string role=String.Empty;
					XmlNode nodeFirstName=nodeActor.SelectSingleNode("FirstName");
					XmlNode nodeLastName=nodeActor.SelectSingleNode("LastName");
					XmlNode nodeRole=nodeActor.SelectSingleNode("Role");
					if (nodeFirstName!=null && nodeFirstName.InnerText!=null) firstname=nodeFirstName.InnerText;
					if (nodeLastName!=null && nodeLastName.InnerText!=null) lastname=nodeLastName.InnerText;
					if (nodeRole!=null && nodeRole.InnerText!=null) role=nodeRole.InnerText;
					string line = String.Format("{0} {1} as {2}\n", firstname, lastname, role);
					cast+=line;
				}

				
				string credits=String.Empty;
				XmlNodeList  creditsList = nodeDVD.SelectNodes("Credits/Credit");
				foreach (XmlNode nodeCredit in creditsList)
				{
					XmlNode nodeFirstName=nodeCredit.SelectSingleNode("FirstName");
					XmlNode nodeLastName=nodeCredit.SelectSingleNode("LastName");
					
					if (credits.Length>0) credits +=" / ";
					credits+=String.Format("{0} {1}",nodeFirstName.InnerText,nodeLastName.InnerText);
				}

				IMDBMovie movie = new IMDBMovie();
				movie.Cast=cast;
				movie.CDLabel=String.Empty;
				movie.Director=String.Empty;
				movie.DVDLabel=String.Empty;
				movie.File=String.Empty;
				movie.Genre=genre;
				movie.IMDBNumber=String.Empty;
				movie.MPARating=nodeRating.InnerText;
				movie.Path=String.Empty;
				movie.Plot=nodeOverview.InnerText;
				movie.PlotOutline=String.Empty;
				movie.Rating=0;
				movie.RunTime=Int32.Parse(nodeDuration.InnerText);
				movie.SearchString=String.Empty;
				movie.TagLine=String.Empty;
				movie.ThumbURL=String.Empty;
				movie.Title=nodeTitle.InnerText;
				movie.Top250=0;
				movie.Votes=String.Empty;
				movie.Watched=0;
				movie.WritingCredits=credits;
				movie.Year=Int32.Parse(nodeYear.InnerText);
				int id=VideoDatabase.AddMovie(movie.Title,true);
				movie.ID=id;
				VideoDatabase.SetMovieInfoById(id,ref movie);
				Application.DoEvents();
			}
			LoadMovies();
		}

		IMDBMovie CurrentMovie
		{
			get
			{
				IMDBMovie movie = new IMDBMovie();
				if (cbTitle.SelectedItem!=null)
				{
					ComboBoxItemMovie cbMovie= (ComboBoxItemMovie)cbTitle.SelectedItem;
					movie = cbMovie.Movie;
					movie.Genre="";
					movie.Cast="";
				}
					//movie.File=
				//movie.Path=
				//movie.Top250=
				//movie.WritingCredits=
				//movie.CDLabel=
				//movie.Database=
				//movie.DVDLabel=
				//movie.IMDBNumber=
				//movie.SearchString=
				unchecked
				{
					if (cbWatched.Checked)
						movie.Watched=1;
					else
						movie.Watched=0;
					movie.Title=tbTitle.Text;
					movie.Director=tbDirector.Text;
					movie.MPARating=tbMPAARating.Text;
					movie.RunTime=Int32.Parse(tbDuration.Text);
					movie.WritingCredits=tbDirector.Text;
					movie.Plot=tbDescription.Text;
					movie.Rating=(float)Double.Parse(tbRating.Text);
					movie.TagLine=tbTagline.Text;
					movie.Year=Int32.Parse(tbYear.Text);
					movie.ThumbURL=textBoxPictureURL.Text;
					movie.Votes=tbVotes.Text;
					movie.PlotOutline=tbPlotOutline.Text;
				}
				foreach (ListViewItem item in listViewGenres.Items)
				{
					if (movie.Genre==String.Empty) movie.Genre=item.Text;
					else movie.Genre += " / " + item.Text;
				}

				foreach (ListViewItem item in listViewMovieActors.Items)
				{
					string actor=item.SubItems[0].Text+" as " + item.SubItems[1].Text;
					if (movie.Cast==String.Empty) movie.Cast=actor;
					else movie.Cast += "\n" + actor;
				}
				return movie;
			}
		}
	}
}

