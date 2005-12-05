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
 * 
 * Thanks to Adam Lock for making the Mozilla ActiveX Control
 * Licensed under the MPL - http://www.mozilla.org/MPL/MPL-1.1.txt
 * 
 * http://www.iol.ie/~locka/mozilla/mozilla.htm
 * 
 */

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using System.Windows.Forms;

namespace MediaPortal.GUI.WebBrowser
{
	/// <summary>
	/// A web browser plugin for mediaportal
	/// </summary>
	public class GUIWebBrowser : GUIWindow
	{
		public const int WINDOW_WEB_BROWSER = 5500;

		private WebBrowserControl wb;
		
		#region Constructor
			/// <summary>
			/// Initializes a new instance of the <see cref="GUIWebBrowser"/> class.
			/// </summary>
			public GUIWebBrowser()
			{
				GetID=WINDOW_WEB_BROWSER;			
			} 
		#endregion

		#region Enumerations
			/// <summary>
			/// Gui Widgets
			/// </summary>
			public enum Controls
			{
				BackButton = 2,
				ForwardButton = 3,
				RefreshButton = 4,
				StopButton = 5,
				FavoritesButton = 6,
				UrlButton = 7,
				Progress = 8,
                HomeButton = 9
			}

		#endregion

		#region Overrides
			/// <summary>
			/// Inits this instance.
			/// </summary>
			/// <returns></returns>
			public override bool Init()
			{
                try
                {

                    wb = WebBrowserControl.Instance;
                    GUIGraphicsContext.form.Controls.Add(wb);
                    wb.Visible = false;
                    wb.Browser.NavigateComplete2 += new AxMOZILLACONTROLLib.DWebBrowserEvents2_NavigateComplete2EventHandler(Browser_NavigateComplete2);
                    wb.Browser.DownloadBegin += new EventHandler(Browser_DownloadBegin);
                    wb.Browser.DownloadComplete += new EventHandler(Browser_DownloadComplete);
                    wb.Browser.BeforeNavigate2 += new AxMOZILLACONTROLLib.DWebBrowserEvents2_BeforeNavigate2EventHandler(Browser_BeforeNavigate2);
                    wb.Browser.StatusTextChange += new AxMOZILLACONTROLLib.DWebBrowserEvents2_StatusTextChangeEventHandler(Browser_StatusTextChange);
                    wb.Browser.ProgressChange += new AxMOZILLACONTROLLib.DWebBrowserEvents2_ProgressChangeEventHandler(Browser_ProgressChange);
                    return Load(GUIGraphicsContext.Skin + @"\webbrowser.xml");
                }
                catch
                {
                    Log.WriteFile(Log.LogType.Error, true, "Unable to load the web browser plugin, verify that Mozilla ActiveX Control is installed");
                }
                return false;
			}

            public override void DeInit()
            {
                base.DeInit();
                wb.Dispose();
                WebBrowserControl.Instance.Dispose();
            }

			/// <summary>
			/// Called when [action].
			/// </summary>
			/// <param name="action">The action.</param>
			public override void OnAction(Action action)
			{
				switch (action.wID)
				{
                    case Action.ActionType.ACTION_SHOW_INFO:
                    {
                        GUIWindowManager.ActivateWindow(GUIFavorites.WINDOW_FAVORITES);
                        break;
                    }
					case Action.ActionType.ACTION_CONTEXT_MENU:
					{
					    wb.ToggleMenu();
						break;
					}
				}
				base.OnAction(action);
			}

			/// <summary>
			/// Called when [message].
			/// </summary>
			/// <param name="message">The message.</param>
			/// <returns></returns>
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
							wb.RefreshBrowser();
							break;
						}
						if (iControl==(int)Controls.StopButton)
						{
							wb.Browser.Stop();
                            break;
						}
                        if (iControl == (int)Controls.HomeButton)
                        {
                            if (Common.HomePage.Length != 0)
                                wb.Browser.Navigate(Common.HomePage);
                            break;
                        }
						if(iControl == (int)Controls.FavoritesButton)
						{
							GUIWindowManager.ActivateWindow(GUIFavorites.WINDOW_FAVORITES);
                            break;
						}
						if(iControl == (int)Controls.UrlButton)
						{
							//hide browser control for keyboard display
							wb.Visible=false;
							//set focus to main form to avoid browser caputuring entered text
							GUIGraphicsContext.form.Focus();
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
                        GUIGraphicsContext.form.Focus();
					}
					break;
				}
				return base.OnMessage(message);
			}
		#endregion

        #region Private Methods
             /// <summary>
		    /// Gets the input from the virtual keyboard window
		    /// </summary>
		    /// <param name="strLine">The STR line.</param>
		    private void GetStringFromKeyboard(ref string strLine)
		    {

                VirtualWebKeyboard keyboard = (VirtualWebKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_WEB_KEYBOARD);
                if (null == keyboard) return;
                keyboard.Reset();
                keyboard.Text = strLine;
                keyboard.DoModal(GetID);
                if (keyboard.IsConfirmed)
                {
                    strLine = keyboard.Text;
                }

                //VirtualKeyboardTest keyboard = (VirtualKeyboardTest)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_TEST_KEYBOARD);
                //if (null == keyboard) return;
                //keyboard.Reset();
                //keyboard.Text = strLine;
                //keyboard.DoModal(GetID);
                //if (keyboard.IsConfirmed)
                //{
                //    strLine = keyboard.Text;
                //}
		    }
        #endregion
       
		#region Browser Events
			/// <summary>
			/// Handles the NavigateComplete2 event of the Browser control.
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="e">The e.</param>
			private void Browser_NavigateComplete2(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_NavigateComplete2Event e)
			{
				GUIPropertyManager.SetProperty("#location.name", wb.Browser.LocationName);
				GUIPropertyManager.SetProperty("#location.url", wb.Browser.LocationURL);
			}
			/// <summary>
			/// Handles the DownloadBegin event of the Browser control.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
			private void Browser_DownloadBegin(object sender, EventArgs e)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE,GetID,0, (int)Controls.Progress,0,0,null); 
				OnMessage(msg);
			}
			/// <summary>
			/// Handles the BeforeNavigate2 event of the Browser control.
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="e">The e.</param>
			private void Browser_BeforeNavigate2(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_BeforeNavigate2Event e)
			{
				GUIPropertyManager.SetProperty("#location.url",e.uRL.ToString());
				GUIPropertyManager.SetProperty("#status", "Loading " + e.uRL.ToString());
			}
			/// <summary>
			/// Handles the StatusTextChange event of the Browser control.
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="e">The e.</param>
			private void Browser_StatusTextChange(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_StatusTextChangeEvent e)
			{
				GUIPropertyManager.SetProperty("#status",e.text);
			}
			/// <summary>
			/// Handles the ProgressChange event of the Browser control.
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="e">The e.</param>
			private void Browser_ProgressChange(object sender, AxMOZILLACONTROLLib.DWebBrowserEvents2_ProgressChangeEvent e)
			{
				double Progress;	
				GUIProgressControl pControl = (GUIProgressControl)GetControl((int)Controls.Progress);
				Progress = (e.progress * 100) / e.progressMax;
				pControl.Percentage = Convert.ToInt32(Progress);
			}
			/// <summary>
			/// Handles the DownloadComplete event of the Browser control.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
			private void Browser_DownloadComplete(object sender, EventArgs e)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN,GetID,0, (int)Controls.Progress,0,0,null); 
				OnMessage(msg);
			}
		#endregion	
	}
}
