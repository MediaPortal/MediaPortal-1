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
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.TVE2.Sections
{
  public class Radio : SectionSettings
  {
    protected MPGroupBox groupBox2;
    protected MPTextBox folderNameTextBox;
    protected MPLabel folderNameLabel;
    protected MPButton browseFolderButton;
    protected FolderBrowserDialog folderBrowserDialog;
    protected OpenFileDialog openFileDialog;
    private MPLabel label2;
    protected IContainer components = null;

    public Radio()
      : this("Radio")
    {
    }

    public Radio(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Disable if TVE3
      if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
      }
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
    protected void InitializeComponent()
    {
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.browseFolderButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.folderNameLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.browseFolderButton);
      this.groupBox2.Controls.Add(this.folderNameTextBox);
      this.groupBox2.Controls.Add(this.folderNameLabel);
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 56);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Internet Radio Stream Settings (asx, pls, ...)";
      // 
      // browseFolderButton
      // 
      this.browseFolderButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browseFolderButton.Location = new System.Drawing.Point(384, 19);
      this.browseFolderButton.Name = "browseFolderButton";
      this.browseFolderButton.Size = new System.Drawing.Size(72, 22);
      this.browseFolderButton.TabIndex = 2;
      this.browseFolderButton.Text = "Browse";
      this.browseFolderButton.Click += new System.EventHandler(this.browseFolderButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.Location = new System.Drawing.Point(168, 20);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.folderNameTextBox.TabIndex = 1;
      this.folderNameTextBox.Text = "";
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 24);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(88, 16);
      this.folderNameLabel.TabIndex = 0;
      this.folderNameLabel.Text = "Streams folder:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Name = "label2";
      this.label2.TabIndex = 0;
      // 
      // Radio
      // 
      this.Controls.Add(this.groupBox2);
      this.Name = "Radio";
      this.Size = new System.Drawing.Size(472, 408);
      this.Load += new System.EventHandler(this.Radio_Load);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    public override void OnSectionActivated()
    {
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        folderNameTextBox.Text = xmlreader.GetValueAsString("radio", "folder", "");
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("radio", "folder", folderNameTextBox.Text);
      }
    }


    protected void browseFolderButton_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where stream playlists will be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }


    private void Radio_Load(object sender, EventArgs e)
    {
    }
  }
}