#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using Gentle.Framework;
using SetupControls;

namespace SetupTv.Sections
{
  public partial class TestChannels : SectionSettings
  {    
    private IList<Card> _cards;
    private Dictionary<int, string> _channelNames;

    //Player _player;
    public TestChannels()
      : this("TestChannels")
    {
     
    }

    public TestChannels(string name)
      : base(name)
    {
      InitializeComponent();
      Init();
    }

    private void Init()
    {
      DoubleBuffered = true;
    }

    public override void OnSectionActivated()
    {
      _cards = Card.ListAll();
      base.OnSectionActivated();      
      RemoteControl.Instance.EpgGrabberEnabled = true;

      comboBoxGroups.Items.Clear();
      IList<ChannelGroup> groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
        comboBoxGroups.Items.Add(new ComboBoxExItem(group.GroupName, -1, group.IdGroup));
      if (comboBoxGroups.Items.Count == 0)
        comboBoxGroups.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      comboBoxGroups.SelectedIndex = 0;

      timer1.Enabled = true;

      mpListView1.Items.Clear();
      _repeat = chkRepeatTest.Checked;

      _channelNames = new Dictionary<int, string>();
      IList<Channel> channels = Channel.ListAll();
      foreach (Channel ch in channels)
      {
        _channelNames.Add(ch.IdChannel, ch.DisplayName);
      }

      _rndFrom = txtRndFrom.Value;
      _rndTo = txtRndTo.Value;

    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      timer1.Enabled = false;
      if (RemoteControl.IsConnected)
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
      }
    }

    private class ThreadParams
    {
      public User _user;
      public List<Channel> _channels;      
    } ;

    /// <summary>
    /// Splits a <see cref="List{T}"/> into multiple chunks.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list to be chunked.</param>
    /// <param name="chunkSize">The size of each chunk.</param>
    /// <returns>A list of chunks.</returns>
    public static List<List<T>> SplitIntoChunks<T>(List<T> source, int chunkSize)
    {
      if (chunkSize <= 0)
      {
        throw new ArgumentException("chunkSize must be greater than 0.");
      }

      List<List<T>> retVal = new List<List<T>>();
      int index = 0;
      while (index < source.Count)
      {
        int count = source.Count - index > chunkSize ? chunkSize : source.Count - index;
        retVal.Add(source.GetRange(index, count));

        index += chunkSize;
      }

      return retVal;
    }

    private bool _usersShareChannels = false;
    private bool _repeat = true;
    private bool _running = false;
    private object _lock = new object();
    private int _concurrentTunes = 0;
    Dictionary<string, bool> _users = new Dictionary<string, bool>();

    private Thread _channelTestThread = null;

    private void ChannelTestThread(List<Channel> channelsO)
    {      
      Random rnd = new Random();      

      while (_running)
      {
        try
        {          
          VirtualCard card;

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
            if (channelChunks.Count >= i + 1)
            {
              IEnumerable<Channel> channelsForUser = channelChunks[i];
              channelsForUser = channelsForUser.Randomize();

              User user = new User();
              user.Name = "setuptv-" + Convert.ToString(rnd.Next(1, 500));
              user.IsAdmin = false;
            
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

    private void mpButtonTimeShift_Click(object sender, EventArgs e)
    {
      if (ServiceHelper.IsStopped) return;      

      _running = !_running;
      
      UpdateButtonCaption();

      List<Channel> channelsO = null;

      if (_channelTestThread == null)
      {
        //_channelTestThread = new Thread(ChannelTestThread);
        _channelTestThread = new Thread(new ParameterizedThreadStart(delegate { ChannelTestThread(channelsO); }));        
        _channelTestThread.Name = "Channel Test Thread";
        _channelTestThread.IsBackground = true;
        _channelTestThread.Priority = ThreadPriority.Lowest;                
        
      }
      
      if (!_running)
      {        
        if (_channelTestThread.IsAlive)
        {
          _channelTestThread.Abort();
          _channelTestThread = null;
        }        
      }
      else
      {

        mpListViewLog.Items.Clear();
        _total = 0;
        _succeeded = 0;
        _failed = 0;
        _avg = 0;
        _firstFail = 0;
        UpdateCounters();

        IEnumerable<Channel> channels = new List<Channel>();
        ComboBoxExItem idItem = (ComboBoxExItem)comboBoxGroups.Items[comboBoxGroups.SelectedIndex];

        ChannelGroup group = ChannelGroup.Retrieve(idItem.Id);
        IList<GroupMap> maps = group.ReferringGroupMap();

        channelsO = channels as List<Channel>;
        foreach (GroupMap map in maps)
        {
          Channel ch = map.ReferencedChannel();
          if (ch.IsTv)
          {
            channelsO.Add(ch);
          }
        }
        _usersShareChannels = chkShareChannels.Checked;
        _tunedelay = txtTuneDelay.Value;
        _concurrentTunes = txtConcurrentTunes.Value;
        _channelTestThread.Start(channelsO);
      }      

    }

    private void TuneChannelsForUser(User user, IEnumerable<Channel> channels)
    {      
      try
      {
        _users[user.Name] = true;
        foreach (Channel ch in channels)
        {
          TuneChannel(ch, ref user);
        }
      }
      finally
      {
        try
        {
          lock (_lock)
          {
            RemoteControl.Instance.StopTimeShifting(ref user);
          }
        }
        catch (Exception e)
        {
        }
        _users[user.Name] = false;
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

    private int _tunedelay = 0;
    private int _succeeded = 0;
    private int _failed = 0;
    private int _total = 0;
    private int _rndTo = 0;
    private int _rndFrom = 0;
    private double _avg = 0;
    private int _firstFail = 0;
    private object _lastTuneLock = new object();
    private Dictionary<string, DateTime> _lastTunesList = new Dictionary<string, DateTime>();    

    private void TuneChannel(Channel channel, ref User user)
    {
      if (!_running)
      {
        return;
      }
                    
        Random rnd = new Random();
        if (channel != null)
        {
          VirtualCard card = new VirtualCard(user);
          TvResult result;
          long mSecsElapsed = 0;
          try
          {
            if (_tunedelay > 0)
            {
              while (true)
              {
                DateTime lastTune = DateTime.MinValue;
                _lastTunesList.TryGetValue(user.Name, out lastTune);
                TimeSpan ts = DateTime.Now - lastTune;

                if (ts.TotalMilliseconds < _tunedelay)
                {
                  //Log.Debug("tune delay");
                  Thread.Sleep(100);
                }
                else
                {
                  break;
                }
              }
            }
            lock (_lock)
            {
              Stopwatch sw = Stopwatch.StartNew();
              result = RemoteControl.Instance.StartTimeShifting(ref user, channel.IdChannel, out card);
              mSecsElapsed = sw.ElapsedMilliseconds;
              _avg += mSecsElapsed;
            }
            if (result == TvResult.Succeeded)
            {
              Add2Log("OK",channel.DisplayName, mSecsElapsed, user.Name, Convert.ToString(card.Id), "");
              user.CardId = card.Id;
              _succeeded++;              
            }
            else
            {
              string err = GetErrMsgFromTVResult(result);
              if (_firstFail == 0 && _running)
              {
                _firstFail = mpListViewLog.Items.Count + 1;
              }
              Add2Log("ERR", channel.DisplayName, mSecsElapsed, user.Name, "N/A", err);              
              _failed++;              
            }
          }
          catch (Exception e)
          {
            Add2Log("EXC", channel.DisplayName, mSecsElapsed, user.Name, "N/A", e.Message);              
            _succeeded++;
            if (_firstFail == 0 && _running)
            {
              _firstFail = _total+2;
            }
          }
          finally
          {
            if (_running)
            {
              _total++;
            }
            UpdateCounters();
            lock (_lastTuneLock)
            {
              _lastTunesList[user.Name] = DateTime.Now;
            }
            Thread.Sleep(rnd.Next(_rndFrom, _rndTo));
          }
        }      
    }

    private string GetErrMsgFromTVResult(TvResult result)
    {
      string err = "";
      switch (result)
      {
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
        case TvResult.ConnectionToSlaveFailed:
          err = "Cannot connect to slave server";
          break;
        case TvResult.NotTheOwner:
          err = "Failed since card is in use and we are not the owner";
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
      }
      return err;
    }

    private delegate void Add2LogDelegate(string state, string channel, double msec, string name, string card, string details);
    private void Add2Log(string state, string channel, double msec, string name, string card, string details)
    {
      if (_running)
      {
        if (this.InvokeRequired)
        {
          Invoke(new Add2LogDelegate(Add2Log), new object[] {state, channel, msec, name, card, details});
          return;
        }

        DateTime time = DateTime.Now;

        ListViewItem item = mpListViewLog.Items.Add(Convert.ToString(mpListViewLog.Items.Count+1));
        item.SubItems.Add(time.ToLongTimeString());
        item.SubItems.Add(state);
        item.SubItems.Add(channel);
        item.SubItems.Add(Convert.ToString(msec));
        item.SubItems.Add(name);
        item.SubItems.Add(card);
        item.SubItems.Add(details);
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsRunning)
      {
        mpListView1.Items.Clear();
        _running = false;
      }
      
      UpdateButtonCaption();      

      mpButtonTimeShift.Enabled = ServiceHelper.IsRunning;
      comboBoxGroups.Enabled = ServiceHelper.IsRunning;
      
      UpdateCardStatus();
    }

    private void UpdateCounters() 
    {
      if (_running)
      {
        txtSucceded.Value = _succeeded;
        txtFailed.Value = _failed;
        txtTotal.Value = _total;
        txtFirstFail.Value = _firstFail;
        if (_avg > 0 && _total > 0)
        {
          txtAvgMsec.Value = Convert.ToInt32(_avg / _total);
        }
        Application.DoEvents();
        Log.Debug("TestChannels: Succeeded={0}", _succeeded);
        Log.Debug("TestChannels: Failed={0}", _failed);
        Log.Debug("TestChannels: Total={0}", _total);
        Log.Debug("TestChannels: Avg mSec={0}", txtAvgMsec.Value);
        Log.Debug("TestChannels: First Fail={0}", _firstFail);
      }
    }

    private void UpdateCardStatus()
    {
      if (ServiceHelper.IsStopped) return;
      if (_cards == null) return;
      if (_cards.Count == 0) return;
      try
      {
        ListViewItem item;
        int cardNo = 0;
        int off = 0;
        foreach (Card card in _cards)
        {
          cardNo++;
          User user = new User();
          user.CardId = card.IdCard;
          if (off >= mpListView1.Items.Count)
          {
            item = mpListView1.Items.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
          }
          else
          {
            item = mpListView1.Items[off];
          }

          bool cardPresent = RemoteControl.Instance.CardPresent(card.IdCard);
          if (!cardPresent)
          {
            item.SubItems[0].Text = cardNo.ToString();
            item.SubItems[1].Text = "n/a";
            item.SubItems[2].Text = "n/a";
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }

          VirtualCard vcard = new VirtualCard(user);
          item.SubItems[0].Text = cardNo.ToString();
          item.SubItems[0].Tag = card.IdCard;
          item.SubItems[1].Text = vcard.Type.ToString();

          if (card.Enabled == false)
          {
            item.SubItems[0].Text = cardNo.ToString();
            item.SubItems[1].Text = vcard.Type.ToString();
            item.SubItems[2].Text = "disabled";
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }

          User[] usersForCard = RemoteControl.Instance.GetUsersForCard(card.IdCard);
          if (usersForCard == null)
          {
            string tmp = "idle";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            item.SubItems[2].Text = tmp;
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }
          if (usersForCard.Length == 0)
          {
            string tmp = "idle";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            item.SubItems[2].Text = tmp;
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
            continue;
          }


          bool userFound = false;
          for (int i = 0; i < usersForCard.Length; ++i)
          {
            string tmp = "idle";
            // Check if the card id fits. Hybrid cards share the context and therefor have
            // the same users.
            if (usersForCard[i].CardId != card.IdCard)
            {
              continue;
            }
            userFound = true;
            vcard = new VirtualCard(usersForCard[i]);
            item.SubItems[0].Text = cardNo.ToString();
            item.SubItems[0].Tag = card.IdCard;
            item.SubItems[1].Text = vcard.Type.ToString();
            if (vcard.IsTimeShifting) tmp = "Timeshifting";
            if (vcard.IsRecording && vcard.User.IsAdmin) tmp = "Recording";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            if (vcard.IsTimeShifting && vcard.IsGrabbingEpg) tmp = "Timeshifting (Grabbing EPG)";
            if (vcard.IsRecording && vcard.User.IsAdmin && vcard.IsGrabbingEpg) tmp = "Recording (Grabbing EPG)";
            item.SubItems[2].Text = tmp;
            tmp = vcard.IsScrambled ? "yes" : "no";
            item.SubItems[4].Text = tmp;
            string channelDisplayName;
            if (_channelNames.TryGetValue(vcard.IdChannel, out channelDisplayName))
            {
              item.SubItems[3].Text = channelDisplayName;
            }
            else
            {
              item.SubItems[3].Text = vcard.ChannelName;
            }
            item.SubItems[5].Text = usersForCard[i].Name;
            item.SubItems[6].Text = card.Name;
            off++;

            if (off >= mpListView1.Items.Count)
            {
              item = mpListView1.Items.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
              item.SubItems.Add("");
            }
            else
            {
              item = mpListView1.Items[off];
            }
          }
          // If we haven't found a user that fits, than it is a hybrid card which is inactive
          // This means that the card is idle.
          if (!userFound)
          {
            item.SubItems[2].Text = "idle";
            item.SubItems[3].Text = "";
            item.SubItems[4].Text = "";
            item.SubItems[5].Text = "";
            item.SubItems[6].Text = card.Name;
            off++;
          }
        }
        for (int i = off; i < mpListView1.Items.Count; ++i)
        {
          item = mpListView1.Items[i];
          item.SubItems[0].Text = "";
          item.SubItems[1].Text = "";
          item.SubItems[2].Text = "";
          item.SubItems[3].Text = "";
          item.SubItems[4].Text = "";
          item.SubItems[5].Text = "";
          item.SubItems[6].Text = "";
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
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
      StringBuilder buffer = new StringBuilder();  
      for (int i = 0; i < mpListViewLog.Columns.Count; i++)  
      {
        buffer.Append(mpListViewLog.Columns[i].Text);  
        buffer.Append("\t");  
      }  
   
      buffer.Append("\n");

      for (int i = 0; i < mpListViewLog.Items.Count; i++)  
      {
        for (int j = 0; j < mpListViewLog.Columns.Count; j++)  
         {
           buffer.Append(mpListViewLog.Items[i].SubItems[j].Text);  
           buffer.Append("\t");  
         }  
   
        buffer.Append("\n");  
      }  
   
      Clipboard.SetText(buffer.ToString());
    }

    private void chkRepeatTest_CheckedChanged(object sender, EventArgs e)
    {
      _repeat = chkRepeatTest.Checked;
    }
  }
}