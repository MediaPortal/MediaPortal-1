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
using System.Drawing;
using System.Collections;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Implementation of a slider control.
	/// </summary>
	public class GUISliderControl : GUIControl
	{
		public enum SpinSelect
		{
			SPIN_BUTTON_DOWN,
			SPIN_BUTTON_UP
		};
	
		
		
		[XMLSkinElement("textureSliderBar")]		protected string	m_strBackground;
		[XMLSkinElement("textureSliderNib")]protected string	m_strSliderTexture;
		[XMLSkinElement("textureSliderNibFocus")]
											protected string  m_strSliderTextureFocus;
	
		protected int       m_iPercent=0;
		protected int       m_iStart=0;
		protected int       m_iEnd=100;
		protected float     m_fStart=0.0f;
		protected float     m_fEnd=1.0f;
		protected int       m_iValue=0;
		protected float     m_fValue=0.0f;
		[XMLSkinElement("subtype")]
		protected GUISpinControl.SpinType   m_iType=GUISpinControl.SpinType.SPIN_CONTROL_TYPE_TEXT;
		protected bool			m_bReverse=false;
		protected float     m_fInterval=0.1f;
		protected ArrayList m_vecLabels = new ArrayList ();
		protected ArrayList m_vecValues= new ArrayList ();
		protected GUIImage m_guiBackground=null;
		protected GUIImage m_guiMid=null;
		protected GUIImage m_guiMidFocus=null;

	  
    
		protected bool     m_bShowRange=false;
	
		public GUISliderControl (int dwParentID) : base(dwParentID)
		{
		}
		/// <summary>
		/// The constructor of the GUISliderControl.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strBackGroundTexture">The background texture of the </param>
		/// <param name="strMidTexture">The unfocused texture.</param>
		/// <param name="strMidTextureFocus">The focused texture</param>
		/// <param name="iType">The type of control.</param>
    public GUISliderControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strBackGroundTexture,string strMidTexture,string strMidTextureFocus,GUISpinControl.SpinType iType)
      :base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
		m_strBackground = strBackGroundTexture;
		m_strSliderTexture = strMidTexture;
		m_strSliderTextureFocus = strMidTextureFocus;
		m_iType=iType;
		FinalizeConstruction();
    }
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			m_guiBackground = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strBackground,0);
			m_guiMid= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strSliderTexture,0);
			m_guiMidFocus= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strSliderTextureFocus,0);
		}


		/// <summary>
		/// Renders the control.
		/// </summary>
    public override void 	Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
      }
      string strValue="";
      float fRange=0.0f;
      float fPos=0.0f;
      float fPercent=0.0f;
      GUIFont m_pFont13 = GUIFontManager.GetFont("font13");
      switch (m_iType)
      {
					// Float based slider
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
          strValue=String.Format("{0}",m_fValue);
          if (null!=m_pFont13)
          {
              m_pFont13.DrawShadowText( (float)m_dwPosX,(float)m_dwPosY, 0xffffffff,
                                        strValue, 
                                        GUIControl.Alignment.ALIGN_LEFT, 
                                        2, 
                                        2,
                                        0xFF020202);
          }
          m_guiBackground.SetPosition(m_dwPosX + 60, m_dwPosY);

          fRange=(float)(m_fEnd-m_fStart);
          fPos  =(float)(m_fValue-m_fStart);
          fPercent = (fPos/fRange)*100.0f;
          m_iPercent = (int) fPercent;
        break;

					// Integer based slider
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
          strValue=String.Format("{0}/{1}",m_iValue, m_iEnd);
          if (null!=m_pFont13)
          {
            m_pFont13.DrawShadowText( (float)m_dwPosX,(float)m_dwPosY, 0xffffffff,
                                            strValue,
                                            GUIControl.Alignment.ALIGN_LEFT, 
                                            2, 
                                            2,
                                            0xFF020202);
          }
          m_guiBackground.SetPosition(m_dwPosX + 60, m_dwPosY);

          fRange= (float)(m_iEnd-m_iStart);
          fPos  = (float)(m_iValue-m_iStart);
          m_iPercent = (int) ((fPos/fRange)*100.0f);
          break;
      }

      //int iHeight=25;
      m_guiBackground.Render(timePassed);
      //m_guiBackground.SetHeight(iHeight);
      m_dwHeight = m_guiBackground.Height;
      m_dwWidth = m_guiBackground.Width + 60;

      float fWidth=(float)(m_guiBackground.TextureWidth - m_guiMid.Width); //-20.0f;

      fPos = (float)m_iPercent;
      fPos /=100.0f;
      fPos *= fWidth;
      fPos += (float) m_guiBackground.XPosition;
      //fPos += 10.0f;
      if ((int)fWidth > 1)
      {
        if (m_bHasFocus)
        {
          m_guiMidFocus.SetPosition((int)fPos, m_guiBackground.YPosition );
          m_guiMidFocus.Render(timePassed);
        }
        else
        {
          m_guiMid.SetPosition((int)fPos, m_guiBackground.YPosition );
          m_guiMid.Render(timePassed);
        }
      }
    }

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
    public override void 	OnAction(Action action)
    {
        GUIMessage message;
        switch ( action.wID )
	      {
          case Action.ActionType.ACTION_MOUSE_CLICK:
            float x=(float)action.fAmount1-m_guiBackground.XPosition;
            if (x <0) x=0;
            if (x >m_guiBackground.RenderWidth) x=m_guiBackground.RenderWidth;
            x/= (float)m_guiBackground.RenderWidth;
            float total,pos;
            switch (m_iType)
            {
              case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
                total=m_fEnd-m_fStart;
                pos= (x*total) ;
                m_fValue=m_fStart+pos;
                m_fValue=(float)Math.Round(m_fValue,1);
                break;

              case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
                float start=m_iStart;
                float end  =m_iEnd;
                total=end-start;
                pos= (x* total);
                m_iValue=m_iStart+(int)pos;
                break;

              default:
                m_iPercent = (int) ( 100f *  x);
                break;
            }
						m_fValue=(float)Math.Round(m_fValue,1);
						message=new GUIMessage (GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
						GUIGraphicsContext.SendMessage(message);
          break;
           
						// decrease the slider value
		      case Action.ActionType.ACTION_MOVE_LEFT:
			      switch (m_iType)
			      {
				      case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
					      if (m_fValue > m_fStart) m_fValue -= m_fInterval;
					      break;

				      case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
					      if (m_iValue > m_iStart) m_iValue --;
					      break;

				      default:
					      if ( m_iPercent >0) m_iPercent --;
					      break;
			      }
						m_fValue=(float)Math.Round(m_fValue,1);
            message=new GUIMessage (GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
			      GUIGraphicsContext.SendMessage(message);
			    break;

						// increase the slider value
		      case Action.ActionType.ACTION_MOVE_RIGHT:
			      switch (m_iType)
			      {
				      case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
					      if (m_fValue < m_fEnd) m_fValue += m_fInterval;
					      break;

				      case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
					      if (m_iValue < m_iEnd) m_iValue ++;
					      break;

				      default:
					      if ( m_iPercent < 100 ) m_iPercent ++;
					      break;
			      }
						m_fValue=(float)Math.Round(m_fValue,1);
            message=new GUIMessage (GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
			      GUIGraphicsContext.SendMessage(message);
			    break;

		      default:
			      base.OnAction(action);
          break;
	      }
    }

		/// <summary>
		/// OnMessage() This method gets called when there's a new message. 
		/// Controls send messages to notify their parents about their state (changes)
		/// By overriding this method a control can respond to the messages of its controls
		/// </summary>
		/// <param name="message">message : contains the message</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool 	OnMessage(GUIMessage message)
    {
	    if (message.TargetControlId == GetID )
	    {
		    switch (message.Message)
		    {
						// Move the slider to a certain position
			    case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
				    Percentage= message.Param1 ;
				    return true;
			    
    				// Reset the slider
			    case GUIMessage.MessageType.GUI_MSG_LABEL_RESET:
			    {
				    Percentage=0;
				    return true;
			    }
		    }
	    }

      return base.OnMessage(message);
    }

		/// <summary>
		/// Get/set the percentage the slider indicates.
		/// </summary>
    public int Percentage
    {
      get { return m_iPercent;}
      set { 
        if (value >=0 && value <=100) m_iPercent=value;
      }
    }

		/// <summary>
		/// Get/set the integer value of the slider.
		/// </summary>
    public int IntValue
    {
      get { return m_iValue;}
      set { 
        if (value >= m_iStart && value <=m_iEnd)
        {
          m_iValue=value;
        }
      }
    }

		/// <summary>
		/// Get/set the float value of the slider.
		/// </summary>
    public float FloatValue
    {
      get { return m_fInterval;}
      set 
      {
        if (value >= m_fStart && value <=m_fEnd)
        {
          m_fInterval=value;
        }
      }
    }

		/// <summary>
		/// Get/Set the spintype of the control.
		/// </summary>
    public GUISpinControl.SpinType SpinType
    {
      get { return m_iType;}
      set { m_iType=value;}
    }

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_guiBackground.PreAllocResources();
      m_guiMid.PreAllocResources();
      m_guiMidFocus.PreAllocResources();

    }

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
    public override void 	AllocResources()
    {
      base.AllocResources();
      m_guiBackground.AllocResources();
      m_guiMid.AllocResources();
      m_guiMidFocus.AllocResources();
    }

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
    public override void 	FreeResources()
    {
      base.FreeResources();
      m_guiBackground.FreeResources();
      m_guiMid.FreeResources();
      m_guiMidFocus.FreeResources();
    }

		/// <summary>
		/// Sets the integer range of the slider.
		/// </summary>
		/// <param name="iStart">Start point</param>
		/// <param name="iEnd">End point</param>
    public void SetRange(int iStart, int iEnd)
    {
      if (iEnd>iStart && iStart>=0)
      {
        m_iStart=iStart;
        m_iEnd=iEnd;
      }
    }

		/// <summary>
		/// Sets the float range of the slider.
		/// </summary>
		/// <param name="fStart">Start point</param>
		/// <param name="fEnd">End point</param>
    public void SetFloatRange(float fStart, float fEnd)
    {
      if (fEnd>m_fStart && m_fStart>=0)
      {
        m_fStart=fStart;
        m_fEnd=fEnd;
      }
    }
		
		/// <summary>
		/// Get/set the interval for the float. 
		/// </summary>
    public float FloatInterval
    {
      get {return m_fInterval;}
      set {m_fInterval=value;}
    }
  
		/// <summary>
		/// Get the name of the background texture.
		/// </summary>
    public string BackGroundTextureName
    {
      get { return m_guiBackground.FileName;}
    }

		/// <summary>
		/// Get the name of the middle texture.
		/// </summary>
    public string BackTextureMidName
    {
      get { return m_guiMid.FileName;}
    }
 
		/// <summary>
		/// Perform an update after a change has occured. E.g. change to a new position.
		/// </summary>		
    protected override void Update()
    {
      m_guiBackground.SetPosition( XPosition, YPosition);
    }
	}
}
