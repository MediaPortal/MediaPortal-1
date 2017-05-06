#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
using MediaPortal.Drawing;
using MediaPortal.ExtensionMethods;
using Microsoft.DirectX.Direct3D;

// used for Keys definition

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIFilmstripControl : GUIControl
  {
    //TODO : add comments
    //TODO : use GUILabelControl for drawing text
    private const int SLEEP_FRAME_COUNT = 2;
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
    [XMLSkinElement("thumbPosXBig")] protected int _positionXThumbBig = 0;
    [XMLSkinElement("thumbPosYBig")] protected int _positionYThumbBig = 0;
    [XMLSkinElement("thumbWidthBig")] protected int _widthThumbBig = 0;
    [XMLSkinElement("thumbHeightBig")] protected int _heightThumbBig = 0;

    [XMLSkinElement("imageFolder")] protected string _imageFolderName = "";
    [XMLSkin("imageFolder", "mask")] protected string _imageFolderMask = "";
    [XMLSkinElement("imageFolderFocus")] protected string _imageFolderNameFocus = "";
    [XMLSkin("imageFolderFocus", "mask")] protected string _imageFolderFocusMask = "";

    [XMLSkinElement("enableFocusZoom")] protected bool _enableFocusZoom = true;

    [XMLSkinElement("textureUp")] protected string _upTextureName = "";
    [XMLSkinElement("textureDown")] protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus = "";

    [XMLSkinElement("spinColor")] protected long _spinControlColor;
    [XMLSkinElement("spinAlign")] protected Alignment _spinControlAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("spinHeight")] protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")] protected int _spinControlWidth;
    [XMLSkinElement("spinPosX")] protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")] protected int _spinControlPositionY;

    [XMLSkinElement("itemHeight")] protected int _itemHeight;
    [XMLSkinElement("itemWidth")] protected int _itemWidth;
    [XMLSkinElement("texturelowHeight")] protected int _textureLowHeight;
    [XMLSkinElement("textureLowWidth")] protected int _textureLowWidth;
    [XMLSkinElement("textureWidth")] protected int _textureWidth = 80;
    [XMLSkinElement("textureHeight")] protected int _textureHeight = 80;
    [XMLSkinElement("thumbPosX")] protected int _thumbNailPositionX = 8;
    [XMLSkinElement("thumbPosY")] protected int _thumbNailPositionY = 8;
    [XMLSkinElement("thumbWidth")] protected int _thumbNailWidth = 64;
    [XMLSkinElement("thumbHeight")] protected int _thumbNailHeight = 64;
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")] protected long _selectedColor = 0xFFFFFFFF;

    [XMLSkinElement("scrollbarBackground")] protected string _scrollbarBackgroundName = "";
    [XMLSkinElement("scrollbarLeft")] protected string _scrollbarLeftName = "";
    [XMLSkinElement("scrollbarRight")] protected string _scrollbarRightName = "";
    [XMLSkinElement("scrollbarYOff")] protected int _scrollbarOffsetY = 0;
    [XMLSkinElement("scrollbarWidth")] protected int _scrollbarWidth = 400;
    [XMLSkinElement("scrollbarHeight")] protected int _scrollbarHeight = 15;
    [XMLSkinElement("showScrollbar")] protected bool _showScrollbar = true;
    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = 1;
    [XMLSkinElement("scrollOffset")] protected int _scrollStartOffset = 0;
    // this is the offset from the first or last element on screen when scrolling should start

    [XMLSkinElement("folderPrefix")] protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")] protected string _folderSuffix = "]";

    [XMLSkinElement("backgroundheight")] protected int _backGroundHeight;
    [XMLSkinElement("backgroundwidth")] protected int _backGroundWidth;
    [XMLSkinElement("backgroundx")] protected int _backGroundPositionX;
    [XMLSkinElement("backgroundy")] protected int _backGroundPositionY;
    [XMLSkinElement("backgrounddiffuse")] protected int _backGroundDiffuseColor;
    [XMLSkinElement("background")] protected string _backgroundTextureName;
    [XMLSkinElement("showBackGround")] protected bool _showBackGround = true;

    [XMLSkinElement("showInfoImage")] protected bool _showInfoImage = true;
    [XMLSkinElement("InfoImageheight")] protected int _infoImageHeight;
    [XMLSkinElement("InfoImagewidth")] protected int _infoImageWidth;
    [XMLSkinElement("InfoImagex")] protected int _infoImagePositionX;
    [XMLSkinElement("InfoImagey")] protected int _infoImagePositionY;
    [XMLSkinElement("InfoImagediffuse")] protected int _infoImageDiffuseColor;
    [XMLSkinElement("InfoImage")] protected string _infoImageName;
    [XMLSkinElement("unfocusedAlpha")] protected int _unfocusedAlpha = 0xFF;
    [XMLSkinElement("frame")] protected string _frameName = "";
    [XMLSkin("frame", "mask")] protected string _frameMask = "";
    [XMLSkinElement("showFrame")] protected bool _showFrame = true;
    [XMLSkinElement("showFolder")] protected bool _showFolder = true;
    [XMLSkinElement("frameFocus")] protected string _frameFocusName = "";
    [XMLSkin("frameFocus", "mask")] protected string _frameFocusMask = "";
    [XMLSkinElement("keepaspectratio")] protected bool _keepAspectRatio = true;

    [XMLSkinElement("showWatchedImage")] protected bool _showWatchedImage = false;
    [XMLSkin("showWatchedImage", "OnFolder")] protected bool _showWatchedImageOnFolder = false;
    [XMLSkin("showWatchedImage", "OnlyOnFocus")] protected bool _showWatchedImageOnlyOnFocus = false;
    [XMLSkinElement("WatchedImagePosX")] protected int _watchedImagePosXLow = 0;
    [XMLSkin("WatchedImagePosX", "Big")] protected int _watchedImagePosXBig = 0;
    [XMLSkinElement("WatchedImagePosY")] protected int _watchedImagePosYLow = 0;
    [XMLSkin("WatchedImagePosY", "Big")] protected int _watchedImagePosYBig = 0;
    [XMLSkinElement("WatchedImageWidth")] protected int _watchedImageWidthLow = 0;
    [XMLSkin("WatchedImageWidth", "Big")] protected int _watchedImageWidthBig = 0;
    [XMLSkinElement("WatchedImageHeight")] protected int _watchedImageHeightLow = 0;
    [XMLSkin("WatchedImageHeight", "Big")] protected int _watchedImageHeightBig = 0;
    [XMLSkinElement("WatchedImageWatchedTexture")] protected string _watchedImageWatchedTexture = string.Empty;
    [XMLSkinElement("WatchedImageUnWatchedTexture")] protected string _watchedImageUnWatchedTexture = string.Empty;

    [XMLSkinElement("showFolderStatusImage")] protected bool _showFolderStatusImage = false;
    [XMLSkinElement("FolderStatusImagePosX")] protected int _folderStatusImagePosXLow = 0;
    [XMLSkin("FolderStatusImagePosX", "Big")] protected int _folderStatusImagePosXBig = 0;
    [XMLSkinElement("FolderStatusImagePosY")] protected int _folderStatusImagePosYLow = 0;
    [XMLSkin("FolderStatusImagePosY", "Big")] protected int _folderStatusImagePosYBig = 0;
    [XMLSkinElement("FolderStatusImageWidth")] protected int _folderStatusImageWidthLow = 0;
    [XMLSkin("FolderStatusImageWidth", "Big")] protected int _folderStatusImageWidthBig = 0;
    [XMLSkinElement("FolderStatusImageHeight")] protected int _folderStatusImageHeightLow = 0;
    [XMLSkin("FolderStatusImageHeight", "Big")] protected int _folderStatusImageHeightBig = 0;
    [XMLSkinElement("FolderStatusImageUserGroupTexture")] protected string _folderStatusImageUserGroupTexture = string.Empty; // Movie User group
    [XMLSkinElement("FolderStatusImageCollectionTexture")] protected string _folderStatusImageCollectionTexture = string.Empty; // Movie Collection
    [XMLSkinElement("FolderStatusImageBdDvdFolderTexture")] protected string _folderStatusImageBdDvdFolderTexture = string.Empty; // BD/DVD
    [XMLSkinElement("FolderStatusImageRemoteTexture")] protected string _folderStatusImageRemoteTexture = string.Empty; // Remote

    [XMLSkinElement("showRatingImage")] protected bool _showRatingImage = false; // Show Rating if Rating greater than 0
    [XMLSkin("showRatingImage", "UserRating")] protected bool _showUserRatingImage = false; // Show UserRating instead Rating if UserRating greater than 0
    [XMLSkinElement("RatingImagePosX")] protected int _ratingImagePosXLow = 0;
    [XMLSkin("RatingImagePosX", "Big")] protected int _ratingImagePosXBig = 0;
    [XMLSkinElement("RatingImagePosY")] protected int _ratingImagePosYLow = 0;
    [XMLSkin("RatingImagePosY", "Big")] protected int _ratingImagePosYBig = 0;
    [XMLSkinElement("RatingImageWidth")] protected int _ratingImageWidthLow = 0;
    [XMLSkin("RatingImageWidth", "Big")] protected int _ratingImageWidthBig = 0;
    [XMLSkinElement("RatingImageHeight")] protected int _ratingImageHeightLow = 0;
    [XMLSkin("RatingImageHeight", "Big")] protected int _ratingImageHeightBig = 0;
    [XMLSkinElement("RatingImageTexturePrefix")] protected string _ratingImageTexturePrefix = string.Empty; // Filename -> Prefix + RatingNumber + Suffix (if Suffix empty then .png)
    [XMLSkinElement("RatingImageTextureSuffix")] protected string _ratingImageTextureSuffix = string.Empty; // For Prefix = Rating, Rating = 5, Suffix = White.png -> Rating5White.png
    [XMLSkinElement("RatingUserImageTexturePrefix")] protected string _ratingUserImageTexturePrefix = string.Empty; // Filename -> Prefix + UserRatingNumber + Suffix (if Suffix empty then .png)
    [XMLSkinElement("RatingUserImageTextureSuffix")] protected string _ratingUserImageTextureSuffix = string.Empty; // For Prefix = Rating, UserRating = 10, Suffix = Red.png -> Rating10Red.png

    [XMLSkinElement("showNewImage")] protected bool _showNewImage = false;
    [XMLSkin("showNewImage", "HotDays")] protected int _newImageHotDays = -1; // -1 Disable
    [XMLSkin("showNewImage", "NewDays")] protected int _newImageNewDays = 3; // -1 Disable
    [XMLSkinElement("NewImagePosX")] protected int _newImagePosXLow = 0;
    [XMLSkin("NewImagePosX", "Big")] protected int _newImagePosXBig = 0;
    [XMLSkinElement("NewImagePosY")] protected int _newImagePosYLow = 0;
    [XMLSkin("NewImagePosY", "Big")] protected int _newImagePosYBig = 0;
    [XMLSkinElement("NewImageWidth")] protected int _newImageWidthLow = 0;
    [XMLSkin("NewImageWidth", "Big")] protected int _newImageWidthBig = 0;
    [XMLSkinElement("NewImageHeight")] protected int _newImageHeightLow = 0;
    [XMLSkin("NewImageHeight", "Big")] protected int _newImageHeightBig = 0;
    [XMLSkinElement("NewImageTexture")] protected string _newImageTexture = "hot.png";
    [XMLSkinElement("NewImageHotTexture")] protected string _newImageTextureHot = "new.png";

    [XMLSkinElement("allowScrolling")] protected bool _allowScrolling = false; // Allow Scrolling in FadeLabel

    [XMLSkinElement("thumbZoom")] protected bool _zoom = false;
    [XMLSkinElement("thumbAlign")] protected Alignment _imageAlignment = Alignment.ALIGN_CENTER;
    [XMLSkinElement("thumbVAlign")] protected VAlignment _imageVAlignment = VAlignment.ALIGN_BOTTOM;
    [XMLSkin("thumbs", "flipX")] protected bool _flipX = false;
    [XMLSkin("thumbs", "flipY")] protected bool _flipY = false;
    [XMLSkin("thumbs", "diffuse")] protected string _diffuseFileName = "";
    [XMLSkin("thumbs", "mask")] protected string _textureMask = "";

    [XMLSkin("InfoImage", "flipX")] protected bool _flipInfoImageX = false;
    [XMLSkin("InfoImage", "flipY")] protected bool _flipInfoImageY = false;
    [XMLSkin("InfoImage", "diffuse")] protected string _diffuseInfoImageFileName = "";
    [XMLSkin("InfoImage", "mask")] protected string _infoImageMask = "";

    [XMLSkinElement("textXOff")] protected int _textXOff = 0;
    [XMLSkinElement("textYOff")] protected int _textYOff = 0;
    [XMLSkinElement("spinCanFocus")] protected bool _spinCanFocus = true;

    [XMLSkinElement("bdDvdDirectoryColor")] protected long _bdDvdDirectoryColor = 0xFFFFFFFF;

    #endregion

    private int _watchedImagePosX = 0;
    private int _watchedImagePosY = 0;
    private int _watchedImageWidth = 0;
    private int _watchedImageHeight = 0;

    private int _folderStatusImagePosX = 0;
    private int _folderStatusImagePosY = 0;
    private int _folderStatusImageWidth = 0;
    private int _folderStatusImageHeight = 0;

    private int _ratingImagePosX = 0;
    private int _ratingImagePosY = 0;
    private int _ratingImageWidth = 0;
    private int _ratingImageHeight = 0;

    private int _newImagePosX = 0;
    private int _newImagePosY = 0;
    private int _newImageWidth = 0;
    private int _newImageHeight = 0;

    private int _itemLowHeight;
    private int _itemLowWidth;
    private int _itemBigHeight;
    private int _itemBigWidth;
    private int _textureBigHeight;
    private int _textureBigWidth;
    private bool _showTexture = true;
    private int _offset = 0;
    private GUIFont _font = null;
    private GUISpinControl _upDownControl = null;
    private GUIListControl.ListType _listType = GUIListControl.ListType.CONTROL_LIST;
    private int _cursorX = 0;
    private int _columns;
    private bool _scrollingRight = false;
    private bool _scrollingLeft = false;
    private int _scrollCounter = 0;
    private int _sleeper = 0;
    private string _suffix = "|";

    private int _lowThumbNailPositionX = 0;
    private int _lowThumbNailPositionY = 0;
    private int _lowThumbNailPositionWidth = 0;
    private int _lowThumbNailPositionHeight = 0;

    private List<GUIListItem> _listItems = new List<GUIListItem>();
    private int _frames = 0;
    private bool _refresh = false;
    protected GUIHorizontalScrollbar _horizontalScrollbar = null;
    protected string _brackedText;
    protected string _scollText;
    private GUIAnimation _imageBackground;
    private GUIImage _imageInfo;
    private DateTime _idleTimer = DateTime.Now;
    private bool _infoChanged = false;
    private bool _reAllocate = false;
    private string _newInfoImageName = "";
    private int _frameLimiter = 1;
    protected double _scrollOffset = 0.0f;
    protected double _timeElapsed = 0.0f;
    protected bool _scrollContinuously = false;
    protected List<GUIAnimation> _frameControl = new List<GUIAnimation>();
    protected List<GUIAnimation> _frameFocusControl = new List<GUIAnimation>();

    private List<GUIAnimation> _imageFolder = new List<GUIAnimation>();
    private List<GUIAnimation> _imageFolderFocus = new List<GUIAnimation>();

    protected List<GUIFadeLabel> _listLabels = new List<GUIFadeLabel>();

    protected List<VisualEffect> _allThumbAnimations = new List<VisualEffect>();

    // Search            
    private DateTime _timerKey = DateTime.Now;
    private char _currentKey = (char)0;
    private char _previousKey = (char)0;
    protected string _searchString = "";
    protected int _lastSearchItem = 0;
    protected bool _enableSMSsearch = true;

    public GUIFilmstripControl(int dwParentID)
      : base(dwParentID) {}

    /*public GUIFilmstripControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, 
      string strFontName, 
      string strImageIcon, 
      string strImageIconFocus, 
      int dwitemWidth, int dwitemHeight, 
      int dwSpinWidth, int dwSpinHeight, 
      string strUp, string strDown, 
      string strUpFocus, string strDownFocus, 
      long dwSpinColor, int dwSpinX, int dwSpinY, 
      string strFont, long dwTextColor, long dwSelectedColor, 
      string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
    }*/

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_positionY > _positionY && _spinControlPositionY < _positionY + _height)
      {
        _spinControlPositionY = _positionY + _height;
      }
      for (int i = 0; i < 30; ++i)
      {
        GUIAnimation anim = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth,
                                                 _itemHeight, _imageFolderName);
        anim.ParentControl = this;
        anim.DimColor = DimColor;
        anim.FlipX = _flipX;
        anim.FlipY = _flipY;
        anim.DiffuseFileName = _diffuseFileName;
        anim.MaskFileName = _imageFolderMask;
        //anim.SetAnimations(ThumbAnimations);
        _imageFolder.Add(anim);

        anim = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth, _itemHeight,
                                    _imageFolderNameFocus);
        anim.ParentControl = this;
        anim.DimColor = DimColor;
        anim.FlipX = _flipX;
        anim.FlipY = _flipY;
        anim.DiffuseFileName = _diffuseFileName;
        anim.MaskFileName = _imageFolderFocusMask;
        //anim.SetAnimations(ThumbAnimations);
        _imageFolderFocus.Add(anim);

        anim = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth, _itemHeight,
                                    _frameName);
        anim.ParentControl = this;
        anim.DimColor = DimColor;
        //anim.FlipX = _flipX;
        //anim.FlipY = _flipY;
        anim.DiffuseFileName = _diffuseFileName;
        anim.MaskFileName = _frameMask;
        //anim.SetAnimations(ThumbAnimations);
        _frameControl.Add(anim);

        anim = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _itemWidth, _itemHeight,
                                    _frameFocusName);
        anim.ParentControl = this;
        anim.DimColor = DimColor;
        //anim.FlipX = _flipX;
        //anim.FlipY = _flipY;
        anim.DiffuseFileName = _diffuseFileName;
        anim.MaskFileName = _frameFocusMask;
        //anim.SetAnimations(ThumbAnimations);
        _frameFocusControl.Add(anim);

        // for label
        GUIFadeLabel fadelabel = new GUIFadeLabel(_parentControlId, _controlId, _positionX, _positionY, _textureWidth,
                                                  _textureHeight, _fontName, _textColor, Alignment.ALIGN_LEFT,
                                                  VAlignment.ALIGN_TOP, 0, 0, 0, " | ");
        fadelabel.ParentControl = this;
        fadelabel.AllowScrolling = _allowScrolling;
        fadelabel.ScrollStartDelay = _scrollStartDelay;
        fadelabel.DimColor = DimColor;
        //fadelabel.AllowFadeIn = false;
        _listLabels.Add(fadelabel);
      }

      _upDownControl = new GUISpinControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _spinControlWidth,
                                          _spinControlHeight, _upTextureName, _downTextureName, _upTextureNameFocus,
                                          _downTextureNameFocus, _fontName, _spinControlColor,
                                          GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, _spinControlAlignment);
      _upDownControl.ParentControl = this;
      _upDownControl.DimColor = DimColor;

      _font = GUIFontManager.GetFont(_fontName);



      // Create the horizontal scrollbar.
      int scrollbarWidth = _scrollbarWidth;
      int scrollbarHeight = _scrollbarHeight;
      GUIGraphicsContext.ScaleHorizontal(ref scrollbarWidth);
      GUIGraphicsContext.ScaleVertical(ref scrollbarHeight);
      int scrollbarPosX = _positionX + (_width / 2) - (scrollbarWidth / 2);

      _horizontalScrollbar = new GUIHorizontalScrollbar(_controlId, 0,
                                                        scrollbarPosX, _positionY + _scrollbarOffsetY,
                                                        scrollbarWidth, scrollbarHeight,
                                                        _scrollbarBackgroundName, _scrollbarLeftName,
                                                        _scrollbarRightName);
      _horizontalScrollbar.ParentControl = this;
      _horizontalScrollbar.DimColor = DimColor;


      _upDownControl.Orientation = eOrientation.Horizontal;
      _upDownControl.SetReverse(true);

      _imageBackground = LoadAnimationControl(0, 0, _backGroundPositionX, _backGroundPositionY, _backGroundWidth,
                                              _backGroundHeight, _backgroundTextureName);
      _imageBackground.ParentControl = this;
      _imageBackground.DimColor = DimColor;

      _imageInfo = new GUIImage(0, 0, _infoImagePositionX, _infoImagePositionY, _infoImageWidth, _infoImageHeight,
                                _infoImageName, 0);
      _imageInfo.ParentControl = this;
      _imageInfo.Filtering = true;
      _imageInfo.KeepAspectRatio = true;
      _imageInfo.ImageAlignment = Alignment.ALIGN_CENTER;
      _imageInfo.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
      _imageInfo.HorizontalAlignment = MediaPortal.Drawing.HorizontalAlignment.Center;
      _imageInfo.VerticalAlignment = MediaPortal.Drawing.VerticalAlignment.Center;
      _imageInfo.DimColor = DimColor;
      _imageInfo.FlipX = _flipInfoImageX;
      _imageInfo.FlipY = _flipInfoImageY;
      _imageInfo.DiffuseFileName = _diffuseInfoImageFileName;
      _imageInfo.MaskFileName = _infoImageMask;

      SetThumbDimensionsLow(_thumbNailPositionX, _thumbNailPositionY, _thumbNailWidth, _thumbNailHeight);
      SetTextureDimensions(_textureWidth, _textureHeight);

      _watchedImagePosX = _watchedImagePosXLow;
      _watchedImagePosY = _watchedImagePosYLow;
      _watchedImageWidth = _watchedImageWidthLow;
      _watchedImageHeight = _watchedImageHeightLow;

      _folderStatusImagePosX = _folderStatusImagePosXLow;
      _folderStatusImagePosY = _folderStatusImagePosYLow;
      _folderStatusImageWidth = _folderStatusImageWidthLow;
      _folderStatusImageHeight = _folderStatusImageHeightLow;

      _ratingImagePosX = _ratingImagePosXLow;
      _ratingImagePosY = _ratingImagePosYLow;
      _ratingImageWidth = _ratingImageWidthLow;
      _ratingImageHeight = _ratingImageHeightLow;

      _newImagePosX = _newImagePosXLow;
      _newImagePosY = _newImagePosYLow;
      _newImageWidth = _newImageWidthLow;
      _newImageHeight = _newImageHeightLow;

      GUIImageAllocator.ClearCachedAllocatorImages();

      GUIPropertyManager.SetProperty("#facadeview.focus.X", string.Empty);
      GUIPropertyManager.SetProperty("#facadeview.focus.Y", string.Empty);
      GUIPropertyManager.SetProperty("#facadeview.focus.Width", string.Empty);
      GUIPropertyManager.SetProperty("#facadeview.focus.Height", string.Empty);
    }








    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScaleRectToScreenResolution(ref _spinControlPositionX, ref _spinControlPositionY,
                                                     ref _spinControlWidth, ref _spinControlHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textureWidth, ref _textureHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _thumbNailPositionX, ref _thumbNailPositionY,
                                                     ref _thumbNailWidth, ref _thumbNailHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textureBigWidth, ref _textureBigHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _positionXThumbBig, ref _positionYThumbBig, ref _widthThumbBig,
                                                     ref _heightThumbBig);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _itemBigWidth, ref _itemBigHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _itemWidth, ref _itemHeight);
      GUIGraphicsContext.ScaleVertical(ref _scrollbarOffsetY);

      //_infoImagePositionX += GUIGraphicsContext.OverScanLeft;
      //_infoImagePositionY += GUIGraphicsContext.OverScanTop;

      //_backGroundPositionX += GUIGraphicsContext.OverScanLeft;
      //_backGroundPositionY += GUIGraphicsContext.OverScanTop;
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _infoImagePositionX, ref _infoImagePositionY,
                                                     ref _infoImageWidth, ref _infoImageHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _backGroundPositionX, ref _backGroundPositionY,
                                                     ref _backGroundWidth, ref _backGroundHeight);

      GUIGraphicsContext.ScaleRectToScreenResolution(ref _watchedImagePosXLow, ref _watchedImagePosYLow,
                                                     ref _watchedImageWidthLow,  ref _watchedImageHeightLow);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _folderStatusImagePosXLow, ref _folderStatusImagePosYLow,
                                                     ref _folderStatusImageWidthLow,  ref _folderStatusImageHeightLow);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _ratingImagePosXLow, ref _ratingImagePosYLow,
                                                     ref _ratingImageWidthLow,  ref _ratingImageHeightLow);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _newImagePosXLow, ref _newImagePosYLow,
                                                     ref _newImageWidthLow, ref _newImageHeightLow);

      GUIGraphicsContext.ScaleRectToScreenResolution(ref _watchedImagePosXBig, ref _watchedImagePosYBig,
                                                     ref _watchedImageWidthBig,  ref _watchedImageHeightBig);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _folderStatusImagePosXBig, ref _folderStatusImagePosYBig,
                                                     ref _folderStatusImageWidthBig,  ref _folderStatusImageHeightBig);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _ratingImagePosXBig, ref _ratingImagePosYBig,
                                                     ref _ratingImageWidthBig,  ref _ratingImageHeightBig);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _newImagePosXBig, ref _newImagePosYBig,
                                                     ref _newImageWidthBig, ref _newImageHeightBig);
      _reAllocate = true;
    }

    /// <summary>
    /// Method which is called if the user selected another item in the filmstrip
    /// This method will update the property manager with the properties of the
    /// newly selected item
    /// </summary>
    protected void OnSelectionChanged()
    {
      if (!IsVisible)
      {
        return;
      }
      _scrollOffset = 0.0f;
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
        GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
        GUIPropertyManager.SetProperty("#selectedindex", strIndex);
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED, WindowId, GetID, ParentID, 0, 0, null);
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

    /// <summary>
    /// Make OverlayImages list for GUIListItem 
    /// </summary>
    /// <param name="pItem"></param>
    /// <param name="itemFocused"></param>
    private List<GUIOverlayImage> GetOverlayListForItem(GUIListItem pItem, bool itemFocused)
    {
      List<GUIOverlayImage> _overlayList = new List<GUIOverlayImage>();
      if (pItem == null)
      {
        return _overlayList; 
      }

      // 1. Rating images
      int _rating = (int)Math.Round(pItem.Rating);
      int _userRating = pItem.UserRating;
      if (_showRatingImage && (_rating > 0 || (_userRating > 0 && _showUserRatingImage)))
      {
        GUIOverlayImage _overlayImage = null;
        if (_userRating > 0 && _showUserRatingImage)
        {
          string _fileName = _ratingUserImageTexturePrefix + _userRating;
          _fileName = _fileName + (string.IsNullOrEmpty(_ratingUserImageTextureSuffix) ? ".png" : _ratingUserImageTextureSuffix);
          _overlayImage = new GUIOverlayImage(_ratingImagePosX, _ratingImagePosY, _ratingImageWidth, _ratingImageHeight, _fileName);
        }
        else if (_rating > 0)
        {
          string _fileName = _ratingImageTexturePrefix + _rating;
          _fileName = _fileName + (string.IsNullOrEmpty(_ratingImageTextureSuffix) ? ".png" : _ratingImageTextureSuffix);
          _overlayImage = new GUIOverlayImage(_ratingImagePosX, _ratingImagePosY, _ratingImageWidth, _ratingImageHeight, _fileName);
        }
        if (_overlayImage != null)
        {
          _overlayList.Add(_overlayImage);
        }
      }

      // 2. Watched/UnWatched images
      if (_showWatchedImage && (!pItem.IsFolder || (pItem.IsFolder && _showWatchedImageOnFolder && pItem.Label != "..")))
      {
        GUIOverlayImage _overlayImage = null;
        if (itemFocused || (!itemFocused && !_showWatchedImageOnlyOnFocus))
        {
          if (pItem.IsPlayed && !string.IsNullOrEmpty(_watchedImageWatchedTexture))
          {
            _overlayImage = new GUIOverlayImage(_watchedImagePosX, _watchedImagePosY, _watchedImageWidth, _watchedImageHeight, _watchedImageWatchedTexture);
          }
          else if (!pItem.IsPlayed && !string.IsNullOrEmpty(_watchedImageUnWatchedTexture)) 
          {
            _overlayImage = new GUIOverlayImage(_watchedImagePosX, _watchedImagePosY, _watchedImageWidth, _watchedImageHeight, _watchedImageUnWatchedTexture);
          }
        }
        if (_overlayImage != null)
        {
          _overlayList.Add(_overlayImage);
        }
      }

      // 3. Folder images
      if (_showFolderStatusImage && pItem.IsFolder)
      {
        GUIOverlayImage _overlayImage = null;
        if (pItem.IsUserGroup && !string.IsNullOrEmpty(_folderStatusImageUserGroupTexture))
        {
          _overlayImage = new GUIOverlayImage(_folderStatusImagePosX, _folderStatusImagePosY, _folderStatusImageWidth, _folderStatusImageHeight, _folderStatusImageUserGroupTexture);
        }
        if (pItem.IsCollection && !string.IsNullOrEmpty(_folderStatusImageCollectionTexture))
        {
          _overlayImage = new GUIOverlayImage(_folderStatusImagePosX, _folderStatusImagePosY, _folderStatusImageWidth, _folderStatusImageHeight, _folderStatusImageCollectionTexture);
        }
        if (_overlayImage != null)
        {
          _overlayList.Add(_overlayImage);
        }
      }

      if (_showFolderStatusImage && pItem.IsBdDvdFolder)
      {
        GUIOverlayImage _overlayImage = null;
        if (!string.IsNullOrEmpty(_folderStatusImageBdDvdFolderTexture))
        {
          _overlayImage = new GUIOverlayImage(_folderStatusImagePosX, _folderStatusImagePosY, _folderStatusImageWidth, _folderStatusImageHeight, _folderStatusImageBdDvdFolderTexture);
          _overlayList.Add(_overlayImage);
        }
      }

      if (_showFolderStatusImage && pItem.IsRemote)
      {
        GUIOverlayImage _overlayImage = null;
        if (!string.IsNullOrEmpty(_folderStatusImageRemoteTexture))
        {
          _overlayImage = new GUIOverlayImage(_folderStatusImagePosX, _folderStatusImagePosY, _folderStatusImageWidth, _folderStatusImageHeight, _folderStatusImageRemoteTexture);
          _overlayList.Add(_overlayImage);
        }
      }

      // 4. New images
      if (_showNewImage && (_newImageHotDays > -1 || _newImageNewDays > -1) && pItem.Updated != DateTime.MinValue)
      {
        int diffDays = (DateTime.Now - pItem.Updated).Days;
        GUIOverlayImage _overlayImage = null;
        if (_newImageHotDays > 0 && !string.IsNullOrEmpty(_newImageTextureHot))
        {
          if (diffDays <= _newImageHotDays)
          {
            _overlayImage = new GUIOverlayImage(_newImagePosX, _newImagePosY, _newImageWidth, _newImageHeight, _newImageTextureHot);
          }
        }
        if (_newImageNewDays > 0 && !string.IsNullOrEmpty(_newImageTexture))
        {
          if (diffDays > _newImageHotDays && diffDays <= _newImageNewDays)
          {
            _overlayImage = new GUIOverlayImage(_newImagePosX, _newImagePosY, _newImageWidth, _newImageHeight, _newImageTexture);
          }
        }
        if (_overlayImage != null)
        {
          _overlayList.Add(_overlayImage);
        }
      }

      return _overlayList;
    }

    /// <summary>
    /// Method to render a single item of the filmstrip
    /// </summary>
    /// <param name="bFocus">true if item shown be drawn focused, false for normal mode</param>
    /// <param name="dwPosX">x-coordinate of the item</param>
    /// <param name="dwPosY">y-coordinate of the item</param>
    /// <param name="pItem">item itself</param>
    private void RenderItem(int itemNumber, float timePassed, bool bFocus, int dwPosX, int dwPosY, GUIListItem pItem)
    {
      if (_font == null)
      {
        return;
      }
      if (pItem == null)
      {
        return;
      }
      if (dwPosY < 0)
      {
        return;
      }

      bool itemFocused = bFocus && Focus && _listType == GUIListControl.ListType.CONTROL_LIST;

      float fTextHeight = 0, fTextWidth = 0;
      _font.GetTextExtent("W", ref fTextWidth, ref fTextHeight);

      float fTextPosY = (float)dwPosY + (float)_textureHeight;

      TransformMatrix tm = null;
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

      //uint currentTime = (uint) (DXUtil.Timer(DirectXTimer.GetAbsoluteTime)*1000.0);
      uint currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;
      // Set oversized value
      int iOverSized = 0;

      if (itemFocused && _enableFocusZoom)
      {
        iOverSized = (_thumbNailWidth + _thumbNailHeight) / THUMBNAIL_OVERSIZED_DIVIDER;
      }

      GUIImage pImage = null;

      if (pItem.HasThumbnail)
      {
        pImage = pItem.Thumbnail;
        if (null == pImage && _sleeper == 0 && !IsAnimating)
        {
          string _guiImageTexture = GUIImageAllocator.BuildConcatImage("Filmstrip:Thumb", pItem.ThumbnailImage,
                                                                       _thumbNailWidth, _thumbNailHeight,
                                                                       GetOverlayListForItem(pItem, itemFocused));
          pImage = new GUIImage(0, 0, _thumbNailPositionX - iOverSized + dwPosX,
                                      _thumbNailPositionY - iOverSized + dwPosY, 
                                      _thumbNailWidth + 2 * iOverSized,
                                      _thumbNailHeight + 2 * iOverSized, 
                                      _guiImageTexture, 0x0);
          pImage.ParentControl = this;
          pImage.AllocResources();
          pImage.SetAnimations(_allThumbAnimations);

          pItem.Thumbnail = pImage;
          _sleeper += SLEEP_FRAME_COUNT;
        }
      }
      else
      {
        if (pItem.HasIconBig)
        {
          pImage = pItem.IconBig;
          if (null == pImage && _sleeper == 0 && !IsAnimating)
          {
            string _guiImageTexture = GUIImageAllocator.BuildConcatImage("Filmstrip:Big", pItem.IconImageBig, 
                                                                         _thumbNailWidth, _thumbNailHeight,
                                                                         GetOverlayListForItem(pItem, itemFocused));
            pImage = new GUIImage(0, 0, _thumbNailPositionX - iOverSized + dwPosX,
                                        _thumbNailPositionY - iOverSized + dwPosY, 
                                        _thumbNailWidth + 2 * iOverSized,
                                        _thumbNailHeight + 2 * iOverSized, 
                                        _guiImageTexture, 0x0);
            pImage.ParentControl = this;
            pImage.AllocResources();
            pImage.SetAnimations(_allThumbAnimations);

            pItem.IconBig = pImage;
            _sleeper += SLEEP_FRAME_COUNT;
          }
        }
      }

      if (null != pImage)
      {
        pImage.KeepAspectRatio = _keepAspectRatio;
        pImage.FlipX = _flipX;
        pImage.FlipY = _flipY;
        pImage.DiffuseFileName = _diffuseFileName;
        pImage.MaskFileName = _textureMask;

        if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
        {
          pImage.SafeDispose();
          pImage.AllocResources();
        }

        pImage.ZoomFromTop = !pItem.IsFolder && _zoom;
        pImage.ImageAlignment = _imageAlignment;
        pImage.ImageVAlignment = _imageVAlignment;
        pImage.Width = _thumbNailWidth + 2 * iOverSized;
        pImage.Height = _thumbNailHeight + 2 * iOverSized;
        pImage.SetPosition(_thumbNailPositionX - iOverSized + dwPosX, _thumbNailPositionY - iOverSized + dwPosY);
        pImage.DimColor = DimColor;

        if (itemFocused)
        {
          pImage.ColourDiffuse = 0xffffffff;

          if (!_scrollingLeft && !_scrollingRight)
          {
            int _focusX = _thumbNailPositionX - iOverSized + dwPosX;
            int _focusY = _thumbNailPositionY - iOverSized + dwPosY;
            int _focusW = _thumbNailWidth + 2 * iOverSized;
            int _focusH = _thumbNailHeight + 2 * iOverSized;

            GUIPropertyManager.SetProperty("#facadeview.focus.X", _focusX.ToString());
            GUIPropertyManager.SetProperty("#facadeview.focus.Y", _focusY.ToString());
            GUIPropertyManager.SetProperty("#facadeview.focus.Width", _focusW.ToString());
            GUIPropertyManager.SetProperty("#facadeview.focus.Height", _focusH.ToString());
          }
          else
          {
            GUIPropertyManager.SetProperty("#facadeview.focus.X", string.Empty);
            GUIPropertyManager.SetProperty("#facadeview.focus.Y", string.Empty);
            GUIPropertyManager.SetProperty("#facadeview.focus.Width", string.Empty);
            GUIPropertyManager.SetProperty("#facadeview.focus.Height", string.Empty);
          }
        }
        else
        {
          pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
        }

        if (pImage.Focus != itemFocused)
        {
          _imageFolderFocus[itemNumber].Focus = !itemFocused;
          // ensure that _imageFolderFocus is in sync with pImage  }
          _imageFolder[itemNumber].Focus = !itemFocused;
          _frameFocusControl[itemNumber].Focus = !itemFocused;
          _frameControl[itemNumber].Focus = !itemFocused;
        }
        pImage.Focus = itemFocused;

        TransformMatrix matrix = GUIGraphicsContext.ControlTransform;
        GUIGraphicsContext.ControlTransform = new TransformMatrix();
        pImage.UpdateVisibility();
        pImage.DoRender(timePassed, currentTime);
        tm = pImage.getTransformMatrix(currentTime);
        GUIGraphicsContext.ControlTransform = matrix;
      }

      TransformMatrix _matrix = GUIGraphicsContext.ControlTransform;
      GUIGraphicsContext.ControlTransform = new TransformMatrix();

      // if (bFocus == true && Focus && _listType == GUIListControl.ListType.CONTROL_LIST)
      if (itemFocused)
      {
        //doesn't render items when scrolling left if this "if" clause is uncommented - should be fixed later
        if (!_scrollingLeft)
        {
          if (_showFolder)
          {
            _imageFolderFocus[itemNumber].SetPosition(dwPosX, dwPosY);
            _imageFolder[itemNumber].Focus = true;
            _imageFolderFocus[itemNumber].Focus = true;

            if (true == _showTexture)
            {
              _imageFolderFocus[itemNumber].UpdateVisibility();
              _imageFolderFocus[itemNumber].DoRender(timePassed, currentTime);
            }
            for (int i = 0; i < _imageFolderFocus.Count; ++i)
            {
              if (i != itemNumber)
              {
                _imageFolder[i].Focus = false;
                _imageFolderFocus[i].Focus = false;
              }
            }
          }

          if (_showFrame)
          {
            _frameFocusControl[itemNumber].Focus = true;
            _frameFocusControl[itemNumber].SetPosition(dwPosX, dwPosY);
            _frameFocusControl[itemNumber].UpdateVisibility();
            _frameFocusControl[itemNumber].DoRender(timePassed, currentTime);
            for (int i = 0; i < _frameFocusControl.Count; ++i)
            {
              if (i != itemNumber)
              {
                _frameControl[i].Focus = false;
                _frameFocusControl[i].Focus = false;
              }
            }
          }
        }
      }
      else
      {
        //doesn't render items when scrolling left if this "if" clause is uncommented - should be fixed later
        if (!_scrollingLeft)
        {
          if (_showFolder)
          {
            _imageFolder[itemNumber].SetPosition(dwPosX, dwPosY);
            _imageFolder[itemNumber].Focus = false;
            _imageFolderFocus[itemNumber].Focus = false;

            if (true == _showTexture)
            {
              _imageFolder[itemNumber].UpdateVisibility();
              _imageFolder[itemNumber].DoRender(timePassed, currentTime);
            }
          }

          if (_showFrame)
          {
            _frameControl[itemNumber].Focus = false;
            _frameFocusControl[itemNumber].Focus = false;
            _frameControl[itemNumber].SetPosition(dwPosX, dwPosY);
            _frameControl[itemNumber].UpdateVisibility();
            _frameControl[itemNumber].DoRender(timePassed, currentTime);
          }
        }
      }

      if (tm != null)
      {
        GUIGraphicsContext.AddTransform(tm);
      }
      if (pItem.Label != "..")
      {
        _listLabels[itemNumber].XPosition = dwPosX + _textXOff;
        _listLabels[itemNumber].YPosition = (int)Math.Truncate(fTextPosY + _textYOff);
        _listLabels[itemNumber].Width = _textureWidth;
        _listLabels[itemNumber].Height = _textureHeight;
        _listLabels[itemNumber].TextColor = dwColor;
        _listLabels[itemNumber].Label = pItem.Label;
        _listLabels[itemNumber].Visible = true;
        _listLabels[itemNumber].AllowScrolling = _allowScrolling;
        _listLabels[itemNumber].ScrollStartDelay = _scrollStartDelay;
        _listLabels[itemNumber].Render(timePassed);
      }
      else
      {
        _listLabels[itemNumber].Visible = false;
        _listLabels[itemNumber].Render(timePassed);
      }
      if (tm != null)
      {
        GUIGraphicsContext.RemoveTransform();
      }
      GUIGraphicsContext.ControlTransform = _matrix;
    }

    /// <summary>
    /// The render method. 
    /// This method will draw the entire filmstrip
    /// </summary>
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


      // Reallocation of the card images occur (for example) when the window size changes.
      // Force all of the card images to be recreated.
      if (_reAllocate)
      {
        for (int i = 0; i < _listItems.Count; i++)
        {
          GUIListItem pItem = _listItems[i];
          pItem.Thumbnail.SafeDispose();
          pItem.Thumbnail = null;
          pItem.IconBig.SafeDispose();
          pItem.IconBig = null;
          pItem.Icon.SafeDispose();
        }
        GUIImageAllocator.ClearCachedAllocatorImages();
        _reAllocate = false;
      }


      int dwPosY = 0;
      if ((_cursorX > 0) && !ValidItem(_cursorX))
      {
        _cursorX = 0;
        OnSelectionChanged();
      }

      if (_sleeper > 0)
      {
        _sleeper--;
      }

      if (_frameLimiter < GUIGraphicsContext.MaxFPS)
        _frameLimiter++;
      else
        _frameLimiter = 1;

      UpdateInfoImage();
      if (_imageBackground != null && _showBackGround)
      {
        _imageBackground.Render(timePassed);
      }

      int _scrollPosititionXOffset = 0;
      if (true == _scrollingRight)
      {
        _scrollPosititionXOffset = -(_itemWidth - _scrollCounter);
      }
      if (true == _scrollingLeft)
      {
        _scrollPosititionXOffset = _itemWidth - _scrollCounter;
      }

      int iStartItem = 30000;
      int iEndItem = -1;
      dwPosY = _positionY;
      int dwPosX;

      //are we scrolling towards the left?
      int itemNumber = 0;
      if (_scrollingLeft)
      {
        // yes, then render item on right
        dwPosX = _positionX - _itemWidth + _scrollPosititionXOffset;
        int iItem = _offset - 1;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          if (iItem < iStartItem)
          {
            iStartItem = iItem;
          }
          if (iItem > iEndItem)
          {
            iEndItem = iItem;
          }

          GUIListItem pItem = _listItems[iItem];
          RenderItem(itemNumber, timePassed, false, dwPosX, dwPosY, pItem);
          itemNumber++;
        }
      }

      // render main panel
      int focusedItemNr = -1;
      int focuseddwPosX = -1;
      int focuseddwPosY = -1;
      GUIListItem focusedItem = null;
      for (int iCol = 0; iCol < _columns; iCol++)
      {
        dwPosX = _positionX + iCol * _itemWidth + _scrollPosititionXOffset;
        int iItem = iCol + _offset;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          if (iItem < iStartItem)
          {
            iStartItem = iItem;
          }
          if (iItem > iEndItem)
          {
            iEndItem = iItem;
          }

          GUIListItem pItem = _listItems[iItem];
          bool bFocus = (_cursorX == iCol);
          if (bFocus)
          {
            focusedItemNr = itemNumber;
            focuseddwPosX = dwPosX;
            focuseddwPosY = dwPosY;
            focusedItem = pItem;
          }
          else
          {
            RenderItem(itemNumber, timePassed, bFocus, dwPosX, dwPosY, pItem);
          }
          itemNumber++;
        }
      }


      //are we scrolling towards the right?
      if (_scrollingRight)
      {
        // yes, then render the new left item
        dwPosX = _positionX + _columns * _itemWidth + _scrollPosititionXOffset;
        _offset++;
        int iItem = _columns + _offset - 1;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          if (iItem < iStartItem)
          {
            iStartItem = iItem;
          }
          if (iItem > iEndItem)
          {
            iEndItem = iItem;
          }

          GUIListItem pItem = _listItems[iItem];
          RenderItem(itemNumber, timePassed, false, dwPosX, dwPosY, pItem);
          itemNumber++;
        }
        _offset--;
      }
      if (focusedItem != null)
      {
        RenderItem(focusedItemNr, timePassed, true, focuseddwPosX, focuseddwPosY, focusedItem);
      }

      //
      _frames = 6;
      int iStep = _itemHeight / _frames;
      if (0 == iStep)
      {
        iStep = 1;
      }
      if (_scrollingLeft)
      {
        _scrollCounter -= iStep;
        if (_scrollCounter <= 0)
        {
          _scrollingLeft = false;
          _offset--;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0)
          {
            iPage++;
          }
          _upDownControl.Value = iPage + 1;
          _refresh = true;
          OnSelectionChanged();
        }
      }

      if (_scrollingRight)
      {
        _scrollCounter -= iStep;
        if (_scrollCounter <= 0)
        {
          _scrollingRight = false;
          _offset++;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0)
          {
            iPage++;
          }
          _upDownControl.Value = iPage + 1;
          _refresh = true;
          OnSelectionChanged();
        }
      }

      if (_imageInfo != null && _showInfoImage)
      {
        _imageInfo.Render(timePassed);
      }

      //free memory
      if (iStartItem < 30000)
      {
        for (int i = 0; i < iStartItem; ++i)
        {
          if (i >= 0 && i < _listItems.Count)
          {
            GUIListItem pItem = _listItems[i];
            if (null != pItem)
            {
              pItem.FreeMemory();
            }
          }
        }
      }

      for (int i = iEndItem + 1; i < _listItems.Count; ++i)
      {
        if (i >= 0 && i < _listItems.Count)
        {
          GUIListItem pItem = _listItems[i];
          if (null != pItem)
          {
            pItem.FreeMemory();
          }
        }
      }

      dwPosY = _positionY + (_itemHeight);

      if (_scrollingLeft || _scrollingRight)
      {
        _refresh = true;
      }

      if (Focus)
      {
        GUIPropertyManager.SetProperty("#highlightedbutton", string.Empty);
      }

      // Render the horizontal scrollbar.
      RenderScrollbar(timePassed);

      base.Render(timePassed);
    }

    private void RenderScrollbar(float timePassed)
    {
      if (_listItems.Count > _columns && _showScrollbar)
      {

        // Render the spin control
        if (_upDownControl != null)
        {
          _upDownControl.HorizontalContentAlignment = MediaPortal.Drawing.HorizontalAlignment.Right;
          _upDownControl.Render(timePassed);
        }

        if (_horizontalScrollbar != null)
        {
          float fPercent = (float)SelectedListItemIndex / (float)(_listItems.Count - 1) * 100.0f;
          if ((int)fPercent != (int)_horizontalScrollbar.Percentage)
          {
            _horizontalScrollbar.Percentage = fPercent;
          }

          // The scrollbar is only rendered when the mouse support is enabled.  Temporarily bypass the global setting to allow
          // the skin to determine whether or not it should be displayed.
          bool ms = GUIGraphicsContext.MouseSupport;
          GUIGraphicsContext.MouseSupport = true;

          _horizontalScrollbar.IsVisible = _showScrollbar;
          // Guarantee that the scrollbar is visible based on skin setting.
          _horizontalScrollbar.Render(timePassed);

          GUIGraphicsContext.MouseSupport = ms;
        }
      }
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          _refresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          {
            _offset = 0;
            _cursorX = 0;
            _upDownControl.Value = 1;
            OnSelectionChanged();

            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
            int iItem = _listItems.Count - 1;
            if (iItem >= 0)
            {
              int iPage = 1;
              while (iItem >= (_columns) && _columns != 0)
              {
                iItem -= (_columns);
                iPage++;
              }

              iItem = _listItems.Count - 1;
              if (iItem >= _columns)
              {
                _cursorX = _columns - 1;
                _offset = iItem - _cursorX;
              }
              else // if (iItem < _columns)
              {
                _cursorX = iItem;
                _offset = 0;
              }

              if (_upDownControl != null)
              {
                _upDownControl.Value = iPage;
              }
              OnSelectionChanged();
            }
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
            if (!OnUp())
            {
              base.OnAction(action);
              return;
            }
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
            if (_horizontalScrollbar != null)
            {
              if (_horizontalScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
              {
                _horizontalScrollbar.OnAction(action);
                int index = (int)((_horizontalScrollbar.Percentage / 100.0f) * _listItems.Count);
                _offset = index;
                _cursorX = 0;
                OnSelectionChanged();
              }
            }
          }
          break;


        case Action.ActionType.ACTION_MOUSE_CLICK:
            {
              int id;
              bool focus;
              bool returnClick = true;
              if (_horizontalScrollbar != null)
              {
                if (_horizontalScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
                {
                  // We require mouse support for the scrollbar to respond properly.  Temporarily bypass the global setting to allow
                  // the action to work for us.
                  bool ms = GUIGraphicsContext.MouseSupport;
                  GUIGraphicsContext.MouseSupport = true;

                  _horizontalScrollbar.OnAction(action);
                  int index = (int)((_horizontalScrollbar.Percentage / 100.0f) * _listItems.Count);
                  _offset = index;
                  _cursorX = 0;
                  OnSelectionChanged();
                  returnClick = false;

                  GUIGraphicsContext.MouseSupport = ms;
                }
              }


              if (_listType == GUIListControl.ListType.CONTROL_LIST && returnClick == true)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                              (int)Action.ActionType.ACTION_SELECT_ITEM, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
            else
            {
              if (_upDownControl != null)
              {
                _upDownControl.OnAction(action);
              }
            }
            _refresh = true;
          }
          break;

        default:
          {
            if (_listType == GUIListControl.ListType.CONTROL_LIST)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                              (int)action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
              _refresh = true;
            }
            else
            {
              if (_upDownControl != null)
              {
                _upDownControl.OnAction(action);
              }
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
            _offset = (_upDownControl.Value - 1) * (_columns);
            _refresh = true;
            OnSelectionChanged();
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = _offset + _cursorX;
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

        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUIImageAllocator.ClearCachedAllocatorImages();
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in _listItems)
          {
            item.FreeMemory();
          }
          _refresh = true;
          if (_imageInfo != null)
          {
            _imageInfo.SafeDispose();
            _imageInfo.AllocResources();
            _imageInfo.DoUpdate();
          }
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
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
            message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
        {
          if (Disabled || !IsVisible || !CanFocus())
          {
            base.OnMessage(message);
            return true;
          }
          _listType = GUIListControl.ListType.CONTROL_LIST;
          _refresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem newItem = message.Object as GUIListItem;
          if (newItem != null)
          {
            _listItems.Add(newItem);
            int iItemsPerPage = _columns;
            int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
            if (iItemsPerPage != 0)
            {
              if ((_listItems.Count % iItemsPerPage) != 0)
              {
                iPages++;
              }
            }
            if (_upDownControl != null)
            {
              _upDownControl.SetRange(1, iPages);
              _upDownControl.Value = 1;
            }
            _refresh = true;
          }
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
          message.Param1 = _offset + _cursorX;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          int iItem = message.Param1;
          SelectItemIndex(iItem);
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

    private void SelectItemIndex(int iItem)
    {
      int iPage = 1;
      int itemCount = _listItems.Count;
      int itemsPerPage = _columns;

      if (iItem < 0) iItem = 0;
      if (iItem >= itemCount) iItem = itemCount - 1;

      if (iItem >= itemCount - (itemCount % itemsPerPage) && itemCount > itemsPerPage)
      {
        // Special case, jump to last page, but fill entire page
        _offset = itemCount - itemsPerPage;
        iItem = itemsPerPage - (itemCount - iItem);
        iPage = ((_offset + _cursorX) / itemsPerPage) + 1;
      }
      else
      {
        _cursorX = 0;
        _offset = 0;
        while (iItem >= (_columns) && _columns != 0)
        {
          _offset += (_columns);
          iItem -= (_columns);
          iPage++;
        }
        if ((iItem != _scrollStartOffset) && (_listItems.Count > _scrollStartOffset))
        {
          // adjust in the middle
          int delta = _scrollStartOffset - iItem;
          if (_offset >= delta)
          {
            iItem += delta;
            _offset -= delta;
          }
        }
      }
      if (_upDownControl != null)
      {
        _upDownControl.Value = iPage;
      }
      _cursorX = iItem;

      OnSelectionChanged();
      _refresh = true;
    }

    /// <summary>
    /// Search for first item starting with searchkey
    /// </summary>
    /// <param name="SearchKey">SearchKey</param>
    private void SearchItem(string SearchKey, SearchType iSearchMethode)
    {
      // Get selected item
      bool bItemFound = false;
      int iCurrentItem = _offset + _cursorX;
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

      if ((bItemFound) && (iItem >= 0 && iItem < _listItems.Count))
      {
        // update spin controls
        int iPage = 1;
        _cursorX = 0;
        _offset = 0;
        while (iItem >= (_columns) && _columns != 0)
        {
          _offset += (_columns);
          iItem -= (_columns);
          iPage++;
        }
        if (_upDownControl != null)
        {
          _upDownControl.Value = iPage;
        }
        _cursorX = iItem;
      }

      _lastSearchItem = _cursorX + _offset;
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
        _timerKey = DateTime.Now;
      }
      if (Key == '#' || Key == '�')
      {
        _timerKey = DateTime.Now;
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
      _timerKey = DateTime.Now;
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 1000)
      {
        _previousKey = (char)0;
        _currentKey = (char)0;
        //        _searchString = string.Empty;
      }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      if (_upDownControl != null)
      {
        _upDownControl.PreAllocResources();
      }
      for (int i = 0; i < _imageFolder.Count; ++i)
      {
        _imageFolder[i].PreAllocResources();
        _imageFolderFocus[i].PreAllocResources();
        _frameControl[i].PreAllocResources();
        _frameFocusControl[i].PreAllocResources();
        _listLabels[i].PreAllocResources();
      }
      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.PreAllocResources();
      }
      if (_imageBackground != null)
      {
        _imageBackground.PreAllocResources();
      }
      if (_imageInfo != null)
      {
        _imageInfo.PreAllocResources();
      }
      _idleTimer = DateTime.Now;
      _infoChanged = false;
      _newInfoImageName = "";
      _offset = 0;
      _cursorX = 0;
    }


    private void Calculate()
    {
      for (int i = 0; i < _imageFolder.Count; ++i)
      {
        _imageFolder[i].Width = _textureWidth;
        _imageFolder[i].Height = _textureHeight;
        _imageFolderFocus[i].Width = _textureWidth;
        _imageFolderFocus[i].Height = _textureHeight;
      }

      for (int i = 0; i < _frameControl.Count; ++i)
      {
        _frameControl[i].Width = _textureWidth;
        _frameControl[i].Height = _textureHeight;
      }

      for (int i = 0; i < _frameFocusControl.Count; ++i)
      {
        _frameFocusControl[i].Width = _textureWidth;
        _frameFocusControl[i].Height = _textureHeight;
      }

      for (int i = 0; i < _listLabels.Count; ++i)
      {
        _listLabels[i].Width = _textureWidth;
        _listLabels[i].Height = _textureHeight;
      }

      float fWidth = 0, fHeight = 0;
      if (_font != null)
      {
        // height of 1 item = folder image height + text row height + space in between
        _font.GetTextExtent("y", ref fWidth, ref fHeight);
      }

      fWidth = (float)_itemWidth;
      fHeight = (float)_itemHeight;

      _columns = (int)(_width / fWidth);

      int iItemsPerPage = _columns;
      int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
      if (iItemsPerPage != 0)
      {
        if ((_listItems.Count % iItemsPerPage) != 0)
        {
          iPages++;
        }
      }
      if (_upDownControl != null)
      {
        _upDownControl.SetRange(1, iPages);
        _upDownControl.Value = 1;
      }
    }

    public override void AllocResources()
    {
      _sleeper = 0;

      base.AllocResources();
      _font = GUIFontManager.GetFont(_fontName);
      if (_imageBackground != null)
      {
        _imageBackground.AllocResources();
      }
      if (_imageInfo != null)
      {
        _imageInfo.AllocResources();
      }
      if (_upDownControl != null)
      {
        _upDownControl.AllocResources();
      }

      if (ThumbAnimations == null || ThumbAnimations.Count < 1)
        _allThumbAnimations.Add(new VisualEffect());
      else
        _allThumbAnimations.AddRange(ThumbAnimations);

      for (int i = 0; i < _imageFolder.Count; ++i)
      {
        _imageFolder[i].AllocResources();
        _imageFolderFocus[i].AllocResources();
        _frameControl[i].AllocResources();
        _frameFocusControl[i].AllocResources();
        _listLabels[i].AllocResources();

        _imageFolder[i].SetAnimations(_allThumbAnimations);
        _imageFolderFocus[i].SetAnimations(_allThumbAnimations);
        _frameControl[i].SetAnimations(_allThumbAnimations);
        _frameFocusControl[i].SetAnimations(_allThumbAnimations);
      }
      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.AllocResources();
      }

      Calculate();

      _upDownControl.ParentID = GetID;
    }

    public override void Dispose()
    {
      _font = null;
      _upDownControl.SafeDispose();

      _listItems.DisposeAndClear();
      _horizontalScrollbar.SafeDispose();
      _imageBackground.SafeDispose();
      _imageInfo.SafeDispose();

      _frameControl.DisposeAndClear();
      _frameFocusControl.DisposeAndClear();
      _imageFolder.DisposeAndClear();
      _imageFolderFocus.DisposeAndClear();
      _listLabels.DisposeAndClear();

      base.Dispose();
    }


    private bool ValidItem(int iX)
    {
      if (iX >= _columns)
      {
        return false;
      }
      if (_offset + iX < _listItems.Count)
      {
        return true;
      }
      return false;
    }


    private void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        //If scrolloffset larger than half the nr of items make scrolloffset that nr
        int maxScrollOffset = _columns / 2;
        if (_scrollStartOffset > maxScrollOffset)
        {
          _scrollStartOffset = maxScrollOffset;
        }

        if (_scrollingRight)
        {
          _scrollingRight = false;
          _offset++;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0)
          {
            iPage++;
          }
          if (_upDownControl != null)
          {
            _upDownControl.Value = iPage + 1;
          }
        }

        // If cursor offset from edge or if left space smaller than scrollStartOffset
        if (_cursorX + 1 >= _columns - _scrollStartOffset &&
            _listItems.Count - (_offset + _cursorX + 1) > _scrollStartOffset)
        {
          _offset++;
          if (!ValidItem(_cursorX))
          {
            _offset--;
          }
          else
          {
            _offset--;
            _scrollCounter = _itemWidth;
            _scrollingRight = true;
          }
          OnSelectionChanged();
          return;
        }
        else
        {
          if (ValidItem(_cursorX + 1))
          {
            _cursorX++;
            OnSelectionChanged();
          }
          else
          {
            _listType = GUIListControl.ListType.CONTROL_LIST;
            this.Focus = true;
            base.OnAction(action);
          }
        }
      }
      else if (_upDownControl != null)
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_UP)
        {
          _upDownControl.Focus = false;
          _listType = GUIListControl.ListType.CONTROL_LIST;
          this.Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
          if (!_upDownControl.Focus)
          {
            base.OnAction(action);
          }
        }
        OnSelectionChanged();
      }
    }

    private void OnLeft()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        //If scrolloffset larger than half the nr of items make scrolloffset that nr
        int maxScrollOffset = _columns / 2;
        if (_scrollStartOffset > maxScrollOffset)
        {
          _scrollStartOffset = maxScrollOffset;
        }

        if (_scrollingLeft)
        {
          _scrollCounter = 0;
          _scrollingLeft = false;
          _offset--;
          int iPage = _offset / (_columns);
          if ((_offset % (_columns)) != 0)
          {
            iPage++;
          }
          if (_upDownControl != null && _spinCanFocus)
          {
            _upDownControl.Focus = true;
          }
          else
          {
            _listType = GUIListControl.ListType.CONTROL_LIST;
            this.Focus = true;
            base.OnAction(action);
          }
        }

        if (_cursorX > 0 && (_cursorX > _scrollStartOffset || _offset == 0))
        {
          _cursorX--;
        }
        else if (_cursorX <= _scrollStartOffset && _offset != 0)
        {
          _scrollCounter = _itemWidth;
          _scrollingLeft = true;
        }
        else
        {
          base.OnAction(action);
        }
        OnSelectionChanged();
      }
      else if (_upDownControl != null)
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN)
        {
          _upDownControl.Focus = false;
          _listType = GUIListControl.ListType.CONTROL_LIST;
          this.Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
        }
        if (!_upDownControl.Focus)
        {
          _listType = GUIListControl.ListType.CONTROL_LIST;
        }
        OnSelectionChanged();
      }
    }

    private bool OnUp()
    {
      OnSelectionChanged();
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        return false;
      }
      else if (_upDownControl != null)
      {
        if (_upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN ||
            _upDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_UP)
        {
          _upDownControl.Focus = false;
          _listType = GUIListControl.ListType.CONTROL_LIST;
          this.Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
        }
        if (!_upDownControl.Focus)
        {
          _listType = GUIListControl.ListType.CONTROL_LIST;
        }
      }
      return true;
    }

    private void OnDown()
    {
      OnSelectionChanged();
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (_listType == GUIListControl.ListType.CONTROL_LIST)
      {
        _listType = GUIListControl.ListType.CONTROL_UPDOWN;
        if (_upDownControl != null)
        {
          if (_spinCanFocus)
          {
            _upDownControl.Focus = true;
          }
          else
          {
            _listType = GUIListControl.ListType.CONTROL_LIST;
            this.Focus = true;
            base.OnAction(action);
          }
        }
      }
      else if (_upDownControl != null)
      {
        _upDownControl.OnAction(action);
        if (!_upDownControl.Focus)
        {
          base.OnAction(action);
        }
      }
    }

    public string ScrollySuffix
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

    private void OnPageUp()
    {
      if (_upDownControl == null)
      {
        return;
      }

      int iPage = _upDownControl.Value;
      if (iPage > 1)
      {
        iPage--;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1) * _columns;
      }
      else
      {
        _offset = 0;
        _cursorX = 0;
        _upDownControl.Value = 1;
      }
      OnSelectionChanged();
    }

    private void OnPageDown()
    {
      if (_upDownControl == null)
      {
        return;
      }

      int iItemsPerPage = _columns;
      int iItem = _listItems.Count - 1;
      if (iItem <= iItemsPerPage)
      {
        _cursorX = iItem;
        _offset = 0;
        _upDownControl.Value = 1;
      }
      else
      {
        int iPages = _listItems.Count / iItemsPerPage;
        int itemsOnLastPage = _listItems.Count % iItemsPerPage;
        if (itemsOnLastPage != 0)
        {
          iPages++;
        }

        int iPage = _upDownControl.Value;
        if (iPage + 1 <= iPages)
        {
          iPage++;
          _upDownControl.Value = iPage;

          if (iPage == iPages && _cursorX <= itemsOnLastPage) 
          {
            _offset = _listItems.Count - iItemsPerPage;
          }
          else
          {
            _offset += iItemsPerPage;
          }
        }
        else // Already on Last page
        {
          _cursorX = iItemsPerPage - 1;
          _offset = iItem - _cursorX; 
        }
        while (_offset > 0 && _offset + iItemsPerPage >= iItem)
        {
          _offset--;
        }
      }
      OnSelectionChanged();
    }


    public void SetTextureDimensions(int iWidth, int iHeight)
    {
      if (iWidth < 0)
      {
        return;
      }
      if (iHeight < 0)
      {
        return;
      }
      _textureWidth = iWidth;
      _textureHeight = iHeight;

      for (int i = 0; i < _imageFolder.Count; ++i)
      {
        _imageFolder[i].Height = _textureHeight;
        _imageFolder[i].Width = _textureWidth;
        _imageFolderFocus[i].Height = _textureHeight;
        _imageFolderFocus[i].Width = _textureWidth;
        _frameControl[i].Width = _textureWidth;
        _frameControl[i].Height = _textureHeight;
        _frameFocusControl[i].Width = _textureWidth;
        _frameFocusControl[i].Height = _textureHeight;
        _listLabels[i].Width = _textureWidth;
        _listLabels[i].Height = _textureHeight;

        _frameControl[i].Refresh();
        _frameFocusControl[i].Refresh();
        _imageFolder[i].Refresh();
        _imageFolderFocus[i].Refresh();
        _listLabels[i].DoUpdate();
      }
    }

    public void SetThumbDimensions(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0)
      {
        return;
      }
      if (iYpos < 0)
      {
        return;
      }
      if (iWidth < 0)
      {
        return;
      }
      if (iHeight < 0)
      {
        return;
      }
      _thumbNailWidth = iWidth;
      _thumbNailHeight = iHeight;
      _thumbNailPositionX = iXpos;
      _thumbNailPositionY = iYpos;
    }

    public void GetThumbDimensions(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iWidth = _thumbNailWidth;
      iHeight = _thumbNailHeight;
      iXpos = _thumbNailPositionX;
      iYpos = _thumbNailPositionY;
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
        if (_itemWidth < 1)
        {
          _itemWidth = 1;
        }
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
        if (_itemHeight < 1)
        {
          _itemHeight = 1;
        }
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
      int iItem = _offset + _cursorX;
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        GUIListItem pItem = _listItems[iItem];
        if (pItem != null)
        {
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
      }
      return iItem;
    }

    public void ShowBigIcons(bool bOnOff)
    {
      if (bOnOff)
      {
        _itemWidth = _itemBigWidth;
        _itemHeight = _itemBigHeight;
        _textureWidth = _textureBigWidth;
        _textureHeight = _textureBigHeight;

        SetThumbDimensions(_positionXThumbBig, _positionYThumbBig, _widthThumbBig, _heightThumbBig);
        SetTextureDimensions(_textureWidth, _textureHeight);

        _watchedImagePosX = _watchedImagePosXBig;
        _watchedImagePosY = _watchedImagePosYBig;
        _watchedImageWidth = _watchedImageWidthBig;
        _watchedImageHeight = _watchedImageHeightBig;

        _folderStatusImagePosX = _folderStatusImagePosXBig;
        _folderStatusImagePosY = _folderStatusImagePosYBig;
        _folderStatusImageWidth = _folderStatusImageWidthBig;
        _folderStatusImageHeight = _folderStatusImageHeightBig;

        _ratingImagePosX = _ratingImagePosXBig;
        _ratingImagePosY = _ratingImagePosYBig;
        _ratingImageWidth = _ratingImageWidthBig;
        _ratingImageHeight = _ratingImageHeightBig;

        _newImagePosX = _newImagePosXBig;
        _newImagePosY = _newImagePosYBig;
        _newImageWidth = _newImageWidthBig;
        _newImageHeight = _newImageHeightBig;
      }
      else
      {
        _itemWidth = _itemLowWidth;
        _itemHeight = _itemLowHeight;
        _textureWidth = _textureLowWidth;
        _textureHeight = _textureLowHeight;

        SetThumbDimensions(_lowThumbNailPositionX, _lowThumbNailPositionY, _lowThumbNailPositionWidth,
                           _lowThumbNailPositionHeight);
        SetTextureDimensions(_textureWidth, _textureHeight);

        _watchedImagePosX = _watchedImagePosXLow;
        _watchedImagePosY = _watchedImagePosYLow;
        _watchedImageWidth = _watchedImageWidthLow;
        _watchedImageHeight = _watchedImageHeightLow;

        _folderStatusImagePosX = _folderStatusImagePosXLow;
        _folderStatusImagePosY = _folderStatusImagePosYLow;
        _folderStatusImageWidth = _folderStatusImageWidthLow;
        _folderStatusImageHeight = _folderStatusImageHeightLow;

        _ratingImagePosX = _ratingImagePosXLow;
        _ratingImagePosY = _ratingImagePosYLow;
        _ratingImageWidth = _ratingImageWidthLow;
        _ratingImageHeight = _ratingImageHeightLow;

        _newImagePosX = _newImagePosXLow;
        _newImagePosY = _newImagePosYLow;
        _newImageWidth = _newImageWidthLow;
        _newImageHeight = _newImageHeightLow;
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
      iXpos = _lowThumbNailPositionX;
      iYpos = _lowThumbNailPositionY;
      iWidth = _lowThumbNailPositionWidth;
      iHeight = _lowThumbNailPositionHeight;
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
      get
      {
        if (_upDownControl == null)
        {
          return 0;
        }
        return _upDownControl.Width / 2;
      }
    }

    public int SpinHeight
    {
      get
      {
        if (_upDownControl == null)
        {
          return 0;
        }
        return _upDownControl.Height;
      }
    }

    public string TexutureUpName
    {
      get
      {
        if (_upDownControl == null)
        {
          return string.Empty;
        }
        return _upDownControl.TexutureUpName;
      }
    }

    public string TexutureDownName
    {
      get
      {
        if (_upDownControl == null)
        {
          return string.Empty;
        }
        return _upDownControl.TexutureDownName;
      }
    }

    public string TexutureUpFocusName
    {
      get
      {
        if (_upDownControl == null)
        {
          return string.Empty;
        }
        return _upDownControl.TexutureUpFocusName;
      }
    }

    public string TexutureDownFocusName
    {
      get
      {
        if (_upDownControl == null)
        {
          return string.Empty;
        }
        return _upDownControl.TexutureDownFocusName;
      }
    }

    public long SpinTextColor
    {
      get
      {
        if (_upDownControl == null)
        {
          return 0;
        }
        return _upDownControl.TextColor;
      }
    }

    public int SpinX
    {
      get
      {
        if (_upDownControl == null)
        {
          return 0;
        }
        return _upDownControl.XPosition;
      }
    }

    public int SpinY
    {
      get
      {
        if (_upDownControl == null)
        {
          return 0;
        }
        return _upDownControl.YPosition;
      }
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
      get
      {
        if (_imageFolderFocus.Count == 0)
        {
          return string.Empty;
        }
        if (_imageFolderFocus[0] == null)
        {
          return string.Empty;
        }
        return _imageFolderFocus[0].FileName;
      }
    }

    public string NoFocusName
    {
      get
      {
        if (_imageFolder.Count == 0)
        {
          return string.Empty;
        }
        if (_imageFolder[0] == null)
        {
          return string.Empty;
        }
        return _imageFolder[0].FileName;
      }
    }

    public int TextureWidthBig
    {
      get { return _textureBigWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textureBigWidth = value;
        if (_textureBigWidth < 0)
        {
          _textureBigWidth = 0;
        }
      }
    }

    public int TextureHeightBig
    {
      get { return _textureBigHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textureBigHeight = value;
        if (_textureBigHeight < 0)
        {
          _textureBigHeight = 0;
        }
      }
    }

    public int ItemWidthBig
    {
      get { return _itemBigWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _itemBigWidth = value;
        if (_itemBigWidth < 0)
        {
          _itemBigWidth = 0;
        }
      }
    }

    public int ItemHeightBig
    {
      get { return _itemBigHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _itemBigHeight = value;
        if (_itemBigHeight < 0)
        {
          _itemBigHeight = 0;
        }
      }
    }

    public int TextureWidthLow
    {
      get { return _textureLowWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textureLowWidth = value;
        if (_textureLowWidth < 0)
        {
          _textureLowWidth = 0;
        }
      }
    }

    public int TextureHeightLow
    {
      get { return _textureLowHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textureLowHeight = value;
        if (_textureLowHeight < 0)
        {
          _textureLowHeight = 0;
        }
      }
    }

    public int ItemWidthLow
    {
      get { return _itemLowWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _itemLowWidth = value;
        if (_itemLowWidth < 0)
        {
          _itemLowWidth = 0;
        }
      }
    }

    public int ItemHeightLow
    {
      get { return _itemLowHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _itemLowHeight = value;
        if (_itemLowHeight < 0)
        {
          _itemLowHeight = 0;
        }
      }
    }

    public void SetThumbDimensionsLow(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0)
      {
        return;
      }
      if (iYpos < 0)
      {
        return;
      }
      if (iWidth < 0)
      {
        return;
      }
      if (iHeight < 0)
      {
        return;
      }
      _lowThumbNailPositionX = iXpos;
      _lowThumbNailPositionY = iYpos;
      _lowThumbNailPositionWidth = iWidth;
      _lowThumbNailPositionHeight = iHeight;
    }

    public void SetThumbDimensionsBig(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0)
      {
        return;
      }
      if (iYpos < 0)
      {
        return;
      }
      if (iWidth < 0)
      {
        return;
      }
      if (iHeight < 0)
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
      if (_horizontalScrollbar != null)
      {
        if (_horizontalScrollbar.HitTest(x, y, out id, out focus))
        {
          return true;
        }
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        Focus = false;
        if (_upDownControl != null)
        {
          if (!_upDownControl.HitTest(x, y, out id, out focus))
          {
            return false;
          }
        }
      }

      if (_upDownControl != null)
      {
        if (_upDownControl.HitTest(x, y, out id, out focus))
        {
          if (_upDownControl.GetMaximum() > 1)
          {
            _listType = GUIListControl.ListType.CONTROL_UPDOWN;
            _upDownControl.Focus = true;
            if (!_upDownControl.Focus)
            {
              _listType = GUIListControl.ListType.CONTROL_LIST;
            }
            return true;
          }
          return true;
        }
      }

      _listType = GUIListControl.ListType.CONTROL_LIST;
      y -= _positionY;
      x -= _positionX;
      _cursorX = (x / _itemWidth);

      while (_cursorX > 0 && _offset + _cursorX >= _listItems.Count)
      {
        _cursorX--;
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
      if (_scrollingLeft)
      {
        return true;
      }
      if (_scrollingRight)
      {
        return true;
      }
      return false;
    }

    public GUIHorizontalScrollbar Scrollbar
    {
      get { return _horizontalScrollbar; }
    }

    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (IsFocused != value && value)
        {
          if (_showTexture == true)
          {
            for (int i = 0; i < _imageFolderFocus.Count; ++i)
            {
              _imageFolderFocus[i].Begin();
            }
          }
        }
        base.Focus = value;
      }
    }

    public void Add(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      _listItems.Add(item);
      int iItemsPerPage = _columns;
      int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
      if (iItemsPerPage != 0)
      {
        if ((_listItems.Count % iItemsPerPage) != 0)
        {
          iPages++;
        }
      }
      if (_upDownControl != null)
      {
        _upDownControl.SetRange(1, iPages);
        _upDownControl.Value = 1;
      }
      _refresh = true;
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
        Log.Error("GUIFilmstripControl.RemoveItem caused an exception: {0}", ex.Message);
      }
      finally
      {
        Monitor.Exit(this);
      }
      _refresh = true;
      return SelectedListItemIndex;
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
      int iItemsPerPage = _columns;
      int iPages = iItemsPerPage == 0 ? 0 : _listItems.Count / iItemsPerPage;
      if (iItemsPerPage != 0)
      {
        if ((_listItems.Count % iItemsPerPage) != 0)
        {
          iPages++;
        }
      }
      if (_upDownControl != null)
      {
        _upDownControl.SetRange(1, iPages);
        _upDownControl.Value = 1;
      }
      _refresh = true;
    }

    public int BackgroundX
    {
      get { return _backGroundPositionX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backGroundPositionX = value;
        if (_imageBackground != null)
        {
          _imageBackground.XPosition = value;
        }
      }
    }

    public int BackgroundY
    {
      get { return _backGroundPositionY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backGroundPositionY = value;
        if (_imageBackground != null)
        {
          _imageBackground.YPosition = value;
        }
      }
    }

    public int BackgroundWidth
    {
      get { return _backGroundWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backGroundWidth = value;
        if (_imageBackground != null)
        {
          _imageBackground.Width = value;
        }
      }
    }

    public int BackgroundHeight
    {
      get { return _backGroundHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _backGroundHeight = value;
        if (_imageBackground != null)
        {
          _imageBackground.Height = value;
        }
      }
    }

    public long BackgroundDiffuse
    {
      get
      {
        if (_imageBackground != null)
        {
          return _imageBackground.ColourDiffuse;
        }
        return 0;
      }
      set
      {
        if (_imageBackground != null)
        {
          _imageBackground.ColourDiffuse = value;
        }
      }
    }

    public string BackgroundFileName
    {
      get
      {
        if (_imageBackground != null)
        {
          return _imageBackground.FileName;
        }
        return string.Empty;
      }
      set
      {
        if (value == null)
        {
          return;
        }
        if (_imageBackground != null)
        {
          _imageBackground.SetFileName(value);
        }
      }
    }

    public int InfoImageX
    {
      get { return _infoImagePositionX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _infoImagePositionX = value;
        if (_imageInfo != null)
        {
          _imageInfo.XPosition = value;
        }
      }
    }

    public int InfoImageY
    {
      get { return _infoImagePositionY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _infoImagePositionY = value;
        if (_imageInfo != null)
        {
          _imageInfo.YPosition = value;
        }
      }
    }

    public int InfoImageWidth
    {
      get { return _infoImageWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _infoImageWidth = value;
        if (_imageInfo != null)
        {
          _imageInfo.Width = value;
        }
      }
    }

    public int InfoImageHeight
    {
      get { return _infoImageHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _infoImageHeight = value;
        if (_imageInfo != null)
        {
          _imageInfo.Height = value;
        }
      }
    }

    public long InfoImageDiffuse
    {
      get
      {
        if (_imageInfo != null)
        {
          return _imageInfo.ColourDiffuse;
        }
        return 0;
      }
      set
      {
        if (_imageInfo != null)
        {
          _imageInfo.ColourDiffuse = value;
        }
      }
    }

    public string InfoImageFileName
    {
      get
      {
        if (_imageInfo != null)
        {
          return _imageInfo.FileName;
        }
        return string.Empty;
      }
      set
      {
        if (value == null)
        {
          return;
        }
        _newInfoImageName = value;
        _infoChanged = true;
        _idleTimer = DateTime.Now;
      }
    }

    private void UpdateInfoImage()
    {
      if (_imageInfo == null)
      {
        return;
      }
      if (_infoChanged)
      {
        TimeSpan ts = DateTime.Now - _idleTimer;
        if (ts.TotalMilliseconds > 500)
        {
          _imageInfo.SetFileName(_newInfoImageName);
          _newInfoImageName = "";
          _infoChanged = false;
        }
      }
    }

    /// <summary>
    /// Method to store(save) the current control rectangle
    /// </summary>
    public override void StorePosition()
    {
      if (_imageInfo != null)
      {
        _imageInfo.StorePosition();
      }
      if (_upDownControl != null)
      {
        _upDownControl.StorePosition();
      }
      if (_imageBackground != null)
      {
        _imageBackground.StorePosition();
      }
      for (int i = 0; i < _imageFolder.Count; ++i)
      {
        _imageFolder[i].StorePosition();
        _imageFolderFocus[i].StorePosition();
        _frameControl[i].StorePosition();
        _frameFocusControl[i].StorePosition();
        _listLabels[i].StorePosition();
      }
      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.StorePosition();
      }

      base.StorePosition();
    }

    /// <summary>
    /// Method to restore the saved-current control rectangle
    /// </summary>
    public override void ReStorePosition()
    {
      if (_imageInfo != null)
      {
        _imageInfo.ReStorePosition();
      }
      if (_upDownControl != null)
      {
        _upDownControl.ReStorePosition();
      }
      if (_imageBackground != null)
      {
        _imageBackground.ReStorePosition();
      }

      for (int i = 0; i < _imageFolder.Count; ++i)
      {
        _imageFolder[i].ReStorePosition();
        _imageFolderFocus[i].ReStorePosition();
        _frameControl[i].ReStorePosition();
        _frameFocusControl[i].ReStorePosition();
        _listLabels[i].ReStorePosition();
      }
      if (_horizontalScrollbar != null)
      {
        _horizontalScrollbar.ReStorePosition();
      }

      if (_imageInfo != null)
      {
        _imageInfo.GetRect(out _infoImagePositionX, out _infoImagePositionY, out _infoImageWidth, out _infoImageHeight);
      }
      if (_imageBackground != null)
      {
        _imageBackground.GetRect(out _backGroundPositionX, out _backGroundPositionY, out _backGroundWidth,
                                 out _backGroundHeight);
      }
      if (_upDownControl != null)
      {
        _upDownControl.GetRect(out _spinControlPositionX, out _spinControlPositionY, out _spinControlWidth,
                               out _spinControlHeight);
      }

      base.ReStorePosition();
    }

    /// <summary>
    /// Method to get animate the current control
    /// </summary>
    public override void Animate(float timePassed, Animator animator)
    {
      if (animator == null)
      {
        return;
      }
      if (_imageInfo != null)
      {
        _imageInfo.Animate(timePassed, animator);
      }
      if (_upDownControl != null)
      {
        _upDownControl.Animate(timePassed, animator);
      }
      if (_imageBackground != null)
      {
        _imageBackground.Animate(timePassed, animator);
      }
      base.Animate(timePassed, animator);
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
        int iItem = _offset + _cursorX;
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
        int iItem = _offset + _cursorX;
        if (iItem >= 0 && iItem < _listItems.Count)
        {
          return iItem;
        }
        return -1;
      }
      set
      {
        SelectItemIndex(value);
      }
    }

    public void Clear()
    {
      GUIImageAllocator.ClearCachedAllocatorImages();
      GUIPropertyManager.SetProperty("#facadeview.focus.X", string.Empty);
      GUIPropertyManager.SetProperty("#facadeview.focus.Y", string.Empty);
      GUIPropertyManager.SetProperty("#facadeview.focus.Width", string.Empty);
      GUIPropertyManager.SetProperty("#facadeview.focus.Height", string.Empty);

      _listItems.DisposeAndClear();
      //GUITextureManager.CleanupThumbs();
      _upDownControl.SetRange(1, 1);
      _upDownControl.Value = 1;
      _cursorX = _offset = 0;
      _frameLimiter = 1;
      _refresh = true;
      OnSelectionChanged();
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
        if (_imageInfo != null)
        {
          _imageInfo.DimColor = value;
        }
        if (_upDownControl != null)
        {
          _upDownControl.DimColor = value;
        }

        for (int i = 0; i < _imageFolder.Count; ++i)
        {
          _imageFolder[i].DimColor = value;
          _imageFolderFocus[i].DimColor = value;
          _frameControl[i].DimColor = value;
          _frameFocusControl[i].DimColor = value;
          _listLabels[i].DimColor = value;
        }
        if (_horizontalScrollbar != null)
        {
          _horizontalScrollbar.DimColor = value;
        }
        foreach (GUIListItem item in _listItems)
        {
          item.DimColor = value;
        }
      }
    }

    public bool EnableSMSsearch
    {
      get { return _enableSMSsearch; }
      set { _enableSMSsearch = value; }
    }

    /// <summary>
    /// Get/set if the aspectration of the images of the items needs to be kept.
    /// </summary>
    public bool KeepAspectRatio
    {
      get { return _keepAspectRatio; }
      set { _keepAspectRatio = value; }
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