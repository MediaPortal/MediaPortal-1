using System;
using System.Collections;
using System.Windows.Forms; // used for Keys definition
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIFilmstripControl : GUIControl
  {
    //TODO : add comments
    //TODO : use GUILabelControl for drawing text
    const int SLEEP_FRAME_COUNT = 2;
    const int THUMBNAIL_OVERSIZED_DIVIDER = 32;

    public enum SearchType
    {
      SEARCH_FIRST,
      SEARCH_PREV,
      SEARCH_NEXT
    } ;

    [XMLSkinElement("remoteColor")] protected long m_dwRemoteColor = 0xffff0000;
    [XMLSkinElement("downloadColor")] protected long m_dwDownloadColor = 0xff00ff00;
    [XMLSkinElement("thumbPosXBig")] protected int m_iThumbXPosBig = 0;
    [XMLSkinElement("thumbPosYBig")] protected int m_iThumbYPosBig = 0;
    [XMLSkinElement("thumbWidthBig")] protected int m_iThumbWidthBig = 0;
    [XMLSkinElement("thumbHeightBig")] protected int m_iThumbHeightBig = 0;

    [XMLSkinElement("imageFolder")] protected string m_strImageFolder = "";
    [XMLSkinElement("imageFolderFocus")] protected string m_strImageFolderFocus = "";

    [XMLSkinElement("textureUp")] protected string m_strUp = "";
    [XMLSkinElement("textureDown")] protected string m_strDown = "";
    [XMLSkinElement("textureUpFocus")] protected string m_strUpFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string m_strDownFocus = "";
    [XMLSkinElement("spinColor")] protected long m_dwSpinColor;
    [XMLSkinElement("spinHeight")] protected int m_dwSpinHeight;
    [XMLSkinElement("spinWidth")] protected int m_dwSpinWidth;
    [XMLSkinElement("spinPosX")] protected int m_dwSpinX;
    [XMLSkinElement("spinPosY")] protected int m_dwSpinY;
    [XMLSkinElement("itemHeight")] protected int m_iItemHeight;
    [XMLSkinElement("itemWidth")] protected int m_iItemWidth;
    [XMLSkinElement("textureHeight")] protected int m_iTextureHeightLow;
    [XMLSkinElement("textureWidth")] protected int m_iTextureWidthLow;
    [XMLSkinElement("thumbWidthBig")] protected int m_iTextureWidth = 80;
    [XMLSkinElement("thumbHeightBig")] protected int m_iTextureHeight = 80;
    [XMLSkinElement("thumbPosX")] protected int m_iThumbXPos = 8;
    [XMLSkinElement("thumbPosY")] protected int m_iThumbYPos = 8;
    [XMLSkinElement("font")] protected string m_strFontName = "";
    [XMLSkinElement("textcolor")] protected long m_dwTextColor = 0xFFFFFFFF;
    [XMLSkinElement("selectedColor")] protected long m_dwSelectedColor = 0xFFFFFFFF;

    [XMLSkinElement("scrollbarbg")] protected string m_strScrollBarBG = "";
    [XMLSkinElement("scrollbartop")] protected string m_strScrollBarTop = "";
    [XMLSkinElement("scrollbarbottom")] protected string m_strScrollBarBottom = "";


    [XMLSkinElement("backgroundheight")] protected int m_iBackgroundHeight;
    [XMLSkinElement("backgroundwidth")] protected int m_iBackgroundWidth;
    [XMLSkinElement("backgroundx")] protected int m_iBackgroundX;
    [XMLSkinElement("backgroundy")] protected int m_iBackgroundY;
    [XMLSkinElement("backgrounddiffuse")] protected int m_dwBackgroundDiffuse;
    [XMLSkinElement("background")] protected string m_strBackground;

    [XMLSkinElement("InfoImageheight")] protected int m_iInfoImageHeight;
    [XMLSkinElement("InfoImagewidth")] protected int m_iInfoImageWidth;
    [XMLSkinElement("InfoImagex")] protected int m_iInfoImageX;
    [XMLSkinElement("InfoImagey")] protected int m_iInfoImageY;
    [XMLSkinElement("InfoImagediffuse")] protected int m_dwInfoImageDiffuse;
    [XMLSkinElement("InfoImage")] protected string m_strInfoImage;

    int m_iItemHeightLow;
    int m_iItemWidthLow;
    int m_iItemHeightBig;
    int m_iItemWidthBig;
    int m_iTextureHeightBig;
    int m_iTextureWidthBig;
    bool m_bShowTexture = true;
    int m_iOffset = 0;
    GUIFont m_pFont = null;
    GUISpinControl m_upDown = null;
    GUIImage m_imgFolder = null;
    GUIImage m_imgFolderFocus = null;
    GUIListControl.ListType m_iSelect = GUIListControl.ListType.CONTROL_LIST;
    int m_iCursorX = 0;
    int m_iColumns;
    bool m_bScrollRight = false;
    bool m_bScrollLeft = false;
    int m_iScrollCounter = 0;
    int m_iSleeper = 0;
    string m_strSuffix = "|";

    int m_iThumbWidth = 64;
    int m_iThumbHeight = 64;

    int m_iThumbXPosLow = 0;
    int m_iThumbYPosLow = 0;
    int m_iThumbWidthLow = 0;
    int m_iThumbHeightLow = 0;

    ArrayList m_vecItems = new ArrayList();
    int scroll_pos = 0;
    int iScrollX = 0;
    int iLastItem = -1;
    int iFrames = 0;
    bool m_bRefresh = false;
    protected GUIverticalScrollbar m_horzScrollbar = null;
    protected string m_strBrackedText;
    protected string m_strScrollText;
    GUIImage m_backgroundImage;
    GUIImage m_infoImage;
    DateTime m_dateTimeIdle = DateTime.Now;
    bool m_bInfoChanged = false;
    string m_strNewInfoImage = "";

    protected double iScrollOffset = 0.0f;
    protected int iCurrentFrame = 0;
    protected double timeElapsed = 0.0f;
    protected bool scrollContinuosly = false;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeed)*0.01f); }
    }


    // Search            
    DateTime m_keyTimer = DateTime.Now;
    char m_CurrentKey = (char) 0;
    char m_PrevKey = (char) 0;
    protected string m_strSearchString = "";
    protected int m_iLastSearchItem = 0;

    public GUIFilmstripControl(int dwParentID) : base(dwParentID)
    {
    }

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
      if (m_dwPosY > m_dwPosY && m_dwSpinY < m_dwPosY + m_dwHeight) m_dwSpinY = m_dwPosY + m_dwHeight;
      m_imgFolder = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, m_iItemWidth, m_iItemHeight, m_strImageFolder, 0);
      m_imgFolderFocus = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, m_iItemWidth, m_iItemHeight, m_strImageFolderFocus, 0);
      m_upDown = new GUISpinControl(m_dwControlID, 0, m_dwSpinX, m_dwSpinY, m_dwSpinWidth, m_dwSpinHeight, m_strUp, m_strDown, m_strUpFocus, m_strDownFocus, m_strFontName, m_dwSpinColor, GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, GUIControl.Alignment.ALIGN_LEFT);
      m_pFont = GUIFontManager.GetFont(m_strFontName);
      int xpos = 5 + m_dwPosX + m_dwWidth;
      if (xpos + 15 > GUIGraphicsContext.Width) xpos = GUIGraphicsContext.Width - 15;
      m_horzScrollbar = new GUIverticalScrollbar(m_dwControlID, 0, 5 + m_dwPosX + m_dwWidth, m_dwPosY, 15, m_dwHeight, m_strScrollBarBG, m_strScrollBarTop, m_strScrollBarBottom);
      m_horzScrollbar.SendNotifies = false;
      m_upDown.Orientation = GUISpinControl.eOrientation.Horizontal;
      m_upDown.SetReverse(true);
      m_backgroundImage = new GUIImage(0, 0, m_iBackgroundX, m_iBackgroundY,
                                       m_iBackgroundWidth, m_iBackgroundHeight, m_strBackground, 0);
      m_infoImage = new GUIImage(0, 0, m_iInfoImageX, m_iInfoImageY,
                                 m_iInfoImageWidth, m_iInfoImageHeight, m_strInfoImage, 0);
      m_infoImage.Filtering = true;
      m_infoImage.KeepAspectRatio = true;
      m_infoImage.Centered = true;

      SetThumbDimensionsLow(m_iThumbXPos, m_iThumbYPos, m_iThumbWidth, m_iThumbHeight);
      SetTextureDimensions(m_iTextureWidth, m_iTextureHeight);

    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_dwSpinX, ref m_dwSpinY, ref m_dwSpinWidth, ref m_dwSpinHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextureWidth, ref m_iTextureHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iThumbXPos, ref m_iThumbYPos, ref m_iThumbWidth, ref m_iThumbHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextureWidthBig, ref m_iTextureHeightBig);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iThumbXPosBig, ref m_iThumbYPosBig, ref m_iThumbWidthBig, ref m_iThumbHeightBig);
      GUIGraphicsContext.ScalePosToScreenResolution(ref m_iItemWidthBig, ref m_iItemHeightBig);
      GUIGraphicsContext.ScalePosToScreenResolution(ref m_iItemWidth, ref m_iItemHeight);

      m_iInfoImageX += GUIGraphicsContext.OverScanLeft;
      m_iInfoImageY += GUIGraphicsContext.OverScanTop;

      m_iBackgroundX += GUIGraphicsContext.OverScanLeft;
      m_iBackgroundY += GUIGraphicsContext.OverScanTop;
      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iInfoImageX, ref m_iInfoImageY, ref m_iInfoImageWidth, ref m_iInfoImageHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iBackgroundX, ref m_iBackgroundY, ref m_iBackgroundWidth, ref m_iBackgroundHeight);
    }

    /// <summary>
    /// Method which is called if the user selected another item in the filmstrip
    /// This method will update the property manager with the properties of the
    /// newly selected item
    /// </summary>
    protected void OnSelectionChanged()
    {
      if (!IsVisible) return;

      // Reset searchstring
      if (m_iLastSearchItem != (m_iCursorX + m_iOffset))
      {
        m_PrevKey = (char) 0;
        m_CurrentKey = (char) 0;
        m_strSearchString = "";
      }

      string strSelected = "";
      string strSelected2 = "";
      string strThumb = "";
      int item = GetSelectedItem(ref strSelected, ref strSelected2, ref strThumb);
      GUIPropertyManager.SetProperty("#selecteditem", strSelected);
      GUIPropertyManager.SetProperty("#selecteditem2", strSelected2);
      GUIPropertyManager.SetProperty("#selectedthumb", strThumb);

      if (!IsVisible) return;
      if (item >= 0 && item < m_vecItems.Count)
      {
        GUIListItem listitem = m_vecItems[item] as GUIListItem;
        if (listitem != null) listitem.ItemSelected(this);
      }

      // ToDo: add searchstring property
      if (m_strSearchString.Length > 0)
        GUIPropertyManager.SetProperty("#selecteditem", "{" + m_strSearchString.ToLower() + "}");
    }


    /// <summary>
    /// Method to render a single item of the filmstrip
    /// </summary>
    /// <param name="bFocus">true if item shown be drawn focused, false for normal mode</param>
    /// <param name="dwPosX">x-coordinate of the item</param>
    /// <param name="dwPosY">y-coordinate of the item</param>
    /// <param name="pItem">item itself</param>
    void RenderItem(float timePassed, bool bFocus, int dwPosX, int dwPosY, GUIListItem pItem)
    {
      if (m_pFont == null) return;
      if (pItem == null) return;
      if (dwPosY < 0) return;
      if (m_imgFolderFocus == null) return;
      if (m_imgFolder == null) return;

      float fTextHeight = 0, fTextWidth = 0;
      m_pFont.GetTextExtent("W", ref fTextWidth, ref fTextHeight);

      float fTextPosY = (float) dwPosY + (float) m_iTextureHeight;

      long dwColor = m_dwTextColor;
      if (pItem.Selected) dwColor = m_dwSelectedColor;
      if (pItem.IsRemote)
      {
        dwColor = m_dwRemoteColor;
        if (pItem.IsDownloading) dwColor = m_dwDownloadColor;
      }

      if (bFocus == true && Focus && m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        m_imgFolderFocus.SetPosition(dwPosX, dwPosY);
        if (true == m_bShowTexture) m_imgFolderFocus.Render(timePassed);

        RenderText((float) dwPosX, fTextPosY, dwColor, pItem.Label, true);
      }
      else
      {
        m_imgFolder.SetPosition(dwPosX, dwPosY);
        if (true == m_bShowTexture) m_imgFolder.Render(timePassed);

        RenderText((float) dwPosX, fTextPosY, dwColor, pItem.Label, false);

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
        if (null == pImage && m_iSleeper == 0 && !IsAnimating)
        {
          pImage = new GUIImage(0, 0, m_iThumbXPos - iOverSized + dwPosX, m_iThumbYPos - iOverSized + dwPosY, m_iThumbWidth + 2*iOverSized, m_iThumbHeight + 2*iOverSized, pItem.ThumbnailImage, 0x0);
          pImage.KeepAspectRatio = true;
          pImage.ZoomFromTop = !pItem.IsFolder;
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
          pImage.ZoomFromTop = !pItem.IsFolder;
          pImage.Width = m_iThumbWidth + 2*iOverSized;
          pImage.Height = m_iThumbHeight + 2*iOverSized;
          int xOff = (m_iThumbWidth + 2*iOverSized - pImage.RenderWidth)/2;
          int yOff = (m_iThumbHeight + 2*iOverSized - pImage.RenderHeight)/2;
          pImage.SetPosition(m_iThumbXPos + dwPosX - iOverSized + xOff, m_iThumbYPos - iOverSized + dwPosY + yOff);
          pImage.Render(timePassed);
        }
      }
      else
      {
        if (pItem.HasIconBig)
        {
          GUIImage pImage = pItem.IconBig;
          if (null == pImage && m_iSleeper == 0 && !IsAnimating)
          {
            pImage = new GUIImage(0, 0, m_iThumbXPos - iOverSized + dwPosX, m_iThumbYPos - iOverSized + dwPosY, m_iThumbWidth + 2*iOverSized, m_iThumbHeight + 2*iOverSized, pItem.IconImageBig, 0x0);
            pImage.KeepAspectRatio = true;
            pImage.ZoomFromTop = !pItem.IsFolder;
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
            pImage.ZoomFromTop = !pItem.IsFolder;
            pImage.Width = m_iThumbWidth + 2*iOverSized;
            pImage.Height = m_iThumbHeight + 2*iOverSized;
            int xOff = (m_iThumbWidth + 2*iOverSized - pImage.RenderWidth)/2;
            int yOff = (m_iThumbHeight + 2*iOverSized - pImage.RenderHeight)/2;
            pImage.SetPosition(m_iThumbXPos - iOverSized + dwPosX + xOff, m_iThumbYPos - iOverSized + dwPosY + yOff);
            pImage.Render(timePassed);
          }
        }
      }
    }

    /// <summary>
    /// The render method. 
    /// This method will draw the entire filmstrip
    /// </summary>
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
      if ((m_iCursorX > 0) && !ValidItem(m_iCursorX))
      {
        m_iCursorX = 0;
        OnSelectionChanged();
      }

      if (m_iSleeper > 0) m_iSleeper--;

      UpdateInfoImage();
      if (m_backgroundImage != null)
      {
        m_backgroundImage.Render(timePassed);
      }

      if (m_horzScrollbar != null)
      {
        float fPercent = (float) m_iOffset + m_iCursorX;
        fPercent /= (float) (m_vecItems.Count);
        fPercent *= 100.0f;
        if ((int) fPercent != (int) m_horzScrollbar.Percentage)
        {
          m_horzScrollbar.Percentage = fPercent;
        }
      }

      int iScrollXOffset = 0;
      if (true == m_bScrollRight)
      {
        iScrollXOffset = - (m_iItemWidth - m_iScrollCounter);
      }
      if (true == m_bScrollLeft)
      {
        iScrollXOffset = m_iItemWidth - m_iScrollCounter;
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
      view.Height = m_iItemHeight;
      view.MinZ = 0.0f;
      view.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = view;

      int iStartItem = 30000;
      int iEndItem = -1;
      dwPosY = m_dwPosY;
      int dwPosX;

      //are we scrolling towards the left?
      if (m_bScrollLeft)
      {
        // yes, then render item on right
        dwPosX = m_dwPosX - m_iItemWidth + iScrollXOffset;
        int iItem = m_iOffset - 1;
        if (iItem >= 0 && iItem < m_vecItems.Count)
        {
          if (iItem < iStartItem) iStartItem = iItem;
          if (iItem > iEndItem) iEndItem = iItem;

          GUIListItem pItem = (GUIListItem) m_vecItems[iItem];
          RenderItem(timePassed, false, dwPosX, dwPosY, pItem);
        }
      }

      // render main panel
      for (int iCol = 0; iCol < m_iColumns; iCol++)
      {
        dwPosX = m_dwPosX + iCol*m_iItemWidth + iScrollXOffset;
        int iItem = iCol + m_iOffset;
        if (iItem >= 0 && iItem < m_vecItems.Count)
        {
          if (iItem < iStartItem) iStartItem = iItem;
          if (iItem > iEndItem) iEndItem = iItem;

          GUIListItem pItem = (GUIListItem) m_vecItems[iItem];
          bool bFocus = (m_iCursorX == iCol);
          RenderItem(timePassed, bFocus, dwPosX, dwPosY, pItem);
        }
      }


      //are we scrolling towards the right?
      if (m_bScrollRight)
      {
        // yes, then render the new left item
        dwPosX = m_dwPosX + m_iColumns*m_iItemWidth + iScrollXOffset;
        m_iOffset ++;
        int iItem = m_iColumns + m_iOffset - 1;
        if (iItem >= 0 && iItem < m_vecItems.Count)
        {
          if (iItem < iStartItem) iStartItem = iItem;
          if (iItem > iEndItem) iEndItem = iItem;

          GUIListItem pItem = (GUIListItem) m_vecItems[iItem];
          RenderItem(timePassed, false, dwPosX, dwPosY, pItem);
        }
        m_iOffset --;
      }
      GUIGraphicsContext.DX9Device.Viewport = oldview;

      //
      iFrames = 12;
      int iStep = m_iItemHeight/iFrames;
      if (0 == iStep) iStep = 1;
      if (m_bScrollLeft)
      {
        m_iScrollCounter -= iStep;
        if (m_iScrollCounter <= 0)
        {
          m_bScrollLeft = false;
          m_iOffset --;
          int iPage = m_iOffset/(m_iColumns);
          if ((m_iOffset%(m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
          m_bRefresh = true;
          OnSelectionChanged();
        }
      }

      if (m_bScrollRight)
      {
        m_iScrollCounter -= iStep;
        if (m_iScrollCounter <= 0)
        {
          m_bScrollRight = false;
          m_iOffset ++;
          int iPage = m_iOffset/(m_iColumns);
          if ((m_iOffset%(m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
          m_bRefresh = true;
          OnSelectionChanged();
        }
      }

      if (m_infoImage != null)
      {
        m_infoImage.Render(timePassed);
      }
      //free memory
      if (iStartItem < 30000)
      {
        for (int i = 0; i < iStartItem; ++i)
        {
          if (i >= 0 && i < m_vecItems.Count)
          {
            GUIListItem pItem = m_vecItems[i] as GUIListItem;
            if (null != pItem)
            {
              pItem.FreeMemory();
            }
          }
        }
      }

      for (int i = iEndItem + 1; i < m_vecItems.Count; ++i)
      {
        if (i >= 0 && i < m_vecItems.Count)
        {
          GUIListItem pItem = m_vecItems[i] as GUIListItem;
          if (null != pItem)
          {
            pItem.FreeMemory();
          }
        }
      }

      dwPosY = m_dwPosY + (m_iItemHeight);

      if (m_upDown != null) m_upDown.Render(timePassed);
      if (m_bScrollLeft || m_bScrollRight)
      {
        m_bRefresh = true;
      }
      int iItemsPerPage = m_iColumns;
      if (m_vecItems.Count > iItemsPerPage && m_horzScrollbar != null)
      {
        m_horzScrollbar.Render(timePassed);
      }
      if (Focus)
        GUIPropertyManager.SetProperty("#highlightedbutton", String.Empty);
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          m_bRefresh = true;
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          m_bRefresh = true;
          break;

        case Action.ActionType.ACTION_HOME:
          {
            m_iOffset = 0;
            m_iCursorX = 0;
            m_upDown.Value = 1;
            OnSelectionChanged();

            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_END:
          {
            int iItem = m_vecItems.Count - 1;
            if (iItem >= 0)
            {
              int iPage = 1;
              m_iCursorX = 0;
              m_iOffset = 0;
              while (iItem >= (m_iColumns))
              {
                m_iOffset += (m_iColumns);
                iItem -= (m_iColumns);
                iPage++;
              }
              if (m_upDown != null) m_upDown.Value = iPage;
              m_iCursorX = iItem;
              OnSelectionChanged();
            }
            m_bRefresh = true;
          }
          break;


        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            OnDown();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (!OnUp())
            {
              base.OnAction(action);
              return;
            }
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (m_strSearchString != "")
              SearchItem(m_strSearchString, SearchType.SEARCH_PREV);
            else
              OnLeft();
            m_bRefresh = true;
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (m_strSearchString != "")
              SearchItem(m_strSearchString, SearchType.SEARCH_NEXT);
            else
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
            if (m_horzScrollbar != null)
            {
              if (m_horzScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
              {
                ///TODO: add horz. scrollbar to filmstrip
              }
              //          m_bDrawFocus=true;
            }
          }
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          {
            int id;
            bool focus;
            if (m_horzScrollbar != null)
            {
              if (m_horzScrollbar.HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
              {
                ///TODO: add horz. scrollbar to filmstrip
                return;
              }
            }

            //m_bDrawFocus=true;
            if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int) Action.ActionType.ACTION_SELECT_ITEM, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
            else
            {
              if (m_upDown != null) m_upDown.OnAction(action);
            }
            m_bRefresh = true;
          }
          break;

        default:
          {
            if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int) action.wID, 0, null);
              GUIGraphicsContext.SendMessage(msg);
              m_bRefresh = true;
            }
            else
            {
              if (m_upDown != null) m_upDown.OnAction(action);
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
            m_iOffset = (m_upDown.Value - 1)*(m_iColumns);
            m_bRefresh = true;
            OnSelectionChanged();
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = m_iOffset + m_iCursorX;
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

        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in m_vecItems)
          {
            item.FreeMemory();
          }
          m_bRefresh = true;
          if (m_infoImage != null)
          {
            m_infoImage.FreeResources();
            m_infoImage.AllocResources();
            m_infoImage.DoUpdate();
          }
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
          if (newItem != null)
          {
            m_vecItems.Add(newItem);
            int iItemsPerPage = m_iColumns;
            int iPages = m_vecItems.Count/iItemsPerPage;
            if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;
            if (m_upDown != null)
            {
              m_upDown.SetRange(1, iPages);
              m_upDown.Value = 1;
            }
            m_bRefresh = true;
          }
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
          message.Param1 = m_iOffset + m_iCursorX;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < m_vecItems.Count)
          {
            int iPage = 1;
            m_iCursorX = 0;
            m_iOffset = 0;
            while (iItem >= (m_iColumns))
            {
              m_iOffset += (m_iColumns);
              iItem -= (m_iColumns);
              iPage++;
            }
            if (m_upDown != null) m_upDown.Value = iPage;
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
      int iCurrentItem = m_iOffset + m_iCursorX;
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

        GUIListItem pItem = (GUIListItem) m_vecItems[iItem];
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
        int iPage = 1;
        m_iCursorX = 0;
        m_iOffset = 0;
        while (iItem >= (m_iColumns))
        {
          m_iOffset += (m_iColumns);
          iItem -= (m_iColumns);
          iPage++;
        }
        if (m_upDown != null) m_upDown.Value = iPage;
        m_iCursorX = iItem;
      }

      m_iLastSearchItem = m_iCursorX + m_iOffset;
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

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      if (m_upDown != null) m_upDown.PreAllocResources();
      if (m_imgFolder != null) m_imgFolder.PreAllocResources();
      if (m_imgFolderFocus != null) m_imgFolderFocus.PreAllocResources();
      if (m_horzScrollbar != null) m_horzScrollbar.PreAllocResources();
      if (m_backgroundImage != null) m_backgroundImage.PreAllocResources();
      if (m_infoImage != null) m_infoImage.PreAllocResources();
      m_dateTimeIdle = DateTime.Now;
      m_bInfoChanged = false;
      m_strNewInfoImage = "";
      m_iOffset = 0;
      m_iCursorX = 0;
    }


    void Calculate()
    {
      if (m_imgFolder != null)
      {
        m_imgFolder.Width = m_iTextureWidth;
        m_imgFolder.Height = m_iTextureHeight;
      }

      if (m_imgFolderFocus != null)
      {
        m_imgFolderFocus.Width = m_iTextureWidth;
        m_imgFolderFocus.Height = m_iTextureHeight;
      }

      float fWidth = 0, fHeight = 0;
      if (m_pFont != null)
      {
        // height of 1 item = folder image height + text row height + space in between
        m_pFont.GetTextExtent("y", ref fWidth, ref fHeight);
      }

      fWidth = (float) m_iItemWidth;
      fHeight = (float) m_iItemHeight;

      m_iColumns = (int) (m_dwWidth/fWidth);

      int iItemsPerPage = m_iColumns;
      int iPages = m_vecItems.Count/iItemsPerPage;
      if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;
      if (m_upDown != null)
      {
        m_upDown.SetRange(1, iPages);
        m_upDown.Value = 1;
      }
    }

    public override void AllocResources()
    {
      m_iSleeper = 0;

      base.AllocResources();
      m_pFont = GUIFontManager.GetFont(m_strFontName);
      if (m_backgroundImage != null) m_backgroundImage.AllocResources();
      if (m_infoImage != null) m_infoImage.AllocResources();
      if (m_upDown != null) m_upDown.AllocResources();
      if (m_imgFolder != null) m_imgFolder.AllocResources();
      if (m_imgFolderFocus != null) m_imgFolderFocus.AllocResources();
      if (m_horzScrollbar != null) m_horzScrollbar.AllocResources();
      Calculate();

      m_upDown.ParentID = GetID;
    }

    public override void FreeResources()
    {
      foreach (GUIListItem item in m_vecItems)
      {
        item.FreeIcons();
      }
      m_vecItems.Clear();
      base.FreeResources();
      if (m_backgroundImage != null) m_backgroundImage.FreeResources();
      if (m_infoImage != null) m_infoImage.FreeResources();
      if (m_upDown != null) m_upDown.FreeResources();
      if (m_imgFolder != null) m_imgFolder.FreeResources();
      if (m_imgFolderFocus != null) m_imgFolderFocus.FreeResources();
      if (m_horzScrollbar != null) m_horzScrollbar.FreeResources();
    }


    bool ValidItem(int iX)
    {
      if (iX >= m_iColumns) return false;
      if (m_iOffset + iX < m_vecItems.Count) return true;
      return false;
    }


    void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        if (m_bScrollRight)
        {
          m_bScrollRight = false;
          m_iOffset ++;
          int iPage = m_iOffset/(m_iColumns);
          if ((m_iOffset%(m_iColumns)) != 0) iPage++;
          if (m_upDown != null) m_upDown.Value = iPage + 1;
        }

        if (m_iCursorX + 1 == m_iColumns)
        {
          m_iOffset ++;
          if (!ValidItem(m_iCursorX))
          {
            m_iOffset --;
          }
          else
          {
            m_iOffset --;
            m_iScrollCounter = m_iItemWidth;
            m_bScrollRight = true;
          }

          OnSelectionChanged();
          return;
        }
        else
        {
          if (ValidItem(m_iCursorX + 1))
          {
            m_iCursorX++;
          }
          OnSelectionChanged();
        }
      }
      else if (m_upDown != null)
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
        if (m_bScrollLeft)
        {
          m_iScrollCounter = 0;
          m_bScrollLeft = false;
          m_iOffset --;
          int iPage = m_iOffset/(m_iColumns);
          if ((m_iOffset%(m_iColumns)) != 0) iPage++;
          if (m_upDown != null) m_upDown.Value = iPage + 1;
        }

        if (m_iCursorX > 0)
        {
          m_iCursorX--;
        }
        else if (m_iCursorX == 0 && m_iOffset != 0)
        {
          m_iScrollCounter = m_iItemWidth;
          m_bScrollLeft = true;
        }
        else
        {
          base.OnAction(action);
        }
        OnSelectionChanged();
      }
      else if (m_upDown != null)
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
        OnSelectionChanged();
      }
    }

    bool OnUp()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        return false;
      }
      else if (m_upDown != null)
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          m_iSelect = GUIListControl.ListType.CONTROL_LIST;
        }
      }
      return true;
    }

    void OnDown()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        m_iSelect = GUIListControl.ListType.CONTROL_UPDOWN;
        if (m_upDown != null) m_upDown.Focus = true;
      }
      else if (m_upDown != null)
      {
        m_upDown.OnAction(action);
        if (!m_upDown.Focus)
        {
          base.OnAction(action);
        }
      }
    }


    void RenderText(float fPosX, float fPosY, long dwTextColor, string wszText, bool bScroll)
    {
      if (m_pFont == null) return;
      if (wszText == null) return;
      if (wszText == String.Empty) return;

      float fTextHeight = 0, fTextWidth = 0;
      float fwidth, fWidth = 0, fHeight = 0;
      m_pFont.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);
      float fMaxWidth = m_iItemWidth - m_iItemWidth/10.0f;
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
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;

      int minX = m_dwPosX;
      if (fPosCX < minX) fPosCX = minX;
      int maxX = m_dwPosX + m_iColumns*m_iItemWidth;
      if (fwidth + fPosCX > maxX)
        fwidth = ((float) maxX) - fPosCX;
      Viewport newviewport = new Viewport();
      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = (int) fPosCX;
      newviewport.Y = (int) fPosCY;
      newviewport.Width = (int) (fwidth);
      newviewport.Height = (int) (fHeight);
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;

      if (!bScroll || fTextWidth <= fMaxWidth)
      {
        m_pFont.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT, (int) fMaxWidth);

        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
        return;
      }
      else
      {
        // scroll
        m_strBrackedText = wszText;
        m_strBrackedText += (" " + m_strSuffix + " ");
        m_pFont.GetTextExtent(m_strBrackedText, ref fTextWidth, ref fTextHeight);

        int iItem = m_iCursorX + m_iOffset;
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
          //          if (iStartFrame > 50)
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
      set
      {
        if (value == null) return;
        m_strSuffix = value;
      }
    }


    void OnPageUp()
    {
      if (m_upDown == null) return;
      int iPage = m_upDown.Value;
      if (iPage > 1)
      {
        iPage--;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1)*m_iColumns;
        OnSelectionChanged();
      }
    }

    void OnPageDown()
    {
      if (m_upDown == null) return;
      int iItemsPerPage = m_iColumns;
      int iPages = m_vecItems.Count/iItemsPerPage;
      if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;

      int iPage = m_upDown.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1)*iItemsPerPage;
      }
      while (m_iCursorX > 0 && m_iOffset + m_iCursorX >= m_vecItems.Count)
      {
        m_iCursorX--;
      }
      OnSelectionChanged();

    }


    public void SetTextureDimensions(int iWidth, int iHeight)
    {
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      m_iTextureWidth = iWidth;
      m_iTextureHeight = iHeight;


      if (m_imgFolder != null)
      {
        m_imgFolder.Height = m_iTextureHeight;
        m_imgFolder.Width = m_iTextureWidth;
        m_imgFolder.Refresh();
      }

      if (m_imgFolderFocus != null)
      {
        m_imgFolderFocus.Width = m_iTextureWidth;
        m_imgFolderFocus.Height = m_iTextureHeight;
        m_imgFolderFocus.Refresh();
      }

    }

    public void SetThumbDimensions(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0) return;
      if (iYpos < 0) return;
      if (iWidth < 0) return;
      if (iHeight < 0) return;
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
        if (m_iItemWidth < 1) m_iItemWidth = 1;
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
        if (m_iItemHeight < 1) m_iItemHeight = 1;
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
      int iItem = m_iOffset + m_iCursorX;
      if (iItem >= 0 && iItem < m_vecItems.Count)
      {
        GUIListItem pItem = m_vecItems[iItem] as GUIListItem;
        if (pItem != null)
        {
          strLabel = pItem.Label;
          strLabel2 = pItem.Label2;
          if (pItem.IsFolder)
          {
            strLabel = String.Format("[{0}]", pItem.Label);
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
      get
      {
        if (m_upDown == null) return 0;
        return m_upDown.Width/2;
      }
    }

    public int SpinHeight
    {
      get
      {
        if (m_upDown == null) return 0;
        return m_upDown.Height;
      }
    }

    public string TexutureUpName
    {
      get
      {
        if (m_upDown == null) return String.Empty;
        return m_upDown.TexutureUpName;
      }
    }

    public string TexutureDownName
    {
      get
      {
        if (m_upDown == null) return String.Empty;
        return m_upDown.TexutureDownName;
      }
    }

    public string TexutureUpFocusName
    {
      get
      {
        if (m_upDown == null) return String.Empty;
        return m_upDown.TexutureUpFocusName;
      }
    }

    public string TexutureDownFocusName
    {
      get
      {
        if (m_upDown == null) return String.Empty;
        return m_upDown.TexutureDownFocusName;
      }
    }

    public long SpinTextColor
    {
      get
      {
        if (m_upDown == null) return 0;
        return m_upDown.TextColor;
      }
    }

    public int SpinX
    {
      get
      {
        if (m_upDown == null) return 0;
        return m_upDown.XPosition;
      }
    }

    public int SpinY
    {
      get
      {
        if (m_upDown == null) return 0;
        return m_upDown.YPosition;
      }
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
      get
      {
        if (m_imgFolderFocus == null) return String.Empty;
        return m_imgFolderFocus.FileName;
      }
    }

    public string NoFocusName
    {
      get
      {
        if (m_imgFolder == null) return String.Empty;
        return m_imgFolder.FileName;
      }
    }


    public int TextureWidthBig
    {
      get { return m_iTextureWidthBig; }
      set
      {
        if (value < 0) return;
        m_iTextureWidthBig = value;
        if (m_iTextureWidthBig < 0) m_iTextureWidthBig = 0;
      }
    }

    public int TextureHeightBig
    {
      get { return m_iTextureHeightBig; }
      set
      {
        if (value < 0) return;
        m_iTextureHeightBig = value;
        if (m_iTextureHeightBig < 0) m_iTextureHeightBig = 0;
      }
    }

    public int ItemWidthBig
    {
      get { return m_iItemWidthBig; }
      set
      {
        if (value < 0) return;
        m_iItemWidthBig = value;
        if (m_iItemWidthBig < 0) m_iItemWidthBig = 0;
      }
    }

    public int ItemHeightBig
    {
      get { return m_iItemHeightBig; }
      set
      {
        if (value < 0) return;
        m_iItemHeightBig = value;
        if (m_iItemHeightBig < 0) m_iItemHeightBig = 0;
      }
    }

    public int TextureWidthLow
    {
      get { return m_iTextureWidthLow; }
      set
      {
        if (value < 0) return;
        m_iTextureWidthLow = value;
        if (m_iTextureWidthLow < 0) m_iTextureWidthLow = 0;
      }
    }

    public int TextureHeightLow
    {
      get { return m_iTextureHeightLow; }
      set
      {
        if (value < 0) return;
        m_iTextureHeightLow = value;
        if (m_iTextureHeightLow < 0) m_iTextureHeightLow = 0;
      }
    }

    public int ItemWidthLow
    {
      get { return m_iItemWidthLow; }
      set
      {
        if (value < 0) return;
        m_iItemWidthLow = value;
        if (m_iItemWidthLow < 0) m_iItemWidthLow = 0;
      }
    }

    public int ItemHeightLow
    {
      get { return m_iItemHeightLow; }
      set
      {
        if (value < 0) return;
        m_iItemHeightLow = value;
        if (m_iItemHeightLow < 0) m_iItemHeightLow = 0;
      }
    }

    public void SetThumbDimensionsLow(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0) return;
      if (iYpos < 0) return;
      if (iWidth < 0) return;
      if (iHeight < 0) return;
      m_iThumbXPosLow = iXpos;
      m_iThumbYPosLow = iYpos;
      m_iThumbWidthLow = iWidth;
      m_iThumbHeightLow = iHeight;
    }

    public void SetThumbDimensionsBig(int iXpos, int iYpos, int iWidth, int iHeight)
    {
      if (iXpos < 0) return;
      if (iYpos < 0) return;
      if (iWidth < 0) return;
      if (iHeight < 0) return;
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
      if (m_horzScrollbar != null)
      {
        if (m_horzScrollbar.HitTest(x, y, out id, out focus)) return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        Focus = false;
        if (m_upDown != null)
        {
          if (!m_upDown.HitTest(x, y, out id, out focus))
            return false;
        }
      }

      if (m_upDown != null)
      {
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
      }

      m_iSelect = GUIListControl.ListType.CONTROL_LIST;
      y -= m_dwPosY;
      x -= m_dwPosX;
      m_iCursorX = (x/m_iItemWidth);

      while (m_iCursorX > 0 && m_iOffset + m_iCursorX >= m_vecItems.Count)
      {
        m_iCursorX--;
      }
      OnSelectionChanged();

      return true;
    }

    public void Sort(System.Collections.IComparer comparer)
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
      if (m_bScrollLeft) return true;
      if (m_bScrollRight) return true;
      return false;
    }

    public GUIverticalScrollbar Scrollbar
    {
      get { return m_horzScrollbar; }
    }


    public override bool Focus
    {
      get { return m_bHasFocus; }
      set
      {
        if (m_bHasFocus != value)
        {
          m_bHasFocus = value;
          //m_bDrawFocus=true;
        }
      }
    }

    public void Add(GUIListItem item)
    {
      if (item == null) return;
      m_vecItems.Add(item);
      int iItemsPerPage = m_iColumns;
      int iPages = m_vecItems.Count/iItemsPerPage;
      if ((m_vecItems.Count%iItemsPerPage) != 0) iPages++;
      if (m_upDown != null)
      {
        m_upDown.SetRange(1, iPages);
        m_upDown.Value = 1;
      }
      m_bRefresh = true;
    }

    public int BackgroundX
    {
      get { return m_iBackgroundX; }
      set
      {
        if (value < 0) return;
        m_iBackgroundX = value;
        if (m_backgroundImage != null) m_backgroundImage.XPosition = value;
      }
    }

    public int BackgroundY
    {
      get { return m_iBackgroundY; }
      set
      {
        if (value < 0) return;
        m_iBackgroundY = value;
        if (m_backgroundImage != null) m_backgroundImage.YPosition = value;
      }
    }

    public int BackgroundWidth
    {
      get { return m_iBackgroundWidth; }
      set
      {
        if (value < 0) return;
        m_iBackgroundWidth = value;
        if (m_backgroundImage != null) m_backgroundImage.Width = value;
      }
    }

    public int BackgroundHeight
    {
      get { return m_iBackgroundHeight; }
      set
      {
        if (value < 0) return;
        m_iBackgroundHeight = value;
        if (m_backgroundImage != null) m_backgroundImage.Height = value;
      }
    }

    public long BackgroundDiffuse
    {
      get
      {
        if (m_backgroundImage != null) return m_backgroundImage.ColourDiffuse;
        return 0;
      }
      set { if (m_backgroundImage != null) m_backgroundImage.ColourDiffuse = value; }
    }

    public string BackgroundFileName
    {
      get
      {
        if (m_backgroundImage != null) return m_backgroundImage.FileName;
        return String.Empty;
      }
      set
      {
        if (value == null) return;
        if (m_backgroundImage != null) m_backgroundImage.SetFileName(value);
      }
    }


    public int InfoImageX
    {
      get { return m_iInfoImageX; }
      set
      {
        if (value < 0) return;
        m_iInfoImageX = value;
        if (m_infoImage != null) m_infoImage.XPosition = value;
      }
    }

    public int InfoImageY
    {
      get { return m_iInfoImageY; }
      set
      {
        if (value < 0) return;
        m_iInfoImageY = value;
        if (m_infoImage != null) m_infoImage.YPosition = value;
      }
    }

    public int InfoImageWidth
    {
      get { return m_iInfoImageWidth; }
      set
      {
        if (value < 0) return;
        m_iInfoImageWidth = value;
        if (m_infoImage != null) m_infoImage.Width = value;
      }
    }

    public int InfoImageHeight
    {
      get { return m_iInfoImageHeight; }
      set
      {
        if (value < 0) return;
        m_iInfoImageHeight = value;
        if (m_infoImage != null) m_infoImage.Height = value;
      }
    }

    public long InfoImageDiffuse
    {
      get
      {
        if (m_infoImage != null)
          return m_infoImage.ColourDiffuse;
        return 0;
      }
      set { if (m_infoImage != null) m_infoImage.ColourDiffuse = value; }
    }

    public string InfoImageFileName
    {
      get
      {
        if (m_infoImage != null)
          return m_infoImage.FileName;
        return String.Empty;
      }
      set
      {
        if (value == null) return;
        m_strNewInfoImage = value;
        m_bInfoChanged = true;
        m_dateTimeIdle = DateTime.Now;
      }
    }

    void UpdateInfoImage()
    {
      if (m_infoImage == null) return;
      if (m_bInfoChanged)
      {
        TimeSpan ts = DateTime.Now - m_dateTimeIdle;
        if (ts.TotalMilliseconds > 500)
        {
          m_infoImage.SetFileName(m_strNewInfoImage);
          m_strNewInfoImage = "";
          m_bInfoChanged = false;
        }
      }
    }


    /// <summary>
    /// Method to store(save) the current control rectangle
    /// </summary>
    public override void StorePosition()
    {
      if (m_infoImage != null) m_infoImage.StorePosition();
      if (m_upDown != null) m_upDown.StorePosition();
      if (m_backgroundImage != null) m_backgroundImage.StorePosition();
      if (m_imgFolder != null) m_imgFolder.StorePosition();
      if (m_imgFolderFocus != null) m_imgFolderFocus.StorePosition();
      if (m_horzScrollbar != null) m_horzScrollbar.StorePosition();

      base.StorePosition();
    }

    /// <summary>
    /// Method to restore the saved-current control rectangle
    /// </summary>
    public override void ReStorePosition()
    {
      if (m_infoImage != null) m_infoImage.ReStorePosition();
      if (m_upDown != null) m_upDown.ReStorePosition();
      if (m_backgroundImage != null) m_backgroundImage.ReStorePosition();
      if (m_imgFolder != null) m_imgFolder.ReStorePosition();
      if (m_imgFolderFocus != null) m_imgFolderFocus.ReStorePosition();
      if (m_horzScrollbar != null) m_horzScrollbar.ReStorePosition();

      if (m_infoImage != null) m_infoImage.GetRect(out m_iInfoImageX, out m_iInfoImageY, out m_iInfoImageWidth, out m_iInfoImageHeight);
      if (m_backgroundImage != null) m_backgroundImage.GetRect(out m_iBackgroundX, out m_iBackgroundY, out m_iBackgroundWidth, out m_iBackgroundHeight);
      if (m_upDown != null) m_upDown.GetRect(out m_dwSpinX, out m_dwSpinY, out m_dwSpinWidth, out m_dwSpinHeight);

      base.ReStorePosition();
    }

    /// <summary>
    /// Method to get animate the current control
    /// </summary>
    public override void Animate(float timePassed, Animator animator)
    {
      if (animator == null) return;
      if (m_infoImage != null) m_infoImage.Animate(timePassed, animator);
      if (m_upDown != null) m_upDown.Animate(timePassed, animator);
      if (m_backgroundImage != null) m_backgroundImage.Animate(timePassed, animator);
      base.Animate(timePassed, animator);
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

    public int Count
    {
      get { return m_vecItems.Count; }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (index < 0 || index >= m_vecItems.Count) return null;
        return (GUIListItem) m_vecItems[index];
      }
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        int iItem = m_iOffset + m_iCursorX;
        if (iItem >= 0 && iItem < m_vecItems.Count)
        {
          GUIListItem pItem = (GUIListItem) m_vecItems[iItem];
          return pItem;
        }
        return null;
      }
    }

    public int SelectedListItemIndex
    {
      get
      {
        int iItem = m_iOffset + m_iCursorX;
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
      m_iCursorX = m_iOffset = 0;
      m_bRefresh = true;
      OnSelectionChanged();
    }
  }
}