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

using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class implementing a GUIButton.
  /// </summary>
  public class GUIButtonControl : GUIControl
  {
    [XMLSkinElement("textureFocus")] protected string _focusedTextureName = "";
    [XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = "";
    [XMLSkinElement("font")] protected string _fontName;
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected long _textColorNoFocus = 0xFFFFFFFF;
    [XMLSkinElement("disabledcolor")] protected long _disabledColor = 0xFF606060;
    [XMLSkinElement("hyperlink")] protected int _hyperLinkWindowId = -1;
    [XMLSkinElement("action")] protected int _actionId = -1;
    [XMLSkinElement("script")] protected string _scriptAction = "";
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("textvalign")] protected VAlignment _textVAlignment = VAlignment.ALIGN_TOP;
    [XMLSkinElement("application")] protected string _application = "";
    [XMLSkinElement("arguments")] protected string _arguments = "";
    [XMLSkinElement("hover")] protected string _hoverFilename = string.Empty;
    [XMLSkinElement("hoverX")] protected int _hoverX;
    [XMLSkinElement("hoverY")] protected int _hoverY;
    [XMLSkinElement("hoverWidth")] protected int _hoverWidth;
    [XMLSkinElement("hoverHeight")] protected int _hoverHeight;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = -1;
    [XMLSkinElement("scrollWrapString")] protected string _userWrapString = "";
    [XMLSkin("textureFocus", "border")] protected string _strBorderTF = "";
    [XMLSkin("textureFocus", "position")] protected string _strBorderPositionTF = "outside";
    [XMLSkin("textureFocus", "textureRepeat")] protected bool _borderTextureRepeatTF = false;
    [XMLSkin("textureFocus", "textureRotate")] protected bool _borderTextureRotateTF = false;
    [XMLSkin("textureFocus", "texture")] protected string _borderTextureFileNameTF = "image_border.png";
    [XMLSkin("textureFocus", "colorKey")] protected long _borderColorKeyTF = 0xFFFFFFFF;
    [XMLSkin("textureNoFocus", "border")] protected string _strBorderTNF = "";
    [XMLSkin("textureNoFocus", "position")] protected string _strBorderPositionTNF = "outside";
    [XMLSkin("textureNoFocus", "textureRepeat")] protected bool _borderTextureRepeatTNF = false;
    [XMLSkin("textureNoFocus", "textureRotate")] protected bool _borderTextureRotateTNF = false;
    [XMLSkin("textureNoFocus", "texture")] protected string _borderTextureFileNameTNF = "image_border.png";
    [XMLSkin("textureNoFocus", "colorKey")] protected long _borderColorKeyTNF = 0xFFFFFFFF;
    [XMLSkin("hover", "border")] protected string _strBorderH = "";
    [XMLSkin("hover", "position")] protected string _strBorderPositionH = "outside";
    [XMLSkin("hover", "textureRepeat")] protected bool _borderTextureRepeatH = false;
    [XMLSkin("hover", "textureRotate")] protected bool _borderTextureRotateH = false;
    [XMLSkin("hover", "texture")] protected string _borderTextureFileNameH = "image_border.png";
    [XMLSkin("hover", "colorKey")] protected long _borderColorKeyH = 0xFFFFFFFF;

    protected int _frameCounter = 0;
    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;
    protected GUIAnimation _hoverImage = null;
    protected GUIControl _labelControl = null;
    private bool _keepLook = false;

    public GUIButtonControl(int dwParentID)
      : base(dwParentID)
    {
    }

    /// <summary>
    /// The constructor of the GUIButtonControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
    /// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degress along x-axis.</param>
    /// <param name="dwShadowDistance">The distance of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUIButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                            string strTextureFocus, string strTextureNoFocus,
                            int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      FinalizeConstruction();
    }

    // allow overriding the textcolor if created by GUIMenuControl
    public GUIButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                            long ltextColor, long ltextcolorNoFocus, string strTextureFocus, string strTextureNoFocus,
                            int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      _textColor = ltextColor;
      _textColorNoFocus = ltextcolorNoFocus;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      FinalizeConstruction();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                           _focusedTextureName);
      _imageFocused.ParentControl = this;
      _imageFocused.Filtering = false;
      _imageFocused.DimColor = DimColor;
      _imageFocused.ColourDiffuse = ColourDiffuse;
      _imageFocused.SetBorder(_strBorderTF, _strBorderPositionTF, _borderTextureRepeatTF, _borderTextureRotateTF, _borderTextureFileNameTF, _borderColorKeyTF);

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _nonFocusedTextureName);
      _imageNonFocused.ParentControl = this;
      _imageNonFocused.Filtering = false;
      _imageNonFocused.DimColor = DimColor;
      _imageNonFocused.ColourDiffuse = ColourDiffuse;
      _imageNonFocused.SetBorder(_strBorderTNF, _strBorderPositionTNF, _borderTextureRepeatTNF, _borderTextureRotateTNF, _borderTextureFileNameTNF, _borderColorKeyTNF);

      if (_hoverFilename != string.Empty)
      {
        GUIGraphicsContext.ScaleRectToScreenResolution(ref _hoverX, ref _hoverY, ref _hoverWidth, ref _hoverHeight);
        _hoverImage = LoadAnimationControl(_parentControlId, _controlId, _hoverX, _hoverY, _hoverWidth, _hoverHeight,
                                           _hoverFilename);
        _hoverImage.ParentControl = this;
        _hoverImage.DimColor = DimColor;
        _hoverImage.ColourDiffuse = ColourDiffuse;
        _hoverImage.SetBorder(_strBorderH, _strBorderPositionH, _borderTextureRepeatH, _borderTextureRotateH, _borderTextureFileNameH, _borderColorKeyH);
      }

      GUILocalizeStrings.LocalizeLabel(ref _label);

      // Use a GUIFadeLabel if a valid scrollStartDelay is specified, otherwise use a GUILabelControl (default)
      if (_scrollStartDelay < 0)
      {
        _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                            _label, _textColor, Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP, false, _shadowAngle, _shadowDistance, _shadowColor);
        ((GUILabelControl)_labelControl).TextAlignment = _textAlignment;
        ((GUILabelControl)_labelControl).TextVAlignment = _textVAlignment;
      }
      else 
      {
        _labelControl = new GUIFadeLabel(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                         _label, _textColor, Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP,
                                        _shadowAngle, _shadowDistance, _shadowColor,
                                         _userWrapString);
        ((GUIFadeLabel)_labelControl).TextAlignment = _textAlignment;
        ((GUIFadeLabel)_labelControl).TextVAlignment = _textVAlignment;
        ((GUIFadeLabel)_labelControl).AllowScrolling = false;
        ((GUIFadeLabel)_labelControl).AllowFadeIn = false;
      }
      _labelControl.DimColor = DimColor;
      _labelControl.ParentControl = this;
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control to scale itself to the current screen resolution
    /// </summary>
    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
    }


    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (value != IsFocused)
        {
          if (value == true)
          {
            if (_imageFocused != null)
            {
              _imageFocused.Begin();
            }
            GUIPropertyManager.SetProperty("#highlightedbutton", Label);
            // When button focus is obtained, the GUIFadeLabel (if specified) is allowed to scroll.
            if (_labelControl is GUIFadeLabel)
            {
              ((GUIFadeLabel)_labelControl).Clear(); // Resets the control to use the delayed start
              ((GUIFadeLabel)_labelControl).AllowScrolling = true;
            }
          }
          else
          {
            if (_imageNonFocused != null)
            {
              _imageNonFocused.Begin();
            }
            // When button focus is lost, the GUIFadeLabel (if specified) is not allowed to scroll.
            if (_labelControl is GUIFadeLabel)
            {
              ((GUIFadeLabel)_labelControl).AllowScrolling = false;
            }
          }
        }

        base.Focus = value;
      }
    }

    /// <summary>
    /// Renders the GUIButtonControl.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }

      // The GUIButtonControl has the focus or the focused look is being maintained.
      // _keepLook forces the render of the focused (or hovered) image when focus may have been lost.
      // _keepLook is managed by onleft,onright,onup,ondown actions.
      if (Focus || _keepLook)
      {
        // Apply the dim color to the render if _keepLook is set; avoid affecting Dimmed otherwise.
        if (_keepLook)
        {
          _imageFocused.Dimmed = true;
        }

        //render the focused image
        _imageFocused.Render(timePassed);

        if (_hoverImage != null)
        {
          _hoverImage.Render(timePassed);
        }
      }
      else
      {
        //render the non-focused image
        _imageNonFocused.Render(timePassed);
      }

      int labelWidth = _width - 2*_textOffsetX;
      if (labelWidth <= 0)
      {
        base.Render(timePassed);
        return;
      }
      _labelControl.Width = labelWidth;
      if (_labelControl is GUILabelControl)
      {
        ((GUILabelControl)_labelControl).TextAlignment = _textAlignment;
        ((GUILabelControl)_labelControl).TextVAlignment = _textVAlignment;
        ((GUILabelControl)_labelControl).Label = _label;
        ((GUILabelControl)_labelControl).TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
      }
      else
      {
        ((GUIFadeLabel)_labelControl).TextAlignment = _textAlignment;
        ((GUIFadeLabel)_labelControl).TextVAlignment = _textVAlignment;
        ((GUIFadeLabel)_labelControl).Label = _label;
        ((GUIFadeLabel)_labelControl).TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
      }

      // render the text on the button
      int x = 0;
      int y = 0;

      switch (_textAlignment)
      {
        case Alignment.ALIGN_LEFT:
          x = _textOffsetX + _positionX;
          break;

        case Alignment.ALIGN_RIGHT:
          x = _positionX + _width - _textOffsetX;
          break;

        case Alignment.ALIGN_CENTER:
          x = _positionX + ((_width/2) - (labelWidth/2));
          break;
      }

      switch (_textVAlignment)
      {
        case VAlignment.ALIGN_TOP:
          y = _textOffsetY + _positionY;
          break;

        case VAlignment.ALIGN_BOTTOM:
          y = _positionY + _height - _textOffsetY;
          break;

        case VAlignment.ALIGN_MIDDLE:
          y = _positionY + ((_height / 2) - (_labelControl.Height / 2));
          break;
      }

      _labelControl.SetPosition(x, y);
      _labelControl.Render(timePassed);
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
      base.OnAction(action);
      GUIMessage message;
      if (Focus)
      {
        switch (action.wID)
        {
          case Action.ActionType.ACTION_MOVE_DOWN:
            _keepLook = _keepLookOnDown;
            break;
          case Action.ActionType.ACTION_MOVE_UP:
            _keepLook = _keepLookOnUp;
            break;
          case Action.ActionType.ACTION_MOVE_LEFT:
            _keepLook = _keepLookOnLeft;
            break;
          case Action.ActionType.ACTION_MOVE_RIGHT:
            _keepLook = _keepLookOnRight;
            break;
        }

        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (ContextMenu != null)
          {
            DoContextMenu();
            return;
          }

          // If this button contains scriptactions call the scriptactions.
          if (_application.Length != 0)
          {
            //button should start an external application, so start it
            Process proc = new Process();

            string workingFolder = Path.GetFullPath(_application);
            string fileName = Path.GetFileName(_application);
            workingFolder = workingFolder.Substring(0, workingFolder.Length - (fileName.Length + 1));
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.WorkingDirectory = workingFolder;
            proc.StartInfo.Arguments = _arguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            //proc.WaitForExit();
          }

          // If this links to another window go to the window.
          if (_hyperLinkWindowId >= 0)
          {
            GUIWindowManager.ActivateWindow((int) _hyperLinkWindowId);
            return;
          }
          // If this button corresponds to an action generate that action.
          if (ActionID >= 0)
          {
            Action newaction = new Action((Action.ActionType) ActionID, 0, 0);
            GUIGraphicsContext.OnAction(newaction);
            return;
          }

          // button selected.
          if (SubItemCount > 0)
          {
            // if we got subitems, then change the label of the control to the next
            //subitem
            SelectedItem++;
            if (SelectedItem >= SubItemCount)
            {
              SelectedItem = 0;
            }
            Label = (string) GetSubItem(SelectedItem);
          }

          // send a message to anyone interested 
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
      // Handle the GUI_MSG_LABEL_SET message
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
          return true;
        }
      }
      // Let the base class handle the other messages
      if (base.OnMessage(message))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();

      if (_hoverImage != null)
      {
        _hoverImage.PreAllocResources();
      }
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

      if (_hoverImage != null)
      {
        _hoverImage.AllocResources();
      }

      _width = _imageFocused.Width;
      _height = _imageFocused.Height;

      if (SubItemCount > 0)
      {
        Label = (string) GetSubItem(SelectedItem);
      }
      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.AllocResources();
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageFocused.FreeResources();
      _imageNonFocused.FreeResources();
      _labelControl.FreeResources();

      if (_hoverImage != null)
      {
        _hoverImage.FreeResources();
      }
    }

    /// <summary>
    /// Sets the position of the control.
    /// </summary>
    /// <param name="dwPosX">The X position.</param>
    /// <param name="dwPosY">The Y position.</param>		
    public override void SetPosition(int dwPosX, int dwPosY)
    {
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

      if (_hoverImage != null)
      {
        _hoverImage.SetAlpha(dwAlpha);
      }
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
    /// Get the filename of the texture when the GUIButtonControl does not have the focus.
    /// </summary>
    public string TexutureNoFocusName
    {
      get { return _nonFocusedTextureName; } //_imageNonFocused.FileName; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIButtonControl has the focus.
    /// </summary>
    public string TexutureFocusName
    {
      get { return _focusedTextureName; } //_imageFocused.FileName; }
    }

    /// <summary>
    /// Get the filename of the hover texture when the GUIButtonControl.
    /// </summary>
    public string HoverFilename
    {
      get { return _hoverFilename; }
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
    /// Get/Set the color of the text on the GUIButtonControl when it has no focus. 
    /// </summary>
    public long TextColorNoFocus
    {
      get { return _textColorNoFocus; }
      set { _textColorNoFocus = value; }
    }


    /// <summary>
    /// Get/set the name of the font of the text of the GUIButtonControl.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName = value;
        if (_labelControl is GUILabelControl)
        {
          ((GUILabelControl)_labelControl).FontName = _fontName;
        }
        else
        {
          ((GUIFadeLabel)_labelControl).FontName = _fontName;
        }
      }
    }

    /// <summary>
    /// Set the text of the GUIButtonControl. 
    /// </summary>
    /// <param name="strFontName">The font name.</param>
    /// <param name="strLabel">The text.</param>
    /// <param name="dwColor">The font color.</param>
    public void SetLabel(string strFontName, string strLabel, long dwColor)
    {
      if (strFontName == null)
      {
        return;
      }
      if (strLabel == null)
      {
        return;
      }
      Label = strLabel;
      _textColor = dwColor;
      _fontName = strFontName;

      if (_labelControl is GUILabelControl)
      {
        ((GUILabelControl)_labelControl).FontName = _fontName;
        ((GUILabelControl)_labelControl).TextColor = dwColor;
        ((GUILabelControl)_labelControl).Label = strLabel;
      }
      else
      {
        ((GUIFadeLabel)_labelControl).FontName = _fontName;
        ((GUIFadeLabel)_labelControl).TextColor = dwColor;
        ((GUIFadeLabel)_labelControl).Label = strLabel;
      }
    }

    /// <summary>
    /// Get/set the text of the GUIButtonControl.
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
        if (_labelControl is GUILabelControl)
        {
          ((GUILabelControl)_labelControl).Label = _label;
        }
        else
        {
          ((GUIFadeLabel)_labelControl).Label = _label;
        }
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
      set
      {
        if (value == null)
        {
          return;
        }
        _scriptAction = value;
      }
    }

    /// <summary>
    /// Get/set the action ID that corresponds to this button.
    /// </summary>
    public int ActionID
    {
      get { return _actionId; }
      set { _actionId = value; }
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
    /// Get/set the X-Position of the hover.
    /// </summary>
    public int HoverX
    {
      get { return _hoverX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverX = value;
      }
    }

    /// <summary>
    /// Get/set the Y-Position of the hover.
    /// </summary>
    public int HoverY
    {
      get { return _hoverY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverY = value;
      }
    }

    /// <summary>
    /// Get/set the width of the hover.
    /// </summary>
    public int HoverWidth
    {
      get { return _hoverWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverWidth = value;
      }
    }

    /// <summary>
    /// Get/set the height of the hover.
    /// </summary>
    public int HoverHeight
    {
      get { return _hoverHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverHeight = value;
      }
    }

    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    public VAlignment TextVAlignment
    {
      get { return _textVAlignment; }
      set { _textVAlignment = value; }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>
    protected override void Update()
    {
      base.Update();

      _imageFocused.ColourDiffuse = ColourDiffuse;
      _imageFocused.Width = _width;
      _imageFocused.Height = _height;

      _imageNonFocused.ColourDiffuse = ColourDiffuse;
      _imageNonFocused.Width = _width;
      _imageNonFocused.Height = _height;

      _imageFocused.SetPosition(_positionX, _positionY);
      _imageNonFocused.SetPosition(_positionX, _positionY);
    }

    public void Refresh()
    {
      Update();
    }

    /// <summary>
    /// Get/Set the the application filename
    /// which should be launched when this button gets clicked
    /// </summary>
    public string Application
    {
      get { return _application; }
      set
      {
        if (value == null)
        {
          return;
        }
        _application = value;
      }
    }

    /// <summary>
    /// Get/Set the arguments for the application
    /// which should be launched when this button gets clicked
    /// </summary>
    public string Arguments
    {
      get { return _arguments; }
      set
      {
        if (value == null)
        {
          return;
        }
        _arguments = value;
      }
    }

    /// <summary>
    /// get/set the current selected item
    /// A button can have 1 or more subitems
    /// each subitem has its own text to render on the button
    /// When the user presses the button, the next item will be selected
    /// and shown on the button
    /// </summary>
    public override int SelectedItem
    {
      get { return _selectedItem; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (SubItemCount > 0)
        {
          _selectedItem = value;
          if (_selectedItem < 0 || _selectedItem >= SubItemCount)
          {
            _selectedItem = 0;
          }
          Label = (string) GetSubItem(_selectedItem);
        }
        else
        {
          _selectedItem = 0;
        }
      }
    }

    private void DoContextMenu()
    {
      IDialogbox dialog = (IDialogbox) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_MENU);

      if (dialog == null)
      {
        return;
      }

      dialog.Reset();
      dialog.SetHeading(924); // menu

      foreach (object item in ContextMenu.Items)
      {
        if (item is MenuItem)
        {
          dialog.Add(((MenuItem) item).Header as string);
        }
      }

      dialog.DoModal(ParentID);

      if (dialog.SelectedId == -1)
      {
        return;
      }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
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