using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralScreensaver : SectionSettings
  {
    #region ctor

    public GeneralScreensaver()
      : this("Screensaver")
    {
    }

    public GeneralScreensaver(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxEnableScreensaver.Checked = xmlreader.GetValueAsBool("general", "screensaver", false);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "screensavertime", 60);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValueAsBool("general", "screensaver", checkBoxEnableScreensaver.Checked);
        xmlreader.SetValue("general", "screensavertime", numericUpDownDelay.Value);
      }
    }

    #endregion
  }
}