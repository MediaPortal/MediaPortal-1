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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Un4seen.BassAsio;

namespace MediaPortal.Configuration.Sections
{
  public partial class MusicASIO : SectionSettings
  {
    #region Constructors/Destructors

    public MusicASIO()
      : this("Music ASIO")
    {
    }

    public MusicASIO(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Private Methods

    private void useASIOCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      asioDeviceComboBox.Items.Clear();
      asioDeviceComboBox.Enabled = useASIOCheckBox.Checked;
      if (useASIOCheckBox.Checked)
      {
        // We must not have checked the "Upmixing" checkbox in the Music setting the same time
        SectionSettings section = GetSection("Music");

        if (section != null)
        {
          bool mixing = (bool) section.GetSetting("mixing");
          if (mixing)
          {
            useASIOCheckBox.Checked = false;
            MessageBox.Show(this, "ASIO and Upmixing (in the Music Tab) must not be used at the same time",
                            "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          }
        }

        // Get all available ASIO devices and add them to the combo box
        string[] asioDevices = BassAsio.BASS_ASIO_GetDeviceDescriptions();
        if (asioDevices.Length == 0)
        {
          MessageBox.Show(this, "No ASIO Devices available in the system.",
                          "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          asioDeviceComboBox.Enabled = false;
          btSettings.Enabled = false;
          useASIOCheckBox.Checked = false;
        }
        else
        {
          asioDeviceComboBox.Items.AddRange(asioDevices);
          asioDeviceComboBox.SelectedIndex = 0;
          btSettings.Enabled = true;
        }
      }
    }

    private void asioDeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Free the selecetd device and activate the new one
      BassAsio.BASS_ASIO_Free();
      if (BassAsio.BASS_ASIO_Init(asioDeviceComboBox.SelectedIndex))
      {
        BASS_ASIO_INFO info = BassAsio.BASS_ASIO_GetInfo();
        if (info != null)
        {
          lbNumberOfChannels.Text = String.Format("Selected device has {0} output channel(s).", info.outputs);
        }
        else
        {
          lbNumberOfChannels.Text = "Couldn't get channel information for selected device";
        }
      }
      else
      {
        MessageBox.Show(this, "Error initialising the selected ASIO device",
                        "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void hScrollBarBalance_ValueChanged(object sender, EventArgs e)
    {
      double balance = (double) hScrollBarBalance.Value/100.0;
      lbBalance.Text = String.Format("{0}", balance);
    }

    private void btSettings_Click(object sender, EventArgs e)
    {
      if (!BassAsio.BASS_ASIO_ControlPanel())
      {
        MessageBox.Show(this, "Selected ASIO device does not have a Control Panel",
                        "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    #endregion

    #region SectionSettings Overloads

    public override void OnSectionActivated()
    {
      //
      // Disable the complete Page, when the Music Player is not the BASS engine
      //
      SectionSettings section = GetSection("Music");

      if (section != null)
      {
        string player = (string) section.GetSetting("audioPlayer");
        if (player.IndexOf("BASS") == -1)
        {
          this.Enabled = false;
          MessageBox.Show(this, "ASIO is only available with the BASS music player selected.",
                          "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        else
        {
          this.Enabled = true;
        }
      }
    }

    /// <summary>
    /// Load ASIO Settings
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bool _useASIO = xmlreader.GetValueAsBool("audioplayer", "asio", false);
        string _asioDeviceSelected = xmlreader.GetValueAsString("audioplayer", "asiodevice", "None");
        hScrollBarBalance.Value = xmlreader.GetValueAsInt("audioplayer", "asiobalance", 0);
        useASIOCheckBox.Checked = _useASIO;

        if (_useASIO)
        {
          string[] asioDevices = BassAsio.BASS_ASIO_GetDeviceDescriptions();
          // Check if the ASIO device read is amongst the one retrieved
          bool found = false;
          for (int i = 0; i < asioDevices.Length; i++)
          {
            if (asioDevices[i] == _asioDeviceSelected)
            {
              found = true;
              bool rc = BassAsio.BASS_ASIO_Init(i);
              break;
            }
          }
          if (found)
          {
            asioDeviceComboBox.SelectedItem = _asioDeviceSelected;
          }
          else
          {
            Log.Warn("Selected ASIO Device not found. {0}", _asioDeviceSelected);
          }
        }
        else
        {
          asioDeviceComboBox.Enabled = false;
          btSettings.Enabled = false;
        }
      }
    }

    /// <summary>
    /// Save ASIO Settings
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // Player Settings
        xmlwriter.SetValueAsBool("audioplayer", "asio", useASIOCheckBox.Checked);

        if (useASIOCheckBox.Checked)
        {
          xmlwriter.SetValue("audioplayer", "asiodevice", asioDeviceComboBox.Text);
        }
        else
        {
          xmlwriter.SetValue("audioplayer", "asiodevice", "None");
        }

        xmlwriter.SetValue("audioplayer", "asiobalance", hScrollBarBalance.Value);
      }
    }

    /// <summary>
    /// Returns Settings as selecteled on this Tab
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override object GetSetting(string name)
    {
      switch (name.ToLower())
      {
        case "useasio":
          return useASIOCheckBox.Checked;
      }

      return null;
    }

    #endregion
  }
}