using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	public class GUITrailersPlugin : ISetupForm
	{
		#region ISetupForm Members

		// Returns the name of the plugin which is shown in the plugin menu
		public string PluginName()
		{
			return GUILocalizeStrings.Get(5900);
		}

		// Returns the description of the plugin is shown in the plugin menu
		public string Description()
		{
			return "Browse Trailers & clips on Yahoo Movies";
		}

		// Returns the author of the plugin which is shown in the plugin menu
		public string Author()      
		{
			return "Zipperzip";
		}	
		
		// show the setup dialog
		public void ShowPlugin()  
		{
		}	

		// Indicates whether plugin can be enabled/disabled
		public bool CanEnable()   
		{
			return true;
		}	

		// get ID of windowplugin belonging to this setup
		public int GetWindowId() 
		{
			return 5900;
		}	
		
		// Indicates if plugin is enabled by default;
		public bool DefaultEnabled()
		{
			return true;
		}	
		// indicates if a plugin has its own setup screen
		public bool HasSetup()    
		{
			return false;
		}    
	
		/// <summary>
		/// If the plugin should have its own button on the main menu of Mediaportal then it
		/// should return true to this method, otherwise if it should not be on home
		/// it should return false
		/// </summary>
		/// <param name="strButtonText">text the button should have</param>
		/// <param name="strButtonImage">image for the button, or empty for default</param>
		/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
		/// <param name="strPictureImage">subpicture for the button or empty for none</param>
		/// <returns>true  : plugin needs its own button on home
		///          false : plugin does not need its own button on home</returns>
		public bool   GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) 
		{
			strButtonText=PluginName();
			strButtonImage=String.Empty;
			strButtonImageFocus=String.Empty;
			strPictureImage=String.Empty;
			return true;
		}
		#endregion
	}
}
