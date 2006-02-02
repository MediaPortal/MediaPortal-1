#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DShowNET;
using DShowNET.Helper;
using DirectShowLib;

namespace MediaPortal.Configuration.Sections
{
  public class Television : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.ComboBox audioCodecComboBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox videoCodecComboBox;
    private System.Windows.Forms.Label label5;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private System.Windows.Forms.ComboBox countryComboBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox inputComboBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox defaultZoomModeComboBox;
    private System.Windows.Forms.Label label6;
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox textBoxTimeShiftBuffer;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox cbDeinterlace;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbTurnOnTv;
    private GroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox byIndexCheckBox;

    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };

    public Television()
      : this("Television")
    {
    }

    public Television(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();


      //
      // Populate the country combobox
      //
      countryComboBox.Items.AddRange(TunerCountries.Countries);

      //
      // Populate video and audio codecs
      //
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);

      videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
      audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbDeinterlace = new System.Windows.Forms.ComboBox();
      this.label8 = new System.Windows.Forms.Label();
      this.defaultZoomModeComboBox = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.videoCodecComboBox = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.audioCodecComboBox = new System.Windows.Forms.ComboBox();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.countryComboBox = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.inputComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBoxTimeShiftBuffer = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.cbTurnOnTv = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.byIndexCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbDeinterlace);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.videoCodecComboBox);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.audioCodecComboBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 152);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // cbDeinterlace
      // 
      this.cbDeinterlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDeinterlace.Items.AddRange(new object[] {
            "None",
            "Bob",
            "Weave",
            "Best"});
      this.cbDeinterlace.Location = new System.Drawing.Point(168, 92);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(288, 21);
      this.cbDeinterlace.TabIndex = 7;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 96);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(96, 16);
      this.label8.TabIndex = 6;
      this.label8.Text = "Deinterlace mode:";
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Original Source Format",
            "Stretch",
            "Zoom",
            "4:3 Letterbox",
            "4:3 Pan and scan"});
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 116);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultZoomModeComboBox.TabIndex = 9;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 120);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(112, 16);
      this.label6.TabIndex = 8;
      this.label6.Text = "Default zoom mode:";
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 20);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(72, 16);
      this.label5.TabIndex = 0;
      this.label5.Text = "Video codec:";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(72, 16);
      this.label3.TabIndex = 2;
      this.label3.Text = "Audio codec:";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 44);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioCodecComboBox.TabIndex = 3;
      // 
      // radioButton1
      // 
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 24);
      this.radioButton1.TabIndex = 0;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.countryComboBox);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.inputComboBox);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(0, 160);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 82);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "TV Tuner";
      // 
      // countryComboBox
      // 
      this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.countryComboBox.Location = new System.Drawing.Point(168, 44);
      this.countryComboBox.MaxDropDownItems = 16;
      this.countryComboBox.Name = "countryComboBox";
      this.countryComboBox.Size = new System.Drawing.Size(288, 21);
      this.countryComboBox.Sorted = true;
      this.countryComboBox.TabIndex = 3;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 48);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(48, 16);
      this.label4.TabIndex = 2;
      this.label4.Text = "Country:";
      // 
      // inputComboBox
      // 
      this.inputComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.inputComboBox.Items.AddRange(new object[] {
            "Antenna",
            "Cable"});
      this.inputComboBox.Location = new System.Drawing.Point(168, 20);
      this.inputComboBox.Name = "inputComboBox";
      this.inputComboBox.Size = new System.Drawing.Size(288, 21);
      this.inputComboBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(72, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Input source:";
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.textBoxTimeShiftBuffer);
      this.groupBox4.Controls.Add(this.label7);
      this.groupBox4.Controls.Add(this.cbTurnOnTv);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox4.Location = new System.Drawing.Point(0, 248);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(472, 88);
      this.groupBox4.TabIndex = 2;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Timeshifting";
      // 
      // textBoxTimeShiftBuffer
      // 
      this.textBoxTimeShiftBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTimeShiftBuffer.Location = new System.Drawing.Point(168, 52);
      this.textBoxTimeShiftBuffer.Name = "textBoxTimeShiftBuffer";
      this.textBoxTimeShiftBuffer.Size = new System.Drawing.Size(288, 20);
      this.textBoxTimeShiftBuffer.TabIndex = 2;
      this.textBoxTimeShiftBuffer.Text = "30";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 56);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(136, 16);
      this.label7.TabIndex = 1;
      this.label7.Text = "Timeshift buffer (minutes):";
      // 
      // cbTurnOnTv
      // 
      this.cbTurnOnTv.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbTurnOnTv.Location = new System.Drawing.Point(16, 24);
      this.cbTurnOnTv.Name = "cbTurnOnTv";
      this.cbTurnOnTv.Size = new System.Drawing.Size(200, 16);
      this.cbTurnOnTv.TabIndex = 0;
      this.cbTurnOnTv.Text = "Auto turn TV on when entering My TV ";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.byIndexCheckBox);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox3.Location = new System.Drawing.Point(0, 342);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(472, 53);
      this.groupBox3.TabIndex = 3;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Channel Numeric Selection";
      this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
      // 
      // byIndexCheckBox
      // 
      this.byIndexCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.byIndexCheckBox.Location = new System.Drawing.Point(16, 19);
      this.byIndexCheckBox.Name = "byIndexCheckBox";
      this.byIndexCheckBox.Size = new System.Drawing.Size(200, 37);
      this.byIndexCheckBox.TabIndex = 0;
      this.byIndexCheckBox.Text = "Select chennel by index (non-US)";
      this.byIndexCheckBox.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // Television
      // 
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "Television";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        cbTurnOnTv.Checked = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        inputComboBox.SelectedItem = xmlreader.GetValueAsString("capture", "tuner", "Antenna");
        textBoxTimeShiftBuffer.Text = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30).ToString();
        byIndexCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "byindex", true);
        int DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 0);
        if (DeInterlaceMode < 0 || DeInterlaceMode > 3)
          DeInterlaceMode = 3;
        cbDeinterlace.SelectedIndex = DeInterlaceMode;



        //
        // We can't set the SelectedItem here as the items in the combobox are of TunerCountry type.
        //
        countryComboBox.Text = xmlreader.GetValueAsString("capture", "countryname", "Netherlands");

        //
        // Set codecs
        //
        string audioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        string videoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
        if (audioCodec == String.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          if (availableAudioFilters.Count > 0)
          {
            bool Mpeg2DecFilterFound = true;
            bool DScalerFilterFound = true;
            audioCodec = (string)availableAudioFilters[0];
            foreach (string filter in availableAudioFilters)
            {
              if (filter.Equals("MPEG/AC3/DTS/LPCM Audio Decoder"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Audio Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound) audioCodec = "MPEG/AC3/DTS/LPCM Audio Decoder";
            else if (DScalerFilterFound) audioCodec = "DScaler Audio Decoder";
          }
        }
        if (videoCodec == String.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          bool Mpeg2DecFilterFound = true;
          bool DScalerFilterFound = true;
          if (availableVideoFilters.Count > 0)
          {
            videoCodec = (string)availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              if (filter.Equals("Mpeg2Dec Filter"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Mpeg2 Video Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound) videoCodec = "Mpeg2Dec Filter";
            else if (DScalerFilterFound) videoCodec = "DScaler Mpeg2 Video Decoder";
          }
        }
        audioCodecComboBox.SelectedItem = audioCodec;
        videoCodecComboBox.SelectedItem = videoCodec;

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("mytv", "defaultar", "normal");

        for (int index = 0; index < aspectRatio.Length; index++)
        {
          if (aspectRatio[index].Equals(defaultAspectRatio))
          {
            defaultZoomModeComboBox.SelectedIndex = index;
            break;
          }
        }
      }
    }


    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        if (cbDeinterlace.SelectedIndex >= 0)
          xmlwriter.SetValue("mytv", "deinterlace", cbDeinterlace.SelectedIndex.ToString());

        xmlwriter.SetValueAsBool("mytv", "autoturnontv", cbTurnOnTv.Checked);
        xmlwriter.SetValue("capture", "tuner", inputComboBox.Text);
        xmlwriter.SetValueAsBool("mytv", "byindex", byIndexCheckBox.Checked);
        try
        {
          int buffer = Int32.Parse(textBoxTimeShiftBuffer.Text);
          xmlwriter.SetValue("capture", "timeshiftbuffer", buffer.ToString());
        }
        catch (Exception) { }
        xmlwriter.SetValue("mytv", "defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);



        if (countryComboBox.Text.Length > 0)
        {
          TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
          xmlwriter.SetValue("capture", "countryname", countryComboBox.Text);
          xmlwriter.SetValue("capture", "country", tunerCountry.Id.ToString());
        }

        //
        // Set codecs
        //
        xmlwriter.SetValue("mytv", "audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "videocodec", videoCodecComboBox.Text);

      }
    }

    public override object GetSetting(string name)
    {
      switch (name)
      {
        case "television.country":
          {
            int countryId = 0;

            if (countryComboBox.SelectedItem != null)
            {
              TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
              countryId = tunerCountry.Id;
            }

            return countryId;
          }

        case "television.countryname":
          return countryComboBox.Text;
        case "television.isCable":
          {
            return (inputComboBox.Text == "Cable");
          }

      }

      return null;
    }

    public override void OnSectionActivated()
    {

    }

    private void groupBox3_Enter(object sender, EventArgs e)
    {

    }

    private void mpCheckBox1_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void label9_Click(object sender, EventArgs e)
    {

    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }
  }
}

