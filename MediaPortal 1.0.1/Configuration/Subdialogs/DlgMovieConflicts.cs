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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Video.Database;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgMovieConflicts.
  /// </summary>
  public class DlgMovieConflicts : MPConfigForm, IMDB.IProgress
  {
    private ListView listView1;
    private ColumnHeader columnHeader1;
    private MPButton button2;
    private MPLabel label1;
    private MPLabel label2;
    private MPTextBox textBoxTitle;
    private MPButton buttonFind;
    private DlgProgress progressDialog = new DlgProgress();
    private string newMovieToFind = string.Empty;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

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
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public int SelectedItem
    {
      get
      {
        if (listView1.SelectedIndices.Count <= 0)
        {
          return 0;
        }
        return listView1.SelectedIndices[0];
      }
    }

    public void AddMovie(string movie)
    {
      listView1.Items.Add(movie);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonFind = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                        {
                                          this.columnHeader1
                                        });
      this.listView1.Location = new System.Drawing.Point(8, 34);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(447, 281);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 440;
      // 
      // button2
      // 
      this.button2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.Location = new System.Drawing.Point(399, 347);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(56, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "Close";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(352, 23);
      this.label1.TabIndex = 3;
      this.label1.Text = "There was a conflict with the following files/movies:";
      // 
      // label2
      // 
      this.label2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.Location = new System.Drawing.Point(7, 320);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(32, 17);
      this.label2.TabIndex = 5;
      this.label2.Text = "Title:";
      // 
      // textBoxTitle
      // 
      this.textBoxTitle.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTitle.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTitle.Location = new System.Drawing.Point(36, 320);
      this.textBoxTitle.Name = "textBoxTitle";
      this.textBoxTitle.Size = new System.Drawing.Size(358, 20);
      this.textBoxTitle.TabIndex = 6;
      // 
      // buttonFind
      // 
      this.buttonFind.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFind.Location = new System.Drawing.Point(399, 319);
      this.buttonFind.Name = "buttonFind";
      this.buttonFind.Size = new System.Drawing.Size(56, 23);
      this.buttonFind.TabIndex = 7;
      this.buttonFind.Text = "Find";
      this.buttonFind.UseVisualStyleBackColor = true;
      this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
      // 
      // DlgMovieConflicts
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(464, 376);
      this.Controls.Add(this.buttonFind);
      this.Controls.Add(this.textBoxTitle);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.listView1);
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
      if (listView1.SelectedItems == null)
      {
        return;
      }
      int index = 0;
      ListViewItem listItem = listView1.SelectedItems[0];
      if (textBoxTitle.Text == string.Empty)
      {
        MessageBox.Show("Please select a movie", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        ;
        return;
      }
      progressDialog.Total = 1;
      progressDialog.Count = 1;

      string file = listItem.Text;
      IMDBMovie movieDetails = new IMDBMovie();
      int id = VideoDatabase.GetMovieInfo(file, ref movieDetails);
      string path, filename;
      Util.Utils.Split(file, out path, out filename);
      movieDetails.Path = path;
      movieDetails.File = filename;
      movieDetails.SearchString = textBoxTitle.Text;
      if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, false))
      {
        if (movieDetails != null)
        {
          index = listItem.Index;
          listView1.Items.Remove(listItem);
          if (listView1.Items.Count > 0)
          {
            if (index >= listView1.Items.Count)
            {
              index = listView1.Items.Count - 1;
            }
            listView1.SelectedIndices.Clear();
            listView1.SelectedIndices.Add(index);
            listView1.Update();
          }
          else
          {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
          }
        }
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        string strFileName = listView1.SelectedItems[0].Text;
        string strMovieName = string.Empty;
        if (Util.Utils.IsDVD(strFileName))
        {
          //DVD
          string strDrive = strFileName.Substring(0, 2);
          strMovieName = Util.Utils.GetDriveName(strDrive);
        }
        else if (strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
        {
          //DVD folder
          string dvdFolder = strFileName.Substring(0, strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
          strMovieName = Path.GetFileName(dvdFolder);
        }
        else
        {
          //Movie 
          strMovieName = Path.GetFileNameWithoutExtension(strFileName);
        }

        textBoxTitle.Text = strMovieName;
      }
      else
      {
        textBoxTitle.Text = "";
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
      MessageBox.Show("No IMDB info found!", fetcher.MovieName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
      MessageBox.Show("Movie details could not be found.", fetcher.MovieName, MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);
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
      if (listView1.Items.Count > 0)
      {
        listView1.SelectedIndices.Clear();
        listView1.SelectedIndices.Add(0);
      }
    }
  }
}