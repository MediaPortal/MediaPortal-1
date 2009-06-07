using System;
using System.Windows.Forms;

namespace MPTail
{
  public partial class frmFindSettings : Form
  {
    public frmFindSettings()
    {
      InitializeComponent();
      checkBoxReverse.Checked = true;
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public string SearchString
    {
      get { return textBox1.Text; }
    }

    public RichTextBoxFinds Options
    {
      get
      {
        RichTextBoxFinds finds = RichTextBoxFinds.None;
        if (checkBoxMatchCase.Checked)
          finds = finds | RichTextBoxFinds.MatchCase;
        if (checkBoxWholeWord.Checked)
          finds = finds | RichTextBoxFinds.WholeWord;
        if (checkBoxReverse.Checked)
          finds = finds | RichTextBoxFinds.Reverse;
        return finds;
      }
    }
  }
}