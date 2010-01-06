using System;
using System.Collections.Specialized;
using TvDatabase;
using System.IO;
using TvLibrary;

namespace SetupTv.Sections
{
  public partial class Epg : SectionSettings
  {
    private string crcSettingsFile = DebugSettings.SettingPath("DisableCRCCheck");

    public Epg()
      : this("DVB EPG") {}

    public Epg(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      TvBusinessLayer layer = new TvBusinessLayer();

      checkBoxAlwaysFillHoles.Checked = (layer.GetSetting("generalEPGAlwaysFillHoles", "no").Value == "yes");
      checkBoxAlwaysUpdate.Checked = (layer.GetSetting("generalEPGAlwaysReplace", "no").Value == "yes");

      checkBoxEnableEPGWhileIdle.Checked = (layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes");
      checkBoxEnableCRCCheck.Checked = !DebugSettings.DisableCRCCheck;
      numericUpDownEpgTimeOut.Value = Convert.ToDecimal(layer.GetSetting("timeoutEPG", "10").Value);
      numericUpDownEpgRefresh.Value = Convert.ToDecimal(layer.GetSetting("timeoutEPGRefresh", "240").Value);
      checkBoxEnableEpgWhileTimeshifting.Checked = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value ==
                                                    "yes");
      numericUpDownTSEpgTimeout.Value = Convert.ToDecimal(layer.GetSetting("timeshiftingEpgGrabberTimeout", "2").Value);

      edTitleTemplate.Text = layer.GetSetting("epgTitleTemplate", "%TITLE%").Value;
      edDescriptionTemplate.Text = layer.GetSetting("epgDescriptionTemplate", "%DESCRIPTION%").Value;
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();

      Setting s = layer.GetSetting("generalEPGAlwaysFillHoles", "no");
      s.Value = checkBoxAlwaysFillHoles.Checked ? "yes" : "no";
      s.Persist();

      s = layer.GetSetting("generalEPGAlwaysReplace", "no");
      s.Value = checkBoxAlwaysUpdate.Checked ? "yes" : "no";
      s.Persist();

      DebugSettings.DisableCRCCheck = !checkBoxEnableCRCCheck.Checked;

      s = layer.GetSetting("idleEPGGrabberEnabled", "yes");
      s.Value = checkBoxEnableEPGWhileIdle.Checked ? "yes" : "no";
      s.Persist();

      s = layer.GetSetting("timeoutEPG", "10");
      s.Value = numericUpDownEpgTimeOut.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutEPGRefresh", "240");
      s.Value = numericUpDownEpgRefresh.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftingEpgGrabberEnabled", "no");
      s.Value = checkBoxEnableEpgWhileTimeshifting.Checked ? "yes" : "no";
      s.Persist();

      s = layer.GetSetting("timeshiftingEpgGrabberTimeout", "2");
      s.Value = numericUpDownTSEpgTimeout.Value.ToString();
      s.Persist();

      s = layer.GetSetting("epgTitleTemplate", "%TITLE%");
      s.Value = edTitleTemplate.Text;
      s.Persist();

      s = layer.GetSetting("epgDescriptionTemplate", "%DESCRIPTION%");
      s.Value = edDescriptionTemplate.Text;
      s.Persist();
    }

    private static string EvalTemplate(string template, NameValueCollection values)
    {
      for (int i = 0; i < values.Count; i++)
        template = template.Replace(values.Keys[i], values[i]);
      return template;
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      NameValueCollection defaults = new NameValueCollection();
      defaults.Add("%TITLE%", "Over the hedge");
      defaults.Add("%DESCRIPTION%",
                   "A scheming raccoon fools a mismatched family of forest creatures into helping him repay a debt of food, by invading the new suburban sprawl that popped up while they were hibernating...and learns a lesson about family himself.");
      defaults.Add("%GENRE%", "movie/drama (general)");
      defaults.Add("%STARRATING%", "6");
      defaults.Add("%STARRATING_STR%", "***+");
      defaults.Add("%CLASSIFICATION%", "PG");
      defaults.Add("%PARENTALRATING%", "8");
      defaults.Add("%NEWLINE%", Environment.NewLine);
      edTitleTest.Text = EvalTemplate(edTitleTemplate.Text, defaults);
      edDescriptionTest.Text = EvalTemplate(edDescriptionTemplate.Text, defaults);
    }
  }
}