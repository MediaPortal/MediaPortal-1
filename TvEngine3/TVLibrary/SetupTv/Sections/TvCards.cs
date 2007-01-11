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
using System.Windows.Forms;
using TvControl;


using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
namespace SetupTv.Sections
{
  public partial class TvCards : SectionSettings
  {
    bool _needRestart = false;
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
      }
      UpdateHybrids();
      RemoteControl.Instance.Restart();

    }
    public override void OnSectionDeActivated()
    {
      ReOrder();
      if (_needRestart)
      {
        RemoteControl.Instance.Restart();
      }
      base.OnSectionDeActivated();
    }
    public override void OnSectionActivated()
    {
      _needRestart = false;
      base.OnSectionActivated();
      mpListView1.Items.Clear();
      Dictionary<string, CardType> cardTypes = new Dictionary<string, CardType>();
      try
      {
        IList dbsCards = Card.ListAll();
        foreach (Card card in dbsCards)
        {
          cardTypes[card.DevicePath] = RemoteControl.Instance.Type(card.IdCard);
        }
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
          ListViewItem item = mpListView1.Items.Add("",0);
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
          item.SubItems.Add(card.Name);
          item.Tag = card;
        }
      }
      catch (Exception)
      {
        MessageBox.Show(this,"Unable to access service. Is the TvService running??");
      }
      ReOrder();
      UpdateHybrids();
      UpdateMenu();
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
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index > 0 &&  index + 1 < mpListView1.Items.Count)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index + 1, item);
        }
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
    }

    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {

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
          TreeNode cardNode=node.Nodes.Add(card.Name);
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
  }
}