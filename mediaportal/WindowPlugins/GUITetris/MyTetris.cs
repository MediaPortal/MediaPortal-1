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

using System;
using System.Xml.Serialization;
using System.Drawing;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

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
      GetID = (int)Window.WINDOW_TETRIS;
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
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
      GUIControlFactory.RegisterControl("tetris", typeof(MyTetrisControl));

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

    bool OnInit()
    {
      _Settings.Load();

      if (_Settings.Music)
      {
        GUIControl.SelectControl(GetID, (int)Controls.ToggleMusic);
      }
      else
      {
        GUIControl.DeSelectControl(GetID, (int)Controls.ToggleMusic);
      }

      if (_Settings.Sound)
      {
        GUIControl.SelectControl(GetID, (int)Controls.ToggleSound);
      }
      else
      {
        GUIControl.DeSelectControl(GetID, (int)Controls.ToggleSound);
      }

      m_wndTetris = GetControl((int)Controls.Tetris) as MyTetrisControl;

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
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get((int)Strings.MyTetris));

      return true;
    }

    void OnDeinit()
    {
    }

    void OnItemClicked(int nControl, bool bOn)
    {
      if (m_wndTetris == null)
      {
        return;
      }

      switch ((Controls)nControl)
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
      return "My Tetris";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add GUITetris.GetHome implementation
      strButtonText = GUILocalizeStrings.Get((int)Strings.MyTetris);
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

    #region Helper enums

    enum Controls
    {
      New = 2,
      ToggleMusic = 3,
      ToggleSound = 4,
      Tetris = 10,
      Score = 202,
      Lines = 204,
      Level = 206,
    };

    enum Strings
    {
      MyTetris = 19001,
    };

    #endregion Helper enums

    #region Member variables

    MyTetrisControl m_wndTetris;
    Settings _Settings = new Settings();

    #endregion Member variables

  }

  /// <summary>
  /// Written by Smirnuff (smirnuff@gmail.com)
  /// </summary>
  public class MyTetrisControl : GUIControl, IHostTetris
  {
    #region Construction

    public MyTetrisControl(int nParentID)
      : base(nParentID)
    {
    }

    #endregion Construction

    #region Properties

    public int Score { get { return (m_theGame != null) ? m_theGame.Score : 0; } }
    public int Lines { get { return (m_theGame != null) ? m_theGame.Lines : 0; } }
    public int Level { get { return (m_theGame != null) ? m_theGame.Level : 0; } }
    public bool Music { get { return m_bMusic; } set { m_bMusic = value; } }
    public bool Sound { get { return m_bSounds; } set { m_bSounds = value; } }
    public State State { get { return (m_theGame != null) ? m_theGame.State : State.Stopped; } set { if (m_theGame != null) m_theGame.State = value; } }

    #endregion Properties

    #region IHostTetris implementation

    public void OnRenderBlock(float timePassed, float x, float y, Color color, int nHint)
    {
      if (nHint != 2)
      {
        int nX = m_nBoardX + (int)(x * m_cxBlock);
        int nY = m_nBoardY + (int)(y * m_cyBlock);

        int nImage = ColorToBlock(color);

        if (nHint == 3)
        {
          if (m_imgBlocksGlow != null && m_imgBlocksGlow[nImage] != null)
          {
            m_imgBlocksGlow[nImage].SetPosition(nX, nY);
            m_imgBlocksGlow[nImage].Render(timePassed);
          }
        }
        else
        {
          if (m_imgBlocks != null && m_imgBlocks[nImage] != null)
          {
            m_imgBlocks[nImage].SetPosition(nX, nY);
            m_imgBlocks[nImage].Render(timePassed);
          }
        }
      }
    }

    public void OnRenderSound(string strFilePath)
    {
      if (m_bSounds)
      {
        MediaPortal.Util.Utils.PlaySound(strFilePath, false, true);
      }
    }

    #endregion IHostTetris implementation

    #region Implementation

    void RenderTexture(float timePassed)
    {
      if (IsFocused)
      {
        m_imgTextureFocused.Render(timePassed);
      }
      else
      {
        m_imgTexture.Render(timePassed);
      }

      // render the guides also
      if (m_imgGuide != null)
      {
        if (m_imgGuide[0] != null)
        {
          m_imgGuide[0].Render(timePassed);
        }

        if (m_imgGuide[1] != null)
        {
          m_imgGuide[1].Render(timePassed);
        }
      }
    }

    void RenderText()
    {
      if (m_Font == null)
      {
        return;
      }

      if (m_theGame != null)
      {
        // draw 'Paused' or 'Game Over' if needed
        if (m_theGame.State == State.Paused)
        {
          if (m_cxPaused == 0 || m_cyPaused == 0)
          {
            float fW = m_cxPaused;
            float fH = m_cyPaused;

            m_Font.GetTextExtent(m_strPaused, ref fW, ref fH);

            m_cxPaused = (int)fW;
            m_cyPaused = (int)fH;
          }

          int x = _positionX + ((_width - m_cxPaused) / 2);
          int y = _positionY + ((_height - m_cyPaused) / 2);

          m_Font.DrawText(x, y - m_cyPaused, m_dwTextColor, m_strPaused, Alignment.ALIGN_LEFT, -1);
        }
        else if (m_theGame.State == State.Stopped)
        {
          if (m_cxGameOver == 0 || m_cyGameOver == 0)
          {
            float fW = m_cxGameOver;
            float fH = m_cyGameOver;

            m_Font.GetTextExtent(m_strGameOver, ref fW, ref fH);

            m_cxGameOver = (int)fW;
            m_cyGameOver = (int)fH;
          }

          int x = _positionX + ((_width - m_cxGameOver) / 2);
          int y = _positionY + ((_height - m_cyGameOver) / 2);

          m_Font.DrawText(x, y - m_cyGameOver, m_dwTextColor, m_strGameOver, Alignment.ALIGN_LEFT, -1);
        }
      }
      else
      {
        if (m_cxPressToStart == 0 || m_cyPressToStart == 0)
        {
          float fW = m_cxPressToStart;
          float fH = m_cyPressToStart;

          m_Font.GetTextExtent(m_strStart, ref fW, ref fH);

          m_cxPressToStart = (int)fW;
          m_cyPressToStart = (int)fH;
        }

        int x = _positionX + ((_width - m_cxPressToStart) / 2);
        int y = _positionY + ((_height - m_cyPressToStart) / 2);

        m_Font.DrawText(x, y - m_cyPressToStart, m_dwTextColor, m_strStart, Alignment.ALIGN_LEFT, -1);
      }
    }

    int ColorToBlock(Color color)
    {
      if (color == Color.Red) return 0;
      if (color == Color.Blue) return 1;
      if (color == Color.Gray) return 2;
      if (color == Color.Yellow) return 3;
      if (color == Color.Cyan) return 4;
      if (color == Color.Orange) return 5;
      if (color == Color.Green) return 6;

      return 0;
    }

    #endregion Implementation

    #region Public methods

    public void Start()
    {
      if (m_theGame == null)
      {
        m_theGame = new Game(this);
      }

      if (m_theGame != null)
      {
        m_theGame.Start();

        GUIControl.FocusControl(_parentControlId, _controlId);
      }
    }

    public void Tick()
    {
      if (m_theGame != null)
      {
        m_theGame.Tick();
      }
    }

    #endregion Public methods

    #region Overrides

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      m_Font = GUIFontManager.GetFont((m_strFont != "" && m_strFont != "-") ? m_strFont : "font18");

      if (m_strTexture != "" && m_strTexture != "-")
      {
        m_imgTexture = new GUIImage(_parentControlId, 9998, _positionX, _positionY, this.Width, this.Height, m_strTexture, m_dwColorDiffuse);
      }

      if (m_strTextureFocused != "" && m_strTextureFocused != "-")
      {
        m_imgTextureFocused = new GUIImage(_parentControlId, 9999, _positionX, _positionY, this.Width, this.Height, m_strTextureFocused, m_dwColorDiffuse);
      }

      m_imgGuide = new GUIImage[2];

      m_nBoardWidth = (m_nBoardWidth == -99999) ? this.Width : m_nBoardWidth;
      m_nBoardHeight = (m_nBoardHeight == -99999) ? this.Height : m_nBoardHeight;

      m_cyBlock = m_nBoardHeight / (Game.Height + 2);
      m_cxBlock = m_cyBlock;
      m_nBoardWidth = m_cxBlock * Game.Width;

      m_nBoardX = (m_nBoardX == -99999) ? _positionX + ((this.Width - (m_cxBlock * Game.Width)) / 2) : m_nBoardX;
      m_nBoardY = (m_nBoardY == -99999) ? _positionY + ((this.Height - (m_cyBlock * Game.Height)) / 2) : m_nBoardY;
      m_nBoardY = m_nBoardY - m_cyBlock;

      m_imgBlocks = new GUIImage[]
			{
				new GUIImage(_parentControlId, 10001, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_red.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10002, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_blue.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10003, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_gray.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10004, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_yellow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10005, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_cyan.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10006, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_orange.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10007, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_green.png", m_dwColorDiffuse),
			};

      m_imgBlocksGlow = new GUIImage[]
			{
				new GUIImage(_parentControlId, 10011, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_red_glow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10012, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_blue_glow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10013, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_gray_glow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10014, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_yellow_glow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10015, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_cyan_glow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10016, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_orange_glow.png", m_dwColorDiffuse),
				new GUIImage(_parentControlId, 10017, _positionX, _positionY, m_cxBlock, m_cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_green_glow.png", m_dwColorDiffuse),
			};

      m_strStart = GUILocalizeStrings.Get(19010);
      m_strPaused = GUILocalizeStrings.Get(19011);
      m_strGameOver = GUILocalizeStrings.Get(19012);

      m_imgBlocksNext = new GUIImage[7];

      if (m_strTextureO != "" && m_strTextureO != "-")
      {
        m_imgBlocksNext[0] = new GUIImage(_parentControlId, 10021, _positionX, _positionY, 0, 0, m_strTextureO, m_dwColorDiffuse);
      }

      if (m_strTextureI != "" && m_strTextureI != "-")
      {
        m_imgBlocksNext[1] = new GUIImage(_parentControlId, 10022, _positionX, _positionY, 0, 0, m_strTextureI, m_dwColorDiffuse);
      }

      if (m_strTextureS != "" && m_strTextureS != "-")
      {
        m_imgBlocksNext[2] = new GUIImage(_parentControlId, 10023, _positionX, _positionY, 0, 0, m_strTextureS, m_dwColorDiffuse);
      }

      if (m_strTextureZ != "" && m_strTextureZ != "-")
      {
        m_imgBlocksNext[3] = new GUIImage(_parentControlId, 10024, _positionX, _positionY, 0, 0, m_strTextureZ, m_dwColorDiffuse);
      }

      if (m_strTextureL != "" && m_strTextureL != "-")
      {
        m_imgBlocksNext[4] = new GUIImage(_parentControlId, 10025, _positionX, _positionY, 0, 0, m_strTextureL, m_dwColorDiffuse);
      }

      if (m_strTextureT != "" && m_strTextureT != "-")
      {
        m_imgBlocksNext[5] = new GUIImage(_parentControlId, 10026, _positionX, _positionY, 0, 0, m_strTextureT, m_dwColorDiffuse);
      }

      if (m_strTextureJ != "" && m_strTextureJ != "-")
      {
        m_imgBlocksNext[6] = new GUIImage(_parentControlId, 10027, _positionX, _positionY, 0, 0, m_strTextureJ, m_dwColorDiffuse);
      }

      if (m_imgBlocksNext != null)
      {
        foreach (GUIImage image in m_imgBlocksNext)
        {
          if (image != null)
          {
            image.ScaleToScreenResolution();
          }
        }
      }

      if (m_strTextureLeft != "" && m_strTextureLeft != "-")
      {
        m_imgGuide[0] = new GUIImage(_parentControlId, 9996, _positionX, _positionY, 0, 0, m_strTextureLeft, m_dwColorDiffuse);
      }

      if (m_strTextureRight != "" && m_strTextureRight != "-")
      {
        m_imgGuide[1] = new GUIImage(_parentControlId, 9997, _positionX, _positionY, 0, 0, m_strTextureRight, m_dwColorDiffuse);
      }
    }

    public override void AllocResources()
    {
      base.AllocResources();

      m_Font = GUIFontManager.GetFont((m_strFont != "" && m_strFont != "-") ? m_strFont : "font18");

      if (m_imgTexture != null)
      {
        m_imgTexture.AllocResources();
      }

      if (m_imgTextureFocused != null)
      {
        m_imgTextureFocused.AllocResources();
      }

      if (m_imgGuide != null)
      {
        if (m_imgGuide[0] != null)
        {
          m_imgGuide[0].AllocResources();
        }

        if (m_imgGuide[1] != null)
        {
          m_imgGuide[1].AllocResources();
        }
      }

      if (m_imgBlocks != null)
      {
        foreach (GUIImage image in m_imgBlocks)
        {
          image.AllocResources();
        }
      }

      if (m_imgGuide[0] != null)
      {
        m_imgGuide[0].Height = m_nBoardHeight - (m_cyBlock * 2);
        m_imgGuide[0].SetPosition(m_nBoardX - (m_imgGuide[0].Width + 2), ((m_nBoardY + m_cyBlock) + (Game.Height * m_cyBlock)) - m_imgGuide[0].Height);
      }

      if (m_imgGuide[1] != null)
      {
        m_imgGuide[1].Height = m_nBoardHeight - (m_cyBlock * 2);
        m_imgGuide[1].SetPosition(m_nBoardX + m_nBoardWidth + 2, ((m_nBoardY + m_cyBlock) + (Game.Height * m_cyBlock)) - m_imgGuide[1].Height);
      }

      if (m_imgBlocksGlow != null)
      {
        foreach (GUIImage image in m_imgBlocksGlow)
        {
          image.AllocResources();
        }
      }

      // we must use local variables for the following transform or we'll lose the original next
      // block positions and experience problems if the user performs multiple fullscreen toggles
      int nNextBlockX = m_nNextBlockX;
      int nNextBlockY = m_nNextBlockY;

      GUIGraphicsContext.ScalePosToScreenResolution(ref nNextBlockX, ref nNextBlockY);

      if (m_imgBlocksNext != null)
      {
        foreach (GUIImage image in m_imgBlocksNext)
        {
          image.AllocResources();

          if (m_nNextBlockAlign == Alignment.ALIGN_LEFT)
          {
            image.SetPosition(nNextBlockX, nNextBlockY - (image.Height / 2));
          }
          else if (m_nNextBlockAlign == Alignment.ALIGN_CENTER)
          {
            image.SetPosition(nNextBlockX - (image.Width / 2), nNextBlockY - (image.Height / 2));
          }
          else if (m_nNextBlockAlign == Alignment.ALIGN_RIGHT)
          {
            image.SetPosition(nNextBlockX - image.Width, nNextBlockY - (image.Height / 2));
          }
        }
      }
    }

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false && IsVisible == false)
        return;

      if (m_theGame != null && m_theGame.State == State.Running && IsFocused == false)
      {
        // force the game to be paused
        m_theGame.State = State.Paused;
      }

      bool bRenderTexture = true;

      if (m_theGame == null || m_theGame != null && m_theGame.State == State.Running)
      {
        // draw the texture first so that it appears behind the blocks
        RenderTexture(timePassed);

        bRenderTexture = false;
      }

      if (m_theGame != null)
      {
        m_theGame.Tick();
        m_theGame.Render(timePassed);
      }

      RenderText();

      if (bRenderTexture)
      {
        // draw the now so that the blocks appear faded
        RenderTexture(timePassed);
      }

      RenderNext(timePassed);
    }

    public void RenderNext(float timePassed)
    {
      int nBlock = m_theGame != null ? (m_theGame.NextBlock - 1) : -1;

      if (nBlock < 0 || nBlock > 6)
      {
        //Log.Info("MyTetrisControl.RenderNext: Block index is out of range");
        return;
      }

      if (m_imgBlocksNext != null && m_imgBlocksNext[nBlock] != null)
      {
        m_imgBlocksNext[nBlock].Render(timePassed);
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED && action.m_key.KeyChar == 0x1B)
      {
        if (m_theGame != null && m_theGame.State == State.Running)
        {
          m_theGame.State = State.Paused;
        }
        else
        {
          base.OnAction(action);
        }
      }
      else if (action.wID == Action.ActionType.ACTION_SELECT_ITEM || (action.wID == Action.ActionType.ACTION_KEY_PRESSED && action.m_key.KeyCode == 0x13))
      {
        if (m_theGame == null)
        {
          Start();
        }
        else if (m_theGame != null)
        {
          switch (m_theGame.State)
          {
            case State.Stopped:
              Start();
              break;
            case State.Paused:
              m_theGame.State = State.Running;
              break;
            case State.Running:
              m_theGame.MoveBlock(Game.Move.Drop);
              break;
          }
        }
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_UP)
      {
        if (m_theGame != null && m_theGame.State == State.Running)
        {
          m_theGame.MoveBlock(Game.Move.Rotate);
        }
        else
        {
          base.OnAction(action);
        }
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_DOWN)
      {
        if (m_theGame != null && m_theGame.State == State.Running)
        {
          m_theGame.MoveBlock(Game.Move.Down);
        }
        else
        {
          base.OnAction(action);
        }
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_LEFT)
      {
        if (m_theGame != null && m_theGame.State == State.Running)
        {
          m_theGame.MoveBlock(Game.Move.Left);
        }
        else
        {
          base.OnAction(action);
        }
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
      {
        if (m_theGame != null && m_theGame.State == State.Running)
        {
          m_theGame.MoveBlock(Game.Move.Right);
        }
        else
        {
          base.OnAction(action);
        }
      }
      else if (m_bWarpEnabled && action.wID >= Action.ActionType.REMOTE_1 && action.wID <= Action.ActionType.REMOTE_9)
      {
        if (m_theGame != null)
        {
          m_theGame.Level = action.wID - Action.ActionType.REMOTE_1;
        }
      }
      else
      {
        base.OnAction(action);
      }
    }

    #endregion Overrides

    #region Skin variables

    [XMLSkinElement("font")]
    protected string m_strFont = "font18";

    [XMLSkinElement("textcolor")]
    protected long m_dwTextColor = 0xFFFFFFFF;

    [XMLSkinElement("colordiffuse")]
    protected long m_dwColorDiffuse = 0xFFFFFFFF;

    [XMLSkinElement("texturefocus")]
    protected string m_strTextureFocused = @"tetris\background_focus.png";

    [XMLSkinElement("texture")]
    protected string m_strTexture = @"tetris\background.png";

    [XMLSkinElement("textureO")]
    protected string m_strTextureO = @"tetris\block_O.png";

    [XMLSkinElement("textureI")]
    protected string m_strTextureI = @"tetris\block_I.png";

    [XMLSkinElement("textureS")]
    protected string m_strTextureS = @"tetris\block_S.png";

    [XMLSkinElement("textureZ")]
    protected string m_strTextureZ = @"tetris\block_Z.png";

    [XMLSkinElement("textureL")]
    protected string m_strTextureL = @"tetris\block_L.png";

    [XMLSkinElement("textureT")]
    protected string m_strTextureT = @"tetris\block_T.png";

    [XMLSkinElement("textureJ")]
    protected string m_strTextureJ = @"tetris\block_J.png";

    [XMLSkinElement("nextblockx")]
    protected int m_nNextBlockX = 60;

    [XMLSkinElement("nextblocky")]
    protected int m_nNextBlockY = 60;

    [XMLSkinElement("nextblockalign")]
    protected Alignment m_nNextBlockAlign = Alignment.ALIGN_CENTER;

    [XMLSkinElement("textureLeft")]
    protected string m_strTextureLeft = @"tetris\guide.png";

    [XMLSkinElement("textureRight")]
    protected string m_strTextureRight = @"tetris\guide.png";

    [XMLSkinElement("boardx")]
    protected int m_nBoardX = -99999;

    [XMLSkinElement("boardy")]
    protected int m_nBoardY = -99999;

    [XMLSkinElement("boardwidth")]
    protected int m_nBoardWidth = -99999;

    [XMLSkinElement("boardheight")]
    protected int m_nBoardHeight = -99999;

    #endregion

    #region Member variables

    Game m_theGame = null;
    GUIFont m_Font = null;
    GUIImage m_imgTexture = null;
    GUIImage m_imgTextureFocused = null;
    int m_cxPaused = 0;
    int m_cyPaused = 0;
    int m_cxGameOver = 0;
    int m_cyGameOver = 0;
    int m_cxPressToStart = 0;
    int m_cyPressToStart = 0;
    string m_strStart;
    string m_strPaused;
    string m_strGameOver;
    bool m_bWarpEnabled = false;
    bool m_bSounds = true;
    bool m_bMusic = false;
    GUIImage[] m_imgBlocks;
    GUIImage[] m_imgBlocksGlow;
    GUIImage[] m_imgGuide;
    GUIImage[] m_imgBlocksNext;
    int m_cxBlock;
    int m_cyBlock;

    #endregion Member variables
  }
}
