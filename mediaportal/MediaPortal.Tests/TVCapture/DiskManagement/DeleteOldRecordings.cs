using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProcessPlugins.DiskSpace;
using MediaPortal.TV.Database;

namespace MediaPortal.Tests.Disk
{
  [TestFixture]
  [Category("DiskManagement")]
  public class DeleteOldRecordings
  {
    [Test]
    public void DontDeleteRecordingsWithMethodAlways()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.Always;
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DontDeleteRecordingsWithMethodSpace()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DontDeleteRecordingsWithMethodWatched()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilWatched;
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DontDeleteRecordingsBeforeEndDate()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
      rec.KeepRecordingTill = DateTime.Now.AddDays(+5);
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DeleteRecordingsAfterEndDate()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
      rec.KeepRecordingTill = DateTime.Now.AddDays(-5);
      Assert.IsTrue(rec.ShouldBeDeleted);
    }

  }
}
