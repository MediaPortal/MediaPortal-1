using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace DirecTV.TunerPlugin
{
  public partial class SetupForm : Form
  {

    private DirecTVSettings _settings;

    public SetupForm()
    {
      InitializeComponent();
      DisableAdavancedSettings();
      LoadSettings();
     
    }

    void LoadSettings()
    {
      _settings = new DirecTVSettings();
      _settings.LoadSettings();
      // Build up box types combobox
      modelComboBox.Items.Clear();
      foreach (string box in DirecTVSettings.BoxTypes)
      {
        modelComboBox.Items.Add(box);
      }
      modelComboBox.SelectedItem = _settings.BoxName;
      // Build up serial port combobox
      portComboBox.Items.Clear();
      foreach (string port in System.IO.Ports.SerialPort.GetPortNames())
      {
        portComboBox.Items.Add(port);
      }
      portComboBox.SelectedItem = _settings.SerialPortName;
      // Build up baud rate combobox
      baudComboBox.Items.Clear();
      foreach (int item in new int[] { 110, 300, 600, 1200, 4800, 9600, 19200, 38400, 57600, 76800, 115200, 230400, 307200, 460800 })
      {
        baudComboBox.Items.Add(item);
      }
      baudComboBox.SelectedItem = _settings.BaudRate;
      // Build up keymap combobox
      keyMapComboBox.Items.Clear();
      foreach (string keymap in DirecTVSettings.KeyMaps)
      {
        keyMapComboBox.Items.Add(keymap);
      }
      keyMapComboBox.SelectedItem = _settings.KeyMapName;
      // set loaded readtimeout
      timeoutNumUpDown.Value = _settings.ReadTimeout;
      // set loaded command set
      commandSetBox.Checked = _settings.UseOldCommandSet;
      // set loaded set channel value
      channelSetBox.Checked = _settings.UseSetChannelForTune;
      // set loaded power on value
      powerOnBox.Checked = _settings.PowerOn;
      // set loaded debug value
      debugBox.Checked = _settings.Debug;
      cbAdvanced.Checked = _settings.Advanced;
      if (_settings.Advanced)
          EnableAdavancedSettings();

      cbtwowaydisable.Checked = _settings.TwoWayDisable;
      cbHideOSD.Checked = _settings.HideOSD;
      cbAllowSubChannels.Checked = _settings.AllowDigitalSubchannels;

    }

    void SaveSettings()
    {
      _settings.Advanced = cbAdvanced.Checked;
      _settings.TwoWayDisable = cbtwowaydisable.Checked;
      _settings.BoxName = (string)modelComboBox.SelectedItem;
      _settings.SerialPortName = (string)portComboBox.SelectedItem;
      _settings.BaudRate = (int)baudComboBox.SelectedItem;
      _settings.KeyMapName = (string)keyMapComboBox.SelectedItem;
      _settings.ReadTimeout = (int)timeoutNumUpDown.Value;
      _settings.UseOldCommandSet = commandSetBox.Checked;
      _settings.UseSetChannelForTune = channelSetBox.Checked;
      _settings.PowerOn = powerOnBox.Checked;
      _settings.Debug = debugBox.Checked;
      _settings.HideOSD = cbHideOSD.Checked;
      _settings.AllowDigitalSubchannels = cbAllowSubChannels.Checked;
      _settings.SaveSettings();
    }

    private void modelComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Set the recommended defaults after the box model has changed
      string box = (string)modelComboBox.SelectedItem;
      if(box.Equals(DirecTVSettings.BoxTypes[0]))
      {
        //RCA_Old
        keyMapComboBox.SelectedItem = DirecTVSettings.KeyMaps[0];
        commandSetBox.Checked = true;
        channelSetBox.Checked = true;
        powerOnBox.Checked = false;
      }
      else if (box.Equals(DirecTVSettings.BoxTypes[1]))
      {
        //RCA_New
        keyMapComboBox.SelectedItem = DirecTVSettings.KeyMaps[0];
        commandSetBox.Checked = false;
        channelSetBox.Checked = true;
        powerOnBox.Checked = false;
      }
      else if (box.Equals(DirecTVSettings.BoxTypes[2]))
      {
        //D10-100
        keyMapComboBox.SelectedItem = DirecTVSettings.KeyMaps[1];
        commandSetBox.Checked = false;
        channelSetBox.Checked = false;
        powerOnBox.Checked = false;
      }
      else if (box.Equals(DirecTVSettings.BoxTypes[2]))
      {
        //D10-200
        keyMapComboBox.SelectedItem = DirecTVSettings.KeyMaps[1];
        commandSetBox.Checked = false;
        channelSetBox.Checked = false;
        powerOnBox.Checked = false;
      }
      baudComboBox.SelectedItem = 9600;
      timeoutNumUpDown.Value = 1000;

    }

    private void okButton_Click(object sender, EventArgs e)
    {
      SaveSettings();
      _settings = null;
      Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      _settings = null;
      Close();
    }

    private void cbAdvanded_CheckedChanged(object sender, EventArgs e)
    {
      if (cbAdvanced.Checked)
      {
        EnableAdavancedSettings();
      }
      else
      {
        DisableAdavancedSettings();
      }
    }

    private void EnableAdavancedSettings()
    {
      advancedBox.Enabled = true;
      baudComboBox.Enabled = true;
      timeoutNumUpDown.Enabled = true;
    }

    private void DisableAdavancedSettings()
    {
      advancedBox.Enabled = false;
      baudComboBox.Enabled = false;
      timeoutNumUpDown.Enabled = false;
    }

  }
}