#region Copyright (C) 2005 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms; // used for Keys definition
using Microsoft.DirectX.Direct3D;
using MediaPortal.Util;

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

    [XMLSkinElement("spaceBetweenItems")]
    protected int m_iSpaceBetweenItems = 2;
    protected int m_iOffset = 0;
    protected int m_iItemsPerPage = 10;
    protected int m_iLastItemPageValues = 0;

    [XMLSkinElement("textureHeight")]
    protected int m_iItemHeight = 10;
    protected ListType m_iSelect = ListType.CONTROL_LIST;
    protected int m_iCursorY = 0;
    [XMLSkinElement("textXOff")]
    protected int _textOffsetX;
    [XMLSkinElement("textYOff")]
    protected int _textOffsetY;
    [XMLSkinElement("textXOff2")]
    protected int _textOffsetX2;
    [XMLSkinElement("textYOff2")]
    protected int _textOffsetY2;
    [XMLSkinElement("textXOff3")]
    protected int _textOffsetX3;
    [XMLSkinElement("textYOff3")]
    protected int _textOffsetY3;

    [XMLSkinElement("itemWidth")]
    protected int m_iImageWidth = 16;
    [XMLSkinElement("itemHeight")]
    protected int m_iImageHeight = 16;
    protected bool m_bUpDownVisible = true;

    protected GUIFont _font = null;
    protected GUIFont _font2 = null;
    protected GUISpinControl m_upDown = null;
    protected List<GUIControl> _listButtons = null;
    protected GUIverticalScrollbar m_vertScrollbar = null;

    protected List<GUIListItem> m__itemList = new List<GUIListItem>();
    protected List<GUILabelControl> _labelControls1 = new List<GUILabelControl>();
    protected List<GUILabelControl> _labelControls2 = new List<GUILabelControl>();
    protected List<GUILabelControl> _labelControls3 = new List<GUILabelControl>();

    [XMLSkinElement("remoteColor")]
    protected long _remoteColor = 0xffff0000;
    [XMLSkinElement("downloadColor")]
    protected long _downloadColor = 0xff00ff00;
    [XMLSkinElement("shadedColor")]
    protected long m_dwShadedColor = 0x20ffffff;
    [XMLSkinElement("textvisible1")]
    protected bool m_bTextVisible1 = true;
    [XMLSkinElement("textvisible2")]
    protected bool m_bTextVisible2 = true;
    [XMLSkinElement("textvisible3")]
    protected bool m_bTextVisible3 = true;
    [XMLSkinElement("PinIconXOff")]
    protected int m_iPinIconOffsetX = 100;
    [XMLSkinElement("PinIconYOff")]
    protected int m_iPinIconOffsetY = 10;
    protected int m_iPinIconWidth = 0;
    protected int m_iPinIconHeight = 0;
    protected bool m_bRefresh = false;
    protected string m_wszText;
    protected string m_wszText2;
    protected string m_strBrackedText;
    [XMLSkinElement("IconXOff")]
    protected int _iconOffsetX = 8;
    [XMLSkinElement("IconYOff")]
    protected int _iconOffsetY = 5;

    protected int _scrollPosition = 0;
    protected int _scrollPosititionX = 0;
    protected int iLastItem = -1;

    protected double _scrollOffset = 0.0f;
    protected int iCurrentFrame = 0;
    protected double timeElapsed = 0.0f;
    protected bool scrollContinuosly = false;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedHorizontal) * 0.01f); }
    }

    [XMLSkinElement("keepaspectratio")]
    protected bool m_bKeepAspectRatio = false;
    protected bool m_bDrawFocus = true;
    [XMLSkinElement("suffix")]
    protected string m_strSuffix = "|";
    [XMLSkinElement("font")]
    protected string _fontName = "";
    [XMLSkinElement("font2")]
    protected string _fontName2Name = "";
    [XMLSkinElement("textcolor")]
    protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolor2")]
    protected long _textColor2 = 0xFFFFFFFF;
    [XMLSkinElement("textcolor3")]
    protected long _textColor3 = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")]
    protected long m_dwSelectedColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor2")]
    protected long m_dwSelectedColor2 = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor3")]
    protected long m_dwSelectedColor3 = 0xFFFFFFFF;

    [XMLSkinElement("folderPrefix")]
    protected string _folderPrefix = "[";
    [XMLSkinElement("folderSuffix")]
    protected string _folderSuffix = "]";

    [XMLSkinElement("textureUp")]
    protected string _upTextureName = "";
    [XMLSkinElement("textureDown")]
    protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")]
    protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")]
    protected string _downTextureNameFocus = "";
    [XMLSkinElement("textureNoFocus")]
    protected string m_strButtonUnfocused = "";
    [XMLSkinElement("textureFocus")]
    protected string m_strButtonFocused = "";

    [XMLSkinElement("scrollbarbg")]
    protected string m_strScrollBarBG = "";
    [XMLSkinElement("scrollbartop")]
    protected string m_strScrollBarTop = "";
    [XMLSkinElement("scrollbarbottom")]
    protected string m_strScrollBarBottom = "";

    [XMLSkinElement("spinColor")]
    protected long _colorSpinColor;
    [XMLSkinElement("spinHeight")]
    protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")]
    protected int _spinControlWidth;
    [XMLSkinElement("spinPosX")]
    protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")]
    protected int _spinControlPositionY;

    [XMLSkinElement("unfocusedAlpha")]
    protected int _unfocusedAlpha = 0xFF;

    bool wordWrap = false;

    // Search            
    DateTime m_keyTimer = DateTime.Now;
    char m_CurrentKey = (char)0;
    char m_PrevKey = (char)0;
    protected string m_strSearchString = "";
    protected int m_iLastSearchItem = 0;

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
      _colorSpinColor = dwSpinColor;
      _spinControlPositionX = dwSpinX;
      _spinControlPositionY = dwSpinY;
      _fontName = strFont;
      m_dwSelectedColor = dwSelectedColor;
      m_dwSelectedColor2 = dwSelectedColor;
      m_dwSelectedColor3 = dwSelectedColor;
      _textColor = dwTextColor;
      _textColor2 = dwTextColor;
      _textColor3 = dwTextColor;
      m_strButtonUnfocused = strButton;
      m_strButtonFocused = strButtonFocus;
      m_strScrollBarBG = strScrollbarBackground;
      m_strScrollBarTop = strScrollbarTop;
      m_strScrollBarBottom = strScrollbarBottom;

      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _font = GUIFontManager.GetFont(_fontName);
      if (_fontName2Name == String.Empty) _fontName2Name = _fontName;
      Font2 = _fontName2Name;

      m_upDown = new GUISpinControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _spinControlWidth, _spinControlHeight, _upTextureName, _downTextureName, _upTextureNameFocus, _downTextureNameFocus, _fontName, _colorSpinColor, GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, GUIControl.Alignment.ALIGN_LEFT);
      m_upDown.ParentControl = this;

      m_vertScrollbar = new GUIverticalScrollbar(_controlId, 0, 5 + _positionX + _width, _positionY, 15, _height, m_strScrollBarBG, m_strScrollBarTop, m_strScrollBarBottom);
      m_vertScrollbar.ParentControl = this;
      m_vertScrollbar.SendNotifies = false;
      m_upDown.WindowId = WindowId;
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _spinControlPositionX, ref _spinControlPositionY, ref _spinControlWidth, ref _spinControlHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX2, ref _textOffsetY2);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX3, ref _textOffsetY3);
      GUIGraphicsContext.ScaleVertical(ref m_iSpaceBetweenItems);
      GUIGraphicsContext.ScaleVertical(ref m_iItemHeight);
      GUIGraphicsContext.ScaleHorizontal(ref _iconOffsetX);
      GUIGraphicsContext.ScaleVertical(ref _iconOffsetY);
      GUIGraphicsContext.ScaleHorizontal(ref m_iPinIconOffsetX);
      GUIGraphicsContext.ScaleVertical(ref m_iPinIconOffsetY);
      GUIGraphicsContext.ScalePosToScreenResolution(ref m_iImageWidth, ref m_iImageHeight);
    }


    protected void OnSelectionChanged()
    {
      if (!IsVisible) return;

      // Reset searchstring
      if (m_iLastSearchItem != (m_iCursorY + m_iOffset))
      {
        m_PrevKey = (char)0;
        m_CurrentKey = (char)0;
        m_strSearchString = "";
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
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED, WindowId, GetID, ParentID, 0, 0, null);
      msg.SendToTargetWindow = true;
      GUIGraphicsContext.SendMessage(msg);

      if (item >= 0 && item < m__itemList.Count)
      {
        GUIListItem listitem = m__itemList[item];
        if (listitem != null) listitem.ItemSelected(this);
      }
      // ToDo: add searchstring property
      if (m_strSearchString.Length > 0)
        GUIPropertyManager.SetProperty("#selecteditem", "{" + m_strSearchString.ToLower() + "}");
    }

    protected virtual void FreeUnusedThumbnails()
    {
      if (m_iLastItemPageValues != m_iOffset + m_iItemsPerPage)
      {
        m_iLastItemPageValues = m_iOffset + m_iItemsPerPage;
        for (int i = 0; i < m_iOffset; ++i)
        {
          GUIListItem pItem = m__itemList[i];
          if (null != pItem)
          {
            bool dispose = true;
            pItem.RetrieveArt = false;
            for (int x = m_iOffset; x < m_iOffset + m_iItemsPerPage && x < m__itemList.Count; x++)
            {
              GUIListItem pItem2 = m__itemList[x];
              pItem2.RetrieveArt = false;
              if ((pItem.IconImage != "" && pItem.IconImage == pItem2.IconImage) ||
                (pItem.IconImageBig != "" && pItem.IconImageBig == pItem2.IconImageBig))
              {
                dispose = false;
                break;
              }
              pItem2.RetrieveArt = true;
            }
            pItem.RetrieveArt = true;
            if (dispose)
              pItem.FreeMemory();
          }
        }
        for (int i = m_iOffset + m_iItemsPerPage + 1; i < m__itemList.Count; ++i)
        {
          GUIListItem pItem = m__itemList[i];
          if (null != pItem)
          {
            pItem.RetrieveArt = false;
            bool dispose = true;
            for (int x = m_iOffset; x < m_iOffset + m_iItemsPerPage && x < m__itemList.Count; x++)
            {
              GUIListItem pItem2 = m__itemList[x];
              pItem2.RetrieveArt = false;
              if ((pItem.IconImageBig != "" && pItem.IconImage == pItem2.IconImageBig) ||
                (pItem.IconImageBig != "" && pItem.IconImageBig == pItem2.IconImageBig))
              {
                dispose = false;
                break;
              }
              pItem2.RetrieveArt = true;
            }
            if (dispose)
              pItem.FreeMemory();
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
            if (gotFocus)
              btn.ColourDiffuse = 0xffffffff;
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
      GUIListItem pItem = m__itemList[buttonNr + m_iOffset];

      if (pItem.HasIcon)
      {
        // show icon
        GUIImage pImage = pItem.Icon;
        if (null == pImage)
        {
          pImage = new GUIImage(0, 0, 0, 0, m_iImageWidth, m_iImageHeight, pItem.IconImage, 0x0);
          pImage.ParentControl = this;
          pImage.KeepAspectRatio = m_bKeepAspectRatio;
          pImage.AllocResources();
          pItem.Icon = pImage;
        }
        if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
        {
          pImage.FreeResources();
          pImage.AllocResources();
        }
        pImage.KeepAspectRatio = m_bKeepAspectRatio;
        pImage.Width = m_iImageWidth;
        pImage.Height = m_iImageHeight;
        pImage.SetPosition(x, y);
        if (gotFocus)
          pImage.ColourDiffuse = 0xffffffff;
        else
          pImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();

        pImage.Render(timePassed);
        pImage = null;
      }
      pItem = null;
    }

    protected virtual void RenderPinIcon(float timePassed, int buttonNr, int x, int y, bool gotFocus)
    {
      GUIListItem pItem = m__itemList[buttonNr + m_iOffset];
      if (pItem.HasPinIcon)
      {
        GUIImage pinImage = pItem.PinIcon;
        if (null == pinImage)
        {
          pinImage = new GUIImage(0, 0, 0, 0, 0, 0, pItem.PinImage, 0x0);
          pinImage.ParentControl = this;
          pinImage.KeepAspectRatio = m_bKeepAspectRatio;
          pinImage.AllocResources();
          pItem.PinIcon = pinImage;
        }
        pinImage.KeepAspectRatio = m_bKeepAspectRatio;
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
        if (gotFocus)
          pinImage.ColourDiffuse = 0xffffffff;
        else
          pinImage.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
        pinImage.Render(timePassed);
        pinImage = null;
      } //if (pItem.HasPinIcon)
      pItem = null;
    }

    protected virtual void RenderLabel(float timePassed, int buttonNr, int dwPosX, int dwPosY, bool gotFocus)
    {
      GUIListItem pItem = m__itemList[buttonNr + m_iOffset];
      long dwColor = _textColor;
      if (pItem.Shaded)
      {
        dwColor = ShadedColor;
      }
      if (pItem.Selected)
      {
        dwColor = m_dwSelectedColor;
      }

      dwPosX += _textOffsetX;
      bool bSelected = false;
      if (buttonNr == m_iCursorY && IsFocused && m_iSelect == ListType.CONTROL_LIST)
      {
        bSelected = true;
      }


      int dMaxWidth = (_width - m_iImageWidth - 16);
      if (m_bTextVisible2 && pItem.Label2.Length > 0)
      {
        if (_textOffsetY == _textOffsetY2)
        {
          dwColor = _textColor2;
          if (pItem.Selected)
          {
            dwColor = m_dwSelectedColor2;
          }
          if (pItem.IsRemote)
          {
            dwColor = _remoteColor;
            if (pItem.IsDownloading) dwColor = _downloadColor;
          }
          int xpos = dwPosX;
          int ypos = dwPosY;
          if (0 == _textOffsetX2)
            xpos = _positionX + _width - 16;
          else
            xpos = _positionX + _textOffsetX2;

          if (_labelControls2 != null)
          {
            if (buttonNr >= 0 && buttonNr < _labelControls2.Count)
            {
              GUILabelControl label2 = _labelControls2[buttonNr];
              if (label2 != null)
              {
                label2.SetPosition(xpos, ypos + 2 + _textOffsetY2);
                if (gotFocus)
                  label2.TextColor = dwColor;
                else
                  label2.TextColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
                label2.Label = pItem.Label2;
                label2.TextAlignment = GUIControl.Alignment.ALIGN_RIGHT;
                label2.FontName = _fontName2Name;
                dMaxWidth -= label2.TextWidth + 20;
              }
            }
          }
        }
      }

      m_wszText = pItem.Label;
      if (m_bTextVisible1)
      {
        dwColor = _textColor;
        if (pItem.Selected)
        {
          dwColor = m_dwSelectedColor;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading) dwColor = _downloadColor;
        }
        if (!gotFocus)
          dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
        RenderText(timePassed, buttonNr, (float)dwPosX, (float)dwPosY + 2 + _textOffsetY, (float)dMaxWidth, dwColor, m_wszText, bSelected);
      } //if (m_bTextVisible1)

      if (pItem.Label2.Length > 0)
      {
        dwColor = _textColor2;
        if (pItem.Selected)
        {
          dwColor = m_dwSelectedColor2;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading) dwColor = _downloadColor;
        }

        if (0 == _textOffsetX2)
          dwPosX = _positionX + _width - 16;
        else
          dwPosX = _positionX + _textOffsetX2;

        m_wszText = pItem.Label2;
        if (m_bTextVisible2)
        {
          if (_labelControls2 != null)
          {
            if (buttonNr >= 0 && buttonNr < _labelControls2.Count)
            {
              GUILabelControl label2 = _labelControls2[buttonNr];
              if (label2 != null)
              {
                label2.SetPosition(dwPosX, dwPosY + 2 + _textOffsetY2);
                if (gotFocus)
                  label2.TextColor = dwColor;
                else
                  label2.TextColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
                label2.Label = m_wszText;
                label2.TextAlignment = GUIControl.Alignment.ALIGN_RIGHT;
                label2.FontName = _fontName2Name;
                label2.Render(timePassed);
                label2 = null;
                //_font.DrawText((float)dwPosX, (float)dwPosY + 2 + _textOffsetY2, dwColor, m_wszText, GUIControl.Alignment.ALIGN_RIGHT);
              } //if (label2!=null)
            } //if (i>=0 && i < _labelControls2.Count)
          } //if (_labelControls2!=null)
        } //if (m_bTextVisible2)
      } //if (pItem.Label2.Length > 0)	
      if (pItem.Label3.Length > 0)
      {
        dwColor = _textColor3;
        if (pItem.Selected)
        {
          dwColor = m_dwSelectedColor3;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading) dwColor = _downloadColor;
        }
        if (0 == _textOffsetX3)
          dwPosX = _positionX + _textOffsetX;
        else
          dwPosX = _positionX + _textOffsetX3;

        int ypos = dwPosY;
        if (0 == _textOffsetY3)
          ypos += _textOffsetY2;
        else
          ypos += _textOffsetY3;
        if (m_bTextVisible3)
        {
          if (_labelControls3 != null)
          {
            if (buttonNr >= 0 && buttonNr < _labelControls3.Count)
            {
              GUILabelControl label3 = _labelControls3[buttonNr];
              if (label3 != null)
              {
                label3.SetPosition(dwPosX, ypos);
                if (gotFocus)
                  label3.TextColor = dwColor;
                else
                  label3.TextColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
                label3.Label = pItem.Label3;
                label3.TextAlignment = GUIControl.Alignment.ALIGN_LEFT;
                label3.FontName = _fontName2Name;
                label3.Render(timePassed);
                label3 = null;
                //_font.DrawText((float)dwPosX, (float)ypos, dwColor, pItem.Label3, GUIControl.Alignment.ALIGN_LEFT);
              } //if (label3!=null)
            } //if (i>=0 && i < _labelControls3.Count)
          } //if (_labelControls3!=null)
        } //if (m_bTextVisible3)
      } //if (pItem.Label3.Length > 0)
      pItem = null;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      timeElapsed += timePassed;
      iCurrentFrame = (int)(timeElapsed / TimeSlice);

      // If there is no font do not render.
      if (null == _font) return;
      // If the control is not visible do not render.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }

      int dwPosY = _positionY;

      // Render the buttons first.
      for (int i = 0; i < m_iItemsPerPage; i++)
      {
        if (i + m_iOffset < m__itemList.Count)
        {
          // render item
          bool gotFocus = false;
          if (m_bDrawFocus && i == m_iCursorY && IsFocused && m_iSelect == ListType.CONTROL_LIST)
            gotFocus = true;
          RenderButton(timePassed, i, _positionX, dwPosY, gotFocus);
        }
        dwPosY += m_iItemHeight + m_iSpaceBetweenItems;
      }

      // Free unused textures if page has changed
      FreeUnusedThumbnails();

      // Render new item list
      dwPosY = _positionY;
      for (int i = 0; i < m_iItemsPerPage; i++)
      {
        int dwPosX = _positionX;
        if (i + m_iOffset < m__itemList.Count)
        {
          bool gotFocus = false;
          if (m_bDrawFocus && i == m_iCursorY && IsFocused && m_iSelect == ListType.CONTROL_LIST)
            gotFocus = true;
          // render the icon
          RenderIcon(timePassed, i, dwPosX + _iconOffsetX, dwPosY + _iconOffsetY, gotFocus);

          dwPosX += (m_iImageWidth + 10);

          // render the text
          RenderLabel(timePassed, i, dwPosX, dwPosY, gotFocus);

          RenderPinIcon(timePassed, i, _positionX, dwPosY, gotFocus);

          dwPosY += m_iItemHeight + m_iSpaceBetweenItems;
        } //if (i + m_iOffset < m__itemList.Count)
      } //for (int i = 0; i < m_iItemsPerPage; i++)

      RenderScrollbar(timePassed, dwPosY);

      if (Focus)
        GUIPropertyManager.SetProperty("#highlightedbutton", String.Empty);
    } //public override void Render()

    protected void RenderScrollbar(float timePassed, int y)
    {
      if (m__itemList.Count > m_iItemsPerPage)
      {
        // Render the spin control
        if (m_bUpDownVisible)
        {
          y = y + m_iItemsPerPage * (m_iItemHeight + m_iSpaceBetweenItems) - m_iSpaceBetweenItems - 5;
          //m_upDown.SetPosition(m_upDown.XPosition,dwPosY+10);
          m_upDown.Render(timePassed);
        }

        // Render the vertical scrollbar
        if (m_vertScrollbar != null)
        {
          float fPercent = (float)m_iCursorY + m_iOffset;
          fPercent /= (float)(m__itemList.Count);
          fPercent *= 100.0f;
          m_vertScrollbar.Height = m_iItemsPerPage * (m_iItemHeight + m_iSpaceBetweenItems);
          m_vertScrollbar.Height -= m_iSpaceBetweenItems;
          if ((int)fPercent != (int)m_vertScrollbar.Percentage)
          {
            m_vertScrollbar.Percentage = fPercent;
          }
          m_vertScrollbar.Render(timePassed);
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
    /// <param name="strTextToRender">The actual text.</param>
    /// <param name="bScroll">A bool indication if there is scrolling or not.</param>
    protected void RenderText(float timePassed, int Item, float fPosX, float fPosY, float fMaxWidth, long dwTextColor, string strTextToRender, bool bScroll)
    {
      // TODO Unify render text methods into one general rendertext method.
      if (_labelControls1 == null) return;
      if (Item < 0 || Item >= _labelControls1.Count) return;

      GUILabelControl label1 = _labelControls1[Item];
      if (label1 == null) return;
      label1.SetPosition((int)fPosX, (int)fPosY);
      label1.TextColor = dwTextColor;
      label1.Label = strTextToRender;
      label1.Width = (int)fMaxWidth;
      label1.TextAlignment = GUIControl.Alignment.ALIGN_LEFT;
      label1.FontName = _fontName;
      if (false == bScroll)
      {
        // don't scroll here => x-position is constant
        label1.Render(timePassed);
        return;
      }


      if (label1.TextWidth <= fMaxWidth)
      {
        label1.Render(timePassed);
        return;
      }


      float fTextHeight = 0, fTextWidth = 0;
      _font.GetTextExtent(strTextToRender, ref fTextWidth, ref fTextHeight);
      float fWidth = 0;

      float fPosCX = fPosX;
      float fPosCY = fPosY;
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;
      if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
      float fHeight = 60.0f;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      if (fHeight <= 0) return;

      float fwidth = fMaxWidth - 5.0f;

      if (fwidth < 1) return;
      if (fHeight < 1) return;

      if (fPosCX <= 0) fPosCX = 0;
      if (fPosCY <= 0) fPosCY = 0;
      Viewport newviewport, oldviewport;
      newviewport = new Viewport();
      oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = (int)fPosCX;
      newviewport.Y = (int)fPosCY;
      newviewport.Width = (int)(fwidth);
      newviewport.Height = (int)(fHeight);
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;
      {
        // scroll
        int iItem = m_iCursorY + m_iOffset;
        m_strBrackedText = strTextToRender;
        m_strBrackedText += (" " + m_strSuffix + " ");
        _font.GetTextExtent(m_strBrackedText, ref fTextWidth, ref fTextHeight);

        if (fTextWidth > fMaxWidth)
        {
          // scrolling necessary
          fMaxWidth += 50.0f;
          m_wszText2 = "";
          if (iLastItem != iItem)
          {
            _scrollPosition = 0;
            iLastItem = iItem;
            _scrollPosititionX = 0;
            _scrollOffset = 0.0f;
            iCurrentFrame = 0;
            timeElapsed = 0.0f;
            scrollContinuosly = false;
          }
          if ((iCurrentFrame > 25 + 12) || scrollContinuosly)
          {
            if (scrollContinuosly)
            {
              _scrollPosititionX = iCurrentFrame;
            }
            else
            {
              _scrollPosititionX = iCurrentFrame - (25 + 12);
            }
            char wTmp;
            if (_scrollPosition >= m_strBrackedText.Length)
              wTmp = ' ';
            else
              wTmp = m_strBrackedText[_scrollPosition];

            _font.GetTextExtent(wTmp.ToString(), ref fWidth, ref fHeight);
            if (_scrollPosititionX - _scrollOffset >= fWidth)
            {
              ++_scrollPosition;
              if (_scrollPosition > m_strBrackedText.Length)
              {
                _scrollPosition = 0;
                _scrollPosititionX = 0;
                _scrollOffset = 0.0f;
                iCurrentFrame = 0;
                timeElapsed = 0.0f;
                scrollContinuosly = true;
              }
              else
              {
                _scrollOffset += fWidth;
              }
            }
            int ipos = 0;
            for (int i = 0; i < m_strBrackedText.Length; i++)
            {
              if (i + _scrollPosition < m_strBrackedText.Length)
                m_wszText2 += m_strBrackedText[i + _scrollPosition];
              else
              {
                if (ipos == 0) m_wszText2 += ' ';
                else m_wszText2 += m_strBrackedText[ipos - 1];
                ipos++;
              }
            }
            if (fPosY >= 0.0)
            {
              //              _font.DrawText((int) (fPosX - _scrollPosititionX + _scrollOffset), fPosY, dwTextColor, m_wszText2, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth - 50f));
              _font.DrawText((int)(fPosX - _scrollPosititionX + _scrollOffset), fPosY, dwTextColor, m_wszText2, GUIControl.Alignment.ALIGN_LEFT, (int)(fMaxWidth - 50f + _scrollPosititionX - _scrollOffset));
            }
          }
          else
          {
            if (fPosY >= 0.0)
              _font.DrawText(fPosX, fPosY, dwTextColor, strTextToRender, GUIControl.Alignment.ALIGN_LEFT, (int)(fMaxWidth - 50f));
          }
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
          m_strSearchString = "";
          OnPageUp();
          m_bRefresh = true;
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          m_bRefresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          {
            m_strSearchString = "";
            m_iOffset = 0;
            m_iCursorY = 0;
            m_upDown.Value = 1;
            OnSelectionChanged();

            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
            m_strSearchString = "";
            int iItem = m__itemList.Count - 1;
            if (iItem >= 0)
            {
              // update spin controls
              int iPage = 1;
              int iSel = iItem;
              while (iSel >= m_iItemsPerPage)
              {
                iPage++;
                iSel -= m_iItemsPerPage;
              }
              m_upDown.Value = iPage;

              // find item
              m_iOffset = 0;
              m_iCursorY = 0;
              while (iItem >= m_iItemsPerPage)
              {
                iItem -= m_iItemsPerPage;
                m_iOffset += m_iItemsPerPage;
              }
              m_iCursorY = iItem;
              OnSelectionChanged();
            }
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            if (m_strSearchString.Trim() != "")
              SearchItem(m_strSearchString, SearchType.SEARCH_NEXT);
            else
              OnDown();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (m_strSearchString.Trim() != "")
              SearchItem(m_strSearchString, SearchType.SEARCH_PREV);
            else
              OnUp();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            m_strSearchString = "";
            OnLeft();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            m_strSearchString = "";
            OnRight();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if (action.m_key != null)
            {
              // Check key
              if (((action.m_key.KeyChar >= 49) && (action.m_key.KeyChar <= 57)) ||
                action.m_key.KeyChar == '*' || action.m_key.KeyChar == '#')
              {
                Press((char)action.m_key.KeyChar);
                return;
              }

              if (action.m_key.KeyChar == (int)Keys.Back)
              {
                if (m_strSearchString.Length > 0)
                  m_strSearchString = m_strSearchString.Remove(m_strSearchString.Length - 1, 1);
                SearchItem(m_strSearchString, SearchType.SEARCH_FIRST);
              }
              if (((action.m_key.KeyChar >= 65) && (action.m_key.KeyChar <= 90)) || (action.m_key.KeyChar == (int)Keys.Space))
              {
                if (action.m_key.KeyChar == (int)Keys.Space && m_strSearchString == String.Empty) return;
                m_strSearchString += (char)action.m_key.KeyChar;
                SearchItem(m_strSearchString, SearchType.SEARCH_FIRST);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int id;
            bool focus;
            if (m_vertScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
            {
              m_bDrawFocus = false;
              m_vertScrollbar.OnAction(action);
              float fPercentage = m_vertScrollbar.Percentage;
              fPercentage /= 100.0f;
              fPercentage *= (float)m__itemList.Count;
              int iChan = (int)fPercentage;
              if (iChan != m_iOffset + m_iCursorY)
              {
                // update spin controls
                int iPage = 1;
                int iSel = iChan;
                while (iSel >= m_iItemsPerPage)
                {
                  iPage++;
                  iSel -= m_iItemsPerPage;
                }
                m_upDown.Value = iPage;

                // find item
                m_iOffset = 0;
                m_iCursorY = 0;
                while (iChan >= m_iItemsPerPage)
                {
                  iChan -= m_iItemsPerPage;
                  m_iOffset += m_iItemsPerPage;
                }
                m_iCursorY = iChan;
                OnSelectionChanged();
              }
              return;
            }
            m_bDrawFocus = true;
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
      if (m_vertScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
      {
        m_bDrawFocus = false;
        m_vertScrollbar.OnAction(action);
        float fPercentage = m_vertScrollbar.Percentage;
        fPercentage /= 100.0f;
        fPercentage *= (float)m__itemList.Count;
        int iChan = (int)fPercentage;
        if (iChan != m_iOffset + m_iCursorY)
        {
          // update spin controls
          int iPage = 1;
          int iSel = iChan;
          while (iSel >= m_iItemsPerPage)
          {
            iPage++;
            iSel -= m_iItemsPerPage;
          }
          m_upDown.Value = iPage;

          // find item
          m_iOffset = 0;
          m_iCursorY = 0;
          while (iChan >= m_iItemsPerPage)
          {
            iChan -= m_iItemsPerPage;
            m_iOffset += m_iItemsPerPage;
          }
          m_iCursorY = iChan;
          OnSelectionChanged();
        }
        return;
      }
      else
      {
        int idtmp;
        bool bUpDown = m_upDown.InControl((int)action.fAmount1, (int)action.fAmount2, out idtmp);
        m_bDrawFocus = true;
        if (!bUpDown && m_iSelect == ListType.CONTROL_LIST)
        {
          m_strSearchString = "";
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int)Action.ActionType.ACTION_SELECT_ITEM, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
        else
        {
          m_upDown.OnAction(action);
        }
        m_bRefresh = true;
      }
    }

    protected virtual void OnDefaultAction(Action action)
    {
      // by default send a message to parent window that user has done an action on the selected item
      // could be, just enter or f3 for info or 0 to delete or y to queue etc...
      if (m_iSelect == ListType.CONTROL_LIST)
      {
        // don't send the messages to a dialog menu
        if ((WindowId != (int)GUIWindow.Window.WINDOW_DIALOG_MENU) || (action.wID == Action.ActionType.ACTION_SELECT_ITEM))
        {
          m_strSearchString = "";
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int)action.wID, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
      }
      else
      {
        m_upDown.OnAction(action);
      }
      m_bRefresh = true;
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
            m_iOffset = (m_upDown.Value - 1) * m_iItemsPerPage;
            while (m_iOffset + m_iCursorY >= m__itemList.Count) m_iCursorY--;
            OnSelectionChanged();
            m_bRefresh = true;
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < m__itemList.Count)
          {
            message.Object = m__itemList[iItem];
          }
          else
          {
            message.Object = null;
          }
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = m_iCursorY + m_iOffset;

          if (iItem >= 0 && iItem < m__itemList.Count)
          {
            message.Object = m__itemList[iItem];
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
          m_iSelect = ListType.CONTROL_LIST;
          m_bRefresh = true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in m__itemList)
          {
            item.FreeMemory();
          }
          m_bRefresh = true;
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem pItem = (GUIListItem)message.Object;
          if (pItem != null) Add(pItem);
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = m__itemList.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          message.Param1 = m_iCursorY + m_iOffset;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          if (message.Param1 >= 0 && message.Param1 < m__itemList.Count)
          {
            int iPage = 1;
            m_iOffset = 0;
            m_iCursorY = message.Param1;
            while (m_iCursorY >= m_iItemsPerPage)
            {
              iPage++;
              m_iOffset += m_iItemsPerPage;
              m_iCursorY -= m_iItemsPerPage;
            }
            m_upDown.Value = iPage;
            OnSelectionChanged();
          }
          m_bRefresh = true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
        {
          foreach (GUIListItem item in m__itemList)
          {
            item.Selected = false;
          }
          if (message.Param1 >= 0 && message.Param1 < m__itemList.Count)
          {
            GUIListItem focusedItem = m__itemList[message.Param1];
            focusedItem.Selected = true;
          }
        }
      }
      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING)
      {
        foreach (GUIListItem item in m__itemList)
        {
          if (item.IsRemote)
          {
            if (message.Label == item.Path)
            {
              item.IsDownloading = true;
              item.Label2 = Utils.GetSize(message.Param1);
              if (item.FileInfo != null)
              {
                double length = (double)item.FileInfo.Length;
                double percent = ((double)message.Param1) / length;
                percent *= 100.0f;
                item.Label2 = String.Format("{0:N}%", percent);
              }
            }
          }
        }
      }
      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED)
      {
        foreach (GUIListItem item in m__itemList)
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

      if (base.OnMessage(message)) return true;

      return false;
    }

    /// <summary>
    /// Search for first item starting with searchkey
    /// </summary>
    /// <param name="SearchKey">SearchKey</param>
    void SearchItem(string SearchKey, SearchType iSearchMethode)
    {
      // Get selected item
      bool bItemFound = false;
      int iCurrentItem = m_iCursorY + m_iOffset;
      if (iSearchMethode == SearchType.SEARCH_FIRST) iCurrentItem = 0;

      int iItem = iCurrentItem;
      do
      {
        if (iSearchMethode == SearchType.SEARCH_NEXT)
        {
          iItem++;
          if (iItem >= m__itemList.Count) iItem = 0;
        }
        if (iSearchMethode == SearchType.SEARCH_PREV && m__itemList.Count > 0)
        {
          iItem--;
          if (iItem < 0) iItem = m__itemList.Count - 1;
        }

        GUIListItem pItem = m__itemList[iItem];
        if (m_strSearchString.Length < 4)
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
          if (iItem >= m__itemList.Count) iItem = 0;
        }
      } while (iItem != iCurrentItem);

      if ((bItemFound) && (iItem >= 0 && iItem < m__itemList.Count))
      {
        // update spin controls
        int iPage = 1;
        int iSel = iItem;
        while (iSel >= m_iItemsPerPage)
        {
          iPage++;
          iSel -= m_iItemsPerPage;
        }
        m_upDown.Value = iPage;

        // find item
        m_iOffset = 0;
        m_iCursorY = 0;
        while (iItem >= m_iItemsPerPage)
        {
          iItem -= m_iItemsPerPage;
          m_iOffset += m_iItemsPerPage;
        }
        m_iCursorY = iItem;
      }

      m_iLastSearchItem = m_iCursorY + m_iOffset;
      OnSelectionChanged();
      m_bRefresh = true;
    }


    /// <summary>
    /// Handle keypress events for SMS style search (key '1'..'9')
    /// </summary>
    /// <param name="Key"></param>
    void Press(char Key)
    {
      // Check key timeout
      CheckTimer();

      // Check different key pressed
      if ((Key != m_PrevKey) && (Key >= '1' && Key <= '9'))
        m_CurrentKey = (char)0;

      if (Key == '*')
      {
        // Backspace
        if (m_strSearchString.Length > 0)
          m_strSearchString = m_strSearchString.Remove(m_strSearchString.Length - 1, 1);
        m_PrevKey = (char)0;
        m_CurrentKey = (char)0;
        m_keyTimer = DateTime.Now;
      }
      else if (Key == '#')
      {
        m_keyTimer = DateTime.Now;
      }
      else if (Key == '1')
      {
        if (m_CurrentKey == 0) m_CurrentKey = ' ';
        else if (m_CurrentKey == ' ') m_CurrentKey = '!';
        else if (m_CurrentKey == '!') m_CurrentKey = '?';
        else if (m_CurrentKey == '?') m_CurrentKey = '.';
        else if (m_CurrentKey == '.') m_CurrentKey = '0';
        else if (m_CurrentKey == '0') m_CurrentKey = '1';
        else if (m_CurrentKey == '1') m_CurrentKey = '-';
        else if (m_CurrentKey == '-') m_CurrentKey = '+';
        else if (m_CurrentKey == '+') m_CurrentKey = ' ';
      }
      else if (Key == '2')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'a';
        else if (m_CurrentKey == 'a') m_CurrentKey = 'b';
        else if (m_CurrentKey == 'b') m_CurrentKey = 'c';
        else if (m_CurrentKey == 'c') m_CurrentKey = '2';
        else if (m_CurrentKey == '2') m_CurrentKey = 'a';
      }
      else if (Key == '3')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'd';
        else if (m_CurrentKey == 'd') m_CurrentKey = 'e';
        else if (m_CurrentKey == 'e') m_CurrentKey = 'f';
        else if (m_CurrentKey == 'f') m_CurrentKey = '3';
        else if (m_CurrentKey == '3') m_CurrentKey = 'd';
      }
      else if (Key == '4')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'g';
        else if (m_CurrentKey == 'g') m_CurrentKey = 'h';
        else if (m_CurrentKey == 'h') m_CurrentKey = 'i';
        else if (m_CurrentKey == 'i') m_CurrentKey = '4';
        else if (m_CurrentKey == '4') m_CurrentKey = 'g';
      }
      else if (Key == '5')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'j';
        else if (m_CurrentKey == 'j') m_CurrentKey = 'k';
        else if (m_CurrentKey == 'k') m_CurrentKey = 'l';
        else if (m_CurrentKey == 'l') m_CurrentKey = '5';
        else if (m_CurrentKey == '5') m_CurrentKey = 'j';
      }
      else if (Key == '6')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'm';
        else if (m_CurrentKey == 'm') m_CurrentKey = 'n';
        else if (m_CurrentKey == 'n') m_CurrentKey = 'o';
        else if (m_CurrentKey == 'o') m_CurrentKey = '6';
        else if (m_CurrentKey == '6') m_CurrentKey = 'm';
      }
      else if (Key == '7')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'p';
        else if (m_CurrentKey == 'p') m_CurrentKey = 'q';
        else if (m_CurrentKey == 'q') m_CurrentKey = 'r';
        else if (m_CurrentKey == 'r') m_CurrentKey = 's';
        else if (m_CurrentKey == 's') m_CurrentKey = '7';
        else if (m_CurrentKey == '7') m_CurrentKey = 'p';
      }
      else if (Key == '8')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 't';
        else if (m_CurrentKey == 't') m_CurrentKey = 'u';
        else if (m_CurrentKey == 'u') m_CurrentKey = 'v';
        else if (m_CurrentKey == 'v') m_CurrentKey = '8';
        else if (m_CurrentKey == '8') m_CurrentKey = 't';
      }
      else if (Key == '9')
      {
        if (m_CurrentKey == 0) m_CurrentKey = 'w';
        else if (m_CurrentKey == 'w') m_CurrentKey = 'x';
        else if (m_CurrentKey == 'x') m_CurrentKey = 'y';
        else if (m_CurrentKey == 'y') m_CurrentKey = 'z';
        else if (m_CurrentKey == 'z') m_CurrentKey = '9';
        else if (m_CurrentKey == '9') m_CurrentKey = 'w';
      }

      if (Key >= '1' && Key <= '9')
      {
        // Check different key pressed
        if (Key == m_PrevKey)
        {
          if (m_strSearchString.Length > 0)
            m_strSearchString = m_strSearchString.Remove(m_strSearchString.Length - 1, 1);
        }
        m_PrevKey = Key;
        m_strSearchString += m_CurrentKey;
      }
      SearchItem(m_strSearchString, SearchType.SEARCH_FIRST);
      m_keyTimer = DateTime.Now;
    }

    void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - m_keyTimer;
      if (ts.TotalMilliseconds >= 800)
      {
        m_PrevKey = (char)0;
        m_CurrentKey = (char)0;
      }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_upDown.PreAllocResources();
      m_vertScrollbar.PreAllocResources();
    }

    protected virtual void AllocButtons()
    {
      for (int i = 0; i < m_iItemsPerPage; ++i)
      {
        GUIButtonControl cntl = new GUIButtonControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _width, m_iItemHeight, m_strButtonFocused, m_strButtonUnfocused);
        cntl.ParentControl = this;
        cntl.AllocResources();
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
      m_upDown.AllocResources();
      m_vertScrollbar.AllocResources();
      _font = GUIFontManager.GetFont(_fontName);
      _font2 = GUIFontManager.GetFont(_fontName2Name);


      float fHeight = (float)m_iItemHeight + (float)m_iSpaceBetweenItems;
      float fTotalHeight = (float)(_height - m_upDown.Height - 5);
      m_iItemsPerPage = (int)(fTotalHeight / fHeight);

      _listButtons = new List<GUIControl>();
      _labelControls1 = new List<GUILabelControl>();
      _labelControls2 = new List<GUILabelControl>();
      _labelControls3 = new List<GUILabelControl>();
      AllocButtons();
      for (int i = 0; i < m_iItemsPerPage; ++i)
      {
        GUILabelControl cntl1 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName, "", _textColor, GUIControl.Alignment.ALIGN_LEFT, false);
        GUILabelControl cntl2 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName2Name, "", _textColor2, GUIControl.Alignment.ALIGN_LEFT, false);
        GUILabelControl cntl3 = new GUILabelControl(_controlId, 0, 0, 0, 0, 0, _fontName2Name, "", _textColor3, GUIControl.Alignment.ALIGN_RIGHT, false);
        cntl1.ParentControl = this;
        cntl2.ParentControl = this;
        cntl3.ParentControl = this;
        cntl1.AllocResources();
        cntl2.AllocResources();
        cntl3.AllocResources();
        _labelControls1.Add(cntl1);
        _labelControls2.Add(cntl2);
        _labelControls3.Add(cntl3);
      }

      int iPages = 1;
      if (m__itemList.Count > 0)
      {
        iPages = m__itemList.Count / m_iItemsPerPage;
        if ((m__itemList.Count % m_iItemsPerPage) != 0) iPages++;
      }
      m_upDown.SetRange(1, iPages);
      m_upDown.Value = 1;
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      foreach (GUIListItem item in m__itemList)
      {
        item.FreeIcons();
      }
      m__itemList.Clear();
      base.FreeResources();
      m_upDown.FreeResources();
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
      m_vertScrollbar.FreeResources();
    }

    /// <summary>
    /// Implementation of the OnRight action.
    /// </summary>
    protected virtual void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      if (m_iSelect == ListType.CONTROL_LIST)
      {
        if (m_upDown.GetMaximum() > 1)
        {
          m_iSelect = ListType.CONTROL_UPDOWN;
          m_upDown.Focus = true;
          if (!m_upDown.Focus)
          {
            m_iSelect = ListType.CONTROL_LIST;
          }
        }
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          if (base._rightControlId != GetID)
          {
            base.OnAction(action);
          }
          m_iSelect = ListType.CONTROL_LIST;
        }
      }
    }

    /// <summary>
    /// Implementation of the OnLeft action.
    /// </summary>
    protected virtual void OnLeft()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
      if (m_iSelect == ListType.CONTROL_LIST)
      {
        base.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = ListType.CONTROL_LIST;
        }
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = ListType.CONTROL_LIST;
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
      if (m_iSelect == ListType.CONTROL_LIST)
      {
        if (m_iCursorY > 0)
        {
          m_iCursorY--;
          OnSelectionChanged();
        }
        else if (m_iCursorY == 0 && m_iOffset != 0)
        {
          m_iOffset--;

          int iPage = 1;
          int iSel = m_iOffset + m_iCursorY;
          while (iSel >= m_iItemsPerPage)
          {
            iPage++;
            iSel -= m_iItemsPerPage;
          }
          m_upDown.Value = iPage;
          OnSelectionChanged();
        }
        else
        {
          // move 2 last item in list
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, m__itemList.Count - 1, 0, null);
          OnMessage(msg);
        }
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = ListType.CONTROL_LIST;
        }
      }
    }

    /// <summary>
    /// Implementation of the OnDown action.
    /// </summary>
    protected virtual void OnDown()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (m_iSelect == ListType.CONTROL_LIST)
      {
        if (m_iCursorY + 1 < m_iItemsPerPage)
        {
          if (m_iOffset + 1 + m_iCursorY < m__itemList.Count)
          {
            m_iCursorY++;
            OnSelectionChanged();
          }
          else
          {
            // move first item in list
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, 0, 0, null);
            OnMessage(msg);
          }
        }
        else
        {
          if (m_iOffset + 1 + m_iCursorY < m__itemList.Count)
          {
            m_iOffset++;

            int iPage = 1;
            int iSel = m_iOffset + m_iCursorY;
            while (iSel >= m_iItemsPerPage)
            {
              iPage++;
              iSel -= m_iItemsPerPage;
            }
            m_upDown.Value = iPage;
            OnSelectionChanged();
          }
          else
          {
            // move first item in list
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, 0, 0, null);
            OnMessage(msg);
          }
        }
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          base.OnAction(action);
        }
      }
    }

    /// <summary>
    /// Get/set the scroll suffic
    /// </summary>
    public String ScrollySuffix
    {
      get { return m_strSuffix; }
      set
      {
        if (value == null) return;
        m_strSuffix = value;
      }
    }

    /// <summary>
    /// Implementation of the OnPageUp action.
    /// </summary>
    protected void OnPageUp()
    {
      int iPage = m_upDown.Value;
      if (iPage > 1)
      {
        iPage--;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1) * m_iItemsPerPage;
      }
      else
      {
        // already on page 1, then select the 1st item
        m_iCursorY = 0;
      }
      OnSelectionChanged();
    }

    /// <summary>
    /// Implementation of the OnPageDown action.
    /// </summary>
    protected void OnPageDown()
    {
      int iPages = m__itemList.Count / m_iItemsPerPage;
      if ((m__itemList.Count % m_iItemsPerPage) != 0) iPages++;

      int iPage = m_upDown.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1) * m_iItemsPerPage;
      }
      else
      {
        // already on last page, move 2 last item in list
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, WindowId, GetID, m__itemList.Count - 1, 0, null);
        OnMessage(msg);
      }
      if (m_iOffset + m_iCursorY >= m__itemList.Count)
      {
        m_iCursorY = (m__itemList.Count - m_iOffset) - 1;
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
      if (iXoffset < 0 || iYOffset < 0) return;
      if (iXoffset2 < 0 || iYOffset2 < 0) return;
      if (iXoffset3 < 0 || iYOffset3 < 0) return;
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
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      m_iImageWidth = iWidth;
      m_iImageHeight = iHeight;
    }

    /// <summary>
    /// Get/set the height of an item.
    /// </summary>
    public int ItemHeight
    {
      get { return m_iItemHeight; }
      set
      {
        if (value < 0) return;
        m_iItemHeight = value;
      }
    }

    /// <summary>
    /// Get/set the space between items.
    /// </summary>
    public int Space
    {
      get { return m_iSpaceBetweenItems; }
      set
      {
        if (value < 0) return;
        m_iSpaceBetweenItems = value;
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
        if (value == null) return;
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
    /// Set the colors of the second label.
    /// </summary>
    /// <param name="dwTextColor"></param>
    /// <param name="dwSelectedColor"></param>
    public void SetColors2(long dwTextColor, long dwSelectedColor)
    {
      _textColor2 = dwTextColor;
      m_dwSelectedColor2 = dwSelectedColor;
    }

    /// <summary>
    /// Set the colors of the second label.
    /// </summary>
    /// <param name="dwTextColor"></param>
    /// <param name="dwSelectedColor"></param>
    public void SetColors3(long dwTextColor, long dwSelectedColor)
    {
      _textColor3 = dwTextColor;
      m_dwSelectedColor3 = dwSelectedColor;
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
      int iItem = m_iCursorY + m_iOffset;
      if (iItem >= 0 && iItem < m__itemList.Count)
      {
        GUIListItem pItem = m__itemList[iItem];
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
      get { return m__itemList.Count; }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (index < 0 || index >= m__itemList.Count) return null;
        return m__itemList[index];
      }
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        int iItem = m_iCursorY + m_iOffset;
        if (iItem >= 0 && iItem < m__itemList.Count)
        {
          GUIListItem pItem = m__itemList[iItem];
          return pItem;
        }
        return null;
      }
    }

    public int SelectedListItemIndex
    {
      get
      {
        int iItem = m_iCursorY + m_iOffset;
        if (iItem >= 0 && iItem < m__itemList.Count)
        {
          return iItem;
        }
        return -1;
      }
      set
      {
        if (value >= 0 && value < m__itemList.Count)
        {
          int iPage = 1;
          m_iOffset = 0;
          m_iCursorY = value;
          while (m_iCursorY >= m_iItemsPerPage)
          {
            iPage++;
            m_iOffset += m_iItemsPerPage;
            m_iCursorY -= m_iItemsPerPage;
          }
          m_upDown.Value = iPage;
          OnSelectionChanged();
        }
        m_bRefresh = true;
      }
    }

    /// <summary>
    /// Set the visibility of the page control.
    /// </summary>
    /// <param name="bVisible">true if visible false otherwise</param>
    public void SetPageControlVisible(bool bVisible)
    {
      m_bUpDownVisible = bVisible;
    }

    /// <summary>
    /// Get/set the shaded color.
    /// </summary>
    public long ShadedColor
    {
      get { return m_dwShadedColor; }
      set { m_dwShadedColor = value; }
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
      get { return m_dwSelectedColor; }
    }

    /// <summary>
    /// Get the color of the text of the second label of a selected item.
    /// </summary>
    public long SelectedColor2
    {
      get { return m_dwSelectedColor2; }
    }

    /// <summary>
    /// Get the color of the text of the second label of a third item.
    /// </summary>
    public long SelectedColor3
    {
      get { return m_dwSelectedColor3; }
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
      get { return m_upDown.Width / 2; }
    }

    // TODO
    public int SpinHeight
    {
      get { return m_upDown.Height; }
    }

    /// <summary>
    /// Gets the name of the unfocused up texture.
    /// </summary>
    public string TexutureUpName
    {
      get { return m_upDown.TexutureUpName; }
    }

    /// <summary>
    /// Gets the name of the unfocused down texture.
    /// </summary>
    public string TexutureDownName
    {
      get { return m_upDown.TexutureDownName; }
    }

    /// <summary>
    /// Gets the name of the focused up texture.
    /// </summary>
    public string TexutureUpFocusName
    {
      get { return m_upDown.TexutureUpFocusName; }
    }

    /// <summary>
    /// Gets the name of the focused down texture.
    /// </summary>
    public string TexutureDownFocusName
    {
      get { return m_upDown.TexutureDownFocusName; }
    }

    // TODO
    public long SpinTextColor
    {
      get { return m_upDown.TextColor; }
    }

    // TODO
    public int SpinX
    {
      get { return m_upDown.XPosition; }
    }

    // TODO
    public int SpinY
    {
      get { return m_upDown.YPosition; }
      set { m_upDown.YPosition = value; }
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
        if (value < 0) return;
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
        if (value < 0) return;
        _iconOffsetY = value;
      }
    }

    public bool TextVisible1
    {
      get { return m_bTextVisible1; }
      set { m_bTextVisible1 = value; }
    }

    public bool TextVisible2
    {
      get { return m_bTextVisible2; }
      set { m_bTextVisible2 = value; }
    }

    public bool TextVisible3
    {
      get { return m_bTextVisible3; }
      set { m_bTextVisible3 = value; }
    }

    /// <summary>
    /// Gets the width of the images of the items. 
    /// </summary>
    public int ImageWidth
    {
      get { return m_iImageWidth; }
    }

    /// <summary>
    /// Gets the height of the images of the items. 
    /// </summary>
    public int ImageHeight
    {
      get { return m_iImageHeight; }
    }

    /// <summary>
    /// Gets the name of the texture for the focused item.
    /// </summary>
    public string ButtonFocusName
    {
      get { return m_strButtonFocused; }
    }

    /// <summary>
    /// Gets the name of the texture for the unfocused item.
    /// </summary>
    public string ButtonNoFocusName
    {
      get { return m_strButtonUnfocused; }
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
      if (m_vertScrollbar.HitTest(x, y, out id, out focus)) return true;

      if (m_upDown.HitTest(x, y, out id, out focus))
      {
        if (m_upDown.GetMaximum() > 1)
        {
          m_iSelect = ListType.CONTROL_UPDOWN;
          m_upDown.Focus = true;
          if (!m_upDown.Focus)
          {
            m_iSelect = ListType.CONTROL_LIST;
          }
          return true;
        }
        return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        return false;
      }
      m_iSelect = ListType.CONTROL_LIST;
      y -= _positionY;
      m_iCursorY = (y / (m_iItemHeight + m_iSpaceBetweenItems));
      while (m_iOffset + m_iCursorY >= m__itemList.Count) m_iCursorY--;
      if (m_iCursorY >= m_iItemsPerPage)
        m_iCursorY = m_iItemsPerPage - 1;
      OnSelectionChanged();
      m_bRefresh = true;

      return true;
    }

    /// <summary>
    /// Sorts the list of items in this control.
    /// </summary>
    /// <param name="comparer">The comparer on which the sort is based.</param>
    public void Sort(System.Collections.Generic.IComparer<GUIListItem> comparer)
    {
      try
      {
        m__itemList.Sort(comparer);
      }
      catch (Exception)
      {
      }
      m_bRefresh = true;
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not.
    /// </summary>
    /// <returns>true or false</returns>
    public override bool NeedRefresh()
    {
      bool bRefresh = m_bRefresh;
      m_bRefresh = false;
      return bRefresh;
    }

    /// <summary>
    /// Get/set if the aspectration of the images of the items needs to be kept.
    /// </summary>
    public bool KeepAspectRatio
    {
      get { return m_bKeepAspectRatio; }
      set { m_bKeepAspectRatio = value; }
    }

    /// <summary>
    /// Gets the vertical scrollbar.
    /// </summary>
    public GUIverticalScrollbar Scrollbar
    {
      get { return m_vertScrollbar; }
    }

    /// <summary>
    /// Get/set if the control has the focus.
    /// </summary>
    public override bool Focus
    {
      get { return IsFocused; }
      set { if (IsFocused != value) { base.Focus = value; m_bDrawFocus = true; } }
    }

    public void Add(GUIListItem item)
    {
      if (item == null) return;
      if (WordWrap)
      {
        ArrayList wrappedLines;
        int dMaxWidth = (_width - m_iImageWidth - 16);

        WordWrapText(item.Label, dMaxWidth, out wrappedLines);
        foreach (string line in wrappedLines)
        {
          m__itemList.Add(new GUIListItem(line));
        }
      }
      else
      {
        m__itemList.Add(item);
      }
      int iPages = m__itemList.Count / m_iItemsPerPage;
      if ((m__itemList.Count % m_iItemsPerPage) != 0) iPages++;
      m_upDown.SetRange(1, iPages);
      m_upDown.Value = 1;
      m_bRefresh = true;
    }

    public int PinIconOffsetX
    {
      get { return m_iPinIconOffsetX; }
      set
      {
        if (value < 0) return;
        m_iPinIconOffsetX = value;
      }
    }

    public int PinIconOffsetY
    {
      get { return m_iPinIconOffsetY; }
      set
      {
        if (value < 0) return;
        m_iPinIconOffsetY = value;
      }
    }

    public int PinIconWidth
    {
      get { return m_iPinIconWidth; }
      set
      {
        if (value < 0) return;
        m_iPinIconWidth = value;
      }
    }

    public int PinIconHeight
    {
      get { return m_iPinIconHeight; }
      set
      {
        if (value < 0) return;
        m_iPinIconHeight = value;
      }
    }

    public void ScrollToEnd()
    {
      while (m_iOffset + 1 + m_iCursorY < m__itemList.Count)
        OnDown();
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
        if (m_upDown != null) m_upDown.ParentID = value;
      }
    }

    public bool WordWrap
    {
      get { return wordWrap; }
      set { wordWrap = value; }
    }

    void WordWrapText(string strText, int iMaxWidth, out ArrayList wrappedLines)
    {
      wrappedLines = new ArrayList();
      GUILabelControl cntl1 = new GUILabelControl(_controlId, 0, 0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height, _fontName, "", _textColor, GUIControl.Alignment.ALIGN_LEFT, false);
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
          if (szLine.Length > 0 || m__itemList.Count > 0)
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
            if (szLine.Length > 0 || m__itemList.Count > 0)
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
        if (m_upDown != null) m_upDown.WindowId = value;
      }
    }

    public void Clear()
    {
      m_iCursorY = 0;
      m_iOffset = 0;
      m__itemList.Clear();
      //GUITextureManager.CleanupThumbs();
      m_upDown.SetRange(1, 1);
      m_upDown.Value = 1;
      m_bRefresh = true;
      OnSelectionChanged();
    }

    public System.Drawing.Rectangle SelectedRectangle
    {
      get
      {
        int selectedIndex = m_iCursorY + m_iOffset;

        if (selectedIndex == -1 || selectedIndex >= m__itemList.Count)
          return System.Drawing.Rectangle.Empty;

        GUIControl btn = _listButtons[m_iCursorY];

        return new System.Drawing.Rectangle(btn.XPosition, btn.YPosition, btn.Width, btn.Height);
      }
    }
  }
}