using System;
using System.Collections;
using System.Diagnostics;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using System.Globalization;
using System.Reflection;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Home
{
	/// <summary>
	/// The implementation of the HomeWindow.  (This window is coupled to the home.xml skin file).
	/// </summary>
	public class HomeWindow : GUIWindow
	{
		enum State
		{
			Idle,
			ScrollUp,
			ScrollDown
		}
		enum Controls:int 
		{
			TemplateHoverImage=1000,
			TemplateButton=1001,
			TemplatePanel=1002,
			TemplateFontLabel=1003
		}
		private int		m_iDateLayout=0; //0=dd-mm-yyyy, 1=mm-dd-yyyy
		private int		m_iButtons=0;
		private int		m_iCurrentButton=0;
		private State m_eState=State.Idle;
		private int   m_iFrame=0;
		private int   m_iStep=1;
		private int   m_iTimes=1;
    private State m_keepState=State.Idle;
		private int   m_iVisibleItems=0;
		private int   m_iOffset=0;
		private int   m_iOffset1=0;
		private int   m_iOffset2=0;
		private int   m_iMiddle=0;
		const int			MAX_FRAMES=9;
    bool          m_bTopBar=false;
		int[]         m_iButtonIds = new int[50];
    DateTime      m_updateTimer=DateTime.MinValue;
    int           m_iMaxHeight;    
    int           m_iMaxWidth ;    
    int           m_iStartXoff;    
    int           m_iStartYoff;    
    int           m_iButtonHeight; 
    bool          m_bAllowScroll=true;
    Viewport      m_newviewport = new Viewport();
    Viewport      m_oldviewport;
    bool          m_bSkipFirstMouseMove=true;

	//Tracking controls by id
	System.Collections.ArrayList m_aryPreControlList = new ArrayList();
	System.Collections.ArrayList m_aryPostControlList = new ArrayList();	
		
		/// <summary>
		/// Constructs the home window and set its ID.
		/// </summary>
		public HomeWindow()
		{
			GetID=(int)GUIWindow.Window.WINDOW_HOME;
		}

		/// <summary>
		/// Initialization of the home window based on the home.xml skin.
		/// </summary>
		/// <returns>A bool containing true if the initialization was perfomed correctly.</returns>
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\home.xml");			
		}

		/// <summary>
		/// OnWindowLoaded() gets called when the window is fully loaded and all controls are initialized
		/// In this home plugin, its now time to add the button for each dynamic plugin
		/// </summary>
		protected override void OnWindowLoaded()
		{

			base.OnWindowLoaded();
			m_iButtons=0;
			// add buttons for dynamic plugins
      ArrayList plugins=PluginManager.SetupForms;
      ProcessPlugins(ref plugins);
			ProcessPlugins(ref plugins);
			ProcessPlugins(ref plugins);
      plugins=null;
			m_iCurrentButton=m_iButtons/2;
			LayoutButtons(0);
			GUIControl.SetControlLabel(GetID, 200,GUIPropertyManager.GetProperty("#date") ); 	 
			GUIControl.SetControlLabel(GetID, 201,GUIPropertyManager.GetProperty("#time") );

      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
		}
		
    private void OnGlobalMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ASKYESNO:
          string Head="",Line1="",Line2="",Line3="";;
					if (message.Param1!=0) Head=GUILocalizeStrings.Get(message.Param1);
					else if (message.Label!=String.Empty) Head=message.Label;
					if (message.Param2!=0) Line1=GUILocalizeStrings.Get(message.Param2);
					else if (message.Label2!=String.Empty) Line1=message.Label2;
					if (message.Param3!=0) Line2=GUILocalizeStrings.Get(message.Param3);
					else if (message.Label3!=String.Empty) Line2=message.Label3;
					if (message.Param4!=0) Line3=GUILocalizeStrings.Get(message.Param4);
					else if (message.Label4!=String.Empty) Line3=message.Label4;
          if ( AskYesNo(Head,Line1,Line2,Line3))
            message.Param1=1;
          else
            message.Param1=0;
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_WARNING:
        {
          string strHead="",strLine1="",strLine2="";
          if (message.Param1!=0) strHead=GUILocalizeStrings.Get(message.Param1);
					else if (message.Label!=String.Empty) strHead=message.Label;
					if (message.Param2!=0) strLine1=GUILocalizeStrings.Get(message.Param2);
					else if (message.Label2!=String.Empty) strLine2=message.Label2;
          if (message.Param3!=0) strLine2=GUILocalizeStrings.Get(message.Param3);
					else if (message.Label3!=String.Empty) strLine2=message.Label3;
          ShowInfo(strHead,strLine1,strLine2);
        }
          break;

        case GUIMessage.MessageType.GUI_MSG_GET_STRING : 
          VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard) return;
          keyboard.Reset();
          keyboard.Text = message.Label;
          keyboard.DoModal(GUIWindowManager.ActiveWindow);
          if (keyboard.IsConfirmed)
          {
            message.Label = keyboard.Text;
          }
          else message.Label = "";
          break;

        case GUIMessage.MessageType.GUI_MSG_GET_PASSWORD: 
          VirtualKeyboard keyboard2 = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard2) return;
          keyboard2.Reset();
          keyboard2.Password=true;
          keyboard2.Text = message.Label;
          keyboard2.DoModal(GUIWindowManager.ActiveWindow);
          if (keyboard2.IsConfirmed)
          {
            message.Label = keyboard2.Text;
          }
          else message.Label = "";
          break;
      }
    }
    void ShowInfo(string strHeading, string strLine1, string strLine2)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
      pDlgOK.SetHeading(strHeading);
      pDlgOK.SetLine(1,strLine1);
      pDlgOK.SetLine(2,strLine2);
      pDlgOK.SetLine(3,"");
      pDlgOK.DoModal( GUIWindowManager.ActiveWindow);

    }

    bool AskYesNo(string strHeading, string strLine1, string strLine2,string strLine3)
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      dlgYesNo.SetHeading(strHeading);
      dlgYesNo.SetLine(1,strLine1);
      dlgYesNo.SetLine(2,strLine2);
      dlgYesNo.SetLine(3,strLine3);
      dlgYesNo.DoModal( GUIWindowManager.ActiveWindow);
      return dlgYesNo.IsConfirmed;

    }
		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				GUIWindowManager.PreviousWindow();
				return;
			}

      // mouse moved, check which control has the focus
      if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE )
      {
        if (m_bSkipFirstMouseMove) 
        {
          m_bSkipFirstMouseMove=false;
          return;
        }
        int x=(int)action.fAmount1;
        int y=(int)action.fAmount2;
        if (x < m_iStartXoff  || x > m_iStartXoff+m_iMaxWidth)
        {
          m_bTopBar=true;
          return;
        }
        if (y < m_iStartYoff  || y > m_iStartYoff+m_iMaxHeight)
        {
          GUIControl cntl=GetControl(base.GetFocusControlId());
          if (cntl!=null) cntl.Focus=false;
          m_bTopBar=true;
          return;
        }
        if (m_bTopBar)
        {
          GUIControl cntl=GetControl(base.GetFocusControlId());
          if (cntl!=null) cntl.Focus=false;
          m_bTopBar=false;
        }

        if (m_bAllowScroll)
        {
          int iMid=(m_iMaxHeight/2) - ((m_iButtonHeight)/2);
          iMid += m_iStartYoff;

          if (x >=m_iStartXoff && x <= m_iStartXoff+m_iMaxWidth)
          {
            bool bOK=false;
            if (y >= m_iStartYoff && y <= m_iStartYoff+m_iButtonHeight) bOK=true;
            if (y >= m_iStartYoff+m_iMaxHeight-m_iButtonHeight && y <= m_iStartYoff+m_iMaxHeight) bOK=true;
            if (bOK)
            {
              int iOff=y-iMid;
              if (iOff<0) 
              {
                m_keepState=State.ScrollUp;
                m_eState=m_keepState;
              }
              else 
              {
                m_keepState=State.ScrollDown;
                m_eState=m_keepState;
              }
              return;
            }
            else 
            {
              m_keepState=State.Idle;
              if (y >= m_iStartYoff && y <= m_iStartYoff+m_iMaxHeight) 
              {
                for (int i=0; i < m_iVisibleItems;++i)
                {
                  GUIButtonControl button = GetControl( m_iButtonIds[i]) as GUIButtonControl;
                  if (y >=button.YPosition && y <= button.YPosition+button.Height)
                  {
                    m_iOffset=i-m_iMiddle;
                    //Trace.WriteLine(String.Format("offset:{0}", m_iOffset));
                    break;
                  }
                }
              }
            }
          }
          else 
          {
            // calculate offset
            m_keepState=State.Idle;
  					
          }
        }
      }

      if (action.wID==Action.ActionType.ACTION_MOVE_LEFT||action.wID==Action.ActionType.ACTION_MOVE_RIGHT)
      {
        //FOCUS TOPBAR
        action.wID=Action.ActionType.ACTION_MOVE_UP;
        m_bTopBar=true;
        GUIControl cntl=GetControl(base.GetFocusControlId());
        if (cntl!=null) cntl.Focus=false;
        return;
      }

			if (action.wID==Action.ActionType.ACTION_MOVE_DOWN)
			{	
        if (m_bTopBar)
        {
          m_bTopBar=false;
          FocusControl(GetID,m_iButtonIds[m_iOffset+m_iMiddle]);
          return;
        }
				if (m_eState!=State.Idle)
				{
					if (m_iStep+1 <MAX_FRAMES) m_iStep++;
					if (m_iTimes<4) m_iTimes++;
					return;
				}
				
				if (m_iOffset+m_iMiddle+3< m_iVisibleItems)
				{
					m_iOffset++;
					
					int iID=GetFocusControlId()+1;
					if (iID >1+m_iButtons) iID=2;
					FocusControl(GetID, iID);
					return;
				}
        if (m_bAllowScroll)
        {
          m_iTimes=1;
          m_iStep=1;
          m_iFrame=0;
          m_eState=State.ScrollDown;
        }
				return;
			}

			if (action.wID==Action.ActionType.ACTION_MOVE_UP)
      {
        if (m_bTopBar)
        {
          m_bTopBar=false;
          FocusControl(GetID,m_iButtonIds[m_iOffset+m_iMiddle]);
          return;
        }
				if (m_eState!=State.Idle)
				{
					if (m_iStep+1 <MAX_FRAMES) m_iStep++;
					if (m_iTimes<4) m_iTimes++;
					return;
				}
				if (m_iOffset+m_iMiddle-1>1)
				{
					m_iOffset--;
					int iID=GetFocusControlId()-1;
					if (iID <2) iID=m_iButtons+1;
					FocusControl(GetID, iID);
					return;
				}
        
        if (m_bAllowScroll)
        {
          m_iTimes=1;
          m_iStep=1;
          m_iFrame=0;
          m_eState=State.ScrollUp;
        } 
				return;
			}
			base.OnAction (action);
		}

		/// <summary>
		/// OnMessage() This method gets called when there's a new message. 
		/// </summary>
		/// <param name="message">An instance of the GUIMessage class containing the message.</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				// Initialization of the window
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					base.OnMessage(message);

          m_aryPreControlList.Clear();
          m_aryPostControlList.Clear();
					m_iMaxHeight    = GetControl( (int)Controls.TemplatePanel).Height;
					m_iMaxWidth     = GetControl( (int)Controls.TemplatePanel).Width;
					m_iStartXoff    = GetControl( (int)Controls.TemplatePanel).XPosition;
					m_iStartYoff    = GetControl( (int)Controls.TemplatePanel).YPosition;
					m_iButtonHeight = GetControl( (int)Controls.TemplateButton).Height;

					m_bTopBar=false;
					// make controls 101-120 invisible... (these are the subpictures shown for each button)
					for (int iControl=102; iControl < 160; iControl++)
					{
						GUIControl.HideControl(GetID, iControl);
					}

					VerifyButtonIndex(ref m_iCurrentButton);
					
					LayoutButtons(0);

					if (m_iOffset!=0)
					{
						FocusControl(GetID,m_iButtonIds[m_iOffset+m_iMiddle]);
					}
					else
					{
						int buttonIndex = m_iCurrentButton;

						//
						// Verify the button index
						//
						VerifyButtonIndex(ref buttonIndex);
						
						//
						// Focus the currently selected control
						//
						FocusControl(GetID, buttonIndex + 2);
					}

					using(AMS.Profile.Xml xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
					{
						m_iDateLayout = xmlreader.GetValueAsInt("home","datelayout",0);
            m_bAllowScroll= xmlreader.GetValueAsBool("home","scroll",true);
					}
          m_bSkipFirstMouseMove=true;
					return true;
				}
				
				// Sets the focus for the controls that are on the window.
				// if the focus changed, then show the correct sub-picture
				case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
				{
					int iControl=message.TargetControlId;
					if (iControl>=2 && iControl <=60)
					{
						// make pictures/controls 101-120 invisible...
						for (int i=102; i < 160; i++)
						{
							GUIControl.HideControl(GetID, i);
						}

			            // and only show the picture belonging to the button which has the focus
						GUIControl.ShowControl(GetID, iControl+100);
					}
				}
				break;

			}
			return base.OnMessage(message);
		}

		/// <summary>
		/// Makes sure that the button index lies within acceptable values.
		/// </summary>
		/// <param name="?"></param>
		private void VerifyButtonIndex(ref int buttonIndex)
		{
			//
			// Don't do any verification if we don't have any buttons to work with
			//
			if(m_iButtons > 0)
			{
				//
				// If the button index has passed the total number of buttons, remove the total number
				// of buttons from the index. Example: Button Index = 6, Total Buttons = 5, Calculate 6 - 5, new Button Index = 1
				//
				while(buttonIndex >= m_iButtons)
					buttonIndex -= m_iButtons;

				//
				// Make sure the calculated button index doesn't become a negative number
				//
				while(buttonIndex < 0) 
					buttonIndex += m_iButtons;
			}
		}

		/// <summary>
		/// Renders the home window.
		/// </summary>
		public override void Render()
		{

			if (m_eState!=State.Idle)
			{
				State newState=Scroll();
				if (newState==State.Idle)
				{
					if (m_eState==State.ScrollDown)
					{
						m_iCurrentButton++;
					}
					else if (m_eState==State.ScrollUp)
					{
						m_iCurrentButton--;
					}

					LayoutButtons(0);

          if (m_bTopBar) m_keepState=State.Idle;
					m_eState = m_keepState;
					m_iFrame = 0;
				}
			}
			
			if (m_aryPreControlList.Count==0) 
			{
				for (int x=0; x < m_vecControls.Count;++x)
				{
					GUIControl control=(GUIControl)m_vecControls[x];
					if ((control.GetID<2 || control.GetID>60))
					{
						m_aryPreControlList.Add(control);
					}
				}
			}
			IEnumerator enumControls = m_aryPreControlList.GetEnumerator();
      while (enumControls.MoveNext())
      {
        ((GUIControl)enumControls.Current).Render();  
      }

			m_oldviewport=GUIGraphicsContext.DX9Device.Viewport;
			m_newviewport.X      = m_iStartXoff+GUIGraphicsContext.OffsetX;
			m_newviewport.Y      = m_iStartYoff+GUIGraphicsContext.OffsetY;
			m_newviewport.Width  = (int)(m_iMaxWidth);
			m_newviewport.Height = (int)(m_iMaxHeight);
			m_newviewport.MinZ   = 0.0f;
			m_newviewport.MaxZ   = 1.0f;
			GUIGraphicsContext.DX9Device.Viewport=m_newviewport;
      if (m_aryPostControlList.Count==0) 
      {	
        for (int x=0; x < m_vecControls.Count;++x)
        {
          GUIControl control=(GUIControl)m_vecControls[x];
          if (control.GetID>=2 && control.GetID<=60)
          {
            m_aryPostControlList.Add(control);
          }
        }
      }
			enumControls = m_aryPostControlList.GetEnumerator();
      while (enumControls.MoveNext())
      {
        GUIControl cntl=((GUIControl)enumControls.Current);
        if (cntl.YPosition >= m_iStartYoff && cntl.YPosition < (m_iStartYoff+m_iMaxHeight))
        {
          cntl.Render();
        }
      }
			GUIGraphicsContext.DX9Device.Viewport=m_oldviewport;

		}


    void ProcessPlugins(ref ArrayList plugins)
    {
      foreach (ISetupForm setup in plugins)
      {
        using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
        {
          bool bHomeDefault=setup.DefaultEnabled();
          bool inhome=xmlreader.GetValueAsBool("home", setup.PluginName(), bHomeDefault);
          if (!inhome) continue;
        }
        Trace.WriteLine(String.Format("plugin:{0}",setup.PluginName()) );
        string strButtonText;
        string strButtonImage;
        string strButtonImageFocus;
        string strPictureImage;
        if (setup.GetHome(out strButtonText,out strButtonImage,out strButtonImageFocus, out strPictureImage))
        {
          string strPluginName=setup.PluginName();
          string strBtnFile;
          if (strButtonImage=="")
          {
            strButtonImage=String.Format("buttonnf_{0}.png", strPluginName);
            strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strButtonImage);
            if (!System.IO.File.Exists(strBtnFile))
            {
              strButtonImage="";
            }
          }
													
          if (strButtonImageFocus=="")
          {
            strButtonImageFocus=String.Format("button_{0}.png", strPluginName);
            strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strButtonImageFocus);
            if (!System.IO.File.Exists(strBtnFile))
            {
              strButtonImageFocus="";
            }
          }

          if (strPictureImage=="")
          {
            strPictureImage=String.Format("hover_{0}.png", strPluginName);
            strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strPictureImage);
            if (!System.IO.File.Exists(strBtnFile))
            {
              strPictureImage="";
            }
          }
          int iHyperLink = setup.GetWindowId();
          AddPluginButton(iHyperLink,strButtonText, strButtonImageFocus,  strButtonImage,strPictureImage);
        }
      }
    }
		
		public void AddPluginButton(int iHyperLink,string strButtonText, string strButtonImageFocus, string strButtonImage,  string strPictureImage)
		{

			if (strButtonImage.Length==0)
				strButtonImage= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TexutureNoFocusName;
			if (strButtonImageFocus.Length==0)
				strButtonImageFocus= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TexutureFocusName;

			string strFontName= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).FontName;
			long   lFontColor = ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TextColor;
			long   lDisabledColor = ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).DisabledColor;
			int xpos  =GetControl( (int)Controls.TemplateButton).XPosition;
			int width =GetControl( (int)Controls.TemplateButton).Width;
			int height=GetControl( (int)Controls.TemplateButton).Height;
			int ypos  =GetControl( (int)Controls.TemplateButton).YPosition;
			int iSpaceBetween =8;
			for (int iButtonId=2; iButtonId < 60; iButtonId++)
			{
				GUIControl cntl = GetControl(iButtonId) as GUIControl;
				if (cntl==null)
				{
					//found it, add the button
					if (iButtonId>2)
						GetControl(iButtonId-1).NavigateDown=iButtonId;
					ypos+=( (iButtonId-2 )*(iSpaceBetween+height) ) ;
					GUIImage img;

					GUIButtonControl button= new GUIButtonControl(GetID,iButtonId,xpos,ypos,width,height,strButtonImageFocus,strButtonImage);
					button.Label=strButtonText;
					button.HyperLink=iHyperLink;
					button.FontName=strFontName;
					button.DisabledColor=lDisabledColor;
					button.TextColor=lFontColor;
          button.TextOffsetX= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TextOffsetX;
					button.TextOffsetY= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TextOffsetY;
					button.ColourDiffuse= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).ColourDiffuse;
					button.SetNavigation(iButtonId-1,2,iButtonId,iButtonId);

          //Trace.WriteLine(String.Format("id:{0} btn:{1}", iButtonId,strButtonText));
					
					button.AllocResources();
					GUIControl btnControl = (GUIControl) button;
					Add(ref btnControl);

					xpos = GetControl((int)Controls.TemplateHoverImage).XPosition;
					ypos = GetControl((int)Controls.TemplateHoverImage).YPosition;
          GUIImage hoverimg=GetControl((int)Controls.TemplateHoverImage) as GUIImage;


					img = new GUIImage(GetID,iButtonId+100,xpos,ypos,width,height,strPictureImage,0);
					img.AllocResources();
          width = GetControl( (int)Controls.TemplateHoverImage).Width;
          if (width == 0) width = img.TextureWidth; 
          height = GetControl( (int)Controls.TemplateHoverImage).Height;
          if (height == 0) height = img.TextureHeight;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          GUIGraphicsContext.ScaleVertical(ref height);
          img.Width=width;
          img.Height=height;
          btnControl = (GUIControl) img;
					Add(ref btnControl);
					m_iButtons++;
					return;

				}
			}
		}

		void LayoutButtons(int iPercentage)
		{
			//
			// Don't perform any layout if we don't have any buttons to layout
			//
			if(m_iButtons == 0)
				return;

			// todo:
			// - pressing keys fast is acting weird, and when skipping to the next item the current item is gone for a small period
			// - musicoverlay scrolling is slower now?
			for (int i=0; i < m_iButtons;++i)
			{
				GUIButtonControl button = GetControl(i+2) as GUIButtonControl;
				button.IsVisible=false;
			}
			int iStartYoff    = m_iStartYoff;
			long lTextColor		= (GetControl( (int)Controls.TemplateButton) as GUIButtonControl).TextColor;
			long lDiffuseColor= (GetControl( (int)Controls.TemplateButton) as GUIButtonControl).ColourDiffuse;
			int iSpaceBetween = 5;

			string strNormalFont= (GetControl( (int)Controls.TemplateButton) as GUIButtonControl).FontName;
			string strSmallFont = (GetControl( (int)Controls.TemplateFontLabel) as GUILabelControl).FontName;

			float fYOff = ((float)iPercentage) * ((float)(m_iButtonHeight+iSpaceBetween));
			fYOff/=100f;
			if (m_eState==State.ScrollUp)
			{
				iStartYoff += (int)(fYOff);
			}
			if (m_eState==State.ScrollDown)
			{
				iStartYoff -= (int)(fYOff);
			}


			lTextColor		= lTextColor & 0x00FFFFFF;
			lDiffuseColor = lDiffuseColor& 0x00FFFFFF;
			int iMaxItems = (m_iMaxHeight+iSpaceBetween)/(m_iButtonHeight+iSpaceBetween);

			int iMid=(m_iMaxHeight/2) - ((m_iButtonHeight)/2);
			iMid += iStartYoff;

			iMaxItems/=2;
			iMaxItems++;
			int iTel=0;
			int iButton=m_iCurrentButton; 

			VerifyButtonIndex(ref iButton);
			
			m_iVisibleItems=0;
			m_iMiddle=iMaxItems;

			while (iTel <= iMaxItems)
			{
				GUIButtonControl button = GetControl(iButton+2) as GUIButtonControl;

				if(button != null)
				{
					m_iButtonIds[iMaxItems-iTel]=button.GetID;
					if (iTel==0 && m_eState!=State.Idle)
					{
						//FocusControl(GetID,iButton+2);
					}
					float fPos=iTel;
					if (m_eState==State.ScrollDown)
					{
						fPos=iTel;
						fPos += ( ((float)iPercentage)/100f );
					}
					if (m_eState==State.ScrollUp)
					{
						fPos=iTel;
						fPos -= ( ((float)iPercentage)/100f );
					}
					float fPercent = 1f - ((fPos) / ((float)(iMaxItems+1)));
					if (fPercent >=1.0f) fPercent=(2.0f-fPercent);
					button.SetAlpha( (int)(fPercent * 255f));

					
					long lAlpha=(long)(fPercent * 255f);
					lAlpha<<=24;
					button.TextColor		= (lTextColor+lAlpha);
					button.ColourDiffuse= (lDiffuseColor+lAlpha);

					int xpos=button.XPosition;
					button.SetPosition(xpos, iMid - (iTel* (m_iButtonHeight+iSpaceBetween) ) );
					if (iTel==0) button.FontName=strNormalFont;
					else button.FontName=strSmallFont;											 

					button.Refresh();
					button.IsVisible=true;
					if (iTel==iMaxItems-1) m_iOffset1=button.GetID;
					iButton--;
					if (iButton<0) iButton=m_iButtons-1;
					m_iVisibleItems++;
				}

				iTel++;
			}

			//------------------------------------------------------------------------------
			iTel=1;
			iButton=m_iCurrentButton+1;

			VerifyButtonIndex(ref iButton);

			while (iTel <= iMaxItems)
			{
				GUIButtonControl button = GetControl(iButton+2) as GUIButtonControl;

				if(button != null)
				{
					m_iButtonIds[iMaxItems+iTel]=button.GetID;

					float fPos=iTel;
					if (m_eState==State.ScrollDown)
					{
						fPos=iTel;
						fPos -= ( ((float)iPercentage)/100f );
					}
					if (m_eState==State.ScrollUp)
					{
						fPos=iTel;
						fPos += ( ((float)iPercentage)/100f );
					}
					float fPercent = 1f - ((fPos) / ((float)(iMaxItems+1)));


					//				button.Height = (int) (fPercent * ((float)m_iButtonHeight));
					button.SetAlpha( (int)(fPercent * 255f) );
					
					long lAlpha=(long)(fPercent * 255f);
					lAlpha<<=24;
					button.TextColor		= (lTextColor+lAlpha);
					button.ColourDiffuse= (lDiffuseColor+lAlpha);

					int xpos=button.XPosition;
					button.SetPosition(xpos, iMid + (iTel* (m_iButtonHeight+iSpaceBetween) ) );
					button.FontName=strSmallFont;											 

					button.Refresh();
					int ypos=button.YPosition;
					int height=button.Height;
					button.IsVisible=true;
					if (iTel==iMaxItems-1) m_iOffset2=button.GetID;
					iButton++;
					if (iButton>=m_iButtons) iButton=0;
					m_iVisibleItems++;
				}

				iTel++;
			}
/*
			for (int i=0; i < m_iVisibleItems;++i)
			{
				Trace.WriteLine( String.Format("{0} but:{1}",i, m_iButtonIds[i]));
			}
			Trace.WriteLine( String.Format("off   :{0}",m_iOffset));
			Trace.WriteLine( String.Format("off1  :{0}",m_iOffset1));
			Trace.WriteLine( String.Format("middle:{0}",m_iMiddle));
			Trace.WriteLine( String.Format("off2  :{0}",m_iOffset2));*/
		}

		State Scroll()
		{
			//System.Threading.Thread.Sleep(1000);
			State newState=m_eState;
			float fPercent = ((float)m_iFrame)  / ((float)MAX_FRAMES);
			fPercent*=100f;

			LayoutButtons( (int) fPercent);

			m_iFrame+=m_iStep;
			if (m_iFrame>MAX_FRAMES)
			{
				m_iFrame=0;
				m_iTimes--;
				EndScroll();
				if (m_iTimes<=0)
				{
					newState=State.Idle;
					
					LayoutButtons( 0);
				}
			}

			return newState;
		}

		/// <summary>
		/// 
		/// </summary>
		void EndScroll()
		{
			if (m_eState==State.ScrollDown)
			{
				m_iCurrentButton++;
				int iBut=m_iCurrentButton;
				VerifyButtonIndex(ref iBut);
				if (m_iOffset==0)
			  {
					FocusControl(GetID,iBut+2);
				}
				else
					FocusControl(GetID,m_iOffset2);
				LayoutButtons(0);
				m_iOffset1=0;
			}

			if (m_eState==State.ScrollUp)
			{
				m_iCurrentButton--;
				int iBut=m_iCurrentButton;
				VerifyButtonIndex(ref iBut);
				if (m_iOffset==0)
				{
					FocusControl(GetID,iBut+2);
				}
				else
					FocusControl(GetID,m_iOffset1);
				LayoutButtons(0);
				m_iOffset1=0;
			}
			m_iFrame=0;
			if (m_iTimes<=0) 
			{
				m_eState=State.Idle;
				m_iStep=1;
			}
		}

		/// <summary>
		/// Returns the id of the currently focused control
		/// </summary>
		/// <returns>Id of the control that currently has focus, -1 if the top-bar has focus.</returns>
		public override int GetFocusControlId()
		{
			if (!m_bTopBar) 
				return base.GetFocusControlId();

			return -1;
		}
    protected void FocusControl(int iWindowID, int iControlId)
    {
      if (m_bTopBar) return;
      GUIControl.FocusControl(iWindowID,iControlId);
    }
		public override void Process()
		{
			// Set the date & time
			if (DateTime.Now.Minute != m_updateTimer.Minute)
			{
				m_updateTimer=DateTime.Now;
				GUIControl.SetControlLabel(GetID, 200,GUIPropertyManager.GetProperty("#date") ); 	 
        GUIControl.SetControlLabel(GetID, 201,GUIPropertyManager.GetProperty("#time") );
			}
		}

	}
}
