using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class ParamEditBool : UserControl, IParamEdit
  {
    private SectionParam Param;

    public ParamEditBool()
    {
      InitializeComponent();
    }

    #region IParamEdit Members

    public void Set(SectionParam param)
    {
      Param = param;
      if (Param.GetValueAsBool())
        radio_Yes.Checked = true;
      else
        radio_No.Checked = true;
    }

    #endregion

    private void radio_No_CheckedChanged(object sender, EventArgs e)
    {
      if (radio_Yes.Checked)
        Param.Value = "YES";
      else
        Param.Value = "NO";
    }
  }
}