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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Channel;
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
    private const int SUBITEM_INDEX_PRIORITY = 5;

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
        channelNumberUpDownNumber.Text = _channel.ChannelNumber;
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
        channelNumberUpDownNumber.Text = LogicalChannelNumber.GLOBAL_DEFAULT;
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
      this.LogDebug("  ID               = {0}", tuningDetail.IdTuningDetail);
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

      if (broadcastStandard == BroadcastStandard.AmRadio)
      {
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
      }
      else if (broadcastStandard == BroadcastStandard.AnalogTelevision)
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
      else if (broadcastStandard == BroadcastStandard.DvbC || broadcastStandard == BroadcastStandard.IsdbC)
      {
        this.LogDebug("  frequency        = {0} kHz", tuningDetail.Frequency);
        if (broadcastStandard == BroadcastStandard.DvbC)
        {
          this.LogDebug("  modulation       = {0}", (ModulationSchemeQam)tuningDetail.Modulation);
          this.LogDebug("  symbol rate      = {0} ks/s", tuningDetail.SymbolRate);
        }
        this.LogDebug("  ONID             = {0}", tuningDetail.OriginalNetworkId);
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  service ID       = {0}", tuningDetail.ServiceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
      }
      else if (
        broadcastStandard == BroadcastStandard.DvbT ||
        broadcastStandard == BroadcastStandard.DvbT2 ||
        broadcastStandard == BroadcastStandard.IsdbT
      )
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
        if (broadcastStandard == BroadcastStandard.DvbDsng || BroadcastStandard.MaskDvbS2.HasFlag(broadcastStandard))
        {
          this.LogDebug("  roll-off factor  = {0}", (RollOffFactor)tuningDetail.RollOffFactor);
          if (BroadcastStandard.MaskDvbS2.HasFlag(broadcastStandard))
          {
            this.LogDebug("  pilot tones      = {0}", (PilotTonesState)tuningDetail.PilotTonesState);
            this.LogDebug("  input stream ID  = {0}", tuningDetail.StreamId);
          }
        }
        if (broadcastStandard != BroadcastStandard.DigiCipher2)
        {
          this.LogDebug("  ONID             = {0}", tuningDetail.OriginalNetworkId);
        }
        this.LogDebug("  TSID             = {0}", tuningDetail.TransportStreamId);
        this.LogDebug("  program number   = {0}", tuningDetail.ServiceId);
        this.LogDebug("  PMT PID          = {0}", tuningDetail.PmtPid);
        if (broadcastStandard == BroadcastStandard.DvbS || BroadcastStandard.MaskDvbS2.HasFlag(broadcastStandard))
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

      if (BroadcastStandard.MaskOpenTvSi.HasFlag(broadcastStandard))
      {
        this.LogDebug("  OpenTV CID       = {0}", tuningDetail.OpenTvChannelId);
      }
      if ((BroadcastStandard.MaskDvb | BroadcastStandard.MaskIsdb).HasFlag(broadcastStandard))
      {
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
        ConsolidateTuningDetailPriorities();
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
      ListViewItem item = new ListViewItem(tuningDetail.IdTuningDetail.ToString(), imageIndex);
      item.Tag = tuningDetail;
      item.SubItems.Add(tuningDetail.Name);
      item.SubItems.Add(tuningDetail.LogicalChannelNumber);
      item.SubItems.Add(tuningDetail.Provider);
      item.SubItems.Add(((BroadcastStandard)tuningDetail.BroadcastStandard).GetDescription());
      item.SubItems.Add(tuningDetail.Priority.ToString());
      item.SubItems.Add(tuningDetail.GetTerseTuningDescription());
      return item;
    }

    private void ConsolidateTuningDetailPriorities(MPListView listView = null)
    {
      // Consolidate priority values such that there are no gaps in the
      // sequence. Two or more tuning details may have the same priority.
      if (listView == null)
      {
        listView = listViewTuningDetails;
      }
      int previousItemPriority = -1;
      int nextItemPriority = 0;
      foreach (ListViewItem item in listView.Items)
      {
        int currentPriority = int.Parse(item.SubItems[SUBITEM_INDEX_PRIORITY].Text);
        if (currentPriority != previousItemPriority)
        {
          nextItemPriority++;
        }
        item.SubItems[SUBITEM_INDEX_PRIORITY].Text = nextItemPriority.ToString();
        previousItemPriority = currentPriority;
      }
    }

    #region button and mouse handlers

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(textBoxName.Text))
      {
        MessageBox.Show("Please enter a name for the channel.", SetupControls.SectionSettings.MESSAGE_CAPTION);
        return;
      }
      string logicalChannelNumber;
      if (!LogicalChannelNumber.Create(channelNumberUpDownNumber.Text, out logicalChannelNumber))
      {
        MessageBox.Show("Please enter a channel number in the form ### or #.#. For example, 123 or 1.23.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK);
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
      _channel.ChannelNumber = logicalChannelNumber;
      _channel.ExternalId = textBoxExternalId.Text;
      _channel.VisibleInGuide = checkBoxVisibleInGuide.Checked;
      _channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(_channel);
      _idChannel = _channel.IdChannel;
      DebugChannelSettings(_channel);

      foreach (ListViewItem item in listViewTuningDetails.Items)
      {
        TuningDetail tuningDetail = item.Tag as TuningDetail;
        int newPriority = int.Parse(item.SubItems[SUBITEM_INDEX_PRIORITY].Text);
        if (tuningDetail.IdTuningDetail <= 0)
        {
          // New tuning detail.
          int originalId = tuningDetail.IdTuningDetail;
          tuningDetail.IdTuningDetail = 0;
          tuningDetail.IdChannel = _channel.IdChannel;
          tuningDetail.MediaType = (int)_mediaType;
          tuningDetail.Priority = newPriority;
          tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
          this.LogInfo("channel: tuning detail {0} saved as {1}", originalId, tuningDetail.IdTuningDetail);
        }
        else
        {
          bool save = false;
          if (tuningDetail.Priority != newPriority)
          {
            // Existing tuning detail priority change.
            this.LogInfo("channel: tuning detail {0} priority changed from {1} to {2}", tuningDetail.IdTuningDetail, tuningDetail.Priority, newPriority);
            tuningDetail.Priority = newPriority;
            save = true;
          }
          if (tuningDetail.IdChannel != _channel.IdChannel)
          {
            // Existing tuning detail moved to this channel.
            tuningDetail.IdChannel = _channel.IdChannel;
            save = true;
          }
          if (save)
          {
            ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
          }
        }
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
          if (tuningDetail.IdTuningDetail > 0)
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
            this.LogInfo("channel: tuning detail {0} deleted", tuningDetail.IdTuningDetail);
            ServiceAgents.Instance.ChannelServiceAgent.DeleteTuningDetail(tuningDetail.IdTuningDetail);
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

      tuningDetail.IdTuningDetail = _newTuningDetailFakeId--;
      tuningDetail.IdChannel = _idChannel;
      tuningDetail.MediaType = (int)_mediaType;
      if (listViewTuningDetails.Items.Count == 0)
      {
        tuningDetail.Priority = 1;
      }
      else
      {
        tuningDetail.Priority = int.Parse(listViewTuningDetails.Items[listViewTuningDetails.Items.Count - 1].SubItems[SUBITEM_INDEX_PRIORITY].Text) + 1;
      }
      this.LogInfo("channel: tuning detail {0} added", tuningDetail.IdTuningDetail);
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
      listViewTuningDetails.BeginUpdate();
      try
      {
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

            if (tuningDetail.IdTuningDetail > 0)
            {
              tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetail(tuningDetail.IdTuningDetail, TuningDetailRelation.Satellite);
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
          string unsavedPriority = item.SubItems[SUBITEM_INDEX_PRIORITY].Text;
          listViewTuningDetails.Items.RemoveAt(index);
          ListViewItem newItem = CreateItemForTuningDetail(tuningDetail);
          newItem.SubItems[SUBITEM_INDEX_PRIORITY].Text = unsavedPriority;
          itemsToReselect.Add(listViewTuningDetails.Items.Insert(index, newItem));
        }

        foreach (ListViewItem item in itemsToReselect)
        {
          item.Selected = true;
        }
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
        listViewTuningDetails.Focus();
      }
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

      int originalTunerCount = items.Count;
      listViewTuningDetails.BeginUpdate();
      try
      {
        foreach (ListViewItem item in items)
        {
          TuningDetail tuningDetail = item.Tag as TuningDetail;
          if (tuningDetail == null)
          {
            continue;
          }

          this.LogInfo("channel: tuning detail {0} deleted", tuningDetail.IdTuningDetail);
          if (tuningDetail.IdTuningDetail > 0)
          {
            ServiceAgents.Instance.ChannelServiceAgent.DeleteTuningDetail(tuningDetail.IdTuningDetail);
          }
          item.Remove();
        }

        if (listViewTuningDetails.Items.Count != originalTunerCount)
        {
          ConsolidateTuningDetailPriorities();
        }
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
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
          int newPriority = int.Parse(item.SubItems[SUBITEM_INDEX_PRIORITY].Text);
          if (newPriority > 1)
          {
            newPriority--;
          }
          item.SubItems[SUBITEM_INDEX_PRIORITY].Text = newPriority.ToString();

          int newIndex = item.Index;
          while (
            newIndex > 0 &&
            newPriority < int.Parse(listViewTuningDetails.Items[newIndex - 1].SubItems[SUBITEM_INDEX_PRIORITY].Text)
          )
          {
            newIndex--;
          }
          if (newIndex != item.Index)
          {
            listViewTuningDetails.Items.RemoveAt(item.Index);
            listViewTuningDetails.Items.Insert(newIndex, item);
          }
        }
        ConsolidateTuningDetailPriorities();
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
        int lastItemPriority = int.Parse(listViewTuningDetails.Items[listViewTuningDetails.Items.Count - 1].SubItems[SUBITEM_INDEX_PRIORITY].Text);
        for (int i = items.Count - 1; i >= 0; i--)
        {
          ListViewItem item = items[i];
          int newPriority = int.Parse(item.SubItems[SUBITEM_INDEX_PRIORITY].Text);
          if (newPriority < Math.Max(listViewTuningDetails.Items.Count, lastItemPriority))
          {
            newPriority++;
          }
          item.SubItems[SUBITEM_INDEX_PRIORITY].Text = newPriority.ToString();

          int newIndex = item.Index;
          while (
            newIndex + 1 < listViewTuningDetails.Items.Count &&
            newPriority > int.Parse(listViewTuningDetails.Items[newIndex + 1].SubItems[SUBITEM_INDEX_PRIORITY].Text)
          )
          {
            newIndex++;
          }
          if (newIndex != item.Index)
          {
            listViewTuningDetails.Items.RemoveAt(item.Index);
            listViewTuningDetails.Items.Insert(newIndex, item);
          }
        }
        ConsolidateTuningDetailPriorities();
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
      }
    }

    private void listViewTuningDetails_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        buttonTuningDetailDelete_Click(null, null);
        e.Handled = true;
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

    private bool IsValidDragDropData(DragEventArgs e, out MPListView source)
    {
      source = e.Data.GetData(typeof(MPListView)) as MPListView;
      if (source == null || source.Name != listViewTuningDetails.Name)
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
      MPListView source;
      if (!IsValidDragDropData(e, out source))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      e.Effect = DragDropEffects.Move;
      listViewTuningDetails.DefaultDragOverHandler(source == listViewTuningDetails, e);
    }

    private void listViewTuningDetails_DragEnter(object sender, DragEventArgs e)
    {
      MPListView source;
      IsValidDragDropData(e, out source);
    }

    private void listViewTuningDetails_DragDrop(object sender, DragEventArgs e)
    {
      MPListView source;
      if (!IsValidDragDropData(e, out source) || source.SelectedItems.Count == 0)
      {
        return;
      }

      // Determine where we're going to insert the dragged item(s).
      Point cp = listViewTuningDetails.PointToClient(new Point(e.X, e.Y));
      ListViewItem dropOnItem = listViewTuningDetails.GetItemAt(cp.X, cp.Y);
      int insertIndex = listViewTuningDetails.Items.Count;
      if (dropOnItem != null)
      {
        insertIndex = dropOnItem.Index;
        if (cp.Y >= dropOnItem.Bounds.Top + (dropOnItem.Bounds.Height / 2))
        {
          insertIndex++;
        }
      }
      else
      {
        foreach (ListViewItem item in listViewTuningDetails.Items)
        {
          if (cp.Y < item.Bounds.Top + (item.Bounds.Height / 2))
          {
            insertIndex = item.Index;
            break;
          }
        }
      }

      // Determine the priority adjustment for the dragged item(s). The
      // priority of the first dragged item will become one more than the
      // priority of the next item up that is not being moved.
      int priorityBase = 1;
      int aboveItemIndex = insertIndex - 1;
      while (aboveItemIndex >= 0)
      {
        ListViewItem item = listViewTuningDetails.Items[aboveItemIndex];
        if (source != listViewTuningDetails || !item.Selected)
        {
          priorityBase = int.Parse(item.SubItems[SUBITEM_INDEX_PRIORITY].Text) + 1;
          break;
        }
        aboveItemIndex--;
      }
      int priorityOffset = int.Parse(source.SelectedItems[0].SubItems[SUBITEM_INDEX_PRIORITY].Text);
      if (source == listViewTuningDetails && priorityOffset == priorityBase)
      {
        // No change in channel or priority => no need to continue.
        listViewTuningDetails.Invalidate();
        return;
      }

      listViewTuningDetails.BeginUpdate();
      if (source != listViewTuningDetails)
      {
        source.BeginUpdate();
      }
      try
      {
        // Remove the dragged item(s) from their source list view.
        List<ListViewItem> selectedItems = new List<ListViewItem>(source.SelectedItems.Count);
        foreach (ListViewItem item in source.SelectedItems)
        {
          selectedItems.Add(item);
          if (source == listViewTuningDetails && item.Index < insertIndex)
          {
            insertIndex--;
          }
          item.Remove();
        }

        // Adjust priority values and insert the items in ascending priority
        // order in our tuning detail list view.
        foreach (ListViewItem item in selectedItems)
        {
          int newPriority = priorityBase + (int.Parse(item.SubItems[SUBITEM_INDEX_PRIORITY].Text) - priorityOffset);
          item.SubItems[SUBITEM_INDEX_PRIORITY].Text = newPriority.ToString();
          while (insertIndex < listViewTuningDetails.Items.Count)
          {
            if (int.Parse(listViewTuningDetails.Items[insertIndex].SubItems[SUBITEM_INDEX_PRIORITY].Text) > newPriority)
            {
              break;
            }
            insertIndex++;
          }
          listViewTuningDetails.Items.Insert(insertIndex++, item);

          // If moving tuning details to this channel, save the tuning details.
          if (_channel != null && source != listViewTuningDetails)
          {
            TuningDetail tuningDetail = item.Tag as TuningDetail;
            int originalChannelId = tuningDetail.IdChannel;
            tuningDetail.IdChannel = _channel.IdChannel;
            tuningDetail.Priority = newPriority;
            if (tuningDetail.IdTuningDetail <= 0)
            {
              // New tuning detail.
              int originalTuningDetailId = tuningDetail.IdTuningDetail;
              tuningDetail.IdTuningDetail = 0;
              tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
              this.LogInfo("channel: new tuning detail {0} moved from channel {1} to channel {2} and saved as {3}", originalTuningDetailId, originalChannelId, _channel.IdChannel, tuningDetail.IdTuningDetail);
            }
            else
            {
              this.LogInfo("channel: tuning detail {0} moved from channel {1} to channel {2}", tuningDetail.IdTuningDetail, originalChannelId, _channel.IdChannel);
              tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(tuningDetail);
            }
            item.Tag = tuningDetail;
          }
        }

        // Finally, consolidate all priority values to remove gaps in the
        // sequence. Don't forget the source list view.
        ConsolidateTuningDetailPriorities(listViewTuningDetails);
        if (source != listViewTuningDetails)
        {
          ConsolidateTuningDetailPriorities(source);
        }
      }
      finally
      {
        listViewTuningDetails.EndUpdate();
        if (source != listViewTuningDetails)
        {
          source.EndUpdate();
        }
      }
    }

    #endregion
  }
}