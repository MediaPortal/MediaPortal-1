using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Support
{
  public partial class ProgressDialog : Form
  {
    public ProgressDialog()
    {
      InitializeComponent();
    }
    public void SetCurrentAction(string currentAction)
    {
      labelCurrentAction.Text=currentAction;
      Update();
    }
    public void UpdateProgress()
    {
      progressBar.PerformStep();
      Update();
    }
  }
}