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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
namespace SetupTv.Sections
{
  public partial class TvChannels : SectionSettings
  {
    public class ListViewColumnSorter : IComparer
    {
      public int SortColumn;
      public SortOrder Order;
      public int Compare(object x, object y)
      {
        int compareResult;
        ListViewItem listviewX, listviewY;
        // Cast the objects to be compared to ListViewItem objects
        listviewX = (ListViewItem)x;
        listviewY = (ListViewItem)y;
        if (SortColumn == 0)
        {
          compareResult = String.Compare(listviewX.Text, listviewY.Text);
        }
        else
        {
          // Compare the two items
          compareResult = String.Compare(listviewX.SubItems[SortColumn].Text,
            listviewY.SubItems[SortColumn].Text);
        }
        // Calculate correct return value based on object comparison
        if (Order == SortOrder.Ascending)
        {
          // Ascending sort is selected,
          // return normal result of compare operation
          return compareResult;
        }
        else if (Order == SortOrder.Descending)
        {
          // Descending sort is selected,
          // return negative result of compare operation
          return (-compareResult);
        }
        else
        {
          // Return '0' to indicate they are equal
          return 0;
        }
      }
    }
    private ListViewColumnSorter lvwColumnSorter;
    public TvChannels()
      : this("TV Channels")
    {
    }

    public TvChannels(string name)
      : base(name)
    {
      InitializeComponent();

      lvwColumnSorter = new ListViewColumnSorter();
      this.mpListView1.ListViewItemSorter = lvwColumnSorter;

    }

    public override void OnSectionDeActivated()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        Channel ch = (Channel)mpListView1.Items[i].Tag;
        if (ch.SortOrder != i + 1)
        {
          ch.SortOrder = i + 1;
        }
        ch.VisibleInGuide = mpListView1.Items[i].Checked;
        ch.Persist();
      }

      //DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      base.OnSectionDeActivated();
    }
    public override void OnSectionActivated()
    {

      CountryCollection countries = new CountryCollection();
      Dictionary<int, CardType> cards = new Dictionary<int, CardType>();
      IList dbsCards = Card.ListAll();
      foreach (Card card in dbsCards)
      {
        cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
      }
      base.OnSectionActivated();

      mpListView1.BeginUpdate();
      mpListView1.Items.Clear();
      IList chs = Channel.ListAll();
      int channelCount = 0;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      IList allmaps = ChannelMap.ListAll();


      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        if (ch.IsTv == false) continue;
        channelCount++;
        IList maps = ch.ReferringChannelMap();
        foreach (ChannelMap map in maps)
        {
          if (cards.ContainsKey(map.IdCard))
          {
            CardType type = cards[map.IdCard];
            switch (type)
            {
              case CardType.Analog: analog = true; break;
              case CardType.DvbC: dvbc = true; break;
              case CardType.DvbT: dvbt = true; break;
              case CardType.DvbS: dvbs = true; break;
              case CardType.Atsc: atsc = true; break;
            }
          }
        }
        StringBuilder builder = new StringBuilder();

        string[] details = new string[4];
        details[0] = "";
        details[1] = "";
        details[2] = "";
        details[3] = "";
        if (analog)
        {
          builder.Append("Analog");
        }
        if (dvbc)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-C");
        }
        if (dvbt)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-T");
        }
        if (dvbs)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-S");
        }
        if (atsc)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("ATSC");
        }
        ListViewItem item = new ListViewItem((mpListView1.Items.Count + 1).ToString());
        item.SubItems.Add(ch.Name);
        item.Checked = ch.VisibleInGuide;
        item.Tag = ch;
        item.SubItems.Add(builder.ToString());
        items.Add(item);

      }
      mpListView1.Items.AddRange(items.ToArray());
      mpListView1.EndUpdate();
      mpLabelChannelCount.Text = String.Format("Total channels:{0}", channelCount);
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      IList details = TuningDetail.ListAll();
      foreach (TuningDetail detail in details) detail.Remove();

      IList groupmaps = GroupMap.ListAll();
      foreach (GroupMap groupmap in groupmaps) groupmap.Remove();

      IList channelMaps = ChannelMap.ListAll();
      foreach (ChannelMap channelMap in channelMaps) channelMap.Remove();

      IList recordings = Recording.ListAll();
      foreach (Recording recording in recordings) recording.Remove();

      IList canceledSchedules = CanceledSchedule.ListAll();
      foreach (CanceledSchedule canceledSchedule in canceledSchedules) canceledSchedule.Remove();

      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules) schedule.Remove();

      IList programs = Program.ListAll();
      foreach (Program program in programs) program.Remove();

      IList channels = Channel.ListAll();
      foreach (Channel channel in channels) channel.Remove();


      mpListView1.EndUpdate();
      OnSectionActivated();
    }

    private void TvChannels_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonClearEncrypted_Click(object sender, EventArgs e)
    {
      //@ TODO : does not work
      IList channels = Channel.ListAll();

      mpListView1.BeginUpdate();
      for (int i = 0; i < channels.Count; ++i)
      {
        Channel ch = (Channel)channels[i];
        if (ch.IsTv)
        {
          for (int x = 0; x < ch.ReferringTuningDetail().Count; x++)
          {
            TuningDetail detail = (TuningDetail)ch.ReferringTuningDetail()[x];
            if (detail.FreeToAir == false)
            {
              ch.Delete();
              break;
            }
          }
        }
      }
      mpListView1.EndUpdate();
      OnSectionActivated();
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        channel.Delete();
        mpListView1.Items.Remove(item);
      }
      mpListView1.EndUpdate();
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index - 1, item);
        }
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {

      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index + 1, item);
        }
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    void ReOrder()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Text = (i + 1).ToString();

        Channel channel = (Channel)mpListView1.Items[i].Tag;
        if (channel.SortOrder != i)
        {
          channel.SortOrder = i;
          channel.Persist();
        }
      }
    }

    private void mpListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      try
      {
        int oldIndex = e.Item;
        ListViewItem item = mpListView1.Items[oldIndex];
        int newIndex = (Int32.Parse(e.Label) - 1);
        if (newIndex == oldIndex) return;

        mpListView1.Items.RemoveAt(oldIndex);
        mpListView1.Items.Insert(newIndex, item);
        ReOrder();
        e.CancelEdit = true;
      }
      catch (Exception)
      {
      }
    }

    private void mpButtonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = channel;
      dlg.ShowDialog();
      channel.Persist();
      OnSectionActivated();
    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter.Order == SortOrder.Ascending)
        {
          lvwColumnSorter.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListView1.Sort();
      ReOrder();
    }

    void AddAttribute(XmlNode node, string tagName, string tagValue)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(tagName);
      attr.InnerText = tagValue;
      node.Attributes.Append(attr);
    }
    void AddAttribute(XmlNode node, string tagName, int tagValue)
    {
      AddAttribute(node, tagName, tagValue.ToString());
    }
    void AddAttribute(XmlNode node, string tagName, bool tagValue)
    {
      AddAttribute(node, tagName, tagValue.ToString());
    }

    void Export()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlNode rootElement=xmlDoc.CreateElement("tvserver");
      AddAttribute(rootElement, "version", "1.0");

      XmlNode nodeServers = xmlDoc.CreateElement("servers");
      IList servers = Server.ListAll();
      foreach (Server server in servers)
      {
        XmlNode nodeServer = xmlDoc.CreateElement("server");
        AddAttribute(nodeServer, "HostName", server.HostName);
        AddAttribute(nodeServer, "IdServer", server.IdServer);
        AddAttribute(nodeServer, "IsMaster", server.IsMaster);

        XmlNode nodeCards = xmlDoc.CreateElement("cards");
        IList cards = Card.ListAll();
        foreach (Card card in cards)
        {
          XmlNode nodeCard = xmlDoc.CreateElement("card");
          AddAttribute(nodeCard, "IdCard", card.IdCard);
          AddAttribute(nodeCard, "DevicePath", card.DevicePath);
          AddAttribute(nodeCard, "Enabled", card.Enabled);
          AddAttribute(nodeCard, "CamType", card.CamType);
          AddAttribute(nodeCard, "GrabEPG", card.GrabEPG);
          AddAttribute(nodeCard, "LastEpgGrab", String.Format("{0}-{1}-{2} {3}:{4}:{5}", card.LastEpgGrab.Year, card.LastEpgGrab.Month, card.LastEpgGrab.Day, card.LastEpgGrab.Hour, card.LastEpgGrab.Minute, card.LastEpgGrab.Second));
          AddAttribute(nodeCard, "Name", card.Name);
          AddAttribute(nodeCard, "Priority", card.Priority);
          AddAttribute(nodeCard, "RecordingFolder", card.RecordingFolder);
          nodeCards.AppendChild(nodeCard);
        }
        nodeServer.AppendChild(nodeCards);
        nodeServers.AppendChild(nodeServer);
      }
      rootElement.AppendChild(nodeServers);

      XmlNode nodechannels = xmlDoc.CreateElement("channels");
      IList channels = Channel.ListAll();
      foreach(Channel channel in channels)
      {
        XmlNode nodechannel = xmlDoc.CreateElement("channel");
        AddAttribute(nodechannel, "Name", channel.Name);
        AddAttribute(nodechannel, "GrabEpg", channel.GrabEpg);
        AddAttribute(nodechannel, "IdChannel", channel.IdChannel);
        AddAttribute(nodechannel, "IsRadio", channel.IsRadio);
        AddAttribute(nodechannel, "IsTv", channel.IsTv);
        AddAttribute(nodechannel, "LastGrabTime", String.Format("{0}-{1}-{2} {3}:{4}:{5}", channel.LastGrabTime.Year, channel.LastGrabTime.Month,channel.LastGrabTime.Day,channel.LastGrabTime.Hour,channel.LastGrabTime.Minute,channel.LastGrabTime.Second));
        AddAttribute(nodechannel, "SortOrder", channel.SortOrder);
        AddAttribute(nodechannel, "TimesWatched", channel.TimesWatched);
        AddAttribute(nodechannel, "TotalTimeWatched", String.Format("{0}-{1}-{2} {3}:{4}:{5}", channel.TotalTimeWatched.Year, channel.TotalTimeWatched.Month, channel.TotalTimeWatched.Day, channel.TotalTimeWatched.Hour, channel.TotalTimeWatched.Minute, channel.TotalTimeWatched.Second));
        AddAttribute(nodechannel, "VisibleInGuide", channel.VisibleInGuide);

        XmlNode nodeMaps = xmlDoc.CreateElement("mappings");
        foreach (ChannelMap map in channel.ReferringChannelMap()) 
        {
          XmlNode nodeMap = xmlDoc.CreateElement("map");
          AddAttribute(nodeMap, "IdCard", map.IdCard);
          AddAttribute(nodeMap, "IdChannel", map.IdChannel);
          AddAttribute(nodeMap, "IdChannelMap", map.IdChannelMap);
          nodeMaps.AppendChild(nodeMap);
        }
        nodechannel.AppendChild(nodeMaps);

        XmlNode nodeTuningDetails = xmlDoc.CreateElement("TuningDetails");
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          XmlNode nodeTune = xmlDoc.CreateElement("tune");
          AddAttribute(nodeTune, "IdChannel", detail.IdChannel);
          AddAttribute(nodeTune, "IdTuning", detail.IdTuning);
          AddAttribute(nodeTune, "AudioPid", detail.AudioPid);
          AddAttribute(nodeTune, "Bandwidth", detail.Bandwidth);
          AddAttribute(nodeTune, "ChannelNumber", detail.ChannelNumber);
          AddAttribute(nodeTune, "ChannelType", (int)detail.ChannelType);
          AddAttribute(nodeTune, "CountryId", (int)detail.CountryId);
          AddAttribute(nodeTune, "Diseqc", (int)detail.Diseqc);
          AddAttribute(nodeTune, "FreeToAir", detail.FreeToAir);
          AddAttribute(nodeTune, "Frequency", detail.Frequency);
          AddAttribute(nodeTune, "MajorChannel", detail.MajorChannel);
          AddAttribute(nodeTune, "MinorChannel", detail.MinorChannel);
          AddAttribute(nodeTune, "Modulation", detail.Modulation);
          AddAttribute(nodeTune, "Name", detail.Name);
          AddAttribute(nodeTune, "NetworkId", detail.NetworkId);
          AddAttribute(nodeTune, "PcrPid", detail.PcrPid);
          AddAttribute(nodeTune, "PmtPid", detail.PmtPid);
          AddAttribute(nodeTune, "Polarisation", (int)detail.Polarisation);
          AddAttribute(nodeTune, "Provider", detail.Provider);
          AddAttribute(nodeTune, "ServiceId", (int)detail.ServiceId);
          AddAttribute(nodeTune, "SwitchingFrequency", (int)detail.SwitchingFrequency);
          AddAttribute(nodeTune, "Symbolrate", (int)detail.Symbolrate);
          AddAttribute(nodeTune, "TransportId", (int)detail.TransportId);
          AddAttribute(nodeTune, "TuningSource", (int)detail.TuningSource);
          AddAttribute(nodeTune, "VideoPid", (int)detail.VideoPid);
          AddAttribute(nodeTune, "VideoSource", (int)detail.VideoSource);
          nodeTuningDetails.AppendChild(nodeTune);
        }
        nodechannel.AppendChild(nodeTuningDetails);

        nodechannels.AppendChild(nodechannel);
      }
      rootElement.AppendChild(nodechannels);
      xmlDoc.AppendChild(rootElement);
      xmlDoc.Save("export.xml");
      MessageBox.Show("Channels exported to 'export.xml'");
    }
  }
}