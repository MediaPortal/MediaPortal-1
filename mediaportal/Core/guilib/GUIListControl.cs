#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using MediaPortal.ExtensionMethods;
using MediaPortal.Profile;

// used for Keys definition
// used for loopDelay

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The implementation of a GUIListControl
  /// </summary>
  public class GUIListControl : GUIControl
  {
    public enum ListType
    {
      CONTROL_LIST,
      CONTROL_UPDOWN
    };

    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    };

    #region Events

    public delegate string GetScrollLabelDelegate(GUIListItem item);

    public event GetScrollLabelDelegate GetScrollLabel;

    #endregion

    [XMLSkinElement("spaceBetweenItems")] protected int _spaceBetweenItems = 2;

    [XMLSkinElement("textureHeight")] protected int _itemHeight = 10;

    [XMLSkinElement("textXOff")] protected int _textOffsetX;
    [XMLSkinElement("textYOff")] protected int _textOffsetY;
    [XMLSkinElement("textXOff2")] protected int _textOffsetX2;
    [XMLSkinElement("textYOff2")] protected int _textOffsetY2;
    [XMLSkinElement("textXOff3")] protected int _textOffsetX3;
    [XMLSkinElement("textYOff3")] protected int _textOffsetY3;
    [XMLSkinElement("textpadding")] protected int _textPadding = 0;
    [XMLSkinElement("textpadding2")] protected int _textPadding2 = 0;
    [XMLSkinElement("textpadding3")] protected int _textPadding3 = 0;

    [XMLSkinElement("itemWidth")] protected int _imageWidth = 16;
    [XMLSkinElement("itemHeight")] protected int _imageHeight = 16;

    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("textalign3")] protected Alignment _text3Alignment = Alignment.ALIGN_LEFT;

    [XMLSkinElement("textcontent3")] protected string _text3Content = string.Empty;

    [XMLSkinElement("remoteColor")] protected long _remoteColor = 0xffff0000;
    [XMLSkinElement("playedColor")] protected long _playedColor = 0xffa0d0ff;
    [XMLSkinElement("downloadColor")] protected long _downloadColor = 0xff00ff00;
    [XMLSkinElement("shadedColor")] protected long _shadedColor = 0x20ffffff;
    [XMLSkinElement("textvisible1")] protected bool _text1Visible = true;
    [XMLSkinElement("textvisible2")] protected bool _text2Visible = true;
    [XMLSkinElement("textvisible3")] protected bool _text3Visible = true;
    [XMLSkinElement("PinIconXOff")] protected int _xOffsetPinIcon = 100;
    [XMLSkinElement("PinIconYOff")] protected int _yOffsetPinIcon = 10;
    [XMLSkinElement("PinIconWidth")] protected int _widthPinIcon = 0;
    [XMLSkinElement("PinIconHeight")] protected int _heightPinIcon = 0;

    [XMLSkinElement("IconXOff")] protected int _iconOffsetX = 8;
    [XMLSkinElement("IconYOff")] protected int _iconOffsetY = 5;

    // this is the offset from the first or last element on screen when scrolling should start
    [XMLSkinElement("scrollOffset")] protected int _scrollStartOffset = 0;

    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = 1;

    [XMLSkinElement("keepaspectratio")] protected bool _keepAspectRatio = false;

    [XMLSkinElement("suffix")] protected string _suffix = "|";
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("font2")] protected string _fontName2Name = "";
    [XMLSkinElement("font3")] protected string _fontName3Name = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolor2")] protected long _textColor2 = 0xFFFFFFFF;
    [XMLSkinElement("textcolor3")] protected long _textColor3 = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")] protected long _selectedColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor2")] protected long _selectedColor2 = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor3")] protected long _selectedColor3 = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected string _textColorNoFocus = "N/A";
    [XMLSkinElement("textcolorNoFocus2")] protected string _textColorNoFocus2 = "N/A";
    [XMLSkinElement("textcolorNoFocus3")] protected string _textColorNoFocus3 = "N/A";
    
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;

    [XMLSkinElement("folderPrefix")] protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")] protected string _folderSuffix = "]";

    [XMLSkinElement("textureUp")] protected string _upTextureName = "";
    [XMLSkinElement("textureDown")] protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus = "";
    [XMLSkinElement("textureNoFocus")] protected string _buttonNonFocusName = "";
    [XMLSkinElement("textureFocus")] protected string _buttonFocusName = "";
    [XMLSkinElement("scrollbarwidth")] protected int _scrollbarWidth = 15;
    [XMLSkinElement("scrollbarXOff")] protected int _scrollbarXOff = 0;

    [XMLSkin("textureNoFocus", "border")] protected string _strBorderBNF = "";

    [XMLSkin("textureNoFocus", "position")] protected GUIImage.BorderPosition _borderPositionBNF =
      GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;

    [XMLSkin("textureNoFocus", "textureRepeat")] protected bool _borderTextureRepeatBNF = false;
    [XMLSkin("textureNoFocus", "textureRotate")] protected bool _borderTextureRotateBNF = false;
    [XMLSkin("textureNoFocus", "texture")] protected string _borderTextureFileNameBNF = "image_border.png";
    [XMLSkin("textureNoFocus", "colorKey")] protected long _borderColorKeyBNF = 0xFFFFFFFF;
    [XMLSkin("textureNoFocus", "corners")] protected bool _borderHasCornersBNF = false;
    [XMLSkin("textureNoFocus", "cornerRotate")] protected bool _borderCornerTextureRotateBNF = true;

    [XMLSkin("textureFocus", "border")] protected string _strBorderBF = "";

    [XMLSkin("textureFocus", "position")] protected GUIImage.BorderPosition _borderPositionBF =
      GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;

    [XMLSkin("textureFocus", "textureRepeat")] protected bool _borderTextureRepeatBF = false;
    [XMLSkin("textureFocus", "textureRotate")] protected bool _borderTextureRotateBF = false;
    [XMLSkin("textureFocus", "texture")] protected string _borderTextureFileNameBF = "image_border.png";
    [XMLSkin("textureFocus", "colorKey")] protected long _borderColorKeyBF = 0xFFFFFFFF;
    [XMLSkin("textureFocus", "corners")] protected bool _borderHasCornersBF = false;
    [XMLSkin("textureFocus", "cornerRotate")] protected bool _borderCornerTextureRotateBF = true;

    [XMLSkinElement("scrollbarbg")] protected string _scrollbarBackgroundName = "";
    [XMLSkinElement("scrollbartop")] protected string _scrollbarTopName = "";
    [XMLSkinElement("scrollbarbottom")] protected string _scrollbarBottomName = "";

    [XMLSkinElement("spinColor")] protected long _spinControlColor;
    [XMLSkinElement("spinAlign")] protected Alignment _spinControlAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("spinHeight")] protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")] protected int _spinControlWidth;
    [XMLSkinElement("spinPosX")] protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")] protected int _spinControlPositionY;

    [XMLSkinElement("unfocusedAlpha")] protected int _unfocusedAlpha = 0xFF;
    [XMLSkin("unfocusedAlpha", "applyToAll")] protected bool _unfocusedAlphaApplyToAll = false;

    [XMLSkinElement("spinCanFocus")] protected bool _spinCanFocus = true;

    [XMLSkinElement("explicitlyEnableScrollLabel")] protected bool _explicitlyEnableScrollLabel = false;

    [XMLSkinElement("bdDvdDirectoryColor")] protected long _bdDvdDirectoryColor = 0xFFFFFFFF;

    protected GUIFont _font = null;
    protected GUIFont _font2 = null;
    protected GUIFont _font3 = null;
    protected GUISpinControl _upDownControl = null;
    protected List<GUIControl> _listButtons = null;
    protected GUIVerticalScrollbar _verticalScrollbar = null;

    protected List<GUIListItem> _listItems = new List<GUIListItem>();
    protected List<GUILabelControl> _labelControls1 = new List<GUILabelControl>();
    protected List<GUILabelControl> _labelControls2 = new List<GUILabelControl>();
    protected List<GUILabelControl> _labelControls3 = new List<GUILabelControl>();

    protected int _offset = 0;
    protected int _itemsPerPage = 10;
    protected int _lastItemPageValue = 0;

    protected ListType _listType = ListType.CONTROL_LIST;
    protected int _cursorX = 0;

    protected bool _upDownControlVisible = true;

    protected bool _refresh = false;
    protected string _textLine2;
    protected string _brackedText;

    protected int _scrollPosition = 0;
    protected int _scrollPosititionX = 0;
    protected int _lastItem = -1;

    protected double _scrollOffsetX = 0.0f;
    protected double _timeElapsed = 0.0f;
    protected bool _scrollContinuously = false;

    protected bool _drawFocus = true;
    protected double _lastCommandTime = 0;
    protected int _loopDelay = 0;

    private int _frameLimiter = 1;
    // Search            
    private DateTime _timerKey = DateTime.Now;
    private char _currentKey = (char)0;
    private char _previousKey = (char)0;
    protected string _searchString = "";
    protected int _lastSearchItem = 0;
    protected bool _enableSMSsearch = true;
    protected bool _enableScrollLabel = false;

    private DateTime _scrollTimer = DateTime.Now;
    private int _scrollCounter;
    private const int _scrollCounterLimit = 3;
    protected string _scrollDirection = "";

    public GUIListControl(int dwParentID) : base(dwParentID)
    {
      WordWrap = false;
    }

    /// <summary>
    /// The constructor of the GUIListControl.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="dwSpinWidth">TODO </param>
    /// <param name="dwSpinHeight">TODO</param>
    /// <param name="strUp">The name of the scroll up unfocused texture.</param>
    /// <param name="strDown">The name of the scroll down unfocused texture.</param>
    /// <param name="strUpFocus">The name of the scroll up focused texture.</param>
    /// <param name="strDownFocus">The name of the scroll down unfocused texture.</param>
    /// <param name="dwSpinColor">TODO </param>
    /// <param name="dwSpinX">TODO </param>
    /// <param name="dwSpinY">TODO </param>
    /// <param name="strFont">The font used in the spin control.</param>
    /// <param name="dwTextColor">The color of the text.</param>
    /// <param name="dwSelectedColor">The color of the text when it is selected.</param>
    /// <param name="strButton">The name of the unfocused button texture.</param>
    /// <param name="strButtonFocus">The name of the focused button texture.</param>
    /// <param name="strScrollbarBackground">The name of the background of the scrollbar texture.</param>
    /// <param name="strScrollbarTop">The name of the top of the scrollbar texture.</param>
    /// <param name="strScrollbarBottom">The name of the bottom of the scrollbar texture.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degress along x-axis.</param>
    /// <param name="dwShadowDistance">The distance of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUIListControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                          int dwSpinWidth, int dwSpinHeight,
                          string strUp, string strDown,
                          string strUpFocus, string strDownFocus,
                          long dwSpinColor, int dwSpinX, int dwSpinY,
                          string strFont, long dwTextColor, long dwSelectedColor,
                          string strButton, string strButtonFocus,
                          string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom,
                          int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      WordWrap = false;
      _spinControlWidth = dwSpinWidth;
      _spinControlHeight = dwSpinHeight;
      _upTextureName = strUp;
      _downTextureName = strDown;
      _upTextureNameFocus = strUpFocus;
      _downTextureNameFocus = strDownFocus;
      _spinControlColor = dwSpinColor;
      _spinControlPositionX = dwSpinX;
      _spinControlPositionY = dwSpinY;
      _fontName = strFont;
      _selectedColor = dwSelectedColor;
      _selectedColor2 = dwSelectedColor;
      _selectedColor3 = dwSelectedColor;
      _textColor = dwTextColor;
      _textColor2 = dwTextColor;
      _textColor3 = dwTextColor;
      _buttonNonFocusName = strButton;
      _buttonFocusName = strButtonFocus;
      _scrollbarBackgroundName = strScrollbarBackground;
      _scrollbarTopName = strScrollbarTop;
      _scrollbarBottomName = strScrollbarBottom;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;

      FinalizeConstruction();
    }

    public override sealed void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      _font = GUIFontManager.GetFont(_fontName);
      if (_fontName2Name == string.Empty)
      {
        _fontName2Name = _fontName;
      }
      if (_fontName3Name == string.Empty)
      {
        _fontName3Name = _fontName2Name;
      }
      Font2 = _fontName2Name;
      Font3 = _fontName3Name;

      _upDownControl = new GUISpinControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _spinControlWidth,
                                          _spinControlHeight, _upTextureName, _downTextureName, _upTextureNameFocus,
                                          _downTextureNameFocus, _fontName, _spinControlColor,
                                          GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, _spinControlAlignment)
                         {
                           ParentControl = this,
                           DimColor = DimColor
                         };

      _verticalScrollbar = new GUIVerticalScrollbar(_controlId, 0, 5 + _positionX + _width + _scrollbarXOff, _positionY,
                                                    _scrollbarWidth, _height,
                                                    _scrollbarBackgroundName, _scrollbarTopName, _scrollbarBottomName)
                             {
                               ParentControl = this,
                               SendNotifies = false,
                               DimColor =DimColor
                             };
      _upDownControl.WindowId = WindowId;

      using (Settings xmlreader = new MPSettings())
      {
        _loopDelay = xmlreader.GetValueAsInt("gui", "listLoopDelay", 100);
      }
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _spinControlPositionX, ref _spinControlPositionY,
                                                     ref _spinControlWidth, ref _spinControlHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX2, ref _textOffsetY2);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX3, ref _textOffsetY3);
      GUIGraphicsContext.ScaleVertical(ref _spaceBetweenItems);
      GUIGraphicsContext.ScaleVertical(ref _itemHeight);
      GUIGraphicsContext.ScaleHorizontal(ref _iconOffsetX);
      GUIGraphicsContext.ScaleVertical(ref _iconOffsetY);
      GUIGraphicsContext.ScaleHorizontal(ref _xOffsetPinIcon);
      GUIGraphicsContext.ScaleVertical(ref _yOffsetPinIcon);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _widthPinIcon, ref _heightPinIcon);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _imageWidth, ref _imageHeight);
    }

    private void item_OnThumbnailRefresh(int buttonNr, bool gotFocus)
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        // Update current focused thumbnail
        GUIListItem item = _listItems[buttonNr + _offset];
        {
          if (gotFocus)
          {
            if (item.HasThumbnail)
            {
              string selectedThumbProperty = GUIPropertyManager.GetProperty("#selectedthumb");
              if (selectedThumbProperty != item.ThumbnailImage)
              {
                GUIPropertyManager.SetProperty("#selectedthumb", string.Empty);
                GUIPropertyManager.SetProperty("#selectedthumb", item.ThumbnailImage);
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED, WindowId, GetID, ParentID, 0, 0, null)
                {
                  SendToTargetWindow = true
                };
                GUIGraphicsContext.SendMessage(msg);
              }
            }
          }
        }
      }
    }

    protected void OnSelectionChanged()
    {
      if (!IsVisible)
      {
        return;
      }

      _scrollPosition = 0;
      _scrollPosititionX = 0;
      _lastItem = -1;
      _scrollOffsetX = 0.0f;
      _timeElapsed = 0.0f;
      // Reset searchstring
      if (_lastSearchItem != (_cursorX + _offset))
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
        _searchString = "";
      }

      string strSelected = "";
      string strSelected2 = "";
      string strThumb = "";
      string strIndex = "";
      int item = GetSelectedItem(ref strSelected, ref strSelected2, ref strThumb, ref strIndex);
      if (!GUIWindowManager.IsRouted)
      {
        GUIPropertyManager.SetProperty("#selecteditem", strSelected);
        GUIPropertyManager.SetProperty("#selecteditem2", strSelected2);
        if (!string.IsNullOrEmpty(strThumb) && string.IsNullOrEmpty(Path.GetPathRoot(strThumb)))
        {
          GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
        }
        else if (MediaPortal.Util.Utils.FileExistsInCache(strThumb))
        {
          GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
        }
        GUIPropertyManager.SetProperty("#selectedindex", strIndex);
        GUIPropertyManager.SetProperty("#highlightedbutton", strSelected);
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED, WindowId, GetID, ParentID, 0, 0, null)
                         {
                           SendToTargetWindow = true
                         };
      GUIGraphicsContext.SendMessage(msg);

      if (item >= 0 && item < _listItems.Count)
      {
        GUIListItem listitem = _listItems[item];
        if (listitem != null)
        {
          listitem.ItemSelected(this);
        }
      }
      // ToDo: add searchstring property
      if (_searchString.Length > 0)
      {
        GUIPropertyManager.SetProperty("#selecteditem", "{" + _searchString.ToLowerInvariant() + "}");
      }
    }

    protected virtual void FreeUnusedThumbnails()
    {
      if (_lastItemPageValue != _offset + _itemsPerPage)
      {
        _lastItemPageValue = _offset + _itemsPerPage;
        for (int i = 0; i < _offset; ++i)
        {
          GUIListItem pItem = _listItems[i];
          if (null != pItem)
          {
            bool dispose = true;
            pItem.RetrieveArt = false;
            for (int x = _offset; x < _offset + _itemsPerPage && x < _listItems.Count; x++)
            {
              GUIListItem pItem2 = _listItems[x];
              //pItem2.RetrieveArt = false;
              if ((pItem.IconImage != "" && pItem.IconImage == pItem2.IconImage) ||
                  (pItem.IconImageBig != "" && pItem.IconImageBig == pItem2.IconImageBig))
              {
                dispose = false;
                break;
              }
              //pItem2.RetrieveArt = true;
            }
            pItem.RetrieveArt = true;
            if (dispose)
            {
              pItem.FreeMemory();
            }
          }
        }
        for (int i = _offset + _itemsPerPage + 1; i < _listItems.Count; ++i)
        {
          GUIListItem pItem = _listItems[i];
          if (null != pItem)
          {
            pItem.RetrieveArt = false;
            bool dispose = true;
            for (int x = _offset; x < _offset + _itemsPerPage && x < _listItems.Count; x++)
            {
              GUIListItem pItem2 = _listItems[x];
              //pItem2.RetrieveArt = false;
              if ((pItem.IconImageBig != "" && pItem.IconImage == pItem2.IconImageBig) ||
                  (pItem.IconImageBig != "" && pItem.IconImageBig == pItem2.IconImageBig))
              {
                dispose = false;
                break;
              }
              //pItem2.RetrieveArt = true;
            }
            if (dispose)
            {
              pItem.FreeMemory();
            }
          }
          if (pItem != null)
          {
            pItem.RetrieveArt = true;
          }
        }
      }
    }

    protected virtual void RenderButton(float timePassed, int buttonNr, int x, int y, bool gotFocus)
    {
      if (_listButtons != null)
      {
        if (buttonNr >= 0 && buttonNr < _listButtons.Count)
        {
          GUIControl btn = _listButtons[buttonNr];
          if (btn != null)
          {
            if (gotFocus || !Focus)
            {
              btn.ColourDiffuse = Color.FromArgb((int)_diffuseColor).ToArgb();
            }
            else
            {
              btn.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)_diffuseColor)).ToArgb();
            }
            if (_listItems[buttonNr].Selected && !gotFocus && _unfocusedAlphaApplyToAll)
            {
              btn.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)_diffuseColor)).ToArgb();
            }
            btn.Focus = gotFocus;
            btn.SetPosition(x, y);
            btn.Render(timePassed);
          }
        }
      }
    }

    protected virtual void RenderIcon(float timePassed, int buttonNr, int x, int y, bool gotFocus)
    {
      GUIListItem pItem = _listItems[buttonNr + _offset];

      if (pItem.HasIcon)
      {
        // show icon
        GUIImage pImage = pItem.Icon;
        if (null == pImage)
        {
          pImage = new GUIImage(0, 0, 0, 0, _imageWidth, _imageHeight, pItem.IconImage, 0x0)
                     {
                       ParentControl = this,
                       KeepAspectRatio = _keepAspectRatio
                     };
          pImage.AllocResources();
          pItem.Icon = pImage;
        }
        if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
        {
          pImage.SafeDispose();

          pImage = new GUIImage(0, 0, 0, 0, _imageWidth, _imageHeight, pItem.IconImage, 0x0)
                     {
                       ParentControl = this,
                       KeepAspectRatio = _keepAspectRatio
                     };
          pImage.AllocResources();
          pItem.Icon = pImage;

          //pImage.AllocResources();
        }
        pImage.KeepAspectRatio = _keepAspectRatio;
        pImage.Width = _imageWidth;
        pImage.Height = _imageHeight;
        pImage.SetPosition(x, y);
        if (gotFocus || !Focus)
        {
          pImage.ColourDiffuse = 0xffffffff;
        }
        else
        {
          pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
        }
        if (!pItem.Selected && !gotFocus && _unfocusedAlphaApplyToAll)
        {
          pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
        }
        pImage.DimColor = DimColor;
        pImage.Render(timePassed);
      }
    }

    protected virtual void RenderPinIcon(float timePassed, int buttonNr, int x, int y, bool gotFocus)
    {
      GUIListItem pItem = _listItems[buttonNr + _offset];
      if (pItem.HasPinIcon)
      {
        GUIImage pinImage = pItem.PinIcon;
        if (null == pinImage)
        {
          //pinImage = new GUIImage(0, 0, 0, 0, 0, 0, pItem.PinImage, 0x0);
          pinImage = new GUIImage(0, 0, 0, 0, _widthPinIcon, _heightPinIcon, pItem.PinImage, 0x0)
                       {
                         ParentControl = this,
                         KeepAspectRatio = _keepAspectRatio
                       };
          pinImage.AllocResources();
          pItem.PinIcon = pinImage;
        }
        pinImage.KeepAspectRatio = _keepAspectRatio;
        pinImage.Width = PinIconWidth;
        pinImage.Height = PinIconHeight;


        if (PinIconOffsetY < 0 || PinIconOffsetX < 0)
        {
          pinImage.SetPosition(x + (_width) - (pinImage.TextureWidth + pinImage.TextureWidth / 2),
                               y + (_height / 2) - (pinImage.TextureHeight / 2));
        }
        else
        {
          pinImage.SetPosition(x + PinIconOffsetX, y + PinIconOffsetY);
        }

        if (gotFocus || !Focus)
        {
          pinImage.ColourDiffuse = 0xffffffff;
        }
        else
        {
          pinImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
        }

        if (!pItem.Selected && !gotFocus && _unfocusedAlphaApplyToAll)
        {
          pinImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
        }
        pinImage.DimColor = DimColor;
        pinImage.Render(timePassed);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="buttonNumber"></param>
    /// <param name="positionX"></param>
    /// <param name="positionY"></param>
    /// <param name="gotFocus"></param>
    protected virtual void RenderLabel(float timePassed, int buttonNumber, int positionX, int positionY, bool gotFocus)
    {
      GUIListItem scrollItem = SelectedListItem;
      if ((_explicitlyEnableScrollLabel || _enableScrollLabel) && ScrollLabelIsScrolling && scrollItem != null)
      {
        string scrollLabel = string.Empty;

        if (GetScrollLabel != null)
        {
          scrollLabel = GetScrollLabel(scrollItem);
        }
        else if (!string.IsNullOrEmpty(scrollItem.Label))
        {
          scrollLabel = scrollItem.Label.Substring(0, 1).ToUpperInvariant();
        }

        if (string.IsNullOrEmpty(scrollLabel))
        {
          scrollLabel = " ";
        }
        GUIPropertyManager.SetProperty("#selecteditem.scrolllabel", scrollLabel);
      }
      else
      {
        GUIPropertyManager.SetProperty("#selecteditem.scrolllabel", " ");
      }

      GUIListItem item = _listItems[buttonNumber + _offset];
      bool selected = buttonNumber == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST;
      long color;

      // apply horizontal text offset to position and calculate width
      positionX += _textOffsetX;
      int labelWidth = _width - _textOffsetX - _imageWidth - GUIGraphicsContext.ScaleHorizontal(20);

      if (_text2Visible && item.Label2.Length > 0 && _textOffsetY == _textOffsetY2)
      {
        // set initial text color
        color = _textColor2;

        // override text color if label is not selected
        if (!gotFocus)
        {
          long value;
          if (long.TryParse(_textColorNoFocus2, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
          {
            color = value;
          }
        }

        // override text color if skin sets it as selected
        if (item.Selected)
        {
          color = _selectedColor2;
        }

        // override text color if item is currently played
        if (item.IsPlayed)
        {
          color = _playedColor;
        }

        // override text color if item is on a remote folder
        if (item.IsRemote)
        {
          color = item.IsDownloading ? _downloadColor : _remoteColor;
        }

        // override text color if item is a BD or DVD folder
        if (item.IsBdDvdFolder)
        {
          color = _bdDvdDirectoryColor;
        }

        // if control is not in focus apply color dimming
        if (!Focus)
        {
          color &= DimColor;
        }

        // apply horizontal text offset to position
        int x = _textOffsetX2 == 0 
          ? _positionX + _width - GUIGraphicsContext.ScaleHorizontal(16) 
          : _positionX + _textOffsetX2;

        if (_labelControls2 != null && buttonNumber >= 0 && buttonNumber < _labelControls2.Count)
        {
          GUILabelControl label2 = _labelControls2[buttonNumber];
          if (label2 != null)
          {
            // set position for rendering
            label2.SetPosition(x, positionY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY2);

            // apply unfocusedAlpha if item is not selected and control is in focus, else use color in its current state for rendering
            label2.TextColor = !gotFocus && Focus
              ? Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int) color)).ToArgb()
              : color;

            // set text, alignment and font for rendering
            label2.Label = item.Label2;
            label2.TextAlignment = Alignment.ALIGN_RIGHT;
            label2.FontName = _fontName2Name;

            // recalculate label width
            labelWidth = label2._positionX - positionX - label2.TextWidth - GUIGraphicsContext.ScaleHorizontal(20);
          }
        }
      }

      if (_text1Visible)
      {
        // set initial text color
        color = _textColor;

        // override text color if label is not selected
        if (!gotFocus)
        {
          long value;
          if (long.TryParse(_textColorNoFocus, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
          {
            color = value;
          }
        }

        // override text color if skin sets it as selected
        if (item.Selected)
        {
          color = _selectedColor;
        }

        // override text color if item is currently played
        if (item.IsPlayed)
        {
          color = _playedColor;
        }

        // override text color if item is on a remote folder
        if (item.IsRemote)
        {
          color = item.IsDownloading ? _downloadColor : _remoteColor;
        }

        // override text color if item is a BD or DVD folder
        if (item.IsBdDvdFolder)
        {
          color = _bdDvdDirectoryColor;
        }

        // apply unfocusedAlpha to color if item is not selected and plugin didn't set it as selected
        if (!gotFocus && !item.Selected)
        {
          color = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)color)).ToArgb();
        }

        // if control is not in focus apply color dimming
        if (!Focus)
        {
          color &= DimColor;
        }

        // apply padding to label width 
        int maxWidth = labelWidth;
        if (_textPadding > 0)
        {
          maxWidth -= GUIGraphicsContext.ScaleHorizontal(_textPadding);
        }

        // render label if it still has a visible length
        if (maxWidth > 0)
        {
          RenderText(timePassed, buttonNumber, positionX, (float) positionY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY, maxWidth, color, item.Label, selected);
        }
        else
        {
          base.Render(timePassed);
        }
      }

      // if there is a second label present process it
      if (item.Label2.Length > 0)
      {
        // set initial text color
        color = _textColor2;

        // override text color if label is not selected
        if (!gotFocus)
        {
          long value;
          if (long.TryParse(_textColorNoFocus2, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
          {
            color = value;
          }
        }

        // override text color if skin sets it as selected
        if (item.Selected)
        {
          color = _selectedColor2;
        }

        // override text color if item is currently played
        if (item.IsPlayed)
        {
          color = _playedColor;
        }

        // override text color if item is on a remote folder
        if (item.IsRemote)
        {
          color = item.IsDownloading ? _downloadColor : _remoteColor; 
        }

        // override text color if item is a BD or DVD folder
        if (item.IsBdDvdFolder)
        {
          color = _bdDvdDirectoryColor;
        }

        // apply unfocusedAlpha to color if item is not selected and plugin didn't set it as selected
        if (!gotFocus && !item.Selected)
        {
          color = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)color)).ToArgb();
        }

        // if control is not in focus apply color dimming
        if (!Focus)
        {
          color &= DimColor;
        }

        // apply horizontal text offset to position
        if (_textOffsetX2 == 0)
        {
          positionX = _positionX + _width - GUIGraphicsContext.ScaleHorizontal(16);
        }
        else
        {
          positionX = _positionX + _textOffsetX2;
        }

        if (_text2Visible && (_labelControls2 != null) && (buttonNumber >= 0) && (buttonNumber < _labelControls2.Count))
        {
          GUILabelControl label2 = _labelControls2[buttonNumber];
          if (label2 != null)
          {
            // set position for rendering
            label2.SetPosition(positionX - GUIGraphicsContext.ScaleHorizontal(6), positionY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY2);

            // set label text color
            label2.TextColor = color;

            // set text, alignment and font for rendering
            label2.Label = item.Label2;
            label2.TextAlignment = Alignment.ALIGN_RIGHT;
            label2.FontName = _fontName2Name;

            // apply padding to label width
            if (_textPadding2 > 0)
            {
              _width -= GUIGraphicsContext.ScaleHorizontal(_textPadding2);
            }
            label2.Width = _width;

            // render label if it still has a visible length
            if (label2.Width > 0)
            {
              label2.Render(timePassed);
            }
            else
            {
              base.Render(timePassed);
            }
          }
        }
      }

      // if there is a third label present process it
      if (item.Label3.Length > 0 || !string.IsNullOrEmpty(_text3Content))
      {
        // set initial text color
        color = _textColor3;

        // override text color if label is not selected
        if (!gotFocus)
        {
          long value;
          if (long.TryParse(_textColorNoFocus3, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
          {
            color = value;
          }
        }

        // override text color if skin sets it as selected
        if (item.Selected)
        {
          color = _selectedColor3;
        }

        // override text color if item is currently played
        if (item.IsPlayed)
        {
          color = _playedColor;
        }

        // override text color if item is on a remote folder
        if (item.IsRemote)
        {
          color = item.IsDownloading ? _downloadColor : _remoteColor;
        }

        // override text color if item is a BD or DVD folder
        if (item.IsBdDvdFolder)
        {
          color = _bdDvdDirectoryColor;
        }

        // apply unfocusedAlpha to color if item is not selected and plugin didn't set it as selected
        if (!gotFocus && !item.Selected)
        {
          color = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)color)).ToArgb();
        }

        // if control is not in focus apply color dimming
        if (!Focus)
        {
          color &= DimColor;
        }

        // apply horizontal text offset to position
        positionX = _textOffsetX3 == 0 ? _positionX + _textOffsetX : _positionX + _textOffsetX3;

        // apply vertical text offset to position
        int ypos = positionY;
        ypos += _textOffsetY3 == 0 ? _textOffsetY2 : _textOffsetY3;

        if (_text3Visible && _labelControls3 != null && buttonNumber >= 0 && buttonNumber < _labelControls3.Count)
        {
          GUILabelControl label3 = _labelControls3[buttonNumber];

          if (label3 != null)
          {
            // set position for rendering
            label3.SetPosition(positionX, ypos);

            // set label text color
            label3.TextColor = color;

            // set label text for rendering
            label3.Label = item.Label3;
            if (!string.IsNullOrEmpty(_text3Content))
            {
              label3.Label = SetLabel(_text3Content, item);
            }

            // set alignment, font and width for rendering
            label3.TextAlignment = _text3Alignment;
            label3.FontName = _fontName3Name;
            label3.Width = _width - _textOffsetX - _imageWidth - GUIGraphicsContext.ScaleHorizontal(34);
            
            // adjust label width with padding
            if (_textPadding3 > 0)
            {
              label3.Width -= GUIGraphicsContext.ScaleHorizontal(_textPadding3);
            }

            // render label if it still has a visible length
            if (label3.Width > 0)
            {
              RenderText(timePassed, buttonNumber, label3, selected);
            }
            else
            {
              base.Render(timePassed);
            }
          }
        }
      }
    }

    private string SetLabel(string textContent, GUIListItem item)
    {
      string label = string.Empty;
      switch (textContent.ToLowerInvariant())
      {
        case "#selectedindex":
          if (item.Label == "..")
          {
            label = string.Empty;
          }
          else
          {
            int index = _listItems.IndexOf(item);
            if (_listItems.Count > 0 && _listItems[0].Label != "..")
            {
              index++;
            }
            label = index.ToString(CultureInfo.InvariantCulture);
          }
          break;

        case "#selecteditem":
          label = item.Label;
          break;

        case "#selecteditem2":
          label = item.Label2;
          break;

          /*case "#itemcount":
          label = GUIPropertyManager.GetProperty("#itemcount");
          break;

        case "#selectedthumb":
          label = item.ThumbnailImage;
          break;*/

        case "#rating":
          label = item.Rating.ToString(CultureInfo.InvariantCulture);
          break;

        case "#duration":
          if (item.Duration == 0)
          {
            label = string.Empty;
          }
          else
          {
            label = MediaPortal.Util.Utils.SecondsToHMSString(item.Duration);
          }
          break;

        case "#shortduration":
          label = MediaPortal.Util.Utils.SecondsToShortHMSString(item.Duration);
          break;

        case "#dvdlabel":
          label = item.DVDLabel;
          break;

        case "#year":
          label = item.Year.ToString(CultureInfo.InvariantCulture);
          break;
      }
      return label;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      _timeElapsed += timePassed;
      // If there is no font do not render.
      if (null == _font)
      {
        base.Render(timePassed);
        return;
      }

      // If the control is not visible do not render.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }

      if (_frameLimiter < GUIGraphicsContext.MaxFPS)
      {
        _frameLimiter++;
      }
      else
      {
        _frameLimiter = 1;
      }

      int dwPosY = _positionY;

      // Render the buttons first.
      for (int i = 0; i < _itemsPerPage; i++)
      {
        if (i + _offset < _listItems.Count)
        {
          // render item
          bool gotFocus = _drawFocus && i == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST;
          RenderButton(timePassed, i, _positionX, dwPosY, gotFocus);
        }
        dwPosY += _itemHeight + _spaceBetweenItems;
      }

      // Free unused textures if page has changed
      FreeUnusedThumbnails();

      // Render new item list
      dwPosY = _positionY;
      for (int i = 0; i < _itemsPerPage; i++)
      {
        int dwPosX = _positionX;
        if (i + _offset < _listItems.Count)
        {
          bool gotFocus = _drawFocus && i == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST;

          int iconX;
          int labelX;
          int pinX;

          int ten = 10;
          GUIGraphicsContext.ScaleHorizontal(ref ten);

          switch (_textAlignment)
          {
            case Alignment.ALIGN_RIGHT:
              iconX = dwPosX + _width - _iconOffsetX - _imageWidth;
              labelX = dwPosX;
              pinX = dwPosX + _width - PinIconWidth;
              break;
            default:
              iconX = dwPosX + _iconOffsetX;
              labelX = dwPosX + _imageWidth + ten;
              pinX = dwPosX;
              break;
          }

          // render the icon
          RenderIcon(timePassed, i, iconX, dwPosY + _iconOffsetY, gotFocus);

          // render the text
          RenderLabel(timePassed, i, labelX, dwPosY, gotFocus);

          RenderPinIcon(timePassed, i, pinX, dwPosY, gotFocus);

          item_OnThumbnailRefresh(i, gotFocus);

          dwPosY += _itemHeight + _spaceBetweenItems;
        }
      }

      RenderScrollbar(timePassed);

      if ((_explicitlyEnableScrollLabel || _enableScrollLabel) && ScrollLabelIsScrolling)
      {
        GUIPropertyManager.SetProperty("#scrolling." + _scrollDirection, "yes");
      }
      else
      {
        GUIPropertyManager.SetProperty("#scrolling.up", "no");
        GUIPropertyManager.SetProperty("#scrolling.down", "no");
      }

      if (Focus)
      {
        GUIPropertyManager.SetProperty("#highlightedbutton", string.Empty);
      }
      base.Render(timePassed);
    }

    //public override void Render()

    protected void RenderScrollbar(float timePassed)
    {
      if (_listItems.Count > _itemsPerPage)
      {
        // Render the spin control
        if (_upDownControlVisible)
        {
          _upDownControl.Render(timePassed);
        }

        // Render the vertical scrollbar
        if (_verticalScrollbar != null)
        {
          float fPercent = (float)SelectedListItemIndex / (_listItems.Count - 1) * 100.0f;

          _verticalScrollbar.Height = _itemsPerPage * (_itemHeight + _spaceBetweenItems);
          _verticalScrollbar.Height -= _spaceBetweenItems;

          if ((int)fPercent != (int)_verticalScrollbar.Percentage)
          {
            _verticalScrollbar.Percentage = fPercent;
          }
          _verticalScrollbar.Render(timePassed);
        }
      }
    }

    /// <summary>
    /// Renders the text.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="fPosX">The X position of the text.</param>
    /// <param name="fPosY">The Y position of the text.</param>
    /// <param name="fMaxWidth">The maximum render width.</param>
    /// <param name="dwTextColor">The color of the text.</param>
    /// <param name="strTextToRender">The actual text.</param>
    /// <param name="bScroll">A bool indication if there is scrolling or not.</param>
    /// <param name="timePassed"></param>
    protected void RenderText(float timePassed, int item, float fPosX, float fPosY, float fMaxWidth, long dwTextColor,
                              string strTextToRender, bool bScroll)
    {
      // TODO Unify render text methods into one general rendertext method.
      if (_labelControls1 == null)
      {
        return;
      }
      if (item < 0 || item >= _labelControls1.Count)
      {
        return;
      }

      GUILabelControl label = _labelControls1[item];

      if (label == null)
      {
        return;
      }
      float textWidth = 0;
      float textHeight = 0;
      _font.GetTextExtent(label.Label, ref textWidth, ref textHeight);

      if (_textAlignment == Alignment.ALIGN_RIGHT && textWidth < fMaxWidth)
      {
        label.SetPosition((int)(fPosX + fMaxWidth), (int)fPosY);
      }
      else
      {
        label.SetPosition((int)fPosX, (int)fPosY);
      }

      label.TextColor = dwTextColor;
      label.Label = strTextToRender;
      label.Width = (int)fMaxWidth;
      label.TextAlignment = textWidth < fMaxWidth ? _textAlignment : Alignment.ALIGN_LEFT;
      label.FontName = _fontName;
      RenderText(timePassed, item, label, bScroll);
    }

    /// <summary>
    /// Renders the text.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="item"></param>
    /// <param name="label">The label to render</param>
    /// <param name="bScroll">A bool indication if there is scrolling or not.</param>
    protected void RenderText(float timePassed, int item, GUILabelControl label, bool bScroll)
    {
      float fPosX = label._positionX;
      float fPosY = label._positionY;
      float fMaxWidth = label.Width;
      long dwTextColor = label.TextColor;
      string strTextToRender = label.Label;
      GUIFont font = GUIFontManager.GetFont(label.FontName);

      if (!bScroll || label.TextWidth <= fMaxWidth)
      {
        // don't scroll here => x-position is constant
        label.Render(timePassed);
        return;
      }

      float fTextHeight = 0;
      float fTextWidth = 0;
      font.GetTextExtent(strTextToRender, ref fTextWidth, ref fTextHeight);
      float fWidth = 0;

      float fPosCX = fPosX;
      float fPosCY = fPosY;

      if (fPosCX < 0)
      {
        fPosCX = 0.0f;
      }
      if (fPosCY < 0)
      {
        fPosCY = 0.0f;
      }

      if (fPosCY > GUIGraphicsContext.Height)
      {
        fPosCY = GUIGraphicsContext.Height;
      }

      float fHeight = 60.0f;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
      {
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      }
      if (fHeight <= 0)
      {
        return;
      }

      //float fwidth = fMaxWidth - 5.0f;
      float fwidth = fMaxWidth + 6f;

      if (fwidth < 1)
      {
        return;
      }
      if (fHeight < 1)
      {
        return;
      }

      if (fPosCX <= 0)
      {
        fPosCX = 0;
      }
      if (fPosCY <= 0)
      {
        fPosCY = 0;
      }

      Rectangle clipRect = new Rectangle
                             {
                               X = (int) fPosCX,
                               Y = (int) fPosCY,
                               Width = (int) (fwidth),
                               Height = (int) (fHeight)
                             };
      GUIGraphicsContext.BeginClip(clipRect);

      // scroll
      int iItem = _cursorX + _offset;
      _brackedText = strTextToRender;
      _brackedText += ("  " + _suffix + " ");
      font.GetTextExtent(_brackedText, ref fTextWidth, ref fTextHeight);

      if (fTextWidth > fMaxWidth)
      {
        // scrolling necessary
        fMaxWidth += 50.0f;
        _textLine2 = "";
        if (_lastItem != iItem)
        {
          _scrollPosition = 0;
          _lastItem = iItem;
          _scrollPosititionX = 0;
          _scrollOffsetX = 0.0f;
          _timeElapsed = 0.0f;
          _scrollContinuously = false;
        }
        if ((int)_timeElapsed > _scrollStartDelay || _scrollContinuously)
        {
          // Add an especially slow setting for far distance + small display + bad eyes + foreign language combination
          if (GUIGraphicsContext.ScrollSpeedHorizontal < 3)
          {
            // Advance one pixel every 3 or 2 frames
            if (_frameLimiter % (4 - GUIGraphicsContext.ScrollSpeedHorizontal) == 0)
            {
              _scrollPosititionX++;
            }
          }
          else
          {
            // advance 1 - 3 pixels every frame
            _scrollPosititionX = _scrollPosititionX + (GUIGraphicsContext.ScrollSpeedHorizontal - 2);
          }

          char wTmp = _scrollPosition >= _brackedText.Length ? ' ' : _brackedText[_scrollPosition];

          font.GetTextExtent(wTmp.ToString(CultureInfo.InvariantCulture), ref fWidth, ref fHeight);
          if (_scrollPosititionX - _scrollOffsetX >= fWidth)
          {
            _scrollPosition++;
            if (_scrollPosition > _brackedText.Length)
            {
              _scrollPosition = 0;
              _scrollPosititionX = 0;
              _scrollOffsetX = 0.0f;
              _timeElapsed = 0.0f;
              _scrollContinuously = true;
            }
            else
            {
              _scrollOffsetX += fWidth;
            }
          }
          int ipos = 0;
          for (int i = 0; i < _brackedText.Length; i++)
          {
            if (i + _scrollPosition < _brackedText.Length)
            {
              _textLine2 += _brackedText[i + _scrollPosition];
            }
            else
            {
              if (ipos == 0)
              {
                _textLine2 += ' ';
              }
              else
              {
                _textLine2 += _brackedText[ipos - 1];
              }
              ipos++;
            }
          }

          if (fPosY >= 0.0)
          {
            long tc = GUIGraphicsContext.MergeAlpha((uint)dwTextColor);
            long sc = GUIGraphicsContext.MergeAlpha((uint)_shadowColor);
            if ((_shadowDistance > 0) && ((_shadowColor >> 24) > 0))
            {
              font.DrawShadowTextWidth((int)(fPosX - _scrollPosititionX + _scrollOffsetX),
                            fPosY, tc, _textLine2, Alignment.ALIGN_LEFT,
                            _shadowAngle, _shadowDistance, sc,
                            (int)(fMaxWidth - 50f + _scrollPosititionX - _scrollOffsetX));
            }
            else
            {
              font.DrawText((int)(fPosX - _scrollPosititionX + _scrollOffsetX),
                            fPosY, tc, _textLine2, Alignment.ALIGN_LEFT,
                            (int)(fMaxWidth - 50f + _scrollPosititionX - _scrollOffsetX));
            }
          }
        }
        else if (fPosY >= 0.0)
        {
          long tc = GUIGraphicsContext.MergeAlpha((uint)dwTextColor);
          long sc = GUIGraphicsContext.MergeAlpha((uint)_shadowColor);
          if ((_shadowDistance > 0) && ((_shadowColor >> 24) > 0))
          {
            font.DrawShadowTextWidth(fPosX, fPosY, tc, strTextToRender, Alignment.ALIGN_LEFT, _shadowAngle, _shadowDistance, sc, (int)(fMaxWidth - 50f));
          }
          else
          {
            font.DrawText(fPosX, fPosY, tc, strTextToRender, Alignment.ALIGN_LEFT, (int)(fMaxWidth - 50f));
          }
        }
      }
      GUIGraphicsContext.EndClip();
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          _searchString = "";
          OnPageUp();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          _searchString = "";
          _offset = 0;
          _cursorX = 0;
          _upDownControl.Value = 1;
          OnSelectionChanged();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_END:
          _searchString = "";
          int iItem = _listItems.Count - 1;
          if (iItem >= 0)
          {
            // update spin controls
            int iPage = 1;
            int iSel = iItem;
            while (iSel >= _itemsPerPage)
            {
              iPage++;
              iSel -= _itemsPerPage;
            }
            _upDownControl.Value = iPage;

            // find item
            _offset = 0;
            _cursorX = 0;
            while (iItem >= _itemsPerPage)
            {
              iItem -= _itemsPerPage;
              _offset += _itemsPerPage;
            }
            _cursorX = iItem;

            // Special handling when more than one page
            if (iPage > 1)
            {
              _offset = _listItems.Count - _itemsPerPage;
              _cursorX = _itemsPerPage - 1;
            }
            OnSelectionChanged();
          }
          _refresh = true;
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          _searchString = "";
          OnDown();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          _searchString = "";
          OnUp();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          _searchString = "";
          OnLeft();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          _searchString = "";
          OnRight();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            // Check key
            if (((action.m_key.KeyChar >= '0') && (action.m_key.KeyChar <= '9')) ||
                action.m_key.KeyChar == '*' || action.m_key.KeyChar == '(' || action.m_key.KeyChar == '#' ||
                action.m_key.KeyChar == '§')
            {
              Press((char) action.m_key.KeyChar);
              return;
            }

            if (action.m_key.KeyChar == (int) Keys.Back && _searchString.Length > 0)
            {
              _searchString = _searchString.Remove(_searchString.Length - 1, 1);
              SearchItem(_searchString, SearchType.SEARCH_FIRST);
            }

            if (((action.m_key.KeyChar >= 65) && (action.m_key.KeyChar <= 90)) ||
                (action.m_key.KeyChar == (int) Keys.Space))
            {
              if (action.m_key.KeyChar == (int) Keys.Space && _searchString == string.Empty)
              {
                return;
              }
              _searchString += (char) action.m_key.KeyChar;
              SearchItem(_searchString, SearchType.SEARCH_FIRST);
            }
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          int id;
          bool focus;
          if (_verticalScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
          {
            _drawFocus = false;
            _verticalScrollbar.OnAction(action);
            float fPercentage = _verticalScrollbar.Percentage;
            fPercentage /= 100.0f;
            fPercentage *= _listItems.Count;
            int iChan = (int) fPercentage;
            if (iChan != _offset + _cursorX)
            {
              // update spin controls
              int iPage = 1;
              int iSel = iChan;
              while (iSel >= _itemsPerPage)
              {
                iPage++;
                iSel -= _itemsPerPage;
              }
              _upDownControl.Value = iPage;

              // find item
              _offset = 0;
              _cursorX = 0;
              while (iChan >= _itemsPerPage)
              {
                iChan -= _itemsPerPage;
                _offset += _itemsPerPage;
              }
              _cursorX = iChan;
              _upDownControl.Value = ((_offset + _cursorX)/_itemsPerPage) + 1;
              OnSelectionChanged();
            }
            return;
          }
          _drawFocus = true;
          break;
        
       case Action.ActionType.ACTION_MOUSE_CLICK:
          OnMouseClick(action);
          break;
        
        default:
          OnDefaultAction(action);
          break;
      }
    }

    protected virtual void OnMouseClick(Action action)
    {
      int id;
      bool focus;
      if (_verticalScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
      {
        _drawFocus = false;
        _verticalScrollbar.OnAction(action);
        float fPercentage = _verticalScrollbar.Percentage;
        fPercentage /= 100.0f;
        fPercentage *= _listItems.Count;
        int iChan = (int)fPercentage;
        if (iChan != _offset + _cursorX)
        {
          // update spin controls
          int iPage = 1;
          int iSel = iChan;
          while (iSel >= _itemsPerPage)
          {
            iPage++;
            iSel -= _itemsPerPage;
          }
          _upDownControl.Value = iPage;

          // find item
          _offset = 0;
          _cursorX = 0;
          while (iChan >= _itemsPerPage)
          {
            iChan -= _itemsPerPage;
            _offset += _itemsPerPage;
          }
          _cursorX = iChan;
          OnSelectionChanged();
        }
      }
      else
      {
        int idtmp;
        bool bUpDown = _upDownControl.InControl((int)action.fAmount1, (int)action.fAmount2, out idtmp);
        _drawFocus = true;
        if (!bUpDown && _listType == ListType.CONTROL_LIST)
        {
          _searchString = "";
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                          (int)Action.ActionType.ACTION_SELECT_ITEM, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
        else
        {
          _upDownControl.OnAction(action);
        }
        _refresh = true;
      }
    }

    protected virtual void OnDefaultAction(Action action)
    {
      // by default send a message to parent window that user has done an action on the selected item
      // could be, just enter or f3 for info or 0 to delete or y to queue etc...
      if (_listType == ListType.CONTROL_LIST)
      {
        // don't send the messages to a dialog menu
        if ((WindowId != (int)GUIWindow.Window.WINDOW_DIALOG_MENU) ||
            (action.wID == Action.ActionType.ACTION_SELECT_ITEM))
        {
          _searchString = "";
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                          (int)action.wID, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
      }
      else
      {
        _upDownControl.OnAction(action);
      }
      _refresh = true;
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
        if (message.SenderControlId == 0 && message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
        {
          int iPages = _listItems.Count/_itemsPerPage;
          if ((_listItems.Count%_itemsPerPage) != 0)
          {
            iPages++;
          }
          if (_upDownControl.Value == iPages)
          {
            // Moved to last page, make sure page is filled
            _offset = _listItems.Count - _itemsPerPage;
          }
          else
          {
            _offset = (_upDownControl.Value - 1)*_itemsPerPage;
          }

          while (_offset + _cursorX >= _listItems.Count)
          {
            _cursorX--;
          }
          OnSelectionChanged();
          _refresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < _listItems.Count)
          {
            message.Object = _listItems[iItem];
          }
          else
          {
            message.Object = null;
          }
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = _cursorX + _offset;

          if (iItem >= 0 && iItem < _listItems.Count)
          {
            message.Object = _listItems[iItem];
          }
          else
          {
            message.Object = null;
          }
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS || message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
        {
          if (Disabled || !IsVisible || !CanFocus())
          {
            base.OnMessage(message);
            return true;
          }
          _listType = ListType.CONTROL_LIST;
          _refresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          lock (GUIGraphicsContext.RenderLock)
          {
            GUITextureManager.CleanupThumbs();
            foreach (GUIListItem item in _listItems)
            {
              item.FreeMemory();
            }
            _refresh = true;
            return true;
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem pItem = (GUIListItem)message.Object;
          if (pItem != null)
          {
            Add(pItem);
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          lock (GUIGraphicsContext.RenderLock)
          {
            Clear();
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = _listItems.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          message.Param1 = _cursorX + _offset;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          SelectItem(message.Param1);
          OnSelectionChanged();
          _refresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
        {
          foreach (GUIListItem item in _listItems)
          {
            item.Selected = false;
          }
          foreach (GUIListItem item in _listItems.Where(item => item.Path.Equals(message.Label, StringComparison.OrdinalIgnoreCase)))
          {
            item.Selected = true;
            break;
          }
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING)
      {
        foreach (GUIListItem item in _listItems)
        {
          if (item.IsRemote && message.Label == item.Path)
          {
            item.IsDownloading = true;
            item.Label2 = MediaPortal.Util.Utils.GetSize(message.Param1);
            if (item.FileInfo != null)
            {
              double length = item.FileInfo.Length;
              if (length == 0)
              {
                item.Label2 = "100%";
              }
              else
              {
                double percent = message.Param1/length;
                percent *= 100.0f;
                item.Label2 = String.Format("{0:n}%", percent);
              }
            }
          }
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED)
      {
        foreach (GUIListItem item in _listItems)
        {
          if (item.IsRemote && message.Label == item.Path)
          {
            item.Path = message.Label2;
            item.IsRemote = false;
            item.IsDownloading = false;
            if (item.FileInfo != null)
            {
              item.Label2 = MediaPortal.Util.Utils.GetSize(item.FileInfo.Length);
            }
          }
        }
      }

      return base.OnMessage(message);
    }

    /// <summary>
    /// Select the item and set the Page accordingly 
    /// </summary>
    /// <param name="item"></param>
    private void SelectItem(int item)
    {
      int itemCount = _listItems.Count;
      if (item >= 0 && item < itemCount)
      {
        if (item >= itemCount - (itemCount % _itemsPerPage) && itemCount > _itemsPerPage)
        {
          // Special case, jump to last page, but fill entire page
          _offset = itemCount - _itemsPerPage;
          _cursorX = _itemsPerPage - (itemCount - item);
        }
        else
        {
          _offset = 0;
          _cursorX = item;
          while (_cursorX >= _itemsPerPage)
          {
            _offset += _itemsPerPage;
            _cursorX -= _itemsPerPage;
          }
          if ((_cursorX < _scrollStartOffset) && (_offset >= _scrollStartOffset))
          {
            _offset -= _scrollStartOffset;
            _cursorX += _scrollStartOffset;
          }
          else if ((_cursorX > _itemsPerPage - _scrollStartOffset) && (_cursorX >= _scrollStartOffset))
          {
            _offset += _scrollStartOffset;
            _cursorX -= _scrollStartOffset;
          }
        }
        _upDownControl.Value = ((_offset + _cursorX) / _itemsPerPage) + 1;
      }
    }


    /// <summary>
    /// Search for first item starting with searchkey
    /// </summary>
    /// <param name="searchKey">SearchKey</param>
    /// <param name="searchMethode"></param>
    private void SearchItem(string searchKey, SearchType searchMethode)
    {
      // Get selected item
      bool bItemFound = false;
      int iCurrentItem = _cursorX + _offset;
      if (searchMethode == SearchType.SEARCH_FIRST)
      {
        iCurrentItem = 0;
      }

      int iItem = iCurrentItem;
      do
      {
        if (searchMethode == SearchType.SEARCH_NEXT)
        {
          iItem++;
          if (iItem >= _listItems.Count)
          {
            iItem = 0;
          }
        }

        if (searchMethode == SearchType.SEARCH_PREV && _listItems.Count > 0)
        {
          iItem--;
          if (iItem < 0)
          {
            iItem = _listItems.Count - 1;
          }
        }

        GUIListItem pItem = _listItems[iItem];
        if (pItem.Label.ToUpperInvariant().StartsWith(searchKey.ToUpperInvariant()))
        {
          bItemFound = true;
          break;
        }

        if (searchMethode == SearchType.SEARCH_FIRST)
        {
          iItem++;
          if (iItem >= _listItems.Count)
          {
            iItem = 0;
          }
        }
      } while (iItem != iCurrentItem);

      if (bItemFound)
      {
        SelectItem(iItem);
      }

      _lastSearchItem = _cursorX + _offset;
      OnSelectionChanged();
      _refresh = true;
    }


    /// <summary>
    /// Handle keypress events for SMS style search (key '1'..'9')
    /// </summary>
    /// <param name="key"></param>
    private void Press(char key)
    {
      if (!_enableSMSsearch) return;

      // Check key timeout
      CheckTimer();

      // Check different key pressed
      if ((key != _previousKey) && (key >= '1' && key <= '9'))
      {
        _currentKey = (char)0;
      }

      switch (key)
      {
        case '(':
        case '*':
          if (_searchString.Length > 0)
          {
            _searchString = _searchString.Remove(_searchString.Length - 1, 1);
          }
          _previousKey = (char)0;
          _currentKey = (char)0;
          _timerKey = DateTime.Now;
          break;
        case '§':
        case '#':
          _timerKey = DateTime.Now;
          break;
        case '1':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = ' ';
              break;
            case ' ':
              _currentKey = '!';
              break;
            case '!':
              _currentKey = '?';
              break;
            case '?':
              _currentKey = '.';
              break;
            case '.':
              _currentKey = '0';
              break;
            case '0':
              _currentKey = '1';
              break;
            case '1':
              _currentKey = '-';
              break;
            case '-':
              _currentKey = '+';
              break;
            case '+':
              _currentKey = ' ';
              break;
          }
          break;
        case '2':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'a';
              break;
            case 'a':
              _currentKey = 'b';
              break;
            case 'b':
              _currentKey = 'c';
              break;
            case 'c':
              _currentKey = '2';
              break;
            case '2':
              _currentKey = 'a';
              break;
          }
          break;
        case '3':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'd';
              break;
            case 'd':
              _currentKey = 'e';
              break;
            case 'e':
              _currentKey = 'f';
              break;
            case 'f':
              _currentKey = '3';
              break;
            case '3':
              _currentKey = 'd';
              break;
          }
          break;
        case '4':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'g';
              break;
            case 'g':
              _currentKey = 'h';
              break;
            case 'h':
              _currentKey = 'i';
              break;
            case 'i':
              _currentKey = '4';
              break;
            case '4':
              _currentKey = 'g';
              break;
          }
          break;
        case '5':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'j';
              break;
            case 'j':
              _currentKey = 'k';
              break;
            case 'k':
              _currentKey = 'l';
              break;
            case 'l':
              _currentKey = '5';
              break;
            case '5':
              _currentKey = 'j';
              break;
          }
          break;
        case '6':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'm';
              break;
            case 'm':
              _currentKey = 'n';
              break;
            case 'n':
              _currentKey = 'o';
              break;
            case 'o':
              _currentKey = '6';
              break;
            case '6':
              _currentKey = 'm';
              break;
          }
          break;
        case '7':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'p';
              break;
            case 'p':
              _currentKey = 'q';
              break;
            case 'q':
              _currentKey = 'r';
              break;
            case 'r':
              _currentKey = 's';
              break;
            case 's':
              _currentKey = '7';
              break;
            case '7':
              _currentKey = 'p';
              break;
          }
          break;
        case '8':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 't';
              break;
            case 't':
              _currentKey = 'u';
              break;
            case 'u':
              _currentKey = 'v';
              break;
            case 'v':
              _currentKey = '8';
              break;
            case '8':
              _currentKey = 't';
              break;
          }
          break;
        case '9':
          switch (_currentKey)
          {
            case '\0':
              _currentKey = 'w';
              break;
            case 'w':
              _currentKey = 'x';
              break;
            case 'x':
              _currentKey = 'y';
              break;
            case 'y':
              _currentKey = 'z';
              break;
            case 'z':
              _currentKey = '9';
              break;
            case '9':
              _currentKey = 'w';
              break;
          }
          break;
      }

      if (key >= '1' && key <= '9')
      {
        // Check different key pressed
        if (key == _previousKey && _searchString.Length > 0)
        {
          _searchString = _searchString.Remove(_searchString.Length - 1, 1);
        }
        _previousKey = key;
        _searchString += _currentKey;
      }
      SearchItem(_searchString, SearchType.SEARCH_FIRST);
      _timerKey = DateTime.Now;
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 1000)
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
      }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _upDownControl.PreAllocResources();
      _verticalScrollbar.PreAllocResources();
    }

    protected virtual void AllocButtons()
    {
      for (int i = 0; i < _itemsPerPage; ++i)
      {
        GUIButtonControl cntl = new GUIButtonControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _width,
                                                     _itemHeight, _buttonFocusName, _buttonNonFocusName,
                                                     _shadowAngle, _shadowDistance, _shadowColor) {ParentControl = this};
        cntl.SetBorderTF(_strBorderBF,
                         _borderPositionBF,
                         _borderTextureRepeatBF,
                         _borderTextureRotateBF,
                         _borderTextureFileNameBF,
                         _borderColorKeyBF,
                         _borderHasCornersBF,
                         _borderCornerTextureRotateBF);
        cntl.SetBorderTNF(_strBorderBNF,
                          _borderPositionBNF,
                          _borderTextureRepeatBNF,
                          _borderTextureRotateBNF,
                          _borderTextureFileNameBNF,
                          _borderColorKeyBNF,
                          _borderHasCornersBNF,
                          _borderCornerTextureRotateBNF);
        cntl.AllocResources();
        cntl.DimColor = DimColor;
        _listButtons.Add(cntl);
      }
    }

    protected virtual void ReleaseButtons()
    {
      _listButtons.DisposeAndClear();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      Dispose();
      base.AllocResources();
      _upDownControl.AllocResources();
      _verticalScrollbar.AllocResources();
      _font = GUIFontManager.GetFont(_fontName);
      _font2 = GUIFontManager.GetFont(_fontName2Name);


      float fHeight = (float)_itemHeight + _spaceBetweenItems;
      float fTotalHeight = _height - _upDownControl.Height - 5;
      _itemsPerPage = (int)(fTotalHeight / fHeight);

      _listButtons = new List<GUIControl>();
      _labelControls1 = new List<GUILabelControl>();
      _labelControls2 = new List<GUILabelControl>();
      _labelControls3 = new List<GUILabelControl>();
      AllocButtons();
      for (int i = 0; i < _itemsPerPage; ++i)
      {
        GUILabelControl cntl1 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName, "", _textColor,
                                                    Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP, false,
                                                    _shadowAngle, _shadowDistance, _shadowColor);
        GUILabelControl cntl2 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName2Name, "", _textColor2,
                                                    Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP, false,
                                                    _shadowAngle, _shadowDistance, _shadowColor);
        GUILabelControl cntl3 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName2Name, "", _textColor3,
                                                    Alignment.ALIGN_RIGHT, VAlignment.ALIGN_TOP, false,
                                                    _shadowAngle, _shadowDistance, _shadowColor);
        cntl1.ParentControl = this;
        cntl2.ParentControl = this;
        cntl3.ParentControl = this;
        cntl1.AllocResources();
        cntl2.AllocResources();
        cntl3.AllocResources();
        cntl1.DimColor = DimColor;
        cntl2.DimColor = DimColor;
        cntl3.DimColor = DimColor;
        _labelControls1.Add(cntl1);
        _labelControls2.Add(cntl2);
        _labelControls3.Add(cntl3);
      }

      int iPages = 1;
      if (_listItems.Count > 0)
      {
        iPages = _itemsPerPage == 0 ? 0 : _listItems.Count / _itemsPerPage;
        if (_itemsPerPage != 0)
        {
          if ((_listItems.Count % _itemsPerPage) != 0)
          {
            iPages++;
          }
        }
      }
      _upDownControl.SetRange(1, iPages);
      _upDownControl.Value = 1;
      _upDownControl.DimColor = DimColor;
      _verticalScrollbar.DimColor = DimColor;
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();

      _font = null;
      _font2 = null;
      _font3 = null;

      _upDownControl.SafeDispose();
      _listButtons.DisposeAndClear();
      _verticalScrollbar.SafeDispose();

      _listItems.DisposeAndClear();
      _labelControls1.DisposeAndClear();
      _labelControls2.DisposeAndClear();
      _labelControls3.DisposeAndClear();
    }

    /// <summary>
    /// Implementation of the OnRight action.
    /// </summary>
    protected virtual void OnRight()
    {
      Action action = new Action {wID = Action.ActionType.ACTION_MOVE_RIGHT};
      if (_listType == ListType.CONTROL_LIST)
      {
        if (_upDownControl.GetMaximum() > 1 && _spinCanFocus)
        {
          OnSelectionChanged();
          _listType = ListType.CONTROL_UPDOWN;
          _upDownControl.Focus = true;
          if (!_upDownControl.Focus)
          {
            _listType = ListType.CONTROL_LIST;
          }
        }
        else
        {
          base.OnAction(action);
          OnSelectionChanged();
        }
      }
      else
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_UP)
        {
          _upDownControl.Focus = false;
          Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
        }
        if (!_upDownControl.Focus)
        {
          if (_rightControlId != GetID)
          {
            base.OnAction(action);
          }
          _listType = ListType.CONTROL_LIST;
        }
      }
    }

    /// <summary>
    /// Implementation of the OnLeft action.
    /// </summary>
    protected virtual void OnLeft()
    {
      OnSelectionChanged();
      Action action = new Action {wID = Action.ActionType.ACTION_MOVE_LEFT};
      if (_listType == ListType.CONTROL_LIST)
      {
        base.OnAction(action);
        if (!_upDownControl.Focus)
        {
          _listType = ListType.CONTROL_LIST;
        }
      }
      else
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN)
        {
          _upDownControl.Focus = false;
          Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
        }

        if (!_upDownControl.Focus)
        {
          _listType = ListType.CONTROL_LIST;
        }
      }
    }

    /// <summary>
    /// Implementation of the OnUp action.
    /// </summary>
    protected virtual void OnUp()
    {
      Action action = new Action {wID = Action.ActionType.ACTION_MOVE_UP};
      if (_listType == ListType.CONTROL_LIST)
      {
        _scrollDirection = "up";
        _scrollTimer = DateTime.Now;
        _scrollCounter++;
        if ((_cursorX > _scrollStartOffset) || ((_cursorX > 0) && (_offset == 0)))
        {
          _cursorX--;
          _upDownControl.Value = ((_offset + _cursorX) / _itemsPerPage) + 1;
          OnSelectionChanged();
          _lastCommandTime = AnimationTimer.TickCount;
        }
        else if (_cursorX <= _scrollStartOffset && _offset > 0)
        {
          _offset--;

          int iPage = 1;
          int iSel = _offset + _cursorX;
          while (iSel >= _itemsPerPage)
          {
            iPage++;
            iSel -= _itemsPerPage;
          }
          _upDownControl.Value = iPage;
          OnSelectionChanged();
          _lastCommandTime = AnimationTimer.TickCount;
        }
        else
        {
          if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
          {
            //check if _downControlId is set -> then go to the window
            if (_upControlId > 0)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _upControlId,
                                              (int)action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
            else
            {
              // move 2 last item in list
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID,
                                              _listItems.Count - 1, 0, null);
              OnMessage(msg);
              // Select item adjusts according to scroll offset, so we may need to adjust again here...
              _offset = (_listItems.Count < _itemsPerPage ? 0 : _listItems.Count - _itemsPerPage);
              _cursorX = _itemsPerPage - 1;
              if (_offset + _cursorX >= _listItems.Count)
              {
                _cursorX = (_listItems.Count - _offset) - 1;
              }
            }
          }
        }
      }
      else
      {
        _upDownControl.OnAction(action);
        if (!_upDownControl.Focus)
        {
          _listType = ListType.CONTROL_LIST;
        }
        _lastCommandTime = AnimationTimer.TickCount;
      }
    }

    /// <summary>
    /// Implementation of the OnDown action.
    /// </summary>
    protected virtual void OnDown()
    {
      Action action = new Action {wID = Action.ActionType.ACTION_MOVE_DOWN};
      if (_listType == ListType.CONTROL_LIST)
      {
        _scrollDirection = "down";
        _scrollTimer = DateTime.Now;
        _scrollCounter++;
        if ((_cursorX + 1 + _scrollStartOffset < _itemsPerPage) ||
            (_offset + 1 + _cursorX + _scrollStartOffset >= _listItems.Count))
        {
          if (_offset + 1 + _cursorX < _listItems.Count)
          {
            if (_cursorX + 1 + _scrollStartOffset >= _itemsPerPage &&
                _cursorX + (_itemsPerPage - _cursorX) < _listItems.Count - _offset)
            {
              _offset++;
            }
            else
            {
              _cursorX++;
            }
            _upDownControl.Value = ((_offset + _cursorX) / _itemsPerPage) + 1;
            OnSelectionChanged();
            _lastCommandTime = AnimationTimer.TickCount;
          }
          else
          {
            if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
            {
              //check if _downControlId is set -> then go to the window
              if (_downControlId > 0)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _downControlId,
                                                (int)action.wID, 0, null);
                GUIGraphicsContext.SendMessage(msg);
              }
              else
              {
                // move first item in list
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, 0, 0,
                                                null);
                OnMessage(msg);
              }
            }
          }
        }
        else
        {
          if (_offset + 1 + _cursorX < _listItems.Count)
          {
            _offset++;

            int iPage = 1;
            int iSel = _offset + _cursorX;
            while (iSel >= _itemsPerPage)
            {
              iPage++;
              iSel -= _itemsPerPage;
            }
            _upDownControl.Value = iPage;
            OnSelectionChanged();
            _lastCommandTime = AnimationTimer.TickCount;
          }
          else
          {
            if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
            {
              //check if _downControlId is set -> then go to the window
              if (_downControlId > 0)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _downControlId,
                                                (int)action.wID, 0, null);
                GUIGraphicsContext.SendMessage(msg);
              }
              else
              {
                // move first item in list
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, 0, 0,
                                                null);
                OnMessage(msg);
              }
            }
          }
        }
      }
      else
      {
        _upDownControl.OnAction(action);
        if (!_upDownControl.Focus)
        {
          base.OnAction(action);
        }
        _lastCommandTime = AnimationTimer.TickCount;
      }
    }

    /// <summary>
    /// Get/set the scroll suffic
    /// </summary>
    public String ScrollySuffix
    {
      get { return _suffix; }
      set
      {
        if (value == null)
        {
          return;
        }
        _suffix = value;
      }
    }

    /// <summary>
    /// Implementation of the OnPageUp action.
    /// </summary>
    protected void OnPageUp()
    {
      int iPage = _upDownControl.Value;
      if (iPage > 1)
      {
        int iPages = _listItems.Count / _itemsPerPage;
        int itemsOnLastPage = _listItems.Count % _itemsPerPage;
        if (itemsOnLastPage != 0)
        {
          iPages++;
        }

        iPage--;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1) * _itemsPerPage;
        if ((iPage + 1) == iPages && itemsOnLastPage != 0)
        {
          // Moving up from last page and last page has less then _itemsPerPage items
          _cursorX -= _itemsPerPage - itemsOnLastPage;
        }
      }
      else
      {
        // already on page 1, then select the 1st item
        _cursorX = 0;
        _offset = 0;
      }
      OnSelectionChanged();
    }

    /// <summary>
    /// Implementation of the OnPageDown action.
    /// </summary>
    protected void OnPageDown()
    {
      int iPages = _listItems.Count / _itemsPerPage;
      int itemsOnLastPage = _listItems.Count % _itemsPerPage;
      if (itemsOnLastPage != 0)
      {
        iPages++;
      }

      int iPage = _upDownControl.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        _upDownControl.Value = iPage;
        if (iPage + 1 <= iPages)
        {
          _offset = (_upDownControl.Value - 1) * _itemsPerPage;
        }
        else
        {
          // Moving to last page, make sure list is filled
          _offset = _listItems.Count - _itemsPerPage;
          // Select correct item
          if (itemsOnLastPage != 0)
          {
            _cursorX += _itemsPerPage - itemsOnLastPage;
          }
        }
      }
      else
      {
        // already on last page, move 2 last item in list
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, WindowId, GetID,
                                        _listItems.Count - 1, 0, null);
        OnMessage(msg);
        // Select item adjusts according to scroll offset, so we may need to adjust again here...
        _offset = (_listItems.Count < _itemsPerPage ? 0 : _listItems.Count - _itemsPerPage);
        _cursorX = _itemsPerPage - 1;
      }
      if (_offset + _cursorX >= _listItems.Count)
      {
        _cursorX = (_listItems.Count - _offset) - 1;
      }
      OnSelectionChanged();
    }

    /// <summary>
    /// Sets the offsets of the text.
    /// </summary>
    /// <param name="iXoffset">The X offset of the first label.</param>
    /// <param name="iYOffset">The Y offset of the first label.</param>
    /// <param name="iXoffset2">The X offset of the second label.</param>
    /// <param name="iYOffset2">The Y offset of the second label.</param>
    /// <param name="iXoffset3">The X offset of the third label.</param>
    /// <param name="iYOffset3">The Y offset of the third label.</param>
    public void SetTextOffsets(int iXoffset, int iYOffset, int iXoffset2, int iYOffset2, int iXoffset3, int iYOffset3)
    {
      if (iXoffset < 0 || iYOffset < 0 || iXoffset2 < 0 || iYOffset2 < 0 || iXoffset3 < 0 || iYOffset3 < 0)
      {
        return;
      }

      _textOffsetX = iXoffset;
      _textOffsetY = iYOffset;
      _textOffsetX2 = iXoffset2;
      _textOffsetY2 = iYOffset2;
      _textOffsetX3 = iXoffset3;
      _textOffsetY3 = iYOffset3;
    }

    /// <summary>
    /// Sets the dimension of the images of the items.
    /// </summary>
    /// <param name="iWidth">The width.</param>
    /// <param name="iHeight">The height.</param>
    public void SetImageDimensions(int iWidth, int iHeight)
    {
      if (iWidth  < 0 || iHeight < 0)
      {
        return;
      }

      _imageWidth = iWidth;
      _imageHeight = iHeight;
    }

    /// <summary>
    /// Get/set the height of an item.
    /// </summary>
    public int ItemHeight
    {
      get { return _itemHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _itemHeight = value;
      }
    }

    /// <summary>
    /// Get/set the space between items.
    /// </summary>
    public int Space
    {
      get { return _spaceBetweenItems; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _spaceBetweenItems = value;
      }
    }

    /// <summary>
    /// Get/set the font for the second label.
    /// </summary>
    public string Font2
    {
      get { return _fontName2Name; }
      set
      {
        if (value == null)
        {
          return;
        }
        if (value != "")
        {
          _fontName2Name = value;
          _font2 = GUIFontManager.GetFont(value);
          if (null == _font2)
          {
            _fontName2Name = _fontName;
            _font2 = GUIFontManager.GetFont(_fontName);
          }
        }
        else
        {
          _fontName2Name = _fontName;
          _font2 = GUIFontManager.GetFont(_fontName);
        }
      }
    }

    /// <summary>
    /// Get/set the font for the second label.
    /// </summary>
    public string Font3
    {
      get { return _fontName3Name; }
      set
      {
        if (value == null)
        {
          return;
        }
        if (value != "")
        {
          _fontName3Name = value;
          _font3 = GUIFontManager.GetFont(value);
          if (null == _font3)
          {
            _fontName3Name = _fontName2Name;
            _font3 = GUIFontManager.GetFont(_fontName2Name);
          }
        }
        else
        {
          _fontName3Name = _fontName2Name;
          _font3 = GUIFontManager.GetFont(_fontName2Name);
        }
      }
    }

    /// <summary>
    /// Set the colors of the second label.
    /// </summary>
    /// <param name="dwTextColor"></param>
    /// <param name="dwSelectedColor"></param>
    public void SetColors2(long dwTextColor, long dwSelectedColor)
    {
      _textColor2 = dwTextColor;
      _selectedColor2 = dwSelectedColor;
    }

    /// <summary>
    /// Set the colors of the second label.
    /// </summary>
    /// <param name="dwTextColor"></param>
    /// <param name="dwSelectedColor"></param>
    public void SetColors3(long dwTextColor, long dwSelectedColor)
    {
      _textColor3 = dwTextColor;
      _selectedColor3 = dwSelectedColor;
    }

    public int GetSelectedItem(ref string strLabel, ref string strLabel2, ref string strThumb, ref string strIndex)
    {
      strLabel = "";
      strLabel2 = "";
      strThumb = "";
      strIndex = "";
      int iItem = _cursorX + _offset;
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        GUIListItem pItem = _listItems[iItem];
        if (string.IsNullOrEmpty(pItem.ThumbnailImage))
        {
          MediaPortal.Util.Utils.SetDefaultIcons(pItem);
          strThumb = pItem.IconImageBig;
        }
        else
        {
          strThumb = pItem.ThumbnailImage;
        }
        strLabel = pItem.Label;
        strLabel2 = pItem.Label2;
        int index = iItem;

        if (_listItems.Count > 0 && _listItems[0].Label != "..")
        {
          index++;
        }
        strIndex = pItem.Label == ".." ? "" : index.ToString(CultureInfo.InvariantCulture);

        if (pItem.IsFolder)
        {
          strLabel = String.Format("{0}{1}{2}", _folderPrefix, pItem.Label, _folderSuffix);
        }
      }
      return iItem;
    }

    public int Count
    {
      get { return _listItems.Count; }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (index < 0 || index >= _listItems.Count)
        {
          return null;
        }
        return _listItems[index];
      }
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        int iItem = _cursorX + _offset;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          GUIListItem pItem = _listItems[iItem];
          return pItem;
        }
        return null;
      }
    }

    public int SelectedListItemIndex
    {
      get
      {
        int iItem = _cursorX + _offset;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          return iItem;
        }
        return -1;
      }
      set
      {
        if (value >= 0 && value < _listItems.Count)
        {
          int iPage = 1;
          _offset = 0;
          _cursorX = value;
          while (_cursorX >= _itemsPerPage)
          {
            iPage++;
            _offset += _itemsPerPage;
            _cursorX -= _itemsPerPage;
          }
          _upDownControl.Value = iPage;
          SelectItem(value);
          OnSelectionChanged();
        }
        _refresh = true;
      }
    }

    /// <summary>
    /// Set the visibility of the page control.
    /// </summary>
    /// <param name="bVisible">true if visible false otherwise</param>
    public void SetPageControlVisible(bool bVisible)
    {
      _upDownControlVisible = bVisible;
    }

    /// <summary>
    /// Get/set the shaded color.
    /// </summary>
    public long ShadedColor
    {
      get { return _shadedColor; }
      set { _shadedColor = value; }
    }

    /// <summary>
    /// Get the color of the first label.
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
    }

    /// <summary>
    /// Get the color of the second label.
    /// </summary>
    public long TextColor2
    {
      get { return _textColor2; }
    }

    /// <summary>
    /// Get the color of the third label.
    /// </summary>
    public long TextColor3
    {
      get { return _textColor3; }
    }

    /// <summary>
    /// Get the color of the text of the first label of a selected item.
    /// </summary>
    public long SelectedColor
    {
      get { return _selectedColor; }
    }

    /// <summary>
    /// Get the color of the text of the second label of a selected item.
    /// </summary>
    public long SelectedColor2
    {
      get { return _selectedColor2; }
    }

    /// <summary>
    /// Get the color of the text of the second label of a third item.
    /// </summary>
    public long SelectedColor3
    {
      get { return _selectedColor3; }
    }

    /// <summary>
    /// Get the fontname of the first label.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
    }

    /// <summary>
    /// Get the fontname of the second label.
    /// </summary>
    public string FontName2
    {
      get { return _fontName2Name; }
    }

    // TODO
    public int SpinWidth
    {
      get { return _upDownControl.Width / 2; }
    }

    // TODO
    public int SpinHeight
    {
      get { return _upDownControl.Height; }
    }

    /// <summary>
    /// Gets the name of the unfocused up texture.
    /// </summary>
    public string TexutureUpName
    {
      get { return _upDownControl.TexutureUpName; }
    }

    /// <summary>
    /// Gets the name of the unfocused down texture.
    /// </summary>
    public string TexutureDownName
    {
      get { return _upDownControl.TexutureDownName; }
    }

    /// <summary>
    /// Gets the name of the focused up texture.
    /// </summary>
    public string TexutureUpFocusName
    {
      get { return _upDownControl.TexutureUpFocusName; }
    }

    /// <summary>
    /// Gets the name of the focused down texture.
    /// </summary>
    public string TexutureDownFocusName
    {
      get { return _upDownControl.TexutureDownFocusName; }
    }

    // TODO
    public long SpinTextColor
    {
      get { return _upDownControl.TextColor; }
    }

    // TODO
    public int SpinX
    {
      get { return _upDownControl.XPosition; }
    }

    // TODO
    public int SpinY
    {
      get { return _upDownControl.YPosition; }
      set { _upDownControl.YPosition = value; }
    }

    /// <summary>
    /// Gets the X offset of the first label.
    /// </summary>
    public int TextOffsetX
    {
      get { return _textOffsetX; }
    }

    /// <summary>
    /// Gets they Y offset of the first label.
    /// </summary>
    public int TextOffsetY
    {
      get { return _textOffsetY; }
    }

    /// <summary>
    /// Gets the X offset of the second label.
    /// </summary>
    public int TextOffsetX2
    {
      get { return _textOffsetX2; }
    }

    /// <summary>
    /// Gets the X offset of the third label.
    /// </summary>
    public int TextOffsetX3
    {
      get { return _textOffsetX3; }
    }

    /// <summary>
    /// Gets they Y offset of the third label.
    /// </summary>
    public int TextOffsetY3
    {
      get { return _textOffsetY3; }
    }

    /// <summary>
    /// Gets they Y offset of the first label.
    /// </summary>
    public int TextOffsetY2
    {
      get { return _textOffsetY2; }
    }

    /// <summary>
    /// Gets they X offset of the icon.
    /// </summary>
    public int IconOffsetX
    {
      get { return _iconOffsetX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _iconOffsetX = value;
      }
    }

    /// <summary>
    /// Gets they Y offset of the icon.
    /// </summary>
    public int IconOffsetY
    {
      get { return _iconOffsetY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _iconOffsetY = value;
      }
    }

    public bool TextVisible1
    {
      get { return _text1Visible; }
      set { _text1Visible = value; }
    }

    public bool TextVisible2
    {
      get { return _text2Visible; }
      set { _text2Visible = value; }
    }

    public bool TextVisible3
    {
      get { return _text3Visible; }
      set { _text3Visible = value; }
    }

    /// <summary>
    /// Gets the width of the images of the items. 
    /// </summary>
    public int ImageWidth
    {
      get { return _imageWidth; }
    }

    /// <summary>
    /// Gets the height of the images of the items. 
    /// </summary>
    public int ImageHeight
    {
      get { return _imageHeight; }
    }

    /// <summary>
    /// Gets the name of the texture for the focused item.
    /// </summary>
    public string ButtonFocusName
    {
      get { return _buttonFocusName; }
    }

    /// <summary>
    /// Gets the name of the texture for the unfocused item.
    /// </summary>
    public string ButtonNoFocusName
    {
      get { return _buttonNonFocusName; }
    }

    /// <summary>
    /// Checks if the x and y coordinates correspond to the current control.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="controlID"></param>
    /// <param name="focused"></param>
    /// <returns>True if the control was hit.</returns>
    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      int id;
      bool focus;
      if (_verticalScrollbar.HitTest(x, y, out id, out focus))
      {
        return true;
      }

      if (_upDownControl.HitTest(x, y, out id, out focus))
      {
        if (_upDownControl.GetMaximum() > 1)
        {
          _listType = ListType.CONTROL_UPDOWN;
          _upDownControl.Focus = true;
          if (!_upDownControl.Focus)
          {
            _listType = ListType.CONTROL_LIST;
          }
          return true;
        }
        return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        return false;
      }
      _listType = ListType.CONTROL_LIST;
      y -= _positionY;
      _cursorX = (y / (_itemHeight + _spaceBetweenItems));
      while (_offset + _cursorX >= _listItems.Count)
      {
        _cursorX--;
      }
      if (_cursorX >= _itemsPerPage)
      {
        _cursorX = _itemsPerPage - 1;
      }
      _upDownControl.Value = ((_offset + _cursorX) / _itemsPerPage) + 1;
      OnSelectionChanged();
      _refresh = true;

      return true;
    }

    /// <summary>
    /// Sorts the list of items in this control.
    /// </summary>
    /// <param name="comparer">The comparer on which the sort is based.</param>
    public void Sort(IComparer<GUIListItem> comparer)
    {
      try
      {
        _listItems.Sort(comparer);
        _enableScrollLabel = false;
      }
      catch (Exception) {}
      _refresh = true;
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not.
    /// </summary>
    /// <returns>true or false</returns>
    public override bool NeedRefresh()
    {
      bool bRefresh = _refresh;
      _refresh = false;
      return bRefresh;
    }

    /// <summary>
    /// Get/set if the aspect ratio of the images of the items needs to be kept.
    /// </summary>
    public bool KeepAspectRatio
    {
      get { return _keepAspectRatio; }
      set { _keepAspectRatio = value; }
    }

    /// <summary>
    /// Gets the vertical scrollbar.
    /// </summary>
    public GUIVerticalScrollbar Scrollbar
    {
      get { return _verticalScrollbar; }
    }

    /// <summary>
    /// Get/set if the control has the focus.
    /// </summary>
    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (IsFocused != value)
        {
          base.Focus = value;
          _drawFocus = true;
        }
      }
    }

    public void Add(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      if (WordWrap)
      {
        ArrayList wrappedLines;
        int dMaxWidth = (_width - _imageWidth - 16);

        WordWrapText(item.Label, dMaxWidth, out wrappedLines);
        foreach (string line in wrappedLines)
        {
          _listItems.Add(new GUIListItem(line));
        }
      }
      else
      {
        _listItems.Add(item);
      }
      int iPages = _itemsPerPage == 0 ? 0 : _listItems.Count / _itemsPerPage;
      if (_itemsPerPage != 0)
      {
        if ((_listItems.Count % _itemsPerPage) != 0)
        {
          iPages++;
        }
      }
      _upDownControl.SetRange(1, iPages);
      _upDownControl.Value = 1;
      _refresh = true;
    }

    public void Insert(int index, GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      if (WordWrap)
      {
        ArrayList wrappedLines;
        int dMaxWidth = (_width - _imageWidth - 16);

        WordWrapText(item.Label, dMaxWidth, out wrappedLines);
        foreach (string line in wrappedLines)
        {
          _listItems.Insert(index, new GUIListItem(line));
          index++;
        }
      }
      else
      {
        _listItems.Insert(index, item);
      }
      int iPages = _itemsPerPage == 0 ? 0 : _listItems.Count / _itemsPerPage;
      if (_itemsPerPage != 0)
      {
        if ((_listItems.Count % _itemsPerPage) != 0)
        {
          iPages++;
        }
      }
      _upDownControl.SetRange(1, iPages);
      _upDownControl.Value = 1;
      _refresh = true;
    }

    public int PinIconOffsetX
    {
      get { return _xOffsetPinIcon; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _xOffsetPinIcon = value;
      }
    }

    public int PinIconOffsetY
    {
      get { return _yOffsetPinIcon; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _yOffsetPinIcon = value;
      }
    }

    public int PinIconWidth
    {
      get { return _widthPinIcon; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _widthPinIcon = value;
      }
    }

    public int PinIconHeight
    {
      get { return _heightPinIcon; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _heightPinIcon = value;
      }
    }

    public void ScrollToEnd()
    {
      while (_offset + 1 + _cursorX < _listItems.Count)
      {
        OnDown();
      }
    }


    /// <summary>
    /// Gets the ID of the control.
    /// </summary>
    public override int GetID
    {
      get { return _controlId; }
      set
      {
        _controlId = value;
        if (_upDownControl != null)
        {
          _upDownControl.ParentID = value;
        }
      }
    }

    public bool WordWrap { get; set; }

    private void WordWrapText(string strText, int iMaxWidth, out ArrayList wrappedLines)
    {
      wrappedLines = new ArrayList();
      GUILabelControl cntl1 = new GUILabelControl(_controlId, 0, 0, 0, GUIGraphicsContext.Width,
                                                  GUIGraphicsContext.Height, _fontName, "", _textColor,
                                                  Alignment.ALIGN_LEFT, VAlignment.ALIGN_TOP, false,
                                                  _shadowAngle, _shadowDistance, _shadowColor) {ParentControl = this};
      cntl1.AllocResources();

      // start wordwrapping
      // Set a flag so we can determine initial justification effects
      //bool bStartingNewLine = true;
      //bool bBreakAtSpace = false;
      int pos = 0;
      int lpos = 0;
      int iLastSpace = -1;
      int iLastSpaceInLine = -1;
      string szLine = "";
      strText = strText.Replace("\r", " ");
      strText.Trim();
      while (pos < strText.Length)
      {
        // Get the current letter in the string
        char letter = strText[pos];

        // Handle the newline character
        if (letter == '\n')
        {
          if (szLine.Length > 0 || _listItems.Count > 0)
          {
            wrappedLines.Add(szLine);
          }
          iLastSpace = -1;
          iLastSpaceInLine = -1;
          lpos = 0;
          szLine = "";
        }
        else
        {
          if (letter == ' ')
          {
            iLastSpace = pos;
            iLastSpaceInLine = lpos;
          }

          if (lpos < 0 || lpos > 1023)
          {
            //OutputDebugString("ERRROR\n");
          }
          szLine += letter;

          string wsTmp = szLine;
          cntl1.Label = wsTmp;
          if (cntl1.TextWidth > iMaxWidth)
          {
            if (iLastSpace > 0 && iLastSpaceInLine != lpos)
            {
              szLine = szLine.Substring(0, iLastSpaceInLine);
              pos = iLastSpace;
            }
            if (szLine.Length > 0 || _listItems.Count > 0)
            {
              wrappedLines.Add(szLine);
            }
            iLastSpaceInLine = -1;
            iLastSpace = -1;
            lpos = 0;
            szLine = "";
          }
          else
          {
            lpos++;
          }
        }
        pos++;
      }
      if (lpos > 0)
      {
        wrappedLines.Add(szLine);
      }
      cntl1.SafeDispose();
    }

    public override int WindowId
    {
      get { return _windowId; }
      set
      {
        _windowId = value;
        if (_upDownControl != null)
        {
          _upDownControl.WindowId = value;
        }
      }
    }

    public void Clear()
    {
      _cursorX = 0;
      _offset = 0;
      _listItems.DisposeAndClear();
      //GUITextureManager.CleanupThumbs();
      _upDownControl.SetRange(1, 1);
      _upDownControl.Value = 1;
      _frameLimiter = 1;
      _refresh = true;
      OnSelectionChanged();
    }

    public Rectangle SelectedRectangle
    {
      get
      {
        int selectedIndex = _cursorX + _offset;

        if (selectedIndex == -1 || selectedIndex >= _listItems.Count)
        {
          return new Rectangle(XPosition, YPosition, Width, Height);
        }
        GUIControl btn = _listButtons[_cursorX];

        return new Rectangle(btn.XPosition, btn.YPosition, btn.Width, btn.Height);
      }
    }


    public virtual int MoveItemDown(int iItem)
    {
      int selectedItemIndex;

      if (iItem < 0 || iItem >= _listItems.Count)
      {
        return -1;
      }

      int iNextItem = iItem + 1;

      if (iNextItem >= _listItems.Count)
      {
        iNextItem = 0;
      }

      GUIListItem item1 = _listItems[iItem];
      GUIListItem item2 = _listItems[iNextItem];

      if (item1 == null || item2 == null)
      {
        return -1;
      }

      try
      {
        //Log.Info("Moving List Item {0} down. Old index:{1}, new index{2}", item1.Path, iItem, iNextItem);
        Monitor.Enter(this);
        _listItems[iItem] = item2;
        _listItems[iNextItem] = item1;
        selectedItemIndex = iNextItem;
      }
      catch (Exception ex)
      {
        Log.Info("GUIListControl.MoveItemDown caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public virtual int MoveItemUp(int iItem)
    {
      int selectedItemIndex;

      if (iItem < 0 || iItem >= _listItems.Count)
      {
        return -1;
      }

      int iPreviousItem = iItem - 1;

      if (iPreviousItem < 0)
      {
        iPreviousItem = _listItems.Count - 1;
      }

      GUIListItem item1 = _listItems[iItem];
      GUIListItem item2 = _listItems[iPreviousItem];

      if (item1 == null || item2 == null)
      {
        return -1;
      }

      try
      {
        //Log.Info("Moving List Item {0} up. Old index:{1}, new index{2}", item1.Path, iItem, iPreviousItem);
        Monitor.Enter(this);
        _listItems[iItem] = item2;
        _listItems[iPreviousItem] = item1;
        selectedItemIndex = iPreviousItem;
      }
      catch (Exception ex)
      {
        Log.Info("GUIListControl.MoveItemUp caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public virtual int RemoveItem(int iItem)
    {
      int selectedItemIndex = -1;

      if (iItem < 0 || iItem >= _listItems.Count)
      {
        return -1;
      }

      try
      {
        //Log.Info("Moving List Item {0} up. Old index:{1}, new index{2}", item1.Path, iItem, iPreviousItem);
        Monitor.Enter(this);
        _listItems.RemoveAt(iItem);
        if (selectedItemIndex >= _listItems.Count)
        {
          selectedItemIndex = _listItems.Count - 1;
        }
      }
      catch (Exception ex)
      {
        Log.Info("GUIListControl.RemoveItem caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_upDownControl != null)
        {
          _upDownControl.DimColor = value;
        }
        if (_verticalScrollbar != null)
        {
          _verticalScrollbar.DimColor = value;
        }
        //foreach (GUIButtonControl ctl in _listButtons) ctl.DimColor = value;
        foreach (GUIListItem item in _listItems)
        {
          item.DimColor = value;
        }
        foreach (GUILabelControl ctl in _labelControls1)
        {
          ctl.DimColor = value;
        }
        foreach (GUILabelControl ctl in _labelControls2)
        {
          ctl.DimColor = value;
        }
        foreach (GUILabelControl ctl in _labelControls3)
        {
          ctl.DimColor = value;
        }
      }
    }

    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    public Alignment Text3Alignment
    {
      get { return _text3Alignment; }
      set { _text3Alignment = value; }
    }

    public ListType TypeOfList
    {
      get { return _listType; }
      set { _listType = value; }
    }

    public int ItemsPerPage
    {
      get { return _itemsPerPage; }
      set { _itemsPerPage = value; }
    }

    public int Offset
    {
      get { return _offset; }
      set { _offset = value; }
    }

    public int CursorX
    {
      get { return _cursorX; }
      set { _cursorX = value; }
    }

    public bool DrawFocus
    {
      get { return _drawFocus; }
      set { _drawFocus = value; }
    }

    public List<GUIListItem> ListItems
    {
      get { return _listItems; }
      set { _listItems = value; }
    }

    public long PlayedColor
    {
      get { return _playedColor; }
      set { _playedColor = value; }
    }

    public long RemoteColor
    {
      get { return _remoteColor; }
      set { _remoteColor = value; }
    }

    public long DownloadColor
    {
      get { return _downloadColor; }
      set { _downloadColor = value; }
    }

    public long BdDvdDirectoryColor
    {
      get { return _bdDvdDirectoryColor; }
      set { _bdDvdDirectoryColor = value; }
    }

    public string Text3Content
    {
      get { return _text3Content; }
      set { _text3Content = value; }
    }

    public bool EnableSMSsearch
    {
      get { return _enableSMSsearch; }
      set { _enableSMSsearch = value; }
    }

    public bool EnableScrollLabel
    {
      get { return _enableScrollLabel; }
      set { _enableScrollLabel = value; }
    }

    private bool ScrollLabelIsScrolling
    {
      get
      {
        TimeSpan ts = DateTime.Now - _scrollTimer;
        bool result = (ts.TotalMilliseconds < 100);
        //how close keypresses should be, before they trigger the label showing
        if (!result)
          _scrollCounter = 0;
        return result && (_scrollCounter >= _scrollCounterLimit);
        //we also take in account how many scrolls passed so far
      }
    }

    public void SetNeedRefresh()
    {
      _refresh = true;
    }
  }
}