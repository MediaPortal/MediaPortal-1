using System;
using System.IO;  
using System.Collections;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Player;
using MediaPortal.Playlists;

namespace MediaPortal.AutoPlay
{
  /// <summary>
  /// AutoPlay functionality.
  /// </summary>
  public class AutoPlay
  {
    #region base variables
    
    static ArrayList m_vecList   = null;
    static Ripper.CDDrive [] m_drives=new CDDrive[0];
    static bool m_dvd=false;
    static bool m_audiocd=false;

    enum MediaType
    {
      UNKNOWN      = 0,
      DVD          = 1,
      AUDIO_CD     = 2,
    }

    #endregion
		
    /// <summary>
    /// Static constructor of the autoplay class.
    /// </summary>
    static AutoPlay()   
    {
      m_vecList   = null;
      Ripper.CDDrive [] m_drives=new CDDrive[0];
      m_dvd=false;
      m_audiocd=false;
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
      CleanupDriveList();
    }
    #region initialization + serialization
   
    private static void LoadSettings()
    {
      try
      {
        using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          m_vecList=new ArrayList();
          for (int i=0; i < 20; i++)
          {
            string strShareName=String.Format("sharename{0}",i);
            string strSharePath=String.Format("sharepath{0}",i);
            string sharename=xmlreader.GetValueAsString("music", strShareName,"");
            string sharepath=xmlreader.GetValueAsString("music", strSharePath,"");
            // Drive found add drive to drivelist
            if (Util.Utils.IsDVD(sharepath))
            {
              if (!m_vecList.Contains(sharepath))m_vecList.Add(sharepath);
            }
          }
          for (int i=0; i < 20; i++)
          {
            string strShareName=String.Format("sharename{0}",i);
            string strSharePath=String.Format("sharepath{0}",i);
            string sharename=xmlreader.GetValueAsString("movies", strShareName,"");
            string sharepath=xmlreader.GetValueAsString("movies", strSharePath,"");
            // Drive found add drive to drivelist
            if (Util.Utils.IsDVD(sharepath))
            {
              if (!m_vecList.Contains(sharepath))m_vecList.Add(sharepath);
            }
          }
          for (int i=0; i < 20; i++)
          {
            string strShareName=String.Format("sharename{0}",i);
            string strSharePath=String.Format("sharepath{0}",i);
            string sharename=xmlreader.GetValueAsString("pictures", strShareName,"");
            string sharepath=xmlreader.GetValueAsString("pictures", strSharePath,"");
            // Drive found add drive to drivelist
            if (Util.Utils.IsDVD(sharepath))
            {
              if (!m_vecList.Contains(sharepath))m_vecList.Add(sharepath);
            }
          }
          // read autoplay information
          m_dvd=xmlreader.GetValueAsBool("dvdplayer","autoplay",true);
          m_audiocd=xmlreader.GetValueAsBool("audioplayer","autoplay",true);

        }
      }
      catch(Exception ex)
      {
        Log.Write("exception in AutoPlay.LoadSettings() {0} {1} {2}",  
          ex.Message,ex.Source,ex.StackTrace);
      }
    }

    private static void StartListeningForEvents()
    {
      int nrOfDrives=m_vecList.Count;
      if (nrOfDrives<=0) return;
      try
      {
        m_drives=new Ripper.CDDrive[nrOfDrives];
        for (int i=0;i<nrOfDrives;i++)
        {
          m_drives[i]=new CDDrive();
          m_drives[i].Open(((string)m_vecList[i])[0]);
          m_drives[i].CDInserted+=new CDDrive.CDNotificationHandler(CDInserted);
          m_drives[i].CDRemoved+=new CDDrive.CDNotificationHandler(CDRemoved);
        }
      }
      catch(Exception ex)
      {
        Log.Write("exception in AutoPlay.StartListeningForEvents() {0} {1} {2}",  
          ex.Message,ex.Source,ex.StackTrace);
      }
    }
        
    #endregion

    #region cleanup

    private static void StopListeningForEvents()
    {
      try
      {
        for (int i=0;i<m_drives.Length;i++)
        {
          m_drives[i].Close();
        }
        
      }
      catch(Exception ex)
      {
        Log.Write("exception in AutoPlay.StopListeningForEvents() {0} {1} {2}",  
          ex.Message,ex.Source,ex.StackTrace);
      }
    }

    private static void CleanupDriveList()
    {
      m_drives=new CDDrive[0];
      m_vecList=null;
    }
    #endregion

    #region capture events

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is removed from a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDRemoved(char DriveLetter)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CD_REMOVED,
        (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
        GUIWindowManager.ActiveWindow,0,0,0,0);
      msg.Label=String.Format("{0}:", DriveLetter);

      Log.Write("media removed from drive {0}",DriveLetter);  
      msg.SendToTargetWindow=true;
      GUIWindowManager.SendThreadMessage(msg);
    }

    /// <summary>
    /// The event that gets triggered whenever  CD/DVD is inserted into a drive.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CDInserted(char DriveLetter)
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      switch(DetectMediaType(DriveLetter))       
      {         
        case MediaType.DVD:
          Log.Write("DVD inserted into drive {0}",DriveLetter);
          if (m_dvd)
          {
            if (null==dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(713));
            dlgYesNo.SetLine(0, GUILocalizeStrings.Get(714));
            dlgYesNo.SetLine(1, "");
            dlgYesNo.SetLine(2, "");
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            if (!dlgYesNo.IsConfirmed) return;
            g_Player.PlayDVD();
          }
          break;                  
        case MediaType.AUDIO_CD:
          Log.Write("Audio CD inserted into drive {0}",DriveLetter);
          if (m_audiocd)
          {
            if (null==dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(713));
            dlgYesNo.SetLine(0, GUILocalizeStrings.Get(715));
            dlgYesNo.SetLine(1, "");
            dlgYesNo.SetLine(2, "");
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            if (!dlgYesNo.IsConfirmed) return;
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD,
              (int)GUIWindow.Window.WINDOW_MUSIC_FILES,
              GUIWindowManager.ActiveWindow,0,0,0,0);
            msg.Label=String.Format("{0}:", DriveLetter);
            msg.SendToTargetWindow=true;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;                 

        default:
          Log.Write("Unknown media type inserted into drive {0}",DriveLetter);  
          break;      
      }
    }
    
    /// <summary>
    /// Detects the media type of the CD/DVD inserted into a drive.
    /// </summary>
    /// <param name="driveLetter">The drive that contains the data.</param>
    /// <returns>The media type of the drive.</returns>
    private static MediaType DetectMediaType(char driveLetter)
    {
      if (Directory.Exists(driveLetter+":\\VIDEO_TS"))
      {
        return MediaType.DVD;
      }
      if (Directory.GetFiles(driveLetter+":\\", "*.cda").Length!=0)
      {
        return MediaType.AUDIO_CD;
      }

      return MediaType.UNKNOWN;
    }
    #endregion
  }
}