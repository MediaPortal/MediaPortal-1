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
  public partial class DependencyForm : Form
  {
    private BindingSource generalDepBindSource = new BindingSource();
    private BindingSource pluginDepBindSource = new BindingSource();
    private MpeCore.PackageClass package;

    public DependencyForm(MpeCore.PackageClass pak)
    {
      InitializeComponent();
      package = pak;
      this.Text = pak.GeneralInfo.Name + " - Dependencies";
      versionLabel.Text = MediaPortal.Common.Utils.CompatibilityManager.GetCurrentVersion().ToString();
      generalDepBindSource.DataSource = package.Dependencies.Items;
      dataGridView1.AutoGenerateColumns = false;
      SetColumnsGeneral();
      dataGridView1.RowHeadersVisible = false;
      dataGridView1.DataSource = generalDepBindSource;
      dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);

      MpeCore.Classes.DependencyItem dep;
      if (!package.CheckMPDependency(out dep))
        MPdepLabel.Visible = true;
      if (package.IsSkin)
      {
        skindepLabel.Visible = true;
        if (package.CheckSkinDependency(out dep))
          skindepLabel.Visible = false;
      }

      if (!package.ProvidesPlugins())
      {
        tabControl1.Controls.Remove(tabPage2);
      }
      else if (package.PluginDependencies.Items.Count == 0)
      {
        pluginLabel.Visible = true;
        tabControl1.Controls.Remove(tabPage2);
      }
      else
      {
        List<SimplePluginDependency> pluginDeps = new List<SimplePluginDependency>();
        foreach (MpeCore.Classes.PluginDependencyItem item in package.PluginDependencies.Items)
        {
          pluginDeps.Add(new SimplePluginDependency(item));
        }
        pluginDepBindSource.DataSource = pluginDeps;
        dataGridView2.AutoGenerateColumns = true;
        dataGridView2.RowHeadersVisible = false;
        dataGridView2.DataSource = pluginDepBindSource;
        dataGridView2.AutoResizeColumns( DataGridViewAutoSizeColumnsMode.AllCells);
        dataGridView2.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView2_CellFormatting);
      }
    }

    void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      MpeCore.Classes.PluginDependencyItem depItem = package.PluginDependencies.Items[e.RowIndex];
      if (depItem == null)
        return;
      if (MpeCore.PackageClass.CheckPluginDependency(depItem))
        e.CellStyle.ForeColor = Color.Green;
      else
        e.CellStyle.ForeColor = Color.Red;
    }

    private void SetColumnsGeneral()
    {
      dataGridView1.ColumnCount = 4;
      dataGridView1.Columns[0].Name = "Name";
      dataGridView1.Columns[0].DataPropertyName = "Name";
      dataGridView1.Columns[1].Name = "Type";
      dataGridView1.Columns[1].DataPropertyName = "Type";
      dataGridView1.Columns[2].Name = "Min version";
      dataGridView1.Columns[2].DataPropertyName = "MinVersion";
      dataGridView1.Columns[3].Name = "Max version";
      dataGridView1.Columns[3].DataPropertyName = "MaxVersion";
    }

    void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      MpeCore.Classes.DependencyItem depItem = package.Dependencies.Items[e.RowIndex];
      if (depItem == null)
        return;
      if (depItem.Type == "Skin")
      {
        MpeCore.Classes.VersionProvider.SkinVersion skinDep = new MpeCore.Classes.VersionProvider.SkinVersion();
        if (skinDep.Validate(depItem))
          e.CellStyle.ForeColor = Color.Green;
        else
          e.CellStyle.ForeColor = Color.Red;
        return;
      }

      if (!depItem.WarnOnly && !MpeCore.MpeInstaller.VersionProviders[depItem.Type].Validate(depItem))
      {
        e.CellStyle.ForeColor = Color.Red;
      }
      else if(MpeCore.MpeInstaller.VersionProviders[depItem.Type].Validate(depItem))
      {
        e.CellStyle.ForeColor = Color.Green;
      }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == Keys.Escape) this.Close();
      return base.ProcessCmdKey(ref msg, keyData);
    }
  }

  public class SimplePluginDependency
  {
    private MpeCore.Classes.PluginDependencyItem baseItem;
    public string Name
    {
      get { return baseItem.AssemblyName; }
    }
    public string Version
    {
      get 
      {
        if (baseItem.CompatibleVersion.Items.Count > 0)
        {
          return baseItem.CompatibleVersion.Items[0].DesignedForVersion;
        }
        return "No version specified!";
      }
    }

    public SimplePluginDependency(MpeCore.Classes.PluginDependencyItem item)
    {
      baseItem = item;
    }    
  }
}
