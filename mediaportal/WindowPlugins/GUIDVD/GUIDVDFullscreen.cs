#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Services;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Adds "Play DVD" button to homescreen
  /// </summary>
  public class GUIDVDFullscreen : GUIWindow, ISetupForm, IShowPlugin
  {

    public GUIDVDFullscreen()
      : base()
    {
      GetID = (int)GUIWindow.Window.WINDOW_DVD;
    }

    public override bool Init()
    {
      return true;
    }

		public override bool OnMessage(GUIMessage message)
		{
			Log.Info("DVDFullscreen: Message: {0}", message.Message.ToString());
			if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
			{
				GUIWindowManager.ReplaceWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
				ISelectDVDHandler selectDVDHandler;
				if (GlobalServiceProvider.IsRegistered<ISelectDVDHandler>())
				{
					selectDVDHandler = GlobalServiceProvider.Get<ISelectDVDHandler>();
				}
				else
				{
					selectDVDHandler = new SelectDVDHandler();
					GlobalServiceProvider.Add<ISelectDVDHandler>(selectDVDHandler);
				}
				string dvdToPlay = selectDVDHandler.ShowSelectDVDDialog(GetID);
				if (dvdToPlay == null || !selectDVDHandler.OnPlayDVD(dvdToPlay, GetID))
				{
          Log.Info("DVDFullscreen: Returning from DVD screen");
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, this.GetID, 0, 0, GetID, 0, null);
					return this.OnMessage(msg);	// Send a de-init msg
				}
				return true;
			}
			return base.OnMessage(message);
		}

    public override void Process()
    {
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public string PluginName()
    {
      return "Play DVD";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(341);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = "";
      return true;
    }

    public string Author()
    {
      return "Mosquiss";
    }

    public string Description()
    {
      return @"Play DVD directly from via menu";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIVideoFiles.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}