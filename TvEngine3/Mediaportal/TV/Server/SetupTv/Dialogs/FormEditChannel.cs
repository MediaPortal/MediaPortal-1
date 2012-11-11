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
using System.Windows.Forms;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditChannel : Form
  {
    private bool _newChannel;
    private MediaTypeEnum _mediaType = MediaTypeEnum.TV;
    private Channel _channel;    
    

    public FormEditChannel()
    {
      InitializeComponent();
    }

    

    public Channel Channel
    {
      get { return _channel; }
      set { _channel = value; }
    }

    public MediaTypeEnum MediaType
    {
      get { return _mediaType; }
      set { _mediaType = value; }
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      if (textBoxName.Text.Length == 0)
      {
        MessageBox.Show("Please enter a name for this channel");
        return;
      }
      _channel.DisplayName = textBoxName.Text;
      _channel.VisibleInGuide = checkBoxVisibleInTvGuide.Checked;
      _channel.MediaType = (int) _mediaType;

      foreach (TuningDetail detail in _channel.TuningDetails)
      {                
        if (detail.ChangeTracker.State != ObjectState.Deleted)
        {
          detail.IdChannel = _channel.IdChannel;
          detail.MediaType = (int)_mediaType;
          if (string.IsNullOrEmpty(detail.Name))
          {
            detail.Name = _channel.DisplayName;
          }
        }    
      }

      _channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(_channel);      

      if (_newChannel)
      {
        if (_mediaType == MediaTypeEnum.TV)
        {
          MappingHelper.AddChannelToGroup(ref _channel, TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);          
        }
        else if (_mediaType == MediaTypeEnum.Radio)
        {
          MappingHelper.AddChannelToGroup(ref _channel, TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);          
        }
      }            


      DialogResult = DialogResult.OK;
      Close();
    }

    private void FormEditChannel_Load(object sender, EventArgs e)
    {
      _newChannel = false;
      if (_channel == null)
      {
        _newChannel = true;
        _channel = ChannelFactory.CreateChannel(MediaTypeEnum.TV, 0, Schedule.MinSchedule, true, Schedule.MinSchedule, 10000, true, "", "");        
      }
      textBoxName.Text = _channel.DisplayName;
      checkBoxVisibleInTvGuide.Checked = _channel.VisibleInGuide;            
      UpdateTuningDetailList();
    }

    private void UpdateTuningDetailList()
    {
      mpListView1.BeginUpdate();
      try
      {
        mpListView1.Items.Clear();
        foreach (TuningDetail detail in _channel.TuningDetails)
        {
          if (detail.ChangeTracker.State == ObjectState.Deleted)
          {
            continue;
          }
          int imageIndex = 1;
          if (detail.FreeToAir == false)
            imageIndex = 2;
          ListViewItem item = new ListViewItem(detail.IdTuning.ToString(), imageIndex);
          item.SubItems.Add(detail.Name);
          item.SubItems.Add(detail.Provider);
          string channelType = detail.ChannelType.ToString();
          string description = "";
          float frequency;
          switch (detail.ChannelType)
          {
            case 0:
              channelType = "Analog";
              if (detail.VideoSource == (int)AnalogChannel.VideoInputType.Tuner)
              {
                frequency = detail.Frequency;
                frequency /= 1000000.0f;
                description = String.Format("#{0} {1} MHz", detail.ChannelNumber, frequency.ToString("f2"));
              }
              else
              {
                description = detail.VideoSource.ToString();
              }
              break;
            case 1:
              channelType = "ATSC";
              description = String.Format("{0} {1}:{2}", detail.ChannelNumber, detail.MajorChannel,
                                          detail.MinorChannel);
              break;
            case 2:
              channelType = "DVB-C";
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              description = String.Format("{0} MHz SR:{1}", frequency.ToString("f2"), detail.Symbolrate);
              break;
            case 3:
              channelType = "DVB-S";
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              description = String.Format("{0} MHz {1}", frequency.ToString("f2"),
                                          (((Polarisation)detail.Polarisation)));
              break;
            case 4:
              channelType = "DVB-T";
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              description = String.Format("{0} MHz BW:{1}", frequency.ToString("f2"), detail.Bandwidth);
              break;
            case 5:
              channelType = "Web-Stream";
              description = detail.Url;
              break;
            case 7:
              channelType = "DVB-IP";
              description = detail.Url;
              break;
          }
          item.SubItems.Add(channelType);
          item.SubItems.Add(description);
          mpListView1.Items.Add(item);
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private static long GetFrequency(string text, string precision)
    {
      float tmp = 123.25f;
      if (tmp.ToString("f" + precision).IndexOf(',') > 0)
      {
        text = text.Replace('.', ',');
      }
      else
      {
        text = text.Replace(',', '.');
      }
      float freq = float.Parse(text);
      freq *= 1000000f;
      return (long)freq;
    }

    private static string SetFrequency(long frequency, string precision)
    {
      float freq = frequency;
      freq /= 1000000f;
      return freq.ToString("f" + precision);
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void menuButtonAdd_Click(object sender, EventArgs e)
    {
      FormChooseTuningDetailType dlg = new FormChooseTuningDetailType();
      dlg.MediaType = _mediaType;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        int tuningType = dlg.TuningType;
        FormTuningDetailCommon form = CreateDialog(tuningType);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
          _channel.TuningDetails.Add(form.TuningDetail);
          UpdateTuningDetailList();
        }
      }
    }

    private void menuButtonEdit_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count == 1)
      {
        int index = mpListView1.SelectedIndices[0];
        TuningDetail tuningDetailToEdit = _channel.TuningDetails[index];
        FormTuningDetailCommon form = CreateDialog(tuningDetailToEdit.ChannelType);
        form.TuningDetail = tuningDetailToEdit;
        form.ShowDialog(this);
        UpdateTuningDetailList();
      }
    }

    private static FormTuningDetailCommon CreateDialog(int tuningType)
    {
      switch (tuningType)
      {
        case 0:
          return new FormAnalogTuningDetail();
        case 1:
          return new FormATSCTuningDetail();
        case 2:
          return new FormDVBCTuningDetail();
        case 3:
          return new FormDVBSTuningDetail();
        case 4:
          return new FormDVBTTuningDetail();
        case 5:
          return new FormWebStreamTuningDetail();
        case 7:
          return new FormDVBIPTuningDetail();
      }
      return null;
    }

    private void menuButtonRemove_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count > 0)
      {
        if (
          MessageBox.Show(this, "Are you sure you want to delete the selected tuningdetails?", "",
                          MessageBoxButtons.YesNo) ==
          DialogResult.Yes)
        {
          for (int i = 0; i < mpListView1.SelectedIndices.Count; i++)
          {
            int index = mpListView1.SelectedIndices[i];
            TuningDetail tuningDetailToDelete = _channel.TuningDetails[index];
            tuningDetailToDelete.ChangeTracker.State = ObjectState.Deleted;                        
          }
          UpdateTuningDetailList();
        }
      }
    }

    private void mpListView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      menuButtonEdit_Click(sender, new EventArgs());
    }

    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      int selectionCount = mpListView1.SelectedIndices.Count;
      if (selectionCount <= 0)
      {
        btnEditTuningDetail.Enabled = false;
        btnRemoveTuningDetail.Enabled = false;
        menuButtonEdit.Enabled = false;
        menuButtonRemove.Enabled = false;
      }

      if (selectionCount > 0)
      {
        btnRemoveTuningDetail.Enabled = true;
        menuButtonRemove.Enabled = true;
        if (selectionCount == 1)
        {
          btnEditTuningDetail.Enabled = true;
          menuButtonEdit.Enabled = true;
        }
        else
        {
          btnEditTuningDetail.Enabled = false;
          menuButtonEdit.Enabled = false;
        }
      }
    }
  }
}