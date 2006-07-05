using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProcessPlugins.DiskSpace;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using System.IO;
using MediaPortal.Utils.Services;

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
      ServiceProvider services = GlobalServiceProvider.Instance;
      StringWriter logString = new StringWriter();
      Log log = new Log(logString, Log.Level.Debug);
      services.Replace<ILog>(log);

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
