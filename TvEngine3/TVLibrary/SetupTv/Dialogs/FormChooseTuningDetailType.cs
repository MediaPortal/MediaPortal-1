using System;
using System.Windows.Forms;

namespace SetupTv.Dialogs
{
  public partial class FormChooseTuningDetailType : Form
  {
    private bool _isTv = true;
    private int _tuningType;
    public int TuningType
    {
      get { return _tuningType; }
    }

    public bool IsTv
    {
      get { return _isTv; }
      set { _isTv = value; }
    }

    public FormChooseTuningDetailType()
    {
      InitializeComponent();
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      _tuningType = GetSelectedTuningType();
      DialogResult = DialogResult.OK;
      Close();
    }

    private int GetSelectedTuningType()
    {
      if (mpRadioButton7.Checked)
      {
        return 0;
      }
      if (mpRadioButton1.Checked)
      {
        return 5;
      }
      if (mpRadioButton2.Checked)
      {
        return 1;
      }
      if (mpRadioButton3.Checked)
      {
        return 2;
      }
      if (mpRadioButton4.Checked)
      {
        return 3;
      }
      if (mpRadioButton5.Checked)
      {
        return 4;
      }
      if (mpRadioButton6.Checked)
      {
        return 7;
      }
      return -1;
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void FormChooseTuningDetailType_Load(object sender, EventArgs e)
    {
      if (_isTv)
      {
        mpRadioButton1.Visible = false;
      }
    }
  }
}
