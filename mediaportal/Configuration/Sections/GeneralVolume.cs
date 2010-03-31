#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralVolume : SectionSettings
  {
    #region Constructors

    public GeneralVolume()
      : base("Volume Settings")
    {
      InitializeComponent();
    }

    #endregion Constructors

    #region Methods

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
      {
        components.Dispose();
      }

      base.Dispose(disposing);
    }

    public override void OnSectionActivated()
    {
      groupBoxVolumeOsd.Visible = SettingsForm.AdvancedMode;
      // No code is using this setting currently
      groupBoxStartup.Visible = false;
      base.OnSectionActivated();
    }

    public override void LoadSettings()
    {
      // default default
      _useClassicHandler.Checked = true;

      using (Settings reader = new MPSettings())
      {
        int volumeStyle = reader.GetValueAsInt("volume", "handler", 1);
        bool isDigital = reader.GetValueAsBool("volume", "digital", true);

        // Force a couple of settings for Vista / Windows 7
        int ver = (OSInfo.OSInfo.OSMajorVersion * 10) + OSInfo.OSInfo.OSMinorVersion;
        if (ver >= 60)
        {
          volumeStyle = 4;
          groupBoxMixerControl.Enabled = false;
        }

        if (ver >= 61)
        {
          isDigital = true;
          groupBoxScale.Enabled = false;
        }

        _useClassicHandler.Checked = volumeStyle == 0;
        _useWindowsHandler.Checked = volumeStyle == 1;
        _useLogarithmicHandler.Checked = volumeStyle == 2;
        _useCustomHandler.Checked = volumeStyle == 3;
        _useVistaHandler.Checked = volumeStyle == 4;
        _customText = reader.GetValueAsString("volume", "table",
                                              "0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055, 49151, 53247, 57343, 61439, 65535");

        int startupStyle = reader.GetValueAsInt("volume", "startupstyle", 0);

        _useLastKnownLevel.Checked = startupStyle == 0;
        _useSystemCurrent.Checked = startupStyle == 1;
        _useCustomLevel.Checked = startupStyle == 2;
        _customLevel = reader.GetValueAsInt("volume", "startuplevel", 52428);

        // When Upmixing has selected, we need to use Master Volume
        if (reader.GetValueAsBool("Music", "mixing", false))
        {
          isDigital = true;
        }
        _useMasterVolume.Checked = !isDigital;
        _useWave.Checked = isDigital;

        _useVolumeOSD.Checked = reader.GetValueAsBool("volume", "defaultVolumeOSD", true);
      }

      _customTextbox.Enabled = _useCustomHandler.Checked;
      _customTextbox.Text = _customTextbox.Enabled ? _customText : string.Empty;

      _levelTextbox.Enabled = _useCustomLevel.Checked;
      _levelTextbox.Text = _levelTextbox.Enabled ? _customLevel.ToString() : string.Empty;
    }

    public override void SaveSettings()
    {
      using (Settings writer = new MPSettings())
      {
        if (_useClassicHandler.Checked)
        {
          writer.SetValue("volume", "handler", 0);
        }
        else if (_useWindowsHandler.Checked)
        {
          writer.SetValue("volume", "handler", 1);
        }
        else if (_useLogarithmicHandler.Checked)
        {
          writer.SetValue("volume", "handler", 2);
        }
        else if (_useCustomHandler.Checked)
        {
          writer.SetValue("volume", "handler", 3);
        }
        else if (_useVistaHandler.Checked)
        {
          writer.SetValue("volume", "handler", 4);
        }

        if (_useLastKnownLevel.Checked)
        {
          writer.SetValue("volume", "startupstyle", 0);
        }
        else if (_useSystemCurrent.Checked)
        {
          writer.SetValue("volume", "startupstyle", 1);
        }
        else if (_useCustomLevel.Checked)
        {
          writer.SetValue("volume", "startupstyle", 2);
        }

        writer.SetValueAsBool("volume", "digital", _useWave.Checked);
        writer.SetValue("volume", "table", _customText);
        writer.SetValue("volume", "startuplevel", _customLevel);
        writer.SetValueAsBool("volume", "defaultVolumeOSD", _useVolumeOSD.Checked);
      }
    }

    private void OnCheckChanged(object sender, EventArgs e)
    {
      _customTextbox.Enabled = sender == _useCustomHandler;
      _customTextbox.Text = _customTextbox.Enabled ? _customText : string.Empty;

      _levelTextbox.Enabled = sender == _useCustomLevel;
      _levelTextbox.Text = _levelTextbox.Enabled ? _customLevel.ToString() : string.Empty;
    }

    private void OnValidateCustomLevel(object sender, CancelEventArgs e)
    {
      try
      {
        string valueText = ((TextBox)sender).Text;

        int percentIndex = valueText.LastIndexOf('%');

        if (percentIndex != -1)
        {
          valueText = valueText.Substring(0, percentIndex);
        }

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
        {
          throw;
        }

        e.Cancel = true;
      }
    }

    private void OnValidateCustomTable(object sender, CancelEventArgs e)
    {
      try
      {
        StringBuilder builder = new StringBuilder();
        ArrayList valueArray = new ArrayList();

        foreach (string token in ((TextBox)sender).Text.Split(new char[] { ',', ';', ' ' }))
        {
          if (token == string.Empty)
          {
            continue;
          }

          // for now we're happy so long as the token can be converted to integer
          valueArray.Add(Math.Max(0, Math.Min(65535, Convert.ToInt32(token))));
        }

        valueArray.Sort();

        // rebuild a fully formatted string to represent the volume table
        foreach (int volume in valueArray)
        {
          if (builder.Length != 0)
          {
            builder.Append(", ");
          }

          builder.Append(volume.ToString());
        }

        if (valueArray.Count < 2)
        {
          e.Cancel = true;
        }

        _customTextbox.Text = builder.ToString();
        _customText = _customTextbox.Text;
      }
      catch (Exception ex)
      {
        if ((ex is FormatException || ex is OverflowException) == false)
        {
          throw;
        }

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
      this.groupBoxVolumeOsd = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._useVolumeOSD = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxMixerControl = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._useWave = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._useMasterVolume = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBoxStartup = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._levelTextbox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this._useCustomLevel = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._useSystemCurrent = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._useLastKnownLevel = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBoxScale = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._useVistaHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._customTextbox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this._useCustomHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._useLogarithmicHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._useWindowsHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this._useClassicHandler = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpLabelOs = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxVolumeOsd.SuspendLayout();
      this.groupBoxMixerControl.SuspendLayout();
      this.groupBoxStartup.SuspendLayout();
      this.groupBoxScale.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxVolumeOsd
      // 
      this.groupBoxVolumeOsd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxVolumeOsd.Controls.Add(this._useVolumeOSD);
      this.groupBoxVolumeOsd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxVolumeOsd.Location = new System.Drawing.Point(0, 257);
      this.groupBoxVolumeOsd.Name = "groupBoxVolumeOsd";
      this.groupBoxVolumeOsd.Size = new System.Drawing.Size(472, 49);
      this.groupBoxVolumeOsd.TabIndex = 3;
      this.groupBoxVolumeOsd.TabStop = false;
      this.groupBoxVolumeOsd.Text = "OSD";
      // 
      // _useVolumeOSD
      // 
      this._useVolumeOSD.AutoSize = true;
      this._useVolumeOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useVolumeOSD.Location = new System.Drawing.Point(16, 19);
      this._useVolumeOSD.Name = "_useVolumeOSD";
      this._useVolumeOSD.Size = new System.Drawing.Size(242, 17);
      this._useVolumeOSD.TabIndex = 0;
      this._useVolumeOSD.Text = "Show default Volume OSD for fullscreen video";
      this._useVolumeOSD.UseVisualStyleBackColor = true;
      // 
      // groupBoxMixerControl
      // 
      this.groupBoxMixerControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMixerControl.Controls.Add(this._useWave);
      this.groupBoxMixerControl.Controls.Add(this._useMasterVolume);
      this.groupBoxMixerControl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMixerControl.Location = new System.Drawing.Point(0, 25);
      this.groupBoxMixerControl.Name = "groupBoxMixerControl";
      this.groupBoxMixerControl.Size = new System.Drawing.Size(472, 75);
      this.groupBoxMixerControl.TabIndex = 2;
      this.groupBoxMixerControl.TabStop = false;
      this.groupBoxMixerControl.Text = "Control";
      // 
      // _useWave
      // 
      this._useWave.AutoSize = true;
      this._useWave.Checked = true;
      this._useWave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useWave.Location = new System.Drawing.Point(16, 47);
      this._useWave.Name = "_useWave";
      this._useWave.Size = new System.Drawing.Size(53, 17);
      this._useWave.TabIndex = 1;
      this._useWave.TabStop = true;
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
      // 
      // groupBoxStartup
      // 
      this.groupBoxStartup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxStartup.Controls.Add(this._levelTextbox);
      this.groupBoxStartup.Controls.Add(this._useCustomLevel);
      this.groupBoxStartup.Controls.Add(this._useSystemCurrent);
      this.groupBoxStartup.Controls.Add(this._useLastKnownLevel);
      this.groupBoxStartup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxStartup.Location = new System.Drawing.Point(0, 307);
      this.groupBoxStartup.Name = "groupBoxStartup";
      this.groupBoxStartup.Size = new System.Drawing.Size(472, 97);
      this.groupBoxStartup.TabIndex = 0;
      this.groupBoxStartup.TabStop = false;
      this.groupBoxStartup.Text = "Startup";
      // 
      // _levelTextbox
      // 
      this._levelTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._levelTextbox.BorderColor = System.Drawing.Color.Empty;
      this._levelTextbox.Enabled = false;
      this._levelTextbox.Location = new System.Drawing.Point(168, 69);
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
      this._useCustomLevel.Location = new System.Drawing.Point(16, 70);
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
      this._useSystemCurrent.Location = new System.Drawing.Point(16, 47);
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
      // groupBoxScale
      // 
      this.groupBoxScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxScale.Controls.Add(this._useVistaHandler);
      this.groupBoxScale.Controls.Add(this._customTextbox);
      this.groupBoxScale.Controls.Add(this._useCustomHandler);
      this.groupBoxScale.Controls.Add(this._useLogarithmicHandler);
      this.groupBoxScale.Controls.Add(this._useWindowsHandler);
      this.groupBoxScale.Controls.Add(this._useClassicHandler);
      this.groupBoxScale.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxScale.Location = new System.Drawing.Point(0, 103);
      this.groupBoxScale.Name = "groupBoxScale";
      this.groupBoxScale.Size = new System.Drawing.Size(472, 151);
      this.groupBoxScale.TabIndex = 1;
      this.groupBoxScale.TabStop = false;
      this.groupBoxScale.Text = "Scale";
      // 
      // _useVistaHandler
      // 
      this._useVistaHandler.AutoSize = true;
      this._useVistaHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useVistaHandler.Location = new System.Drawing.Point(16, 93);
      this._useVistaHandler.Name = "_useVistaHandler";
      this._useVistaHandler.Size = new System.Drawing.Size(133, 17);
      this._useVistaHandler.TabIndex = 3;
      this._useVistaHandler.Text = "Windows V&ista / Win 7";
      this._useVistaHandler.UseVisualStyleBackColor = true;
      this._useVistaHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
      // 
      // _customTextbox
      // 
      this._customTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._customTextbox.BorderColor = System.Drawing.Color.Empty;
      this._customTextbox.Enabled = false;
      this._customTextbox.Location = new System.Drawing.Point(168, 115);
      this._customTextbox.Name = "_customTextbox";
      this._customTextbox.Size = new System.Drawing.Size(288, 20);
      this._customTextbox.TabIndex = 5;
      this._customTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidateCustomTable);
      // 
      // _useCustomHandler
      // 
      this._useCustomHandler.AutoSize = true;
      this._useCustomHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useCustomHandler.Location = new System.Drawing.Point(16, 116);
      this._useCustomHandler.Name = "_useCustomHandler";
      this._useCustomHandler.Size = new System.Drawing.Size(62, 17);
      this._useCustomHandler.TabIndex = 4;
      this._useCustomHandler.Text = "C&ustom:";
      this._useCustomHandler.UseVisualStyleBackColor = true;
      this._useCustomHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
      // 
      // _useLogarithmicHandler
      // 
      this._useLogarithmicHandler.AutoSize = true;
      this._useLogarithmicHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useLogarithmicHandler.Location = new System.Drawing.Point(16, 70);
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
      this._useWindowsHandler.Checked = true;
      this._useWindowsHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useWindowsHandler.Location = new System.Drawing.Point(16, 24);
      this._useWindowsHandler.Name = "_useWindowsHandler";
      this._useWindowsHandler.Size = new System.Drawing.Size(173, 17);
      this._useWindowsHandler.TabIndex = 0;
      this._useWindowsHandler.TabStop = true;
      this._useWindowsHandler.Text = "&Windows XP (load from registry)";
      this._useWindowsHandler.UseVisualStyleBackColor = true;
      this._useWindowsHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
      // 
      // _useClassicHandler
      // 
      this._useClassicHandler.AutoSize = true;
      this._useClassicHandler.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._useClassicHandler.Location = new System.Drawing.Point(16, 47);
      this._useClassicHandler.Name = "_useClassicHandler";
      this._useClassicHandler.Size = new System.Drawing.Size(91, 17);
      this._useClassicHandler.TabIndex = 1;
      this._useClassicHandler.Text = "&Classic (linear)";
      this._useClassicHandler.UseVisualStyleBackColor = true;
      this._useClassicHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
      // 
      // mpLabelOs
      // 
      this.mpLabelOs.AutoSize = true;
      this.mpLabelOs.Location = new System.Drawing.Point(68, 7);
      this.mpLabelOs.Name = "mpLabelOs";
      this.mpLabelOs.Size = new System.Drawing.Size(301, 13);
      this.mpLabelOs.TabIndex = 4;
      this.mpLabelOs.Text = "Due to different OS behaviours, not all options will be available";
      // 
      // GeneralVolume
      // 
      this.Controls.Add(this.mpLabelOs);
      this.Controls.Add(this.groupBoxVolumeOsd);
      this.Controls.Add(this.groupBoxMixerControl);
      this.Controls.Add(this.groupBoxStartup);
      this.Controls.Add(this.groupBoxScale);
      this.Name = "GeneralVolume";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxVolumeOsd.ResumeLayout(false);
      this.groupBoxVolumeOsd.PerformLayout();
      this.groupBoxMixerControl.ResumeLayout(false);
      this.groupBoxMixerControl.PerformLayout();
      this.groupBoxStartup.ResumeLayout(false);
      this.groupBoxStartup.PerformLayout();
      this.groupBoxScale.ResumeLayout(false);
      this.groupBoxScale.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    #region Fields

    private Container components = null;
    private int _customLevel;
    private string _customText = string.Empty;
    private MPTextBox _customTextbox;
    private MPGroupBox groupBoxScale;
    private MPGroupBox groupBoxStartup;
    private MPTextBox _levelTextbox;
    private MPRadioButton _useClassicHandler;
    private MPRadioButton _useCustomHandler;
    private MPRadioButton _useCustomLevel;
    private MPRadioButton _useWindowsHandler;
    private MPRadioButton _useLastKnownLevel;
    private MPRadioButton _useLogarithmicHandler;
    private MPGroupBox groupBoxMixerControl;
    private MPRadioButton _useMasterVolume;
    private MPRadioButton _useWave;

    #endregion Fields

    private MPGroupBox groupBoxVolumeOsd;
    private MPCheckBox _useVolumeOSD;
    private MPRadioButton _useVistaHandler;
    private MPLabel mpLabelOs;

    private MPRadioButton _useSystemCurrent;
  }
}