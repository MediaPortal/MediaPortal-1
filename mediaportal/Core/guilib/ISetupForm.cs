using System;

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
    string PluginName();  // Returns the name of the plugin which is shown in the plugin menu
    string Description(); // Returns the description of the plugin is shown in the plugin menu
    string Author();      // Returns the author of the plugin which is shown in the plugin menu
    void   ShowPlugin();  // show the setup dialog
    bool   CanEnable();   // Indicates whether plugin can be enabled/disabled
    int    GetWindowId(); // get ID of windowplugin belonging to this setup
		bool   DefaultEnabled(); // Indicates if plugin is enabled by default;
    bool   HasSetup();    // indicates if a plugin has its own setup screen
    
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
    bool   GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage); 
  }
}
