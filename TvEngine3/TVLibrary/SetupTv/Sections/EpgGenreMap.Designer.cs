namespace SetupTv.Sections
{
  partial class EpgGenreMap
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EpgGenreMap));
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpButtonGenreIsMovie = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonEnableGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.label26 = new System.Windows.Forms.Label();
      this.listViewGuideGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.colGuideGenreName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colIsMovie = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.buttonMapGenres = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUnmapGenres = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewProgramGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.listViewMappedGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpButtonGenreIsMovie);
      this.tabPage1.Controls.Add(this.mpButtonEnableGenre);
      this.tabPage1.Controls.Add(this.label26);
      this.tabPage1.Controls.Add(this.listViewGuideGenres);
      this.tabPage1.Controls.Add(this.buttonMapGenres);
      this.tabPage1.Controls.Add(this.buttonUnmapGenres);
      this.tabPage1.Controls.Add(this.listViewProgramGenres);
      this.tabPage1.Controls.Add(this.listViewMappedGenres);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(477, 405);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "EPG Genre Map";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpButtonGenreIsMovie
      // 
      this.mpButtonGenreIsMovie.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonGenreIsMovie.Location = new System.Drawing.Point(364, 125);
      this.mpButtonGenreIsMovie.MinimumSize = new System.Drawing.Size(36, 22);
      this.mpButtonGenreIsMovie.Name = "mpButtonGenreIsMovie";
      this.mpButtonGenreIsMovie.Size = new System.Drawing.Size(100, 23);
      this.mpButtonGenreIsMovie.TabIndex = 90;
      this.mpButtonGenreIsMovie.Text = "Toggle movie";
      this.mpButtonGenreIsMovie.UseVisualStyleBackColor = true;
      this.mpButtonGenreIsMovie.Click += new System.EventHandler(this.mpButtonGenreIsMovie_Click);
      // 
      // mpButtonEnableGenre
      // 
      this.mpButtonEnableGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonEnableGenre.Location = new System.Drawing.Point(364, 154);
      this.mpButtonEnableGenre.Name = "mpButtonEnableGenre";
      this.mpButtonEnableGenre.Size = new System.Drawing.Size(100, 23);
      this.mpButtonEnableGenre.TabIndex = 89;
      this.mpButtonEnableGenre.Text = "Toggle enabled";
      this.mpButtonEnableGenre.UseVisualStyleBackColor = true;
      this.mpButtonEnableGenre.Click += new System.EventHandler(this.mpButtonEnableGenre_Click);
      // 
      // label26
      // 
      this.label26.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label26.Location = new System.Drawing.Point(10, 10);
      this.label26.Name = "label26";
      this.label26.Size = new System.Drawing.Size(457, 79);
      this.label26.TabIndex = 88;
      this.label26.Text = resources.GetString("label26.Text");
      // 
      // listViewGuideGenres
      // 
      this.listViewGuideGenres.AllowDrop = true;
      this.listViewGuideGenres.AllowRowReorder = true;
      this.listViewGuideGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewGuideGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGuideGenreName,
            this.colIsMovie,
            this.colEnabled});
      this.listViewGuideGenres.HideSelection = false;
      this.listViewGuideGenres.IsChannelListView = false;
      this.listViewGuideGenres.LabelEdit = true;
      this.listViewGuideGenres.Location = new System.Drawing.Point(10, 101);
      this.listViewGuideGenres.MultiSelect = false;
      this.listViewGuideGenres.Name = "listViewGuideGenres";
      this.listViewGuideGenres.OwnerDraw = true;
      this.listViewGuideGenres.Size = new System.Drawing.Size(350, 130);
      this.listViewGuideGenres.TabIndex = 20;
      this.listViewGuideGenres.UseCompatibleStateImageBehavior = false;
      this.listViewGuideGenres.View = System.Windows.Forms.View.Details;
      this.listViewGuideGenres.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewGuideGenres_AfterLabelEdit);
      this.listViewGuideGenres.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewGuideGenres_DrawColumnHeader);
      this.listViewGuideGenres.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewGuideGenres_DrawSubItem);
      this.listViewGuideGenres.SelectedIndexChanged += new System.EventHandler(this.listViewGuideGenres_SelectedIndexChanged);
      // 
      // colGuideGenreName
      // 
      this.colGuideGenreName.Text = "MediaPortal Genres";
      this.colGuideGenreName.Width = 200;
      // 
      // colIsMovie
      // 
      this.colIsMovie.Text = "Movie";
      this.colIsMovie.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // colEnabled
      // 
      this.colEnabled.Text = "Enabled";
      this.colEnabled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // buttonMapGenres
      // 
      this.buttonMapGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonMapGenres.Location = new System.Drawing.Point(220, 295);
      this.buttonMapGenres.Name = "buttonMapGenres";
      this.buttonMapGenres.Size = new System.Drawing.Size(36, 23);
      this.buttonMapGenres.TabIndex = 18;
      this.buttonMapGenres.Text = "<<";
      this.buttonMapGenres.UseVisualStyleBackColor = true;
      this.buttonMapGenres.Click += new System.EventHandler(this.buttonMapGenres_Click);
      // 
      // buttonUnmapGenres
      // 
      this.buttonUnmapGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUnmapGenres.Location = new System.Drawing.Point(220, 335);
      this.buttonUnmapGenres.Name = "buttonUnmapGenres";
      this.buttonUnmapGenres.Size = new System.Drawing.Size(36, 23);
      this.buttonUnmapGenres.TabIndex = 17;
      this.buttonUnmapGenres.Text = ">>";
      this.buttonUnmapGenres.UseVisualStyleBackColor = true;
      this.buttonUnmapGenres.Click += new System.EventHandler(this.buttonUnmapGenres_Click);
      // 
      // listViewProgramGenres
      // 
      this.listViewProgramGenres.AllowDrop = true;
      this.listViewProgramGenres.AllowRowReorder = true;
      this.listViewProgramGenres.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewProgramGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12});
      this.listViewProgramGenres.FullRowSelect = true;
      this.listViewProgramGenres.HideSelection = false;
      this.listViewProgramGenres.IsChannelListView = false;
      this.listViewProgramGenres.Location = new System.Drawing.Point(277, 244);
      this.listViewProgramGenres.Name = "listViewProgramGenres";
      this.listViewProgramGenres.Size = new System.Drawing.Size(190, 150);
      this.listViewProgramGenres.TabIndex = 19;
      this.listViewProgramGenres.UseCompatibleStateImageBehavior = false;
      this.listViewProgramGenres.View = System.Windows.Forms.View.Details;
      this.listViewProgramGenres.DoubleClick += new System.EventHandler(this.buttonMapGenres_Click);
      // 
      // columnHeader12
      // 
      this.columnHeader12.Text = "Unmapped Program Genres";
      this.columnHeader12.Width = 155;
      // 
      // listViewMappedGenres
      // 
      this.listViewMappedGenres.AllowDrop = true;
      this.listViewMappedGenres.AllowRowReorder = true;
      this.listViewMappedGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewMappedGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader13});
      this.listViewMappedGenres.FullRowSelect = true;
      this.listViewMappedGenres.HideSelection = false;
      this.listViewMappedGenres.IsChannelListView = false;
      this.listViewMappedGenres.LargeImageList = this.imageList1;
      this.listViewMappedGenres.Location = new System.Drawing.Point(10, 244);
      this.listViewMappedGenres.Name = "listViewMappedGenres";
      this.listViewMappedGenres.Size = new System.Drawing.Size(190, 150);
      this.listViewMappedGenres.SmallImageList = this.imageList1;
      this.listViewMappedGenres.TabIndex = 16;
      this.listViewMappedGenres.UseCompatibleStateImageBehavior = false;
      this.listViewMappedGenres.View = System.Windows.Forms.View.Details;
      this.listViewMappedGenres.DoubleClick += new System.EventHandler(this.buttonUnmapGenres_Click);
      // 
      // columnHeader13
      // 
      this.columnHeader13.Text = "Mapped Program Genres";
      this.columnHeader13.Width = 155;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "QuestionMark.png");
      this.imageList1.Images.SetKeyName(1, "enable.jpg");
      this.imageList1.Images.SetKeyName(2, "disable.jpg");
      this.imageList1.Images.SetKeyName(3, "movie.jpg");
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(485, 431);
      this.tabControl1.TabIndex = 11;
      // 
      // EpgGenreMap
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "EpgGenreMap";
      this.Size = new System.Drawing.Size(491, 437);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
    private System.Windows.Forms.TabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPListView listViewGuideGenres;
    private System.Windows.Forms.ColumnHeader colGuideGenreName;
    private MediaPortal.UserInterface.Controls.MPButton buttonMapGenres;
    private MediaPortal.UserInterface.Controls.MPButton buttonUnmapGenres;
    private MediaPortal.UserInterface.Controls.MPListView listViewProgramGenres;
    private System.Windows.Forms.ColumnHeader columnHeader12;
    private MediaPortal.UserInterface.Controls.MPListView listViewMappedGenres;
    private System.Windows.Forms.ColumnHeader columnHeader13;
    private System.Windows.Forms.Label label26;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.ColumnHeader colEnabled;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonGenreIsMovie;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEnableGenre;
    private System.Windows.Forms.ColumnHeader colIsMovie;
  }
}