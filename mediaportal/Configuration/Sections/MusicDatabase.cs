using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
	public class MusicDatabase : MediaPortal.Configuration.SectionSettings
	{
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckedListBox sharesListBox;
    private System.Windows.Forms.Button startButton;
    private System.Windows.Forms.CheckBox buildThumbsCheckBox;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label countLabel;
    private System.Windows.Forms.Label fileLabel;
    private System.Windows.Forms.Button cancelButton;
		private System.ComponentModel.IContainer components = null;

    internal class MusicData
    {
      public string FilePath;
      public MusicTag Tag;

      public MusicData(string filePath, MusicTag tag)
      {
        this.FilePath = filePath;
        this.Tag = tag;
      }
    }

    bool stopRebuild = false;
    ArrayList extractedTags;
    ArrayList availableFiles;
    string strLastThumb = "";

    // This should really be in a common place?
    const string AlbumThumbsFolder = @"thumbs\music\albums";

    public MusicDatabase() :  this("Music Database")
    {
    }

    public MusicDatabase(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

      groupBox2.Enabled = false;
		}

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }
    string[] extensions = new string[] { ".mp3" };

    public override void OnSectionActivated()
    {
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load selected shares
      //
      SectionSettings section = SectionSettings.GetSection("MusicShares");

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
      section = SectionSettings.GetSection("MusicExtensions");

      if(section != null)
      {
        string extensions = (string)section.GetSetting("extensions");
        Extensions = extensions.Split(new char[] { ',' });
      }

      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        buildThumbsCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "buildThumbs", false);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("musicfiles", "buildThumbs", buildThumbsCheckBox.Checked);
      }
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
			this.buildThumbsCheckBox = new System.Windows.Forms.CheckBox();
			this.startButton = new System.Windows.Forms.Button();
			this.sharesListBox = new System.Windows.Forms.CheckedListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.fileLabel = new System.Windows.Forms.Label();
			this.countLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.buildThumbsCheckBox);
			this.groupBox1.Controls.Add(this.startButton);
			this.groupBox1.Controls.Add(this.sharesListBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 216);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Music Database";
			// 
			// buildThumbsCheckBox
			// 
			this.buildThumbsCheckBox.Checked = true;
			this.buildThumbsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.buildThumbsCheckBox.Location = new System.Drawing.Point(16, 152);
			this.buildThumbsCheckBox.Name = "buildThumbsCheckBox";
			this.buildThumbsCheckBox.Size = new System.Drawing.Size(264, 16);
			this.buildThumbsCheckBox.TabIndex = 3;
			this.buildThumbsCheckBox.Text = "Use coverart embedded in MP3s for thumbnails";
			// 
			// startButton
			// 
			this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.startButton.Location = new System.Drawing.Point(16, 176);
			this.startButton.Name = "startButton";
			this.startButton.Size = new System.Drawing.Size(408, 23);
			this.startButton.TabIndex = 0;
			this.startButton.Text = "Update music database from selected shares";
			this.startButton.Click += new System.EventHandler(this.startButton_Click);
			// 
			// sharesListBox
			// 
			this.sharesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.sharesListBox.CheckOnClick = true;
			this.sharesListBox.Location = new System.Drawing.Point(16, 40);
			this.sharesListBox.Name = "sharesListBox";
			this.sharesListBox.Size = new System.Drawing.Size(408, 94);
			this.sharesListBox.TabIndex = 2;
			this.sharesListBox.SelectedIndexChanged += new System.EventHandler(this.sharesListBox_SelectedIndexChanged);
			this.sharesListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListBox_ItemCheck);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(176, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "Music Folders";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.cancelButton);
			this.groupBox2.Controls.Add(this.fileLabel);
			this.groupBox2.Controls.Add(this.countLabel);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.progressBar);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 256);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 112);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Progress";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(349, 27);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// fileLabel
			// 
			this.fileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.fileLabel.Location = new System.Drawing.Point(16, 56);
			this.fileLabel.Name = "fileLabel";
			this.fileLabel.Size = new System.Drawing.Size(408, 23);
			this.fileLabel.TabIndex = 3;
			// 
			// countLabel
			// 
			this.countLabel.Location = new System.Drawing.Point(96, 32);
			this.countLabel.Name = "countLabel";
			this.countLabel.Size = new System.Drawing.Size(192, 23);
			this.countLabel.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 23);
			this.label2.TabIndex = 1;
			this.label2.Text = "Scanned files:";
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(16, 80);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(408, 16);
			this.progressBar.TabIndex = 0;
			// 
			// MusicDatabase
			// 
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "MusicDatabase";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
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
            SetStatus("Scanning files for valid tags");
            extractedTags = new ArrayList(totalFiles);
            ScanFiles(totalFiles);
            rebuildState = RebuildState.Updating; 
            break;
          }

          case RebuildState.Updating:
          {
            SetStatus("Updating database with tag information from " + extractedTags.Count + " files");
            UpdateDatabase();
            rebuildState = RebuildState.Done; 
            break;
          }

          case RebuildState.Done:
            SetStatus("Database has been successfully rebuilt");
            stopRebuild = true;
            rebuildState = RebuildState.None;
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
        ScanFile(file);

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
    private void ScanFile(string file)
    {
      //
      // Don't scan playlists
      //
      if(PlayListFactory.IsPlayList(file) == false)
      {
        //
        // Don't load tags from CD's
        //
        string fileExtension = Path.GetExtension(file).ToLower();
        if(fileExtension.Equals(".cda") == false)
        {
          //
          // Use tag reader to extract information.  Added ref to embedded image, which
          // adds little overhead as the image is always parsed out (at least for id3).
          //
          byte[] imageBytes = null;
          MusicTag tag = TagReader.TagReader.ReadTag( file, ref imageBytes );

          if(tag != null)
          {
            //
            // Add to list of extracted tags
            //
            extractedTags.Add(new MusicData(file, tag));

            //
            // Create thumbnail for the album
            //
            if ( buildThumbsCheckBox.Checked )
            {
              // What to call the thumb?
              string name = String.Format( "{0}-{1}", tag.Artist, tag.Album );
              string strSmallThumb =  Utils.GetCoverArtName( AlbumThumbsFolder, name );

              if ( strSmallThumb != strLastThumb )
              {
                // Use the folder.jpg if it's there
                string strImageFile = Utils.GetFolderThumb( file );

                try
                {
                  if ( System.IO.File.Exists( strImageFile ) )
                  {
                    MediaPortal.Util.Picture.CreateThumbnail( strImageFile, strSmallThumb, 128, 128, 0 );
                    // MediaPortal.Util.Picture.CreateThumbnail( strImageFile, strLargeThumb, 512, 512, 0 );

                    strLastThumb = strSmallThumb;
                  }
                  else if ( imageBytes != null )
                  {
                    MemoryStream memoryStream = new MemoryStream( imageBytes );
                    Image image = Image.FromStream( memoryStream );

                    MediaPortal.Util.Picture.CreateThumbnail( image, strSmallThumb, 128, 128, 0 );
                    // MediaPortal.Util.Picture.CreateThumbnail( image, strLargeThumb, 512, 512, 0 );

                    strLastThumb = strSmallThumb;
                  }
                }
                catch ( Exception )
                {
                  // Just skip this one
                  Log.Write( "Invalid image in {0}", file );
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateDatabase()
    {
      int totalFiles = extractedTags.Count;
      int currentFile = 0;

      progressBar.Value = 0;
      progressBar.Maximum = totalFiles;

      //
      // Setup database
      //
      MediaPortal.Music.Database.MusicDatabase database = null;
      
      try
      {
        //
        // Allocate and open database
        //
        database = new MediaPortal.Music.Database.MusicDatabase();
        database.BeginTransaction();

        foreach(MusicData data in extractedTags)
        {
          if(stopRebuild)
          {
            database.RollbackTransaction();
            return;
          }

          Song song         = new Song();

          song.FileName     = data.FilePath;

          song.Title		    = data.Tag.Title;
          song.Genre		    = data.Tag.Genre;
          song.Artist	      = data.Tag.Artist;
          song.Album		    = data.Tag.Album;
          song.Year			    =	data.Tag.Year;
          song.Track			  = data.Tag.Track;
          song.Duration	    = data.Tag.Duration;

          //
          // Add to database
          //
          database.AddSong(song, true);

          SetCount(++currentFile, totalFiles);
          progressBar.PerformStep();
        }

        database.CommitTransaction();

        extractedTags.Clear();
      }
      catch
      {
        if(database != null)
          database.RollbackTransaction();
      }
      finally
      {
      }
    }

    private void clearButton_Click(object sender, System.EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the entire music database?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if(dialogResult == DialogResult.Yes)
      {
        string database = @"database\musicdatabase2.db";
        if(File.Exists(database))
        {
          File.Delete(database);
        }

        MessageBox.Show("Music database has been cleared");
      }
    }
	}
}

