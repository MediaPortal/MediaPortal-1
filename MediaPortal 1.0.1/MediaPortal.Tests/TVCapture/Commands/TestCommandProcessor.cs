#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using NUnit.Framework;

namespace MediaPortal.Tests.Commands
{
  [TestFixture]
  public class TestCommandProcessor
  {
    #region dummy command

    public class DummyCommand : CardCommand
    {
      private bool _executed = false;

      public bool Executed
      {
        get { return _executed; }
      }

      public override void Execute(CommandProcessor handler)
      {
        _executed = true;
      }
    }

    #endregion

    private string _externalChannel;

    #region general tests

    [SetUp]
    public void Init()
    {
      TVChannel ch;
      TVDatabase.ClearAll();
      RadioDatabase.ClearAll();

      // add 3 channels
      ch = new TVChannel("RTL 4");
      ch.External = true;
      ch.ExternalTunerChannel = "RTL 4";
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 5");
      ch.External = true;
      ch.ExternalTunerChannel = "RTL 5";
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("SBS 6");
      ch.External = true;
      ch.ExternalTunerChannel = "SBS 6";
      TVDatabase.AddChannel(ch);

      RadioStation station = new RadioStation();
      station.Name = "BBC Radio";
      RadioDatabase.AddStation(ref station);

      station = new RadioStation();
      station.Name = "RTL FM";
      RadioDatabase.AddStation(ref station);
      g_Player.Factory = new DummyPlayerFactory();
      PlayListPlayer.SingletonPlayer.InitTest();
      g_Player.Stop();
      GUIWindowManager.OnThreadMessageHandler +=
        new GUIWindowManager.ThreadMessageHandler(GUIWindowManager_OnThreadMessageHandler);
    }

    private void GUIWindowManager_OnThreadMessageHandler(object sender, GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL)
      {
        _externalChannel = message.Label;
      }
    }

    [Test]
    public void TestCtor()
    {
      CommandProcessor proc = new CommandProcessor();
      Assert.AreEqual(proc.CurrentCardIndex, -1);
      Assert.AreEqual(proc.TVChannelName, string.Empty);
    }

    [Test]
    public void TestDummyCommand()
    {
      DummyCommand cmd = new DummyCommand();
      Assert.IsFalse(cmd.Executed);
      CommandProcessor proc = new CommandProcessor();
      proc.AddCommand(cmd);
      proc.ProcessCommands();
      Assert.IsTrue(cmd.Executed);
    }

    [Test]
    public void TestCheckRecordingsCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      proc.scheduler.UpdateTimer();
      Assert.IsFalse(proc.scheduler.TimeToProcessRecordings);

      proc.AddCommand(new CheckRecordingsCommand());
      ProcessCommands(proc);

      Assert.IsTrue(proc.scheduler.TimeToProcessRecordings);
    }

    [Test]
    public void TestIsBusy()
    {
      CommandProcessor proc = new CommandProcessor();
      Assert.IsFalse(proc.IsBusy);
      proc.AddCommand(new CheckRecordingsCommand());
      Assert.IsTrue(proc.IsBusy);
      proc.ProcessCommands();
      Assert.IsFalse(proc.IsBusy);
    }

    #endregion

    #region test tv viewing

    [Test]
    [Category("view tv")]
    public void TestViewStartCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
    }

    [Test]
    [Category("view tv")]
    public void TestViewZap()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
      WatchTv(proc, "RTL 5");
      Assert.IsFalse(g_Player.Playing);
    }

    [Test]
    [Category("view tv")]
    public void TestViewStopCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
      StopTv(proc);
    }

    #endregion

    #region test tv viewing with timeshifting on

    [Test]
    [Category("timeshift tv")]
    public void TestTimeShiftStartCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
    }

    [Test]
    [Category("timeshift tv")]
    public void TestTimeShiftZap()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //lets watch TV
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);

      //switch channels
      TimeShiftTv(proc, "RTL 5");
      Assert.IsTrue(g_Player.Playing);
    }

    #endregion

    #region test radio

    [Test]
    [Category("Radio")]
    public void TestRadio()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      StartRadio(proc, "BBC Radio");
      StopRadio(proc);
    }

    [Test]
    [Category("Radio Zapping")]
    public void TestRadioZapping()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      StartRadio(proc, "BBC Radio");
      StartRadio(proc, "RTL FM");
      StartRadio(proc, "BBC Radio");
    }

    [Test]
    [Category("Radio tv zap")]
    public void TestSwapBetweenTvAndRadio()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //lets watch TV
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);

      StartRadio(proc, "BBC Radio");

      WatchTv(proc, "RTL 5");
      Assert.IsFalse(g_Player.Playing);

      StartRadio(proc, "BBC Radio");
    }

    [Test]
    [Category("timeshift tv")]
    public void TestTimeShiftStopCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //lets watch TV
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
      //lets stop TV
      StopTv(proc);
    }

    #endregion

    #region test switch timeshift off/on

    [Test]
    [Category("switch timeshift on/off")]
    public void TestSwitchTimeShiftOnOff()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //watch TV
      WatchTv(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
      WatchTv(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
      StopTv(proc);
    }

    #endregion

    #region test recording

    [Test]
    [Category("Recording")]
    public void TestStartRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
    }

    [Test]
    [Category("Recording")]
    public void TestStopRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
      StopRecord(proc);
    }

    [Test]
    [Category("Recording")]
    public void TestStartRecording2()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      TimeShiftTv(proc, "RTL 5");
      Assert.IsTrue(g_Player.Playing);
      StartRecord(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
      StopRecord(proc);
      Assert.IsTrue(g_Player.Playing);
    }

    [Test]
    [Category("Recording")]
    public void TestStartRecording3()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      WatchTv(proc, "RTL 5");
      Assert.IsFalse(g_Player.Playing);
      StartRecord(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
      StopRecord(proc);
      Assert.IsTrue(g_Player.Playing);
    }

    [Test]
    [Category("Recording")]
    public void TestRecordingInPast()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      AddSchedule("RTL 4", DateTime.Now.AddHours(-6), DateTime.Now.AddHours(-5));
      DoSchedule(proc);
      Assert.IsFalse(card1.IsRecording);
    }

    [Test]
    [Category("Recording")]
    public void TestRecordingInFuture()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      AddSchedule("RTL 4", DateTime.Now.AddHours(+1), DateTime.Now.AddHours(+2));
      DoSchedule(proc);
      Assert.IsFalse(card1.IsRecording);
    }

    [Test]
    [Category("Recording")]
    public void TestPreRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      AddSchedule("RTL 4", DateTime.Now.AddMinutes(2), DateTime.Now.AddHours(+2));
      DoSchedule(proc);
      Assert.IsTrue(card1.IsRecording);
      Assert.IsTrue(card1.InternalGraph.IsRecording());
    }

    [Test]
    [Category("Recording")]
    public void TestZapWhileRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);

      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);

      //zap to rtl 5 (should fail)
      proc.AddCommand(new TimeShiftTvCommand("RTL 5"));
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, "RTL 4");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "RTL 4");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.IsTrue(proc.TVCards[0].InternalGraph.IsTimeShifting() || proc.TVCards[0].InternalGraph.IsRecording());
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(proc.CurrentCardIndex), @"C:\card1\live.tv");

      Assert.IsTrue(g_Player.Playing);
      StopRecord(proc);
      Assert.IsTrue(g_Player.Playing);
    }

    [Test]
    [Category("Recording")]
    public void TestTurnOffTimeshiftWhileRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      TimeShiftTv(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);
      StartRecord(proc, "RTL 4");
      Assert.IsTrue(g_Player.Playing);

      //zap to rtl 5 (should fail)
      proc.AddCommand(new ViewTvCommand("RTL 5"));
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, "RTL 4");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "RTL 4");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.IsTrue(proc.TVCards[0].InternalGraph.IsTimeShifting() || proc.TVCards[0].InternalGraph.IsRecording());
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(proc.CurrentCardIndex), @"C:\card1\live.tv");

      Assert.IsTrue(g_Player.Playing);
      StopRecord(proc);
      Assert.IsTrue(g_Player.Playing);
    }

    [Test]
    [Category("Recording")]
    public void TestZapToRecordingChannel()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");
      Assert.IsFalse(g_Player.Playing);

      //switch channels
      WatchTv(proc, "RTL 4", true);
      Assert.IsTrue(g_Player.Playing);
    }

    #endregion

    #region test audio switching

    [Test]
    [Category("Audio stream selection")]
    public void TestGetAudioLanguageList()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TimeShiftTv(proc, "RTL 4");
      ArrayList list = card1.GetAudioLanguageList();
      Assert.AreEqual(list.Count, 3);
      Assert.AreEqual((int) list[0], 123);
      Assert.AreEqual((int) list[1], 456);
      Assert.AreEqual((int) list[2], 789);
    }

    [Test]
    [Category("Audio stream selection")]
    public void TestSwitchAudioStreamWithTimeShiftEnabled()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TimeShiftTv(proc, "RTL 4");
      proc.AddCommand(new SetAudioLanguageCommand(123));
      ProcessCommands(proc);
      Assert.AreEqual(card1.GetAudioLanguage(), 123);

      proc.AddCommand(new SetAudioLanguageCommand(456));
      ProcessCommands(proc);
      Assert.AreEqual(card1.GetAudioLanguage(), 456);


      proc.AddCommand(new SetAudioLanguageCommand(789));
      ProcessCommands(proc);
      Assert.AreEqual(card1.GetAudioLanguage(), 789);
    }

    [Test]
    [Category("Audio stream selection")]
    public void TestSwitchAudioStreamWithTimeShiftDisabled()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      proc.AddCommand(new SetAudioLanguageCommand(123));
      ProcessCommands(proc);
      Assert.AreEqual(card1.GetAudioLanguage(), 123);

      proc.AddCommand(new SetAudioLanguageCommand(456));
      ProcessCommands(proc);
      Assert.AreEqual(card1.GetAudioLanguage(), 456);


      proc.AddCommand(new SetAudioLanguageCommand(789));
      ProcessCommands(proc);
      Assert.AreEqual(card1.GetAudioLanguage(), 789);
    }

    #endregion

    #region helper functions

    private void ProcessCommands(CommandProcessor proc)
    {
      do
      {
        proc.ProcessCommands();
        proc.ProcessCards();
      } while (proc.IsBusy);
    }

    private void DoSchedule(CommandProcessor proc)
    {
      proc.AddCommand(new CheckRecordingsCommand());
      do
      {
        proc.ProcessCommands();
        proc.ProcessCards();
        proc.ProcessScheduler();
        proc.ProcessCards();
      } while (proc.IsBusy);
    }

    private void StopRecord(CommandProcessor proc)
    {
      DateTime dtNow = DateTime.Now;
      TVRecording rec = proc.TVCards[0].CurrentTVRecording;
      proc.AddCommand(new StopRecordingCommand());
      ProcessCommands(proc);
      proc.ProcessScheduler();
      Assert.IsFalse(proc.TVCards[0].IsRecording);
      Assert.IsFalse(proc.TVCards[0].InternalGraph.IsRecording());
      Assert.IsFalse(proc.TVCards[0].IsPostRecording);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording, null);
      Assert.AreEqual(proc.TVCards[0].RecordingFileName.Length, 0);

      //check if recording has been canceled
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      CompareDates(recs[0].CanceledTime, dtNow);

      //check if recorded tv has been added
      List<TVRecorded> recList = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recList);
      Assert.AreEqual(rec.Channel, recList[0].Channel);
      Assert.AreEqual(rec.Title, recList[0].Title);
    }

    private TVRecording AddSchedule(string channelName, DateTime dtStart, DateTime dtEnd)
    {
      DateTime startTime = dtStart;
      DateTime endTime = dtEnd;
      TVRecording rec = new TVRecording();
      rec.Channel = channelName;
      rec.Title = "unknown";
      rec.Start = Util.Utils.datetolong(startTime);
      rec.End = Util.Utils.datetolong(endTime);
      rec.RecType = TVRecording.RecordingType.Once;
      TVDatabase.AddRecording(ref rec);
      return rec;
    }

    private void StartRecord(CommandProcessor proc, string channelName, DateTime dtStart, DateTime dtEnd)
    {
      string chanName = proc.TVChannelName;
      int cardIndex = proc.CurrentCardIndex;
      TVRecording rec = AddSchedule(channelName, dtStart, dtEnd);

      //start recording it
      DoSchedule(proc);

      int cardNo;
      bool result = proc.IsRecordingSchedule(rec, out cardNo);

      Assert.IsTrue(result);
      Assert.AreEqual(cardNo, 0);
      Assert.AreEqual(proc.CurrentCardIndex, cardIndex);
      if (cardIndex == 0)
      {
        Assert.AreEqual(proc.TVChannelName, channelName);
      }
      Assert.AreEqual(proc.TVCards[0].TVChannel, rec.Channel);
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.IsTrue(proc.TVCards[0].IsRecording);
      Assert.IsTrue(proc.TVCards[0].InternalGraph.IsRecording());
      Assert.IsFalse(proc.TVCards[0].IsPostRecording);
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(0), @"C:\card1\live.tv");
      Assert.AreEqual(proc.TVCards[0].CurrentProgramRecording, null);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.Title, rec.Title);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.Start, rec.Start);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.End, rec.End);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.Channel, rec.Channel);
      Assert.IsTrue(proc.TVCards[0].RecordingFileName.Length > 0);
      Assert.AreEqual(proc.TVCards[0].TVChannel, _externalChannel);
    }

    private void StartRecord(CommandProcessor proc, string channelName)
    {
      StartRecord(proc, channelName, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(+1));
    }

    private void StopTv(CommandProcessor proc)
    {
      proc.AddCommand(new StopTvCommand());
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, -1);
      Assert.AreEqual(proc.TVChannelName, "");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
      Assert.IsFalse(proc.TVCards[0].InternalGraph.IsTimeShifting());
      CompareDates(proc.TVCards[0].TimeShiftingStarted, DateTime.MinValue);
      Assert.IsFalse(g_Player.Playing);
    }

    private void TimeShiftTv(CommandProcessor proc, string channelName)
    {
      bool isTimeShifting = proc.TVCards[0].IsTimeShifting;
      //timeshift TV
      DateTime dtNow = DateTime.Now;
      proc.AddCommand(new TimeShiftTvCommand(channelName));
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(proc.TVCards[0].TVChannel, channelName);
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.IsTrue(proc.TVCards[0].InternalGraph.IsTimeShifting() || proc.TVCards[0].InternalGraph.IsRecording());
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(proc.CurrentCardIndex), @"C:\card1\live.tv");
      if (!isTimeShifting)
      {
        CompareDates(proc.TVCards[0].TimeShiftingStarted, dtNow);
      }
      Assert.IsTrue(g_Player.Playing);
      Assert.AreEqual(g_Player.CurrentFile, proc.GetTimeShiftFileName(proc.CurrentCardIndex));

      Assert.AreEqual(proc.TVCards[0].TVChannel, _externalChannel);
    }

    private void WatchTv(CommandProcessor proc, string channelName)
    {
      //watch TV
      proc.AddCommand(new ViewTvCommand(channelName));
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(proc.TVCards[0].TVChannel, channelName);
      Assert.IsTrue(proc.TVCards[0].View);
      Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
      Assert.IsFalse(proc.TVCards[0].InternalGraph.IsTimeShifting());
      if (proc.TVCards[0].IsTimeShifting)
      {
        Assert.IsTrue(g_Player.Playing);
        Assert.AreEqual(g_Player.CurrentFile, proc.GetTimeShiftFileName(proc.CurrentCardIndex));
      }
      else
      {
        Assert.IsFalse(g_Player.Playing);
      }
      CompareDates(proc.TVCards[0].TimeShiftingStarted, DateTime.MinValue);

      Assert.AreEqual(proc.TVCards[0].TVChannel, _externalChannel);
    }

    private void WatchTv(CommandProcessor proc, string channelName, bool shouldBeTimeShifting)
    {
      //watch TV
      proc.AddCommand(new ViewTvCommand(channelName));
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(proc.TVCards[0].TVChannel, channelName);
      if (shouldBeTimeShifting)
      {
        Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
        Assert.IsTrue(proc.TVCards[0].InternalGraph.IsTimeShifting() || proc.TVCards[0].InternalGraph.IsRecording());
      }
      else
      {
        Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
        Assert.IsFalse(proc.TVCards[0].InternalGraph.IsTimeShifting());
      }
      if (proc.TVCards[0].IsTimeShifting)
      {
        Assert.IsTrue(g_Player.Playing);
        Assert.AreEqual(g_Player.CurrentFile, proc.GetTimeShiftFileName(proc.CurrentCardIndex));
      }
      else
      {
        Assert.IsFalse(g_Player.Playing);
      }
      Assert.AreEqual(proc.TVCards[0].TVChannel, _externalChannel);
    }

    private void StartRadio(CommandProcessor proc, string stationName)
    {
      proc.AddCommand(new StartRadioCommand(stationName));
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.IsTrue(proc.TVCards[0].IsRadio);
      Assert.IsTrue(proc.TVCards[0].InternalGraph.IsRadio());
      Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
      Assert.IsFalse(proc.TVCards[0].InternalGraph.IsTimeShifting());
      Assert.IsFalse(g_Player.Playing);
    }

    private void StopRadio(CommandProcessor proc)
    {
      proc.AddCommand(new StopRadioCommand());
      ProcessCommands(proc);
      Assert.AreEqual(proc.CurrentCardIndex, -1);
      Assert.AreEqual(proc.TVChannelName, "");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
      Assert.IsFalse(proc.TVCards[0].IsRadio);
      if (proc.TVCards[0].InternalGraph != null)
      {
        Assert.IsFalse(proc.TVCards[0].InternalGraph.IsTimeShifting());
        Assert.IsFalse(proc.TVCards[0].InternalGraph.IsRadio());
      }
      Assert.IsFalse(g_Player.Playing);
    }

    private void CompareDates(DateTime dt1, DateTime dt2)
    {
      Assert.AreEqual(dt1.Year, dt2.Year);
      Assert.AreEqual(dt1.Month, dt2.Month);
      Assert.AreEqual(dt1.Day, dt2.Day);
      Assert.AreEqual(dt1.Hour, dt2.Hour);
      Assert.AreEqual(dt1.Minute, dt2.Minute);
      // Assert.AreEqual(dt1.Second, dt2.Second);
    }

    #endregion
  }
}