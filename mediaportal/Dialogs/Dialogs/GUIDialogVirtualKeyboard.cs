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
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.Dialogs
{
  public abstract class VirtualKeyboard : GUIWindow, IRenderLayer
  {
    [SkinControl(1)] protected GUIKeyboard _keyboard = null;
    [SkinControl(2)] protected GUIImage _background = null;

    #region Base Dialog Variables

    private int _parentWindowId;
    private GUIWindow _parentWindow;
    public bool _previousOverlayVisible = true;

    #endregion

    protected VirtualKeyboard()
    {
      // Constructor is overridden by concrete class constructor.
    }

    public override bool Init()
    {
      // Register the keyboards event handler for text changes.
      _keyboard.TextChanged += new GUIKeyboard.TextChangedEventHandler(keyboard_TextChanged);
      return true;
    }

    // GUIKeyboard will call this method when the text changes.  This method then invokes the callers (keyboard users) text
    // changed event handler.  This propogates the text changed event from the GUIKeyboard through this class back to the
    // keyboard user.
    private void keyboard_TextChanged(int kindOfSearch, string data)
    {
      if (TextChanged != null)
      {
        TextChanged(kindOfSearch, data);
      }
    }

    public delegate void TextChangedEventHandler(int kindOfSearch, string evtData);

    public event TextChangedEventHandler TextChanged;

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public void Reset()
    {
      // Allocate keyboard resources during a Reset().  Withou this, the caller may override skin properties following the Reset()
      // and the subsequent call to DoModal() will AllocResources() and clobber the callers overrides (AllocResources() is reentrant).
      AllocResources();
      _keyboard.Reset();
      _keyboard.InitializeInstance();
    }

    public string Text
    {
      get { return _keyboard.Text; }
      set { _keyboard.Text = value; }
    }

    public string Label
    {
      get { return _keyboard.Label; }
      set { _keyboard.Label = value; }
    }

    public void SetLabelAsInitialText(bool value)
    {
      _keyboard.SetLabelAsInitialText(value);
    }

    public int MAX_CHARS
    {
      get { return _keyboard.MAX_CHARS; }
    }

    public void SetMaxLength(int maxLen)
    {
      _keyboard.SetMaxLength(maxLen);
    }

    public bool Password
    {
      set { _keyboard.Password = value; }
    }

    public bool IsConfirmed
    {
      get { return _keyboard.PressedEnter; }
    }

    public bool IsSearchKeyboard
    {
      set { _keyboard.IsSearchKeyboard = value; }
    }

    public bool ShiftTurnedOn
    {
      get { return _keyboard._shiftTurnedOn; }
      set { _keyboard._shiftTurnedOn = value; }
    }

    public void InitializeBackground()
    {
      // Position the background based on the keyboard position, if requested.
      if (_background != null)
      {
        if (_background.XPosition < 0)
        {
          _background.XPosition = (GUIGraphicsContext.Width - _background.Width) / 2;
        }

        if (_background.YPosition < 0)
        {
          _background.YPosition = (GUIGraphicsContext.Height - _background.Height) / 2;
        }
      }
    }

    public void PageLoad()
    {
      AllocResources();
      //_keyboard.InitializeInstance();
      _previousOverlayVisible = GUIGraphicsContext.Overlay;
      _keyboard.PressedEnter = false;
      GUIGraphicsContext.Overlay = false;
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      InitializeBackground();
      _keyboard.ResetLabelAsInitialText();

      if (!_keyboard._useSearchLayout && !_keyboard._password)
      {
        using (MediaPortal.Profile.MPSettings xmlreader = new MediaPortal.Profile.MPSettings())
        {
          _keyboard.SmsStyleText = xmlreader.GetValueAsBool("general", "smsstyleinput", true);
        }
      }

      GUIPropertyManager.SetProperty("#VirtualKeyboard.SMSStyleInput", _keyboard.SmsStyleText.ToString().ToLowerInvariant());

      base.OnPageLoad();

      Log.Debug("Window: {0} init", ToString());
    }

    public void PageDestroy()
    {
      if (!_keyboard._useSearchLayout && !_keyboard._password)
      {
        using (MediaPortal.Profile.MPSettings xmlwriter = new Profile.MPSettings())
        {
          xmlwriter.SetValueAsBool("general", "smsstyleinput", _keyboard.SmsStyleText);
        }
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        base.OnPageDestroy(_parentWindowId);
        GUIGraphicsContext.Overlay = _previousOverlayVisible;
        Dispose();

        GUIWindowManager.UnRoute();
        _parentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      GUILayerManager.UnRegisterLayer(this);

      Log.Debug("Window: {0} deinit", ToString());
    }

    public override void OnAction(Action action)
    {
      // We handle all actions locally, do not allow the window to handle actions.
      _keyboard.OnAction(action);
    }

    public void DoModal(int dwParentId)
    {
      _parentWindowId = dwParentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowId);
      if (null == _parentWindow)
      {
        _parentWindowId = 0;
        return;
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);

      // active this window... (with its own OnPageLoad)
      PageLoad();

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _keyboard.IsVisible = true;
      _keyboard.Position = _keyboard.TextEntered.Length;
      while (_keyboard.IsVisible && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }

      PageDestroy();
    }

    public static bool GetKeyboard(ref string strLine, int windowId)
    {
        VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
        if (null == keyboard)
        {
          return false;
        }
        keyboard.Reset();
        keyboard.Text = strLine;
        keyboard.DoModal(windowId);
        if (keyboard.IsConfirmed)
        {
          strLine = keyboard.Text;
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

  public class StandardKeyboard : VirtualKeyboard, IStandardKeyboard
  {
    public StandardKeyboard() : base()
    {
      if (Load(GUIGraphicsContext.GetThemedSkinFile(@"\stdKeyboard.xml")))
      {
        GetID = (int)Window.WINDOW_VIRTUAL_KEYBOARD;
        _keyboard.InitializeInstance();
      }
    }
  }
}