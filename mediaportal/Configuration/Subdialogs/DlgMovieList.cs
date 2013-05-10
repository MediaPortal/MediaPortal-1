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
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgMovieList.
  /// </summary>
  public class DlgMovieList : MPConfigForm
  {
    private ListView _listView1;
    private ColumnHeader _columnHeader1;
    private MPButton _buttonOk;
    private MPButton _buttonCancel;
    private MPLabel _label1;
    private MPLabel _labelFileName;
    private MPLabel _label2;
    private MPTextBox _textBoxTitle;
    private MPButton _buttonFind;
    private bool _newFind = false;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container _components = null;

    public DlgMovieList()
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

    public string Filename
    {
      set { _labelFileName.Text = value; }
    }

    // Changed colors and font style (better visualization) for AKAs (too many titles so
    // better visualization by colour where movie begins and ends within AKAs)
    // This will work with IMDB_WITH _AKA Grabber
    // Other grabbers pass this
    public void AddMovie(string movie)
    {
      // AKA titles - no change
      if (movie.Contains("IMDB_With_AKA"))
      {
        ListViewItem newItem = new ListViewItem();
        newItem.Text = movie;
        newItem.ForeColor = System.Drawing.Color.Black;
        newItem.Font = new System.Drawing.Font(_listView1.Font, System.Drawing.FontStyle.Regular);
        _listView1.Items.Add(newItem);
      }
      else
      {
        // Original title - style change
        // Bold and Green if there is words match, Bold and orange for others
        ListViewItem newItem = new ListViewItem();
        newItem.Text = movie;
        if (movie.ToLowerInvariant().Contains(_labelFileName.Text.ToLowerInvariant()))
        {
          newItem.ForeColor = System.Drawing.Color.Green;
        }
        else
        {
          newItem.ForeColor = System.Drawing.Color.OrangeRed;
        }
        newItem.Font = new System.Drawing.Font(_listView1.Font, System.Drawing.FontStyle.Bold);
        _listView1.Items.Add(newItem);
      }
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
      this._buttonFind = new MediaPortal.UserInterface.Controls.MPButton();
      this._textBoxTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this._label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._labelFileName = new MediaPortal.UserInterface.Controls.MPLabel();
      this._label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this._buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
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
      this._listView1.Location = new System.Drawing.Point(8, 52);
      this._listView1.Name = "_listView1";
      this._listView1.Size = new System.Drawing.Size(446, 261);
      this._listView1.TabIndex = 0;
      this._listView1.UseCompatibleStateImageBehavior = false;
      this._listView1.View = System.Windows.Forms.View.Details;
      this._listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
      // 
      // columnHeader1
      // 
      this._columnHeader1.Text = "Name";
      this._columnHeader1.Width = 572;
      // 
      // buttonFind
      // 
      this._buttonFind.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._buttonFind.Location = new System.Drawing.Point(404, 319);
      this._buttonFind.Name = "_buttonFind";
      this._buttonFind.Size = new System.Drawing.Size(50, 22);
      this._buttonFind.TabIndex = 7;
      this._buttonFind.Text = "Find";
      this._buttonFind.UseVisualStyleBackColor = true;
      this._buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
      // 
      // textBoxTitle
      // 
      this._textBoxTitle.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this._textBoxTitle.BorderColor = System.Drawing.Color.Empty;
      this._textBoxTitle.Location = new System.Drawing.Point(37, 320);
      this._textBoxTitle.Name = "_textBoxTitle";
      this._textBoxTitle.Size = new System.Drawing.Size(362, 20);
      this._textBoxTitle.TabIndex = 6;
      // 
      // label2
      // 
      this._label2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._label2.Location = new System.Drawing.Point(6, 323);
      this._label2.Name = "_label2";
      this._label2.Size = new System.Drawing.Size(34, 16);
      this._label2.TabIndex = 5;
      this._label2.Text = "Title:";
      // 
      // labelFileName
      // 
      this._labelFileName.Location = new System.Drawing.Point(8, 24);
      this._labelFileName.Name = "_labelFileName";
      this._labelFileName.Size = new System.Drawing.Size(369, 16);
      this._labelFileName.TabIndex = 4;
      // 
      // label1
      // 
      this._label1.Location = new System.Drawing.Point(7, 8);
      this._label1.Name = "_label1";
      this._label1.Size = new System.Drawing.Size(100, 16);
      this._label1.TabIndex = 3;
      this._label1.Text = "IMDB resutls for:";
      // 
      // buttonCancel
      // 
      this._buttonCancel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._buttonCancel.Location = new System.Drawing.Point(404, 345);
      this._buttonCancel.Name = "_buttonCancel";
      this._buttonCancel.Size = new System.Drawing.Size(50, 23);
      this._buttonCancel.TabIndex = 2;
      this._buttonCancel.Text = "Cancel";
      this._buttonCancel.UseVisualStyleBackColor = true;
      this._buttonCancel.Click += new System.EventHandler(this.button2_Click);
      // 
      // buttonOK
      // 
      this._buttonOk.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._buttonOk.Location = new System.Drawing.Point(349, 345);
      this._buttonOk.Name = "_buttonOk";
      this._buttonOk.Size = new System.Drawing.Size(50, 23);
      this._buttonOk.TabIndex = 1;
      this._buttonOk.Text = "OK";
      this._buttonOk.UseVisualStyleBackColor = true;
      this._buttonOk.Click += new System.EventHandler(this.button1_Click);
      // 
      // DlgMovieList
      // 
      this.AcceptButton = this._buttonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this._buttonCancel;
      this.ClientSize = new System.Drawing.Size(464, 376);
      this.Controls.Add(this._buttonFind);
      this.Controls.Add(this._textBoxTitle);
      this.Controls.Add(this._label2);
      this.Controls.Add(this._labelFileName);
      this.Controls.Add(this._label1);
      this.Controls.Add(this._buttonCancel);
      this.Controls.Add(this._buttonOk);
      this.Controls.Add(this._listView1);
      this.MinimumSize = new System.Drawing.Size(393, 354);
      this.Name = "DlgMovieList";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "IMDB Movie results";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private void button1_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void listView1_DoubleClick(object sender, EventArgs e)
    {
      base.OnDoubleClick(e);
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void buttonFind_Click(object sender, EventArgs e)
    {
      _buttonFind.Enabled = false;
      _buttonOk.Enabled = false;
      _buttonCancel.Enabled = false;
      _textBoxTitle.Enabled = false;
      this._newFind = true;
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    public bool IsNewFind
    {
      get { return this._newFind; }
    }

    public string NewTitleToFind
    {
      get { return this._textBoxTitle.Text; }
    }
  }
}