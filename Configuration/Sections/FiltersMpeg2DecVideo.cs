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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using DShowNET;
using DirectShowLib;
using MediaPortal.GUI.Library;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersMPEG2DecVideo : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSettings;
    private MediaPortal.UserInterface.Controls.MPLabel labelDeinterlaceMethod;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDeinterlace;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxForcedSubtitles;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxPlanar;
    private MediaPortal.UserInterface.Controls.MPLabel labelBrightness;
    private MediaPortal.UserInterface.Controls.MPLabel labelContrast;
    private MediaPortal.UserInterface.Controls.MPLabel labelHue;
    private MediaPortal.UserInterface.Controls.MPLabel labelSaturation;
    private System.Windows.Forms.TrackBar trackBarBrightness;
    private System.Windows.Forms.TrackBar trackBarContrast;
    private System.Windows.Forms.TrackBar trackBarHue;
    private System.Windows.Forms.TrackBar trackBarSaturation;
    private MediaPortal.UserInterface.Controls.MPLabel labelNote;
    private MediaPortal.UserInterface.Controls.MPButton buttonReset;
    private MediaPortal.UserInterface.Controls.MPButton buttonTvDefaults;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxOutputInterlaced;
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public FiltersMPEG2DecVideo()
      : this("MPV Decoder")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public FiltersMPEG2DecVideo(string name)
      : base(name)
    {

      // This call is required by the Windows Form Designer.
      InitializeComponent();
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
      this.groupBoxSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxOutputInterlaced = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonReset = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonTvDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelSaturation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHue = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelContrast = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelBrightness = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelDeinterlaceMethod = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxForcedSubtitles = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxPlanar = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.trackBarSaturation = new System.Windows.Forms.TrackBar();
      this.trackBarHue = new System.Windows.Forms.TrackBar();
      this.trackBarContrast = new System.Windows.Forms.TrackBar();
      this.trackBarBrightness = new System.Windows.Forms.TrackBar();
      this.groupBoxSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarSaturation)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarHue)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarContrast)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.checkBoxOutputInterlaced);
      this.groupBoxSettings.Controls.Add(this.buttonReset);
      this.groupBoxSettings.Controls.Add(this.buttonTvDefaults);
      this.groupBoxSettings.Controls.Add(this.labelNote);
      this.groupBoxSettings.Controls.Add(this.labelSaturation);
      this.groupBoxSettings.Controls.Add(this.labelHue);
      this.groupBoxSettings.Controls.Add(this.labelContrast);
      this.groupBoxSettings.Controls.Add(this.labelBrightness);
      this.groupBoxSettings.Controls.Add(this.comboBoxDeinterlace);
      this.groupBoxSettings.Controls.Add(this.labelDeinterlaceMethod);
      this.groupBoxSettings.Controls.Add(this.checkBoxForcedSubtitles);
      this.groupBoxSettings.Controls.Add(this.checkBoxPlanar);
      this.groupBoxSettings.Controls.Add(this.trackBarSaturation);
      this.groupBoxSettings.Controls.Add(this.trackBarHue);
      this.groupBoxSettings.Controls.Add(this.trackBarContrast);
      this.groupBoxSettings.Controls.Add(this.trackBarBrightness);
      this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSettings.Location = new System.Drawing.Point(0, 0);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(472, 405);
      this.groupBoxSettings.TabIndex = 0;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // checkBoxOutputInterlaced
      // 
      this.checkBoxOutputInterlaced.AutoSize = true;
      this.checkBoxOutputInterlaced.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxOutputInterlaced.Location = new System.Drawing.Point(16, 70);
      this.checkBoxOutputInterlaced.Name = "checkBoxOutputInterlaced";
      this.checkBoxOutputInterlaced.Size = new System.Drawing.Size(157, 17);
      this.checkBoxOutputInterlaced.TabIndex = 15;
      this.checkBoxOutputInterlaced.Text = "Set interlaced flag for output";
      this.checkBoxOutputInterlaced.UseVisualStyleBackColor = true;
      this.checkBoxOutputInterlaced.CheckedChanged += new System.EventHandler(this.checkBoxOutputInterlaced_CheckedChanged);
      // 
      // buttonReset
      // 
      this.buttonReset.Location = new System.Drawing.Point(272, 322);
      this.buttonReset.Name = "buttonReset";
      this.buttonReset.Size = new System.Drawing.Size(75, 23);
      this.buttonReset.TabIndex = 14;
      this.buttonReset.Text = "Reset";
      this.buttonReset.UseVisualStyleBackColor = true;
      this.buttonReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // buttonTvDefaults
      // 
      this.buttonTvDefaults.Location = new System.Drawing.Point(165, 322);
      this.buttonTvDefaults.Name = "buttonTvDefaults";
      this.buttonTvDefaults.Size = new System.Drawing.Size(75, 23);
      this.buttonTvDefaults.TabIndex = 13;
      this.buttonTvDefaults.Text = "TV defaults";
      this.buttonTvDefaults.UseVisualStyleBackColor = true;
      this.buttonTvDefaults.Click += new System.EventHandler(this.buttonTvDefaults_Click);
      // 
      // labelNote
      // 
      this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelNote.Location = new System.Drawing.Point(13, 362);
      this.labelNote.Name = "labelNote";
      this.labelNote.Size = new System.Drawing.Size(440, 40);
      this.labelNote.TabIndex = 12;
      this.labelNote.Text = "Note: Using a non-planar output format, bob deinterlacer, or adjusting color prop" +
          "erties may degrade performance. \"Auto\" deinterlacer will switch to \"Blend\" if ne" +
          "cessary.";
      // 
      // labelSaturation
      // 
      this.labelSaturation.Location = new System.Drawing.Point(13, 272);
      this.labelSaturation.Name = "labelSaturation";
      this.labelSaturation.Size = new System.Drawing.Size(64, 16);
      this.labelSaturation.TabIndex = 8;
      this.labelSaturation.Text = "Saturation:";
      // 
      // labelHue
      // 
      this.labelHue.Location = new System.Drawing.Point(13, 232);
      this.labelHue.Name = "labelHue";
      this.labelHue.Size = new System.Drawing.Size(32, 16);
      this.labelHue.TabIndex = 6;
      this.labelHue.Text = "Hue:";
      // 
      // labelContrast
      // 
      this.labelContrast.Location = new System.Drawing.Point(13, 192);
      this.labelContrast.Name = "labelContrast";
      this.labelContrast.Size = new System.Drawing.Size(56, 16);
      this.labelContrast.TabIndex = 4;
      this.labelContrast.Text = "Contrast:";
      // 
      // labelBrightness
      // 
      this.labelBrightness.Location = new System.Drawing.Point(13, 152);
      this.labelBrightness.Name = "labelBrightness";
      this.labelBrightness.Size = new System.Drawing.Size(64, 16);
      this.labelBrightness.TabIndex = 2;
      this.labelBrightness.Text = "Brightness:";
      // 
      // comboBoxDeinterlace
      // 
      this.comboBoxDeinterlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDeinterlace.Items.AddRange(new object[] {
            "Auto",
            "Weave",
            "Blend",
            "Bob",
            "Field Shift"});
      this.comboBoxDeinterlace.Location = new System.Drawing.Point(131, 103);
      this.comboBoxDeinterlace.Name = "comboBoxDeinterlace";
      this.comboBoxDeinterlace.Size = new System.Drawing.Size(322, 21);
      this.comboBoxDeinterlace.TabIndex = 11;
      // 
      // labelDeinterlaceMethod
      // 
      this.labelDeinterlaceMethod.Location = new System.Drawing.Point(13, 106);
      this.labelDeinterlaceMethod.Name = "labelDeinterlaceMethod";
      this.labelDeinterlaceMethod.Size = new System.Drawing.Size(112, 16);
      this.labelDeinterlaceMethod.TabIndex = 10;
      this.labelDeinterlaceMethod.Text = "Deinterlace method:";
      // 
      // checkBoxForcedSubtitles
      // 
      this.checkBoxForcedSubtitles.AutoSize = true;
      this.checkBoxForcedSubtitles.Checked = true;
      this.checkBoxForcedSubtitles.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxForcedSubtitles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxForcedSubtitles.Location = new System.Drawing.Point(16, 47);
      this.checkBoxForcedSubtitles.Name = "checkBoxForcedSubtitles";
      this.checkBoxForcedSubtitles.Size = new System.Drawing.Size(166, 17);
      this.checkBoxForcedSubtitles.TabIndex = 1;
      this.checkBoxForcedSubtitles.Text = "Always display forced subtitles";
      this.checkBoxForcedSubtitles.UseVisualStyleBackColor = true;
      // 
      // checkBoxPlanar
      // 
      this.checkBoxPlanar.AutoSize = true;
      this.checkBoxPlanar.Checked = true;
      this.checkBoxPlanar.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxPlanar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPlanar.Location = new System.Drawing.Point(16, 24);
      this.checkBoxPlanar.Name = "checkBoxPlanar";
      this.checkBoxPlanar.Size = new System.Drawing.Size(266, 17);
      this.checkBoxPlanar.TabIndex = 0;
      this.checkBoxPlanar.Text = "Enable planar YUV media types (YV12, I420, IYUV)";
      this.checkBoxPlanar.UseVisualStyleBackColor = true;
      // 
      // trackBarSaturation
      // 
      this.trackBarSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarSaturation.LargeChange = 10;
      this.trackBarSaturation.Location = new System.Drawing.Point(131, 268);
      this.trackBarSaturation.Maximum = 200;
      this.trackBarSaturation.Name = "trackBarSaturation";
      this.trackBarSaturation.Size = new System.Drawing.Size(330, 45);
      this.trackBarSaturation.TabIndex = 9;
      this.trackBarSaturation.TickFrequency = 25;
      this.trackBarSaturation.Value = 100;
      // 
      // trackBarHue
      // 
      this.trackBarHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarHue.LargeChange = 15;
      this.trackBarHue.Location = new System.Drawing.Point(131, 228);
      this.trackBarHue.Maximum = 360;
      this.trackBarHue.Name = "trackBarHue";
      this.trackBarHue.Size = new System.Drawing.Size(330, 45);
      this.trackBarHue.TabIndex = 7;
      this.trackBarHue.TickFrequency = 45;
      this.trackBarHue.Value = 180;
      // 
      // trackBarContrast
      // 
      this.trackBarContrast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarContrast.LargeChange = 10;
      this.trackBarContrast.Location = new System.Drawing.Point(131, 188);
      this.trackBarContrast.Maximum = 200;
      this.trackBarContrast.Name = "trackBarContrast";
      this.trackBarContrast.Size = new System.Drawing.Size(330, 45);
      this.trackBarContrast.TabIndex = 5;
      this.trackBarContrast.TickFrequency = 25;
      this.trackBarContrast.Value = 100;
      // 
      // trackBarBrightness
      // 
      this.trackBarBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarBrightness.LargeChange = 16;
      this.trackBarBrightness.Location = new System.Drawing.Point(131, 148);
      this.trackBarBrightness.Maximum = 256;
      this.trackBarBrightness.Name = "trackBarBrightness";
      this.trackBarBrightness.Size = new System.Drawing.Size(330, 45);
      this.trackBarBrightness.TabIndex = 3;
      this.trackBarBrightness.TickFrequency = 32;
      this.trackBarBrightness.Value = 128;
      // 
      // MPEG2DecVideoFilter
      // 
      this.Controls.Add(this.groupBoxSettings);
      this.Name = "MPEG2DecVideoFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarSaturation)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarHue)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarContrast)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    byte[] ConvertFromRegisty(int regValue)
    {
      byte[] tempArray = new byte[4];
      for (int i = 0; i < 4; i++)
        tempArray[i] = (byte)((regValue & (0xFF << 8 * i)) >> 8 * i);
      return tempArray;
    }

    public override void LoadSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Gabest\Filters\MPEG Video Decoder"))
        if (subkey != null)
        {
          try
          {
            if ((int)subkey.GetValue("PlanarYUV") == 1)
              checkBoxPlanar.Checked = true;
            else
              checkBoxPlanar.Checked = false;

            if ((int)subkey.GetValue("ForcedSubtitles") == 1)
              checkBoxForcedSubtitles.Checked = true;
            else
              checkBoxForcedSubtitles.Checked = false;

            if ((int)subkey.GetValue("Interlaced") == 1)
              checkBoxOutputInterlaced.Checked = true;
            else
              checkBoxOutputInterlaced.Checked = false;

            comboBoxDeinterlace.SelectedIndex = (int)subkey.GetValue("DeinterlaceMethod");

            byte[] convertedKey;

            convertedKey = ConvertFromRegisty((int)subkey.GetValue("Brightness"));
            trackBarBrightness.Value = (int)((BitConverter.ToSingle(convertedKey, 0)) + ((BitConverter.ToSingle(convertedKey, 0)) >= 0 ? 0.5f : -0.5f)) + 128;

            convertedKey = ConvertFromRegisty((int)subkey.GetValue("Contrast"));
            trackBarContrast.Value = (int)((100 * BitConverter.ToSingle(convertedKey, 0) + 0.5f));

            convertedKey = ConvertFromRegisty((int)subkey.GetValue("Hue"));
            trackBarHue.Value = (int)((BitConverter.ToSingle(convertedKey, 0)) + ((BitConverter.ToSingle(convertedKey, 0)) >= 0 ? 0.5f : -0.5f)) + 180;

            convertedKey = ConvertFromRegisty((int)subkey.GetValue("Saturation"));
            trackBarSaturation.Value = (int)((100 * BitConverter.ToSingle(convertedKey, 0) + 0.5f));
          }
          catch (Exception ex)
          {
            Log.Info("Exception while loading MPV settings: {0}", ex.Message);
          }
        }
        else
          Log.Info("Registry Key not found: {0}", "Software\\Gabest\\Filters\\MPEG Video Decoder", true);
    }

    public override void SaveSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Gabest\Filters\MPEG Video Decoder"))
        if (subkey != null)
        {
          int regValue;

          if (checkBoxPlanar.Checked)
            regValue = 1;
          else
            regValue = 0;
          subkey.SetValue("PlanarYUV", regValue);

          if (checkBoxForcedSubtitles.Checked)
            regValue = 1;
          else
            regValue = 0;
          subkey.SetValue("ForcedSubtitles", regValue);

          if (checkBoxOutputInterlaced.Checked)
            regValue = 1;
          else
            regValue = 0;
          subkey.SetValue("Interlaced", regValue);

          subkey.SetValue("DeinterlaceMethod", (int)comboBoxDeinterlace.SelectedIndex);

          float brightness = Convert.ToSingle((UInt32)(trackBarBrightness.Value));
          brightness = (brightness - (brightness >= 0 ? 0.5f : -0.5f)) - 127.5f;
          subkey.SetValue("Brightness", BitConverter.ToInt32(BitConverter.GetBytes(brightness), 0), RegistryValueKind.DWord);

          float contrast = Convert.ToSingle((UInt32)(trackBarContrast.Value));
          contrast = contrast / 100;
          subkey.SetValue("Contrast", BitConverter.ToInt32(BitConverter.GetBytes(contrast), 0), RegistryValueKind.DWord);

          float hue = Convert.ToSingle((UInt32)(trackBarHue.Value));
          hue = (hue - (hue >= 0 ? 0.5f : -0.5f)) - 179.5f;
          subkey.SetValue("Hue", BitConverter.ToInt32(BitConverter.GetBytes(hue), 0), RegistryValueKind.DWord);

          float saturation = Convert.ToSingle((UInt32)(trackBarSaturation.Value));
          saturation = saturation / 100;
          subkey.SetValue("Saturation", BitConverter.ToInt32(BitConverter.GetBytes(saturation), 0), RegistryValueKind.DWord);

          /// Floats are dumped to the registry by taking their address and moving 4 bytes into it (unsigned long conversion)
          /// see ~CMpeg2DecFilter()

          //regValue = 1;
          //byte[] arBitshift = new byte[4];
          //arBitshift[0] = regValue & FF;
          //arBitshift[1] = (regValue >> 8) & FF;
          //arBitshift[2] = (regValue >> 16) & FF;
          //arBitshift[3] = (regValue >> 24) & FF;
          //subkey.SetValue("Contrast", BitConverter.GetBytes(arBitshift), RegistryValueKind.DWord);
        }
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      trackBarBrightness.Value = 128;
      trackBarContrast.Value = 100;
      trackBarHue.Value = 180;
      trackBarSaturation.Value = 100;
      checkBoxPlanar.Checked = true;
      comboBoxDeinterlace.SelectedIndex = 0;
      checkBoxOutputInterlaced.Checked = false;
    }

    private void buttonTvDefaults_Click(object sender, EventArgs e)
    {
      trackBarBrightness.Value = 112;
      trackBarContrast.Value = 116;
      comboBoxDeinterlace.SelectedIndex = 3;
      checkBoxOutputInterlaced.Checked = false;
    }

    private void checkBoxOutputInterlaced_CheckedChanged(object sender, EventArgs e)
    {
      comboBoxDeinterlace.Enabled = !checkBoxOutputInterlaced.Checked;
    }

  }
}