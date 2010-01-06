using System.ComponentModel;
using System.ServiceProcess;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralStartupDelay : SectionSettings
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private new const IContainer components = null;

    #region ctor

    public GeneralStartupDelay()
      : this("Startup Delay") {}

    public GeneralStartupDelay(string name)
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
        nudDelay.Value = xmlreader.GetValueAsInt("general", "startup delay", 0);
      }
      //
      // On single seat WaitForTvService is forced enabled !
      //
      cbWaitForTvService.Checked = Common.IsSingleSeat();
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("general", "startup delay", nudDelay.Value);
        xmlreader.SetValueAsBool("general", "wait for tvserver", cbWaitForTvService.Checked);
      }
    }

    #endregion
  }
}