using System;
using System.Windows.Forms;

using MediaPortal.GUI.Library;

namespace MediaPortal.Remotes
{
	public sealed class InputDevices
	{
		#region Constructors

		private InputDevices()
		{
		}

		#endregion Constructors

		#region Methods

		public static void Init(/* SplashScreen splashScreen */)
		{
			MCE2005Remote.Init(GUIGraphicsContext.ActiveForm);
			FireDTVRemote.Init(GUIGraphicsContext.ActiveForm);

			HCWRemote = new HCWRemote();
			if (HCWRemote.Enabled)
			{
//				if (splashScreen != null) splashScreen.SetInformation("Initializing Hauppauge remote...");
				HCWRemote.Init();
			}
		}

		public static void Stop()
		{
			MCE2005Remote.DeInit();
			HCWRemote.DeInit();
			FireDTVRemote.DeInit();
      diRemote.Stop();
		}

		public static bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
		{
			action = null;
			key = (char)0;
			keyCode = Keys.A;

			HCWRemote.WndProc(msg);

			if(HIDListener.WndProc(ref msg, out action, out key, out keyCode))
				return true;

			if(MCE2005Remote.WndProc(ref msg, out action, out key, out keyCode))
				return true;

		    if(FireDTVRemote.WndProc(ref msg, out action, out  key, out keyCode))
				return true;

			return false;
		}

		#endregion Methods

		#region Fields

		static MCE2005Remote MCE2005Remote = new MCE2005Remote();
		static HCWRemote HCWRemote = new HCWRemote();
    static DirectInputHandler diRemote = new DirectInputHandler();
		static MediaPortal.RemoteControls.FireDTVRemote FireDTVRemote = new MediaPortal.RemoteControls.FireDTVRemote();

		#endregion Fields
	}
}
