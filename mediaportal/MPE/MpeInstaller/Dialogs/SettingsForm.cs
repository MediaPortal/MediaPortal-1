using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MpeInstaller.Dialogs
{
  public partial class SettingsForm : Form
  {
    public SettingsForm()
    {
      InitializeComponent();
    }

    private void chk_update_CheckedChanged(object sender, EventArgs e)
    {
      numeric_Days.Enabled = chk_update.Checked;
      chk_updateExtension.Enabled = chk_update.Checked;
    }
  }
}
