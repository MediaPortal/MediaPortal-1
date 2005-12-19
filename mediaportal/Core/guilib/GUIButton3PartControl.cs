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
using System.Drawing;
using System.Diagnostics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class implementing a button which consists of 3 parts
	/// a left part, a middle part and a right part
	/// These are presented as [ Left Middle Right ]
	/// Each part has 2 images, 
	/// 1 for the normal state
	/// and 1 for the focused state
	/// Further the button can have an image (icon) which can be positioned 
	/// 
	/// </summary>
	public class GUIButton3PartControl : GUIControl
	{
		//TODO: make use of GUILabelControl to draw all text
		protected GUIImage               m_imgFocusLeft=null;
    protected GUIImage               m_imgNoFocusLeft=null;  
    protected GUIImage               m_imgFocusMid=null;
    protected GUIImage               m_imgNoFocusMid=null;  
    protected GUIImage               m_imgFocusRight=null;
    protected GUIImage               m_imgNoFocusRight=null;
    protected GUIImage               m_imgIcon=null;  
    protected string								 m_strLabel1="";
    protected string								 m_strLabel2="";
    protected string                 fontName1=String.Empty;
    protected string    						 fontName2=String.Empty;
    protected long  								 m_dwTextColor1=(long)0xFFFFFFFF;
    protected long  								 m_dwTextColor2=(long)0xFFFFFFFF;
		protected long  								 m_dwDisabledColor=(long)0xFF606060;
		protected int                    m_lHyperLinkWindowID=-1;
    protected int                    m_iAction=-1;
		protected string								 m_strScriptAction="";
    protected int                    m_iTextOffsetX1=10;
    protected int                    m_iTextOffsetY1=2;
    protected int                    m_iTextOffsetX2=10;
    protected int                    m_iTextOffsetY2=2;
    protected string                 m_strText1;
    protected string                 m_strText2;  
    protected string                 m_strApplication="";
    protected string                 m_strArguments="";
    protected int                    m_iIconOffsetX=-1;
    protected int                    m_iIconOffsetY=-1;
    protected int                    m_iIconWidth=-1;
    protected int                    m_iIconHeight=-1;
    protected bool                   m_bIconKeepAspectRatio=false;
    protected bool                   m_bIconCentered=false;
    protected bool                   m_bIconZoom=false;
    GUILabelControl                  cntlLabel1=null;
    GUILabelControl                  cntlLabel2=null;
    bool                             containsProperty1=false;
    bool                             containsProperty2=false;
		bool														 renderLeftPart=true;
		bool														 renderRightPart=true;
    //Sprite                           sprite=null;
    
		/// <summary>
		/// The constructor of the GUIButton3PartControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
		/// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
		public GUIButton3PartControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,  
                                  string strTextureFocusLeft, 
                                  string strTextureFocusMid, 
                                  string strTextureFocusRight, 
                                  string strTextureNoFocusLeft,
                                  string strTextureNoFocusMid,
                                  string strTextureNoFocusRight,
                                  string strTextureIcon)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
      m_imgIcon        =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,0, 0, strTextureIcon,0);
			m_imgFocusLeft   =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureFocusLeft,0);
      m_imgFocusMid    =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureFocusMid,0);
      m_imgFocusRight  =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureFocusRight,0);
      m_imgNoFocusLeft =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureNoFocusLeft,0);
      m_imgNoFocusMid  =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureNoFocusMid,0);
      m_imgNoFocusRight=new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureNoFocusRight,0);
      m_bSelected=false;
      cntlLabel1 = new GUILabelControl(dwParentID);
      cntlLabel2 = new GUILabelControl(dwParentID);
		}
  
		/// <summary>
		/// Renders the GUIButton3PartControl.
		/// </summary>
		public override void Render(float timePassed)
		{
			// Do not render if not visible.
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }
      m_strText1=m_strLabel1;
      m_strText2=m_strLabel2;
      if (containsProperty1) m_strText1=GUIPropertyManager.Parse(m_strLabel1);
      if (containsProperty2) m_strText2=GUIPropertyManager.Parse(m_strLabel2);

//      sprite.Begin(SpriteFlags.None);
			// if the GUIButton3PartControl has the focus
			if (Focus)
			{
				//render the focused images
				//if (m_imgIcon!=null) GUIFontManager.Present();//TODO:not nice. but needed for the tvguide
				if (renderLeftPart) m_imgFocusLeft.Render(timePassed);
				m_imgFocusMid.Render(timePassed);
				if (renderRightPart) m_imgFocusRight.Render(timePassed);
				GUIPropertyManager.SetProperty("#highlightedbutton", m_strText1);
			}
			else 
			{
				//else render the non-focus images
				//if (m_imgIcon!=null) GUIFontManager.Present();//TODO:not nice. but needed for the tvguide
				if (renderLeftPart) m_imgNoFocusLeft.Render(timePassed);  		
				m_imgNoFocusMid.Render(timePassed);  		
				if (renderRightPart) m_imgNoFocusRight.Render(timePassed);
      }

			//render the icon
      if (m_imgIcon!=null) 
      {
        m_imgIcon.Render(timePassed);
      }
//      sprite.End();

			// render the 1st line of text on the button
      int iWidth=m_imgNoFocusMid.Width-10-m_iTextOffsetX1;
			if (iWidth<=0) iWidth=1;
			if (m_imgNoFocusMid.IsVisible && m_strText1.Length>0 )
			{
        int widthLeft =(int)((float)m_imgFocusLeft.TextureWidth * ((float)m_dwHeight/(float)m_imgFocusLeft.TextureHeight));
        int xoff=m_iTextOffsetX1+widthLeft ;

        if (Disabled )
          cntlLabel1.TextColor=m_dwDisabledColor;
        else
          cntlLabel1.TextColor=m_dwTextColor1;
        cntlLabel1.SetPosition(xoff+m_dwPosX,m_iTextOffsetY1+m_dwPosY);
        cntlLabel1.TextAlignment=GUIControl.Alignment.ALIGN_LEFT;
        cntlLabel1.FontName=fontName1;
        cntlLabel1.Label=m_strText1;
        cntlLabel1.Width=iWidth;
        cntlLabel1.Render(timePassed);
			}
     
			// render the 2nd line of text on the button
			if (m_imgNoFocusMid.IsVisible && m_strText2.Length>0)
      {
        int widthLeft =(int)((float)m_imgFocusLeft.TextureWidth * ((float)m_dwHeight/(float)m_imgFocusLeft.TextureHeight));
        int xoff=m_iTextOffsetX2+widthLeft;

        if (Disabled )
          cntlLabel2.TextColor=m_dwDisabledColor;
        else
          cntlLabel2.TextColor=m_dwTextColor2;
        cntlLabel2.SetPosition(xoff+m_dwPosX,m_iTextOffsetY2+m_dwPosY);
        cntlLabel2.TextAlignment=GUIControl.Alignment.ALIGN_LEFT;
        cntlLabel2.FontName=fontName1;
        cntlLabel2.Label=m_strText2;
        cntlLabel2.Width=iWidth-10;
        cntlLabel2.Render(timePassed);
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
			base.OnAction(action);
			GUIMessage message ;
      if (Focus)
      {
				//is the button clicked?
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
					// yes,
					//If this button contains scriptactions call the scriptactions.
          if (m_strApplication.Length!=0)
          {
							//button should start an external application, so start it
              Process proc = new Process();
              string strWorkingDir=System.IO.Path.GetFullPath(m_strApplication);
              string strFileName=System.IO.Path.GetFileName(m_strApplication);
              strWorkingDir=strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length+1) );
              proc.StartInfo.FileName=strFileName;
              proc.StartInfo.WorkingDirectory=strWorkingDir;
              proc.StartInfo.Arguments=m_strArguments;
              proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
	            proc.StartInfo.CreateNoWindow=true;
              proc.Start();
              //proc.WaitForExit();
          }

					// If this links to another window go to the window.
          if (m_lHyperLinkWindowID >=0)
          {
						//then switch to the other window
            GUIWindowManager.ActivateWindow((int)m_lHyperLinkWindowID);
            return;
          }

					// If this button corresponds to an action generate that action.
          if (ActionID >=0)
          {
            Action newaction = new Action((Action.ActionType)ActionID,0,0);
            GUIGraphicsContext.OnAction(newaction);
            return;
          }

          // button selected.
          // send a message to the parent window
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
			// Handle the GUI_MSG_LABEL_SET message
			if ( message.TargetControlId==GetID )
			{
				if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
				{
          if (message.Label!=null)
          {
            m_strLabel1=message.Label;
            containsProperty1=ContainsProperty(m_strLabel1);
          }
					return true;
				}
			}
			// Let the base class handle the other messages
			if (base.OnMessage(message)) return true;
			return false;
		}

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
		public override void PreAllocResources()
		{
			base.PreAllocResources();
			m_imgFocusLeft.PreAllocResources();
      m_imgFocusMid.PreAllocResources();
      m_imgFocusRight.PreAllocResources();
      m_imgNoFocusLeft.PreAllocResources();
      m_imgNoFocusMid.PreAllocResources();
      m_imgNoFocusRight.PreAllocResources();
      m_imgIcon.PreAllocResources();
    }
		
		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
		public override void AllocResources()
		{
			base.AllocResources();
			m_imgFocusLeft.AllocResources();
      m_imgFocusMid.AllocResources();
      m_imgFocusRight.AllocResources();
      m_imgNoFocusLeft.AllocResources();
      m_imgNoFocusMid.AllocResources();
      m_imgNoFocusRight.AllocResources();
      m_imgIcon.AllocResources();

      cntlLabel1.AllocResources();
      cntlLabel2.AllocResources();
//      sprite=new Sprite(GUIGraphicsContext.DX9Device);
		}

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources()
		{
			base.FreeResources();
			m_imgFocusLeft.FreeResources();
      m_imgFocusMid.FreeResources();
      m_imgFocusRight.FreeResources();
      m_imgNoFocusLeft.FreeResources();
      m_imgNoFocusMid.FreeResources();
      m_imgNoFocusRight.FreeResources();
      m_imgIcon.FreeResources();

      
      cntlLabel1.FreeResources();
      cntlLabel2.FreeResources();
      /*if (sprite!=null)
      {
        if (!sprite.Disposed) sprite.Dispose();
        sprite=null;
      }*/
    }

		/// <summary>
		/// Get/set the color of the text when the GUIButton3PartControl is disabled.
		/// </summary>
		public long DisabledColor
		{
			get { return m_dwDisabledColor;}
			set {m_dwDisabledColor=value;}
		}
		
		/// <summary>
		/// Get the filename of the texture when the GUIButton3PartControl does not have the focus.
		/// </summary>
		public string TexutureNoFocusLeftName
		{ 
      get { return m_imgNoFocusLeft.FileName;} 
      set 
      {
        if (value==null) return;
        m_imgNoFocusLeft.SetFileName(value);
      }
		}
    public string TexutureNoFocusMidName
    { 
      get { return m_imgNoFocusMid.FileName;} 
      set 
      {
        if (value==null) return;
        m_imgNoFocusMid.SetFileName(value);
      }
    }
    public string TexutureNoFocusRightName
    { 
      get { return m_imgNoFocusRight.FileName;} 
      set 
      {
        if (value==null) return;
        m_imgNoFocusRight.SetFileName(value);
      }
    }

		/// <summary>
		/// Get the filename of the texture when the GUIButton3PartControl has the focus.
		/// </summary>
		public string TexutureFocusLeftName
		{ 
      get {return m_imgFocusLeft.FileName;} 
      set 
      {
        if (value==null) return;
        m_imgFocusLeft.SetFileName(value);
      }
		}
    public string TexutureFocusMidName
    { 
      get {return m_imgFocusMid.FileName;} 
      set 
      {
        if (value==null) return;
        m_imgFocusMid.SetFileName(value);
      }
    }
    public string TexutureFocusRightName
    { 
      get 
      {
        return m_imgFocusRight.FileName;
      } 
      set { 
        if (value==null) return;
        m_imgFocusRight.SetFileName(value);
      }
    }

		/// <summary>
		/// Get/set the filename of the icon texture
		/// </summary>
    public string TexutureIcon
    {
      get { 
				if (m_imgIcon==null) return String.Empty;
				return m_imgIcon.FileName;
			}
      set
      {
        if (m_imgIcon!=null)
        {
					m_imgIcon.SetFileName(value);
					m_imgIcon.Width=m_iIconWidth;
					m_imgIcon.Height=m_iIconHeight;
					Update();
        }
      }
    }
		
		/// <summary>
		/// Set the color of the text on the GUIButton3PartControl. 
		/// </summary>
		public long	TextColor1
		{ 
			get { return m_dwTextColor1;}
			set { m_dwTextColor1=value;}
		}
    public long	TextColor2
    { 
      get { return m_dwTextColor2;}
      set { m_dwTextColor2=value;}
    }

		/// <summary>
		/// Get/set the name of the font of the text of the GUIButton3PartControl.
		/// </summary>
		public string FontName1
		{ 
			get { 
        return fontName1;
			}
			set { 
				if (value==null) return;
				fontName1=value;
			}
		}

    public string FontName2
    { 
      get { 
				return fontName2; 
			}
      set { 
				if (value==null) return;
				fontName2=value;
			}
    }

		/// <summary>
		/// Set the text of the GUIButton3PartControl. 
		/// </summary>
		/// <param name="strFontName">The font name.</param>
		/// <param name="strLabel">The text.</param>
		/// <param name="dwColor">The font color.</param>
		public void SetLabel1( string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null) return;
      if (strLabel==null) return;
			m_strLabel1=strLabel;
			m_dwTextColor1=dwColor;
      fontName1=strFontName;
      containsProperty1=ContainsProperty(m_strLabel1);
		}
    public void SetLabel2( string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null) return;
      if (strLabel==null) return;
      m_strLabel2=strLabel;
      m_dwTextColor2=dwColor;
      fontName2=strFontName;
      containsProperty2=ContainsProperty(m_strLabel2);
    }

		/// <summary>
		/// Get/set the text of the GUIButton3PartControl.
		/// </summary>
		public string Label1
		{ 
			get { return m_strLabel1; }
      set 
      { 
        if (value==null) return;
        m_strLabel1=value;
        containsProperty1=ContainsProperty(m_strLabel1);
      }
    }
    public string Label2
    { 
      get { return m_strLabel2; }
      set 
      { 
        if (value==null) return;
        m_strLabel2=value;
        containsProperty2=ContainsProperty(m_strLabel2);
      }
    }

		/// <summary>
		/// Get/set the window ID to which the GUIButton3PartControl links.
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
      set 
      { 
        if (value==null) return;
        m_strScriptAction=value; 
      }
		}

		/// <summary>
		/// Get/set the action ID that corresponds to this button.
		/// </summary>
    public int ActionID
    {
      get { return m_iAction;}
      set { m_iAction=value;}

    }

    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX1
    {
      get { return m_iTextOffsetX1;}
      set 
      { 
        if (value<0) return;
        m_iTextOffsetX1=value;
      }
    }
    public int TextOffsetX2
    {
      get { return m_iTextOffsetX2;}
      set 
      { 
        if (value<0) return;
        m_iTextOffsetX2=value;
      }
    }
    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY1
    {
      get { return m_iTextOffsetY1;}
      set 
      { 
        if (value<0) return;
        m_iTextOffsetY1=value;
      }
    }
    public int TextOffsetY2
    {
      get { return m_iTextOffsetY2;}
      set { 
        if (value<0) return;
        m_iTextOffsetY2=value;
      }
    }

		/// <summary>
		/// Perform an update after a change has occured. E.g. change to a new position.
		/// </summary>
		protected override void  Update() 
		{
			base.Update();
  
      m_imgFocusLeft.ColourDiffuse=ColourDiffuse;
      m_imgFocusMid.ColourDiffuse=ColourDiffuse;
      m_imgFocusRight.ColourDiffuse=ColourDiffuse;

      m_imgNoFocusLeft.ColourDiffuse=ColourDiffuse;
      m_imgNoFocusMid.ColourDiffuse=ColourDiffuse;
      m_imgNoFocusRight.ColourDiffuse=ColourDiffuse;      
      
			m_imgFocusLeft.Height =m_dwHeight;
			m_imgFocusMid.Height  =m_dwHeight;
			m_imgFocusRight.Height=m_dwHeight;
      
      int width;

      int widthLeft =(int)((float)m_imgFocusLeft.TextureWidth * ((float)m_dwHeight/(float)m_imgFocusLeft.TextureHeight));
      int widthRight=(int)((float)m_imgFocusRight.TextureWidth * ((float)m_dwHeight/(float)m_imgFocusRight.TextureHeight));
      int widthMid = m_dwWidth - widthLeft - widthRight;
      if (widthMid < 0) widthMid=0;

      while(true)
      {
				width=widthLeft+widthRight+widthMid;
        if (width > m_dwWidth)
        {
          if (widthMid>0) widthMid--;
          else 
          {
            if (widthLeft>0) widthLeft--;
            if (widthRight>0) widthRight--;
          }
        }
        else break;
      } 

			m_imgFocusLeft.Width=widthLeft;
			m_imgFocusMid.Width=widthMid;
			m_imgFocusRight.Width=widthRight;
			if (widthLeft==0) m_imgFocusLeft.IsVisible=false;
			else m_imgFocusLeft.IsVisible=true;
			
			if (widthMid==0) m_imgFocusMid.IsVisible=false;
			else m_imgFocusMid.IsVisible=true;

			if (widthRight==0) m_imgFocusRight.IsVisible=false;
			else m_imgFocusRight.IsVisible=true;

			m_imgNoFocusLeft.Width=widthLeft;
			m_imgNoFocusMid.Width=widthMid;
			m_imgNoFocusRight.Width=widthRight;
			if (widthLeft==0) m_imgNoFocusLeft.IsVisible=false;
			else m_imgNoFocusLeft.IsVisible=true;
			
			if (widthMid==0) m_imgNoFocusMid.IsVisible=false;
			else m_imgNoFocusMid.IsVisible=true;

			if (widthRight==0) m_imgNoFocusRight.IsVisible=false;
			else m_imgNoFocusRight.IsVisible=true;

      m_imgFocusLeft.SetPosition (m_dwPosX, m_dwPosY);
      m_imgFocusMid.SetPosition  (m_dwPosX + widthLeft, m_dwPosY);
      m_imgFocusRight.SetPosition(m_dwPosX + m_dwWidth - widthRight, m_dwPosY);


      m_imgNoFocusLeft.SetPosition (m_dwPosX, m_dwPosY);
      m_imgNoFocusMid.SetPosition  (m_dwPosX + widthLeft, m_dwPosY);
      m_imgNoFocusRight.SetPosition(m_dwPosX + m_dwWidth - widthRight, m_dwPosY);



			if (m_imgIcon!=null)
			{
        m_imgIcon.KeepAspectRatio=m_bIconKeepAspectRatio;
        m_imgIcon.Centered=m_bIconCentered;
        m_imgIcon.Zoom=m_bIconZoom;
				if (IconOffsetY<0 || IconOffsetX<0)
				{
          int iWidth=m_imgIcon.TextureWidth;
          if (iWidth>=m_dwWidth)
          {
            m_imgIcon.Width=m_dwWidth;
            iWidth=m_dwWidth;
          }
          int offset=(iWidth + iWidth/2);
          if (offset > m_dwWidth) offset=m_dwWidth;
					m_imgIcon.SetPosition(m_dwPosX+(m_dwWidth)  - offset,
																m_dwPosY+(m_dwHeight/2) - (m_imgIcon.TextureHeight/2) );
				}
				else
				{
					m_imgIcon.SetPosition(m_dwPosX+IconOffsetX,m_dwPosY+IconOffsetY );
				}
        
			}
    }

		public void Refresh()
		{
			Update();
		}

    /// <summary>
    /// Get/Set the icon to be zoomed into the dest. rectangle
    /// </summary>
    public bool IconZoom
    {
      get { return m_bIconZoom; }
      set { m_bIconZoom = value; }
    }
    /// <summary>
    /// Get/Set the icon to keep it's aspectratio in the dest. rectangle
    /// </summary>
    public bool IconKeepAspectRatio
    {
      get { return m_bIconKeepAspectRatio; }
      set { m_bIconKeepAspectRatio = value; }
    }
    /// <summary>
    /// Get/Set the icon centered in the dest. rectangle
    /// </summary>
    public bool IconCentered
    {
      get { return m_bIconCentered; }
      set { m_bIconCentered = value; }
    }
    /// <summary>
		/// Get/Set the the application filename
		/// which should be launched when this button gets clicked
		/// </summary>
	  public string Application
	  {
	    get { return m_strApplication; }
      set 
      { 
        if (m_strApplication==null) return;
        m_strApplication = value; 
      }
	  }

		/// <summary>
		/// Get/Set the arguments for the application
		/// which should be launched when this button gets clicked
		/// </summary>
	  public string Arguments
	  {
	    get { return m_strArguments; }
	    set { 
        if (m_strArguments==null) return;
        m_strArguments = value; 
      }
	  }
	
		/// <summary>
		/// Get/Set the x-position of the icon
		/// </summary>
    public int IconOffsetX
    {
      get { return m_iIconOffsetX; }
      set { m_iIconOffsetX = value; }
    }
	
		/// <summary>
		/// Get/Set the y-position of the icon
		/// </summary>
    public int IconOffsetY
    {
      get { return m_iIconOffsetY; }
      set { m_iIconOffsetY= value; }
    }
	
		/// <summary>
		/// Get/Set the width of the icon
		/// </summary>
    public int IconWidth
    {
      get { return m_iIconWidth; }
      set 
      { 
        m_iIconWidth= value; 
        if (m_imgIcon!=null) m_imgIcon.Width=m_iIconWidth;
      }
    }
		/// <summary>
		/// Get/Set the height of the icon
		/// </summary>
    public int IconHeight
    {
      get { return m_iIconHeight; }
      set { 
        if (value<0) return;
        m_iIconHeight= value; 
        if (m_imgIcon!=null) m_imgIcon.Height=m_iIconHeight;
      }
    }
    bool ContainsProperty(string text)
    {
      if (text==null) return false;
      if (text.IndexOf("#")>=0) return true;
      return false;
    }
		public bool RenderLeft
		{
			get { return renderLeftPart;}
			set {renderLeftPart=value;}
		}
		public bool RenderRight
		{
			get { return renderRightPart;}
			set {renderRightPart=value;}
		}
  }
}
