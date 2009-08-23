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
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Profile;
using MediaPortal.Util;
using Win32.Utils.Cd;

namespace MediaPortal.Ripper
{
  /// <summary>
  /// AutoPlay functionality.
  /// </summary>
  public class AutoPlay
  {
    #region base variables

    private static DeviceVolumeMonitor _deviceMonitor;
    private static string m_dvd = "No";
    private static string m_audiocd = "No";
    private static ArrayList allfiles;

    // A hidden window to allow us to listen to WndProc messages
    // Winamp viz doesn't like when it receives notify that the WndProc handler has changed
    private static NativeWindow _nativeWindow;
    private static IntPtr _windowHandle;

    // For event filtering
    private static DateTime _wakeupTime = new DateTime();
    private static DateTime _volumeRemovalTime = new DateTime();
    private static int _wakeupDelay = 10000;       // In milliseconds
    private static int _volumeRemovalDelay = 7500; // In milliseconds

    private enum MediaType
    {
      UNKNOWN = 0,
      DVD = 1,
      AUDIO_CD = 2,
      PHOTOS = 3,
      VIDEOS = 4,
      AUDIO = 5,
      BLURAY = 6,
      HDDVD = 7,
      VCD = 8
    }

    #endregion

    /// <summary>
    /// singleton. Dont allow any instance of this class so make the constructor private
    /// </summary>
    private AutoPlay()
    {
    }

    /// <summary>
    /// Static constructor of the autoplay class.
    /// </summary>
    static AutoPlay()
    {
      m_dvd = "No";
      m_audiocd = "No";
      allfiles = new ArrayList();

      _nativeWindow = new NativeWindow();
      CreateParams cp = new CreateParams();
      _nativeWindow.CreateHandle(cp);
      _windowHandle = _nativeWindow.Handle;
    }

    ~AutoPlay()
    {
      _deviceMonitor.Dispose();
      _deviceMonitor = null;
    }

    /// <summary>
    /// Starts listening for events on the optical drives.
    /// </summary>
    public static void StartListening()
    {
      LoadSettings();
      StartListeningForEvents();
    }

    /// <summary>
    /// Stops listening for events on the optical drives and cleans up.
    /// </summary>
    public static void StopListening()
    {
      StopListeningForEvents();
    }

    /// <summary>
    /// Provides system last wake uptime (from S3/S4 state)
    /// </summary>
    public static void SetWakeupTime(DateTime wakeupTime)
    {
      _wakeupTime = wakeupTime;
    }

    #region initialization + serialization

    private static void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        m_dvd = xmlreader.GetValueAsString("dvdplayer", "autoplay", "Ask");
        m_audiocd = xmlreader.GetValueAsString("audioplayer", "autoplay", "No");
        _wakeupDelay = xmlreader.GetValueAsInt("debug", "wakeupDelay", 10000);
        _volumeRemovalDelay = xmlreader.GetValueAsInt("debug", "volumeRemovalDelay", 7500);
      }
    }

    private static void StartListeningForEvents()
    {
      _deviceMonitor = new DeviceVolumeMonitor(_windowHandle);
      _deviceMonitor.OnVolumeInserted += new DeviceVolumeAction(VolumeInserted);
      _deviceMonitor.OnVolumeRemoved += new DeviceVolumeAction(VolumeRemoved);      
      _deviceMonitor.AsynchronousEvents = true;
      _deviceMonitor.Enabled = true;
    }

    #endregion

    #region cleanup

    private static void StopListeningForEvents()
    {
      if (_deviceMonitor != null)
      {
        _deviceMonitor.Enabled = false;
        _deviceMonitor.OnVolumeInserted -= new DeviceVolumeAction(VolumeInserted);
        _deviceMonitor.OnVolumeRemoved -= new DeviceVolumeAction(VolumeRemoved);        
        _deviceMonitor.Dispose();
      }
      _deviceMonitor = null;
    }

    #endregion

    #region capture events
    
    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is removed from a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDRemoved(string DriveLetter)
    {
      Log.Info("Media removed from drive {0}", DriveLetter);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
                                      (int) GUIWindow.Window.WINDOW_MUSIC_FILES,
                                      GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = String.Format("{0}", DriveLetter);
      msg.SendToTargetWindow = true;
      GUIWindowManager.SendThreadMessage(msg);

      msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
                           (int) GUIWindow.Window.WINDOW_VIDEOS,
                           GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = DriveLetter;
      msg.SendToTargetWindow = true;
      GUIWindowManager.SendThreadMessage(msg);
    }

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is inserted into a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDInserted(string DriveLetter)
    {      
      Log.Info("Media inserted into drive {0}", DriveLetter);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_INSERTED,
                                      (int) 0,
                                      GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = DriveLetter;
      GUIWindowManager.SendThreadMessage(msg);
    }

    /// <summary>
    /// The event that gets triggered whenever a new volume is removed.
    /// </summary>	
    private static void VolumeRemoved(int bitMask)
    {
      string driveLetter = _deviceMonitor.MaskToLogicalPaths(bitMask);
            
      _volumeRemovalTime = DateTime.Now;
      TimeSpan ts = DateTime.Now - _wakeupTime;

      // AnyDVD is causing media removed & inserted events when waking up from S3/S4 
      // We need to filter out those as it could trigger false autoplay event 
      // Event filtering is skipped for mounted images
      if (DaemonTools.GetVirtualDrive() != driveLetter
        && ts.TotalMilliseconds < _wakeupDelay)
      {
        Log.Info("Ignoring volume removed event - drive {0} - time after wakeup {1} s",
          driveLetter, ts.TotalMilliseconds / 1000);
        return;
      }
      Log.Debug("Volume removed from drive {0}", driveLetter);
      Log.Debug("  time after wakeup {0} s", ts.TotalMilliseconds / 1000);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED,
                                      (int) 0,
                                      GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = driveLetter;
      GUIWindowManager.SendThreadMessage(msg);
      CDRemoved(driveLetter);
    }

    /// <summary>
    /// The event that gets triggered whenever a new volume is inserted.
    /// </summary>	
    private static void VolumeInserted(int bitMask)
    {
      string driveLetter = _deviceMonitor.MaskToLogicalPaths(bitMask);

      TimeSpan tsWakeup = DateTime.Now - _wakeupTime;
      TimeSpan tsVolumeRemoval = DateTime.Now - _volumeRemovalTime;

      // no AnyDVD check is made for mounted images
      if (DaemonTools.GetVirtualDrive() != driveLetter)
      {
        // AnyDVD is causing media removed & inserted events when waking up from S3/S4 
        // We need to filter out those as it could trigger false autoplay event 
        if (tsWakeup.TotalMilliseconds < (_wakeupDelay + _volumeRemovalDelay)
          || (tsVolumeRemoval.TotalMilliseconds < _volumeRemovalDelay
          && tsWakeup.TotalMilliseconds < (_wakeupDelay + _volumeRemovalDelay * 2)))
        {
          Log.Info("Ignoring volume inserted event - drive {0} - timespan wakeup {1} s - timespan volume removal {2} s",
            driveLetter, tsWakeup.TotalMilliseconds / 1000, tsVolumeRemoval.TotalMilliseconds / 1000);
          Log.Debug("   _wakeupDelay = {0} _volumeRemovalDelay = {1}", _wakeupDelay, _volumeRemovalDelay);
          return;
        }
      }

      Log.Debug("Volume inserted into drive {0}", driveLetter);     
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED,
                                      (int) 0,
                                      GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
      msg.Label = driveLetter;
      GUIWindowManager.SendThreadMessage(msg);
      if(Util.Utils.IsDVD(driveLetter))
        CDInserted(driveLetter);
    }

    private static bool ShouldWeAutoPlay(MediaType iMedia)
    {      
      Log.Info("Check if we want to autoplay a {0}", iMedia);
      if (GUIWindowManager.IsRouted)
      {
        return false;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, null);
      msg.Param1 = 713;
      switch (iMedia)
      {
        case MediaType.PHOTOS: // Photo
          msg.Param2 = 530;
          break;
        case MediaType.VIDEOS: // Movie
          msg.Param2 = 531;
          break;
        case MediaType.AUDIO: // Audio
          msg.Param2 = 532;
          break;
        default:
          msg.Param2 = 714;
          break;
      }
      msg.Param3 = 0;
      GUIWindowManager.SendMessage(msg);
      if (msg.Param1 != 0)
      {
        //stop tv...
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TV, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);

        //stop radio...
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
        return true;
      }
      return false;
    }

    public static void ExamineCD(string strDrive)
    {
      ExamineCD(strDrive, false);
    }

    public static void ExamineCD(string strDrive, bool forcePlay)
    {
      // don't interrupt if we're already playing something
      if (g_Player.Playing && !forcePlay)
      {
        return;
      }      
      if (strDrive == null || strDrive.Length==0)
      {
        return;
      }     

      StopListening();

      GUIMessage msg;
      bool shouldPlay  = false;
      switch (DetectMediaType(strDrive))
      {
        case MediaType.AUDIO_CD:
          Log.Info("Audio CD inserted into drive {0}", strDrive);
          //m_audiocd tells us if we want to autoplay or not
          bool PlayAudioCd = false;
          if (forcePlay || m_audiocd == "Yes")
          {
            // Automaticaly play the CD
            PlayAudioCd = true;
            Log.Info("CD Autoplay = auto");
          }
          else if (m_audiocd == "Ask")
          {
            if (ShouldWeAutoPlay(MediaType.AUDIO_CD))
            {
              PlayAudioCd = true;
              Log.Info("CD Autoplay, answered yes");
            }
            else
            {
              Log.Info("CD Autoplay, answered no");
            }
          }
          if (PlayAudioCd)
          {            
            // Send a message with the drive to the message handler. 
            // The message handler will play the CD
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD,
                                 (int) GUIWindow.Window.WINDOW_MUSIC_FILES,
                                 GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.PHOTOS:
          if (forcePlay || ShouldWeAutoPlay(MediaType.PHOTOS))
          {
            Log.Info("Media with photo's inserted into drive {0}", strDrive);
            GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_PICTURES);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME,
                                 (int) GUIWindow.Window.WINDOW_PICTURES,
                                 GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.BLURAY:
          Log.Info("BLU-RAY volume inserted {0}", strDrive);
          GUIMessage msgBluray = new GUIMessage(GUIMessage.MessageType.GUI_MSG_BLURAY_DISK_INSERTED, 0, 0, 0, 0, 0, null);
          msgBluray.Label = strDrive;
          GUIGraphicsContext.SendMessage(msgBluray);
          break;

        case MediaType.HDDVD:
          Log.Info("HD DVD volume inserted {0}", strDrive);
          GUIMessage msgHDDVD = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HDDVD_DISK_INSERTED, 0, 0, 0, 0, 0, null);
          msgHDDVD.Label = strDrive;
          GUIGraphicsContext.SendMessage(msgHDDVD);
          break;

        case MediaType.DVD:
          Log.Info("DVD volume inserted {0}", strDrive);          
          if (forcePlay || m_dvd == "Yes")
          {
            Log.Info("Autoplay: Yes, start DVD in {0}", strDrive);
            shouldPlay = true;
          }
          else if (m_dvd == "Ask")
          {
            if (ShouldWeAutoPlay(MediaType.DVD))
            {
              Log.Info("Autoplay: Answered yes, start DVD in {0}", strDrive);
              shouldPlay = true;
            }
            else
            {
              Log.Info("Autoplay: Answered no, do not start DVD in {0}", strDrive);
            }
          }
          if (shouldPlay)
          {
            g_Player.PlayDVD(strDrive + @"\VIDEO_TS\VIDEO_TS.IFO");
            g_Player.ShowFullScreenWindow();
          }
          break;

        case MediaType.VCD:
          Log.Info("VCD volume inserted {0}", strDrive);
          bool bPlay = false;
          if (forcePlay || m_dvd == "Yes")
          {
            Log.Info("Autoplay: Yes, start VCD in {0}", strDrive);
            bPlay = true;
          }
          else if (m_dvd == "Ask")
          {
            if (ShouldWeAutoPlay(MediaType.DVD))
            {
              Log.Info("Autoplay: Answered yes, start VCD in {0}", strDrive);
              bPlay = true;
            }
            else
            {
              Log.Info("Autoplay: Answered no, do not start VCD in {0}", strDrive);
            }
          }
          if (bPlay)
          {
            long lMaxLength = 0;
            string sPlayFile = "";
            string[] files = Directory.GetFiles(strDrive + "\\MPEGAV");
            foreach (string file in files)
            {
              FileInfo info = new FileInfo(file);
              if (info.Length > lMaxLength)
              {
                lMaxLength = info.Length;
                sPlayFile = file;
              }
            }
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            g_Player.Play(sPlayFile);
          }
          break;

        case MediaType.VIDEOS:
          Log.Info("Video volume inserted {0}", strDrive);
          if (forcePlay || ShouldWeAutoPlay(MediaType.VIDEOS))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEOS);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
                                 (int)GUIWindow.Window.WINDOW_VIDEOS,
                                 GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.AUDIO:
          Log.Info("Media with audio inserted into drive {0}", strDrive);
          
          PlayAudioCd = false;
          if (forcePlay || m_audiocd == "Yes")
          {
            // Automaticaly play the CD
            Log.Debug("Adding all Audio Files to Playlist");
            PlayAudioFiles();
          }
          else if (m_audiocd == "Ask")
          {
            if (ShouldWeAutoPlay(MediaType.AUDIO))
            {
              PlayAudioCd = true;
              Log.Info("Audio Autoplay, answered yes");
            }
            else
            {
              Log.Info("Audio Autoplay, answered no");
            }
          }
          if (PlayAudioCd)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_FILES);
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
                                   (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
                                   GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
              msg.Label = strDrive;
              msg.SendToTargetWindow = true;
              GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        default:
          Log.Info("Unknown media type inserted into drive {0}", strDrive);
          break;
      }

      StartListening();
    }

    public static void ExamineVolume(string strDrive)
    {
      // don't interrupt if we're already playing something
      if (g_Player.Playing)
      {
        return;
      }
      if (strDrive == null || strDrive.Length == 0 || Util.Utils.IsDVD(strDrive))
      {
        return;
      }
      
      GUIMessage msg;
      switch (DetectMediaType(strDrive))
      {        
        case MediaType.PHOTOS:
          Log.Info("Photo volume inserted {0}", strDrive);
          if (ShouldWeAutoPlay(MediaType.PHOTOS))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_PICTURES);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
                                 (int)GUIWindow.Window.WINDOW_PICTURES,
                                 GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.VIDEOS:
          Log.Info("Video volume inserted {0}", strDrive);
          if (ShouldWeAutoPlay(MediaType.VIDEOS))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEOS);
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
                                 (int)GUIWindow.Window.WINDOW_VIDEOS,
                                 GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
            msg.Label = strDrive;
            msg.SendToTargetWindow = true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;

        case MediaType.AUDIO:
          Log.Info("Audio volume inserted {0}", strDrive);
          if (ShouldWeAutoPlay(MediaType.AUDIO))
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_FILES);
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY,
                                   (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
                                   GUIWindowManager.ActiveWindow, 0, 0, 0, 0);
              msg.Label = strDrive;
              msg.SendToTargetWindow = true;
              GUIWindowManager.SendThreadMessage(msg);
            }
          break;

        default:
          Log.Info("ExamineVolume: Unknown media type inserted into drive {0}", strDrive);
          break;
      }
    }

    private static bool isARedBookCD(string driveLetter)
    {
      try
      {
        if (driveLetter.Length < 1)
        {
          return false;
        }
        int cddaTracks = 0;
        CDDrive m_Drive = new CDDrive();

        if (m_Drive.IsOpened)
        {
          m_Drive.Close();
        }
        if (m_Drive.Open(driveLetter[0]) && m_Drive.IsCDReady() && m_Drive.Refresh())
        {
          cddaTracks = m_Drive.GetNumAudioTracks();
          m_Drive.Close();
          // another method to find out audio cd as first one can fail with some discs
          if (cddaTracks <= 0)
          {
            string[] files = Directory.GetFiles(driveLetter,"*.cda",SearchOption.TopDirectoryOnly);
            if (files != null && files.Length > 0)
            {
              cddaTracks = files.Length;
            }
          }          
        }
        if (cddaTracks > 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    private static void GetAllFiles(string strFolder, ref ArrayList allfiles)
    {
      if (strFolder == null)
      {
        return;
      }
      if (strFolder.Length == 0)
      {
        return;
      }
      if (allfiles == null)
      {
        return;
      }
      try
      {
        string[] files = Directory.GetFiles(strFolder);
        if (files != null && files.Length > 0)
        {
          for (int i = 0; i < files.Length; ++i)
          {
            allfiles.Add(files[i]);
          }
        }
        string[] folders = Directory.GetDirectories(strFolder);
        if (folders != null && folders.Length > 0)
        {
          for (int i = 0; i < folders.Length; ++i)
          {
            GetAllFiles(folders[i], ref allfiles);
          }
        }
      }
      catch (Exception)
      {
      }
    }

    /// <summary>
    /// Detects the media type of the CD/DVD inserted into a drive.
    /// </summary>
    /// <param name="driveLetter">The drive that contains the data.</param>
    /// <returns>The media type of the drive.</returns>
    private static MediaType DetectMediaType(string strDrive)
    {      
      if (strDrive == null)
      {
        return MediaType.UNKNOWN;
      }
      if (strDrive == string.Empty)
      {
        return MediaType.UNKNOWN;
      }
      try
      {
        if (Directory.Exists(strDrive + "\\VIDEO_TS"))
        {
          return MediaType.DVD;
        }

        if (File.Exists(strDrive + "\\BDMV\\index.bdmv"))
        {
          return MediaType.BLURAY;
        }

        if (Directory.Exists(strDrive + "\\HVDVD_TS"))
        {
          return MediaType.HDDVD;
        }

        if (Directory.Exists(strDrive + "\\MPEGAV"))
        {
          return MediaType.VCD;
        }

        if (isARedBookCD(strDrive))
        {
          return MediaType.AUDIO_CD;
        }

        allfiles.Clear();
        GetAllFiles(strDrive + "\\", ref allfiles);
        foreach (string FileName in allfiles)
        {
          string ext = Path.GetExtension(FileName).ToLower();
          if (Util.Utils.IsVideo(FileName))
          {
            return MediaType.VIDEOS;
          }
        }

        foreach (string FileName in allfiles)
        {
          string ext = Path.GetExtension(FileName).ToLower();
          if (Util.Utils.IsAudio(FileName))
          {
            return MediaType.AUDIO;
          }
        }

        foreach (string FileName in allfiles)
        {
          if (Util.Utils.IsPicture(FileName))
          {
            return MediaType.PHOTOS;
          }
        }
      }
      catch (Exception)
      {
      }
      return MediaType.UNKNOWN;
    }

    private static void PlayAudioFiles()
    {
      PlayListPlayer playlistPlayer = PlayListPlayer.SingletonPlayer;

      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
      foreach (string file in allfiles)
      {
        if (!Util.Utils.IsAudio(file))
        {
          continue;
        }

        PlayListItem item = new PlayListItem();
        item.FileName = file;
        item.Type = PlayListItem.PlayListItemType.Audio;
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(item);
      }

      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // and start playing it
        Log.Debug("Start playing Playlist");
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Reset();
        playlistPlayer.Play(0);

        // and activate the playlist window
        GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
      }
    }
    #endregion
  }
}