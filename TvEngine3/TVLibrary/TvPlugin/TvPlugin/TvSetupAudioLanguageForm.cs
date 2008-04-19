using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TvPlugin
{
  public partial class TvSetupAudioLanguageForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    public TvSetupAudioLanguageForm()
    {
      InitializeComponent();
    }
    public void InitForm(List<String> languageCodes,List<String> languages,string preferredLanguages)
    {
      mpListViewLanguages.Items.Clear();
      for (int i = 0; i < languages.Count; i++)
      {
        ListViewItem item = new ListViewItem();
        item.Text = languages[i];
        item.Tag = languageCodes[i];
        item.Checked = preferredLanguages.Contains(languageCodes[i]);
        mpListViewLanguages.Items.Add(item);
      }
    }
    public string GetConfig()
    {
      string prefLangs="";
      foreach (ListViewItem item in mpListViewLanguages.Items)
      {
        if (item.Checked)
          prefLangs += (string)item.Tag + ";";
      }
      return prefLangs;
    }
  }
}