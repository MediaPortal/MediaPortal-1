using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.Topbar
{
	/// <summary>
	/// 
	/// </summary>
	public class TopBarSetup : ISetupForm
	{
		public TopBarSetup()
		{
    }
    #region ISetupForm Members
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
    public string Description()
    {
      return "Render and manages the navigation bar";
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_TOPBAR;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = "";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "Topbar";
    }

    public void ShowPlugin()
    {
      System.Windows.Forms.Form setup = new TopBarSetupForm();
      setup.ShowDialog();
    }

    #endregion
  }
}
