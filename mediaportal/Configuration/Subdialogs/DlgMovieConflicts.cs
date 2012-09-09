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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Video.Database;
using MediaPortal.Profile;
using System.Text.RegularExpressions;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgMovieConflicts.
  /// </summary>
  public class DlgMovieConflicts : MPConfigForm, IMDB.IProgress
  {
    private ListView _listView1;
    private ColumnHeader _columnHeader1;
    private MPButton _button2;
    private MPLabel _label1;
    private MPLabel _label2;
    private MPTextBox _textBoxTitle;
    private MPButton _buttonFind;
    private DlgProgress _progressDialog = new DlgProgress();
    private string _newMovieToFind = string.Empty;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container _components = null;

    public DlgMovieConflicts()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_components != null)
        {
          _components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public int SelectedItem
    {
      get
      {
        if (_listView1.SelectedIndices.Count <= 0)
        {
          return 0;
        }
        return _listView1.SelectedIndices[0];
      }
    }

    // Changed code-Just red color as warning for conflicts
    public void AddMovie(string movie)
    {
      _listView1.Items.Add(movie).ForeColor = System.Drawing.Color.Red;
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this._listView1 = new System.Windows.Forms.ListView();
      this._columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this._button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this._label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._textBoxTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this._buttonFind = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this._listView1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this._listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                         {
                                           this._columnHeader1
                                         });
      this._listView1.Location = new System.Drawing.Point(8, 34);
      this._listView1.Name = "_listView1";
      this._listView1.Size = new System.Drawing.Size(447, 281);
      this._listView1.TabIndex = 0;
      this._listView1.UseCompatibleStateImageBehavior = false;
      this._listView1.View = System.Windows.Forms.View.Details;
      this._listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this._columnHeader1.Text = "Name";
      this._columnHeader1.Width = 440;
      // 
      // button2
      // 
      this._button2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._button2.Location = new System.Drawing.Point(399, 347);
      this._button2.Name = "_button2";
      this._button2.Size = new System.Drawing.Size(56, 23);
      this._button2.TabIndex = 2;
      this._button2.Text = "Close";
      this._button2.UseVisualStyleBackColor = true;
      this._button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // label1
      // 
      this._label1.Location = new System.Drawing.Point(16, 8);
      this._label1.Name = "_label1";
      this._label1.Size = new System.Drawing.Size(352, 23);
      this._label1.TabIndex = 3;
      this._label1.Text = "There was a conflict with the following files/movies:";
      // 
      // label2
      // 
      this._label2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._label2.Location = new System.Drawing.Point(7, 320);
      this._label2.Name = "_label2";
      this._label2.Size = new System.Drawing.Size(32, 20);
      this._label2.TabIndex = 5;
      this._label2.Text = "Title:";
      this._label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textBoxTitle
      // 
      this._textBoxTitle.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this._textBoxTitle.BorderColor = System.Drawing.Color.Empty;
      this._textBoxTitle.Location = new System.Drawing.Point(36, 320);
      this._textBoxTitle.Name = "_textBoxTitle";
      this._textBoxTitle.Size = new System.Drawing.Size(358, 20);
      this._textBoxTitle.TabIndex = 6;
      // 
      // buttonFind
      // 
      this._buttonFind.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._buttonFind.Location = new System.Drawing.Point(399, 319);
      this._buttonFind.Name = "_buttonFind";
      this._buttonFind.Size = new System.Drawing.Size(56, 23);
      this._buttonFind.TabIndex = 7;
      this._buttonFind.Text = "Find";
      this._buttonFind.UseVisualStyleBackColor = true;
      this._buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
      // 
      // DlgMovieConflicts
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(464, 376);
      this.Controls.Add(this._buttonFind);
      this.Controls.Add(this._textBoxTitle);
      this.Controls.Add(this._label2);
      this.Controls.Add(this._label1);
      this.Controls.Add(this._button2);
      this.Controls.Add(this._listView1);
      this.MinimumSize = new System.Drawing.Size(393, 354);
      this.Name = "DlgMovieConflicts";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "IMDB Movie conflicts";
      this.Load += new System.EventHandler(this.DlgMovieConflicts_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private void button2_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void buttonFind_Click(object sender, EventArgs e)
    {
      int index = 0;
      ListViewItem listItem = _listView1.SelectedItems[0];
      if (_textBoxTitle.Text == string.Empty)
      {
        MessageBox.Show("Please select a movie", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        ;
        return;
      }
      _progressDialog.Total = 1;
      _progressDialog.Count = 1;

      string file = listItem.Text;
      IMDBMovie movieDetails = new IMDBMovie();
      string path, filename;
      Util.Utils.Split(file, out path, out filename);
      movieDetails.Path = path;
      movieDetails.File = filename;
      movieDetails.SearchString = _textBoxTitle.Text;
      if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, false))
      {
        if (movieDetails != null)
        {
          index = listItem.Index;
          _listView1.Items.Remove(listItem);
          if (_listView1.Items.Count > 0)
          {
            if (index >= _listView1.Items.Count)
            {
              index = _listView1.Items.Count - 1;
            }
            _listView1.SelectedIndices.Clear();
            _listView1.SelectedIndices.Add(index);
            _listView1.Update();
          }
          else
          {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
          }
        }
      }
    }

    // Changed - remove CDx from movie name
    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_listView1.SelectedItems.Count > 0)
      {
        string strFileName = _listView1.SelectedItems[0].Text;
        string strMovieName = string.Empty;
        
        if (Util.Utils.IsDVD(strFileName))
        {
          // DVD
          string strDrive = strFileName.Substring(0, 2);
          strMovieName = Util.Utils.GetDriveName(strDrive);
        }
        else if (strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
        {
          // DVD folder
          string dvdFolder = strFileName.Substring(0, strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
          strMovieName = Path.GetFileName(dvdFolder);
        }
        else if (strFileName.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV") >= 0)
        {
          // BD folder
          string bdFolder = strFileName.Substring(0, strFileName.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV"));
          strMovieName = Path.GetFileName(bdFolder);
        }
        else
        {
          // Movie - Movie folder title + remove CD from title
          string dir = Path.GetDirectoryName(strFileName);
          bool foldercheck = Util.Utils.IsFolderDedicatedMovieFolder(dir);
          
          if (foldercheck)
          {
            strMovieName = Path.GetFileName(Path.GetDirectoryName(strFileName));
          }
          else
          {
            strMovieName = Path.GetFileNameWithoutExtension(strFileName);
          }

          // Test pattern (CD, DISK, Part, X-Y...) and remove it from filename
          var pattern = Util.Utils.StackExpression();
          
          for (int i = 0; i < pattern.Length; i++)
          {
            if (foldercheck == false && pattern[i].IsMatch(strMovieName))
            {
              strMovieName = pattern[i].Replace(strMovieName, "");
            }
          }
        }

        Util.Utils.RemoveStackEndings(ref strMovieName);
        _textBoxTitle.Text = strMovieName;
      }
      else
      {
        _textBoxTitle.Text = "";
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
      this.Update();
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
      MessageBox.Show("No IMDB info found!", fetcher.MovieName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
      this.Update();
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
      _progressDialog.SetHeading("Downloading Actors and roles...");
      _progressDialog.SetLine1("Downloading Actors and Roles...");
      _progressDialog.SetLine2(fetcher.MovieName);
      _progressDialog.Instance = fetcher;
      return true;
    }

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
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
      MessageBox.Show("Movie details could not be found.", fetcher.MovieName, MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);
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
        _newMovieToFind = dlg.NewTitleToFind;
        selectedMovie = -1;
      }
      return true;
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selectedActor)
    {
      selectedActor= -1;
      return true;
    }

    public bool OnScanStart(int total)
    {
      return true;
    }

    public bool OnScanEnd()
    {
      return true;
    }

    public bool OnScanIterating(int count)
    {
      return true;
    }

    public bool OnScanIterated(int count)
    {
      return true;
    }

    #endregion

    private void DlgMovieConflicts_Load(object sender, EventArgs e)
    {
      if (_listView1.Items.Count > 0)
      {
        _listView1.SelectedIndices.Clear();
        _listView1.SelectedIndices.Add(0);
      }
    }
  }
}