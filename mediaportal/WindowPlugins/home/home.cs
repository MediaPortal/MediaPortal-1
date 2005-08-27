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

#region Usings
using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Topbar;
using MediaPortal.GUI.GUIScript;
using MediaPortal.TV.Database;
using System.Globalization;
using System.Reflection;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
#endregion

namespace MediaPortal.GUI.Home
{
	/// <summary>
	/// The implementation of the HomeWindow.  (This window is coupled to the home.xml skin file).
	/// </summary>
	public class HomeWindow : GUIWindow
	{
		#region Private Enumerations
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
		#endregion

		#region Private Variables
		[SkinControlAttribute(200)]			protected GUILabelControl lblDate=null;
		[SkinControlAttribute(201)]			protected GUILabelControl lblTime=null;

		private int		m_iDateLayout=0; //0=Day DD. Month, 1=Day Month DD
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
		
		bool					fixedScroll=false;
		bool					backButtons=false;
		bool					useTopBarSub=false;
		bool					noTopBar=false;
		bool					useMenuShortcuts=false;
		int[]         m_iButtonIds = new int[60];  
		DateTime      m_updateTimer=DateTime.MinValue;
		DateTime      m_updateOwnTimer=DateTime.MinValue;
		int           m_iMaxHeight;    
		int           m_iMaxWidth ;    
		int           m_iStartXoff;    
		int           m_iStartYoff;    
		int           m_iButtonHeight; 
		int[]					myPlugins = {0,0,0,0,0,0,0,0,0,0};
		int						myPluginsCount;
		int						subMenu=0;
		bool          m_bAllowScroll=true;
		bool					useMyPlugins=true;
		bool					useMenus=false;
		bool					inMyPlugins=false;
		bool					inSubMenu=false;
		bool					inSecondMenu=false;
		bool					noScrollSubs=false;
		bool          m_bSkipFirstMouseMove=true;
		TreeView			treeView = new TreeView();
		string				selectedButton="";
		string				subButton="";
		string				skinName="";
		string				ownDate="";
		string				currentDate="";
		int						menuView=0;
		bool					firstDate=false;
		Viewport      m_newviewport = new Viewport();
		Viewport      m_oldviewport;
		ScriptHandler gScript=new ScriptHandler();

		GUITopbar topBar = new GUITopbar();
		GUITopbarHome	topBarHome = new GUITopbarHome();

		//Tracking controls by id
		System.Collections.ArrayList m_aryPreControlList = new ArrayList();
		System.Collections.ArrayList m_aryPostControlList = new ArrayList();	

		protected GUIImage	m_imgFocus=null;
	
		#endregion

		#region Constructor
		/// <summary>
		/// Constructs the home window and set its ID.
		/// </summary>
		public HomeWindow()
		{
			GetID=(int)GUIWindow.Window.WINDOW_HOME;
		}
		#endregion

		#region Overrides		
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

			using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_iDateLayout = xmlreader.GetValueAsInt("home","datelayout",0);   
				m_bAllowScroll= xmlreader.GetValueAsBool("home","scroll",true);
				fixedScroll= xmlreader.GetValueAsBool("home","scrollfixed",false);		// fix scrollbar in the middle of menu
				useMenus=xmlreader.GetValueAsBool("home","usemenus",false);						// use new menu handling
				useMyPlugins=xmlreader.GetValueAsBool("home","usemyplugins",true);		// use previous menu handling
				noScrollSubs=xmlreader.GetValueAsBool("home","noScrollsubs",false);	
				backButtons=xmlreader.GetValueAsBool("home","backbuttons",false);
				skinName=xmlreader.GetValueAsString("skin","name","BlueTwo");
				ownDate=xmlreader.GetValueAsString("home","ownDate","Day DD. Month");
				useTopBarSub=xmlreader.GetValueAsBool("home","useTopBarSub",false);
				noTopBar=xmlreader.GetValueAsBool("home","noTopBarSub",false);
				useMenuShortcuts=xmlreader.GetValueAsBool("home","useMenuShortcuts",false);
			}
			if (useMenus==true) 
			{
				if (System.IO.File.Exists(Application.StartupPath + @"\menu2.bin"))
				{
					loadTree(treeView, Application.StartupPath + @"\menu2.bin");
				} 
				else 
				{
					loadTree(treeView, Application.StartupPath + @"\menu.bin");// if new menu handling load menutree
				}
			}
			
			m_iButtons=0;
			// add buttons for dynamic plugins
			ArrayList plugins=PluginManager.SetupForms;
			ProcessPlugins(ref plugins);
			if (m_iButtons>0)																											
			{
				while (m_iButtons<10)
					ProcessPlugins(ref plugins);
			}
			if (fixedScroll==true) 
			{
				AddScrollBar();
			}
			plugins=null;
			m_iCurrentButton=m_iButtons/2;
			LayoutButtons(0);
			GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
			topBar.UseTopBarSub=false;
			topBarHome.UseTopBarSub=false;
		}
		
		private void OnGlobalMessage(GUIMessage message)
		{

			if (message.Message==GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM)
			{
				if (GUIGraphicsContext.IsFullScreenVideo) return;
				GUIDialogNotify dialogNotify=(GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
				TVProgram notify=message.Object as TVProgram;
				if (notify==null) return ;
				dialogNotify.SetHeading(1016);
				dialogNotify.SetText(String.Format("{0}\n{1}",notify.Title,notify.Description));
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,notify.Channel);
				dialogNotify.SetImage( strLogo);
				dialogNotify.TimeOut=10;
				dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
			}
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_NOTIFY:
					ShowNotify(message.Label,message.Label2,message.Label3);
				break;
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

		void ShowNotify(string strHeading,string description,string imgFileName)
		{
			GUIDialogNotify dlgYesNo = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
			dlgYesNo.SetHeading(strHeading);
			dlgYesNo.SetText(description);
			dlgYesNo.SetImage(imgFileName);
			dlgYesNo.DoModal( GUIWindowManager.ActiveWindow);
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
			if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)	
			{
				if(action.m_key.KeyChar == 89 || action.m_key.KeyChar == 121 ) 
				{

				}
				return;
			}
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) 
			{
				if(useMyPlugins==true && inMyPlugins==true)   // if frodo´s menu style used set menu to MyPlugins entrys
				{	
					for (int i=102; i < 160; i++)
					{
						GUIControl.HideControl(GetID, i);
					}
					m_iButtons=0;
					inMyPlugins=false;
					if (useTopBarSub==true || noTopBar==true) 
					{
						topBar.UseTopBarSub=false;
						topBarHome.UseTopBarSub=false;
					}

					for (int iButt=2; iButt < 60; iButt++)
					{
						m_iButtonIds[iButt]=0;
						GUIControl bCntl = GetControl(iButt) as GUIControl;
						if (bCntl!=null) 
						{
							Remove(iButt);
						}
					}
					for (int iButt=102; iButt < 160; iButt++)
					{
						GUIControl bCntl = GetControl(iButt) as GUIControl;
						if (bCntl!=null) 
						{
							Remove(iButt);
						}
					}
					ResetButtons();
					ArrayList plugins=PluginManager.SetupForms;
					ProcessPlugins(ref plugins);
					if (m_iButtons>0)
					{
						while (m_iButtons<10)
							ProcessPlugins(ref plugins);
					}
					plugins=null;

					m_iCurrentButton=m_iButtons/2;
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
					return;
				}

				if(useMenus==true && inSubMenu==true) // if gucky´s menu style handling load sub menu tree
				{	
					GoBackMenu();
					return;
				}
				GUIWindowManager.ShowPreviousWindow();
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
					
					return;
				}
				if (y < m_iStartYoff  || y > m_iStartYoff+m_iMaxHeight)
				{
					GUIControl cntl=GetControl(base.GetFocusControlId());
					if (cntl!=null) cntl.Focus=false;
					
					return;
				}
				if (IsTopBarActive())
				{
					GUIControl cntl=GetControl(base.GetFocusControlId());
					if (cntl!=null) cntl.Focus=false;
					
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
							if (fixedScroll!=true) 
							{
								m_keepState=State.Idle;
								if (fixedScroll==false) 
								{
									if (y >= m_iStartYoff && y <= m_iStartYoff+m_iMaxHeight) 
									{
										for (int i=0; i < m_iVisibleItems;++i)
										{
											GUIButtonControl button = GetControl( m_iButtonIds[i]) as GUIButtonControl;
											if (y >=button.YPosition && y <= button.YPosition+button.Height)
											{
												m_iOffset=i-m_iMiddle;
												break;
											}
										}
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
				
				GUIControl cntl=GetControl(base.GetFocusControlId());
				if (cntl!=null) cntl.Focus=false;
				return;
			}

			if (action.wID==Action.ActionType.ACTION_MOVE_DOWN)
			{	
				if (IsTopBarActive())
				{
					
					FocusControl(GetID,m_iButtonIds[m_iOffset+m_iMiddle]);
					return;
				}
				if (m_eState!=State.Idle)
				{
					if (m_iStep+1 <MAX_FRAMES) m_iStep++;
					if (m_iTimes<4) m_iTimes++;
					return;
				}
				int off=3;
				if (fixedScroll) off=5;
				if (m_iOffset+m_iMiddle+off< m_iVisibleItems)
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
				if (IsTopBarActive())
				{
					
					FocusControl(GetID,m_iButtonIds[m_iOffset+m_iMiddle]);
					return;
				}
				if (m_eState!=State.Idle)
				{
					if (m_iStep+1 <MAX_FRAMES) m_iStep++;
					if (m_iTimes<4) m_iTimes++;
					return;
				}
				int off=1;
				if (fixedScroll) off=3;
				if (m_iOffset+m_iMiddle-off>1)
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
					if (lblDate!=null) lblDate.Label=GetDate(); 
					if (lblTime!=null) lblTime.Label=GUIPropertyManager.GetProperty("#time") ;

					ResetButtons();
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
					/*m_imgFocus  = new GUIImage(GetID, 61, 10, 10,200, 30, selectedTexture,0);
					m_imgFocus.IsVisible=true;
					m_imgFocus.Render();
					*/
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
				case GUIMessage.MessageType.GUI_MSG_CLICKED:  // Handle Menu tags
					//get sender control
					base.OnMessage(message);
					int bControl=message.SenderControlId;
					if (bControl>1 && bControl<60)
					{
						GUIControl cntl = GetControl(bControl) as GUIControl;
						if (cntl!=null) 
						{ // Call SubMenu									
							if(useMyPlugins==true) 
							{	
								bool isplugin=false;
								for(int i=0;i<myPluginsCount;i++) 
								{
									if(bControl==myPlugins[i])
									{ 
										isplugin=true;
									}
								}
								if(isplugin==true)
								{
									for (int i=102; i < 160; i++)
									{
										GUIControl.HideControl(GetID, i);
									}
									m_iButtons=0;
									inMyPlugins=true;
									if (useTopBarSub==true) 
									{
										topBar.UseTopBarSub=true;
										topBarHome.UseTopBarSub=true;
									}

									for (int iButt=2; iButt < 60; iButt++)
									{
										m_iButtonIds[iButt]=0;
										GUIControl bCntl = GetControl(iButt) as GUIControl;
										if (bCntl!=null) 
										{
											Remove(iButt);
										}
									}
									for (int iButt=102; iButt < 160; iButt++)
									{
										GUIControl bCntl = GetControl(iButt) as GUIControl;
										if (bCntl!=null) 
										{
											Remove(iButt);
										}
									}
									ResetButtons();
									ArrayList plugins=PluginManager.SetupForms;
									ProcessPlugins(ref plugins);
									if (m_iButtons>0)
									{
										while (m_iButtons<10)
											ProcessPlugins(ref plugins);
									}
									plugins=null;
									m_iCurrentButton=m_iButtons/2;
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
								}
							}
							if (useMenus==true) // Call submenu new style
							{
								GUIButtonControl button = GetControl(bControl) as GUIButtonControl;
								if (button.HyperLink==-2)
								{
									break;
								}
								if (button.HyperLink==-3)
								{
									GoBackMenu();
									break;
								}
								if (button.HyperLink==-500)
								{
									gScript.SetGlobalVar("insubmenu",inSubMenu);
									gScript.StartScript(button.Label);
									break;
								}
								if (inSecondMenu==true) 
								{
									break;
								}
								if (inSubMenu==true) 
								{
									inSecondMenu=true;
								} 
								else 
								{
									inSubMenu=true;
								}
								if (useTopBarSub==true) 
								{
									topBar.UseTopBarSub=true;
									topBarHome.UseTopBarSub=true;
								}
								if (noTopBar==true) 
								{
									topBar.UseTopBarSub=false;
									topBarHome.UseTopBarSub=true;
								}

								GUIButtonControl cButt = GetControl(bControl) as GUIButtonControl;
								selectedButton=cButt.Label;
								for (int i=102; i < 160; i++)
								{
									GUIControl.HideControl(GetID, i);
								}
								m_iButtons=0;
								for (int iButt=2; iButt < 60; iButt++)
								{
									m_iButtonIds[iButt]=0;
									GUIControl bCntl = GetControl(iButt) as GUIControl;
									if (bCntl!=null) 
									{
										Remove(iButt);
									}
								}
								for (int iButt=102; iButt < 160; iButt++)
								{
									GUIControl bCntl = GetControl(iButt) as GUIControl;
									if (bCntl!=null) 
									{
										Remove(iButt);
									}
								}
								ResetButtons();
								ArrayList plugins=PluginManager.SetupForms;
								ProcessPlugins(ref plugins);
								if (menuView > m_iButtons && noScrollSubs==true) 
								{
									int xm = (menuView-m_iButtons)/2;
									for (int iButt=2; iButt < 60; iButt++)
									{
										m_iButtonIds[iButt]=0;
										GUIControl bCntl = GetControl(iButt) as GUIControl;
										if (bCntl!=null) 
										{
											Remove(iButt);
										}
									}
									for (int iButt=102; iButt < 160; iButt++)
									{
										GUIControl bCntl = GetControl(iButt) as GUIControl;
										if (bCntl!=null) 
										{
											Remove(iButt);
										}
									}	
									m_iButtons=0;
									for (int i=0; i<xm-1; i++) AddPluginButton(-2," ", "",  "", "");	
									ProcessPlugins(ref plugins);
									for (int i=0; i<xm+1; i++) AddPluginButton(-2," ", "",  "", "");	
									ProcessPlugins(ref plugins);
									for (int i=0; i<xm+1; i++) AddPluginButton(-2," ", "",  "", "");	
									ProcessPlugins(ref plugins);
									AddPluginButton(-2," ", "",  "", "");
								} 
								else 
								{
									if (m_iButtons>0)
									{
										while (m_iButtons<10)
											ProcessPlugins(ref plugins);
									}
								}
								plugins=null;
								m_iCurrentButton=m_iButtons/2;
								for (int i=102; i < 160; i++)
								{
									GUIControl.HideControl(GetID, i);
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
							}
						}
					}
					break;
			}
			return base.OnMessage(message);
		}
		#endregion

		#region Private Methods

		private void GoBackMenu()
		{
			if(inSecondMenu==true) 
			{
				inSecondMenu=false;
				selectedButton=subButton;
			} 
			else 
			{
				inSubMenu=false;
				inMyPlugins=false;
				if (useTopBarSub==true || noTopBar==true) 
				{
					topBar.UseTopBarSub=false;
					topBarHome.UseTopBarSub=false;
				}
			}
			for (int i=102; i < 160; i++)
			{
				GUIControl.HideControl(GetID, i);
			}
			m_iButtons=0;
			for (int iButt=2; iButt < 60; iButt++)
			{
				m_iButtonIds[iButt]=0;
				GUIControl bCntl = GetControl(iButt) as GUIControl;
				if (bCntl!=null) 
				{
					Remove(iButt);
				}
			}
			for (int iButt=102; iButt < 160; iButt++)
			{
				GUIControl bCntl = GetControl(iButt) as GUIControl;
				if (bCntl!=null) 
				{
					Remove(iButt);
				}
			}
			ResetButtons();
			ArrayList plugins=PluginManager.SetupForms;
			ProcessPlugins(ref plugins);
			if (m_iButtons>0)
			{
				while (m_iButtons<10)
					ProcessPlugins(ref plugins);
			}
			plugins=null;
			m_iCurrentButton=m_iButtons/2;
			VerifyButtonIndex(ref m_iCurrentButton);
			LayoutButtons(0);
			if (m_iOffset!=0)
			{
				FocusControl(GetID,m_iButtonIds[m_iOffset+m_iMiddle]);
			}
			else
			{
				int buttonIndex = m_iCurrentButton;
				VerifyButtonIndex(ref buttonIndex);
				FocusControl(GetID, buttonIndex + 2);
			}
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
		public override void Render(float timePassed)
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

					if (IsTopBarActive()) m_keepState=State.Idle;
					m_eState = m_keepState;
					m_iFrame = 0;
				}
			}
			
			if (m_aryPreControlList.Count==0) 
			{
				for (int x=0; x < controlList.Count;++x)
				{
					GUIControl control=(GUIControl)controlList[x];
					if ((control.GetID<2 || control.GetID>60))
					{
						m_aryPreControlList.Add(control);
					}
				}
			}

			IEnumerator enumControls = m_aryPreControlList.GetEnumerator();
			while (enumControls.MoveNext())
			{
				((GUIControl)enumControls.Current).Render(timePassed);  
			}
			int x1=m_iStartXoff+GUIGraphicsContext.OffsetX;
			int y1=m_iStartYoff+GUIGraphicsContext.OffsetY;

			m_oldviewport=GUIGraphicsContext.DX9Device.Viewport;
			m_newviewport.X      = (int)x1;
			m_newviewport.Y			 = (int)y1;
			m_newviewport.Width  = (int)(m_iMaxWidth);
			m_newviewport.Height = (int)(m_iMaxHeight);
			m_newviewport.MinZ   = 0.0f;
			m_newviewport.MaxZ   = 1.0f;
			GUIGraphicsContext.DX9Device.Viewport=m_newviewport;
			if (m_aryPostControlList.Count==0) 
			{	
				for (int x=0; x < controlList.Count;++x)
				{
					GUIControl control=(GUIControl)controlList[x];
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
				if (cntl.YPosition + cntl.Height >= y1 && cntl.YPosition <y1+m_iMaxHeight)
				{
					cntl.Render(timePassed);
				}
			}
			GUIGraphicsContext.DX9Device.Viewport=m_oldviewport;
			enumControls=null;
		}

		/// <summary>
		/// Reads all plugins for a Menu
		/// </summary>
		void ProcessPlugins(ref ArrayList plugins)
		{
			string tnText="";
			int mainnodes=treeView.Nodes.Count;

			// Clear plugin count
			if (m_iButtons==0) myPluginsCount=0;
			int myPlgInCount=0;

			if (useMenus==true) 
			{
				if (inSubMenu==true)  // search submenu tree
				{
					if (inSecondMenu==true) // second Menulevel
					{
						for (int i=0;i<treeView.Nodes[subMenu].Nodes.Count;i++) 
						{
							int l=treeView.Nodes[subMenu].Nodes[i].Text.IndexOf(" {",0);
							if (l>0) 
							{
								tnText=treeView.Nodes[subMenu].Nodes[i].Text.Substring(0,l);
							} 
							else 
							{
								tnText=treeView.Nodes[subMenu].Nodes[i].Text;
							}
							if (tnText=="("+selectedButton+")") 
							{
								addMenu(treeView.Nodes[subMenu].Nodes[i],plugins);
								break;
							}
						}						
					} 
					else 
					{
						for (int i=0;i<mainnodes;i++) 
						{
							int l=treeView.Nodes[i].Text.IndexOf(" {",0);
							if (l>0) 
							{
								tnText=treeView.Nodes[i].Text.Substring(0,l);
							} 
							else 
							{
								tnText=treeView.Nodes[i].Text;
							}
							if (tnText=="("+selectedButton+")") 
							{
								addMenu(treeView.Nodes[i],plugins);	
								subMenu=i;
								subButton=selectedButton;
								break;
							}
						}
					}
				} 
				else 
				{ 
					TreeNode tnMain=new TreeNode();
					for (int i=0;i<mainnodes;i++) 
					{
						tnMain.Nodes.Add(treeView.Nodes[i]);
					}
					addMenu(tnMain,plugins);	
				}
			} 
			else 
			{
				foreach (ISetupForm setup in plugins) 
				{
					
					using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						bool bHomeDefault=setup.DefaultEnabled();
						bool inhome;
						if(useMyPlugins==true)
						{
							bool pluginEnabled=xmlreader.GetValueAsBool("myplugins", setup.PluginName(), bHomeDefault);
							if(pluginEnabled==true)
								myPlgInCount++;
						}
						if(inMyPlugins==true)
						{
							inhome=xmlreader.GetValueAsBool("myplugins", setup.PluginName(), bHomeDefault);
						} 
						else
						{
							inhome=xmlreader.GetValueAsBool("home", setup.PluginName(), bHomeDefault);  
						}
						if (!inhome) continue;
					}
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
						if (iHyperLink!=740)
						{
							AddPluginButton(iHyperLink,strButtonText, strButtonImageFocus,  strButtonImage,strPictureImage);
						}
					}
				}
				if (useMyPlugins==true && inMyPlugins==false && myPlgInCount>0) // if My Plugin Call make MyPlugin Button
				{
					string strBtnPic=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,"hover_my plugins.png");
					string strButtonImageFocus=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,"button_my plugins.png");
					if (!System.IO.File.Exists(strButtonImageFocus))
					{
						strButtonImageFocus="";
					}
					string strButtonImage=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,"buttonNF_my plugins.png");
					if (!System.IO.File.Exists(strButtonImage))
					{
						strButtonImage="";
					}
					AddPluginButton(34,GUILocalizeStrings.Get(913), strButtonImageFocus,  strButtonImage, strBtnPic);						
				}
			}
		}

		void addMenu(TreeNode tnMain,ArrayList plugins) 
		{
			foreach (TreeNode tn in tnMain.Nodes) // make buttons for each menu entry 
			{
				if (tn.Text.StartsWith("(")) 
				{
					string strBtnFile="";
					int l1=tn.Text.IndexOf("{",0);
					int l2=tn.Text.IndexOf("}",0);
					string strBtnText=tn.Text.Substring(1,tn.Text.IndexOf(")",0)-1);
					if(l1>0) 
					{
						strBtnFile=tn.Text.Substring(l1+1,(l2-l1)-1);
						strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strBtnFile);
						if (!System.IO.File.Exists(strBtnFile))
						{
							strBtnFile="";
						}
					}
					AddPluginButton(-1,strBtnText, "",  "", strBtnFile);	
					continue;
				}
				if (tn.Text.StartsWith("[")) 
				{
					string strBtnFile="";
					int l1=tn.Text.IndexOf("{",0);
					int l2=tn.Text.IndexOf("}",0);
					string strBtnText=tn.Text.Substring(1,tn.Text.IndexOf("]",0)-1);
					if(l1>0) 
					{
						strBtnFile=tn.Text.Substring(l1+1,(l2-l1)-1);
						strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strBtnFile);
						if (!System.IO.File.Exists(strBtnFile))
						{
							strBtnFile="";
						}
					}
					string btnTxt="";
					gScript.SetGlobalVar("insubmenu",inSubMenu);
					btnTxt=gScript.LoadScript(strBtnText);
					AddPluginButton(-500,btnTxt, "",  "", strBtnFile);	
					continue;
				}
				foreach (ISetupForm setup in plugins)
				{
					if(tn.Text==setup.PluginName()) 
					{
						string strButtonText;
						string strButtonImage;
						string strButtonImageFocus;
						string strPictureImage;
						if (setup.GetHome(out strButtonText,out strButtonImage,out strButtonImageFocus, out strPictureImage))
						{
							string strBtnFile;
							if (strButtonImage=="")
							{
								strButtonImage=String.Format("buttonnf_{0}.png", setup.PluginName());
								strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strButtonImage);
								if (!System.IO.File.Exists(strBtnFile))
								{
									strButtonImage="";
								}
							}
													
							if (strButtonImageFocus=="")
							{
								strButtonImageFocus=String.Format("button_{0}.png", setup.PluginName());
								strBtnFile=String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin,strButtonImageFocus);
								if (!System.IO.File.Exists(strBtnFile))
								{
									strButtonImageFocus="";
								}
							}
							if (strPictureImage=="")
							{
								strPictureImage=String.Format("hover_{0}.png", setup.PluginName());
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
			}
			if (backButtons==true)
			{
				if (inSubMenu==true || inSecondMenu==true) 
				{
					AddPluginButton(-3,GUILocalizeStrings.Get(712), "",  "", "");	
				}
			}
		}

		/// <summary>
		/// Creates a dummy button for fixed scroll bar
		/// </summary>
		public void AddScrollBar() 
		{
			string strButtonImage= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TexutureNoFocusName;
			string strButtonImageFocus= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TexutureFocusName;
			string strFontName= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).FontName;
			long   lFontColor = ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TextColor;
			long   lDisabledColor = ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).DisabledColor;
			int xpos  =GetControl( (int)Controls.TemplateButton).XPosition;
			int width =GetControl( (int)Controls.TemplateButton).Width;
			int height=GetControl( (int)Controls.TemplateButton).Height;
			int ypos  =GetControl( (int)Controls.TemplateButton).YPosition;
			
			GUIControl cntl = GetControl(61) as GUIControl;
			if (cntl==null)
			{
				GUIButtonControl button= new GUIButtonControl(GetID,61,xpos,ypos,width,height,strButtonImageFocus,strButtonImage);
				button.Label="";
				button.HyperLink=-1;
				button.FontName=strFontName;
				button.DisabledColor=lDisabledColor;
				button.TextColor=lFontColor;
				button.TextOffsetX= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TextOffsetX;
				button.TextOffsetY= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).TextOffsetY;
				button.ColourDiffuse= ((GUIButtonControl)GetControl((int)Controls.TemplateButton)).ColourDiffuse;			
				button.AllocResources();
				GUIControl btnControl = (GUIControl) button;
				Add(ref btnControl);
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
					if (useMyPlugins==true && iHyperLink==34) // if My Plugin Call set ID in Array
					{
						myPlugins[myPluginsCount++]=iButtonId;
					} 
					else 
					{
						button.HyperLink=iHyperLink;
					}
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
					img.IsVisible=false;
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
			if (menuView<2) menuView=iMaxItems;

			int iMid=(m_iMaxHeight/2) - ((m_iButtonHeight)/2);
			int iScMid=iMid+m_iStartYoff;
			iMid += iStartYoff;

			iMaxItems/=2;
			iMaxItems++;
			int iTel=0;
			int iButton=m_iCurrentButton; 

			VerifyButtonIndex(ref iButton);
			
			m_iVisibleItems=0;
			m_iMiddle=iMaxItems;

			if (fixedScroll==true) 
			{
				GUIButtonControl scbutton = GetControl(61) as GUIButtonControl;  // Dummy Button for fixed scroll bar
				int xscpos=scbutton.XPosition;
				scbutton.SetPosition(xscpos, iScMid);
				scbutton.IsVisible=true;
			}

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

					//	button.Height = (int) (fPercent * ((float)m_iButtonHeight));
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
		}

		State Scroll()
		{
			if (fixedScroll==true) 
			{
				if (m_iFrame==0) 
				{
					UnFocusControl(GetID,GetFocusControlId());
					FocusControl(GetID,61);
				}	
			}
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
			if (!IsTopBarActive()) 
				return base.GetFocusControlId();
			return -1;
		}

		protected void FocusControl(int iWindowID, int iControlId)
		{
			if (IsTopBarActive()) return;
			GUIControl.FocusControl(iWindowID,iControlId);
		}

		protected void UnFocusControl(int iWindowID, int iControlId)
		{
			if (IsTopBarActive()) return;
			GUIControl.UnfocusControl(iWindowID,iControlId);
		}

		public override void Process()
		{
			// Set the date & time
			if (DateTime.Now.Minute != m_updateTimer.Minute)
			{
				m_updateTimer=DateTime.Now;	 
				if (lblDate!=null) lblDate.Label=GetDate(); 
				if (lblTime!=null) lblTime.Label=GUIPropertyManager.GetProperty("#time") ;
			}
		}

		void ResetButtons()
		{			
			m_aryPreControlList.Clear();
			m_aryPostControlList.Clear();
			m_iMaxHeight    = GetControl( (int)Controls.TemplatePanel).Height;
			m_iMaxWidth     = GetControl( (int)Controls.TemplatePanel).Width;
			m_iStartXoff    = GetControl( (int)Controls.TemplatePanel).XPosition;
			m_iStartYoff    = GetControl( (int)Controls.TemplatePanel).YPosition;
			m_iButtonHeight = GetControl( (int)Controls.TemplateButton).Height;

			
			// make controls 101-120 invisible... (these are the subpictures shown for each button)
			for (int iControl=102; iControl < 160; iControl++)
			{
				GUIControl.HideControl(GetID, iControl);
			}
		}

		public static int loadTree(TreeView tree, string filename)
		{
			if (File.Exists(filename))
			{
				Stream file = File.Open(filename, FileMode.Open);
				BinaryFormatter bf = new BinaryFormatter();
				object obj = null;
				try
				{
					obj = bf.Deserialize(file);
				}
				catch (System.Runtime.Serialization.SerializationException e)
				{
					MessageBox.Show("De-Serialization failed : {0}", e.Message);
					return -1;
				}
				file.Close();
				ArrayList alist = obj as ArrayList;
				foreach (object item in alist)
				{
					Hashtable ht = item as Hashtable;
					TreeNode tn = new TreeNode(ht["Text"].ToString());
					tn.Tag = ht["Tag"];
					tn.ImageIndex = Convert.ToInt32(ht["SelectedImageIndex"].ToString());
					tn.SelectedImageIndex = Convert.ToInt32(ht["SelectedImageIndex"].ToString());

					string fPath = ht["FullPath"].ToString();
					string[] parts = fPath.Split(tree.PathSeparator.ToCharArray());
					if (parts.Length > 1)
					{
						TreeNode parentNode = null;
						TreeNodeCollection nodes = tree.Nodes;
						searchNode(parts, ref parentNode, nodes);

						if (parentNode != null)
						{
							parentNode.Nodes.Add(tn);
						}
					}
					else tree.Nodes.Add(tn);
				}
				return 0;
			}
			else return -2; 
		}
		
		private static void searchNode(string[] parts, ref TreeNode parentNode, TreeNodeCollection nodes)
		{
			foreach (TreeNode n in nodes)
			{
				if (n.Text.Equals(parts[parts.Length - 2].ToString()))
				{
					parentNode = n;
					return;
				}
				else searchNode(parts, ref parentNode, n.Nodes);
			}
		}

		/// <summary>
		/// Build the current date from the system and localize it based on the user own Date declaration.
		/// </summary>
		/// <returns>A string containing the localized version of the date.</returns>
		protected string ConvertOwnDate(string own,string day,string month)
		{
			StringBuilder cown = new StringBuilder(own);
			string s;

			DateTime cur=DateTime.Now;
			s=cown.ToString();
			s=s.ToUpper();
			int inx=s.IndexOf("MM",0);
			if (inx>=0) 
			{	
				cown.Remove(inx,2);
				cown.Insert(inx,cur.Month.ToString());
			}
			s=cown.ToString();
			s=s.ToUpper();
			inx=s.IndexOf("DD",0);
			if (inx>=0) 
			{	
				cown.Remove(inx,2);
				cown.Insert(inx,cur.Day.ToString());
			}
			s=cown.ToString();
			s=s.ToUpper();
			inx=s.IndexOf("MONTH",0);
			if (inx>=0) 
			{	
				cown.Remove(inx,5);
				cown.Insert(inx,month);
			}
			s=cown.ToString();
			s=s.ToUpper();
			inx=s.IndexOf("DAY",0);
			if (inx>=0) 
			{	
				cown.Remove(inx,3);
				cown.Insert(inx,day);
			}
			s=cown.ToString();
			s=s.ToUpper();
			inx=s.IndexOf("YY",0);
			if (inx>=0) 
			{	
				cown.Remove(inx,2);
				int sy=cur.Year-2000;
				if (sy<10) 
				{
					cown.Insert(inx,"0"+sy.ToString());
				} 
				else 
				{
					cown.Insert(inx,sy.ToString());
				}
			}
			s=cown.ToString();
			s=s.ToUpper();
			inx=s.IndexOf("YEAR",0);
			if (inx>=0) 
			{	
				cown.Remove(inx,4);
				cown.Insert(inx,cur.Year.ToString());
			}
			return(cown.ToString());
		}		

		/// <summary>
		/// Get the current date from the system and localize it based on the user preferences.
		/// </summary>
		/// <returns>A string containing the localized version of the date.</returns>
		protected string GetDate()
		{
			if (DateTime.Now.Day != m_updateOwnTimer.Day || firstDate==false)
			{
				firstDate=true;
				m_updateOwnTimer=DateTime.Now;	 
				string strDate="";
				DateTime cur=DateTime.Now;
				string day;
				switch (cur.DayOfWeek)
				{
					case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(11);	break;
					case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(12);	break;
					case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(13);	break;
					case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(14);	break;
					case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(15);	break;
					case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(16);	break;
					default:	day = GUILocalizeStrings.Get(17);	break;
				}

				string month;
				switch (cur.Month)
				{
					case 1 :	month= GUILocalizeStrings.Get(21);	break;
					case 2 :	month= GUILocalizeStrings.Get(22);	break;
					case 3 :	month= GUILocalizeStrings.Get(23);	break;
					case 4 :	month= GUILocalizeStrings.Get(24);	break;
					case 5 :	month= GUILocalizeStrings.Get(25);	break;
					case 6 :	month= GUILocalizeStrings.Get(26);	break;
					case 7 :	month= GUILocalizeStrings.Get(27);	break;
					case 8 :	month= GUILocalizeStrings.Get(28);	break;
					case 9 :	month= GUILocalizeStrings.Get(29);	break;
					case 10:	month= GUILocalizeStrings.Get(30);	break;
					case 11:	month= GUILocalizeStrings.Get(31);	break;
					default:	month= GUILocalizeStrings.Get(32);	break;
				}

				if (m_iDateLayout==0) 
				{
					strDate=String.Format("{0} {1}. {2}",day, cur.Day, month);
				}
				if (m_iDateLayout==1)
				{
					strDate=String.Format("{0} {1} {2}",day, month, cur.Day);
				}
				if (m_iDateLayout==2)
				{
					strDate=ConvertOwnDate(ownDate,day, month);
				}
				currentDate=strDate;
			}
			return currentDate;
		}
		bool IsTopBarActive()
		{
			GUIWindow window=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TOPBARHOME);
			return (window.Focused);
		}
		#endregion

	}
}
