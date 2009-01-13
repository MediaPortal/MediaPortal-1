#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgProgress.
  /// </summary>
  public class DlgProgress : MPConfigForm
  {
    private MPButton button2;
    private MPLabel label1;
    private MPLabel label2;
    private ProgressBar progressBar;
    private Object instance;
    private ProgressBar progressBar1;
    private MPLabel mpLabel1;
    private MPButton mpButton1;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    private int count = 0;
    private int total = 0;
    private bool cancelScan = false;

    public DlgProgress()
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // button2
      // 
      this.button2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.Location = new System.Drawing.Point(326, 114);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(56, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "Cancel";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(9, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(391, 16);
      this.label1.TabIndex = 3;
      this.label1.Text = "label1";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(9, 30);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(391, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "label2";
      // 
      // progressBar
      // 
      this.progressBar.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar.Location = new System.Drawing.Point(12, 46);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(370, 15);
      this.progressBar.TabIndex = 5;
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(12, 91);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(370, 15);
      this.progressBar1.TabIndex = 6;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpLabel1.Location = new System.Drawing.Point(12, 73);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(351, 16);
      this.mpLabel1.TabIndex = 7;
      this.mpLabel1.Text = "mpLabel1";
      // 
      // mpButton1
      // 
      this.mpButton1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButton1.Location = new System.Drawing.Point(239, 114);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(82, 23);
      this.mpButton1.TabIndex = 8;
      this.mpButton1.Text = "Cancel Scan";
      this.mpButton1.UseVisualStyleBackColor = true;
      this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // DlgProgress
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(392, 139);
      this.Controls.Add(this.mpButton1);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.progressBar);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximumSize = new System.Drawing.Size(417, 173);
      this.MinimumSize = new System.Drawing.Size(250, 173);
      this.Name = "DlgProgress";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Searching IMDB...";
      this.Load += new System.EventHandler(this.DlgProgress_Load);
      this.ResumeLayout(false);
    }

    #endregion

    private void button2_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    public void CloseProgress()
    {
      this.ResetProgress();
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    public void ResetProgress()
    {
      SetLine1("");
      SetLine2("");
      SetPercentage(0);
      SetHeading("");
      button2.Enabled = true;
    }

    public void DisableCancel()
    {
      button2.Enabled = false;
    }

    public void SetHeading(string label)
    {
      this.Text = label;
    }

    public void SetLine1(string label)
    {
      this.label1.Text = label;
    }

    public void SetLine2(string label)
    {
      this.label2.Text = label;
    }

    public void SetPercentage(int percent)
    {
      this.progressBar.Value = percent;
    }

    public bool IsInstance(Object obj)
    {
      return (this.instance == obj);
    }

    public Object Instance
    {
      set { this.instance = value; }
    }

    public int Total
    {
      get { return this.Total; }
      set
      {
        this.total = value;
        this.progressBar1.Maximum = total;
      }
    }

    public int Count
    {
      get { return this.count; }
      set
      {
        this.count = value;
        this.mpLabel1.Text = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(189), this.count, this.total);
        this.progressBar1.Value = count;
      }
    }

    public bool CancelScan
    {
      get { return this.cancelScan; }
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      this.cancelScan = true;
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void DlgProgress_Load(object sender, EventArgs e)
    {
      if (this.total > 1)
      {
        this.mpButton1.Visible = true;
      }
      else
      {
        this.mpButton1.Visible = false;
      }
      this.cancelScan = false;
    }
  }
}