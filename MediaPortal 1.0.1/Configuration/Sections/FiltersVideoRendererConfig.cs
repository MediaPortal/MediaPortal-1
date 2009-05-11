using System;
using MediaPortal.Profile;
using MediaPortal.ServiceImplementations;
using OsDetection;

namespace MediaPortal.Configuration.Sections
{
  public partial class FiltersVideoRenderer : SectionSettings
  {
    public FiltersVideoRenderer()
      : this("Video Renderer Settings")
    {
    }

    public FiltersVideoRenderer(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      base.LoadSettings();
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        //VMR9 settings
        checkboxMpNonsquare.Checked = xmlreader.GetValueAsBool("general", "nonsquare", true);
          // http://msdn2.microsoft.com/en-us/library/ms787438(VS.85).aspx
        checkboxDXEclusive.Checked = xmlreader.GetValueAsBool("general", "exclusivemode", true);
        mpVMR9FilterMethod.Text = xmlreader.GetValueAsString("general", "dx9filteringmode", "Gaussian Quad Filtering");
          // http://msdn2.microsoft.com/en-us/library/ms788066.aspx
        checkBoxVMRWebStreams.Checked = xmlreader.GetValueAsBool("general", "usevrm9forwebstreams", true);
        checkBoxDecimateMask.Checked = xmlreader.GetValueAsBool("general", "dx9decimatemask", false);
          // http://msdn2.microsoft.com/en-us/library/ms787452(VS.85).aspx

        bool ValueEVR = false;

        try
        {
          //EVR - VMR9 selection
          OSVersionInfo os = new OperatingSystemVersion();
          int ver = (os.OSMajorVersion*10) + os.OSMinorVersion;
          ValueEVR = ver >= 60 ? true : false;
        }
        catch (Exception ex)
        {
          Log.Error("FilterVideoRendererConfig: Os detection unsuccessful - {0}", ex.Message);
        }

        radioButtonEVR.Checked = xmlreader.GetValueAsBool("general", "useEVRenderer", ValueEVR);
      }
    }

    //public override void OnSectionActivated()
    //{
    //  base.OnSectionActivated();
    //}

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("general", "nonsquare", checkboxMpNonsquare.Checked);
        xmlwriter.SetValueAsBool("general", "exclusivemode", checkboxDXEclusive.Checked);
        xmlwriter.SetValue("general", "dx9filteringmode", mpVMR9FilterMethod.Text);
        xmlwriter.SetValueAsBool("general", "usevrm9forwebstreams", checkBoxVMRWebStreams.Checked);
        xmlwriter.SetValueAsBool("general", "dx9decimatemask", checkBoxDecimateMask.Checked);
        xmlwriter.SetValueAsBool("general", "useEVRenderer", radioButtonEVR.Checked);
      }
    }

    private void radioButtonEVR_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonEVR.Checked == true)
      {
        checkBoxVMRWebStreams.Enabled = false;
        checkboxDXEclusive.Enabled = false;
        checkboxMpNonsquare.Enabled = false;
        checkBoxDecimateMask.Enabled = false;
        mpVMR9FilterMethod.Enabled = false;
        labelFilteringHint.Enabled = false;
      }
    }

    private void radioButtonVMR9_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonVMR9.Checked == true)
      {
        checkBoxVMRWebStreams.Enabled = true;
        checkboxDXEclusive.Enabled = true;
        checkboxMpNonsquare.Enabled = true;
        checkBoxDecimateMask.Enabled = true;
        mpVMR9FilterMethod.Enabled = true;
        labelFilteringHint.Enabled = true;
      }
    }
  }
}