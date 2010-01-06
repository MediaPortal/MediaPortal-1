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
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MoviePlayer : SectionSettings
  {
    private MPGroupBox groupBoxExternalPlayer;
    private MPButton parametersButton;
    private MPTextBox parametersTextBox;
    private MPLabel label2;
    private MPButton fileNameButton;
    private MPTextBox fileNameTextBox;
    private MPLabel label1;
    private MPCheckBox externalPlayerCheckBox;
    private OpenFileDialog openFileDialog;
    private IContainer components = null;
    private MPGroupBox wmvGroupBox;
    private MPCheckBox wmvCheckBox;
    private MPLabel mpLabel2;
    private bool _init = false;

    public MoviePlayer()
      : this("Video Player") {}

    public MoviePlayer(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      groupBoxExternalPlayer.Visible = SettingsForm.AdvancedMode;
      wmvCheckBox.Enabled = !Startup._automaticMovieCodec;
      if (wmvCheckBox.Enabled == false)
      {
        wmvCheckBox.Checked = false;
      }
      _init = true;
      LoadSettings();
    }

    /// <summary>
    /// Loads the movie player settings
    /// </summary>
    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlreader = new MPSettings())
      {
        fileNameTextBox.Text = xmlreader.GetValueAsString("movieplayer", "path", "");
        parametersTextBox.Text = xmlreader.GetValueAsString("movieplayer", "arguments", "");
        externalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "internal", true);
        externalPlayerCheckBox.Checked = !externalPlayerCheckBox.Checked;
        wmvCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "wmvaudio", false);
      }
    }

    /// <summary>
    /// Saves movie player settings and codec info.
    /// </summary>
    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "path", fileNameTextBox.Text);
        xmlwriter.SetValue("movieplayer", "arguments", parametersTextBox.Text);
        xmlwriter.SetValueAsBool("movieplayer", "internal", !externalPlayerCheckBox.Checked);
        xmlwriter.SetValueAsBool("movieplayer", "wmvaudio", wmvCheckBox.Checked);
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
    private void InitializeComponent()
    {
      this.groupBoxExternalPlayer = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.externalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.parametersButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.fileNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.wmvGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.wmvCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxExternalPlayer.SuspendLayout();
      this.wmvGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxExternalPlayer
      // 
      this.groupBoxExternalPlayer.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxExternalPlayer.Controls.Add(this.externalPlayerCheckBox);
      this.groupBoxExternalPlayer.Controls.Add(this.parametersButton);
      this.groupBoxExternalPlayer.Controls.Add(this.parametersTextBox);
      this.groupBoxExternalPlayer.Controls.Add(this.label2);
      this.groupBoxExternalPlayer.Controls.Add(this.fileNameButton);
      this.groupBoxExternalPlayer.Controls.Add(this.fileNameTextBox);
      this.groupBoxExternalPlayer.Controls.Add(this.label1);
      this.groupBoxExternalPlayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxExternalPlayer.Location = new System.Drawing.Point(3, 71);
      this.groupBoxExternalPlayer.Name = "groupBoxExternalPlayer";
      this.groupBoxExternalPlayer.Size = new System.Drawing.Size(472, 112);
      this.groupBoxExternalPlayer.TabIndex = 1;
      this.groupBoxExternalPlayer.TabStop = false;
      this.groupBoxExternalPlayer.Text = "External player";
      // 
      // externalPlayerCheckBox
      // 
      this.externalPlayerCheckBox.AutoSize = true;
      this.externalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.externalPlayerCheckBox.Location = new System.Drawing.Point(19, 28);
      this.externalPlayerCheckBox.Name = "externalPlayerCheckBox";
      this.externalPlayerCheckBox.Size = new System.Drawing.Size(231, 17);
      this.externalPlayerCheckBox.TabIndex = 0;
      this.externalPlayerCheckBox.Text = "Use external player (replaces internal player)";
      this.externalPlayerCheckBox.UseVisualStyleBackColor = true;
      this.externalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.externalPlayerCheckBox_CheckedChanged);
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.Location = new System.Drawing.Point(384, 84);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(72, 22);
      this.parametersButton.TabIndex = 6;
      this.parametersButton.Text = "List";
      this.parametersButton.UseVisualStyleBackColor = true;
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.BorderColor = System.Drawing.Color.Empty;
      this.parametersTextBox.Location = new System.Drawing.Point(168, 84);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(208, 20);
      this.parametersTextBox.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 88);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 15);
      this.label2.TabIndex = 4;
      this.label2.Text = "Parameters:";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.Location = new System.Drawing.Point(384, 60);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 3;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.UseVisualStyleBackColor = true;
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.fileNameTextBox.Location = new System.Drawing.Point(168, 60);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.fileNameTextBox.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 64);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Path/Filename:";
      // 
      // wmvGroupBox
      // 
      this.wmvGroupBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.wmvGroupBox.Controls.Add(this.mpLabel2);
      this.wmvGroupBox.Controls.Add(this.wmvCheckBox);
      this.wmvGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.wmvGroupBox.Location = new System.Drawing.Point(3, 3);
      this.wmvGroupBox.Name = "wmvGroupBox";
      this.wmvGroupBox.Size = new System.Drawing.Size(472, 62);
      this.wmvGroupBox.TabIndex = 7;
      this.wmvGroupBox.TabStop = false;
      this.wmvGroupBox.Text = "WMV playback (internal player)";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(34, 39);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(326, 16);
      this.mpLabel2.TabIndex = 10;
      this.mpLabel2.Text = "Will not be applied if Automatic Decoder Settings enabled.";
      // 
      // wmvCheckBox
      // 
      this.wmvCheckBox.AutoSize = true;
      this.wmvCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.wmvCheckBox.Location = new System.Drawing.Point(19, 19);
      this.wmvCheckBox.Name = "wmvCheckBox";
      this.wmvCheckBox.Size = new System.Drawing.Size(233, 17);
      this.wmvCheckBox.TabIndex = 0;
      this.wmvCheckBox.Text = "Use 5.1 audio playback for WMV movie files";
      this.wmvCheckBox.UseVisualStyleBackColor = true;
      // 
      // MoviePlayer
      // 
      this.Controls.Add(this.wmvGroupBox);
      this.Controls.Add(this.groupBoxExternalPlayer);
      this.Name = "MoviePlayer";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxExternalPlayer.ResumeLayout(false);
      this.groupBoxExternalPlayer.PerformLayout();
      this.wmvGroupBox.ResumeLayout(false);
      this.wmvGroupBox.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void externalPlayerCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      fileNameTextBox.Enabled =
        fileNameButton.Enabled = parametersTextBox.Enabled = parametersButton.Enabled = externalPlayerCheckBox.Checked;
    }

    /// <summary>
    /// sets the external movies player source file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void fileNameButton_Click(object sender, EventArgs e)
    {
      using (openFileDialog = new OpenFileDialog())
      {
        openFileDialog.FileName = fileNameTextBox.Text;
        openFileDialog.CheckFileExists = true;
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Filter = "exe files (*.exe)|*.exe";
        openFileDialog.FilterIndex = 0;
        openFileDialog.Title = "Select movie player";
        DialogResult dialogResult = openFileDialog.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
          fileNameTextBox.Text = openFileDialog.FileName;
        }
      }
    }

    /// <summary>
    /// sets the external movies player parameters.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void parametersButton_Click(object sender, EventArgs e)
    {
      ParameterForm parameters = new ParameterForm();
      parameters.AddParameter("%filename%", "Will be replaced by currently selected media file");
      if (parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }
  }
}