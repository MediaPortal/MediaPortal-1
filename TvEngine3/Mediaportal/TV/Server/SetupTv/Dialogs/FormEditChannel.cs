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

      foreach (ServiceDetail detail in _channel.ServiceDetails)
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
        _channel = ChannelFactory.CreateChannel(MediaTypeEnum.TV, 0, Schedule.MinSchedule, 10000, true, "", "");
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
        foreach (ServiceDetail serviceDetail in _channel.ServiceDetails)
        {
          if (serviceDetail.ChangeTracker.State == ObjectState.Deleted)
          {
            continue;
          }
          int imageIndex = 1;
          if (((EncryptionSchemeEnum)serviceDetail.EncryptionScheme) != EncryptionSchemeEnum.Free)
          {
            imageIndex = 2;
          }
          var tuningDetail = serviceDetail.TuningDetail;

          ListViewItem item = new ListViewItem(tuningDetail.IdTuningDetail.ToString(), imageIndex);
          item.SubItems.Add(serviceDetail.Name);

          var serviceDetailDvb = serviceDetail as ServiceDvb;
          if (serviceDetailDvb != null)
          {
            item.SubItems.Add(serviceDetailDvb.Provider);
          }
          
          string channelType = "";
          string description = "";
          float frequency;
          if (tuningDetail is TuningDetailAnalog)
          {
            var tuningDetailAnalog = tuningDetail as TuningDetailAnalog;
            channelType = "Analog";
            if (tuningDetailAnalog.VideoSource == (int) AnalogChannel.VideoInputType.Tuner)
            {
              frequency = tuningDetailAnalog.Frequency.GetValueOrDefault(0);
              frequency /= 1000000.0f;
              description = String.Format("#{0} {1} MHz", serviceDetail.LogicalChannelNumber, frequency.ToString("f2"));
            }
            else
            {
              description = tuningDetailAnalog.VideoSource.GetValueOrDefault(0).ToString();
            }
          }
          else if (tuningDetail is TuningDetailAtsc)
          {
            var tuningDetailAtsc = tuningDetail as TuningDetailAtsc;
            channelType = "ATSC";
            description = String.Format("{0} {1}:{2}", serviceDetail.LogicalChannelNumber, serviceDetail.MajorChannel,
                                          serviceDetail.MinorChannel);
          }
          else if (tuningDetail is TuningDetailCable)
          {
            channelType = "DVB-C";
            var tuningDetailCable = tuningDetail as TuningDetailCable;
            frequency = tuningDetailCable.Frequency.GetValueOrDefault(0);
            frequency /= 1000.0f;
            description = String.Format("{0} MHz SR:{1}", frequency.ToString("f2"), tuningDetailCable.SymbolRate);
          }
          else if (tuningDetail is TuningDetailSatellite)
          {
            channelType = "DVB-S";
            var tuningDetailSatellite = tuningDetail as TuningDetailSatellite;
            frequency = tuningDetailSatellite.Frequency.GetValueOrDefault(0);
            frequency /= 1000.0f;
            description = String.Format("{0} MHz {1}", frequency.ToString("f2"),
                                        (((Polarisation)tuningDetailSatellite.Polarisation.GetValueOrDefault(0))));
          }
          else if (tuningDetail is TuningDetailTerrestrial)
          {
            channelType = "DVB-T";
            var tuningDetailTerrestrial = tuningDetail as TuningDetailTerrestrial;
            frequency = tuningDetailTerrestrial.Frequency.GetValueOrDefault(0);
            frequency /= 1000.0f;
            description = String.Format("{0} MHz BW:{1}", frequency.ToString("f2"), tuningDetailTerrestrial.Bandwidth);
          }
          else if (tuningDetail is TuningDetailStream)
          {
            channelType = "Web-Stream/IP";
            var tuningDetailStream = tuningDetail as TuningDetailStream;
            if (tuningDetailStream != null)
            {
              description = tuningDetailStream.Url;
            }
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
          _channel.ServiceDetails.Add(form.ServiceDetail);
          UpdateTuningDetailList();
        }
      }
    }

    private void menuButtonEdit_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count == 1)
      {
        int index = mpListView1.SelectedIndices[0];
        ServiceDetail detailToEdit = _channel.ServiceDetails[index];
        FormTuningDetailCommon form = CreateDialog(detailToEdit);
        form.ServiceDetail = detailToEdit;
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
          break;

        case 1:
          return new FormATSCTuningDetail();
          break;

        case 2:
          return new FormDVBCTuningDetail();
          break;

        case 3:
          return new FormDVBSTuningDetail();
          break;

        case 4:
          return new FormDVBTTuningDetail();
          break;

        case 5:
          return new FormWebStreamTuningDetail();
          break;

        case 7:
          return new FormDVBIPTuningDetail();
          break;
      }
      
      return null;
    }

    private static FormTuningDetailCommon CreateDialog(ServiceDetail detailToEdit)
    {
      var tuningDetail = detailToEdit.TuningDetail;
      
      if (tuningDetail is TuningDetailAnalog)
      {
        return new FormAnalogTuningDetail();
      }
      else if (tuningDetail is TuningDetailAtsc)
      {
        return new FormATSCTuningDetail();
      }
      else if (tuningDetail is TuningDetailCable)
      {
        return new FormDVBCTuningDetail();
      }
      else if (tuningDetail is TuningDetailSatellite)
      {
        return new FormDVBSTuningDetail();
      }
      else if (tuningDetail is TuningDetailTerrestrial)
      {
        return new FormDVBTTuningDetail();
      }
      else if (tuningDetail is TuningDetailStream)
      {
        if (detailToEdit is ServiceDvb)
        {
          return new FormWebStreamTuningDetail();
        }        
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
            TuningDetail tuningDetailToDelete = _channel.ServiceDetails[index].TuningDetail;
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