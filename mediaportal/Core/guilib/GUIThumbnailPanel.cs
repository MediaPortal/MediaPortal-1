/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms; // used for Keys definition
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIThumbnailPanel : GUIControl
  {
    const int SLEEP_FRAME_COUNT = 1;
    const int THUMBNAIL_OVERSIZED_DIVIDER = 32;

    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    } ;

    [XMLSkinElement("remoteColor")] protected long m_dwRemoteColor = 0xffff0000;
    [XMLSkinElement("downloadColor")] protected long m_dwDownloadColor = 0xff00ff00;

    [XMLSkinElement("thumbPosX")] protected int m_iThumbXPos = 8;
    [XMLSkinElement("thumbPosY")] protected int m_iThumbYPos = 8;
    [XMLSkinElement("thumbWidth")] protected int m_iThumbWidth = 64;
    [XMLSkinElement("thumbHeight")] protected int m_iThumbHeight = 64;

    [XMLSkinElement("itemHeight")] protected int m_iItemHeight;
    [XMLSkinElement("itemWidth")] protected int m_iItemWidth;
    /*"itemHeight"*/
    protected int m_iItemHeightLow;
    /*"itemWidth"*/
    protected int m_iItemWidthLow;
    [XMLSkinElement("textureHeight")] protected int m_iTextureHeightLow;
    [XMLSkinElement("textureWidth")] protected int m_iTextureWidthLow;

    [XMLSkinElement("itemHeightBig")] protected int m_iItemHeightBig = 150;
    [XMLSkinElement("itemWidthBig")] protected int m_iItemWidthBig = 150;
    [XMLSkinElement("thumbWidthBig")] protected int m_iTextureWidth = 80;
    [XMLSkinElement("thumbHeightBig")] protected int m_iTextureHeight = 80;
    [XMLSkinElement("thumbZoom")] protected bool m_bZoom = false;
    [XMLSkinElement("textureHeightBig")] protected int m_iTextureHeightBig = 128;
    [XMLSkinElement("textureWidthBig")] protected int m_iTextureWidthBig = 128;

    [XMLSkinElement("font")] protected string m_strFontName = "";
    [XMLSkinElement("textcolor")] protected long m_dwTextColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")] protected long m_dwSelectedColor = 0xFFFFFFFF;
    [XMLSkinElement("spinColor")] protected long m_dwSpinColor;
    [XMLSkinElement("spinHeight")] protected int m_dwSpinHeight;
    [XMLSkinElement("spinWidth")] protected int m_dwSpinWidth;
    [XMLSkinElement("spinPosX")] protected int m_dwSpinX;
    [XMLSkinElement("spinPosY")] protected int m_dwSpinY;

    [XMLSkinElement("scrollbarbg")] protected string m_strScrollBarBG = "";
    [XMLSkinElement("scrollbartop")] protected string m_strScrollBarTop = "";
    [XMLSkinElement("scrollbarbottom")] protected string m_strScrollBarBottom = "";

    [XMLSkinElement("textureUp")] protected string m_strUp = "";
    [XMLSkinElement("textureDown")] protected string m_strDown = "";
    [XMLSkinElement("textureUpFocus")] protected string m_strUpFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string m_strDownFocus = "";
    [XMLSkinElement("imageFolder")] protected string m_strImageFolder = "";
    [XMLSkinElement("imageFolderFocus")] protected string m_strImageFolderFocus = "";
    protected List<GUIButtonControl> m_button = null;

    int m_iThumbXPosLow = 0;
    int m_iThumbYPosLow = 0;
    int m_iThumbWidthLow = 0;
    int m_iThumbHeightLow = 0;

    [XMLSkinElement("thumbPosXBig")] protected int m_iThumbXPosBig = 0;
    [XMLSkinElement("thumbPosYBig")] protected int m_iThumbYPosBig = 0;
    [XMLSkinElement("thumbWidthBig")] protected int m_iThumbWidthBig = 0;
    [XMLSkinElement("thumbHeightBig")] protected int m_iThumbHeightBig = 0;

	[XMLSkinElement("folderPrefix")] protected string _folderPrefix = "[";
	[XMLSkinElement("folderSuffix")] protected string _folderSuffix = "]";
	  
	[XMLSkinElement("unfocusedAlpha")] protected int _unfocusedAlpha = 0xFF;
	  
	  bool m_bShowTexture = true;
    int m_iOffset = 0;
    int m_iLastItemPageValues = 0;

    GUIListControl.ListType m_iSelect = GUIListControl.ListType.CONTROL_LIST;
    int m_iCursorX = 0;
    int m_iCursorY = 0;
    int m_iRows;
    int m_iColumns;
    bool m_bScrollUp = false;
    bool m_bScrollDown = false;
    int m_iScrollCounter = 0;
    string m_strSuffix = "|";
    GUIFont m_pFont = null;
    GUISpinControl m_upDown = null;

    List<GUIListItem> m_vecItems = new List<GUIListItem>();
    int scroll_pos = 0;
    int iScrollX = 0;
    int iLastItem = -1;
    int iFrames = 12;
    int m_iSleeper = 0;
    bool m_bRefresh = false;
    protected GUIverticalScrollbar m_vertScrollbar = null;
    protected string m_strBrackedText;
    protected string m_strScrollText;

    protected double iScrollOffset = 0.0f;
    protected int iCurrentFrame = 0;
    protected double timeElapsed = 0.0f;
    protected bool scrollContinuosly = false;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedHorizontal)*0.01f); }
    }


    // Search
    DateTime m_keyTimer = DateTime.Now;
    char m_CurrentKey = (char) 0;
    char m_PrevKey = (char) 0;
    protected string m_strSearchString = "";
    protected int m_iLastSearchItem = 0;

    public GUIThumbnailPanel(int dwParentID) : base(dwParentID)
    {
    }

    public GUIThumbnailPanel(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
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
      // Please fix or remove this check: (dwPosY > dwPosY - always false)
      //if (dwPosY > dwPosY && dwSpinY < dwPosY + dwHeight) dwSpinY = dwPosY + dwHeight;
      m_strImageFolder = strImageIcon;
      m_strImageFolderFocus = strImageIconFocus;
      m_iItemWidth = dwitemWidth;
      m_iItemHeight = dwitemHeight;
      m_dwSpinWidth = dwSpinWidth;
      m_dwSpinHeight = dwSpinHeight;
      m_strUp = strUp;
      m_strDown = strDown;
      m_strUpFocus = strUpFocus;
      m_strDownFocus = strDownFocus;
      m_dwSpinColor = dwSpinColor;
      m_dwSpinX = dwSpinX;
      m_dwSpinY = dwSpinY;
      m_strFontName = strFont;
      m_dwTextColor = dwTextColor;
      m_dwSelectedColor = dwSelectedColor;

      m_strScrollBarBG = strScrollbarBackground;
      m_strScrollBarTop = strScrollbarTop;
      m_strScrollBarBottom = strScrollbarBottom;
      FinalizeConstruction();

    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      m_iItemHeightLow = m_iItemHeight;
      m_iItemWidthLow = m_iItemWidth;
      m_iTextureWidthLow = m_iTextureWidth;
      m_iTextureHeightLow = m_iTextureHeight;

      m_upDown = new GUISpinControl(GetID, 0, m_dwSpinX, m_dwSpinY,
                                    m_dwSpinWidth, m_dwSpinHeight,
                                    m_strUp, m_strDown, m_strUpFocus, m_strDownFocus,
                                    m_strFontName, m_dwSpinColor,
                                    GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT,
                                    GUIControl.Alignment.ALIGN_LEFT);
      int xpos = 5 + m_dwPosX + m_dwWidth;
      if (xpos + 15 > GUIGraphicsContext.Width) xpos = GUIGraphicsContext.Width - 15;
      m_vertScrollbar = new GUIverticalScrollbar(m_dwControlID, 0,
                                                 5 + m_dwPosX + m_dwWidth, m_dwPosY, 15, m_dwHeight,
                                                 m_strScrollBarBG, m_strScrollBarTop, m_strScrollBarBottom);
      m_vertScrollbar.SendNotifies = false;
      m_pFont = GUIFontManager.GetFont(m_strFontName);
      SetTextureDimensions(m_iTextureWidth, m_iTextureHeight);
      SetThumbDimensionsLow(m_iThumbXPos, m_iThumbYPos, m_iThumbWidth, m_iThumbHeight);

    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScaleRectToScreenResolution
        (ref m_dwSpinX, ref m_dwSpinY, ref m_dwSpinWidth, ref m_dwSpinHeight);
      GUIGraphicsContext.ScalePosToScreenResolution
        (ref m_iTextureWidth, ref m_iTextureHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution
        (ref m_iThumbXPos, ref m_iThumbYPos, ref m_iThumbWidth, ref m_iThumbHeight);
      GUIGraphicsContext.ScalePosToScreenResolution
        (ref m_iTextureWidthBig, ref m_iTextureHeightBig);
      GUIGraphicsContext.ScaleRectToScreenResolution
        (ref m_iThumbXPosBig, ref m_iThumbYPosBig,
         ref m_iThumbWidthBig, ref m_iThumbHeightBig);
      GUIGraphicsContext.ScalePosToScreenResolution
        (ref m_iItemWidthBig, ref m_iItemHeightBig);
      GUIGraphicsContext.ScalePosToScreenResolution
        (ref m_iItemWidth, ref m_iItemHeight);

    }


    protected void OnSelectionChanged()
    {
      if (!IsVisible) return;

      // Reset searchstring
      if (m_iLastSearchItem != (m_iOffset + m_iCursorY*m_iColumns + m_iCursorX))
      {
        m_PrevKey = (char) 0;
        m_CurrentKey = (char) 0;
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
      }

      if (item >= 0 && item < m_vecItems.Count)
      {
        GUIListItem listitem = m_vecItems[item] ;
        if (listitem != null) listitem.ItemSelected(this);
      }
      // ToDo: add searchstring property
      if (m_strSearchString.Length > 0)
        GUIPropertyManager.SetProperty("#selecteditem", "{" + m_strSearchString.ToLower() + "}");
    }


    void RenderItem(float timePassed, int iButton, bool bFocus, int dwPosX, int dwPosY, GUIListItem pItem, bool buttonOnly)
    {
      if (m_button == null) return;
      if (iButton < 0 || iButton >= m_button.Count) return;
      GUIButtonControl btn = m_button[iButton] ;
      if (btn == null) return;

      float fTextPosY = (float) dwPosY + (float) m_iTextureHeight;

      long dwColor = m_dwTextColor;
      if (pItem.Selected) dwColor = m_dwSelectedColor;
			if (!bFocus)
				dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();

      if (pItem.IsRemote)
      {
        dwColor = m_dwRemoteColor;
        if (pItem.IsDownloading) dwColor = m_dwDownloadColor;
      }
      if (bFocus == true && Focus && m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (buttonOnly)
        {
					btn.ColourDiffuse=0xffffffff;
          btn.Focus = true;
          btn.SetPosition(dwPosX, dwPosY);
          if (true == m_bShowTexture) btn.Render(timePassed);
          return;
        }
        if (fTextPosY >= m_dwPosY) RenderText((float) dwPosX, fTextPosY, dwColor, pItem.Label, true);
      }
      else
      {
        if (buttonOnly)
				{
					btn.ColourDiffuse=Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
          btn.Focus = false;
          btn.SetPosition(dwPosX, dwPosY);
          if (true == m_bShowTexture) btn.Render(timePassed);
          return;
        }
        if (fTextPosY >= m_dwPosY) RenderText((float) dwPosX, fTextPosY, dwColor, pItem.Label, false);
      }

      // Set oversized value
      int iOverSized = 0;
      if (bFocus && Focus)
      {
        iOverSized = (m_iThumbWidth + m_iThumbHeight)/THUMBNAIL_OVERSIZED_DIVIDER;
      }

      if (pItem.HasThumbnail)
      {
        GUIImage pImage = pItem.Thumbnail;
        if (null == pImage /*&& m_iSleeper==0 */&& !IsAnimating)
        {
          pImage = new GUIImage(0, 0, m_iThumbXPos - iOverSized + dwPosX, m_iThumbYPos - iOverSized + dwPosY, m_iThumbWidth + 2*iOverSized, m_iThumbHeight + 2*iOverSized, pItem.ThumbnailImage, 0x0);
          pImage.KeepAspectRatio = true;
          pImage.ZoomFromTop = !pItem.IsFolder && m_bZoom;
          pImage.AllocResources();

          pItem.Thumbnail = pImage;
          int xOff = (m_iThumbWidth + 2*iOverSized - pImage.RenderWidth)/2;
          int yOff = (m_iThumbHeight + 2*iOverSized - pImage.RenderHeight)/2;
          pImage.SetPosition(m_iThumbXPos - iOverSized + dwPosX + xOff, m_iThumbYPos - iOverSized + dwPosY + yOff);
          pImage.Render(timePassed);
          m_iSleeper += SLEEP_FRAME_COUNT;
        }
        if (null != pImage)
        {
          if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
          {
            pImage.FreeResources();
            pImage.AllocResources();
          }
          pImage.ZoomFromTop = !pItem.IsFolder && m_bZoom;
          pImage.Width = m_iThumbWidth + 2*iOverSized;
          pImage.Height = m_iThumbHeight + 2*iOverSized;
          int xOff = (m_iThumbWidth + 2*iOverSized - pImage.RenderWidth)/2;
          int yOff = (m_iThumbHeight + 2*iOverSized - pImage.RenderHeight)/2;
          pImage.SetPosition(m_iThumbXPos + dwPosX - iOverSized + xOff, m_iThumbYPos - iOverSized + dwPosY + yOff);
					if (bFocus)
						pImage.ColourDiffuse=0xffffffff;
					else
						pImage.ColourDiffuse=Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();

          pImage.Render(timePassed);
        }
      }
      else
      {
        if (pItem.HasIconBig)
        {
          GUIImage pImage = pItem.IconBig;
          if (null == pImage /*&& m_iSleeper==0 */&& !IsAnimating)
          {
            pImage = new GUIImage(0, 0, m_iThumbXPos - iOverSized + dwPosX, m_iThumbYPos - iOverSized + dwPosY, m_iThumbWidth + 2*iOverSized, m_iThumbHeight + 2*iOverSized, pItem.IconImageBig, 0x0);
            pImage.KeepAspectRatio = true;
            pImage.ZoomFromTop = !pItem.IsFolder && m_bZoom;

            pImage.AllocResources();
            pItem.IconBig = pImage;
            int xOff = (m_iThumbWidth + 2*iOverSized - pImage.RenderWidth)/2;
            int yOff = (m_iThumbHeight + 2*iOverSized - pImage.RenderHeight)/2;
            pImage.SetPosition(m_iThumbXPos + dwPosX - iOverSized + xOff, m_iThumbYPos - iOverSized + dwPosY + yOff);
            pImage.Render(timePassed);
            m_iSleeper += SLEEP_FRAME_COUNT;
          }
          if (null != pImage)
          {
            if (pImage.TextureHeight == 0 && pImage.TextureWidth == 0)
            {
              pImage.FreeResources();
              pImage.AllocResources();
            }
            pImage.ZoomFromTop = !pItem.IsFolder && m_bZoom;
            pImage.Width = m_iThumbWidth + 2*iOverSized;
            pImage.Height = m_iThumbHeight + 2*iOverSized;
            int xOff = (m_iThumbWidth + 2*iOverSized - pImage.RenderWidth)/2;
            int yOff = (m_iThumbHeight + 2*iOverSized - pImage.RenderHeight)/2;
						pImage.SetPosition(m_iThumbXPos - iOverSized + dwPosX + xOff, m_iThumbYPos - iOverSized + dwPosY + yOff);
						if (bFocus)
							pImage.ColourDiffuse=0xffffffff;
						else
							pImage.ColourDiffuse=Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();

            pImage.Render(timePassed);
          }
        }
      }
    }

    public override void Render(float timePassed)
    {
      timeElapsed += timePassed;
      iCurrentFrame = (int) (timeElapsed/TimeSlice);

      if (null == m_pFont) return;
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }

      int dwPosY = 0;
      if ((m_iCursorX > 0 || m_iCursorY > 0) && !ValidItem(m_iCursorX, m_iCursorY))
      {
        m_iCursorX = 0;
        m_iCursorY = 0;
        OnSelectionChanged();
      }
      if (m_iSleeper > 0) m_iSleeper--;

      if (m_vertScrollbar != null)
      {
        float fPercent = (float) m_iCursorY*m_iColumns + m_iOffset + m_iCursorX;
        fPercent /= (float) (m_vecItems.Count);
        fPercent *= 100.0f;
        if ((int) fPercent != (int) m_vertScrollbar.Percentage)
        {
          m_vertScrollbar.Percentage = fPercent;
        }
      }

      int iScrollYOffset = 0;
      if (true == m_bScrollDown)
      {
        iScrollYOffset = - (m_iItemHeight - m_iScrollCounter);
      }
      if (true == m_bScrollUp)
      {
        iScrollYOffset = m_iItemHeight - m_iScrollCounter;
      }

      Viewport oldview = GUIGraphicsContext.DX9Device.Viewport;
      Viewport view = new Viewport();
      float fx = (float) m_dwPosX;
      float fy = (float) m_dwPosY;
      GUIGraphicsContext.Correct(ref fx, ref fy);

      if (fx <= 0) fx = 0;
      if (fy <= 0) fy = 0;
      view.X = (int) fx;
      view.Y = (int) fy;
      view.Width = m_iColumns*m_iItemWidth;
      view.Height = m_iRows*m_iItemHeight;
      view.MinZ = 0.0f;
      view.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = view;

      // Free unused textures if page has changed
      int iStartItem = m_iOffset;
      int iEndItem = m_iRows*m_iColumns + m_iOffset;
      if ((m_iLastItemPageValues != iStartItem + iEndItem) && (iScrollYOffset == 0))
      {
        m_iLastItemPageValues = iStartItem + iEndItem;
        if (iStartItem < m_vecItems.Count)
        {
          for (int i = 0; i < iStartItem; ++i)
          {
            GUIListItem pItem = m_vecItems[i];
            if (null != pItem)
            {
              pItem.FreeMemory();
            }
          }
        }

        for (int i = iEndItem + 1; i < m_vecItems.Count; ++i)
        {
          GUIListItem pItem = m_vecItems[i];
          if (null != pItem)
          {
            pItem.FreeMemory();
          }
        }
      }

      for (int i = 0; i < 2; ++i)
      {
        if (m_bScrollUp)
        {
          // render item on top
          dwPosY = m_dwPosY - m_iItemHeight + iScrollYOffset;
          m_iOffset -= m_iColumns;
          for (int iCol = 0; iCol < m_iColumns; iCol++)
          {
            int dwPosX = m_dwPosX + iCol*m_iItemWidth;
            int iItem = iCol + m_iOffset;
            if (iItem > 0 && iItem < m_vecItems.Count)
            {
              GUIListItem pItem = m_vecItems[iItem];
              RenderItem(timePassed, 0, false, dwPosX, dwPosY, pItem, i == 0);
              if (iItem < iStartItem) iStartItem = iItem;
              if (iItem > iEndItem) iEndItem = iItem;
            }
          }
          m_iOffset += m_iColumns;
        }

        // render main panel
        for (int iRow = 0; iRow < m_iRows; iRow++)
        {
          dwPosY = m_dwPosY + iRow*m_iItemHeight + iScrollYOffset;
          for (int iCol = 0; iCol < m_iColumns; iCol++)
          {
            int dwPosX = m_dwPosX + iCol*m_iItemWidth;
            int iItem = iRow*m_iColumns + iCol + m_iOffset;
            if (iItem < m_vecItems.Count)
            {
              GUIListItem pItem = m_vecItems[iItem];
              bool bFocus = (m_iCursorX == iCol && m_iCursorY == iRow);
              RenderItem(timePassed, iRow*m_iRows + iCol, bFocus, dwPosX, dwPosY, pItem, i == 0);
              if (iItem < iStartItem) iStartItem = iItem;
              if (iItem > iEndItem) iEndItem = iItem;
            }
          }
        }

        if (m_bScrollDown)
        {
          // render item on bottom
          dwPosY = m_dwPosY + m_iRows*m_iItemHeight + iScrollYOffset;
          for (int iCol = 0; iCol < m_iColumns; iCol++)
          {
            int dwPosX = m_dwPosX + iCol*m_iItemWidth;
            int iItem = m_iRows*m_iColumns + iCol + m_iOffset;
            if (iItem < m_vecItems.Count)
            {
              GUIListItem pItem = m_vecItems[iItem];
              RenderItem(timePassed, 0, false, dwPosX, dwPosY, pItem, i == 0);
              if (iItem < iStartItem) iStartItem = iItem;
              if (iItem > iEndItem) iEndItem = iItem;
            }
          }
        }
      }

      GUIGraphicsContext.DX9Device.Viewport = oldview;

      //  iFrames = 12;
      int iStep = m_iItemHeight/iFrames;
      if (0 == iStep) iStep = 1;
      if (m_bScrollDown)
      {
        m_iScrollCounter -= iStep;
        if (m_iScrollCounter <= 0)
        {
          m_bScrollDown = false;
          m_iOffset += m_iColumns;
          int iPage = m_iOffset/(m_iRows*m_iColumns);
          if ((m_iOffset%(m_iRows*m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
          m_bRefresh = true;
          OnSelectionChanged();
        }
      }
      if (m_bScrollUp)
      {
        m_iScrollCounter -= iStep;
        if (m_iScrollCounter <= 0)
        {
          m_bScrollUp = false;
          m_iOffset -= m_iColumns;
          int iPage = m_iOffset/(m_iRows*m_iColumns);
          if ((m_iOffset%(m_iRows*m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
          m_bRefresh = true;
          OnSelectionChanged();
        }
      }

      dwPosY = m_dwPosY + m_iRows*(m_iItemHeight);
      //m_upDown.SetPosition(m_upDown.XPosition,dwPosY);
      m_upDown.Render(timePassed);
      if (m_bScrollDown || m_bScrollUp)
      {
        m_bRefresh = true;
      }
      int iItemsPerPage = m_iRows*m_iColumns;
      if (m_vecItems.Count > iItemsPerPage)
      {
        m_vertScrollbar.Render(timePassed);
      }
      if (Focus)
        GUIPropertyManager.SetProperty("#highlightedbutton", String.Empty);
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
					m_strSearchString="";
          OnPageUp();
          m_bRefresh = true;
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          m_strSearchString="";
					OnPageDown();
          m_bRefresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          {
            m_strSearchString="";
						m_iOffset = 0;
            m_iCursorY = 0;
            m_iCursorX = 0;
            m_upDown.Value = 1;
            OnSelectionChanged();

            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
						m_strSearchString="";
            int iChan = m_vecItems.Count - 1;
            if (iChan >= 0)
            {
              // update spin controls
              int iItemsPerPage = m_iRows*m_iColumns;
              int iPage = 1;
              int iSel = iChan;
              while (iSel >= iItemsPerPage)
              {
                iPage++;
                iSel -= iItemsPerPage;
              }
              m_upDown.Value = iPage;

              // find item
              m_iOffset = 0;
              m_iCursorY = 0;
              while (iChan >= iItemsPerPage)
              {
                iChan -= iItemsPerPage;
                m_iOffset += iItemsPerPage;
              }
              while (iChan >= m_iColumns)
              {
                iChan -= m_iColumns;
                m_iCursorY++;
              }
              m_iCursorX = iChan;
              OnSelectionChanged();
            }
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            if (m_strSearchString != "")
              SearchItem(m_strSearchString, SearchType.SEARCH_NEXT);
            else
              OnDown();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (m_strSearchString != "")
              SearchItem(m_strSearchString, SearchType.SEARCH_PREV);
            else
              OnUp();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
						m_strSearchString="";	
            OnLeft();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
						m_strSearchString="";
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
                Press((char) action.m_key.KeyChar);
                return;
              }

              if (action.m_key.KeyChar == (int) Keys.Back)
              {
                if (m_strSearchString.Length > 0)
                  m_strSearchString = m_strSearchString.Remove(m_strSearchString.Length - 1, 1);
                SearchItem(m_strSearchString, SearchType.SEARCH_FIRST);
              }
              if (((action.m_key.KeyChar >= 65) && (action.m_key.KeyChar <= 90)) || (action.m_key.KeyChar == (int) Keys.Space))
              {
                if (action.m_key.KeyChar == (int) Keys.Space && m_strSearchString == String.Empty) return;
                m_strSearchString += (char) action.m_key.KeyChar;
                SearchItem(m_strSearchString, SearchType.SEARCH_FIRST);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int id;
            bool focus;
            if (m_vertScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
            {
              int iItemsPerPage = m_iRows*m_iColumns;
//            m_bDrawFocus=false;
              m_vertScrollbar.OnAction(action);
              float fPercentage = m_vertScrollbar.Percentage;
              fPercentage /= 100.0f;
              fPercentage *= (float) m_vecItems.Count;
              int iChan = (int) fPercentage;
              if (iChan != m_iOffset + m_iCursorY*m_iColumns + m_iCursorX)
              {
                // update spin controls
                int iPage = 1;
                int iSel = iChan;
                while (iSel >= iItemsPerPage)
                {
                  iPage++;
                  iSel -= iItemsPerPage;
                }
                m_upDown.Value = iPage;

                // find item
                m_iOffset = 0;
                m_iCursorY = 0;
                while (iChan >= iItemsPerPage)
                {
                  iChan -= iItemsPerPage;
                  m_iOffset += iItemsPerPage;
                }
                while (iChan >= m_iColumns)
                {
                  iChan -= m_iColumns;
                  m_iCursorY++;
                }
                m_iCursorX = iChan;
                OnSelectionChanged();
              }
              return;
            }
//          m_bDrawFocus=true;
          }
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            int id;
            bool focus;
            if (m_vertScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
            {
              //m_bDrawFocus=false;
              int iItemsPerPage = m_iRows*m_iColumns;
              m_vertScrollbar.OnAction(action);
              float fPercentage = m_vertScrollbar.Percentage;
              fPercentage /= 100.0f;
              fPercentage *= (float) m_vecItems.Count;
              int iChan = (int) fPercentage;
              if (iChan != m_iOffset + m_iCursorY*m_iColumns + m_iCursorX)
              {
                // update spin controls
                int iPage = 1;
                int iSel = iChan;
                while (iSel >= iItemsPerPage)
                {
                  iPage++;
                  iSel -= iItemsPerPage;
                }
                m_upDown.Value = iPage;

                // find item
                m_iOffset = 0;
                m_iCursorY = 0;
                while (iChan >= iItemsPerPage)
                {
                  iChan -= iItemsPerPage;
                  m_iOffset += iItemsPerPage;
                }
                while (iChan >= m_iColumns)
                {
                  iChan -= m_iColumns;
                  m_iCursorY++;
                }
                m_iCursorX = iChan;
                OnSelectionChanged();
              }
              return;
            }
            else
            {
              //m_bDrawFocus=true;
              if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
              {
								m_strSearchString="";
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int) Action.ActionType.ACTION_SELECT_ITEM, 0, null);
                GUIGraphicsContext.SendMessage(msg);
              }
              else
              {
                m_upDown.OnAction(action);
              }
              m_bRefresh = true;
            }
          }
          break;

        default:
          {
            if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
            {
							m_strSearchString="";
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int) action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
              m_bRefresh = true;
            }
            else
            {
              m_upDown.OnAction(action);
              m_bRefresh = true;
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
            m_iOffset = (m_upDown.Value - 1)*(m_iRows*m_iColumns);
            m_bRefresh = true;
            OnSelectionChanged();
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
          if (iItem >= 0 && iItem < m_vecItems.Count)
          {
            message.Object = m_vecItems[iItem];
          }
          else
          {
            message.Object = null;
          }
          m_bRefresh = true;
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < m_vecItems.Count)
          {
            message.Object = m_vecItems[iItem];
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
          foreach (GUIListItem item in m_vecItems)
          {
            item.FreeMemory();
          }
          m_bRefresh = true;
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
          m_bRefresh = true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem newItem = message.Object as GUIListItem;
          if (newItem != null) m_vecItems.Add(newItem);
          int iItemsPerPage = m_iRows*m_iColumns;
          int iPages = m_vecItems.Count/iItemsPerPage;
          if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;
          m_upDown.SetRange(1, iPages);
          m_upDown.Value = 1;
          m_bRefresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = m_vecItems.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          message.Param1 = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < m_vecItems.Count)
          {
            int iPage = 1;
            m_iCursorX = 0;
            m_iCursorY = 0;
            m_iOffset = 0;
            while (iItem >= (m_iRows*m_iColumns))
            {
              m_iOffset += (m_iRows*m_iColumns);
              iItem -= (m_iRows*m_iColumns);
              iPage++;
            }
            while (iItem >= m_iColumns)
            {
              m_iCursorY++;
              iItem -= m_iColumns;
            }
            m_upDown.Value = iPage;
            m_iCursorX = iItem;
            OnSelectionChanged();
          }
          m_bRefresh = true;
        }
      }
      if (message.Message == GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING)
      {
        foreach (GUIListItem item in m_vecItems)
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
        foreach (GUIListItem item in m_vecItems)
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
      int iCurrentItem = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
      if (iSearchMethode == SearchType.SEARCH_FIRST) iCurrentItem = 0;
      int iItem = iCurrentItem;
      do
      {
        if (iSearchMethode == SearchType.SEARCH_NEXT)
        {
          iItem++;
          if (iItem >= m_vecItems.Count) iItem = 0;
        }
        if (iSearchMethode == SearchType.SEARCH_PREV && m_vecItems.Count > 0)
        {
          iItem--;
          if (iItem < 0) iItem = m_vecItems.Count - 1;
        }

        GUIListItem pItem = m_vecItems[iItem];
        if (pItem.Label.ToUpper().StartsWith(SearchKey.ToUpper()) == true)
        {
          bItemFound = true;
          break;
        }

        if (iSearchMethode == SearchType.SEARCH_FIRST)
        {
          iItem++;
          if (iItem >= m_vecItems.Count) iItem = 0;
        }
      } while (iItem != iCurrentItem);

      if ((bItemFound) && (iItem >= 0 && iItem < m_vecItems.Count))
      {
        // update spin controls
        int iItemsPerPage = m_iRows*m_iColumns;
        int iPage = 1;
        int iSel = iItem;
        while (iSel >= iItemsPerPage)
        {
          iPage++;
          iSel -= iItemsPerPage;
        }
        m_upDown.Value = iPage;

        // find item
        m_iOffset = 0;
        m_iCursorY = 0;
        while (iItem >= iItemsPerPage)
        {
          iItem -= iItemsPerPage;
          m_iOffset += iItemsPerPage;
        }
        while (iItem >= m_iColumns)
        {
          iItem -= m_iColumns;
          m_iCursorY++;
        }
        m_iCursorX = iItem;
      }

      m_iLastSearchItem = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
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
        m_CurrentKey = (char) 0;

      if (Key == '*')
      {
        // Backspace
        if (m_strSearchString.Length > 0)
          m_strSearchString = m_strSearchString.Remove(m_strSearchString.Length - 1, 1);
        m_PrevKey = (char) 0;
        m_CurrentKey = (char) 0;
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
        m_PrevKey = (char) 0;
        m_CurrentKey = (char) 0;
      }
    }

    public override void PreAllocResources()
    {
      if (null == m_pFont) return;
      base.PreAllocResources();
      m_upDown.PreAllocResources();
      m_vertScrollbar.PreAllocResources();
    }


    void Calculate()
    {
      float fWidth = 0, fHeight = 0;

      // height of 1 item = folder image height + text row height + space in between
      m_pFont.GetTextExtent("y", ref fWidth, ref fHeight);

      fWidth = (float) m_iItemWidth;
      fHeight = (float) m_iItemHeight;
      float fTotalHeight = (float) (m_dwHeight - 5);
      m_iRows = (int) (fTotalHeight/fHeight);
      m_iColumns = (int) (m_dwWidth/fWidth);

      int iItemsPerPage = m_iRows*m_iColumns;
      int iPages = m_vecItems.Count/iItemsPerPage;
      if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;
      m_upDown.SetRange(1, iPages);
      m_upDown.Value = 1;

      // Dispose used buttoncontrols
      if (m_button != null)
      {
        for (int i = 0; i < m_button.Count; ++i)
        {
          GUIButtonControl cntl = m_button[i];
          cntl.FreeResources();
        }
      }
      m_button = null;

      // Create new buttoncontrols
      m_button = new List<GUIButtonControl>();
      for (int i = 0; i < m_iColumns*m_iRows; ++i)
      {
        GUIButtonControl btn = new GUIButtonControl(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, m_iTextureWidth, m_iTextureHeight, m_strImageFolderFocus, m_strImageFolder);
        btn.AllocResources();
        m_button.Add(btn);
      }
    }

    public override void AllocResources()
    {
      m_iSleeper = 0;
      m_pFont = GUIFontManager.GetFont(m_strFontName);
      base.AllocResources();
      m_upDown.AllocResources();
      m_vertScrollbar.AllocResources();
      Calculate();
    }

    public override void FreeResources()
    {
      foreach (GUIListItem item in m_vecItems)
      {
        item.FreeIcons();
      }
      if (m_button != null)
      {
        for (int i = 0; i < m_button.Count; ++i)
        {
          GUIButtonControl cntl = m_button[i];
          cntl.FreeResources();
        }
      }
      m_button = null;
      m_vecItems.Clear();
      base.FreeResources();
      m_upDown.FreeResources();
      m_vertScrollbar.FreeResources();
    }


    bool ValidItem(int iX, int iY)
    {
      if (iX < 0) return false;
      if (iY < 0) return false;
      if (iX >= m_iColumns) return false;
      if (iY >= m_iRows) return false;
      if (m_iOffset + iY*m_iColumns + iX < m_vecItems.Count) return true;
      return false;
    }


    void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (m_iCursorX + 1 < m_iColumns && ValidItem(m_iCursorX + 1, m_iCursorY))
        {
          m_iCursorX++;
          OnSelectionChanged();
          return;
        }

        if (m_upDown.GetMaximum() > 1)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_UPDOWN;
          m_upDown.Focus = true;
        }
        OnSelectionChanged();
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          base.OnAction(action);
        }
        OnSelectionChanged();
      }
    }

    void OnLeft()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (m_iCursorX > 0)
        {
          m_iCursorX--;
          OnSelectionChanged();
          return;
        }
        base.OnAction(action);
        OnSelectionChanged();
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
        OnSelectionChanged();
      }
    }

    void OnUp()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (m_bScrollUp)
        {
          m_iScrollCounter = 0;
          m_bScrollUp = false;
          m_iOffset -= m_iColumns;
          int iPage = m_iOffset/(m_iRows*m_iColumns);
          if ((m_iOffset%(m_iRows*m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
        }

        if (m_iCursorY > 0)
        {
          m_iCursorY--;
        }
        else if (m_iCursorY == 0 && m_iOffset != 0)
        {
          m_iScrollCounter = m_iItemHeight;
          m_bScrollUp = true;
        }
        else
        {
          // move to the last item
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, m_vecItems.Count - 1, 0, null);
          OnMessage(msg);
        }
        OnSelectionChanged();
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
      }
    }

    void OnDown()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (m_bScrollDown)
        {
          m_bScrollDown = false;
          m_iOffset += m_iColumns;
          int iPage = m_iOffset/(m_iRows*m_iColumns);
          if ((m_iOffset%(m_iRows*m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
        }

        if (m_iCursorY + 1 == m_iRows)
        {
          m_iOffset += m_iColumns;
          if (!ValidItem(m_iCursorX, m_iCursorY))
          {
            m_iCursorX = 0;
            if (!ValidItem(m_iCursorX, m_iCursorY))
            {
              // move to the first item
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, 0, 0, null);
              OnMessage(msg);
            }
          }
          else
          {
            m_iOffset -= m_iColumns;
            m_iScrollCounter = m_iItemHeight;
            m_bScrollDown = true;
          }

          OnSelectionChanged();
          return;
        }
        else
        {
          if (ValidItem(m_iCursorX, m_iCursorY + 1))
          {
            m_iCursorY++;
          }
          else
          {
            if (ValidItem(0, m_iCursorY + 1))
            {
              m_iCursorX = 0;
              m_iCursorY++;
            }
            else
            {
              // move to the first item
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID, 0, 0, null);
              OnMessage(msg);
            }
          }
          OnSelectionChanged();
        }
      }
      else
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
      }
    }


    void RenderText(float fPosX, float fPosY, long dwTextColor, string wszText, bool bScroll)
    {
      float fwidth, fWidth = 0, fHeight = 0;
      float fMaxWidth = m_iItemWidth - (m_iItemWidth/10.0f);
      float fPosCX = fPosX;
      float fPosCY = fPosY;
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;
      if (fPosCY > GUIGraphicsContext.Height)
        fPosCY = (float) GUIGraphicsContext.Height;
      fHeight = 60.0f;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      if (fHeight <= 0) return;

      fwidth = fMaxWidth - 5.0f;

      if (fPosCX <= 0) fPosCX = 0;
      if (fPosCY <= 0) fPosCY = 0;
      int viewportMaxY = GUIGraphicsContext.DX9Device.Viewport.Height + GUIGraphicsContext.DX9Device.Viewport.Y;
      if (fPosCY + fHeight > viewportMaxY)
      {
        fHeight = (float) viewportMaxY - fPosCY;
      }
      Viewport newviewport = new Viewport();
      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = (int) fPosCX;
      newviewport.Y = (int) fPosCY;
      newviewport.Width = (int) (fwidth);
      newviewport.Height = (int) (fHeight);
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;

      if (!bScroll)
      {
        m_pFont.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth));
        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
        return;
      }

      float fTextHeight = 0, fTextWidth = 0;
      m_pFont.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);

      if (fTextWidth <= fMaxWidth)
      {
        m_pFont.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth));
        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
        return;
      }
      else
      {
        // scroll
        m_strBrackedText = wszText;
        m_strBrackedText += (" " + m_strSuffix + " ");
        m_pFont.GetTextExtent(m_strBrackedText, ref fTextWidth, ref fTextHeight);

        int iItem = m_iCursorX + m_iCursorY*m_iColumns + m_iOffset;
        if (fTextWidth > fMaxWidth)
        {
          fMaxWidth += 50.0f;
          m_strScrollText = "";
          if (iLastItem != iItem)
          {
            scroll_pos = 0;
            iLastItem = iItem;
            iScrollX = 0;
            iScrollOffset = 0.0f;
            iCurrentFrame = 0;
            timeElapsed = 0.0f;
            scrollContinuosly = false;
          }
          if ((iCurrentFrame > 25 + 12) || scrollContinuosly)
          {
            if (scrollContinuosly)
            {
              iScrollX = iCurrentFrame;
            }
            else
            {
              iScrollX = iCurrentFrame - (25 + 12);
            }
            char wTmp;
            if (scroll_pos >= m_strBrackedText.Length)
              wTmp = ' ';
            else
              wTmp = m_strBrackedText[scroll_pos];

            m_pFont.GetTextExtent(wTmp.ToString(), ref fWidth, ref fHeight);
            if (iScrollX - iScrollOffset >= fWidth)
            {
              ++scroll_pos;
              if (scroll_pos > m_strBrackedText.Length)
              {
                scroll_pos = 0;
                iScrollX = 0;
                iScrollOffset = 0.0f;
                iCurrentFrame = 0;
                timeElapsed = 0.0f;
                scrollContinuosly = true;
              }
              else
              {
                iScrollOffset += fWidth;
              }
            }
            int ipos = 0;
            for (int i = 0; i < m_strBrackedText.Length; i++)
            {
              if (i + scroll_pos < m_strBrackedText.Length)
                m_strScrollText += m_strBrackedText[i + scroll_pos];
              else
              {
                if (ipos == 0) m_strScrollText += ' ';
                else m_strScrollText += m_strBrackedText[ipos - 1];
                ipos++;
              }
            }
            if (fPosY >= 0.0)
            {
              //              m_pFont.DrawText((int) (fPosX - iScrollX + iScrollOffset), fPosY, dwTextColor, m_strScrollText, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth - 50f));
              m_pFont.DrawText((int) (fPosX - iScrollX + iScrollOffset), fPosY, dwTextColor, m_strScrollText, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth - 50f + iScrollX - iScrollOffset));
            }

          }
          else
          {
            if (fPosY >= 0.0)
              m_pFont.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT, (int) (fMaxWidth - 50f));
          }
        }
        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
      }
    }

    public string ScrollySuffix
    {
      get { return m_strSuffix; }
      set { m_strSuffix = value; }
    }


    void OnPageUp()
    {
      int iPage = m_upDown.Value;
      if (iPage > 1)
      {
        iPage--;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1)*m_iColumns*m_iRows;
        OnSelectionChanged();
      }
    }

    void OnPageDown()
    {
      int iItemsPerPage = m_iRows*m_iColumns;
      int iPages = m_vecItems.Count/iItemsPerPage;
      if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;

      int iPage = m_upDown.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1)*iItemsPerPage;
      }
      while (m_iCursorX > 0 && m_iOffset + m_iCursorY*m_iColumns + m_iCursorX >= m_vecItems.Count)
      {
        m_iCursorX--;
      }
      while (m_iCursorY > 0 && m_iOffset + m_iCursorY*m_iColumns + m_iCursorX >= m_vecItems.Count)
      {
        m_iCursorY--;
      }
      OnSelectionChanged();

    }


    public void SetTextureDimensions(int iWidth, int iHeight)
    {
      if (iWidth < 0 || iHeight < 0) return;
      m_iTextureWidth = iWidth;
      m_iTextureHeight = iHeight;

      if (m_button != null)
      {
        foreach (GUIButtonControl btn in m_button)
        {
          btn.Height = m_iTextureHeight;
          btn.Width = m_iTextureWidth;
          btn.DoUpdate();
        }
      }
    }

    public void SetThumbDimensions(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0 || iYpos < 0 || iWidth < 0 || iHeight < 0) return;
      m_iThumbWidth = iWidth;
      m_iThumbHeight = iHeight;
      m_iThumbXPos = iXpos;
      m_iThumbYPos = iYpos;
    }

    public void GetThumbDimensions(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iWidth = m_iThumbWidth;
      iHeight = m_iThumbHeight;
      iXpos = m_iThumbXPos;
      iYpos = m_iThumbYPos;
    }


    public int ItemWidth
    {
      get { return m_iItemWidth; }
      set
      {
        if (value < 0) return;

        m_iItemWidth = value;
        FreeResources();
        AllocResources();
      }
    }

    public int ItemHeight
    {
      get { return m_iItemHeight; }
      set
      {
        if (value < 0) return;
        m_iItemHeight = value;
        FreeResources();
        AllocResources();
      }
    }

    public bool ShowTexture
    {
      get { return m_bShowTexture; }
      set { m_bShowTexture = value; }
    }


    public int GetSelectedItem(ref string strLabel, ref string strLabel2, ref string strThumbnail)
    {
      strLabel = "";
      strLabel2 = "";
      strThumbnail = "";
      int iItem = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
      if (iItem >= 0 && iItem < m_vecItems.Count)
      {
        GUIListItem pItem = m_vecItems[iItem];
        strLabel = pItem.Label;
        strLabel2 = pItem.Label2;
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
        m_iItemWidth = m_iItemWidthBig;
        m_iItemHeight = m_iItemHeightBig;
        m_iTextureWidth = m_iTextureWidthBig;
        m_iTextureHeight = m_iTextureHeightBig;
        SetThumbDimensions(m_iThumbXPosBig, m_iThumbYPosBig, m_iThumbWidthBig, m_iThumbHeightBig);
        SetTextureDimensions(m_iTextureWidth, m_iTextureHeight);
      }
      else
      {
        m_iItemWidth = m_iItemWidthLow;
        m_iItemHeight = m_iItemHeightLow;
        m_iTextureWidth = m_iTextureWidthLow;
        m_iTextureHeight = m_iTextureHeightLow;
        SetThumbDimensions(m_iThumbXPosLow, m_iThumbYPosLow, m_iThumbWidthLow, m_iThumbHeightLow);
        SetTextureDimensions(m_iTextureWidth, m_iTextureHeight);
      }
      Calculate();
      m_bRefresh = true;
    }


    public void GetThumbDimensionsBig(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iXpos = m_iThumbXPosBig;
      iYpos = m_iThumbYPosBig;
      iWidth = m_iThumbWidthBig;
      iHeight = m_iThumbHeightBig;
    }

    public void GetThumbDimensionsLow(ref int iXpos, ref int iYpos, ref int iWidth, ref int iHeight)
    {
      iXpos = m_iThumbXPosLow;
      iYpos = m_iThumbYPosLow;
      iWidth = m_iThumbWidthLow;
      iHeight = m_iThumbHeightLow;
    }

    public long TextColor
    {
      get { return m_dwTextColor; }
    }

    public long SelectedColor
    {
      get { return m_dwSelectedColor; }
    }

    public string FontName
    {
      get { return m_strFontName; }
    }

    public int SpinWidth
    {
      get { return m_upDown.Width/2; }
    }

    public int SpinHeight
    {
      get { return m_upDown.Height; }
    }

    public string TexutureUpName
    {
      get { return m_upDown.TexutureUpName; }
    }

    public string TexutureDownName
    {
      get { return m_upDown.TexutureDownName; }
    }

    public string TexutureUpFocusName
    {
      get { return m_upDown.TexutureUpFocusName; }
    }

    public string TexutureDownFocusName
    {
      get { return m_upDown.TexutureDownFocusName; }
    }

    public long SpinTextColor
    {
      get { return m_upDown.TextColor; }
    }

    public int SpinX
    {
      get { return m_upDown.XPosition; }
    }

    public int SpinY
    {
      get { return m_upDown.YPosition; }
    }

    public int TextureWidth
    {
      get { return m_iTextureWidth; }
    }

    public int TextureHeight
    {
      get { return m_iTextureHeight; }
    }

    public string FocusName
    {
      get { return m_strImageFolderFocus; }
    }

    public string NoFocusName
    {
      get { return m_strImageFolder; }
    }


    public int TextureWidthBig
    {
      get { return m_iTextureWidthBig; }
      set
      {
        if (value < 0) return;
        m_iTextureWidthBig = value;
      }
    }

    public int TextureHeightBig
    {
      get { return m_iTextureHeightBig; }
      set
      {
        if (value < 0) return;
        m_iTextureHeightBig = value;
      }
    }

    public int ItemWidthBig
    {
      get { return m_iItemWidthBig; }
      set
      {
        if (value < 0) return;
        m_iItemWidthBig = value;
      }
    }

    public int ItemHeightBig
    {
      get { return m_iItemHeightBig; }
      set
      {
        if (value < 0) return;
        m_iItemHeightBig = value;
      }
    }

    public int TextureWidthLow
    {
      get { return m_iTextureWidthLow; }
      set
      {
        if (value < 0) return;
        m_iTextureWidthLow = value;
      }
    }

    public int TextureHeightLow
    {
      get { return m_iTextureHeightLow; }
      set
      {
        if (value < 0) return;
        m_iTextureHeightLow = value;
      }
    }

    public int ItemWidthLow
    {
      get { return m_iItemWidthLow; }
      set
      {
        if (value < 0) return;
        m_iItemWidthLow = value;
      }
    }

    public int ItemHeightLow
    {
      get { return m_iItemHeightLow; }
      set
      {
        if (value < 0) return;
        m_iItemHeightLow = value;
      }
    }

    public void SetThumbDimensionsLow(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0 || iYpos < 0 || iWidth < 0 || iHeight < 0) return;
      m_iThumbXPosLow = iXpos;
      m_iThumbYPosLow = iYpos;
      m_iThumbWidthLow = iWidth;
      m_iThumbHeightLow = iHeight;
    }

    public void SetThumbDimensionsBig(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0 || iYpos < 0 || iWidth < 0 || iHeight < 0) return;
      m_iThumbXPosBig = iXpos;
      m_iThumbYPosBig = iYpos;
      m_iThumbWidthBig = iWidth;
      m_iThumbHeightBig = iHeight;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      int id;
      bool focus;
      if (m_vertScrollbar.HitTest(x, y, out id, out focus)) return true;
      if (!base.HitTest(x, y, out id, out focus))
      {
        Focus = false;
        return false;
      }
      if (m_upDown.HitTest(x, y, out id, out focus))
      {
        if (m_upDown.GetMaximum() > 1)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_UPDOWN;
          m_upDown.Focus = true;
          if (!m_upDown.Focus)
          {
            m_iSelect = GUIListControl.ListType.CONTROL_LIST;
          }
          return true;
        }
        return true;
      }
      m_iSelect = GUIListControl.ListType.CONTROL_LIST;
      y -= m_dwPosY;
      x -= m_dwPosX;
      int iCursorY = (y/m_iItemHeight);
      int iCursorX = (x/m_iItemWidth);

      // Check item inside panel
      if (iCursorX < 0 || iCursorX >= m_iRows) return false;
      if (iCursorY < 0 || iCursorY >= m_iColumns) return false;
      m_iCursorY = iCursorY;
      m_iCursorX = iCursorX;

      while (m_iCursorX > 0 && m_iOffset + m_iCursorY*m_iColumns + m_iCursorX >= m_vecItems.Count)
      {
        m_iCursorX--;
      }
      while (m_iCursorY > 0 && m_iOffset + m_iCursorY*m_iColumns + m_iCursorX >= m_vecItems.Count)
      {
        m_iCursorY--;
      }
      OnSelectionChanged();

      return true;
    }

    public void Sort(System.Collections.Generic.IComparer<GUIListItem> comparer)
    {
      try
      {
        m_vecItems.Sort(comparer);
      }
      catch (Exception)
      {
      }
      m_bRefresh = true;
    }

    public override bool NeedRefresh()
    {
      if (m_bRefresh)
      {
        m_bRefresh = false;
        return true;
      }
      if (m_bScrollDown) return true;
      if (m_bScrollUp) return true;
      return false;
    }

    public GUIverticalScrollbar Scrollbar
    {
      get { return m_vertScrollbar; }
    }


	public override bool Focus
	{
		get { return IsFocused; }
		set { if(IsFocused != value) base.Focus = value; }
	}

    public void Add(GUIListItem item)
    {
      if (item == null) return;
      m_vecItems.Add(item);
      int iItemsPerPage = m_iRows*m_iColumns;
      int iPages = m_vecItems.Count/iItemsPerPage;
      if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;
      m_upDown.SetRange(1, iPages);
      m_upDown.Value = 1;
      m_bRefresh = true;
    }

    /// <summary>
    /// Gets the ID of the control.
    /// </summary>
    public override int GetID
    {
      get { return m_dwControlID; }
      set
      {
        m_dwControlID = value;
        if (m_upDown != null) m_upDown.ParentID = value;
      }
    }


    public override int WindowId
    {
      get { return m_iWindowID; }
      set
      {
        m_iWindowID = value;
        if (m_upDown != null) m_upDown.WindowId = value;
      }
    }

    public int Count
    {
      get { return m_vecItems.Count; }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (index < 0 || index >= m_vecItems.Count) return null;
        return m_vecItems[index];
      }
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        int iItem = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
        if (iItem >= 0 && iItem < m_vecItems.Count)
        {
          GUIListItem pItem = m_vecItems[iItem];
          return pItem;
        }
        return null;
      }
    }

    public int SelectedListItemIndex
    {
      get
      {
        int iItem = m_iOffset + m_iCursorY*m_iColumns + m_iCursorX;
        if (iItem >= 0 && iItem < m_vecItems.Count)
        {
          return iItem;
        }
        return -1;
      }
    }

    public void Clear()
    {
      m_vecItems.Clear();
      //GUITextureManager.CleanupThumbs();
      m_upDown.SetRange(1, 1);
      m_upDown.Value = 1;
      m_iCursorX = m_iCursorY = m_iOffset = 0;
      m_bRefresh = true;
      OnSelectionChanged();

    }
  }
}