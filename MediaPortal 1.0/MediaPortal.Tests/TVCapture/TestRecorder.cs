#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using NUnit.Framework;
using ProcessPlugins.DiskSpace;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using System.IO;

namespace MediaPortal.Tests.TVCapture
{
  [TestFixture]
  public class TestRecorder
  {
    class CommandProcessorMock : CommandProcessor
    {
      public override void WaitTillFinished()
      {
        while (IsBusy)
        {
          ProcessCommands();
          ProcessScheduler();
        }
      }
    };

    [SetUp]
    public void Init()
    {
        GlobalServiceProvider.Replace<ILog>(new NoLog());

      TVChannel ch;
      TVDatabase.ClearAll();

      // add 3 channels
      ch = new TVChannel("RTL 4"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 5"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("SBS 6"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("CNN"); TVDatabase.AddChannel(ch);
      CommandProcessorMock proc = new CommandProcessorMock();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      Recorder.Start(proc);
      proc.Paused = false;
    }

    [Test]
    public void TestViewTv()
    {
      ViewTv("RTL 4");
    }

    [Test]
    public void TestTimeShiftTv()
    {
      TimeShiftTv("RTL 4");
    }

    [Test]
    public void TestZapping1()
    {
      ViewTv("RTL 4");
      ViewTv("RTL 5");
      ViewTv("SBS 6");
    }

    [Test]
    public void TestZapping2()
    {
      TimeShiftTv("RTL 4");
      TimeShiftTv("RTL 5");
      TimeShiftTv("SBS 6");
    }

    [Test]
    public void TestZapping3()
    {
      TimeShiftTv("RTL 4");
      ViewTv("RTL 5");
      TimeShiftTv("SBS 6");
      ViewTv("CNN");
    }
    [Test]
    public void TestStopTv()
    {
      TimeShiftTv("RTL 4");
      StopTv();
      ViewTv("CNN");
      StopTv();
    }

    void StopTv()
    {
      string errorMessage;
      Recorder.StartViewing("", false, false, false, out errorMessage);
      Recorder.CommandProcessor.WaitTillFinished();

      Assert.IsFalse(Recorder.IsViewing());
      Assert.IsFalse(Recorder.IsTimeShifting());
      Assert.IsFalse(Recorder.IsRecording());
      Assert.AreEqual(Recorder.TVChannelName, "");
    }

    void TimeShiftTv(string channelName)
    {
      string errorMessage;
      Recorder.StartViewing(channelName, true, true, false, out errorMessage);
      Recorder.CommandProcessor.WaitTillFinished();
      

      Assert.IsTrue(Recorder.IsTimeShifting());
      Assert.IsFalse(Recorder.IsRecording());
      Assert.AreEqual(Recorder.TVChannelName, channelName);
    }

    void ViewTv(string channelName)
    {
      string errorMessage;
      Recorder.StartViewing(channelName, true, false, false, out errorMessage);
      Recorder.CommandProcessor.WaitTillFinished();
      Assert.IsTrue(Recorder.IsViewing());
      Assert.IsFalse(Recorder.IsTimeShifting());
      Assert.IsFalse(Recorder.IsRecording());
      Assert.AreEqual(Recorder.TVChannelName, channelName);
    }
  }
}
