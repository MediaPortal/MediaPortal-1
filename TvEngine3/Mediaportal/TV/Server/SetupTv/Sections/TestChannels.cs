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

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

#endregion

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TestChannels : SectionSettings
  {
    private DateTime _lastTune;
    private readonly Dictionary<string, bool> _users = new Dictionary<string, bool>();
    private double _avg;
    private IList<Tuner> _tuners;
    private Dictionary<int, string> _channelNames;
    private int _concurrentTunes;
    private int _failed;
    private int _firstFail;
    private readonly object _lock = new object();
    private bool _repeat = true;
    private int _rndFrom;
    private int _rndTo;
    private bool _running;
    private int _succeeded;
    private int _ignored;
    private int _total;
    private int _tunedelay;
    private bool _usersShareChannels;
    private readonly object _listViewLock = new object();
    private bool _rndPrio;

    public TestChannels()
      : base("Test Channels")
    {
      InitializeComponent();
      DoubleBuffered = true;
    }

    public override void OnSectionActivated()
    {
      _tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerRelation.None);
      base.OnSectionActivated();

      comboBoxGroups.Items.Clear();
      IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroups(ChannelGroupRelation.None);
      foreach (ChannelGroup group in groups)
        comboBoxGroups.Items.Add(new ComboBoxExItem(group.GroupName, -1, group.IdGroup));
      if (comboBoxGroups.Items.Count == 0)
        comboBoxGroups.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      comboBoxGroups.SelectedIndex = 0;

      timer1.Enabled = true;

      mpListView1.Items.Clear();
      _repeat = chkRepeatTest.Checked;

      _channelNames = new Dictionary<int, string>();
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels(ChannelRelation.None);
      foreach (Channel ch in channels)
      {
        _channelNames.Add(ch.IdChannel, ch.Name);
      }

      _rndFrom = txtRndFrom.Value;
      _rndTo = txtRndTo.Value;

      _rndPrio = chkRndPrio.Checked;
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      timer1.Enabled = false;
    }

    /// <summary>
    ///   Splits a <see cref = "List{T}" /> into multiple chunks.
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    /// <param name = "list">The list to be chunked.</param>
    /// <param name="source"></param>
    /// <param name = "chunkSize">The size of each chunk.</param>
    /// <returns>A list of chunks.</returns>
    private static List<List<T>> SplitIntoChunks<T>(List<T> source, int chunkSize)
    {
      try
      {
        if (chunkSize <= 0)
        {
          Log.Debug("chunkSize must be greater than 0.");
          return null;
          //throw new ArgumentException("chunkSize must be greater than 0.");
        }

        var retVal = new List<List<T>>();
        int index = 0;
        while (index < source.Count)
        {
          int count = source.Count - index > chunkSize ? chunkSize : source.Count - index;
          retVal.Add(source.GetRange(index, count));

          index += chunkSize;
        }
        return retVal;
      }
      catch (Exception) { }
      return null;
    }

    private void ChannelTestThread(List<Channel> channelsO)
    {
      var rnd = new Random();

      while (_running)
      {
        try
        {
          List<List<Channel>> channelChunks = null;

          if (_usersShareChannels)
          {
            channelChunks = new List<List<Channel>>();

            for (int i = 0; i < _concurrentTunes; i++)
            {
              channelChunks.Add(channelsO);
            }
          }
          else
          {
            channelChunks = SplitIntoChunks(channelsO, (int)Decimal.Floor(channelsO.Count / _concurrentTunes));
          }

          for (int i = 0; i < _concurrentTunes; i++)
          {
            if (channelChunks != null && channelChunks.Count >= i + 1)
            {
              IEnumerable<Channel> channelsForUser = channelChunks[i];
              channelsForUser = channelsForUser.Randomize();

              int priority = GetUserPriority();
              string name = "stress-" + Convert.ToString(rnd.Next(1, 500)) + " [" + priority + "]";              
              IUser user = UserFactory.CreateBasicUser(name, priority);

              while (_users.ContainsKey(user.Name))
              {
                user.Name = "stress-" + Convert.ToString(rnd.Next(1, 500)) + " [" + priority + "]";
              }

              _users.Add(user.Name, true);
              ThreadPool.QueueUserWorkItem(delegate { TuneChannelsForUser(user, channelsForUser); });
              Thread.Sleep(500);
            }
          }

          while (true)
          {
            int nrOfBusy = 0;

            foreach (KeyValuePair<string, bool> kvp in _users)
            {
              bool isCurrentBusy = kvp.Value;
              if (isCurrentBusy)
              {
                nrOfBusy++;
              }
            }

            if (nrOfBusy == 0)
            {
              _users.Clear();
              break;
            }
            Thread.Sleep(100);
            Application.DoEvents();
          }
        }
        finally
        {
          if (!_repeat)
          {
            _running = false;
          }
        }
      }
    }

    private int GetUserPriority()
    {
      int rndPrio;
      if (_rndPrio)
      {
        var rnd = new Random();
        rndPrio = rnd.Next(1, 10);
      }
      else
      {
        rndPrio = UserFactory.DEFAULT_PRIORITY_OTHER;  
      }
      return rndPrio;
    }

    private void mpButtonTimeShift_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;

      _running = !_running;

      UpdateButtonCaption();

      if (_running)
      {
        mpListViewLog.Items.Clear();
        txtDisc.Value = 0;
        _total = 0;
        _succeeded = 0;
        _ignored = 0;
        _failed = 0;
        _avg = 0;
        _firstFail = 0;
        UpdateCounters();

        ComboBoxExItem idItem = (ComboBoxExItem)comboBoxGroups.Items[comboBoxGroups.SelectedIndex];
        List<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByGroupId(idItem.Id, ChannelRelation.None).ToList();
        Thread channelTestThread = new Thread(new ParameterizedThreadStart(delegate { ChannelTestThread(channels); }));
        channelTestThread.Name = "Channel Test Thread";
        channelTestThread.IsBackground = true;
        channelTestThread.Priority = ThreadPriority.Lowest;
        _usersShareChannels = chkShareChannels.Checked;
        _tunedelay = txtTuneDelay.Value;
        _concurrentTunes = txtConcurrentTunes.Value;
        channelTestThread.Start(channels);
      }
    }

    private void TuneChannelsForUser(IUser user, IEnumerable<Channel> channels)
    {
      int nextRowIndexForDiscUpdate = -1;
      try
      {
        _users[user.Name] = true;
        foreach (Channel ch in channels)
        {
          if (!_running)
          {
            break;
          }
          TuneChannel(ch, ref user, ref nextRowIndexForDiscUpdate);
        }
      }
      finally
      {
        try
        {
          if (chkSynch.Checked)
          {
            lock (_lock)
            {
              UpdateDiscontinuityCounter(user, nextRowIndexForDiscUpdate);
              ServiceAgents.Instance.ControllerServiceAgent.StopTimeShifting(user.Name, out user);
            }
          }
          else
          {
            UpdateDiscontinuityCounter(user, nextRowIndexForDiscUpdate);
            ServiceAgents.Instance.ControllerServiceAgent.StopTimeShifting(user.Name, out user);
          }
        }
        catch (Exception) {}
        if (user != null)
        {
          _users[user.Name] = false;
        }
      }
    }

    private void UpdateButtonCaption()
    {
      if (_running)
      {
        mpButtonTimeShift.Text = "Stop test";
      }
      else
      {
        mpButtonTimeShift.Text = "Start test";
      }
    }

    private void TuneChannel(Channel channel, ref IUser user, ref int nextRowIndexForDiscUpdate)
    {
      if (!_running)
      {
        return;
      }

      var rnd = new Random();
      if (channel != null)
      {
        long mSecsElapsed = 0;
        try
        {
          if (_tunedelay > 0)
          {
            while (true)
            {
              TimeSpan ts = DateTime.Now - _lastTune;

              if (ts.TotalMilliseconds < _tunedelay)
              {
                Thread.Sleep(100);
              }
              else
              {
                break;
              }
            }
          }

          _lastTune = DateTime.Now;

          IVirtualCard card = null;
          TvResult result;
          if (chkSynch.Checked)
          {
            lock (_lock)
            {
              user = StartTimeshifting(channel, user, nextRowIndexForDiscUpdate, out mSecsElapsed, out result, out card);
            }
          }
          else
          {
            user = StartTimeshifting(channel, user, nextRowIndexForDiscUpdate, out mSecsElapsed, out result, out card);
          }
          if (result == TvResult.Succeeded)
          {
            int cardId = -1;
            if (card != null)
            {
              cardId = card.Id;
            }
            nextRowIndexForDiscUpdate = Add2Log("OK", channel.Name, mSecsElapsed, user.Name,
                                                Convert.ToString(cardId), "");
            user.CardId = cardId;
            _succeeded++;
          }
          else if (result == TvResult.CardIsDisabled ||
                   result == TvResult.AllCardsBusy ||
                   result == TvResult.CardIsDisabled ||
                   result == TvResult.ChannelNotMappedToAnyCard ||
                   result == TvResult.TuneCancelled ||
                   result == TvResult.ChannelNotActive
            )
          {
            string err = GetErrMsgFromTVResult(result);
            nextRowIndexForDiscUpdate = -1;
            nextRowIndexForDiscUpdate = Add2Log("INF", channel.Name, mSecsElapsed, user.Name, "N/A", err);
            _ignored++;
          }
          else
          {
            string err = GetErrMsgFromTVResult(result);
            nextRowIndexForDiscUpdate = -1;
            if (_firstFail == 0 && _running)
            {
              _firstFail = mpListViewLog.Items.Count + 1;
            }
            nextRowIndexForDiscUpdate = Add2Log("ERR", channel.Name, mSecsElapsed, user.Name,
                                                Convert.ToString(user.FailedCardId), err);
            _failed++;
          }
        }
        catch (Exception e)
        {
          nextRowIndexForDiscUpdate = Add2Log("EXC", channel.Name, mSecsElapsed, user.Name,
                                              Convert.ToString(user.FailedCardId), e.Message);
          _ignored++;
          if (_firstFail == 0 && _running)
          {
            _firstFail = _total + 2;
          }
        }
        finally
        {
          if (_running)
          {
            _total++;
          }
          UpdateCounters();
          
          Thread.Sleep(rnd.Next(_rndFrom, _rndTo));
        }
      }
    }

    private IUser StartTimeshifting(Channel channel, IUser user, int nextRowIndexForDiscUpdate, out long mSecsElapsed,
                                    out TvResult result, out IVirtualCard card)
    {
      Stopwatch sw = Stopwatch.StartNew();
      UpdateDiscontinuityCounter(user, nextRowIndexForDiscUpdate);
      result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(user.Name, channel.IdChannel, out card, out user, user.Priority);
      mSecsElapsed = sw.ElapsedMilliseconds;
      _avg += mSecsElapsed;
      return user;
    }

    private string GetErrMsgFromTVResult(TvResult result)
    {
      string err = "";
      switch (result)
      {
        case TvResult.NoPmtFound:
          err = "No PMT found";
          break;
        case TvResult.NoSignalDetected:
          err = "No signal";
          break;
        case TvResult.CardIsDisabled:
          err = "Card is not enabled";
          break;
        case TvResult.AllCardsBusy:
          err = "All cards are busy";
          break;
        case TvResult.ChannelIsScrambled:
          err = "Channel is scrambled";
          break;
        case TvResult.NoVideoAudioDetected:
          err = "No Video/Audio detected";
          break;
        case TvResult.UnableToStartGraph:
          err = "Unable to create/start graph";
          break;
        case TvResult.ChannelNotMappedToAnyCard:
          err = "Channel is not mapped to any card";
          break;
        case TvResult.NoTuningDetails:
          err = "No tuning information available for this channel";
          break;
        case TvResult.UnknownChannel:
          err = "Unknown channel";
          break;
        case TvResult.UnknownError:
          err = "Unknown error occured";
          break;
        case TvResult.GraphBuildingFailed:
          err = "Unable to create graph";
          break;
        case TvResult.SWEncoderMissing:
          err = "No suppported software encoder installed";
          break;
        case TvResult.NoFreeDiskSpace:
          err = "No free disk space";
          break;
        case TvResult.TuneCancelled:
          err = "Tune cancelled";
          break;
        case TvResult.ChannelNotActive:
          err = "Channel not active";
          break;
      }
      return err;
    }

    private void UpdateDiscontinuityCounter(IUser user, int nextRowIndexForDiscUpdate)
    {
      if (_running)
      {
        if (InvokeRequired)
        {
          Invoke(new UpdateDiscontinuityCounterDelegate(UpdateDiscontinuityCounter),
                 new object[] {user, nextRowIndexForDiscUpdate});
          return;
        }
      }

      if (nextRowIndexForDiscUpdate > 0)
      {
        ListViewItem item = mpListViewLog.Items[nextRowIndexForDiscUpdate - 1];
        if (user.CardId > 0)
        {
          int discCounter = 0;
          int totalBytes = 0;
          ServiceAgents.Instance.ControllerServiceAgent.GetStreamQualityCounters(user.Name, out totalBytes, out discCounter);
          item.SubItems[7].Text = Convert.ToString(discCounter);

          txtDisc.Value += discCounter;
        }
        else
        {
          item.SubItems[7].Text = "N/A";
        }
      }
    }

    private int Add2Log(string state, string channel, double msec, string name, string card, string details)
    {
      int itemNr = 0;
      if (_running)
      {
        if (InvokeRequired)
        {
          return (int)Invoke(new Add2LogDelegate(Add2Log), new object[] {state, channel, msec, name, card, details});
        }

        lock (_listViewLock)
        {
          DateTime time = DateTime.Now;
          itemNr = mpListViewLog.Items.Count + 1;
          ListViewItem item = mpListViewLog.Items.Add(Convert.ToString(itemNr));
          item.SubItems.Add(time.ToLongTimeString());
          item.SubItems.Add(state);
          item.SubItems.Add(channel);
          item.SubItems.Add(Convert.ToString(msec));
          item.SubItems.Add(name);
          item.SubItems.Add(card);
          item.SubItems.Add("wait..");
          item.SubItems.Add(details);
        }
      }
      return itemNr;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable)
      {
        mpListView1.Items.Clear();
        _running = false;
      }

      UpdateButtonCaption();

      mpButtonTimeShift.Enabled = ServiceHelper.IsAvailable;
      comboBoxGroups.Enabled = ServiceHelper.IsAvailable;

      UpdateCardStatus();
    }

    private void UpdateCounters()
    {
      if (_running)
      {
        txtIgnored.Value = _ignored;
        txtSucceded.Value = _succeeded;
        txtFailed.Value = _failed;
        txtTotal.Value = _total;
        txtFirstFail.Value = _firstFail;
        if (_avg > 0 && _total > 0)
        {
          txtAvgMsec.Value = Convert.ToInt32(_avg / _total);
        }
        Application.DoEvents();
        this.LogDebug("TestChannels: Succeeded={0}", _succeeded);
        this.LogDebug("TestChannels: Failed={0}", _failed);
        this.LogDebug("TestChannels: Ignored={0}", _ignored);
        this.LogDebug("TestChannels: Total={0}", _total);
        this.LogDebug("TestChannels: Avg mSec={0}", txtAvgMsec.Value);
        this.LogDebug("TestChannels: First Fail={0}", _firstFail);
      }
    }

    private void UpdateCardStatus()
    {
      if (_tuners == null)
      {
        return;
      }
      if (_tuners.Count == 0)
      {
        return;
      }

      Utils.UpdateCardStatus(mpListView1);
    }

    private void txtRndFrom_TextChanged(object sender, EventArgs e)
    {
      _rndFrom = txtRndFrom.Value;
    }

    private void txtRndTo_TextChanged(object sender, EventArgs e)
    {
      _rndTo = txtRndTo.Value;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      var buffer = new StringBuilder();

      buffer.Append(lblSucceeded.Text + txtSucceded.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblAvgMsec.Text + txtAvgMsec.Text + " msec");
      buffer.Append(Environment.NewLine);

      buffer.Append(lblFailed.Text + txtFailed.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblFirstFail.Text + txtFirstFail.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblIgnored.Text + txtIgnored.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblTotal.Text + txtTotal.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblDisc.Text + txtDisc.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblNrOfConcurrentUsers.Text + txtConcurrentTunes.Text);
      buffer.Append(Environment.NewLine);

      buffer.Append(chkRndPrio.Text + ":" + chkRndPrio.Checked);
      buffer.Append(Environment.NewLine);

      buffer.Append(lblTuneDelayMsec.Text + txtTuneDelay.Text + " msec");
      buffer.Append(Environment.NewLine);

      buffer.Append(lblEachTuneWillLast.Text + txtRndFrom.Text + " - " + txtRndTo.Text + " msec");
      buffer.Append(Environment.NewLine);

      buffer.Append(chkShareChannels.Text + ":" + chkShareChannels.Checked);
      buffer.Append(Environment.NewLine);

      buffer.Append(chkRepeatTest.Text + ":" + chkRepeatTest.Checked);
      buffer.Append(Environment.NewLine);

      buffer.Append(chkSynch.Text + ":" + chkSynch.Checked);
      buffer.Append(Environment.NewLine);

      buffer.Append(Environment.NewLine);

      for (int i = 0; i < mpListViewLog.Columns.Count; i++)
      {
        buffer.Append(mpListViewLog.Columns[i].Text);
        buffer.Append("\t");
      }

      buffer.Append(Environment.NewLine);

      for (int i = 0; i < mpListViewLog.Items.Count; i++)
      {
        for (int j = 0; j < mpListViewLog.Columns.Count; j++)
        {
          buffer.Append(mpListViewLog.Items[i].SubItems[j].Text);
          buffer.Append("\t");
        }

        buffer.Append(Environment.NewLine);
      }

      Clipboard.SetText(buffer.ToString());
    }

    private void chkRepeatTest_CheckedChanged(object sender, EventArgs e)
    {
      _repeat = chkRepeatTest.Checked;
    }

    private void mpListViewLog_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (_running)
      {
        return;
      }

      var listView = mpListViewLog as ListView;

      if (listView == null)
      {
        return;
      }

      ListViewSorter Sorter = null;
      if (listView.ListViewItemSorter == null)
      {
        Sorter = new ListViewSorter();
        listView.ListViewItemSorter = Sorter;
      }
      else
      {
        Sorter = (ListViewSorter)listView.ListViewItemSorter;
      }


      if (Sorter.LastSort == e.Column)
      {
        if (listView.Sorting == SortOrder.Ascending)
        {
          listView.Sorting = SortOrder.Descending;
        }
        else
        {
          listView.Sorting = SortOrder.Ascending;
        }
      }
      else
      {
        listView.Sorting = SortOrder.Descending;
      }

      Sorter.LastSort = e.Column;
      Sorter.ByColumn = e.Column;

      listView.Sort();
    }

    #region Nested type: Add2LogDelegate

    private delegate int Add2LogDelegate(
      string state, string channel, double msec, string name, string card, string details);

    private delegate void UpdateDiscontinuityCounterDelegate(User user, int nextRowIndexForDiscUpdate);

    #endregion

    private void chkRndPrio_CheckedChanged(object sender, EventArgs e)
    {
      _rndPrio = chkRndPrio.Checked;
    }
  }

  public class ListViewSorter : IComparer
  {
    public int ByColumn { get; set; }

    public int LastSort { get; set; }

    #region IComparer Members

    public int Compare(object o1, object o2)
    {
      if (!(o1 is ListViewItem))
        return (0);
      if (!(o2 is ListViewItem))
        return (0);

      ListViewItem lvi1 = (ListViewItem)o2;
      string str1 = lvi1.SubItems[ByColumn].Text;
      ListViewItem lvi2 = (ListViewItem)o1;
      string str2 = lvi2.SubItems[ByColumn].Text;

      int result;
      if (lvi1.ListView.Sorting == SortOrder.Ascending)
        result = String.Compare(str1, str2);
      else
        result = String.Compare(str2, str1);

      LastSort = ByColumn;

      return (result);
    }

    #endregion
  }
}