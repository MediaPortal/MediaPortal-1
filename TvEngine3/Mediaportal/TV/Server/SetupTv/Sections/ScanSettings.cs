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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class ScanSettings : SectionSettings
  {
    #region private classes

    private class DisplaySoftwareEncoder
    {
      private String _installed;
      private SoftwareEncoder _encoder;

      public event PropertyChangedEventHandler _propertyChanged;

      public DisplaySoftwareEncoder(SoftwareEncoder encoder)
      {
        _installed = "No";
        _encoder = encoder;
      }

      public String Installed
      {
        get { return _installed; }
        set { _installed = value; }
      }

      public String Priority
      {
        get { return _encoder.Priority.ToString(); }
        set
        {
          _encoder.Priority = Convert.ToInt32(value);
          NotifyPropertyChanged("Priority");
        }
      }

      public String Name
      {
        get { return _encoder.Name; }
      }

      public bool Reusable
      {
        get { return _encoder.Reusable; }
        set
        {
          _encoder.Reusable = value;
          NotifyPropertyChanged("Reusable");
        }
      }

      private void NotifyPropertyChanged(String name)
      {
        if (_propertyChanged != null)
        {
          _propertyChanged(this, new PropertyChangedEventArgs(name));
        }
      }

      public void Persist()
      {
        //_encoder.Persist();
        ServiceAgents.Instance.CardServiceAgent.SaveSoftwareEncoder(_encoder);
      }
    }

    #endregion

    private BindingList<DisplaySoftwareEncoder> _bindingVideoEncoders;
    private BindingList<DisplaySoftwareEncoder> _bindingAudioEncoders;

    public ScanSettings()
      : this("General") {}

    public ScanSettings(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      

      numericUpDownTune.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("timeoutTune", "2").Value);
      numericUpDownPAT.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("timeoutPAT", "5").Value);
      numericUpDownCAT.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("timeoutCAT", "5").Value);
      numericUpDownPMT.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("timeoutPMT", "10").Value);
      numericUpDownSDT.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("timeoutSDT", "20").Value);
      numericUpDownAnalog.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("timeoutAnalog", "20").Value);

      delayDetectUpDown.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("delayCardDetect", "0").Value);

      checkBoxEnableLinkageScanner.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("linkageScannerEnabled", "no").Value == "yes");

      mpComboBoxPrio.Items.Clear();
      mpComboBoxPrio.Items.Add("Realtime");
      mpComboBoxPrio.Items.Add("High");
      mpComboBoxPrio.Items.Add("Above Normal");
      mpComboBoxPrio.Items.Add("Normal");
      mpComboBoxPrio.Items.Add("Below Normal");
      mpComboBoxPrio.Items.Add("Idle");

      try
      {
        mpComboBoxPrio.SelectedIndex = Convert.ToInt32(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("processPriority", "3").Value);
        //default is normal=3       
      }
      catch (Exception)
      {
        mpComboBoxPrio.SelectedIndex = 3; //fall back to default which is normal=3
      }

      BuildLists();
      numericUpDownReuseLimit.Value = Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("softwareEncoderReuseLimit", "0").Value);
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("timeoutTune", numericUpDownTune.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("timeoutPAT", numericUpDownPAT.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("timeoutCAT", numericUpDownCAT.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("timeoutPMT", numericUpDownPMT.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("timeoutSDT", numericUpDownSDT.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("timeoutAnalog", numericUpDownAnalog.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("linkageScannerEnabled", checkBoxEnableLinkageScanner.Checked ? "yes" : "no");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("processPriority", mpComboBoxPrio.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("delayCardDetect", delayDetectUpDown.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("softwareEncoderReuseLimit", numericUpDownReuseLimit.Value.ToString());      

      foreach (DisplaySoftwareEncoder encoder in _bindingVideoEncoders)
      {
        encoder.Persist();
      }

      foreach (DisplaySoftwareEncoder encoder in _bindingAudioEncoders)
      {
        encoder.Persist();
      }
    }

    private void mpComboBoxPrio_SelectedIndexChanged(object sender, EventArgs e)
    {
      System.Diagnostics.Process process;
      try
      {
        process = System.Diagnostics.Process.GetProcessesByName("TVService")[0];
      }
      catch (Exception ex)
      {
        Log.Write("could not set priority on tvservice - the process might be terminated : " + ex.Message);
        return;
      }

      try
      {
        switch (mpComboBoxPrio.SelectedIndex)
        {
          case 0:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            break;
          case 1:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            break;
          case 2:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
            break;
          case 3:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
          case 4:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            break;
          case 5:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle;
            break;
          default:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
        }
      }
      catch (Exception exp)
      {
        Log.Write(string.Format("Could not set priority on tvservice. Error on setting process.PriorityClass: {0}",
                                exp.Message));
        return;
      }
    }

    private void BuildLists()
    {
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(FilterCategory.VideoCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(FilterCategory.AudioCompressorCategory);
      DsDevice[] devices3 = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);

      bool found;      
      IList<SoftwareEncoder> encoders = ServiceAgents.Instance.CardServiceAgent.ListAllSofwareEncodersVideo();
      _bindingVideoEncoders = new BindingList<DisplaySoftwareEncoder>();
      foreach (SoftwareEncoder encoder in encoders)
      {
        found = false;
        DisplaySoftwareEncoder displayEncoder = new DisplaySoftwareEncoder(encoder);
        if (devices1.Any(t => t.Name == encoder.Name))
        {
          found = true;
          displayEncoder.Installed = "Yes";
        }
        if (!found)
        {
          for (int i = 0; i < devices3.Length; i++)
          {
            if (devices3[i].Name == encoder.Name)
            {
              displayEncoder.Installed = "Yes";
              break;
            }
          }
        }
        _bindingVideoEncoders.Add(displayEncoder);
      }
      mpListViewVideo.DataSource = _bindingVideoEncoders;

      encoders = ServiceAgents.Instance.CardServiceAgent.ListAllSofwareEncodersAudio();
      _bindingAudioEncoders = new BindingList<DisplaySoftwareEncoder>();
      foreach (SoftwareEncoder encoder in encoders)
      {
        found = false;
        DisplaySoftwareEncoder displayEncoder = new DisplaySoftwareEncoder(encoder);
        for (int i = 0; i < devices2.Length; i++)
        {
          if (devices2[i].Name == encoder.Name)
          {
            found = true;
            displayEncoder.Installed = "Yes";
            break;
          }
        }
        if (!found)
        {
          for (int i = 0; i < devices3.Length; i++)
          {
            if (devices3[i].Name == encoder.Name)
            {
              displayEncoder.Installed = "Yes";
              break;
            }
          }
        }
        _bindingAudioEncoders.Add(displayEncoder);
      }
      mpListViewAudio.DataSource = _bindingAudioEncoders;
    }

    private void MoveEncodersUp(DataGridView grid, BindingList<DisplaySoftwareEncoder> list)
    {
      DataGridViewSelectedRowCollection srows = grid.SelectedRows;
      List<int> indices = new List<int>();
      if (srows.Count == 0)
      {
        return;
      }

      // Juggle rows...
      for (int i = 0; i < srows.Count; i++)
      {
        int index = srows[i].Index;
        indices.Add(index);
        if (index > 0)
        {
          list[index].Priority = (Convert.ToInt32(list[index].Priority) - 1).ToString();
          list[index - 1].Priority = (Convert.ToInt32(list[index - 1].Priority) + 1).ToString();
          DisplaySoftwareEncoder item = list[index];
          list.RemoveAt(index);
          list.Insert(index - 1, item);
        }
      }

      // Maintain selection...
      grid.ClearSelection();
      for (int i = 0; i < indices.Count; i++)
      {
        int index = indices[i];
        if (index == 0)
        {
          grid.Rows[index].Selected = true;
        }
        else
        {
          grid.Rows[index - 1].Selected = true;
        }
      }
    }

    private void MoveEncodersDown(DataGridView grid, BindingList<DisplaySoftwareEncoder> list)
    {
      DataGridViewSelectedRowCollection srows = grid.SelectedRows;
      List<int> indices = new List<int>();
      if (srows.Count == 0)
      {
        return;
      }

      // Juggle rows...
      for (int i = srows.Count - 1; i >= 0; i--)
      {
        int index = srows[i].Index;
        indices.Add(index);
        if (index < grid.Rows.Count - 1)
        {
          list[index].Priority = (Convert.ToInt32(list[index].Priority) + 1).ToString();
          list[index + 1].Priority = (Convert.ToInt32(list[index + 1].Priority) - 1).ToString();
          DisplaySoftwareEncoder item = list[index + 1];
          list.RemoveAt(index + 1);
          list.Insert(index, item);
        }
      }

      // Maintain selection...
      grid.ClearSelection();
      for (int i = indices.Count - 1; i >= 0; i--)
      {
        int index = indices[i];
        if (index == grid.Rows.Count - 1)
        {
          grid.Rows[index].Selected = true;
        }
        else
        {
          grid.Rows[index + 1].Selected = true;
        }
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      MoveEncodersUp(mpListViewVideo, _bindingVideoEncoders);
    }

    private void button4_Click(object sender, EventArgs e)
    {
      MoveEncodersUp(mpListViewAudio, _bindingAudioEncoders);
    }

    private void button2_Click(object sender, EventArgs e)
    {
      MoveEncodersDown(mpListViewVideo, _bindingVideoEncoders);
    }

    private void button3_Click(object sender, EventArgs e)
    {
      MoveEncodersDown(mpListViewAudio, _bindingAudioEncoders);
    }
  }
}