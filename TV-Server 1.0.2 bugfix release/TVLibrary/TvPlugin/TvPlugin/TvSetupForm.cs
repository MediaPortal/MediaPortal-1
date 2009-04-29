using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using TvLibrary.Epg;

namespace TvPlugin
{
  public partial class TvSetupForm : MPConfigForm
  {
    #region variables

    private string _serverHostName;
    private string _preferredLanguages;
    private bool _preferAC3;
    private bool _rebuildGraphOnNewVideoSpecs = true;
    private bool _rebuildGraphOnNewAudioSpecs = true;
    private bool _avoidSeeking;
    private List<String> languagesAvail;
    private List<String> languageCodes;

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _serverHostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
        _preferredLanguages = xmlreader.GetValueAsString("tvservice", "preferredlanguages", "");
        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        _rebuildGraphOnNewAudioSpecs = xmlreader.GetValueAsBool("tvservice", "rebuildgraphOnNewAudioSpecs", true);
        _rebuildGraphOnNewVideoSpecs = xmlreader.GetValueAsBool("tvservice", "rebuildgraphOnNewVideoSpecs", true);
        _avoidSeeking = xmlreader.GetValueAsBool("tvservice", "avoidSeeking", false);
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("tvservice", "hostname", _serverHostName);
        xmlreader.SetValue("tvservice", "preferredlanguages", _preferredLanguages);
        xmlreader.SetValueAsBool("tvservice", "preferac3", _preferAC3);

        xmlreader.SetValueAsBool("tvservice", "rebuildgraphOnNewAudioSpecs", _rebuildGraphOnNewAudioSpecs);
        xmlreader.SetValueAsBool("tvservice", "rebuildgraphOnNewVideoSpecs", _rebuildGraphOnNewVideoSpecs);
        xmlreader.SetValueAsBool("tvservice", "avoidSeeking", _avoidSeeking);
      }
    }

    #endregion

    public TvSetupForm()
    {
      InitializeComponent();
    }

    private void TvSetupForm_Load(object sender, EventArgs e)
    {
      Languages languages = new Languages();
      languagesAvail = languages.GetLanguages();
      languageCodes = languages.GetLanguageCodes();
      LoadSettings();
      mpTextBoxHostname.Text = _serverHostName;
      mpTextBoxPreferredLanguages.Text = _preferredLanguages;
      mpCheckBoxPrefAC3.Checked = _preferAC3;
      mpCheckBoxPrefRebuildGraphVideoChanged.Checked = _rebuildGraphOnNewVideoSpecs;
      mpCheckBoxPrefRebuildGraphAudioChanged.Checked = _rebuildGraphOnNewAudioSpecs;
      mpCheckBoxavoidSeekingonChannelChange.Checked = _avoidSeeking;

      string toolTip = "";

      toolTip = "Use this option to make sure that the graph" + Environment.NewLine;
      toolTip += "is rebuilt when changing to a channel that" + Environment.NewLine;
      toolTip += "contains different video specifications than the previous" + Environment.NewLine;
      toolTip += "channel (ex. mpeg2 to h264). This can cause a slightly slower channel" + Environment.NewLine;
      toolTip += "change speed. Should only be used if you are" + Environment.NewLine;
      toolTip += "having problems with video codec/drivers (blank screen) on channel change.";
      toolTipChannelChangeVideoChanged.SetToolTip(mpCheckBoxPrefRebuildGraphVideoChanged, toolTip);

      toolTip = "Use this option to make sure that the graph" + Environment.NewLine;
      toolTip += "is rebuilt when changing to a channel that" + Environment.NewLine;
      toolTip += "contains different audio specifications than the previous" + Environment.NewLine;
      toolTip += "channel (ex. mpeg1 to ac3). This can cause a slightly slower channel" + Environment.NewLine;
      toolTip += "change speed. Should only be used if you are" + Environment.NewLine;
      toolTip += "having problems with audio codec/drivers (no sound) on channel change.";
      toolTipChannelChangeAudioChanged.SetToolTip(mpCheckBoxPrefRebuildGraphAudioChanged, toolTip);
    }

    private void mpTextBoxHostname_TextChanged(object sender, EventArgs e)
    {
      _serverHostName = mpTextBoxHostname.Text;
    }

    private void mpCheckBoxPrefAC3_CheckedChanged(object sender, EventArgs e)
    {
      _preferAC3 = mpCheckBoxPrefAC3.Checked;
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
    }

    private void mpButtonSelectLanguages_Click(object sender, EventArgs e)
    {
      TvSetupAudioLanguageForm frm = new TvSetupAudioLanguageForm();
      frm.InitForm(languageCodes, languagesAvail, _preferredLanguages);
      if (frm.ShowDialog() == DialogResult.OK)
      {
        _preferredLanguages = frm.GetConfig();
        mpTextBoxPreferredLanguages.Text = _preferredLanguages;
      }
    }


    private void mpCheckBoxPrefRebuildGraphOnNewAVSpecs_CheckedChanged(object sender, EventArgs e)
    {
      _rebuildGraphOnNewAudioSpecs = mpCheckBoxPrefRebuildGraphAudioChanged.Checked;
    }

    private void mpCheckBoxavoidSeekingonChannelChange_CheckedChanged(object sender, EventArgs e)
    {
      _avoidSeeking = mpCheckBoxavoidSeekingonChannelChange.Checked;
    }

    private void mpCheckBoxPrefRebuildGraphVideoChanged_CheckedChanged(object sender, EventArgs e)
    {
      _rebuildGraphOnNewVideoSpecs = mpCheckBoxPrefRebuildGraphVideoChanged.Checked;
    }
  }
}