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
using System.Text;
using System.Windows.Forms;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using System.Threading;
using SetupControls;

namespace SetupTv.Sections.Helpers
{
  /// <summary>
  /// Class that handles populating and filtering of a listview with
  /// Channels (Radio or TV)
  /// </summary>
  internal class ChannelListViewHandler
  {
    private const int MS_SLEEP_BEFORE_FILTERING = 150;

    internal ListView _listView = null; //the listview control that displays the items
    internal IList<Channel> _allChannels = null; //all available channels
    internal Dictionary<int, CardType> _allCards = null; //all available cards
    internal TextBox _currentText = null; //the textbox that contains the text for filtering
    private Thread _fillListViewThread; //the currently active thread
    private ChannelType _type; //the type of the texbox, currently that's tv or radio
    private Dictionary<int, ListViewItem> _listViewCache; //A list of allready created listviewitems

    /// <summary>
    /// Creates a new ChannelListView Handler
    /// </summary>
    /// <param name="listView"></param>
    /// <param name="channels"></param>
    /// <param name="allCards"></param>
    /// <param name="textBox"></param>
    internal ChannelListViewHandler(ListView listView, IList<Channel> channels, Dictionary<int, CardType> allCards,
                                    TextBox textBox, ChannelType type)
    {
      _listView = listView;
      _allChannels = channels;
      _allCards = allCards;
      _currentText = textBox;
      _type = type;
      _listViewCache = new Dictionary<int, ListViewItem>();
    }

    /// <summary>
    /// Start filtering the list in a thread
    /// </summary>
    /// <param name="searchText"></param>
    internal void FilterListView(String searchText)
    {
      _fillListViewThread = new Thread(new ParameterizedThreadStart(FillListViewChannels));
      _fillListViewThread.Priority = ThreadPriority.BelowNormal;
      _fillListViewThread.Start(searchText);
    }

    /// <summary>
    /// Indicates if the listview is currently in the process of being populated
    /// </summary>
    internal bool PopulateRunning { get; set; }

    /// <summary>
    /// Fill the listview with all channels that fit the criteria (searchname)
    /// </summary>
    private void FillListViewChannels(object filterObject)
    {
      if (filterObject == null) return;

      String filterText = ((String)filterObject).ToLower();

      //Sleep for MS_SLEEP_BEFORE_FILTERING ms before starting filtering (to prevent searches while
      //user is still typing)
      Thread.Sleep(MS_SLEEP_BEFORE_FILTERING);

      Application.UseWaitCursor = true;
      PopulateRunning = true;

      if (InvokeHasTextChanged(filterText))
      {
//After waiting for MS_SLEEP_BEFORE_FILTERING, the search text isn't valid anymore (user changed text) -> return
        Application.UseWaitCursor = false;
        return;
      }

      try
      {
        Log.Debug("Filter listview for " + filterText);
        _listView.Invoke(new MethodInvoker(delegate()
                                             {
                                               _listView.Items.Clear();
                                               _listView.BeginUpdate();
                                             }));

        List<ListViewItem> items = new List<ListViewItem>();
        for (int i = 0; i < _allChannels.Count; i++)
        {
          if (InvokeHasTextChanged(filterText))
          {
//the search term changed while we were filtering
            Log.Debug("Cancel filtering for " + filterText);
            break;
          }

          Channel ch = _allChannels[i];

          if (ch.DisplayName != null &&
              (filterText.Equals("") || ContainsCaseInvariant(ch.DisplayName, filterText)))
          {
            _listView.Invoke(new MethodInvoker(delegate() { items.Add(CreateListViewItemForChannel(ch, _allCards)); }));
          }
        }


        if (!InvokeHasTextChanged(filterText))
        {
//after filtering is done the filter is still valid
          _listView.Invoke(new MethodInvoker(delegate()
                                               {
                                                 _listView.Items.Clear();
                                                 _listView.Items.AddRange(items.ToArray());
                                                 _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                                               }));
          Log.Debug("Finished filtering " + items.Count + " items for " + filterText);
        }
        else
        {
          return;
        }
      }
      catch (Exception exp)
      {
        Log.Error("RefreshAllChannels error: {0}", exp.StackTrace);
      }
      finally
      {
        _listView.Invoke(new MethodInvoker(delegate()
                                             {
                                               _listView.EndUpdate();
                                               Application.UseWaitCursor = false;
                                             }));
        PopulateRunning = false;
      }
    }

    /// <summary>
    /// Checks if the text of the filter textbox is still the same as the text we're filtering for
    /// </summary>
    /// <returns>True if text has changed, false otherwise</returns>
    private bool InvokeHasTextChanged(String filterText)
    {
      bool textChanged = false;
      _currentText.Invoke(
        new MethodInvoker(
          delegate() { textChanged = !_currentText.Text.Equals(filterText, StringComparison.InvariantCultureIgnoreCase); }));
      return textChanged;
    }

    /// <summary>
    /// Checks if text 2 is contained in text 1 with casing
    /// being ignored
    /// </summary>
    /// <param name="text1">Full text</param>
    /// <param name="text2">Text we search for</param>
    private bool ContainsCaseInvariant(String text1, String text2)
    {
      return (text1.IndexOf(text2, StringComparison.InvariantCultureIgnoreCase) >= 0);
    }

    /// <summary>
    /// Create a listview item for a channel
    /// </summary>
    /// <param name="ch">Channel</param>
    /// <param name="cards">All available cards</param>
    /// <returns>Listview item representing the channel</returns>
    internal ListViewItem CreateListViewItemForChannel(Channel ch, Dictionary<int, CardType> cards)
    {
      if (_listViewCache.ContainsKey(ch.IdChannel)) return _listViewCache[ch.IdChannel];

      bool analog = false;
      bool dvbc = false;
      bool dvbt = false;
      bool dvbs = false;
      bool atsc = false;
      bool dvbip = false;
      bool webstream = false;
      bool notmapped = true;
      if (ch.IsWebstream())
      {
        webstream = true;
        notmapped = false;
      }
      if (notmapped)
      {
        IList<ChannelMap> maps = ch.ReferringChannelMap();
        foreach (ChannelMap map in maps)
        {
          if (cards.ContainsKey(map.IdCard))
          {
            CardType type = cards[map.IdCard];
            switch (type)
            {
              case CardType.Analog:
                analog = true;
                notmapped = false;
                break;
              case CardType.DvbC:
                dvbc = true;
                notmapped = false;
                break;
              case CardType.DvbT:
                dvbt = true;
                notmapped = false;
                break;
              case CardType.DvbS:
                dvbs = true;
                notmapped = false;
                break;
              case CardType.Atsc:
                atsc = true;
                notmapped = false;
                break;
              case CardType.DvbIP:
                dvbip = true;
                notmapped = false;
                break;
            }
          }
        }
      }
      ListViewItem item = new ListViewItem(ch.DisplayName);
      item.Checked = ch.VisibleInGuide;
      item.Tag = ch;
      item.SubItems.Add(ch.ChannelNumber.ToString());

      IList<string> groups = ch.GroupNames;
      List<string> groupNames = new List<string>();
      foreach (string groupName in groups)
      {
        if (groupName != TvConstants.TvGroupNames.AllChannels &&
            groupName != TvConstants.RadioGroupNames.AllChannels)
        {
//Don't add "All Channels"
          groupNames.Add(groupName);
        }
      }
      string group = String.Join(", ", groupNames.ToArray());
      item.SubItems.Add(group);

      List<string> providers = new List<string>();
      IList<TuningDetail> tuningDetails = ch.ReferringTuningDetail();
      bool hasFta = false;
      bool hasScrambled = false;
      foreach (TuningDetail detail in tuningDetails)
      {
        if (!providers.Contains(detail.Provider) && !String.IsNullOrEmpty(detail.Provider))
        {
          providers.Add(detail.Provider);
        }
        if (detail.FreeToAir)
        {
          hasFta = true;
        }
        if (!detail.FreeToAir)
        {
          hasScrambled = true;
        }
      }

      string provider = String.Join(", ", providers.ToArray());
      item.SubItems.Add(provider);

      int imageIndex = 0;
      if (_type == ChannelType.Tv)
      {
        if (hasFta && hasScrambled)
        {
          imageIndex = 5;
        }
        else if (hasScrambled)
        {
          imageIndex = 4;
        }
        else
        {
          imageIndex = 3;
        }
      }
      else if (_type == ChannelType.Radio)
      {
        if (hasFta && hasScrambled)
        {
          imageIndex = 2;
        }
        else if (hasScrambled)
        {
          imageIndex = 1;
        }
        else
        {
          imageIndex = 0;
        }
      }
      item.ImageIndex = imageIndex;

      StringBuilder builder = new StringBuilder();

      if (notmapped)
      {
        builder.Append("Channel not mapped to a card");
      }
      else
      {
        if (analog)
        {
          builder.Append("Analog");
        }
        if (dvbc)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("DVB-C");
        }
        if (dvbt)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("DVB-T");
        }
        if (dvbs)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("DVB-S");
        }
        if (atsc)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("ATSC");
        }
        if (dvbip)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-IP");
        }
        if (webstream)
        {
          if (builder.Length > 0)
            builder.Append(",");
          builder.Append("Webstream");
        }
      }

      item.SubItems.Add(builder.ToString());
      item.SubItems.Add(tuningDetails.Count.ToString());

      _listViewCache.Add(ch.IdChannel, item);

      return item;
    }
  }
}