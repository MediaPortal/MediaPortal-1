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

using System.Diagnostics;
using System.IO;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class implementing a button which consists of 3 parts
  /// a left part, a middle part and a right part
  /// These are presented as [ Left Middle Right ]
  /// Each part has 2 images, 
  /// 1 for the normal state
  /// and 1 for the focused state
  /// Further the button can have an image (icon) which can be positioned 
  /// 
  /// </summary>
  public class GUIButton3PartControl : GUIControl
  {
    //TODO: make use of GUILabelControl to draw all text
    [XMLSkinElement("textureFocusedLeft")] protected string _textureFocusedLeft;
    [XMLSkinElement("textureNonFocusedLeft")] protected string _textureNonFocusedLeft;
    [XMLSkinElement("textureFocusedMid")] protected string _textureFocusedMid;
    [XMLSkinElement("textureNonFocusedMid")] protected string _textureNonFocusedMid;
    [XMLSkinElement("textureFocusedRight")] protected string _textureFocusedRight;
    [XMLSkinElement("textureNonFocusedRight")] protected string _textureNonFocusedRight;
    [XMLSkinElement("textureIcon")] protected string _textureIcon;
    [XMLSkinElement("textureIcon2")] protected string _textureIcon2;
    protected GUIImage _imageNonFocusedMid = null;
    protected GUIImage _imageFocusedRight = null;
    protected GUIImage _imageNonFocusedRight = null;
    protected GUIImage _imageFocusedLeft = null;
    protected GUIImage _imageNonFocusedLeft = null;
    protected GUIImage _imageFocusedMid = null;
    protected GUIImage _imageIcon = null;
    protected GUIImage _imageIcon2 = null;
    [XMLSkinElement("label1")] protected string _tagLabel1 = "";
    [XMLSkinElement("label2")] protected string _tagLabel2 = "";
    [XMLSkinElement("font1")] protected string _fontName1 = string.Empty;
    [XMLSkinElement("font2")] protected string _fontName2 = string.Empty;
    [XMLSkinElement("textcolor1")] protected long _textColor1 = (long)0xFFFFFFFF;
    [XMLSkinElement("textcolor2")] protected long _textColor2 = (long)0xFFFFFFFF;
    [XMLSkinElement("disabledColor")] protected long _disabledColor = (long)0xFF606060;
    protected int _hyperLinkWindowId = -1;
    protected int _actionId = -1;
    protected string _scriptAction = "";
    [XMLSkinElement("onclick")] protected string _onclick = "";
    [XMLSkinElement("textOffsetX1")] protected int _textOffsetX1 = 10;
    [XMLSkinElement("textOffsetY1")] protected int _textOffsetY1 = 2;
    [XMLSkinElement("textOffsetX2")] protected int _textOffsetX2 = 10;
    [XMLSkinElement("textOffsetY2")] protected int _textOffsetY2 = 2;
    protected string _cachedTextLabel1;
    protected string _cachedTextLabel2;
    protected string _application = "";
    protected string _arguments = "";
    [XMLSkinElement("iconOffsetX")] protected int _iconOffsetX = -1;
    [XMLSkinElement("iconOffsetY")] protected int _iconOffsetY = -1;
    [XMLSkinElement("iconWidth")] protected int _iconWidth = -1;
    [XMLSkinElement("iconHeight")] protected int _iconHeight = -1;
    [XMLSkinElement("iconKeepAspectRatio")] protected bool _iconKeepAspectRatio = false;
    [XMLSkinElement("iconCentered")] protected bool _iconCentered = false;
    [XMLSkinElement("iconZoomed")] protected bool _iconZoomed = false;
    [XMLSkinElement("iconAlign")] protected Alignment _iconAlign = Alignment.ALIGN_LEFT;
    [XMLSkinElement("iconVAlign")] protected VAlignment _iconVAlign = VAlignment.ALIGN_TOP;
    [XMLSkinElement("iconInlineLabel1")] protected bool _iconInlineLabel1 = false;
    [XMLSkinElement("icon2OffsetX")] protected int _icon2OffsetX = -1;
    [XMLSkinElement("icon2OffsetY")] protected int _icon2OffsetY = -1;
    [XMLSkinElement("icon2Width")] protected int _icon2Width = -1;
    [XMLSkinElement("icon2Height")] protected int _icon2Height = -1;
    [XMLSkinElement("icon2KeepAspectRatio")] protected bool _icon2KeepAspectRatio = false;
    [XMLSkinElement("icon2Centered")] protected bool _icon2Centered = false;
    [XMLSkinElement("icon2Zoomed")] protected bool _icon2Zoomed = false;
    [XMLSkinElement("icon2Align")] protected Alignment _icon2Align = Alignment.ALIGN_LEFT;
    [XMLSkinElement("icon2VAlign")] protected VAlignment _icon2VAlign = VAlignment.ALIGN_TOP;
    [XMLSkinElement("icon2InlineLabel1")] protected bool _icon2InlineLabel1 = false;
    [XMLSkinElement("shadowAngle1")] protected int _shadowAngle1 = 0;
    [XMLSkinElement("shadowDistance1")] protected int _shadowDistance1 = 0;
    [XMLSkinElement("shadowColor1")] protected long _shadowColor1 = 0xFF000000;
    [XMLSkinElement("shadowAngle2")] protected int _shadowAngle2 = 0;
    [XMLSkinElement("shadowDistance2")] protected int _shadowDistance2 = 0;
    [XMLSkinElement("shadowColor2")] protected long _shadowColor2 = 0xFF000000;

    [XMLSkin("textureFocusedLeft", "border")] protected string _strBorderTFL = "";
    [XMLSkin("textureFocusedLeft", "position")] protected GUIImage.BorderPosition _borderPositionTFL = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureFocusedLeft", "textureRepeat")] protected bool _borderTextureRepeatTFL = false;
    [XMLSkin("textureFocusedLeft", "textureRotate")] protected bool _borderTextureRotateTFL = false;
    [XMLSkin("textureFocusedLeft", "texture")] protected string _borderTextureFileNameTFL = "image_border.png";
    [XMLSkin("textureFocusedLeft", "colorKey")] protected long _borderColorKeyTFL = 0xFFFFFFFF;
    [XMLSkin("textureFocusedLeft", "corners")] protected bool _borderHasCornersTFL = false;
    [XMLSkin("textureFocusedLeft", "cornerRotate")] protected bool _borderCornerTextureRotateTFL = true;
    [XMLSkin("textureFocusedLeft", "tileFill")] protected bool _textureFocusedLeftTileFill = false;
    [XMLSkin("textureFocusedLeft", "overlay")] protected string _overlayTFL = "";
    [XMLSkin("textureFocusedLeft", "colordiffuse")] protected long _diffuseColorTFL = 0xFFFFFFFF;

    [XMLSkin("textureNonFocusedLeft", "border")] protected string _strBorderTNFL = "";
    [XMLSkin("textureNonFocusedLeft", "position")] protected GUIImage.BorderPosition _borderPositionTNFL = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureNonFocusedLeft", "textureRepeat")] protected bool _borderTextureRepeatTNFL = false;
    [XMLSkin("textureNonFocusedLeft", "textureRotate")] protected bool _borderTextureRotateTNFL = false;
    [XMLSkin("textureNonFocusedLeft", "texture")] protected string _borderTextureFileNameTNFL = "image_border.png";
    [XMLSkin("textureNonFocusedLeft", "colorKey")] protected long _borderColorKeyTNFL = 0xFFFFFFFF;
    [XMLSkin("textureNonFocusedLeft", "corners")] protected bool _borderHasCornersTNFL = false;
    [XMLSkin("textureNonFocusedLeft", "cornerRotate")] protected bool _borderCornerTextureRotateTNFL = true;
    [XMLSkin("textureNonFocusedLeft", "tileFill")] protected bool _textureNonFocusedLeftTileFill = false;
    [XMLSkin("textureNonFocusedLeft", "overlay")] protected string _overlayTNFL = "";
    [XMLSkin("textureNonFocusedLeft", "colordiffuse")] protected long _diffuseColorTNFL = 0xFFFFFFFF;

    [XMLSkin("textureFocusedMid", "border")] protected string _strBorderTFM = "";
    [XMLSkin("textureFocusedMid", "position")] protected GUIImage.BorderPosition _borderPositionTFM = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureFocusedMid", "textureRepeat")] protected bool _borderTextureRepeatTFM = false;
    [XMLSkin("textureFocusedMid", "textureRotate")] protected bool _borderTextureRotateTFM = false;
    [XMLSkin("textureFocusedMid", "texture")] protected string _borderTextureFileNameTFM = "image_border.png";
    [XMLSkin("textureFocusedMid", "colorKey")] protected long _borderColorKeyTFM = 0xFFFFFFFF;
    [XMLSkin("textureFocusedMid", "corners")] protected bool _borderHasCornersTFM = false;
    [XMLSkin("textureFocusedMid", "cornerRotate")] protected bool _borderCornerTextureRotateTFM = true;
    [XMLSkin("textureFocusedMid", "tileFill")] protected bool _textureFocusedMidTileFill = false;
    [XMLSkin("textureFocusedMid", "overlay")] protected string _overlayTFM = "";
    [XMLSkin("textureFocusedMid", "colordiffuse")] protected long _diffuseColorTFM = 0xFFFFFFFF;

    [XMLSkin("textureNonFocusedMid", "border")] protected string _strBorderTNFM = "";
    [XMLSkin("textureNonFocusedMid", "position")] protected GUIImage.BorderPosition _borderPositionTNFM = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureNonFocusedMid", "textureRepeat")] protected bool _borderTextureRepeatTNFM = false;
    [XMLSkin("textureNonFocusedMid", "textureRotate")] protected bool _borderTextureRotateTNFM = false;
    [XMLSkin("textureNonFocusedMid", "texture")] protected string _borderTextureFileNameTNFM = "image_border.png";
    [XMLSkin("textureNonFocusedMid", "colorKey")] protected long _borderColorKeyTNFM = 0xFFFFFFFF;
    [XMLSkin("textureNonFocusedMid", "corners")] protected bool _borderHasCornersTNFM = false;
    [XMLSkin("textureNonFocusedMid", "cornerRotate")] protected bool _borderCornerTextureRotateTNFM = true;
    [XMLSkin("textureNonFocusedMid", "tileFill")] protected bool _textureNonFocusedMidTileFill = false;
    [XMLSkin("textureNonFocusedMid", "overlay")] protected string _overlayTNFM = "";
    [XMLSkin("textureNonFocusedMid", "colordiffuse")] protected long _diffuseColorTNFM = 0xFFFFFFFF;

    [XMLSkin("textureFocusedRight", "border")] protected string _strBorderTFR = "";
    [XMLSkin("textureFocusedRight", "position")] protected GUIImage.BorderPosition _borderPositionTFR = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureFocusedRight", "textureRepeat")] protected bool _borderTextureRepeatTFR = false;
    [XMLSkin("textureFocusedRight", "textureRotate")] protected bool _borderTextureRotateTFR = false;
    [XMLSkin("textureFocusedRight", "texture")] protected string _borderTextureFileNameTFR = "image_border.png";
    [XMLSkin("textureFocusedRight", "colorKey")] protected long _borderColorKeyTFR = 0xFFFFFFFF;
    [XMLSkin("textureFocusedRight", "corners")] protected bool _borderHasCornersTFR = false;
    [XMLSkin("textureFocusedRight", "cornerRotate")] protected bool _borderCornerTextureRotateTFR = true;
    [XMLSkin("textureFocusedRight", "tileFill")] protected bool _textureFocusedRightTileFill = false;
    [XMLSkin("textureFocusedRight", "overlay")] protected string _overlayTFR = "";
    [XMLSkin("textureFocusedRight", "colordiffuse")] protected long _diffuseColorTFR = 0xFFFFFFFF;

    [XMLSkin("textureNonFocusedRight", "border")] protected string _strBorderTNFR = "";
    [XMLSkin("textureNonFocusedRight", "position")] protected GUIImage.BorderPosition _borderPositionTNFR = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureNonFocusedRight", "textureRepeat")] protected bool _borderTextureRepeatTNFR = false;
    [XMLSkin("textureNonFocusedRight", "textureRotate")] protected bool _borderTextureRotateTNFR = false;
    [XMLSkin("textureNonFocusedRight", "texture")] protected string _borderTextureFileNameTNFR = "image_border.png";
    [XMLSkin("textureNonFocusedRight", "colorKey")] protected long _borderColorKeyTNFR = 0xFFFFFFFF;
    [XMLSkin("textureNonFocusedRight", "corners")] protected bool _borderHasCornersTNFR = false;
    [XMLSkin("textureNonFocusedRight", "cornerRotate")] protected bool _borderCornerTextureRotateTNFR = true;
    [XMLSkin("textureNonFocusedRight", "tileFill")] protected bool _textureNonFocusedRightTileFill = false;
    [XMLSkin("textureNonFocusedRight", "overlay")] protected string _overlayTNFR = "";
    [XMLSkin("textureNonFocusedRight", "colordiffuse")] protected long _diffuseColorTNFR = 0xFFFFFFFF;

    [XMLSkin("textureIcon", "border")] protected string _strBorderTI = "";
    [XMLSkin("textureIcon", "position")] protected GUIImage.BorderPosition _borderPositionTI = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureIcon", "textureRepeat")] protected bool _borderTextureRepeatTI = false;
    [XMLSkin("textureIcon", "textureRotate")] protected bool _borderTextureRotateTI = false;
    [XMLSkin("textureIcon", "texture")] protected string _borderTextureFileNameTI = "image_border.png";
    [XMLSkin("textureIcon", "colorKey")] protected long _borderColorKeyTI = 0xFFFFFFFF;
    [XMLSkin("textureIcon", "corners")] protected bool _borderHasCornersTI = false;
    [XMLSkin("textureIcon", "cornerRotate")] protected bool _borderCornerTextureRotateTI = true;

    [XMLSkin("textureIcon2", "border")] protected string _strBorderTI2 = "";
    [XMLSkin("textureIcon2", "position")] protected GUIImage.BorderPosition _borderPositionTI2 = GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("textureIcon2", "textureRepeat")] protected bool _borderTextureRepeatTI2 = false;
    [XMLSkin("textureIcon2", "textureRotate")] protected bool _borderTextureRotateTI2 = false;
    [XMLSkin("textureIcon2", "texture")] protected string _borderTextureFileNameTI2 = "image_border.png";
    [XMLSkin("textureIcon2", "colorKey")] protected long _borderColorKeyTI2 = 0xFFFFFFFF;
    [XMLSkin("textureIcon2", "corners")] protected bool _borderHasCornersTI2 = false;
    [XMLSkin("textureIcon2", "cornerRotate")] protected bool _borderCornerTextureRotateTI2 = true;

    private GUILabelControl _labelControl1 = null;
    private GUILabelControl _labelControl2 = null;
    private bool _containsProperty1 = false;
    private bool _containsProperty2 = false;
    private bool renderLeftPart = true;
    private bool renderRightPart = true;
    private bool stretchIfNotRendered = false;
    //Sprite                           sprite=null;
    private bool _property1Changed = false;
    private bool _property2Changed = false;
    private bool _reCalculate = false;
    private bool _registeredForEvent = false;

    /// <summary>
    /// empty constructor
    /// </summary>
    public GUIButton3PartControl() {}

    /// <summary>
    /// The basic constructur of the GUIControl class.
    /// </summary>
    public GUIButton3PartControl(int dwParentID)
      : this()
    {
      _parentControlId = dwParentID;
    }

    /// <summary>
    /// The constructor of the GUIButton3PartControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
    /// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
    public GUIButton3PartControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                 string strTextureFocusLeft,
                                 string strTextureFocusMid,
                                 string strTextureFocusRight,
                                 string strTextureNoFocusLeft,
                                 string strTextureNoFocusMid,
                                 string strTextureNoFocusRight,
                                 string strTextureIcon)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _textureFocusedLeft = strTextureFocusLeft;
      _textureFocusedMid = strTextureFocusMid;
      _textureFocusedRight = strTextureFocusRight;
      _textureNonFocusedLeft = strTextureNoFocusLeft;
      _textureNonFocusedMid = strTextureNoFocusMid;
      _textureNonFocusedRight = strTextureNoFocusRight;
      _textureIcon = strTextureIcon;
      _textureIcon2 = strTextureIcon;
      _imageIcon = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, 0, 0, strTextureIcon, 0);
      _imageIcon2 = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, 0, 0, strTextureIcon, 0);
      _imageFocusedLeft = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTextureFocusLeft,
                                       0);
      _imageFocusedMid = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTextureFocusMid, 0);
      _imageFocusedRight = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTextureFocusRight,
                                        0);
      _imageNonFocusedLeft = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight,
                                          strTextureNoFocusLeft, 0);
      _imageNonFocusedMid = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight,
                                         strTextureNoFocusMid, 0);
      _imageNonFocusedRight = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight,
                                           strTextureNoFocusRight, 0);
      _isSelected = false;
      _labelControl1 = new GUILabelControl(dwParentID);
      _labelControl2 = new GUILabelControl(dwParentID);
      _imageIcon.ParentControl = this;
      _imageIcon2.ParentControl = this;
      _imageFocusedLeft.ParentControl = this;
      _imageFocusedMid.ParentControl = this;
      _imageFocusedRight.ParentControl = this;
      _imageNonFocusedLeft.ParentControl = this;
      _imageNonFocusedMid.ParentControl = this;
      _imageNonFocusedRight.ParentControl = this;
      _labelControl1.ParentControl = this;
      _labelControl2.ParentControl = this;

      _imageFocusedLeft.DimColor = DimColor;
      _imageFocusedMid.DimColor = DimColor;
      _imageFocusedRight.DimColor = DimColor;
      _imageNonFocusedLeft.DimColor = DimColor;
      _imageNonFocusedMid.DimColor = DimColor;
      _imageNonFocusedRight.DimColor = DimColor;
      _imageIcon.DimColor = DimColor;
      _imageIcon2.DimColor = DimColor;
      _labelControl1.DimColor = DimColor;
      _labelControl2.DimColor = DimColor;

      _imageFocusedLeft.OverlayFileName = _overlayTFL;
      _imageFocusedMid.OverlayFileName = _overlayTFM;
      _imageFocusedRight.OverlayFileName = _overlayTFR;
      _imageNonFocusedLeft.OverlayFileName = _overlayTNFL;
      _imageNonFocusedMid.OverlayFileName = _overlayTNFM;
      _imageNonFocusedRight.OverlayFileName = _overlayTNFR;

      _imageIcon2.Visible = false; // Constructor creates icon2 as a copy of icon1.
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      _imageIcon = new GUIImage(ParentID, GetID, _positionX, _positionY, 0, 0, _textureIcon, 0);
      _imageIcon2 = new GUIImage(ParentID, GetID, _positionX, _positionY, 0, 0, _textureIcon2, 0);
      _imageFocusedLeft = new GUIImage(ParentID, GetID, _positionX, _positionY, Width, Height, _textureFocusedLeft, 0);
      _imageFocusedMid = new GUIImage(ParentID, GetID, _positionX, _positionY, Width, Height, _textureFocusedMid, 0);
      _imageFocusedRight = new GUIImage(ParentID, GetID, _positionX, _positionY, Width, Height, _textureFocusedRight, 0);
      _imageNonFocusedLeft = new GUIImage(ParentID, GetID, _positionX, _positionY, Width, Height, _textureNonFocusedLeft,
                                          0);
      _imageNonFocusedMid = new GUIImage(ParentID, GetID, _positionX, _positionY, Width, Height, _textureNonFocusedMid,
                                         0);
      _imageNonFocusedRight = new GUIImage(ParentID, GetID, _positionX, _positionY, Width, Height,
                                           _textureNonFocusedRight, 0);
      _isSelected = false;
      _labelControl1 = new GUILabelControl(ParentID);
      _labelControl2 = new GUILabelControl(ParentID);
      _imageIcon.ParentControl = this;
      _imageIcon2.ParentControl = this;
      _imageFocusedLeft.ParentControl = this;
      _imageFocusedMid.ParentControl = this;
      _imageFocusedRight.ParentControl = this;
      _imageNonFocusedLeft.ParentControl = this;
      _imageNonFocusedMid.ParentControl = this;
      _imageNonFocusedRight.ParentControl = this;
      _labelControl1.ParentControl = this;
      _labelControl2.ParentControl = this;

      _imageFocusedLeft.DimColor = DimColor;
      _imageFocusedMid.DimColor = DimColor;
      _imageFocusedRight.DimColor = DimColor;
      _imageNonFocusedLeft.DimColor = DimColor;
      _imageNonFocusedMid.DimColor = DimColor;
      _imageNonFocusedRight.DimColor = DimColor;
      _imageIcon.DimColor = DimColor;
      _imageIcon2.DimColor = DimColor;
      _labelControl1.DimColor = DimColor;
      _labelControl2.DimColor = DimColor;
      _labelControl1.SetShadow(_shadowAngle1, _shadowDistance1, _shadowColor1);
      _labelControl2.SetShadow(_shadowAngle2, _shadowDistance2, _shadowColor2);

      _imageFocusedLeft.SetBorder(_strBorderTFL, _borderPositionTFL, _borderTextureRepeatTFL, _borderTextureRotateTFL,
                                  _borderTextureFileNameTFL, _borderColorKeyTFL, _borderHasCornersTFL,
                                  _borderCornerTextureRotateTFL);

      _imageNonFocusedLeft.SetBorder(_strBorderTNFL, _borderPositionTNFL, _borderTextureRepeatTNFL,
                                     _borderTextureRotateTNFL,
                                     _borderTextureFileNameTNFL, _borderColorKeyTNFL, _borderHasCornersTNFL,
                                     _borderCornerTextureRotateTNFL);

      _imageFocusedMid.SetBorder(_strBorderTFM, _borderPositionTFM, _borderTextureRepeatTFM, _borderTextureRotateTFM,
                                 _borderTextureFileNameTFM, _borderColorKeyTFM, _borderHasCornersTFM,
                                 _borderCornerTextureRotateTFM);

      _imageNonFocusedMid.SetBorder(_strBorderTNFM, _borderPositionTNFM, _borderTextureRepeatTNFM,
                                    _borderTextureRotateTNFM,
                                    _borderTextureFileNameTNFM, _borderColorKeyTNFM, _borderHasCornersTNFM,
                                    _borderCornerTextureRotateTNFM);

      _imageFocusedRight.SetBorder(_strBorderTFR, _borderPositionTFR, _borderTextureRepeatTFR, _borderTextureRotateTFR,
                                   _borderTextureFileNameTFR, _borderColorKeyTFR, _borderHasCornersTFR,
                                   _borderCornerTextureRotateTFR);

      _imageNonFocusedRight.SetBorder(_strBorderTNFR, _borderPositionTNFR, _borderTextureRepeatTNFR,
                                      _borderTextureRotateTNFR,
                                      _borderTextureFileNameTNFR, _borderColorKeyTNFR, _borderHasCornersTNFR,
                                      _borderCornerTextureRotateTNFR);

      _imageIcon.SetBorder(_strBorderTI, _borderPositionTI, _borderTextureRepeatTI, _borderTextureRotateTI,
                           _borderTextureFileNameTI, _borderColorKeyTI, _borderHasCornersTI,
                           _borderCornerTextureRotateTI);

      _imageIcon2.SetBorder(_strBorderTI2, _borderPositionTI2, _borderTextureRepeatTI2, _borderTextureRotateTI2,
                            _borderTextureFileNameTI2, _borderColorKeyTI2, _borderHasCornersTI2,
                            _borderCornerTextureRotateTI2);

      TileFillTFL = _textureFocusedLeftTileFill;
      TileFillTNFL = _textureNonFocusedLeftTileFill;
      TileFillTFM = _textureFocusedMidTileFill;
      TileFillTNFM = _textureNonFocusedMidTileFill;
      TileFillTFR = _textureFocusedRightTileFill;
      TileFillTNFR = _textureNonFocusedRightTileFill;
    }

    /// <summary>
    /// Renders the GUIButton3PartControl.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible.
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }
      _cachedTextLabel1 = _tagLabel1;
      _cachedTextLabel2 = _tagLabel2;
      if (_containsProperty1 && _property1Changed)
      {
        _property1Changed = false;
        _cachedTextLabel1 = GUIPropertyManager.Parse(_tagLabel1);
        if (_cachedTextLabel1 == null)
        {
          _cachedTextLabel1 = "";
        }
        _reCalculate = true;
      }
      if (_containsProperty2 && _property2Changed)
      {
        _property2Changed = false;
        _cachedTextLabel2 = GUIPropertyManager.Parse(_tagLabel2);
        if (_cachedTextLabel2 == null)
        {
          _cachedTextLabel2 = "";
        }
        _reCalculate = true;
      }
      if (_reCalculate)
      {
        Calculate();
      }

      // if the GUIButton3PartControl has the focus
      if (Focus)
      {
        //render the focused images
        //if (_imageIcon!=null) GUIFontManager.Present();//TODO:not nice. but needed for the tvguide

        if (renderLeftPart)
        {
          _imageFocusedLeft.ColourDiffuse = ColourDiffuseTFL;
          _imageFocusedLeft.OverlayFileName = OverlayFileNameTFL;
          _imageFocusedLeft.Render(timePassed);
        }

        _imageFocusedMid.ColourDiffuse = ColourDiffuseTFM;
        _imageFocusedMid.OverlayFileName = OverlayFileNameTFM;
        _imageFocusedMid.Render(timePassed);

        if (renderRightPart)
        {
          _imageFocusedRight.ColourDiffuse = ColourDiffuseTFR;
          _imageFocusedRight.OverlayFileName = OverlayFileNameTFR;
          _imageFocusedRight.Render(timePassed);
        }
        GUIPropertyManager.SetProperty("#highlightedbutton", _cachedTextLabel1);
      }
      else
      {
        //else render the non-focus images
        //if (_imageIcon!=null) GUIFontManager.Present();//TODO:not nice. but needed for the tvguide

        if (renderLeftPart)
        {
          _imageNonFocusedLeft.ColourDiffuse = ColourDiffuseTNFL;
          _imageNonFocusedLeft.OverlayFileName = OverlayFileNameTNFL;
          _imageNonFocusedLeft.Render(timePassed);
        }

        _imageNonFocusedMid.ColourDiffuse = ColourDiffuseTNFM;
        _imageNonFocusedMid.OverlayFileName = OverlayFileNameTNFM;
        _imageNonFocusedMid.Render(timePassed);

        if (renderRightPart)
        {
          _imageNonFocusedRight.ColourDiffuse = ColourDiffuseTNFR;
          _imageNonFocusedRight.OverlayFileName = OverlayFileNameTNFR;
          _imageNonFocusedRight.Render(timePassed);
        }
      }

      // render the 1st line of text on the button
      int iWidth = _imageNonFocusedMid.Width - 10 - _textOffsetX1;

      // Shorten the text width so the text does not overlap a right aligned or inlined icon.
      if (_imageIcon != null && (_iconAlign == Alignment.ALIGN_RIGHT) || IconInlineLabel1)
      {
        iWidth -= (_imageIcon.Width + IconOffsetX);
      }
      if (_imageIcon2 != null && (_icon2Align == Alignment.ALIGN_RIGHT) || Icon2InlineLabel1)
      {
        iWidth -= (_imageIcon2.Width + Icon2OffsetX);
      }

      // render the 1st line of text on the button
      if (_imageNonFocusedMid.IsVisible && _cachedTextLabel1.Length > 0)
      {
        int widthLeft = 0;
        if (RenderLeft)
        {
          widthLeft = (int)((float)_imageFocusedLeft.TextureWidth * ((float)_height / (float)_imageFocusedLeft.TextureHeight));
        }
        int xoff = _textOffsetX1 + widthLeft;

        if (Disabled)
        {
          _labelControl1.TextColor = _disabledColor;
        }
        else
        {
          _labelControl1.TextColor = _textColor1;
        }
        _labelControl1.SetPosition(xoff + _positionX, _textOffsetY1 + _positionY);
        _labelControl1.TextAlignment = Alignment.ALIGN_LEFT;
        _labelControl1.FontName = _fontName1;
        _labelControl1.Label = _cachedTextLabel1;

        // Before rendering the label determine whether or not an icon is positioned inline.  If we have an inline icon
        // then determine whether we have enough room in the button image to render the icon without shortening the text.
        // If there is enough room then do not shorten the text, otherwise shorten the text.
        if (_imageIcon != null && IconInlineLabel1)
        {
          if (_labelControl1.TextWidth + IconOffsetX + IconWidth > iWidth)
          {
            iWidth -= (IconOffsetX + IconWidth);
          }
        }
        if (_imageIcon2 != null && Icon2InlineLabel1)
        {
          if (_labelControl1.TextWidth + Icon2OffsetX + Icon2Width > iWidth)
          {
            iWidth -= (Icon2OffsetX + Icon2Width);
          }
        }
        if (iWidth <= 0)
        {
          iWidth = 1;
        }

        _labelControl1.Width = iWidth;
        _labelControl1.Render(timePassed);
      }

      // render the 2nd line of text on the button
      if (_imageNonFocusedMid.IsVisible && _cachedTextLabel2.Length > 0)
      {
        int widthLeft =
          (int)((float)_imageFocusedLeft.TextureWidth * ((float)_height / (float)_imageFocusedLeft.TextureHeight));
        int xoff = _textOffsetX2 + widthLeft;

        if (Disabled)
        {
          _labelControl2.TextColor = _disabledColor;
        }
        else
        {
          _labelControl2.TextColor = _textColor2;
        }
        _labelControl2.SetPosition(xoff + _positionX, _textOffsetY2 + _positionY);
        _labelControl2.TextAlignment = Alignment.ALIGN_LEFT;
        _labelControl2.FontName = _fontName1;
        _labelControl2.Label = _cachedTextLabel2;
        _labelControl2.Width = iWidth - 10;
        _labelControl2.Render(timePassed);
      }

      //render the icon
      if (_imageIcon != null)
      {
        // If the icon should be inline with the text then move it to the end of the text.
        if (_iconInlineLabel1)
        {
          int iconX = _labelControl1.XPosition + System.Math.Min(_labelControl1.Width, _labelControl1.TextWidth) +
                      IconOffsetX;
          int iconY = _imageIcon.YPosition;
          _imageIcon.SetPosition(iconX, iconY);
        }
        // Ensure that the icon stays within the bounds of the button, else do not render the icon.
        if ((_imageIcon.XPosition >= _positionX) &&
            (_imageIcon.XPosition + _imageIcon.Width <= _positionX + _width))
        {
          _imageIcon.Render(timePassed);
        }
      }

      //render icon2
      if (_imageIcon2 != null)

      {
        // If the icon should be inline with the text then move it to the end of the text.
        if (_icon2InlineLabel1)
        {
          int iconX = _labelControl1.XPosition + System.Math.Min(_labelControl1.Width, _labelControl1.TextWidth) +
                      Icon2OffsetX;
          int iconY = _imageIcon2.YPosition;
          _imageIcon2.SetPosition(iconX, iconY);
        }
        // Ensure that the icon2 stays within the bounds of the button, else do not render the icon.
        if ((_imageIcon2.XPosition >= _positionX) &&
            (_imageIcon2.XPosition + _imageIcon2.Width <= _positionX + _width))
        {
          _imageIcon2.Render(timePassed);
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
      base.OnAction(action);
      GUIMessage message;
      if (Focus)
      {
        //is the button clicked?
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          // If this button has a click setting then execute the setting.
          if (_onclick.Length != 0)
          {
            GUIPropertyManager.Parse(_onclick, GUIExpressionManager.ExpressionOptions.EVALUATE_ALWAYS);
          }

          //If this button contains scriptactions call the scriptactions.
          if (_application.Length != 0)
          {
            //button should start an external application, so start it
            Process proc = new Process();
            string strWorkingDir = Path.GetFullPath(_application);
            string strFileName = Path.GetFileName(_application);
            strWorkingDir = strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length + 1));
            proc.StartInfo.FileName = strFileName;
            proc.StartInfo.WorkingDirectory = strWorkingDir;
            proc.StartInfo.Arguments = _arguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            //proc.WaitForExit();
          }

          // If this links to another window go to the window.
          if (_hyperLinkWindowId >= 0)
          {
            //then switch to the other window
            GUIWindowManager.ActivateWindow((int)_hyperLinkWindowId);
            return;
          }

          // If this button corresponds to an action generate that action.
          if (ActionID >= 0)
          {
            Action newaction = new Action((Action.ActionType)ActionID, 0, 0);
            GUIGraphicsContext.OnAction(newaction);
            return;
          }

          // button selected.
          // send a message to the parent window
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
            Label1 = message.Label;
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
      _imageFocusedLeft.PreAllocResources();
      _imageFocusedMid.PreAllocResources();
      _imageFocusedRight.PreAllocResources();
      _imageNonFocusedLeft.PreAllocResources();
      _imageNonFocusedMid.PreAllocResources();
      _imageNonFocusedRight.PreAllocResources();
      _imageIcon.PreAllocResources();
      _imageIcon2.PreAllocResources();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _imageFocusedLeft.AllocResources();
      _imageFocusedMid.AllocResources();
      _imageFocusedRight.AllocResources();
      _imageNonFocusedLeft.AllocResources();
      _imageNonFocusedMid.AllocResources();
      _imageNonFocusedRight.AllocResources();
      _imageIcon.AllocResources();
      _imageIcon2.AllocResources();

      _labelControl1.AllocResources();
      _labelControl2.AllocResources();
      _property1Changed = true;
      _property2Changed = true;
      _reCalculate = true;

      if (_registeredForEvent == false)
      {
        GUIPropertyManager.OnPropertyChanged -=
          new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);

        GUIPropertyManager.OnPropertyChanged +=
          new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
        _registeredForEvent = true;
      }
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (tag == null)
      {
        return;
      }
      if (_containsProperty1)
      {
        if (_tagLabel1.IndexOf(tag) >= 0)
        {
          _property1Changed = true;
        }
      }
      if (_containsProperty2)
      {
        if (_tagLabel2.IndexOf(tag) >= 0)
        {
          _property2Changed = true;
        }
      }
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _imageFocusedLeft.SafeDispose();
      _imageFocusedMid.SafeDispose();
      _imageFocusedRight.SafeDispose();
      _imageNonFocusedLeft.SafeDispose();
      _imageNonFocusedMid.SafeDispose();
      _imageNonFocusedRight.SafeDispose();
      _imageIcon.SafeDispose();
      _imageIcon2.SafeDispose();

      _labelControl1.SafeDispose();
      _labelControl2.SafeDispose();


      GUIPropertyManager.OnPropertyChanged -=
        new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
    }

    /// <summary>
    /// Get/set the color of the text when the GUIButton3PartControl is disabled.
    /// </summary>
    public long DisabledColor
    {
      get { return _disabledColor; }
      set
      {
        if (_disabledColor != value)
        {
          _disabledColor = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIButton3PartControl does not have the focus.
    /// </summary>
    public string TexutureNoFocusLeftName
    {
      get { return _textureNonFocusedLeft; } // _imageNonFocusedLeft.FileName
      set
      {
        if (value == null)
        {
          return;
        }
        _textureNonFocusedLeft = value;
        if (_imageNonFocusedLeft != null)
        {
          _imageNonFocusedLeft.SetFileName(value);
        }
      }
    }

    public string TexutureNoFocusMidName
    {
      get { return _textureNonFocusedMid; } // _imageNonFocusedMid.FileName
      set
      {
        if (value == null)
        {
          return;
        }
        _textureNonFocusedMid = value;
        if (_imageNonFocusedMid != null)
        {
          _imageNonFocusedMid.SetFileName(value);
        }
      }
    }

    public string TexutureNoFocusRightName
    {
      get { return _textureNonFocusedRight; } // _imageNonFocusedRight.FileName
      set
      {
        if (value == null)
        {
          return;
        }
        _textureNonFocusedRight = value;
        if (_imageNonFocusedRight != null)
        {
          _imageNonFocusedRight.SetFileName(value);
        }
      }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIButton3PartControl has the focus.
    /// </summary>
    public string TexutureFocusLeftName
    {
      get { return _textureFocusedLeft; } // _imageFocusedLeft.FileName
      set
      {
        if (value == null)
        {
          return;
        }
        _textureFocusedLeft = value;
        if (_imageFocusedLeft != null)
        {
          _imageFocusedLeft.SetFileName(value);
        }
      }
    }

    public string TexutureFocusMidName
    {
      get { return _textureFocusedMid; } // _imageFocusedMid.FileName
      set
      {
        if (value == null)
        {
          return;
        }
        _textureFocusedMid = value;
        if (_imageFocusedMid != null)
        {
          _imageFocusedMid.SetFileName(value);
        }
      }
    }

    public string TexutureFocusRightName
    {
      get { return _textureFocusedRight; } // _imageFocusedRight.FileName
      set
      {
        if (value == null)
        {
          return;
        }
        _textureFocusedRight = value;
        if (_imageFocusedRight != null)
        {
          _imageFocusedRight.SetFileName(value);
        }
      }
    }

    /// <summary>
    /// Get/set the filename of the icon texture
    /// </summary>
    public string TexutureIcon
    {
      get
      {
        if (_imageIcon == null)
        {
          return string.Empty;
        }
        return _imageIcon.FileName;
      }
      set
      {
        if (_imageIcon != null && _imageIcon.FileName != value)
        {
          _imageIcon.IsVisible = true;
          _imageIcon.SetFileName(value);
          _imageIcon.Width = _iconWidth;
          _imageIcon.Height = _iconHeight;
          _reCalculate = true;

          if (value == string.Empty)
          {
            _imageIcon.IsVisible = false;
          }
        }
      }
    }

    /// <summary>
    /// Get/set the filename of the icon2 texture
    /// </summary>
    public string TexutureIcon2
    {
      get
      {
        if (_imageIcon2 == null)
        {
          return string.Empty;
        }
        return _imageIcon2.FileName;
      }
      set
      {
        if (_imageIcon2 != null && _imageIcon2.FileName != value)
        {
          _imageIcon2.IsVisible = true;
          _imageIcon2.SetFileName(value);
          _imageIcon2.Width = _iconWidth;
          _imageIcon2.Height = _iconHeight;
          _reCalculate = true;

          if (value == string.Empty)
          {
            _imageIcon2.IsVisible = false;
          }
        }
      }
    }

    /// <summary>
    /// Set the color of the text on the GUIButton3PartControl. 
    /// </summary>
    public long TextColor1
    {
      get { return _textColor1; }
      set { _textColor1 = value; }
    }

    public long TextColor2
    {
      get { return _textColor2; }
      set { _textColor2 = value; }
    }

    /// <summary>
    /// Get/set the name of the font of the text of the GUIButton3PartControl.
    /// </summary>
    public string FontName1
    {
      get { return _fontName1; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName1 = value;
      }
    }

    public string FontName2
    {
      get { return _fontName2; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName2 = value;
      }
    }

    /// <summary>
    /// Set the text of the GUIButton3PartControl. 
    /// </summary>
    /// <param name="fontName">The font name.</param>
    /// <param name="label">The text.</param>
    /// <param name="color">The font color.</param>
    public void SetLabel1(string fontName, string label, long color)
    {
      if (fontName == null)
      {
        return;
      }
      if (label == null)
      {
        return;
      }
      if (fontName != _fontName1 || label != _tagLabel1 || color != _textColor1)
      {
        _tagLabel1 = label;
        _textColor1 = color;
        _fontName1 = fontName;
        _containsProperty1 = ContainsProperty(_tagLabel1);
        _property1Changed = true;
      }
    }

    public void SetLabel2(string fontName, string label, long color)
    {
      if (fontName == null)
      {
        return;
      }
      if (label == null)
      {
        return;
      }
      if (fontName != _fontName2 || label != _tagLabel2 || color != _textColor2)
      {
        _tagLabel2 = label;
        _textColor2 = color;
        _fontName2 = fontName;
        _containsProperty2 = ContainsProperty(_tagLabel2);
        _property2Changed = true;
      }
    }

    /// <summary>
    /// Get/set the text of the GUIButton3PartControl.
    /// </summary>
    public string Label1
    {
      get { return _tagLabel1; }
      set
      {
        if (value == null)
        {
          return;
        }
        if (_tagLabel1 != value)
        {
          _tagLabel1 = value;
          _containsProperty1 = ContainsProperty(_tagLabel1);
          _property1Changed = true;
        }
      }
    }

    public string Label2
    {
      get { return _tagLabel2; }
      set
      {
        if (value == null)
        {
          return;
        }
        if (_tagLabel2 != value)
        {
          _tagLabel2 = value;
          _containsProperty1 = ContainsProperty(_tagLabel2);
          _property1Changed = true;
        }
      }
    }

    /// <summary>
    /// Get/set the window ID to which the GUIButton3PartControl links.
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
    public int TextOffsetX1
    {
      get { return _textOffsetX1; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (_textOffsetX1 != value)
        {
          _textOffsetX1 = value;
          _reCalculate = true;
        }
      }
    }

    public int TextOffsetX2
    {
      get { return _textOffsetX2; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (_textOffsetX2 != value)
        {
          _textOffsetX2 = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY1
    {
      get { return _textOffsetY1; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (_textOffsetY1 != value)
        {
          _textOffsetY1 = value;
          _reCalculate = true;
        }
      }
    }

    public int TextOffsetY2
    {
      get { return _textOffsetY2; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (_textOffsetY1 != value)
        {
          _textOffsetY2 = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>
    protected void Calculate()
    {
      _reCalculate = false;

      _imageFocusedLeft.Height = _height;
      _imageFocusedMid.Height = _height;
      _imageFocusedRight.Height = _height;

      _imageFocusedLeft.Refresh();
      _imageFocusedRight.Refresh();
      int width;

      int widthLeft =
        (int)((float)_imageFocusedLeft.TextureWidth * ((float)_height / (float)_imageFocusedLeft.TextureHeight));
      int widthRight =
        (int)((float)_imageFocusedRight.TextureWidth * ((float)_height / (float)_imageFocusedRight.TextureHeight));
      int widthMid = _width - widthLeft - widthRight;
      if (widthMid < 0)
      {
        widthMid = 0;
      }

      while (true)
      {
        width = widthLeft + widthRight + widthMid;
        if (width > _width)
        {
          if (widthMid > 0)
          {
            widthMid--;
          }
          else
          {
            if (widthLeft > 0)
            {
              widthLeft--;
            }
            if (widthRight > 0)
            {
              widthRight--;
            }
          }
        }
        else
        {
          break;
        }
      }

      // If either the left or right (or both) parts are not to be rendered then stretching allows for the middle part
      // to stretch into the left or right parts space.
      if (stretchIfNotRendered)
      {
        if (!RenderLeft)
        {
          widthMid += widthLeft;
          widthLeft = 0;
        }
        if (!RenderRight)
        {
          widthMid += widthRight;
          widthRight = 0;
        }
      }

      _imageFocusedLeft.Width = widthLeft;
      _imageFocusedMid.Width = widthMid;
      _imageFocusedRight.Width = widthRight;
      if (widthLeft == 0)
      {
        _imageFocusedLeft.IsVisible = false;
      }
      else
      {
        _imageFocusedLeft.IsVisible = true;
      }

      if (widthMid == 0)
      {
        _imageFocusedMid.IsVisible = false;
      }
      else
      {
        _imageFocusedMid.IsVisible = true;
      }

      if (widthRight == 0)
      {
        _imageFocusedRight.IsVisible = false;
      }
      else
      {
        _imageFocusedRight.IsVisible = true;
      }

      _imageNonFocusedLeft.Width = widthLeft;
      _imageNonFocusedMid.Width = widthMid;
      _imageNonFocusedRight.Width = widthRight;
      if (widthLeft == 0)
      {
        _imageNonFocusedLeft.IsVisible = false;
      }
      else
      {
        _imageNonFocusedLeft.IsVisible = true;
      }

      if (widthMid == 0)
      {
        _imageNonFocusedMid.IsVisible = false;
      }
      else
      {
        _imageNonFocusedMid.IsVisible = true;
      }

      if (widthRight == 0)
      {
        _imageNonFocusedRight.IsVisible = false;
      }
      else
      {
        _imageNonFocusedRight.IsVisible = true;
      }

      _imageFocusedLeft.SetPosition(_positionX, _positionY);
      _imageFocusedMid.SetPosition(_positionX + widthLeft, _positionY);
      _imageFocusedRight.SetPosition(_positionX + _width - widthRight, _positionY);

      _imageNonFocusedLeft.SetPosition(_positionX, _positionY);
      _imageNonFocusedMid.SetPosition(_positionX + widthLeft, _positionY);
      _imageNonFocusedRight.SetPosition(_positionX + _width - widthRight, _positionY);

      if (_imageIcon != null)
      {
        _imageIcon.KeepAspectRatio = _iconKeepAspectRatio;
        _imageIcon.ImageAlignment = Alignment.ALIGN_CENTER;
        _imageIcon.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
        _imageIcon.Zoom = _iconZoomed;
        _imageIcon.Refresh();

        // The checks for center and middle align preserve legacy behavior and allow flexibility for the offset values when
        // using alignment values.
        if ((IconOffsetY < 0 || IconOffsetX < 0) &&
            (_iconAlign != Alignment.ALIGN_CENTER && _iconVAlign != VAlignment.ALIGN_MIDDLE))
        {
          int iWidth = _imageIcon.TextureWidth;
          if (iWidth >= _width)
          {
            _imageIcon.Width = _width;
            iWidth = _width;
          }
          int offset = (iWidth + iWidth / 2);
          if (offset > _width)
          {
            offset = _width;
          }
          _imageIcon.SetPosition(_positionX + (_width) - offset,
                                 _positionY + (_height / 2) - (_imageIcon.TextureHeight / 2));
        }
        else
        {
          int xPos = _positionX;
          int yPos = _positionY;

          switch (_iconAlign)
          {
            case Alignment.ALIGN_LEFT:
              xPos = _positionX + IconOffsetX;
              break;
            case Alignment.ALIGN_RIGHT:
              xPos = _positionX + _width - _imageIcon.TextureWidth - IconOffsetX;
              break;
            case Alignment.ALIGN_CENTER:
              xPos = (_positionX + _width - _imageIcon.TextureWidth) / 2 + IconOffsetX;
              break;
          }
          switch (_iconVAlign)
          {
            case VAlignment.ALIGN_TOP:
              yPos = _positionY + IconOffsetY;
              break;
            case VAlignment.ALIGN_BOTTOM:
              yPos = _positionY + _height - IconOffsetY;
              break;
            case VAlignment.ALIGN_MIDDLE:
              yPos = _positionY + (_height / 2) - (_imageIcon.TextureHeight / 2) + IconOffsetY;
              break;
          }
          _imageIcon.SetPosition(xPos, yPos);
        }
      }

      if (_imageIcon2 != null)
      {
        _imageIcon2.KeepAspectRatio = _icon2KeepAspectRatio;
        _imageIcon2.ImageAlignment = Alignment.ALIGN_CENTER;
        _imageIcon2.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
        _imageIcon2.Zoom = _icon2Zoomed;
        _imageIcon2.Refresh();

        // The checks for center and middle align preserve legacy behavior and allow flexibility for the offset values when
        // using alignment values.
        if ((Icon2OffsetY < 0 || Icon2OffsetX < 0) &&
            (_icon2Align != Alignment.ALIGN_CENTER && _icon2VAlign != VAlignment.ALIGN_MIDDLE))
        {
          int iWidth = _imageIcon2.TextureWidth;
          if (iWidth >= _width)
          {
            _imageIcon2.Width = _width;
            iWidth = _width;
          }
          int offset = (iWidth + iWidth / 2);
          if (offset > _width)
          {
            offset = _width;
          }
          _imageIcon2.SetPosition(_positionX + (_width) - offset,
                                  _positionY + (_height / 2) - (_imageIcon2.TextureHeight / 2));
        }
        else
        {
          int xPos = _positionX;
          int yPos = _positionY;

          switch (_icon2Align)
          {
            case Alignment.ALIGN_LEFT:
              xPos = _positionX + Icon2OffsetX;
              break;
            case Alignment.ALIGN_RIGHT:
              xPos = _positionX + _width - _imageIcon2.TextureWidth - Icon2OffsetX;
              break;
            case Alignment.ALIGN_CENTER:
              xPos = (_positionX + _width - _imageIcon2.TextureWidth) / 2 + Icon2OffsetX;
              break;
          }
          switch (_icon2VAlign)
          {
            case VAlignment.ALIGN_TOP:
              yPos = _positionY + Icon2OffsetY;
              break;
            case VAlignment.ALIGN_BOTTOM:
              yPos = _positionY + _height - Icon2OffsetY;
              break;
            case VAlignment.ALIGN_MIDDLE:
              yPos = _positionY + (_height / 2) - (_imageIcon2.TextureHeight / 2) + Icon2OffsetY;
              break;
          }
          _imageIcon2.SetPosition(xPos, yPos);
        }
      }
    }

    public void Refresh()
    {
      _reCalculate = true;
    }

    /// <summary>
    /// Get/Set the icon to be zoomed into the dest. rectangle
    /// </summary>
    public bool IconZoom
    {
      get { return _iconZoomed; }
      set
      {
        if (_iconZoomed != value)
        {
          _iconZoomed = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon to keep it's aspectratio in the dest. rectangle
    /// </summary>
    public bool IconKeepAspectRatio
    {
      get { return _iconKeepAspectRatio; }
      set
      {
        if (_iconKeepAspectRatio != value)
        {
          _iconKeepAspectRatio = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon centered in the dest. rectangle
    /// </summary>
    public bool IconCentered
    {
      get { return _iconCentered; }
      set
      {
        if (_iconCentered != value)
        {
          _iconCentered = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon alignment property
    /// </summary>
    public Alignment IconAlign
    {
      get { return _iconAlign; }
      set
      {
        if (_iconAlign != value)
        {
          _iconAlign = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon vertical alignment property
    /// </summary>
    public VAlignment IconVAlign
    {
      get { return _iconVAlign; }
      set
      {
        if (_iconVAlign != value)
        {
          _iconVAlign = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon inline label1 property
    /// </summary>
    public bool IconInlineLabel1
    {
      get { return _iconInlineLabel1; }
      set
      {
        if (_iconInlineLabel1 != value)
        {
          _iconInlineLabel1 = value;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon2 to be zoomed into the dest. rectangle
    /// </summary>
    public bool Icon2Zoom
    {
      get { return _icon2Zoomed; }
      set
      {
        if (_icon2Zoomed != value)
        {
          _icon2Zoomed = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon2 to keep it's aspectratio in the dest. rectangle
    /// </summary>
    public bool Icon2KeepAspectRatio
    {
      get { return _icon2KeepAspectRatio; }
      set
      {
        if (_icon2KeepAspectRatio != value)
        {
          _icon2KeepAspectRatio = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon2 centered in the dest. rectangle
    /// </summary>
    public bool Icon2Centered
    {
      get { return _icon2Centered; }
      set
      {
        if (_icon2Centered != value)
        {
          _icon2Centered = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon2 alignment property
    /// </summary>
    public Alignment Icon2Align
    {
      get { return _icon2Align; }
      set
      {
        if (_icon2Align != value)
        {
          _icon2Align = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon2 vertical alignment property
    /// </summary>
    public VAlignment Icon2VAlign
    {
      get { return _icon2VAlign; }
      set
      {
        if (_icon2VAlign != value)
        {
          _icon2VAlign = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the icon inline label1 property
    /// </summary>
    public bool Icon2InlineLabel1
    {
      get { return _icon2InlineLabel1; }
      set
      {
        if (_icon2InlineLabel1 != value)
        {
          _icon2InlineLabel1 = value;
        }
      }
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
        if (_application == null)
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
        if (_arguments == null)
        {
          return;
        }
        _arguments = value;
      }
    }

    /// <summary>
    /// Get/Set the x-position of the icon
    /// </summary>
    public int IconOffsetX
    {
      get { return _iconOffsetX; }
      set
      {
        if (_iconOffsetX != value)
        {
          _iconOffsetX = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the y-position of the icon
    /// </summary>
    public int IconOffsetY
    {
      get { return _iconOffsetY; }
      set
      {
        if (_iconOffsetY != value)
        {
          _iconOffsetY = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the x-position of the icon2
    /// </summary>
    public int Icon2OffsetX
    {
      get { return _icon2OffsetX; }
      set
      {
        if (_icon2OffsetX != value)
        {
          _icon2OffsetX = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the y-position of the icon2
    /// </summary>
    public int Icon2OffsetY
    {
      get { return _icon2OffsetY; }
      set
      {
        if (_icon2OffsetY != value)
        {
          _icon2OffsetY = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/Set the width of the icon
    /// </summary>
    public int IconWidth
    {
      get { return _iconWidth; }
      set
      {
        _iconWidth = value;
        if (_imageIcon != null)
        {
          _imageIcon.Width = _iconWidth;
        }
      }
    }

    /// <summary>
    /// Get/Set the height of the icon
    /// </summary>
    public int IconHeight
    {
      get { return _iconHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _iconHeight = value;
        if (_imageIcon != null)
        {
          _imageIcon.Height = _iconHeight;
        }
      }
    }

    /// <summary>
    /// Get/Set the width of the icon2
    /// </summary>
    public int Icon2Width
    {
      get { return _icon2Width; }
      set
      {
        _icon2Width = value;
        if (_imageIcon2 != null)
        {
          _imageIcon2.Width = _icon2Width;
        }
      }
    }

    /// <summary>
    /// Get/Set the height of the icon2
    /// </summary>
    public int Icon2Height
    {
      get { return _icon2Height; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _icon2Height = value;
        if (_imageIcon2 != null)
        {
          _imageIcon2.Height = _icon2Height;
        }
      }
    }

    private bool ContainsProperty(string text)
    {
      if (text == null)
      {
        return false;
      }
      if (text.IndexOf("#") >= 0)
      {
        return true;
      }
      return false;
    }

    public bool RenderLeft
    {
      get { return renderLeftPart; }
      set { renderLeftPart = value; }
    }

    public bool RenderRight
    {
      get { return renderRightPart; }
      set { renderRightPart = value; }
    }

    public bool StretchIfNotRendered
    {
      get { return stretchIfNotRendered; }
      set { stretchIfNotRendered = value; }
    }
    
    public override int Width
    {
      get { return base.Width; }
      set
      {
        if (base.Width != value)
        {
          base.Width = value;
          _reCalculate = true;
        }
      }
    }

    public override int Height
    {
      get { return base.Height; }
      set
      {
        if (base.Height != value)
        {
          base.Height = value;
          _reCalculate = true;
        }
      }
    }

    public override long ColourDiffuse
    {
      get { return ColourDiffuseTNF; }
      set
      {
        ColourDiffuseTF = value;
        ColourDiffuseTNF = value;
      }
    }

    public long ColourDiffuseTF
    {
      get { return ColourDiffuseTFM; }
      set
      {
        ColourDiffuseTFM = value;
        ColourDiffuseTFR = value;
        ColourDiffuseTFL = value;
      }
    }

    public long ColourDiffuseTNF
    {
      get { return ColourDiffuseTNFM; }
      set
      {
        ColourDiffuseTNFM = value;
        ColourDiffuseTNFR = value;
        ColourDiffuseTNFL = value;
      }
    }

    public long ColourDiffuseTFM
    {
      get { return _diffuseColorTFM; }
      set
      {
        _diffuseColorTFM = value;
      }
    }

    public long ColourDiffuseTNFM
    {
      get { return _diffuseColorTNFM; }
      set
      {
        _diffuseColorTNFM = value;
      }
    }

    public long ColourDiffuseTFR
    {
      get { return _diffuseColorTFR; }
      set
      {
        _diffuseColorTFR = value;
      }
    }

    public long ColourDiffuseTNFR
    {
      get { return _diffuseColorTNFR; }
      set
      {
        _diffuseColorTNFR = value;
      }
    }

    public long ColourDiffuseTFL
    {
      get { return _diffuseColorTFL; }
      set
      {
        _diffuseColorTFL = value;
      }
    }

    public long ColourDiffuseTNFL
    {
      get { return _diffuseColorTNFL; }
      set
      {
        _diffuseColorTNFL = value;
      }
    }

    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (_positionX == dwPosX && _positionY == dwPosY)
      {
        return;
      }
      _positionX = dwPosX;
      _positionY = dwPosY;
      _reCalculate = true;
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageFocusedLeft != null)
        {
          _imageFocusedLeft.DimColor = value;
        }
        if (_imageFocusedMid != null)
        {
          _imageFocusedMid.DimColor = value;
        }
        if (_imageFocusedRight != null)
        {
          _imageFocusedRight.DimColor = value;
        }
        if (_imageNonFocusedLeft != null)
        {
          _imageNonFocusedLeft.DimColor = value;
        }
        if (_imageNonFocusedMid != null)
        {
          _imageNonFocusedMid.DimColor = value;
        }
        if (_imageNonFocusedRight != null)
        {
          _imageNonFocusedRight.DimColor = value;
        }
        if (_imageIcon != null)
        {
          _imageIcon.DimColor = value;
        }
        if (_imageIcon2 != null)
        {
          _imageIcon2.DimColor = value;
        }
        if (_labelControl1 != null)
        {
          _labelControl1.DimColor = value;
        }
        if (_labelControl2 != null)
        {
          _labelControl2.DimColor = value;
        }
      }
    }

    public void SetShadow1(int angle, int distance, long color)
    {
      _shadowAngle1 = angle;
      _shadowDistance1 = distance;
      _shadowColor1 = color;
      _labelControl1.SetShadow(_shadowAngle1, _shadowDistance1, _shadowColor1);
    }

    public void SetShadow2(int angle, int distance, long color)
    {
      _shadowAngle2 = angle;
      _shadowDistance2 = distance;
      _shadowColor2 = color;
      _labelControl2.SetShadow(_shadowAngle2, _shadowDistance2, _shadowColor2);
    }

    public void SetBorderTFL(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                             string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTFL = border;
      _borderPositionTFL = position;
      _borderTextureRepeatTFL = repeat;
      _borderTextureRotateTFL = rotate;
      _borderTextureFileNameTFL = texture;
      _borderColorKeyTFL = colorKey;
      _borderHasCornersTFL = hasCorners;
      _borderCornerTextureRotateTFL = cornerRotate;
      _imageFocusedLeft.SetBorder(_strBorderTFL, _borderPositionTFL, _borderTextureRepeatTFL, _borderTextureRotateTFL,
                                  _borderTextureFileNameTFL, _borderColorKeyTFL, _borderHasCornersTFL,
                                  _borderCornerTextureRotateTFL);
    }

    public void SetBorderTNFL(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                              string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTNFL = border;
      _borderPositionTNFL = position;
      _borderTextureRepeatTNFL = repeat;
      _borderTextureRotateTNFL = rotate;
      _borderTextureFileNameTNFL = texture;
      _borderColorKeyTNFL = colorKey;
      _borderHasCornersTNFL = hasCorners;
      _borderCornerTextureRotateTNFL = cornerRotate;
      _imageNonFocusedLeft.SetBorder(_strBorderTNFL, _borderPositionTNFL, _borderTextureRepeatTNFL,
                                     _borderTextureRotateTNFL,
                                     _borderTextureFileNameTNFL, _borderColorKeyTNFL, _borderHasCornersTNFL,
                                     _borderCornerTextureRotateTNFL);
    }

    public void SetBorderTFM(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                             string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTFM = border;
      _borderPositionTFM = position;
      _borderTextureRepeatTFM = repeat;
      _borderTextureRotateTFM = rotate;
      _borderTextureFileNameTFM = texture;
      _borderColorKeyTFM = colorKey;
      _borderHasCornersTFM = hasCorners;
      _borderCornerTextureRotateTFM = cornerRotate;
      _imageFocusedMid.SetBorder(_strBorderTFM, _borderPositionTFM, _borderTextureRepeatTFM, _borderTextureRotateTFM,
                                 _borderTextureFileNameTFM, _borderColorKeyTFM, _borderHasCornersTFM,
                                 _borderCornerTextureRotateTFM);
    }

    public void SetBorderTNFM(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                              string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTNFM = border;
      _borderPositionTNFM = position;
      _borderTextureRepeatTNFM = repeat;
      _borderTextureRotateTNFM = rotate;
      _borderTextureFileNameTNFM = texture;
      _borderColorKeyTNFM = colorKey;
      _borderHasCornersTNFM = hasCorners;
      _borderCornerTextureRotateTNFM = cornerRotate;
      _imageNonFocusedMid.SetBorder(_strBorderTNFM, _borderPositionTNFM, _borderTextureRepeatTNFM,
                                    _borderTextureRotateTNFM,
                                    _borderTextureFileNameTNFM, _borderColorKeyTNFM, _borderHasCornersTNFM,
                                    _borderCornerTextureRotateTNFM);
    }

    public void SetBorderTFR(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                             string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTFR = border;
      _borderPositionTFR = position;
      _borderTextureRepeatTFR = repeat;
      _borderTextureRotateTFR = rotate;
      _borderTextureFileNameTFR = texture;
      _borderColorKeyTFR = colorKey;
      _borderHasCornersTFR = hasCorners;
      _borderCornerTextureRotateTFR = cornerRotate;
      _imageFocusedRight.SetBorder(_strBorderTFR, _borderPositionTFR, _borderTextureRepeatTFR, _borderTextureRotateTFR,
                                   _borderTextureFileNameTFR, _borderColorKeyTFR, _borderHasCornersTFR,
                                   _borderCornerTextureRotateTFR);
    }

    public void SetBorderTNFR(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                              string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTNFR = border;
      _borderPositionTNFR = position;
      _borderTextureRepeatTNFR = repeat;
      _borderTextureRotateTNFR = rotate;
      _borderTextureFileNameTNFR = texture;
      _borderColorKeyTNFR = colorKey;
      _borderHasCornersTNFR = hasCorners;
      _borderCornerTextureRotateTNFR = cornerRotate;
      _imageNonFocusedRight.SetBorder(_strBorderTNFR, _borderPositionTNFR, _borderTextureRepeatTNFR,
                                      _borderTextureRotateTNFR,
                                      _borderTextureFileNameTNFR, _borderColorKeyTNFR, _borderHasCornersTNFR,
                                      _borderCornerTextureRotateTNFR);
    }

    public void SetBorderTI(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                            string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTI = border;
      _borderPositionTI = position;
      _borderTextureRepeatTI = repeat;
      _borderTextureRotateTI = rotate;
      _borderTextureFileNameTI = texture;
      _borderColorKeyTI = colorKey;
      _borderHasCornersTI = hasCorners;
      _borderCornerTextureRotateTI = cornerRotate;
      _imageIcon.SetBorder(_strBorderTI, _borderPositionTI, _borderTextureRepeatTI, _borderTextureRotateTI,
                           _borderTextureFileNameTI, _borderColorKeyTI, _borderHasCornersTI,
                           _borderCornerTextureRotateTI);
    }

    public void SetBorderTI2(string border, GUIImage.BorderPosition position, bool repeat, bool rotate,
                             string texture, long colorKey, bool hasCorners, bool cornerRotate)
    {
      _strBorderTI2 = border;
      _borderPositionTI2 = position;
      _borderTextureRepeatTI2 = repeat;
      _borderTextureRotateTI2 = rotate;
      _borderTextureFileNameTI2 = texture;
      _borderColorKeyTI2 = colorKey;
      _borderHasCornersTI2 = hasCorners;
      _borderCornerTextureRotateTI2 = cornerRotate;
      _imageIcon2.SetBorder(_strBorderTI2, _borderPositionTI2, _borderTextureRepeatTI2, _borderTextureRotateTI2,
                            _borderTextureFileNameTI2, _borderColorKeyTI2, _borderHasCornersTI2,
                            _borderCornerTextureRotateTI2);
    }

    public bool TileFillTFL
    {
      get { return _imageFocusedLeft.TileFill; }
      set { _imageFocusedLeft.TileFill = value; }
    }

    public bool TileFillTNFL
    {
      get { return _imageNonFocusedLeft.TileFill; }
      set { _imageNonFocusedLeft.TileFill = value; }
    }

    public bool TileFillTFM
    {
      get { return _imageFocusedMid.TileFill; }
      set { _imageFocusedMid.TileFill = value; }
    }

    public bool TileFillTNFM
    {
      get { return _imageNonFocusedMid.TileFill; }
      set { _imageNonFocusedMid.TileFill = value; }
    }

    public bool TileFillTFR
    {
      get { return _imageFocusedRight.TileFill; }
      set { _imageFocusedRight.TileFill = value; }
    }

    public bool TileFillTNFR
    {
      get { return _imageNonFocusedRight.TileFill; }
      set { _imageNonFocusedRight.TileFill = value; }
    }

    public string OverlayFileNameTFL
    {
      get { return _overlayTFL; }
      set
      {
        if (_overlayTFL == value)
        {
          return;
        }
        _overlayTFL = value;
      }
    }

    public string OverlayFileNameTNFL
    {
      get { return _overlayTNFL; }
      set
      {
        if (_overlayTNFL == value)
        {
          return;
        }
        _overlayTNFL = value;
      }
    }

    public string OverlayFileNameTFM
    {
      get { return _overlayTFM; }
      set
      {
        if (_overlayTFM == value)
        {
          return;
        }
        _overlayTFM = value;
      }
    }

    public string OverlayFileNameTNFM
    {
      get { return _overlayTNFM; }
      set
      {
        if (_overlayTNFM == value)
        {
          return;
        }
        _overlayTNFM = value;
      }
    }

    public string OverlayFileNameTFR
    {
      get { return _overlayTFR; }
      set
      {
        if (_overlayTFR == value)
        {
          return;
        }
        _overlayTFR = value;
      }
    }

    public string OverlayFileNameTNFR
    {
      get { return _overlayTNFR; }
      set
      {
        if (_overlayTNFR == value)
        {
          return;
        }
        _overlayTNFR = value;
      }
    }
  }
}