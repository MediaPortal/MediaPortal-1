using System;
using System.Drawing;
using System.Diagnostics;

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
		protected int                    m_dwFrameCounter=0;
    protected string								 m_strLabel1="";
    protected string								 m_strLabel2="";
    protected GUIFont 							 m_pFont1=null;
    protected GUIFont 							 m_pFont2=null;
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
		}
  
		/// <summary>
		/// Renders the GUIButton3PartControl.
		/// </summary>
		public override void Render()
		{
			// Do not render if not visible.
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }
      m_strText1=GUIPropertyManager.Parse(m_strLabel1);
      m_strText2=GUIPropertyManager.Parse(m_strLabel2);

			// if the GUIButton3PartControl has the focus
			if (Focus)
			{
				//render the focused images
				m_imgFocusLeft.Render();
				m_imgFocusMid.Render();
				m_imgFocusRight.Render();
        m_dwFrameCounter++;
			}
			else 
			{
				//else render the non-focus images
				m_imgNoFocusLeft.Render();  		
				m_imgNoFocusMid.Render();  		
				m_imgNoFocusRight.Render();
      }

			//render the icon
      if (m_imgIcon!=null) m_imgIcon.Render();

			// render the 1st line of text on the button
      int iWidth=m_imgNoFocusMid.Width;
			if (m_strText1.Length > 0 && m_pFont1!=null)
			{
        int xoff=m_iTextOffsetX1+m_imgNoFocusLeft.TextureWidth;
				if (Disabled )
					m_pFont1.DrawTextWidth((float)xoff+m_dwPosX, (float)m_iTextOffsetY1+m_dwPosY,m_dwDisabledColor,m_strText1,iWidth, GUIControl.Alignment.ALIGN_LEFT);
				else
					m_pFont1.DrawTextWidth((float)xoff+m_dwPosX, (float)m_iTextOffsetY1+m_dwPosY,m_dwTextColor2,m_strText1,iWidth, GUIControl.Alignment.ALIGN_LEFT);
			}
      
			// render the 2nd line of text on the button
			if (m_strText2.Length > 0 && m_pFont2!=null)
      {
        int xoff=m_iTextOffsetX2+m_imgNoFocusLeft.TextureWidth;
        if (Disabled )
          m_pFont2.DrawTextWidth((float)xoff+m_dwPosX, (float)m_iTextOffsetY2+m_dwPosY,m_dwDisabledColor,m_strText2,iWidth, GUIControl.Alignment.ALIGN_LEFT);
        else
          m_pFont2.DrawTextWidth((float)xoff+m_dwPosX, (float)m_iTextOffsetY2+m_dwPosY,m_dwTextColor2,m_strText2,iWidth, GUIControl.Alignment.ALIGN_LEFT);
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
					m_strLabel1=message.Label;
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
			m_dwFrameCounter=0;
			m_imgFocusLeft.AllocResources();
      m_imgFocusMid.AllocResources();
      m_imgFocusRight.AllocResources();
      m_imgNoFocusLeft.AllocResources();
      m_imgNoFocusMid.AllocResources();
      m_imgNoFocusRight.AllocResources();
      m_imgIcon.AllocResources();
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
      set { m_imgNoFocusLeft.SetFileName(value);}
		}
    public string TexutureNoFocusMidName
    { 
      get { return m_imgNoFocusMid.FileName;} 
      set { m_imgNoFocusMid.SetFileName(value);}
    }
    public string TexutureNoFocusRightName
    { 
      get { return m_imgNoFocusRight.FileName;} 
      set { m_imgNoFocusRight.SetFileName(value);}
    }

		/// <summary>
		/// Get the filename of the texture when the GUIButton3PartControl has the focus.
		/// </summary>
		public string TexutureFocusLeftName
		{ 
      get {return m_imgFocusLeft.FileName;} 
      set { m_imgFocusLeft.SetFileName(value);}
		}
    public string TexutureFocusMidName
    { 
      get {return m_imgFocusMid.FileName;} 
      set { m_imgFocusMid.SetFileName(value);}
    }
    public string TexutureFocusRightName
    { 
      get {return m_imgFocusRight.FileName;} 
      set { m_imgFocusRight.SetFileName(value);}
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
					m_imgIcon.SetFileName(value);
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
				if (m_pFont1==null) return String.Empty;
				return m_pFont1.FontName; 
			}
			set { 
				if (value==null) return;
				if (value==String.Empty) return;
				m_pFont1 = GUIFontManager.GetFont(value);
			}
		}

    public string FontName2
    { 
      get { 
				if (m_pFont2==null) return String.Empty;
				return m_pFont2.FontName; 
			}
      set { 
				if (value==null) return;
				if (value==String.Empty) return;
				m_pFont2 = GUIFontManager.GetFont(value);
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
			m_strLabel1=strLabel;
			m_dwTextColor1=dwColor;
      if (strFontName!="" && strFontName!="-")
			  m_pFont1=GUIFontManager.GetFont(strFontName);
		}
    public void SetLabel2( string strFontName,string strLabel,long dwColor)
    {
      m_strLabel2=strLabel;
      m_dwTextColor2=dwColor;
      if (strFontName!="" && strFontName!="-")
        m_pFont2=GUIFontManager.GetFont(strFontName);
    }

		/// <summary>
		/// Get/set the text of the GUIButton3PartControl.
		/// </summary>
		public string Label1
		{ 
			get { return m_strLabel1; }
			set { m_strLabel1=value;}
    }
    public string Label2
    { 
      get { return m_strLabel2; }
      set { m_strLabel2=value;}
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
			set { m_strScriptAction=value; }
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
      set { m_iTextOffsetX1=value;}
    }
    public int TextOffsetX2
    {
      get { return m_iTextOffsetX2;}
      set { m_iTextOffsetX2=value;}
    }
    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY1
    {
      get { return m_iTextOffsetY1;}
      set { m_iTextOffsetY1=value;}
    }
    public int TextOffsetY2
    {
      get { return m_iTextOffsetY2;}
      set { m_iTextOffsetY2=value;}
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
      
      int width;
      while(true)
      {
        m_imgFocusLeft.Width =m_imgFocusLeft.TextureWidth;
        m_imgFocusMid.Width  =m_dwWidth - (m_imgFocusLeft.TextureWidth+m_imgFocusRight.TextureWidth);
        m_imgFocusRight.Width=m_imgFocusRight.TextureWidth;
  
        width=m_imgFocusLeft.Width +m_imgFocusMid.Width+m_imgFocusRight.Width;
        if (width > m_dwWidth)
        {
          if (m_imgFocusMid.Width>0) m_imgFocusMid.Width--;
          else 
          {
            if (m_imgFocusLeft.Width>0) m_imgFocusLeft.Width--;
            if (m_imgFocusRight.Width>0) m_imgFocusRight.Width--;
          }
        }
        else break;
      } 
      
      m_imgFocusLeft.Height =m_dwHeight;
      m_imgFocusMid.Height  =m_dwHeight;
      m_imgFocusRight.Height=m_dwHeight;

			m_imgNoFocusLeft.ColourDiffuse =ColourDiffuse;
      m_imgNoFocusMid.ColourDiffuse  =ColourDiffuse;
      m_imgNoFocusRight.ColourDiffuse=ColourDiffuse;

      while (true)
      {
        m_imgNoFocusLeft.Width =m_imgNoFocusLeft.TextureWidth;
        m_imgNoFocusMid.Width  =m_dwWidth - (m_imgNoFocusLeft.TextureWidth+m_imgNoFocusRight.TextureWidth);
        m_imgNoFocusRight.Width=m_imgNoFocusRight.TextureWidth;

        width=m_imgNoFocusLeft.Width +m_imgNoFocusMid.Width+m_imgNoFocusRight.Width;
        if (width > m_dwWidth)
        {
          if (m_imgNoFocusMid.Width>0) m_imgNoFocusMid.Width--;
          else 
          {
            if (m_imgNoFocusLeft.Width>0) m_imgNoFocusLeft.Width--;
            if (m_imgNoFocusRight.Width>0) m_imgNoFocusRight.Width--;
          }
        }
        else break;
      }

      m_imgNoFocusLeft.Height =m_dwHeight;
      m_imgNoFocusMid.Height  =m_dwHeight;
      m_imgNoFocusRight.Height=m_dwHeight;

      m_imgFocusLeft.SetPosition (m_dwPosX, m_dwPosY);
      m_imgFocusMid.SetPosition  (m_dwPosX +m_imgFocusLeft.TextureWidth, m_dwPosY);
      m_imgFocusRight.SetPosition(m_dwPosX +m_dwWidth-m_imgFocusRight.TextureWidth, m_dwPosY);


      m_imgNoFocusLeft.SetPosition (m_dwPosX, m_dwPosY);
      m_imgNoFocusMid.SetPosition  (m_dwPosX +m_imgFocusLeft.TextureWidth, m_dwPosY);
      m_imgNoFocusRight.SetPosition(m_dwPosX +m_dwWidth-m_imgFocusRight.TextureWidth, m_dwPosY);

			m_imgFocusLeft.DoUpdate();
			m_imgFocusMid.DoUpdate();
			m_imgFocusRight.DoUpdate();
			m_imgNoFocusLeft.DoUpdate();
			m_imgNoFocusMid.DoUpdate();
			m_imgNoFocusRight.DoUpdate();
      


			if (m_imgIcon!=null)
			{
				if (m_dwWidth<m_imgIcon.TextureWidth)
					m_imgIcon.IsVisible=false;
				else
					m_imgIcon.IsVisible=true;

				if (IconOffsetY<0 || IconOffsetX<0)
				{
					m_imgIcon.SetPosition(m_dwPosX+(m_dwWidth)  - (m_imgIcon.TextureWidth + m_imgIcon.TextureWidth/2),
																m_dwPosY+(m_dwHeight/2) - (m_imgIcon.TextureHeight/2) );
				}
				else
				{
					m_imgIcon.SetPosition(m_dwPosX+IconOffsetX,m_dwPosY+IconOffsetY );
				}
				m_imgIcon.DoUpdate();
			}
    }

		public void Refresh()
		{
			Update();
		}

		/// <summary>
		/// Get/Set the the application filename
		/// which should be launched when this button gets clicked
		/// </summary>
	  public string Application
	  {
	    get { return m_strApplication; }
	    set { m_strApplication = value; }
	  }

		/// <summary>
		/// Get/Set the arguments for the application
		/// which should be launched when this button gets clicked
		/// </summary>
	  public string Arguments
	  {
	    get { return m_strArguments; }
	    set { m_strArguments = value; }
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
      set { 
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
        m_iIconHeight= value; 
        if (m_imgIcon!=null) m_imgIcon.Height=m_iIconHeight;
      }
    }
  }
}
