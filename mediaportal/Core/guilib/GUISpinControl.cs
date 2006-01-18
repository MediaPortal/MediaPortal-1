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

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
	public class GUISpinControl : GUIControl
	{
		public enum SpinType //:int
		{
			SPIN_CONTROL_TYPE_INT,
			SPIN_CONTROL_TYPE_FLOAT,
			SPIN_CONTROL_TYPE_TEXT,

			// needed for XAML parser
			Int = SPIN_CONTROL_TYPE_INT,
			Float = SPIN_CONTROL_TYPE_FLOAT,
			Text = SPIN_CONTROL_TYPE_TEXT,
		};

		public enum SpinSelect
		{
			SPIN_BUTTON_DOWN,
			SPIN_BUTTON_UP
		};
		[XMLSkinElement("showrange")]		protected bool			m_bShowRange=true;
		[XMLSkinElement("digits")]			protected int			m_iDigits=-1;
		[XMLSkinElement("reverse")]			protected bool			m_bReverse=false;
		[XMLSkinElement("textcolor")]		protected long  		_textColor=0xFFFFFFFF;
		[XMLSkinElement("font")]			protected string		_fontName="";
		[XMLSkinElement("textureUp")]		protected string		_upTextureName;
		[XMLSkinElement("textureDown")]		protected string		_downTextureName;
		[XMLSkinElement("textureUpFocus")]	protected string		_upTextureNameFocus; 
		[XMLSkinElement("textureDownFocus")]protected string		_downTextureNameFocus;

		[XMLSkinElement("align")]					protected Alignment		_alignment = Alignment.ALIGN_LEFT;
		[XMLSkinElement("spintype")]				protected GUISpinControl.SpinType		m_iType = SpinType.SPIN_CONTROL_TYPE_TEXT;
		[XMLSkinElement("orientation")]		protected eOrientation	m_orientation = eOrientation.Horizontal;

		protected bool      autoCheck=true;
		protected int       m_iStart=0;
		protected int       m_iEnd=100;
		protected float     m_fStart=0.0f;
		protected float     m_fEnd=1.0f;
		protected int       m_iValue=0;
		protected float     m_fValue=0.0f;
		
		protected SpinSelect m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
		protected float     m_fInterval=0.1f;
		protected ArrayList _listLabels = new ArrayList ();
		protected ArrayList m_vecValues= new ArrayList ();
		protected GUIImage m_imgspinUp=null;
		protected GUIImage m_imgspinDown=null;
		protected GUIImage m_imgspinUpFocus=null;
		protected GUIImage m_imgspinDownFocus=null;
	  
    
		protected GUIFont  _font=null;
		protected string   m_szTyped="";
    GUILabelControl      _labelControl=null;
	
		public GUISpinControl (int dwParentID) : base(dwParentID)
		{
		}
    public GUISpinControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strUp, string strDown, string strUpFocus, string strDownFocus, string strFont, long dwTextColor, SpinType iType,GUIControl.Alignment dwAlign)
      :base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      
      _textColor=dwTextColor;
      _fontName=strFont;
      _alignment=dwAlign;
      m_iType=iType;
	  
	  _downTextureName = strDown;
	  _upTextureName = strUp;
	  _upTextureNameFocus = strUpFocus;
	  _downTextureNameFocus = strDownFocus;

	  FinalizeConstruction();
    }
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction();
		  m_imgspinUp		= new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height,_upTextureName,0);
      m_imgspinUp.ParentControl = this;

		  m_imgspinDown		= new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height,_downTextureName,0);
      m_imgspinDown.ParentControl = this;

      m_imgspinUpFocus = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _upTextureNameFocus, 0);
      m_imgspinUpFocus.ParentControl = this;
      
      m_imgspinDownFocus = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _downTextureNameFocus, 0);
      m_imgspinDownFocus.ParentControl = this;

      m_imgspinUp.Filtering=false;
      m_imgspinDown.Filtering=false;
      m_imgspinUpFocus.Filtering=false;
      m_imgspinDownFocus.Filtering=false;
      _labelControl = new GUILabelControl(_parentControlId);
      _labelControl.CacheFont=true;
      _labelControl.ParentControl = this;
	  }

    public override void 	Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible)
        {
          m_szTyped=String.Empty;
          return;
        }
      }
      if (!Focus)
      {
        m_szTyped=String.Empty;
      }
      int dwPosX=_positionX;
      string wszText;

      if (m_iType == SpinType.SPIN_CONTROL_TYPE_INT)
      {
        string strValue=m_iValue.ToString();
        if (m_iDigits>1)
        {
          while (strValue.Length<m_iDigits) strValue="0"+ strValue;
        }
        if (m_bShowRange)
          wszText=strValue+ "/"+ m_iEnd.ToString();
        else
          wszText=strValue.ToString();
      }
      else if (m_iType==SpinType.SPIN_CONTROL_TYPE_FLOAT)
        wszText=String.Format("{0:2}/{1:2}",m_fValue, m_fEnd);
      else
      {
        wszText="";
        if (m_iValue>=0 && m_iValue < _listLabels.Count )
        {
          if (m_bShowRange)
          {
            wszText=String.Format("({0}/{1}) {2}", m_iValue+1,(int)_listLabels.Count,_listLabels[m_iValue] );
          }
          else
          {
            wszText=(string) _listLabels[m_iValue] ;
          }
        }
        else String.Format("?{0}?",m_iValue);
          
      }

			int iTextXPos=_positionX;
			int iTextYPos=_positionY;
      if ( _alignment== GUIControl.Alignment.ALIGN_LEFT)
      {
          if (_font!=null)
          {
						if (wszText!=null && wszText.Length>0)
						{
							float fTextHeight=0,fTextWidth=0;
							_font.GetTextExtent( wszText, ref fTextWidth, ref fTextHeight);
              if (Orientation==eOrientation.Horizontal)
              {
                m_imgspinUpFocus.SetPosition((int)fTextWidth + 5+dwPosX+ m_imgspinDown.Width, _positionY);
                m_imgspinUp.SetPosition((int)fTextWidth + 5+dwPosX+ m_imgspinDown.Width, _positionY);
                m_imgspinDownFocus.SetPosition((int)fTextWidth + 5+dwPosX, _positionY);
                m_imgspinDown.SetPosition((int)fTextWidth + 5+dwPosX, _positionY);
              }
              else
              {
                m_imgspinUpFocus.SetPosition((int)fTextWidth + 5+dwPosX, _positionY-(Height/2));
                m_imgspinUp.SetPosition((int)fTextWidth + 5+dwPosX, _positionY-(Height/2));
                m_imgspinDownFocus.SetPosition((int)fTextWidth + 5+dwPosX, _positionY+(Height/2));
                m_imgspinDown.SetPosition((int)fTextWidth + 5+dwPosX, _positionY+(Height/2));
              }
						}
          }
      }
			if ( _alignment== GUIControl.Alignment.ALIGN_CENTER)
			{
				if (_font!=null)
				{
					float fTextHeight=1,fTextWidth=1;
					if (wszText!=null && wszText.Length>0)
					{
						_font.GetTextExtent( wszText, ref fTextWidth, ref fTextHeight);
					}
					if (Orientation==eOrientation.Horizontal)
					{
						iTextXPos=dwPosX+m_imgspinUp.Width;
						iTextYPos=_positionY;
						m_imgspinDownFocus.SetPosition((int)dwPosX, _positionY);
						m_imgspinDown.SetPosition((int)dwPosX, _positionY);
						m_imgspinUpFocus.SetPosition((int)fTextWidth+m_imgspinUp.Width + dwPosX, _positionY);
						m_imgspinUp.SetPosition((int)fTextWidth +m_imgspinUp.Width+ dwPosX, _positionY);

						fTextHeight/=2.0f;
						float fPosY = ((float)_height)/2.0f;
						fPosY-=fTextHeight;
						fPosY+=(float)iTextYPos;
						iTextYPos=(int)fPosY;

					}
					else
					{
						iTextXPos=dwPosX;
						iTextYPos=_positionY+Height;
						m_imgspinUpFocus.SetPosition((int)+dwPosX		, _positionY-(Height+(int)fTextHeight)/2);
						m_imgspinUp.SetPosition((int)dwPosX					, _positionY-(Height+(int)fTextHeight)/2);
						m_imgspinDownFocus.SetPosition((int)dwPosX	, _positionY+(Height+(int)fTextHeight)/2);
						m_imgspinDown.SetPosition((int)dwPosX				, _positionY+(Height+(int)fTextHeight)/2);
					}
				}
			}

      if (m_iSelect==SpinSelect.SPIN_BUTTON_UP)
      {
          if (m_bReverse)
          {
              if ( !CanMoveDown() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
          }
          else
          {
              if ( !CanMoveUp() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
          }
      }

      if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN)
      {
          if (m_bReverse)
          {
              if ( !CanMoveUp() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_UP;
          }
          else
          {
              if ( !CanMoveDown() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_UP;
          }
      }

      if ( Focus )
      {
          bool bShow=CanMoveUp();
          if (m_bReverse)
              bShow = CanMoveDown();

          if (m_iSelect==SpinSelect.SPIN_BUTTON_UP && bShow )
              m_imgspinUpFocus.Render(timePassed);
          else
              m_imgspinUp.Render(timePassed);

          bShow=CanMoveDown();
          if (m_bReverse)
              bShow = CanMoveUp();
          if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN && bShow)
              m_imgspinDownFocus.Render(timePassed);
          else
              m_imgspinDown.Render(timePassed);
      }
      else
      {
          m_imgspinUp.Render(timePassed);
          m_imgspinDown.Render(timePassed);
      }

			if (_font!=null)
			{

        _labelControl.FontName=_fontName;
        _labelControl.TextColor=_textColor;
        _labelControl.Label=wszText;
	  
				if (Disabled)
					_labelControl.TextColor &= 0x80ffffff;
        if ( _alignment!= GUIControl.Alignment.ALIGN_CENTER)
				{
					if (wszText!=null && wszText.Length>0)
					{
            _labelControl.TextAlignment=_alignment;
            float fHeight=(float)_labelControl.TextHeight;
						fHeight/=2.0f;
						float fPosY = ((float)_height)/2.0f;
						fPosY-=fHeight;
						fPosY+=(float)_positionY;

						_labelControl.SetPosition(_positionX-3,(int)fPosY);
          }
				}
				else
				{
          _labelControl.SetPosition(iTextXPos,iTextYPos);
          _labelControl.TextAlignment=GUIControl.Alignment.ALIGN_LEFT;
				}
        _labelControl.Render(timePassed);
      }
    }

    public override void 	OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.REMOTE_0:
        case Action.ActionType.REMOTE_1:
        case Action.ActionType.REMOTE_2:
        case Action.ActionType.REMOTE_3:
        case Action.ActionType.REMOTE_4:
        case Action.ActionType.REMOTE_5:
        case Action.ActionType.REMOTE_6:
        case Action.ActionType.REMOTE_7:
        case Action.ActionType.REMOTE_8:
        case Action.ActionType.REMOTE_9:
        {
          if ( ((m_iDigits == -1) && (m_szTyped.Length >= 3)) ||
               ((m_iDigits != -1) && (m_szTyped.Length >= m_iDigits)) )
          {
            m_szTyped="";
          }
          int iNumber = action.wID - Action.ActionType.REMOTE_0;
     
          m_szTyped+= (char)(iNumber+'0');
          int iValue;
          iValue=Int32.Parse(m_szTyped);
          switch (m_iType)
          {
            case SpinType.SPIN_CONTROL_TYPE_INT:
            {
              // Value entered
              if ( ((m_iDigits == -1) && (m_szTyped.Length >= 3)) ||
                   ((m_iDigits != -1) && (m_szTyped.Length >= m_iDigits)) )
              {
                // Check value
                if (iValue < m_iStart) iValue = m_iStart;
                m_szTyped=iValue.ToString();
              }

              if (iValue > m_iEnd)
              {
                m_szTyped="";
                m_szTyped += (char)(iNumber+'0');
                iValue=Int32.Parse(m_szTyped);
              }

              m_iValue=iValue;
              if (m_iValue >= m_iStart && m_iValue <= m_iEnd)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
                GUIGraphicsContext.SendMessage(msg);
              }
            }  
              break;

            case SpinType.SPIN_CONTROL_TYPE_TEXT:
            {
              if (iValue < 0|| iValue >= _listLabels.Count)
              {
                iValue = 0;
              }

              m_iValue=iValue;
              if (m_iValue >= 0 && m_iValue < _listLabels.Count)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
                msg.Label = (string)_listLabels[m_iValue];
                GUIGraphicsContext.SendMessage(msg);
              }
            }  
              break;

          }
        }
          break;
      }
      if (action.wID == Action.ActionType.ACTION_PAGE_UP)
      {
        if (!m_bReverse)
          PageDown();
        else
          PageUp();
        return;
      }
      if (action.wID == Action.ActionType.ACTION_PAGE_DOWN)
      {
        if (!m_bReverse)
          PageUp();
        else
          PageDown();
        return;
      }
      bool bUp=false;
      bool bDown=false;
      if (Orientation==eOrientation.Horizontal && action.wID == Action.ActionType.ACTION_MOVE_LEFT)
        bUp=true;
      if (Orientation==eOrientation.Vertical && action.wID == Action.ActionType.ACTION_MOVE_DOWN)
        bUp=true;
      if (bUp)
      {
        if (m_iSelect==SpinSelect.SPIN_BUTTON_UP)
        {
          if (m_bReverse)
          {
            if (CanMoveUp() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return;
            }
          }
          else
          {
            if (CanMoveDown() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return;
            }
          }
        }
      }
      if (Orientation==eOrientation.Horizontal && action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
        bDown=true;
      if (Orientation==eOrientation.Vertical && action.wID == Action.ActionType.ACTION_MOVE_UP)
        bDown=true;

      if (bDown)
      {
        if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN)
        {
          if (m_bReverse)
          {
            if (CanMoveDown() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return;
            }
          }
          else
          {
            if (CanMoveUp() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return;
            }
          }
        }
      }
      if (Focus)
      {
        if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (m_iSelect==SpinSelect.SPIN_BUTTON_UP)
          {
            if (m_bReverse)
              MoveDown();
            else
              MoveUp();
            action.wID=Action.ActionType.ACTION_INVALID;
            return;
          }
          if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN)
          {
            if (m_bReverse)
              MoveUp();
            else
              MoveDown();
            action.wID=Action.ActionType.ACTION_INVALID;
            return;
          }
        }
      }
      base.OnAction(action);
    }

    public override bool 	OnMessage(GUIMessage message)
    {
      if (base.OnMessage(message) )
      {
          if (!Focus)
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
          else
            if (message.Param1 == (int)SpinSelect.SPIN_BUTTON_UP)
              m_iSelect = SpinSelect.SPIN_BUTTON_UP; 
            else 
              m_iSelect = SpinSelect.SPIN_BUTTON_DOWN;
          return true;
      }
      if (message.TargetControlId == GetID )
      {
          switch (message.Message)
          {
              case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
                Value= (int)message.Param1;
                return true;
              

              case GUIMessage.MessageType.GUI_MSG_LABEL_RESET:
              {
									
                  _listLabels.Clear();
                  m_vecValues.Clear();
                  Value=0;
                  return true;
              }

          case GUIMessage.MessageType.GUI_MSG_SHOWRANGE:
              if (message.Param1!=0 )
                  m_bShowRange=true;
              else
                  m_bShowRange=false;
              break;

              case GUIMessage.MessageType.GUI_MSG_LABEL_ADD:
              {
                  AddLabel(message.Label, (int)message.Param1);
                  return true;
              }

              case GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED:
              {
                  message.Param1= (int)Value ;
                  message.Param2=(int)m_iSelect;

                  if (m_iType==SpinType.SPIN_CONTROL_TYPE_TEXT)
                  {
                      if ( m_iValue>= 0 && m_iValue < _listLabels.Count )
                          message.Label=(string)_listLabels[m_iValue];

                      if ( m_iValue>= 0 && m_iValue < m_vecValues.Count )
                          message.Param1=(int)m_vecValues[m_iValue];
                  }
                  return true;
              }
          }
      }
      return false;

    }
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_imgspinUp.PreAllocResources();
      m_imgspinUpFocus.PreAllocResources();
      m_imgspinDown.PreAllocResources();
      m_imgspinDownFocus.PreAllocResources();

    }
    public override void 	AllocResources()
    {
      base.AllocResources();
      m_imgspinUp.AllocResources();
      m_imgspinUpFocus.AllocResources();
      m_imgspinDown.AllocResources();
      m_imgspinDownFocus.AllocResources();

      _font=GUIFontManager.GetFont(_fontName);
      SetPosition(_positionX, _positionY);

			if (SubItemCount>0)
			{
				m_iType = SpinType.SPIN_CONTROL_TYPE_TEXT;
				_listLabels.Clear();
				m_vecValues.Clear();
				for (int i=0; i < SubItemCount;++i)
				{
					string subitem= (string)GetSubItem(i);
					
					_listLabels.Add(subitem);
					m_vecValues.Add(i);
				}
			}
    }
    public override void 	FreeResources()
    {
      base.FreeResources();
      m_imgspinUp.FreeResources();
      m_imgspinUpFocus.FreeResources();
      m_imgspinDown.FreeResources();
      m_imgspinDownFocus.FreeResources();
      m_szTyped="";

    }
    public override void 	SetPosition(int dwPosX, int dwPosY)
    {
      if (dwPosX<0) return;
      if (dwPosY<0) return;
      base.SetPosition(dwPosX, dwPosY);

      if (Orientation==eOrientation.Horizontal)
      {
        m_imgspinDownFocus.SetPosition(dwPosX, dwPosY);
        m_imgspinDown.SetPosition(dwPosX, dwPosY);

        m_imgspinUp.SetPosition(_positionX + m_imgspinDown.Width,_positionY);
        m_imgspinUpFocus.SetPosition(_positionX + m_imgspinDownFocus.Width,_positionY);
      }
      else
      {
        m_imgspinUp.SetPosition(_positionX ,_positionY+Height/2);
        m_imgspinUpFocus.SetPosition(_positionX ,_positionY+Height/2);

        m_imgspinDownFocus.SetPosition(dwPosX, dwPosY-Height/2);
        m_imgspinDown.SetPosition(dwPosX, dwPosY-Height/2);

      }
    }
    public override int Width
    {
      get { 
        if (Orientation==eOrientation.Horizontal)
        {
          return m_imgspinDown.Width * 2 ;
        }
        else
        {
          return m_imgspinDown.Width;
        }
      }
    }

    public void SetRange(int iStart, int iEnd)
    {
      m_iStart=iStart;
      m_iEnd=iEnd;
    }
    public void SetFloatRange(float fStart, float fEnd)
    {
      m_fStart=fStart;
      m_fEnd=fEnd;
    }
    public int Value
    {
      get 
      {
        if (m_iValue < m_iStart) m_iValue = m_iStart;
        if (m_iValue > m_iEnd) m_iValue = m_iEnd;
        return m_iValue;
      }
      set { m_iValue=value;}
    }
    public float FloatValue
    {
      get
      {
        if (m_fValue < m_fStart) m_fValue = m_fStart;
        if (m_fValue > m_fEnd) m_fValue = m_fEnd;
        return m_fValue;
      }
      set { m_fValue=value;}
    }
    public void AddLabel(string strLabel, int  iValue)
    {
      if (strLabel==null) return;
      _listLabels.Add(strLabel);
      m_vecValues.Add(iValue);
    }
		public void Reset()
		{
			_listLabels.Clear();
			m_vecValues.Clear();
			Value=0;
		}
    public string GetLabel()
    {
      if (m_iValue <0 || m_iValue >= _listLabels.Count) return "";
      string strLabel=(string)_listLabels[ m_iValue];
      return strLabel;    
    }
    public override bool Focus
    {
      get { return base.Focus;}
      set 
      { 
        base.Focus=value;
        if(!IsFocused)
        {
          switch (m_iType)
          {
            case SpinType.SPIN_CONTROL_TYPE_INT:
              if (m_iValue < m_iStart) m_iValue = m_iStart;
              if (m_iValue > m_iEnd) m_iValue = m_iEnd;
              break;          
    
            case SpinType.SPIN_CONTROL_TYPE_TEXT:
              if (m_iValue <0 || m_iValue >= _listLabels.Count) m_iValue = 0;
              break;
          
            case SpinType.SPIN_CONTROL_TYPE_FLOAT:
              if (m_fValue < m_fStart) m_fValue = m_fStart;
              if (m_fValue > m_fEnd) m_fValue = m_fEnd;
              break;          
          }
        }
      }
    }
    public void SetReverse(bool bOnOff)
    {
      m_bReverse=bOnOff;
    }
    public int GetMaximum()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          return m_iEnd;
          
    
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
          return (int)_listLabels.Count;
          
        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          return (int)(m_fEnd*10.0f);
          
      }
      return 100;

    }
    public int GetMinimum()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          return m_iStart;
          
    
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
          return 1;
          
        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          return (int)(m_fStart*10.0f);
          
      }
      return 0;
    }
    public string TexutureUpName 
    { 
      get {return m_imgspinUp.FileName; }
    }
    public string TexutureDownName 
    { 
      get {return m_imgspinDown.FileName; }
    }
    public string TexutureUpFocusName 
    { 
      get {return m_imgspinUpFocus.FileName;} 
    }
    public string TexutureDownFocusName
    { 
      get {return m_imgspinDownFocus.FileName; }
    }
    public long TextColor 
    { 
      get {return _textColor;}
    }
    public string FontName
    { 
      get {return _fontName; }
    }
    public GUIControl.Alignment TextAlignment 
    { 
      get { return _alignment;}
    }
    public GUISpinControl.SpinType UpDownType 
    { 
      get { return m_iType;}
	  set { m_iType=value;}
    }
    public int SpinWidth 
    { 
      get { return m_imgspinUp.Width; }
    }
    public int SpinHeight
    { 
      get { return m_imgspinUp.Height; }
    }
    public float FloatInterval
    {
      get {return m_fInterval;}
      set {m_fInterval=value;}
    }

    public bool ShowRange
    {
      get { return m_bShowRange;}
      set {m_bShowRange=value;}
    }
    public int Digits
    {
      get { return m_iDigits;}
      set {m_iDigits=value;}
    }

    protected void PageUp()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
            if (m_iValue-10 >= m_iStart)
                m_iValue-=10;
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
            GUIGraphicsContext.SendMessage(msg);
            return;
        }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        {
            if (m_iValue-10 >= 0)
                m_iValue-=10;
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
            GUIGraphicsContext.SendMessage(msg);
            return;
        }
      }
    }

    protected void PageDown()
    {
      switch (m_iType)
      {
          case SpinType.SPIN_CONTROL_TYPE_INT:
          {
              if (m_iValue+10 <= m_iEnd)
                  m_iValue+=10;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }
          case SpinType.SPIN_CONTROL_TYPE_TEXT:
          {
              if (m_iValue+10 < (int)_listLabels.Count )
                  m_iValue+=10;
              GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }
      }

    }
    protected bool			CanMoveDown()
    {
			if (!AutoCheck) return true;
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
          if (m_iValue+1 <= m_iEnd)
            return true;
          return false;
        }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
        {
          if (m_fValue+m_fInterval <= m_fEnd)
            return true;
          return false;
        }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        {
          if (m_iValue+1 < (int)_listLabels.Count)
            return true;
          return false;
        }
      }
      return false;
    }

    protected bool CanMoveUp()
		{
			if (!AutoCheck) return true;
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
          if (m_iValue-1 >= m_iStart)
            return true;
          return false;
        }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
        {
          if (m_fValue-m_fInterval >= m_fStart)
            return true;
          return false;
        }
         
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        {
          if (m_iValue-1 >= 0)
            return true;
          return false;
        }
      }
      return false;
    }
    protected void			MoveUp()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
            if (m_iValue-1 >= m_iStart)
                m_iValue--;
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
            msg.Param1=m_iValue;
            GUIGraphicsContext.SendMessage(msg);
            return;
        }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
              if (m_fValue-m_fInterval >= m_fStart)
                  m_fValue-=m_fInterval;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }
         

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
          {
              if (m_iValue-1 >= 0)
                  m_iValue--;
          
              if (m_iValue< _listLabels.Count)
              {
                GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
                msg.Label=(string)_listLabels[m_iValue];
                GUIGraphicsContext.SendMessage(msg);
              }
              return;
          }
      }

    }

    protected void			MoveDown()
    {
      switch (m_iType)
      {
          case SpinType.SPIN_CONTROL_TYPE_INT:
          {
              if (m_iValue+1 <= m_iEnd)
                  m_iValue++;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }

          case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
              if (m_fValue+m_fInterval <= m_fEnd)
                  m_fValue+=m_fInterval;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              msg.Param1=m_iValue;
              GUIGraphicsContext.SendMessage(msg);
              return;
          }

          case SpinType.SPIN_CONTROL_TYPE_TEXT:
          {
              if (m_iValue+1 < (int)_listLabels.Count)
                  m_iValue++;
              if (m_iValue < (int)_listLabels.Count)
              {
                GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
                msg.Label=(string)_listLabels[m_iValue];
                GUIGraphicsContext.SendMessage(msg);
              }
              return;
          }
      }
    }

    public override bool InControl(int x, int y, out int iControlId)
    {
      iControlId=GetID;
      if (x >= m_imgspinUp.XPosition && x <= m_imgspinUp.XPosition+m_imgspinUp.RenderWidth)
      {
        if (y >= m_imgspinUp.YPosition && y <= m_imgspinUp.YPosition+m_imgspinUp.RenderHeight)
        {
          return true;
        }
      }
      if (x >= m_imgspinDown.XPosition && x <= m_imgspinDown.XPosition+m_imgspinDown.RenderWidth)
      {
        if (y >= m_imgspinDown.YPosition && y <= m_imgspinDown.YPosition+m_imgspinDown.RenderHeight)
        {
          return true;
        }
      }
      return false;
    }

    public override bool HitTest(int x,int y,out int controlID, out bool focused)
    {
      controlID=GetID;
      focused=Focus;
			if (x >= m_imgspinUp.XPosition && x <= m_imgspinUp.XPosition+m_imgspinUp.RenderWidth)
			{
				if (y >= m_imgspinUp.YPosition && y <= m_imgspinUp.YPosition+m_imgspinUp.RenderHeight)
				{
          if (m_bReverse)
          {
            if (CanMoveDown())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return true;
            }
          }
          else
          {
            if (CanMoveUp())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return true;
            }
          }
				}
			}
			if (x >= m_imgspinDown.XPosition && x <= m_imgspinDown.XPosition+m_imgspinDown.RenderWidth)
			{
				if (y >= m_imgspinDown.YPosition && y <= m_imgspinDown.YPosition+m_imgspinDown.RenderHeight)
				{
          if (m_bReverse)
          {
            if (CanMoveUp())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return true;
            }
          }
          else
          {
            if (CanMoveDown())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return true;
            }
          }
				}
			}
			Focus=false;
			return false;
		}

    public eOrientation Orientation
    {
      get { return m_orientation;}
      set { m_orientation=value;}
    }

    public override bool CanFocus()
    {
      if (!IsVisible) return false;
      if (Disabled) return false;
      if (m_iType==SpinType.SPIN_CONTROL_TYPE_INT)
      {
        if (m_iStart==m_iEnd) return false; 
      }
      
      if (m_iType==SpinType.SPIN_CONTROL_TYPE_TEXT)
      {
        if (_listLabels.Count < 2) return false;
      }
      return true;
    }
		public bool AutoCheck
		{
			get { return autoCheck;}
			set { autoCheck=value;}
		}
		public GUISpinControl.SpinSelect SelectedButton
		{
			get { return m_iSelect;}
			set { m_iSelect=value;}
		}
	}
}
