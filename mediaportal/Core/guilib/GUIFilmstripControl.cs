using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
	public class GUIFilmstripControl : GUIControl
	{
    const int SLEEP_FRAME_COUNT=2;
		int                   m_iItemHeightLow;
		int                   m_iItemWidthLow;


		int                   m_iItemHeightBig;
		int                   m_iItemWidthBig;
		int                   m_iTextureHeightBig;
		int                   m_iTextureWidthBig;

		bool                   m_bShowTexture = true;
		int                   m_iOffset = 0;

		GUIListControl.ListType m_iSelect = GUIListControl.ListType.CONTROL_LIST;
		int										m_iCursorX = 0; 
		int                   m_iColumns;
		bool									m_bScrollRight = false;
		bool									m_bScrollLeft = false;
		//    bool                  m_bDrawFocus=true;
		int										m_iScrollCounter = 0;
    int                   m_iSleeper=0;
		string m_strSuffix = "|";

		int m_iThumbWidth = 64;
		int m_iThumbHeight = 64;
  
		int m_iThumbXPosLow = 0;
		int m_iThumbYPosLow = 0;
		int m_iThumbWidthLow = 0;
		int m_iThumbHeightLow = 0;
		[XMLSkinElement("thumbPosXBig")] protected int m_iThumbXPosBig = 0;
		[XMLSkinElement("thumbPosYBig")] protected int m_iThumbYPosBig = 0;
		[XMLSkinElement("thumbWidthBig")] protected int m_iThumbWidthBig = 0;
		[XMLSkinElement("thumbHeightBig")] protected int m_iThumbHeightBig = 0;

		GUIFont m_pFont = null;
		GUISpinControl m_upDown = null;
		[XMLSkinElement("imageFolder")]		protected string	m_strImageFolder="";
		GUIImage m_imgFolder = null;
		[XMLSkinElement("imageFolderFocus")]protected string	m_strImageFolderFocus="";
		GUIImage m_imgFolderFocus = null;
    
		[XMLSkinElement("textureUp")]		protected string	m_strUp="";
		[XMLSkinElement("textureDown")]		protected string	m_strDown="";
		[XMLSkinElement("textureUpFocus")]	protected string	m_strUpFocus=""; 
		[XMLSkinElement("textureDownFocus")]protected string	m_strDownFocus="";
		[XMLSkinElement("spinColor")]		protected long		m_dwSpinColor;
		[XMLSkinElement("spinHeight")]		protected int		m_dwSpinHeight;
		[XMLSkinElement("spinWidth")]		protected int		m_dwSpinWidth;
		[XMLSkinElement("spinPosX")]		protected int		m_dwSpinX;
		[XMLSkinElement("spinPosY")]		protected int		m_dwSpinY;
		[XMLSkinElement("itemHeight")]			protected int m_iItemHeight;
		[XMLSkinElement("itemWidth")]			protected int m_iItemWidth;
		[XMLSkinElement("textureHeight")]		protected int m_iTextureHeightLow;
		[XMLSkinElement("textureWidth")]		protected int m_iTextureWidthLow;
		[XMLSkinElement("thumbWidthBig")]		protected int	m_iTextureWidth = 80;
		[XMLSkinElement("thumbHeightBig")]		protected int	m_iTextureHeight = 80;
		[XMLSkinElement("thumbPosX")]			protected int m_iThumbXPos = 8;
		[XMLSkinElement("thumbPosY")]			protected int m_iThumbYPos = 8;
		[XMLSkinElement("font")]			protected string	m_strFontName="";
		[XMLSkinElement("textcolor")]		protected long  m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("selectedColor")]	protected long  m_dwSelectedColor=0xFFFFFFFF;

		[XMLSkinElement("scrollbarbg")]		protected string	m_strScrollBarBG="";
		[XMLSkinElement("scrollbartop")]	protected string	m_strScrollBarTop="";
		[XMLSkinElement("scrollbarbottom")]	protected string	m_strScrollBarBottom="";


		[XMLSkinElement("backgroundheight")]		protected int		m_iBackgroundHeight;
		[XMLSkinElement("backgroundwidth")]		protected int		m_iBackgroundWidth;
		[XMLSkinElement("backgroundx")]		protected int		m_iBackgroundX;
		[XMLSkinElement("backgroundy")]		protected int		m_iBackgroundY;
		[XMLSkinElement("backgrounddiffuse")]		protected int	m_dwBackgroundDiffuse;
		[XMLSkinElement("background")]		protected string	m_strBackground;
		
		[XMLSkinElement("InfoImageheight")]		protected int		m_iInfoImageHeight;
		[XMLSkinElement("InfoImagewidth")]		protected int		m_iInfoImageWidth;
		[XMLSkinElement("InfoImagex")]		protected int		m_iInfoImageX;
		[XMLSkinElement("InfoImagey")]		protected int		m_iInfoImageY;
		[XMLSkinElement("InfoImagediffuse")]		protected int	m_dwInfoImageDiffuse;
		[XMLSkinElement("InfoImage")]		protected string	m_strInfoImage;

	  ArrayList m_vecItems = new ArrayList();
    int 									scroll_pos = 0;
    int 									iScrollX = 0;
    int 									iLastItem = -1;
    int 									iFrames = 0;
    int 									iStartFrame = 0;
    protected int         m_iDelayFrame = 0;
    bool                  m_bRefresh = false;
    protected             GUIverticalScrollbar m_horzScrollbar = null;
    protected string      m_strBrackedText;
    protected string      m_strScrollText;
    GUIImage              m_backgroundImage;
    GUIImage              m_infoImage;
    DateTime              m_dateTimeIdle=DateTime.Now;
    bool                  m_bInfoChanged=false;
    string                m_strNewInfoImage="";
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
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction ();
		  if (m_dwPosY > m_dwPosY && m_dwSpinY < m_dwPosY + m_dwHeight) m_dwSpinY = m_dwPosY + m_dwHeight;
		  m_imgFolder = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, m_iItemWidth, m_iItemHeight, m_strImageFolder, 0);
		  m_imgFolderFocus = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, m_iItemWidth, m_iItemHeight, m_strImageFolderFocus, 0);
		  m_upDown = new GUISpinControl(m_dwControlID, 0, m_dwSpinX, m_dwSpinY, m_dwSpinWidth, m_dwSpinHeight, m_strUp, m_strDown, m_strUpFocus, m_strDownFocus, m_strFontName, m_dwSpinColor, GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, GUIControl.Alignment.ALIGN_LEFT);
		  m_pFont = GUIFontManager.GetFont(m_strFontName);
		  int xpos = 5 + m_dwPosX + m_dwWidth;
		  if (xpos + 15 > GUIGraphicsContext.Width) xpos = GUIGraphicsContext.Width - 15;
		  m_horzScrollbar = new GUIverticalScrollbar(m_dwControlID, 0, 5 + m_dwPosX + m_dwWidth, m_dwPosY, 15, m_dwHeight, m_strScrollBarBG , m_strScrollBarTop, m_strScrollBarBottom);
		  m_horzScrollbar.SendNotifies = false;
		  m_upDown.Orientation=GUISpinControl.eOrientation.Horizontal;
		  m_upDown.SetReverse(true);
		  m_backgroundImage=new GUIImage(0,0,m_iBackgroundX,m_iBackgroundY,
			  m_iBackgroundWidth,m_iBackgroundHeight, m_strBackground,0);
		  m_infoImage=new GUIImage(0,0,m_iInfoImageX,m_iInfoImageY,
			  m_iInfoImageWidth,m_iInfoImageHeight, m_strInfoImage,0);
		  m_infoImage.Filtering=true;
		  m_infoImage.KeepAspectRatio=true;
		  m_infoImage.Centered=true;

			SetThumbDimensionsLow(m_iThumbXPos,m_iThumbYPos,m_iThumbWidth,m_iThumbHeight);
		  SetTextureDimensions(m_iTextureWidth,m_iTextureHeight);
			
	  }
		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution ();
			GUIGraphicsContext.ScaleRectToScreenResolution(ref m_dwSpinX, ref m_dwSpinY , ref m_dwSpinWidth, ref m_dwSpinHeight);
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextureWidth, ref m_iTextureHeight);
			GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iThumbXPos, ref m_iThumbYPos, ref m_iThumbWidth, ref m_iThumbHeight);
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextureWidthBig, ref m_iTextureHeightBig);
			GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iThumbXPosBig, ref m_iThumbYPosBig, 	ref m_iThumbWidthBig, ref m_iThumbHeightBig);
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iItemWidthBig, ref m_iItemHeightBig);
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iItemWidth, ref m_iItemHeight);

      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iInfoImageX, ref m_iInfoImageY, ref m_iInfoImageWidth, ref m_iInfoImageHeight);
      GUIGraphicsContext.ScaleRectToScreenResolution(ref m_iBackgroundX, ref m_iBackgroundY, ref m_iBackgroundWidth, ref m_iBackgroundHeight);
    }

    protected void OnSelectionChanged()
    {
      if (!IsVisible) return;
      string strSelected = "";
      string strSelected2 = "";
      string strThumb = "";
      int item=GetSelectedItem(ref strSelected, ref strSelected2, ref strThumb);
      GUIPropertyManager.SetProperty("#selecteditem", strSelected);
      GUIPropertyManager.SetProperty("#selecteditem2", strSelected2);
      GUIPropertyManager.SetProperty("#selectedthumb", strThumb);

      if (!IsVisible) return;
      if (item>=0 && item < m_vecItems.Count)
      {
        GUIListItem listitem=(GUIListItem)m_vecItems[item];
        listitem.ItemSelected(this);
      }
    }


    void RenderItem(bool bFocus, int dwPosX, int dwPosY, GUIListItem pItem)
    {

      float fTextHeight = 0, fTextWidth = 0;
      m_pFont.GetTextExtent("W", ref fTextWidth, ref fTextHeight);

      float fTextPosY = (float)dwPosY + (float)m_iTextureHeight;


      long dwColor = m_dwTextColor;
      if (pItem.Selected) dwColor = m_dwSelectedColor;
      if (bFocus == true && Focus && m_iSelect == GUIListControl.ListType.CONTROL_LIST)
      {
        m_imgFolderFocus.SetPosition(dwPosX, dwPosY);
        if (true == m_bShowTexture) m_imgFolderFocus.Render();

        RenderText((float)dwPosX, (float)fTextPosY, dwColor, pItem.Label, true);
      }
      else
      {
        m_imgFolder.SetPosition(dwPosX, dwPosY);
        if (true == m_bShowTexture) m_imgFolder.Render();

        RenderText((float)dwPosX, (float)fTextPosY, dwColor, pItem.Label, false);

      }
      if (pItem.HasThumbnail)
      {
        GUIImage pImage = pItem.Thumbnail;
        if (null == pImage && m_iSleeper==0 && !IsAnimating)
        {
          pImage = new GUIImage(0, 0, m_iThumbXPos + dwPosX, m_iThumbYPos + dwPosY, m_iThumbWidth, m_iThumbHeight, pItem.ThumbnailImage, 0x0);
          pImage.KeepAspectRatio = true;
          pImage.AllocResources();
          pItem.Thumbnail = pImage;
          int xOff = (m_iThumbWidth - pImage.RenderWidth) / 2;
          int yOff = (m_iThumbHeight - pImage.RenderHeight) / 2;
          pImage.SetPosition(m_iThumbXPos + dwPosX + xOff, m_iThumbYPos + dwPosY + yOff);
          pImage.Render();
          m_iSleeper+=SLEEP_FRAME_COUNT;
        }
        if (null != pImage)
        {
          if (pImage.TextureHeight==0&&pImage.TextureWidth==0)
          {
            pImage.FreeResources();
            pImage.AllocResources();
          }
          pImage.Width = m_iThumbWidth;
          pImage.Height = m_iThumbHeight;
          int xOff = (m_iThumbWidth - pImage.RenderWidth) / 2;
          int yOff = (m_iThumbHeight - pImage.RenderHeight) / 2;
          pImage.SetPosition(m_iThumbXPos + dwPosX + xOff, m_iThumbYPos + dwPosY + yOff);
          pImage.Render();
        }
      }
      else
      {
        if (pItem.HasIconBig)
        {
          GUIImage pImage = pItem.IconBig;
          if (null == pImage && m_iSleeper==0 && !IsAnimating)
          {
            pImage = new GUIImage(0, 0, m_iThumbXPos + dwPosX, m_iThumbYPos + dwPosY, m_iThumbWidth, m_iThumbHeight, pItem.IconImageBig, 0x0);
            pImage.KeepAspectRatio = true;
            pImage.AllocResources();
            pItem.IconBig = pImage;
            int xOff = (m_iThumbWidth - pImage.RenderWidth) / 2;
            int yOff = (m_iThumbHeight - pImage.RenderHeight) / 2;
            pImage.SetPosition(m_iThumbXPos + dwPosX + xOff, m_iThumbYPos + dwPosY + yOff);
            pImage.Render();
            m_iSleeper+=SLEEP_FRAME_COUNT;
          }
          if (null != pImage)
          {
            pImage.Width = m_iThumbWidth;
            pImage.Height = m_iThumbHeight;
            int xOff = (m_iThumbWidth - pImage.RenderWidth) / 2;
            int yOff = (m_iThumbHeight - pImage.RenderHeight) / 2;
            pImage.SetPosition(m_iThumbXPos + dwPosX + xOff, m_iThumbYPos + dwPosY + yOff);
            pImage.Render();
          }
        }
      }
    }

    public override void Render()
    {
      if (null == m_pFont) return;
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
      }

      int dwPosY = 0;
      if ((m_iCursorX > 0 ) && !ValidItem(m_iCursorX))
      {
        m_iCursorX = 0;
        OnSelectionChanged();
      }
    
      if (m_iSleeper>0) m_iSleeper--;

      UpdateInfoImage();
      if (m_backgroundImage!=null)
      {
        m_backgroundImage.Render();
      }
      if (m_infoImage!=null)
      {
        m_infoImage.Render();
      }

      if (m_horzScrollbar != null)
      {
        float fPercent = (float) m_iOffset + m_iCursorX;
        fPercent /= (float)(m_vecItems.Count);
        fPercent *= 100.0f;
        if ((int)fPercent != (int)m_horzScrollbar.Percentage)
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
      float fx = (float)m_dwPosX;
      float fy = (float)m_dwPosY;
      GUIGraphicsContext.Correct(ref fx, ref fy);

      if (fx<=0) fx=0;
      if (fy<=0) fy=0;
      view.X = (int)fx;
      view.Y = (int)fy;
      view.Width = (int)(m_iColumns * m_iItemWidth);
      view.Height = (int)(m_iItemHeight);
      view.MinZ = 0.0f;
      view.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = view;

      int iStartItem = 30000;
      int iEndItem = -1;
      dwPosY= m_dwPosY ;
      int dwPosX;

      if (m_bScrollLeft)
      {
        // render item on bottom
        dwPosX = m_dwPosX - m_iItemWidth + iScrollXOffset;
        int iItem = m_iOffset-1;
        if (iItem>=0 && iItem < m_vecItems.Count)
        {
          if (iItem < iStartItem) iStartItem=iItem;
          if (iItem >iEndItem) iEndItem=iItem;

          GUIListItem pItem = (GUIListItem)m_vecItems[iItem];
          RenderItem(false, dwPosX, dwPosY, pItem);
        }
      }

      // render main panel
      for (int iCol = 0; iCol < m_iColumns; iCol++)
      {
        dwPosX = m_dwPosX + iCol * m_iItemWidth+ iScrollXOffset;
        int iItem =  iCol + m_iOffset;
        if (iItem >=0 && iItem < (int)m_vecItems.Count)
        {
          if (iItem < iStartItem) iStartItem=iItem;
          if (iItem >iEndItem) iEndItem=iItem;

          GUIListItem pItem = (GUIListItem)m_vecItems[iItem];
          bool bFocus = (m_iCursorX == iCol );
          RenderItem(bFocus, dwPosX, dwPosY, pItem);
        }
      }


      if (m_bScrollRight)
      {
        // render item on top
        dwPosX = m_dwPosX+ m_iColumns* m_iItemWidth+  iScrollXOffset;
        m_iOffset ++;
        int iItem = m_iColumns + m_iOffset-1;
        if (iItem >= 0 && iItem < (int)m_vecItems.Count)
        {
          if (iItem < iStartItem) iStartItem=iItem;
          if (iItem >iEndItem) iEndItem=iItem;

          GUIListItem pItem = (GUIListItem)m_vecItems[iItem];
          RenderItem(false, dwPosX, dwPosY, pItem);
        }
        m_iOffset --;
      }
      GUIGraphicsContext.DX9Device.Viewport = oldview;
      


      //
      iFrames = 12;
      int iStep = m_iItemHeight / iFrames;
      if (0 == iStep) iStep = 1;
      if (m_bScrollLeft)
      {
        m_iScrollCounter -= iStep;
        if (m_iScrollCounter <= 0)
        {
          m_bScrollLeft = false;
          m_iOffset --;
          int iPage = m_iOffset / ( m_iColumns);
          if ((m_iOffset % (m_iColumns)) != 0) iPage++;
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
          int iPage = m_iOffset / (m_iColumns);
          if ((m_iOffset % (m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
          m_bRefresh = true;
          OnSelectionChanged();
        }
      }
      //free memory
      if (iStartItem < 30000)
      {
        for (int i = 0; i < iStartItem; ++i)
        {
          GUIListItem pItem = (GUIListItem)m_vecItems[i];
          if (null != pItem)
          {
            pItem.FreeMemory();
          }
        }
      }

      for (int i = iEndItem + 1; i < m_vecItems.Count; ++i) 
      {
        GUIListItem pItem = (GUIListItem)m_vecItems[i];
        if (null != pItem)
        {
          pItem.FreeMemory();
        }
      }
      
      dwPosY = m_dwPosY + (m_iItemHeight);
      //m_upDown.SetPosition(m_upDown.XPosition,dwPosY);
      m_upDown.Render();
      if (m_bScrollLeft || m_bScrollRight)
      {
        m_bRefresh = true;
      }
      int iItemsPerPage = m_iColumns;
      if (m_vecItems.Count > iItemsPerPage)
      {
        m_horzScrollbar.Render();
      }
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
      case Action.ActionType.ACTION_PAGE_UP : 
        OnPageUp();
        m_bRefresh = true;
        break;

      case Action.ActionType.ACTION_PAGE_DOWN : 
        OnPageDown();
        m_bRefresh = true;
        break;


      case Action.ActionType.ACTION_MOVE_DOWN : 
      {
        OnDown();
        m_bRefresh = true;
      }
        break;
		    
      case Action.ActionType.ACTION_MOVE_UP : 
      {
        if (!OnUp())
        {
          base.OnAction(action);
          return;
        }
        m_bRefresh = true;
      }
        break;

      case Action.ActionType.ACTION_MOVE_LEFT : 
      {
        OnLeft();
        m_bRefresh = true;
      }
        break;

      case Action.ActionType.ACTION_MOVE_RIGHT : 
      {
        OnRight();
        m_bRefresh = true;
      }
        break;
      case Action.ActionType.ACTION_KEY_PRESSED : 
        break;

      case Action.ActionType.ACTION_MOUSE_MOVE : 
      {
        int id;
        bool focus;
        if (m_horzScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
        {
          ///@@@TODO
        }
        //          m_bDrawFocus=true;
      } 
        break;
      case Action.ActionType.ACTION_MOUSE_CLICK : 
      {
        int id;
        bool focus;
        if (m_horzScrollbar.HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
        {
          ///@@@TODO
        }
        else
        {
          //m_bDrawFocus=true;
          if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
          {
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
        break;

      default : 
      {
        if (m_iSelect == GUIListControl.ListType.CONTROL_LIST)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, (int)action.wID, 0, null);
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
            m_iOffset = (m_upDown.Value - 1) * (m_iColumns);
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
        if (message.Message== GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          GUITextureManager.CleanupThumbs();
          foreach (GUIListItem item in m_vecItems)
          {
            item.FreeMemory();
          }
          m_bRefresh = true;
          m_infoImage.FreeResources();
          m_infoImage.AllocResources();
          m_infoImage.DoUpdate();
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
          m_vecItems.Add((GUIListItem) message.Object);
          int iItemsPerPage = m_iColumns;
          int iPages = m_vecItems.Count / iItemsPerPage;
          if ((m_vecItems.Count % iItemsPerPage) != 0) iPages++;
          m_upDown.SetRange(1, iPages);
          m_upDown.Value = 1;
          m_bRefresh = true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          m_vecItems.Clear();
          //GUITextureManager.CleanupThumbs();
          m_upDown.SetRange(1, 1);
          m_upDown.Value = 1;
          m_iCursorX =  m_iOffset = 0;
          m_bRefresh = true;
          OnSelectionChanged();

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
          int iItem = (int)message.Param1;
          if (iItem >= 0 && iItem < m_vecItems.Count)
          {
            int iPage = 1;
            m_iCursorX = 0;
            m_iOffset = 0;
            while (iItem >= ( m_iColumns))
            {
              m_iOffset += ( m_iColumns);
              iItem -= ( m_iColumns);
              iPage++;
            }
            m_upDown.Value = iPage;
            m_iCursorX = iItem;
            OnSelectionChanged();
          }
          m_bRefresh = true;
        }
      }

      if (base.OnMessage(message)) return true;

      return false;

    }

    public override void PreAllocResources()
    {
      if (null == m_pFont) return;
      base.PreAllocResources();
      m_upDown.PreAllocResources();
      m_imgFolder.PreAllocResources();
      m_imgFolderFocus.PreAllocResources();
      m_horzScrollbar.PreAllocResources();
      m_backgroundImage.PreAllocResources();
      m_infoImage.PreAllocResources();
      m_dateTimeIdle=DateTime.Now;
      m_bInfoChanged=false;
      m_strNewInfoImage="";
      m_iOffset=0;
      m_iCursorX=0;
    }


    void Calculate()
    {
      m_imgFolder.Width = m_iTextureWidth;
      m_imgFolder.Height = m_iTextureHeight;
      m_imgFolderFocus.Width = m_iTextureWidth;
      m_imgFolderFocus.Height = m_iTextureHeight;
			
      float fWidth = 0, fHeight = 0;

      // height of 1 item = folder image height + text row height + space in between
      m_pFont.GetTextExtent("y",ref fWidth, ref fHeight);


      fWidth = (float)m_iItemWidth;
      fHeight = (float)m_iItemHeight;
      float fTotalHeight = (float)(m_dwHeight - 5);


      m_iColumns = (int)(m_dwWidth / fWidth);

      int iItemsPerPage = m_iColumns;
      int iPages = m_vecItems.Count / iItemsPerPage;
      if ((m_vecItems.Count % iItemsPerPage) != 0) iPages++;
      m_upDown.SetRange(1, iPages);
      m_upDown.Value = 1;
    }

    public override void AllocResources()
    {
      m_iSleeper=0;
      if (null == m_pFont) return;
      base.AllocResources();
      m_backgroundImage.AllocResources();
      m_infoImage.AllocResources();
      m_upDown.AllocResources();
      m_imgFolder.AllocResources();
      m_imgFolderFocus.AllocResources();
      m_horzScrollbar.AllocResources();
      Calculate();

      m_upDown.ParentID=GetID;
    }

    public override void FreeResources()
    {
      foreach (GUIListItem item in m_vecItems)
      {
        item.FreeIcons();
      }
      m_vecItems.Clear();
      base.FreeResources();
      m_backgroundImage.FreeResources();
      m_upDown.FreeResources();
      m_imgFolder.FreeResources();
      m_imgFolderFocus.FreeResources();
      m_horzScrollbar.FreeResources();
      m_infoImage.FreeResources();
    }

		
    bool ValidItem(int iX)
    {
      if (iX >= m_iColumns) return false;
      if (m_iOffset  + iX < (int)m_vecItems.Count) return true;
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
          int iPage = m_iOffset / (m_iColumns);
          if ((m_iOffset % (m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
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
          if (ValidItem(m_iCursorX+1))
          {
            m_iCursorX++;
          }
          OnSelectionChanged();
        }
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
        if (m_bScrollLeft)
        {
          m_iScrollCounter = 0;
          m_bScrollLeft = false;
          m_iOffset --;
          int iPage = m_iOffset / (m_iColumns);
          if ((m_iOffset % (m_iColumns)) != 0) iPage++;
          m_upDown.Value = iPage + 1;
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

    bool OnUp()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      if (m_iSelect == GUIListControl.ListType.CONTROL_LIST) 
      {
        return false;
      }
      else
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
        m_iSelect= GUIListControl.ListType.CONTROL_UPDOWN;
        m_upDown.Focus = true;
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


    void RenderText(float fPosX, float fPosY, long dwTextColor, string wszText, bool bScroll)
    {
      float fTextHeight = 0, fTextWidth = 0;
      float fwidth, fWidth = 0, fHeight = 0;
      m_pFont.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);
      float fMaxWidth = m_iItemWidth - m_iItemWidth / 10.0f;
      float fPosCX = fPosX;
      float fPosCY = fPosY;
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;
      if (fPosCY > GUIGraphicsContext.Height) 
        fPosCY = (float)GUIGraphicsContext.Height;
      fHeight = 60.0f;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      if (fHeight <= 0) return;

      fwidth = fMaxWidth - 5.0f;
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;

      Viewport newviewport = new Viewport();
      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = (int)fPosCX;
      newviewport.Y = (int)fPosCY;
      newviewport.Width = (int)(fwidth);
      newviewport.Height = (int)(fHeight);
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;

      if (!bScroll || fTextWidth <= fMaxWidth)
      {
        m_pFont.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT);
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
          fMaxWidth += 50;
          m_strScrollText = "";
          if (iLastItem != iItem)
          {
            scroll_pos = 0;
            iLastItem = iItem;
            iStartFrame = 0;
            iScrollX = 1;
            m_iDelayFrame = 0;
          }
          if (iStartFrame > 50)
          {
            char wTmp;
            if (scroll_pos >= m_strBrackedText.Length)
              wTmp = ' ';
            else
              wTmp = m_strBrackedText[scroll_pos];
								
            m_pFont.GetTextExtent(wTmp.ToString(),ref fWidth, ref fHeight);
            if (iScrollX >= fWidth)
            {
              ++scroll_pos;
              if (scroll_pos > m_strBrackedText.Length)
                scroll_pos = 0;
              iFrames = 0;
              iScrollX = 1;
            }
            else 
            {
              m_iDelayFrame++;
              if (true || m_iDelayFrame > 2)
              {
                m_iDelayFrame = 0;
                iScrollX++;
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
              m_pFont.DrawText(fPosX - iScrollX, fPosY, dwTextColor, m_strScrollText, GUIControl.Alignment.ALIGN_LEFT);
								
          }
          else
          {
            iStartFrame++;
            if (fPosY >= 0.0)
              m_pFont.DrawText(fPosX, fPosY, dwTextColor, wszText, GUIControl.Alignment.ALIGN_LEFT);
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
        m_iOffset = (m_upDown.Value - 1) * m_iColumns ;
        OnSelectionChanged();
      }
    }

    void OnPageDown()
    {
      int iItemsPerPage = m_iColumns;
      int iPages = m_vecItems.Count / iItemsPerPage;
      if ((m_vecItems.Count % iItemsPerPage) != 0) iPages++;

      int iPage = m_upDown.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        m_upDown.Value = iPage;
        m_iOffset = (m_upDown.Value - 1) * iItemsPerPage;
      }
      while (m_iCursorX > 0 && m_iOffset + m_iCursorX >= (int) m_vecItems.Count)
      {
        m_iCursorX--;
      }
      OnSelectionChanged();

    }



    public void SetTextureDimensions(int iWidth, int iHeight)
    {
      m_iTextureWidth = iWidth;
      m_iTextureHeight = iHeight;


      m_imgFolder.Height = m_iTextureHeight;
      m_imgFolderFocus.Height = m_iTextureHeight;

      m_imgFolder.Width = m_iTextureWidth;
      m_imgFolderFocus.Width = m_iTextureWidth;
      
      m_imgFolder.Refresh();
      m_imgFolderFocus.Refresh();
    }

    public void SetThumbDimensions(int iXpos, int iYpos, int iWidth, int iHeight)
    {
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
        m_iItemWidth = (int)value;
        FreeResources();
        AllocResources();
      }
    }

    public int ItemHeight
    {
      get { return m_iItemHeight; }
      set
      {
        m_iItemHeight = (int)value;
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
      if (iItem >= 0 && iItem < (int)m_vecItems.Count)
      {
        GUIListItem pItem = (GUIListItem)m_vecItems[iItem];
        strLabel = pItem.Label;
        strLabel2 = pItem.Label2;
        if (pItem.IsFolder)
        {
          strLabel = String.Format("[{0}]", pItem.Label);
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

    public long TextColor { get { return m_dwTextColor; } }
    public long SelectedColor { get { return m_dwSelectedColor; } }
    public string FontName { get { return m_pFont.FontName; } }
    public int SpinWidth { get { return m_upDown.Width / 2; } }
    public int SpinHeight { get { return m_upDown.Height; } }
    public string TexutureUpName { get { return m_upDown.TexutureUpName; } }
    public string TexutureDownName { get { return m_upDown.TexutureDownName; } }
    public string TexutureUpFocusName { get { return m_upDown.TexutureUpFocusName; } }
    public string TexutureDownFocusName { get { return m_upDown.TexutureDownFocusName; } }
    public long SpinTextColor { get { return m_upDown.TextColor; } }
    public int SpinX { get { return m_upDown.XPosition; } }
    public int SpinY { get { return m_upDown.YPosition; } }
    public int TextureWidth { get { return m_iTextureWidth; } }
    public int TextureHeight { get { return m_iTextureHeight; } }
    public string FocusName { get { return m_imgFolderFocus.FileName; } }
    public string NoFocusName { get { return m_imgFolder.FileName; } }
 

    public int TextureWidthBig 
    {	 
      get { return m_iTextureWidthBig; } 
      set { m_iTextureWidthBig = value; }
    }
    public int TextureHeightBig
    { 
      get { return m_iTextureHeightBig; } 
      set { m_iTextureHeightBig = value; }
    }
    public int ItemWidthBig
    { 
      get { return m_iItemWidthBig; }
      set { m_iItemWidthBig = value; }
    }
    public int ItemHeightBig 
    { 
      get { return m_iItemHeightBig; } 
      set { m_iItemHeightBig = value; }
    }

    public int TextureWidthLow 
    { 
      get { return m_iTextureWidthLow; } 
      set { m_iTextureWidthLow = value; }
    }
    public int TextureHeightLow
    { 
      get { return m_iTextureHeightLow; } 
      set { m_iTextureHeightLow = value; }
    }
    public int ItemWidthLow
    { 
      get { return m_iItemWidthLow; } 
      set { m_iItemWidthLow = value; }
    }
    public int ItemHeightLow 
    { 
      get { return m_iItemHeightLow; } 
      set { m_iItemHeightLow = value; }
    }
  
    public void SetThumbDimensionsLow(int iXpos, int iYpos, int iWidth, int iHeight) 
    { 
      m_iThumbXPosLow = iXpos;
      m_iThumbYPosLow = iYpos;
      m_iThumbWidthLow = iWidth;
      m_iThumbHeightLow = iHeight;
    }
    public void SetThumbDimensionsBig(int iXpos, int iYpos, int iWidth, int iHeight) 
    { 
      m_iThumbXPosBig = iXpos;
      m_iThumbYPosBig = iYpos;
      m_iThumbWidthBig = iWidth;
      m_iThumbHeightBig = iHeight;
    }

    public override bool HitTest(int x,int y,out int controlID, out bool focused)
    {
      controlID=GetID;
      focused=Focus;
      int id;
      bool focus;
      if (m_horzScrollbar.HitTest(x, y,out id, out focus)) return true;
      if (!base.HitTest(x, y,out id, out focus)) 
      {
        Focus = false;
        if (!m_upDown.HitTest(x, y,out id, out focus))
          return false;
      }
      if (m_upDown.HitTest(x, y,out id, out focus))
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
      y -= (int)m_dwPosY;
      x -= (int)m_dwPosX;
      m_iCursorX = (x / m_iItemWidth);

      while (m_iCursorX > 0 && m_iOffset + m_iCursorX >= (int) m_vecItems.Count)
      {
        m_iCursorX--;
      } 
      OnSelectionChanged();

      return true;
    }
    public void Sort(System.Collections.IComparer comparer)
    {
      m_vecItems.Sort(comparer);
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
      m_vecItems.Add(item);
      int iItemsPerPage =  m_iColumns;
      int iPages = m_vecItems.Count / iItemsPerPage;
      if ((m_vecItems.Count % iItemsPerPage) != 0) iPages++;
      m_upDown.SetRange(1, iPages);
      m_upDown.Value = 1;
      m_bRefresh = true;
    }

    public int BackgroundX
    {
      get { return m_iBackgroundX;}
      set { m_iBackgroundX=value;m_backgroundImage.XPosition=value;}
    }
    public int BackgroundY
    {
      get { return m_iBackgroundY;}
      set { m_iBackgroundY=value; m_backgroundImage.YPosition=value;}
    }
    public int BackgroundWidth
    {
      get { return m_iBackgroundWidth;}
      set { m_iBackgroundWidth=value;m_backgroundImage.Width=value;}
    }
    public int BackgroundHeight
    {
      get { return m_iBackgroundHeight;}
      set { m_iBackgroundHeight=value;m_backgroundImage.Height=value;}
    }
    public long BackgroundDiffuse
    {
      get { return m_backgroundImage.ColourDiffuse;}
      set { m_backgroundImage.ColourDiffuse=value;}
    }
    public string BackgroundFileName
    {
      get { return m_backgroundImage.FileName;}
      set { m_backgroundImage.SetFileName(value);}
    }
    

    public int InfoImageX
    {
      get { return m_iInfoImageX;}
      set { m_iInfoImageX=value;m_infoImage.XPosition=value;}
    }
    public int InfoImageY
    {
      get { return m_iInfoImageY;}
      set { m_iInfoImageY=value;m_infoImage.YPosition=value;}
    }
    public int InfoImageWidth
    {
      get { return m_iInfoImageWidth;}
      set { m_iInfoImageWidth=value;m_infoImage.Width=value;}
    }
    public int InfoImageHeight
    {
      get { return m_iInfoImageHeight;}
      set { m_iInfoImageHeight=value;m_infoImage.Height=value;}
    }
    public long InfoImageDiffuse
    {
      get { return m_infoImage.ColourDiffuse;}
      set { m_infoImage.ColourDiffuse=value;}
    }
    public string InfoImageFileName
    {
      get { return m_infoImage.FileName;}
      set { 
        m_strNewInfoImage=value;
        m_bInfoChanged=true;
        m_dateTimeIdle=DateTime.Now;
      }
    }
    void UpdateInfoImage()
    {
      if (m_bInfoChanged)
      {
        TimeSpan ts = DateTime.Now-m_dateTimeIdle;
        if (ts.TotalMilliseconds>500)
        {  
          string oldFile=m_infoImage.FileName ;
          
          m_infoImage.SetFileName(m_strNewInfoImage);
          m_strNewInfoImage="";
          m_bInfoChanged=false;
        }
      }
    }

    
    public override void StorePosition()
    {
      m_infoImage.StorePosition();
      m_upDown.StorePosition();
      m_backgroundImage.StorePosition();
      m_imgFolder.StorePosition();
      m_imgFolderFocus.StorePosition();
      m_horzScrollbar.StorePosition();
      
      base.StorePosition();
    }

    public override void ReStorePosition()
    {
      m_infoImage.ReStorePosition();
      m_upDown.ReStorePosition();
      m_backgroundImage.ReStorePosition();
      m_imgFolder.ReStorePosition();
      m_imgFolderFocus.ReStorePosition();
      m_horzScrollbar.StorePosition();
      
      m_infoImage.GetRect(out m_iInfoImageX, out m_iInfoImageY, out m_iInfoImageWidth,out m_iInfoImageHeight);
      m_backgroundImage.GetRect(out m_iBackgroundX, out m_iBackgroundY, out m_iBackgroundWidth,out m_iBackgroundHeight);
      m_upDown.GetRect(out m_dwSpinX, out m_dwSpinY, out m_dwSpinWidth, out m_dwSpinHeight);

      base.ReStorePosition();
    }

    public override void Animate(Animator animator)
    {
      m_infoImage.Animate(animator);
      m_upDown.Animate(animator);
      m_backgroundImage.Animate(animator);
      base.Animate(animator);
    }


		/// <summary>
		/// Gets the ID of the control.
		/// </summary>
		public override int GetID
		{
			get { return m_dwControlID; }
			set 
			{ 
				m_dwControlID=value;
				if (m_upDown!=null) m_upDown.ParentID=value;
			}
		}
  }
}
