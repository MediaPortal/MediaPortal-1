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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using NUnit.Framework;

namespace MediaPortal.Tests.TVCapture.Commands
{
  [TestFixture]
  public class TestMultiTunerStrategy
  {
    #region constants

    private const int HIGHEST = 1;
    private const int MEDIUM = 0;
    private const int LOWEST = 2;

    #endregion

    private CommandProcessor _processor;
    private TVCaptureDevice[] _cards;

    [SetUp]
    public void Init()
    {
      TVChannel ch;
      TVDatabase.ClearAll();
      g_Player.Factory = new DummyPlayerFactory();
      PlayListPlayer.SingletonPlayer.InitTest();
      g_Player.Stop();

      // add 3 channels
      ch = new TVChannel("RTL 4");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 5");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("SBS 6");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 7");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("MTV");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("CNN");
      TVDatabase.AddChannel(ch);
      List<TVChannel> channelList = new List<TVChannel>();
      TVDatabase.GetChannels(ref channelList);
      foreach (TVChannel chan in channelList)
      {
        TVDatabase.MapChannelToCard(chan.ID, 1);
        TVDatabase.MapChannelToCard(chan.ID, 2);
        TVDatabase.MapChannelToCard(chan.ID, 3);
      }

      //add 3 cards
      _processor = new CommandProcessor();
      _cards = new TVCaptureDevice[3];
      _cards[0] = _processor.TVCards.AddDummyCard("dummy2");
      _cards[1] = _processor.TVCards.AddDummyCard("dummy3");
      _cards[2] = _processor.TVCards.AddDummyCard("dummy4");

      //setup priority for all cards from low-hi (2->0->1)
      _cards[HIGHEST].Priority = 10; //highest
      _cards[MEDIUM].Priority = 5;
      _cards[LOWEST].Priority = 1;

      //setup recording paths for all cards
      //Use C: drive because not everybody has D or E drives
      _cards[HIGHEST].RecordingPath = "c:";
      _cards[MEDIUM].RecordingPath = "c:";
      _cards[LOWEST].RecordingPath = "c:";

      using (
        FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Create,
                                               FileAccess.Write, FileShare.Read))
      {
        SoapFormatter formatter = new SoapFormatter();
        formatter.Serialize(fileStream, _cards);
        fileStream.Close();
      }

      RadioStation station = new RadioStation();
      station.Name = "BBC Radio";
      RadioDatabase.AddStation(ref station);
      RadioDatabase.MapChannelToCard(station.ID, 1);

      station = new RadioStation();
      station.Name = "RTL FM";
      RadioDatabase.AddStation(ref station);
      RadioDatabase.MapChannelToCard(station.ID, 2);
    }

    [Test]
    public void TestRadioOn2Tuners()
    {
      StartRadio("RTL FM", 1); // should be on card 2
      Assert.IsTrue(_processor.TVCards[1].IsRadio);
      Assert.IsFalse(_processor.TVCards[0].IsRadio);
      Assert.IsTrue(_processor.TVCards[1].InternalGraph.IsRadio());

      StartRadio("BBC Radio", 0); // should be on card 1
      Assert.IsTrue(_processor.TVCards[0].IsRadio);
      Assert.IsFalse(_processor.TVCards[1].IsRadio);
      Assert.IsTrue(_processor.TVCards[0].InternalGraph.IsRadio());
      Assert.IsFalse(_processor.TVCards[1].InternalGraph.IsRadio());


      StartRadio("RTL FM", 1); // should be on card 2
      Assert.IsTrue(_processor.TVCards[1].IsRadio);
      Assert.IsFalse(_processor.TVCards[0].IsRadio);
      Assert.IsTrue(_processor.TVCards[1].InternalGraph.IsRadio());
      Assert.IsFalse(_processor.TVCards[0].InternalGraph.IsRadio());
    }

    [Test]
    public void RecordOn2Tuners()
    {
      TVRecording rec = AddSchedule("RTL 4", DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(10));
      DoSchedule();
      VerifyRecord(HIGHEST, rec);

      TVRecording rec2 = AddSchedule("RTL 5", DateTime.Now.AddMinutes(-22), DateTime.Now.AddMinutes(33));
      DoSchedule();
      VerifyRecord(MEDIUM, rec2);
      Assert.IsFalse(g_Player.Playing);
    }

    [Test]
    public void RecordOn3Tuners()
    {
      TVRecording rec = AddSchedule("RTL 4", DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(10));
      DoSchedule();
      VerifyRecord(HIGHEST, rec);

      TVRecording rec2 = AddSchedule("RTL 5", DateTime.Now.AddMinutes(-22), DateTime.Now.AddMinutes(33));
      DoSchedule();
      VerifyRecord(MEDIUM, rec2);

      TVRecording rec3 = AddSchedule("SBS 6", DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1));
      DoSchedule();
      VerifyRecord(LOWEST, rec3);
      DoSchedule();
      Assert.IsFalse(g_Player.Playing);
    }

    [Test]
    public void RecordOn3TunersZap()
    {
      RecordOn3Tuners();
      Zap("RTL 4");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(HIGHEST);

      Zap("RTL 5");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(MEDIUM);

      Zap("SBS 6");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(LOWEST);
    }

    [Test]
    public void TestNoFreeCard()
    {
      RecordOn3Tuners();
      Zap("SBS 6");
      Assert.IsTrue(g_Player.Playing);

      Zap("CNN");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(LOWEST);
    }

    [Test]
    public void Record2ZapOnThird()
    {
      RecordOn2Tuners();
      Zap("SBS 6");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(LOWEST);

      Zap("MTV");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(LOWEST);

      Zap("CNN");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(LOWEST);

      Zap("RTL 4");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(HIGHEST);

      Zap("RTL 5");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(MEDIUM);
    }

    [Test]
    public void ZapAfterRecordingStopped()
    {
      RecordOn3Tuners();
      Zap("SBS 6");
      Assert.IsTrue(g_Player.Playing);

      StopRecord();
      VerifyNotRecording(LOWEST);

      Zap("CNN");
      Assert.IsTrue(g_Player.Playing);
      VerifyTimeShift(LOWEST);
    }

    #region helper functions

    private void VerifyTimeShift(int card)
    {
      Assert.IsTrue(_cards[card].IsTimeShifting);
      Assert.IsTrue(_cards[card].InternalGraph.IsTimeShifting() || _cards[card].InternalGraph.IsRecording());
      Assert.AreEqual(_processor.CurrentCardIndex, card);
      Assert.AreEqual(_processor.TVChannelName, _cards[card].TVChannel);
    }

    private void VerifyNotRecording(int card)
    {
      Assert.IsFalse(_cards[card].IsRecording);
      Assert.IsFalse(_cards[card].InternalGraph.IsRecording());
      Assert.AreEqual(_cards[card].CurrentTVRecording, null);
    }

    private void VerifyRecord(int card, TVRecording rec)
    {
      Assert.IsTrue(_cards[card].IsRecording);
      Assert.IsTrue(_cards[card].InternalGraph.IsRecording());
      Assert.AreEqual(_cards[card].CurrentTVRecording.Channel, rec.Channel);
      Assert.AreEqual(_cards[card].CurrentTVRecording.Title, rec.Title);
      Assert.AreEqual(_cards[card].CurrentTVRecording.End, rec.End);
      Assert.AreEqual(_cards[card].CurrentTVRecording.Start, rec.Start);
    }

    private void Zap(string channelName)
    {
      _processor.AddCommand(new TimeShiftTvCommand(channelName));
      do
      {
        _processor.ProcessCommands();
        _processor.ProcessCards();
      } while (_processor.IsBusy);
    }

    private void DoSchedule()
    {
      _processor.AddCommand(new CheckRecordingsCommand());
      do
      {
        _processor.ProcessScheduler();
        _processor.ProcessCommands();
        _processor.ProcessCards();
      } while (_processor.IsBusy);
    }

    private void StopRecord()
    {
      _processor.AddCommand(new StopRecordingCommand());
      do
      {
        _processor.ProcessCommands();
        _processor.ProcessCards();
      } while (_processor.IsBusy);
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

    private void StartRadio(string stationName, int expectedCard)
    {
      _processor.AddCommand(new StartRadioCommand(stationName));
      _processor.ProcessCommands();
      Assert.AreEqual(_processor.CurrentCardIndex, expectedCard);
      Assert.IsTrue(_processor.TVCards[expectedCard].IsRadio);
      Assert.IsTrue(_processor.TVCards[expectedCard].InternalGraph.IsRadio());
      Assert.IsFalse(_processor.TVCards[expectedCard].IsTimeShifting);
      Assert.IsFalse(_processor.TVCards[expectedCard].InternalGraph.IsTimeShifting());
      Assert.IsFalse(g_Player.Playing);
    }

    #endregion
  }
}