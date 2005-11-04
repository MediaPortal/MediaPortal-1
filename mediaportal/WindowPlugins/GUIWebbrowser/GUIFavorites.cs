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
	public class GUIFavorites : GUIWindow
	{
		public const int WINDOW_FAVORITES = 5501;

		DirectoryHistory	m_history = new DirectoryHistory();
		string	currentFolder = String.Empty;
		[SkinControlAttribute(50)]	protected GUIFacadeControl facadeView=null;

		#region Constructor
			public GUIFavorites()
			{
				GetID=5501;
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
				LoadDirectory(FavoritesPath);
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
								IniFile objINI = new IniFile(facadeView.SelectedListItem.Path);
								WebBrowserControl.Instance.Browser.Navigate(objINI.IniReadValue("InternetShortcut", "URL"));							
								GUIWindowManager.ShowPreviousWindow();
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

		/// <summary>
		/// Gets the Internet Explorer Favorites path for the current user 
		/// from the Windows Registry.
		/// </summary>
		private string FavoritesPath
		{
			get
			{
				Microsoft.Win32.RegistryKey objRegKey = Microsoft.Win32.Registry.CurrentUser;
				Microsoft.Win32.RegistryKey objFav = objRegKey.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Folders");

				return (string)objFav.GetValue("Favorites");
			 }
		}

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
				GUIControl.AddListItemControl(GetID,facadeView.GetID,item);
			}
		}

	}
}
