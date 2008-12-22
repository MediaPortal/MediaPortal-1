using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class login_form : MPInstallerForm
  {
    public string username = string.Empty;
    public string password = string.Empty;
    public login_form(string usr,string pass)
    {
      InitializeComponent();
      textBox1.Text = usr;
      textBox2.Text = pass;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      username = textBox1.Text;
      password = textBox2.Text;
      this.Close();
    }
  }
}