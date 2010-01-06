using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralScreensaver : SectionSettings
  {
    #region ctor

    public GeneralScreensaver()
      : this("Screensaver") {}

    public GeneralScreensaver(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        checkBoxEnableScreensaver.Checked = xmlreader.GetValueAsBool("general", "IdleTimer", true);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
        radioBtnBlankScreen.Checked = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValueAsBool("general", "IdleTimer", checkBoxEnableScreensaver.Checked);
        xmlreader.SetValue("general", "IdleTimeValue", numericUpDownDelay.Value);
        xmlreader.SetValueAsBool("general", "IdleBlanking", radioBtnBlankScreen.Checked);
      }
    }

    #endregion

    private void checkBoxEnableScreensaver_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxIdleAction.Enabled = numericUpDownDelay.Enabled = checkBoxEnableScreensaver.Checked;
    }
  }
}