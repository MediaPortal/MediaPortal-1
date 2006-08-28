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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class Volume : MediaPortal.Configuration.SectionSettings
  {
    #region Constructors

    public Volume()
      : base("Volume Settings")
    {
      InitializeComponent();
    }

    #endregion Constructors

    #region Methods

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
        components.Dispose();

      base.Dispose(disposing);
    }

    public override void LoadSettings()
    {
      // default default
      _useClassicHandler.Checked = true;

      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        int volumeStyle = reader.GetValueAsInt("volume", "handler", 0);

        _useClassicHandler.Checked = volumeStyle == 0;
        _useWindowsHandler.Checked = volumeStyle == 1;
        _useLogarithmicHandler.Checked = volumeStyle == 2;
        _useCustomHandler.Checked = volumeStyle == 3;
        _customText = reader.GetValueAsString("volume", "table", string.Empty);

        int startupStyle = reader.GetValueAsInt("volume", "startupstyle", 0);

        _useLastKnownLevel.Checked = startupStyle == 0;
        _useSystemCurrent.Checked = startupStyle == 1;
        _useCustomLevel.Checked = startupStyle == 2;
        _customLevel = reader.GetValueAsInt("volume", "startuplevel", 52428);

        bool isDigital = reader.GetValueAsBool("volume", "digital", false);
        _useMasterVolume.Checked = !isDigital;
        _useWave.Checked = isDigital;
      }

      if (_customText == string.Empty)
        _customText = "0, 1039, 1234, 1467, 1744, 2072, 2463,  2927,  3479,  4135,  4914,  5841, 6942,  8250,  9806, 11654, 13851, 16462, 19565, 23253, 27636, 32845, 39037, 46395, 55141, 65535";

      _customTextbox.Enabled = _useCustomHandler.Checked;
      _customTextbox.Text = _customTextbox.Enabled ? _customText : string.Empty;

      _levelTextbox.Enabled = _useCustomLevel.Checked;
      _levelTextbox.Text = _levelTextbox.Enabled ? _customLevel.ToString() : string.Empty;
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        if (_useClassicHandler.Checked)
          writer.SetValue("volume", "handler", 0);
        else if (_useWindowsHandler.Checked)
          writer.SetValue("volume", "handler", 1);
        else if (_useLogarithmicHandler.Checked)
          writer.SetValue("volume", "handler", 2);
        else if (_useCustomHandler.Checked)
          writer.SetValue("volume", "handler", 3);

        if (_useLastKnownLevel.Checked)
          writer.SetValue("volume", "startupstyle", 0);
        else if (_useSystemCurrent.Checked)
          writer.SetValue("volume", "startupstyle", 1);
        else if (_useCustomLevel.Checked)
          writer.SetValue("volume", "startupstyle", 2);

        writer.SetValue("volume", "digital", _useWave.Checked ? "yes" : "no");
        writer.SetValue("volume", "table", _customText);
        writer.SetValue("volume", "startuplevel", _customLevel);
      }
    }

    void OnCheckChanged(object sender, System.EventArgs e)
    {
      _customTextbox.Enabled = sender == _useCustomHandler;
      _customTextbox.Text = _customTextbox.Enabled ? _customText : string.Empty;

      _levelTextbox.Enabled = sender == _useCustomLevel;
      _levelTextbox.Text = _levelTextbox.Enabled ? _customLevel.ToString() : string.Empty;
    }

    void OnValidateCustomLevel(object sender, System.ComponentModel.CancelEventArgs e)
    {
      try
      {
        string valueText = ((TextBox)sender).Text;

        int percentIndex = valueText.LastIndexOf('%');

        if (percentIndex != -1)
          valueText = valueText.Substring(0, percentIndex);

        _customLevel = Math.Max(0, Math.Min(65535, int.Parse(valueText)));

        if (percentIndex != -1)
        {
          _customLevel = Math.Max(0, Math.Min(65535, (int)(((float)_customLevel * 65535) / 100)));
          _levelTextbox.Text = _customLevel.ToString();
        }
      }
      catch (Exception ex)
      {
        if ((ex is FormatException || ex is OverflowException) == false)
          throw;

        e.Cancel = true;
      }
    }

    void OnValidateCustomTable(object sender, System.ComponentModel.CancelEventArgs e)
    {
      try
      {
        StringBuilder builder = new StringBuilder();

        ArrayList valueArray = new ArrayList();

        foreach (string token in ((TextBox)sender).Text.Split(new char[] { ',', ';', ' ' }))
        {
          if (token == string.Empty)
            continue;

          // for now we're happy so long as the token can be converted to integer
          valueArray.Add(Math.Max(0, Math.Min(65535, Convert.ToInt32(token))));
        }

        valueArray.Sort();

        // rebuild a fully formatted string to represent the volume table
        foreach (int volume in valueArray)
        {
          if (builder.Length != 0)
            builder.Append(", ");

          builder.Append(volume.ToString());
        }

        if (valueArray.Count < 2)
          e.Cancel = true;

        _customTextbox.Text = builder.ToString();
        _customText = _customTextbox.Text;
      }
      catch (Exception ex)
      {
        if ((ex is FormatException || ex is OverflowException) == false)
          throw;

        e.Cancel = true;
      }
    }

    #endregion Methods

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
    this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this._customTextbox = new MediaPortal.UserInterface.Controls.MPTextBox();
    this._useCustomHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this._useLogarithmicHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this._useWindowsHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this._useClassicHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this._levelTextbox = new MediaPortal.UserInterface.Controls.MPTextBox();
    this._useCustomLevel = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this._useSystemCurrent = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this._useLastKnownLevel = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this._useWave = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this._useMasterVolume = new MediaPortal.UserInterface.Controls.MPRadioButton();
    this.groupBox1.SuspendLayout();
    this.groupBox2.SuspendLayout();
    this.groupBox3.SuspendLayout();
    this.SuspendLayout();
    // 
    // groupBox1
    // 
    this.groupBox1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.groupBox1.Controls.Add(this._customTextbox);
    this.groupBox1.Controls.Add(this._useCustomHandler);
    this.groupBox1.Controls.Add(this._useLogarithmicHandler);
    this.groupBox1.Controls.Add(this._useWindowsHandler);
    this.groupBox1.Controls.Add(this._useClassicHandler);
    this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.groupBox1.Location = new System.Drawing.Point(0, 113);
    this.groupBox1.Name = "groupBox1";
    this.groupBox1.Size = new System.Drawing.Size(472, 128);
    this.groupBox1.TabIndex = 0;
    this.groupBox1.TabStop = false;
    this.groupBox1.Text = "Style";
    // 
    // _customTextbox
    // 
    this._customTextbox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this._customTextbox.Enabled = false;
    this._customTextbox.Location = new System.Drawing.Point(168, 92);
    this._customTextbox.Name = "_customTextbox";
    this._customTextbox.Size = new System.Drawing.Size(288, 20);
    this._customTextbox.TabIndex = 4;
    this._customTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidateCustomTable);
    // 
    // _useCustomHandler
    // 
    this._useCustomHandler.AutoSize = true;
    this._useCustomHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useCustomHandler.Location = new System.Drawing.Point(16, 96);
    this._useCustomHandler.Name = "_useCustomHandler";
    this._useCustomHandler.Size = new System.Drawing.Size(62, 17);
    this._useCustomHandler.TabIndex = 3;
    this._useCustomHandler.Text = "C&ustom:";
    this._useCustomHandler.UseVisualStyleBackColor = true;
    this._useCustomHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // _useLogarithmicHandler
    // 
    this._useLogarithmicHandler.AutoSize = true;
    this._useLogarithmicHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useLogarithmicHandler.Location = new System.Drawing.Point(16, 72);
    this._useLogarithmicHandler.Name = "_useLogarithmicHandler";
    this._useLogarithmicHandler.Size = new System.Drawing.Size(78, 17);
    this._useLogarithmicHandler.TabIndex = 2;
    this._useLogarithmicHandler.Text = "&Logarithmic";
    this._useLogarithmicHandler.UseVisualStyleBackColor = true;
    this._useLogarithmicHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // _useWindowsHandler
    // 
    this._useWindowsHandler.AutoSize = true;
    this._useWindowsHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useWindowsHandler.Location = new System.Drawing.Point(16, 24);
    this._useWindowsHandler.Name = "_useWindowsHandler";
    this._useWindowsHandler.Size = new System.Drawing.Size(103, 17);
    this._useWindowsHandler.TabIndex = 0;
    this._useWindowsHandler.Text = "&Windows default";
    this._useWindowsHandler.UseVisualStyleBackColor = true;
    this._useWindowsHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // _useClassicHandler
    // 
    this._useClassicHandler.AutoSize = true;
    this._useClassicHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useClassicHandler.Location = new System.Drawing.Point(16, 48);
    this._useClassicHandler.Name = "_useClassicHandler";
    this._useClassicHandler.Size = new System.Drawing.Size(57, 17);
    this._useClassicHandler.TabIndex = 1;
    this._useClassicHandler.Text = "&Classic";
    this._useClassicHandler.UseVisualStyleBackColor = true;
    this._useClassicHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // groupBox2
    // 
    this.groupBox2.Controls.Add(this._levelTextbox);
    this.groupBox2.Controls.Add(this._useCustomLevel);
    this.groupBox2.Controls.Add(this._useSystemCurrent);
    this.groupBox2.Controls.Add(this._useLastKnownLevel);
    this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.groupBox2.Location = new System.Drawing.Point(0, 0);
    this.groupBox2.Name = "groupBox2";
    this.groupBox2.Size = new System.Drawing.Size(472, 107);
    this.groupBox2.TabIndex = 1;
    this.groupBox2.TabStop = false;
    this.groupBox2.Text = "Startup";
    // 
    // _levelTextbox
    // 
    this._levelTextbox.Enabled = false;
    this._levelTextbox.Location = new System.Drawing.Point(168, 72);
    this._levelTextbox.Name = "_levelTextbox";
    this._levelTextbox.Size = new System.Drawing.Size(288, 20);
    this._levelTextbox.TabIndex = 3;
    this._levelTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidateCustomLevel);
    // 
    // _useCustomLevel
    // 
    this._useCustomLevel.AutoSize = true;
    this._useCustomLevel.Enabled = false;
    this._useCustomLevel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useCustomLevel.Location = new System.Drawing.Point(16, 72);
    this._useCustomLevel.Name = "_useCustomLevel";
    this._useCustomLevel.Size = new System.Drawing.Size(59, 17);
    this._useCustomLevel.TabIndex = 2;
    this._useCustomLevel.Text = "Custom";
    this._useCustomLevel.UseVisualStyleBackColor = true;
    this._useCustomLevel.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // _useSystemCurrent
    // 
    this._useSystemCurrent.AutoSize = true;
    this._useSystemCurrent.Enabled = false;
    this._useSystemCurrent.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useSystemCurrent.Location = new System.Drawing.Point(16, 48);
    this._useSystemCurrent.Name = "_useSystemCurrent";
    this._useSystemCurrent.Size = new System.Drawing.Size(194, 17);
    this._useSystemCurrent.TabIndex = 1;
    this._useSystemCurrent.Text = "Use the current system volume level";
    this._useSystemCurrent.UseVisualStyleBackColor = true;
    this._useSystemCurrent.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // _useLastKnownLevel
    // 
    this._useLastKnownLevel.AutoSize = true;
    this._useLastKnownLevel.Enabled = false;
    this._useLastKnownLevel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useLastKnownLevel.Location = new System.Drawing.Point(16, 24);
    this._useLastKnownLevel.Name = "_useLastKnownLevel";
    this._useLastKnownLevel.Size = new System.Drawing.Size(141, 17);
    this._useLastKnownLevel.TabIndex = 0;
    this._useLastKnownLevel.Text = "Last known volume level";
    this._useLastKnownLevel.UseVisualStyleBackColor = true;
    this._useLastKnownLevel.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
    // 
    // groupBox3
    // 
    this.groupBox3.Controls.Add(this._useWave);
    this.groupBox3.Controls.Add(this._useMasterVolume);
    this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.groupBox3.Location = new System.Drawing.Point(0, 247);
    this.groupBox3.Name = "groupBox3";
    this.groupBox3.Size = new System.Drawing.Size(472, 88);
    this.groupBox3.TabIndex = 2;
    this.groupBox3.TabStop = false;
    this.groupBox3.Text = "Control";
    // 
    // _useWave
    // 
    this._useWave.AutoSize = true;
    this._useWave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useWave.Location = new System.Drawing.Point(16, 56);
    this._useWave.Name = "_useWave";
    this._useWave.Size = new System.Drawing.Size(53, 17);
    this._useWave.TabIndex = 1;
    this._useWave.Text = "&Wave";
    this._useWave.UseVisualStyleBackColor = true;
    // 
    // _useMasterVolume
    // 
    this._useMasterVolume.AutoSize = true;
    this._useMasterVolume.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this._useMasterVolume.Location = new System.Drawing.Point(16, 24);
    this._useMasterVolume.Name = "_useMasterVolume";
    this._useMasterVolume.Size = new System.Drawing.Size(94, 17);
    this._useMasterVolume.TabIndex = 0;
    this._useMasterVolume.Text = "&Master Volume";
    this._useMasterVolume.UseVisualStyleBackColor = true;
    this._useMasterVolume.CheckedChanged += new System.EventHandler(this._useMasterVolume_CheckedChanged);
    // 
    // Volume
    // 
    this.Controls.Add(this.groupBox3);
    this.Controls.Add(this.groupBox2);
    this.Controls.Add(this.groupBox1);
    this.Name = "Volume";
    this.Size = new System.Drawing.Size(472, 408);
    this.groupBox1.ResumeLayout(false);
    this.groupBox1.PerformLayout();
    this.groupBox2.ResumeLayout(false);
    this.groupBox2.PerformLayout();
    this.groupBox3.ResumeLayout(false);
    this.groupBox3.PerformLayout();
    this.ResumeLayout(false);

    }
    #endregion

    #region Fields

    Container components = null;
    int _customLevel;
    string _customText = string.Empty;
    private MPTextBox _customTextbox;
    MPGroupBox groupBox1;
    MPGroupBox groupBox2;
    private MPTextBox _levelTextbox;
    private MPRadioButton _useClassicHandler;
    private MPRadioButton _useCustomHandler;
    private MPRadioButton _useCustomLevel;
    private MPRadioButton _useWindowsHandler;
    private MPRadioButton _useLastKnownLevel;
    private MPRadioButton _useLogarithmicHandler;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPRadioButton _useMasterVolume;
    private MediaPortal.UserInterface.Controls.MPRadioButton _useWave;

    #endregion Fields

    private MPRadioButton _useSystemCurrent;


    private void _useMasterVolume_CheckedChanged(object sender, System.EventArgs e)
    {


    }
  }
}
