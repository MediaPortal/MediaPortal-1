#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Threading;
using MediaPortal.Utils.Services;
using MediaPortal.GUI.Library;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Video.Database;
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class MovieDatabase : MediaPortal.Configuration.SectionSettings, IMDB.IProgress
  {
    private class MovieTitleComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        IMDBMovie movie1 = x as IMDBMovie;
        IMDBMovie movie2 = y as IMDBMovie;
        return movie1.Title.CompareTo(movie2.Title);
      }

      #endregion

    }

    internal class ComboBoxItemDatabase
    {
      public string database;
      public string language;
      public string limit;

      public ComboBoxItemDatabase(string database, string language, string limit)
      {
        this.database = database;
        this.language = language;
        this.limit = limit;
      }
      public override string ToString()
      {
        return String.Format("{0} ({1})", database, language);
      }
    }
    // The LVI being edited
    private ListViewItem _editItem;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.CheckedListBox sharesListBox;
    private MediaPortal.UserInterface.Controls.MPButton startButton;
    private System.ComponentModel.IContainer components = null;

    private DlgProgress progressDialog = new DlgProgress();
    private string newMovieToFind = string.Empty;
    internal class ComboBoxItemMovie
    {
      public string Title;
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
      public string Title;
      public string URL;

      public ComboBoxArt(string title, string url)
      {
        this.Title = title;
        this.URL = url;
      }
      public override string ToString()
      {
        return Title;
      }
    }

    bool _isFuzzyMatching = true;
    //ArrayList extractedTags;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl2;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage4;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage5;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage6;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage7;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPButton buttonImport;
    private MediaPortal.UserInterface.Controls.MPButton btnDelete;
    private MediaPortal.UserInterface.Controls.MPTextBox tbWritingCredits;
    private MediaPortal.UserInterface.Controls.MPLabel label18;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPlotOutline;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPTextBox tbVotes;
    private MediaPortal.UserInterface.Controls.MPLabel label16;
    private MediaPortal.UserInterface.Controls.MPButton buttonLookupMovie;
    private MediaPortal.UserInterface.Controls.MPButton btnSave;
    private MediaPortal.UserInterface.Controls.MPTextBox tbTagline;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbWatched;
    private MediaPortal.UserInterface.Controls.MPTextBox tbDescription;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTitle;
    private MediaPortal.UserInterface.Controls.MPTextBox tbMPAARating;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPTextBox tbYear;
    private MediaPortal.UserInterface.Controls.MPLabel label10;
    private MediaPortal.UserInterface.Controls.MPTextBox tbDirector;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private MediaPortal.UserInterface.Controls.MPTextBox tbDuration;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPTextBox tbRating;
    private MediaPortal.UserInterface.Controls.MPTextBox tbTitle;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxNewGenre;
    private MediaPortal.UserInterface.Controls.MPButton btnDeleteGenre;
    private MediaPortal.UserInterface.Controls.MPButton buttonNewGenre;
    private MediaPortal.UserInterface.Controls.MPButton buttonUnmapGenre;
    private MediaPortal.UserInterface.Controls.MPButton buttonMapGenre;
    private System.Windows.Forms.ListView listViewGenres;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ListView listViewAllGenres;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxNewActor;
    private MediaPortal.UserInterface.Controls.MPButton buttonDeleteActor;
    private MediaPortal.UserInterface.Controls.MPButton buttonNewActor;
    private MediaPortal.UserInterface.Controls.MPButton buttonUnmapActors;
    private MediaPortal.UserInterface.Controls.MPButton buttonMapActors;
    private System.Windows.Forms.ListView listViewMovieActors;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ListView listViewAllActors;
    private System.Windows.Forms.ColumnHeader chName;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox6;
    private MediaPortal.UserInterface.Controls.MPButton buttonRemoveFile;
    private MediaPortal.UserInterface.Controls.MPButton buttonAddFile;
    private System.Windows.Forms.ListView listViewFiles;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox7;
    private MediaPortal.UserInterface.Controls.MPButton btnAmazon;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxPictures;
    private MediaPortal.UserInterface.Controls.MPButton btnLookupImage;
    private MediaPortal.UserInterface.Controls.MPLabel label15;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPictureURL;
    private System.Windows.Forms.PictureBox pictureBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButton2;
    private TabPage tabPage8;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPButton mpButton1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBox1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPButton bDatabaseDown;
    private MediaPortal.UserInterface.Controls.MPButton bDatabaseUp;
    private ListView lvDatabase;
    private ColumnHeader chDatabaseDB;
    private ColumnHeader chDatabaseLanguage;
    private ColumnHeader chDatabaseLimit;
    private MediaPortal.UserInterface.Controls.MPTextBox listViewTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox _fuzzyMatchingCheckBox;
    bool _scanning = false;
    ArrayList conflictFiles = new ArrayList();

    public MovieDatabase()
      : this("Movie Database")
    {
    }

    public MovieDatabase(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      cbTitle.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }
    string[] extensions = new string[] { ".avi" };

    public override void OnSectionActivated()
    {
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load selected shares
      //
      SectionSettings section = SectionSettings.GetSection("MovieShares");

      if (section != null)
      {
        ArrayList shares = (ArrayList)section.GetSetting("shares");

        foreach (string share in shares)
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

      if (section != null)
      {
        string extensions = (string)section.GetSetting("extensions");
        Extensions = extensions.Split(new char[] { ',' });
      }

      UpdateControlStatus();
      LoadMovies(0);
      if (cbTitle.Items.Count > 0)
        cbTitle.SelectedIndex = 0;
    }


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButton2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.startButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.sharesListBox = new System.Windows.Forms.CheckedListBox();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage8 = new System.Windows.Forms.TabPage();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._fuzzyMatchingCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpComboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bDatabaseDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.bDatabaseUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.lvDatabase = new System.Windows.Forms.ListView();
      this.chDatabaseDB = new System.Windows.Forms.ColumnHeader();
      this.chDatabaseLanguage = new System.Windows.Forms.ColumnHeader();
      this.chDatabaseLimit = new System.Windows.Forms.ColumnHeader();
      this.listViewTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabControl2 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.tbWritingCredits = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label18 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbPlotOutline = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbVotes = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonLookupMovie = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.tbTagline = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbWatched = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbDescription = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbTitle = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tbMPAARating = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbYear = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbDirector = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbDuration = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbRating = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPage6 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox6 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonRemoveFile = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonAddFile = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewFiles = new System.Windows.Forms.ListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.tabPage5 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBoxNewActor = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonDeleteActor = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonNewActor = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUnmapActors = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonMapActors = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewMovieActors = new System.Windows.Forms.ListView();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.listViewAllActors = new System.Windows.Forms.ListView();
      this.chName = new System.Windows.Forms.ColumnHeader();
      this.tabPage4 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBoxNewGenre = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.btnDeleteGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonNewGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUnmapGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonMapGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewGenres = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.listViewAllGenres = new System.Windows.Forms.ListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.tabPage7 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox7 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnAmazon = new MediaPortal.UserInterface.Controls.MPButton();
      this.comboBoxPictures = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.btnLookupImage = new MediaPortal.UserInterface.Controls.MPButton();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxPictureURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage8.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabPage2.SuspendLayout();
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
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.mpButton2);
      this.groupBox1.Controls.Add(this.startButton);
      this.groupBox1.Controls.Add(this.sharesListBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(448, 192);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Scan Movie Folders";
      // 
      // mpButton2
      // 
      this.mpButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButton2.Location = new System.Drawing.Point(328, 160);
      this.mpButton2.Name = "mpButton2";
      this.mpButton2.Size = new System.Drawing.Size(104, 22);
      this.mpButton2.TabIndex = 3;
      this.mpButton2.Text = "Reset database";
      this.mpButton2.UseVisualStyleBackColor = true;
      this.mpButton2.Click += new System.EventHandler(this.clearButton_Click);
      // 
      // startButton
      // 
      this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.startButton.Location = new System.Drawing.Point(106, 160);
      this.startButton.Name = "startButton";
      this.startButton.Size = new System.Drawing.Size(216, 22);
      this.startButton.TabIndex = 1;
      this.startButton.Text = "Update database from selected shares";
      this.startButton.UseVisualStyleBackColor = true;
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
      this.sharesListBox.Size = new System.Drawing.Size(416, 109);
      this.sharesListBox.TabIndex = 0;
      this.sharesListBox.SelectedIndexChanged += new System.EventHandler(this.sharesListBox_SelectedIndexChanged);
      this.sharesListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListBox_ItemCheck);
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage8);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage8
      // 
      this.tabPage8.Controls.Add(this.groupBox2);
      this.tabPage8.Location = new System.Drawing.Point(4, 22);
      this.tabPage8.Name = "tabPage8";
      this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage8.Size = new System.Drawing.Size(464, 382);
      this.tabPage8.TabIndex = 2;
      this.tabPage8.Text = "Settings";
      this.tabPage8.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this._fuzzyMatchingCheckBox);
      this.groupBox2.Controls.Add(this.mpButton1);
      this.groupBox2.Controls.Add(this.mpComboBox1);
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.bDatabaseDown);
      this.groupBox2.Controls.Add(this.bDatabaseUp);
      this.groupBox2.Controls.Add(this.lvDatabase);
      this.groupBox2.Controls.Add(this.listViewTextBox);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(6, 15);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(448, 219);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Internet database search settings";
      // 
      // _fuzzyMatchingCheckBox
      // 
      this._fuzzyMatchingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._fuzzyMatchingCheckBox.AutoSize = true;
      this._fuzzyMatchingCheckBox.Checked = true;
      this._fuzzyMatchingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this._fuzzyMatchingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._fuzzyMatchingCheckBox.Location = new System.Drawing.Point(16, 36);
      this._fuzzyMatchingCheckBox.Name = "_fuzzyMatchingCheckBox";
      this._fuzzyMatchingCheckBox.Size = new System.Drawing.Size(205, 17);
      this._fuzzyMatchingCheckBox.TabIndex = 9;
      this._fuzzyMatchingCheckBox.Text = "Automatically select the nearest match";
      this._fuzzyMatchingCheckBox.UseVisualStyleBackColor = true;
      this._fuzzyMatchingCheckBox.CheckedChanged += new System.EventHandler(this.OnFuzzyMatchingCheckedChanged);
      // 
      // mpButton1
      // 
      this.mpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButton1.Location = new System.Drawing.Point(272, 192);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(72, 22);
      this.mpButton1.TabIndex = 7;
      this.mpButton1.Text = "Add";
      this.mpButton1.UseVisualStyleBackColor = true;
      this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // mpComboBox1
      // 
      this.mpComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpComboBox1.Location = new System.Drawing.Point(120, 193);
      this.mpComboBox1.Name = "mpComboBox1";
      this.mpComboBox1.Size = new System.Drawing.Size(122, 21);
      this.mpComboBox1.TabIndex = 6;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(13, 196);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(105, 13);
      this.mpLabel1.TabIndex = 5;
      this.mpLabel1.Text = "Available databases:";
      // 
      // bDatabaseDown
      // 
      this.bDatabaseDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bDatabaseDown.Location = new System.Drawing.Point(110, 165);
      this.bDatabaseDown.Name = "bDatabaseDown";
      this.bDatabaseDown.Size = new System.Drawing.Size(72, 22);
      this.bDatabaseDown.TabIndex = 4;
      this.bDatabaseDown.Text = "Down";
      this.bDatabaseDown.UseVisualStyleBackColor = true;
      this.bDatabaseDown.Click += new System.EventHandler(this.bDatabaseDown_Click);
      // 
      // bDatabaseUp
      // 
      this.bDatabaseUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bDatabaseUp.Location = new System.Drawing.Point(32, 165);
      this.bDatabaseUp.Name = "bDatabaseUp";
      this.bDatabaseUp.Size = new System.Drawing.Size(72, 22);
      this.bDatabaseUp.TabIndex = 3;
      this.bDatabaseUp.Text = "Up";
      this.bDatabaseUp.UseVisualStyleBackColor = true;
      this.bDatabaseUp.Click += new System.EventHandler(this.bDatabaseUp_Click);
      // 
      // lvDatabase
      // 
      this.lvDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lvDatabase.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chDatabaseDB,
            this.chDatabaseLanguage,
            this.chDatabaseLimit});
      this.lvDatabase.FullRowSelect = true;
      this.lvDatabase.GridLines = true;
      this.lvDatabase.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.lvDatabase.HideSelection = false;
      this.lvDatabase.Location = new System.Drawing.Point(16, 59);
      this.lvDatabase.MultiSelect = false;
      this.lvDatabase.Name = "lvDatabase";
      this.lvDatabase.Size = new System.Drawing.Size(416, 100);
      this.lvDatabase.TabIndex = 0;
      this.lvDatabase.UseCompatibleStateImageBehavior = false;
      this.lvDatabase.View = System.Windows.Forms.View.Details;
      this.lvDatabase.DoubleClick += new System.EventHandler(this.lvDatabase_DoubleClick);
      this.lvDatabase.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lvDatabase_KeyUp);
      // 
      // chDatabaseDB
      // 
      this.chDatabaseDB.Text = "Database";
      this.chDatabaseDB.Width = 70;
      // 
      // chDatabaseLanguage
      // 
      this.chDatabaseLanguage.Text = "Language";
      this.chDatabaseLanguage.Width = 70;
      // 
      // chDatabaseLimit
      // 
      this.chDatabaseLimit.Text = "Limit";
      this.chDatabaseLimit.Width = 272;
      // 
      // listViewTextBox
      // 
      this.listViewTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTextBox.Location = new System.Drawing.Point(136, 76);
      this.listViewTextBox.Name = "listViewTextBox";
      this.listViewTextBox.Size = new System.Drawing.Size(94, 20);
      this.listViewTextBox.TabIndex = 8;
      this.listViewTextBox.Visible = false;
      this.listViewTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.listViewTextBox_KeyPress);
      this.listViewTextBox.Leave += new System.EventHandler(this.listViewTextBox_Leave);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.groupBox1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(464, 382);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Scan";
      this.tabPage2.UseVisualStyleBackColor = true;
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
      this.tabPage1.UseVisualStyleBackColor = true;
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
      this.tabControl2.TabIndex = 0;
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
      this.tabPage3.UseVisualStyleBackColor = true;
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
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(440, 336);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      // 
      // buttonImport
      // 
      this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonImport.Location = new System.Drawing.Point(200, 312);
      this.buttonImport.Name = "buttonImport";
      this.buttonImport.Size = new System.Drawing.Size(72, 17);
      this.buttonImport.TabIndex = 26;
      this.buttonImport.Text = "Import";
      this.buttonImport.UseVisualStyleBackColor = true;
      this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Location = new System.Drawing.Point(360, 312);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(72, 17);
      this.btnDelete.TabIndex = 28;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // tbWritingCredits
      // 
      this.tbWritingCredits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbWritingCredits.Location = new System.Drawing.Point(88, 140);
      this.tbWritingCredits.Name = "tbWritingCredits";
      this.tbWritingCredits.Size = new System.Drawing.Size(336, 20);
      this.tbWritingCredits.TabIndex = 12;
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(8, 144);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(88, 16);
      this.label18.TabIndex = 11;
      this.label18.Text = "Writing Credits:";
      // 
      // tbPlotOutline
      // 
      this.tbPlotOutline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPlotOutline.Location = new System.Drawing.Point(88, 68);
      this.tbPlotOutline.Name = "tbPlotOutline";
      this.tbPlotOutline.Size = new System.Drawing.Size(336, 20);
      this.tbPlotOutline.TabIndex = 6;
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(8, 72);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(64, 16);
      this.label17.TabIndex = 5;
      this.label17.Text = "Plot outline:";
      // 
      // tbVotes
      // 
      this.tbVotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbVotes.Location = new System.Drawing.Point(88, 276);
      this.tbVotes.Name = "tbVotes";
      this.tbVotes.Size = new System.Drawing.Size(96, 20);
      this.tbVotes.TabIndex = 24;
      // 
      // label16
      // 
      this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label16.Location = new System.Drawing.Point(8, 280);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(40, 16);
      this.label16.TabIndex = 23;
      this.label16.Text = "Votes:";
      // 
      // buttonLookupMovie
      // 
      this.buttonLookupMovie.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookupMovie.Location = new System.Drawing.Point(352, 43);
      this.buttonLookupMovie.Name = "buttonLookupMovie";
      this.buttonLookupMovie.Size = new System.Drawing.Size(72, 22);
      this.buttonLookupMovie.TabIndex = 4;
      this.buttonLookupMovie.Text = "Lookup";
      this.buttonLookupMovie.UseVisualStyleBackColor = true;
      this.buttonLookupMovie.Click += new System.EventHandler(this.buttonLookupMovie_Click);
      // 
      // btnSave
      // 
      this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.Location = new System.Drawing.Point(280, 312);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(72, 17);
      this.btnSave.TabIndex = 27;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // tbTagline
      // 
      this.tbTagline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbTagline.Location = new System.Drawing.Point(88, 92);
      this.tbTagline.Name = "tbTagline";
      this.tbTagline.Size = new System.Drawing.Size(336, 20);
      this.tbTagline.TabIndex = 8;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label4.Location = new System.Drawing.Point(8, 232);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(32, 16);
      this.label4.TabIndex = 15;
      this.label4.Text = "Year:";
      // 
      // cbWatched
      // 
      this.cbWatched.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cbWatched.AutoSize = true;
      this.cbWatched.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbWatched.Location = new System.Drawing.Point(243, 279);
      this.cbWatched.Name = "cbWatched";
      this.cbWatched.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.cbWatched.Size = new System.Drawing.Size(98, 17);
      this.cbWatched.TabIndex = 25;
      this.cbWatched.Text = "Watched          ";
      this.cbWatched.UseVisualStyleBackColor = true;
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
      this.tbDescription.TabIndex = 14;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(32, 16);
      this.label3.TabIndex = 2;
      this.label3.Text = "Title:";
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(8, 168);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(64, 16);
      this.label13.TabIndex = 13;
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
      this.cbTitle.TabIndex = 1;
      this.cbTitle.SelectedIndexChanged += new System.EventHandler(this.cbTitle_SelectedIndexChanged);
      // 
      // tbMPAARating
      // 
      this.tbMPAARating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.tbMPAARating.Location = new System.Drawing.Point(328, 252);
      this.tbMPAARating.Name = "tbMPAARating";
      this.tbMPAARating.Size = new System.Drawing.Size(96, 20);
      this.tbMPAARating.TabIndex = 22;
      // 
      // label11
      // 
      this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label11.Location = new System.Drawing.Point(248, 256);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(80, 16);
      this.label11.TabIndex = 21;
      this.label11.Text = "MPAA Rating:";
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.Location = new System.Drawing.Point(248, 232);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(40, 16);
      this.label6.TabIndex = 17;
      this.label6.Text = "Rating:";
      // 
      // tbYear
      // 
      this.tbYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbYear.Location = new System.Drawing.Point(88, 228);
      this.tbYear.Name = "tbYear";
      this.tbYear.Size = new System.Drawing.Size(96, 20);
      this.tbYear.TabIndex = 16;
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(8, 96);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(48, 16);
      this.label10.TabIndex = 7;
      this.label10.Text = "Tagline:";
      // 
      // tbDirector
      // 
      this.tbDirector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbDirector.Location = new System.Drawing.Point(88, 116);
      this.tbDirector.Name = "tbDirector";
      this.tbDirector.Size = new System.Drawing.Size(336, 20);
      this.tbDirector.TabIndex = 10;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(8, 120);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(48, 16);
      this.label9.TabIndex = 9;
      this.label9.Text = "Director:";
      // 
      // tbDuration
      // 
      this.tbDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbDuration.Location = new System.Drawing.Point(88, 252);
      this.tbDuration.Name = "tbDuration";
      this.tbDuration.Size = new System.Drawing.Size(96, 20);
      this.tbDuration.TabIndex = 20;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(56, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Title:";
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label8.Location = new System.Drawing.Point(8, 256);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(56, 16);
      this.label8.TabIndex = 19;
      this.label8.Text = "Duration:";
      // 
      // tbRating
      // 
      this.tbRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRating.Location = new System.Drawing.Point(328, 228);
      this.tbRating.Name = "tbRating";
      this.tbRating.Size = new System.Drawing.Size(96, 20);
      this.tbRating.TabIndex = 18;
      // 
      // tbTitle
      // 
      this.tbTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbTitle.Location = new System.Drawing.Point(88, 44);
      this.tbTitle.Name = "tbTitle";
      this.tbTitle.Size = new System.Drawing.Size(256, 20);
      this.tbTitle.TabIndex = 3;
      // 
      // tabPage6
      // 
      this.tabPage6.Controls.Add(this.groupBox6);
      this.tabPage6.Location = new System.Drawing.Point(4, 25);
      this.tabPage6.Name = "tabPage6";
      this.tabPage6.Size = new System.Drawing.Size(440, 339);
      this.tabPage6.TabIndex = 3;
      this.tabPage6.Text = "Files";
      this.tabPage6.UseVisualStyleBackColor = true;
      // 
      // groupBox6
      // 
      this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox6.Controls.Add(this.buttonRemoveFile);
      this.groupBox6.Controls.Add(this.buttonAddFile);
      this.groupBox6.Controls.Add(this.listViewFiles);
      this.groupBox6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox6.Location = new System.Drawing.Point(0, 0);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(440, 336);
      this.groupBox6.TabIndex = 0;
      this.groupBox6.TabStop = false;
      // 
      // buttonRemoveFile
      // 
      this.buttonRemoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonRemoveFile.Location = new System.Drawing.Point(352, 304);
      this.buttonRemoveFile.Name = "buttonRemoveFile";
      this.buttonRemoveFile.Size = new System.Drawing.Size(72, 22);
      this.buttonRemoveFile.TabIndex = 2;
      this.buttonRemoveFile.Text = "Remove";
      this.buttonRemoveFile.UseVisualStyleBackColor = true;
      this.buttonRemoveFile.Click += new System.EventHandler(this.buttonRemoveFile_Click);
      // 
      // buttonAddFile
      // 
      this.buttonAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddFile.Location = new System.Drawing.Point(272, 304);
      this.buttonAddFile.Name = "buttonAddFile";
      this.buttonAddFile.Size = new System.Drawing.Size(72, 22);
      this.buttonAddFile.TabIndex = 1;
      this.buttonAddFile.Text = "Add";
      this.buttonAddFile.UseVisualStyleBackColor = true;
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
      this.listViewFiles.TabIndex = 0;
      this.listViewFiles.UseCompatibleStateImageBehavior = false;
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
      this.tabPage5.UseVisualStyleBackColor = true;
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
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
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
      this.textBoxNewActor.TabIndex = 4;
      // 
      // buttonDeleteActor
      // 
      this.buttonDeleteActor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDeleteActor.Location = new System.Drawing.Point(96, 296);
      this.buttonDeleteActor.Name = "buttonDeleteActor";
      this.buttonDeleteActor.Size = new System.Drawing.Size(72, 22);
      this.buttonDeleteActor.TabIndex = 6;
      this.buttonDeleteActor.Text = "Remove";
      this.buttonDeleteActor.UseVisualStyleBackColor = true;
      this.buttonDeleteActor.Click += new System.EventHandler(this.buttonDeleteActor_Click);
      // 
      // buttonNewActor
      // 
      this.buttonNewActor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNewActor.Location = new System.Drawing.Point(16, 296);
      this.buttonNewActor.Name = "buttonNewActor";
      this.buttonNewActor.Size = new System.Drawing.Size(72, 22);
      this.buttonNewActor.TabIndex = 5;
      this.buttonNewActor.Text = "Add";
      this.buttonNewActor.UseVisualStyleBackColor = true;
      this.buttonNewActor.Click += new System.EventHandler(this.buttonNewActor_Click);
      // 
      // buttonUnmapActors
      // 
      this.buttonUnmapActors.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonUnmapActors.Location = new System.Drawing.Point(184, 152);
      this.buttonUnmapActors.Name = "buttonUnmapActors";
      this.buttonUnmapActors.Size = new System.Drawing.Size(40, 22);
      this.buttonUnmapActors.TabIndex = 2;
      this.buttonUnmapActors.Text = "<<";
      this.buttonUnmapActors.UseVisualStyleBackColor = true;
      this.buttonUnmapActors.Click += new System.EventHandler(this.buttonUnmapActors_Click);
      // 
      // buttonMapActors
      // 
      this.buttonMapActors.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonMapActors.Location = new System.Drawing.Point(184, 120);
      this.buttonMapActors.Name = "buttonMapActors";
      this.buttonMapActors.Size = new System.Drawing.Size(40, 22);
      this.buttonMapActors.TabIndex = 1;
      this.buttonMapActors.Text = ">>";
      this.buttonMapActors.UseVisualStyleBackColor = true;
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
      this.listViewMovieActors.TabIndex = 3;
      this.listViewMovieActors.UseCompatibleStateImageBehavior = false;
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
      this.listViewAllActors.TabIndex = 0;
      this.listViewAllActors.UseCompatibleStateImageBehavior = false;
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
      this.tabPage4.UseVisualStyleBackColor = true;
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
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
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
      this.textBoxNewGenre.TabIndex = 4;
      // 
      // btnDeleteGenre
      // 
      this.btnDeleteGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnDeleteGenre.Location = new System.Drawing.Point(96, 296);
      this.btnDeleteGenre.Name = "btnDeleteGenre";
      this.btnDeleteGenre.Size = new System.Drawing.Size(72, 22);
      this.btnDeleteGenre.TabIndex = 6;
      this.btnDeleteGenre.Text = "Remove";
      this.btnDeleteGenre.UseVisualStyleBackColor = true;
      this.btnDeleteGenre.Click += new System.EventHandler(this.btnDeleteGenre_Click);
      // 
      // buttonNewGenre
      // 
      this.buttonNewGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNewGenre.Location = new System.Drawing.Point(16, 296);
      this.buttonNewGenre.Name = "buttonNewGenre";
      this.buttonNewGenre.Size = new System.Drawing.Size(72, 22);
      this.buttonNewGenre.TabIndex = 5;
      this.buttonNewGenre.Text = "Add";
      this.buttonNewGenre.UseVisualStyleBackColor = true;
      this.buttonNewGenre.Click += new System.EventHandler(this.buttonNewGenre_Click);
      // 
      // buttonUnmapGenre
      // 
      this.buttonUnmapGenre.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonUnmapGenre.Location = new System.Drawing.Point(184, 152);
      this.buttonUnmapGenre.Name = "buttonUnmapGenre";
      this.buttonUnmapGenre.Size = new System.Drawing.Size(40, 22);
      this.buttonUnmapGenre.TabIndex = 2;
      this.buttonUnmapGenre.Text = "<<";
      this.buttonUnmapGenre.UseVisualStyleBackColor = true;
      this.buttonUnmapGenre.Click += new System.EventHandler(this.buttonUnmapGenre_Click);
      // 
      // buttonMapGenre
      // 
      this.buttonMapGenre.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonMapGenre.Location = new System.Drawing.Point(184, 120);
      this.buttonMapGenre.Name = "buttonMapGenre";
      this.buttonMapGenre.Size = new System.Drawing.Size(40, 22);
      this.buttonMapGenre.TabIndex = 1;
      this.buttonMapGenre.Text = ">>";
      this.buttonMapGenre.UseVisualStyleBackColor = true;
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
      this.listViewGenres.TabIndex = 3;
      this.listViewGenres.UseCompatibleStateImageBehavior = false;
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
      this.listViewAllGenres.TabIndex = 0;
      this.listViewAllGenres.UseCompatibleStateImageBehavior = false;
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
      this.tabPage7.UseVisualStyleBackColor = true;
      // 
      // groupBox7
      // 
      this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox7.Controls.Add(this.btnAmazon);
      this.groupBox7.Controls.Add(this.comboBoxPictures);
      this.groupBox7.Controls.Add(this.btnLookupImage);
      this.groupBox7.Controls.Add(this.label15);
      this.groupBox7.Controls.Add(this.textBoxPictureURL);
      this.groupBox7.Controls.Add(this.pictureBox1);
      this.groupBox7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox7.Location = new System.Drawing.Point(0, 0);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(440, 336);
      this.groupBox7.TabIndex = 0;
      this.groupBox7.TabStop = false;
      // 
      // btnAmazon
      // 
      this.btnAmazon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAmazon.Location = new System.Drawing.Point(16, 44);
      this.btnAmazon.Name = "btnAmazon";
      this.btnAmazon.Size = new System.Drawing.Size(110, 22);
      this.btnAmazon.TabIndex = 3;
      this.btnAmazon.Text = "Search Cover Art";
      this.btnAmazon.UseVisualStyleBackColor = true;
      this.btnAmazon.Click += new System.EventHandler(this.btnAmazon_Click);
      // 
      // comboBoxPictures
      // 
      this.comboBoxPictures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxPictures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPictures.Enabled = false;
      this.comboBoxPictures.Location = new System.Drawing.Point(136, 44);
      this.comboBoxPictures.Name = "comboBoxPictures";
      this.comboBoxPictures.Size = new System.Drawing.Size(288, 21);
      this.comboBoxPictures.TabIndex = 4;
      this.comboBoxPictures.SelectedIndexChanged += new System.EventHandler(this.comboBoxPictures_SelectedIndexChanged);
      // 
      // btnLookupImage
      // 
      this.btnLookupImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLookupImage.Location = new System.Drawing.Point(352, 19);
      this.btnLookupImage.Name = "btnLookupImage";
      this.btnLookupImage.Size = new System.Drawing.Size(72, 22);
      this.btnLookupImage.TabIndex = 2;
      this.btnLookupImage.Text = "Download";
      this.btnLookupImage.UseVisualStyleBackColor = true;
      this.btnLookupImage.Click += new System.EventHandler(this.btnLookupImage_Click);
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(16, 24);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(80, 16);
      this.label15.TabIndex = 0;
      this.label15.Text = "Image-URL:";
      // 
      // textBoxPictureURL
      // 
      this.textBoxPictureURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxPictureURL.Location = new System.Drawing.Point(136, 20);
      this.textBoxPictureURL.Name = "textBoxPictureURL";
      this.textBoxPictureURL.Size = new System.Drawing.Size(208, 20);
      this.textBoxPictureURL.TabIndex = 1;
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
      // MovieDatabase
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "MovieDatabase";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage8.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tabPage6.ResumeLayout(false);
      this.groupBox6.ResumeLayout(false);
      this.tabPage5.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.tabPage4.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.tabPage7.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      this.groupBox7.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
      groupBox1.Enabled = false;
      RebuildDatabase();
      groupBox1.Enabled = true;

    }

    /// <summary>
    /// 
    /// </summary>
    private void RebuildDatabase()
    {
      ArrayList availablePaths = new ArrayList();
      for (int index = 0; index < sharesListBox.CheckedIndices.Count; index++)
      {
        string path = sharesListBox.Items[(int)sharesListBox.CheckedIndices[index]].ToString();
        availablePaths.Add(path);
      }
      conflictFiles = new ArrayList();
      IMDBFetcher.ScanIMDB(this, availablePaths, _isFuzzyMatching);
    }

    private void clearButton_Click(object sender, System.EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the entire video database?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if (dialogResult == DialogResult.Yes)
      {
        string database = _config.Get(Config.Options.DatabasePath) + "VideoDatabaseV5.db3";
        if (File.Exists(database))
        {
          VideoDatabase.Dispose();
          try
          {
            File.Delete(database);
          }
          catch (Exception)
          {
            MessageBox.Show("Video database could not be cleared", "Video Database", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
          }
          finally
          {
            VideoDatabase.ReOpen();
          }
        }
        MessageBox.Show("Video database has been cleared", "Video Database", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }
    #region IMDB.IProgress
    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      if (progressDialog.IsInstance(fetcher))
      {
        progressDialog.DisableCancel();
      }
      return true;
    }
    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      progressDialog.SetLine1(line1);
      progressDialog.SetLine2(line2);
      if (percent > 0)
        progressDialog.SetPercentage(percent);
      progressDialog.Update();
    }
    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      progressDialog.ResetProgress();
      progressDialog.SetHeading("Searching IMDB...");
      progressDialog.SetLine1(fetcher.MovieName);
      progressDialog.SetLine2(String.Empty);
      progressDialog.Instance = fetcher;
      return true;
    }
    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      DialogResult result = progressDialog.ShowDialog(this);
      this.Update();
      if (result == DialogResult.Cancel)
      {
        return false;
      }
      return true;
    }
    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      if (progressDialog.IsInstance(fetcher))
      {
        progressDialog.CloseProgress();
      }
      return true;
    }
    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        conflictFiles.Add(fetcher.Movie);
      }
      else
      {
        MessageBox.Show("No IMDB info found!", fetcher.MovieName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      return false;
    }
    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      progressDialog.ResetProgress();
      progressDialog.SetHeading("Downloading Movie details...");
      progressDialog.SetLine1("Downloading Movie details...");
      progressDialog.SetLine2(fetcher.MovieName);
      progressDialog.Instance = fetcher;
      return true;
    }
    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      progressDialog.Instance = fetcher;
      DialogResult result = progressDialog.ShowDialog(this);
      this.Update();
      if (result == DialogResult.Cancel)
      {
        return false;
      }
      return true;
    }
    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      if (progressDialog.IsInstance(fetcher))
      {
        progressDialog.CloseProgress();
      }
      return true;
    }
    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      progressDialog.ResetProgress();
      progressDialog.SetHeading("Downloading Actor info...");
      progressDialog.SetLine1("Downloading Actor info...");
      progressDialog.SetLine2(fetcher.MovieName);
      progressDialog.Instance = fetcher;
      return true;
    }
    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      progressDialog.Instance = fetcher;
      DialogResult result = progressDialog.ShowDialog(this);
      this.Update();
      if (result == DialogResult.Cancel)
      {
        return false;
      }
      return true;
    }
    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      return true;
    }
    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        conflictFiles.Add(fetcher.Movie);
      }
      else
      {
        MessageBox.Show("Movie details could not be found.", fetcher.MovieName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      return true;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      movieName = newMovieToFind;
      if (movieName == string.Empty)
      {
        return false;
      }
      return true;
    }
    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      if (_scanning)
      {
        conflictFiles.Add(fetcher.Movie);
        selectedMovie = -1;
        return false;
      }
      else
      {
        DlgMovieList dlg = new DlgMovieList();
        dlg.Filename = fetcher.MovieName;
        for (int i = 0; i < fetcher.Count; ++i)
          dlg.AddMovie(fetcher[i].Title);
        DialogResult result = dlg.ShowDialog(this);
        this.Update();
        if (result == DialogResult.Cancel)
        {
          selectedMovie = -1;
          return false;
        }
        selectedMovie = dlg.SelectedItem;
        if (dlg.IsNewFind)
        {
          newMovieToFind = dlg.NewTitleToFind;
          selectedMovie = -1;
        }
        return true;
      }
    }
    public bool OnScanStart(int total)
    {
      _scanning = true;
      progressDialog.Total = total;

      return true;
    }
    public bool OnScanEnd()
    {
      if (conflictFiles.Count > 0)
      {
        DlgMovieConflicts dlg = new DlgMovieConflicts();
        for (int i = 0; i < this.conflictFiles.Count; ++i)
        {
          IMDBMovie currentMovie = (IMDBMovie)conflictFiles[i];
          string strFileName = string.Empty;
          string path = currentMovie.Path;
          string filename = currentMovie.File;
          if (path != string.Empty)
          {
            if (path.EndsWith(@"\"))
            {
              path = path.Substring(0, path.Length - 1);
              currentMovie.Path = path;
            }
            if (filename.StartsWith(@"\"))
            {
              filename = filename.Substring(1);
              currentMovie.File = filename;
            }
            strFileName = path + @"\" + filename;
          }
          else
          {
            strFileName = filename;
          }
          dlg.AddMovie(strFileName);
        }
        dlg.ShowDialog(this);
      }
      _scanning = false;
      return true;
    }
    public bool OnScanIterating(int count)
    {
      progressDialog.Count = count;
      return true;
    }
    public bool OnScanIterated(int count)
    {
      progressDialog.Count = count;
      if (progressDialog.CancelScan)
      {
        return false;
      }
      return true;
    }

    #endregion


    void LoadMovies(int id)
    {
      cbTitle.Items.Clear();
      ArrayList movies = new ArrayList();
      VideoDatabase.GetMovies(ref movies);
      movies.Sort(new MovieTitleComparer());
      int i = 0;
      int index = 0;
      foreach (IMDBMovie movie in movies)
      {
        ComboBoxItemMovie newItem = new ComboBoxItemMovie(movie.Title, movie);
        cbTitle.Items.Add(newItem);
        if (id == movie.ID)
        {
          index = i;
        }
        ++i;
      }


      IMDBMovie movieNew = new IMDBMovie();
      movieNew.Title = "New...";
      ComboBoxItemMovie emptyItem = new ComboBoxItemMovie("New...", movieNew);
      cbTitle.Items.Add(emptyItem);
      cbTitle.SelectedIndex = index;
    }
    void UpdateEdit(IMDBMovie movie)
    {
      tbTitle.Text = movie.Title;
      tbTagline.Text = movie.TagLine;
      tbYear.Text = movie.Year.ToString();
      tbVotes.Text = movie.Votes;
      tbRating.Text = movie.Rating.ToString();
      tbDirector.Text = movie.Director;
      tbWritingCredits.Text = movie.WritingCredits;
      tbDescription.Text = movie.Plot;
      textBoxPictureURL.Text = movie.ThumbURL;
      tbPlotOutline.Text = movie.PlotOutline;
      tbMPAARating.Text = movie.MPARating;
      tbDuration.Text = movie.RunTime.ToString();
      if (movie.Watched > 0) cbWatched.Checked = true;
      else cbWatched.Checked = false;
      if (pictureBox1.Image != null)
      {
        pictureBox1.Image.Dispose();
        pictureBox1.Image = null;
      }
      listViewMovieActors.Items.Clear();
      listViewGenres.Items.Clear();
      listViewAllGenres.Items.Clear();
      listViewAllActors.Items.Clear();
      listViewFiles.Items.Clear();
      comboBoxPictures.Items.Clear();
      comboBoxPictures.Enabled = false;
      if (movie.ID >= 0)
      {
        string file = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, movie.Title);
        if (System.IO.File.Exists(file))
        {
          using (Image img = Image.FromFile(file))
          {
            Bitmap result = new Bitmap(img.Width, img.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
              g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
              g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
              g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            }
            pictureBox1.Image = result;
          }
        }
        char[] splitter = { '\n', ',' };
        string[] actors = movie.Cast.Split(splitter);
        if (actors.Length > 0)
        {
          for (int i = 0; i < actors.Length; ++i)
          {
            string actor;
            string role = "";
            int pos = actors[i].IndexOf(" as ");
            if (pos >= 0)
            {
              actor = actors[i].Substring(0, pos);
              role = actors[i].Substring(pos + 4);
            }
            else
              actor = actors[i];
            actor = actor.Trim();
            ListViewItem item = new ListViewItem(actor);
            item.SubItems.Add(role);
            listViewMovieActors.Items.Add(item);
          }
        }
        listViewMovieActors.Sort();

        string szGenres = movie.Genre;
        ArrayList vecGenres = new ArrayList();
        if (szGenres.IndexOf("/") >= 0)
        {
          Tokens f = new Tokens(szGenres, new char[] { '/' });
          foreach (string strGenre in f)
          {
            listViewGenres.Items.Add(strGenre.Trim());
          }
        }
        else
        {
          string strGenre = movie.Genre;
          listViewGenres.Items.Add(strGenre.Trim());
        }

        listViewGenres.Sort();
        ArrayList filenames = new ArrayList();
        VideoDatabase.GetFiles(movie.ID, ref filenames);
        foreach (string filename in filenames)
        {
          listViewFiles.Items.Add(filename);
        }
      }
      ArrayList genres = new ArrayList();
      VideoDatabase.GetGenres(genres);
      foreach (string genre in genres)
      {
        bool add = true;
        foreach (ListViewItem item in listViewGenres.Items)
        {
          if (item.Text == genre)
          {
            add = false;
            break;
          }
        }
        if (add)
          listViewAllGenres.Items.Add(genre);
      }
      listViewAllGenres.Sort();

      ArrayList listActors = new ArrayList();
      VideoDatabase.GetActors(listActors);
      foreach (string actor in listActors)
      {
        bool add = true;
        foreach (ListViewItem item in listViewMovieActors.Items)
        {
          if (item.Text == actor)
          {
            add = false;
            break;
          }
        }
        if (add)
          listViewAllActors.Items.Add(actor);
      }
      listViewAllActors.Sort();

    }

    private void cbTitle_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (cbTitle.SelectedItem == null) return;
      ComboBoxItemMovie item = (ComboBoxItemMovie)cbTitle.SelectedItem;
      UpdateEdit(item.Movie);
    }

    private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (tabControl1.SelectedTab == tabPage1)
      {
        LoadMovies(0);
      }
    }

    private void buttonMapGenre_Click(object sender, System.EventArgs e)
    {
      if (listViewAllGenres.SelectedItems == null) return;

      for (int i = 0; i < listViewAllGenres.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewAllGenres.SelectedItems[i];

        listViewGenres.Items.Add(listItem.Text);
      }

      for (int i = listViewAllGenres.SelectedItems.Count - 1; i >= 0; i--)
      {
        ListViewItem listItem = listViewAllGenres.SelectedItems[i];

        listViewAllGenres.Items.Remove(listItem);
      }
    }

    private void buttonUnmapGenre_Click(object sender, System.EventArgs e)
    {
      if (listViewAllGenres.SelectedItems == null) return;
      for (int i = 0; i < listViewGenres.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewGenres.SelectedItems[i];
        listViewAllGenres.Items.Add(listItem.Text);
      }

      for (int i = listViewGenres.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewGenres.SelectedItems[i];
        listViewGenres.Items.Remove(listItem);
      }
    }

    private void buttonMapActors_Click(object sender, System.EventArgs e)
    {
      if (listViewAllActors.SelectedItems == null) return;

      for (int i = 0; i < listViewAllActors.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewAllActors.SelectedItems[i];

        ListViewItem newItem = new ListViewItem(listItem.Text);
        newItem.SubItems.Add("");
        listViewMovieActors.Items.Add(newItem);
      }

      for (int i = listViewAllActors.SelectedItems.Count - 1; i >= 0; i--)
      {
        ListViewItem listItem = listViewAllActors.SelectedItems[i];

        listViewAllActors.Items.Remove(listItem);
      }
    }

    private void buttonUnmapActors_Click(object sender, System.EventArgs e)
    {
      if (listViewMovieActors.SelectedItems == null) return;
      for (int i = 0; i < listViewMovieActors.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewMovieActors.SelectedItems[i];
        listViewAllActors.Items.Add(listItem.Text);
      }

      for (int i = listViewMovieActors.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewMovieActors.SelectedItems[i];
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
      find_file.Title = "Find files for " + tbTitle.Text;
      if (find_file.ShowDialog(this) == DialogResult.OK)
      {
        listViewFiles.Items.Add(find_file.FileName);

      }
    }

    private void buttonRemoveFile_Click(object sender, System.EventArgs e)
    {
      if (listViewFiles.SelectedItems == null) return;
      for (int i = listViewFiles.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewFiles.SelectedItems[i];
        listViewFiles.Items.Remove(listItem);
      }
    }

    private void buttonDeleteActor_Click(object sender, System.EventArgs e)
    {
      if (listViewAllActors.SelectedItems == null) return;
      if (MessageBox.Show("Are you sure you want to delete the selected actors?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        for (int i = listViewAllActors.SelectedItems.Count - 1; i >= 0; --i)
        {
          ListViewItem listItem = listViewAllActors.SelectedItems[i];
          VideoDatabase.DeleteActor(listItem.Text);
          listViewAllActors.Items.Remove(listItem);
          string file = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, listItem.Text);
          if (System.IO.File.Exists(file))
          {
            System.IO.File.Delete(file);
          }
          file = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MovieActors, listItem.Text);
          if (System.IO.File.Exists(file))
          {
            System.IO.File.Delete(file);
          }
        }
      }
    }

    private void buttonNewActor_Click(object sender, System.EventArgs e)
    {
      if (textBoxNewActor.Text.Length == 0) return;
      VideoDatabase.AddActor(textBoxNewActor.Text);
      listViewAllActors.Items.Add(textBoxNewActor.Text);

    }

    private void btnDeleteGenre_Click(object sender, System.EventArgs e)
    {
      if (listViewAllGenres.SelectedItems == null) return;
      if (MessageBox.Show("Are you sure you want to delete the selected genres?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        for (int i = listViewAllGenres.SelectedItems.Count - 1; i >= 0; --i)
        {
          ListViewItem listItem = listViewAllGenres.SelectedItems[i];
          VideoDatabase.DeleteGenre(listItem.Text);
          listViewAllGenres.Items.Remove(listItem);
        }
      }
    }

    private void buttonNewGenre_Click(object sender, System.EventArgs e)
    {
      if (textBoxNewGenre.Text.Length == 0) return;
      VideoDatabase.AddGenre(textBoxNewGenre.Text);
      listViewAllGenres.Items.Add(textBoxNewGenre.Text);
    }

    private void buttonNewMovie_Click(object sender, System.EventArgs e)
    {
      cbTitle.SelectedItem = null;
      IMDBMovie details = new IMDBMovie();
      UpdateEdit(details);
    }

    private void buttonLookupMovie_Click(object sender, System.EventArgs e)
    {
      if (tbTitle.Text == String.Empty)
      {
        MessageBox.Show("Please enter a movie title", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); ;
        return;
      }
      buttonLookupMovie.Enabled = false;
      btnSave.Enabled = false;
      tabControl2.Enabled = false;
      tabControl1.Enabled = false;
      progressDialog.Total = 1;
      progressDialog.Count = 1;
      IMDBMovie movieDetails = CurrentMovie;
      string file = string.Empty;
      if (listViewFiles.Items.Count > 0)
      {
        file = listViewFiles.Items[0].Text;
      }
      if (file == string.Empty)
      {
        file = tbTitle.Text;
      }
      string path, filename;
      MediaPortal.Util.Utils.Split(file, out path, out filename);
      movieDetails.Path = path;
      movieDetails.File = filename;
      movieDetails.SearchString = tbTitle.Text;
      GetInfoFromIMDB(ref movieDetails, false);
      buttonLookupMovie.Enabled = true;
      btnSave.Enabled = true;
      tabControl2.Enabled = true;
      tabControl1.Enabled = true;
    }
    private void GetInfoFromIMDB(ref IMDBMovie movieDetails, bool fuzzyMatch)
    {
      string file, path, filename;
      path = movieDetails.Path;
      filename = movieDetails.File;
      if (path != string.Empty)
      {
        if (path.EndsWith(@"\"))
        {
          path = path.Substring(0, path.Length - 1);
          movieDetails.Path = path;
        }
        if (filename.StartsWith(@"\"))
        {
          filename = filename.Substring(1);
          movieDetails.File = filename;
        }
        file = path + System.IO.Path.DirectorySeparatorChar + filename;
      }
      else
      {
        file = filename;
      }

      int id = movieDetails.ID;
      if (id < 0)
      {
        ServiceProvider services = GlobalServiceProvider.Instance;
        ILog log = services.Get<ILog>();
        log.Info("Adding file:{0}", file);
        id = VideoDatabase.AddMovieFile(file);
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions( MediaPortal.Util.Utils.VideoExtensions);
        ArrayList items = dir.GetDirectoryUnProtected(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder) continue;
          if ( MediaPortal.Util.Utils.ShouldStack(item.Path, file) && item.Path != file)
          {
            string strPath, strFileName;

            MediaPortal.Database.DatabaseUtility.Split(item.Path, out strPath, out strFileName);
            MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strPath);
            MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strFileName);
            int pathId = VideoDatabase.AddPath(strPath);
            VideoDatabase.AddFile(id, pathId, strFileName);
          }
        }
        movieDetails.ID = id;
        string searchString = movieDetails.SearchString;
        VideoDatabase.SetMovieInfoById(movieDetails.ID, ref movieDetails);
        movieDetails.SearchString = searchString;
      }
      if (IMDBFetcher.RefreshIMDB(this, ref movieDetails, fuzzyMatch))
      {
        if (movieDetails != null)
        {
          LoadMovies(movieDetails.ID);
        }
      }

    }
    private void btnSave_Click(object sender, System.EventArgs e)
    {
      IMDBMovie details = CurrentMovie;
      if (details.ID >= 0)
      {
        VideoDatabase.RemoveGenresForMovie(details.ID);
        VideoDatabase.RemoveActorsForMovie(details.ID);
        VideoDatabase.RemoveFilesForMovie(details.ID);
      }
      else
      {
        string file;
        if (listViewFiles.Items.Count > 0)
        {
          file = listViewFiles.Items[0].Text;
        }
        else
        {
          file = details.Title;
        }
        if (file == string.Empty)
        {
          MessageBox.Show("Please enter a movie title or movie file", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
        details.ID = VideoDatabase.AddMovieFile(file);
        if (details.ID == -1)
        {
          MessageBox.Show("Could not save movie to database", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return;
        }
      }

      VideoDatabase.SetMovieInfoById(details.ID, ref details);

      //add files to movie
      foreach (ListViewItem item in listViewFiles.Items)
      {
        string strPath, strFileName;

        MediaPortal.Database.DatabaseUtility.Split(item.Text, out strPath, out strFileName);
        MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strPath);
        MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strFileName);

        int pathId = VideoDatabase.AddPath(strPath);
        VideoDatabase.AddFile(details.ID, pathId, strFileName);
      }
      LoadMovies(details.ID);
    }

    private void btnLookupImage_Click(object sender, System.EventArgs e)
    {
      if (textBoxPictureURL.Text == String.Empty) return;
      if (pictureBox1.Image != null)
      {
        pictureBox1.Image.Dispose();
        pictureBox1.Image = null;
      }
      string strThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MovieTitle, tbTitle.Text);
      string LargeThumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, tbTitle.Text);
      MediaPortal.Util.Utils.FileDelete(strThumb);
      MediaPortal.Util.Utils.FileDelete(LargeThumb);
      IMDBFetcher.DownloadCoverArt(Thumbs.MovieTitle, textBoxPictureURL.Text, tbTitle.Text);

      string file = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, tbTitle.Text);
      if (System.IO.File.Exists(file))
      {
        using (Image img = Image.FromFile(file))
        {
          Bitmap result = new Bitmap(img.Width, img.Height);
          using (Graphics g = Graphics.FromImage(result))
          {
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
          }
          pictureBox1.Image = result;
        }
      }
    }
    private void btnAmazon_Click(object sender, System.EventArgs e)
    {
      btnAmazon.Enabled = false;
      comboBoxPictures.Items.Clear();
      comboBoxPictures.Enabled = false;
      comboBoxPictures.Items.Add(new ComboBoxArt("Searching the Internet...", ""));
      comboBoxPictures.SelectedIndex = 0;
      IMPawardsSearch impSearch = new IMPawardsSearch();
      impSearch.Search(CurrentMovie.Title);
      AmazonImageSearch search = new AmazonImageSearch();
      search.Search(CurrentMovie.Title);
      comboBoxPictures.Items.Clear();
      if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
      {
        for (int i = 0; i < impSearch.Count; ++i)
        {
          ComboBoxArt art = new ComboBoxArt(String.Format("IMP Awards Picture {0}", (i + 1)), impSearch[i]);
          comboBoxPictures.Items.Add(art);
        }
        comboBoxPictures.Enabled = true;
      }
      if (search.Count > 0)
      {
        for (int i = 0; i < search.Count; ++i)
        {
          ComboBoxArt art = new ComboBoxArt(String.Format("Amazon Picture {0}", (i + 1)), search[i]);
          comboBoxPictures.Items.Add(art);
        }
        comboBoxPictures.Enabled = true;
      }
      if (comboBoxPictures.Items.Count == 0)
      {
        comboBoxPictures.Items.Clear();
        comboBoxPictures.Items.Add(new ComboBoxArt("No results found...", ""));
        comboBoxPictures.SelectedIndex = 0;
      }
      btnAmazon.Enabled = true;
    }

    private void btnDelete_Click(object sender, System.EventArgs e)
    {
      if (CurrentMovie.ID < 0)
      {
        return;
      }
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this movie?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (dialogResult == DialogResult.Yes)
      {
        VideoDatabase.DeleteMovieInfoById(CurrentMovie.ID);
        string file = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, CurrentMovie.Title);
        if (System.IO.File.Exists(file))
        {
          System.IO.File.Delete(file);
        }
        file = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MovieTitle, CurrentMovie.Title);
        if (System.IO.File.Exists(file))
        {
          System.IO.File.Delete(file);
        }
        LoadMovies(0);
      }
    }

    private void comboBoxPictures_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      ComboBoxArt art = comboBoxPictures.SelectedItem as ComboBoxArt;
      if ((art != null) && (comboBoxPictures.Enabled))
      {
        textBoxPictureURL.Text = art.URL;
      }
    }

    private void tabControl2_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }

    private void buttonImport_Click(object sender, System.EventArgs e)
    {
      int id = 0;
      System.Windows.Forms.OpenFileDialog find_file = new OpenFileDialog();
      find_file.RestoreDirectory = true;
      find_file.DefaultExt = "xml";
      find_file.Filter = "DVD Profile|*.xml";
      find_file.InitialDirectory = ".";
      find_file.Title = "Select DVD Profiler database" + tbTitle.Text;
      if (find_file.ShowDialog(this) != DialogResult.OK) return;
      XmlDocument doc = new XmlDocument();
      doc.Load(find_file.FileName);
      XmlNodeList dvdList = doc.DocumentElement.SelectNodes("/Collection/DVD");
      foreach (XmlNode nodeDVD in dvdList)
      {
        XmlNode nodeTitle = nodeDVD.SelectSingleNode("Title");
        XmlNode nodeRating = nodeDVD.SelectSingleNode("Rating");
        XmlNode nodeYear = nodeDVD.SelectSingleNode("ProductionYear");
        XmlNode nodeDuration = nodeDVD.SelectSingleNode("RunningTime");
        XmlNode nodeOverview = nodeDVD.SelectSingleNode("Overview");

        string genre = String.Empty;
        XmlNodeList genreList = nodeDVD.SelectNodes("Genres/Genre");
        foreach (XmlNode nodeGenre in genreList)
        {
          // added check to see if nodeGenre was valid
          if (nodeGenre != null && nodeGenre.InnerText != null)
          {
            if (genre.Length > 0) genre += " / ";
            genre += nodeGenre.InnerText;
          }
        }
        string cast = "Cast overview:";
        XmlNodeList actorsList = nodeDVD.SelectNodes("Actors/Actor");
        foreach (XmlNode nodeActor in actorsList)
        {
          string firstname = String.Empty;
          string lastname = String.Empty;
          string role = String.Empty;
          XmlNode nodeFirstName = nodeActor.SelectSingleNode("FirstName");
          XmlNode nodeLastName = nodeActor.SelectSingleNode("LastName");
          XmlNode nodeRole = nodeActor.SelectSingleNode("Role");
          if (nodeFirstName != null && nodeFirstName.InnerText != null) firstname = nodeFirstName.InnerText;
          if (nodeLastName != null && nodeLastName.InnerText != null) lastname = nodeLastName.InnerText;
          if (nodeRole != null && nodeRole.InnerText != null) role = nodeRole.InnerText;
          string line = String.Format("{0} {1} as {2}\n", firstname, lastname, role);
          cast += line;
        }


        string credits = String.Empty;
        XmlNodeList creditsList = nodeDVD.SelectNodes("Credits/Credit");
        foreach (XmlNode nodeCredit in creditsList)
        {
          // Added check for firstname, lastname valid
          string firstname = String.Empty;
          string lastname = String.Empty;
          XmlNode nodeFirstName = nodeCredit.SelectSingleNode("FirstName");
          XmlNode nodeLastName = nodeCredit.SelectSingleNode("LastName");
          if (nodeFirstName != null && nodeFirstName.InnerText != null) firstname = nodeFirstName.InnerText;
          if (nodeLastName != null && nodeLastName.InnerText != null) lastname = nodeLastName.InnerText;
          if (credits.Length > 0) credits += " / ";
          credits += String.Format("{0} {1}", firstname, lastname);
        }

        IMDBMovie movie = new IMDBMovie();
        movie.Cast = cast;
        movie.CDLabel = String.Empty;
        movie.Director = String.Empty;
        movie.DVDLabel = String.Empty;
        movie.File = String.Empty;
        movie.Genre = genre;
        movie.IMDBNumber = String.Empty;
        // Added check to validate rating
        if (nodeRating != null && nodeRating.InnerText != null) movie.MPARating = nodeRating.InnerText;
        else movie.MPARating = "NR";
        movie.Path = String.Empty;
        // Added check to validate overview and duration
        if (nodeOverview != null && nodeOverview.InnerText != null) movie.Plot = nodeOverview.InnerText;
        else movie.Plot = String.Empty;
        movie.PlotOutline = String.Empty;
        movie.Rating = 0;
        if (nodeDuration != null && nodeDuration.InnerText != null)
          movie.RunTime = Int32.Parse(nodeDuration.InnerText);
        else movie.RunTime = 0;
        movie.SearchString = String.Empty;
        movie.TagLine = String.Empty;
        movie.ThumbURL = String.Empty;
        movie.Title = nodeTitle.InnerText;
        movie.Top250 = 0;
        movie.Votes = String.Empty;
        movie.Watched = 0;
        movie.WritingCredits = credits;
        // Added check to validate year
        if (nodeYear != null && nodeYear != null) movie.Year = Int32.Parse(nodeYear.InnerText);
        else movie.Year = 0;
        id = VideoDatabase.AddMovie(movie.Title, true);
        movie.ID = id;
        VideoDatabase.SetMovieInfoById(id, ref movie);
        System.Windows.Forms.Application.DoEvents();
      }
      LoadMovies(id);
    }

    IMDBMovie CurrentMovie
    {
      get
      {
        IMDBMovie movie = new IMDBMovie();
        if (cbTitle.SelectedItem != null)
        {
          ComboBoxItemMovie cbMovie = (ComboBoxItemMovie)cbTitle.SelectedItem;
          movie.ID = cbMovie.Movie.ID;
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
            movie.Watched = 1;
          else
            movie.Watched = 0;
          movie.Title = tbTitle.Text;
          movie.Director = tbDirector.Text;
          movie.MPARating = tbMPAARating.Text;
          movie.RunTime = Int32.Parse(tbDuration.Text);
          movie.WritingCredits = tbWritingCredits.Text;
          movie.Plot = tbDescription.Text;
          movie.Rating = (float)Double.Parse(tbRating.Text);
          movie.TagLine = tbTagline.Text;
          movie.Year = Int32.Parse(tbYear.Text);
          movie.ThumbURL = textBoxPictureURL.Text;
          movie.Votes = tbVotes.Text;
          movie.PlotOutline = tbPlotOutline.Text;
        }
        foreach (ListViewItem item in listViewGenres.Items)
        {
          if (movie.Genre == String.Empty) movie.Genre = item.Text;
          else movie.Genre += " / " + item.Text;
        }

        foreach (ListViewItem item in listViewMovieActors.Items)
        {
          string actor = item.SubItems[0].Text;
          if (item.SubItems[1].Text != string.Empty)
          {
            actor += " as " + item.SubItems[1].Text;
          }
          if (movie.Cast == String.Empty) movie.Cast = actor;
          else movie.Cast += "\n" + actor;
        }
        return movie;
      }
    }


    private void OnFuzzyMatchingCheckedChanged(object sender, System.EventArgs e)
    {
      _isFuzzyMatching = ((CheckBox)sender).Checked;
    }

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(base._config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        _isFuzzyMatching = xmlreader.GetValueAsBool("movies", "fuzzyMatching", true);

        _fuzzyMatchingCheckBox.Checked = _isFuzzyMatching;
        // Load settings for Database
        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
        bool imdbFound = false;
        bool ofdbFound = false;
        bool frdbFound = false;
        bool filmAffinityFound = false;
        if (iNumber > 0)
        {
          string strLimit = "";
          string strDatabase = "";
          string strLanguage = "";
          for (int i = 0; i < iNumber; i++)
          {
            strLimit = xmlreader.GetValueAsString("moviedatabase", "limit" + i.ToString(), "false");
            strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i.ToString(), "false");
            strLanguage = xmlreader.GetValueAsString("moviedatabase", "language" + i.ToString(), "false");
            if ((strLimit != "false") && (strDatabase != "false") && (strLanguage != "false"))
            {
              // create entry for the database
              lvDatabase.Items.Add(strDatabase);
              lvDatabase.Items[i].SubItems.Add(strLanguage);
              lvDatabase.Items[i].SubItems.Add(strLimit);
              if (strDatabase == "IMDB")
                imdbFound = true;
              else if (strDatabase == "OFDB")
                ofdbFound = true;
              else if (strDatabase == "FRDB")
                frdbFound = true;
              else if (strDatabase == "FilmAffinity")
                filmAffinityFound = true;
            }
          }
        }
        if (!imdbFound)
          mpComboBox1.Items.Add(new ComboBoxItemDatabase("IMDB", "english", "25"));

        if (!ofdbFound)
          mpComboBox1.Items.Add(new ComboBoxItemDatabase("OFDB", "german", "25"));

        if (!frdbFound)
          mpComboBox1.Items.Add(new ComboBoxItemDatabase("FRDB", "french", "25"));

        if (!filmAffinityFound)
          mpComboBox1.Items.Add(new ComboBoxItemDatabase("FilmAffinity", "spanish", "20"));

        if (mpComboBox1.Items.Count > 0)
          mpComboBox1.SelectedIndex = 0;

        // set the first entry "activ"
        if (lvDatabase.Items.Count > 0)
          lvDatabase.Items[0].Selected = true;

      }

    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(base._config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("movies", "fuzzyMatching", _isFuzzyMatching);
        // Database
        xmlwriter.SetValue("moviedatabase", "number", this.lvDatabase.Items.Count);
        for (int i = 0; i < this.lvDatabase.Items.Count; i++)
        {
          xmlwriter.SetValue("moviedatabase", "database" + i.ToString(), this.lvDatabase.Items[i].SubItems[0].Text);
          xmlwriter.SetValue("moviedatabase", "limit" + i.ToString(), this.lvDatabase.Items[i].SubItems[2].Text);
          xmlwriter.SetValue("moviedatabase", "language" + i.ToString(), this.lvDatabase.Items[i].SubItems[1].Text);
        }
        for (int i = this.lvDatabase.Items.Count; i < 4; i++)
        {
          xmlwriter.RemoveEntry("moviedatabase", "database" + i.ToString());
          xmlwriter.RemoveEntry("moviedatabase", "limit" + i.ToString());
          xmlwriter.RemoveEntry("moviedatabase", "language" + i.ToString());
        }
      }
    }

    private void bDatabaseDown_Click(object sender, System.EventArgs e)
    {
      // Moves the selected entry down
      // get the entry
      ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
      // guilty entry?
      if (indexes.Count == 1)
      {
        int index = indexes[0];
        // not the last entry?
        if (index < lvDatabase.Items.Count - 1)
        {
          // save current text
          string strSub0 = lvDatabase.Items[index + 1].SubItems[0].Text;
          string strSub1 = lvDatabase.Items[index + 1].SubItems[1].Text;
          string strSub2 = lvDatabase.Items[index + 1].SubItems[2].Text;
          // copy text
          lvDatabase.Items[index + 1].SubItems[0].Text = lvDatabase.Items[index].SubItems[0].Text;
          lvDatabase.Items[index + 1].SubItems[1].Text = lvDatabase.Items[index].SubItems[1].Text;
          lvDatabase.Items[index + 1].SubItems[2].Text = lvDatabase.Items[index].SubItems[2].Text;
          // restore backuped text
          lvDatabase.Items[index].SubItems[0].Text = strSub0;
          lvDatabase.Items[index].SubItems[1].Text = strSub1;
          lvDatabase.Items[index].SubItems[2].Text = strSub2;
          // move the selection down
          lvDatabase.Items[index].Selected = false;
          lvDatabase.Items[index + 1].Selected = true;
        }
      }
    }

    private void bDatabaseUp_Click(object sender, System.EventArgs e)
    {
      // Moves the selected entry up
      // get the entry
      ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
      // guilty entry?
      if (indexes.Count == 1)
      {
        int index = indexes[0];
        // not the first entry?
        if (index > 0)
        {
          // save current text
          string strSub0 = lvDatabase.Items[index - 1].SubItems[0].Text;
          string strSub1 = lvDatabase.Items[index - 1].SubItems[1].Text;
          string strSub2 = lvDatabase.Items[index - 1].SubItems[2].Text;
          // copy text
          lvDatabase.Items[index - 1].SubItems[0].Text = lvDatabase.Items[index].SubItems[0].Text;
          lvDatabase.Items[index - 1].SubItems[1].Text = lvDatabase.Items[index].SubItems[1].Text;
          lvDatabase.Items[index - 1].SubItems[2].Text = lvDatabase.Items[index].SubItems[2].Text;
          // restore backuped text
          lvDatabase.Items[index].SubItems[0].Text = strSub0;
          lvDatabase.Items[index].SubItems[1].Text = strSub1;
          lvDatabase.Items[index].SubItems[2].Text = strSub2;
          // move the selection up
          lvDatabase.Items[index].Selected = false;
          lvDatabase.Items[index - 1].Selected = true;
        }
      }
    }
    private void lvDatabase_KeyUp(Object o, KeyEventArgs e)
    {
      if (e.KeyCode == System.Windows.Forms.Keys.Delete || e.KeyCode == System.Windows.Forms.Keys.Back)
      {
        if (lvDatabase.SelectedIndices.Count > 0)
        {
          string strSub0 = lvDatabase.SelectedItems[0].SubItems[0].Text;
          string strSub1 = lvDatabase.SelectedItems[0].SubItems[1].Text;
          string strSub2 = lvDatabase.SelectedItems[0].SubItems[2].Text;
          mpComboBox1.Items.Add(new ComboBoxItemDatabase(strSub0, strSub1, strSub2));
          int index = lvDatabase.SelectedItems[0].Index;
          lvDatabase.Items.Remove(lvDatabase.SelectedItems[0]);
          lvDatabase.Update();
          if (lvDatabase.Items.Count > 0)
          {
            if (index >= lvDatabase.Items.Count)
            {
              index = lvDatabase.Items.Count - 1;
            }
            lvDatabase.SelectedIndices.Clear();
            lvDatabase.SelectedIndices.Add(index);
          }
          SaveSettings();
        }
      }
    }
    private void mpButton1_Click(object sender, EventArgs e)
    {
      ComboBoxItemDatabase database = mpComboBox1.SelectedItem as ComboBoxItemDatabase;
      if (database != null)
      {
        ListViewItem item = this.lvDatabase.Items.Add(database.database);
        item.SubItems.Add(database.language);
        item.SubItems.Add(database.limit);
        mpComboBox1.Items.Remove(database);
        if (mpComboBox1.Items.Count > 0)
        {
          mpComboBox1.SelectedIndex = 0;
        }
        SaveSettings();
      }
    }

    private void lvDatabase_DoubleClick(object sender, EventArgs e)
    {
      Point pt = lvDatabase.PointToClient(Cursor.Position);
      ListViewItem item = lvDatabase.GetItemAt(pt.X, pt.Y);
      if (item != null)
      {
        Rectangle lviBounds;
        int subItemX;

        Rectangle subItemRect = Rectangle.Empty;
        lviBounds = item.GetBounds(ItemBoundsPortion.Entire);
        subItemX = lviBounds.Left + lvDatabase.Columns[0].Width + lvDatabase.Columns[1].Width;
        subItemRect = new Rectangle(subItemX, lviBounds.Top, lvDatabase.Columns[2].Width, lviBounds.Height);
        if (subItemRect.X < 0)
        {
          // Left edge of SubItem not visible - adjust rectangle position and width
          subItemRect.Width += subItemRect.X;
          subItemRect.X = 0;
        }
        if (subItemRect.X + subItemRect.Width > lvDatabase.Width)
        {
          // Right edge of SubItem not visible - adjust rectangle width
          subItemRect.Width = lvDatabase.Width - subItemRect.Left;
        }

        // Subitem bounds are relative to the location of the ListView!
        subItemRect.Offset(lvDatabase.Left, lvDatabase.Top);

        // In case the editing control and the listview are on different parents,
        // account for different origins
        Point origin = new Point(0, 0);
        Point lvOrigin = lvDatabase.Parent.PointToScreen(origin);
        Point ctlOrigin = listViewTextBox.Parent.PointToScreen(origin);

        subItemRect.Offset(lvOrigin.X - ctlOrigin.X, lvOrigin.Y - ctlOrigin.Y);

        // Position and show editor
        listViewTextBox.Bounds = subItemRect;
        listViewTextBox.Text = item.SubItems[2].Text;
        listViewTextBox.Visible = true;
        listViewTextBox.BringToFront();
        listViewTextBox.Focus();

        _editItem = item;
      }
    }
    private void listViewTextBox_Leave(object sender, EventArgs e)
    {
      // cell editor losing focus
      EndEditing(true);
    }

    private void listViewTextBox_KeyPress(object sender, System.Windows.Forms.PreviewKeyDownEventArgs e)
    {
      switch (e.KeyCode)
      {
        case System.Windows.Forms.Keys.Escape:
          {
            e.IsInputKey = true;
            EndEditing(false);
            break;
          }

        case System.Windows.Forms.Keys.Enter:
          {
            e.IsInputKey = true;
            EndEditing(true);
            break;
          }
      }
    }

    /// <summary>
    /// Accept or discard current value of cell editor control
    /// </summary>
    /// <param name="AcceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
    public void EndEditing(bool AcceptChanges)
    {
      if (AcceptChanges && (_editItem != null))
      {
        _editItem.SubItems[2].Text = listViewTextBox.Text;
      }
      _editItem = null;
      listViewTextBox.Visible = false;
    }


  }
}

