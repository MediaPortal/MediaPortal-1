using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;


namespace TvPlugin
{
  public partial class TvSetupForm : Form
  {
    #region variables
    private string _serverHostName;
    private string _preferredLanguages;
    private bool _preferAC3;
    private bool _rebuildGraphOnNewCard;
    private bool _rebuildGraphOnNewAVSpecs;
    private bool _avoidSeeking;
    private List<String> languagesAvail;
    private List<String> languageCodes;
    #endregion

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _serverHostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
        _preferredLanguages = xmlreader.GetValueAsString("tvservice", "preferredlanguages", "");
        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        _rebuildGraphOnNewAVSpecs = xmlreader.GetValueAsBool("tvservice", "rebuildgraphOnNewAVSpecs", true);
        _rebuildGraphOnNewCard = xmlreader.GetValueAsBool("tvservice", "rebuildgraphOnNewCard", true);
        _avoidSeeking = xmlreader.GetValueAsBool("tvservice", "avoidSeeking", false);
      }
    }
    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("tvservice", "hostname", _serverHostName);
        xmlreader.SetValue("tvservice","preferredlanguages",_preferredLanguages);
        xmlreader.SetValueAsBool("tvservice","preferac3",_preferAC3);
        xmlreader.SetValueAsBool("tvservice", "rebuildgraphOnNewAVSpecs", _rebuildGraphOnNewAVSpecs);
        xmlreader.SetValueAsBool("tvservice", "rebuildgraphOnNewCard", _rebuildGraphOnNewCard);
        xmlreader.SetValueAsBool("tvservice", "avoidSeeking", _avoidSeeking );        
      }
    }
    #endregion

    public TvSetupForm()
    {
      InitializeComponent();
    }

    private void TvSetupForm_Load(object sender, EventArgs e)
    {
      TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
      languagesAvail = languages.GetLanguages();
      languageCodes = languages.GetLanguageCodes();
      LoadSettings();
      mpTextBoxHostname.Text = _serverHostName;
      mpTextBoxPreferredLanguages.Text = _preferredLanguages;
      mpCheckBoxPrefAC3.Checked = _preferAC3;
      mpCheckBoxPrefRebuildGraphOnNewCard.Checked = _rebuildGraphOnNewCard;
      mpCheckBoxPrefRebuildGraphOnNewAVSpecs.Checked = _rebuildGraphOnNewAVSpecs;
      mpCheckBoxavoidSeekingonChannelChange.Checked = _avoidSeeking;

      string toolTip =  "Use this option to make sure that the graph" + Environment.NewLine; 
      toolTip += "is rebuilt when changing to a channel that" + Environment.NewLine; 
      toolTip += "belongs on another card" + Environment.NewLine; 
      toolTip += "This can cause a slightly slower channel" + Environment.NewLine;
      toolTip += "change speed. Should only be used if you are" + Environment.NewLine;
      toolTip += "having problems when changing channels across cards.";
      toolTipChannelChangeOnNewCard.SetToolTip(mpCheckBoxPrefRebuildGraphOnNewCard, toolTip);

      toolTip = "Use this option to make sure that the graph" + Environment.NewLine;
      toolTip += "is rebuilt when changing to a channel that" + Environment.NewLine;
      toolTip += "contains different A/V specs than the previous" + Environment.NewLine;
      toolTip += "channel. This can cause a slightly slower channel" + Environment.NewLine;
      toolTip += "change speed. Should only be used if you are" + Environment.NewLine;
      toolTip += "having problems with ac3 or video codec on channel change.";
      toolTipChannelChangeOnNewAVSpecs.SetToolTip(mpCheckBoxPrefRebuildGraphOnNewAVSpecs, toolTip);      
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

    private void mpCheckBoxPrefRebuildGraphOnNewCard_CheckedChanged(object sender, EventArgs e)
    {
      _rebuildGraphOnNewCard = mpCheckBoxPrefRebuildGraphOnNewCard.Checked;
    }

    private void mpCheckBoxPrefRebuildGraphOnNewAVSpecs_CheckedChanged(object sender, EventArgs e)
    {
      _rebuildGraphOnNewAVSpecs = mpCheckBoxPrefRebuildGraphOnNewAVSpecs.Checked;
    }

    private void mpCheckBoxavoidSeekingonChannelChange_CheckedChanged(object sender, EventArgs e)
    {
      _avoidSeeking = mpCheckBoxavoidSeekingonChannelChange.Checked;
    }

  }
}