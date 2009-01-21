#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
        if (x.language.Equals(y.language))
        {
          return x.title.CompareTo(y.title);
        }
        else
        {
          return x.language.CompareTo(y.language);
        }
      }

      #endregion
    }

    internal class ComboBoxItemDatabase
    {
      public string database;
      public string title;
      public string language;
      public string limit;

      public ComboBoxItemDatabase()
      {
      }

      public override string ToString()
      {
        return String.Format("{0}: {1} [{2}]", language, title, database);
      }
    }

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

    #endregion

    // grabber index holds information/urls of available grabbers to download
    private string GrabberIndexFile = Config.GetFile(Config.Dir.Config, "MovieInfoGrabber.xml");
    private const string GrabberIndexURL = @"http://install.team-mediaportal.com/MP1/MovieInfoGrabber.xml";

    /// <summary>
    /// Dictionary contains all grabber scripts.
    /// The Key is used for the filename, where the grabber is from.
    /// Will be refreshed on start and after online update.
    /// </summary>
    private Dictionary<string, IIMDBScriptGrabber> grabberList;

    // The LVI being edited
    private ListViewItem _editItem;

    private bool _scanning = false;
    private bool useLocalImage = false;

    private DlgProgress progressDialog = new DlgProgress();
    private string newMovieToFind = string.Empty;

    private bool _isFuzzyMatching = true;
    //ArrayList extractedTags;

    private bool settingsLoaded = false;

    private ArrayList conflictFiles = new ArrayList();

    #region ctor

    public MovieDatabase()
      : this("Video Database")
    {
    }

    public MovieDatabase(string name)
      : base("Video Database")
    {
      InitializeComponent();

      this.linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://forum.team-mediaportal.com/movie-info-grabbers-287/");
    }

    #endregion

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }

    private string[] extensions = new string[] {".avi"};

    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    public override void OnSectionActivated()
    {
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
        ArrayList shares = (ArrayList) section.GetSetting("shares");

        foreach (string share in shares)
        {
          //
          // Add to share to list box and default to selected
          //
          sharesListBox.Items.Add(share, CheckState.Checked);
        }
      }

      // Movie Folders
      // Fetch extensions
      //
      section = GetSection("Video Extensions");

      if (section != null)
      {
        string extensions = (string) section.GetSetting("extensions");
        Extensions = extensions.Split(new char[] {','});
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
        string path = sharesListBox.Items[(int) sharesListBox.CheckedIndices[index]].ToString();
        availablePaths.Add(path);
      }
      conflictFiles = new ArrayList();
      IMDBFetcher.ScanIMDB(this, availablePaths, _isFuzzyMatching, skipCheckBox.Checked, actorsCheckBox.Checked);
    }

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
        MessageBox.Show("Video database has been cleared", "Video Database", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
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
      {
        progressDialog.SetPercentage(percent);
      }
      progressDialog.Update();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      progressDialog.ResetProgress();
      progressDialog.SetHeading("Searching IMDB...");
      progressDialog.SetLine1(fetcher.MovieName);
      progressDialog.SetLine2(string.Empty);
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
        MessageBox.Show("Movie details could not be found.", fetcher.MovieName, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
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
        {
          dlg.AddMovie(fetcher[i].Title);
        }
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
          IMDBMovie currentMovie = (IMDBMovie) conflictFiles[i];
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

    #endregion

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
      cbTitle.SelectedIndex = index;
    }

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
      tbDescription.Text = movie.Plot;

      if (movie.ThumbURL.Length > 7 && movie.ThumbURL.Substring(0, 7).Equals("file://"))
      {
        useLocalImage = true;
        tbImageLocation.Text = movie.ThumbURL.Substring(7);
      }
      else
      {
        useLocalImage = false;
        tbImageLocation.Text = movie.ThumbURL;
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

      if (pictureBox1.Image != null)
      {
        pictureBox1.Image.Dispose();
        pictureBox1.Image = null;
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
      imagesListBox.Items.Clear();
      imagesListBox.Enabled = false;

      if (movie.ID >= 0)
      {
        string file = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, movie.Title);

        if (File.Exists(file))
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
            pictureBox1.Image = result;
          }
        }

        char[] splitter = {'\n', ','};
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
        ArrayList vecGenres = new ArrayList();
        if (szGenres.IndexOf("/") >= 0)
        {
          Tokens f = new Tokens(szGenres, new char[] {'/'});
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
      if (tabControl1.SelectedTab == tabPage1)
      {
        LoadMovies(0);
      }
    }

    #region Editor tab

    private void buttonMapGenre_Click(object sender, EventArgs e)
    {
      if (listViewAllGenres.SelectedItems == null)
      {
        return;
      }

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
      if (listViewAllGenres.SelectedItems == null)
      {
        return;
      }
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
      if (listViewAllActors.SelectedItems == null)
      {
        return;
      }

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
      if (listViewMovieActors.SelectedItems == null)
      {
        return;
      }
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

    private void buttonAddFile_Click(object sender, EventArgs e)
    {
      OpenFileDialog find_file = new OpenFileDialog();
      //find_file.RestoreDirectory = true;
      find_file.DefaultExt = "avi";
      find_file.Filter =
        "Avi Files|*.avi|Recordings|*.dvr-ms|Mpeg files|*.mpeg|Mpeg files|*.mpg|Windows Media|*.wmv|All files|*.*";
      find_file.InitialDirectory = ".";
      find_file.Title = "Find files for " + tbTitle.Text;
      find_file.Multiselect = true;

      if (find_file.ShowDialog(this) == DialogResult.OK)
      {
        foreach (String file in find_file.FileNames)
        {
          listViewFiles.Items.Add(file);
        }
      }
    }

    private void buttonRemoveFile_Click(object sender, EventArgs e)
    {
      if (listViewFiles.SelectedItems == null)
      {
        return;
      }
      for (int i = listViewFiles.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listViewFiles.SelectedItems[i];
        listViewFiles.Items.Remove(listItem);
      }
    }

    private void buttonDeleteActor_Click(object sender, EventArgs e)
    {
      if (listViewAllActors.SelectedItems == null)
      {
        return;
      }
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
    }

    private void btnDeleteGenre_Click(object sender, EventArgs e)
    {
      if (listViewAllGenres.SelectedItems == null)
      {
        return;
      }
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

    private void buttonNewMovie_Click(object sender, EventArgs e)
    {
      cbTitle.SelectedItem = null;
      IMDBMovie details = new IMDBMovie();
      UpdateEdit(details);
    }

    private void buttonLookupMovie_Click(object sender, EventArgs e)
    {
      if (tbTitle.Text == string.Empty)
      {
        MessageBox.Show("Please enter a movie title", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        ;
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
      Util.Utils.Split(file, out path, out filename);
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
      if (IMDBFetcher.RefreshIMDB(this, ref movieDetails, fuzzyMatch, false))
      {
        if (movieDetails != null)
        {
          LoadMovies(movieDetails.ID);
        }
      }
    }

    private void btnSave_Click(object sender, EventArgs e)
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

      VideoDatabase.SetMovieInfoById(details.ID, ref details);
      //add files to movie
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

      LoadMovies(details.ID);
    }

    private void btnAmazon_Click(object sender, EventArgs e)
    {
      btnAmazon.Enabled = false;
      imagesListBox.Items.Clear();
      imagesListBox.Enabled = false;

      string strFilename = string.Empty;
      string strPath = string.Empty;
      Util.Utils.Split(listViewFiles.Items[0].Text, out strPath, out strFilename);

      DirectoryInfo di = new DirectoryInfo(strPath);
      FileInfo[] jpgFiles = di.GetFiles("*.jpg");

      int count = 1;

      foreach (FileInfo file in jpgFiles)
      {
        ComboBoxArt art = new ComboBoxArt(String.Format("Local Picture {0}", count), file.FullName);
        imagesListBox.Items.Add(art);
        ++count;
      }

      jpgFiles = di.GetFiles("*.jpeg");

      foreach (FileInfo file in jpgFiles)
      {
        ComboBoxArt art = new ComboBoxArt(String.Format("Local Picture {0}", count), file.FullName);
        imagesListBox.Items.Add(art);
        ++count;
      }

      IMPawardsSearch impSearch = new IMPawardsSearch();
      impSearch.Search(CurrentMovie.Title);

      if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
      {
        for (int i = 0; i < impSearch.Count; ++i)
        {
          ComboBoxArt art = new ComboBoxArt(String.Format("IMP Awards Picture {0}", (i + 1)), impSearch[i]);
          imagesListBox.Items.Add(art);
        }
      }

      AmazonImageSearch amazonSearch = new AmazonImageSearch();
      amazonSearch.Search(CurrentMovie.Title);

      if (amazonSearch.Count > 0)
      {
        for (int i = 0; i < amazonSearch.Count; ++i)
        {
          ComboBoxArt art = new ComboBoxArt(String.Format("Amazon Picture {0}", (i + 1)), amazonSearch[i]);
          imagesListBox.Items.Add(art);
        }
      }

      if (imagesListBox.Items.Count == 0)
      {
        imagesListBox.Items.Clear();
        imagesListBox.Items.Add(new ComboBoxArt("No results found...", ""));
      }
      else
      {
        imagesListBox.Enabled = true;
      }

      imagesListBox.SelectedIndex = 0;
      btnAmazon.Enabled = true;
    }

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
        VideoDatabase.DeleteMovieInfoById(CurrentMovie.ID);
        string file = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, CurrentMovie.Title);
        if (File.Exists(file))
        {
          File.Delete(file);
        }
        file = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, CurrentMovie.Title);
        if (File.Exists(file))
        {
          File.Delete(file);
        }
        LoadMovies(0);
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
        if (nodeRating != null && nodeRating.InnerText != null)
        {
          movie.MPARating = nodeRating.InnerText;
        }
        else
        {
          movie.MPARating = "NR";
        }
        movie.Path = string.Empty;
        // Added check to validate overview and duration
        if (nodeOverview != null && nodeOverview.InnerText != null)
        {
          movie.Plot = nodeOverview.InnerText;
        }
        else
        {
          movie.Plot = string.Empty;
        }
        movie.PlotOutline = string.Empty;
        movie.Rating = 0;
        if (nodeDuration != null && nodeDuration.InnerText != null)
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
        if (nodeYear != null && nodeYear != null)
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

    private IMDBMovie CurrentMovie
    {
      get
      {
        IMDBMovie movie = new IMDBMovie();
        if (cbTitle.SelectedItem != null)
        {
          ComboBoxItemMovie cbMovie = (ComboBoxItemMovie) cbTitle.SelectedItem;
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
          movie.Plot = tbDescription.Text;
          movie.Rating = (float) Double.Parse(tbRating.Text);
          movie.TagLine = tbTagline.Text;
          movie.Year = Int32.Parse(tbYear.Text);
          movie.ThumbURL = (useLocalImage ? "file://" + tbImageLocation.Text : tbImageLocation.Text);
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
        return movie;
      }
    }

    #endregion

    private void OnFuzzyMatchingCheckedChanged(object sender, EventArgs e)
    {
      _isFuzzyMatching = ((CheckBox) sender).Checked;
      SaveSettings();
    }

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _isFuzzyMatching = xmlreader.GetValueAsBool("movies", "fuzzyMatching", true);

        _fuzzyMatchingCheckBox.Checked = _isFuzzyMatching;

        // Load activated databases
        skipCheckBox.Checked = xmlreader.GetValueAsBool("moviedatabase", "scanskipexisting", false);
        actorsCheckBox.Checked = xmlreader.GetValueAsBool("moviedatabase", "getactors", true);

        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
        if (iNumber > 0)
        {
          string strLimit = "";
          string strDatabase = "";
          string strLanguage = "";
          string strTitle = "";
          for (int i = 0; i < iNumber; i++)
          {
            strLimit = xmlreader.GetValueAsString("moviedatabase", "limit" + i.ToString(), "false");
            strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i.ToString(), "false");
            strLanguage = xmlreader.GetValueAsString("moviedatabase", "language" + i.ToString(), "false");
            strTitle = xmlreader.GetValueAsString("moviedatabase", "title" + i.ToString(), "false");

            if ((strLimit != "false") && (strDatabase != "false") && (strLanguage != "false") && (strTitle != "false"))
            {
              ListViewItem item = this.lvDatabase.Items.Add(strDatabase);
              item.SubItems.Add(strTitle);
              item.SubItems.Add(strLanguage);
              item.SubItems.Add(strLimit);
            }
          }
        }

        ReloadGrabberScripts();
      }

      settingsLoaded = true;
    }

    public override void SaveSettings()
    {
      if (!settingsLoaded)
      {
        return;
      }

      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("movies", "fuzzyMatching", _isFuzzyMatching);
        // Database
        xmlwriter.SetValueAsBool("moviedatabase", "scanskipexisting", skipCheckBox.Checked);
        xmlwriter.SetValueAsBool("moviedatabase", "getactors", actorsCheckBox.Checked);

        xmlwriter.SetValue("moviedatabase", "number", this.lvDatabase.Items.Count);
        for (int i = 0; i < this.lvDatabase.Items.Count; i++)
        {
          xmlwriter.SetValue("moviedatabase", "database" + i.ToString(),
                             this.lvDatabase.Items[i].SubItems[chDatabaseDB.Index].Text);
          xmlwriter.SetValue("moviedatabase", "title" + i.ToString(),
                             this.lvDatabase.Items[i].SubItems[chDatabaseTitle.Index].Text);
          xmlwriter.SetValue("moviedatabase", "language" + i.ToString(),
                             this.lvDatabase.Items[i].SubItems[chDatabaseLanguage.Index].Text);
          xmlwriter.SetValue("moviedatabase", "limit" + i.ToString(),
                             this.lvDatabase.Items[i].SubItems[chDatabaseLimit.Index].Text);
        }
        for (int i = this.lvDatabase.Items.Count; i < 4; i++)
        {
          xmlwriter.RemoveEntry("moviedatabase", "database" + i.ToString());
          xmlwriter.RemoveEntry("moviedatabase", "title" + i.ToString());
          xmlwriter.RemoveEntry("moviedatabase", "language" + i.ToString());
          xmlwriter.RemoveEntry("moviedatabase", "limit" + i.ToString());
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

      Rectangle lviBounds;
      int subItemX;

      Rectangle subItemRect = Rectangle.Empty;
      lviBounds = item.GetBounds(ItemBoundsPortion.Entire);

      subItemX = lviBounds.Left;
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

      ListViewItem item = this.lvDatabase.Items.Add(database.database);
      item.SubItems.Add(database.title);
      item.SubItems.Add(database.language);
      item.SubItems.Add(database.limit);

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

      progressDialog = new DlgProgress();
      progressDialog.SetHeading("Updating MovieInfo grabber scripts...");
      progressDialog.TopMost = true;

      // download index file
      progressDialog.SetLine1("Downloading the index file...");
      progressDialog.Total = 1;
      progressDialog.Count = 1;
      progressDialog.Show();
      if (DownloadFile(GrabberIndexFile, GrabberIndexURL) == false)
      {
        progressDialog.CloseProgress();
        return;
      }

      // read index file
      if (!File.Exists(GrabberIndexFile))
      {
        MessageBox.Show("No GrabberIndexFile found.");
        progressDialog.CloseProgress();
        return;
      }
      XmlDocument doc = new XmlDocument();
      doc.Load(GrabberIndexFile);
      XmlNodeList sectionNodes = doc.SelectNodes("MovieInfoGrabber/grabber");

      // download all grabbers
      progressDialog.Total = sectionNodes.Count;
      for (int i = 0; i < sectionNodes.Count; i++)
      {
        if (progressDialog.DialogResult == DialogResult.Cancel)
        {
          break;
        }

        string url = sectionNodes[i].Attributes["url"].Value;
        string id = Path.GetFileName(url);

        progressDialog.SetLine1("Downloading grabber: " + id);
        progressDialog.Count = i;

        if (DownloadFile(IMDB.ScriptDirectory + @"\" + id, url) == false)
        {
          progressDialog.CloseProgress();
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

      progressDialog.CloseProgress();

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
    /// <param name="AcceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
    public void EndEditing(bool AcceptChanges)
    {
      if (AcceptChanges && (_editItem != null))
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
      grabberList = new Dictionary<string, IIMDBScriptGrabber>();

      Directory.CreateDirectory(IMDB.ScriptDirectory);
      DirectoryInfo di = new DirectoryInfo(IMDB.ScriptDirectory);

      FileInfo[] fileList = di.GetFiles("*.csscript", SearchOption.AllDirectories);
      foreach (FileInfo f in fileList)
      {
        try
        {
          AsmHelper script = new AsmHelper(CSScript.Load(f.FullName, null, false));
          IIMDBScriptGrabber grabber = (IIMDBScriptGrabber) script.CreateObject("Grabber");

          grabberList.Add(Path.GetFileNameWithoutExtension(f.FullName), grabber);
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

      foreach (KeyValuePair<string, IIMDBScriptGrabber> grabber in grabberList)
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
          item.database = grabber.Key;
          item.language = grabber.Value.GetLanguage();
          item.limit = IMDB.DEFAULT_SEARCH_LIMIT.ToString();
          item.title = grabber.Value.GetName();

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
      string GrabberTempFile = Path.GetTempFileName();

      Application.DoEvents();
      try
      {
        if (File.Exists(GrabberTempFile))
        {
          File.Delete(GrabberTempFile);
        }

        Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
        Application.DoEvents();

        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
        {
          Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream, Encoding.Default))
            {
              using (TextWriter tout = File.CreateText(GrabberTempFile))
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
        File.Move(GrabberTempFile, filepath);
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

    private void btnBrowse_Click(object sender, EventArgs e)
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
        UpdateActiveMovieImageAndThumbs(tbImageLocation.Text);
      }
    }

    private void cbTitle_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cbTitle.SelectedItem != null)
      {
        ComboBoxItemMovie item = (ComboBoxItemMovie) cbTitle.SelectedItem;
        UpdateEdit(item.Movie);
      }
    }

    private void imagesListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBoxArt art = imagesListBox.SelectedItem as ComboBoxArt;
      if (art != null)
      {
        tbImageLocation.Text = art.URL;
      }

      UpdateActiveMovieImageAndThumbs(tbImageLocation.Text);
    }

    private int BinarySearch(ListView.ListViewItemCollection items, string item)
    {
      int left = 0;
      int right = items.Count - 1;
      int midPoint = 0;

      while (left <= right)
      {
        midPoint = (left + right)/2;
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

    private void UpdateActiveMovieImageAndThumbs(string strImageURL)
    {
      if (strImageURL == string.Empty)
      {
        return;
      }

      bool bIsURL = (strImageURL.Substring(0, 7) == @"http://");

      // Clear previous image
      if (pictureBox1.Image != null)
      {
        pictureBox1.Image.Dispose();
        pictureBox1.Image = null;
      }

      string strThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, tbTitle.Text);
      string LargeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, tbTitle.Text);

      // Delete old thumbs
      Util.Utils.FileDelete(strThumb);
      Util.Utils.FileDelete(LargeThumb);

      if (bIsURL)
      {
        IMDBFetcher.DownloadCoverArt(Thumbs.MovieTitle, strImageURL, tbTitle.Text);
      }
      else
      {
        if (!File.Exists(strImageURL))
        {
          return;
        }
      }

      // Create new thumbs
      try
      {
        if (Util.Picture.CreateThumbnail(strImageURL, strThumb, (int) Thumbs.ThumbResolution,
                                         (int) Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
        {
          Util.Picture.CreateThumbnail(strImageURL, LargeThumb, (int) Thumbs.ThumbLargeResolution,
                                       (int) Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
        }
      }
      catch (Exception)
      {
      }

      string file = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, tbTitle.Text);
      if (File.Exists(file))
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
            pictureBox1.Image = result;
          }
        }
        catch (Exception)
        {
        }
      }

      if (!bIsURL)
      {
        useLocalImage = true;
        VideoDatabase.SetThumbURL(CurrentMovie.ID, "file://" + strImageURL);
      }
      else
      {
        VideoDatabase.SetThumbURL(CurrentMovie.ID, strImageURL);
        useLocalImage = false;
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start((string) e.Link.LinkData);
    }
  }
}