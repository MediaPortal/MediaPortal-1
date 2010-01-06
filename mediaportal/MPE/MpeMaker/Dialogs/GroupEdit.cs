using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class GroupEdit : Form
  {
    public GroupItem group = new GroupItem();

    public GroupEdit()
    {
      InitializeComponent();
    }

    private void GroupEdit_Load(object sender, EventArgs e) {}

    public void Set(GroupItem item)
    {
      group = item;
      txt_name.Text = item.Name;
      txt_displayname.Text = item.DisplayName;
    }

    public GroupItem Get()
    {
      group.Name = txt_name.Text;
      group.DisplayName = txt_displayname.Text;
      return group;
    }

    private void txt_name_TextChanged(object sender, EventArgs e) {}
  }
}