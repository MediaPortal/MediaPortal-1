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
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using Tetris;

namespace MediaPortal.Games.Tetris
{
  /// <summary>
  /// Written by Smirnoff (smirnuff@gmail.com)
  /// </summary>
  [PluginIcons("WindowPlugins.GUITetris.Tetris.gif", "WindowPlugins.GUITetris.TetrisDisabled.gif")]
  public class MyTetris : GUIWindow, ISetupForm, IShowPlugin
  {
    #region Construction, initialization & cleanup

    public MyTetris()
    {
      GetID = (int) Window.WINDOW_TETRIS;
    }

    ~MyTetris()
    {
    }

    #endregion Construction, initialization & cleanup

    #region Serialization

    [Serializable]
    public class Settings
    {
      protected bool m_bMusic;
      protected bool m_bSound;
      protected int m_nHighscore;

      public Settings()
      {
        m_bMusic = true;
        m_bSound = true;
        m_nHighscore = 0;
      }

      [XmlElement("Music")]
      public bool Music
      {
        get { return m_bMusic; }
        set { m_bMusic = value; }
      }

      [XmlElement("Sound")]
      public bool Sound
      {
        get { return m_bSound; }
        set { m_bSound = value; }
      }

      [XmlElement("Highscore")]
      public int Highscore
      {
        get { return m_nHighscore; }
        set { m_nHighscore = value; }
      }

      public void Load()
      {
        using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          m_bMusic = xmlreader.GetValueAsBool("tetris", "music", true);
          m_bSound = xmlreader.GetValueAsBool("tetris", "sound", true);

          m_nHighscore = xmlreader.GetValueAsInt("tetris", "highscore", -1);

          if (m_nHighscore == -1)
          {
            m_nHighscore = xmlreader.GetValueAsInt("tetris", "hiscore", 0);
          }
        }
      }

      public void Save()
      {
        using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValueAsBool("tetris", "music", m_bMusic);
          xmlwriter.SetValueAsBool("tetris", "sound", m_bSound);
          xmlwriter.SetValue("tetris", "highscore", m_nHighscore);
        }
      }
    }

    #endregion Serialization

    #region Overrides

    public override bool Init()
    {
      // pre-register the control class so that the factory knows how to create it
      GUIControlFactory.RegisterControl("tetris", typeof (MyTetrisControl));

      return Load(GUIGraphicsContext.Skin + @"\mytetris.xml");
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (m_wndTetris != null && m_wndTetris.Focus && m_wndTetris.State == State.Running)
        {
          m_wndTetris.State = State.Paused;
        }
        else
        {
          GUIWindowManager.ShowPreviousWindow();
        }
      }
      else
      {
        base.OnAction(action);
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
          return OnInit();
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          OnDeinit();
          break;
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          OnItemClicked(message.SenderControlId, message.Param1 == 1);
          break;
      }

      return base.OnMessage(message);
    }

    public override void Render(float timePassed)
    {
      if (m_wndTetris != null)
      {
        GUIPropertyManager.SetProperty("#tetris_score", m_wndTetris.Score.ToString());
        GUIPropertyManager.SetProperty("#tetris_lines", m_wndTetris.Lines.ToString());
        GUIPropertyManager.SetProperty("#tetris_level", m_wndTetris.Level.ToString());

        if (m_wndTetris.Score > _Settings.Highscore)
        {
          _Settings.Highscore = m_wndTetris.Score;
          _Settings.Save();

          GUIPropertyManager.SetProperty("#tetris_highscore", _Settings.Highscore.ToString());
        }
      }

      base.Render(timePassed);
    }

    #endregion

    #region Implementation

    private bool OnInit()
    {
      _Settings.Load();

      if (_Settings.Music)
      {
        GUIControl.SelectControl(GetID, (int) Controls.ToggleMusic);
      }
      else
      {
        GUIControl.DeSelectControl(GetID, (int) Controls.ToggleMusic);
      }

      if (_Settings.Sound)
      {
        GUIControl.SelectControl(GetID, (int) Controls.ToggleSound);
      }
      else
      {
        GUIControl.DeSelectControl(GetID, (int) Controls.ToggleSound);
      }

      m_wndTetris = GetControl((int) Controls.Tetris) as MyTetrisControl;

      int nScore = 0;
      int nLines = 0;
      int nLevel = 0;

      if (m_wndTetris != null)
      {
        nScore = m_wndTetris.Score;
        nLines = m_wndTetris.Lines;
        nLevel = m_wndTetris.Level;

        m_wndTetris.Sound = _Settings.Sound;
        m_wndTetris.Music = _Settings.Music;

        // we don't get told when the green (return to home window) button is hit so
        // we make it look like we correctly paused the game when it was hit here
        if (m_wndTetris.State == State.Running)
        {
          m_wndTetris.State = State.Paused;
        }
      }

      GUIPropertyManager.SetProperty("#tetris_score", nScore.ToString());
      GUIPropertyManager.SetProperty("#tetris_lines", nLines.ToString());
      GUIPropertyManager.SetProperty("#tetris_level", nLevel.ToString());
      GUIPropertyManager.SetProperty("#tetris_highscore", _Settings.Highscore.ToString());
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get((int) Strings.MyTetris));

      return true;
    }

    private void OnDeinit()
    {
    }

    private void OnItemClicked(int nControl, bool bOn)
    {
      if (m_wndTetris == null)
      {
        return;
      }

      switch ((Controls) nControl)
      {
        case Controls.New:
          m_wndTetris.Start();
          break;
        case Controls.ToggleMusic:
          _Settings.Music = bOn;
          _Settings.Save();
          break;
        case Controls.ToggleSound:
          _Settings.Sound = bOn;
          _Settings.Save();
          break;
      }

      m_wndTetris.Music = _Settings.Music;
      m_wndTetris.Sound = _Settings.Sound;
    }

    #endregion Implementation

    #region Helper enums

    private enum Controls
    {
      New = 2,
      ToggleMusic = 3,
      ToggleSound = 4,
      Tetris = 10,
      Score = 202,
      Lines = 204,
      Level = 206,
    } ;

    private enum Strings
    {
      MyTetris = 19001,
    } ;

    #endregion Helper enums

    #region Member variables

    private MyTetrisControl m_wndTetris;
    private Settings _Settings = new Settings();

    #endregion Member variables

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public string PluginName()
    {
      return "Tetris";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      // TODO:  Add GUITetris.GetHome implementation
      strButtonText = GUILocalizeStrings.Get((int) Strings.MyTetris);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = @"hover_tetris.png";
      return true;
    }

    public string Author()
    {
      return "Smirnuff";
    }

    public string Description()
    {
      return "Play the famous Tetris game in MediaPortal";
    }

    public void ShowPlugin()
    {
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion
  }
}