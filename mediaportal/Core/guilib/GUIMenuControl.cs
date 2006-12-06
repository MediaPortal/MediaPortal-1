#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

#region Usings
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms; // used for Keys definition
using System.Windows.Media.Animation;
using MediaPortal.Services;
using MediaPortal.Utils.Services;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
#endregion

namespace MediaPortal.GUI.Library
{
  public class GUIMenuControl : GUIControl
  {
    #region Properties (Skin)    
    [XMLSkinElement("spaceBetweenButtons")]       protected int     _spaceBetweenButtons  = 8;
    [XMLSkinElement("textcolor")]                 protected long    _textColor            = 0xFFFFFFFF;
    [XMLSkinElement("textColorNoFocus")]          protected long    _textColorNoFocus     = 0xFFFFFFFF;
    //[XMLSkinElement("textAlign")]                Alignment        _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("buttonWidth")]               protected int     _buttonWidth          = 60;
    [XMLSkinElement("buttonHeight")]              protected int     _buttonHeight         = 30;
    [XMLSkinElement("buttonTextXOff")]            protected int     _buttonTextXOffset    = 10;
    [XMLSkinElement("buttonTextYOff")]            protected int     _buttonTextYOffset    = 8;
    [XMLSkinElement("buttonOffset")]              protected int     _buttonOffset         = 25;      // offset from the border to the buttons
    [XMLSkinElement("buttonFont")]                protected string  _buttonFont           = "font16";
    [XMLSkinElement("numberOfButtons")]           protected int     _numberOfButtons      = 5;
    [XMLSkinElement("textureBackground")]         protected string  _textureBackground    = String.Empty;
    [XMLSkinElement("textureButtonFocus")]        protected string  _textureButtonFocus   = String.Empty;
    [XMLSkinElement("textureButtonNoFocus")]      protected string  _textureButtonNoFocus = String.Empty;
    [XMLSkinElement("textureHoverNoFocus")]       protected string  _textureHoverNoFocus  = String.Empty;
    [XMLSkinElement("hoverX")]                    protected int     _hoverPositionX       = 0;
    [XMLSkinElement("hoverY")]                    protected int     _hoverPositionY       = 0;
    [XMLSkinElement("hoverWidth")]                protected int     _hoverWidth           = 0;
    [XMLSkinElement("hoverHeight")]               protected int     _hoverHeight          = 0;
    [XMLSkinElement("hoverKeepAspectratio")]      protected bool    _hoverKeepAspectRatio = true;
    [XMLSkinElement("scrollTimeMin")]             protected int     _scrollTimeMin        = 100;        // min duration for a scrolling - speedup
    [XMLSkinElement("scrollTime")]                protected int     _scrollTimeMax        = 160;        // max. duration for a scrolling - normal
    [XMLSkinElement("spaceAfterSelected")]        protected int     _spaceAfterSelected   = 0;
    [XMLSkinElement("horizontal")]                protected bool    _horizontal           = false;
    [XMLSkinElement("showAllHover")]              protected bool    _showAllHover         = false;

    #endregion

    #region Enums
    protected enum State
    {
      Idle,
      ScrollUp,
      ScrollDown,
      ScrollUpFinal,
      ScrollDownFinal
    }
    #endregion

    #region Variables
    // Private Variables
    // Protected Variables
    protected List<MenuButtonInfo> _buttonInfos = new List<MenuButtonInfo>();
    protected List<GUIButtonControl> _buttonList = new List<GUIButtonControl>();
    protected List<GUIAnimation> _hoverList = new List<GUIAnimation>();
    protected GUIAnimation _backgroundImage = null;
    protected GUIAnimation _focusImage = null;
    //protected int _buttonWidth          = 0;          // width of one button 
    protected State _currentState = State.Idle; // current State of the animation
    protected State _nextState = State.Idle; // what follows when _currentState ends
    protected State _mouseState = State.Idle; // autoscrolling with the mouse 
    protected Viewport _newViewport = new Viewport(); // own viewport for scrolling
    protected Viewport _oldViewport;                  // storage of the currrent Viewport 
    protected int _animationTime = 0;          // duration for a scroll animation
    protected int _focusPosition = 0;          // current position of the focus bar 
    protected bool _fixedScroll = true; 		  // fix scrollbar in the middle of menu
    protected bool _useMyPlugins = true;
    protected bool _ignoreFirstUpDown = false;
    protected GUIAnimation _hoverImage = null;
    protected AnimationGroup _scrollButton = new AnimationGroup();
    protected AnimationGroup _scrollText = new AnimationGroup();
    protected int _lastButtonValue = 0;
    protected int _lastTextValue = 0;
    protected bool _reverseAnimation = false;
    protected bool _enableAnimation = true;
    // Public Variables    
    #endregion

    #region Properties
    public int FocusedButton
    {
      get { return _focusPosition; }
      set
      {
        bool oldFocus = _buttonList[_focusPosition].Focus;
        _buttonList[_focusPosition].Focus = false;
        _focusPosition = value;
        _buttonList[_focusPosition].Focus = oldFocus;
        if (!_horizontal) _focusImage.SetPosition(_focusImage._positionX, _buttonList[_focusPosition]._positionY);
        else _focusImage.SetPosition(_buttonList[_focusPosition]._positionX, _focusImage._positionY);
      }
    }

    public bool FixedScroll
    {
      get { return _fixedScroll; }
      set { _fixedScroll = value; }
    }

    public bool EnableAnimation
    {
      get { return _enableAnimation; }
      set { _enableAnimation = value; }
    }

    public List<MenuButtonInfo> ButtonInfos
    {
      get { return _buttonInfos; }
    }

    #endregion

    #region Constructors/Destructors
    public GUIMenuControl(int dwParentID)
      : base(dwParentID)
    {
    }

    #endregion

    #region Protected Methods

    #region Settings
    protected void LoadSetting()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string section = "Menu" + this.ParentID.ToString();
        int focus = xmlreader.GetValueAsInt(section, "focus", 3);
        string label = xmlreader.GetValue(section, "label");

        if (label == null) return;

        for (int i = 0; i < _buttonList.Count; i++)
        {
          if (_buttonList[focus].Label.Equals(label))
          {
            FocusedButton = focus;
            break;
          }
          // move top button to the end of the list
          foreach (GUIButtonControl btn in _buttonList)
          {
            // move all buttons up
            if (!_horizontal) btn.SetPosition(btn._positionX, btn._positionY - (_buttonHeight + _spaceBetweenButtons));
            else btn.SetPosition(btn._positionX - (_buttonWidth + _spaceBetweenButtons), btn._positionY);
          }
          // move top button to the end of the list
          GUIButtonControl button = _buttonList[0];
          _buttonList.RemoveAt(0);
          if (!_horizontal) button._positionY = _buttonList[_buttonList.Count - 1]._positionY + _spaceBetweenButtons + _buttonHeight;
          else button._positionX = _buttonList[_buttonList.Count - 1]._positionX + _spaceBetweenButtons + _buttonWidth;
          _buttonList.Add(button);
        }
      }
    }

    protected void SaveSetting()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string section = "Menu" + this.ParentID.ToString();
        xmlwriter.SetValue(section, "focus", FocusedButton.ToString());
        if (_buttonList.Count > 0) xmlwriter.SetValue(section, "label", _buttonList[FocusedButton].Label);
      }
    }
    #endregion

    #region OnUp, OnDown
    protected void OnUp()
    {
      if (_currentState != State.Idle)
      {
        _nextState = State.ScrollUp;
        if (_animationTime > _scrollTimeMin) _animationTime -= 5;
        return;
      }
      _buttonList[_focusPosition].Focus = false;  // hide button focus for animation
      if (!_horizontal) _buttonList[_focusPosition]._positionY += _spaceAfterSelected;
      else _buttonList[_focusPosition]._positionX += _spaceAfterSelected;
      _focusImage.Visible = true;                 // show Image for animation 
      _currentState = State.ScrollUp;
      InitMovement();
      _nextState = State.Idle;
      LoadHoverImage(FocusedButton - 1);
    }

    protected void OnDown()
    {
      if (_currentState != State.Idle)
      {
        _nextState = State.ScrollDown;
        if (_animationTime > _scrollTimeMin) _animationTime -= 5;
        return;
      }
      _buttonList[_focusPosition].Focus = false;  // hide button focus for animation
      if (!_horizontal) _buttonList[_focusPosition + 1]._positionY -= _spaceAfterSelected;
      else _buttonList[_focusPosition + 1]._positionX -= _spaceAfterSelected;
      _focusImage.Visible = true;                 // show Image for animation 
      _currentState = State.ScrollDown;
      InitMovement();
      _nextState = State.Idle;
      LoadHoverImage(FocusedButton + 1);
    }

    #endregion

    #region Animation
    protected void InitMovement()
    {
      if (_scrollButton.Count > 0) _scrollButton.Clear();
      if (_scrollText.Count > 0) _scrollText.Clear();

      if (_fixedScroll)                                       // we have a fixed scrollbar -> move the text
      {
        if (!_horizontal) 
        { 
          //_buttonList[_focusPosition + 1]._positionY -= _spaceAfterSelected; 
          _scrollText.Add(new Animation(_animationTime, 0, _buttonHeight + _spaceBetweenButtons)); 
        }
        else 
        { 
          //_buttonList[_focusPosition + 1]._positionX -= _spaceAfterSelected; 
          _scrollText.Add(new Animation(_animationTime, 0, _buttonWidth + _spaceBetweenButtons)); 
        }
        _scrollText.Begin();
        _lastTextValue = (int)_scrollText.StartValue;
      }
      else
      {
        if (((FocusedButton == 2) && (_currentState == State.ScrollUp)) ||                     // or scrollbar reaches top
            ((FocusedButton == _numberOfButtons - 1) && (_currentState == State.ScrollDown))) // or scrollbar reaches button
        {

          if (!_horizontal) { _buttonList[_focusPosition + 1]._positionY -= _spaceAfterSelected; _scrollText.Add(new Animation(_animationTime, 0, _buttonHeight + _spaceBetweenButtons)); }
          else { _buttonList[_focusPosition + 1]._positionX -= _spaceAfterSelected; _scrollText.Add(new Animation(_animationTime, 0, _buttonWidth + _spaceBetweenButtons)); }
          _scrollText.Begin();
          _lastTextValue = (int)_scrollText.StartValue;
          _reverseAnimation = true;

          /*  int delta = (_buttonHeight + _spaceBetweenButtons)*2/3;
            _scrollButton.Add(new Animation(_animationTime*2/3, 0, -delta));
            _scrollButton.Add(new Animation(_animationTime*2/3, -delta, 0));
            _scrollButton.Begin();
            _lastButtonValue = (int)_scrollButton.StartValue;
            */
        }
        else
        {

          if (!_horizontal) { _buttonList[_focusPosition + 1]._positionY -= _spaceAfterSelected; _scrollText.Add(new Animation(_animationTime, 0, _buttonHeight + _spaceBetweenButtons)); }
          else { _buttonList[_focusPosition + 1]._positionX -= _spaceAfterSelected; _scrollText.Add(new Animation(_animationTime, 0, _buttonWidth + _spaceBetweenButtons)); }
          _scrollButton.Begin();
          _lastButtonValue = (int)_scrollButton.StartValue;
        }
      }
    }

    protected void InsertFinalAnimation()
    {
      if (_scrollButton.Count > 0) _scrollButton.Clear();
      if (_scrollText.Count > 0) _scrollText.Clear();
      int delta = 4;
      int time = _scrollTimeMax / 4;

      _buttonList[FocusedButton].Focus = false;
      if (_fixedScroll)     // we have a fixed scrollbar -> move the text
      {
        _scrollText.Add(new Animation(2 * time, 0, 2 * delta));
        _scrollText.Add(new Animation(3 * time, 2 * delta, -delta));
        _scrollText.Add(new Animation(1 * time, -delta, 0));
        _scrollText.Begin();
        _lastTextValue = (int)_scrollText.StartValue;
      }
      else
      {
        if (_reverseAnimation)
        {
          _scrollText.Add(new Animation(2 * time, 0, 2 * delta));
          _scrollText.Add(new Animation(3 * time, 2 * delta, -delta));
          _scrollText.Add(new Animation(1 * time, -delta, 0));
          _scrollText.Begin();
          _lastTextValue = (int)_scrollText.StartValue;
          delta = -delta;
          _reverseAnimation = false;
        }
        else
        {
          _scrollButton.Add(new Animation(2 * time, 0, 2 * delta));
          _scrollButton.Add(new Animation(3 * time, 2 * delta, -delta));
          _scrollButton.Add(new Animation(1 * time, -delta, 0));
          _scrollButton.Begin();
          _lastButtonValue = (int)_scrollButton.StartValue;
        }
      }
    }

    protected void AnimationMovement(float timePassed)
    {
      AnimationMovementButton(timePassed);
      AnimationMovementText(timePassed);
    }

    protected void AnimationMovementButton(float timePassed)
    {
      if (_scrollButton.Count < 1) return;
      if (_scrollButton.Running == false) return;

      int tmpValue = _lastButtonValue;
      _lastButtonValue = (int)_scrollButton.Value;
      int increment = _lastButtonValue - tmpValue;
      if ((_currentState == State.ScrollUp) || (_currentState == State.ScrollUpFinal)) increment = -increment;
      if (!_fixedScroll)  // scrollbar can move 
      {
        if (!_horizontal) _focusImage.SetPosition(_focusImage._positionX, _focusImage._positionY + increment);
        else _focusImage.SetPosition(_focusImage._positionX + increment, _focusImage._positionY);
      }
    }

    protected void AnimationMovementText(float timePassed)
    {
      if (_scrollText.Count < 1) return;
      if (_scrollText.Running == false) return;

      int tmpValue = _lastTextValue;
      _lastTextValue = (int)_scrollText.Value;
      int increment = _lastTextValue - tmpValue;
      if ((_currentState == State.ScrollDown) || (_currentState == State.ScrollDownFinal)) increment = -increment;
      if ((_fixedScroll) ||                                                   // we have a fixed scrollbar -> move the text
          ((FocusedButton == 2) || (FocusedButton == _numberOfButtons - 1))) // or scrollbar reaches top/button
      {
        foreach (GUIControl control in _buttonList)
        {
          if (!_horizontal) control.SetPosition(control._positionX, control._positionY + increment);
          else control.SetPosition(control._positionX + increment, control._positionY);
        }
      }
    }

    protected void AnimationFinished(float timePassed)
    {
      if ((_scrollButton.Count < 1) && (_scrollText.Count < 1)) return;
      if ((_scrollButton.Running != false) || (_scrollText.Running != false)) return;

      _buttonList[FocusedButton].Focus = false;  // unfocus before any changes
      _focusImage.Visible = false;
      if (!_horizontal && _buttonList[1]._positionY < _positionY)  // Are there two buttons on top not visible?
      {
        // move the top element to the end
        GUIButtonControl button = _buttonList[0];
        _buttonList.RemoveAt(0);
        button._positionY = _buttonList[_buttonList.Count - 1]._positionY + _spaceBetweenButtons + _buttonHeight;
        _buttonList.Add(button);
      }
      else if (_horizontal && _buttonList[1]._positionX < _positionX)
      {
        // move the left element to the right
        GUIButtonControl button = _buttonList[0];
        _buttonList.RemoveAt(0);
        button._positionX = _buttonList[_buttonList.Count - 1]._positionX + _spaceBetweenButtons + _buttonWidth;
        _buttonList.Add(button);
      }
      if (!_horizontal && _buttonList[0]._positionY >= _positionY) // Is the top button visible? 
      {
        // move one button from the end of the list to the beginning
        GUIButtonControl button = _buttonList[_buttonList.Count - 1];
        _buttonList.RemoveAt(_buttonList.Count - 1);
        button._positionY = _positionY + _buttonOffset - _buttonHeight;
        _buttonList.Insert(0, button);
      }
      else if (_horizontal && _buttonList[0]._positionX >= _positionX) // Is the left button visible? 
      {
        // move one button from the end of the list to the beginning
        GUIButtonControl button = _buttonList[_buttonList.Count - 1];
        _buttonList.RemoveAt(_buttonList.Count - 1);
        button._positionX = _positionX + _buttonOffset - _buttonWidth;
        _buttonList.Insert(0, button);
      }

      if (!_fixedScroll)            // if the focusedButton is movable, then ...
      {
        switch (_currentState)
        {
          case State.ScrollDown:
            if (FocusedButton + 1 < _numberOfButtons) FocusedButton++;
            break;
          case State.ScrollUp:
            if (FocusedButton > 2) FocusedButton--;
            break;
        }
      }
      if (!_horizontal) _focusImage.SetPosition(_focusImage._positionX, _buttonList[FocusedButton]._positionY);
      else _focusImage.SetPosition(_buttonList[FocusedButton]._positionX, _focusImage._positionY);
      _buttonList[FocusedButton].Focus = true;                                             // focus after all changes
      //LoadHoverImage(FocusedButton);
      if (_scrollButton.Count > 0) _scrollButton.Clear();
      if (_scrollText.Count > 0) _scrollText.Clear();

      if ((_enableAnimation) && (_nextState == State.Idle) && (_mouseState == State.Idle))                      // let us check if we should do the final animation
      {
        switch (_currentState)
        {
          case State.ScrollDown:
            _currentState = State.ScrollDownFinal;
            InsertFinalAnimation();
            break;
          case State.ScrollUp:
            _currentState = State.ScrollUpFinal;
            InsertFinalAnimation();
            break;
          default:
            _currentState = State.Idle;
            break;
        }
      }
      else
      {
        _currentState = State.Idle;
        if ((_nextState == State.ScrollDown) || (_mouseState == State.ScrollDown)) OnDown();
        else if ((_nextState == State.ScrollUp) || (_mouseState == State.ScrollUp)) OnUp();
      }
    }
    #endregion

    #region Hover
    protected void LoadHoverImage(int position)
    {
      _hoverImage = null;
      if ((position < 0) || (position >= _hoverList.Count)) return;

      foreach (GUIAnimation hover in _hoverList)
      {
        if (hover.GetID == _buttonList[position].GetID)
        {
          _hoverImage = hover;
          _hoverImage.Begin();
          break;
        }
      }
    }
    #endregion

    #endregion

    #region <Base class> Overloads

    #region Init functions
    public override void FinalizeConstruction()
    {
      _focusPosition = (_numberOfButtons / 2) + 1;                  // +1, because of the hidden button for scrolling
      if (!_horizontal)
      {
        Height = ((_buttonHeight + _spaceBetweenButtons) * _numberOfButtons) + 2 * _buttonOffset + _spaceAfterSelected;
        _buttonWidth = Width -2 * _buttonOffset;
      }
      else
      {
        Width = ((_buttonWidth + _spaceBetweenButtons) * _numberOfButtons) + 2 * _buttonOffset + _spaceAfterSelected;
        Height = _buttonHeight;
      }
      base.FinalizeConstruction();
    }

    public override void OnInit()
    {
      _animationTime = _scrollTimeMax;
      LoadSetting();
      for (int i = (_numberOfButtons / 2); i < _buttonList.Count; i++)
      {
        if (!_horizontal) _buttonList[i]._positionY += _spaceAfterSelected;
        else _buttonList[i]._positionX += _spaceAfterSelected;
      }
      LoadHoverImage(FocusedButton);
      base.OnInit();
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleVertical(ref _buttonOffset);
      if (!_horizontal)
      {
        GUIGraphicsContext.ScaleVertical(ref _spaceBetweenButtons);
        GUIGraphicsContext.ScaleVertical(ref _spaceAfterSelected);
      }
      else
      {
        GUIGraphicsContext.ScaleHorizontal(ref _spaceBetweenButtons);
        GUIGraphicsContext.ScaleHorizontal(ref _spaceAfterSelected);
      }
      GUIGraphicsContext.ScaleVertical(ref _buttonHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _buttonTextXOffset, ref _buttonTextYOffset);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _hoverPositionX, ref _hoverPositionY);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _hoverWidth, ref _hoverHeight);
    }

    #endregion

    #region Resources
    public override void AllocResources()
    {
      int buttonX = 0;
      int buttonY = 0;
      if (!_horizontal)
      {
        buttonX = _positionX + _buttonOffset;
        buttonY = _positionY + _buttonOffset + _spaceBetweenButtons;
        buttonY -= (_buttonHeight + _spaceBetweenButtons);           // one invisible button for scrolling 
      }
      else
      {
        buttonX = _positionX + _buttonOffset + _spaceBetweenButtons;
        buttonY = _positionY + _buttonOffset;
        buttonX -= (_buttonWidth + _spaceBetweenButtons);           // one invisible button for scrolling 
      }

      int controlID = 1;
      _buttonList.Clear();
      _hoverList.Clear();
      while ((_buttonInfos.Count > 0) && (_buttonList.Count < _numberOfButtons + 1))
      {
        for (int i = 0; i < _buttonInfos.Count; i++)
        {
          MenuButtonInfo info = _buttonInfos[i];
          GUIButtonControl button;
          if (_showAllHover)
          {
            //MessageBox.Show(info.HoverName + " " + info.NonFocusHoverName);
            button = new GUIButtonControl(GetID, controlID, buttonX, buttonY, _buttonWidth, _buttonHeight, _textColor, _textColorNoFocus,
                                      (info.HoverName != "") ? info.HoverName : _textureButtonFocus,
                                      (info.NonFocusHoverName != "") ? info.NonFocusHoverName : _textureHoverNoFocus);
          }
          else
          {
            button = new GUIButtonControl(GetID, controlID, buttonX, buttonY, _buttonWidth, _buttonHeight, _textColor, _textColorNoFocus,
                                      (info.FocusTextureName != "") ? info.FocusTextureName : _textureButtonFocus,
                                      (info.NonFocusTextureName != "") ? info.NonFocusTextureName : _textureButtonNoFocus);
          }
          button.Label = info.Text;
          button.Data = info;
          button.ParentControl = this;
          button.FontName = _buttonFont;
          button.TextOffsetX = _buttonTextXOffset;
          button.TextOffsetY = _buttonTextYOffset;
          button.DimColor = DimColor;
          button.AllocResources();
          _buttonList.Add(button);
          if (!_horizontal) buttonY += (_buttonHeight + _spaceBetweenButtons);
          else buttonX += (_buttonWidth + _spaceBetweenButtons);
          controlID++;
        }
      }
      _backgroundImage = LoadAnimationControl(GetID, controlID, _positionX, _positionY, _width, Height, _textureBackground);
      _backgroundImage.AllocResources();
      controlID++;

      if (!_horizontal) _focusImage = LoadAnimationControl(GetID, controlID, buttonX, _buttonList[FocusedButton]._positionY, _buttonWidth, _buttonHeight, _textureButtonFocus);
      else _focusImage = LoadAnimationControl(GetID, controlID, _buttonList[FocusedButton]._positionX, buttonY, _buttonWidth, _buttonHeight, _textureButtonFocus);
      _focusImage.AllocResources();
      _focusImage.Visible = false;

      if ((_hoverHeight > 0) && (_hoverWidth > 0))
      {
        foreach (GUIButtonControl btn in _buttonList)
        {
          string fileName = null;
          foreach (MenuButtonInfo info in _buttonInfos)
          {
            if (info.Text.Equals(btn.Label))
            {
              fileName = info.HoverName;
              break;
            }              
          }
          if (fileName != null)
          {
            GUIAnimation hover = LoadAnimationControl(GetID, btn.GetID, _hoverPositionX, _hoverPositionY, _hoverWidth, _hoverHeight, fileName);
            hover.KeepAspectRatio = _hoverKeepAspectRatio;
            hover.RepeatBehavior = new RepeatBehavior(1);
            hover.AllocResources();
            _hoverList.Add(hover);
          }
        }
      }
      base.AllocResources();
    }

    public override void FreeResources()
    {
      SaveSetting();
      foreach (GUIControl control in _buttonList) control.FreeResources();
      foreach (GUIAnimation hover in _hoverList) hover.FreeResources();
      _buttonList.Clear();
      _hoverList.Clear();
      if (_backgroundImage != null) _backgroundImage.FreeResources();
      if (_focusImage != null) _focusImage.FreeResources();
      base.FreeResources();
    }
    #endregion

    #region OnAction
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int x = (int)action.fAmount1;
            int y = (int)action.fAmount2;
            int controlID = 0;
            bool focused = false;

            _ignoreFirstUpDown = false;
            if (HitTest(x, y, out controlID, out focused))
            {
              double middlePosY = YPosition + Height / 2;
              double middlePosX = XPosition + Width / 2;
              if (_fixedScroll)  // we can not move the scrollbar
              {
                if (!_horizontal && y < middlePosY - _buttonHeight / 2 - _spaceBetweenButtons)
                {
                  middlePosY -= _buttonHeight / 2;
                  _animationTime = _scrollTimeMax - (int)((_scrollTimeMax - _scrollTimeMin) *
                                   (Math.Abs(middlePosY - y) / (middlePosY - YPosition)));
                  if (_currentState == State.Idle) OnUp();
                  else _mouseState = State.ScrollUp;
                }
                else if (!_horizontal && y > middlePosY + _buttonHeight / 2 + _spaceBetweenButtons)
                {
                  middlePosY += _buttonHeight / 2;
                  _animationTime = _scrollTimeMax - (int)((_scrollTimeMax - _scrollTimeMin) *
                                   (Math.Abs(middlePosY - y) / (middlePosY - YPosition)));
                  if (_currentState == State.Idle) OnDown();
                  else _mouseState = State.ScrollDown;
                }
                else if (_horizontal && x < middlePosX - _buttonWidth / 2 - _spaceBetweenButtons)
                {
                  middlePosY -= _buttonWidth / 2;
                  _animationTime = _scrollTimeMax - (int)((_scrollTimeMax - _scrollTimeMin) *
                                                   (Math.Abs(middlePosY - x) / (middlePosY - XPosition)));
                  if (_currentState == State.Idle) OnUp();
                  else _mouseState = State.ScrollUp;
                }
                else if (_horizontal && x > middlePosX + _buttonWidth / 2 + _spaceBetweenButtons)
                {
                  middlePosY += _buttonWidth / 2;
                  _animationTime = _scrollTimeMax - (int)((_scrollTimeMax - _scrollTimeMin) *
                                                   (Math.Abs(middlePosY - x) / (middlePosY - XPosition)));
                  if (_currentState == State.Idle) OnDown();
                  else _mouseState = State.ScrollDown;
                }
                else
                {
                  _mouseState = State.Idle;
                }
              }
              else   // scrollbar can move
              {
                if (!_horizontal && y < YPosition + _buttonOffset + _spaceBetweenButtons + _buttonHeight)
                {
                  if (_currentState == State.Idle) OnUp();
                  else _mouseState = State.ScrollUp;
                }
                else if (!_horizontal && y > YPosition + Height - _buttonOffset - 2 * _spaceBetweenButtons - _buttonHeight)
                {
                  if (_currentState == State.Idle) OnDown();
                  else _mouseState = State.ScrollDown;
                }
                else if (_horizontal && x < XPosition + _buttonOffset + _spaceBetweenButtons + _buttonWidth)
                {
                  if (_currentState == State.Idle) OnUp();
                  else _mouseState = State.ScrollUp;
                }
                else if (_horizontal && x > XPosition + Width - _buttonOffset - 2 * _spaceBetweenButtons - _buttonWidth)
                {
                  if (_currentState == State.Idle) OnDown();
                  else _mouseState = State.ScrollDown;
                }
                else if (!_horizontal)// direct selection in the middle
                {
                  _mouseState = State.Idle;
                  if (_currentState == State.Idle)
                  {  // move scroll bar only when idle
                    double button = _buttonHeight + _spaceBetweenButtons;
                    int position = 1 + (int)Math.Round(((double)y - YPosition - 2 * _buttonOffset) / button);
                    if (Math.Abs(FocusedButton - position) > 1)
                    {
                      FocusedButton = position;
                      LoadHoverImage(FocusedButton);
                    }
                    else
                    {
                      if (position > FocusedButton) OnDown();
                      else if (position < FocusedButton) OnUp();
                    }
                  }
                }
                else// direct selection in the middle
                {
                  _mouseState = State.Idle;
                  if (_currentState == State.Idle)
                  {  // move scroll bar only when idle
                    double button = _buttonWidth + _spaceBetweenButtons;
                    int position = 1 + (int)Math.Round(((double)x - XPosition - 2 * _buttonOffset) / button);
                    if (Math.Abs(FocusedButton - position) > 1)
                    {
                      FocusedButton = position;
                      LoadHoverImage(FocusedButton);
                    }
                    else
                    {
                      if (position > FocusedButton) OnDown();
                      else if (position < FocusedButton) OnUp();
                    }
                  }
                }
              }
            }
            break;
          }

        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (!_horizontal)
            {
              if (_ignoreFirstUpDown)
              {
                _ignoreFirstUpDown = false;
                return;
              }
              if (_currentState == State.Idle) _animationTime = _scrollTimeMax;
              _mouseState = State.Idle;
              OnUp();
              return;  
            }
            break;
            // no return here to pass the base.action through (think of topbar) ;-)                         
          }

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            if (!_horizontal)
            {
              if (_ignoreFirstUpDown)
              {
                _ignoreFirstUpDown = false;
                return;
              }
              if (_currentState == State.Idle) _animationTime = _scrollTimeMax;
              _mouseState = State.Idle;
              OnDown();
              return;
            }
            break; 
          }

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (_horizontal)
            {
              if (_ignoreFirstUpDown)
              {
                _ignoreFirstUpDown = false;
                return;
              }
              if (_currentState == State.Idle) _animationTime = _scrollTimeMax;
              _mouseState = State.Idle;
              OnUp();
              return;
            }
            break;
            // no return here to pass the base.action through (think of topbar) ;-)            
          }

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (_horizontal)
            {
              if (_ignoreFirstUpDown)
              {
                _ignoreFirstUpDown = false;
                return;
              }
              if (_currentState == State.Idle) _animationTime = _scrollTimeMax;
              _mouseState = State.Idle;
              OnDown();
              return;
            }
            break;
          }

        case Action.ActionType.ACTION_SELECT_ITEM:

        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            if ((_currentState == State.ScrollUp) || (_currentState == State.ScrollDown)) return;
            MenuButtonInfo info = _buttonList[FocusedButton].Data as MenuButtonInfo;
            if (info != null)
            {
              // button selected.
              // send a message to the parent window
              GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, info.PluginID, 0, info);
              GUIGraphicsContext.SendMessage(message);
            }
            break;
          }

      }

      base.OnAction(action);
    }
    #endregion

    #region Render
    public override void Render(float timePassed)
    {
      if (_backgroundImage != null) _backgroundImage.Render(timePassed);
      if (_hoverImage != null && !_showAllHover) _hoverImage.Render(timePassed);
      _oldViewport = GUIGraphicsContext.DX9Device.Viewport;
      if (!_horizontal)
      {
        _newViewport.X = _positionX;
        _newViewport.Y = _positionY + _buttonOffset;
        _newViewport.Width = Width;
        _newViewport.Height = Height - 2 * _buttonOffset;
      }
      else
      {
        _newViewport.X = _positionX + _buttonOffset;
        _newViewport.Y = _positionY;
        _newViewport.Width = Width - 2 * _buttonOffset;
        _newViewport.Height = Height;
      }
      _newViewport.MinZ = 0.0f;
      _newViewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = _newViewport;

      if (_currentState != State.Idle) AnimationMovement(timePassed);
      if (Focus) _focusImage.Render(timePassed);
      foreach (GUIButtonControl button in _buttonList)
      {
        button.Render(timePassed);
      }
      GUIGraphicsContext.DX9Device.Viewport = _oldViewport;
      if (_currentState != State.Idle) AnimationFinished(timePassed);
      base.Render(timePassed);
    }
    #endregion

    #region Focus
    public override bool Focus
    {
      get { return base.Focus; }
      set
      {
        if (value == false)
        {
          _mouseState = State.Idle;  // stop scrolling
        }
        else
        {
          //_ignoreFirstUpDown = true;
          if (_backgroundImage != null) _backgroundImage.Begin();
          if (_focusImage != null) _focusImage.Begin();
        }
        base.Focus = value;
        if (_buttonList.Count > FocusedButton) _buttonList[FocusedButton].Focus = value;
      }
    }
    #endregion

    #endregion
  }

  #region MenuButtonInfo
  public class MenuButtonInfo
  {
    #region Variables
    protected string _text;
    protected int _pluginID;
    protected string _focusedTextureName;
    protected string _nonFocusedTextureName;
    protected string _hoverName;
    protected string _nonFocusHoverName;
    #endregion

    #region Constructors/Destructors
    public MenuButtonInfo(string Text, int PlugInID, string FocusTextureName, string NonFocusName, string HoverName, string NonFocusHoverName)
    {
      _text = Text;
      _pluginID = PlugInID;
      _focusedTextureName = FocusTextureName;
      _nonFocusedTextureName = NonFocusName;
      _hoverName = HoverName;
      _nonFocusHoverName = NonFocusHoverName;
    }

    public MenuButtonInfo(string Text, int PlugInID, string FocusTextureName, string NonFocusName, string HoverName)
    {
      _text = Text;
      _pluginID = PlugInID;
      _focusedTextureName = FocusTextureName;
      _nonFocusedTextureName = NonFocusName;
      _hoverName = HoverName;
      _nonFocusHoverName = NonFocusName;
    }
    #endregion

    #region Properties
    public string Text
    { get { return _text; } }

    public int PluginID
    { get { return _pluginID; } }

    public string FocusTextureName
    { get { return _focusedTextureName; } }

    public string NonFocusTextureName
    { get { return _nonFocusedTextureName; } }

    public string HoverName
    { get { return _hoverName; } }

    public string NonFocusHoverName
    { get { return _nonFocusHoverName; } }
    #endregion
  }

  #endregion

}
