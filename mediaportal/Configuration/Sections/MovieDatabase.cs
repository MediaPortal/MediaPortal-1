using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
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
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckedListBox sharesListBox;
    private System.Windows.Forms.Button startButton;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label countLabel;
    private System.Windows.Forms.Label fileLabel;
    private System.Windows.Forms.Button cancelButton;
		private System.ComponentModel.IContainer components = null;

		const string TitleThumbsFolder=@"thumbs\Videos\Title";
		const string ActorThumbsFolder=@"thumbs\Videos\Actors";

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
		private System.Windows.Forms.Label labelLine1;
		private System.Windows.Forms.Label labelLine2;
    ArrayList availableFiles;

    public MovieDatabase() :  this("Movie Database")
    {
    }

    public MovieDatabase(string name) : base(name)
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
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.fileLabel = new System.Windows.Forms.Label();
			this.countLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.labelLine1 = new System.Windows.Forms.Label();
			this.labelLine2 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.startButton);
			this.groupBox1.Controls.Add(this.sharesListBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 168);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Movie Database";
			// 
			// startButton
			// 
			this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.startButton.Location = new System.Drawing.Point(16, 128);
			this.startButton.Name = "startButton";
			this.startButton.Size = new System.Drawing.Size(408, 23);
			this.startButton.TabIndex = 0;
			this.startButton.Text = "Update movie database from selected shares";
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
			this.sharesListBox.Size = new System.Drawing.Size(408, 79);
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
			this.label1.Text = "Movie Folders";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.labelLine2);
			this.groupBox2.Controls.Add(this.labelLine1);
			this.groupBox2.Controls.Add(this.cancelButton);
			this.groupBox2.Controls.Add(this.fileLabel);
			this.groupBox2.Controls.Add(this.countLabel);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.progressBar);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 184);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 224);
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
			this.fileLabel.Location = new System.Drawing.Point(16, 72);
			this.fileLabel.Name = "fileLabel";
			this.fileLabel.Size = new System.Drawing.Size(408, 16);
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
			this.progressBar.Location = new System.Drawing.Point(16, 96);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(408, 16);
			this.progressBar.TabIndex = 0;
			// 
			// labelLine1
			// 
			this.labelLine1.Location = new System.Drawing.Point(16, 128);
			this.labelLine1.Name = "labelLine1";
			this.labelLine1.Size = new System.Drawing.Size(400, 16);
			this.labelLine1.TabIndex = 4;
			// 
			// labelLine2
			// 
			this.labelLine2.Location = new System.Drawing.Point(16, 160);
			this.labelLine2.Name = "labelLine2";
			this.labelLine2.Size = new System.Drawing.Size(400, 16);
			this.labelLine2.TabIndex = 5;
			// 
			// MovieDatabase
			// 
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "MovieDatabase";
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
			if(stopRebuild)  return;

			string ext=System.IO.Path.GetExtension(file.ToLower());
			if (ext==".ifo") return;
			if (ext==".vob") return;
			IMDBMovie movieDetails=new IMDBMovie();
			IMDB imdb = new IMDB(this);
			int selectedItem=0;
			int id=VideoDatabase.GetMovieInfo(file,ref movieDetails);
			if (id<0) 
			{
				Application.DoEvents();
				imdb.Find( Utils.GetFilename(file));
				Application.DoEvents();
				if(stopRebuild)  return;
				if (imdb.Count<=0) return;
				if (imdb.Count>0)
				{
					DlgMovieList dlg = new DlgMovieList();
					dlg.Filename=file;
					for (int i=0; i < imdb.Count;++i)
						dlg.AddMovie(imdb[i].Title);
					if (dlg.ShowDialog() == DialogResult.Cancel) return;
					selectedItem=dlg.SelectedItem;
				}

				if (stopRebuild) return;
				if ( imdb.GetDetails(imdb[selectedItem],ref movieDetails))
				{
					if(stopRebuild)  return;
					Application.DoEvents();
					id=VideoDatabase.AddMovie(file,false);
					VideoDatabase.SetMovieInfo(file,ref movieDetails);

					if (stopRebuild) return;
					//download thumbnail
					DownloadThumnail(TitleThumbsFolder,movieDetails.ThumbURL,movieDetails.Title);
					
					if (stopRebuild) return;
					Application.DoEvents();
				}
			}
			if (id>=0)
			{
				//"Cast overview:\nNaomi Watts as Rachel Keller\nMartin Henderson as Noah Clay\nDavid Dorfman as Aidan Keller\nBrian Cox as Richard Morgan\nJane Alexander as Dr. Grasnik\nLindsay Frost as Ruth Embry\nAmber Tamblyn as Katie Embry\nRachael Bella as Rebecca ''Becca'' Kotler\nDaveigh Chase as Samara Morgan\nShannon Cochran as Anna Morgan\nSandra Thigpen as Teacher\nRichard Lineback as Innkeeper\nSasha Barrese as Girl Teen #1\nTess Hall as Girl Teen #2\nAdam Brody as Kellen, Male Teen #1"
				DownloadActors(movieDetails);
				DownloadDirector(movieDetails);
			}
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
			string strThumb = Utils.GetCoverArtName(ActorThumbsFolder,actor);
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
						DownloadThumnail(ActorThumbsFolder,imdbActor.ThumbnailUrl,actor);
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
					string strThumb = Utils.GetCoverArtName(ActorThumbsFolder,actor);
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
								DownloadThumnail(ActorThumbsFolder,imdbActor.ThumbnailUrl,actor);
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
        string database = @"database\VideoDatabaseV4.db";
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

	}
}

