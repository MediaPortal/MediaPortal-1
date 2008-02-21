using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MPTail
{
  public partial class frmSearchParams : Form
  {
    public frmSearchParams(string caption,SearchParameters searchParams)
    {
      InitializeComponent();
      this.Text = caption;
      edSearch.Text = searchParams.searchStr;
      edColor.BackColor = searchParams.highlightColor;
      cbCase.Checked = searchParams.caseSensitive;
    }
    public void GetConfig(SearchParameters searchParams)
    {
      searchParams.searchStr=edSearch.Text;
      searchParams.highlightColor = edColor.BackColor;
      searchParams.caseSensitive = cbCase.Checked;
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }

    private void btnChooseColor_Click(object sender, EventArgs e)
    {
      colorDialog1.Color = edColor.BackColor;
      if (colorDialog1.ShowDialog() == DialogResult.OK)
        edColor.BackColor = colorDialog1.Color;
    }
  }
}