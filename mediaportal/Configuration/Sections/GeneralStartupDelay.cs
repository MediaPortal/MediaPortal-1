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
      : this("Startup Delay")
    {
    }

    public GeneralStartupDelay(string name)
      : base("Startup Delay")
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        nudDelay.Value = xmlreader.GetValueAsInt("general", "startup delay", 0);
        //cbWaitForTvService.Checked = xmlreader.GetValueAsBool("general", "wait for tvserver", false);
      }
      //
      // If TvService exist on local machine, then we are in singleseat
      //
      cbWaitForTvService.Checked = false;

      if (Util.Utils.UsingTvServer)
      {
        foreach (ServiceController ctrl in ServiceController.GetServices())
        {
          if (ctrl.DisplayName == "TVService")
          {
            //
            // On single seat WaitForTvService is forced enabled !
            //
            cbWaitForTvService.Checked = true;
            break;
          }
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("general", "startup delay", nudDelay.Value);
        xmlreader.SetValueAsBool("general", "wait for tvserver", cbWaitForTvService.Checked);
      }
    }

    #endregion
  }
}