#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Interface for plugin setup configuration screens. 
  /// 
  /// Plugins may have a configuration screen. By implementing this interface in your plugin
  /// MediaPortal will add it to the tools->plugin menu where users can configure your plugin
  /// Look at the home subproject for a sample 
  /// </summary>
  public interface ISetupForm
  {
    string PluginName(); // Returns the name of the plugin which is shown in the plugin menu
    string Description(); // Returns the description of the plugin is shown in the plugin menu
    string Author(); // Returns the author of the plugin which is shown in the plugin menu
    void ShowPlugin(); // show the setup dialog
    bool CanEnable(); // Indicates whether plugin can be enabled/disabled
    int GetWindowId(); // get ID of windowplugin belonging to this setup
    bool DefaultEnabled(); // Indicates if plugin is enabled by default;
    bool HasSetup(); // indicates if a plugin has its own setup screen

    /// <summary>
    /// If the plugin should have its own button on the home menu of Mediaportal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true  : plugin needs its own button on home
    ///          false : plugin does not need its own button on home</returns>
    bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                 out string strPictureImage);
  }

  /// <summary>
  /// Extends the Interface for plugin setup configuration screens.
  /// This Interface adds new features like: ShowDefaultHome()
  /// </summary>
  public interface IShowPlugin
  {
    /// <summary>
    /// Indicates, if a windowplugin is shown by default in Home screen, or in My Plugins
    /// </summary>
    /// <returns>true : plugin defaults to be shown in Home</returns>
    /// <returns>false: plugin defaults to be shown in My Plugins</returns>
    bool ShowDefaultHome();
  }
}