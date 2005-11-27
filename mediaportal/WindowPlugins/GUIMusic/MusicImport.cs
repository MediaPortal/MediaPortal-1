/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using Yeti.Lame;
using WaveLib;
using Roger.ID3;

namespace MediaPortal.MusicImport
{
  public class MusicImport
  {
    private static CDDrive m_Drive;
    private static bool m_Ripping = false;
    private static bool m_CancelRipping = false;
    private static bool mp3ReplaceExisting = true;
    private static bool mp3VBR = true;
    private static bool mp3MONO = false;
    private static bool mp3CBR = false;
    private static bool mp3FastMode = false;
    private static bool mp3Organize = true;
    private static bool mp3Database = true;
    private static bool mp3Background = false;
    private static Mp3Writer m_Writer = null;
    private static int mp3Quality = 2;
    private static int mp3BitRate = 2;
    private static int mp3Priority = 0;
    private static string mp3ImportDir = "C:";
    private static string[] Rates;
    private const string Mpeg1BitRates = "128,160,192,224,256,320";
    private static Queue importQueue = new Queue();
    private static int GetID;

    public class TrackInfo
    {
      public string TempFileName;
      public string TargetFileName;
      public string TargetDir;
      public MediaPortal.TagReader.MusicTag MusicTag;
      public int TrackCount;
      public GUIListItem Item;
    }

    public static bool Ripping
    {
      get { return m_Ripping; }
    }

    Thread EncodeThread;

    public MusicImport()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        mp3VBR = xmlreader.GetValueAsBool("musicimport", "mp3vbr", true);
        mp3MONO = xmlreader.GetValueAsBool("musicimport", "mp3mono", false);
        mp3CBR = xmlreader.GetValueAsBool("musicimport", "mp3cbr", false);
        mp3FastMode = xmlreader.GetValueAsBool("musicimport", "mp3fastmode", false);
        mp3ReplaceExisting = xmlreader.GetValueAsBool("musicimport", "mp3replaceexisting", true);
        mp3BitRate = xmlreader.GetValueAsInt("musicimport", "mp3bitrate", 2);
        mp3Quality = xmlreader.GetValueAsInt("musicimport", "mp3quality", 2);
        mp3Priority = xmlreader.GetValueAsInt("musicimport", "mp3priority", 0);
        mp3ImportDir = xmlreader.GetValueAsString("musicimport", "mp3importdir", "C:");
        mp3Organize = xmlreader.GetValueAsBool("musicimport", "mp3organize", true);
        mp3Database = xmlreader.GetValueAsBool("musicimport", "mp3database", true);
        mp3Background = xmlreader.GetValueAsBool("musicimport", "mp3background", false);
      }
      Rates = Mpeg1BitRates.Split(',');

      EncodeThread = new Thread(new ThreadStart(MusicImport.ThreadEncodeTrack));
      EncodeThread.Name = "CD Import";
      EncodeThread.IsBackground = true;
      EncodeThread.Priority = (ThreadPriority)mp3Priority;

      dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(1103);
        dlgProgress.SetLine(1, "");
        dlgProgress.SetLine(2, "");
        dlgProgress.SetPercentage(0);
        dlgProgress.ShowProgressBar(true);
      }
    }

    public static void WriteWaveData(object sender, DataReadEventArgs ea)
    {
      if (m_Writer != null)
        m_Writer.Write(ea.Data, 0, (int)ea.DataSize);
    }

    private static void CdReadProgress(object sender, ReadProgressEventArgs ea)
    {
      ulong Percent = ((ulong)ea.BytesRead * 100) / ea.Bytes2Read;
      if (!mp3Background)
      {
        dlgProgress.SetPercentage((int)Percent);
        if (dlgProgress.IsCanceled)
          m_CancelRipping = true;
      }
      ea.CancelRead |= m_CancelRipping;
    }

    private String FilterInvalidChars(String input)
    {
      String copy = String.Empty;
      for (int i = 0; i < input.Length; i++)
        switch (input[i])
        {
          case '\\':
          case '/':
          case ':':
          case '*':
          case '?':
          case '"':
          case '<':
          case '>':
          case '|':
            Log.Write("CDIMP: removing \"" + input[i] + "\" from filename");
            break;
          default:
            copy += input[i];
            break;
        }
      return copy;
    }

    public void Cancel()
    {
      m_CancelRipping = true;
    }

    public void EncodeTrack(GUIFacadeControl facadeView, int getID)
    {
      GetID = getID;
      if (File.Exists("lame_enc.dll"))
      {
        GUIListItem item = facadeView.SelectedListItem;
        char[] Drives = CDDrive.GetCDDriveLetters();
        if ((!item.IsFolder) && (Array.IndexOf(Drives, item.Path[0]) > -1))
        {
          TrackInfo trackInfo = new TrackInfo();
          if ((TagReader.MusicTag)item.MusicTag == null)
          {
            TagReader.MusicTag musicTag = new TagReader.MusicTag();
            musicTag.Artist = "Unknown Artist";
            musicTag.Album = "Unknown Album";
            musicTag.Title = "Track " + item.Label.Substring(5);
            musicTag.Track = Convert.ToInt16(item.Label.Substring(5));
            item.MusicTag = musicTag;
          }
          trackInfo.MusicTag = (TagReader.MusicTag)item.MusicTag;
          trackInfo.TrackCount = facadeView.Count - 1;
          trackInfo.Item = item;

          if (item.Label != "..")
            try
            {
              EncodeTrack(trackInfo);
            }
            catch { }
        }
      }
    }

    public void EncodeDisc(GUIFacadeControl facadeView, int getID)
    {
      GetID = getID;
      if (File.Exists("lame_enc.dll"))
      {
        char[] Drives = CDDrive.GetCDDriveLetters();
        for (int i = 1; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          if ((!item.IsFolder) && (Array.IndexOf(Drives, item.Path[0]) > -1))
          {
            TrackInfo trackInfo = new TrackInfo();
            if ((TagReader.MusicTag)item.MusicTag == null)
            {
              TagReader.MusicTag musicTag = new TagReader.MusicTag();
              musicTag.Artist = "Unknown Artist";
              musicTag.Album = "Unknown Album";
              musicTag.Title = "Track " + item.Label.Substring(5);
              musicTag.Track = Convert.ToInt16(item.Label.Substring(5));
              item.MusicTag = musicTag;
            }
            trackInfo.MusicTag = (TagReader.MusicTag)item.MusicTag;
            trackInfo.TrackCount = facadeView.Count - 1;
            trackInfo.Item = item;

            try
            {
              EncodeTrack(trackInfo);
            }
            catch { }
          }
        }
      }
    }

    private void EncodeTrack(TrackInfo trackInfo)
    {
      string mp3TargetDir = mp3ImportDir;

      if (mp3Organize)
      {
        if ((trackInfo.MusicTag.Artist != "Unknown Artist") || (trackInfo.MusicTag.Album != "Unknown Album"))
          mp3TargetDir = mp3ImportDir + "\\" + FilterInvalidChars(trackInfo.MusicTag.Artist) + "\\";
        else
          mp3TargetDir = mp3ImportDir + "\\";
        mp3TargetDir = mp3TargetDir + FilterInvalidChars(trackInfo.MusicTag.Album);

        if ((trackInfo.MusicTag.Artist == "Unknown Artist") && (trackInfo.MusicTag.Album == "Unknown Album") && Directory.Exists(mp3TargetDir))
        {
          int i = 1;
          while (Directory.Exists(string.Format("{0}-{1}", mp3TargetDir, i)))
            ++i;
          mp3TargetDir = string.Format("{0}-{1}", mp3TargetDir, i);

        }
      }
      trackInfo.TargetDir = mp3TargetDir;
      trackInfo.TempFileName = string.Format("temp\\{0:00} " + FilterInvalidChars(trackInfo.MusicTag.Title) + ".mp3", trackInfo.MusicTag.Track);
      trackInfo.TargetFileName = string.Format(mp3TargetDir + "\\{0:00} " + FilterInvalidChars(trackInfo.MusicTag.Title) + ".mp3", trackInfo.MusicTag.Track);

      importQueue.Enqueue(trackInfo);

      if (dlgProgress != null)
      {
        if (importQueue.Count > 1)
          dlgProgress.SetHeading(string.Format(GUILocalizeStrings.Get(1105) + " ({0} " + GUILocalizeStrings.Get(1104) + ")", importQueue.Count));
        else
          dlgProgress.SetHeading(GUILocalizeStrings.Get(1103));
      }

      if (!m_Ripping)
        EncodeThread.Start();
    }

    static GUIDialogProgress dlgProgress;

    static void ThreadEncodeTrack()
    {
      m_Ripping = true;

      if (mp3Background)
        GUIWaitCursor.Show();
      else if (dlgProgress != null)
      {
        dlgProgress.SetPercentage(0);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
        dlgProgress.ShowProgressBar(true);
      }

      while (importQueue.Count > 0)
      {
        TrackInfo trackInfo = (TrackInfo)importQueue.Dequeue();
        if ((dlgProgress != null) && !mp3Background)
        {
          if (importQueue.Count > 0)
            dlgProgress.SetHeading(string.Format(GUILocalizeStrings.Get(1105) + " ({0} " + GUILocalizeStrings.Get(1104) + ")", importQueue.Count + 1));
          else
            dlgProgress.SetHeading(GUILocalizeStrings.Get(1103));

          dlgProgress.SetLine(2, string.Format("{0:00}. {1} - {2}", trackInfo.MusicTag.Track, trackInfo.MusicTag.Artist, trackInfo.MusicTag.Title));
          if (dlgProgress.IsCanceled)
            m_CancelRipping = true;
        }

        if (!m_CancelRipping)
        {
          m_Drive = new CDDrive();
          SaveTrack(trackInfo);
          if (File.Exists(trackInfo.TempFileName) && !m_CancelRipping)
          {
            #region Tagging
            try
            {
              Tags tags = Tags.FromFile(trackInfo.TempFileName);
              tags["TRCK"] = trackInfo.MusicTag.Track.ToString() + "/" + trackInfo.TrackCount.ToString();
              tags["TALB"] = trackInfo.MusicTag.Album;
              tags["TPE1"] = trackInfo.MusicTag.Artist;
              tags["TIT2"] = trackInfo.MusicTag.Title;
              tags["TCON"] = trackInfo.MusicTag.Genre;
              if (trackInfo.MusicTag.Year > 0)
                tags["TYER"] = trackInfo.MusicTag.Year.ToString();
              tags["TENC"] = "Media Portal / Lame";
              tags.Save(trackInfo.TempFileName);
            }
            catch { }
            #endregion
            #region Database
            try
            {
              if (!Directory.Exists(trackInfo.TargetDir))
                Directory.CreateDirectory(trackInfo.TargetDir);

              if (File.Exists(trackInfo.TargetFileName))
                if (mp3ReplaceExisting)
                  File.Delete(trackInfo.TargetFileName);

              if (!File.Exists(trackInfo.TargetFileName))
                File.Move(trackInfo.TempFileName, trackInfo.TargetFileName);

              if (File.Exists(trackInfo.TargetFileName) && mp3Database)
              {
                MusicDatabase dbs = new MusicDatabase();
                Song song = new Song();

                song.FileName = trackInfo.TargetFileName;
                song.Album = trackInfo.MusicTag.Album;
                song.Artist = trackInfo.MusicTag.Artist;
                song.Duration = trackInfo.MusicTag.Duration;
                song.Genre = trackInfo.MusicTag.Genre;
                song.Rating = trackInfo.MusicTag.Rating;
                song.TimesPlayed = trackInfo.MusicTag.TimesPlayed;
                song.Title = trackInfo.MusicTag.Title;
                song.Track = trackInfo.MusicTag.Track;
                song.Year = trackInfo.MusicTag.Year;
                dbs.AddSong(song, true);

                // what about???
                // song.albumId;
                // song.artistId;
                // song.Favorite;
                // song.genreId;
                // song.songId;
                // trackInfo.MusicTag.Comment;
              }
            }
            catch
            {
              Log.Write("CDIMP: Error moving encoded file {0} to new location {1}", trackInfo.TempFileName, trackInfo.TargetFileName);
            }
            #endregion
          }
        }
      }
      if (mp3Background)
        GUIWaitCursor.Hide();
      else
        dlgProgress.Close();
      m_CancelRipping = false;
      m_Ripping = false;
    }

    private static void SaveTrack(TrackInfo trackInfo)
    {
      string targetFileName = trackInfo.MusicTag.Title;
      if (!Directory.Exists("temp"))
        Directory.CreateDirectory("temp");

      if (m_Drive.Open(trackInfo.Item.Path[0]))
      {
        char[] Drives = CDDrive.GetCDDriveLetters();
        if ((Array.IndexOf(Drives, trackInfo.Item.Path[0]) > -1) && (m_Drive.IsCDReady()) && (m_Drive.Refresh()))
        {
          try
          {
            m_Drive.LockCD();
            if (dlgProgress.IsCanceled)
              m_CancelRipping = true;
            if (!m_CancelRipping)
              try
              {
                try
                {
                  WaveFormat Format = new WaveFormat(44100, 16, 2);
                  BE_CONFIG mp3Config = new BE_CONFIG(Format);
                  if (mp3VBR)
                  {
                    mp3Config.format.lhv1.bEnableVBR = 1;
                    if (mp3FastMode)
                      mp3Config.format.lhv1.nVbrMethod = VBRMETHOD.VBR_METHOD_NEW;
                    else
                      mp3Config.format.lhv1.nVbrMethod = VBRMETHOD.VBR_METHOD_DEFAULT;
                    mp3Config.format.lhv1.nVBRQuality = mp3Quality;
                  }
                  else if (mp3CBR)
                  {
                    mp3Config.format.lhv1.bEnableVBR = 0;
                    mp3Config.format.lhv1.nVbrMethod = VBRMETHOD.VBR_METHOD_NONE;
                    mp3Config.format.lhv1.dwBitrate = Convert.ToUInt16(Rates[mp3BitRate]);
                  }
                  else
                  {
                    mp3Config.format.lhv1.bEnableVBR = 1;
                    mp3Config.format.lhv1.nVbrMethod = VBRMETHOD.VBR_METHOD_ABR;
                    mp3Config.format.lhv1.dwVbrAbr_bps = Convert.ToUInt16(Rates[mp3BitRate]);
                  }

                  if (mp3MONO)
                    mp3Config.format.lhv1.nMode = MpegMode.MONO;

                  Stream WaveFile = new FileStream(trackInfo.TempFileName, FileMode.Create, FileAccess.Write);
                  m_Writer = new Mp3Writer(WaveFile, Format, mp3Config);
                  if (!m_CancelRipping) try
                    {
                      Log.Write("CDIMP: Processing track {0}", trackInfo.MusicTag.Track);

                      DateTime InitTime = DateTime.Now;
                      if (m_Drive.ReadTrack(trackInfo.MusicTag.Track, new CdDataReadEventHandler(WriteWaveData), new CdReadProgressEventHandler(CdReadProgress)) > 0)
                      {
                        if (dlgProgress.IsCanceled)
                          m_CancelRipping = true;
                        if (!m_CancelRipping)
                        {
                          TimeSpan Duration = DateTime.Now - InitTime;
                          double Speed = m_Drive.TrackSize(trackInfo.MusicTag.Track) / Duration.TotalSeconds / Format.nAvgBytesPerSec;
                          Log.Write("CDIMP: Done reading track {0} at {1:0.00}x speed", trackInfo.MusicTag.Track, Speed);
                        }
                      }
                      else
                      {
                        Log.Write("CDIMP: Error reading track {0}", trackInfo.MusicTag.Track);
                        m_Writer.Close();
                        WaveFile.Close();
                        if (File.Exists(trackInfo.TempFileName))
                          try
                          {
                            File.Delete(trackInfo.TempFileName);
                          }
                          catch { }
                        //progressBar1.Value = 0;
                      }
                    }
                    finally
                    {
                      m_Writer.Close();
                      m_Writer = null;
                      WaveFile.Close();
                    }
                }
                finally
                { }
              }
              finally
              {
                m_Drive.UnLockCD();
              }
          }
          finally
          {
            //progressBar1.Value = 0;
          }
        }
        if (dlgProgress.IsCanceled)
          m_CancelRipping = true;
        if (m_CancelRipping)
        {
          if (File.Exists(trackInfo.TempFileName))
            File.Delete(trackInfo.TempFileName);
          m_Drive.UnLockCD();
        }
      }
    }
  }
}