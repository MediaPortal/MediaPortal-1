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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The implementation of a selection button (e.g., the Switch View button in My Music).
  /// </summary>
  public class GUISelectButtonControl : GUIControl
  {
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected long _textColorNoFocus = 0xFFFFFFFF;
    [XMLSkinElement("disabledcolor")] protected long _disabledColor = 0xFF606060;
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("font")] protected string _fontName;
    [XMLSkinElement("textureFocus")] protected string _textureFocusName = "";
    [XMLSkinElement("textureNoFocus")] protected string _textureNoFocusName = "";
    [XMLSkinElement("texturebg")] protected string _backgroundTextureName = "";
    [XMLSkinElement("textureLeft")] protected string _leftTextureName = "";
    [XMLSkinElement("textureRight")] protected string _rightTextureName = "";
    [XMLSkinElement("textureLeftFocus")] protected string _leftFocusName = "";
    [XMLSkinElement("textureRightFocus")] protected string _rightFocusName = "";
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;
    [XMLSkinElement("textXOff2")] protected int _textOffsetX2 = 0;
    [XMLSkinElement("textYOff2")] protected int _textOffsetY2 = 0;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    protected GUIAnimation _imageBackground = null;
    protected GUIAnimation _imageLeft = null;
    protected GUIAnimation _imageLeftFocus = null;
    protected GUIAnimation _imageRight = null;
    protected GUIAnimation _imageRightFocus = null;
    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;

    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;

    protected GUIFont _font = null;
    protected bool _showSelect = false;


    protected int _frameCounter = 0;


    protected int _hyperLinkWindowId = -1;
    protected string _scriptAction = "";
    protected int _defaultItem = -1;
    protected int _startFrame = 0;
    protected bool _leftSelected = false;
    protected bool _rightSelected = false;
    protected long _ticks = 0;
    protected bool _updateNeeded = false;
    protected bool _autoHide = true;
    protected GUILabelControl _labelControl = null;
    protected bool _resetSelectionAfterFocusLost = true;

    public event EventHandler CaptionChanged;

    public GUISelectButtonControl(int dwParentID)
      : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUISelectButtonControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strButtonFocus">The filename containing the texture of the butten, when the button has the focus.</param>
    /// <param name="strButton">The filename containing the texture of the butten, when the button does not have the focus.</param>
    /// <param name="strSelectBackground">The background texture of the button.</param>
    /// <param name="strSelectArrowLeft">The texture of the left non-focused arrow.</param>
    /// <param name="strSelectArrowLeftFocus">The texture of the left focused arrow.</param>
    /// <param name="strSelectArrowRight">The texture of the right non-focused arrow.</param>
    /// <param name="strSelectArrowRightFocus">The texture of the right focused arrow.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degress along x-axis.</param>
    /// <param name="dwShadowDistance">The distance of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUISelectButtonControl(int dwParentID, int dwControlId,
                                  int dwPosX, int dwPosY,
                                  int dwWidth, int dwHeight,
                                  string strButtonFocus, string strButton,
                                  string strSelectBackground,
                                  string strSelectArrowLeft, string strSelectArrowLeftFocus,
                                  string strSelectArrowRight, string strSelectArrowRightFocus,
                                  int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _textureFocusName = strButtonFocus;
      _textureNoFocusName = strButton;
      _backgroundTextureName = strSelectBackground;
      _rightTextureName = strSelectArrowRight;
      _rightFocusName = strSelectArrowRightFocus;
      _leftTextureName = strSelectArrowLeft;
      _leftFocusName = strSelectArrowLeftFocus;
      _updateNeeded = false;
      _isSelected = false;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      int x1 = 16;
      int y1 = 16;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      _imageFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                           _textureFocusName);
      _imageFocused.ParentControl = this;
      _imageFocused.DimColor = DimColor;

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _textureNoFocusName);
      _imageNonFocused.ParentControl = this;
      _imageNonFocused.DimColor = DimColor;

      _imageBackground = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _backgroundTextureName);
      _imageBackground.ParentControl = this;
      _imageBackground.DimColor = DimColor;

      _imageLeft = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, x1, y1, _leftTextureName);
      _imageLeft.DimColor = DimColor;
      _imageLeft.ParentControl = this;

      _imageLeftFocus = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, x1, y1,
                                             _leftFocusName);
      _imageLeftFocus.ParentControl = this;
      _imageLeftFocus.DimColor = DimColor;

      _imageRight = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, x1, y1, _rightTextureName);
      _imageRight.ParentControl = this;
      _imageRight.DimColor = DimColor;

      _imageRightFocus = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, x1, y1,
                                              _rightFocusName);
      _imageRightFocus.ParentControl = this;
      _imageRightFocus.DimColor = DimColor;

      if (_fontName != "" && _fontName != "-")
      {
        _font = GUIFontManager.GetFont(_fontName);
      }
      GUILocalizeStrings.LocalizeLabel(ref _label);
      _imageFocused.Filtering = false;
      _imageNonFocused.Filtering = false;
      _imageBackground.Filtering = false;
      _imageLeft.Filtering = false;
      _imageLeftFocus.Filtering = false;
      _imageRight.Filtering = false;
      _imageRightFocus.Filtering = false;
      _labelControl = new GUILabelControl(_parentControlId);
      _labelControl.CacheFont = true;
      _labelControl.ParentControl = this;
      _labelControl.SetShadow(_shadowAngle, _shadowDistance, _shadowColor);
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX2, ref _textOffsetY2);
    }


    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
    /// some controls (for example the fadelabel) contain scrolling texts and need 2
    /// ne re-rendered constantly.
    /// </summary>
    /// <returns>true or false</returns>
    public override bool NeedRefresh()
    {
      if (_showSelect)
      {
        return true;
      }
      if (_updateNeeded)
      {
        _updateNeeded = false;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }


      if (Focus)
      {
        _showSelect = true;
      }
      else
      {
        _showSelect = false;
      }

      //	Are we in selection mode
      if (_showSelect)
      {
        //	Yes, render the select control

        //	render background, left and right arrow

        _imageBackground.Render(timePassed);

        long dwTextColor = Focus ? _textColor : _textColorNoFocus;

        //	User has moved left...
        if (_leftSelected)
        {
          //	...render focused arrow
          _startFrame++;
          if (_autoHide && _startFrame >= 25)
          {
            _startFrame = 0;
            _leftSelected = false;
            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                                null);
            GUIWindowManager.SendThreadMessage(message);
            _updateNeeded = true;
          }
          _imageLeftFocus.Render(timePassed);

          //	If we are moving left
          //	render item text as disabled
          dwTextColor = _disabledColor;
        }
        else
        {
          //	Render none focused arrow
          _imageLeft.Render(timePassed);
        }


        //	User has moved right...
        if (_rightSelected)
        {
          //	...render focused arrow
          _startFrame++;
          if (_autoHide && _startFrame >= 25)
          {
            _startFrame = 0;
            _rightSelected = false;
            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                                null);
            GUIWindowManager.SendThreadMessage(message);
            _updateNeeded = true;
          }
          _imageRightFocus.Render(timePassed);

          //	If we are moving right
          //	render item text as disabled
          dwTextColor = _disabledColor;
        }
        else
        {
          //	Render none focused arrow
          _imageRight.Render(timePassed);
        }


        //	Render text if a current item is available
        if (SelectedItem >= 0 && null != _font && SelectedItem < _subItemList.Count)
        {
          _labelControl.FontName = _font.FontName;
          if (_textAlignment == Alignment.ALIGN_RIGHT)
          {
            _labelControl.SetPosition(_positionX + _width - _imageLeft.Width - _textOffsetX, _textOffsetY + _positionY);
          }
          else
          {
            _labelControl.SetPosition(_positionX + _imageLeft.Width + _textOffsetX, _textOffsetY + _positionY);
          }

          _labelControl.TextColor = dwTextColor;
          _labelControl.TextAlignment = _textAlignment;
          _labelControl.Label = (string)_subItemList[SelectedItem];
          _labelControl.Width = _width - (_imageRight.Width + _imageLeft.Width + _textOffsetX);
          _labelControl.Render(timePassed);
        }
        /*
                //	Select current item, if user doesn't 
                //	move left or right for 1.5 sec.
                long dwTicksSpan=DateTime.Now.Ticks-_ticks;
                dwTicksSpan/=10000;
                if ((float)(dwTicksSpan/1000)>0.8f)
                {
                  //	User hasn't moved disable selection mode...
                  _showSelect=false;

                  //	...and send a thread message.
                  //	(Sending a message with SendMessage 
                  //	can result in a GPF.)
                  GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
                  GUIWindowManager.SendThreadMessage(message);
                  _updateNeeded=true;
                }
        */
      } //	if (_showSelect)
      else
      {
        //	No, render a normal button

        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }

        if (Focus)
        {
/*
          int dwAlphaCounter = _frameCounter+2;
          int dwAlphaChannel;
          if ((dwAlphaCounter%128)>=64)
            dwAlphaChannel = dwAlphaCounter%64;
          else
            dwAlphaChannel = 63-(dwAlphaCounter%64);

          dwAlphaChannel += 192;
          SetAlpha(dwAlphaChannel );
          _imageFocused.IsVisible=true;
          _imageNonFocused.IsVisible=false;
          _frameCounter++;*/
          _imageFocused.Render(timePassed);
        }
        else
        {
          //SetAlpha(0xff);
          _imageNonFocused.Render(timePassed);
        }

        if (_label != null && _label.Length > 0 && _font != null)
        {
          _labelControl.FontName = _font.FontName;
          if (_textAlignment == Alignment.ALIGN_RIGHT)
          {
            _labelControl.SetPosition(_positionX + _width - _textOffsetX2, _textOffsetY2 + _positionY);
          }
          else
          {
            _labelControl.SetPosition(_textOffsetX2 + _positionX, _textOffsetY2 + _positionY);
          }

          if (Disabled || _subItemList.Count == 0)
          {
            _labelControl.TextColor = _disabledColor;
          }
          else
          {
            _labelControl.TextColor = Focus ? _textColor : _textColorNoFocus;
          }
          _labelControl.TextAlignment = _textAlignment;
          _labelControl.Label = _label;
          _labelControl.Render(timePassed);
        }
      }
      base.Render(timePassed);
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      GUIMessage message;
      if (!_showSelect)
      {
        if (Focus)
        {
          if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
          {
            //	Enter selection mode
            _showSelect = true;

            //	Start timer, if user doesn't select an item
            //	or moves left/right. The control will 
            //	automatically select the current item.
            _ticks = DateTime.Now.Ticks;
            return;
          }
        }
      }
      else
      {
        if (action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIWindowManager.SendThreadMessage(msg);
          return;
        }
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
        {
          if (_rightSelected)
          {
            action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
            OnAction(action);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIWindowManager.SendThreadMessage(msg);
            _updateNeeded = true;
            return;
          }
          else if (_leftSelected)
          {
            action.wID = Action.ActionType.ACTION_MOVE_LEFT;
            OnAction(action);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIWindowManager.SendThreadMessage(msg);
            _updateNeeded = true;
            return;
          }
          else
          {
            //	User has selected an item, disable selection mode...
            _showSelect = false;

            // ...and send a message.
            message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
            GUIGraphicsContext.SendMessage(message);
            return;
          }
        }
        else if (action.wID == Action.ActionType.ACTION_MOVE_LEFT)
        {
          //	Set for visual feedback
          _leftSelected = true;
          _rightSelected = false;
          _startFrame = 0;

          //	Reset timer for automatically selecting
          //	the current item.
          _ticks = DateTime.Now.Ticks;

          //	Switch to previous item
          if (_subItemList.Count > 0)
          {
            SelectedItem--;
            if (SelectedItem < 0)
            {
              SelectedItem = _subItemList.Count - 1;
            }

            if (CaptionChanged != null)
            {
              CaptionChanged(this, EventArgs.Empty);
            }
          }
          return;
        }
        else if (action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
        {
          //	Set for visual feedback
          _rightSelected = true;
          _leftSelected = false;
          _startFrame = 0;

          //	Reset timer for automatically selecting
          //	the current item.
          _ticks = DateTime.Now.Ticks;

          //	Switch to next item
          if (_subItemList.Count > 0)
          {
            SelectedItem++;
            if (SelectedItem >= (int)_subItemList.Count)
            {
              SelectedItem = 0;
            }

            if (CaptionChanged != null)
            {
              CaptionChanged(this, EventArgs.Empty);
            }
          }
          return;
        }
        if (action.wID == Action.ActionType.ACTION_MOVE_UP || action.wID == Action.ActionType.ACTION_MOVE_DOWN)
        {
          //	Disable selection mode when moving up or down
          _showSelect = false;
          if (_resetSelectionAfterFocusLost)
          {
            SelectedItem = _defaultItem;
          }
        }
      }

      base.OnAction(action);

      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (_scriptAction.Length > 0)
          {
            message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
            message.Label = _scriptAction;
            //g_actionManager.CallScriptAction(message); // TODO!
          }

          if (_hyperLinkWindowId >= 0)
          {
            GUIWindowManager.ActivateWindow((int)_hyperLinkWindowId);
            return;
          }
          // button selected.
          // send a message
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
        }
      }
    }

    /// <summary>
    /// OnMessage() This method gets called when there's a new message. 
    /// Controls send messages to notify their parents about their state (changes)
    /// By overriding this method a control can respond to the messages of its controls
    /// </summary>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        // Adds an item to the list.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          Add(message.Label);
        }
          // Resets the list of items.
        else if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }
          // Gets the selected item.
        else if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          if (SelectedItem >= 0 && SelectedItem < _subItemList.Count)
          {
            message.Param1 = SelectedItem;
            message.Label = (string)_subItemList[SelectedItem];
          }
          else
          {
            message.Param1 = -1;
            message.Label = "";
          }
        }
          // Selects an item.
        else if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          _defaultItem = SelectedItem = (int)message.Param1;
        }
      }
      // Sets the label of the control.
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            _label = message.Label;
          }

          return true;
        }
      }
      if (base.OnMessage(message))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageFocused.FreeResources();
      _imageNonFocused.FreeResources();
      _imageBackground.FreeResources();

      _imageLeft.FreeResources();
      _imageLeftFocus.FreeResources();

      _imageRight.FreeResources();
      _imageRightFocus.FreeResources();


      _labelControl.FreeResources();
      _showSelect = false;
    }

    public override bool CanFocus()
    {
      if (!IsVisible)
      {
        return false;
      }
      if (Disabled)
      {
        return false;
      }
      if (_subItemList.Count < 2)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();
      _imageBackground.PreAllocResources();

      _imageLeft.PreAllocResources();
      _imageLeftFocus.PreAllocResources();

      _imageRight.PreAllocResources();
      _imageRightFocus.PreAllocResources();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _frameCounter = 0;
      _imageFocused.AllocResources();
      _imageNonFocused.AllocResources();
      _width = _imageFocused.Width;
      _height = _imageFocused.Height;
      _imageBackground.AllocResources();

      _imageLeft.AllocResources();
      _imageLeftFocus.AllocResources();

      _imageRight.AllocResources();
      _imageRightFocus.AllocResources();

      _font = GUIFontManager.GetFont(_fontName);

      //	Position right arrow
      int x1 = 8;
      int x2 = 16;
      GUIGraphicsContext.ScaleHorizontal(ref x1);
      GUIGraphicsContext.ScaleHorizontal(ref x2);
      int dwPosX = (_positionX + _width - x1) - x2;

      int y1 = 16;
      GUIGraphicsContext.ScaleVertical(ref y1);
      int dwPosY = _positionY + (_height - y1) / 2;
      _imageRight.SetPosition(dwPosX, dwPosY);
      _imageRightFocus.SetPosition(dwPosX, dwPosY);

      //	Position left arrow
      dwPosX = _positionX + x1;
      _imageLeft.SetPosition(dwPosX, dwPosY);
      _imageLeftFocus.SetPosition(dwPosX, dwPosY);

      _labelControl.AllocResources();
    }

    /// <summary>
    /// Sets the position of the control.
    /// </summary>
    /// <param name="dwPosX">The X position.</param>
    /// <param name="dwPosY">The Y position.</param>	
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (dwPosX < 0)
      {
        return;
      }
      if (dwPosY < 0)
      {
        return;
      }
      base.SetPosition(dwPosX, dwPosY);
      _imageFocused.SetPosition(dwPosX, dwPosY);
      _imageNonFocused.SetPosition(dwPosX, dwPosY);
    }

    /// <summary>
    /// Changes the alpha transparency component of the colordiffuse.
    /// </summary>
    /// <param name="dwAlpha">The new value of the colordiffuse.</param>
    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      _imageFocused.SetAlpha(dwAlpha);
      _imageNonFocused.SetAlpha(dwAlpha);
    }

    /// <summary>
    /// Get/set the color of the text when the GUIButtonControl is disabled.
    /// </summary>
    public long DisabledColor
    {
      get { return _disabledColor; }
      set { _disabledColor = value; }
    }

    /// <summary>
    /// Gets the name of the texture for the unfocused button.
    /// </summary>
    public string TexutureNoFocusName
    {
      get { return _imageNonFocused.FileName; }
    }

    /// <summary>
    /// Gets the name of the texture for the unfocused button.
    /// </summary>
    public string TexutureFocusName
    {
      get { return _imageFocused.FileName; }
    }

    /// <summary>
    /// Set the color of the text on the GUIButtonControl. 
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get the fontname of the label.
    /// </summary>
    public string FontName
    {
      get { return _font.FontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        _font.FontName = value;
      }
    }

    /// <summary>
    /// Set the text of the control. 
    /// </summary>
    /// <param name="strFontName">The font name.</param>
    /// <param name="strLabel">The text.</param>
    /// <param name="dwColor">The font color.</param>
    public void SetLabel(string strFontName, string strLabel, long dwColor)
    {
      if (strLabel == null)
      {
        return;
      }
      if (strFontName == null)
      {
        return;
      }
      _label = strLabel;
      _textColor = dwColor;
      _font = GUIFontManager.GetFont(strFontName);
    }

    /// <summary>
    /// Get/set the text of the control.
    /// </summary>
    public string Label
    {
      get { return _label; }
      set
      {
        if (value == null)
        {
          return;
        }
        _label = value;
      }
    }

    /// <summary>
    /// Get/set the window ID to which the GUIButtonControl links.
    /// </summary>
    public int HyperLink
    {
      get { return _hyperLinkWindowId; }
      set { _hyperLinkWindowId = value; }
    }

    /// <summary>
    /// Get/set the scriptaction that needs to be performed when the button is clicked.
    /// </summary>
    public string ScriptAction
    {
      get { return _scriptAction; }
      set { _scriptAction = value; }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>
    protected override void Update()
    {
      base.Update();

      _imageFocused.Width = _width;
      _imageFocused.Height = _height;

      _imageNonFocused.Width = _width;
      _imageNonFocused.Height = _height;
      _imageBackground.Width = _width;
      _imageBackground.Height = _height;


      _imageFocused.SetPosition(_positionX, _positionY);
      _imageNonFocused.SetPosition(_positionX, _positionY);
      _imageBackground.SetPosition(_positionX, _positionY);
    }

    /// <summary>
    /// Gets the name of the left texture for the unfocused button.
    /// </summary>
    public string TextureLeft
    {
      get { return _imageLeft.FileName; }
    }

    /// <summary>
    /// Gets the name of the left texture for the focused button.
    /// </summary>
    public string TextureLeftFocus
    {
      get { return _imageLeftFocus.FileName; }
    }

    /// <summary>
    /// Gets the name of the right texture for the unfocused button.
    /// </summary>
    public string TextureRight
    {
      get { return _imageRight.FileName; }
    }

    /// <summary>
    /// Gets the name of the right texture for the focused button.
    /// </summary>
    public string TextureRightFocus
    {
      get { return _imageRightFocus.FileName; }
    }

    /// <summary>
    /// Gets the name of the background texture for the button.
    /// </summary>
    public string TextureBackground
    {
      get { return _imageBackground.FileName; }
    }

    /// <summary>
    /// Checks if the x and y coordinates correspond to the current control.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>True if the control was hit.</returns>
    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      _autoHide = true;
      // first check if mouse is within bounds of button
      if (x < _imageBackground.XPosition || x > _imageBackground.XPosition + _imageBackground.RenderWidth ||
          y < _imageBackground.YPosition || y > _imageBackground.YPosition + _imageBackground.RenderHeight)
      {
        return false;
      }

      //yes it is
      // check if control is selected

      _autoHide = false;
      // control is selected
      // first check left button

      if (x >= _imageLeftFocus.XPosition && x <= _imageLeftFocus.XPosition + _imageLeftFocus.RenderWidth)
      {
        if (y >= _imageLeftFocus.YPosition && y <= _imageLeftFocus.YPosition + _imageLeftFocus.RenderHeight)
        {
          if (!_leftSelected)
          {
            _leftSelected = true;
            _rightSelected = false;
            _ticks = DateTime.Now.Ticks;
            _startFrame = 0;
          }
          return true;
        }
        else
        {
          _leftSelected = false;
        }
      }
      else
      {
        _leftSelected = false;
      }

      // check right button
      if (x >= _imageRightFocus.XPosition && x <= _imageRightFocus.XPosition + _imageRightFocus.RenderWidth)
      {
        if (y >= _imageRightFocus.YPosition && y <= _imageRightFocus.YPosition + _imageRightFocus.RenderHeight)
        {
          if (!_rightSelected)
          {
            _rightSelected = true;
            _leftSelected = false;
            _ticks = DateTime.Now.Ticks;
            _startFrame = 0;
            return true;
          }
          return true;
        }
        else
        {
          _rightSelected = false;
        }
      }
      else
      {
        _rightSelected = false;
      }
      return true;
    }

    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX
    {
      get { return _textOffsetX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetX = value;
      }
    }

    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY
    {
      get { return _textOffsetY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetY = value;
      }
    }

    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX2
    {
      get { return _textOffsetX2; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetX2 = value;
      }
    }

    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY2
    {
      get { return _textOffsetY2; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetY2 = value;
      }
    }

    public string SelectedLabel
    {
      get
      {
        if (SelectedItem >= 0 && SelectedItem < _subItemList.Count)
        {
          return (string)_subItemList[SelectedItem];
        }
        return string.Empty;
      }
    }

    public override int SelectedItem
    {
      get { return base.SelectedItem; }
      set
      {
        base.SelectedItem = value;

        if (CaptionChanged != null)
        {
          CaptionChanged(this, EventArgs.Empty);
        }
      }
    }

    public void Clear()
    {
      _subItemList.Clear();
      SelectedItem = -1;
      _defaultItem = -1;
    }

    public void Add(string line)
    {
      if (_subItemList.Count <= 0)
      {
        SelectedItem = 0;
        _defaultItem = 0;
      }
      _subItemList.Add(line);
    }

    public bool RestoreSelection
    {
      get { return _resetSelectionAfterFocusLost; }
      set { _resetSelectionAfterFocusLost = value; }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageBackground != null)
        {
          _imageBackground.DimColor = value;
        }
        if (_imageLeft != null)
        {
          _imageLeft.DimColor = value;
        }
        if (_imageLeftFocus != null)
        {
          _imageLeftFocus.DimColor = value;
        }
        if (_imageRight != null)
        {
          _imageRight.DimColor = value;
        }
        if (_imageRightFocus != null)
        {
          _imageRightFocus.DimColor = value;
        }
        if (_imageFocused != null)
        {
          _imageFocused.DimColor = value;
        }
        if (_imageNonFocused != null)
        {
          _imageNonFocused.DimColor = value;
        }
        if (_labelControl != null)
        {
          _labelControl.DimColor = value;
        }
      }
    }
  }
}