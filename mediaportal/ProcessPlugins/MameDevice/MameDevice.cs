using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.MameDevice
{
	/// <summary>
	/// Summary description for MameDevice.
	/// </summary>
	public class MameDevice : ISetupForm, IPluginReceiver
	{
    const int WM_KEYDOWN             = 0x0100;
    const int WM_SYSKEYDOWN          = 0x0104;

    HCWHandler MameMapper;

		public MameDevice()
		{
    }
    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "MAME Input Device Bridge";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return 0;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "mPod";
    }

    public string PluginName()
    {
      return "MAME Devices";
    }

    public bool HasSetup()
    {
      return false;
    }

    public void ShowPlugin()
    {
    }

    #endregion

    #region IPluginReceiver Members

    public bool WndProc(ref System.Windows.Forms.Message msg)
    {
      if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
      {
        Log.Write("WM_KEYDOWN: wParam {0}", (int)msg.WParam);
        return MameMapper.MapAction((int)msg.WParam);
      }
      return false;
    }

    #endregion

    #region IPlugin Members

    public void Start()
    {
      bool result = false;
      MameMapper = new HCWHandler("MameDevice", out result);
    }

    public void Stop()
    {
      // TODO:  Add MameDevice.Stop implementation
    }

    #endregion
  }
}
