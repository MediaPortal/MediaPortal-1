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

/*
namespace MediaPortal.Tests.Disk
{
  [TestFixture]
  [Category("DiskManagement")]
  [Ignore("Not finished")]
  public class TestFreeDiskSpace
  {
    void SetupDatabase()
    {
      //delete all recordings
      TVDatabase.DeleteAllRecordedTv();
      TVChannel chan1 = new TVChannel(); chan1.Name = "RTL4";
      TVChannel chan2 = new TVChannel(); chan2.Name = "RTL5";
      TVChannel chan3 = new TVChannel(); chan3.Name = "SBS6";
      TVDatabase.AddChannel(chan1);
      TVDatabase.AddChannel(chan2);
      TVDatabase.AddChannel(chan3);

      //next lets create 3 new recorded tv apps
      TVRecorded rec = new TVRecorded();
      rec.Channel = "RTL4";
      rec.Description = "test show 1";
      rec.Start = Utils.datetolong(new DateTime(2005, 12, 11, 14, 45, 00));
      rec.End = Utils.datetolong(rec.StartTime.AddMinutes(90));
      rec.Title = "test show 1";
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;
      rec.Genre = "none";
      rec.FileName = @"c:\rtl4_test_20051211144500.dvr-ms";
      TVDatabase.AddRecordedTV(rec);
      CreateFile(rec.FileName);

      rec.Channel = "RTL5";
      rec.Description = "test show 2";
      rec.Start = Utils.datetolong(new DateTime(2005, 12, 1, 20, 00, 00));
      rec.End = Utils.datetolong( rec.StartTime.AddMinutes(30));
      rec.Title = "test show 2";
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;
      rec.Genre = "none";
      rec.FileName = @"c:\rtl4_test_20051212200000.dvr-ms";
      TVDatabase.AddRecordedTV(rec);
      CreateFile(rec.FileName);

      rec.Channel = "SBS6";
      rec.Description = "test show 3";
      rec.Start = Utils.datetolong(new DateTime(2005, 12, 28, 21, 30, 00));
      rec.End = Utils.datetolong( rec.StartTime.AddMinutes(30));
      rec.Title = "test show 3";
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;
      rec.Genre = "none";
      rec.FileName = @"c:\rtl4_test_20051228213000.dvr-ms";
      TVDatabase.AddRecordedTV(rec);
      CreateFile(rec.FileName);
    }

    void SetupCards()
    {
      try
      {
        System.IO.File.Delete("capturecards.xml");
      }
      catch (Exception) { }
      ArrayList list = new ArrayList();
      TVCaptureDevice card = new TVCaptureDevice();
      card.ID = 1;
      card.FriendlyName = "card1";
      card.VideoDevice = "FireDTV BDA Receiver DVBC";
      card.VideoDeviceMoniker = @"@device:pnp:\\?\avc#digital_everywhere&#38;firedtv_c#ci&#38;typ_5&#38;id_0#1e04003600871200#{fd0a5af4-b41d-11d2-9c95-00c04f7971e0}\{cb365890-165f-11d0-a195-0020afd156e4}";
      card.AudioDevice="SoundMAX Digital Audio";
      card.VideoCompressor="";
      card.AudioCompressor="";
      card.CommercialName="FireDTV DVB-C";
      card.UseForRecording=true;
      card.UseForTV=true;
      card.SupportsMPEG2=true;
      card.IsMCECard=false;
      card.IsBDACard=true;
      card.FrameSize = new System.Drawing.Size(0,0);
      card.FrameRate=0;
      card.RecordingPath=@"C:\";
      card.DeleteOnLowDiskspace = true;
      
      list.Add(card);
      using (FileStream fileStream = new FileStream("capturecards.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        SoapFormatter formatter = new SoapFormatter();
        formatter.Serialize(fileStream, list);
        fileStream.Close();
      }
    }
    
    
    
    void CreateFile(string fileName)
    {
      byte[] byArray = new byte[1024*1024];//1meg
      using (FileStream strm = new FileStream(fileName, FileMode.OpenOrCreate))
      {
        //create a file of 20 megs;
        for (int i=0; i < 20;++i)
          strm.Write(byArray,0,byArray.Length);
        strm.Close();
      }
    }

    [Test]
    public void Test1()
    {
      SetupDatabase();
      //  SetupCards();
    //  Recorder.Start();
      ulong oneRec = 1024 * 1024 * 20;
      ulong freeSpaceAvailable = Utils.GetFreeDiskSpace("c:");
      freeSpaceAvailable -= 5 * oneRec;
      freeSpaceAvailable /= 1024;
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("freediskspace", "c", freeSpaceAvailable.ToString());
      }
      MediaPortal.Profile.Xml.SaveCache();

      DiskManagement.ResetTimer();
      DiskManagement.CheckFreeDiskSpace();
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      Assert.AreEqual(recordings.Count, 3);

      Assert.IsTrue(System.IO.File.Exists(@"c:\rtl4_test_20051211144500.dvr-ms"));
      Assert.IsTrue(System.IO.File.Exists(@"c:\rtl4_test_20051212200000.dvr-ms"));
      Assert.IsTrue(System.IO.File.Exists(@"c:\rtl4_test_20051228213000.dvr-ms"));
      
      
      freeSpaceAvailable = Utils.GetFreeDiskSpace("c:");
      freeSpaceAvailable += oneRec;
      freeSpaceAvailable +=1024*1024;
      freeSpaceAvailable /= 1024;
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("freediskspace", "c", freeSpaceAvailable.ToString());
      }
      MediaPortal.Profile.Xml.SaveCache();

      DiskManagement.ResetTimer();
      DiskManagement.CheckFreeDiskSpace();

      recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      Assert.AreEqual(recordings.Count, 1);

      Assert.IsFalse(System.IO.File.Exists(@"c:\rtl4_test_20051211144500.dvr-ms"));
      Assert.IsFalse(System.IO.File.Exists(@"c:\rtl4_test_20051212200000.dvr-ms"));
      Assert.IsTrue(System.IO.File.Exists(@"c:\rtl4_test_20051228213000.dvr-ms"));
      
      Utils.FileDelete(@"c:\rtl4_test_20051211144500.dvr-ms");
      Utils.FileDelete(@"c:\rtl4_test_20051212200000.dvr-ms");
      Utils.FileDelete(@"c:\rtl4_test_20051228213000.dvr-ms");
    }
  }
}
*/