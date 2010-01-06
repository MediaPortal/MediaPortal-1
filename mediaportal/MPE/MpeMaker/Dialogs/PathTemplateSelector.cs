using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;

namespace MpeMaker.Dialogs
{
  public partial class PathTemplateSelector : Form
  {
    public PathTemplateSelector()
    {
      InitializeComponent();
    }

    public string Result { get; set; }

    private void PathTemplateSelector_Load(object sender, EventArgs e)
    {
      listView1.Items.Clear();
      foreach (var pathprovider in MpeInstaller.PathProviders)
      {
        ListViewGroup group = new ListViewGroup(pathprovider.Value.Name);
        listView1.Groups.Add(group);
        foreach (var paths in pathprovider.Value.Paths)
        {
          ListViewItem item = new ListViewItem(paths.Key);
          item.Group = group;
          item.SubItems.Add(paths.Value);
          listView1.Items.Add(item);
        }
      }
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        DialogResult = DialogResult.OK;
        Result = listView1.SelectedItems[0].Text;
        Close();
      }
    }
  }
}