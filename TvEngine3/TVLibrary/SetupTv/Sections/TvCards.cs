/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using Microsoft.Win32;
using DirectShowLib;
namespace SetupTv.Sections
{
  public partial class TvCards : SectionSettings
  {
    private bool _needRestart = false;
    int cardId = 0;
    private Dictionary<string, CardType> cardTypes = new Dictionary<string, CardType>();
    private TabPage usbWINTV_tabpage = null;

    #region CardInfo class
    public class CardInfo
    {
      public Card card;
      public CardInfo(Card newcard)
      {
        card = newcard;
      }
      public override string ToString()
      {
        return card.Name;
      }
    }
    #endregion

    public TvCards()
      : this("TV Cards")
    {
    }

    public TvCards(string name)
      : base(name)
    {
      InitializeComponent();
    }

    void UpdateMenu()
    {
      placeInHybridCardToolStripMenuItem.DropDownItems.Clear();
      IList groups = CardGroup.ListAll();
      foreach (CardGroup group in groups)
      {
        ToolStripMenuItem item = new ToolStripMenuItem(group.Name);
        item.Tag = group;
        item.Click += new EventHandler(placeInHybridCardToolStripMenuItem_Click);
        placeInHybridCardToolStripMenuItem.DropDownItems.Add(item);
      }
      ToolStripMenuItem itemNew = new ToolStripMenuItem("New...");
      itemNew.Click += new EventHandler(placeInHybridCardToolStripMenuItem_Click);
      placeInHybridCardToolStripMenuItem.DropDownItems.Add(itemNew);
    }

    void placeInHybridCardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
      CardGroup group;
      if (menuItem.Tag == null)
      {
        GroupNameForm dlg = new GroupNameForm();
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }
        group = new CardGroup(dlg.GroupName);
        group.Persist();
        UpdateMenu();
      }
      else
      {
        group = (CardGroup)menuItem.Tag;
      }
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[indexes[i]];
        Card card = (Card)item.Tag;
        CardGroupMap map = new CardGroupMap(card.IdCard, group.IdCardGroup);
        map.Persist();
        card.PreloadCard = false;
        card.Persist();
      }
      UpdateHybrids();
      RemoteControl.Instance.Restart();
    }

    public override void OnSectionDeActivated()
    {
      ReOrder();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting s = layer.GetSetting("enableWinTVTray", "no");
      if (checkBoxWinTVTray.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();
      if (_needRestart)
      {
        bool isAnyUserTS = false;
        bool isRec = false;
        bool isUserTS = false;
        bool isRecOrTS = RemoteControl.Instance.IsAnyCardRecordingOrTimeshifting(new User(), out isUserTS, out isAnyUserTS, out isRec);

        if (!isAnyUserTS && !isRec && !isRecOrTS && !isUserTS)
        {
          RemoteControl.Instance.Restart();
        }
        else
        {
          MessageBox.Show(this, "In order to apply new settings - please restart tvservice manually when done timeshifting / recording.");
        }
      }
      base.OnSectionDeActivated();
    }

    void SaveWinTVSettings(int cardId, string name, string moniker)
    {
      String fileName = "WinTV-CI.xml";
      XmlTextWriter writer = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
      writer.Formatting = Formatting.Indented;
      writer.Indentation = 1;
      writer.IndentChar = (char)9;
      writer.WriteStartDocument(true);
      writer.WriteStartElement("configuration"); //<configuration>
      writer.WriteAttributeString("version", "1");
      writer.WriteStartElement("card"); //<card>
      writer.WriteAttributeString("cardId", XmlConvert.ToString(cardId));
      writer.WriteAttributeString("name", name);
      writer.WriteStartElement("device"); //<device>
      writer.WriteElementString("path", moniker);
      writer.WriteEndElement(); //</device>
      writer.WriteEndElement(); //</card>
      writer.WriteEndElement(); //</configuration>
      writer.WriteEndDocument();
      writer.Close();
    }

    void LoadWinTVSettings()
    {
      string configfile = "WinTV-CI.xml";
      XmlDocument doc = new XmlDocument();
      doc.Load(configfile);
      XmlNode cardNode = doc.DocumentElement.SelectSingleNode("/configuration/card");
      cardId = int.Parse(cardNode.Attributes["cardId"].Value);
    }

    public override void OnSectionActivated()
    {
      _needRestart = false;
      UpdateList();
      TvBusinessLayer layer = new TvBusinessLayer();
      checkBoxWinTVTray.Checked = (layer.GetSetting("enableWinTVTray", "no").Value == "yes");
    }

    void UpdateList()
    {
      base.OnSectionActivated();
      mpListView1.Items.Clear();
      try
      {
        IList dbsCards = Card.ListAll();
        foreach (Card card in dbsCards)
        {
          cardTypes[card.DevicePath] = RemoteControl.Instance.Type(card.IdCard);
          mpComboBoxCard.Items.Add(new CardInfo(card));
        }
        LoadWinTVSettings();
        mpComboBoxCard.SelectedIndex = cardId-1;
      }
      catch (Exception)
      {
      }
      try
      {
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Card));
        sb.AddOrderByField(false, "priority");
        SqlStatement stmt = sb.GetStatement(true);
        IList cards = ObjectFactory.GetCollection(typeof(Card), stmt.Execute());
        for (int i = 0; i < cards.Count; ++i)
        {
          Card card = (Card)cards[i];
          string cardType = "";
          if (cardTypes.ContainsKey(card.DevicePath))
          {
            cardType = cardTypes[card.DevicePath].ToString();
          }
          ListViewItem item = mpListView1.Items.Add("", 0);
          item.SubItems.Add(card.Priority.ToString());
          if (card.Enabled)
          {
            item.Checked = true;
            item.Font = new Font(item.Font, FontStyle.Regular);
            item.Text = "Yes";
          }
          else
          {
            item.Checked = false;
            item.Font = new Font(item.Font, FontStyle.Strikeout);
            item.Text = "No";
          }
          item.SubItems.Add(cardType);

          if (card.CAM)
          {
            item.SubItems.Add("Yes");
          }
          else
          {
            item.SubItems.Add("No");
          }
          if (cardType.ToLower().Contains("dvb") || cardType.ToLower().Contains("atsc"))//CAM limit doesn't apply to non-digital cards
            item.SubItems.Add(card.DecryptLimit.ToString());
          else
            item.SubItems.Add("");
          item.SubItems.Add(card.Name);
          //check if card is really available before setting to enabled.
          bool cardPresent = RemoteControl.Instance.CardPresent(card.IdCard);
          if (!cardPresent)
          {
            item.SubItems.Add("No");
          }
          else
          {
            item.SubItems.Add("Yes");
          }
          if (cardType.ToLower().Contains("dvb") || cardType.ToLower().Contains("atsc"))//CAM limit doesn't apply to non-digital cards
          {
            if (!card.GrabEPG)
            {
              item.SubItems.Add("No");
            }
            else
            {
              item.SubItems.Add("Yes");
            }
          }
          else
          {
            item.SubItems.Add("");
          }
          item.Tag = card;
        }
      }
      catch (Exception)
      {
        MessageBox.Show(this, "Unable to access service. Is the TvService running??");
      }
      ReOrder();
      UpdateHybrids();
      checkWinTVCI();
      UpdateMenu();
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    void checkWinTVCI()
    {
      //check if the hauppauge wintv usb CI module is installed
      DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      DsDevice usbWinTvDevice = null;
      for (int capIndex = 0; capIndex < capDevices.Length; capIndex++)
      {
        if (capDevices[capIndex].Name != null)
        {
          if (capDevices[capIndex].Name.ToLower() == "wintvciusbbda source")
          {
            usbWinTvDevice = capDevices[capIndex];
            break;
          }
        }
      }
      if (usbWinTvDevice == null)
      {
        if (usbWINTV_tabpage == null)
        {
          usbWINTV_tabpage = tabControl1.TabPages[2];
          tabControl1.TabPages.RemoveAt(2);
        }
      }
      else if (usbWINTV_tabpage != null)
      {
        tabControl1.TabPages.Insert(2, usbWINTV_tabpage);
      }
    }

    private void buttonUp_Click(object sender, EventArgs e)
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
      _needRestart = true;
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      if (mpListView1.Items.Count < 2) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mpListView1.Items[index];
        mpListView1.Items.RemoveAt(index);
        if (index + 1 < mpListView1.Items.Count)
          mpListView1.Items.Insert(index + 1, item);
        else
          mpListView1.Items.Add(item);
      }
      ReOrder();
      mpListView1.EndUpdate();
      _needRestart = true;
    }

    void ReOrder()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].SubItems[1].Text = (mpListView1.Items.Count - i).ToString();

        Card card = (Card)mpListView1.Items[i].Tag;
        card.Priority = mpListView1.Items.Count - i;
        if (card.Enabled != mpListView1.Items[i].Checked)
          _needRestart = true;

        card.Enabled = mpListView1.Items[i].Checked;
        card.Persist();
      }
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (e.Item.Checked)
        e.Item.Font = new Font(e.Item.Font, FontStyle.Regular);
      else
        e.Item.Font = new Font(e.Item.Font, FontStyle.Strikeout);
      buttonEdit.Enabled = e.Item.Checked;
    }

    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enabled = mpListView1.SelectedItems.Count == 1;
      if (enabled)
      {
        Card card = (Card)mpListView1.SelectedItems[0].Tag;
        enabled = !RemoteControl.Instance.CardPresent(card.IdCard);
      }
      if (mpListView1.SelectedItems.Count == 1)
      {
        string cardType = mpListView1.SelectedItems[0].SubItems[2].Text.ToLower();
        if (mpListView1.SelectedItems[0].Checked && (cardType.Contains("dvb") || cardType.Contains("atsc") || cardType.Contains("analog"))) // Only some cards can be edited
          buttonEdit.Enabled = true;
        else
          buttonEdit.Enabled = false;
      }
      buttonRemove.Enabled = enabled;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    void UpdateHybrids()
    {
      treeView1.Nodes.Clear();
      IList cardGroups = CardGroup.ListAll();
      foreach (CardGroup group in cardGroups)
      {
        TreeNode node = treeView1.Nodes.Add(group.Name);
        node.Tag = group;
        IList cards = group.CardGroupMaps();
        foreach (CardGroupMap map in cards)
        {
          Card card = map.ReferringCard();
          TreeNode cardNode = node.Nodes.Add(card.Name);
          cardNode.Tag = card;
        }
      }
    }

    private void deleteCardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TreeNode node = treeView1.SelectedNode;
      if (node == null) return;
      Card card = node.Tag as Card;
      if (card == null) return;
      CardGroup group = node.Parent.Tag as CardGroup;
      IList cards = group.CardGroupMaps();
      foreach (CardGroupMap map in cards)
      {
        if (map.IdCard == card.IdCard)
        {
          map.Remove();
          break;
        }
      }
      UpdateHybrids();
      RemoteControl.Instance.Restart();
    }

    private void deleteEntireHybridCardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TreeNode node = treeView1.SelectedNode;
      if (node == null) return;
      CardGroup group = node.Tag as CardGroup;
      if (group == null) return;
      group.Delete();
      UpdateHybrids();
      RemoteControl.Instance.Restart();
    }

    private void buttonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes == null || indexes.Count == 0) return;      
      ListViewItem item = mpListView1.Items[indexes[0]];
      ReOrder();
      UpdateList();
      FormEditCard dlg = new FormEditCard();
      dlg.Card = (Card)item.Tag;
      dlg.CardType = cardTypes[((Card)item.Tag).DevicePath].ToString();
      dlg.ShowDialog();
      dlg.Card.Persist();
      _needRestart = true;
      UpdateList();
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
      Card card = (Card)mpListView1.SelectedItems[0].Tag;
      mpListView1.Items.Remove(mpListView1.SelectedItems[0]);
      RemoteControl.Instance.CardRemove(card.IdCard);
    }

    private void mpComboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)mpComboBoxCard.SelectedItem;
      int _winTVCardNumber = info.card.IdCard;
      string _winTVCardName = info.card.Name;
      string _winTVMoniker = info.card.DevicePath;
      SaveWinTVSettings(_winTVCardNumber, _winTVCardName, _winTVMoniker);
    }

    private void linkLabelHybridCard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      string Url = "http://www.team-mediaportal.com/manual/TV-Server/Configuration/TVServers/HybridCards";
      System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(Url);
      System.Diagnostics.Process.Start(sInfo);
    }
  }
}
