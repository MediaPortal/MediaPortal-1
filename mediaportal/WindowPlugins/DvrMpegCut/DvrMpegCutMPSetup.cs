using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace DvrMpegCutMP
{
	public class DvrMpegCutMPSetup : ISetupForm
	{
		int windowID = 170601;

		public DvrMpegCutMPSetup()
		{ }

		#region ISetupForm Member

		public string Author()
		{
			return "kaybe and brutus, skin by Ralph";
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
			return "to cut mpeg and dvr-ms files";
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = PluginName();
			strButtonImage = String.Empty;
			strButtonImageFocus = String.Empty;
			strPictureImage = String.Empty;
			return true;
		}

		public int GetWindowId()
		{
			return windowID;
		}

		public bool HasSetup()
		{
			return false;
		}

		public string PluginName()
		{
			return "Dvr-MpegCut";
		}

		public void ShowPlugin()
		{
			System.Windows.Forms.MessageBox.Show("Nothing to configure, just enable and start MP ;)");
		}

		#endregion
	}
}
