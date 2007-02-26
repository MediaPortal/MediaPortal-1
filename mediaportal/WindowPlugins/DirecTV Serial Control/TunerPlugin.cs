using System;
using System.Text;

using MediaPortal.GUI.Library;


namespace DirecTV.TunerPlugin
{
  public class TunerPlugin : IPlugin, ISetupForm
  {

    private const int _windowID = 0;                // process plugin - doesn't need windowID
    private DirecTVSettings _settings;              // Serialized settings placeholder
    private SerialInterface _serialInterface;       // generic interface to DirecTV boxes
    
		public TunerPlugin()
		{
    }

    public void Start()
		{
      _settings = new DirecTVSettings();
      _settings.LoadSettings();
      _serialInterface = new SerialInterface(
        _settings.BoxType,
        _settings.CommandSet,
        _settings.KeyMap,
        _settings.SerialPortName,
        _settings.BaudRate,
        _settings.ReadTimeout
        );
      _serialInterface.PowerOnBeforeTuning = _settings.PowerOn;
      _serialInterface.UseSetChannelForTune = _settings.UseSetChannelForTune;
      _serialInterface.TwoWayDisable = _settings.TwoWayDisable;
      _serialInterface.HideOSD = _settings.HideOSD;
      _serialInterface.AllowDigitalSubchannels = _settings.AllowDigitalSubchannels;

      if (_settings.Debug)
      {
        _serialInterface.OnDebugMessage += new SerialInterface.DebugMessageHandler(Log.Debug);
        Log.Debug("DirecTV tuner plugin extensive logging enabled");
        _serialInterface.DumpConfig();
      }
      if (_settings.AllowDigitalSubchannels)
      {
        Log.Debug("DirecTV tuner plugin Allowing Digital Subchannels");
      }
      _serialInterface.OpenPort();
      _settings = null;
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      Log.Debug("DirecTV tuner plugin is started");
    }

    public void Stop()
    {
      _serialInterface.ClosePort();
      _serialInterface = null;
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnThreadMessage);
      Log.Debug("DirecTV tuner plugin is stopped");
    }

    private void OnThreadMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL)
      {
        Log.Debug("DirecTV TUNE Message Recieved: {0}", message.Label);

        string channelString = message.Label;
        if (_serialInterface.AllowDigitalSubchannels)
        {
          _serialInterface.TuneToChannel(channelString);
        }
        else
        {
          int channelNumber = Convert.ToInt32(channelString);
          if (channelNumber >= 0 && channelNumber <= 65535)
          {
            _serialInterface.TuneToChannel(channelNumber);
          }
        }
      }
    }
        
    #region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string PluginName()
		{
			return "DirecTV serial tuner";
		}

		public bool HasSetup()
		{
			return true;
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return _windowID;
		}

		public bool GetHome(
      out string strButtonText,
      out string strButtonImage,
      out string strButtonImageFocus,
      out string strPictureImage
      )
		{
			strButtonText = string.Empty;
			strButtonImage = string.Empty;
			strButtonImageFocus = string.Empty;
			strPictureImage = string.Empty;
			return false;
		}

		public string Author()
		{
			return "micheloe";
		}

		public string Description()
		{
			return "DirectTV Serial Tuner Plugin";
		}

		public void ShowPlugin()
		{
      System.Windows.Forms.Form f = new SetupForm();
      f.ShowDialog();
    }

		#endregion
  }
}
