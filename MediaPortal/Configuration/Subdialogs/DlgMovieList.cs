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
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgMovieList.
  /// </summary>
  public class DlgMovieList : MPConfigForm
  {
    private ListView listView1;
    private ColumnHeader columnHeader1;
    private MPButton buttonOK;
    private MPButton buttonCancel;
    private MPLabel label1;
    private MPLabel labelFileName;
    private MPLabel label2;
    private MPTextBox textBoxTitle;
    private MPButton buttonFind;
    private bool newFind = false;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

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

    public string Filename
    {
      set { labelFileName.Text = value; }
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
      this.buttonFind = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelFileName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
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
      this.listView1.Location = new System.Drawing.Point(8, 52);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(446, 261);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 572;
      // 
      // buttonFind
      // 
      this.buttonFind.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFind.Location = new System.Drawing.Point(404, 319);
      this.buttonFind.Name = "buttonFind";
      this.buttonFind.Size = new System.Drawing.Size(50, 22);
      this.buttonFind.TabIndex = 7;
      this.buttonFind.Text = "Find";
      this.buttonFind.UseVisualStyleBackColor = true;
      this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
      // 
      // textBoxTitle
      // 
      this.textBoxTitle.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTitle.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTitle.Location = new System.Drawing.Point(37, 320);
      this.textBoxTitle.Name = "textBoxTitle";
      this.textBoxTitle.Size = new System.Drawing.Size(362, 20);
      this.textBoxTitle.TabIndex = 6;
      // 
      // label2
      // 
      this.label2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.Location = new System.Drawing.Point(6, 323);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(34, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "Title:";
      // 
      // labelFileName
      // 
      this.labelFileName.Location = new System.Drawing.Point(8, 24);
      this.labelFileName.Name = "labelFileName";
      this.labelFileName.Size = new System.Drawing.Size(369, 16);
      this.labelFileName.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(7, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 3;
      this.label1.Text = "IMDB resutls for:";
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.Location = new System.Drawing.Point(404, 345);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(50, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.button2_Click);
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.Location = new System.Drawing.Point(349, 345);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(50, 23);
      this.buttonOK.TabIndex = 1;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.button1_Click);
      // 
      // DlgMovieList
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(464, 376);
      this.Controls.Add(this.buttonFind);
      this.Controls.Add(this.textBoxTitle);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.labelFileName);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.listView1);
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
      buttonFind.Enabled = false;
      buttonOK.Enabled = false;
      buttonCancel.Enabled = false;
      textBoxTitle.Enabled = false;
      this.newFind = true;
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    public bool IsNewFind
    {
      get { return this.newFind; }
    }

    public string NewTitleToFind
    {
      get { return this.textBoxTitle.Text; }
    }
  }
}