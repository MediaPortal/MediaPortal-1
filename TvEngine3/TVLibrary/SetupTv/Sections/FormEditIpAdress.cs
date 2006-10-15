using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SetupTv.Sections
{
  public partial class FormEditIpAdress : Form
  {
    string _hostName = "";
    public FormEditIpAdress()
    {
      InitializeComponent();
    }

    private void FormEditIpAdress_Load(object sender, EventArgs e)
    {
      textBox1.Text = HostName;

    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      HostName= textBox1.Text ;
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    public string HostName
    {
      get
      {
        return _hostName;
      }
      set
      {
        _hostName = value;
      }
    }
  }
}