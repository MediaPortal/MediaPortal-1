#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *  Copyright (C) 2005-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Microsoft.DirectX.Direct3D;
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
    } ;

    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    } ;

    [XMLSkinElement("spaceBetweenItems")] protected int _spaceBetweenItems = 2;
    protected int _offset = 0;
    protected int _itemsPerPage = 10;
    protected int _lastItemPageValue = 0;

    [XMLSkinElement("textureHeight")] protected int _itemHeight = 10;
    protected ListType _listType = ListType.CONTROL_LIST;
    protected int _cursorX = 0;
    [XMLSkinElement("textXOff")] protected int _textOffsetX;
    [XMLSkinElement("textYOff")] protected int _textOffsetY;
    [XMLSkinElement("textXOff2")] protected int _textOffsetX2;
    [XMLSkinElement("textYOff2")] protected int _textOffsetY2;
    [XMLSkinElement("textXOff3")] protected int _textOffsetX3;
    [XMLSkinElement("textYOff3")] protected int _textOffsetY3;

    [XMLSkinElement("itemWidth")] protected int _imageWidth = 16;
    [XMLSkinElement("itemHeight")] protected int _imageHeight = 16;

    protected bool _upDownControlVisible = true;
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;

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

    protected bool _refresh = false;
    protected string _textLine;
    protected string _textLine2;
    protected string _brackedText;
    [XMLSkinElement("IconXOff")] protected int _iconOffsetX = 8;
    [XMLSkinElement("IconYOff")] protected int _iconOffsetY = 5;

    protected int _scrollPosition = 0;
    protected int _scrollPosititionX = 0;
    protected int _lastItem = -1;

    protected double _scrollOffsetX = 0.0f;
    protected int _currentFrame = 0;
    protected double _timeElapsed = 0.0f;
    protected bool _scrollContinuosly = false;

    [XMLSkinElement("scrollOffset")] protected int _scrollStartOffset = 0;
                                                   // this is the offset from the first or last element on screen when scrolling should start

    protected int _loopDelay = 100; // wait at the last item this amount of msec until loop to the first item
    protected double _lastCommandTime = 0;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedHorizontal)*0.01f); }
    }

    [XMLSkinElement("keepaspectratio")] protected bool _keepAspectRatio = false;
    protected bool _drawFocus = true;
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

    [XMLSkinElement("folderPrefix")] protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")] protected string _folderSuffix = "]";

    [XMLSkinElement("textureUp")] protected string _upTextureName = "";
    [XMLSkinElement("textureDown")] protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus = "";
    [XMLSkinElement("textureNoFocus")] protected string _buttonNonFocusName = "";
    [XMLSkinElement("textureFocus")] protected string _buttonFocusName = "";

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

    [XMLSkinElement("spinCanFocus")] protected bool _spinCanFocus = true;

    private bool _wordWrapping = false;

    // Search            
    private DateTime _timerKey = DateTime.Now;
    private char _currentKey = (char) 0;
    private char _previousKey = (char) 0;
    protected string _searchString = "";
    protected int _lastSearchItem = 0;

    public GUIListControl(int dwParentID)
      : base(dwParentID)
    {
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
    public GUIListControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                          int dwSpinWidth, int dwSpinHeight,
                          string strUp, string strDown,
                          string strUpFocus, string strDownFocus,
                          long dwSpinColor, int dwSpinX, int dwSpinY,
                          string strFont, long dwTextColor, long dwSelectedColor,
                          string strButton, string strButtonFocus,
                          string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
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

      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
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
                                          GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, _spinControlAlignment);
      _upDownControl.ParentControl = this;
      _upDownControl.DimColor = DimColor;

      _verticalScrollbar = new GUIVerticalScrollbar(_controlId, 0, 5 + _positionX + _width, _positionY, 15, _height,
                                                    _scrollbarBackgroundName, _scrollbarTopName, _scrollbarBottomName);
      _verticalScrollbar.ParentControl = this;
      _verticalScrollbar.SendNotifies = false;
      _verticalScrollbar.DimColor = DimColor;
      _upDownControl.WindowId = WindowId;
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

    public override bool CanFocus()
    {
      return base.CanFocus();
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
      _currentFrame = 0;
      _timeElapsed = 0.0f;
      // Reset searchstring
      if (_lastSearchItem != (_cursorX + _offset))
      {
        _previousKey = (char) 0;
        _currentKey = (char) 0;
        _searchString = "";
      }

      string strSelected = "";
      string strSelected2 = "";
      string strThumb = "";
      int item = GetSelectedItem(ref strSelected, ref strSelected2, ref strThumb);
      if (!GUIWindowManager.IsRouted)
      {
        GUIPropertyManager.SetProperty("#selecteditem", strSelected);
        GUIPropertyManager.SetProperty("#selecteditem2", strSelected2);
        GUIPropertyManager.SetProperty("#selectedthumb", strThumb);
        GUIPropertyManager.SetProperty("#highlightedbutton", strSelected);
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
        GUIPropertyManager.SetProperty("#selecteditem", "{" + _searchString.ToLower() + "}");
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
          pItem.RetrieveArt = true;
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
              btn.ColourDiffuse = 0xffffffff;
            }
            else
            {
              btn.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
            }
            btn.Focus = gotFocus;
            btn.SetPosition(x, y);
            btn.Render(timePassed);
          }
          btn = null;
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
          pImage = new GUIImage(0, 0, 0, 0, _imageWidth, _imageHeight, pItem.IconImage, 0x0);
          pImage.ParentControl = this;
          pImage.KeepAspectRatio = _keepAspectRatio;
          pImage.AllocResources();
          pItem.Icon = pImage;
        }
        if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
        {
          pImage.FreeResources();
          pImage.AllocResources();
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
        pImage.DimColor = DimColor;
        pImage.Render(timePassed);
        pImage = null;
      }
      pItem = null;
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
          pinImage = new GUIImage(0, 0, 0, 0, _widthPinIcon, _heightPinIcon, pItem.PinImage, 0x0);
          pinImage.ParentControl = this;
          pinImage.KeepAspectRatio = _keepAspectRatio;
          pinImage.AllocResources();
          pItem.PinIcon = pinImage;
        }
        pinImage.KeepAspectRatio = _keepAspectRatio;
        pinImage.Width = PinIconWidth;
        pinImage.Height = PinIconHeight;


        if (PinIconOffsetY < 0 || PinIconOffsetX < 0)
        {
          pinImage.SetPosition(x + (_width) - (pinImage.TextureWidth + pinImage.TextureWidth/2),
                               y + (_height/2) - (pinImage.TextureHeight/2));
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
        pinImage.DimColor = DimColor;
        pinImage.Render(timePassed);
        pinImage = null;
      } //if (pItem.HasPinIcon)
      pItem = null;
    }

    protected virtual void RenderLabel(float timePassed, int buttonNr, int dwPosX, int dwPosY, bool gotFocus)
    {
      GUIListItem pItem = _listItems[buttonNr + _offset];
      long dwColor = _textColor;
      if (pItem.Shaded)
      {
        dwColor = ShadedColor;
      }

      if (pItem.Selected)
      {
        dwColor = _selectedColor;
      }

      if (!Focus)
      {
        dwColor &= DimColor;
      }

      dwPosX += _textOffsetX;
      bool bSelected = false;
      if (buttonNr == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST)
      {
        bSelected = true;
      }

      int dMaxWidth = (_width - _textOffsetX - _imageWidth - GUIGraphicsContext.ScaleHorizontal(20));
      if ((_text2Visible && pItem.Label2.Length > 0) &&
          (_textOffsetY == _textOffsetY2))
      {
        dwColor = _textColor2;

        if (pItem.Selected)
        {
          dwColor = _selectedColor2;
        }

        if (pItem.IsPlayed)
        {
          dwColor = _playedColor;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }

        if (!Focus)
        {
          dwColor &= DimColor;
        }

        int xpos = dwPosX;
        int ypos = dwPosY;

        if (0 == _textOffsetX2)
        {
          xpos = _positionX + _width - GUIGraphicsContext.ScaleHorizontal(16);
        }
        else
        {
          xpos = _positionX + _textOffsetX2;
        }

        if ((_labelControls2 != null) &&
            (buttonNr >= 0) && (buttonNr < _labelControls2.Count))
        {
          GUILabelControl label2 = _labelControls2[buttonNr];
          if (label2 != null)
          {
            label2.SetPosition(xpos, ypos + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY2);

            if (gotFocus || !Focus)
            {
              label2.TextColor = dwColor;
            }
            else
            {
              label2.TextColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int) dwColor)).ToArgb();
            }

            label2.Label = pItem.Label2;
            label2.TextAlignment = Alignment.ALIGN_RIGHT;
            label2.FontName = _fontName2Name;
            dMaxWidth = label2._positionX - dwPosX - label2.TextWidth - GUIGraphicsContext.ScaleHorizontal(20);
            //dMaxWidth -= (label2.TextWidth + GUIGraphicsContext.ScaleHorizontal(20));
          }
        }
      }

      _textLine = pItem.Label;
      if (_text1Visible)
      {
        dwColor = _textColor;

        if (pItem.Selected)
        {
          dwColor = _selectedColor;
        }

        if (pItem.IsPlayed)
        {
          dwColor = _playedColor;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }
        if (!pItem.Selected && !gotFocus)
        {
          dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int) dwColor)).ToArgb();
        }
        if (!Focus)
        {
          dwColor &= DimColor;
        }

        RenderText(timePassed, buttonNr, (float) dwPosX,
                   (float) dwPosY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY, (float) dMaxWidth, dwColor,
                   _textLine, bSelected);
      }

      if (pItem.Label2.Length > 0)
      {
        dwColor = _textColor2;

        if (pItem.Selected)
        {
          dwColor = _selectedColor2;
        }

        if (pItem.IsPlayed)
        {
          dwColor = _playedColor;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }
        if (!Focus)
        {
          dwColor &= DimColor;
        }

        if (0 == _textOffsetX2)
        {
          dwPosX = _positionX + _width - GUIGraphicsContext.ScaleHorizontal(16);
        }
        else
        {
          dwPosX = _positionX + _textOffsetX2;
        }

        _textLine = pItem.Label2;

        if (_text2Visible &&
            (_labelControls2 != null) &&
            (buttonNr >= 0) && (buttonNr < _labelControls2.Count))
        {
          GUILabelControl label2 = _labelControls2[buttonNr];
          if (label2 != null)
          {
            label2.SetPosition(dwPosX - GUIGraphicsContext.ScaleHorizontal(6),
                               dwPosY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY2);
            if (gotFocus || !Focus)
            {
              label2.TextColor = dwColor;
            }
            else
            {
              label2.TextColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int) dwColor)).ToArgb();
            }
            label2.Label = _textLine;
            label2.TextAlignment = Alignment.ALIGN_RIGHT;
            label2.FontName = _fontName2Name;
            label2.Render(timePassed);
            label2 = null;
          }
        }
      }

      if (pItem.Label3.Length > 0)
      {
        dwColor = _textColor3;

        if (pItem.Selected)
        {
          dwColor = _selectedColor3;
        }

        if (pItem.IsPlayed)
        {
          dwColor = _playedColor;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }
        if (!Focus)
        {
          dwColor &= DimColor;
        }

        if (0 == _textOffsetX3)
        {
          dwPosX = _positionX + _textOffsetX;
        }
        else
        {
          dwPosX = _positionX + _textOffsetX3;
        }

        int ypos = dwPosY;

        if (0 == _textOffsetY3)
        {
          ypos += _textOffsetY2;
        }
        else
        {
          ypos += _textOffsetY3;
        }

        if (_text3Visible &&
            (_labelControls3 != null) &&
            (buttonNr >= 0) && (buttonNr < _labelControls3.Count))
        {
          GUILabelControl label3 = _labelControls3[buttonNr];

          if (label3 != null)
          {
            label3.SetPosition(dwPosX, ypos);
            if (gotFocus || !Focus)
            {
              label3.TextColor = dwColor;
            }
            else
            {
              label3.TextColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int) dwColor)).ToArgb();
            }
            label3.Label = pItem.Label3;
            label3.TextAlignment = Alignment.ALIGN_LEFT;
            label3.FontName = _fontName3Name;
            label3.Width = (_width - _textOffsetX - _imageWidth - GUIGraphicsContext.ScaleHorizontal(34));

            RenderText(timePassed, buttonNr, label3, bSelected);
            label3 = null;
          }
        }
      }
      pItem = null;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      _timeElapsed += timePassed;
      _currentFrame = (int) (_timeElapsed/TimeSlice);

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

      int dwPosY = _positionY;

      // Render the buttons first.
      for (int i = 0; i < _itemsPerPage; i++)
      {
        if (i + _offset < _listItems.Count)
        {
          // render item
          bool gotFocus = false;
          if (_drawFocus && i == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST)
          {
            gotFocus = true;
          }
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
          bool gotFocus = false;
          if (_drawFocus && i == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST)
          {
            gotFocus = true;
          }

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

          dwPosX += (_imageWidth + ten);
          // render the text
          RenderLabel(timePassed, i, labelX, dwPosY, gotFocus);

          RenderPinIcon(timePassed, i, pinX, dwPosY, gotFocus);

          dwPosY += _itemHeight + _spaceBetweenItems;
        } //if (i + _offset < _listItems.Count)
      } //for (int i = 0; i < _itemsPerPage; i++)

      RenderScrollbar(timePassed, dwPosY);

      if (Focus)
      {
        GUIPropertyManager.SetProperty("#highlightedbutton", string.Empty);
      }
      base.Render(timePassed);
    } //public override void Render()

    protected void RenderScrollbar(float timePassed, int y)
    {
      if (_listItems.Count > _itemsPerPage)
      {
        // Render the spin control
        if (_upDownControlVisible)
        {
          y = y + _itemsPerPage*(_itemHeight + _spaceBetweenItems) - _spaceBetweenItems - 5;
          //_upDownControl.SetPosition(_upDownControl.XPosition,dwPosY+10);
          _upDownControl.Render(timePassed);
        }

        // Render the vertical scrollbar
        if (_verticalScrollbar != null)
        {
          float fPercent = (float) _cursorX + _offset;
          fPercent /= (float) (_listItems.Count);
          fPercent *= 100.0f;
          _verticalScrollbar.Height = _itemsPerPage*(_itemHeight + _spaceBetweenItems);
          _verticalScrollbar.Height -= _spaceBetweenItems;
          if ((int) fPercent != (int) _verticalScrollbar.Percentage)
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
    /// <param name="fPosX">The X position of the text.</param>
    /// <param name="fPosY">The Y position of the text.</param>
    /// <param name="fMaxWidth">The maximum render width.</param>
    /// <param name="dwTextColor">The color of the text.</param>
    /// <param name="labelNumber">The number of the label (1 or 3)</param>
    /// <param name="strTextToRender">The actual text.</param>
    /// <param name="bScroll">A bool indication if there is scrolling or not.</param>
    protected void RenderText(float timePassed, int Item, float fPosX, float fPosY, float fMaxWidth, long dwTextColor,
                              string strTextToRender, bool bScroll)
    {
      // TODO Unify render text methods into one general rendertext method.
      if (_labelControls1 == null)
      {
        return;
      }
      if (Item < 0 || Item >= _labelControls1.Count)
      {
        return;
      }

      GUILabelControl label = _labelControls1[Item];

      if (label == null)
      {
        return;
      }
      float textWidth = 0;
      float textHeight = 0;
      _font.GetTextExtent(label.Label, ref textWidth, ref textHeight);

      if (_textAlignment == Alignment.ALIGN_RIGHT && textWidth < fMaxWidth)
      {
        label.SetPosition((int) (fPosX + fMaxWidth), (int) fPosY);
      }
      else
      {
        label.SetPosition((int) fPosX, (int) fPosY);
      }

      label.TextColor = dwTextColor;
      label.Label = strTextToRender;
      label.Width = (int) fMaxWidth;
      if (textWidth < fMaxWidth)
      {
        label.TextAlignment = _textAlignment;
      }
      else
      {
        label.TextAlignment = Alignment.ALIGN_LEFT;
      }
      label.FontName = _fontName;
      RenderText(timePassed, Item, label, bScroll);
    }

    /// <summary>
    /// Renders the text.
    /// </summary>
    /// <param name="label">The label to render</param>
    /// <param name="bScroll">A bool indication if there is scrolling or not.</param>
    protected void RenderText(float timePassed, int Item, GUILabelControl label, bool bScroll)
    {
      float fPosX = label._positionX;
      float fPosY = label._positionY;
      float fMaxWidth = label.Width;
      long dwTextColor = label.TextColor;
      string strTextToRender = label.Label;
      GUIFont font = GUIFontManager.GetFont(label.FontName);

      if (!bScroll ||
          (label.TextWidth <= fMaxWidth))
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
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);

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
        fPosCY = (float) GUIGraphicsContext.Height;
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

      Viewport newviewport, oldviewport;
      newviewport = new Viewport();
      oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = (int) fPosCX;
      newviewport.Y = (int) fPosCY;
      newviewport.Width = (int) (fwidth);
      newviewport.Height = (int) (fHeight);
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;
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
          _currentFrame = 0;
          _timeElapsed = 0.0f;
          _scrollContinuosly = false;
        }
        if ((_currentFrame > 25 + 12) || _scrollContinuosly)
        {
          if (_scrollContinuosly)
          {
            _scrollPosititionX = _currentFrame;
          }
          else
          {
            _scrollPosititionX = _currentFrame - (25 + 12);
          }

          char wTmp;
          if (_scrollPosition >= _brackedText.Length)
          {
            wTmp = ' ';
          }
          else
          {
            wTmp = _brackedText[_scrollPosition];
          }

          font.GetTextExtent(wTmp.ToString(), ref fWidth, ref fHeight);
          if (_scrollPosititionX - _scrollOffsetX >= fWidth)
          {
            ++_scrollPosition;
            if (_scrollPosition > _brackedText.Length)
            {
              _scrollPosition = 0;
              _scrollPosititionX = 0;
              _scrollOffsetX = 0.0f;
              _currentFrame = 0;
              _timeElapsed = 0.0f;
              _scrollContinuosly = true;
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
            font.DrawText((int) (fPosX - _scrollPosititionX + _scrollOffsetX),
                          fPosY, dwTextColor, _textLine2, Alignment.ALIGN_LEFT,
                          (int) (fMaxWidth - 50f + _scrollPosititionX - _scrollOffsetX));
          }
        }
        else if (fPosY >= 0.0)
        {
          font.DrawText(fPosX, fPosY, dwTextColor, strTextToRender, Alignment.ALIGN_LEFT, (int) (fMaxWidth - 50f));
        }
      }
      GUIGraphicsContext.DX9Device.Viewport = oldviewport;
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
          {
            _searchString = "";
            _offset = 0;
            _cursorX = 0;
            _upDownControl.Value = 1;
            OnSelectionChanged();

            _refresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
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
                  action.m_key.KeyChar == '§')
              {
                Press((char) action.m_key.KeyChar);
                return;
              }

              if (action.m_key.KeyChar == (int) Keys.Back)
              {
                if (_searchString.Length > 0)
                {
                  _searchString = _searchString.Remove(_searchString.Length - 1, 1);
                }
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
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int id;
            bool focus;
            if (_verticalScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
            {
              _drawFocus = false;
              _verticalScrollbar.OnAction(action);
              float fPercentage = _verticalScrollbar.Percentage;
              fPercentage /= 100.0f;
              fPercentage *= (float) _listItems.Count;
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
          }
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            OnMouseClick(action);
          }
          break;
        default:
          {
            OnDefaultAction(action);
          }
          break;
      }
    }

    protected virtual void OnMouseClick(Action action)
    {
      int id;
      bool focus;
      if (_verticalScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
      {
        _drawFocus = false;
        _verticalScrollbar.OnAction(action);
        float fPercentage = _verticalScrollbar.Percentage;
        fPercentage /= 100.0f;
        fPercentage *= (float) _listItems.Count;
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
          OnSelectionChanged();
        }
        return;
      }
      else
      {
        int idtmp;
        bool bUpDown = _upDownControl.InControl((int) action.fAmount1, (int) action.fAmount2, out idtmp);
        _drawFocus = true;
        if (!bUpDown && _listType == ListType.CONTROL_LIST)
        {
          _searchString = "";
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                          (int) Action.ActionType.ACTION_SELECT_ITEM, 0, null);
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
        if ((WindowId != (int) GUIWindow.Window.WINDOW_DIALOG_MENU) ||
            (action.wID == Action.ActionType.ACTION_SELECT_ITEM))
        {
          _searchString = "";
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                          (int) action.wID, 0, null);
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
        if (message.SenderControlId == 0)
        {
          if (message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
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
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
            message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
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
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in _listItems)
          {
            item.FreeMemory();
          }
          _refresh = true;
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem pItem = (GUIListItem) message.Object;
          if (pItem != null)
          {
            Add(pItem);
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
          foreach (GUIListItem item in _listItems)
          {
            if (item.Path.Equals(message.Label, StringComparison.OrdinalIgnoreCase))
            {
              item.Selected = true;
              break;
            }
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
              item.Label2 = MediaPortal.Util.Utils.GetSize(message.Param1);
              if (item.FileInfo != null)
              {
                double length = (double) item.FileInfo.Length;
                if (length == 0)
                {
                  item.Label2 = "100%";
                }
                else
                {
                  double percent = ((double) message.Param1)/length;
                  percent *= 100.0f;
                  item.Label2 = String.Format("{0:n}%", percent);
                }
              }
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
              if (item.FileInfo != null)
              {
                item.Label2 = MediaPortal.Util.Utils.GetSize(item.FileInfo.Length);
              }
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

    /// <summary>
    /// Select the item and set the Page accordengly 
    /// </summary>
    /// <param name="SearchKey">SearchKey</param>
    private void SelectItem(int item)
    {
      int itemCount = _listItems.Count;
      if (item >= 0 && item < itemCount)
      {
        if (item >= itemCount - (itemCount%_itemsPerPage) && itemCount > _itemsPerPage)
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
        _upDownControl.Value = ((_offset + _cursorX)/_itemsPerPage) + 1;
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
      int iCurrentItem = _cursorX + _offset;
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
        if (_searchString.Length < 4)
        {
          // Short search string
          if (pItem.Label.ToUpper().StartsWith(SearchKey.ToUpper()) == true)
          {
            bItemFound = true;
            break;
          }
        }
        else
        {
          // Long search string
          if (pItem.Label.ToUpper().IndexOf(SearchKey.ToUpper()) >= 0)
          {
            bItemFound = true;
            break;
          }
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
      // Check key timeout
      CheckTimer();

      // Check different key pressed
      if ((Key != _previousKey) && (Key >= '1' && Key <= '9'))
      {
        _currentKey = (char) 0;
      }

      if (Key == '*' || Key == '(')
      {
        // Backspace
        if (_searchString.Length > 0)
        {
          _searchString = _searchString.Remove(_searchString.Length - 1, 1);
        }
        _previousKey = (char) 0;
        _currentKey = (char) 0;
        _timerKey = DateTime.Now;
      }
      else if (Key == '#' || Key == '§')
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
        _previousKey = (char) 0;
        _currentKey = (char) 0;
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
                                                     _itemHeight, _buttonFocusName, _buttonNonFocusName);
        cntl.ParentControl = this;
        cntl.AllocResources();
        cntl.DimColor = DimColor;
        _listButtons.Add(cntl);
      }
    }

    protected virtual void ReleaseButtons()
    {
      if (_listButtons != null)
      {
        for (int i = 0; i < _listButtons.Count; ++i)
        {
          _listButtons[i].FreeResources();
        }
      }
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _upDownControl.AllocResources();
      _verticalScrollbar.AllocResources();
      _font = GUIFontManager.GetFont(_fontName);
      _font2 = GUIFontManager.GetFont(_fontName2Name);


      float fHeight = (float) _itemHeight + (float) _spaceBetweenItems;
      float fTotalHeight = (float) (_height - _upDownControl.Height - 5);
      _itemsPerPage = (int) (fTotalHeight/fHeight);

      _listButtons = new List<GUIControl>();
      _labelControls1 = new List<GUILabelControl>();
      _labelControls2 = new List<GUILabelControl>();
      _labelControls3 = new List<GUILabelControl>();
      AllocButtons();
      for (int i = 0; i < _itemsPerPage; ++i)
      {
        GUILabelControl cntl1 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName, "", _textColor,
                                                    Alignment.ALIGN_LEFT, false);
        GUILabelControl cntl2 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName2Name, "", _textColor2,
                                                    Alignment.ALIGN_LEFT, false);
        GUILabelControl cntl3 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName2Name, "", _textColor3,
                                                    Alignment.ALIGN_RIGHT, false);
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
        iPages = _listItems.Count/_itemsPerPage;
        if ((_listItems.Count%_itemsPerPage) != 0)
        {
          iPages++;
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
    public override void FreeResources()
    {
      foreach (GUIListItem item in _listItems)
      {
        item.FreeIcons();
      }
      _listItems.Clear();
      base.FreeResources();
      _upDownControl.FreeResources();
      ReleaseButtons();
      if (_labelControls1 != null)
      {
        for (int i = 0; i < _labelControls1.Count; ++i)
        {
          _labelControls1[i].FreeResources();
        }
      }
      if (_labelControls2 != null)
      {
        for (int i = 0; i < _labelControls2.Count; ++i)
        {
          _labelControls2[i].FreeResources();
        }
      }
      if (_labelControls3 != null)
      {
        for (int i = 0; i < _labelControls3.Count; ++i)
        {
          _labelControls3[i].FreeResources();
        }
      }
      _listButtons = null;
      _labelControls1 = null;
      _labelControls2 = null;
      _labelControls3 = null;
      _verticalScrollbar.FreeResources();
    }

    /// <summary>
    /// Implementation of the OnRight action.
    /// </summary>
    protected virtual void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
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
          this.Focus = true;
        }
        else
        {
          _upDownControl.OnAction(action);
        }
        if (!_upDownControl.Focus)
        {
          if (base._rightControlId != GetID)
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
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
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
          this.Focus = true;
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
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (_listType == ListType.CONTROL_LIST)
      {
        if ((_cursorX > _scrollStartOffset) || ((_cursorX > 0) && (_offset == 0)))
        {
          _cursorX--;
          _upDownControl.Value = ((_offset + _cursorX)/_itemsPerPage) + 1;
          OnSelectionChanged();
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
        }
        else
        {
          if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
          {
            //check if _downControlId is set -> then go to the window
            if (_upControlId > 0)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _upControlId,
                                              (int) action.wID, 0, null);
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
      }
      _lastCommandTime = AnimationTimer.TickCount;
    }

    /// <summary>
    /// Implementation of the OnDown action.
    /// </summary>
    protected virtual void OnDown()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (_listType == ListType.CONTROL_LIST)
      {
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
            _upDownControl.Value = ((_offset + _cursorX)/_itemsPerPage) + 1;
            OnSelectionChanged();
          }
          else
          {
            if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
            {
              //check if _downControlId is set -> then go to the window
              if (_downControlId > 0)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _downControlId,
                                                (int) action.wID, 0, null);
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
          }
          else
          {
            if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
            {
              //check if _downControlId is set -> then go to the window
              if (_downControlId > 0)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _downControlId,
                                                (int) action.wID, 0, null);
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
      }
      _lastCommandTime = AnimationTimer.TickCount;
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
        int iPages = _listItems.Count/_itemsPerPage;
        int itemsOnLastPage = _listItems.Count%_itemsPerPage;
        if (itemsOnLastPage != 0)
        {
          iPages++;
        }

        iPage--;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1)*_itemsPerPage;
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
      int iPages = _listItems.Count/_itemsPerPage;
      int itemsOnLastPage = _listItems.Count%_itemsPerPage;
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
          _offset = (_upDownControl.Value - 1)*_itemsPerPage;
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
    /// <param name="iXoffset2">The X offset of the third label.</param>
    /// <param name="iYOffset2">The Y offset of the third label.</param>
    public void SetTextOffsets(int iXoffset, int iYOffset, int iXoffset2, int iYOffset2, int iXoffset3, int iYOffset3)
    {
      if (iXoffset < 0 || iYOffset < 0)
      {
        return;
      }
      if (iXoffset2 < 0 || iYOffset2 < 0)
      {
        return;
      }
      if (iXoffset3 < 0 || iYOffset3 < 0)
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
      if (iWidth < 0)
      {
        return;
      }
      if (iHeight < 0)
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

    /// <summary>
    /// Get the selected item
    /// </summary>
    /// <param name="strLabel"></param>
    /// <returns></returns>
    public int GetSelectedItem(ref string strLabel, ref string strLabel2, ref string strThumb)
    {
      strLabel = "";
      strLabel2 = "";
      strThumb = "";
      int iItem = _cursorX + _offset;
      if (iItem >= 0 && iItem < _listItems.Count)
      {
        GUIListItem pItem = _listItems[iItem];
        strLabel = pItem.Label;
        strLabel2 = pItem.Label2;
        strThumb = pItem.ThumbnailImage;
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
      get { return _upDownControl.Width/2; }
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
      _cursorX = (y/(_itemHeight + _spaceBetweenItems));
      while (_offset + _cursorX >= _listItems.Count)
      {
        _cursorX--;
      }
      if (_cursorX >= _itemsPerPage)
      {
        _cursorX = _itemsPerPage - 1;
      }
      _upDownControl.Value = ((_offset + _cursorX)/_itemsPerPage) + 1;
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
      }
      catch (Exception)
      {
      }
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
    /// Get/set if the aspectration of the images of the items needs to be kept.
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
      int iPages = _listItems.Count/_itemsPerPage;
      if ((_listItems.Count%_itemsPerPage) != 0)
      {
        iPages++;
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

    public bool WordWrap
    {
      get { return _wordWrapping; }
      set { _wordWrapping = value; }
    }

    private void WordWrapText(string strText, int iMaxWidth, out ArrayList wrappedLines)
    {
      wrappedLines = new ArrayList();
      GUILabelControl cntl1 = new GUILabelControl(_controlId, 0, 0, 0, GUIGraphicsContext.Width,
                                                  GUIGraphicsContext.Height, _fontName, "", _textColor,
                                                  Alignment.ALIGN_LEFT, false);
      cntl1.ParentControl = this;
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
      cntl1.FreeResources();
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
      _listItems.Clear();
      //GUITextureManager.CleanupThumbs();
      _upDownControl.SetRange(1, 1);
      _upDownControl.Value = 1;
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
          return new Rectangle(this.XPosition, this.YPosition, this.Width, this.Height);
        }
        //return System.Drawing.Rectangle.Empty;

        GUIControl btn = _listButtons[_cursorX];

        return new Rectangle(btn.XPosition, btn.YPosition, btn.Width, btn.Height);
      }
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
  }
}