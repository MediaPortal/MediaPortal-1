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
	/// The implementation of a selection button (e.g., the Switch View button in My Music).
	/// </summary>
	public class GUISelectButtonControl : GUIControl
	{
		[XMLSkinElement("textcolor")]		protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("disabledcolor")]	protected long  m_dwDisabledColor=0xFF606060;
		[XMLSkinElement("label")]			protected string	m_strLabel="";
		[XMLSkinElement("font")]			protected string	m_strFontName;
		[XMLSkinElement("textureFocus")]	protected string	m_strFocusTexture="";
		[XMLSkinElement("textureNoFocus")]	protected string	m_strNoFocusTexture="";
		[XMLSkinElement("texturebg")]		protected string    m_strBackground="";
		[XMLSkinElement("textureLeft")]		protected string	m_strLeft="";
		[XMLSkinElement("textureRight")]	protected string	m_strRight="";
		[XMLSkinElement("textureLeftFocus")]	protected string	m_strLeftFocus="";
		[XMLSkinElement("textureRightFocus")]	protected string	m_strRightFocus="";
		[XMLSkinElement("textXOff")]		protected int       m_iTextOffsetX=0;
		[XMLSkinElement("textYOff")]		protected int       m_iTextOffsetY=0;
		[XMLSkinElement("textXOff2")]		protected int       m_iTextOffsetX2=0;
		[XMLSkinElement("textYOff2")]		protected int       m_iTextOffsetY2=0;
											protected GUIImage	m_imgBackground=null;
											protected GUIImage	m_imgLeft=null;
											protected GUIImage	m_imgLeftFocus=null;
											protected GUIImage	m_imgRight=null;
											protected GUIImage	m_imgRightFocus=null;
											protected GUIImage	m_imgFocus=null;
											protected GUIImage	m_imgNoFocus=null;  
											protected GUIFont	m_pFont=null;
											protected bool		m_bShowSelect=false;

    
		protected int                     m_dwFrameCounter=0;
		
		

		protected int                     m_lHyperLinkWindowID=-1;
		protected string									m_strScriptAction="";
		protected int											m_iDefaultItem=-1;
		protected int											m_iStartFrame=0;
		protected bool										m_bLeftSelected=false;
		protected bool										m_bRightSelected=false;
		protected long										m_dwTicks=0;
		protected bool                    m_bUpdateNeeded=false;
		protected bool                    m_bAutoHide=true;
    protected GUILabelControl         m_label=null;
		protected bool										resetSelectionAfterFocusLost=true;
		
	

	  public GUISelectButtonControl(int dwParentID) : base(dwParentID)
	  {
	  }
		/// <summary>
		/// The constructor of the GUISelectButtonControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strButtonFocus">The filename containing the texture of the butten, when the button has the focus.</param>
		/// <param name="strButton">The filename containing the texture of the butten, when the button does not have the focus.</param>
		/// <param name="strSelectBackground">The background texture of the button.</param>
		/// <param name="strSelectArrowLeft">The texture of the left non-focused arrow.</param>
		/// <param name="strSelectArrowLeftFocus">The texture of the left focused arrow.</param>
		/// <param name="strSelectArrowRight">The texture of the right non-focused arrow.</param>
		/// <param name="strSelectArrowRightFocus">The texture of the right focused arrow.</param>
    public GUISelectButtonControl(int dwParentID, int dwControlId, 
                                  int dwPosX, int dwPosY, 
                                  int dwWidth, int dwHeight, 
                                  string strButtonFocus,  string strButton,
                                  string strSelectBackground,
                                  string strSelectArrowLeft,  string strSelectArrowLeftFocus,
                                  string strSelectArrowRight,  string strSelectArrowRightFocus)
          :base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
    {
		m_strFocusTexture = strButtonFocus;
		m_strNoFocusTexture = strButton;
		m_strBackground = strSelectBackground;
		m_strRight = strSelectArrowRight;
		m_strRightFocus = strSelectArrowRightFocus;
		m_strLeft = strSelectArrowLeft;
		m_strLeftFocus = strSelectArrowLeftFocus;
		m_bUpdateNeeded=false;
		m_bSelected=false;
		FinalizeConstruction();
    }
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction ();
		  int x1=16;
		  int y1=16;
		  GUIGraphicsContext.ScalePosToScreenResolution(ref x1,ref y1);
		  m_imgFocus		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strFocusTexture,0);
		  m_imgNoFocus		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strNoFocusTexture,0);
		  m_imgBackground	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strBackground,0);
		  m_imgLeft			= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, x1, y1,m_strLeft,0);
		  m_imgLeftFocus	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, x1, y1,m_strLeftFocus,0);
		  m_imgRight		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, x1, y1,m_strRight,0);
		  m_imgRightFocus	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY, x1, y1,m_strRightFocus,0);
		  if (m_strFontName!="" && m_strFontName!="-")
			  m_pFont=GUIFontManager.GetFont(m_strFontName);
		  GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
      m_imgFocus.Filtering=false;
      m_imgNoFocus.Filtering=false;
      m_imgBackground.Filtering=false;
      m_imgLeft.Filtering=false;
      m_imgLeftFocus.Filtering=false;
      m_imgRight.Filtering=false;
      m_imgRightFocus.Filtering=false;
      m_label=new GUILabelControl(m_dwParentID);
      m_label.CacheFont=true;
	  }
		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution ();
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextOffsetX, ref m_iTextOffsetY);
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextOffsetX2, ref m_iTextOffsetY2);
		}


		/// <summary>
		/// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
		/// some controls (for example the fadelabel) contain scrolling texts and need 2
		/// ne re-rendered constantly.
		/// </summary>
		/// <returns>true or false</returns>
		public override bool  NeedRefresh()
		{
			if (m_bShowSelect) return true;
			if (m_bUpdateNeeded)
			{
				m_bUpdateNeeded=false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }


			if (Focus) m_bShowSelect=true;
			else m_bShowSelect=false;

      //	Are we in selection mode
      if (m_bShowSelect)
      {
        //	Yes, render the select control

        //	render background, left and right arrow

        m_imgBackground.Render(timePassed);

        long dwTextColor=m_dwTextColor;

        //	User has moved left...
        if(m_bLeftSelected)
        {
          //	...render focused arrow
          m_iStartFrame++;
          if(m_bAutoHide && m_iStartFrame>=25)
          {
            m_iStartFrame=0;
            m_bLeftSelected=false;
						GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
						GUIWindowManager.SendThreadMessage(message);
						m_bUpdateNeeded=true;
					}
          m_imgLeftFocus.Render(timePassed);

          //	If we are moving left
          //	render item text as disabled
          dwTextColor=m_dwDisabledColor;
        }
        else
        {
          //	Render none focused arrow
          m_imgLeft.Render(timePassed);
        }


        //	User has moved right...
        if(m_bRightSelected)
        {
          //	...render focused arrow
          m_iStartFrame++;
          if(m_bAutoHide && m_iStartFrame>=25)
          {
            m_iStartFrame=0;
            m_bRightSelected=false;
						GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
						GUIWindowManager.SendThreadMessage(message);
						m_bUpdateNeeded=true;
					}
          m_imgRightFocus.Render(timePassed);

          //	If we are moving right
          //	render item text as disabled
          dwTextColor=m_dwDisabledColor;
        }
        else
        {
          //	Render none focused arrow
          m_imgRight.Render(timePassed);
        }


        //	Render text if a current item is available
        if (SelectedItem>=0 && null!=m_pFont && SelectedItem < m_SubItems.Count)
        {
          m_label.FontName=m_pFont.FontName;
          m_label.SetPosition(m_dwPosX+m_imgLeft.Width+m_iTextOffsetX,m_iTextOffsetY+m_dwPosY);
          m_label.TextColor=dwTextColor;
          m_label.Label=(string)m_SubItems[SelectedItem];
					m_label.Width=m_dwWidth - (m_imgRight.Width+m_imgLeft.Width+m_iTextOffsetX);
          m_label.Render(timePassed);
        }
/*
        //	Select current item, if user doesn't 
        //	move left or right for 1.5 sec.
        long dwTicksSpan=DateTime.Now.Ticks-m_dwTicks;
				dwTicksSpan/=10000;
        if ((float)(dwTicksSpan/1000)>0.8f)
        {
          //	User hasn't moved disable selection mode...
          m_bShowSelect=false;

          //	...and send a thread message.
          //	(Sending a message with SendMessage 
          //	can result in a GPF.)
          GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
          GUIWindowManager.SendThreadMessage(message);
          m_bUpdateNeeded=true;
        }
*/					
        
      }	//	if (m_bShowSelect)
      else
      {
        //	No, render a normal button

        if (!IsVisible ) return;

        if (Focus)
        {/*
          int dwAlphaCounter = m_dwFrameCounter+2;
          int dwAlphaChannel;
          if ((dwAlphaCounter%128)>=64)
            dwAlphaChannel = dwAlphaCounter%64;
          else
            dwAlphaChannel = 63-(dwAlphaCounter%64);

          dwAlphaChannel += 192;
          SetAlpha(dwAlphaChannel );
          m_imgFocus.IsVisible=true;
          m_imgNoFocus.IsVisible=false;
          m_dwFrameCounter++;*/
          m_imgFocus.Render(timePassed);
        }
        else 
        {
          //SetAlpha(0xff);
          m_imgNoFocus.Render(timePassed); 
        }

        if (m_strLabel!=null&&m_strLabel.Length > 0 && m_pFont!=null)
        {
          m_label.FontName=m_pFont.FontName;
          m_label.SetPosition(m_iTextOffsetX2+m_dwPosX,m_iTextOffsetY2+m_dwPosY);
          if (Disabled || m_SubItems.Count==0)
            m_label.TextColor=m_dwDisabledColor;
          else
              m_label.TextColor=m_dwTextColor;
          m_label.Label=m_strLabel;
          m_label.Render(timePassed);
        }
      }
    }

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
    public override void OnAction( Action action) 
    {
      GUIMessage message;
      if (!m_bShowSelect)
      {
        if (Focus)
        {
          if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
          {
            //	Enter selection mode
            m_bShowSelect=true;

            //	Start timer, if user doesn't select an item
            //	or moves left/right. The control will 
            //	automatically select the current item.
            m_dwTicks=DateTime.Now.Ticks;
            return;
          }
        }
      }
      else
      {
				if (action.wID == Action.ActionType.ACTION_SELECT_ITEM)
				{
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
					GUIWindowManager.SendThreadMessage(msg);
					return;
				}
        if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK)
        {
          if (m_bRightSelected)
          {
            action.wID=Action.ActionType.ACTION_MOVE_RIGHT;
            OnAction(action);
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
						GUIWindowManager.SendThreadMessage(msg);
						m_bUpdateNeeded=true;
						return;
          }
          else if (m_bLeftSelected)
          {
						action.wID=Action.ActionType.ACTION_MOVE_LEFT;
						OnAction(action);
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
						GUIWindowManager.SendThreadMessage(msg);
						m_bUpdateNeeded=true;
						return;
          }
          else
          {
            //	User has selected an item, disable selection mode...
            m_bShowSelect=false;

            // ...and send a message.
            message=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID ,0,0,null);
            GUIGraphicsContext.SendMessage(message);
            return;
          }
        }
        else if (action.wID == Action.ActionType.ACTION_MOVE_LEFT)
        {
          //	Set for visual feedback
          m_bLeftSelected=true;
          m_bRightSelected=false;
          m_iStartFrame=0;

          //	Reset timer for automatically selecting
          //	the current item.
          m_dwTicks=DateTime.Now.Ticks;

          //	Switch to previous item
          if (m_SubItems.Count>0)
          {
            SelectedItem--;
            if (SelectedItem<0) 
              SelectedItem=m_SubItems.Count-1;
          }
          return;
        }
        else if (action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
        {
          //	Set for visual feedback
          m_bRightSelected=true;
          m_bLeftSelected=false;
          m_iStartFrame=0;

          //	Reset timer for automatically selecting
          //	the current item.
          m_dwTicks=DateTime.Now.Ticks;

          //	Switch to next item
          if (m_SubItems.Count>0)
          {
            SelectedItem++;
            if (SelectedItem>=(int)m_SubItems.Count) 
              SelectedItem=0;
          }
          return;
        }
        if (action.wID == Action.ActionType.ACTION_MOVE_UP || action.wID == Action.ActionType.ACTION_MOVE_DOWN )
        {
          //	Disable selection mode when moving up or down
          m_bShowSelect=false;
					if (resetSelectionAfterFocusLost)
					{
						SelectedItem=m_iDefaultItem;
					}
        }

      }

      base.OnAction(action);
      
      if (Focus)
      {
        if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (m_strScriptAction.Length > 0)
          {
            message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID,0,0,null);
            message.Label=m_strScriptAction;
            //g_actionManager.CallScriptAction(message); // TODO!
          }

          if (m_lHyperLinkWindowID >=0)
          {
            GUIWindowManager.ActivateWindow((int)m_lHyperLinkWindowID);
            return;
          }
          // button selected.
          // send a message
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID,0,0,null );
          GUIGraphicsContext.SendMessage(message);
        }
      }
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
      if ( message.TargetControlId==GetID )
      {
					// Adds an item to the list.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
					Add(message.Label);
        }
					// Resets the list of items.
        else if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
					Clear();
        }
					// Gets the selected item.
        else if (message.Message==GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          if (SelectedItem>=0 && SelectedItem < m_SubItems.Count)
          {
            message.Param1=SelectedItem;
            message.Label=(string)m_SubItems[SelectedItem];
          }
          else
          {
            message.Param1=-1;
            message.Label="";
          }
        }
					// Selects an item.
        else if (message.Message==GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          m_iDefaultItem=SelectedItem=(int)message.Param1;
        }
      }
				// Sets the label of the control.
      if ( message.TargetControlId==GetID )
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label!=null)
            m_strLabel=message.Label ;

          return true;
        }
      }
      if (base.OnMessage(message)) return true;
      return false;
    }

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources()
    {
			base.FreeResources();
			m_imgFocus.FreeResources();
			m_imgNoFocus.FreeResources();
			m_imgBackground.FreeResources();

			m_imgLeft.FreeResources();
			m_imgLeftFocus.FreeResources();

			m_imgRight.FreeResources();
			m_imgRightFocus.FreeResources();

      
      m_label.FreeResources();
			m_bShowSelect=false;
		}

    public override bool CanFocus()
    {
      if (!IsVisible) return false;
      if (Disabled) return false;
      if (m_SubItems.Count<2) return false;
      return true;
    }

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_imgFocus.PreAllocResources();
      m_imgNoFocus.PreAllocResources();
      m_imgBackground.PreAllocResources();

      m_imgLeft.PreAllocResources();
      m_imgLeftFocus.PreAllocResources();

      m_imgRight.PreAllocResources();
      m_imgRightFocus.PreAllocResources();
    }

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      m_dwFrameCounter=0;
      m_imgFocus.AllocResources();
      m_imgNoFocus.AllocResources();
      m_dwWidth=m_imgFocus.Width;
      m_dwHeight=m_imgFocus.Height;
      m_imgBackground.AllocResources();
	
      m_imgLeft.AllocResources();
      m_imgLeftFocus.AllocResources();

      m_imgRight.AllocResources();
      m_imgRightFocus.AllocResources();

      m_pFont=GUIFontManager.GetFont(m_strFontName);

      //	Position right arrow
			int x1=8;
			int x2=16;
			GUIGraphicsContext.ScaleHorizontal(ref x1);
			GUIGraphicsContext.ScaleHorizontal(ref x2);
      int dwPosX=(m_dwPosX+m_dwWidth-x1) - x2;

			int y1=16;
			GUIGraphicsContext.ScaleVertical(ref y1);
      int dwPosY=m_dwPosY+(m_dwHeight-y1)/2;
      m_imgRight.SetPosition(dwPosX,dwPosY);
      m_imgRightFocus.SetPosition(dwPosX,dwPosY);

      //	Position left arrow
      dwPosX=m_dwPosX+x1;
      m_imgLeft.SetPosition(dwPosX, dwPosY);
      m_imgLeftFocus.SetPosition(dwPosX, dwPosY);

      m_label.AllocResources();
    }

		/// <summary>
		/// Sets the position of the control.
		/// </summary>
		/// <param name="dwPosX">The X position.</param>
		/// <param name="dwPosY">The Y position.</param>	
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (dwPosX<0) return;
      if (dwPosY<0) return;
      base.SetPosition(dwPosX, dwPosY);
      m_imgFocus.SetPosition(dwPosX, dwPosY);
      m_imgNoFocus.SetPosition(dwPosX, dwPosY);
    }

		/// <summary>
		/// Changes the alpha transparency component of the colordiffuse.
		/// </summary>
		/// <param name="dwAlpha">The new value of the colordiffuse.</param>
    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      m_imgFocus.SetAlpha(dwAlpha);
      m_imgNoFocus.SetAlpha(dwAlpha);
    }

		/// <summary>
		/// Get/set the color of the text when the GUIButtonControl is disabled.
		/// </summary>
    public long DisabledColor
    {
      get { return m_dwDisabledColor;}
      set {m_dwDisabledColor=value;}
    }

		/// <summary>
		/// Gets the name of the texture for the unfocused button.
		/// </summary>
    public string TexutureNoFocusName
    { 
      get { return m_imgNoFocus.FileName;} 
    }

		/// <summary>
		/// Gets the name of the texture for the unfocused button.
		/// </summary>
		public string TexutureFocusName
    { 
      get {return m_imgFocus.FileName;} 
    }
		
		/// <summary>
		/// Set the color of the text on the GUIButtonControl. 
		/// </summary>
    public long	TextColor 
    { 
      get { return m_dwTextColor;}
      set { m_dwTextColor=value;}
    }

		/// <summary>
		/// Get the fontname of the label.
		/// </summary>
    public string FontName
    { 
      get { return m_pFont.FontName; }
      set 
      {   
        if (value ==null) return;
        m_pFont.FontName=value;}
    }

		/// <summary>
		/// Set the text of the control. 
		/// </summary>
		/// <param name="strFontName">The font name.</param>
		/// <param name="strLabel">The text.</param>
		/// <param name="dwColor">The font color.</param>
    public void SetLabel( string strFontName,string strLabel,long dwColor)
    {
      if (strLabel ==null) return;
      if (strFontName ==null) return;
      m_strLabel=strLabel;
      m_dwTextColor=dwColor;
      m_pFont=GUIFontManager.GetFont(strFontName);
    }

		/// <summary>
		/// Get/set the text of the control.
		/// </summary>
    public string Label
    { 
      get { return m_strLabel; }
      set 
      {  
        if (value ==null) return;
        m_strLabel=value;
      }
    }

		/// <summary>
		/// Get/set the window ID to which the GUIButtonControl links.
		/// </summary>
    public int HyperLink
    { 
      get { return m_lHyperLinkWindowID;}
      set {m_lHyperLinkWindowID=value;}
    }

		/// <summary>
		/// Get/set the scriptaction that needs to be performed when the button is clicked.
		/// </summary>
    public string ScriptAction  
    { 
      get { return m_strScriptAction; }
      set { m_strScriptAction=value; }
    }

		/// <summary>
		/// Perform an update after a change has occured. E.g. change to a new position.
		/// </summary>
    protected override void  Update() 
    {
      base.Update();
  
      m_imgFocus.Width=m_dwWidth;
      m_imgFocus.Height=m_dwHeight;

      m_imgNoFocus.Width=m_dwWidth;
      m_imgNoFocus.Height=m_dwHeight;
      m_imgBackground.Width=m_dwWidth;
      m_imgBackground.Height=m_dwHeight;


      m_imgFocus.SetPosition(m_dwPosX, m_dwPosY);
      m_imgNoFocus.SetPosition(m_dwPosX, m_dwPosY);
      m_imgBackground.SetPosition(m_dwPosX, m_dwPosY);

    }

		/// <summary>
		/// Gets the name of the left texture for the unfocused button.
		/// </summary>
    public string TextureLeft
    { 
      get { return m_imgLeft.FileName;}
    }

		/// <summary>
		/// Gets the name of the left texture for the focused button.
		/// </summary>
    public string TextureLeftFocus  
    { 
      get { return m_imgLeftFocus.FileName;}
    }

		/// <summary>
		/// Gets the name of the right texture for the unfocused button.
		/// </summary>
    public string TextureRight
    { 
      get { return m_imgRight.FileName;}
    }

		/// <summary>
		/// Gets the name of the right texture for the focused button.
		/// </summary>
    public string TextureRightFocus  
    { 
      get { return m_imgRightFocus.FileName;}
    }

		/// <summary>
		/// Gets the name of the background texture for the button.
		/// </summary>
    public string TextureBackground
    { 
      get {return m_imgBackground.FileName;}
    }

		/// <summary>
		/// Checks if the x and y coordinates correspond to the current control.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>True if the control was hit.</returns>
    public override bool HitTest(int x,int y,out int controlID, out bool focused)
    {
      controlID=GetID;
      focused=Focus;
      m_bAutoHide=true;
      // first check if mouse is within bounds of button
			if (x < m_imgBackground.XPosition || x > m_imgBackground.XPosition+m_imgBackground.RenderWidth ||
				  y < m_imgBackground.YPosition || y > m_imgBackground.YPosition+m_imgBackground.RenderHeight ) 
			{
				return false;
			}

      //yes it is
      // check if control is selected

      m_bAutoHide=false;
      // control is selected
      // first check left button

			if (x >= m_imgLeftFocus.XPosition && x <= m_imgLeftFocus.XPosition+m_imgLeftFocus.RenderWidth)
			{
				if (y >= m_imgLeftFocus.YPosition && y <= m_imgLeftFocus.YPosition+m_imgLeftFocus.RenderHeight)
				{
          if (!m_bLeftSelected)
          {
            m_bLeftSelected=true;
            m_bRightSelected=false;
            m_dwTicks=DateTime.Now.Ticks;
            m_iStartFrame=0;
          }
					return true;
				}
        else m_bLeftSelected=false;
      }
      else m_bLeftSelected=false;

      // check right button
			if (x >= m_imgRightFocus.XPosition && x <= m_imgRightFocus.XPosition+m_imgRightFocus.RenderWidth)
			{
				if (y >= m_imgRightFocus.YPosition && y <= m_imgRightFocus.YPosition+m_imgRightFocus.RenderHeight)
				{
          if (!m_bRightSelected)
          {
            m_bRightSelected=true;
            m_bLeftSelected=false;
            m_dwTicks=DateTime.Now.Ticks;
            m_iStartFrame=0;
            return true;
          }
          return true;
        }
        else m_bRightSelected=false;
			}
			else m_bRightSelected=false;
			return true;
		}
    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX
    {
      get { return m_iTextOffsetX;}
      set 
      {  
        if (value < 0) return;
        m_iTextOffsetX=value;
      }
    }
    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY
    {
      get { return m_iTextOffsetY;}
      set 
      {  
        if (value < 0) return;
        m_iTextOffsetY=value;
      }
		}
		/// <summary>
		/// Get/set the X-offset of the label.
		/// </summary>
		public int TextOffsetX2
		{
			get { return m_iTextOffsetX2;}
      set 
      {  
        if (value < 0) return;
        m_iTextOffsetX2=value;
      }
		}
		/// <summary>
		/// Get/set the Y-offset of the label.
		/// </summary>
		public int TextOffsetY2
		{
			get { return m_iTextOffsetY2;}
			set { 
        if (value < 0) return;
        m_iTextOffsetY2=value;
      }
		}
		public string SelectedLabel
		{
			get
			{
				if (SelectedItem>=0 && SelectedItem < m_SubItems.Count)
				{
					return (string)m_SubItems[SelectedItem];
				}
				return String.Empty;
			}
		}
		public void Clear()
		{
			m_SubItems.Clear();
			SelectedItem=-1;
			m_iDefaultItem=-1;
		}
		public void Add(string line)
		{
			if (m_SubItems.Count<=0)
			{
				SelectedItem=0;
				m_iDefaultItem=0;
			}
			m_SubItems.Add( line);
		}
		public bool RestoreSelection
		{
			get
			{
				return resetSelectionAfterFocusLost;
			}
			set
			{
				resetSelectionAfterFocusLost=value;
			}
		}
	}
}
