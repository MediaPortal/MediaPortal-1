using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;
namespace SetupTv.Sections
{
  public partial class Servers : SectionSettings
  {
    public Servers()
      : this("TV Servers")
    {
    }

    public Servers(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      EntityList<Server> servers=DatabaseManager.Instance.GetEntities<Server>();
      mpListView1.Items.Clear();
      foreach (Server server in servers)
      {
        ListViewItem item = mpListView1.Items.Add(server.HostName);
        if (server.IsMaster)
        {
          item.SubItems.Add("Master");
        }
        else
        {
          item.SubItems.Add("Slave");
        }
        item.Tag = server;
      }
    }

    private void Servers_Load(object sender, EventArgs e)
    {

    }

    private void buttonDelete_Click(object sender, EventArgs e)
    {

      if (mpListView1.SelectedIndices.Count < 1) return;
      int index = mpListView1.SelectedIndices[0];
      ListViewItem item = mpListView1.Items[index];
      Server server = (Server)item.Tag;
      server.DeleteAll();
      OnSectionActivated();
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {

    }

    private void buttonMaster_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count < 1) return;
      int index = mpListView1.SelectedIndices[0];
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[i];
        Server server = (Server)item.Tag;
        if (i != index)
        {
          item.SubItems[0].Text = "Slave";
          server.IsMaster = false;
        }
        else
        {
          item.SubItems[0].Text = "Master";
          server.IsMaster = true;
          RemoteControl.HostName = server.HostName;
        }
        server.IsMaster = false;
      }
    }
  }
}