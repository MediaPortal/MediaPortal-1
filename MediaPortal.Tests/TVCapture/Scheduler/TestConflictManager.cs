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
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using NUnit.Framework;

namespace MediaPortal.Tests.TVCapture.Scheduler
{
  [TestFixture]
  [Category("ConfictManager")]
  public class TestConflictManager
  {
    #region Test SetUp routines and Mock class

    private class CommandProcessorMock : CommandProcessor
    {
      public override void WaitTillFinished()
      {
        while (IsBusy)
        {
          ProcessCommands();
          ProcessScheduler();
        }
      }
    } ;

    private bool backgroundWorkerFinished = false;

    private void AddRecording(string Channel, DateTime Start, int Duration, string Title)
    {
      TVRecording rec = new TVRecording();
      rec.Channel = Channel;
      rec.Start = Util.Utils.datetolong(Start);
      rec.End = Util.Utils.datetolong(rec.StartTime.AddMinutes(Duration));
      rec.Title = Title;
      rec.RecType = TVRecording.RecordingType.Once;
      TVDatabase.AddRecording(ref rec);
    }

    private void SetupDatabase()
    {
      //delete all recordings
      TVDatabase.ClearAll();
      // create some channels
      TVChannel ch;
      ch = new TVChannel("RTL1");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL2");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL3");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL4");
      TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL5");
      TVDatabase.AddChannel(ch);

      // insert some recordings
      DateTime dt = DateTime.Now.AddDays(1);
      // FirstRecording
      AddRecording("RTL1", dt, 60, "test show 1");
      // starts before FirstRecording, ends before FirstRecording ends
      AddRecording("RTL2", dt.AddMinutes(-30), 60, "test show 2");
      // starts before FirstRecording, ends after FirstRecording ends
      AddRecording("RTL3", dt.AddMinutes(-30), 120, "test show 3");
      // starts after FirstRecording, ends before FirstRecording ends
      AddRecording("RTL4", dt.AddMinutes(5), 15, "test show 4");
      // starts after FirstRecording, ends after FirstRecording ends
      AddRecording("RTL5", dt.AddMinutes(15), 60, "test show 5");
      // starts before FirstRecording, ends before FirstRecording ends
      AddRecording("RTL5", dt.AddMinutes(-90), 45, "test show 6");
      // starts after FirstRecording, ends after FirstRecording ends
      AddRecording("RTL1", dt.AddMinutes(95), 60, "test show 7");
    }

    private void SetupOneRecorder()
    {
      CommandProcessorMock proc = new CommandProcessorMock();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      Recorder.Start(proc);
    }

    private void SetupThreeRecorder()
    {
      CommandProcessorMock proc = new CommandProcessorMock();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TVCaptureDevice card2 = proc.TVCards.AddDummyCard("dummy2");
      TVCaptureDevice card3 = proc.TVCards.AddDummyCard("dummy3");
      Recorder.Start(proc);
    }

    private void ConflictManager_OnConflictsUpdated()
    {
      backgroundWorkerFinished = true;
    }

    #endregion

    [TestFixtureSetUp]
    public void MainSetUp()
    {
      SetupDatabase();
      SetupOneRecorder();
      backgroundWorkerFinished = false;
      ConflictManager.OnConflictsUpdated +=
        new ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
      TVUtil u = ConflictManager.Util;
      int i = 0;
      while ((backgroundWorkerFinished == false) && (i < 25))
      {
        Thread.Sleep(200);
        i++;
      }
    }

    [Test]
    public void TestGetConflictingRecordings()
    {
      List<TVRecording> recordingList = new List<TVRecording>();
      TVRecording[] conflicts;
      TVDatabase.GetRecordings(ref recordingList);
      Assert.AreEqual(7, recordingList.Count);

      foreach (TVRecording rec in recordingList)
      {
        conflicts = null;
        conflicts = ConflictManager.GetConflictingRecordings(rec);
        Assert.IsNotNull(conflicts);
        if (conflicts != null)
        {
          if ((rec.Title.Equals("test show 6")) || (rec.Title.Equals("test show 7")))
          {
            Assert.AreEqual(0, conflicts.Length, rec.Title);
          }
          else
          {
            Assert.AreEqual(4, conflicts.Length, rec.Title);
          }
        }
      }
    }


    [Test]
    public void TestGetConflictingSeries()
    {
      List<TVRecording> recordingList = new List<TVRecording>();
      List<TVRecording> seriesList = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordingList);
      Assert.AreEqual(7, recordingList.Count);

      foreach (TVRecording rec in recordingList)
      {
        seriesList.Clear();
        ConflictManager.GetConflictingSeries(rec, seriesList);
        if ((rec.Title.Equals("test show 6")) || (rec.Title.Equals("test show 7")))
        {
          Assert.AreEqual(0, seriesList.Count, rec.Title);
        }
        else
        {
          Assert.AreEqual(1, seriesList.Count, rec.Title);
        }
      }
    }

    [Test]
    public void TestIsConflict()
    {
      List<TVRecording> recordingList = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordingList);
      Assert.AreEqual(7, recordingList.Count);

      foreach (TVRecording rec in recordingList)
      {
        bool conflict = ConflictManager.IsConflict(rec);
        if ((rec.Title.Equals("test show 6")) || (rec.Title.Equals("test show 7")))
        {
          Assert.IsFalse(conflict, rec.Title);
        }
        else
        {
          Assert.IsTrue(conflict, rec.Title);
        }
      }
    }
  }
}