//using System;
//using System.Collections.Generic;
//using System.Text;
//using NUnit.Framework;
//using MediaPortal.TV.DiskSpace;
//using MediaPortal.TV.Database;

//namespace MediaPortal.Tests.Disk
//{
//  [TestFixture]
//  [Category("DiskManagement")]
//  public class DeleteOldRecordings
//  {
//    [Test]
//    public void DontDeleteRecordingsWithMethodAlways()
//    {
//      TVRecorded rec = new TVRecorded();
//      rec.KeepRecordingMethod = TVRecorded.KeepMethod.Always;
//      Assert.IsFalse(RecordingManagement.ShouldDeleteRecording(rec));
//    }
//    [Test]
//    public void DontDeleteRecordingsWithMethodSpace()
//    {
//      TVRecorded rec = new TVRecorded();
//      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;
//      Assert.IsFalse(RecordingManagement.ShouldDeleteRecording(rec));
//    }
//    [Test]
//    public void DontDeleteRecordingsWithMethodWatched()
//    {
//      TVRecorded rec = new TVRecorded();
//      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilWatched;
//      Assert.IsFalse(RecordingManagement.ShouldDeleteRecording(rec));
//    }
//    [Test]
//    public void DontDeleteRecordingsBeforeEndDate()
//    {
//      TVRecorded rec = new TVRecorded();
//      rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
//      rec.KeepRecordingTill = DateTime.Now.AddDays(+5);
//      Assert.IsFalse(RecordingManagement.ShouldDeleteRecording(rec));
//    }
//    [Test]
//    public void DeleteRecordingsAfterEndDate()
//    {
//      TVRecorded rec = new TVRecorded();
//      rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
//      rec.KeepRecordingTill = DateTime.Now.AddDays(-5);
//      Assert.IsTrue(RecordingManagement.ShouldDeleteRecording(rec));
//    }

//    [Test]
//    public void ProcessAfterDayChange()
//    {
//      Assert.IsTrue(RecordingManagement.TimeToDeleteOldRecordings(DateTime.Now));
//      Assert.IsFalse(RecordingManagement.TimeToDeleteOldRecordings(DateTime.Now));
//      Assert.IsTrue(RecordingManagement.TimeToDeleteOldRecordings(DateTime.Now.AddDays(1)));
//      Assert.IsFalse(RecordingManagement.TimeToDeleteOldRecordings(DateTime.Now.AddDays(1)));
//    }
//  }
//}
