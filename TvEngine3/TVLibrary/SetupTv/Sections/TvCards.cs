using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;
namespace SetupTv.Sections
{
  public partial class TvCards : SectionSettings
  {
    public TvCards()
      : this("TV Cards")
    {
    }

    public TvCards(string name)
      : base(name)
    {
      InitializeComponent();
    }
    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpListView1.Items.Clear();
      Dictionary<string, CardType> cardTypes = new Dictionary<string, CardType>();
      try
      {
        EntityList<Card> dbsCards = DatabaseManager.Instance.GetEntities<Card>();
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
        EntityQuery query = new EntityQuery(typeof(Card));
        query.AddOrderBy(Card.PriorityEntityColumn, ListSortDirection.Descending);

        EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>(query);
        for (int i = 0; i < cards.Count; ++i)
        {
          string cardType = "";
          if (cardTypes.ContainsKey(cards[i].DevicePath))
          {
            cardType = cardTypes[cards[i].DevicePath].ToString();
          }
          ListViewItem item = mpListView1.Items.Add(cards[i].Priority.ToString());
          item.SubItems.Add(cardType);
          item.SubItems.Add(cards[i].Name);
          item.Tag = cards[i];
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Unable to access service. Is the TvService running??");
      }
      ReOrder();
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
        mpListView1.Items[i].Text = (mpListView1.Items.Count - i).ToString();

        Card card = (Card)mpListView1.Items[i].Tag;
        card.Priority = mpListView1.Items.Count - i;
      }
    }
  }
}