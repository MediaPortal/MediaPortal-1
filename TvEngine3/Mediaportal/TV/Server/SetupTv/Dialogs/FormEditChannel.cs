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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditChannel : Form
  {
    private int _idChannel = -1;
    private int _newTuningDetailFakeId = -1;
    private MediaType _mediaType = MediaType.Television;
    private Channel _channel = null;

    public FormEditChannel(int idChannel, MediaType mediaType)
    {
      _idChannel = idChannel;
      _mediaType = mediaType;
      InitializeComponent();
    }

    public int IdChannel
    {
      get
      {
        return _idChannel;
      }
    }

    private void FormEditChannel_Load(object sender, EventArgs e)
    {
      if (_idChannel >= 0)
      {
        this.LogInfo("channel: start edit, ID = {0}", _idChannel);
        Text = "Edit Channel";
        _channel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(_idChannel, ChannelRelation.None);
        labelIdValue.Text = _channel.IdChannel.ToString();
        textBoxName.Text = _channel.Name;
        textBoxNumber.Text = _channel.ChannelNumber;
        textBoxExternalId.Text = _channel.ExternalId;
        checkBoxVisibleInGuide.Checked = _channel.VisibleInGuide;
        DebugChannelSettings(_channel);
      }
      else
      {
        this.LogInfo("channel: create new");
        Text = "Add Channel";
        labelIdValue.Text = string.Empty;
        textBoxName.Text = string.Empty;
        textBoxNumber.Text = string.Empty;
        textBoxExternalId.Text = string.Empty;
        checkBoxVisibleInGuide.Checked = true;
      }

      UpdateTuningDetailList();
    }

    private void DebugChannelSettings(Channel channel)
    {
      this.LogDebug("channel: settings...");
      this.LogDebug("  ID               = {0}", channel.IdChannel);
      this.LogDebug("  name             = {0}", channel.Name);
      this.LogDebug("  number           = {0}", channel.ChannelNumber);
      this.LogDebug("  external ID      = {0}", channel.ExternalId);
      this.LogDebug("  media type       = {0}", (MediaType)channel.MediaType);
      this.LogDebug("  visible in guide = {0}", channel.VisibleInGuide);
      this.LogDebug("  last grab time   = {0}", channel.LastGrabTime);
    }

    private void DebugTuningDetailSettings(TuningDetail tuningDetail)
    {
      this.LogDebug("channel: tuning detail...");
      this.LogDebug("  ID               = {0}", tuningDetail.IdTuning);
      this.LogDebug("  name             = {0}", tuningDetail.Name);
      this.LogDebug("  number           = {0}", tuningDetail.LogicalChannelNumber);
      this.LogDebug("  priority         = {0}", tuningDetail.Priority);
      this.LogDebug("  provider         = {0}", tuningDetail.Provider);
      this.LogDebug("  encrypted?       = {0}", tuningDetail.IsEncrypted);
      this.LogDebug("  high definition? = {0}", tuningDetail.IsHighDefinition);
      this.LogDebug("  3D?              = {0}", tuningDetail.IsThreeDimensional);
      this.LogDebug("  grab EPG?        = {0}", tuningDetail.GrabEpg);
      this.LogDebug("  last EPG grab    = {0}", tuningDetail.LastEpgGrabTime);
      BroadcastStandard broadcastStandard = (BroadcastStandard)tuningDetail.BroadcastStandard;
      this.LogDebug("  standard         = {0}", broadcastStandard);

      if (broadcastStandard == BroadcastStandard.AnalogTelevision)
      {
        this.LogDebug("  physical channel = {0}", tuningDetail.PhysicalChannelNumber);
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
        this.LogDebug("  country          = {0}", tuningDetail.CountryId);
        this.LogDebug("  tuner source     = {0}", (AnalogTunerSource)tuningDetail.TuningSource);
      }
      else if (broadcastStandard == BroadcastStandard.Atsc || broadcastStandard == BroadcastStandard.Scte)
      {
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
        if (broadcastStandard == BroadcastStandard.Atsc)
        {
          this.LogDebug("  modulation       = {0}", (ModulationSchemeVsb)tuningDetail.Modulation);
        }
        else
        {
          this.LogDebug("  modulation       = {0}", (ModulationSchemeQam)tuningDetail.Modulation);
        }
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  program number   = {0}", tuningDetail.ServiceId);
        this.LogDebug("  source ID        = {0}", tuningDetail.SourceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
      }
      else if (broadcastStandard == BroadcastStandard.ExternalInput)
      {
        this.LogDebug("  video source     = {0}", (CaptureSourceVideo)tuningDetail.VideoSource);
        this.LogDebug("  audio source     = {0}", (CaptureSourceAudio)tuningDetail.AudioSource);
        this.LogDebug("  VCR signal?      = {0}", tuningDetail.IsVcrSignal);
      }
      else if (broadcastStandard == BroadcastStandard.DvbC)
      {
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
        this.LogDebug("  modulation       = {0}", (ModulationSchemeQam)tuningDetail.Modulation);
        this.LogDebug("  symbol rate      = {0} ks/s", tuningDetail.SymbolRate);
        this.LogDebug("  ONID             = {0}", tuningDetail.OriginalNetworkId);
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  service ID       = {0}", tuningDetail.ServiceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
      }
      else if (broadcastStandard == BroadcastStandard.DvbT || broadcastStandard == BroadcastStandard.DvbT2)
      {
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
        this.LogDebug("  bandwidth        = {0} kHz", tuningDetail.Bandwidth);
        if (broadcastStandard == BroadcastStandard.DvbT2)
        {
          this.LogDebug("  PLP ID           = {0}", tuningDetail.StreamId);
        }
        this.LogDebug("  ONID             = {0}", tuningDetail.OriginalNetworkId);
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  service ID       = {0}", tuningDetail.ServiceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
      }
      else if (broadcastStandard == BroadcastStandard.FmRadio)
      {
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
      }
      else if (BroadcastStandard.MaskSatellite.HasFlag(broadcastStandard))
      {
        this.LogDebug("  satellite        = {0}", tuningDetail.Satellite);
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
        this.LogDebug("  polarisation     = {0}", (Polarisation)tuningDetail.Polarisation);
        this.LogDebug("  modulation       = {0}", (ModulationSchemePsk)tuningDetail.Modulation);
        this.LogDebug("  symbol rate      = {0} ks/s", tuningDetail.SymbolRate);
        this.LogDebug("  FEC code rate    = {0}", (FecCodeRate)tuningDetail.FecCodeRate);
        if (broadcastStandard == BroadcastStandard.DvbS2)
        {
          this.LogDebug("  pilot tones      = {0}", (PilotTonesState)tuningDetail.PilotTonesState);
          this.LogDebug("  roll-off factor  = {0}", (RollOffFactor)tuningDetail.RollOffFactor);
          this.LogDebug("  input stream ID  = {0}", tuningDetail.StreamId);
        }
        if (BroadcastStandard.MaskDvb.HasFlag(broadcastStandard) || broadcastStandard == BroadcastStandard.SatelliteTurboFec)
        {
          this.LogDebug("  ONID             = {0}", tuningDetail.OriginalNetworkId);
        }
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  program number   = {0}", tuningDetail.ServiceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
        if (BroadcastStandard.MaskDvb.HasFlag(broadcastStandard))
        {
          this.LogDebug("  Freesat CID      = {0}", tuningDetail.FreesatChannelId);
        }
      }
      else if (broadcastStandard == BroadcastStandard.DvbIp)
      {
        this.LogDebug("  URL              = {0}", tuningDetail.Url);
        this.LogDebug("  ONID             = {0}", tuningDetail.OriginalNetworkId);
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  service ID       = {0}", tuningDetail.ServiceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
      }

      if (BroadcastStandard.MaskDvb.HasFlag(broadcastStandard))
      {
        this.LogDebug("  OpenTV CID       = {0}", tuningDetail.OpenTvChannelId);
        this.LogDebug("  EPG ONID         = {0}", tuningDetail.EpgOriginalNetworkId);
        this.LogDebug("  EPG TSID         = {0}", tuningDetail.EpgTransportStreamId);
        this.LogDebug("  EPG service ID   = {0}", tuningDetail.EpgServiceId);
      }
    }

    private void UpdateTuningDetailList()
    {
      if (_idChannel <= 0)
      {
        return;
      }
      listViewTuningDetails.BeginUpdate();
      try
      {
        listViewTuningDetails.Items.Clear();
        foreach (TuningDetail tuningDetail in ServiceAgents.Instance.ChannelServiceAgent.ListAllTuningDetailsByChannel(_idChannel, TuningDetailRelation.Satellite))
        {
          DebugTuningDetailSettings(tuningDetail);
          listViewTuningDetails.Items.Add(CreateItemForTuningDetail(tuningDetail));
        }
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
      }
    }

    private ListViewItem CreateItemForTuningDetail(TuningDetail tuningDetail)
    {
      int imageIndex = 0;
      if (_mediaType == MediaType.Television)
      {
        if (tuningDetail.IsEncrypted)
        {
          imageIndex = 1;
        }
        else
        {
          imageIndex = 0;
        }
      }
      else if (_mediaType == MediaType.Radio)
      {
        if (tuningDetail.IsEncrypted)
        {
          imageIndex = 3;
        }
        else
        {
          imageIndex = 2;
        }
      }
      ListViewItem item = new ListViewItem(tuningDetail.IdTuning.ToString(), imageIndex);
      item.Tag = tuningDetail;
      item.SubItems.Add(tuningDetail.Name);
      item.SubItems.Add(tuningDetail.LogicalChannelNumber);
      item.SubItems.Add(tuningDetail.Provider);
      item.SubItems.Add(((BroadcastStandard)tuningDetail.BroadcastStandard).GetDescription());
      item.SubItems.Add(tuningDetail.GetDescriptiveString());
      return item;
    }

    #region button and mouse handlers

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(textBoxName.Text))
      {
        MessageBox.Show("Please enter a name for the channel.", SetupControls.SectionSettings.MESSAGE_CAPTION);
        return;
      }
      int intChannelNumber;
      float floatChannelNumber;
      if (
        string.IsNullOrWhiteSpace(textBoxNumber.Text) ||
        (
          !int.TryParse(textBoxNumber.Text, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out intChannelNumber) &&
          !float.TryParse(textBoxNumber.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatChannelNumber)
        )
      )
      {
        MessageBox.Show("Please enter a channel number in the form ### or #.#. For example, 123 or 1.10.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK);
        return;
      }

      if (_channel == null)
      {
        this.LogInfo("channel: save new");
        _channel = new Channel();
        _channel.MediaType = (int)_mediaType;
      }
      else
      {
        this.LogInfo("channel: save changes, ID = {0}", _idChannel);
      }
      _channel.Name = textBoxName.Text;
      _channel.ChannelNumber = textBoxNumber.Text;
      _channel.ExternalId = textBoxExternalId.Text;
      _channel.VisibleInGuide = checkBoxVisibleInGuide.Checked;
      _channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(_channel);
      _idChannel = _channel.IdChannel;
      DebugChannelSettings(_channel);

      int priority = 1;
      foreach (ListViewItem item in listViewTuningDetails.Items)
      {
        TuningDetail tuningDetail = item.Tag as TuningDetail;
        if (tuningDetail.IdTuning <= 0)
        {
          // New tuning detail.
          int originalId = tuningDetail.IdTuning;
          tuningDetail.IdTuning = 0;
          tuningDetail.IdChannel = _channel.IdChannel;
          tuningDetail.MediaType = (int)_mediaType;
          tuningDetail.Priority = priority;
          tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
          this.LogInfo("channel: tuning detail {0} saved as {1}", originalId, tuningDetail.IdTuning);
        }
        else
        {
          if (tuningDetail.Priority != priority)
          {
            // Existing tuning detail priority change.
            this.LogInfo("channel: tuning detail {0} priority changed from {1} to {2}", tuningDetail.IdTuning, tuningDetail.Priority, priority);
            tuningDetail.Priority = priority;
            ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
          }
          else if (tuningDetail.IdChannel != _channel.IdChannel)
          {
            // Existing tuning detail moved to this channel.
            tuningDetail.IdChannel = _channel.IdChannel;
            ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
          }
        }

        priority++;
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      if (_channel == null)
      {
        this.LogInfo("channel: cancel new, ID = {0}", _idChannel);

        // Delete tuning details that were moved to this channel.
        bool confirmed = false;
        foreach (ListViewItem item in listViewTuningDetails.Items)
        {
          TuningDetail tuningDetail = item.Tag as TuningDetail;
          if (tuningDetail.IdTuning > 0)
          {
            if (!confirmed)
            {
              DialogResult result = MessageBox.Show("Tuning details that have been moved to this channel will be lost. Are you sure you want to cancel?", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
              if (result == DialogResult.No)
              {
                this.LogInfo("channel: cancel cancelation to avoid loss of tuning details");
                return;
              }
              confirmed = true;
            }
            this.LogInfo("channel: tuning detail {0} deleted", tuningDetail.IdTuning);
            ServiceAgents.Instance.ChannelServiceAgent.DeleteTuningDetail(tuningDetail.IdTuning);
          }
        }
      }
      else
      {
        this.LogInfo("channel: cancel changes, ID = {0}", _idChannel);
      }

      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void buttonTuningDetailAdd_Click(object sender, EventArgs e)
    {
      FormEditTuningDetailCommon form = null;
      using (FormSelectTuningDetailType dlg = new FormSelectTuningDetailType(_mediaType))
      {
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }
        form = dlg.TuningDetailForm;
      }

      form.TuningDetail = null;
      TuningDetail tuningDetail = null;
      try
      {
        if (form.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }
        tuningDetail = form.TuningDetail;
      }
      finally
      {
        form.Dispose();
      }

      tuningDetail.IdTuning = _newTuningDetailFakeId--;
      tuningDetail.IdChannel = _idChannel;
      tuningDetail.MediaType = (int)_mediaType;
      tuningDetail.Priority = listViewTuningDetails.Items.Count + 1;
      this.LogInfo("channel: tuning detail {0} added", tuningDetail.IdTuning);
      DebugTuningDetailSettings(tuningDetail);
      listViewTuningDetails.Items.Add(CreateItemForTuningDetail(tuningDetail));
    }

    private void buttonTuningDetailEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuningDetails.SelectedItems;
      if (items == null)
      {
        return;
      }

      IList<ListViewItem> itemsToReselect = new List<ListViewItem>(items.Count);
      foreach (ListViewItem item in items)
      {
        TuningDetail tuningDetail = item.Tag as TuningDetail;
        if (tuningDetail == null)
        {
          continue;
        }

        FormEditTuningDetailCommon form = FormSelectTuningDetailType.GetTuningDetailFormForBroadcastStandard((BroadcastStandard)tuningDetail.BroadcastStandard);
        if (form == null)
        {
          continue;
        }

        try
        {
          form.TuningDetail = tuningDetail;
          if (form.ShowDialog(this) != DialogResult.OK)
          {
            continue;
          }

          if (tuningDetail.IdTuning > 0)
          {
            tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetail(tuningDetail.IdTuning, TuningDetailRelation.Satellite);
          }
          else
          {
            tuningDetail = form.TuningDetail;
          }
        }
        finally
        {
          form.Dispose();
        }

        DebugTuningDetailSettings(tuningDetail);
        int index = item.Index;
        listViewTuningDetails.Items.RemoveAt(index);
        itemsToReselect.Add(listViewTuningDetails.Items.Insert(index, CreateItemForTuningDetail(tuningDetail)));
      }

      foreach (ListViewItem item in itemsToReselect)
      {
        item.Selected = true;
      }
      listViewTuningDetails.Focus();
    }

    private void buttonTuningDetailDelete_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuningDetails.SelectedItems;
      if (items == null || items.Count == 0)
      {
        return;
      }
      DialogResult result = MessageBox.Show("Are you sure you want to delete the selected tuning detail(s)?", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes)
      {
        return;
      }

      foreach (ListViewItem item in items)
      {
        TuningDetail tuningDetail = item.Tag as TuningDetail;
        if (tuningDetail == null)
        {
          continue;
        }

        this.LogInfo("channel: tuning detail {0} deleted", tuningDetail.IdTuning);
        if (tuningDetail.IdTuning > 0)
        {
          ServiceAgents.Instance.ChannelServiceAgent.DeleteTuningDetail(tuningDetail.IdTuning);
        }
        item.Remove();
      }
    }

    private void buttonTuningDetailPriorityUp_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuningDetails.SelectedItems;
      if (items == null || listViewTuningDetails.Items.Count < 2)
      {
        return;
      }

      listViewTuningDetails.BeginUpdate();
      try
      {
        foreach (ListViewItem item in items)
        {
          int index = item.Index;
          if (index > 0)
          {
            listViewTuningDetails.Items.RemoveAt(index);
            listViewTuningDetails.Items.Insert(index - 1, item);
          }
        }
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
      }
    }

    private void buttonTuningDetailPriorityDown_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewTuningDetails.SelectedItems;
      if (items == null || listViewTuningDetails.Items.Count < 2)
      {
        return;
      }

      listViewTuningDetails.BeginUpdate();
      try
      {
        for (int i = items.Count - 1; i >= 0; i--)
        {
          ListViewItem item = items[i];
          int index = item.Index;
          if (index + 1 < listViewTuningDetails.Items.Count)
          {
            listViewTuningDetails.Items.RemoveAt(index);
            listViewTuningDetails.Items.Insert(index + 1, item);
          }
        }
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
      }
    }

    private void listViewTuningDetails_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      buttonTuningDetailEdit_Click(sender, new EventArgs());
    }

    private void listViewTuningDetails_SelectedIndexChanged(object sender, EventArgs e)
    {
      int selectionCount = listViewTuningDetails.SelectedIndices.Count;
      buttonTuningDetailEdit.Enabled = selectionCount > 0;
      buttonTuningDetailDelete.Enabled = buttonTuningDetailEdit.Enabled;
    }

    #endregion

    #region drag and drop

    private bool IsValidDragDropData(DragEventArgs e)
    {
      MPListView listView = e.Data.GetData(typeof(MPListView)) as MPListView;
      if (listView == null)
      {
        e.Effect = DragDropEffects.None;
        return false;
      }
      e.Effect = DragDropEffects.Move;
      return true;
    }

    private void listViewTuningDetails_ItemDrag(object sender, ItemDragEventArgs e)
    {
      listViewTuningDetails.DoDragDrop(listViewTuningDetails, DragDropEffects.Move);
    }

    private void listViewTuningDetails_DragOver(object sender, DragEventArgs e)
    {
      if (IsValidDragDropData(e))
      {
        listViewTuningDetails.DefaultDragOverHandler(e);
      }
    }

    private void listViewTuningDetails_DragEnter(object sender, DragEventArgs e)
    {
      IsValidDragDropData(e);
    }

    private void listViewTuningDetails_DragDrop(object sender, DragEventArgs e)
    {
      if (!IsValidDragDropData(e))
      {
        return;
      }

      MPListView listView = e.Data.GetData(typeof(MPListView)) as MPListView;
      if (listView == listViewTuningDetails)
      {
        // Prioritising (row ordering).
        listViewTuningDetails.DefaultDragDropHandler(e);
        return;
      }

      // Moving between channels.
      // Determine where we're going to insert the dragged item(s).
      Point cp = listViewTuningDetails.PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = listViewTuningDetails.GetItemAt(cp.X, cp.Y);
      int dropIndex;
      if (dragToItem == null)
      {
        if (listViewTuningDetails.Items.Count != 0)
        {
          return;
        }
        dropIndex = 0;
      }
      else
      {
        // Always drop below as there is no space to draw the insert line above the first item.
        dropIndex = dragToItem.Index;
        dropIndex++;
      }

      listView.BeginUpdate();
      listViewTuningDetails.BeginUpdate();
      try
      {
        // Move the items.
        foreach (ListViewItem item in listView.SelectedItems)
        {
          listView.Items.RemoveAt(item.Index);
          listViewTuningDetails.Items.Insert(dropIndex++, item);
          if (_channel != null)
          {
            TuningDetail tuningDetail = item.Tag as TuningDetail;
            int originalChannelId = tuningDetail.IdChannel;
            tuningDetail.IdChannel = _channel.IdChannel;
            tuningDetail.Priority = dropIndex;
            if (tuningDetail.IdTuning <= 0)
            {
              // New tuning detail.
              int originalTuningDetailId = tuningDetail.IdTuning;
              tuningDetail.IdTuning = 0;
              tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
              this.LogInfo("channel: new tuning detail {0} moved from channel {1} to channel {2} and saved as {3}", originalTuningDetailId, originalChannelId, _channel.IdChannel, tuningDetail.IdTuning);
            }
            else
            {
              this.LogInfo("channel: tuning detail {0} moved from channel {1} to channel {2}", tuningDetail.IdTuning, originalChannelId, _channel.IdChannel);
              tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
            }
            item.Tag = tuningDetail;
          }
        }
      }
      finally
      {
        listView.EndUpdate();
        listViewTuningDetails.EndUpdate();
      }
    }

    #endregion
  }
}