using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TvDatabase;
using TvLibrary.Log;


namespace SetupTv.Sections
{
  public partial class PTVGSetup : SetupTv.SectionSettings
  {
    #region constructor

    public PTVGSetup()
      : this("Personal TV Guide Setup")
    {
      InitializeComponent();
    }

    public PTVGSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Public Members

    public override void OnSectionActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      debug.Checked = layer.GetSetting("PTVGDebugMode", "false").Value == "true";
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("PTVGDebugMode", "false");
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