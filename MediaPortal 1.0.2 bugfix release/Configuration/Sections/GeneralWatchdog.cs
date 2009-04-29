using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralWatchdog : SectionSettings
  {
    #region ctor

    public GeneralWatchdog()
      : this("Watchdog")
    {
    }

    public GeneralWatchdog(string name)
      : base("Watchdog")
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxEnableWatchdog.Checked = xmlreader.GetValueAsBool("general", "watchdogEnabled", false);
        checkBoxAutoRestart.Checked = xmlreader.GetValueAsBool("general", "restartOnError", true);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "restart delay", 10);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValueAsBool("general", "watchdogEnabled", checkBoxEnableWatchdog.Checked);
        xmlreader.SetValueAsBool("general", "restartOnError", checkBoxAutoRestart.Checked);
        xmlreader.SetValue("general", "restart delay", numericUpDownDelay.Value);
      }
    }

    #endregion
  }
}