using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Plugin interface for non-GUI plugins (process plugins)
	/// process plugins dont have an window and do all their processing in the background
	/// Example of a process plugin is the winlirc plugin which receives the actions from a remote control
	/// and sends them to mediaportal
	/// Process plugins should be copied in the plugins/process folder of mediaportal
	/// Process plugins can have their own setup by implementing ISetupForm
	/// </summary>
	public interface IPlugin
	{
		/// <summary>
		/// This method will be called by mediaportal to start your process plugin
		/// </summary>
    void Start();

		
		/// <summary>
		/// This method will be called by mediaportal to stop your process plugin
		/// </summary>
    void Stop();
	}

	public interface IPluginReceiver:IPlugin
	{
		/// <summary>
		/// This method will be called by mediaportal to send system messages to your process plugin,
		/// if the plugin implements WndProc (optional) / added by mPod
		/// </summary>
		bool WndProc(ref System.Windows.Forms.Message msg);
	}
}
