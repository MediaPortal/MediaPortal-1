using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SetupTv.Sections
{
  public partial class GroupNameForm : Form
  {
    string _groupName="new group";

    public GroupNameForm()
    {
      InitializeComponent();
    }

    private void GroupName_Load(object sender, EventArgs e)
    {
      mpTextBox1.Text = _groupName;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      _groupName = mpTextBox1.Text;
      Close();
    }

    public string GroupName
    {
      get
      {
        return _groupName;
      }
      set
      {
        _groupName = value;
      }
    }
  }
}