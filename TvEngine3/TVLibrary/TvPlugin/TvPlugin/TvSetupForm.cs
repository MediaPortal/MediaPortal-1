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
      }
    }
    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("tvservice", "hostname", _serverHostName);
        xmlreader.SetValue("tvservice","preferredlanguages",_preferredLanguages);
        xmlreader.SetValueAsBool("tvservice","preferac3",_preferAC3);
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
  }
}