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

#region usings

using System;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

#endregion

namespace TvPlugin
{
  /// <summary>
  /// TvCropSettings
  /// </summary>
  public class TvCropSettings : GUIWindow, IRenderLayer
  {
    [SkinControl(2)] protected GUIButtonControl btnClose = null;
    [SkinControl(8)] protected GUISpinControl spinTop = null;
    [SkinControl(12)] protected GUISpinControl spinBottom = null;
    [SkinControl(16)] protected GUISpinControl spinLeft = null;
    [SkinControl(20)] protected GUISpinControl spinRight = null;

    private bool _running;
    private int _parentWindowID = 0;
    private CropSettings _cropSettings;
    private GUIWindow _parentWindow = null;

    /// <summary>
    /// Collection of Controls which are accessed from this class
    /// </summary>
    private enum Controls
    {
      CONTROL_EXIT = 2,
      CONTROL_CARD_LABEL = 5,
      CONTROL_CROP_TOP = 8,
      CONTROL_CROP_BOTTOM = 12,
      CONTROL_CROP_LEFT = 16,
      CONTROL_CROP_RIGHT = 20,
    }

    #region Ctor

    /// <summary>
    /// Constructor
    /// </summary>
    public TvCropSettings()
    {
      GetID = (int) Window.WINDOW_TV_CROP_SETTINGS;
    }

    #endregion

    /// <summary>
    /// Init method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\TVCropSettings.xml");
      GetID = (int) Window.WINDOW_TV_CROP_SETTINGS;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      return bResult;
    }

    /// <summary>
    /// Delayed load
    /// </summary>
    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    /// <summary>
    /// Close
    /// </summary>
    private void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        _running = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }


    /// <summary>
    /// On Message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            // fetch settings for the current capture card
            _cropSettings = TvCropManager.CropSettings;
            GUILabelControl cardLabel = GetControl((int) Controls.CONTROL_CARD_LABEL) as GUILabelControl;
            cardLabel.Visible = false;

            foreach (int iCtl in Enum.GetValues(typeof (Controls)))
            {
              if (GetControl(iCtl) is GUISpinControl)
              {
                GUISpinControl cntl = (GUISpinControl) GetControl(iCtl);
                cntl.ShowRange = false;
              }
            }
            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_CROP_TOP);
            for (int i = 0; i <= 200; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_CROP_TOP, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_CROP_TOP, _cropSettings.Top);
            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_CROP_BOTTOM);
            for (int i = 0; i <= 200; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_CROP_BOTTOM, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_CROP_BOTTOM, _cropSettings.Bottom);
            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_CROP_LEFT);
            for (int i = 0; i <= 200; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_CROP_LEFT, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_CROP_LEFT, _cropSettings.Left);
            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_CROP_RIGHT);
            for (int i = 0; i <= 200; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_CROP_RIGHT, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_CROP_RIGHT, _cropSettings.Right);

            break;
          }
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int) Controls.CONTROL_EXIT)
            {
              Close();
            }
            else if (iControl == (int) Controls.CONTROL_CROP_TOP)
            {
              _cropSettings.Top = Int32.Parse(message.Label);
            }
            else if (iControl == (int) Controls.CONTROL_CROP_BOTTOM)
            {
              _cropSettings.Bottom = Int32.Parse(message.Label);
            }
            else if (iControl == (int) Controls.CONTROL_CROP_LEFT)
            {
              _cropSettings.Left = Int32.Parse(message.Label);
            }
            else if (iControl == (int) Controls.CONTROL_CROP_RIGHT)
            {
              _cropSettings.Right = Int32.Parse(message.Label);
            }

            // ativate & save settings for the current capture card
            TvCropManager.CropSettings = _cropSettings;

            break;
          }
      }
      return base.OnMessage(message);
    }

    /// <summary>
    /// On action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_CONTEXT_MENU:
          Close();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          Close();
          return;
      }
      base.OnAction(action);
    }

    /// <summary>
    /// Page gets destroyed
    /// </summary>
    /// <param name="new_windowId"></param>
    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      _running = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      AllocResources();
      ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
      base.OnPageLoad();
    }

    /// <summary>
    /// Do this modal
    /// </summary>
    /// <param name="dwParentId"></param>
    public void DoModal(int parentId)
    {
      _parentWindowID = parentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowID);
      if (null == _parentWindow)
      {
        _parentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _running = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
        if (!GUIGraphicsContext.Vmr9Active)
        {
          Thread.Sleep(50);
        }
      }
      GUILayerManager.UnRegisterLayer(this);
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      if (_running)
      {
        Render(timePassed);
      }
    }

    #endregion
  }
}