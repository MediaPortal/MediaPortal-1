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
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersMonogramAACDecoder : SectionSettings
  {
    private MPComboBox comboBoxSpeakerOutput;
    private MPGroupBox mpGroupBox1;
    private MPLabel labelSpeakerOut;
    private MPLabel labelVolume;
    private TrackBar volumeTrackBar;
    private PictureBox pictureBox1;
    private MPGradientLabel mpGradientLabel1;
    private MPLabel mpLabel2;
    private MPLabel mpLabel1;
    private IContainer components = null;

    /// <summary>
    /// Sets Monogram AAC decoder output & volume settings
    /// </summary>
    public FiltersMonogramAACDecoder()
      : this("Monogram AAC Decoder Settings")
    {
    }

    /// <summary>
    /// Will add configuration child to section tree
    /// </summary>
    public FiltersMonogramAACDecoder(string name)
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
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (FiltersMonogramAACDecoder));
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.mpGradientLabel1 = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.volumeTrackBar = new System.Windows.Forms.TrackBar();
      this.labelVolume = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelSpeakerOut = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxSpeakerOutput = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.volumeTrackBar)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image) (resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(0, 2);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(472, 60);
      this.pictureBox1.TabIndex = 3;
      this.pictureBox1.TabStop = false;
      // 
      // mpGradientLabel1
      // 
      this.mpGradientLabel1.Caption = "";
      this.mpGradientLabel1.FirstColor = System.Drawing.Color.DarkSlateBlue;
      this.mpGradientLabel1.LastColor = System.Drawing.Color.White;
      this.mpGradientLabel1.Location = new System.Drawing.Point(0, 60);
      this.mpGradientLabel1.Name = "mpGradientLabel1";
      this.mpGradientLabel1.Size = new System.Drawing.Size(472, 8);
      this.mpGradientLabel1.TabIndex = 4;
      this.mpGradientLabel1.TextColor = System.Drawing.SystemColors.ControlText;
      this.mpGradientLabel1.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
                                                               System.Drawing.FontStyle.Regular,
                                                               System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.volumeTrackBar);
      this.mpGroupBox1.Controls.Add(this.labelVolume);
      this.mpGroupBox1.Controls.Add(this.labelSpeakerOut);
      this.mpGroupBox1.Controls.Add(this.comboBoxSpeakerOutput);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 79);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(458, 144);
      this.mpGroupBox1.TabIndex = 2;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "AAC Audio Decoder Settings";
      // 
      // volumeTrackBar
      // 
      this.volumeTrackBar.Location = new System.Drawing.Point(116, 93);
      this.volumeTrackBar.Minimum = -10;
      this.volumeTrackBar.Name = "volumeTrackBar";
      this.volumeTrackBar.Size = new System.Drawing.Size(290, 45);
      this.volumeTrackBar.TabIndex = 4;
      this.volumeTrackBar.ValueChanged += new System.EventHandler(this.volumeTrackBar_ValueChanged);
      // 
      // labelVolume
      // 
      this.labelVolume.AutoSize = true;
      this.labelVolume.Location = new System.Drawing.Point(19, 81);
      this.labelVolume.Name = "labelVolume";
      this.labelVolume.Size = new System.Drawing.Size(79, 13);
      this.labelVolume.TabIndex = 3;
      this.labelVolume.Text = "Volume: 0.0 dB";
      // 
      // labelSpeakerOut
      // 
      this.labelSpeakerOut.AutoSize = true;
      this.labelSpeakerOut.Location = new System.Drawing.Point(19, 28);
      this.labelSpeakerOut.Name = "labelSpeakerOut";
      this.labelSpeakerOut.Size = new System.Drawing.Size(132, 13);
      this.labelSpeakerOut.TabIndex = 2;
      this.labelSpeakerOut.Text = "Speaker Output Properties";
      // 
      // comboBoxSpeakerOutput
      // 
      this.comboBoxSpeakerOutput.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSpeakerOutput.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxSpeakerOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSpeakerOutput.Items.AddRange(new object[]
                                                  {
                                                    "No channel mixing",
                                                    "Mix to mono (1 channel)",
                                                    "Mix to stereo (2 channels)",
                                                    "Mix to 2.1 (3 channels)",
                                                    "Mix to quadrophonic (4 channels)",
                                                    "Mix to 5.1 (6 channels)"
                                                  });
      this.comboBoxSpeakerOutput.Location = new System.Drawing.Point(103, 50);
      this.comboBoxSpeakerOutput.Name = "comboBoxSpeakerOutput";
      this.comboBoxSpeakerOutput.Size = new System.Drawing.Size(316, 21);
      this.comboBoxSpeakerOutput.TabIndex = 1;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(74, 111);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(47, 13);
      this.mpLabel1.TabIndex = 5;
      this.mpLabel1.Text = "-10.0 dB";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(400, 111);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(44, 13);
      this.mpLabel2.TabIndex = 6;
      this.mpLabel2.Text = "10.0 dB";
      // 
      // MonogramAACDecoderFilter
      // 
      this.Controls.Add(this.mpGradientLabel1);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "MonogramAACDecoderFilter";
      this.Size = new System.Drawing.Size(472, 408);
      ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.volumeTrackBar)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// Loads the decoders settings
    /// </summary>
    public override void LoadSettings()
    {
      Int32 regSpeakerOut;
      Int32 regVolume;
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\MONOGRAM\MONOGRAM AAC Decoder"))
      {
        if (subkey != null)
        {
          try
          {
            regSpeakerOut = (Int32) subkey.GetValue("MixMode", 0);
            regVolume = (Int32) subkey.GetValue("Volume", 0);
            comboBoxSpeakerOutput.SelectedIndex = regSpeakerOut;
            volumeTrackBar.Value = regVolume/100;
          }
          catch (Exception)
          {
          }
        }
      }
    }

    public override void SaveSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\MONOGRAM\MONOGRAM AAC Decoder"))
      {
        if (subkey != null)
        {
          subkey.SetValue("MixMode", (Int32) comboBoxSpeakerOutput.SelectedIndex);
          subkey.SetValue("Volume", (Int32) volumeTrackBar.Value*100);
        }
      }
    }

    /// <summary>
    /// Saves the decoders settings.
    /// </summary>
    private void volumeTrackBar_ValueChanged(object sender, EventArgs e)
    {
      if (volumeTrackBar.Value > 0)
      {
        labelVolume.Text = String.Format(" Volume: {0:+#0.0} dB", volumeTrackBar.Value);
      }
      else
      {
        labelVolume.Text = String.Format(" Volume: {0:#0.0} dB", volumeTrackBar.Value);
      }
    }
  }
}