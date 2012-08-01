#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using System.Linq;

namespace MediaPortal.Ripper
{
  /// <summary>
  /// AutoPlay functionality.
  /// </summary>
  public class AutoPlay
  {
    #region base variables

    private static string autoplayVideo;
    private static string autoplayAudio;
    private static string autoplayPhoto;
    private static ArrayList mediaFiles;
    private static bool enabled;
    private static MediaType mediaType;
    private static MediaSubType mediaSubType;

    public enum MediaType // global category
    {
      UNKNOWN = 0,
      PHOTO = 1,
      VIDEO = 2,
      AUDIO = 3
    }

    public enum MediaSubType // add here new formats
    {
      UNKNOWN = 0,
      DVD = 1,
      AUDIO_CD = 2,
      BLURAY = 3,
      HDDVD = 4,
      VCD = 5,
      FILES = 6
    }

    #endregion

    /// <summary>
    /// singleton. Dont allow any instance of this class so make the constructor private
    /// </summary>
    private AutoPlay() {}

    /// <summary>
    /// Static constructor of the autoplay class.
    /// </summary>
    static AutoPlay()
    {
      LoadSettings();
      mediaFiles = new ArrayList();
      enabled = true;
    }

    ~AutoPlay() {}

    /// <summary>
    /// Starts listening for events on the optical drives.
    /// </summary>
    public static void StartListening()
    {
      enabled = true;
    }

    /// <summary>
    /// Stops listening for events on the optical drives and cleans up.
    /// </summary>
    public static void StopListening()
    {
      enabled = false;
    }

    public static void ExamineVolume(string strDrive)
    {
      ExamineCD(strDrive, false);
    }

    public static void ExamineCD(string strDrive)
    {
      ExamineCD(strDrive, false);
    }

    public static void ExamineCD(string strDrive, bool forcePlay)
    {
      if (!enabled)
        return;

      if (string.IsNullOrEmpty(strDrive) || (!forcePlay && DaemonTools.GetVirtualDrive().StartsWith(strDrive)))

        return;

      StopListening();
      DetectMediaType(strDrive);

      if (!forcePlay && !ShouldWeAutoPlay(mediaType))
      {
        StartListening();
        return;
      }

      Log.Info("Autoplay: Start playing media type '{0}/{1}' from drive '{2}'", mediaType, mediaSubType, strDrive);

      // Set time for avoid ANYDVD (removal/insert detection)
      RemovableDriveHelper.SetExamineCDTime(DateTime.Now);

      switch (mediaType)
      {
        case MediaType.VIDEO:
          {
            switch (mediaSubType)
            {
              case MediaSubType.FILES:
              case MediaSubType.DVD:
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME, 0, 0, 0, 0, 0, null);
                msg.Label = strDrive;
                msg.Param1 = (int)mediaType;
                msg.Param2 = (int)mediaSubType;
                msg.Object = mediaFiles;
                GUIGraphicsContext.SendMessage(msg);
                break;

              case MediaSubType.BLURAY:
              case MediaSubType.HDDVD:
                msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME, 0, 0, 0, 0, 0, null);
                msg.Label = strDrive;
                msg.Param1 = (int)mediaType;
                msg.Param2 = (int)mediaSubType;
                GUIGraphicsContext.SendMessage(msg);
                break;

              case MediaSubType.VCD:
                long lMaxLength = 0;
                string sPlayFile = "";
                string[] files = Directory.GetFiles(Path.Combine(strDrive, "MPEGAV"));
                foreach (string file in files)
                {
                  FileInfo info = new FileInfo(file);
                  if (info.Length > lMaxLength)
                  {
                    lMaxLength = info.Length;
                    sPlayFile = file;
                  }
                }
                mediaFiles.Clear();
                mediaFiles.Add(sPlayFile);
                msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME, 0, 0, 0, 0, 0, null);
                msg.Label = strDrive;
                msg.Param1 = (int)mediaType;
                msg.Param2 = (int)mediaSubType;
                msg.Object = mediaFiles;
                GUIGraphicsContext.SendMessage(msg);
                break;
            }
          }
          break;

        case MediaType.AUDIO:
          switch (mediaSubType)
          {
            case MediaSubType.FILES:
            case MediaSubType.AUDIO_CD:
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME, 0, 0, 0, 0, 0, null);
              msg.Label = strDrive;
              msg.Param1 = (int)mediaType;
              msg.Param2 = (int)mediaSubType;
              msg.Object = mediaFiles;
              GUIGraphicsContext.SendMessage(msg);
              break;
          }
          break;

        case MediaType.PHOTO:
          switch (mediaSubType)
          {
            case MediaSubType.FILES:
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME, 0, 0, 0, 0, 0, null);
              msg.Label = strDrive;
              msg.Param1 = (int)mediaType;
              msg.Param2 = (int)mediaSubType;
              msg.Object = mediaFiles;
              GUIGraphicsContext.SendMessage(msg);
              break;
          }
          break;

        default:
          Log.Info("Unknown media type inserted into drive {0}", strDrive);
          break;
      }
      StartListening();
    }

    #region initialization + serialization

    private static void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        autoplayVideo = xmlreader.GetValueAsString("general", "autoplay_video", "Ask");
        autoplayAudio = xmlreader.GetValueAsString("general", "autoplay_audio", "Ask");
        autoplayPhoto = xmlreader.GetValueAsString("general", "autoplay_photo", "Ask");
      }
    }

    #endregion

    # region private methods

    private static bool ShouldWeAutoPlay(MediaType iMedia)
    {
      Log.Info("Check if we want to autoplay media type: {0}", iMedia);
      if (GUIWindowManager.IsRouted)
        return false;

      string askPlaying;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, null);
      msg.Param1 = 713;
      switch (iMedia)
      {
        case MediaType.PHOTO:
          msg.Param2 = 530;
          askPlaying = autoplayPhoto;
          break;
        case MediaType.VIDEO:
          msg.Param2 = 531;
          askPlaying = autoplayVideo;
          break;
        case MediaType.AUDIO:
          msg.Param2 = 532;
          askPlaying = autoplayAudio;
          break;
        default:
          msg.Param2 = 714;
          askPlaying = "No";
          break;
      }
      msg.Param3 = 0;
      switch (askPlaying)
      {
        case "Ask":
          GUIWindowManager.SendMessage(msg);
          if (msg.Param1 != 0)
            return true;
          break;
        case "Yes":
          return true;
      }
      return false;
    }

    /// <summary>
    /// Detects the media type of the CD/DVD inserted into a drive.
    /// </summary>
    /// <param name="driveLetter">The drive that contains the data.</param>
    private static void DetectMediaType(string strDrive)
    {
      mediaType = MediaType.UNKNOWN;
      mediaSubType = MediaSubType.UNKNOWN;

      if (string.IsNullOrEmpty(strDrive))
        return;

      try
      {
        if (Directory.Exists(Path.Combine(strDrive, "VIDEO_TS")))
        {
          mediaType = MediaType.VIDEO;
          mediaSubType = MediaSubType.DVD;
          return;
        }

        if (Directory.Exists(Path.Combine(strDrive, "BDMV")))
        {
          mediaType = MediaType.VIDEO;
          mediaSubType = MediaSubType.BLURAY;
          return;
        }

        if (Directory.Exists(Path.Combine(strDrive, "HVDVD_TS")))
        {
          mediaType = MediaType.VIDEO;
          mediaSubType = MediaSubType.HDDVD;
          return;
        }

        if (Directory.Exists(Path.Combine(strDrive, "MPEGAV")))
        {
          mediaType = MediaType.VIDEO;
          mediaSubType = MediaSubType.VCD;
          return;
        }

        GetMediaTypeFromFiles(Path.GetPathRoot(strDrive));
      }
      catch (Exception) {}
    }

    private static void GetMediaTypeFromFiles(string strFolder)
    {
      if (string.IsNullOrEmpty(strFolder))
        return;

      try
      {
        ArrayList audioFiles = new ArrayList();
        ArrayList photoFiles = new ArrayList();
        ArrayList videoFiles = new ArrayList();
        mediaSubType = MediaSubType.FILES;

        string[] files = null;
        DirectoryInfo di = new DirectoryInfo(strFolder);
        if (di != null)
          files = di.GetFiles("*.*", SearchOption.AllDirectories).Select(a => a.FullName).ToArray();

        if (files != null && files.Length > 0)
        {
          //for (int i = files.Length - 1; i >= 0; i--)
          for (int i = 0; i < files.Length; i++)
          {
            if (Util.Utils.IsVideo(files[i]))
            {
              videoFiles.Add(files[i]);
            }
            else if (Util.Utils.IsAudio(files[i]))
            {
              audioFiles.Add(files[i]);
              if (Path.GetExtension(files[i]).ToLower() == ".cda")
                mediaSubType = MediaSubType.AUDIO_CD;
            }
            else if (Util.Utils.IsPicture(files[i]))
            {
              photoFiles.Add(files[i]);
            }
          }
        }

        mediaFiles.Clear();
        if (videoFiles.Count > 0)
        {
          mediaType = MediaType.VIDEO;
          mediaFiles = videoFiles;
        }
        else if (audioFiles.Count > 0)
        {
          mediaType = MediaType.AUDIO;
          mediaFiles = audioFiles;
        }
        else if (photoFiles.Count > 0)
        {
          mediaType = MediaType.PHOTO;
          mediaFiles = photoFiles;
        }
      }
      catch (Exception) {}
    }

    #endregion
  }
}