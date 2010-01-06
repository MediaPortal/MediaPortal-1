using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class ParamEditScript : UserControl, IParamEdit
  {
    private SectionParam Param;

    public ParamEditScript()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      EditScript dlg = new EditScript();
      dlg.Script = Param.Value;
      dlg.ShowDialog();
      Param.Value = dlg.Script;
    }

    public void Set(SectionParam param)
    {
      Param = param;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://wiki.team-mediaportal.com/MpeMakerScript");
    }
  }
}