using MediaPortal.Util;

namespace MediaPortal.ControlDevices
{
  public abstract class AbstractControlSettings : IControlSettings
  {
    static private string CONFIG_SECTION = "controldevice";
    static private string CONFIG_FILE = "MediaPortal.xml";
    protected IControlPlugin _plugin;
    protected string _prefix;
    protected bool _enabled;
    protected bool _enableInput;
    protected bool _enableOutput;

    /// <summary>
    /// 
    /// </summary>
    bool _verbose;

    private AbstractControlSettings() { }

    public AbstractControlSettings(IControlPlugin plugin,string prefix)
    {
      _plugin = plugin;
      _prefix = prefix;
    }

    public bool Enabled
    {
      set { _enabled = value; }
      get { return _enabled; }
    }

    public bool EnableInput
    {
      set { _enableInput = value; }
      get { return _enableInput; }
    }

    public bool EnableOutput
    {
      set { _enableOutput = value; }
      get { return _enableOutput; }
    }

    public bool Verbose
    {
      set { _enabled = value; }
      get { return _enabled; }
    }

    public string Prefix
    {
      get { return _prefix; }
    }

    public void Load()
    {
      _enabled = GetValueAsBool("enable", _enabled);
      _enableInput = GetValueAsBool("enable_input", _enableInput);
      _enableOutput = GetValueAsBool("enable_output", _enableOutput);
      _verbose = GetValueAsBool("verbose", _verbose);
    }

    public void Save()
    {
      SetValueAsBool("enable", _enabled);
      SetValueAsBool("verbose", _verbose);
    }

    public abstract System.Windows.Forms.UserControl SettingsPanel { get; }

    public abstract void ShowAdvancedSettings();

    protected void SetValueAsBool(string name, bool val)
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, CONFIG_FILE)))
      {
        xmlwriter.SetValueAsBool(CONFIG_SECTION, _prefix + "_" + name, val);
      }
    }

    protected void SetValue(string name,object val)
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, CONFIG_FILE)))
      {
        xmlwriter.SetValue(CONFIG_SECTION, _prefix + "_" + name, val);
      }
    }


    protected object GetValue(string name)
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, CONFIG_FILE)))
      {
        return xmlreader.GetValue(CONFIG_SECTION, _prefix + "_" + name);
      }
    }

    protected bool GetValueAsBool(string name, bool def)
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, CONFIG_FILE)))
      {
        return xmlreader.GetValueAsBool(CONFIG_SECTION, _prefix + "_" + name, def);
      }
    }
  }
}
