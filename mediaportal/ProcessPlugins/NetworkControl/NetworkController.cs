using System;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using MediaPortal.GUI.Library;

namespace NetworkControl
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class NetworkController : MediaPortal.GUI.Library.IPlugin, MediaPortal.GUI.Library.ISetupForm
	{
		HttpChannel channel=null;
		NetworkControl.Mediaportal mpController = new Mediaportal();
		public NetworkController()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region IPlugin Members

		public void Start()
		{
			channel = new HttpChannel (1095);     
			ChannelServices.RegisterChannel (channel);
			Type type=typeof(NetworkControl.Mediaportal);
			RemotingConfiguration.RegisterWellKnownServiceType(type,"Mediaportal",WellKnownObjectMode.Singleton);
		}

		public void Stop()
		{
			if (channel!=null)
			{
			}
		}

		#endregion

		#region ISetupForm Members

		public bool CanEnable()
		{
			return false;
		}

		public string Description()
		{
			return "Network control plugin";
		}

		public bool DefaultEnabled()
		{
			// TODO:  Add NetworkController.DefaultEnabled implementation
			return true;
		}

		public int GetWindowId()
		{
			return 0;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			// TODO:  Add NetworkController.GetHome implementation
			strButtonText = null;
			strButtonImage = null;
			strButtonImageFocus = null;
			strPictureImage = null;
			return false;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string PluginName()
		{
			// TODO:  Add NetworkController.PluginName implementation
			return "Network control";
		}

		public bool HasSetup()
		{
			// TODO:  Add NetworkController.HasSetup implementation
			return false;
		}

		public void ShowPlugin()
		{
			// TODO:  Add NetworkController.ShowPlugin implementation
		}

		#endregion
	}
}
