using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

namespace SetupTv.Sections
{
  public partial class TvEpgGrabber : SectionSettings
  {
    public TvEpgGrabber()
      : this("TV Epg grabber")
    {
    }

    public TvEpgGrabber(string name)
      : base(name)
    {
      InitializeComponent();
    }

    void LoadLanguages()
    {
      mpListView2.BeginUpdate();
      mpListView2.Items.Clear();
      TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
      List<String> codes = languages.GetLanguageCodes();
      List<String> list = languages.GetLanguages();

      
      int index = 0;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      string values = "";
      foreach (string lang in list)
      {
        ListViewItem item = mpListView2.Items.Add(lang);
        item.Tag = codes[index];
        index++;
        if (setting.Value == "")
        {
          values += item.Tag;
          values += ",";
        }
        else
        {
          if (setting.Value.IndexOf((string)item.Tag) >= 0)
          {
            item.Checked = true;
          }
        }
      }
      mpListView2.Sort();

      mpListView2.EndUpdate();
      if (setting.Value == "")
      {
        setting.Value = values;
        DatabaseManager.Instance.SaveChanges();
      }
      
    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      SaveSettings();
    }

    public override void OnSectionActivated()
    {
      mpListView1.BeginUpdate();
      LoadLanguages();
      CountryCollection countries = new CountryCollection();
      Dictionary<string, CardType> cards = new Dictionary<string, CardType>();
      EntityList<Card> dbsCards = DatabaseManager.Instance.GetEntities<Card>();
      foreach (Card card in dbsCards)
      {
        cards[card.DevicePath] = RemoteControl.Instance.Type(card.IdCard);
      }
      base.OnSectionActivated();
      mpListView1.Items.Clear();

      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddOrderBy(Channel.SortOrderEntityColumn);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);

      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        if (ch.IsTv == false) continue;
        ListViewItem item = mpListView1.Items.Add(ch.Name);
        foreach (ChannelMap map in ch.ChannelMaps)
        {
          if (cards.ContainsKey(map.Card.DevicePath))
          {
            CardType type = cards[map.Card.DevicePath];
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
        string line = "";
        string[] details = new string[4];
        if (analog)
        {
          line += "Analog";
        }
        if (dvbc)
        {
          if (line != "") line += ",";
          line += "DVB-C";
        }
        if (dvbt)
        {
          if (line != "") line += ",";
          line += "DVB-T";
        }
        if (dvbs)
        {
          if (line != "") line += ",";
          line += "DVB-S";
        }
        if (atsc)
        {
          if (line != "") line += ",";
          line += "ATSC";
        }
        item.SubItems.Add(line);
        item.Checked = ch.GrabEpg;
        item.Tag = ch;

      }
      mpListView1.EndUpdate();
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      Channel channel = e.Item.Tag as Channel;
      if (channel == null) return;
      channel.GrabEpg = e.Item.Checked;
    }

    private void mpButtonAll_Click(object sender, EventArgs e)
    {
      mpListView2.BeginUpdate();
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        mpListView2.Items[i].Checked = true;
      }
      TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
      List<String> codes = languages.GetLanguageCodes();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      foreach (string code in codes)
      {
        setting.Value += code;
        setting.Value += ",";
      }
      mpListView2.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpButtonNone_Click(object sender, EventArgs e)
    {
      mpListView2.BeginUpdate();
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        mpListView2.Items[i].Checked = false;
      }
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      setting.Value = ",";
      DatabaseManager.Instance.SaveChanges();
      mpListView2.EndUpdate();
    }

    public override void SaveSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      setting.Value = ",";
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        if (mpListView2.Items[i].Checked)
        {
          string code = (string)mpListView2.Items[i].Tag;
          setting.Value += code;
          setting.Value += ",";

        }
      }
      DatabaseManager.Instance.SaveChanges();
      base.SaveSettings();
    }

    public override void LoadSettings()
    {
      base.LoadSettings();
    }

    private void mpButtonAllChannels_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Checked = true;
      }
      mpListView1.EndUpdate();
    }

    private void mpButtonNoneChannels_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Checked = false;
      }
      mpListView1.EndUpdate();
    }

    private void mpListView2_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
    }

  }
}