#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CSScriptLibrary;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using System.Text.RegularExpressions;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class MovieDatabase : SectionSettings, IMDB.IProgress
  {
    #region classes

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

    private class DatabaseComparer : IComparer<ComboBoxItemDatabase>
    {
      #region IComparer<ComboBoxItemDatabase> Member

      public int Compare(ComboBoxItemDatabase x, ComboBoxItemDatabase y)
      {
        if (x.Language.Equals(y.Language))
        {
          return x.Name.CompareTo(y.Name);
        }
        return x.Language.CompareTo(y.Language);
      }

      #endregion
    }

    internal class ComboBoxItemDatabase
    {
      public string Database;
      public string Name;
      public string Language;
      public string Limit;

      //public ComboBoxItemDatabase() {}

      public override string ToString()
      {
        return String.Format("{0}: {1} [{2}]", Language, Name, Database);
      }
    }

    internal class ComboBoxItemMovie
    {
      public string Title;
      public IMDBMovie Movie;

      public ComboBoxItemMovie(string title, IMDBMovie movie)
      {
        Title = title;
        Movie = movie;
      }

      public override string ToString()
      {
        return Title;
      }
    }

    internal class ComboBoxArt
    {
      public string Title;
      public string Url;

      public ComboBoxArt(string title, string url)
      {
        Title = title;
        Url = url;
      }

      public override string ToString()
      {
        return Title;
      }
    }

    #endregion

    // grabber index holds information/urls of available grabbers to download
    private string _grabberIndexFile = Config.GetFile(Config.Dir.Config, "MovieInfoGrabber.xml");
    private const string GrabberIndexUrl = @"http://install.team-mediaportal.com/MP1/MovieInfoGrabber.xml";

    /// <summary>
    /// Dictionary contains all grabber scripts.
    /// The Key is used for the filename, where the grabber is from.
    /// Will be refreshed on start and after online update.
    /// </summary>
    private Dictionary<string, IIMDBScriptGrabber> _grabberList;

    #region Variables

    // The LVI being edited
    private ListViewItem _editItem;
    private bool _scanning;
    private bool _useLocalImage;
    private bool _useLocalImageFanart;
    private DlgProgress _progressDialog = new DlgProgress();
    private string _newMovieToFind = string.Empty;
    private bool _isFuzzyMatching = true;

    // Fanart & new refresh movie & actors 
    private bool _useFanArt;
    private bool _refreshByImdBid;
    private int _idMovie;
    // Actors table
    private System.Data.DataTable _actTable = new System.Data.DataTable();
    // Last used file extension filter (Fileopen dialog when adding video file manually)
    private int _lastExt;
    // Folder name as movie title
    private bool _useFolderAsTitle;
    // Cover upgrade
    private bool _coversUpgraded;
    private bool _settingsLoaded;
    // Clear listboxes (fanart and cover), used on movierefresh for not delete items in listbox
    private bool _clearListBox = true;
    // Fanart file image index (from 0 to 4), number of downloaded fanarts per movie (max 5)
    private int _fanartImgIndex;
    // Refresh images backgroundworker state
    private bool _isRefreshing;

    private ArrayList _conflictFiles = new ArrayList();

    private List<BaseShares.ShareData> _sharesData = null;

    #endregion

    #region ctor

    public MovieDatabase()
      : this("Video Database") {}

    public MovieDatabase(string name)
      : base("Video Database")
    {
      InitializeComponent();

      linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://forum.team-mediaportal.com/movie-info-grabbers-287/");
      linkLabel2.Links.Add(0, linkLabel1.Text.Length, "http://forum.team-mediaportal.com/movie-info-grabbers-287/");
    }

    #endregion

    private string[] Extensions
    {
      get { return _extensions; }
      set { _extensions = value; }
    }

    private string[] _extensions = new[] {".avi"};

    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components;

    public override void OnSectionActivated()
    {
      if (!_settingsLoaded)
        Load();
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load selected shares
      //
      SectionSettings section = GetSection("Video Folders");

      if (section != null)
      {
        _sharesData = (List<BaseShares.ShareData>)section.GetSetting("sharesdata");
        foreach (BaseShares.ShareData share in _sharesData)
        {
          //
          // Add to share to list box and default to selected
          //
          sharesListBox.Items.Add(share.Folder, share.ScanShare);
        }
      }

      // Movie Folders
      // Fetch extensions
      //
      section = GetSection("Video Extensions");

      if (section != null)
      {
        string extensions = (string)section.GetSetting("extensions");
        Extensions = extensions.Split(new[] {','});
      }

      UpdateControlStatus();
      LoadMovies(0);
      if (cbTitle.Items.Count > 0)
      {
        cbTitle.SelectedIndex = 0;
      }
    }

    #region Scan tab

    private void sharesListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      BaseShares.ShareData share = _sharesData[e.Index];
      share.ScanShare = e.NewValue == CheckState.Checked ? true : false;
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
    private void sharesListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void startButton_Click(object sender, EventArgs e)
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
        string path = sharesListBox.Items[sharesListBox.CheckedIndices[index]].ToString();
        availablePaths.Add(path);
      }
      // Clean covers and fanarts (only if refresh cb is checked)
      if (refreshdbCheckBox.Checked)
      {
        for (int i = 0; i < cbTitle.Items.Count - 1; i++)
        {
          cbTitle.SelectedIndex = i;
          ComboBoxItemMovie item = (ComboBoxItemMovie)cbTitle.SelectedItem;
          CurrentMovie.ID = item.Movie.ID;
          string strFilenameAndPath = string.Empty;

          if (listViewFiles.Items.Count > 0)
          {
            strFilenameAndPath = listViewFiles.Items[0].Text;
          }
          // Delete covers
          FanArt.DeleteCovers(CurrentMovie.Title, CurrentMovie.ID);
          // Delete fanarts
          FanArt.DeleteFanarts(strFilenameAndPath, CurrentMovie.Title);
        }
      }
      _conflictFiles = new ArrayList();
      IMDBFetcher.ScanIMDB(this, availablePaths, _isFuzzyMatching, skipCheckBox.Checked, actorsCheckBox.Checked,
                           refreshdbCheckBox.Checked);
      LoadMovies(0);
    }

    // Changed - Fanart delete files
    private void clearButton_Click(object sender, EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the entire video database?",
                                                  "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (dialogResult == DialogResult.Yes)
      {
        string database = Config.GetFile(Config.Dir.Database, "VideoDatabaseV5.db3");
        if (File.Exists(database))
        {
          VideoDatabase.Dispose();
          try
          {
            File.Delete(database);
            // FanArt delete all files
            string configDir;
            FanArt.GetFanArtFolder(out configDir);
            if (useFanartCheckBox.CheckState == CheckState.Checked)
            {
              DialogResult dialogResultFanart = MessageBox.Show("Delete all fanarts (All files in " +
                                                                configDir +
                                                                " will be deleted) ?",
                                                                "Information", MessageBoxButtons.YesNo,
                                                                MessageBoxIcon.Question);
              if (dialogResultFanart == DialogResult.Yes)
              {
                string files = @"*.jpg"; // Only delete jpg files
                string[] fileList = Directory.GetFiles(configDir, files);
                foreach (string file in fileList)
                {
                  File.Delete(file);
                }
              }
            }
          }
          catch (Exception)
          {
            MessageBox.Show("Video database could not be cleared", "Video Database", MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
            return;
          }
          finally
          {
            VideoDatabase.ReOpen();
          }
        }
        // Actor detail clear
        cbActor.DataSource = null;
        cbActor.Items.Clear();
        _actTable.Clear();
        _actTable.Dispose();
        tbBirthDate.Text = string.Empty;
        tbBirthPlace.Text = string.Empty;
        tbBiography.Text = string.Empty;
        tbThumbLoc.Text = string.Empty;
        if (pictureBoxActor.Image != null)
        {
          pictureBoxActor.Image.Dispose();
          pictureBoxActor.Image = null;
        }
        // Clear Actors listboxes
        listViewAllActors.Items.Clear();
        listViewMovieActors.Items.Clear();
        // Clear Genres listboxes
        listViewAllGenres.Items.Clear();
        listViewGenres.Items.Clear();
        MessageBox.Show("Video database has been cleared", "Video Database", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
      }
    }

    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      if (_progressDialog.IsInstance(fetcher))
      {
        _progressDialog.DisableCancel();
      }
      return true;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      _progressDialog.SetLine1(line1);
      _progressDialog.SetLine2(line2);
      if (percent > 0)
      {
        _progressDialog.SetPercentage(percent);
      }
      _progressDialog.Update();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      _progressDialog.ResetProgress();
      _progressDialog.SetHeading("Searching IMDB...");
      _progressDialog.SetLine1(fetcher.MovieName);
      _progressDialog.SetLine2(string.Empty);
      _progressDialog.Instance = fetcher;
      return true;
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      DialogResult result = _progressDialog.ShowDialog(this);
      Update();
      if (result == DialogResult.Cancel)
      {
        return false;
      }
      return true;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      if (_progressDialog.IsInstance(fetcher))
      {
        _progressDialog.CloseProgress();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
      }
      else
      {
        MessageBox.Show("No IMDB info found!", fetcher.MovieName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      return false;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      _progressDialog.ResetProgress();
      _progressDialog.SetHeading("Downloading Movie details...");
      _progressDialog.SetLine1("Downloading Movie details...");
      _progressDialog.SetLine2(fetcher.MovieName);
      _progressDialog.Instance = fetcher;
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      _progressDialog.Instance = fetcher;
      DialogResult result = _progressDialog.ShowDialog(this);
      Update();
      if (result == DialogResult.Cancel)
      {
        return false;
      }
      return true;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      if (_progressDialog.IsInstance(fetcher))
      {
        _progressDialog.CloseProgress();
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      _progressDialog.ResetProgress();
      _progressDialog.SetHeading("Downloading Actor info...");
      _progressDialog.SetLine1("Downloading Actor info...");
      _progressDialog.SetLine2(fetcher.MovieName);
      _progressDialog.Instance = fetcher;
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      _progressDialog.Instance = fetcher;
      DialogResult result = _progressDialog.ShowDialog(this);
      Update();
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
        _conflictFiles.Add(fetcher.Movie);
      }
      else
      {
        MessageBox.Show("Movie details could not be found.", fetcher.MovieName, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
      }
      return true;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      movieName = _newMovieToFind;
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
        _conflictFiles.Add(fetcher.Movie);
        selectedMovie = -1;
        return false;
      }
      DlgMovieList dlg = new DlgMovieList();
      dlg.Filename = fetcher.MovieName;
      for (int i = 0; i < fetcher.Count; ++i)
      {
        dlg.AddMovie(fetcher[i].Title);
      }
      DialogResult result = dlg.ShowDialog(this);
      Update();
      if (result == DialogResult.Cancel)
      {
        selectedMovie = -1;
        return false;
      }
      selectedMovie = dlg.SelectedItem;
      if (dlg.IsNewFind)
      {
        _newMovieToFind = dlg.NewTitleToFind;
        selectedMovie = -1;
      }
      return true;
    }

    public bool OnScanStart(int total)
    {
      _scanning = true;
      _progressDialog.Total = total;

      return true;
    }

    public bool OnScanEnd()
    {
      if (_conflictFiles.Count > 0)
      {
        DlgMovieConflicts dlg = new DlgMovieConflicts();
        for (int i = 0; i < _conflictFiles.Count; ++i)
        {
          if (_conflictFiles[i] == null)
		  {
            continue;
		  }
          IMDBMovie currentMovie = (IMDBMovie)_conflictFiles[i];
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
      _progressDialog.Count = count;
      return true;
    }

    public bool OnScanIterated(int count)
    {
      _progressDialog.Count = count;
      if (_progressDialog.CancelScan)
      {
        _progressDialog.ResetProgress();
        return false;
      }
      return true;
    }

    #endregion

    #endregion

    // Load movies from the database and refresh current selected movie infos (full refresh)
    private void LoadMovies(int id)
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
      // This cause movie info refresh (triggers cbTitle_SelectedIndexChanged)
      cbTitle.SelectedIndex = index;
    }

    // Changed - cover find, title suffix for problem with covers and movie with the same name
    private void UpdateEdit(IMDBMovie movie)
    {
      listViewMovieActors.BeginUpdate();
      listViewGenres.BeginUpdate();
      listViewAllGenres.BeginUpdate();
      listViewAllActors.BeginUpdate();
      listViewFiles.BeginUpdate();

      tbDiscNr.Text = (movie.DVDLabel.Length > 4
                         ? Convert.ToString(Convert.ToInt16(movie.DVDLabel.Substring(4)))
                         : string.Empty);
      tbTitle.Text = movie.Title;
      tbTagline.Text = movie.TagLine;
      tbYear.Text = movie.Year.ToString();
      tbVotes.Text = movie.Votes;
      tbRating.Text = movie.Rating.ToString();
      tbDirector.Text = movie.Director;
      tbWritingCredits.Text = movie.WritingCredits;
      _idMovie = movie.ID;
      tbMovieID.Text = _idMovie.ToString();
      tbSummary.Text = movie.Plot;
      tbReview.Text = movie.UserReview; // New dbcolumn for movie details
      tbIMDBNr.Text = movie.IMDBNumber; // Needed for cover search
      //
      // Images (cover and fanart)
      //
      if (movie.ThumbURL.Length > 7 && movie.ThumbURL.Substring(0, 7).Equals("file://"))
      {
        _useLocalImage = true;
        tbImageLocation.Text = movie.ThumbURL.Substring(7);
      }
      else
      {
        _useLocalImage = false;
        tbImageLocation.Text = movie.ThumbURL;
      }
      // Fanart
      if (movie.FanartURL.Length > 7 && movie.FanartURL.Substring(0, 7).Equals("file://"))
      {
        _useLocalImageFanart = true;
        tbFanartLocation.Text = movie.FanartURL.Substring(7);
      }
      else
      {
        _useLocalImageFanart = false;
        tbFanartLocation.Text = movie.FanartURL;
      }

      tbPlotOutline.Text = movie.PlotOutline;
      tbMPAARating.Text = movie.MPARating;
      tbDuration.Text = movie.RunTime.ToString();

      if (movie.Watched > 0)
      {
        cbWatched.Checked = true;
      }
      else
      {
        cbWatched.Checked = false;
      }

      if (pictureBoxCover.Image != null)
      {
        pictureBoxCover.Image.Dispose();
        pictureBoxCover.Image = null;
      }

      foreach (ListViewItem item in listViewMovieActors.Items)
      {
        listViewAllActors.Items.Add(item.Text);
      }

      foreach (ListViewItem item in listViewGenres.Items)
      {
        listViewAllGenres.Items.Add(item.Text);
      }

      listViewMovieActors.Items.Clear();
      listViewGenres.Items.Clear();
      listViewFiles.Items.Clear();
      if (_clearListBox)
      {
        coversListBox.Items.Clear();
        coversListBox.Enabled = false;
      }

      if (movie.ID >= 0)
      {
        // Title suffix for problem with covers and movie with the same name
        string titleExt = movie.Title + "{" + movie.ID + "}";
        string file = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);

        if (File.Exists(file) && !_isRefreshing)
        {
          using (Image img = Image.FromFile(file))
          {
            Bitmap result = new Bitmap(img.Width, img.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
              g.CompositingQuality = Thumbs.Compositing;
              g.InterpolationMode = Thumbs.Interpolation;
              g.SmoothingMode = Thumbs.Smoothing;
              g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            }
            pictureBoxCover.Image = result;
          }
        }

        char[] splitter = {'\n', ','};
        string[] actors = movie.Cast.Split(splitter);

        if (actors.Length > 0)
        {
          for (int i = 0; i < actors.Length; ++i)
          {
            string actor;
            string role = string.Empty;
            int pos = actors[i].IndexOf(" as ");

            if (pos >= 0)
            {
              actor = actors[i].Substring(0, pos);
              role = actors[i].Substring(pos + 4);
            }
            else
            {
              actor = actors[i];
            }

            actor = actor.Trim();
            ListViewItem item = new ListViewItem(actor);
            item.SubItems.Add(role);
            listViewMovieActors.Items.Add(item);

            for (int actorIndex = listViewAllActors.Items.Count - 1; actorIndex >= 0; --actorIndex)
            {
              if (listViewAllActors.Items[actorIndex].Text == actor)
              {
                listViewAllActors.Items.RemoveAt(actorIndex);
                break;
              }
            }
          }
        }

        listViewMovieActors.Sort();

        string szGenres = movie.Genre;
        if (szGenres.IndexOf("/") >= 0)
        {
          Tokens f = new Tokens(szGenres, new[] {'/'});
          foreach (string strGenre in f)
          {
            String strCurrentGenre = strGenre.Trim();
            listViewGenres.Items.Add(strCurrentGenre);

            for (int i = listViewAllGenres.Items.Count - 1; i >= 0; --i)
            {
              if (listViewAllGenres.Items[i].Text == strCurrentGenre)
              {
                listViewAllGenres.Items.RemoveAt(i);
                break;
              }
            }
          }
        }
        else
        {
          String strCurrentGenre = movie.Genre.Trim();

          listViewGenres.Items.Add(strCurrentGenre);

          for (int i = listViewAllGenres.Items.Count - 1; i >= 0; --i)
          {
            if (listViewAllGenres.Items[i].Text == strCurrentGenre)
            {
              listViewAllGenres.Items.RemoveAt(i);
              break;
            }
          }
        }

        listViewGenres.Sort();
        ArrayList filenames = new ArrayList();
        VideoDatabase.GetFiles(movie.ID, ref filenames);
        foreach (string filename in filenames)
        {
          listViewFiles.Items.Add(filename);
        }
      }

      if (listViewAllGenres.Items.Count == 0)
      {
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
          {
            listViewAllGenres.Items.Add(genre);
          }
        }

        listViewAllGenres.Sort();
      }

      if (listViewAllActors.Items.Count == 0)
      {
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
          {
            listViewAllActors.Items.Add(actor);
          }
        }

        listViewAllActors.Sort();
      }
      listViewMovieActors.EndUpdate();
      listViewGenres.EndUpdate();
      listViewAllGenres.EndUpdate();
      listViewAllActors.EndUpdate();
      listViewFiles.EndUpdate();
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedTab == tabPageEditor)
      {
        LoadMovies(0);
      }
    }

    #region Editor tab

    private void buttonMapGenre_Click(object sender, EventArgs e)
    {
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

    private void buttonUnmapGenre_Click(object sender, EventArgs e)
    {
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

    private void buttonMapActors_Click(object sender, EventArgs e)
    {
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

    private void buttonUnmapActors_Click(object sender, EventArgs e)
    {
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

    // Changed - media types added, tbtitle = file
    private void buttonAddFile_Click(object sender, EventArgs e)
    {
      AddFile();
    }

    private void AddFile()
    {
      OpenFileDialog findFile = new OpenFileDialog();
      // find_file.RestoreDirectory = true;

      // Let's add MP users video extensions
      string ext; // Initial MP extensions from MP config.xml
      string extMP; // MP config extensions prepared for file dialog
      string extList; // Extension description in file dialog for MP extensions
      using (Settings xmlreader = new MPSettings())
      {
        ext = xmlreader.GetValueAsString("movies", "extensions", Util.Utils.VideoExtensionsDefault);
        extList = ext.Replace(".", "*.").Replace(",", ", ");
        extMP = ext.Replace(".", "*.").Replace(",", ";");
      }
      // Added some new common media types for file filter
      string commonExt = "|3GPP2 Multimedia File|*.3g2|3GPP Multimedia File|*.3gp|" +
                         "Advanced Streaming Format|*.asf|Avi Files|*.avi|" +
                         "DivX movie|*.div|DivX Media Format|*.divx|Recordings|*.dvr-ms|" +
                         "Flash Video File|*.flv|" +
                         "MPEG-2 stream (Blu-Ray)|*.m2ts|Matroska video file|*.mkv|Apple QuickTime movie file|*.mov|" +
                         "MPEG-4 Video File|*.mp4|Mpeg files|*.mpeg|Mpeg files|*.mpg|AVCHD MPEG-2 transport stream file|*.mts|" +
                         "Apple QuickTime movie clip|*.qt|" +
                         "RealMedia|*.rm|Real-time streaming protocol file|*.rtsp|" +
                         "MPEG-TV recorded file|*.ts|" +
                         "DVD Video Object File|*.vob|" +
                         "Windows Media|*.wmv|";

      findFile.Filter =
        "MP Media Files(" + extList + ")|" + extMP + commonExt + "All files|*.*";
      // Set remembered previously used file filter, if it's first usage then index is 0 (All MP media)
      findFile.FilterIndex = _lastExt;
      findFile.InitialDirectory = ".";
      findFile.Title = "Find files for " + tbTitle.Text;
      findFile.Multiselect = true;

      if (findFile.ShowDialog(this) == DialogResult.OK)
      {
        foreach (String file in findFile.FileNames)
        {
          listViewFiles.Items.Add(file);
        }
        string filename;
        if (listViewFiles.Items.Count > 0)
        {
          // We will take first file, other files is not relevant as it should be rest of the same set
          // TODO - Maybe to put some kind of checking so user doesn't have opportunity to select different named files
          string path;
          Util.Utils.Split(listViewFiles.Items[0].Text, out path, out filename);
          // Remember last used file filter
          _lastExt = findFile.FilterIndex;
          // Put in the title -> filename - Users wish
          filename = filename.Remove(filename.LastIndexOf("."));
          tbTitle.Text = filename;
        }
      }
    }

    private void buttonRemoveFile_Click(object sender, EventArgs e)
    {
      for (int i = listViewFiles.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewFiles.SelectedItems[i];
        listViewFiles.Items.Remove(listItem);
      }
    }

    private void buttonDeleteActor_Click(object sender, EventArgs e)
    {
      if (
        MessageBox.Show("Are you sure you want to delete the selected actors?", "Are you sure?", MessageBoxButtons.YesNo) ==
        DialogResult.Yes)
      {
        for (int i = listViewAllActors.SelectedItems.Count - 1; i >= 0; --i)
        {
          ListViewItem listItem = listViewAllActors.SelectedItems[i];
          VideoDatabase.DeleteActor(listItem.Text);
          listViewAllActors.Items.Remove(listItem);
          string file = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, listItem.Text);
          if (File.Exists(file))
          {
            File.Delete(file);
          }
          file = Util.Utils.GetCoverArtName(Thumbs.MovieActors, listItem.Text);
          if (File.Exists(file))
          {
            File.Delete(file);
          }
        }
        // Actor details
        ActorsTableRefresh(Int32.Parse(tbMovieID.Text));
        PopulateActorInfo();
      }
    }

    private void buttonNewActor_Click(object sender, EventArgs e)
    {
      if (textBoxNewActor.Text.Length == 0)
      {
        return;
      }
      VideoDatabase.AddActor(textBoxNewActor.Text);
      listViewAllActors.Items.Add(textBoxNewActor.Text);
      // Sort listview
      listViewAllActors.Sort();
      // Actor details
      ActorsTableRefresh(Int32.Parse(tbMovieID.Text));
      PopulateActorInfo();
    }

    private void btnDeleteGenre_Click(object sender, EventArgs e)
    {
      if (
        MessageBox.Show("Are you sure you want to delete the selected genres?", "Are you sure?", MessageBoxButtons.YesNo) ==
        DialogResult.Yes)
      {
        for (int i = listViewAllGenres.SelectedItems.Count - 1; i >= 0; --i)
        {
          ListViewItem listItem = listViewAllGenres.SelectedItems[i];
          VideoDatabase.DeleteGenre(listItem.Text);
          listViewAllGenres.Items.Remove(listItem);
        }
      }
    }

    private void buttonNewGenre_Click(object sender, EventArgs e)
    {
      if (textBoxNewGenre.Text.Length == 0)
      {
        return;
      }
      VideoDatabase.AddGenre(textBoxNewGenre.Text);
      listViewAllGenres.Items.Add(textBoxNewGenre.Text);
    }

    /*
        private void buttonNewMovie_Click(object sender, EventArgs e)
        {
          cbTitle.SelectedItem = null;
          IMDBMovie details = new IMDBMovie();
          UpdateEdit(details);
        }
    */

    // Changed - FanArt - refresh movie by tt number
    private void buttonLookupMovie_Click(object sender, EventArgs e)
    {
      if (tbTitle.Text == string.Empty & _refreshByImdBid == false)
      {
        MessageBox.Show("Please enter a movie title.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      // Check IMDBid
      if (_refreshByImdBid && CheckImdbId(tbIMDBNr.Text) == false)
      {
        MessageBox.Show("Incorrect IMDB ID. ID must be like (tt1234567).", "Information", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        return;
      }
      string strFilenameAndPath = string.Empty;
      if (listViewFiles.Items.Count > 0)
      {
        strFilenameAndPath = listViewFiles.Items[0].Text;
      }
      // Cover delete
      string title = cbTitle.Items[cbTitle.SelectedIndex].ToString();
      FanArt.DeleteCovers(title, CurrentMovie.ID);
      // Fanart delete
      FanArt.DeleteFanarts(strFilenameAndPath, title);

      buttonLookupMovie.Enabled = false;
      btnSave.Enabled = false;
      tabControl2.Enabled = false; // Subtab options for main
      tabControl1.Enabled = false; // Main tab (settings, scan, editor)
      _progressDialog.Total = 1;
      _progressDialog.Count = 1;
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
      Util.Utils.Split(file, out path, out filename);
      movieDetails.Path = path;
      movieDetails.File = filename;
      // Search by IMDB ID number 
      if (_refreshByImdBid == false)
      {
        // Clean old actors info
        if (CurrentMovie.ID > 0)
          VideoDatabase.RemoveActorsForMovie(CurrentMovie.ID);
        movieDetails.IMDBNumber = string.Empty;
        movieDetails.SearchString = tbTitle.Text;
        GetInfoFromIMDB(ref movieDetails, false);
      }
      else
      {
        movieDetails.SearchString = tbIMDBNr.Text;
        GetInfoFromIMDB(ref movieDetails, true);
      }

      // Fanart
      string fileArtMovie = string.Empty;
      FanArt.GetFanArtfilename(movieDetails.Title, 0, out fileArtMovie);
      pictureBoxFanArt.ImageLocation = fileArtMovie;
      // End fanart

      buttonLookupMovie.Enabled = true;
      btnSave.Enabled = true;
      tabControl2.Enabled = true; // Subtab options for main
      tabControl1.Enabled = true; // Main tab (settings, scan, editor)
    }

    private void GetInfoFromIMDB(ref IMDBMovie movieDetails, bool fuzzyMatch)
    {
      string file;
      string path = movieDetails.Path;
      string filename = movieDetails.File;
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
        file = path + Path.DirectorySeparatorChar + filename;
      }
      else
      {
        file = filename;
      }

      int id = movieDetails.ID;
      if (id < 0)
      {
        Log.Info("Adding file:{0}", file);
        id = VideoDatabase.AddMovieFile(file);
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.VideoExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            continue;
          }
          if (Util.Utils.ShouldStack(item.Path, file) && item.Path != file)
          {
            string strPath, strFileName;

            DatabaseUtility.Split(item.Path, out strPath, out strFileName);
            DatabaseUtility.RemoveInvalidChars(ref strPath);
            DatabaseUtility.RemoveInvalidChars(ref strFileName);
            int pathId = VideoDatabase.AddPath(strPath);
            VideoDatabase.AddFile(id, pathId, strFileName);
          }
        }
        movieDetails.ID = id;
        string searchString = movieDetails.SearchString;
        VideoDatabase.SetMovieInfoById(movieDetails.ID, ref movieDetails);
        movieDetails.SearchString = searchString;
      }
      if (IMDBFetcher.RefreshIMDB(this, ref movieDetails, fuzzyMatch, actorsCheckBox.Checked, true))
      {
        if (movieDetails != null)
        {
          LoadMovies(movieDetails.ID);
        }
      }
    }

    // Changed - added exception check, forbidden chars for filenames
    private void btnSave_Click(object sender, EventArgs e)
    {
      //
      // Exception check
      //
      int resultInt;
      float resultFloat;
      // Year tbcheck
      int.TryParse(tbYear.Text, out resultInt);
      if (resultInt == 0)
        tbYear.Text = "1900";
      // Duration tbcheck
      int.TryParse(tbDuration.Text, out resultInt);
      if (resultInt == 0)
        tbDuration.Text = "0";
      // Rating tbcheck
      float.TryParse(tbRating.Text, out resultFloat);
      if (resultFloat == 0)
        tbRating.Text = "0";
      // IMDB id
      if ((tbIMDBNr.Text.Length < 9 | tbIMDBNr.Text.Length > 9) && !tbIMDBNr.Text.StartsWith("tt"))
      {
        tbIMDBNr.Text = string.Empty;
      }
      else
      {
        Match ttNo = Regex.Match(tbIMDBNr.Text, @"tt[\d]{7}?");
        if (ttNo.Success == false)
        {
          tbIMDBNr.Text = string.Empty;
        }
      }
      
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
          MessageBox.Show("Please enter a movie title or movie file", "Information", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return;
        }
        details.ID = VideoDatabase.AddMovieFile(file);
        if (details.ID == -1)
        {
          MessageBox.Show("Could not save movie to database", "Information", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return;
        }
      }
      //
      // IMDB id save if user change it
      //
      details.IMDBNumber = tbIMDBNr.Text;

      VideoDatabase.SetMovieInfoById(details.ID, ref details);
      // Add files to movie
      string strPath = string.Empty;
      foreach (ListViewItem item in listViewFiles.Items)
      {
        string strFileName;

        DatabaseUtility.Split(item.Text, out strPath, out strFileName);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        int pathId = VideoDatabase.AddPath(strPath);
        VideoDatabase.AddFile(details.ID, pathId, strFileName);
      }
      string dvdLabel = string.Empty;
      if (GetValidatedDVDLabel(ref dvdLabel))
      {
        if (dvdLabel.Length > 0)
        {
          if (!Util.Utils.IsDVD(strPath))
          {
            if (
              MessageBox.Show(
                "The file list suggests that this movie is not on disc and thus the label will not be shown. Store it anyway?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
              VideoDatabase.SetDVDLabel(details.ID, dvdLabel);
            }
          }
          else
          {
            VideoDatabase.SetDVDLabel(details.ID, dvdLabel);
          }
        }
      }
      else
      {
        MessageBox.Show("Disc # is invalid and has not been stored. Enter an integer between 0 and 999", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      // Refresh movies if new is added manualy
      if (cbTitle.SelectedIndex == cbTitle.Items.Count - 1)
      {
        LoadMovies(0);
      }
      else // Refresh selected
      {
        UpdateActiveMovieImageAndThumbs(tbImageLocation.Text, CurrentMovie.ID, CurrentMovie.Title);
        RefreshMovie(details.ID, cbTitle.SelectedIndex);
      }
    }

    // Change in code for Cover find
    private void btnSearchCover_Click(object sender, EventArgs e)
    {
      int max = 0; // Max progressbar value

      // Cover search source check
      if (chbTMDBCoverSource.Checked)
      {
        max++;
      }
      if (chbImpAwCoverSource.Checked)
      {
        max++;
      }
      if (chbIMDBCoverSource.Checked)
      {
        max++;
      }

      btnSearchCover.Enabled = false;
      coversListBox.Items.Clear();
      coversListBox.Enabled = false;

      // PBar intialization (progress bar when covers is searched)
      Cursor = Cursors.WaitCursor;
      pbSearchCover.Minimum = 0;
      pbSearchCover.Maximum = max;
      pbSearchCover.Value = 0;

      // Draw percentage into progressbar
      ProgressBarDrawPercentage(ref pbSearchCover, 0);

      // Local images
      string strFilename = string.Empty;
      string strPath = string.Empty;
      Util.Utils.Split(listViewFiles.Items[0].Text, out strPath, out strFilename);

      if (Directory.Exists(strPath))
      {
        max++;
        pbSearchCover.Maximum = max;
        ProgressBarAdvance(ref pbSearchCover, "Searching Local IMG... ", true);

        DirectoryInfo di = new DirectoryInfo(strPath);
        FileInfo[] jpgFiles = di.GetFiles("*.jpg");

        int count = 1;

        foreach (FileInfo file in jpgFiles)
        {
          ComboBoxArt art = new ComboBoxArt(String.Format("Local Picture {0}", count), file.FullName);
          coversListBox.Items.Add(art);
          coversListBox.Refresh();
          ++count;
        }
        ProgressBarAdvance(ref pbSearchCover, "Searching Local IMG... ", false);
      }

      // TMDB Cover search
      if (chbTMDBCoverSource.Checked)
      {
        ProgressBarAdvance(ref pbSearchCover, "Searching TMDB... ", true);

        TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();
        // Call is made by IMDBNumber parameter because we're targeting specific movie not guessing
        tmdbSearch.SearchCovers(tbCoverSearchStr.Text, CurrentMovie.IMDBNumber);
        if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
        {
          for (int i = 0; i < tmdbSearch.Count; ++i)
          {
            ComboBoxArt art = new ComboBoxArt(String.Format("TMDB Cover {0}", (i + 1)), tmdbSearch[i]);
            coversListBox.Items.Add(art);
            coversListBox.Refresh();
          }
        }
        ProgressBarAdvance(ref pbSearchCover, "Searching TMDB... ", false);
      }

      // IMPAwards covers
      if (chbImpAwCoverSource.Checked)
      {
        ProgressBarAdvance(ref pbSearchCover, "Searching IMPAw... ", true);

        IMPAwardsSearch impSearch = new IMPAwardsSearch();
        impSearch.SearchCovers(CurrentMovie.Title, CurrentMovie.IMDBNumber);

        if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
        {
          for (int i = 0; i < impSearch.Count; ++i)
          {
            // Get picture name without extension
            string impawPicName = Path.GetFileNameWithoutExtension(impSearch[i]);
            // Better is to see Picture name in the list
            ComboBoxArt art = new ComboBoxArt(String.Format("IMPAw " + impawPicName, (i + 1)), impSearch[i]);
            coversListBox.Items.Add(art);
            coversListBox.Refresh();
          }
        }
        ProgressBarAdvance(ref pbSearchCover, "Searching IMP Awards... ", false);
      }

      // IMDB Cover Search
      if (chbIMDBCoverSource.Checked)
      {
        ProgressBarAdvance(ref pbSearchCover, "Searching IMDB... ", true);

        IMDBSearch imdbSearch = new IMDBSearch();
        // Call is made by IMDBNumber parameter because we're targeting specific movie not guessing
        imdbSearch.SearchCovers(CurrentMovie.IMDBNumber, false);
        if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
        {
          for (int i = 0; i < imdbSearch.Count; ++i)
          {
            if (i == 0)
            {
              ComboBoxArt art = new ComboBoxArt("IMDB   Default", imdbSearch[i]);
              coversListBox.Items.Add(art);
              coversListBox.Refresh();
            }
            else
            {
              ComboBoxArt art = new ComboBoxArt(String.Format("IMDB   Picture {0}", (i + 1)), imdbSearch[i]);
              coversListBox.Items.Add(art);
              coversListBox.Refresh();
            }
          }
        }
        ProgressBarAdvance(ref pbSearchCover, "Searching IMDB... ", false);
      }

      if (coversListBox.Items.Count == 0)
      {
        coversListBox.Items.Clear();
        coversListBox.Items.Add(new ComboBoxArt("No covers found...", ""));
      }
      else
      {
        coversListBox.Enabled = true;
      }
      // End search covers
      ComboBoxArt artImage = coversListBox.SelectedItem as ComboBoxArt;
      if (artImage != null)
      {
        tbImageLocation.Text = artImage.Url;
      }
      //UpdateActiveMovieImageAndThumbs(tbImageLocation.Text);
      // Refresh movie
      if (coversListBox.Items.Count > 0)
      {
        coversListBox.SelectedIndex = 0;
      }
      else
      {
        _clearListBox = false;
        RefreshMovie(CurrentMovie.ID, cbTitle.SelectedIndex);
        _clearListBox = true;
      }
      btnSearchCover.Enabled = true;
      pbSearchCover.Value = 0;
      Cursor = Cursors.Default;
    }

    // Changed - fanart delete files
    private void btnDelete_Click(object sender, EventArgs e)
    {
      if (CurrentMovie.ID < 0)
      {
        return;
      }
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this movie?", "Information",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (dialogResult == DialogResult.Yes)
      {
        string strFilenameAndPath = string.Empty;
        if (listViewFiles.Items.Count > 0)
        {
          strFilenameAndPath = listViewFiles.Items[0].Text;
        }
        // Delete movie
        VideoDatabase.DeleteMovieInfoById(CurrentMovie.ID);
        // Delete covers
        FanArt.DeleteCovers(CurrentMovie.Title, CurrentMovie.ID);
        // Delete fanarts
        FanArt.DeleteFanarts(strFilenameAndPath, CurrentMovie.Title);
        // When delete movie from the database do not back to index 0
        {
          int currentIndex = cbTitle.SelectedIndex;
          if (currentIndex > 0)
          {
            currentIndex--;
          }
          LoadMovies(0);
          cbTitle.SelectedIndex = currentIndex;
        }
      }
    }

    private void buttonImport_Click(object sender, EventArgs e)
    {
      int id = 0;
      OpenFileDialog find_file = new OpenFileDialog();
      find_file.RestoreDirectory = true;
      find_file.DefaultExt = "xml";
      find_file.Filter = "DVD Profile|*.xml";
      find_file.InitialDirectory = ".";
      find_file.Title = "Select DVD Profiler database" + tbTitle.Text;
      if (find_file.ShowDialog(this) != DialogResult.OK)
      {
        return;
      }
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

        string genre = string.Empty;
        XmlNodeList genreList = nodeDVD.SelectNodes("Genres/Genre");
        foreach (XmlNode nodeGenre in genreList)
        {
          // added check to see if nodeGenre was valid
          if (nodeGenre != null && nodeGenre.InnerText != null)
          {
            if (genre.Length > 0)
            {
              genre += " / ";
            }
            genre += nodeGenre.InnerText;
          }
        }
        string cast = "Cast overview:";
        XmlNodeList actorsList = nodeDVD.SelectNodes("Actors/Actor");
        foreach (XmlNode nodeActor in actorsList)
        {
          string firstname = string.Empty;
          string lastname = string.Empty;
          string role = string.Empty;
          XmlNode nodeFirstName = nodeActor.SelectSingleNode("FirstName");
          XmlNode nodeLastName = nodeActor.SelectSingleNode("LastName");
          XmlNode nodeRole = nodeActor.SelectSingleNode("Role");
          if (nodeFirstName != null && nodeFirstName.InnerText != null)
          {
            firstname = nodeFirstName.InnerText;
          }
          if (nodeLastName != null && nodeLastName.InnerText != null)
          {
            lastname = nodeLastName.InnerText;
          }
          if (nodeRole != null && nodeRole.InnerText != null)
          {
            role = nodeRole.InnerText;
          }
          string line = String.Format("{0} {1} as {2}\n", firstname, lastname, role);
          cast += line;
        }


        string credits = string.Empty;
        XmlNodeList creditsList = nodeDVD.SelectNodes("Credits/Credit");
        foreach (XmlNode nodeCredit in creditsList)
        {
          // Added check for firstname, lastname valid
          string firstname = string.Empty;
          string lastname = string.Empty;
          XmlNode nodeFirstName = nodeCredit.SelectSingleNode("FirstName");
          XmlNode nodeLastName = nodeCredit.SelectSingleNode("LastName");
          if (nodeFirstName != null && nodeFirstName.InnerText != null)
          {
            firstname = nodeFirstName.InnerText;
          }
          if (nodeLastName != null && nodeLastName.InnerText != null)
          {
            lastname = nodeLastName.InnerText;
          }
          if (credits.Length > 0)
          {
            credits += " / ";
          }
          credits += String.Format("{0} {1}", firstname, lastname);
        }

        IMDBMovie movie = new IMDBMovie();
        movie.Cast = cast;
        movie.CDLabel = string.Empty;
        movie.Director = string.Empty;
        movie.DVDLabel = string.Empty;
        movie.File = string.Empty;
        movie.Genre = genre;
        movie.IMDBNumber = string.Empty;
        // Added check to validate rating
        if (nodeRating != null)
        {
          movie.MPARating = nodeRating.InnerText;
        }
        else
        {
          movie.MPARating = "NR";
        }
        movie.Path = string.Empty;
        // Added check to validate overview and duration
        if (nodeOverview != null)
        {
          movie.Plot = nodeOverview.InnerText;
        }
        else
        {
          movie.Plot = string.Empty;
        }
        movie.PlotOutline = string.Empty;
        movie.Rating = 0;
        if (nodeDuration != null)
        {
          movie.RunTime = Int32.Parse(nodeDuration.InnerText);
        }
        else
        {
          movie.RunTime = 0;
        }
        movie.SearchString = string.Empty;
        movie.TagLine = string.Empty;
        movie.ThumbURL = string.Empty;
        movie.Title = nodeTitle.InnerText;
        movie.Top250 = 0;
        movie.Votes = string.Empty;
        movie.Watched = 0;
        movie.WritingCredits = credits;
        // Added check to validate year
        if (nodeYear != null)
        {
          movie.Year = Int32.Parse(nodeYear.InnerText);
        }
        else
        {
          movie.Year = 0;
        }
        id = VideoDatabase.AddMovie(movie.Title, true);
        movie.ID = id;
        VideoDatabase.SetMovieInfoById(id, ref movie);
        Application.DoEvents();
      }
      LoadMovies(id);
    }

    // Changed
    private IMDBMovie CurrentMovie
    {
      get
      {
        IMDBMovie movie = new IMDBMovie();
        if (cbTitle.SelectedItem != null)
        {
          ComboBoxItemMovie cbMovie = (ComboBoxItemMovie)cbTitle.SelectedItem;
          movie.ID = cbMovie.Movie.ID;
          // Needed for IMDB Covers search Deda 30.4.2010
          movie.IMDBNumber = cbMovie.Movie.IMDBNumber;
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
          {
            movie.Watched = 1;
          }
          else
          {
            movie.Watched = 0;
          }

          movie.Title = tbTitle.Text;
          movie.Director = tbDirector.Text;
          movie.MPARating = tbMPAARating.Text;
          movie.RunTime = Int32.Parse(tbDuration.Text);
          movie.WritingCredits = tbWritingCredits.Text;
          movie.Plot = tbSummary.Text;
          movie.UserReview = tbReview.Text; // Added review         
          movie.Rating = (float)Double.Parse(tbRating.Text);
          movie.TagLine = tbTagline.Text;
          movie.Year = Int32.Parse(tbYear.Text);
          movie.ThumbURL = (_useLocalImage ? "file://" + tbImageLocation.Text : tbImageLocation.Text);
          movie.FanartURL = (_useLocalImageFanart ? "file://" + tbFanartLocation.Text : tbFanartLocation.Text);
          movie.Votes = tbVotes.Text;
          movie.PlotOutline = tbPlotOutline.Text;
        }
        foreach (ListViewItem item in listViewGenres.Items)
        {
          if (movie.Genre == string.Empty)
          {
            movie.Genre = item.Text;
          }
          else
          {
            movie.Genre += " / " + item.Text;
          }
        }

        foreach (ListViewItem item in listViewMovieActors.Items)
        {
          string actor = item.SubItems[0].Text;
          if (item.SubItems[1].Text != string.Empty)
          {
            actor += " as " + item.SubItems[1].Text;
          }
          if (movie.Cast == string.Empty)
          {
            movie.Cast = actor;
          }
          else
          {
            movie.Cast += "\n" + actor;
          }
        }
        // Sort listview
        listViewMovieActors.Sort();
        return movie;
      }
    }

    #endregion

    private void OnFuzzyMatchingCheckedChanged(object sender, EventArgs e)
    {
      _isFuzzyMatching = ((CheckBox)sender).Checked;
      SaveSettings();
    }

    #region Persistance

    public override void LoadSettings() {}

    // Changed, added vdb start, cover upgrade, fanarts, folder movie title
    private void Load()
    {
      Cursor.Current = Cursors.WaitCursor;
      using (Settings xmlreader = new MPSettings())
      {
        // Cover file names upgrade
        _coversUpgraded = xmlreader.GetValueAsBool("moviedatabase", "coversupgraded", false);
        if (_coversUpgraded)
        {
          btnUpgradeCovers.Enabled = false;
          btnDowngradeCovers.Enabled = true;
        }
        else
        {
          btnUpgradeCovers.Enabled = true;
          btnDowngradeCovers.Enabled = false;
        }

        _isFuzzyMatching = xmlreader.GetValueAsBool("movies", "fuzzyMatching", true);

        _fuzzyMatchingCheckBox.Checked = _isFuzzyMatching;

        // FanArt setting
        string configDir;
        FanArt.GetFanArtFolder(out configDir);
        if (Directory.Exists(configDir))
        {
          _useFanArt = xmlreader.GetValueAsBool("moviedatabase", "usefanart", false);
        }
        else
        {
          _useFanArt = false;
        }
        useFanartCheckBox.Checked = _useFanArt;
        fanartQ.Value = xmlreader.GetValueAsInt("moviedatabase", "fanartnumber", 1);
        chbFanartShare.Checked = xmlreader.GetValueAsBool("moviedatabase", "usefanartshare", true);

        if (_useFanArt)
        {
          fanartQ.Enabled = true;
          chbFanartShare.Enabled = true;
        }
        else
        {
          fanartQ.Enabled = false;
          chbFanartShare.Enabled = false;
        }
        SetFanartFileIndexLabel(0);

        // Folder movie title
        _useFolderAsTitle = xmlreader.GetValueAsBool("moviedatabase", "usefolderastitle", false);
        useFoldernameCheckBox.Checked = _useFolderAsTitle;
        if (_useFolderAsTitle)
        {
          preferFileNameCheckBox.Visible = true;
        }
        preferFileNameCheckBox.Checked = xmlreader.GetValueAsBool("moviedatabase", "preferfilenameforsearch", false);

        // Strip movie title prefix
        checkBoxStripTitlePrefix.Checked = xmlreader.GetValueAsBool("moviedatabase", "striptitleprefixes", false);
        tbTitlePrefixes.Text = xmlreader.GetValueAsString("moviedatabase", "titleprefixes", "The, Les, Die");

        // Load activated databases-Changed 
        skipCheckBox.Checked = true; // xmlreader.GetValueAsBool("moviedatabase", "scanskipexisting", false);
        actorsCheckBox.Checked = xmlreader.GetValueAsBool("moviedatabase", "getactors", true);

        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
        if (iNumber > 0)
        {
          for (int i = 0; i < iNumber; i++)
          {
            string strLimit = xmlreader.GetValueAsString("moviedatabase", "limit" + i, "false");
            string strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i, "false");
            string strLanguage = xmlreader.GetValueAsString("moviedatabase", "language" + i, "false");
            string strTitle = xmlreader.GetValueAsString("moviedatabase", "title" + i, "false");

            if ((strLimit != "false") && (strDatabase != "false") && (strLanguage != "false") && (strTitle != "false"))
            {
              ListViewItem item = lvDatabase.Items.Add(strDatabase);
              item.SubItems.Add(strTitle);
              item.SubItems.Add(strLanguage);
              item.SubItems.Add(strLimit);
            }
          }
        }

        ReloadGrabberScripts();
      }
      Cursor.Current = Cursors.Default;
      _settingsLoaded = true;
    }

    // Changed, added vdb start, cover upgrade, fanarts, folder movie title
    public override void SaveSettings()
    {
      if (!_settingsLoaded)
      {
        return;
      }

      using (Settings xmlwriter = new MPSettings())
      {
        // Cover upgrade
        xmlwriter.SetValueAsBool("moviedatabase", "coversupgraded", _coversUpgraded);

        xmlwriter.SetValueAsBool("movies", "fuzzyMatching", _isFuzzyMatching);
        // FanArt
        xmlwriter.SetValueAsBool("moviedatabase", "usefanart", _useFanArt);
        xmlwriter.SetValue("moviedatabase", "fanartnumber", (int)fanartQ.Value);
        xmlwriter.SetValueAsBool("moviedatabase", "usefanartshare", chbFanartShare.Checked);

        // Folder movie title
        xmlwriter.SetValueAsBool("moviedatabase", "usefolderastitle", _useFolderAsTitle);
        xmlwriter.SetValueAsBool("moviedatabase", "preferfilenameforsearch", preferFileNameCheckBox.Checked);

        // Strip movie title prefix
        xmlwriter.SetValueAsBool("moviedatabase", "striptitleprefixes", checkBoxStripTitlePrefix.Checked);
        xmlwriter.SetValue("moviedatabase", "titleprefixes", tbTitlePrefixes.Text);

        // Database
        xmlwriter.SetValueAsBool("moviedatabase", "scanskipexisting", skipCheckBox.Checked);
        xmlwriter.SetValueAsBool("moviedatabase", "getactors", actorsCheckBox.Checked);

        xmlwriter.SetValue("moviedatabase", "number", lvDatabase.Items.Count);
        for (int i = 0; i < lvDatabase.Items.Count; i++)
        {
          xmlwriter.SetValue("moviedatabase", "database" + i,
                             lvDatabase.Items[i].SubItems[chDatabaseDB.Index].Text);
          xmlwriter.SetValue("moviedatabase", "title" + i,
                             lvDatabase.Items[i].SubItems[chDatabaseTitle.Index].Text);
          xmlwriter.SetValue("moviedatabase", "language" + i,
                             lvDatabase.Items[i].SubItems[chDatabaseLanguage.Index].Text);
          xmlwriter.SetValue("moviedatabase", "limit" + i,
                             lvDatabase.Items[i].SubItems[chDatabaseLimit.Index].Text);
        }
        for (int i = lvDatabase.Items.Count; i < 4; i++)
        {
          xmlwriter.RemoveEntry("moviedatabase", "database" + i);
          xmlwriter.RemoveEntry("moviedatabase", "title" + i);
          xmlwriter.RemoveEntry("moviedatabase", "language" + i);
          xmlwriter.RemoveEntry("moviedatabase", "limit" + i);
        }
      }
    }

    #endregion

    #region grabber tab

    private void bDatabaseDown_Click(object sender, EventArgs e)
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
        SaveSettings();
      }
    }

    private void bDatabaseUp_Click(object sender, EventArgs e)
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
        SaveSettings();
      }
    }

    private void lvDatabase_DeleteSelectedItem()
    {
      if (lvDatabase.SelectedIndices.Count <= 0)
      {
        return;
      }

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

      UpdateAvailableScripts();
    }

    private void lvDatabase_KeyUp(Object o, KeyEventArgs e)
    {
      if (e.KeyCode == System.Windows.Forms.Keys.Delete || e.KeyCode == System.Windows.Forms.Keys.Back)
      {
        lvDatabase_DeleteSelectedItem();
      }
    }

    private void lvDatabase_DoubleClick(object sender, EventArgs e)
    {
      Point pt = lvDatabase.PointToClient(Cursor.Position);
      ListViewItem item = lvDatabase.GetItemAt(pt.X, pt.Y);
      if (item == null)
      {
        return;
      }

      Rectangle subItemRect = Rectangle.Empty;
      Rectangle lviBounds = item.GetBounds(ItemBoundsPortion.Entire);

      int subItemX = lviBounds.Left;
      int i = 0;
      while (i < chDatabaseLimit.Index)
      {
        subItemX += lvDatabase.Columns[i].Width;
        i++;
      }

      subItemRect = new Rectangle(subItemX, lviBounds.Top, lvDatabase.Columns[chDatabaseLimit.Index].Width,
                                  lviBounds.Height);
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
      Point ctlOrigin = mpNumericUpDownLimit.Parent.PointToScreen(origin);

      subItemRect.Offset(lvOrigin.X - ctlOrigin.X, lvOrigin.Y - ctlOrigin.Y);

      // Position and show editor
      mpNumericUpDownLimit.Bounds = subItemRect;

      // if the value is not able to parse don't show the editor
      int limit;
      if (int.TryParse(item.SubItems[chDatabaseLimit.Index].Text, out limit))
      {
        mpNumericUpDownLimit.Value = limit;
      }
      else
      {
        mpNumericUpDownLimit.Value = IMDB.DEFAULT_SEARCH_LIMIT;
      }

      mpNumericUpDownLimit.Visible = true;
      mpNumericUpDownLimit.BringToFront();
      mpNumericUpDownLimit.Focus();

      _editItem = item;
    }

    private void mpDelete_Click(object sender, EventArgs e)
    {
      lvDatabase_DeleteSelectedItem();
    }

    private void mpButtonAdd_Click(object sender, EventArgs e)
    {
      ComboBoxItemDatabase database = mpComboBoxAvailableDatabases.SelectedItem as ComboBoxItemDatabase;
      if (database == null)
      {
        return;
      }

      ListViewItem item = this.lvDatabase.Items.Add(database.Database);
      item.SubItems.Add(database.Name);
      item.SubItems.Add(database.Language);
      item.SubItems.Add(database.Limit);

      SaveSettings();

      UpdateAvailableScripts();
    }

    private void mpButtonUpdateGrabber_Click(object sender, EventArgs e)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        MessageBox.Show("Update failed. Please check your internet connection!", "", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
        return;
      }

      _progressDialog = new DlgProgress();
      _progressDialog.SetHeading("Updating MovieInfo grabber scripts...");
      _progressDialog.TopMost = true;

      // download index file
      _progressDialog.SetLine1("Downloading the index file...");
      _progressDialog.SetLine2("Downloading...");
      _progressDialog.Total = 1;
      _progressDialog.Count = 1;
      _progressDialog.Show();
      if (DownloadFile(_grabberIndexFile, GrabberIndexUrl) == false)
      {
        _progressDialog.CloseProgress();
        return;
      }

      // read index file
      if (!File.Exists(_grabberIndexFile))
      {
        MessageBox.Show("No GrabberIndexFile found.");
        _progressDialog.CloseProgress();
        return;
      }
      XmlDocument doc = new XmlDocument();
      doc.Load(_grabberIndexFile);
      XmlNodeList sectionNodes = doc.SelectNodes("MovieInfoGrabber/grabber");

      // download all grabbers
      _progressDialog.Total = sectionNodes.Count;
      int percent = 0;
      for (int i = 0; i < sectionNodes.Count; i++)
      {
        if (_progressDialog.DialogResult == DialogResult.Cancel)
        {
          break;
        }

        string url = sectionNodes[i].Attributes["url"].Value;
        string id = Path.GetFileName(url);
        _progressDialog.SetLine1("Downloading grabber: " + id);
        _progressDialog.SetLine2("Processing grabbers...");
        _progressDialog.SetPercentage(percent);
        _progressDialog.Count = i + 1;
        percent += 100 / (sectionNodes.Count - 1);

        if (DownloadFile(IMDB.ScriptDirectory + @"\" + id, url) == false)
        {
          _progressDialog.CloseProgress();
          return;
        }
      }

      // Ask to remove out of date grabbers
      if (MessageBox.Show("Do you want to delete all grabbers which are not supported anymore?", "Movie Info grabber",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) ==
          DialogResult.Yes)
      {
        DirectoryInfo di = new DirectoryInfo(IMDB.ScriptDirectory);
        FileInfo[] fileList = di.GetFiles("*.csscript", SearchOption.AllDirectories);
        foreach (FileInfo f in fileList)
        {
          bool found = false;

          for (int i = 0; i < sectionNodes.Count; i++)
          {
            string url = sectionNodes[i].Attributes["url"].Value;

            if (f.Name == Path.GetFileName(url))
            {
              found = true;
              break;
            }
          }

          if (!found)
          {
            f.Delete();
          }
        }
      }

      _progressDialog.CloseProgress();

      ReloadGrabberScripts();
    }

    private void mpNumericUpDownLimit_Leave(object sender, EventArgs e)
    {
      // cell editor losing focus
      EndEditing(true);
    }

    private void mpNumericUpDownLimit_KeyPress(object sender, PreviewKeyDownEventArgs e)
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
    /// <param name="acceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
    public void EndEditing(bool acceptChanges)
    {
      if (acceptChanges && (_editItem != null))
      {
        _editItem.SubItems[chDatabaseLimit.Index].Text = mpNumericUpDownLimit.Value.ToString();
      }
      _editItem = null;
      mpNumericUpDownLimit.Visible = false;
    }

    /// <summary>
    /// Search for all valid GrabberScript files found in scriptDirectory.
    /// </summary>
    private void ReloadGrabberScripts()
    {
      _grabberList = new Dictionary<string, IIMDBScriptGrabber>();

      Directory.CreateDirectory(IMDB.ScriptDirectory);
      DirectoryInfo di = new DirectoryInfo(IMDB.ScriptDirectory);

      FileInfo[] fileList = di.GetFiles("*.csscript", SearchOption.AllDirectories);
      foreach (FileInfo f in fileList)
      {
        try
        {
          AsmHelper script = new AsmHelper(CSScript.Load(f.FullName, null, false));
          IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");

          _grabberList.Add(Path.GetFileNameWithoutExtension(f.FullName), grabber);
        }
        catch (Exception ex)
        {
          //textBox3.Text = ex.Message;
          Log.Error("Script grabber error file: {0}, message : {1}", f.FullName, ex.Message);
        }
      }

      UpdateAvailableScripts();
    }

    /// <summary>
    /// Reloads all grabber in the combobox, which are not used atm.
    /// </summary>
    private void UpdateAvailableScripts()
    {
      List<ComboBoxItemDatabase> dbList = new List<ComboBoxItemDatabase>();

      foreach (KeyValuePair<string, IIMDBScriptGrabber> grabber in _grabberList)
      {
        bool found = false;
        foreach (ListViewItem item in lvDatabase.Items)
        {
          if (item.SubItems[chDatabaseDB.Index].Text == grabber.Key)
          {
            found = true;
            break;
          }
        }

        if (!found)
        {
          ComboBoxItemDatabase item = new ComboBoxItemDatabase();
          item.Database = grabber.Key;
          item.Language = grabber.Value.GetLanguage();
          item.Limit = IMDB.DEFAULT_SEARCH_LIMIT.ToString();
          item.Name = grabber.Value.GetName();

          dbList.Add(item);
        }
      }

      // sort all available db entries
      dbList.Sort(new DatabaseComparer());

      // add dbentries to comboBox
      mpComboBoxAvailableDatabases.Items.Clear();
      mpComboBoxAvailableDatabases.Items.AddRange(dbList.ToArray());

      // set the first entry "activ"
      if (mpComboBoxAvailableDatabases.Items.Count > 0)
      {
        mpComboBoxAvailableDatabases.SelectedIndex = 0;
        mpButtonAddGrabber.Enabled = true;
      }
      else
      {
        mpButtonAddGrabber.Enabled = false;
      }
    }

    private bool DownloadFile(string filepath, string url)
    {
      string grabberTempFile = Path.GetTempFileName();

      Application.DoEvents();
      try
      {
        if (File.Exists(grabberTempFile))
        {
          File.Delete(grabberTempFile);
        }

        Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // request.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}
        Application.DoEvents();

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream, Encoding.Default))
            {
              using (TextWriter tout = File.CreateText(grabberTempFile))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null)
                  {
                    break;
                  }
                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        File.Delete(filepath);
        File.Move(grabberTempFile, filepath);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("EXCEPTION in DownloadFile | {0}\r\n{1}", ex.Message, ex.Source);
        MessageBox.Show("Grabber update failed.");
        return false;
      }
    }

    #endregion

    private bool GetValidatedDVDLabel(ref string dvdLabel)
    {
      if (tbDiscNr.Text.Length == 0)
      {
        dvdLabel = string.Empty;
        return true;
      }

      int discNr;
      try
      {
        discNr = Convert.ToInt16(tbDiscNr.Text);
      }
      catch (Exception)
      {
        return false;
      }
      if (discNr < 0 || discNr > 999)
      {
        return false;
      }

      // Note: Convert from string to int and then back to string is not totally uncalled for. 
      // We don't want the user to enter e.g. 0043 and get away with it ;-)
      if (discNr < 10)
      {
        dvdLabel = "DVD#00" + Convert.ToString(discNr);
      }
      else if (discNr < 100)
      {
        dvdLabel = "DVD#0" + Convert.ToString(discNr);
      }
      else
      {
        dvdLabel = "DVD#" + Convert.ToString(discNr);
      }
      return true;
    }

    // Changed - Fanart-Actor info, refresh infos with cached data (use LoadMovies(int movieDBid) for full refresh)
    private void cbTitle_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cbTitle.SelectedItem != null)
      {
        ComboBoxItemMovie item = (ComboBoxItemMovie)cbTitle.SelectedItem;
        UpdateEdit(item.Movie);
        string configDir;
        FanArt.GetFanArtFolder(out configDir);

        if (_clearListBox)
        {
          fanartListBox.Items.Clear();
          _fanartImgIndex = 0;
          SetFanartFileIndexLabel(_fanartImgIndex);
        }
        else
        {
          tbFanartLocation.Text = FanArt.SetFanArtFileName(item.Movie.Title, _fanartImgIndex);
        }
        if (!_isRefreshing)
        {
          pictureBoxFanArt.ImageLocation = FanArt.SetFanArtFileName(item.Movie.Title, _fanartImgIndex);
          // Update cover search string
          tbCoverSearchStr.Text = tbTitle.Text;
          // FanArt Picture
          tbFASearchString.Text = tbTitle.Text; // Update fanart search string
          // Actor details and actor movies fill or clear
          ActorsTableRefresh(Int32.Parse(tbMovieID.Text));
          PopulateActorInfo();
          if (cbActorMovies.Items.Count < 0)
            cbActorMovies.SelectedIndex = -1;
        }
        // Fanart tab fields show/hide
        ShowHide();
      }
    }

    private void btnBrowseLocalCover_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();

      dlg.AddExtension = true;
      dlg.Filter = "JPEG Image (*.jpg,*.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*";
      dlg.RestoreDirectory = false;

      if (listViewFiles.Items.Count > 0)
      {
        string strFilename = string.Empty;
        string strPath = string.Empty;
        Util.Utils.Split(listViewFiles.Items[0].Text, out strPath, out strFilename);
        dlg.InitialDirectory = strPath;
      }
      else
      {
        // start in current folder
        dlg.InitialDirectory = ".";
      }

      // open dialog
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        tbImageLocation.Text = dlg.FileName;
        UpdateActiveMovieImageAndThumbs(tbImageLocation.Text, CurrentMovie.ID, CurrentMovie.Title);
        // Refresh movie
        RefreshMovie(CurrentMovie.ID, cbTitle.SelectedIndex);
      }
    }

    // Cover listbox item click
    private void coversListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBoxArt art = coversListBox.SelectedItem as ComboBoxArt;
      if (art != null)
      {
        tbImageLocation.Text = art.Url;
      }
      UpdateActiveMovieImageAndThumbs(tbImageLocation.Text, CurrentMovie.ID, CurrentMovie.Title);
      // Refresh movie
      _clearListBox = false;
      RefreshMovie(CurrentMovie.ID, cbTitle.SelectedIndex);
      _clearListBox = true;
    }

    /*
        private int BinarySearch(ListView.ListViewItemCollection items, string item)
        {
          int left = 0;
          int right = items.Count - 1;
          int midPoint = 0;

          while (left <= right)
          {
            midPoint = (left + right) / 2;
            int comparisonValue = item.CompareTo(items[midPoint].Text);

            if (comparisonValue == 0)
            {
              return midPoint;
            }
            else if (comparisonValue > 0)
            {
              left = midPoint + 1;
            }
            else
            {
              right = midPoint - 1;
            }
          }

          return -1;
        }
    */

    // Save thumbs for covers and actors, database update with pic link
    private void UpdateActiveMovieImageAndThumbs(string strImageUrl, int movieID, string movieTitle)
    {
      if (strImageUrl == string.Empty)
      {
        return;
      }

      bool bIsUrl = (strImageUrl.Substring(0, 7) == @"http://");

      // Clear previous image
      if (pictureBoxCover.Image != null)
      {
        pictureBoxCover.Image.Dispose();
        pictureBoxCover.Image = null;
      }
      // Cover save new method
      string titleExt = movieTitle + "{" + movieID + "}";
      string strThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
      string largeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);

      // Delete old thumbs
      Util.Utils.FileDelete(strThumb);
      Util.Utils.FileDelete(largeThumb);

      // Create thumbs for URL files
      if (bIsUrl)
      {
        IMDBFetcher.DownloadCoverArt(Thumbs.MovieTitle, strImageUrl, titleExt);
      }
      else
      {
        if (!File.Exists(strImageUrl))
        {
          return;
        }
      }
      // folder.jpg for ripped DVDs
      try
      {
        string fileDVD = listViewFiles.Items[0].Text;
        string path, filename;
        Util.Utils.Split(fileDVD, out path, out filename);

        if (filename.ToUpper() == "VIDEO_TS.IFO")
        {
          string directoryDVD = path.Substring(0, path.LastIndexOf("\\"));
          if (Directory.Exists(directoryDVD))
          {
            File.Copy(largeThumb, directoryDVD + "\\folder.jpg", true);
          }
        }
      }
      catch (Exception) {}
      // Create new thumbs for local user files
      try
      {
        if (!bIsUrl)
        {
          if (Util.Picture.CreateThumbnail(strImageUrl, strThumb, (int)Thumbs.ThumbResolution,
                                           (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
          {
            Util.Picture.CreateThumbnail(strImageUrl, largeThumb, (int)Thumbs.ThumbLargeResolution,
                                         (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
          }
        }
      }
      catch (Exception) {}

      string file = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
      if (File.Exists(file) && !_isRefreshing)
      {
        try
        {
          using (Image img = Image.FromFile(file))
          {
            Bitmap result = new Bitmap(img.Width, img.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
              g.CompositingQuality = Thumbs.Compositing;
              g.InterpolationMode = Thumbs.Interpolation;
              g.SmoothingMode = Thumbs.Smoothing;
              g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            }
            pictureBoxCover.Image = result;
          }
        }
        catch (Exception) {}
      }

      if (!bIsUrl)
      {
        _useLocalImage = true;
        VideoDatabase.SetThumbURL(movieID, "file://" + strImageUrl);
      }
      else
      {
        VideoDatabase.SetThumbURL(movieID, strImageUrl);
        _useLocalImage = false;
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start((string)e.Link.LinkData);
    }

    #region New controls code

    #region Covers

    // Refresh all covers
    private void btnRefreshAllCovers_Click(object sender, EventArgs e)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        MessageBox.Show("No active Internet connection.\nPlease check your network settings and try again.",
                        "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (!chbTMDBCoverSource.Checked && !chbImpAwCoverSource.Checked && !chbIMDBCoverSource.Checked)
      {
        MessageBox.Show("No cover source selected.");
        return;
      }

      // Some cleanup before refresh
      coversListBox.Items.Clear();
      // Set refresh status for background worker
      _isRefreshing = true;
      // Freeze current panel (do not mess up while refreshing)
      this.Enabled = false;
      // Progress setup
      _progressDialog = new DlgProgress();
      _progressDialog.SetHeading("Refreshing covers");
      _progressDialog.TopMost = true;
      _progressDialog.DisableCancel();
      _progressDialog.SetLine1("Downloading cover for:");
      _progressDialog.SetLine2("Downloading...");
      _progressDialog.SetPercentage(100);
      _progressDialog.Total = cbTitle.Items.Count - 1;
      _progressDialog.Count = 1;
      _progressDialog.Show();
      // Set backgroundworker
      BackgroundWorker bgwCover = new BackgroundWorker();
      bgwCover.WorkerSupportsCancellation = true;
      bgwCover.WorkerReportsProgress = false;
      bgwCover.DoWork += new DoWorkEventHandler(RefreshCovers);
      bgwCover.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CancelWorker);
      // Start worker by passing parameter moviecollection
      bgwCover.RunWorkerAsync();

      while (_isRefreshing)
      {
        if (!_progressDialog.CancelScan)
        {
          Application.DoEvents();
        }
        else
        {
          _isRefreshing = false;
          bgwCover.CancelAsync();
          return;
        }
      }
    }

    // Refresh covers DoWork event handler
    private void RefreshCovers(object sender, DoWorkEventArgs e)
    {
      ArrayList movies = new ArrayList();
      VideoDatabase.GetMovies(ref movies);

      foreach (IMDBMovie movie in movies)
      {
        if (!_isRefreshing)
        {
          e.Cancel = true;
          break;
        }

        _progressDialog.SetLine1("Downloading cover for: " + movie.Title);

        // Skip no IMDBid movie (better than fetch wrong cover)
        if (CheckImdbId(movie.IMDBNumber) == false)
        {
          if (_progressDialog.Count < movies.Count - 1)
            _progressDialog.Count++;
          continue;
        }
        //TMDB Search first (best covers)
        TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();
        if (chbTMDBCoverSource.Checked)
          tmdbSearch.SearchCovers(movie.Title, movie.IMDBNumber);

        if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
        {
          // Update database with new cover
          UpdateActiveMovieImageAndThumbs(tmdbSearch[0], movie.ID, movie.Title);
        }

        // IMP Awards if TMDB fail
        IMPAwardsSearch impSearch = new IMPAwardsSearch();
        if (tmdbSearch.Count == 0 && chbImpAwCoverSource.Checked)
        {
          impSearch.SearchCovers(movie.Title, movie.IMDBNumber);
          if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
          {
            // Update database with new cover
            UpdateActiveMovieImageAndThumbs(impSearch[0], movie.ID, movie.Title);
          }
        }

        // IMDB Search if all fail
        if (impSearch.Count == 0 && tmdbSearch.Count == 0 && chbIMDBCoverSource.Checked)
        {
          IMDBSearch imdbSearch = new IMDBSearch();
          imdbSearch.SearchCovers(movie.IMDBNumber, true);
          if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
          {
            // Update database with new cover
            UpdateActiveMovieImageAndThumbs(imdbSearch[0], movie.ID, movie.Title);
          }
        }
        // Update progress
        if (_progressDialog.Count < movies.Count - 1)
          _progressDialog.Count++;
      }
      _isRefreshing = false;
    }

    #endregion

    #region Fanart

    // Get FanArt list for selected movie
    private void btnSearchFanart_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      fanartListBox.Items.Clear();
      fanartListBox.Refresh();
      // Proceed only if fanart options is enabled
      if (useFanartCheckBox.CheckState == CheckState.Checked)
      {
        string strFile = string.Empty;
        string strPath = string.Empty;

        Util.Utils.Split(listViewFiles.Items[0].Text, out strPath, out strFile);
        if (strFile != string.Empty & strPath != string.Empty)
        {
          FanArt.DeleteFanarts(listViewFiles.Items[0].Text, CurrentMovie.Title);
          // Download fanarts
          FanArt fanartSearch = new FanArt();
          if (!tbFASearchString.Enabled)
          {
            fanartSearch.GetTmdbFanartByApi
              (strPath, strFile, CurrentMovie.IMDBNumber, CurrentMovie.Title, false, (int)fanartQ.Value,
               chbFanartShare.Checked, string.Empty);
          }
          else
          {
            fanartSearch.GetTmdbFanartByApi
              (strPath, strFile, CurrentMovie.IMDBNumber, CurrentMovie.Title, false, (int)fanartQ.Value,
               chbFanartShare.Checked, tbFASearchString.Text);
          }
          // Update database
          VideoDatabase.SetFanartURL(CurrentMovie.ID, fanartSearch.DefaultFanartUrl);
          tbFanartLocation.Text = fanartSearch.DefaultFanartUrl;
          // Refresh movie
          LoadMovies(CurrentMovie.ID);
          // Update fanart picturebox image
          pictureBoxFanArt.ImageLocation = fanartSearch.FanartTitleFile; // fileart;
          if ((fanartSearch.Count > 0) && (fanartSearch[0] != string.Empty))
          {
            for (int i = 0; i < fanartSearch.Count; ++i)
            {
              // Get picture name without extension
              ComboBoxArt fanart = new ComboBoxArt(String.Format("Fanart " + (i + 1), (i + 1)), fanartSearch[i]);
              fanartListBox.Items.Add(fanart);
            }
            // Refresh movie
            _clearListBox = false;
            RefreshMovie(CurrentMovie.ID, cbTitle.SelectedIndex);
            _clearListBox = true;
          }
          else
          {
            fanartListBox.Items.Clear();
            fanartListBox.Items.Add(new ComboBoxArt("No fanarts found...", ""));
          }
          _fanartImgIndex = 0;
          SetFanartFileIndexLabel(_fanartImgIndex);
        }
      }
      else
      {
        MessageBox.Show("Fanart option is disabled. To enable it check \"Use Fanart\" in Settings tab");
      }

      Cursor = Cursors.Default;
    }

    // Fanart list box item click
    private void fanartListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Clear previous image
      if (pictureBoxFanArt.Image != null)
      {
        pictureBoxFanArt.Image.Dispose();
        pictureBoxFanArt.Image = null;
      }
      ComboBoxArt fanart = fanartListBox.SelectedItem as ComboBoxArt;

      if (fanart != null)
      {
        try
        {
          Cursor = Cursors.WaitCursor;
          string strFile = string.Empty;
          string strPath = string.Empty;

          Util.Utils.Split(listViewFiles.Items[0].Text, out strPath, out strFile);
          if (strFile != string.Empty & strPath != string.Empty)
          {
            FanArt fanartSearch = new FanArt();
            fanartSearch.GetTmdbFanartByUrl
              (strPath, strFile, CurrentMovie.Title, fanart.Url, _fanartImgIndex, chbFanartShare.Checked);
            FanArt.GetFanArtfilename(CurrentMovie.Title, _fanartImgIndex, out strFile);
            pictureBoxFanArt.ImageLocation = strFile;
            VideoDatabase.SetFanartURL(CurrentMovie.ID, fanart.Url);
            tbFanartLocation.Text = strFile;
          }
        }
        catch (Exception) {}
        finally
        {
          // Refresh movie
          _clearListBox = false;
          RefreshMovie(CurrentMovie.ID, cbTitle.SelectedIndex);
          _clearListBox = true;
          Cursor = Cursors.Default;
        }
      }
    }

    // Refresh all movies fanart - random arts
    private void btnRefreshAllFanarts_Click(object sender, EventArgs e)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        MessageBox.Show("No active Internet connection.\nPlease check your network settings and try again.",
                        "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      // Proceed only if fanart checkbox is enabled
      if (useFanartCheckBox.CheckState == CheckState.Checked)
      {
        // Clear previous image
        if (pictureBoxFanArt.Image != null)
        {
          pictureBoxFanArt.Image.Dispose();
          pictureBoxFanArt.Image = null;
        }

        // Some cleanup before start
        tbFASearchString.Text = string.Empty;
        fanartListBox.Items.Clear();
        // Set refresh status for background worker
        _isRefreshing = true;
        // Freeze current panel (do not mess up while refreshing)
        this.Enabled = false;
        // Progress setup
        _progressDialog = new DlgProgress();
        _progressDialog.SetHeading("Refreshing fanart");
        _progressDialog.TopMost = true;
        _progressDialog.DisableCancel();
        _progressDialog.SetLine1("Downloading fanart for:");
        _progressDialog.SetLine2("Downloading...");
        _progressDialog.SetPercentage(100);
        _progressDialog.Total = cbTitle.Items.Count - 1;
        _progressDialog.Count = 1;
        _progressDialog.Show();
        // Set bacgroundworker
        BackgroundWorker bgwFanart = new BackgroundWorker();
        bgwFanart.WorkerSupportsCancellation = true;
        bgwFanart.WorkerReportsProgress = false;
        bgwFanart.DoWork += new DoWorkEventHandler(RefreshFanart);
        bgwFanart.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CancelWorker);
        // Start worker by passing parameter moviecollection
        bgwFanart.RunWorkerAsync();

        while (_isRefreshing)
        {
          if (!_progressDialog.CancelScan)
          {
            Application.DoEvents();
          }
          else
          {
            _isRefreshing = false;
            bgwFanart.CancelAsync();
            ;
            return;
          }
        }
      }
      else
      {
        _isRefreshing = false;
        MessageBox.Show("Fanart option is disabled. To enable it check \"Use Fanart\" in Settings tab");
      }
    }

    // Refresh fanarts DoWork event handler
    private void RefreshFanart(object sender, DoWorkEventArgs e)
    {
      ArrayList movies = new ArrayList();
      ArrayList movieFiles = new ArrayList();

      VideoDatabase.GetMovies(ref movies);

      foreach (IMDBMovie movie in movies)
      {
        if (!_isRefreshing)
        {
          e.Cancel = true;
          break;
        }

        _progressDialog.SetLine1("Downloading fanart for: " + movie.Title);

        // Skip no IMDBid movie (better than fetch wrong art)
        if (CheckImdbId(movie.IMDBNumber) == false)
        {
          if (_progressDialog.Count < movies.Count - 1)
            _progressDialog.Count++;
          continue;
        }
        VideoDatabase.GetFiles(movie.ID, ref movieFiles);
        string strFile = string.Empty;
        string strPath = string.Empty;

        DatabaseUtility.Split((string)movieFiles[0], out strPath, out strFile);

        // Clean old fanarts
        FanArt.DeleteFanarts((string)movieFiles[0], movie.Title);

        if (strFile != string.Empty & strPath != string.Empty)
        {
          // Find fanart
          FanArt fanartSearch = new FanArt();
          fanartSearch.GetTmdbFanartByApi
            (strPath, strFile, movie.IMDBNumber, movie.Title, true, (int)fanartQ.Value, chbFanartShare.Checked, string.Empty);

          // Update fanart URL in vdb
          VideoDatabase.SetFanartURL(movie.ID, fanartSearch.DefaultFanartUrl);
        }
        if (_progressDialog.Count < movies.Count - 1)
          _progressDialog.Count++;
      }
      _isRefreshing = false;
    }

    // Browse Fanarts button (local picture)
    private void btnBrofseFA_Click(object sender, EventArgs e)
    {
      // Proceed only if fanart checkbox is enabled
      if (useFanartCheckBox.CheckState == CheckState.Checked)
      {
        OpenFileDialog dlg = new OpenFileDialog();
        dlg.AddExtension = true;
        dlg.Filter = "JPEG Image (*.jpg,*.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*";
        dlg.RestoreDirectory = false;
        // start in current folder
        dlg.InitialDirectory = ".";

        // open dialog
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
          tbFanartLocation.Text = dlg.FileName;
          // Save to database
          string fanartFile = "file://" + dlg.FileName;
          if (_fanartImgIndex == 0)
            VideoDatabase.SetFanartURL(CurrentMovie.ID, fanartFile);

          // Copy selected picture to fanart directory
          string strFile = string.Empty;
          string strPath = string.Empty;
          DatabaseUtility.Split(listViewFiles.Items[0].Text, out strPath, out strFile);

          if (strFile != string.Empty & strPath != string.Empty)
          {
            FanArt fanartSearch = new FanArt();
            fanartSearch.GetLocalFanart
              (strPath, strFile, CurrentMovie.Title, fanartFile, _fanartImgIndex, chbFanartShare.Checked);
            fanartListBox.Items.Clear();
            // Clear previous image in fanart picturebox
            if (pictureBoxFanArt.Image != null)
            {
              pictureBoxFanArt.Image.Dispose();
              pictureBoxFanArt.Image = null;
            }
            pictureBoxFanArt.ImageLocation = dlg.FileName;
            // Refresh movie
            _clearListBox = false;
            RefreshMovie(CurrentMovie.ID, cbTitle.SelectedIndex);
            _clearListBox = true;
          }
        }
      }
      else
      {
        MessageBox.Show("Fanart option is disabled. To enable it check \"Use Fanart\" in Settings tab");
      }
    }

    // Set Fanart file current index text
    private void SetFanartFileIndexLabel(int index)
    {
      labelFanartImageIndex.Text = String.Format("Image {0} of {1}", index + 1, (int)fanartQ.Value);
    }

    // Next fanart file image
    private void btFanartNext_Click(object sender, EventArgs e)
    {
      if (_fanartImgIndex < (int)fanartQ.Value - 1)
      {
        _fanartImgIndex++;
        SetFanartFileIndexLabel(_fanartImgIndex);
        ShowFanartPicture(_fanartImgIndex);
      }
    }

    // previous fanart file image
    private void btFanartPrevious_Click(object sender, EventArgs e)
    {
      if (_fanartImgIndex > 0)
      {
        _fanartImgIndex--;
        SetFanartFileIndexLabel(_fanartImgIndex);
        ShowFanartPicture(_fanartImgIndex);
      }
    }

    // Show fanart picture from file by index
    private void ShowFanartPicture(int index)
    {
      // Clear previous image
      if (pictureBoxFanArt.Image != null)
      {
        pictureBoxFanArt.Image.Dispose();
        pictureBoxFanArt.Image = null;
      }
      string strFile = string.Empty;
      FanArt.GetFanArtfilename(CurrentMovie.Title, index, out strFile);
      pictureBoxFanArt.ImageLocation = strFile;
      tbFanartLocation.Text = strFile;
    }

    #endregion

    #region Settings

    // Skip existing checkbox
    private void skipCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (skipCheckBox.CheckState == CheckState.Checked)
      {
        refreshdbCheckBox.CheckState = CheckState.Unchecked;
      }
    }

    // Folder as movie title checkbox
    private void useFoldername_CheckedChanged(object sender, EventArgs e)
    {
      _useFolderAsTitle = ((CheckBox)sender).Checked;
      if (_useFolderAsTitle)
      {
        preferFileNameCheckBox.Visible = true;
      }
      else
      {
        preferFileNameCheckBox.Visible = false;
        preferFileNameCheckBox.Checked = false;
      }
      SaveSettings();
    }

    // Prefer filename rather than folder name
    private void preferFileNameCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      SaveSettings();
    }

    // Refresh movie by IMDB ID checkbox
    private void cbRefreshByTT_CheckedChanged(object sender, EventArgs e)
    {
      if (cbRefreshByTT.CheckState == CheckState.Checked)
      {
        _refreshByImdBid = true;
        tbIMDBNr.Enabled = true;
      }
      else
      {
        _refreshByImdBid = false;
        tbIMDBNr.Enabled = false;
      }
    }

    ///
    // Refresh existing movies checkbox - only one setting is valid (Skip files in database or Refresh existing files
    // so first exclude second and vice versa
    private void refreshdbCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (refreshdbCheckBox.CheckState == CheckState.Checked)
      {
        skipCheckBox.CheckState = CheckState.Unchecked;
      }
      else
      {
        skipCheckBox.CheckState = CheckState.Checked;
      }
    }

    ///
    // FanArt CheckBox
    private void useFanart_CheckedChanged(object sender, EventArgs e)
    {
      string configDir;
      FanArt.GetFanArtFolder(out configDir);
      if (Directory.Exists(configDir))
      {
        _useFanArt = ((CheckBox)sender).Checked;
        SaveSettings();
        if (_useFanArt)
        {
          fanartQ.Enabled = true;
          chbFanartShare.Enabled = true;
        }
        else
        {
          fanartQ.Enabled = false;
          chbFanartShare.Enabled = false;
        }
      }
      else
      {
        if (useFanartCheckBox.CheckState == CheckState.Checked)
        {
          MessageBox.Show("Fanart plugin is not installed or configured properly.", "Error!");
          useFanartCheckBox.CheckState = CheckState.Unchecked;
          _useFanArt = false;
          fanartQ.Enabled = false;
          chbFanartShare.Enabled = false;
          SaveSettings();
        }
      }
    }

    ///
    // Fanart quantity downloads number changed
    private void fanartQ_ValueChanged(object sender, EventArgs e)
    {
      SaveSettings();
    }

    ///
    // Save Strip movie title prefix setting
    private void cbStripTitlePrefix_CheckedChanged(object sender, EventArgs e)
    {
      using (Settings xmlwriter = new MPSettings())
      {
        // Strip movie title prefix
        xmlwriter.SetValueAsBool("moviedatabase", "striptitleprefixes", checkBoxStripTitlePrefix.Checked);
        xmlwriter.SetValue("moviedatabase", "titleprefixes", tbTitlePrefixes.Text);
      }
    }

    #endregion

    #region Actors

    // ComboBox actor index change action
    private void cbActor_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        // Clear previous infos
        tbBirthDate.Text = string.Empty;
        tbBirthPlace.Text = string.Empty;
        tbMiniBiography.Text = string.Empty;
        tbBiography.Text = string.Empty;
        tbThumbLoc.Text = string.Empty;
        // Also picture if exists
        if (pictureBoxActor.Image != null)
        {
          pictureBoxActor.Image.Dispose();
        }
        pictureBoxActor.Image = null;
        // Check if actor is selected
        if (cbActor.Items.Count > 0 && cbActor.SelectedIndex > -1)
        {
          int actorID = int.Parse((string)cbActor.SelectedValue);
          IMDBActor actdetail = new IMDBActor();
          actdetail = VideoDatabase.GetActorInfo(actorID);

          // Populate new infos for selected actor
          tbBirthDate.Text = actdetail.DateOfBirth;
          tbBirthPlace.Text = actdetail.PlaceOfBirth;
          tbMiniBiography.Text = actdetail.MiniBiography;
          tbBiography.Text = actdetail.Biography;
          tbThumbLoc.Text = actdetail.ThumbnailUrl;
          pictureBoxActor.ImageLocation = actdetail.ThumbnailUrl;
        }
      }
      catch (Exception) {}
    }

    // Save actor detail changes
    private void btnSaveActorInfo_Click(object sender, EventArgs e)
    {
      try
      {
        if (Int32.Parse(cbActor.SelectedValue.ToString()) > 1)
        {
          IMDBActor imdbActor = new IMDBActor();
          imdbActor.id = Int32.Parse(cbActor.SelectedValue.ToString());
          imdbActor.Name = cbActor.Text.Replace(" - Director", ""); // Remove director suffix from the name
          imdbActor.MiniBiography = tbMiniBiography.Text;
          imdbActor.Biography = tbBiography.Text;
          imdbActor.DateOfBirth = tbBirthDate.Text;
          imdbActor.PlaceOfBirth = tbBirthPlace.Text;

          bool isUrl = (tbThumbLoc.Text.Substring(0, 7) == @"http://");
          if (isUrl)
          {
            imdbActor.ThumbnailUrl = tbThumbLoc.Text;
          }
          else
          {
            imdbActor.ThumbnailUrl = string.Empty;
            tbThumbLoc.Text = string.Empty;
          }
          // Update actor info
          VideoDatabase.SetActorInfo(imdbActor.id, imdbActor);
          // Update actor thumb
          if (imdbActor.ThumbnailUrl != string.Empty)
          {
            string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, imdbActor.Name);
            string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieActors, imdbActor.Name);
            // Delete old thumbs
            Util.Utils.FileDelete(largeCoverArt);
            Util.Utils.FileDelete(coverArt);
            // Save new thumbs
            IMDBFetcher.DownloadCoverArt(Thumbs.MovieActors, imdbActor.ThumbnailUrl, imdbActor.Name);
            // Clear old image from picture box
            if (pictureBoxActor.Image != null)
            {
              pictureBoxActor.Image.Dispose();
              pictureBoxActor.Image = null;
            }
            // Set new image into pbox
            pictureBoxActor.ImageLocation = imdbActor.ThumbnailUrl;
          }
          MessageBox.Show("Actor info saved.");
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Save failed.");
      }
    }

    // Refresh Movie actors info
    private void btnRefreshActors_Click(object sender, EventArgs e)
    {
      _progressDialog.Total = 1;
      _progressDialog.Count = 1;
      // Start fetch
      if (IMDBFetcher.FetchMovieActors(this, CurrentMovie))
      {
        ActorsTableRefresh(CurrentMovie.ID);
        cbActor.SelectedIndex = 0;
      }
      else
      {
        cbActor.SelectedIndex = -1;
      }
    }

    // Actors table 
    private void ActorsTableRefresh(int movieID)
    {
      // Find Actor

      // DirectorID - just to show who is movie director in the list
      IMDBMovie findDirector = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(movieID, ref findDirector);
      string directorID = findDirector.DirectorID.ToString();

      // Actor datatable
      System.Data.DataTable actTable = new System.Data.DataTable();
      actTable.Columns.Add("ID", typeof (string));
      actTable.Columns.Add("Name", typeof (string));

      ArrayList actorsByMovie = new ArrayList();
      VideoDatabase.GetActorsByMovieID(movieID, ref actorsByMovie);
      if (actorsByMovie.Count > 0)
      {
        char[] splitter = {'|'};
        // Populate datatable from array
        foreach (string actorByMovie in actorsByMovie)
        {
          string[] actors = actorByMovie.Split(splitter);
          if (actors[0] == directorID)
            actors[1] = actors[1] + " - Director";
          actTable.Rows.Add(actors[0], actors[1]);
        }
      }
      // Clear old data
      cbActor.DataSource = null;
      cbActor.Items.Clear();
      cbActorMovies.DataSource = null;
      cbActorMovies.Items.Clear();
      // Data bind
      actTable.DefaultView.Sort = actTable.Columns["Name"].ColumnName + " asc"; // Sort by name, ascending
      cbActor.DisplayMember = "Name";
      cbActorMovies.DisplayMember = "Name";
      cbActor.ValueMember = "ID";
      cbActorMovies.ValueMember = "ID";
      cbActor.DataSource = actTable;
      cbActorMovies.DataSource = actTable;
    }

    // Populate actor info controls
    private void PopulateActorInfo()
    {
      try
      {
        if (cbActor.Items.Count > 0)
        {
          cbActor.SelectedIndex = 0;
          string value = cbActor.SelectedValue.ToString();
          IMDBActor actdetail = new IMDBActor();
          actdetail = VideoDatabase.GetActorInfo(int.Parse(value));

          if (actdetail != null)
          {
            tbBirthDate.Text = actdetail.DateOfBirth;
            tbBirthPlace.Text = actdetail.PlaceOfBirth;
            tbMiniBiography.Text = actdetail.MiniBiography;
            tbBiography.Text = actdetail.Biography;
            tbThumbLoc.Text = actdetail.ThumbnailUrl;
            pictureBoxActor.ImageLocation = actdetail.ThumbnailUrl;
          }
        }
        else
        {
          tbBirthDate.Text = string.Empty;
          tbBirthPlace.Text = string.Empty;
          tbMiniBiography.Text = string.Empty;
          tbBiography.Text = string.Empty;
          tbThumbLoc.Text = string.Empty;
          if (pictureBoxActor.Image != null)
          {
            pictureBoxActor.Image.Dispose();
          }
          pictureBoxActor.Image = null;
        }
      }
      catch (Exception) {}
    }

    // Go to actor IMDB page
    private void linklabelActor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        string value = cbActor.SelectedValue.ToString();
        IMDBActor actdetail = new IMDBActor();
        actdetail = VideoDatabase.GetActorInfo(int.Parse(value));
        string url = "http://www.imdb.com/name/" + actdetail.IMDBActorID;

        Process.Start(url);
      }
      catch (Exception) {}
    }

    // Actor movies info index changed by actor
    private void cbActorMovies_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Clear datagrid
      dgActorMovies.Rows.Clear();

      if (cbActorMovies.Items.Count > 0 & cbActorMovies.SelectedIndex > -1)
      {
        string value = cbActorMovies.SelectedValue.ToString();
        IMDBActor actdetail = new IMDBActor();
        actdetail = VideoDatabase.GetActorInfo(int.Parse(value));
        // If no detail, finish
        if (actdetail == null)
        {
          return;
        }
        // Populate datagrid
        for (int i = 0; i < actdetail.Count; i++)
        {
          dgActorMovies.Rows.Add();
          dgActorMovies.Rows[i].Cells[0].Value = actdetail[i].Year.ToString();
          dgActorMovies.Rows[i].Cells[1].Value = actdetail[i].MovieTitle;
          dgActorMovies.Rows[i].Cells[1].ToolTipText = "http://www.imdb.com/title/" + actdetail[i].imdbID;
          dgActorMovies.Rows[i].Cells[2].Value = actdetail[i].Role;
          dgActorMovies.Rows[i].Cells[3].Value = actdetail[i].imdbID;

          // Mark movie in our collection
          // This can maybe slow down initialization with huge movie list (leave as test)
          // Changed method 08.08.2010, reported it works fast with 500+ movies in collection
          ArrayList movies = new ArrayList();
          string sql = "SELECT * FROM movieinfo WHERE IMDBID = '" + actdetail[i].imdbID + "'";
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false);

          if (movies.Count > 0)
          {
            dgActorMovies.Rows[i].DefaultCellStyle.ForeColor = Color.Blue;
            dgActorMovies.Rows[i].DefaultCellStyle.Font = new Font(dgActorMovies.Font, FontStyle.Bold);
          }
        }
      }
    }

    // DataGrid movie click for IMDB page
    private void dgActorMovies_CellClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == 1 & e.RowIndex > -1)
      {
        try
        {
          string url = dgActorMovies.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText;
          Process.Start(url);
        }
        catch (Exception) {}
        finally {}
      }
    }

    // Actor movie link click
    private void linkActorMovie_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        string value = cbActorMovies.SelectedValue.ToString();
        IMDBActor actdetail = new IMDBActor();
        actdetail = VideoDatabase.GetActorInfo(int.Parse(value));
        string url = "http://www.imdb.com/name/" + actdetail.IMDBActorID;

        Process.Start(url);
      }
      catch (Exception) {}
      finally {}
    }

    // Refresh actors info for all movies
    private void buttonRefreshAllActors_Click(object sender, EventArgs e)
    {
      int movieCollection = cbTitle.Items.Count - 1;
      int cbIndex = cbTitle.SelectedIndex;
      _progressDialog.Total = movieCollection;
      _progressDialog.Count = 1;

      for (int i = 0; i < movieCollection; ++i)
      {
        cbTitle.SelectedIndex = i;
        ComboBoxItemMovie item = (ComboBoxItemMovie)cbTitle.SelectedItem;
        CurrentMovie.ID = item.Movie.ID;
        // Fetch actors
        if (IMDBFetcher.FetchMovieActors(this, CurrentMovie))
        {
          ActorsTableRefresh(CurrentMovie.ID);
        }
        if (_progressDialog.CancelScan)
        {
          break;
        }
        if (_progressDialog.Count < movieCollection)
          _progressDialog.Count++;
      }
      cbTitle.SelectedIndex = cbIndex;
    }

    #endregion

    #region Tools

    // Clear actors trash from the database (ie. unknown, nmxxxxx)
    private void btnClearActorsTrash_Click(object sender, EventArgs e)
    {
      ArrayList listActors = new ArrayList();
      VideoDatabase.GetActors(listActors);

      int actorsCount = listActors.Count;
      int deleted = 0;

      // Pbar initialization
      pbTools.Maximum = actorsCount;
      pbTools.Minimum = 0;
      pbTools.Value = 0;

      foreach (string actor in listActors)
      {
        if (actor == "unknown" | actor.StartsWith("nm"))
        {
          VideoDatabase.DeleteActor(actor);
          deleted++;
        }
        //
        // Progressbar advance
        //
        ProgressBarAdvance(ref pbTools, string.Empty, false);
      }
      pbTools.Value = 0;
      // Refresh actors list view
      listViewAllActors.Items.Clear();
      listViewAllActors.BeginUpdate();
      listActors = new ArrayList();
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
        {
          listViewAllActors.Items.Add(actor);
        }
      }
      listViewAllActors.EndUpdate();
      // Actor details
      ActorsTableRefresh(Int32.Parse(tbMovieID.Text));
      PopulateActorInfo();
      MessageBox.Show(deleted + " removed.", "", MessageBoxButtons.OK);
    }

    // Upgrade covers
    private void btnUpgradeCovers_Click(object sender, EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure?",
                                                  "Cover files Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
      if (dialogResult == DialogResult.No)
      {
        return;
      }

      ArrayList movies = new ArrayList();
      VideoDatabase.GetMovies(ref movies);

      // Progress bar initialization
      pbTools.Maximum = movies.Count;
      pbTools.Minimum = 0;
      pbTools.Value = 0;

      Cursor = Cursors.WaitCursor;

      foreach (IMDBMovie movie in movies)
      {
        string title = movie.Title;
        int id = movie.ID;
        //
        // Old file names
        //
        string strThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, title);
        string largeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, title);
        //
        // New filenames
        //
        title = Util.Utils.MakeFileName(title);
        string strThumbNew = Path.GetDirectoryName(strThumb) + "\\" + title + "{" + id + "}.jpg";
        string strLargeThumbNew = Path.GetDirectoryName(largeThumb) + "\\" + title + "{" + id + "}L.jpg";
        //
        // Copy new files and delete old ones
        //
        if (File.Exists(strThumb))
        {
          File.Copy(strThumb, strThumbNew, true);
          Util.Utils.FileDelete(strThumb);
        }
        if (File.Exists(largeThumb))
        {
          File.Copy(largeThumb, strLargeThumbNew, true);
          Util.Utils.FileDelete(largeThumb);
        }
        //
        // Progressbar advance
        //
        ProgressBarAdvance(ref pbTools, string.Empty, false);
      }
      pbTools.Value = 0;
      cbTitle.SelectedIndex = 0;
      btnUpgradeCovers.Enabled = false;
      btnDowngradeCovers.Enabled = true;
      //
      // Save upgrade status
      //
      _coversUpgraded = true;
      SaveSettings();

      Cursor = Cursors.Default;
    }

    // Downgrade covers
    private void btnDowngradeCovers_Click(object sender, EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure?",
                                                  "Cover files Downgrade", MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);
      if (dialogResult == DialogResult.No)
      {
        return;
      }

      ArrayList movies = new ArrayList();
      VideoDatabase.GetMovies(ref movies);

      // Progress bar initialization
      pbTools.Maximum = movies.Count;
      pbTools.Minimum = 0;
      pbTools.Value = 0;

      Cursor = Cursors.WaitCursor;

      foreach (IMDBMovie movie in movies)
      {
        int id = movie.ID;
        string title = movie.Title + "{" + id + "}";
        //
        //Old file names
        //
        string strThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, title);
        string largeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, title);
        //
        // New filenames
        //
        title = Util.Utils.MakeFileName(movie.Title);
        string strThumbNew = Path.GetDirectoryName(strThumb) + "\\" + title + ".jpg";
        string strLargeThumbNew = Path.GetDirectoryName(largeThumb) + "\\" + title + "L.jpg";
        //
        // Copy new files and delete old ones
        //
        if (File.Exists(strThumb))
        {
          File.Copy(strThumb, strThumbNew, true);
          Util.Utils.FileDelete(strThumb);
        }
        if (File.Exists(largeThumb))
        {
          File.Copy(largeThumb, strLargeThumbNew, true);
          Util.Utils.FileDelete(largeThumb);
        }
        //
        // Progressbar advance
        //
        ProgressBarAdvance(ref pbTools, string.Empty, false);
      }
      pbTools.Value = 0;
      cbTitle.SelectedIndex = 0;
      btnUpgradeCovers.Enabled = true;
      btnDowngradeCovers.Enabled = false;
      //
      // Save upgrade status
      //
      _coversUpgraded = false;
      SaveSettings();

      Cursor = Cursors.Default;
    }
    

    #endregion

    #region Other

    // IMDB film page link click if somebody wants to know more about movie
    private void linkLabelIMDBNumber_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (tbIMDBNr.Text != string.Empty)
      {
        string url = "http://www.imdb.com/title/" + tbIMDBNr.Text;
        Process.Start(url);
      }
    }

    // Edit tab index change
    private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
    {
      ShowHide();
    }

    // Progress bar advance
    private void ProgressBarAdvance(ref UserInterface.Controls.MPProgressBar progressBar, string text, bool refreshOnly)
    {
      if (!refreshOnly)
      {
        progressBar.Value++;
      }
      progressBar.Refresh();
      //
      //Draw percentage into progressbar
      //
      int percent = (int)((progressBar.Value / (double)progressBar.Maximum) * 100);
      progressBar.CreateGraphics().DrawString(text + percent + "%",
                                              new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black,
                                              new PointF(progressBar.Width / 2 - 10, progressBar.Height / 2 - 7));
    }

    // Progress bar draw percentage
    private void ProgressBarDrawPercentage(ref UserInterface.Controls.MPProgressBar progressBar, int percent)
    {
      progressBar.CreateGraphics().DrawString(percent + "%",
                                              new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black,
                                              new PointF(progressBar.Width / 2 - 10, progressBar.Height / 2 - 7));
    }

    // Stop/Complete worker event handler
    private void CancelWorker(object sender, RunWorkerCompletedEventArgs e)
    {
      BackgroundWorker bgw = (BackgroundWorker)sender;
      _progressDialog.CloseProgress();
      // Show message
      if (e.Cancelled)
      {
        MessageBox.Show("Refreshing canceled !");
      }
      else if (e.Error != null)
      {
        MessageBox.Show("Error: " + e.Error.Message);
      }
      else
      {
        MessageBox.Show("Done!");
      }
      bgw.Dispose();
      // Refresh all movies
      LoadMovies(0);
      cbTitle.SelectedIndex = 0;
      this.Enabled = true;
    }

    // Show-hide some of the controls
    private void ShowHide()
    {
      if (CheckImdbId(tbIMDBNr.Text))
      {
        labelFASearchString.Visible = false;
        tbFASearchString.Visible = false;
        labCoverSearchStr.Visible = false;
        tbCoverSearchStr.Visible = false;
        chbIMDBCoverSource.Enabled = true;
        chbImpAwCoverSource.Enabled = true;
      }
      else
      {
        labelFASearchString.Visible = true;
        tbFASearchString.Visible = true;
        labCoverSearchStr.Visible = true;
        tbCoverSearchStr.Visible = true;
        chbIMDBCoverSource.Enabled = false;
        chbIMDBCoverSource.Checked = false;
        chbImpAwCoverSource.Enabled = false;
        chbImpAwCoverSource.Checked = false;
      }
    }

    // Check if IMDBtt number is valid
    private bool CheckImdbId(string id)
    {
      // IMDBtt search check tt number, must be exactly 9 chars with leading zeros if needed
      if (id.Length < 9 | id.Length > 9)
      {
        return false;
      }
      // Final IMDBtt check
      Match ttNo = Regex.Match(id, @"tt[\d]{7}?");
      if (!ttNo.Success)
      {
        return false;
      }
      return true;
    }

    // Movie refresh (only selected)
    private void RefreshMovie(int movieIndex, int cbSlectedIndex)
    {
      IMDBMovie movie = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(movieIndex, ref movie);
      ComboBoxItemMovie movieItem = new ComboBoxItemMovie(movie.Title, movie);
      cbTitle.Items[cbSlectedIndex] = movieItem;
    }

    // New movie button action
    private void btnNew_Click(object sender, EventArgs e)
    {
      int count = cbTitle.Items.Count - 1;
      cbTitle.SelectedIndex = count;
      tabControl2.SelectedIndex = 2;
      AddFile();
    }

    #endregion

    #endregion
  }
}