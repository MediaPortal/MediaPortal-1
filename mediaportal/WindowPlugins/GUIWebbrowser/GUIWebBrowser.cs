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
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.WebBrowser
{
	/// <summary>
	/// A web browser plugin for mediaportal
	/// </summary>
	public class GUIWebBrowser : GUIWindow, ISetupForm
	{
		WebBrowserControl wb;

		#region Constructor
			public GUIWebBrowser()
			{
				GetID=5500;
				wb = WebBrowserControl.Instance;
				GUIGraphicsContext.form.Controls.Add(wb);
				wb.Visible=false;
				wb.Browser.NavigateComplete2+= new AxMOZILLACONTROLLib.DWebBrowserEvents2_NavigateComplete2EventHandler(Browser_NavigateComplete2);
				wb.Browser.DownloadBegin+=new EventHandler(Browser_DownloadBegin);
				wb.Browser.DownloadComplete+=new EventHandler(Browser_DownloadComplete);
				wb.Browser.BeforeNavigate2+=new AxMOZILLACONTROLLib.DWebBrowserEvents2_BeforeNavigate2EventHandler(Browser_BeforeNavigate2);
				wb.Browser.StatusTextChange+=new AxMOZILLACONTROLLib.DWebBrowserEvents2_StatusTextChangeEventHandler(Browser_StatusTextChange);
				wb.Browser.ProgressChange+=new AxMOZILLACONTROLLib.DWebBrowserEvents2_ProgressChangeEventHandler(Browser_ProgressChange);
			}
		#endregion

		#region Private Enumerations
			/// <summary>
			/// Gui Widgets
			/// </summary>
			enum Controls
			{
				BackButton = 2,
				ForwardButton = 3,
				RefreshButton = 4,
				StopButton = 5,
				FavoritesButton = 6,
				UrlButton = 7,
				Progress = 8
			}

		#endregion

		#region Overrides
			public override bool Init()
			{
				return Load (GUIGraphicsContext.Skin+@"\webbrowser.xml");
			}

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_KEY_PRESSED:
				{
					//space bar
					if(action.m_key.KeyChar == 32)
					{	
					
					}
					break;
				}
				case Action.ActionType.ACTION_PREVIOUS_MENU:
				{
					GUIWindowManager.ShowPreviousWindow();
					return;
				}

			}
			
			base.OnAction(action);
		}

			public override bool OnMessage(GUIMessage message)
			{
				switch ( message.Message )
				{

					case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					{
						base.OnMessage(message);
						GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(4000));
					
						//make web browser visible			
						wb.Visible=true;
						wb.Focus();
						return true;

					}
					case GUIMessage.MessageType.GUI_MSG_CLICKED:
					{
						//get sender control
						base.OnMessage(message);
						int iControl=message.SenderControlId;
							
						if (iControl==(int)Controls.BackButton)
						{
							wb.Browser.GoBack();
							break;
						}
						if (iControl==(int)Controls.ForwardButton)
						{
							wb.Browser.GoForward();
							break;
						}
						if (iControl==(int)Controls.RefreshButton)
						{
							object refreshType = WebBrowserControl.RefreshConstants.REFRESH_COMPLETELY;
							wb.Browser.Refresh2(ref refreshType);
							break;
						}
						if (iControl==(int)Controls.StopButton)
						{
							wb.Browser.Stop();
							
						}
						if(iControl == (int)Controls.FavoritesButton)
						{
							GUIWindowManager.ActivateWindow(GUIFavorites.WINDOW_FAVORITES);
						}
						if(iControl == (int)Controls.UrlButton)
						{
							//hide browser control for keyboard display
							wb.Visible=false;
							//set focus to main form to avoid browser caputuring entered text
							GUIGraphicsContext.form.Focus();
							///Utils.ApplicationContext.Focus();
							wb.Enabled=false;
							string strName= string.Empty;
							GetStringFromKeyboard(ref strName);
							if (strName.Length != 0)
							{
								wb.Browser.Navigate(strName);
								GUIPropertyManager.SetProperty("#location.url", strName);
								GUIPropertyManager.SetProperty("#location.name", string.Empty);
							}
							wb.Visible=true;
							wb.Enabled=true;
							break;
						}
						return true;
					}
					case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
					{
						//hide the browser
						wb.Visible=false;						
					}
					break;

				}
				return base.OnMessage(message);
			}
		#endregion

		/// <summary>
		/// Gets the input from the virtual keyboard window
		/// </summary>
		/// <param name="strLine"></param>
		private void GetStringFromKeyboard(ref string strLine)
		{
		
			VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
			if (null == keyboard) return;
			keyboard.Reset();
			keyboard.Text = strLine;
			keyboard.DoModal(GetID);
			if (keyboard.IsConfirmed)
			{
				strLine = keyboard.Text;
			}
		}
		

		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string Description()
		{
			return "A web browser plugin for media portal.";
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return 5500;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(4000);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "hover_my alarm.png";
			return true;
		}

		public string Author()
		{
			return "Devo";
		}

		public string PluginName()
		{
			return GUILocalizeStrings.Get(4000);
		}

		public bool HasSetup()
		{
			return false;
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIWebBrowser.ShowPlugin implementation
		}

		#endregion

		#region Browser Events
			/// <summary>
			/// 
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			private void Browser_NavigateComplete2(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_NavigateComplete2Event e)
			{
				//GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850) + @"/" + strName);
				GUIPropertyManager.SetProperty("#location.name", wb.Browser.LocationName);
				GUIPropertyManager.SetProperty("#location.url", wb.Browser.LocationURL);
			
			}
			/// <summary>
			/// 
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			private void Browser_DownloadBegin(object sender, EventArgs e)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE,GetID,0, (int)Controls.Progress,0,0,null); 
				OnMessage(msg);
			}

			private void Browser_BeforeNavigate2(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_BeforeNavigate2Event e)
			{
				GUIPropertyManager.SetProperty("#location.url",e.uRL.ToString());
				GUIPropertyManager.SetProperty("#status", "Loading " + e.uRL.ToString());
			}

			private void Browser_StatusTextChange(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_StatusTextChangeEvent e)
			{
				GUIPropertyManager.SetProperty("#status",e.text);
			}

			private void Browser_ProgressChange(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_ProgressChangeEvent e)
			{
				double Progress;	
				GUIProgressControl pControl = (GUIProgressControl)GetControl((int)Controls.Progress);
				Progress = (e.progress * 100) / e.progressMax;
				pControl.Percentage = Convert.ToInt32(Progress);
			}
			private void Browser_DownloadComplete(object sender, EventArgs e)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN,GetID,0, (int)Controls.Progress,0,0,null); 
				OnMessage(msg);
			}
		#endregion
	}
}
