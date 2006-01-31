using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using NUnit.Framework;

using MediaPortal.Util;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.TV.DiskSpace;
using ProcessPlugins.DiskSpace;

namespace MediaPortal.Tests.Disk
{
  [TestFixture]
  [Category("DiskManagement")]
  public class TestEpisodeManagement
  {
    [Test]
    public void DoNotUseEpsiodeManagementForSingleRecordings()
    {
      TVRecording rec = new TVRecording();
      rec.RecType = TVRecording.RecordingType.Once;
      Assert.IsFalse(rec.DoesUseEpisodeManagement);
    }
    [Test]
    public void DoNotUseEpsiodeManagementForUnsetRecordings()
    {
      TVRecording rec = new TVRecording();
      rec.RecType = TVRecording.RecordingType.Daily;
      rec.EpisodesToKeep = Int32.MaxValue;
      Assert.IsFalse(rec.DoesUseEpisodeManagement);
    }
    [Test]
    public void DoNotUseEpsiodeManagementForSingleEpsiode()
    {
      TVRecording rec = new TVRecording();
      rec.RecType = TVRecording.RecordingType.Daily;
      rec.EpisodesToKeep = 0;
      Assert.IsFalse(rec.DoesUseEpisodeManagement);
    }
    [Test]
    public void UseEpsiodeManagementForRest()
    {
      TVRecording rec = new TVRecording();
      rec.RecType = TVRecording.RecordingType.WeekDays;
      rec.EpisodesToKeep = 5;
      Assert.IsTrue(rec.DoesUseEpisodeManagement);
    }
    [Test] 
    public void FilterEpisodes()
    {
      TVRecorded rec;
      List<TVRecorded> recordings = new List<TVRecorded>();
      rec = new TVRecorded(); rec.Title = "title1"; rec.Start = 20051223200000;recordings.Add(rec);
      rec = new TVRecorded(); rec.Title = "title2"; rec.Start = 20051224200000;recordings.Add(rec);
      rec = new TVRecorded(); rec.Title = "title1"; rec.Start = 20051225200000;recordings.Add(rec);
      rec = new TVRecorded(); rec.Title = "title3"; rec.Start = 20051226200000;recordings.Add(rec);
      rec = new TVRecorded(); rec.Title = "title4"; rec.Start = 20051227200000;recordings.Add(rec);
      rec = new TVRecorded(); rec.Title = "title1"; rec.Start = 20051228200000;recordings.Add(rec);

      EpisodeManagement mgr = new EpisodeManagement();
      List<TVRecorded> episodes = mgr.GetEpisodes("title1", recordings);
      Assert.AreEqual(episodes.Count ,3);
      Assert.AreEqual(episodes[0].Start, 20051223200000);
      Assert.AreEqual(episodes[1].Start, 20051225200000);
      Assert.AreEqual(episodes[2].Start, 20051228200000);
    }

    [Test]
    public void GetOldestEpisode()
    {
      TVRecorded rec;
      List<TVRecorded> episodes = new List<TVRecorded>();
      rec = new TVRecorded(); rec.Title = "title1"; rec.Start = 20051223200000; episodes.Add(rec);
      rec = new TVRecorded(); rec.Title = "title2"; rec.Start = 20051224200000; episodes.Add(rec);
      rec = new TVRecorded(); rec.Title = "title3"; rec.Start = 20051225200000; episodes.Add(rec);
      rec = new TVRecorded(); rec.Title = "title4"; rec.Start = 20051222200000; episodes.Add(rec);//oldest
      rec = new TVRecorded(); rec.Title = "title5"; rec.Start = 20051227200000; episodes.Add(rec);
      rec = new TVRecorded(); rec.Title = "title6"; rec.Start = 20051228200000; episodes.Add(rec);


      EpisodeManagement mgr = new EpisodeManagement();
      rec = mgr.GetOldestEpisode(episodes);
      Assert.AreEqual(rec.Start, 20051222200000);
    }
  }
}
