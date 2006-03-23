/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;


namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogNotify : GUIWindow, IRenderLayer
  {

    #region Base Dialog Variables
    bool m_bRunning = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;
    #endregion
    [SkinControlAttribute(4)]
    protected GUIButtonControl btnClose = null;
    [SkinControlAttribute(3)]
    protected GUILabelControl lblHeading = null;
    [SkinControlAttribute(5)]
    protected GUIImage imgLogo = null;
    [SkinControlAttribute(6)]
    protected GUITextControl txtArea = null;

    bool m_bPrevOverlay = false;
    int timeOutInSeconds = 8;
    DateTime vmr7UpdateTimer = DateTime.Now;
    bool m_bNeedRefresh = false;
      string logoUrl = string.Empty;


    public GUIDialogNotify()
    {
      GetID = (int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogNotify.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return true; }
    }
    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
    {
      //needRefresh = true;
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }

      base.OnAction(action);
    }

    #region Base Dialog Members


    void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
        m_bRunning = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    public void DoModal(int dwParentId)
    {

      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      m_bRunning = true;
      DateTime timeStart = DateTime.Now;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();

        TimeSpan timeElapsed = DateTime.Now - timeStart;
        if (TimeOut > 0)
        {
          if (timeElapsed.TotalSeconds >= TimeOut)
          {
            Close();
            return;
          }
        }
      }
    }
    #endregion

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnClose)
      {
        Close();
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      //needRefresh = true;
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            m_pParentWindow = null;
            m_bRunning = false;
            GUIGraphicsContext.Overlay = m_bPrevOverlay;
            FreeResources();
            DeInitControls();
            GUILayerManager.UnRegisterLayer(this);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            m_bPrevOverlay = GUIGraphicsContext.Overlay;
            base.OnMessage(message);
            GUIGraphicsContext.Overlay = base.IsOverlayAllowed;
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
            if (imgLogo != null)
            {
                SetImage(logoUrl);
            }

          }
          return true;

      }

      return base.OnMessage(message);
    }

    public void Reset()
    {
      LoadSkin();
      AllocResources();
      InitControls();
      timeOutInSeconds = 5;
      logoUrl = string.Empty;
    }
    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
    }


    public void SetHeading(int iString)
    {

      SetHeading(GUILocalizeStrings.Get(iString));
    }

    public void SetText(string text)
    {
      txtArea.Label = text;
    }
    public void SetImage(string filename)
    {
        logoUrl = filename;
        if (System.IO.File.Exists(filename))
        {
            if (imgLogo != null)
            {
                imgLogo.SetFileName(filename);
                m_bNeedRefresh = true;
                imgLogo.IsVisible = true;
            }
        }
        else
        {
            if (imgLogo != null)
            {
                imgLogo.IsVisible = false;
                m_bNeedRefresh = true;
            }
        }
    }
    public void SetImageDimensions(Size size, bool keepAspectRatio,bool centered)
    {
      if (imgLogo == null) return;
      imgLogo.Width = size.Width;
      imgLogo.Height = size.Height;
      imgLogo.KeepAspectRatio = keepAspectRatio;
      imgLogo.Centered = centered;
    } 

    public int TimeOut
    {
      get
      {
        return timeOutInSeconds;
      }
      set
      {
        timeOutInSeconds = value;
      }

    }
      public override bool NeedRefresh()
      {
          if (m_bNeedRefresh)
          {
              m_bNeedRefresh = false;
              return true;
          }
          return false;
      }

    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }
    #endregion
  }
}
