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
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Microsoft.DirectX.Direct3D;
using MediaPortal.ExtensionMethods;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIThumbnailPanel : GUIControl
  {
    private const int SLEEP_FRAME_COUNT = 1;
    private const int THUMBNAIL_OVERSIZED_DIVIDER = 32;

    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    } ;

    #region Skin Elements

    [XMLSkinElement("remoteColor")] protected long _remoteColor = 0xffff0000;
    [XMLSkinElement("playedColor")] protected long _playedColor = 0xffa0d0ff;
    [XMLSkinElement("downloadColor")] protected long _downloadColor = 0xff00ff00;

    [XMLSkinElement("thumbPosX")] protected int _xPositionThumbNail = 8;
    [XMLSkinElement("thumbPosY")] protected int _yPositionThumbNail = 8;
    [XMLSkinElement("thumbWidth")] protected int _thumbNailWidth = 64;
    [XMLSkinElement("thumbHeight")] protected int _thumbNailHeight = 64;

    [XMLSkinElement("itemHeight")] protected int _itemHeight;
    [XMLSkinElement("itemWidth")] protected int _itemWidth;

    [XMLSkinElement("textureHeight")] protected int _textureHeight;
    [XMLSkinElement("textureWidth")] protected int _textureWidth;

    [XMLSkinElement("itemHeightBig")] protected int _bigItemHeight = 150;
    [XMLSkinElement("itemWidthBig")] protected int _bigItemWidth = 150;
    [XMLSkinElement("thumbWidthBig")] protected int _bigThumbWidth = 80;
    [XMLSkinElement("thumbHeightBig")] protected int _bigThumbHeight = 80;
    [XMLSkinElement("thumbZoom")] protected bool _zoom = false;
    [XMLSkinElement("enableFocusZoom")] protected bool _enableFocusZoom = true;
    [XMLSkinElement("textureHeightBig")] protected int _bigTextureHeight = 128;
    [XMLSkinElement("textureWidthBig")] protected int _bigTextureWidth = 128;

    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")] protected long _selectedColor = 0xFFFFFFFF;

    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;

    [XMLSkinElement("spinColor")] protected long _spinControlColor;
    [XMLSkinElement("spinAlign")] protected Alignment _spinControlAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("spinHeight")] protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")] protected int _spinControlWidth;
    [XMLSkinElement("spinPosX")] protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")] protected int _spinControlPositionY;

    [XMLSkinElement("scrollbarbg")] protected string _scrollbarBackGroundTextureName = "";
    [XMLSkinElement("scrollbartop")] protected string _scrollbarTopTextureName = "";
    [XMLSkinElement("scrollbarbottom")] protected string _scrollbarBottomTextureName = "";
    [XMLSkinElement("scrollbarwidth")] protected int _scrollbarWidth = 15;
    [XMLSkinElement("scrollbarXOff")] protected int _scrollbarXOff = 0;

    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = 1;
    [XMLSkinElement("scrollOffset")] protected int _scrollStartOffset = 0;
    // this is the offset from the first or last element on screen when scrolling should start

    [XMLSkinElement("textureUp")] protected string _upTextureName = "";
    [XMLSkinElement("textureDown")] protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus = "";
    [XMLSkinElement("imageFolder")] protected string _imageFolderName = "";
    [XMLSkin("imageFolder", "mask")] protected string _imageFolderMask = "";
    [XMLSkinElement("imageFolderFocus")] protected string _imageFolderNameFocus = "";
    [XMLSkin("imageFolderFocus", "mask")] protected string _imageFolderFocusMask = "";

    [XMLSkinElement("thumbPosXBig")] protected int _positionXThumbBig = 0;
    [XMLSkinElement("thumbPosYBig")] protected int _positionYThumbBig = 0;
    [XMLSkinElement("thumbWidthBig")] protected int _widthThumbBig = 0;
    [XMLSkinElement("thumbHeightBig")] protected int _heightThumbBig = 0;

    [XMLSkinElement("folderPrefix")] protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")] protected string _folderSuffix = "]";

    [XMLSkinElement("textXOff")] protected int _textXOff = 0;
    [XMLSkinElement("textYOff")] protected int _textYOff = 0;

    [XMLSkinElement("zoomXPixels")] protected int _zoomXPixels = 0;
    [XMLSkinElement("zoomYPixels")] protected int _zoomYPixels = 0;
    [XMLSkinElement("hideUnfocusTexture")] protected bool _hideUnfocusTexture = false;
    [XMLSkinElement("renderFocusText")] protected bool _renderFocusText = true;
    [XMLSkinElement("renderUnfocusText")] protected bool _renderUnfocusText = true;

    [XMLSkinElement("unfocusedAlpha")] protected int _unfocusedAlpha = 0xFF;
    [XMLSkinElement("spinCanFocus")] protected bool _spinCanFocus = true;

    [XMLSkinElement("frameNoFocus")] protected string _frameNoFocusName = "";
    [XMLSkin("frameNoFocus", "mask")] protected string _frameNoFocusMask = "";
    [XMLSkinElement("frameFocus")] protected string _frameFocusName = "";
    [XMLSkin("frameFocus", "mask")] protected string _frameFocusMask = "";
    [XMLSkinElement("showFrame")] protected bool _showFrame = true;
    [XMLSkinElement("keepaspectratio")] protected bool _keepAspectRatio = true;

    [XMLSkinElement("textureMask")] protected string _textureMask = "";

    [XMLSkinElement("bdDvdDirectoryColor")] protected long _bdDvdDirectoryColor = 0xFFFFFFFF;

    #endregion

    protected int _lowItemHeight;
    protected int _lowItemWidth;

    protected int _lowTextureHeight;
    protected int _lowTextureWidth;

    protected double _lastCommandTime = 0;
    protected int _loopDelay = 0;

    protected List<GUIButtonControl> _listButtons = null;
    protected List<GUIFadeLabel> _listLabels = null;

    private int _xPositionThumbNailLow = 0;
    private int _yPositionThumbNailLow = 0;
    private int _widthThumbNailLow = 0;
    private int _heightThumbNailLow = 0;

    private bool _showTexture = true;
    private int _offset = 0;
    private int _lLastItemPageValues = 0;

    private GUIListControl.ListType m_iSelect = GUIListControl.ListType.CONTROL_LIST;
    private int _cursorX = 0;
    private int _cursorY = 0;
    private int _rowCount;
    private int _columnCount;
    private bool _scrollingUp = false;
    private bool _scrollingDown = false;
    private bool _scrollingDownLast = false;
    private bool _compensatingDown, _compensatingUp = false;
    private int _scrollCounter = 0;
    private string _suffix = "|";
    private GUIFont _font = null;
    private GUISpinControl _controlUpDown = null;

    private List<GUIListItem> _listItems = new List<GUIListItem>();
    private int _frames = 6;
    private int _sleeper = 0;
    private bool _refresh = false;
    protected GUIVerticalScrollbar _verticalScrollBar = null;
    protected string _brackedText;
    protected string _scrollText;

    protected double _scrollOffset = 0.0f;
    protected double _timeElapsed = 0.0f;
    protected bool _scrollContinuously = false;
    private int _frameLimiter = 1;
    // Search
    private DateTime _keyTimer = DateTime.Now;
    private char _currentKey = (char)0;
    private char _previousKey = (char)0;
    protected string _searchString = "";
    protected int _lastSearchItem = 0;
    protected bool _enableSMSsearch = true;

    protected GUIAnimation _frameNoFocusControl = null;
    protected GUIAnimation _frameFocusControl = null;

    protected List<VisualEffect> _allThumbAnimations = new List<VisualEffect>();

    public GUIThumbnailPanel(int dwParentID)
      : base(dwParentID) {}

    public GUIThumbnailPanel(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                             string strImageIcon,
                             string strImageIconFocus,
                             int dwitemWidth, int dwitemHeight,
                             int dwSpinWidth, int dwSpinHeight,
                             string strUp, string strDown,
                             string strUpFocus, string strDownFocus,
                             long dwSpinColor, int dwSpinX, int dwSpinY,
                             string strFont, long dwTextColor, long dwSelectedColor,
                             string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom,
                             int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      // Please fix or remove this check: (dwPosY > dwPosY - always false)
      //if (dwPosY > dwPosY && dwSpinY < dwPosY + dwHeight) dwSpinY = dwPosY + dwHeight;
      _imageFolderName = strImageIcon;
      _imageFolderNameFocus = strImageIconFocus;
      _itemWidth = dwitemWidth;
      _itemHeight = dwitemHeight;
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
      _textColor = dwTextColor;
      _selectedColor = dwSelectedColor;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;

      _scrollbarBackGroundTextureName = strScrollbarBackground;
      _scrollbarTopTextureName = strScrollbarTop;
      _scrollbarBottomTextureName = strScrollbarBottom;
      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _lowItemHeight = _itemHeight;
      _lowItemWidth = _itemWidth;
      _lowTextureWidth = _textureWidth;
      _lowTextureHeight = _textureHeight;

      _controlUpDown = new GUISpinControl(GetID, 0, _spinControlPositionX, _spinControlPositionY,
                                          _spinControlWidth, _spinControlHeight,
                                          _upTextureName, _downTextureName, _upTextureNameFocus, _downTextureNameFocus,
                                          _fontName, _spinControlColor,
                                          GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT,
                                          _spinControlAlignment);
      _controlUpDown.ParentControl = this;
      _controlUpDown.DimColor = DimColor;

      int xpos = 5 + _positionX + _width;
      if (xpos + 15 > GUIGraphicsContext.Width)
      {
        xpos = GUIGraphicsContext.Width - 15;
      }
      _verticalScrollBar = new GUIVerticalScrollbar(_controlId, 0,
                                                    5 + _positionX + _width + _scrollbarXOff, _positionY,
                                                    _scrollbarWidth, _height,
                                                    _scrollbarBackGroundTextureName, _scrollbarTopTextureName,
                                                    _scrollbarBottomTextureName);
      _verticalScrollBar.ParentControl = this;
      _verticalScrollBar.SendNotifies = false;
      _verticalScrollBar.DimColor = DimColor;
      _font = GUIFontManager.GetFont(_fontName);
      SetTextureDimensions(_textureWidth, _textureHeight);
      SetThumbDimensionsLow(_xPositionThumbNail, _yPositionThumbNail, _thumbNailWidth, _thumbNailHeight);

      _frameNoFocusControl = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth,
                                                  _itemHeight,
                                                  _frameNoFocusName);
      _frameNoFocusControl.ParentControl = this;
      _frameNoFocusControl.DimColor = DimColor;
      _frameNoFocusControl.MaskFileName = _frameNoFocusMask;
      _frameNoFocusControl.SetAnimations(_allThumbAnimations);


      _frameFocusControl = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth,
                                                _itemHeight,
                                                _frameFocusName);
      _frameFocusControl.ParentControl = this;
      _frameFocusControl.DimColor = DimColor;
      _frameFocusControl.MaskFileName = _frameFocusMask;
      _frameFocusControl.SetAnimations(_allThumbAnimations);

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
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textureWidth, ref _textureHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _xPositionThumbNail, ref _yPositionThumbNail,
                                                     ref _thumbNailWidth, ref _thumbNailHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _bigTextureWidth, ref _bigTextureHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _positionXThumbBig, ref _positionYThumbBig, ref _widthThumbBig,
                                                     ref _heightThumbBig);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _bigItemWidth, ref _bigItemHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _itemWidth, ref _itemHeight);
    }


    protected void OnSelectionChanged()
    {
      if (!IsVisible)
      {
        return;
      }

      _scrollOffset = 0.0f;
      _timeElapsed = 0.0f;

      // Reset searchstring
      if (_lastSearchItem != (_offset + _cursorY * _columnCount + _cursorX))
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
        GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
        GUIPropertyManager.SetProperty("#selectedindex", strIndex);
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED, WindowId, GetID, ParentID, 0, 0,
                                      null);
      msg.SendToTargetWindow = true;
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


    private void RenderItem(float timePassed, int iButton, bool bFocus, int dwPosX, int dwPosY, GUIListItem pItem,
                            bool buttonOnly)
    {
      if (_listButtons == null)
      {
        return;
      }
      if (iButton < 0 || iButton >= _listButtons.Count)
      {
        return;
      }
      GUIButtonControl btn = _listButtons[iButton];
      if (btn == null)
      {
        return;
      }

      btn.Width = _textureWidth;
      btn.Height = _textureHeight;

      bool clipping = false;
      if (bFocus && Focus)
      {
        Rectangle clipRect = new Rectangle();
        clipRect.X = _positionX - _zoomXPixels / 2;
        clipRect.Y = _positionY - _zoomYPixels / 2;
        clipRect.Width = (_columnCount * _itemWidth) + _zoomXPixels + (_zoomXPixels / 2);
        clipRect.Height = (_rowCount * _itemHeight) + _zoomYPixels + (_zoomYPixels / 2);
        if (clipRect.X < 0)
        {
          clipRect.X = 0;
        }
        if (clipRect.Y < 0)
        {
          clipRect.Y = 0;
        }
        GUIGraphicsContext.BeginClip(clipRect, false);
        clipping = true;
      }

      float fTextPosY = (float)dwPosY + (float)_textureHeight;

      long dwColor = _textColor;
      if (pItem.Selected)
      {
        dwColor = _selectedColor;
      }
      if (pItem.IsPlayed)
      {
        dwColor = _playedColor;
      }
      if (!bFocus && Focus)
      {
        dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
      }

      if (pItem.IsRemote)
      {
        dwColor = _remoteColor;
        if (pItem.IsDownloading)
        {
          dwColor = _downloadColor;
        }
      }

      if (pItem.IsBdDvdFolder)
      {
        dwColor = _bdDvdDirectoryColor;
      }

      if (!Focus)
      {
        dwColor &= DimColor;
      }

      if (bFocus == true && Focus && m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (buttonOnly)
        {
          btn.ColourDiffuse = 0xffffffff;
          btn.Focus = true;
          btn.SetPosition(dwPosX, dwPosY);
          //if (true == _showTexture) btn.Render(timePassed);

          if (clipping)
          {
            GUIGraphicsContext.EndClip();
          }
          return;
        }
        if (fTextPosY >= _positionY && _renderFocusText)
        {
          _listLabels[iButton].XPosition = dwPosX + _textXOff;
          _listLabels[iButton].YPosition = (int)Math.Truncate(fTextPosY + _textYOff + _zoomYPixels);
          _listLabels[iButton].Width = _textureWidth;
          _listLabels[iButton].Height = _textureHeight;
          _listLabels[iButton].TextColor = dwColor;
          _listLabels[iButton].Label = pItem.Label;
          _listLabels[iButton].AllowScrolling = true;
          _listLabels[iButton].Render(timePassed);
        }
      }
      else
      {
        if (buttonOnly)
        {
          btn.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
          if (!btn.Focus)
          {
            btn.SetPosition(dwPosX, dwPosY);
          }
          btn.Focus = false;
          if (!_hideUnfocusTexture)
          {
            btn.Render(timePassed);
          }
          if (_showFrame)
          {
            _frameFocusControl.Focus = btn.Focus;
            _frameFocusControl.SetPosition(btn._positionX, btn._positionY);
            _frameFocusControl.Width = btn.Width;
            _frameFocusControl.Height = btn.Height;
            _frameFocusControl.Render(timePassed);
          }

          if (clipping)
          {
            GUIGraphicsContext.EndClip();
          }
          return;
        }
        if (fTextPosY >= _positionY && _renderUnfocusText)
        {
          _listLabels[iButton].XPosition = dwPosX + _textXOff;
          _listLabels[iButton].YPosition = (int)Math.Truncate(fTextPosY + _textYOff);
          _listLabels[iButton].Width = _textureWidth;
          _listLabels[iButton].Height = _textureHeight;
          _listLabels[iButton].TextColor = dwColor;
          _listLabels[iButton].Label = pItem.Label;
          _listLabels[iButton].AllowScrolling = false;
          _listLabels[iButton].Render(timePassed);
        }
      }

      // Set oversized value
      int iOverSized = 0;
      if (bFocus && Focus && _enableFocusZoom && _zoomXPixels == 0 && _zoomYPixels == 0)
      {
        iOverSized = (_thumbNailWidth + _thumbNailHeight) / THUMBNAIL_OVERSIZED_DIVIDER;
      }

      GUIImage pFocusImage = null;
      if (pItem.HasThumbnail)
      {
        GUIImage pImage = pItem.Thumbnail;
        pFocusImage = pImage;
        if (null == pImage /*&& _sleeper==0 */&& !IsAnimating)
        {
          pImage = new GUIImage(0, 0, _xPositionThumbNail - iOverSized + dwPosX,
                                _yPositionThumbNail - iOverSized + dwPosY, _thumbNailWidth + 2 * iOverSized,
                                _thumbNailHeight + 2 * iOverSized, pItem.ThumbnailImage, 0x0);

          if (pImage != null)
          {
            pImage.ParentControl = this;
            pImage.KeepAspectRatio = _keepAspectRatio;
            pImage.ImageAlignment = Alignment.ALIGN_CENTER;
            pImage.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
            pImage.MaskFileName = _textureMask;
            pImage.ZoomFromTop = !pItem.IsFolder && _zoom;
            pImage.AllocResources();
            pItem.Thumbnail = pImage;
            pImage.SetPosition(_xPositionThumbNail - iOverSized + dwPosX, _yPositionThumbNail - iOverSized + dwPosY);
            pImage.DimColor = DimColor;
            if (bFocus || !Focus)
            {
              pImage.ColourDiffuse = 0xffffffff;
            }
            else
            {
              pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            }
            if (bFocus && (_zoomXPixels != 0 || _zoomYPixels != 0))
            {
              pImage.Width = _textureWidth + _zoomXPixels;
              pImage.Height = _textureHeight + _zoomYPixels;
              pImage.SetPosition(dwPosX - (_zoomXPixels / 2), dwPosY - (_zoomYPixels / 2));
            }
            pImage.Render(timePassed);
            _sleeper += SLEEP_FRAME_COUNT;
          }
        }
        if (null != pImage)
        {
          if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
          {
            pImage.SafeDispose();
            pImage.AllocResources();
          }
          pImage.ZoomFromTop = !pItem.IsFolder && _zoom;
          pImage.Width = _thumbNailWidth + 2 * iOverSized;
          pImage.Height = _thumbNailHeight + 2 * iOverSized;
          pImage.ImageAlignment = Alignment.ALIGN_CENTER;
          pImage.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
          pImage.MaskFileName = _textureMask;
          pImage.SetPosition(_xPositionThumbNail + dwPosX - iOverSized, _yPositionThumbNail - iOverSized + dwPosY);
          pImage.DimColor = DimColor;
          if (bFocus || !Focus)
          {
            pImage.ColourDiffuse = 0xffffffff;
          }
          else
          {
            pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
          }
          if (bFocus && (_zoomXPixels != 0 || _zoomYPixels != 0))
          {
            pImage.Width = _textureWidth + _zoomXPixels;
            pImage.Height = _textureHeight + _zoomYPixels;
            pImage.SetPosition(dwPosX - (_zoomXPixels / 2), dwPosY - (_zoomYPixels / 2));
          }
          pImage.Render(timePassed);
        }
      }
      else
      {
        if (pItem.HasIconBig)
        {
          GUIImage pImage = pItem.IconBig;
          pFocusImage = pImage;
          if (null == pImage /*&& _sleeper==0 */&& !IsAnimating)
          {
            pImage = new GUIImage(0, 0, _xPositionThumbNail - iOverSized + dwPosX,
                                  _yPositionThumbNail - iOverSized + dwPosY, _thumbNailWidth + 2 * iOverSized,
                                  _thumbNailHeight + 2 * iOverSized, pItem.IconImageBig, 0x0);
            pImage.ParentControl = this;
            pImage.KeepAspectRatio = _keepAspectRatio;
            pImage.ImageAlignment = Alignment.ALIGN_CENTER;
            pImage.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
            pImage.MaskFileName = _textureMask;
            pImage.ZoomFromTop = !pItem.IsFolder && _zoom;

            pImage.AllocResources();
            pItem.IconBig = pImage;
            pImage.SetPosition(_xPositionThumbNail + dwPosX - iOverSized, _yPositionThumbNail - iOverSized + dwPosY);
            pImage.DimColor = DimColor;
            if (bFocus || !Focus)
            {
              pImage.ColourDiffuse = 0xffffffff;
            }
            else
            {
              pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            }
            if (bFocus && (_zoomXPixels != 0 || _zoomYPixels != 0))
            {
              pImage.Width = _textureWidth + _zoomXPixels;
              pImage.Height = _textureHeight + _zoomYPixels;
              pImage.SetPosition(dwPosX - (_zoomXPixels / 2), dwPosY - (_zoomYPixels / 2));
            }
            pImage.Render(timePassed);
            _sleeper += SLEEP_FRAME_COUNT;
          }
          else if (null != pImage)
          {
            if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
            {
              pImage.SafeDispose();
              pImage.AllocResources();
            }
            pImage.ZoomFromTop = !pItem.IsFolder && _zoom;
            pImage.ImageAlignment = Alignment.ALIGN_CENTER;
            pImage.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
            pImage.MaskFileName = _textureMask;
            pImage.Width = _thumbNailWidth + 2 * iOverSized;
            pImage.Height = _thumbNailHeight + 2 * iOverSized;
            pImage.SetPosition(_xPositionThumbNail - iOverSized + dwPosX, _yPositionThumbNail - iOverSized + dwPosY);
            pImage.DimColor = DimColor;
            if (bFocus || !Focus)
            {
              pImage.ColourDiffuse = 0xffffffff;
            }
            else
            {
              pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            }
            if (bFocus && (_zoomXPixels != 0 || _zoomYPixels != 0))
            {
              pImage.Width = _textureWidth + _zoomXPixels;
              pImage.Height = _textureHeight + _zoomYPixels;
              pImage.SetPosition(dwPosX - (_zoomXPixels / 2), dwPosY - (_zoomYPixels / 2));
            }
            pImage.Render(timePassed);
          }
        }
      }
      if (bFocus && Focus)
      {
        btn.Width = _textureWidth + _zoomXPixels;
        btn.Height = _textureHeight + _zoomYPixels;
        btn.SetPosition(dwPosX - (_zoomXPixels / 2), dwPosY - (_zoomYPixels / 2));
        btn.Render(timePassed);
        if (pFocusImage != null/* && _zoomXPixels == 0 && _zoomYPixels == 0*/)
        {
          pFocusImage.ImageAlignment = Alignment.ALIGN_CENTER;
          pFocusImage.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
          pFocusImage.Render(timePassed);
        }
        if (_showFrame)
        {
          //_frameFocusControl.Focus = btn.Focus;
          _frameFocusControl.SetPosition(btn._positionX, btn._positionY);
          _frameFocusControl.Width = btn.Width;
          _frameFocusControl.Height = btn.Height;
          _frameFocusControl.Render(timePassed);
        }
      }
      else
      {
        if (_showFrame)
        {
          //_frameNoFocusControl.Focus = btn.Focus;
          _frameNoFocusControl.SetPosition(dwPosX, dwPosY);
          _frameNoFocusControl.Width = btn.Width + 2 * iOverSized;
          _frameNoFocusControl.Height = btn.Height + 2 * iOverSized;
          _frameNoFocusControl.Render(timePassed);
        }
        btn.Width = _textureWidth;
        btn.Height = _textureHeight;
      }

      if (clipping)
      {
        GUIGraphicsContext.EndClip();
      }
    }


    public override void Render(float timePassed)
    {
      _timeElapsed += timePassed;

      if (null == _font)
      {
        base.Render(timePassed);
        return;
      }

      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }

      if (_frameLimiter < GUIGraphicsContext.MaxFPS)
        _frameLimiter++;
      else
        _frameLimiter = 1;

      if (_scrollStartOffset >= _rowCount)
        _scrollStartOffset = _rowCount - 1;

      int dwPosY = 0;
      if ((_cursorX > 0 || _cursorY > 0) && !ValidItem(_cursorX, _cursorY))
      {
        _cursorX = 0;
        _cursorY = 0;
        OnSelectionChanged();
      }
      if (_sleeper > 0)
      {
        _sleeper--;
      }

      int iScrollYOffset = 0;
      if (_scrollingDown)
      {
        iScrollYOffset = -(_itemHeight - _scrollCounter);
      }
      if (_scrollingUp)
      {
        iScrollYOffset = _itemHeight - _scrollCounter;
      }

      float fx = (float)_positionX;
      float fy = (float)_positionY;

      if (fx <= 0)
      {
        fx = 0;
      }
      if (fy <= 0)
      {
        fy = 0;
      }

      Rectangle clipRect = new Rectangle();
      clipRect.X = (int)fx;
      clipRect.Y = (int)fy;
      clipRect.Width = _columnCount * _itemWidth;
      clipRect.Height = _rowCount * _itemHeight;
      GUIGraphicsContext.BeginClip(clipRect);

      // Free unused textures if page has changed
      int iStartItem = _offset;
      int iEndItem = _rowCount * _columnCount + _offset;
      if ((_lLastItemPageValues != iStartItem + iEndItem) && (iScrollYOffset == 0))
      {
        _lLastItemPageValues = iStartItem + iEndItem;
        if (iStartItem < _listItems.Count)
        {
          for (int i = 0; i < iStartItem; ++i)
          {
            GUIListItem pItem = _listItems[i];
            if (null != pItem)
            {
              pItem.FreeMemory();
            }
          }
        }

        for (int i = iEndItem + 1; i < _listItems.Count; ++i)
        {
          GUIListItem pItem = _listItems[i];
          if (null != pItem)
          {
            pItem.FreeMemory();
          }
        }
      }

      int focusButton = -1;
      int focusX = -1;
      int focusY = -1;
      GUIListItem focusItem = null;
      for (int i = 1; i < 2; ++i)
      {
        if (_scrollingUp)
        {
          // render item on top
          dwPosY = _positionY - _itemHeight + iScrollYOffset;
          _offset -= _columnCount;
          for (int iCol = 0; iCol < _columnCount; iCol++)
          {
            int dwPosX = _positionX + iCol * _itemWidth;
            int iItem = iCol + _offset;
            if (iItem >= 0 && iItem < _listItems.Count)
            {
              GUIListItem pItem = _listItems[iItem];
              RenderItem(timePassed, 0, false, dwPosX, dwPosY, pItem, i == 0);
              if (iItem < iStartItem)
              {
                iStartItem = iItem;
              }
              if (iItem > iEndItem)
              {
                iEndItem = iItem;
              }
            }
          }
          _offset += _columnCount;
        }

        // render main panel

        for (int iRow = 0; iRow < _rowCount; iRow++)
        {
          dwPosY = _positionY + iRow * _itemHeight + iScrollYOffset;
          for (int iCol = 0; iCol < _columnCount; iCol++)
          {
            int dwPosX = _positionX + iCol * _itemWidth;
            int iItem = iRow * _columnCount + iCol + _offset;
            if (iItem >= 0 && iItem < _listItems.Count)
            {
              GUIListItem pItem = _listItems[iItem];
              bool bFocus = (_cursorX == iCol && _cursorY == iRow);
              if (!bFocus)
              {
                RenderItem(timePassed, iRow * _columnCount + iCol, bFocus, dwPosX, dwPosY, pItem, i == 0);
              }
              if (bFocus)
              {
                focusButton = iRow * _columnCount + iCol;
                focusX = dwPosX;
                focusY = dwPosY;
                focusItem = pItem;
              }
              if (iItem < iStartItem)
              {
                iStartItem = iItem;
              }
              if (iItem > iEndItem)
              {
                iEndItem = iItem;
              }
            }
          }
        }

        if (_scrollingDown)
        {
          // render item on bottom
          dwPosY = _positionY + _rowCount * _itemHeight + iScrollYOffset;
          for (int iCol = 0; iCol < _columnCount; iCol++)
          {
            int dwPosX = _positionX + iCol * _itemWidth;
            int iItem = _rowCount * _columnCount + iCol + _offset;
            if (iItem >= 0 && iItem < _listItems.Count)
            {
              GUIListItem pItem = _listItems[iItem];
              RenderItem(timePassed, 0, false, dwPosX, dwPosY, pItem, i == 0);
              if (iItem < iStartItem)
              {
                iStartItem = iItem;
              }
              if (iItem > iEndItem)
              {
                iEndItem = iItem;
              }
            }
          }
        }
      }
      if (focusButton != -1 && focusX != -1 && focusY != -1 && focusItem != null)
      {
        RenderItem(timePassed, focusButton, Focus && !_controlUpDown.Focus, focusX, focusY, focusItem, true);
        RenderItem(timePassed, focusButton, Focus && !_controlUpDown.Focus, focusX, focusY, focusItem, false);
      }

      GUIGraphicsContext.EndClip();

      //  _frames = 12;
      int iStep = _itemHeight / _frames;
      if (0 == iStep)
      {
        iStep = 1;
      }
      if (_scrollingDown)
      {
        _scrollCounter -= iStep;
        if (_scrollCounter <= 0)
        {
          _scrollingDown = false;
          _offset += _columnCount;
          int iPage = _offset / (_rowCount * _columnCount);
          if ((_offset % (_rowCount * _columnCount)) != 0)
          {
            iPage++;
          }
          _controlUpDown.Value = iPage + 1;
          if (_scrollingDownLast)
          {
            _scrollingDownLast = false;
            _cursorX = _listItems.Count % _columnCount - 1 >= 0 ? _listItems.Count % _columnCount - 1 : 0;
          }
          if (_compensatingDown)
          {
            _cursorY -= 1;
            _compensatingDown = false;
            OnDown();
          }
          _refresh = true;
          OnSelectionChanged();
        }
      }
      if (_scrollingUp)
      {
        _scrollCounter -= iStep;
        if (_scrollCounter <= 0)
        {
          _scrollingUp = false;
          _offset -= _columnCount;
          int iPage = _offset / (_rowCount * _columnCount);
          if ((_offset % (_rowCount * _columnCount)) != 0)
          {
            iPage++;
          }
          _controlUpDown.Value = iPage + 1;
          if (_compensatingUp)
          {
            _cursorY += 1;
            _compensatingUp = false;
            OnUp();
          }
          _refresh = true;
          OnSelectionChanged();
        }
      }

      dwPosY = _positionY + _rowCount * (_itemHeight);
      RenderScrollbar(timePassed, dwPosY);

      if (_scrollingDown || _scrollingUp)
      {
        _refresh = true;
      }

      if (Focus)
      {
        GUIPropertyManager.SetProperty("#highlightedbutton", string.Empty);
      }
      base.Render(timePassed);
    }

    private void RenderScrollbar(float timePassed, int y)
    {
      int iItemsPerPage = _rowCount * _columnCount;
      if (_listItems.Count > iItemsPerPage)
      {
        // Render the spin control
        if (_controlUpDown != null)
        {
          //_controlUpDown.SetPosition(_controlUpDown.XPosition,y);
          _controlUpDown.Render(timePassed);
        }

        // Render the vertical scrollbar
        if (_verticalScrollBar != null)
        {
          float fPercent = (float)_cursorY * _columnCount + _offset + _cursorX;
          fPercent /= (float)(_listItems.Count);
          fPercent *= 100.0f;
          if ((int)fPercent != (int)_verticalScrollBar.Percentage)
          {
            _verticalScrollBar.Percentage = fPercent;
          }
          _verticalScrollBar.Render(timePassed);
        }
      }
    }

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
          _searchString = "";
          OnPageDown();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          {
            _searchString = "";
            _offset = 0;
            _cursorY = 0;
            _cursorX = 0;
            _controlUpDown.Value = 1;
            OnSelectionChanged();

            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
            _searchString = "";
            SelectItem(_listItems.Count - 1);
            /* int iChan = _listItems.Count - 1;
             if (iChan >= 0)
             {
               // update spin controls
               int iItemsPerPage = _rowCount * _columnCount;
               int iPage = 1;
               int iSel = iChan;
               while (iSel >= iItemsPerPage)
               {
                 iPage++;
                 iSel -= iItemsPerPage;
               }
               _controlUpDown.Value = iPage;

               // find item
               _offset = 0;
               _cursorY = 0;
               while (iChan >= iItemsPerPage)
               {
                 iChan -= iItemsPerPage;
                 _offset += iItemsPerPage;
               }
               while (iChan >= _columnCount)
               {
                 iChan -= _columnCount;
                 _cursorY++;
               }
               _cursorX = iChan;

               // Special handling when more than one page
               if (iPage > 1)
               {
                 while ((iChan+_columnCount) < iItemsPerPage)
                 {
                   _offset -= _columnCount;
                   iChan += _columnCount;
                   _cursorY++;
                 }
               }


               OnSelectionChanged();
             }*/
            OnSelectionChanged();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            _searchString = "";
            OnDown();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            _searchString = "";
            OnUp();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            _searchString = "";
            OnLeft();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            _searchString = "";
            OnRight();
            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if (action.m_key != null)
            {
              // Check key
              if (((action.m_key.KeyChar >= '0') && (action.m_key.KeyChar <= '9')) ||
                  action.m_key.KeyChar == '*' || action.m_key.KeyChar == '(' || action.m_key.KeyChar == '#' ||
                  action.m_key.KeyChar == '�')
              {
                Press((char)action.m_key.KeyChar);
                return;
              }

              if (action.m_key.KeyChar == (int)Keys.Back)
              {
                if (_searchString.Length > 0)
                {
                  _searchString = _searchString.Remove(_searchString.Length - 1, 1);
                  SearchItem(_searchString, SearchType.SEARCH_FIRST);
                }
              }
              if (((action.m_key.KeyChar >= 65) && (action.m_key.KeyChar <= 90)) ||
                  (action.m_key.KeyChar == (int)Keys.Space))
              {
                if (action.m_key.KeyChar == (int)Keys.Space && _searchString == string.Empty)
                {
                  return;
                }
                _searchString += (char)action.m_key.KeyChar;
                SearchItem(_searchString, SearchType.SEARCH_FIRST);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int id;
            bool focus;
            if (_verticalScrollBar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
            {
              int iItemsPerPage = _rowCount * _columnCount;
              //            _drawFocus=false;
              _verticalScrollBar.OnAction(action);
              float fPercentage = _verticalScrollBar.Percentage;
              fPercentage /= 100.0f;
              fPercentage *= (float)_listItems.Count;
              int iChan = (int)fPercentage;
              if (iChan != _offset + _cursorY * _columnCount + _cursorX)
              {
                // update spin controls
                int iPage = 1;
                int iSel = iChan;
                while (iSel >= iItemsPerPage)
                {
                  iPage++;
                  iSel -= iItemsPerPage;
                }
                _controlUpDown.Value = iPage;

                // find item
                _offset = 0;
                _cursorY = 0;
                while (iChan >= iItemsPerPage)
                {
                  iChan -= iItemsPerPage;
                  _offset += iItemsPerPage;
                }
                while (iChan >= _columnCount)
                {
                  iChan -= _columnCount;
                  _cursorY++;
                }
                _cursorX = iChan;
                OnSelectionChanged();
              }
              return;
            }
            //          _drawFocus=true;
          }
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            int id;
            bool focus;
            if (_verticalScrollBar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
            {
              //_drawFocus=false;
              int iItemsPerPage = _rowCount * _columnCount;
              _verticalScrollBar.OnAction(action);
              float fPercentage = _verticalScrollBar.Percentage;
              fPercentage /= 100.0f;
              fPercentage *= (float)_listItems.Count;
              int iChan = (int)fPercentage;
              if (iChan != _offset + _cursorY * _columnCount + _cursorX)
              {
                // update spin controls
                int iPage = 1;
                int iSel = iChan;
                while (iSel >= iItemsPerPage)
                {
                  iPage++;
                  iSel -= iItemsPerPage;
                }
                _controlUpDown.Value = iPage;

                // find item
                _offset = 0;
                _cursorY = 0;
                while (iChan >= iItemsPerPage)
                {
                  iChan -= iItemsPerPage;
                  _offset += iItemsPerPage;
                }
                while (iChan >= _columnCount)
                {
                  iChan -= _columnCount;
                  _cursorY++;
                }
                _cursorX = iChan;
                OnSelectionChanged();
              }
              return;
            }
            else
            {
              //_drawFocus=true;
              if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
              {
                _searchString = "";
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                                (int)Action.ActionType.ACTION_SELECT_ITEM, 0, null);
                GUIGraphicsContext.SendMessage(msg);
              }
              else
              {
                _controlUpDown.OnAction(action);
              }
              _refresh = true;
            }
          }
          break;

        default:
          {
            if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                              (int)action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
              _refresh = true;
            }
            else
            {
              _controlUpDown.OnAction(action);
              _refresh = true;
            }
          }
          break;
      }
    }


    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.SenderControlId == 0)
        {
          if (message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
          {
            _offset = (_controlUpDown.Value - 1) * (_rowCount * _columnCount);
            _refresh = true;
            OnSelectionChanged();
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = _offset + _cursorY * _columnCount + _cursorX;
          if (iItem >= 0 && iItem < _listItems.Count)
          {
            message.Object = _listItems[iItem];
          }
          else
          {
            message.Object = null;
          }
          _refresh = true;
          return true;
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
        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in _listItems)
          {
            item.FreeMemory();
          }
          _refresh = true;
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
            message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
        {
          if (Disabled || !IsVisible || !CanFocus())
          {
            base.OnMessage(message);
            return true;
          }
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
          _refresh = true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem newItem = message.Object as GUIListItem;
          if (newItem != null)
          {
            _listItems.Add(newItem);
          }
          int iItemsPerPage = _rowCount * _columnCount;
          int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
          if (iItemsPerPage != 0)
          {
            if ((_listItems.Count % iItemsPerPage) != 0)
            {
              iPages++;
            }
          }
          _controlUpDown.SetRange(1, iPages);
          _controlUpDown.Value = 1;
          _refresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = _listItems.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          message.Param1 = _offset + _cursorY * _columnCount + _cursorX;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          SelectItem(message.Param1);
          OnSelectionChanged();
          _refresh = true;
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
      {
        foreach (GUIListItem item in _listItems)
        {
          item.Selected = false;
        }
        foreach (GUIListItem item in _listItems)
        {
          if (item.Path.Equals(message.Label, StringComparison.OrdinalIgnoreCase))
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
          if (item.IsRemote)
          {
            if (message.Label == item.Path)
            {
              item.IsDownloading = true;
            }
          }
        }
      }
      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED)
      {
        foreach (GUIListItem item in _listItems)
        {
          if (item.IsRemote)
          {
            if (message.Label == item.Path)
            {
              item.Path = message.Label2;
              item.IsRemote = false;
              item.IsDownloading = false;
            }
          }
        }
      }

      if (base.OnMessage(message))
      {
        return true;
      }

      return false;
    }

    private void SelectItem(int iItem)
    {
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        int iPage = 1;
        _cursorX = 0;
        _cursorY = 0;
        _offset = 0;
        int selItem = iItem;

        if (_listItems.Count - iItem + 1 <= _columnCount) // special handling for the last rows
        {
          int iItemsPerPage = (_rowCount * _columnCount);
          iPage = iItemsPerPage == 0 ? 0 : (iItem + 1) / iItemsPerPage;
          if ((iItem + 1) % iItemsPerPage > 0)
          {
            iPage++;
          }
          _offset = (iPage - 1) * iItemsPerPage;
          iItem -= _offset;

          while ((iItem < _columnCount * (_rowCount - 1)) && (_offset > 0))
          {
            _offset -= (_columnCount);
            iItem += (_columnCount);
          }
          while (iItem >= _columnCount)
          {
            _cursorY++;
            iItem -= _columnCount;
          }

          _cursorX = iItem;

          // The following check is a complete hack used to make sure the list is drawn correctly in the specific situation for which
          // it checks.  This hack exists because rewriting the entire algorithm above would have otherwise been necessary.
          if (_cursorY == _rowCount - 1 && selItem != _listItems.Count && _cursorX == _columnCount - 1)
          {
            _offset += _columnCount;
            --_cursorY;
          }

          //_cursorX = _cursorY = 1;
          _controlUpDown.Value = iPage;
          return;
        }


        while (iItem >= (_rowCount * _columnCount))
        {
          _offset += (_rowCount * _columnCount);
          iItem -= (_rowCount * _columnCount);
          iPage++;
        }
        while ((iItem <= _columnCount * _scrollStartOffset) && (_offset > 0))
        {
          _offset -= (_columnCount);
          iItem += (_columnCount);
        }
        while (iItem >= _columnCount)
        {
          if (_cursorY + 1 >= _rowCount - _scrollStartOffset)
          {
            _offset += (_columnCount);
            iItem -= (_columnCount);
          }
          else
          {
            _cursorY++;
            iItem -= _columnCount;
          }
        }
        _controlUpDown.Value = iPage;
        _cursorX = iItem;
      }
    }


    /// <summary>
    /// Search for first item starting with searchkey
    /// </summary>
    /// <param name="SearchKey">SearchKey</param>
    private void SearchItem(string SearchKey, SearchType iSearchMethode)
    {
      // Get selected item
      bool bItemFound = false;
      int iCurrentItem = _offset + _cursorY * _columnCount + _cursorX;
      if (iSearchMethode == SearchType.SEARCH_FIRST)
      {
        iCurrentItem = 0;
      }
      int iItem = iCurrentItem;
      do
      {
        if (iSearchMethode == SearchType.SEARCH_NEXT)
        {
          iItem++;
          if (iItem >= _listItems.Count)
          {
            iItem = 0;
          }
        }
        if (iSearchMethode == SearchType.SEARCH_PREV && _listItems.Count > 0)
        {
          iItem--;
          if (iItem < 0)
          {
            iItem = _listItems.Count - 1;
          }
        }

        GUIListItem pItem = _listItems[iItem];
        if (pItem.Label.ToUpperInvariant().StartsWith(SearchKey.ToUpperInvariant()) == true)
        {
          bItemFound = true;
          break;
        }

        if (iSearchMethode == SearchType.SEARCH_FIRST)
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

      _lastSearchItem = _offset + _cursorY * _columnCount + _cursorX;
      OnSelectionChanged();
      _refresh = true;
    }


    /// <summary>
    /// Handle keypress events for SMS style search (key '1'..'9')
    /// </summary>
    /// <param name="Key"></param>
    private void Press(char Key)
    {
      if (!_enableSMSsearch) return;

      // Check key timeout
      CheckTimer();
      // Check different key pressed
      if ((Key != _previousKey) && (Key >= '1' && Key <= '9'))
      {
        _currentKey = (char)0;
      }

      if (Key == '*' || Key == '(')
      {
        // Backspace
        if (_searchString.Length > 0)
        {
          _searchString = _searchString.Remove(_searchString.Length - 1, 1);
        }
        _previousKey = (char)0;
        _currentKey = (char)0;
        _keyTimer = DateTime.Now;
      }
      if (Key == '#' || Key == '�')
      {
        _keyTimer = DateTime.Now;
      }
      else if (Key == '1')
      {
        if (_currentKey == 0)
        {
          _currentKey = ' ';
        }
        else if (_currentKey == ' ')
        {
          _currentKey = '!';
        }
        else if (_currentKey == '!')
        {
          _currentKey = '?';
        }
        else if (_currentKey == '?')
        {
          _currentKey = '.';
        }
        else if (_currentKey == '.')
        {
          _currentKey = '0';
        }
        else if (_currentKey == '0')
        {
          _currentKey = '1';
        }
        else if (_currentKey == '1')
        {
          _currentKey = '-';
        }
        else if (_currentKey == '-')
        {
          _currentKey = '+';
        }
        else if (_currentKey == '+')
        {
          _currentKey = ' ';
        }
      }
      else if (Key == '2')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'a';
        }
        else if (_currentKey == 'a')
        {
          _currentKey = 'b';
        }
        else if (_currentKey == 'b')
        {
          _currentKey = 'c';
        }
        else if (_currentKey == 'c')
        {
          _currentKey = '2';
        }
        else if (_currentKey == '2')
        {
          _currentKey = 'a';
        }
      }
      else if (Key == '3')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'd';
        }
        else if (_currentKey == 'd')
        {
          _currentKey = 'e';
        }
        else if (_currentKey == 'e')
        {
          _currentKey = 'f';
        }
        else if (_currentKey == 'f')
        {
          _currentKey = '3';
        }
        else if (_currentKey == '3')
        {
          _currentKey = 'd';
        }
      }
      else if (Key == '4')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'g';
        }
        else if (_currentKey == 'g')
        {
          _currentKey = 'h';
        }
        else if (_currentKey == 'h')
        {
          _currentKey = 'i';
        }
        else if (_currentKey == 'i')
        {
          _currentKey = '4';
        }
        else if (_currentKey == '4')
        {
          _currentKey = 'g';
        }
      }
      else if (Key == '5')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'j';
        }
        else if (_currentKey == 'j')
        {
          _currentKey = 'k';
        }
        else if (_currentKey == 'k')
        {
          _currentKey = 'l';
        }
        else if (_currentKey == 'l')
        {
          _currentKey = '5';
        }
        else if (_currentKey == '5')
        {
          _currentKey = 'j';
        }
      }
      else if (Key == '6')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'm';
        }
        else if (_currentKey == 'm')
        {
          _currentKey = 'n';
        }
        else if (_currentKey == 'n')
        {
          _currentKey = 'o';
        }
        else if (_currentKey == 'o')
        {
          _currentKey = '6';
        }
        else if (_currentKey == '6')
        {
          _currentKey = 'm';
        }
      }
      else if (Key == '7')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'p';
        }
        else if (_currentKey == 'p')
        {
          _currentKey = 'q';
        }
        else if (_currentKey == 'q')
        {
          _currentKey = 'r';
        }
        else if (_currentKey == 'r')
        {
          _currentKey = 's';
        }
        else if (_currentKey == 's')
        {
          _currentKey = '7';
        }
        else if (_currentKey == '7')
        {
          _currentKey = 'p';
        }
      }
      else if (Key == '8')
      {
        if (_currentKey == 0)
        {
          _currentKey = 't';
        }
        else if (_currentKey == 't')
        {
          _currentKey = 'u';
        }
        else if (_currentKey == 'u')
        {
          _currentKey = 'v';
        }
        else if (_currentKey == 'v')
        {
          _currentKey = '8';
        }
        else if (_currentKey == '8')
        {
          _currentKey = 't';
        }
      }
      else if (Key == '9')
      {
        if (_currentKey == 0)
        {
          _currentKey = 'w';
        }
        else if (_currentKey == 'w')
        {
          _currentKey = 'x';
        }
        else if (_currentKey == 'x')
        {
          _currentKey = 'y';
        }
        else if (_currentKey == 'y')
        {
          _currentKey = 'z';
        }
        else if (_currentKey == 'z')
        {
          _currentKey = '9';
        }
        else if (_currentKey == '9')
        {
          _currentKey = 'w';
        }
      }

      if (Key >= '1' && Key <= '9')
      {
        // Check different key pressed
        if (Key == _previousKey)
        {
          if (_searchString.Length > 0)
          {
            _searchString = _searchString.Remove(_searchString.Length - 1, 1);
          }
        }
        _previousKey = Key;
        _searchString += _currentKey;
      }
      SearchItem(_searchString, SearchType.SEARCH_FIRST);
      _keyTimer = DateTime.Now;
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _keyTimer;
      if (ts.TotalMilliseconds >= 1000)
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
      }
    }

    public override void PreAllocResources()
    {
      if (null == _font)
      {
        return;
      }
      base.PreAllocResources();
      _controlUpDown.PreAllocResources();
      _verticalScrollBar.PreAllocResources();
      if (_showFrame)
      {
        _frameFocusControl.PreAllocResources();
        _frameNoFocusControl.PreAllocResources();
      }
    }


    private void Calculate()
    {
      float fWidth = 0, fHeight = 0;

      // height of 1 item = folder image height + text row height + space in between
      _font.GetTextExtent("y", ref fWidth, ref fHeight);

      fWidth = (float)_itemWidth;
      fHeight = (float)_itemHeight;
      float fTotalHeight = (float)(_height - 5);
      _rowCount = (int)(fTotalHeight / fHeight);
      _columnCount = (int)(_width / fWidth);

      int iItemsPerPage = _rowCount * _columnCount;
      int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
      if (iItemsPerPage != 0)
      {
        if ((_listItems.Count % iItemsPerPage) != 0)
        {
          iPages++;
        }
      }
      _controlUpDown.SetRange(1, iPages);
      _controlUpDown.Value = 1;

      // Dispose used buttoncontrols
      if (_listButtons != null)
      {
        for (int i = 0; i < _listButtons.Count; ++i)
        {
          GUIButtonControl cntl = _listButtons[i];
          cntl.SafeDispose();
        }
      }
      _listButtons = null;

      if (_listLabels != null)
      {
        for (int i = 0; i < _listLabels.Count; ++i)
        {
          GUIFadeLabel cntl = _listLabels[i];
          cntl.SafeDispose();
        }
      }
      _listLabels = null;

      // Create new buttoncontrols
      _listButtons = new List<GUIButtonControl>();
      _listLabels = new List<GUIFadeLabel>();
      for (int i = 0; i < _columnCount * _rowCount; ++i)
      {
        GUIButtonControl btn = new GUIButtonControl(_parentControlId, _controlId, _positionX, _positionY, _textureWidth,
                                                    _textureHeight, _imageFolderNameFocus, _imageFolderName,
                                                    _shadowAngle, _shadowDistance, _shadowColor);
        btn.ParentControl = this;
        btn.SetFocusedTextureMask(_textureMask);
        btn.SetNonFocusedTextureMask(_textureMask);
        btn.SetHoverTextureMask(_textureMask);
        btn.AllocResources();

        _listButtons.Add(btn);

        GUIFadeLabel fadelabel = new GUIFadeLabel(_parentControlId, _controlId, _positionX, _positionY, _textureWidth,
                                                  _textureHeight, _fontName, _textColor, Alignment.ALIGN_LEFT,
                                                  VAlignment.ALIGN_TOP, _shadowAngle, _shadowDistance, _shadowColor,
                                                  " | ");
        fadelabel.DimColor = DimColor;
        fadelabel.ParentControl = this;
        fadelabel.AllowScrolling = false;
        //fadelabel.AllowFadeIn = false;
        fadelabel.AllocResources();

        _listLabels.Add(fadelabel);
      }
    }

    public override void AllocResources()
    {
      _sleeper = 0;
      _font = GUIFontManager.GetFont(_fontName);
      base.AllocResources();
      _controlUpDown.AllocResources();
      _controlUpDown.DimColor = DimColor;
      _verticalScrollBar.AllocResources();
      _verticalScrollBar.DimColor = DimColor;

      if (ThumbAnimations == null || ThumbAnimations.Count < 1)
        _allThumbAnimations.Add(new VisualEffect());
      else
        _allThumbAnimations.AddRange(ThumbAnimations);

      if (_showFrame)
      {
        _frameFocusControl.AllocResources();
        _frameNoFocusControl.AllocResources();
        _frameFocusControl.DimColor = DimColor;
        _frameNoFocusControl.DimColor = DimColor;

        _frameFocusControl.SetAnimations(_allThumbAnimations);
        _frameNoFocusControl.SetAnimations(_allThumbAnimations);
      }

      Calculate();
    }

    public override void Dispose()
    {
      _listItems.DisposeAndClear();
      _listButtons.DisposeAndClear();
      _listLabels.DisposeAndClear();

      _listButtons = null;
      _listLabels = null;
      base.Dispose();
      _controlUpDown.SafeDispose();
      _verticalScrollBar.SafeDispose();
      //_font.Dispose(null, null);
      _font = null;

      _frameFocusControl.SafeDispose();
      _frameNoFocusControl.SafeDispose();
    }


    private bool ValidItem(int iX, int iY)
    {
      if (iX < 0)
      {
        return false;
      }
      if (iY < 0)
      {
        return false;
      }
      if (iX >= _columnCount)
      {
        return false;
      }
      if (iY >= _rowCount)
      {
        return false;
      }
      if (_offset + iY * _columnCount + iX < _listItems.Count)
      {
        return true;
      }
      return false;
    }


    private void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (_cursorX + 1 < _columnCount && ValidItem(_cursorX + 1, _cursorY))
        {
          _cursorX++;
          OnSelectionChanged();
          return;
        }

        if (_controlUpDown.GetMaximum() > 1 && _spinCanFocus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_UPDOWN;
          _controlUpDown.Focus = true;
        }
        else
        {
          base.OnAction(action);
        }
        OnSelectionChanged();
      }
      else
      {
        _controlUpDown.OnAction(action);
        if (!_controlUpDown.Focus)
        {
          base.OnAction(action);
        }
        OnSelectionChanged();
      }
    }

    private void OnLeft()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (_cursorX > 0)
        {
          _cursorX--;
          OnSelectionChanged();
          return;
        }
        base.OnAction(action);
        OnSelectionChanged();
      }
      else
      {
        _controlUpDown.OnAction(action);
        if (!_controlUpDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
        OnSelectionChanged();
      }
    }

    private void OnUp()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (_scrollingUp)
        {
          _scrollCounter = 0;
          _scrollingUp = false;
          _offset -= _columnCount;
          int iPage = _offset / (_rowCount * _columnCount);
          if ((_offset % (_rowCount * _columnCount)) != 0)
          {
            iPage++;
          }
          _controlUpDown.Value = iPage + 1;
          if (_compensatingUp)
          {
            _cursorY += 1;
            _compensatingUp = false;
            OnUp();
          }
        }

        if ((_cursorY > _scrollStartOffset) || ((_cursorY > 0) && (_offset == 0)))
        {
          _cursorY--;
        }
        else if (_cursorY <= _scrollStartOffset && _offset != 0)
        {
          if (_cursorY < _scrollStartOffset)
            _compensatingUp = true;
          _scrollCounter = _itemHeight;
          _scrollingUp = true;
        }
        else
        {
          if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
          {
            //check if _downControlId is set -> then go to the window
            if (NavigateUp > 0)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, NavigateUp,
                                              (int)action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
            else
            {
              _offset = (CalculateRows(_listItems.Count) - _rowCount) * _columnCount;
              if (_offset < 0)
              {
                _offset = 0;
              }
              _cursorY = _rowCount - 1;
              //while (_offset + _cursorY * _columnCount + _cursorX >= _listItems.Count)
              //{
              //  _cursorY--;
              //}
              while (!ValidItem(_cursorX, _cursorY) && _cursorX > 0)
              {
                _cursorX--;
              }
            }
          }
        }
        OnSelectionChanged();
      }
      else
      {
        _controlUpDown.OnAction(action);
        if (_controlUpDown.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN ||
            _controlUpDown.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_UP)
        {
          _controlUpDown.Focus = false;
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
          Focus = true;
        }
        else
        {
          _controlUpDown.OnAction(action);
        }

        if (!_controlUpDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
      }
      _lastCommandTime = AnimationTimer.TickCount;
    }

    private void OnDown()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (_scrollingDown)
        {
          _scrollingDown = false;
          _offset += _columnCount;
          int iPage = _offset / (_rowCount * _columnCount);
          if ((_offset % (_rowCount * _columnCount)) != 0)
          {
            iPage++;
          }
          _controlUpDown.Value = iPage + 1;
          if (_scrollingDownLast)
          {
            _scrollingDownLast = false;
            _cursorX = _listItems.Count % _columnCount - 1 >= 0 ? _listItems.Count % _columnCount - 1 : 0;
          }
          if (_compensatingDown)
          {
            _cursorY -= 1;
            _compensatingDown = false;
            OnDown();
          }
        }
        int iMaxRows = _listItems.Count / _columnCount;
        int iHalfLastRowElements = _listItems.Count % _columnCount;
        if (iHalfLastRowElements > 0)
        {
          iMaxRows++;
        }
        int iCurrRow = (_offset + _columnCount + (_cursorY) * _columnCount) / _columnCount;
        //if (_cursorY + 1 > _rowCount - _scrollStartOffset && iCurrRow != iMaxRows) _cursorY--;
        int iNextRow = (_offset + _columnCount + (_cursorY + 1) * _columnCount) / _columnCount;

        //if ((_cursorY + 1 >= _rowCount - _scrollStartOffset) && (iNextRow <= iMaxRows - _scrollStartOffset))
        if ((_cursorY + 1 >= _rowCount - _scrollStartOffset) && (_offset + (_columnCount * _rowCount) < _listItems.Count))
          
        {
          // we reached the scroll row
          {
            _offset += _columnCount;
            if (!ValidItem(_cursorX, _cursorY))
            {
              // we reached the last row 
              _offset -= _columnCount;
              int iOffsetInc = _columnCount - _cursorX - 1 + iHalfLastRowElements;
              _offset += iOffsetInc;
              if (!ValidItem(_cursorX, _cursorY))
              {
                SetDownFocus(action, iOffsetInc);
              }
              else
              {
                _offset -= iOffsetInc;// +iHalfLastRowElements;
                _scrollCounter = _itemHeight;
                _scrollingDown = true;
                _scrollingDownLast = true;
              }
            }
            else
            {
              {
                _offset -= _columnCount;
                if (_cursorY > _rowCount - _scrollStartOffset - 1)
                  _compensatingDown = true;
                  //_cursorY -= 1;
                _scrollCounter = _itemHeight;
                _scrollingDown = true;
              }
            }

            OnSelectionChanged();
            _lastCommandTime = AnimationTimer.TickCount;
            return;
          }
        }
        else
        {
          if (ValidItem(_cursorX, _cursorY + 1))
          {
            _cursorY++;
          }
          else if (ValidItem(iHalfLastRowElements - 1, _cursorY + 1))
          {
            _cursorX = iHalfLastRowElements - 1;
            _cursorY++;
          }
          else
          {
            SetDownFocus(action, 0);
          }
          OnSelectionChanged();
        }
      }
      else
      {
        _controlUpDown.OnAction(action);
        if (!_controlUpDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
      }
      _lastCommandTime = AnimationTimer.TickCount;
    }

    private void SetDownFocus(Action action, int offsetDecrease) {
      if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
      {
        //check if _downControlId is set -> then go to the window
        if (NavigateDown > 0)
        {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID,
                                            NavigateDown, (int)action.wID, 0, null);
            GUIGraphicsContext.SendMessage(msg);
        }
        else
        {
          // move to the first row
          _offset = 0;
          _cursorY = 0;
        }
      }
      else if (offsetDecrease != 0)
      {
        _offset -= offsetDecrease;
      }
    }

    public string ScrollySuffix
    {
      get { return _suffix; }
      set { _suffix = value; }
    }

    private void OnPageUp()
    {
      int iItemsPerPage = _rowCount * _columnCount;

      // Sanity check, we shouldnt do anything if there arent more than one page of items
      if (iItemsPerPage > _listItems.Count)
      {
        return;
      }

      if (_offset >= iItemsPerPage)
      {
        _offset -= iItemsPerPage;
      }
      else
      {
        _offset = 0;
        if (_cursorY == 0)
        {
          _offset = (CalculateRows(_listItems.Count) - _rowCount) * _columnCount;
          _cursorY = _rowCount - _scrollStartOffset - 1;
        }
        else
        {
          _cursorY = 0;
        }
      }
      _controlUpDown.Value = CalculatePages(_offset);
      OnSelectionChanged();
    }

    private void OnPageDown()
    {
      int iItemsPerPage = _rowCount * _columnCount;
      int lastPageOffset = (CalculateRows(_listItems.Count) - _rowCount) * _columnCount;

      // Sanity check, shouldnt scroll when there isnt enough items.
      if (iItemsPerPage > _listItems.Count)
      {
        return;
      }

      if (_offset + iItemsPerPage <= lastPageOffset)
      {
        _offset += iItemsPerPage;
      }
      else
      {
        _offset = lastPageOffset;
        if ((_offset + (_cursorY + 1) * _columnCount + _cursorX) >= _listItems.Count)
        {
          _offset = 0;
          _cursorY = 0;
        }
        else
        {
          while ((_offset + (_cursorY + 1) * _columnCount + _cursorX) < _listItems.Count)
          {
            _cursorY++;
          }
        }
      }

      _controlUpDown.Value = CalculatePages(_offset);
      OnSelectionChanged();
    }


    public void SetTextureDimensions(int iWidth, int iHeight)
    {
      if (iWidth < 0 || iHeight < 0)
      {
        return;
      }
      _textureWidth = iWidth;
      _textureHeight = iHeight;

      if (_listButtons != null)
      {
        foreach (GUIButtonControl btn in _listButtons)
        {
          btn.Height = _textureHeight;
          btn.Width = _textureWidth;
          btn.DoUpdate();
        }
      }

      if (_listLabels != null)
      {
        foreach (GUIFadeLabel fadelabel in _listLabels)
        {
          fadelabel.Height = _textureHeight;
          fadelabel.Width = _textureWidth;
          fadelabel.DoUpdate();
        }
      }
    }

    public void SetThumbDimensions(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0 || iYpos < 0 || iWidth < 0 || iHeight < 0)
      {
        return;
      }
      _thumbNailWidth = iWidth;
      _thumbNailHeight = iHeight;
      _xPositionThumbNail = iXpos;
      _yPositionThumbNail = iYpos;
    }

    public void GetThumbDimensions(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iWidth = _thumbNailWidth;
      iHeight = _thumbNailHeight;
      iXpos = _xPositionThumbNail;
      iYpos = _yPositionThumbNail;
    }


    public int ItemWidth
    {
      get { return _itemWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }

        _itemWidth = value;
        Dispose();
        AllocResources();
      }
    }

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
        Dispose();
        AllocResources();
      }
    }

    public bool ShowTexture
    {
      get { return _showTexture; }
      set { _showTexture = value; }
    }

    public int GetSelectedItem(ref string strLabel, ref string strLabel2, ref string strThumbnail, ref string strIndex)
    {
      strLabel = "";
      strLabel2 = "";
      strThumbnail = "";
      strIndex = "";
      int iItem = _offset + _cursorY * _columnCount + _cursorX;
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        GUIListItem pItem = _listItems[iItem];
        strLabel = pItem.Label;
        strLabel2 = pItem.Label2;
        int index = iItem;

        if (_listItems[0].Label != "..")
          index++;
        if (pItem.Label == "..")
          strIndex = "";
        else
          strIndex = index.ToString();

        if (pItem.IsFolder)
        {
          strLabel = String.Format("{0}{1}{2}", _folderPrefix, pItem.Label, _folderSuffix);
        }
        strThumbnail = pItem.ThumbnailImage;
      }
      return iItem;
    }


    public void ShowBigIcons(bool bOnOff)
    {
      if (bOnOff)
      {
        _itemWidth = _bigItemWidth;
        _itemHeight = _bigItemHeight;
        _textureWidth = _bigTextureWidth;
        _textureHeight = _bigTextureHeight;
        SetThumbDimensions(_positionXThumbBig, _positionYThumbBig, _widthThumbBig, _heightThumbBig);
        SetTextureDimensions(_textureWidth, _textureHeight);
      }
      else
      {
        _itemWidth = _lowItemWidth;
        _itemHeight = _lowItemHeight;
        _textureWidth = _lowTextureWidth;
        _textureHeight = _lowTextureHeight;
        SetThumbDimensions(_xPositionThumbNailLow, _yPositionThumbNailLow, _widthThumbNailLow, _heightThumbNailLow);
        SetTextureDimensions(_textureWidth, _textureHeight);
      }
      Calculate();
      _refresh = true;
    }


    public void GetThumbDimensionsBig(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iXpos = _positionXThumbBig;
      iYpos = _positionYThumbBig;
      iWidth = _widthThumbBig;
      iHeight = _heightThumbBig;
    }

    public void GetThumbDimensionsLow(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iXpos = _xPositionThumbNailLow;
      iYpos = _yPositionThumbNailLow;
      iWidth = _widthThumbNailLow;
      iHeight = _heightThumbNailLow;
    }

    public long TextColor
    {
      get { return _textColor; }
    }

    public long SelectedColor
    {
      get { return _selectedColor; }
    }

    public string FontName
    {
      get { return _fontName; }
    }

    public int SpinWidth
    {
      get { return _controlUpDown.Width / 2; }
    }

    public int SpinHeight
    {
      get { return _controlUpDown.Height; }
    }

    public string TexutureUpName
    {
      get { return _controlUpDown.TexutureUpName; }
    }

    public string TexutureDownName
    {
      get { return _controlUpDown.TexutureDownName; }
    }

    public string TexutureUpFocusName
    {
      get { return _controlUpDown.TexutureUpFocusName; }
    }

    public string TexutureDownFocusName
    {
      get { return _controlUpDown.TexutureDownFocusName; }
    }

    public long SpinTextColor
    {
      get { return _controlUpDown.TextColor; }
    }

    public int SpinX
    {
      get { return _controlUpDown.XPosition; }
    }

    public int SpinY
    {
      get { return _controlUpDown.YPosition; }
    }

    public int TextureWidth
    {
      get { return _textureWidth; }
    }

    public int TextureHeight
    {
      get { return _textureHeight; }
    }

    public string FocusName
    {
      get { return _imageFolderNameFocus; }
    }

    public string NoFocusName
    {
      get { return _imageFolderName; }
    }


    public int TextureWidthBig
    {
      get { return _bigTextureWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _bigTextureWidth = value;
      }
    }

    public int TextureHeightBig
    {
      get { return _bigTextureHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _bigTextureHeight = value;
      }
    }

    public int ItemWidthBig
    {
      get { return _bigItemWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _bigItemWidth = value;
      }
    }

    public int ItemHeightBig
    {
      get { return _bigItemHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _bigItemHeight = value;
      }
    }

    public int TextureWidthLow
    {
      get { return _lowTextureWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _lowTextureWidth = value;
      }
    }

    public int TextureHeightLow
    {
      get { return _lowTextureHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _lowTextureHeight = value;
      }
    }

    public int ItemWidthLow
    {
      get { return _lowItemWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _lowItemWidth = value;
      }
    }

    public int ItemHeightLow
    {
      get { return _lowItemHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _lowItemHeight = value;
      }
    }

    public void SetThumbDimensionsLow(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0 || iYpos < 0 || iWidth < 0 || iHeight < 0)
      {
        return;
      }
      _xPositionThumbNailLow = iXpos;
      _yPositionThumbNailLow = iYpos;
      _widthThumbNailLow = iWidth;
      _heightThumbNailLow = iHeight;
    }

    public void SetThumbDimensionsBig(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0 || iYpos < 0 || iWidth < 0 || iHeight < 0)
      {
        return;
      }
      _positionXThumbBig = iXpos;
      _positionYThumbBig = iYpos;
      _widthThumbBig = iWidth;
      _heightThumbBig = iHeight;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      int id;
      bool focus;
      if (_verticalScrollBar.HitTest(x, y, out id, out focus))
      {
        return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        Focus = false;
        return false;
      }
      if (_controlUpDown.HitTest(x, y, out id, out focus))
      {
        if (_controlUpDown.GetMaximum() > 1)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_UPDOWN;
          _controlUpDown.Focus = true;
          if (!_controlUpDown.Focus)
          {
            m_iSelect = GUIListControl.ListType.CONTROL_LIST;
          }
          return true;
        }
        return true;
      }
      m_iSelect = GUIListControl.ListType.CONTROL_LIST;
      y -= _positionY;
      x -= _positionX;
      int iCursorY = (y / _itemHeight);
      int iCursorX = (x / _itemWidth);

      // Check item inside panel
      if (iCursorX < 0 || iCursorX >= _columnCount)
      {
        return false;
      }
      if (iCursorY < 0 || iCursorY >= _rowCount)
      {
        return false;
      }
      _cursorY = iCursorY;
      _cursorX = iCursorX;

      while (_cursorX > 0 && _offset + _cursorY * _columnCount + _cursorX >= _listItems.Count)
      {
        _cursorX--;
      }
      while (_cursorY > 0 && _offset + _cursorY * _columnCount + _cursorX >= _listItems.Count)
      {
        _cursorY--;
      }
      OnSelectionChanged();

      return true;
    }

    public void Sort(IComparer<GUIListItem> comparer)
    {
      try
      {
        _listItems.Sort(comparer);
      }
      catch (Exception) {}
      _refresh = true;
    }

    public override bool NeedRefresh()
    {
      if (_refresh)
      {
        _refresh = false;
        return true;
      }
      if (_scrollingDown)
      {
        return true;
      }
      if (_scrollingUp)
      {
        return true;
      }
      return false;
    }

    public GUIVerticalScrollbar Scrollbar
    {
      get { return _verticalScrollBar; }
    }


    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (IsFocused != value)
        {
          base.Focus = value;
        }
        _controlUpDown.Focus = false;
      }
    }

    public void Add(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      _listItems.Add(item);
      int iItemsPerPage = _rowCount * _columnCount;
      int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
      if (iItemsPerPage != 0)
      {
        if ((_listItems.Count % iItemsPerPage) != 0)
        {
          iPages++;
        }
      }
      _controlUpDown.SetRange(1, iPages);
      _controlUpDown.Value = 1;
      _refresh = true;
    }

    public void Replace(int index, GUIListItem item)
    {
      if (item != null && index >= 0 && index < _listItems.Count)
      {
        _listItems[index] = item;
      }
    }

    public void Insert(int index, GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      _listItems.Insert(index, item);
      int iItemsPerPage = _rowCount * _columnCount;
      int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
      if (iItemsPerPage != 0)
      {
        if ((_listItems.Count % iItemsPerPage) != 0)
        {
          iPages++;
        }
      }
      _controlUpDown.SetRange(1, iPages);
      _controlUpDown.Value = 1;
      _refresh = true;
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
        if (_controlUpDown != null)
        {
          _controlUpDown.ParentID = value;
        }
      }
    }


    public override int WindowId
    {
      get { return _windowId; }
      set
      {
        _windowId = value;
        if (_controlUpDown != null)
        {
          _controlUpDown.WindowId = value;
        }
      }
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
        int iItem = _offset + _cursorY * _columnCount + _cursorX;
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
        int iItem = _offset + _cursorY * _columnCount + _cursorX;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          return iItem;
        }
        return -1;
      }
      set
      {
        SelectItem(value);
        OnSelectionChanged();
        _refresh = true;
      }
    }

    public virtual int RemoveItem(int iItem)
    {
      if (iItem < 0 || iItem > _listItems.Count)
      {
        return -1;
      }

      try
      {
        Monitor.Enter(this);
        _listItems.RemoveAt(iItem);
      }
      catch (Exception ex)
      {
        Log.Error("GUIThumbnailPanel.RemoveItem caused an exception: {0}", ex.Message);
      }
      finally
      {
        Monitor.Exit(this);
      }
      _refresh = true;
      return SelectedListItemIndex;
    }

    public void Clear()
    {
      _listItems.DisposeAndClear();
      //GUITextureManager.CleanupThumbs();
      _controlUpDown.SetRange(1, 1);
      _controlUpDown.Value = 1;
      _cursorX = _cursorY = _offset = 0;
      _frameLimiter = 1;
      _refresh = true;
      OnSelectionChanged();
    }

    public virtual int MoveItemDown(int iItem)
    {
      int selectedItemIndex = -1;

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
        //_log.Info("Moving List Item {0} down. Old index:{1}, new index{2}", item1.Path, iItem, iNextItem);
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
      int selectedItemIndex = -1;

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
        //_log.Info("Moving List Item {0} up. Old index:{1}, new index{2}", item1.Path, iItem, iPreviousItem);
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

    //GUIImage GetThumbnail(GUIListItem item)
    //{
    //  if (item.IsRemote) return null;
    //  if (item.IsFolder)
    //  {
    //    if (item.Label == "..")
    //      return null;
    //    string file = System.IO.Path.GetFileName(item.Path);
    //    string path = item.Path.Substring(0, item.Path.Length - (file.Length + 1));
    //    ThumbnailDatabase dbs = ThumbnailDatabaseCache.Get(path);
    //    ThumbNail thumb = dbs.Get(item.Path);
    //    if (thumb == null) return null;
    //    using (Image img = thumb.Image)
    //    {
    //      GUITextureManager.LoadFromMemory(img, String.Format("[{0}]", file), 0, img.Width, img.Height);
    //      GUIImage guiImage = new GUIImage(0, 0, 0, 0, 0, 0, String.Format("[{0}]", file), 0x0);
    //      return guiImage;
    //    }
    //  }
    //  else
    //  {
    //    string file = System.IO.Path.GetFileName(item.Path); ;
    //    string path = item.Path.Substring(0, item.Path.Length - (file.Length + 1));
    //    ThumbnailDatabase dbs = ThumbnailDatabaseCache.Get(path);
    //    ThumbNail thumb = dbs.Get(file);
    //    if (thumb == null) return null;
    //    using (Image img = thumb.Image)
    //    {
    //      GUITextureManager.LoadFromMemory(img, String.Format("[{0}]", file), 0, img.Width, img.Height);
    //      GUIImage guiImage = new GUIImage(0, 0, 0, 0, 0, 0, String.Format("[{0}]", file), 0x0);
    //      return guiImage;
    //    }
    //  }
    //}

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_controlUpDown != null)
        {
          _controlUpDown.DimColor = value;
        }
        if (_verticalScrollBar != null)
        {
          _verticalScrollBar.DimColor = value;
        }
        foreach (GUIListItem item in _listItems)
        {
          item.DimColor = value;
        }

        if (_listLabels != null)
        {
          foreach (GUIFadeLabel fadelabel in _listLabels)
          {
            fadelabel.DimColor = DimColor;
          }
        }
      }
    }

    protected int CalculatePages(int item)
    {
      int pages = item / (_rowCount * _columnCount) + 1; // Pages are starting from 1 => +1
      if (item % (_rowCount * _columnCount) > 0)
      {
        pages++;
      }
      return pages;
    }

    protected int CalculateRows(int item)
    {
      int rows = item / _columnCount;
      if (item % _columnCount > 0)
      {
        rows++;
      }
      return rows;
    }


    protected void ScrollItems(int itemInc)
    {
      int currentRow = CalculateRows(_offset + _cursorY * _columnCount + _cursorX);
      int nextRow = CalculateRows(_offset + _cursorY * _columnCount + _cursorX + itemInc);
    }

    public bool EnableSMSsearch
    {
      get { return _enableSMSsearch; }
      set { _enableSMSsearch = value; }
    }

    public List<GUIListItem> ListItems
    {
      get { return _listItems; }
      set { _listItems = value; }
    }

    public void SetNeedRefresh()
    {
      _refresh = true;
    }
  }
}