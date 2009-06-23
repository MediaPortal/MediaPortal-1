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
using MediaPortal.Dialogs;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsUICalibration : GUIWindow
  {
    private const int CONTROL_LABEL = 2;
    private const float ZOOM_MAX = 1.25f;
    private const float ZOOM_MIN = 0.75f;
    private int m_iCountU;
    private int m_iCountD;
    private int m_iCountL;
    private int m_iCountR;
    private int m_iSpeed;
    private long m_dwLastTime;
    private int m_iMode; // 0 == Position, 1 == Zoom.
    private bool m_bModeLocked; // Latches on mode change to limit toggle speed.
    private int m_iLogWidth; // Notional (logical) screen width
    private int m_iLogHeight; // Notional (logical) screen height
    private float m_orgZoomVertical;

    // Ensure m_iLogWidth/Height are in range.
    // Returns true if either are clamped.
    private bool ClampLogicalScreenSize()
    {
      bool bClamped;
      int iCurrWidth = m_iLogWidth;
      int iCurrHeight = m_iLogHeight;

      m_iLogWidth = (int)
                    Math.Max((float) GUIGraphicsContext.Width*ZOOM_MIN,
                             Math.Min((float) GUIGraphicsContext.Width*ZOOM_MAX, (float) m_iLogWidth));

      m_iLogHeight = (int)
                     Math.Max((float) GUIGraphicsContext.Height*ZOOM_MIN,
                              Math.Min((float) GUIGraphicsContext.Height*ZOOM_MAX, (float) m_iLogHeight));

      bClamped = (m_iLogWidth != iCurrWidth) || (m_iLogHeight != iCurrHeight);

      return bClamped;
    }

    // Update dialog to reflect current values.
    private void UpdateControlLabel()
    {
      string strOffset;

      int iX1 = GUIGraphicsContext.OffsetX;
      int iY1 = GUIGraphicsContext.OffsetY;
      int iX2 = iX1 + (int) Math.Round(GUIGraphicsContext.ZoomHorizontal
                                       *(float) GUIGraphicsContext.Width);
      int iY2 = iY1 + (int) Math.Round(GUIGraphicsContext.ZoomVertical
                                       *(float) GUIGraphicsContext.Height);

      if (m_iMode == 1)
      {
        // Zoom
        strOffset = String.Format("{0},{1} - [{2},{3}]",
                                  iX1, iY1, iX2, iY2);
      }
      else
      {
        // Offset
        strOffset = String.Format("[{0},{1}] - {2},{3}",
                                  iX1, iY1, iX2, iY2);
      }

      GUIControl.SetControlLabel(GetID, CONTROL_LABEL, strOffset);
    }

    public GUISettingsUICalibration()
    {
      GetID = (int) Window.WINDOW_UI_CALIBRATION;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settingsUICalibration.xml");
    }

    public override int GetFocusControlId()
    {
      return 1;
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
        m_bModeLocked = false;
      }
      m_dwLastTime = (DateTime.Now.Ticks/10000);

      bool bChanged = false;
      int iXOff = GUIGraphicsContext.OffsetX;
      int iYOff = GUIGraphicsContext.OffsetY;

      int iLogWidthMin = (int) ((float) GUIGraphicsContext.Width*ZOOM_MIN);
      int iLogWidthMax = (int) ((float) GUIGraphicsContext.Width*ZOOM_MAX);
      int iLogHeightMin = (int) ((float) GUIGraphicsContext.Height*ZOOM_MIN);
      int iLogHeightMax = (int) ((float) GUIGraphicsContext.Height*ZOOM_MAX);

      // Check if screen res change has invalidated
      // the current sizes.
      if (ClampLogicalScreenSize())
      {
        bChanged = true;
      }

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      if (m_iSpeed > 10)
      {
        m_iSpeed = 10; // Speed limit for accellerated cursors
      }

      switch (action.wID)
      {
        case Action.ActionType.ACTION_SELECT_ITEM:
          {
            // Cycle modes
            // Only 0,1 currently.
            if (!m_bModeLocked)
            {
              m_iMode = 1 - m_iMode;
              m_bModeLocked = true;
              bChanged = true;
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (m_iCountL == 0)
            {
              m_iSpeed = 1;
            }

            if (m_iMode == 1)
            {
              // Zoom
              if (m_iLogWidth > iLogWidthMin)
              {
                m_iLogWidth -= m_iSpeed;
                bChanged = true;
                m_iCountL++;
                if (m_iCountL > 5)
                {
                  m_iSpeed += 1;
                  m_iCountL = 1;
                }
              }
            }
            else
            {
              // Offset
              if (iXOff > -128)
              {
                iXOff -= m_iSpeed;
                bChanged = true;
                m_iCountL++;
                if (m_iCountL > 5)
                {
                  m_iSpeed += 1;
                  m_iCountL = 1;
                }
              }
            }
            m_iCountU = 0;
            m_iCountD = 0;
            m_iCountR = 0;
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (m_iCountR == 0)
            {
              m_iSpeed = 1;
            }

            if (m_iMode == 1)
            {
              // Zoom
              if (m_iLogWidth < iLogWidthMax)
              {
                m_iLogWidth += m_iSpeed;
                bChanged = true;
                m_iCountR++;
                if (m_iCountR > 5)
                {
                  m_iSpeed += 1;
                  m_iCountR = 1;
                }
              }
            }
            else
            {
              // Offset
              if (iXOff < 128)
              {
                iXOff += m_iSpeed;
                bChanged = true;
                m_iCountR++;
                if (m_iCountR > 5)
                {
                  m_iSpeed += 1;
                  m_iCountR = 1;
                }
              }
            }

            m_iCountU = 0;
            m_iCountD = 0;
            m_iCountL = 0;
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (m_iCountU == 0)
            {
              m_iSpeed = 1;
            }

            if (m_iMode == 1)
            {
              // Zoom
              if (m_iLogHeight > iLogHeightMin)
              {
                m_iLogHeight -= m_iSpeed;
                bChanged = true;
                m_iCountU++;
                if (m_iCountU > 5)
                {
                  m_iSpeed += 1;
                  m_iCountU = 1;
                }
              }
            }
            else
            {
              // Offset
              if (iYOff > -128)
              {
                iYOff -= m_iSpeed;
                bChanged = true;
                m_iCountU++;
                if (m_iCountU > 5)
                {
                  m_iSpeed += 1;
                  m_iCountU = 1;
                }
              }
            }
            m_iCountD = 0;
            m_iCountL = 0;
            m_iCountR = 0;
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            if (m_iCountD == 0)
            {
              m_iSpeed = 1;
            }

            if (m_iMode == 1)
            {
              // Zoom
              if (m_iLogHeight < iLogHeightMax)
              {
                m_iLogHeight += m_iSpeed;
                bChanged = true;
                m_iCountD++;
                if (m_iCountD > 5)
                {
                  m_iSpeed += 1;
                  m_iCountD = 1;
                }
              }
            }
            else
            {
              if (iYOff < 128)
              {
                iYOff += m_iSpeed;
                bChanged = true;
                m_iCountD++;
                if (m_iCountD > 5)
                {
                  m_iSpeed += 1;
                  m_iCountD = 1;
                }
              }
            }
            m_iCountU = 0;
            m_iCountL = 0;
            m_iCountR = 0;
          }
          break;

        case Action.ActionType.ACTION_CALIBRATE_RESET:
          //if (m_iMode == 1)
          //{
          m_iLogWidth = GUIGraphicsContext.Width;
          m_iLogHeight = GUIGraphicsContext.Height;
          //}
          //else
          //{
          iXOff = 0;
          iYOff = 0;
          //}
          bChanged = true;
          m_iSpeed = 1;
          m_iCountU = 0;
          m_iCountD = 0;
          m_iCountL = 0;
          m_iCountR = 0;
          break;

        case Action.ActionType.ACTION_ANALOG_MOVE:
          float fX = 2*action.fAmount1;
          float fY = 2*action.fAmount2;
          if (fX != 0.0 || fY != 0.0)
          {
            bChanged = true;
            if (m_iMode == 1)
            {
              m_iLogWidth += (int) fX;
              m_iLogHeight -= (int) fY;
            }
            else
            {
              iXOff += (int) fX;
              if (iXOff < -128)
              {
                iXOff = -128;
              }
              if (iXOff > 128)
              {
                iXOff = 128;
              }

              iYOff -= (int) fY;
              if (iYOff < -128)
              {
                iYOff = -128;
              }
              if (iYOff > 128)
              {
                iYOff = 128;
              }
            }
          }
          break;
      }
      // do the movement
      if (bChanged)
      {
        ClampLogicalScreenSize();
        GUIGraphicsContext.OffsetX = iXOff;
        GUIGraphicsContext.OffsetY = iYOff;

        float fZoomHorz = (float) m_iLogWidth/(float) GUIGraphicsContext.Width;
        float fZoomVert = (float) m_iLogHeight/(float) GUIGraphicsContext.Height;

        GUIGraphicsContext.ZoomHorizontal = fZoomHorz;
        GUIGraphicsContext.ZoomVertical = fZoomVert;

        GUIGraphicsContext.OffsetX = GUIGraphicsContext.OffsetX;
        GUIGraphicsContext.OffsetY = GUIGraphicsContext.OffsetY;
        GUIGraphicsContext.ZoomHorizontal = GUIGraphicsContext.ZoomHorizontal;
        GUIGraphicsContext.ZoomVertical = GUIGraphicsContext.ZoomVertical;

        GUIWindowManager.OnResize();
        GUIWindowManager.PreInit();
        UpdateControlLabel();

        ResetAllControls();
        GUIWindowManager.ResetAllControls();
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            GUIWindowManager.OnResize();
            GUIWindowManager.PreInit();
            GUIGraphicsContext.Save();
            if (m_orgZoomVertical != GUIGraphicsContext.ZoomVertical) // only vertical zoom affects font sizes
            {
              GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
              if (dlgNotify != null)
              {
                dlgNotify.Reset();
                dlgNotify.ClearAll();
                dlgNotify.SetHeading(213); // UI Calibration
                dlgNotify.SetText(GUILocalizeStrings.Get(2650)); // Reloading fonts, please wait...
                dlgNotify.TimeOut = 1;
                dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
              }
              GUIFontManager.LoadFonts(Config.GetFile(Config.Dir.Skin, GUIGraphicsContext.Skin, "fonts.xml"));
              GUIFontManager.InitializeDeviceObjects();
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            m_iSpeed = 1;
            m_iCountU = 0;
            m_iCountD = 0;
            m_iCountL = 0;
            m_iCountR = 0;
            m_iMode = 0;
            m_bModeLocked = true;
            m_orgZoomVertical = GUIGraphicsContext.ZoomVertical;
            m_iLogWidth = (int) Math.Round((float) GUIGraphicsContext.Width*(float) GUIGraphicsContext.ZoomHorizontal);
            m_iLogHeight = (int) Math.Round((float) GUIGraphicsContext.Height*(float) GUIGraphicsContext.ZoomVertical);
            ClampLogicalScreenSize();
            UpdateControlLabel();
            return true;
          }
      }
      return base.OnMessage(message);
    }
  }
}