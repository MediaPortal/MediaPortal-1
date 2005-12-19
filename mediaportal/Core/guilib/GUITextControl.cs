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
using System;
using System.Collections;



namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITextControl : GUIControl
	{
		public enum ListType
		{
			CONTROL_LIST,
			CONTROL_UPDOWN
		};
		protected int										m_iSpaceBetweenItems=4;
		protected int                   m_iOffset=0;
		protected int                   m_iItemsPerPage=10;
		protected int                   m_iItemHeight=10;
		protected int										m_iTextOffsetX=0;
		protected int										m_iTextOffsetY=0;
		protected int										m_iTextOffsetX2=0;
		protected int										m_iTextOffsetY2=0;
		protected int										m_iImageWidth=16;
		protected int										m_iImageHeight=16;
		

		protected GUIFont								m_pFont=null;
		protected GUISpinControl				m_upDown=null;
		protected string								m_strSuffix="|";
		protected ArrayList							m_vecItems = new ArrayList();
		protected bool                  m_bEnableUpDown=true;

		protected int scroll_pos = 0;
		protected int iScrollX=0;
		protected int iLastItem=-1;
		protected int iFrames=0;
		protected int iStartFrame=0;
		[XMLSkinElement("font")]			protected string		m_strFontName="";
		[XMLSkinElement("textcolor")]		protected long  		m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("textureUp")]		protected string		m_strUp;
		[XMLSkinElement("textureDown")]		protected string		m_strDown;
		[XMLSkinElement("textureUpFocus")]	protected string		m_strUpFocus; 
		[XMLSkinElement("textureDownFocus")]protected string		m_strDownFocus;
		[XMLSkinElement("spinHeight")]		protected int			m_dwSpinHeight;
		[XMLSkinElement("spinWidth")]		protected int			m_dwSpinWidth;
		[XMLSkinElement("spinColor")]		protected long			m_dwSpinColor;
		[XMLSkinElement("spinPosX")]		protected int			m_dwSpinX;
		[XMLSkinElement("spinPosY")]		protected int			m_dwSpinY;
		[XMLSkinElement("label")]			protected string	m_strProperty="";

		bool                            containsProperty=false;
		string                          m_strPrevProperty="a";
		public GUITextControl (int dwParentID) : base(dwParentID)
		{
		}
		public GUITextControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, 
									string strFontName, 
									int dwSpinWidth,int dwSpinHeight,
									string strUp, string strDown, 
									string strUpFocus, string strDownFocus, 
									long dwSpinColor,int dwSpinX, int dwSpinY,
									long dwTextColor)
		:base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
		{
			m_strFontName = strFontName;
			m_dwSpinHeight = dwSpinHeight;
			m_dwSpinWidth = dwSpinWidth;
			m_strUp = strUp;
			m_strUpFocus = strUpFocus;
			m_strDown = strDown;
			m_strDownFocus = strDownFocus;
			m_dwSpinColor = dwSpinColor;
			m_dwSpinX = dwSpinX;
			m_dwSpinY = dwSpinY;
			m_dwTextColor=dwTextColor;
			FinalizeConstruction();
		}		
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			m_upDown=new GUISpinControl(m_dwControlID, 0, m_dwSpinX, m_dwSpinY, m_dwSpinWidth, m_dwSpinHeight, m_strUp, m_strDown, m_strUpFocus, m_strDownFocus, m_strFontName, m_dwSpinColor,GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT,GUIControl.Alignment.ALIGN_LEFT);
			m_pFont=GUIFontManager.GetFont(m_strFontName);
			if (m_strProperty.IndexOf("#")>=0) 
				containsProperty=true;

		}
		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution();
			
			GUIGraphicsContext.ScaleRectToScreenResolution(ref m_dwSpinX, ref m_dwSpinY,ref m_dwSpinWidth, ref m_dwSpinHeight);
		}

		public override void Render(float timePassed)
		{
			if (null==m_pFont) return;
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
      }

			if (containsProperty)
			{ 
				string strText=GUIPropertyManager.Parse(m_strProperty);
				
				strText=strText.Replace("\\r","\r");
				if (strText!=m_strPrevProperty)
				{
					m_iOffset=0;
					m_vecItems.Clear();

					m_strPrevProperty=strText;
					SetText(strText);
				}
			}

			int dwPosY=m_dwPosY;

      for (int i=0; i < m_iItemsPerPage; i++)
      {
		    int dwPosX=m_dwPosX;
        if (i+m_iOffset < m_vecItems.Count )
        {
          // render item
			    GUIListItem item=(GUIListItem)m_vecItems[i+m_iOffset];
			    string strLabel1=item.Label;
			    string strLabel2=item.Label2;

			    string wszText1=String.Format("{0}", strLabel1 );
			    int dMaxWidth=m_dwWidth+16;
			    if (strLabel2.Length>0)
			    {
				    string wszText2;
				    float fTextWidth=0,fTextHeight=0;
				    wszText2=String.Format("{0}", strLabel2 );
				    m_pFont.GetTextExtent( wszText2, ref fTextWidth,ref fTextHeight);
				    dMaxWidth -= (int)(fTextWidth);

				    m_pFont.DrawTextWidth((float)dwPosX+dMaxWidth, (float)dwPosY+2, m_dwTextColor,wszText2,(float)fTextWidth,GUIControl.Alignment.ALIGN_LEFT);
			    }
			    m_pFont.DrawTextWidth((float)dwPosX, (float)dwPosY+2, m_dwTextColor,wszText1,(float)dMaxWidth,GUIControl.Alignment.ALIGN_LEFT);
          dwPosY += (int)m_iItemHeight;
        }
      }
			if (m_bEnableUpDown)
			{
				int iPages=m_vecItems.Count / m_iItemsPerPage;
				if ( (m_vecItems.Count % m_iItemsPerPage) !=0) iPages++;

				if (iPages>1)
					m_upDown.Render(timePassed);
			}
		}

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_PAGE_UP:
					OnPageUp();
				break;

				case Action.ActionType.ACTION_PAGE_DOWN:
					OnPageDown();
				break;

				case Action.ActionType.ACTION_MOVE_DOWN:
				{
					OnDown();
				}
				break;
	    
				case Action.ActionType.ACTION_MOVE_UP:
				{
					OnUp();
				}
				break;

				case Action.ActionType.ACTION_MOVE_LEFT:
				{
					OnLeft();
				}
				break;

				case Action.ActionType.ACTION_MOVE_RIGHT:
				{
					OnRight();
				}
				break;

				default:
				{
          int ivalue=m_upDown.Value;
					m_upDown.OnAction(action);
          if (m_upDown.Value!=ivalue)
          {
            m_iOffset=(m_upDown.Value-1)*m_iItemsPerPage;
          }
				}
					break;
			}
		}


		public override bool OnMessage(GUIMessage message)
		{
			if (message.TargetControlId == GetID )
			{
				if (message.SenderControlId==0)
				{
					if (message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
					{
						m_iOffset=(m_upDown.Value-1)*m_iItemsPerPage;
						while (m_iOffset>= m_vecItems.Count) m_iOffset--;
					}
				}
        if (message.Message==GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem=message.Param1;
          if (iItem >=0 && iItem < m_vecItems.Count)
          {
            message.Object=m_vecItems[iItem];
          }
          else 
          {
            message.Object=null;
          }
          return true;
        }
				if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
					message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
				{
						base.OnMessage(message);
					m_upDown.Focus=Focus;
					return true;
				}
        if (message.Message==GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem=m_iOffset;
          if (iItem >=0 && iItem < m_vecItems.Count)
          {
            message.Object=m_vecItems[iItem];
          }
          else 
          {
            message.Object=null;
          }
          return true;
        }
				if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
				{
					containsProperty=false;
					m_strProperty="";
					GUIListItem pItem=message.Object as GUIListItem;
          if (pItem!=null)
          {
            m_vecItems.Add( pItem);
            Calculate();
          }
				}

				if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
				{
					containsProperty=false;
					m_strProperty="";
					m_iOffset=0;
					m_vecItems.Clear();
					m_upDown.SetRange(1,1);
					m_upDown.Value=1;
				}
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1=m_vecItems.Count;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL2_SET)
        {
          int iItem=message.Param1;
          if (iItem >=0 && iItem < m_vecItems.Count)
          {
            GUIListItem item=(GUIListItem)m_vecItems[iItem];
            item.Label2= message.Label ;
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
					if (message.Label!=null)
					{
						Label=message.Label;
					}
        }


				if (message.Message==GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
				{
					message.Param1=m_iOffset;
				}
				
			}

			if ( base.OnMessage(message) ) return true;

			return false;
		}

		public override void PreAllocResources()
		{
			if (null==m_pFont) return;
			base.PreAllocResources();
			m_upDown.PreAllocResources();
		}

		
		public override void AllocResources()
		{
			if (null==m_pFont) return;
			base.AllocResources();
			m_upDown.AllocResources();

      m_pFont=GUIFontManager.GetFont(m_strFontName);
			Calculate();

		}

		public override void FreeResources()
    {
			m_strPrevProperty="";
      m_vecItems.Clear();
			base.FreeResources();
			m_upDown.FreeResources();
		}
		
		protected void OnRight()
		{
			Action action = new Action();
			action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
			m_upDown.OnAction(action);
			if (!m_upDown.Focus)
			{
				Focus=false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, m_dwControlRight, (int)action.wID,0,null);
				GUIGraphicsContext.SendMessage(msg);
			}
			
		}

		
		protected void OnLeft()
		{
			Action action = new Action();
			action.wID = Action.ActionType.ACTION_MOVE_LEFT;
			m_upDown.OnAction(action);
			if (!m_upDown.Focus)
			{
				Focus=false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, m_dwControlLeft, (int)action.wID,0,null);
				GUIGraphicsContext.SendMessage(msg);
			}
		}

		
		protected void OnUp()
		{
			Action action = new Action();
			action.wID = Action.ActionType.ACTION_MOVE_UP;
			m_upDown.OnAction(action);
			if (!m_upDown.Focus)
			{
				Focus=false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, m_dwControlUp, (int)action.wID,0,null);
				GUIGraphicsContext.SendMessage(msg);
			}
			
		}


		protected void OnDown()
		{
			Action action=new Action();
			action.wID = Action.ActionType.ACTION_MOVE_DOWN;
			m_upDown.OnAction(action);

			if (!m_upDown.Focus)
			{
				Focus=false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, m_dwControlDown, (int)action.wID,0,null);
				GUIGraphicsContext.SendMessage(msg);
			}			
		}

		public String ScrollySuffix
		{
			get { return m_strSuffix;}
			set {
        if (value==null) return;
        m_strSuffix=value;
      }
		}

		
		protected void OnPageUp()
		{
			int iPage = m_upDown.Value;
			if (iPage > 1)
			{
				iPage--;
				m_upDown.Value=iPage;
				m_iOffset=(m_upDown.Value-1)*m_iItemsPerPage;
			}
			else 
			{
				// already on page 1, then select the 1st item
				
			}
		}

		
		protected void OnPageDown()
		{
			int iPages=m_vecItems.Count / m_iItemsPerPage;
			if ( (m_vecItems.Count % m_iItemsPerPage) !=0) iPages++;

			int iPage = m_upDown.Value;
			if (iPage+1 <= iPages)
			{
				iPage++;
				m_upDown.Value=iPage;
				m_iOffset=(m_upDown.Value-1)*m_iItemsPerPage;
			}
			else
			{
				// already on last page, move 2 last item in list
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT,WindowId, GetID, GetID, m_vecItems.Count -1,0,null); 
				OnMessage(msg);
			}
		}

		
		public void SetTextOffsets(int iXoffset, int iYOffset,int iXoffset2, int iYOffset2)
		{
      if (iXoffset< 0 ||iYOffset<0) return;
      if (iXoffset2< 0 ||iYOffset2<0) return;
			m_iTextOffsetX = iXoffset;
			m_iTextOffsetY = iYOffset;
			m_iTextOffsetX2 = iXoffset2;
			m_iTextOffsetY2 = iYOffset2;
		}
		
		public void SetImageDimensions(int iWidth, int iHeight)
		{
      if (iWidth<0 || iHeight<0) return;
			m_iImageWidth  = iWidth;
			m_iImageHeight = iHeight;
		}

		public int ItemHeight
		{
			get { return m_iItemHeight;}
      set 
      {
        if (value<0) return;
        m_iItemHeight=value;
      }
		}
		public int Space
		{
			get { return m_iSpaceBetweenItems;}
			set {
        if (value<0) return;
        m_iSpaceBetweenItems=value;
      }
		}

		public long	TextColor
		{ 
			get { return m_dwTextColor;}
		}


		public string	FontName  
		{ 
			get 
			{
				if (m_pFont==null) return "";
				return m_pFont.FontName; 
			}
		}

		public int	SpinWidth  
		{ 
			get { return m_upDown.Width/2; }
		}

		public int	SpinHeight  
		{ 
			get { return m_upDown.Height; }
		}

		public string	TexutureUpName  
		{ 
			get { return m_upDown.TexutureUpName; }
		}

		public string	TexutureDownName  
		{ 
			get { return m_upDown.TexutureDownName; }
		}

		public string	TexutureUpFocusName  
		{ 
			get { return m_upDown.TexutureUpFocusName; }
		}

		public string	TexutureDownFocusName  
		{ 
			get { return m_upDown.TexutureDownFocusName; }
		}

		public long		SpinTextColor  
		{ 
			get { return m_upDown.TextColor;}
		}

		public int		SpinX  
		{ 
			get { return m_upDown.XPosition;}
		}

		public int		SpinY  
		{ 
			get { return m_upDown.YPosition;}
		}


		public int TextOffsetX  
		{ 
			get { return m_iTextOffsetX;}
		}

		public int TextOffsetY  
		{ 
			get { return m_iTextOffsetY;}
		}
		public int TextOffsetX2  
		{ 
			get { return m_iTextOffsetX2;}
		}
		public int TextOffsetY2  
		{ 
			get { return m_iTextOffsetY2;}
		}

		public int ImageWidth  
		{ 
			get { return m_iImageWidth;}
		}
		public int ImageHeight  
		{ 
			get { return m_iImageHeight;}
		}
		public string Suffix  
		{ 
			get { return m_strSuffix;}
		}

    public override bool HitTest(int x,int y,out int controlID, out bool focused)
    {
      controlID=GetID;
      focused=Focus;
      int id;
      bool focus;
			if ( m_upDown.HitTest(x,y,out id,out focus))
			{
				if (m_upDown.GetMaximum() > 1)
				{
					m_upDown.Focus=true;
					return true;
				}
				return true;
			}
      if (!base.HitTest(x,y,out id,out focus)) 
      {
        Focus=false;
        return false;
      }
      return false;
		}

    void SetText(string strText)
    {
      if (strText==null) return;
	    m_vecItems.Clear();
	    // start wordwrapping
      // Set a flag so we can determine initial justification effects
      //bool bStartingNewLine = true;
	    //bool bBreakAtSpace = false;
      int pos=0;
	    int lpos=0;
	    int iLastSpace=-1;
	    int iLastSpaceInLine=-1;
	    string szLine="";
      while( pos < (int)strText.Length )
      {
        // Get the current letter in the string
        char letter = (char)strText[pos];

        // Handle the newline character
        if (letter == '\n' )
        {
			    GUIListItem item=new GUIListItem(szLine);
			    m_vecItems.Add(item);
			    iLastSpace=-1;
			    iLastSpaceInLine=-1;
          lpos=0;
          szLine="";
        }
		    else
		    {
			    if (letter==' ') 
			    {
				    iLastSpace=pos;
				    iLastSpaceInLine=lpos;
			    }

			    if (lpos < 0 || lpos >1023)
			    {
				    //OutputDebugString("ERRROR\n");
			    }
			    szLine+=letter;

			    float fwidth=0,fheight=0;
			    string wsTmp=szLine;
			    m_pFont.GetTextExtent(wsTmp,ref fwidth,ref fheight);
			    if (fwidth > m_dwWidth)
			    {
				    if (iLastSpace > 0 && iLastSpaceInLine != lpos)
				    {
					    szLine=szLine.Substring(0,iLastSpaceInLine);
					    pos=iLastSpace;
				    }
				    GUIListItem item = new GUIListItem(szLine);
				    m_vecItems.Add(item);
				    iLastSpaceInLine=-1;
				    iLastSpace=-1;
				    lpos=0;
            szLine="";
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
		    GUIListItem item=new GUIListItem(szLine);
		    m_vecItems.Add(item);
	    }
      
      Calculate();
    }


    void Calculate()
    {

      float fWidth=0,fHeight=0;
			if (m_pFont==null) return;
      m_pFont.GetTextExtent( "y", ref fWidth,ref fHeight);
      //fHeight+=10.0f;

      //fHeight+=2;
			if (fHeight<=0) fHeight=1;
			m_iItemHeight			= (int)fHeight;
			
      float fTotalHeight= (float)(m_dwHeight);
      m_iItemsPerPage		= (int)(fTotalHeight / fHeight);
			if (m_iItemsPerPage==0) 
			{
				m_iItemsPerPage=1;
			}

      int iPages=m_vecItems.Count / m_iItemsPerPage;
      if ((m_vecItems.Count % m_iItemsPerPage) >0)iPages++;
      if (iPages>1)
      {
        fTotalHeight= (float)(m_dwHeight-m_upDown.Height-5);
        m_iItemsPerPage		= (int)(fTotalHeight / fHeight);

				if (m_iItemsPerPage==0) 
					m_iItemsPerPage=1;
        iPages=m_vecItems.Count / m_iItemsPerPage;
        if ((m_vecItems.Count % m_iItemsPerPage) >0)iPages++;
      }
      m_upDown.SetRange(1,iPages);
      m_upDown.Value=1;
      if (iPages==0) 
        m_upDown.IsVisible=false;
      else 
        m_upDown.IsVisible=true;
    }

    public bool EnableUpDown
    {
      get { return m_bEnableUpDown;}
      set { m_bEnableUpDown=value;}
    }

		/// <summary>
		/// Get/set the text of the label.
		/// </summary>
		public string Property
		{
			get { return m_strProperty; }
			set 
			{
				m_strProperty=value;
				if (m_strProperty.IndexOf("#")>=0) 
					containsProperty=true;
			}
		}
		public void Clear()
		{
				containsProperty=false;
			m_strProperty="";
			m_iOffset=0;
			m_vecItems.Clear();
			m_upDown.SetRange(1,1);
			m_upDown.Value=1;
		}
		public string Label
		{
			set
			{
				if (m_strProperty!=value || m_vecItems.Count==0)
				{
					m_strProperty=value;
					if (m_strProperty.IndexOf("#")>=0) 
						containsProperty=true;

					m_vecItems.Clear();
					m_upDown.SetRange(1,1);
					m_upDown.Value=1;
					SetText( value );
				}
			}
		}
	}
}
