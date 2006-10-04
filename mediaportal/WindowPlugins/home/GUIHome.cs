#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

#region Usings
using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Topbar;
#endregion

namespace MediaPortal.GUI.Home
{
	/// <summary>
	/// The implementation of the GUIHome Window.  (This window is coupled to the home.xml skin file).
	/// </summary>
  public class GUIHome : GUIHomeBaseWindow //, ISetupForm, IShowPlugin
	{

		#region Constructor
		public GUIHome()
		{
      //GetID = 4000; 
			GetID =(int)GUIWindow.Window.WINDOW_HOME;
    }
		#endregion

    #region Override
    public override bool Init()
		{
      return (Load(GUIGraphicsContext.Skin + @"\myHome.xml"));
		}

		protected override void LoadButtonNames()
		{
			if (menuMain == null) return;
			menuMain.ButtonInfos.Clear();
			ArrayList plugins = PluginManager.SetupForms;
			foreach (ISetupForm setup in plugins)
			{
        string plugInText;
        string focusTexture;
        string nonFocusTexture;
        string hover;
        if (setup.GetHome(out plugInText, out focusTexture, out nonFocusTexture, out hover))
        {
					IShowPlugin showPlugin = setup as IShowPlugin;
          if (setup.PluginName().Equals("Home")) continue;
					if (_useMyPlugins)
					{
						if ((showPlugin != null) && (showPlugin.ShowDefaultHome() == false)) continue;
					}
          if ((focusTexture == null) || (focusTexture.Length < 1))       focusTexture    = setup.PluginName();
          if ((nonFocusTexture == null) || (nonFocusTexture.Length < 1)) nonFocusTexture = setup.PluginName();
          if ((hover == null) || (hover.Length < 1))                     hover           = setup.PluginName();
          focusTexture    = GetFocusTextureFileName(focusTexture);
          nonFocusTexture = GetNonFocusTextureFileName(nonFocusTexture);
          hover           = GetHoverFileName(hover);
          menuMain.ButtonInfos.Add(new MenuButtonInfo(plugInText, setup.GetWindowId(), focusTexture, nonFocusTexture, hover));
				}
			}
      if (_useMyPlugins)
      {
        string focusTexture    = GetFocusTextureFileName("my plugins");
        string nonFocusTexture = GetNonFocusTextureFileName("my plugins");
        string hover           = GetHoverFileName("my plugins");
        menuMain.ButtonInfos.Add(new MenuButtonInfo(GUILocalizeStrings.Get(913), (int)GUIWindow.Window.WINDOW_MYPLUGINS, focusTexture, nonFocusTexture, hover));
      }
		}
 
    #endregion
/*      
    #region ISetupForm members
    public string PluginName()
    {
      return "Home";
    }

    public string Description()
    {
      return "Home";
    }

    public string Author()
    {
      return "Bavarian";
    }
    public void ShowPlugin()
    {
      System.Windows.Forms.Form setup = new GUIHomeSetupForm();
      setup.ShowDialog();
    }
    //System.Reflection.TargetInvocationException
    public bool HasSetup()
    {
      return true;
    }

    public bool CanEnable()
    {
      return true;
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
      strButtonText = PluginName();
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return true;
    }

    #endregion

    #region IShowPlugin members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion
    */
	}
}
