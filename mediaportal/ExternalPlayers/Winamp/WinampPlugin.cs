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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace MediaPortal.WinampPlayer
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  [PluginIcons("ExternalPlayers.Winamp.WinampLogo.png", "ExternalPlayers.Winamp.WinampLogoDisabled.png")]
  public class WinampPlugin : IExternalPlayer
  {
    private const string m_author = "int_20h";
    private const string m_player = "Winamp";
    private const string m_version = "1.0";
    private bool m_bStoppedManualy = false;

    /*
    private string[] m_supportedExtensions = new string[]{".cda", ".mid", ".midi", ".rmi", ".kar", ".miz", ".mod", ".mdz", ".nst",
                                                          ".stm", ".stz", ".s3m", ".s3z", ".it", ".itz", ".xm", ".xmz", ".mtm", 
                                                          ".ult", ".669", ".far", ".amf", ".okt", ".ptm", ".mp3", ".mp2", ".mp1",
                                                          ".aac", ".m4a", ".mp4", ".nsa", ".ogg", ".wav", ".voc", ".au", ".snd",
                                                          ".aif", ".aiff", ".wma", ".m3u", ".pls"};
    */
    private string[] m_supportedExtensions = new string[0];

    private WinampController m_winampController = null;
    private string m_strCurrentFile = null;
    private int m_volume = 100;
    private bool _notifyPlaying = false;
    private bool _isCDA = false;

    public WinampPlugin()
    {
    }

    public override void ShowPlugin()
    {
      ConfigurationForm confForm = new ConfigurationForm();
      confForm.ShowDialog();
    }

    public override string Description()
    {
      if (m_supportedExtensions.Length == 0)
      {
        return "Nullsoft Winamp media player - http://www.winamp.com";
      }
      return base.Description();
    }

    public override string AuthorName
    {
      get { return m_author; }
    }

    public override string PlayerName
    {
      get { return m_player; }
    }

    public override string VersionNumber
    {
      get { return m_version; }
    }

    public override string[] GetAllSupportedExtensions()
    {
      readConfig();
      return m_supportedExtensions;
    }

    public override bool SupportsFile(string filename)
    {
      readConfig();
      string ext = null;
      int dot = filename.LastIndexOf("."); // couldn't find the dot to get the extension
      if (dot == -1)
      {
        return false;
      }

      ext = filename.Substring(dot).Trim();
      if (ext.Length == 0)
      {
        return false; // no extension so return false;
      }

      ext = ext.ToLower();

      for (int i = 0; i < m_supportedExtensions.Length; i++)
      {
        if (m_supportedExtensions[i].Equals(ext))
        {
          return true;
        }
      }

      // could not match the extension, so return false;
      return false;
    }

    private void readConfig()
    {
      string strExt = null;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strExt = xmlreader.GetValueAsString("winampplugin", "enabledextensions", "");
      }
      if (strExt != null && strExt.Length > 0)
      {
        m_supportedExtensions = strExt.Split(new char[] {':', ','});
        for (int i = 0; i < m_supportedExtensions.Length; i++)
        {
          m_supportedExtensions[i] = m_supportedExtensions[i].Trim();
        }
      }
    }

    public override bool Play(string strFile)
    {
      _isCDA = false;
      if (strFile.IndexOf(".cda") >= 0)
      {
        string strTrack = "";
        int pos = strFile.IndexOf(".cda");
        if (pos >= 0)
        {
          pos--;
          while (Char.IsDigit(strFile[pos]) && pos > 0)
          {
            strTrack = strFile[pos] + strTrack;
            pos--;
          }
        }

        string strDrive = strFile.Substring(0, 1);
        strDrive += ":";
        strFile = String.Format("{0}Track{1}.cda", strDrive, strTrack);
        _isCDA = true;
      }

      m_winampController = new WinampController();
      m_bStoppedManualy = false;
      if (m_winampController != null)
      {
        // stop other media which might be active until now.
        if (g_Player.Playing)
        {
          g_Player.Stop();
        }

        m_strCurrentFile = strFile;
        m_winampController.ClearPlayList();
        m_winampController.Volume = m_volume;
        m_winampController.AppendToPlayList(strFile);
        m_winampController.Play();
        _notifyPlaying = true;
        return true;
      }

      _notifyPlaying = false;
      return false;
    }

    public override double Duration
    {
      get
      {
        if (m_winampController != null)
        {
          return m_winampController.GetCurrentSongDuration();
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (m_winampController != null)
        {
          return m_winampController.Position;
        }
        return 0.0d;
      }
    }

    public override void Pause()
    {
      if (m_winampController != null)
      {
        m_winampController.Pause();
      }
    }

    public override bool Paused
    {
      get
      {
        if (m_winampController != null)
        {
          return (m_winampController.Status() == WinampController.PAUSED);
        }
        return false;
      }
    }

    public override bool Playing
    {
      get
      {
        if (m_winampController != null)
        {
          return (m_winampController.Status() == WinampController.PLAYING)
                 || (m_winampController.Status() == WinampController.PAUSED);
        }
        return false;
      }
    }

    public override bool Ended
    {
      get
      {
        if (m_bStoppedManualy)
        {
          return false;
        }
        if (m_winampController != null)
        {
          return (m_winampController.Status() == WinampController.STOPPED);
        }
        return false;
      }
    }

    public override bool Stopped
    {
      get
      {
        if (m_bStoppedManualy)
        {
          if (m_winampController != null)
          {
            return (m_winampController.Status() == WinampController.STOPPED);
          }
        }
        return false;
      }
    }

    public override string CurrentFile
    {
      get { return m_strCurrentFile; }
    }

    public override void Stop()
    {
      if (m_winampController != null)
      {
        m_bStoppedManualy = true;
        m_winampController.Stop();
        _notifyPlaying = false;
      }
    }

    public override int Volume
    {
      get { return m_volume; }
      set
      {
        if (m_volume != value)
        {
          m_winampController.Volume = value;
          m_volume = value;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      double dCurTime = CurrentPosition;
      dTime = dCurTime + dTime;
      if (dTime < 0.0d)
      {
        dTime = 0.0d;
      }
      if (dTime < Duration)
      {
        m_winampController.Position = dTime;
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (dTime < 0.0d)
      {
        dTime = 0.0d;
      }
      if (dTime < Duration)
      {
        m_winampController.Position = dTime;
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      double dCurrentPos = CurrentPosition;
      double dDuration = Duration;

      double fCurPercent = (dCurrentPos/Duration)*100.0d;
      double fOnePercent = Duration/100.0d;
      fCurPercent = fCurPercent + (double) iPercentage;
      fCurPercent *= fOnePercent;
      if (fCurPercent < 0.0d)
      {
        fCurPercent = 0.0d;
      }
      if (fCurPercent < Duration)
      {
        m_winampController.Position = fCurPercent;
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (iPercentage < 0)
      {
        iPercentage = 0;
      }
      if (iPercentage >= 100)
      {
        iPercentage = 100;
      }
      double fPercent = Duration/100.0f;
      fPercent *= (double) iPercentage;
      m_winampController.Position = fPercent;
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }

      if (_notifyPlaying && CurrentPosition >= 10.0)
      {
        _notifyPlaying = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
        msg.Label = CurrentFile;
        GUIWindowManager.SendThreadMessage(msg);
      }
    }

    public override bool IsCDA
    {
      get { return _isCDA; }
    }
  }
}