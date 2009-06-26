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
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsMovieCalibration : GUIWindow
  {
    private enum Controls
    {
      CONTROL_LABEL_ROW1 = 2
      ,
      CONTROL_LABEL_ROW2 = 3
      ,
      CONTROL_TOP_LEFT = 8
      ,
      CONTROL_BOTTOM_RIGHT = 9
      ,
      CONTROL_SUBTITLES = 10
      ,
      CONTROL_PIXEL_RATIO = 11
      ,
      CONTROL_VIDEO = 20
      ,
      CONTROL_OSD = 12
    } ;

    private int m_iCountU = 0;
    private int m_iCountD = 0;
    private int m_iCountL = 0;
    private int m_iCountR = 0;
    private int m_iSpeed = 0;
    private long m_dwLastTime = 0;
    private int m_iControl = 0;
    private float m_fPixelRatioBoxHeight = 0.0f;

    public GUISettingsMovieCalibration()
    {
      GetID = (int) Window.WINDOW_MOVIE_CALIBRATION;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settingsScreenCalibration.xml");
    }


    public override void OnAction(Action action)
    {
      if ((DateTime.Now.Ticks/10000) - m_dwLastTime > 500)
      {
        m_iSpeed = 1;
        m_iCountU = 0;
        m_iCountD = 0;
        m_iCountL = 0;
        m_iCountR = 0;
      }
      m_dwLastTime = (DateTime.Now.Ticks/10000);

      int x, y;
      if (m_iControl == (int) Controls.CONTROL_TOP_LEFT)
      {
        x = GUIGraphicsContext.OverScanLeft;
        y = GUIGraphicsContext.OverScanTop;
      }
      else if (m_iControl == (int) Controls.CONTROL_BOTTOM_RIGHT)
      {
        x = GUIGraphicsContext.OverScanWidth + GUIGraphicsContext.OverScanLeft;
        y = GUIGraphicsContext.OverScanHeight + GUIGraphicsContext.OverScanTop;
      }
      else if (m_iControl == (int) Controls.CONTROL_SUBTITLES)
      {
        x = 0;
        y = GUIGraphicsContext.Subtitles;
      }
      else if (m_iControl == (int) Controls.CONTROL_OSD)
      {
        x = 0;
        y = (GUIGraphicsContext.Height + GUIGraphicsContext.OSDOffset);
      }
      else // (m_iControl == (int)Controls.CONTROL_PIXEL_RATIO)
      {
        y = 256;
        x = (int) (256.0f/GUIGraphicsContext.PixelRatio);
      }

      float fPixelRatio = GUIGraphicsContext.PixelRatio;
      if (m_iSpeed > 10)
      {
        m_iSpeed = 10; // Speed limit for accellerated cursors
      }

      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (m_iCountL == 0)
            {
              m_iSpeed = 1;
            }
            x -= m_iSpeed;
            m_iCountL++;
            if (m_iCountL > 5 && m_iSpeed < 10)
            {
              m_iSpeed += 1;
              m_iCountL = 1;
            }
            m_iCountU = 0;
            m_iCountD = 0;
            m_iCountR = 0;
            if (m_iControl == (int) Controls.CONTROL_PIXEL_RATIO)
            {
              fPixelRatio -= 0.01f;
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (m_iCountR == 0)
            {
              m_iSpeed = 1;
            }
            x += m_iSpeed;
            m_iCountR++;
            if (m_iCountR > 5 && m_iSpeed < 10)
            {
              m_iSpeed += 1;
              m_iCountR = 1;
            }
            m_iCountU = 0;
            m_iCountD = 0;
            m_iCountL = 0;
            if (m_iControl == (int) Controls.CONTROL_PIXEL_RATIO)
            {
              fPixelRatio += 0.01f;
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (m_iCountU == 0)
            {
              m_iSpeed = 1;
            }
            y -= m_iSpeed;
            m_iCountU++;
            if (m_iCountU > 5 && m_iSpeed < 10)
            {
              m_iSpeed += 1;
              m_iCountU = 1;
            }
            m_iCountD = 0;
            m_iCountL = 0;
            m_iCountR = 0;
            if (m_iControl == (int) Controls.CONTROL_PIXEL_RATIO)
            {
              fPixelRatio -= 0.05f;
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            if (m_iCountD == 0)
            {
              m_iSpeed = 1;
            }
            y += m_iSpeed;
            m_iCountD++;
            if (m_iCountD > 5 && m_iSpeed < 10)
            {
              m_iSpeed += 1;
              m_iCountD = 1;
            }
            m_iCountU = 0;
            m_iCountL = 0;
            m_iCountR = 0;
            if (m_iControl == (int) Controls.CONTROL_PIXEL_RATIO)
            {
              fPixelRatio += 0.05f;
            }
          }
          break;

        case Action.ActionType.ACTION_CALIBRATE_SWAP_ARROWS:
          m_iControl++;
          if (m_iControl > (int) Controls.CONTROL_OSD)
          {
            m_iControl = (int) Controls.CONTROL_TOP_LEFT;
          }
          m_iSpeed = 1;
          m_iCountU = 0;
          m_iCountD = 0;
          m_iCountL = 0;
          m_iCountR = 0;
          return;


        case Action.ActionType.ACTION_CALIBRATE_RESET:
          //GUIGraphicsContext.ResetScreenParameters(m_Res[m_iCurRes]);
          GUIGraphicsContext.OverScanLeft = 0;
          GUIGraphicsContext.OverScanTop = 0;
          GUIGraphicsContext.PixelRatio = 1.0f;
          GUIGraphicsContext.OSDOffset = 0;
          GUIGraphicsContext.Subtitles = 530;

          GUIGraphicsContext.OverScanWidth = GUIGraphicsContext.Width;
          GUIGraphicsContext.OverScanHeight = GUIGraphicsContext.Height;
          m_iSpeed = 1;
          m_iCountU = 0;
          m_iCountD = 0;
          m_iCountL = 0;
          m_iCountR = 0;
          GUIWindowManager.ResetAllControls();

          return;


        case Action.ActionType.ACTION_CHANGE_RESOLUTION:
          // choose the next resolution in our list
          //m_iCurRes++;
          //if (m_iCurRes == m_Res.size())
          //  m_iCurRes = 0;
          //Sleep(1000);
          //GUIGraphicsContext.SetGUIResolution(m_Res[m_iCurRes]);
          GUIWindowManager.ResetAllControls();
          return;


        case Action.ActionType.ACTION_ANALOG_MOVE:
          x += (int) (2*action.fAmount1);
          y -= (int) (2*action.fAmount2);
          break;
      }
      // do the movement
      switch (m_iControl)
      {
        case (int) Controls.CONTROL_TOP_LEFT:
          if (x < 0)
          {
            x = 0;
          }
          if (y < 0)
          {
            y = 0;
          }
          if (x > 128)
          {
            x = 128;
          }
          if (y > 128)
          {
            y = 128;
          }
          GUIGraphicsContext.OverScanWidth += GUIGraphicsContext.OverScanLeft - x;
          GUIGraphicsContext.OverScanHeight += GUIGraphicsContext.OverScanTop - y;
          GUIGraphicsContext.OverScanLeft = x;
          GUIGraphicsContext.OverScanTop = y;
          break;
        case (int) Controls.CONTROL_BOTTOM_RIGHT:
          if (x > GUIGraphicsContext.Width)
          {
            x = GUIGraphicsContext.Width;
          }
          if (y > GUIGraphicsContext.Height)
          {
            y = GUIGraphicsContext.Height;
          }
          if (x < GUIGraphicsContext.Width - 128)
          {
            x = GUIGraphicsContext.Width - 128;
          }
          if (y < GUIGraphicsContext.Height - 128)
          {
            y = GUIGraphicsContext.Height - 128;
          }
          GUIGraphicsContext.OverScanWidth = x - GUIGraphicsContext.OverScanLeft;
          GUIGraphicsContext.OverScanHeight = y - GUIGraphicsContext.OverScanTop;
          break;
        case (int) Controls.CONTROL_SUBTITLES:
          if (y > GUIGraphicsContext.Height)
          {
            y = GUIGraphicsContext.Height;
          }
          if (y < GUIGraphicsContext.Height - 128)
          {
            y = GUIGraphicsContext.Height - 128;
          }
          GUIGraphicsContext.Subtitles = y;
          break;
        case (int) Controls.CONTROL_OSD:
          GUIGraphicsContext.OSDOffset = (y - GUIGraphicsContext.Height);
          GUIWindow window = GUIWindowManager.GetWindow((int) Window.WINDOW_OSD);
          window.ResetAllControls();
          break;
        case (int) Controls.CONTROL_PIXEL_RATIO:
          GUIGraphicsContext.PixelRatio = fPixelRatio;
          break;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
            //g_application.m_guiWindowOSD.OnMessage(msg);	// Send an init msg to the OSD
            GUIGraphicsContext.Save();
            GUIGraphicsContext.Calibrating = false;
            // reset our screen resolution to what it was initially
            //GUIGraphicsContext.SetGUIResolution(g_stSettings.m_ScreenResolution);
            // Inform the player so we can update the resolution
            //if (g_application.m_pPlayer)
            //  g_application.m_pPlayer.Update();	
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            m_iControl = (int) Controls.CONTROL_TOP_LEFT;
            m_iSpeed = 1;
            m_iCountU = 0;
            m_iCountD = 0;
            m_iCountL = 0;
            m_iCountR = 0;
            GUIGraphicsContext.Calibrating = true;
            // Inform the player so we can update the resolution

            // disable the UI calibration for our controls...
            GUIImage pControl = (GUIImage) GetControl((int) Controls.CONTROL_BOTTOM_RIGHT);
            if (null != pControl)
            {
              pControl.CalibrationEnabled = false;
              pControl = (GUIImage) GetControl((int) Controls.CONTROL_TOP_LEFT);
              pControl.CalibrationEnabled = false;
              pControl = (GUIImage) GetControl((int) Controls.CONTROL_SUBTITLES);
              pControl.CalibrationEnabled = false;
              pControl = (GUIImage) GetControl((int) Controls.CONTROL_PIXEL_RATIO);
              pControl.CalibrationEnabled = false;
              pControl = (GUIImage) GetControl((int) Controls.CONTROL_OSD);
              pControl.CalibrationEnabled = false;
              m_fPixelRatioBoxHeight = (float) pControl.Height;
            }

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
            GUIWindow window = GUIWindowManager.GetWindow((int) Window.WINDOW_OSD);
            window.OnMessage(msg); // Send an init msg to the OSD
            return true;
          }
      }
      return base.OnMessage(message);
    }

    public override void Render(float timePassed)
    {
      // hide all our controls
      GUIImage pControl = (GUIImage) GetControl((int) Controls.CONTROL_BOTTOM_RIGHT);
      if (null != pControl)
      {
        pControl.IsVisible = false;
      }
      pControl = (GUIImage) GetControl((int) Controls.CONTROL_SUBTITLES);
      if (null != pControl)
      {
        pControl.IsVisible = false;
      }
      pControl = (GUIImage) GetControl((int) Controls.CONTROL_TOP_LEFT);
      if (null != pControl)
      {
        pControl.IsVisible = false;
      }
      pControl = (GUIImage) GetControl((int) Controls.CONTROL_PIXEL_RATIO);
      if (null != pControl)
      {
        pControl.IsVisible = false;
      }
      pControl = (GUIImage) GetControl((int) Controls.CONTROL_OSD);
      if (null != pControl)
      {
        pControl.IsVisible = false;
      }

      int iXOff, iYOff;
      string strStatus = "";
      switch (m_iControl)
      {
        case (int) Controls.CONTROL_TOP_LEFT:
          {
            iXOff = GUIGraphicsContext.OverScanLeft;
            iYOff = GUIGraphicsContext.OverScanTop;
            pControl = (GUIImage) GetControl((int) Controls.CONTROL_TOP_LEFT);
            if (null != pControl)
            {
              pControl.IsVisible = true;
              pControl.Height = pControl.TextureHeight;
              pControl.Width = pControl.TextureWidth;
              pControl.SetPosition(iXOff, iYOff);
            }
            string strMode = GUILocalizeStrings.Get(272);
            strStatus = String.Format("{0} ({1},{2})", strMode, iXOff, iYOff);
            GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABEL_ROW2, GUILocalizeStrings.Get(276));
          }
          break;
        case (int) Controls.CONTROL_BOTTOM_RIGHT:
          {
            iXOff = GUIGraphicsContext.OverScanLeft;
            iYOff = GUIGraphicsContext.OverScanTop;
            iXOff += GUIGraphicsContext.OverScanWidth;
            iYOff += GUIGraphicsContext.OverScanHeight;
            pControl = (GUIImage) GetControl((int) Controls.CONTROL_BOTTOM_RIGHT);
            if (null != pControl)
            {
              pControl.IsVisible = true;
              int iTextureWidth = pControl.TextureWidth;
              int iTextureHeight = pControl.TextureHeight;
              pControl.Height = iTextureHeight;
              pControl.Width = pControl.TextureWidth;
              pControl.SetPosition(iXOff - iTextureWidth, iYOff - iTextureHeight);
              int iXOff1 = GUIGraphicsContext.Width - iXOff;
              int iYOff1 = GUIGraphicsContext.Height - iYOff;
              string strMode = GUILocalizeStrings.Get(273);
              strStatus = String.Format("{0} ({1},{2})", strMode, iXOff1, iYOff1);
              GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABEL_ROW2, GUILocalizeStrings.Get(276));
            }
          }
          break;
        case (int) Controls.CONTROL_SUBTITLES:
          {
            iXOff = GUIGraphicsContext.OverScanLeft;
            iYOff = GUIGraphicsContext.Subtitles;

            int iScreenWidth = GUIGraphicsContext.OverScanWidth;

            pControl = (GUIImage) GetControl((int) Controls.CONTROL_SUBTITLES);
            if (null != pControl)
            {
              pControl.IsVisible = true;
              int iTextureWidth = pControl.TextureWidth;
              int iTextureHeight = pControl.TextureHeight;

              pControl.Height = pControl.TextureHeight;
              pControl.Width = pControl.TextureWidth;
              pControl.SetPosition(iXOff + (iScreenWidth - iTextureWidth)/2, iYOff - iTextureHeight);
              string strMode = GUILocalizeStrings.Get(274);
              strStatus = String.Format("{0} ({1})", strMode, iYOff);
              GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABEL_ROW2, GUILocalizeStrings.Get(277));
            }
          }
          break;
        case (int) Controls.CONTROL_PIXEL_RATIO:
          {
            float fSqrtRatio = (float) Math.Sqrt(GUIGraphicsContext.PixelRatio);
            pControl = (GUIImage) GetControl((int) Controls.CONTROL_PIXEL_RATIO);
            if (null != pControl)
            {
              pControl.IsVisible = true;
              int iControlHeight = (int) (m_fPixelRatioBoxHeight*fSqrtRatio);
              int iControlWidth = (int) (m_fPixelRatioBoxHeight/fSqrtRatio);
              pControl.Width = iControlWidth;
              pControl.Height = iControlHeight;
              iXOff = GUIGraphicsContext.OverScanLeft;
              iYOff = GUIGraphicsContext.OverScanTop;
              int iScreenWidth = GUIGraphicsContext.OverScanWidth;
              int iScreenHeight = GUIGraphicsContext.OverScanHeight;
              pControl.SetPosition(iXOff + (iScreenWidth - iControlWidth)/2, iYOff + (iScreenHeight - iControlHeight)/2);
              string strMode = GUILocalizeStrings.Get(275);
              strStatus = String.Format("{0} ({1:#.##})", strMode, GUIGraphicsContext.PixelRatio);
              GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABEL_ROW2, GUILocalizeStrings.Get(278));
            }
          }
          break;

        case (int) Controls.CONTROL_OSD:
          {
            iXOff = GUIGraphicsContext.OverScanLeft;
            iYOff = GUIGraphicsContext.Subtitles;
            iYOff = (GUIGraphicsContext.Height + GUIGraphicsContext.OSDOffset);

            int iScreenWidth = GUIGraphicsContext.OverScanWidth;

            pControl = (GUIImage) GetControl((int) Controls.CONTROL_OSD);
            if (null != pControl)
            {
              //pControl.IsVisible=true;
              int iTextureWidth = pControl.TextureWidth;
              int iTextureHeight = pControl.TextureHeight;

              pControl.SetPosition(iXOff + (iScreenWidth - iTextureWidth)/2, iYOff - iTextureHeight);
              string strMode = GUILocalizeStrings.Get(469);
              strStatus = String.Format("{0} ({1}, Offset={2})", strMode, iYOff, GUIGraphicsContext.OSDOffset);
              GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABEL_ROW2, GUILocalizeStrings.Get(468));
            }
          }
          break;
      }

      string strText;

      strText = String.Format("{0}x{1} | {2}", GUIGraphicsContext.Width, GUIGraphicsContext.Height, strStatus);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABEL_ROW1, strText);

      base.Render(timePassed);

      /*
      GUIFont font1=GUIFontManager.GetFont("font13");
      if (font1!=null)
      {
        for (int i=0; i < 780; i+=20)
        {
          System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.White,1);
          GUIGraphicsContext.graphics.DrawLine(pen,0,i,720,i);
          pen.Dispose();
          font1.DrawText(100,i,0xffffffff,i.ToString(),GUICheckMarkControl.Alignment.ALIGN_LEFT);
        }
        System.Drawing.Pen pen2 = new System.Drawing.Pen(System.Drawing.Color.Red,1);
        GUIGraphicsContext.graphics.DrawLine(pen2,0,GUIGraphicsContext.Height-1,720,GUIGraphicsContext.Height-1);
        pen2.Dispose();

      }*/

      if (m_iControl == (int) Controls.CONTROL_OSD)
      {
        GUIWindow window = GUIWindowManager.GetWindow((int) Window.WINDOW_OSD);
        window.Render(timePassed);
      }
    }

    public override int GetFocusControlId()
    {
      return 1;
    }
  }
}