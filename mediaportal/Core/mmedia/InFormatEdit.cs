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
using System.Data;
using System.Windows.Forms;
using System.Globalization;
using WaveLib;

namespace Yeti.MMedia
{
  /// <summary>
  /// Summary description for EditFormat.
  /// </summary>
  public class EditFormat : System.Windows.Forms.UserControl, IEditFormat
  {
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxChannels;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxBitsPerSample;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox textBoxSampleRate;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.ComponentModel.IContainer components;
    private WaveFormat m_OrigFormat;
    private System.Windows.Forms.ErrorProvider errorProvider1;

    private bool m_FireConfigChangeEvent = true;

    public EditFormat()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
      this.Format = new WaveFormat(44100, 16, 2); //Set default values
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

    public bool ReadOnly
    {
      get
      {
        return textBoxSampleRate.ReadOnly;
      }
      set
      {
        textBoxSampleRate.ReadOnly = value;
        comboBoxBitsPerSample.Enabled = comboBoxChannels.Enabled = !value;
      }
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.comboBoxChannels = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxBitsPerSample = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxSampleRate = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
      this.SuspendLayout();
      // 
      // comboBoxChannels
      // 
      this.comboBoxChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxChannels.Items.AddRange(new object[] {
                                                          "MONO",
                                                          "STEREO"});
      this.comboBoxChannels.Location = new System.Drawing.Point(96, 56);
      this.comboBoxChannels.Name = "comboBoxChannels";
      this.comboBoxChannels.Size = new System.Drawing.Size(112, 21);
      this.comboBoxChannels.TabIndex = 13;
      this.comboBoxChannels.SelectedIndexChanged += new System.EventHandler(this.comboBoxChannels_SelectedIndexChanged);
      // 
      // comboBoxBitsPerSample
      // 
      this.comboBoxBitsPerSample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBitsPerSample.Items.AddRange(new object[] {
                                                               "8 bits per sample",
                                                               "16 bits per sample"});
      this.comboBoxBitsPerSample.Location = new System.Drawing.Point(96, 96);
      this.comboBoxBitsPerSample.Name = "comboBoxBitsPerSample";
      this.comboBoxBitsPerSample.Size = new System.Drawing.Size(112, 21);
      this.comboBoxBitsPerSample.TabIndex = 12;
      this.comboBoxBitsPerSample.SelectedIndexChanged += new System.EventHandler(this.comboBoxBitsPerSample_SelectedIndexChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 96);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 23);
      this.label3.TabIndex = 11;
      this.label3.Text = "Bits per sample:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 16);
      this.label2.TabIndex = 10;
      this.label2.Text = "Audio mode:";
      // 
      // textBoxSampleRate
      // 
      this.textBoxSampleRate.Location = new System.Drawing.Point(96, 16);
      this.textBoxSampleRate.Name = "textBoxSampleRate";
      this.textBoxSampleRate.Size = new System.Drawing.Size(112, 20);
      this.textBoxSampleRate.TabIndex = 8;
      this.textBoxSampleRate.Text = "44100";
      this.toolTip1.SetToolTip(this.textBoxSampleRate, "Sample rate, in samples per second. ");
      this.textBoxSampleRate.Value = 44100;
      this.textBoxSampleRate.FormatValid += new System.EventHandler(this.textBoxSampleRate_FormatValid);
      this.textBoxSampleRate.FormatError += new System.EventHandler(this.textBoxSampleRate_FormatError);
      this.textBoxSampleRate.TextChanged += new System.EventHandler(this.textBoxSampleRate_TextChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(72, 16);
      this.label1.TabIndex = 9;
      this.label1.Text = "Sample rate:";
      // 
      // errorProvider1
      // 
      this.errorProvider1.ContainerControl = this;
      // 
      // EditFormat
      // 
      this.Controls.Add(this.comboBoxChannels);
      this.Controls.Add(this.comboBoxBitsPerSample);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBoxSampleRate);
      this.Controls.Add(this.label1);
      this.Name = "EditFormat";
      this.Size = new System.Drawing.Size(288, 200);
      this.ResumeLayout(false);

    }

    #endregion

    #region IConfigControl Members

    public void DoApply()
    {
      // Nothing to do
    }

    public void DoSetInitialValues()
    {
      m_FireConfigChangeEvent = false;
      try
      {
        textBoxSampleRate.Text = m_OrigFormat.nSamplesPerSec.ToString();
        if (m_OrigFormat.wBitsPerSample == 8)
        {
          comboBoxBitsPerSample.SelectedIndex = 0;
        }
        else
        {
          comboBoxBitsPerSample.SelectedIndex = 1;
        }
        if (m_OrigFormat.nChannels == 1)
        {
          comboBoxChannels.SelectedIndex = 0;
        }
        else
        {
          comboBoxChannels.SelectedIndex = 1;
        }
      }
      finally
      {
        m_FireConfigChangeEvent = true;
      }
    }
    [Browsable(false)]
    public Control ConfigControl
    {
      get
      {
        return this;
      }
    }
    [Browsable(false)]
    public string ControlName
    {
      get
      {
        return "Input Format";
      }
    }

    public event System.EventHandler ConfigChange;

    #endregion

    #region IEditFormat members

    [Browsable(false)]
    public WaveFormat Format
    {
      get
      {
        int rate = int.Parse(textBoxSampleRate.Text, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite);
        int bits;
        int channels;
        if (comboBoxBitsPerSample.SelectedIndex == 0)
        {
          bits = 8;
          comboBoxBitsPerSample.SelectedIndex = 0;
        }
        else
        {
          bits = 16;
        }
        if (comboBoxChannels.SelectedIndex == 0)
        {
          channels = 1;
        }
        else
        {
          channels = 2;
        }
        return new WaveFormat(rate, bits, channels);
      }
      set
      {
        m_OrigFormat = value;
        DoSetInitialValues();
      }
    }

    #endregion

    private void OnConfigChange(System.EventArgs e)
    {
      if (m_FireConfigChangeEvent && (ConfigChange != null))
      {
        ConfigChange(this, e);
      }
    }

    private void textBoxSampleRate_TextChanged(object sender, System.EventArgs e)
    {
      // TODO: Validate text
      OnConfigChange(EventArgs.Empty);
    }

    private void comboBoxChannels_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      OnConfigChange(EventArgs.Empty);
    }

    private void comboBoxBitsPerSample_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      OnConfigChange(EventArgs.Empty);
    }

    private void textBoxSampleRate_FormatError(object sender, System.EventArgs e)
    {
      errorProvider1.SetError(textBoxSampleRate, "Number expected");
    }

    private void textBoxSampleRate_FormatValid(object sender, System.EventArgs e)
    {
      errorProvider1.SetError(textBoxSampleRate, "");
    }
  }
}
