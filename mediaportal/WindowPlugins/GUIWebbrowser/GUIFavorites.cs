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
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.WebBrowser
{
	/// <summary>
	/// A web browser plugin for mediaportal
	/// </summary>
	public class GUIFavorites : GUIWindow
	{
		public const int WINDOW_FAVORITES = 5501;

		DirectoryHistory m_history = new DirectoryHistory();
		string	currentFolder = String.Empty;
		[SkinControlAttribute(50)]	protected GUIFacadeControl facadeView=null;

		#region Constructor
			public GUIFavorites()
			{
                GetID = WINDOW_FAVORITES;
			}
		#endregion

		#region Private Enumerations
			/// <summary>
			/// Gui Widgets
			/// </summary>
			enum Controls
			{
				BackButton = 3,
			}

		#endregion

		#region Overrides
			public override bool Init()
			{
				return Load (GUIGraphicsContext.Skin+@"\WebFavorites.xml");
			}

			protected override void OnPageLoad()
			{
				base.OnPageLoad();
                if (FavoritesPath.Length != 0)
                {
                    LoadDirectory(FavoritesPath);
                }
                else
                {
                    //no favorites folder specified.
                    ShowErrorDialog(4003);
                }
				
			}

			public override bool OnMessage(GUIMessage message)
			{
				switch ( message.Message )
				{

					case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					{
						base.OnMessage(message);
						GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(4001));
						return true;
					}
					case GUIMessage.MessageType.GUI_MSG_CLICKED:
					{
						//get sender control
						base.OnMessage(message);
						int iControl=message.SenderControlId;
							
						if (iControl==(int)Controls.BackButton)
						{
							GUIWindowManager.ShowPreviousWindow();
							break;
						}
						if (iControl==facadeView.GetID)
						{
							if(facadeView.SelectedListItem.IsFolder)
							{
								LoadDirectory(facadeView.SelectedListItem.Path);
							}
							else
							{
								// The URL file is in standard "INI" format
                                if (facadeView.SelectedListItem != null)
                                {
                                    try
                                    {
                                        IniFile objINI = new IniFile(facadeView.SelectedListItem.Path);
                                        WebBrowserControl.Instance.Browser.Navigate(objINI.IniReadValue("InternetShortcut", "URL"));
                                        GUIWindowManager.ShowPreviousWindow();

                                    }
                                    catch
                                    {
                                        ShowErrorDialog(4002);
                                    }
                                }	
							}
							break;
						}
						
						
						return true;
					}
					case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
					{
						break;
					}
					case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
					{
						currentFolder=message.Label;
						LoadDirectory(currentFolder);
						break;
					}
						
				}
				return base.OnMessage(message);
			}

			
		#endregion

        #region Private Properties
            /// <summary>
		    /// Gets the Internet Favorites path selected by the user
		    /// </summary>
		    private static string FavoritesPath
		    {
			    get
			    {
                    using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
                    {
                       return xmlreader.GetValueAsString("webbrowser", "favoritesFolder", string.Empty);
                    }
			     }
		    }
         #endregion

        #region Private Methods
            /// <summary>
            /// Loads the directory.
            /// </summary>
            /// <param name="newFolderName">New name of the folder.</param>
            private void LoadDirectory(string newFolderName)
            {
                GUIListItem selectedListItem = facadeView.SelectedListItem;

                if (selectedListItem != null)
                {
                    if (selectedListItem.IsFolder && selectedListItem.Label != "..")
                    {
                        m_history.Set(selectedListItem.Label, currentFolder);
                    }
                }
                currentFolder = newFolderName;

                GUIControl.ClearControl(GetID, facadeView.GetID);
                VirtualDirectory Directory;
                ArrayList itemlist;
                ArrayList UrlExtensions = new ArrayList();
                UrlExtensions.Add(".url");

                Directory = new VirtualDirectory();
                Directory.SetExtensions(UrlExtensions);

                itemlist = Directory.GetDirectory(currentFolder);

                foreach (GUIListItem item in itemlist)
                {
                    GUIControl.AddListItemControl(GetID, facadeView.GetID, item);
                }
            }

            /// <summary>
            /// Shows the Error Dialog
            /// </summary>
            private void ShowErrorDialog(int messsageNumber)
            {
                GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                if (dlgOK != null)
                {
                    dlgOK.SetHeading(257);
                    dlgOK.SetLine(1, messsageNumber);
                    dlgOK.SetLine(2, "");
                    dlgOK.DoModal(GetID);
                }
                return;
            }

        #endregion
 
	}
}
