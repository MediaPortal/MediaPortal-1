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
using Microsoft.Win32;

//using MediaPortal.GUI.Library;  no longer needed for logging

using System.Runtime.InteropServices;

using DShowNET;
using DirectShowLib;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{

  public class MPEG2DecVideoFilter : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPComboBox cbDeinterlace;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbForcedSubtitles;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbPlanar;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private System.Windows.Forms.TrackBar tbBrightness;
    private System.Windows.Forms.TrackBar tbContrast;
    private System.Windows.Forms.TrackBar tbHue;
    private System.Windows.Forms.TrackBar tbSaturation;
    private Label lblCNote;
    private Button btnReset;
    private Button button1;
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public MPEG2DecVideoFilter()
      : this("MPEG2Dec Video Decoder")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public MPEG2DecVideoFilter(string name)
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbForcedSubtitles = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbPlanar = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbSaturation = new System.Windows.Forms.TrackBar();
      this.tbHue = new System.Windows.Forms.TrackBar();
      this.tbContrast = new System.Windows.Forms.TrackBar();
      this.tbBrightness = new System.Windows.Forms.TrackBar();
      this.lblCNote = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.btnReset = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbHue)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.btnReset);
      this.groupBox1.Controls.Add(this.button1);
      this.groupBox1.Controls.Add(this.lblCNote);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.cbDeinterlace);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbForcedSubtitles);
      this.groupBox1.Controls.Add(this.cbPlanar);
      this.groupBox1.Controls.Add(this.tbSaturation);
      this.groupBox1.Controls.Add(this.tbHue);
      this.groupBox1.Controls.Add(this.tbContrast);
      this.groupBox1.Controls.Add(this.tbBrightness);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 405);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(13, 253);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 16);
      this.label8.TabIndex = 8;
      this.label8.Text = "Saturation:";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(13, 213);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(32, 16);
      this.label7.TabIndex = 6;
      this.label7.Text = "Hue:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(13, 173);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(56, 16);
      this.label6.TabIndex = 4;
      this.label6.Text = "Contrast:";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(13, 133);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 16);
      this.label4.TabIndex = 2;
      this.label4.Text = "Brightness:";
      // 
      // cbDeinterlace
      // 
      this.cbDeinterlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDeinterlace.Items.AddRange(new object[] {
            "Auto",
            "Weave",
            "Blend",
            "BOB",
            "Field Shift"});
      this.cbDeinterlace.Location = new System.Drawing.Point(165, 87);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(288, 21);
      this.cbDeinterlace.TabIndex = 11;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(13, 91);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(112, 16);
      this.label2.TabIndex = 10;
      this.label2.Text = "Deinterlace method:";
      // 
      // cbForcedSubtitles
      // 
      this.cbForcedSubtitles.AutoSize = true;
      this.cbForcedSubtitles.Checked = true;
      this.cbForcedSubtitles.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbForcedSubtitles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbForcedSubtitles.Location = new System.Drawing.Point(16, 48);
      this.cbForcedSubtitles.Name = "cbForcedSubtitles";
      this.cbForcedSubtitles.Size = new System.Drawing.Size(166, 17);
      this.cbForcedSubtitles.TabIndex = 1;
      this.cbForcedSubtitles.Text = "Always display forced subtitles";
      this.cbForcedSubtitles.UseVisualStyleBackColor = true;
      // 
      // cbPlanar
      // 
      this.cbPlanar.AutoSize = true;
      this.cbPlanar.Checked = true;
      this.cbPlanar.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbPlanar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbPlanar.Location = new System.Drawing.Point(16, 24);
      this.cbPlanar.Name = "cbPlanar";
      this.cbPlanar.Size = new System.Drawing.Size(266, 17);
      this.cbPlanar.TabIndex = 0;
      this.cbPlanar.Text = "Enable planar YUV media types (YV12, I420, IYUV)";
      this.cbPlanar.UseVisualStyleBackColor = true;
      // 
      // tbSaturation
      // 
      this.tbSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbSaturation.LargeChange = 10;
      this.tbSaturation.Location = new System.Drawing.Point(157, 249);
      this.tbSaturation.Maximum = 200;
      this.tbSaturation.Name = "tbSaturation";
      this.tbSaturation.Size = new System.Drawing.Size(304, 48);
      this.tbSaturation.TabIndex = 9;
      this.tbSaturation.TickFrequency = 20;
      this.tbSaturation.Value = 100;
      // 
      // tbHue
      // 
      this.tbHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbHue.LargeChange = 16;
      this.tbHue.Location = new System.Drawing.Point(157, 209);
      this.tbHue.Maximum = 360;
      this.tbHue.Name = "tbHue";
      this.tbHue.Size = new System.Drawing.Size(304, 48);
      this.tbHue.TabIndex = 7;
      this.tbHue.TickFrequency = 30;
      this.tbHue.Value = 180;
      // 
      // tbContrast
      // 
      this.tbContrast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbContrast.LargeChange = 10;
      this.tbContrast.Location = new System.Drawing.Point(157, 169);
      this.tbContrast.Maximum = 200;
      this.tbContrast.Name = "tbContrast";
      this.tbContrast.Size = new System.Drawing.Size(304, 48);
      this.tbContrast.TabIndex = 5;
      this.tbContrast.TickFrequency = 20;
      this.tbContrast.Value = 100;
      // 
      // tbBrightness
      // 
      this.tbBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbBrightness.LargeChange = 16;
      this.tbBrightness.Location = new System.Drawing.Point(157, 129);
      this.tbBrightness.Maximum = 255;
      this.tbBrightness.Name = "tbBrightness";
      this.tbBrightness.Size = new System.Drawing.Size(304, 48);
      this.tbBrightness.TabIndex = 3;
      this.tbBrightness.TickFrequency = 16;
      this.tbBrightness.Value = 128;
      // 
      // lblCNote
      // 
      this.lblCNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCNote.Location = new System.Drawing.Point(13, 362);
      this.lblCNote.Name = "lblCNote";
      this.lblCNote.Size = new System.Drawing.Size(440, 40);
      this.lblCNote.TabIndex = 12;
      this.lblCNote.Text = "Note: Using a non-planar output format, bob deinterlacer, or adjusting color prop" +
          "eries may degrade performance. \"Auto\" deinterlacer will switch to \"Blend\" if nec" +
          "essary.";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(165, 303);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 13;
      this.button1.Text = "PC -> TV";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // btnReset
      // 
      this.btnReset.Location = new System.Drawing.Point(272, 303);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(75, 23);
      this.btnReset.TabIndex = 14;
      this.btnReset.Text = "Reset";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // MPEG2DecVideoFilter
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "MPEG2DecVideoFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbHue)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    public override void LoadSettings()
    {
      RegistryKey hkcu = Registry.CurrentUser;
      RegistryKey subkey = hkcu.OpenSubKey(@"Software\Gabest\Filters\MPEG Video Decoder");
      if (subkey != null)
      {
        try
        {
          Int32 regValue = (Int32)subkey.GetValue("PlanarYUV");
          if (regValue == 1)
            cbPlanar.Checked = true;
          else
            cbPlanar.Checked = false;

          regValue = (Int32)subkey.GetValue("ForcedSubtitles");
          if (regValue == 1)
            cbForcedSubtitles.Checked = true;
          else
            cbForcedSubtitles.Checked = false;


          regValue = (Int32)subkey.GetValue("DeinterlaceMethod");
          cbDeinterlace.SelectedIndex = regValue;
          ///
          /// needs some cleanup here - one single function could do..
          /// 
          regValue = (Int32)subkey.GetValue("Brightness");
          byte[] foo = new byte[4];
          for (int i = 0; i < 4; i++)
            foo[i] = (byte)((regValue & (0xFF << 8 * i)) >> 8 * i);
          //Log.Write("DEBUG: Brightness - reg obj. value: {0}", BitConverter.ToInt32(foo, 0).ToString());
          ///(int)(m_procamp[0] + (m_procamp[0] >= 0 ? 0.5f : -0.5f)) + 128
          regValue = (int)((BitConverter.ToSingle(foo, 0)) + ((BitConverter.ToSingle(foo, 0)) >= 0 ? 0.5f : -0.5f)) + 128;
          //Log.Write("DEBUG: Brightness - prepared Int value: {0}", regValue.ToString());
          tbBrightness.Value = regValue;

          regValue = (Int32)subkey.GetValue("Contrast");
          for (int i = 0; i < 4; i++)
            foo[i] = (byte)((regValue & (0xFF << 8 * i)) >> 8 * i);
          //Log.Write("DEBUG: Contrast - reg obj. value: {0}", BitConverter.ToInt32(foo, 0).ToString());
          /// m_procamp_slider[3].SetPos((int)(100*m_procamp[3] + 0.5f));
          regValue = (int)((100 * BitConverter.ToSingle(foo, 0) + 0.5f));
          //Log.Write("DEBUG: Contrast - prepared Int value: {0}", regValue.ToString());
          tbContrast.Value = regValue;

          regValue = (Int32)subkey.GetValue("Hue");
          for (int i = 0; i < 4; i++)
            foo[i] = (byte)((regValue & (0xFF << 8 * i)) >> 8 * i);
          //Log.Write("DEBUG: Hue - reg obj. value: {0}", BitConverter.ToInt32(foo, 0).ToString());
          ///(int)(m_procamp[0] + (m_procamp[0] >= 0 ? 0.5f : -0.5f)) + 180
          regValue = (int)((BitConverter.ToSingle(foo, 0)) + ((BitConverter.ToSingle(foo, 0)) >= 0 ? 0.5f : -0.5f)) + 180;
          //Log.Write("DEBUG: Hue - prepared Int value: {0}", regValue.ToString());
          //byte[] arBitshift = new byte[4];
          //for (int i = 0; i < 32; i++)
          //{
          //    arBitshift[i % 8] += (byte)(((UInt32)((regValue >> i) & 1)) << i);
          //}
          tbHue.Value = regValue;

          regValue = (Int32)subkey.GetValue("Saturation");
          for (int i = 0; i < 4; i++)
            foo[i] = (byte)((regValue & (0xFF << 8 * i)) >> 8 * i);
          //Log.Write("DEBUG: Saturation - reg obj. value: {0}", BitConverter.ToInt32(foo, 0).ToString());
          regValue = (int)((100 * BitConverter.ToSingle(foo, 0) + 0.5f));
          //Log.Write("DEBUG: Saturation - prepared Int value: {0}", regValue.ToString());
          tbSaturation.Value = regValue;
        }
        catch (Exception e)
        {
          MediaPortal.GUI.Library.Log.Write("Exception while loading MPV settings: {0}", e.Message);
          //throw e;
        }
        finally
        {
          subkey.Close();
        }
      }
      else
        MediaPortal.GUI.Library.Log.Write("Registry Key not found: {0}", "Software\\Gabest\\Filters\\MPEG Video Decoder", true);
    }
    public override void SaveSettings()
    {
      RegistryKey hkcu = Registry.CurrentUser;
      RegistryKey subkey = hkcu.CreateSubKey(@"Software\Gabest\Filters\MPEG Video Decoder");

      if (subkey != null)
      {
        Int32 regValue;
        if (cbPlanar.Checked)
          regValue = 1;
        else
          regValue = 0;
        subkey.SetValue("PlanarYUV", regValue);

        if (cbForcedSubtitles.Checked)
          regValue = 1;
        else
          regValue = 0;
        subkey.SetValue("ForcedSubtitles", regValue);

        subkey.SetValue("DeinterlaceMethod", (Int32)cbDeinterlace.SelectedIndex);

        /// Floats are dumped to the registry by taking their address and moving 4 bytes into it (unsigned long conversion)
        /// see ~CMpeg2DecFilter()

        //regValue = 1;
        //byte[] arBitshift = new byte[4];
        //arBitshift[0] = regValue & FF;
        //arBitshift[1] = (regValue >> 8) & FF;
        //arBitshift[2] = (regValue >> 16) & FF;
        //arBitshift[3] = (regValue >> 24) & FF;
        //subkey.SetValue("Contrast", BitConverter.GetBytes(arBitshift), RegistryValueKind.DWord);

        UInt32 valueInt = (UInt32)(tbBrightness.Value);
        float valueFloat = Convert.ToSingle(valueInt);
        valueFloat = (valueFloat - (valueFloat >= 0 ? 0.5f : -0.5f)) - 128;
        //Log.Write("DEBUG: Write_Brightness - calc value: {0}", valueFloat.ToString());
        subkey.SetValue("Brightness", BitConverter.ToInt32(BitConverter.GetBytes(valueFloat), 0), RegistryValueKind.DWord);

        valueInt = (UInt32)(tbContrast.Value);
        valueFloat = Convert.ToSingle(valueInt);
        valueFloat = valueFloat / 100; //- 0.5f;
        //Log.Write("DEBUG: Write_Contrast - calc value: {0}", valueFloat.ToString());
        subkey.SetValue("Contrast", BitConverter.ToInt32(BitConverter.GetBytes(valueFloat), 0), RegistryValueKind.DWord);

        valueInt = (UInt32)(tbHue.Value);
        valueFloat = Convert.ToSingle(valueInt);
        valueFloat = (valueFloat - (valueFloat >= 0 ? 0.5f : -0.5f)) - 180;
        //Log.Write("DEBUG: Write_Hue - calc value: {0}", valueFloat.ToString());
        subkey.SetValue("Hue", BitConverter.ToInt32(BitConverter.GetBytes(valueFloat), 0), RegistryValueKind.DWord);

        valueInt = (UInt32)(tbSaturation.Value);
        valueFloat = Convert.ToSingle(valueInt);
        valueFloat = valueFloat / 100; //- 0.5f;
        //Log.Write("DEBUG: Write_Saturation - calc value: {0}", valueFloat.ToString());
        subkey.SetValue("Saturation", BitConverter.ToInt32(BitConverter.GetBytes(valueFloat), 0), RegistryValueKind.DWord);

        subkey.Close();
      }
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      tbBrightness.Value = 128;
      tbContrast.Value = 100;
      tbHue.Value = 180;
      tbSaturation.Value = 100;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      tbBrightness.Value = 112;
      tbContrast.Value = 116;
    }

  }
}