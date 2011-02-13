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
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralAutoplay : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPGroupBox mpGroupBox2;
    private MPLabel labelAutoPlay;
    private MPComboBox autoPlayAudioComboBox;
    private IContainer components = null;
    private MPGroupBox mpGroupBox3;
    private MPLabel mpLabel2;
    private MPComboBox autoPlayPhotoComboBox;
    private MPGroupBox mpGroupBox1;
    private MPLabel mpLabel1;
    private MPComboBox autoPlayVideoComboBox;
    private string autoplayAudio;
    private string autoplayVideo;
    private string autoplayPhoto;

    private string[] autoPlayOptions = new string[]
                                         {
                                           "play",
                                           "do not play",
                                           "ask what to do"
                                         };

    public GeneralAutoplay()
      : this("Autoplay") {}

    public GeneralAutoplay(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      autoPlayAudioComboBox.Items.Clear();
      autoPlayAudioComboBox.Items.AddRange(autoPlayOptions);
      autoPlayVideoComboBox.Items.Clear();
      autoPlayVideoComboBox.Items.AddRange(autoPlayOptions);
      autoPlayPhotoComboBox.Items.Clear();
      autoPlayPhotoComboBox.Items.AddRange(autoPlayOptions);
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


    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        autoplayVideo = xmlreader.GetValueAsString("general", "autoplay_video", "Ask");
        autoplayAudio = xmlreader.GetValueAsString("general", "autoplay_audio", "Ask");
        autoplayPhoto = xmlreader.GetValueAsString("general", "autoplay_photo", "Ask");
      }

      switch (autoplayAudio)
      {
        case "No":
          autoPlayAudioComboBox.Text = autoPlayOptions[1];
          break;

        case "Ask":
          autoPlayAudioComboBox.Text = autoPlayOptions[2];
          break;

        default:
          autoPlayAudioComboBox.Text = autoPlayOptions[0];
          break;
      }

      switch (autoplayVideo)
      {
        case "No":
          autoPlayVideoComboBox.Text = autoPlayOptions[1];
          break;

        case "Ask":
          autoPlayVideoComboBox.Text = autoPlayOptions[2];
          break;

        default:
          autoPlayVideoComboBox.Text = autoPlayOptions[0];
          break;
      }

      switch (autoplayPhoto)
      {
        case "No":
          autoPlayPhotoComboBox.Text = autoPlayOptions[1];
          break;

        case "Ask":
          autoPlayPhotoComboBox.Text = autoPlayOptions[2];
          break;

        default:
          autoPlayPhotoComboBox.Text = autoPlayOptions[0];
          break;
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        if (autoPlayAudioComboBox.Text == autoPlayOptions[1])
          autoplayAudio = "No";
        else if (autoPlayAudioComboBox.Text == autoPlayOptions[2])
          autoplayAudio = "Ask";
        else
          autoplayAudio = "Yes";

        if (autoPlayVideoComboBox.Text == autoPlayOptions[1])
          autoplayVideo = "No";
        else if (autoPlayVideoComboBox.Text == autoPlayOptions[2])
          autoplayVideo = "Ask";
        else
          autoplayVideo = "Yes";

        if (autoPlayPhotoComboBox.Text == autoPlayOptions[1])
          autoplayPhoto = "No";
        else if (autoPlayPhotoComboBox.Text == autoPlayOptions[2])
          autoplayPhoto = "Ask";
        else
          autoplayPhoto = "Yes";

        xmlwriter.SetValue("general", "autoplay_audio", autoplayAudio);
        xmlwriter.SetValue("general", "autoplay_video", autoplayVideo);
        xmlwriter.SetValue("general", "autoplay_photo", autoplayPhoto);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.autoPlayPhotoComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.autoPlayVideoComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelAutoPlay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.autoPlayAudioComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.mpGroupBox3);
      this.groupBox1.Controls.Add(this.mpGroupBox1);
      this.groupBox1.Controls.Add(this.mpGroupBox2);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 326);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Select action for media insertion";
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.mpLabel2);
      this.mpGroupBox3.Controls.Add(this.autoPlayPhotoComboBox);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(6, 181);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(460, 69);
      this.mpGroupBox3.TabIndex = 2;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Photo";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(6, 25);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(39, 13);
      this.mpLabel2.TabIndex = 0;
      this.mpLabel2.Text = "action:";
      // 
      // autoPlayPhotoComboBox
      // 
      this.autoPlayPhotoComboBox.BorderColor = System.Drawing.Color.Empty;
      this.autoPlayPhotoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.autoPlayPhotoComboBox.Location = new System.Drawing.Point(51, 22);
      this.autoPlayPhotoComboBox.Name = "autoPlayPhotoComboBox";
      this.autoPlayPhotoComboBox.Size = new System.Drawing.Size(145, 21);
      this.autoPlayPhotoComboBox.TabIndex = 1;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.autoPlayVideoComboBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 106);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(460, 69);
      this.mpGroupBox1.TabIndex = 2;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Video";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 25);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(39, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "action:";
      // 
      // autoPlayVideoComboBox
      // 
      this.autoPlayVideoComboBox.BorderColor = System.Drawing.Color.Empty;
      this.autoPlayVideoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.autoPlayVideoComboBox.Location = new System.Drawing.Point(51, 22);
      this.autoPlayVideoComboBox.Name = "autoPlayVideoComboBox";
      this.autoPlayVideoComboBox.Size = new System.Drawing.Size(145, 21);
      this.autoPlayVideoComboBox.TabIndex = 1;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.labelAutoPlay);
      this.mpGroupBox2.Controls.Add(this.autoPlayAudioComboBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 31);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(460, 69);
      this.mpGroupBox2.TabIndex = 1;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Audio";
      // 
      // labelAutoPlay
      // 
      this.labelAutoPlay.AutoSize = true;
      this.labelAutoPlay.Location = new System.Drawing.Point(6, 25);
      this.labelAutoPlay.Name = "labelAutoPlay";
      this.labelAutoPlay.Size = new System.Drawing.Size(39, 13);
      this.labelAutoPlay.TabIndex = 0;
      this.labelAutoPlay.Text = "action:";
      // 
      // autoPlayAudioComboBox
      // 
      this.autoPlayAudioComboBox.BorderColor = System.Drawing.Color.Empty;
      this.autoPlayAudioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.autoPlayAudioComboBox.Location = new System.Drawing.Point(51, 22);
      this.autoPlayAudioComboBox.Name = "autoPlayAudioComboBox";
      this.autoPlayAudioComboBox.Size = new System.Drawing.Size(145, 21);
      this.autoPlayAudioComboBox.TabIndex = 1;
      // 
      // GeneralAutoplay
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBox1);
      this.Name = "GeneralAutoplay";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion
  }
}