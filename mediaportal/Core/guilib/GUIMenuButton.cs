
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

using System.Collections;
using System.Diagnostics;
using System.IO;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class implementing a GUIMenuButton.
  /// </summary>
  public class GUIMenuButton : GUIControl
  {
    public enum ButtonMode //:int
    {
      BUTTON_MODE_DIALOG_LIST,  // present a dialog window with a list of choices
      BUTTON_MODE_SPIN_LIST,    // present selection arrows as a spin control list of choices

      // needed for XAML parser
      DialogList = BUTTON_MODE_DIALOG_LIST,
      SpinList = BUTTON_MODE_SPIN_LIST
    };

    // Determines the overall behavior of the button.
    [XMLSkinElement("mode")] protected ButtonMode _buttonMode = ButtonMode.BUTTON_MODE_DIALOG_LIST;
    [XMLSkinElement("textureFocus")] protected string _focusedTextureName = "";
    [XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = "";
    [XMLSkinElement("onclick")] protected string _onclick = "";
    [XMLSkinElement("binding")] protected string _binding = "";
    [XMLSkinElement("font")] protected string _fontName;
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("valueTextInButton")] protected bool _valueTextInButton = false;
    [XMLSkin("valueTextInButton", "align")] protected Alignment _valueTextInButtonAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("valuePrefixText")] protected string _prefixText = "";
    [XMLSkin("valuePrefixText", "join")] protected string _prefixTextJoin = "";
    [XMLSkinElement("valueSuffixText")] protected string _suffixText = "";
    [XMLSkin("valueSuffixText", "join")] protected string _suffixTextJoin = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected long _textColorNoFocus = 0xFFFFFFFF;
    [XMLSkinElement("disabledcolor")] protected long _disabledColor = 0xFF606060;
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkin("textXOff", "hasMargin")] protected bool _textOffsetXHasMargin = true;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("textvalign")] protected VAlignment _textVAlignment = VAlignment.ALIGN_TOP;
    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = -1;
    [XMLSkinElement("scrollWrapString")] protected string _userWrapString = "";
    [XMLSkinElement("hover")] protected string _hoverFilename = string.Empty;
    [XMLSkinElement("hoverX")] protected int _hoverX;
    [XMLSkinElement("hoverY")] protected int _hoverY;
    [XMLSkinElement("hoverWidth")] protected int _hoverWidth;
    [XMLSkinElement("hoverHeight")] protected int _hoverHeight;
    [XMLSkin("textureFocus", "border")] protected string _strBorderTF = "";
    [XMLSkin("textureFocus", "position")] protected GUIImage.BorderPosition _borderPositionTF = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureFocus", "textureRepeat")] protected bool _borderTextureRepeatTF = false;
    [XMLSkin("textureFocus", "textureRotate")] protected bool _borderTextureRotateTF = false;
    [XMLSkin("textureFocus", "texture")] protected string _borderTextureFileNameTF = "image_border.png";
    [XMLSkin("textureFocus", "colorKey")] protected long _borderColorKeyTF = 0xFFFFFFFF;
    [XMLSkin("textureFocus", "corners")] protected bool _borderHasCornersTF = false;
    [XMLSkin("textureFocus", "cornerRotate")] protected bool _borderCornerTextureRotateTF = true;
    [XMLSkin("textureFocus", "tileFill")] protected bool _textureFocusTileFill = false;
    [XMLSkin("textureFocus", "mask")] protected string _strMaskTF = "";
    [XMLSkin("textureNoFocus", "border")] protected string _strBorderTNF = "";
    [XMLSkin("textureNoFocus", "position")] protected GUIImage.BorderPosition _borderPositionTNF = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureNoFocus", "textureRepeat")] protected bool _borderTextureRepeatTNF = false;
    [XMLSkin("textureNoFocus", "textureRotate")] protected bool _borderTextureRotateTNF = false;
    [XMLSkin("textureNoFocus", "texture")] protected string _borderTextureFileNameTNF = "image_border.png";
    [XMLSkin("textureNoFocus", "colorKey")] protected long _borderColorKeyTNF = 0xFFFFFFFF;
    [XMLSkin("textureNoFocus", "corners")] protected bool _borderHasCornersTNF = false;
    [XMLSkin("textureNoFocus", "cornerRotate")] protected bool _borderCornerTextureRotateTNF = true;
    [XMLSkin("textureNoFocus", "mask")] protected string _strMaskTNF = "";
    [XMLSkin("textureNoFocus", "tileFill")] protected bool _textureNoFocusTileFill = false;
    [XMLSkin("hover", "border")] protected string _strBorderH = "";
    [XMLSkin("hover", "position")] protected GUIImage.BorderPosition _borderPositionH = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("hover", "textureRepeat")] protected bool _borderTextureRepeatH = false;
    [XMLSkin("hover", "textureRotate")] protected bool _borderTextureRotateH = false;
    [XMLSkin("hover", "texture")] protected string _borderTextureFileNameH = "image_border.png";
    [XMLSkin("hover", "colorKey")] protected long _borderColorKeyH = 0xFFFFFFFF;
    [XMLSkin("hover", "corners")] protected bool _borderHasCornersH = false;
    [XMLSkin("hover", "cornerRotate")] protected bool _borderCornerTextureRotateH = true;
    [XMLSkin("hover", "mask")] protected string _strMaskH = "";
    [XMLSkin("hover", "tileFill")] protected bool _hoverTileFill = false;

    // Mode SpinList skin elements
    [XMLSkinElement("textureUp")] protected string _upTextureName;
    [XMLSkinElement("textureDown")] protected string _downTextureName;
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus;
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus;
    [XMLSkinElement("spinWidth")] protected int _spinWidth = 16;
    [XMLSkinElement("spinHeight")] protected int _spinHeight = 16;
    [XMLSkinElement("spinXOff")] protected int _spinOffsetX = 0;
    [XMLSkinElement("spinYOff")] protected int _spinOffsetY = 0;
    [XMLSkinElement("spinalign")] protected Alignment _spinAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("spinvalign")] protected VAlignment _spinVAlignment = VAlignment.ALIGN_MIDDLE;
    [XMLSkinElement("spinTextXOff")] protected int _spinTextOffsetX = 0;
    [XMLSkinElement("spinTextYOff")] protected int _spinTextOffsetY = 0;
    [XMLSkinElement("showrange")] protected bool _showRange = true;
    [XMLSkinElement("digits")] protected int _digits = -1;
    [XMLSkinElement("reverse")] protected bool _reverse = false;
    [XMLSkinElement("cycleItems")] protected bool _cycleItems = false;
    [XMLSkinElement("spintype")] protected GUISpinControl.SpinType _spinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_TEXT;
    [XMLSkinElement("orientation")] protected eOrientation _orientation = eOrientation.Horizontal;

    // Mode DialogList skin elements
    [XMLSkinElement("dialogTitle")] protected string _dialogTitle = "";
    [XMLSkinElement("dialogShowNumbers")] protected bool _dialogShowNumbers = true;

    protected int _frameCounter = 0;
    private bool _keepLook = false;
    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;
    protected GUIAnimation _hoverImage = null;
    protected GUIControl _labelControl = null;
    protected GUIControl _valueLabelControl = null;
    protected GUISpinControl _spinControl = null;

    // List of dialog menu labels and values are held locally.  Spin list labels and values are given to the spinControl.
    private ArrayList _listMenuLabels = new ArrayList();
    private ArrayList _listMenuValues = new ArrayList();
    private int _listSelected = 0;
    private bool _isRangeSet = false;

    public GUIMenuButton(int dwParentID) : base(dwParentID) { }

    /// <summary>
    /// The constructor of the GUIMenuButton class as a spin list.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strTextureFocus">The filename containing the texture of the button, when the button has the focus.</param>
    /// <param name="strTextureNoFocus">The filename containing the texture of the button, when the button does not have the focus.</param>
    /// <param name="strUpFocus">The filename containing the texture of the spin up button, when the button has the focus.</param>
    /// <param name="strUpNoFocus">The filename containing the texture of the spin up button, when the button does not have the focus.</param>
    /// <param name="strDownFocus">The filename containing the texture of the spin down button, when the button has the focus.</param>
    /// <param name="strDownNoFocus">The filename containing the texture of the spin down button, when the button does not have the focus.</param>
    /// <param name="dwSpinWidth">The width of the spin control.</param>
    /// <param name="dwSpinHeight">The height of the spin control.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degrees along x-axis.</param>
    /// <param name="dwShadowDistance">The height of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUIMenuButton(int dwParentID, int dwControlId, int dwPosX,
                         int dwPosY, int dwWidth, int dwHeight,
                         string strTextureFocus, string strTextureNoFocus,
                         string strUpFocus, string strUpNoFocus, string strDownFocus, string strDownNoFocus,
                         int dwSpinWidth, int dwSpinHeight,
                         int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _buttonMode = ButtonMode.BUTTON_MODE_SPIN_LIST;
      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      _upTextureName = strUpNoFocus;
      _upTextureNameFocus = strUpFocus;
      _downTextureName = strDownNoFocus;
      _downTextureNameFocus = strDownFocus;
      _spinWidth = dwSpinWidth;
      _spinHeight = dwSpinHeight;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      FinalizeConstruction();
    }

    /// <summary>
    /// The constructor of the GUIMenuButton class as a dialog list.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degrees along x-axis.</param>
    /// <param name="dwShadowDistance">The height of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUIMenuButton(int dwParentID, int dwControlId, int dwPosX,
                         int dwPosY, int dwWidth, int dwHeight,
                         int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _buttonMode = ButtonMode.BUTTON_MODE_DIALOG_LIST;
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
      _imageFocused.SetBorder(_strBorderTF, _borderPositionTF, _borderTextureRepeatTF, _borderTextureRotateTF,
        _borderTextureFileNameTF, _borderColorKeyTF, _borderHasCornersTF, _borderCornerTextureRotateTF);
      TileFillTF = _textureFocusTileFill;
      _imageFocused.MaskFileName = _strMaskTF;

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _nonFocusedTextureName);
      _imageNonFocused.ParentControl = this;
      _imageNonFocused.Filtering = false;
      _imageNonFocused.DimColor = DimColor;
      _imageNonFocused.SetBorder(_strBorderTNF, _borderPositionTNF, _borderTextureRepeatTNF, _borderTextureRotateTNF,
        _borderTextureFileNameTNF, _borderColorKeyTNF, _borderHasCornersTNF, _borderCornerTextureRotateTNF);
      TileFillTNF = _textureNoFocusTileFill;
      _imageNonFocused.MaskFileName = _strMaskTNF;

      _prefixText = _prefixText + _prefixTextJoin;
      _suffixText = _suffixTextJoin + _suffixText;

      GUILocalizeStrings.LocalizeLabel(ref _label);
      GUILocalizeStrings.LocalizeLabel(ref _prefixText);
      GUILocalizeStrings.LocalizeLabel(ref _suffixText);
      GUILocalizeStrings.LocalizeLabel(ref _dialogTitle);

      if (_hoverFilename != string.Empty)
      {
        GUIGraphicsContext.ScaleRectToScreenResolution(ref _hoverX, ref _hoverY, ref _hoverWidth, ref _hoverHeight);
        _hoverImage = LoadAnimationControl(_parentControlId, _controlId, _hoverX, _hoverY, _hoverWidth, _hoverHeight,
                                           _hoverFilename);
        _hoverImage.ParentControl = this;
        _hoverImage.DimColor = DimColor;
        _hoverImage.ColourDiffuse = ColourDiffuse;
        _hoverImage.SetBorder(_strBorderH, _borderPositionH, _borderTextureRepeatH, _borderTextureRotateH,
          _borderTextureFileNameH, _borderColorKeyH, _borderHasCornersH, _borderCornerTextureRotateH);
        TileFillH = _hoverTileFill;
        _hoverImage.MaskFileName = _strMaskH;
      }

      if (_scrollStartDelay < 0)
      {
        _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                            _label, _textColor, Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP, false,
                                            _shadowAngle, _shadowDistance, _shadowColor);
        ((GUILabelControl)_labelControl).ParentControl = this;
        ((GUILabelControl)_labelControl).DimColor = DimColor;
        ((GUILabelControl)_labelControl).TextAlignment = _textAlignment;
        ((GUILabelControl)_labelControl).TextVAlignment = _textVAlignment;

        // If the value should be displayed in the button but the alignment is not left then we need another control to display the right aligned
        // button value.
        if (_valueTextInButtonAlignment == Alignment.ALIGN_RIGHT)
        {
          _valueLabelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                                   _label, _textColor, Alignment.ALIGN_RIGHT, VAlignment.ALIGN_TOP, false,
                                                   _shadowAngle, _shadowDistance, _shadowColor);
          ((GUILabelControl)_valueLabelControl).ParentControl = this;
          ((GUILabelControl)_valueLabelControl).DimColor = DimColor;
          ((GUILabelControl)_valueLabelControl).TextVAlignment = _textVAlignment;
        }
      }
      else
      {
        _labelControl = new GUIFadeLabel(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                         _textColor, Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP,
                                        _shadowAngle, _shadowDistance, _shadowColor,
                                         _userWrapString);
        ((GUIFadeLabel)_labelControl).TextAlignment = _textAlignment;
        ((GUIFadeLabel)_labelControl).TextVAlignment = _textVAlignment;
        ((GUIFadeLabel)_labelControl).AllowScrolling = false;
        ((GUIFadeLabel)_labelControl).AllowFadeIn = false;

        // If the value should be displayed in the button but the alignment is not left then we need another control to display the right aligned
        // button value.
        if (_valueTextInButtonAlignment == Alignment.ALIGN_RIGHT)
        {
          _valueLabelControl = new GUIFadeLabel(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                                _textColor, Alignment.ALIGN_RIGHT, VAlignment.ALIGN_TOP,
                                                _shadowAngle, _shadowDistance, _shadowColor,
                                                _userWrapString);
          ((GUIFadeLabel)_valueLabelControl).ParentControl = this;
          ((GUIFadeLabel)_valueLabelControl).DimColor = DimColor;
          ((GUIFadeLabel)_valueLabelControl).TextVAlignment = _textVAlignment;
          ((GUIFadeLabel)_valueLabelControl).AllowScrolling = false;
          ((GUIFadeLabel)_valueLabelControl).AllowFadeIn = false;
        }
      }

      // Build elements for the specified button mode.
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_SPIN_LIST:

          string spinFontName = _fontName;
          if (_valueTextInButton)
          {
            // When the spin control font name is null the spin control will not render the text.
            // _valueTextInButton will render the spin control text in the button label.
            spinFontName = null;
          }

          _spinControl = new GUISpinControl(GetID, 0, _positionX + _width - _spinWidth, _positionY, _spinWidth,
                                              _spinHeight, _upTextureName, _downTextureName,
                                              _upTextureNameFocus, _downTextureNameFocus,
                                              spinFontName, _textColor, _spinType, _spinAlignment);
          _spinControl.ParentControl = this;
          _spinControl.DimColor = DimColor;
          _spinControl.ShowRange = _showRange;
          _spinControl.Digits = _digits;
          _spinControl.SetReverse(_reverse);
          _spinControl.Orientation = _orientation;
          _spinControl.CycleItems = _cycleItems;
          _spinControl.SetShadow(_shadowAngle, _shadowDistance, _shadowColor);
          _spinControl.PrefixText = _prefixText;
          _spinControl.SuffixText = _suffixText;
          _spinControl.TextOffsetX = _spinTextOffsetX;
          _spinControl.TextOffsetY = _spinTextOffsetY;
          break;

        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          break;
      }

      // Add subitems to the menu.
      int value = 0;
      for (int i = 0; i < SubItemCount; ++i)
      {
        string strItem = GUIPropertyManager.GetProperty((string)GetSubItem(i));
        if (strItem == null || strItem == "")
        {
          // Refetch the subitem if a property value was not returned.
          strItem = (string)GetSubItem(i);
        }

        // Allow for the subitem to be a CSV list of values.  If it is then add each item in the CSV list.
        ArrayList items = new ArrayList(strItem.Split(new char[] { ',' }));
        for (int j=0; j < items.Count; j++)
        {
          AddItem(items[j].ToString(), value++);
        }
      }

      for (int i = 0; i < SubItemCount; ++i)
      {
        RemoveSubItem(i);
      }

      // If specified, bind the selection.
      if (_binding.Length > 0)
      {
        bindToValue(_binding);
      }

      // Initialize the controls properties.
      SetProperties();
    }

    private void bindToValue(string strValue)
    {
      string strTemp = strValue;
      strValue = GUIPropertyManager.GetProperty(strValue);
      if (strValue == null || strValue == "")
      {
        strValue = strTemp;
      }
      SetSelectedItemByLabel(strValue);
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();

      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          _spinControl.PreAllocResources();
          break;
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
      _width = _imageFocused.Width;
      _height = _imageFocused.Height;
      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.AllocResources();

      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          _spinControl.AllocResources();
          break;
      }
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _imageFocused.SafeDispose();
      _imageNonFocused.SafeDispose();
      _labelControl.SafeDispose();

      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          _spinControl.SafeDispose();
          break;
      }
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

    /// <summary>
    /// Gets and sets whether or not the control has focus.
    /// </summary>
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

        switch (_buttonMode)
        {
          case ButtonMode.BUTTON_MODE_DIALOG_LIST:
            break;
          case ButtonMode.BUTTON_MODE_SPIN_LIST:
            _spinControl.Focus = value;
            break;
        }      
      }
    }

    /// <summary>
    /// Renders the GUIMenuButton.
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

      // If specified, bind the selection.
      if (_binding.Length > 0)
      {
        bindToValue(_binding);
      }

      // This control has the focus or the focused look is being maintained.
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

      // Perform actions appropriate for the button mode.
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          RenderAsDialogList(timePassed);
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          RenderAsSpinList(timePassed);
          break;
      }
      base.Render(timePassed);
    }

    /// <summary>
    /// Renders the button to present a dialog window on select.
    /// </summary>
    private void RenderAsDialogList(float timePassed)
    {
      // Render the button label.
      // Compute with of label so that the text fits within the button.
      int labelWidth = _width;
      if (_textOffsetXHasMargin)
      {
        labelWidth = _width - (2 * _textOffsetX);
      }
      if (labelWidth <= 0)
      {
        return;
      }
      _labelControl.Width = labelWidth;

      RenderButtonLabel(timePassed, labelWidth);
    }

    /// <summary>
    /// Renders the button as a spin list button.
    /// </summary>
    private void RenderAsSpinList(float timePassed)
    {
      // Render the button label.
      // Compute width of label so that the text does not overlap the spin controls.
      int labelWidth = _width - (2 * _spinWidth) - _textOffsetX;
      if (_textOffsetXHasMargin)
      {
        labelWidth = _width - (2 * _textOffsetX) - (2 * _spinWidth) - _textOffsetX;
      }

      if (labelWidth <= 0)
      {
        base.Render(timePassed);
        return;
      }
      _labelControl.Width = labelWidth;

      RenderButtonLabel(timePassed, labelWidth);

      // Render the spin control.
      int x = 0;
      int y = 0;

      switch (_spinAlignment)
      {
        case Alignment.ALIGN_LEFT:
          x = _spinOffsetX + _positionX;
          break;

        case Alignment.ALIGN_RIGHT:
          x = _positionX + _width - _spinOffsetX - _spinControl.Width;
          break;

        case Alignment.ALIGN_CENTER:
          x = _positionX + ((_width / 2) - (_spinControl.Width / 2));
          break;
      }

      switch (_spinVAlignment)
      {
        case VAlignment.ALIGN_TOP:
          y = _spinOffsetY + _positionY;
          break;

        case VAlignment.ALIGN_BOTTOM:
          y = _positionY + _height - _spinOffsetY - _spinControl.Height;
          break;

        case VAlignment.ALIGN_MIDDLE:
          y = _positionY + ((_height / 2) - (_spinControl.Height / 2));
          break;
      }

      _spinControl.TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
      _spinControl.SetPosition(x, y);
      _spinControl.Render(timePassed);
    }

    /// <summary>
    /// Renders the button label.
    /// </summary>
    private void RenderButtonLabel(float timePassed, int labelWidth)
    {
      string labelText = _label;
      string valueText = "";

      if (_valueTextInButton)
      {
        if (_isRangeSet)
        {
          valueText = SelectedItemValue.ToString();
        }
        else
        {
          valueText = SelectedItemLabel;
        }

        if (_prefixText.Length > 0)
        {
          valueText = _prefixText + valueText;
        }

        if (_suffixText.Length > 0)
        {
          valueText = valueText + _suffixText;
        }

        if (_valueTextInButtonAlignment == Alignment.ALIGN_LEFT)
        {
          labelText += valueText;
        }
      }

      // Render the button label text on the button
      if (_labelControl is GUILabelControl)
      {
        ((GUILabelControl)_labelControl).TextAlignment = _textAlignment;
        ((GUILabelControl)_labelControl).TextVAlignment = _textVAlignment;
        ((GUILabelControl)_labelControl).Label = labelText;
        ((GUILabelControl)_labelControl).TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
      }
      else
      {
        ((GUIFadeLabel)_labelControl).TextAlignment = _textAlignment;
        ((GUIFadeLabel)_labelControl).TextVAlignment = _textVAlignment;
        ((GUIFadeLabel)_labelControl).Label = labelText;
        ((GUIFadeLabel)_labelControl).TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
      }

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
          x = _positionX + ((_width / 2) - (labelWidth / 2));
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

      // Render the button value text on the button separately is required
      if (_valueTextInButtonAlignment == Alignment.ALIGN_RIGHT)
      {
        if (_valueLabelControl is GUILabelControl)
        {
          ((GUILabelControl)_valueLabelControl).TextAlignment = Alignment.ALIGN_RIGHT;
          ((GUILabelControl)_valueLabelControl).TextVAlignment = _textVAlignment;
          ((GUILabelControl)_valueLabelControl).Label = valueText;
          ((GUILabelControl)_valueLabelControl).TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
        }
        else
        {
          ((GUIFadeLabel)_valueLabelControl).TextAlignment = Alignment.ALIGN_RIGHT;
          ((GUIFadeLabel)_valueLabelControl).TextVAlignment = _textVAlignment;
          ((GUIFadeLabel)_valueLabelControl).Label = valueText;
          ((GUIFadeLabel)_valueLabelControl).TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;
        }

        // X position forced to the right.
        x = _positionX + _width - _textOffsetX;

        _valueLabelControl.SetPosition(x, y);
        _valueLabelControl.Render(timePassed);
      }
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

    /// <summary>
    /// Refreshes the control.
    /// </summary>
    public void Refresh()
    {
      Update();
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
      if (Focus)
      {
        // The order of execution in this section is important.  The action is handled first, then properties are set to reflect
        // state of the button, then click actions are handled.  This allows click actions to consume the buttons new state.

        // Preserve the action for processing after delegation.
        Action.ActionType myAction = action.wID;

        // Perform actions appropriate for the button mode.
        switch (_buttonMode)
        {
          case ButtonMode.BUTTON_MODE_DIALOG_LIST:
            PerformDialogListButtonAction(action);
            break;
          case ButtonMode.BUTTON_MODE_SPIN_LIST:
            PerformSpinListButtonAction(action);
            break;
        }

        // Set properties for the selection.
        SetProperties();

        if (myAction == Action.ActionType.ACTION_MOUSE_CLICK || myAction == Action.ActionType.ACTION_SELECT_ITEM)
        {
          // send a message to anyone interested 
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);

          // If this button has a click setting then execute the setting.
          if (_onclick.Length != 0)
          {
            GUIPropertyManager.Parse(_onclick, GUIExpressionManager.ExpressionOptions.EVALUATE_ALWAYS);
          }
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
      // Let the spin control handle messages first
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          if (_spinControl.OnMessage(message))
          {
            return true;
          }
          break;
      }

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

    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#selectedlabel" + GetID, SelectedItemLabel);
      GUIPropertyManager.SetProperty("#selectedvalue" + GetID, SelectedItemValue.ToString());
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
    }

    /// <summary>
    /// Get/set the color of the text when the GUIMenuButton is disabled.
    /// </summary>
    public long DisabledColor
    {
      get { return _disabledColor; }
      set { _disabledColor = value; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIMenuButton does not have the focus.
    /// </summary>
    public string TextureNoFocusName
    {
      get { return _imageNonFocused.FileName; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIMenuButton has the focus.
    /// </summary>
    public string TextureFocusName
    {
      get { return _imageFocused.FileName; }
    }

    /// <summary>
    /// Set the color of the text on the GUIMenuButton. 
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/set the name of the font of the text of the GUIMenuButton.
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
    /// Adds a new menu entry composed of a label and integer value.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="value"></param>
    public void AddItem(string label, int value)
    {
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:

          // Hold the diaog labels and values locally.
          if (label != null)
          {
            _listMenuLabels.Add(label);
            _listMenuValues.Add(value);
          }
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          AddSpinLabel(label, value);
          break;
      }
    }

    /// <summary>
    /// Adds a range of integer value menu entries.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void AddItemRange(int start, int end)
    {
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:

          for (int i = start; i <= end; i++)
          {
            _listMenuLabels.Add(i);
            _listMenuValues.Add(i);
          }
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          SetSpinRange(start, end);
          break;
      }
      _isRangeSet = true;
    }

    /// <summary>
    /// Returns the number of menu entries.
    /// </summary>
    /// <returns></returns>
    public int GetItemCount()
    {
      int returnVal = 0;
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          returnVal = _listMenuLabels.Count;
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          returnVal = _spinControl.GetItemCount();
          break;
      }
      return returnVal;
    }

    /// <summary>
    /// Remove all of the menu entries.
    /// </summary>
    public void ClearMenu()
    {
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          _listMenuLabels.Clear();
          _listMenuValues.Clear();
          _listSelected = 0;
          _isRangeSet = false;
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          ClearSpinLabels();
          break;
      }
    }

    /// <summary>
    /// Gets and sets the label of the selected menu item.
    /// </summary>
    public string SelectedItemLabel
    {
      get
      {
        string returnVal = "";
        switch (_buttonMode)
        {
          case ButtonMode.BUTTON_MODE_DIALOG_LIST:
            if (_listSelected < 0 || _listSelected >= _listMenuLabels.Count)
            {
              returnVal = "";
            }
            else
            {
              returnVal = _listMenuLabels[_listSelected] + "";
            }
            break;
          case ButtonMode.BUTTON_MODE_SPIN_LIST:
            returnVal = SpinLabel + "";
            break;
        }
        return returnVal;
      }
    }

    /// <summary>
    /// Gets and sets the value of the selected menu item.
    /// </summary>
    public int SelectedItemValue
    {
      get
      {
        int returnVal = 0;
        switch (_buttonMode)
        {
          case ButtonMode.BUTTON_MODE_DIALOG_LIST:
            if (_listSelected < 0 || _listSelected >= _listMenuValues.Count)
            {
              returnVal = 0;
            }
            else
            {
              returnVal = (int)_listMenuValues[_listSelected];
            }
            break;
          case ButtonMode.BUTTON_MODE_SPIN_LIST:
            returnVal = SpinValue;
            break;
        }
        return returnVal;
      }
    }

    /// <summary>
    /// Gets and sets the selected menu item by menu entry index.
    /// </summary>
    public override int SelectedItem
    {
      get
      {
        int returnVal = 0;
        switch (_buttonMode)
        {
          case ButtonMode.BUTTON_MODE_DIALOG_LIST:
            returnVal = _listSelected;
            break;
          case ButtonMode.BUTTON_MODE_SPIN_LIST:
            returnVal = SpinValue;
            break;
        }
        return returnVal;
      }
      set
      {
        switch (_buttonMode)
        {
          case ButtonMode.BUTTON_MODE_DIALOG_LIST:

            if (_isRangeSet)
            {
              // A range of values has been set, the index in the list is offset by the start value (the first entry in the list).
              _listSelected = value - (int)_listMenuValues[0];
            }
            else
            {
              _listSelected = value;
            }
            // Avoid exceptions; protect the index from out of bound errors.
            if (_listSelected < 0 || _listSelected >= _listMenuValues.Count)
            {
              _listSelected = 0;
            }

            break;
          case ButtonMode.BUTTON_MODE_SPIN_LIST:
            SpinValue = value;
            break;
        }
      }
    }

    private int FindItemByLabel(string label)
    {
      int item = 0;

      // Find the menu item with the specified label.
      // This algorithm chooses the first match.  It's a programming error to have to menu entries with the same label.
      for (int i = 0; i < _listMenuLabels.Count; i++)
      {
        if ((string)_listMenuLabels[i] == label)
        {
          item = i;
          break;
        }
      }
      return item;
    }

    private int FindItemByValue(int value)
    {
      int item = 0;

      // Find the menu item with the specified value.
      // This algorithm chooses the first match.  It's a programming error to have to menu entries with the same value.
      for (int i = 0; i < _listMenuValues.Count; i++)
      {
        if ((int)_listMenuValues[i] == value)
        {
          item = i;
          break;
        }
      }
      return item;
    }

    /// <summary>
    /// Set the selected menu item entry by finding the entry 
    /// </summary>
    /// <param name="value"></param>
    public void SetSelectedItemByValue(int value)
    {
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          _listSelected = FindItemByValue(value);
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          SpinValue = _spinControl.FindItemByValue(value);
          break;
      }
    }

    public void SetSelectedItemByLabel(string label)
    {
      switch (_buttonMode)
      {
        case ButtonMode.BUTTON_MODE_DIALOG_LIST:
          _listSelected = FindItemByLabel(label);
          break;
        case ButtonMode.BUTTON_MODE_SPIN_LIST:
          SpinValue = _spinControl.FindItemByLabel(label);
          break;
      }
    }

    /// <summary>
    /// Set the text of the GUIMenuButton. 
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
    /// Get/set the text of the GUIMenuButton.
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
    /// Set the text horizontal alignment in the button.
    /// </summary>
    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    /// <summary>
    /// Set the text vertical alignment in the button.
    /// </summary>
    public VAlignment TextVAlignment
    {
      get { return _textVAlignment; }
      set { _textVAlignment = value; }
    }

    /// <summary>
    /// Set the color to use when the control is dimmed.
    /// </summary>
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
        if (_spinControl != null)
        {
          _spinControl.DimColor = value;
        }
      }
    }

    /// <summary>
    /// Set the border for the focused texture.
    /// </summary>
    /// <param name="border"></param>
    /// <param name="position"></param>
    /// <param name="repeat"></param>
    /// <param name="rotate"></param>
    /// <param name="texture"></param>
    /// <param name="colorKey"></param>
    /// <param name="hasCorners"></param>
    /// <param name="cornerRotate"></param>
    public void SetBorderTF(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
      string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTF = border;
      _borderPositionTF = position;
      _borderTextureRepeatTF = repeat;
      _borderTextureRotateTF = rotate;
      _borderTextureFileNameTF = texture;
      _borderColorKeyTF = colorKey;
      _borderHasCornersTF = hasCorners;
      _borderCornerTextureRotateTF = cornerRotate;
      _imageFocused.SetBorder(_strBorderTF, _borderPositionTF, _borderTextureRepeatTF, _borderTextureRotateTF,
        _borderTextureFileNameTF, _borderColorKeyTF, _borderHasCornersTF, _borderCornerTextureRotateTF);
    }

    /// <summary>
    /// Set the border for the non-focused texture.
    /// </summary>
    /// <param name="border"></param>
    /// <param name="position"></param>
    /// <param name="repeat"></param>
    /// <param name="rotate"></param>
    /// <param name="texture"></param>
    /// <param name="colorKey"></param>
    /// <param name="hasCorners"></param>
    /// <param name="cornerRotate"></param>
    public void SetBorderTNF(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
      string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTNF = border;
      _borderPositionTNF = position;
      _borderTextureRepeatTNF = repeat;
      _borderTextureRotateTNF = rotate;
      _borderTextureFileNameTNF = texture;
      _borderColorKeyTNF = colorKey;
      _borderHasCornersTNF = hasCorners;
      _borderCornerTextureRotateTNF = cornerRotate;
      _imageNonFocused.SetBorder(_strBorderTNF, _borderPositionTNF, _borderTextureRepeatTNF, _borderTextureRotateTNF,
        _borderTextureFileNameTNF, _borderColorKeyTNF, _borderHasCornersTNF, _borderCornerTextureRotateTNF);
    }

    /// <summary>
    /// Set the border for the hover image.
    /// </summary>
    /// <param name="border"></param>
    /// <param name="position"></param>
    /// <param name="repeat"></param>
    /// <param name="rotate"></param>
    /// <param name="texture"></param>
    /// <param name="colorKey"></param>
    /// <param name="hasCorners"></param>
    /// <param name="cornerRotate"></param>
    public void SetBorderH(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
      string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderH = border;
      _borderPositionH = position;
      _borderTextureRepeatH = repeat;
      _borderTextureRotateH = rotate;
      _borderTextureFileNameH = texture;
      _borderColorKeyH = colorKey;
      _borderHasCornersH = hasCorners;
      _borderCornerTextureRotateH = cornerRotate;
      _hoverImage.SetBorder(_strBorderH, _borderPositionH, _borderTextureRepeatH, _borderTextureRotateH,
        _borderTextureFileNameH, _borderColorKeyH, _borderHasCornersH, _borderCornerTextureRotateH);
    }

    /// <summary>
    /// Gets and sets the focused texture tile fill setting.
    /// </summary>
    public bool TileFillTF
    {
      get { return _imageFocused.TileFill; }
      set { _imageFocused.TileFill = value; }
    }

    /// <summary>
    /// Gets and sets the non-focused texture tile fill setting.
    /// </summary>
    public bool TileFillTNF
    {
      get { return _imageNonFocused.TileFill; }
      set { _imageNonFocused.TileFill = value; }
    }

    /// <summary>
    /// Gets and sets the hover image tile fill setting.
    /// </summary>
    public bool TileFillH
    {
      get { return _hoverImage.TileFill; }
      set { _hoverImage.TileFill = value; }
    }

    /// <summary>
    /// Sets the texture mask for the focused texture.
    /// </summary>
    /// <param name="mask"></param>
    public void SetTextureMaskTF(string mask)
    {
      if (null != _imageFocused)
      {
        _strMaskTF = mask;
        _imageFocused.MaskFileName = _strMaskTF;
      }
    }

    /// <summary>
    /// Sets the texture mask for the non-focused texture.
    /// </summary>
    /// <param name="mask"></param>
    public void SetTextureMaskNF(string mask)
    {
      if (null != _imageNonFocused)
      {
        _strMaskTNF = mask;
        _imageNonFocused.MaskFileName = _strMaskTNF;
      }
    }

    /// <summary>
    /// Sets the texture mask for the hover image.
    /// </summary>
    /// <param name="mask"></param>
    public void SetTextureMaskH(string mask)
    {
      if (null != _hoverImage)
      {
        _strMaskH = mask;
        _hoverImage.MaskFileName = _strMaskH;
      }
    }

    #region SpinListMethods

    private void PerformSpinListButtonAction(Action action)
    {
      // Allow the spin control to handle actions first.
      _spinControl.OnAction(action);

      // If the spin control handled the action then avoid the base action handler.
      // In particular this avoids the move up,down,left,right actions from leaving this control too soon.
      if (!_spinControl.ActionHandled)
      {
        base.OnAction(action);
      }
    }

    private void AddSpinLabel(string label, int value)
    {
      _spinControl.AddLabel(label, value);
    }

    private int SpinValue
    {
      get
      {
        return _spinControl.Value;
      }
      set
      {
        _spinControl.Value = value;
      }
    }

    private void ClearSpinLabels()
    {
      _spinControl.Reset();
    }

    private string SpinLabel
    {
      get
      {
        return _spinControl.GetLabel();
      }
    }

    private void SetSpinRange(int iStart, int iEnd)
    {
      _spinControl.SetRange(iStart, iEnd);
    }

    private int SpinMaxValue()
    {
      return _spinControl.GetMaximum();
    }

    private int SpinMinValue()
    {
      return _spinControl.GetMinimum();
    }

    /// <summary>
    /// Offsets the default X position of the spin control in the button.
    /// </summary>
    public int SpinOffsetX
    {
      get { return _spinOffsetX; }
      set { _spinOffsetX = value; }
    }

    /// <summary>
    /// Offsets the default Y position of the spin control in the button.
    /// </summary>
    public int SpinOffsetY
    {
      get { return _spinOffsetY; }
      set { _spinOffsetY = value; }
    }

    /// <summary>
    /// The vertical alignment of the spin control in the button.
    /// </summary>
    public VAlignment SpinVAlignment
    {
      get { return _spinVAlignment; }
      set { _spinVAlignment = value; }
    }

    #endregion

    #region DialogListMethods

    private void PerformDialogListButtonAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
      {
        // Construct and present the menu as a dialog window.
        IDialogbox dialogItemSelect = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dialogItemSelect != null)
        {
          dialogItemSelect.Reset();
          dialogItemSelect.ShowQuickNumbers = _dialogShowNumbers;
          dialogItemSelect.SetHeading(_dialogTitle);

          for (int i=0; i < _listMenuLabels.Count; i++)
          {
            dialogItemSelect.Add(_listMenuLabels[i] + ""); // Convert to string since menu item may not be a string (e.g., int).
            if (_listMenuLabels[_listSelected] == _listMenuLabels[i])
            {
              dialogItemSelect.SelectOption("" + (_listSelected + 1));  // The dialog list is 1-based, our array is 0-based.
            }
          }
          dialogItemSelect.DoModal(ParentID);

          // Set the list index based on the menu selection.
          if (dialogItemSelect.SelectedId >= 1)
          {
            _listSelected = dialogItemSelect.SelectedId - 1;  // The dialog list is 1-based, our array is 0-based.

            // Send a message to anyone interested in knowing the selected value.
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, ParentID, 0, this.GetID, _listSelected, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
        }
      }

      base.OnAction(action);
    }

    #endregion

  }
}
