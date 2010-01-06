using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class CMSetup : SetupTv.SectionSettings
  {
    #region constructors

    public CMSetup()
      : this("Conflicts Manager Setup") {}

    public CMSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Public Members

    public override void OnSectionActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      analyzeMode.SelectedIndex = Convert.ToInt32(layer.GetSetting("CMAnalyzeMode", "0").Value);
      debug.Checked = layer.GetSetting("CMDebugMode", "false").Value == "true";
    }

    public override void OnSectionDeActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("CMDebugMode", "false");
      if (debug.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
      base.OnSectionDeActivated();
    }

    #endregion
  }
}