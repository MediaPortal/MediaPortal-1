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
		protected int										_textOffsetX=0;
		protected int										_textOffsetY=0;
		protected int										_textOffsetX2=0;
		protected int										_textOffsetY2=0;
		protected int										m_iImageWidth=16;
		protected int										m_iImageHeight=16;
		

		protected GUIFont								_font=null;
		protected GUISpinControl				m_upDown=null;
		protected string								m_strSuffix="|";
		protected ArrayList							m__itemList = new ArrayList();
		protected bool                  m_bEnableUpDown=true;

		protected int _scrollPosition = 0;
		protected int _scrollPosititionX=0;
		protected int iLastItem=-1;
		protected int iFrames=0;
		protected int iStartFrame=0;
		[XMLSkinElement("font")]			protected string		_fontName="";
		[XMLSkinElement("textcolor")]		protected long  		_textColor=0xFFFFFFFF;
		[XMLSkinElement("textureUp")]		protected string		_upTextureName;
		[XMLSkinElement("textureDown")]		protected string		_downTextureName;
		[XMLSkinElement("textureUpFocus")]	protected string		_upTextureNameFocus; 
		[XMLSkinElement("textureDownFocus")]protected string		_downTextureNameFocus;
		[XMLSkinElement("spinHeight")]		protected int			_spinControlHeight;
		[XMLSkinElement("spinWidth")]		protected int			_spinControlWidth;
		[XMLSkinElement("spinColor")]		protected long			_colorSpinColor;
		[XMLSkinElement("spinPosX")]		protected int			_spinControlPositionX;
		[XMLSkinElement("spinPosY")]		protected int			_spinControlPositionY;
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
			_fontName = strFontName;
			_spinControlHeight = dwSpinHeight;
			_spinControlWidth = dwSpinWidth;
			_upTextureName = strUp;
			_upTextureNameFocus = strUpFocus;
			_downTextureName = strDown;
			_downTextureNameFocus = strDownFocus;
			_colorSpinColor = dwSpinColor;
			_spinControlPositionX = dwSpinX;
			_spinControlPositionY = dwSpinY;
			_textColor=dwTextColor;
			FinalizeConstruction();
		}		
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
      m_upDown = new GUISpinControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _spinControlWidth, _spinControlHeight, _upTextureName, _downTextureName, _upTextureNameFocus, _downTextureNameFocus, _fontName, _colorSpinColor, GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, GUIControl.Alignment.ALIGN_LEFT);
      m_upDown.ParentControl = this;
			_font=GUIFontManager.GetFont(_fontName);
			if (m_strProperty.IndexOf("#")>=0) 
				containsProperty=true;

		}
		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution();
			
			GUIGraphicsContext.ScaleRectToScreenResolution(ref _spinControlPositionX, ref _spinControlPositionY,ref _spinControlWidth, ref _spinControlHeight);
		}

		public override void Render(float timePassed)
		{
			if (null==_font) return;
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
					m__itemList.Clear();

					m_strPrevProperty=strText;
					SetText(strText);
				}
			}

			int dwPosY=_positionY;

      for (int i=0; i < m_iItemsPerPage; i++)
      {
		    int dwPosX=_positionX;
        if (i+m_iOffset < m__itemList.Count )
        {
          // render item
			    GUIListItem item=(GUIListItem)m__itemList[i+m_iOffset];
			    string strLabel1=item.Label;
			    string strLabel2=item.Label2;

			    string wszText1=String.Format("{0}", strLabel1 );
			    int dMaxWidth=_width+16;
			    if (strLabel2.Length>0)
			    {
				    string wszText2;
				    float fTextWidth=0,fTextHeight=0;
				    wszText2=String.Format("{0}", strLabel2 );
				    _font.GetTextExtent( wszText2, ref fTextWidth,ref fTextHeight);
				    dMaxWidth -= (int)(fTextWidth);

				    _font.DrawTextWidth((float)dwPosX+dMaxWidth, (float)dwPosY+2, _textColor,wszText2,(float)fTextWidth,GUIControl.Alignment.ALIGN_LEFT);
			    }
			    _font.DrawTextWidth((float)dwPosX, (float)dwPosY+2, _textColor,wszText1,(float)dMaxWidth,GUIControl.Alignment.ALIGN_LEFT);
          dwPosY += (int)m_iItemHeight;
        }
      }
			if (m_bEnableUpDown)
			{
				int iPages=m__itemList.Count / m_iItemsPerPage;
				if ( (m__itemList.Count % m_iItemsPerPage) !=0) iPages++;

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
						while (m_iOffset>= m__itemList.Count) m_iOffset--;
					}
				}
        if (message.Message==GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem=message.Param1;
          if (iItem >=0 && iItem < m__itemList.Count)
          {
            message.Object=m__itemList[iItem];
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
          if (iItem >=0 && iItem < m__itemList.Count)
          {
            message.Object=m__itemList[iItem];
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
            m__itemList.Add( pItem);
            Calculate();
          }
				}

				if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
				{
					containsProperty=false;
					m_strProperty="";
					m_iOffset=0;
					m__itemList.Clear();
					m_upDown.SetRange(1,1);
					m_upDown.Value=1;
				}
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1=m__itemList.Count;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL2_SET)
        {
          int iItem=message.Param1;
          if (iItem >=0 && iItem < m__itemList.Count)
          {
            GUIListItem item=(GUIListItem)m__itemList[iItem];
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
			if (null==_font) return;
			base.PreAllocResources();
			m_upDown.PreAllocResources();
		}

		
		public override void AllocResources()
		{
			if (null==_font) return;
			base.AllocResources();
			m_upDown.AllocResources();

      _font=GUIFontManager.GetFont(_fontName);
			Calculate();

		}

		public override void FreeResources()
    {
			m_strPrevProperty="";
      m__itemList.Clear();
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
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, _rightControlId, (int)action.wID,0,null);
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
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, _leftControlId, (int)action.wID,0,null);
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
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, _upControlId, (int)action.wID,0,null);
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
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,WindowId,GetID, _downControlId, (int)action.wID,0,null);
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
			int iPages=m__itemList.Count / m_iItemsPerPage;
			if ( (m__itemList.Count % m_iItemsPerPage) !=0) iPages++;

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
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT,WindowId, GetID, GetID, m__itemList.Count -1,0,null); 
				OnMessage(msg);
			}
		}

		
		public void SetTextOffsets(int iXoffset, int iYOffset,int iXoffset2, int iYOffset2)
		{
      if (iXoffset< 0 ||iYOffset<0) return;
      if (iXoffset2< 0 ||iYOffset2<0) return;
			_textOffsetX = iXoffset;
			_textOffsetY = iYOffset;
			_textOffsetX2 = iXoffset2;
			_textOffsetY2 = iYOffset2;
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
			get { return _textColor;}
		}


		public string	FontName  
		{ 
			get 
			{
				if (_font==null) return "";
				return _font.FontName; 
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
			get { return _textOffsetX;}
		}

		public int TextOffsetY  
		{ 
			get { return _textOffsetY;}
		}
		public int TextOffsetX2  
		{ 
			get { return _textOffsetX2;}
		}
		public int TextOffsetY2  
		{ 
			get { return _textOffsetY2;}
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
	    m__itemList.Clear();
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
			    m__itemList.Add(item);
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
			    _font.GetTextExtent(wsTmp,ref fwidth,ref fheight);
			    if (fwidth > _width)
			    {
				    if (iLastSpace > 0 && iLastSpaceInLine != lpos)
				    {
					    szLine=szLine.Substring(0,iLastSpaceInLine);
					    pos=iLastSpace;
				    }
				    GUIListItem item = new GUIListItem(szLine);
				    m__itemList.Add(item);
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
		    m__itemList.Add(item);
	    }
      
      Calculate();
    }


    void Calculate()
    {

      float fWidth=0,fHeight=0;
			if (_font==null) return;
      _font.GetTextExtent( "y", ref fWidth,ref fHeight);
      //fHeight+=10.0f;

      //fHeight+=2;
			if (fHeight<=0) fHeight=1;
			m_iItemHeight			= (int)fHeight;
			
      float fTotalHeight= (float)(_height);
      m_iItemsPerPage		= (int)(fTotalHeight / fHeight);
			if (m_iItemsPerPage==0) 
			{
				m_iItemsPerPage=1;
			}

      int iPages=m__itemList.Count / m_iItemsPerPage;
      if ((m__itemList.Count % m_iItemsPerPage) >0)iPages++;
      if (iPages>1)
      {
        fTotalHeight= (float)(_height-m_upDown.Height-5);
        m_iItemsPerPage		= (int)(fTotalHeight / fHeight);

				if (m_iItemsPerPage==0) 
					m_iItemsPerPage=1;
        iPages=m__itemList.Count / m_iItemsPerPage;
        if ((m__itemList.Count % m_iItemsPerPage) >0)iPages++;
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
			m__itemList.Clear();
			m_upDown.SetRange(1,1);
			m_upDown.Value=1;
		}
		public string Label
		{
			set
			{
				if (m_strProperty!=value || m__itemList.Count==0)
				{
					m_strProperty=value;
					if (m_strProperty.IndexOf("#")>=0) 
						containsProperty=true;

					m__itemList.Clear();
					m_upDown.SetRange(1,1);
					m_upDown.Value=1;
					SetText( value );
				}
			}
		}
	}
}
