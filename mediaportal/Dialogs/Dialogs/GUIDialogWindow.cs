#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  public abstract class GUIDialogWindow : GUIInternalWindow, IRenderLayer
  {
    #region Variables

    // Private Variables
    private Object thisLock = new Object(); // used in PageDestroy
    // Protected Variables
    protected int _selectedLabel = -1;
    protected GUIWindow _parentWindow = null;
    protected int _parentWindowID = -1;
    protected bool _prevOverlay = false;
    protected bool _running = false;
    protected IRenderLayer _prevLayer = null;
    // Public Variables

    #endregion

    #region Properties

    // Public Properties
    public virtual int SelectedLabel
    {
      get { return _selectedLabel; }
      set { _selectedLabel = value; }
    }

    #endregion

    #region Public Methods

    public virtual void PageLoad(int ParentID)
    {
      if (!MediaPortal.Player.g_Player.Playing)
      {
        MediaPortal.Player.VolumeHandler.Dispose();
      }
      if (GUIWindowManager.IsRouted)
      {
        GUIDialogWindow win = (GUIDialogWindow)GUIWindowManager.GetWindow(GUIWindowManager.RoutedWindow);
        if (win != null)
        {
          win.PageDestroy();
        }
      }

      _parentWindowID = ParentID;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowID);
      if (_parentWindow == null)
      {
        _parentWindowID = 0;
        return;
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (thisLock)
      {
        GUIWindowManager.RouteToWindow(GetID);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, _parentWindowID, 0,
                                        null);
        OnMessage(msg);
        _running = true;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    public virtual void PageDestroy()
    {
      if (_running == false)
      {
        return;
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (thisLock)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, _parentWindowID, 0,
                                        null);
        OnMessage(msg);
        if (GUIWindowManager.RoutedWindow == GetID)
        {
          GUIWindowManager.UnRoute(); // only unroute if we still the routed window
        }
        _parentWindow = null;
        _running = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      while (IsAnimating(AnimationType.WindowClose) &&
             GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }

    public virtual void Reset()
    {
      if (_running)
      {
        PageDestroy();
      }
      LoadSkin();
      AllocResources();
      InitControls();
      _selectedLabel = -1;
    }

    public virtual void DoModal(int ParentID)
    {
      if (!_running)
      {
        PageLoad(ParentID);
      }

      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        if (ProcessDoModal() == false)
        {
          break;
        }
      }

      if (_running)
      {
        PageDestroy();
      }
    }

    public virtual bool ProcessDoModal()
    {
      GUIWindowManager.Process();
      return true;
    }

    #endregion

    #region Protected Methods

    protected void SetControlLabel(int WindowID, int ControlID, string LabelText)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, WindowID, 0, ControlID, 0, 0, null);
      msg.Label = LabelText;
      OnMessage(msg);
    }

    protected void HideControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    protected void ShowControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    protected void DisableControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    protected void EnableControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    #endregion

    #region <Base class> Overloads

    #region SupportsDelayedLoad

    public override bool SupportsDelayedLoad
    {
      get { return true; }
    }

    #endregion

    #region PreInit

    public override void PreInit() {}

    #endregion

    #region OnAction

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||
          action.wID == Action.ActionType.ACTION_PREVIOUS_MENU ||
          action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        PageDestroy();
        return;
      }
      base.OnAction(action);
    }

    #endregion

    #region OnMessage

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            _prevLayer = GUILayerManager.GetLayer(GUILayerManager.LayerType.Dialog);
            _prevOverlay = GUIGraphicsContext.Overlay;
            GUIGraphicsContext.Overlay = base.IsOverlayAllowed;
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);

            GUIPropertyManager.SetProperty("#currentmoduleid", Convert.ToString(GUIWindowManager.ActiveWindow));
            GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());
            Log.Debug("DialogWindow: {0} init", this.ToString());

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, _defaultControlId, 0, 0,
                                            null);
            OnMessage(msg);

            OnPageLoad();

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            //base.OnMessage(message);
            _running = false;
            _parentWindowID = 0;
            _parentWindow = null;
            GUIGraphicsContext.Overlay = _prevOverlay;
            Dispose();
            DeInitControls();
            GUILayerManager.UnRegisterLayer(this);
            GUILayerManager.RegisterLayer(_prevLayer, GUILayerManager.LayerType.Dialog);
            return true;
          }
      }
      return base.OnMessage(message);
    }

    #endregion

    #endregion

    #region <Interface> Implementations

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      //return true;
      return _running;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion

    #endregion
  }
}