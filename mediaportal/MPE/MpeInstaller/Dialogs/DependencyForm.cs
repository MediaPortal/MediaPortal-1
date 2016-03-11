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
      versionLabel.Text = string.Format("MediaPortal {0} (API: {1})  -  Skin {2}",
        MediaPortal.Common.Utils.CompatibilityManager.MediaPortalReleaseForApiVersion(MediaPortal.Common.Utils.CompatibilityManager.GetCurrentMaxVersion()),
        MediaPortal.Common.Utils.CompatibilityManager.GetCurrentVersion(),
        MediaPortal.Common.Utils.CompatibilityManager.SkinVersion);
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

      if (!package.ProvidesPlugins() && !package.PluginDependencies.Items.Any())
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
        var mpVersion = MediaPortal.Common.Utils.CompatibilityManager.GetCurrentSubSystemVersion("*");
        foreach (MpeCore.Classes.PluginDependencyItem item in package.PluginDependencies.Items)
        {
          pluginDeps.Add(new SimplePluginDependency(item) { CurrentVersion = mpVersion, SubSystem ="*" });
          if (item.SubSystemsUsed != null)
          {
            foreach (var subSystem in item.SubSystemsUsed.Items)
            {
              var subSystemVersion = MediaPortal.Common.Utils.CompatibilityManager.GetCurrentSubSystemVersion(subSystem.Name);
              pluginDeps.Add(new SimplePluginDependency(item) { SubSystem = subSystem.Name, CurrentVersion = subSystemVersion });
            }
          }
        }
        pluginDepBindSource.DataSource = pluginDeps;
        dataGridView2.AutoGenerateColumns = true;
        dataGridView2.RowHeadersVisible = false;
        dataGridView2.DataSource = pluginDepBindSource;
        dataGridView2.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(dataGridView2_CellToolTipTextNeeded);
        dataGridView2.AutoResizeColumns( DataGridViewAutoSizeColumnsMode.AllCells);
        dataGridView2.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView2_CellFormatting);
      }
    }

    void dataGridView2_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
      if (e.RowIndex >= 0 && e.ColumnIndex == 4)
      {
        SimplePluginDependency depItem = pluginDepBindSource[e.RowIndex] as SimplePluginDependency;
        if (depItem.CurrentVersion != null)
          e.ToolTipText = string.Format("since MediaPortal {0}",
              MediaPortal.Common.Utils.CompatibilityManager.MediaPortalReleaseForApiVersion(depItem.CurrentVersion));
      }
    }

    void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      SimplePluginDependency depItem = pluginDepBindSource[e.RowIndex] as SimplePluginDependency;
      if (depItem == null)
        return;
      else
      {
        Version compatibleVersion = null;
        try { compatibleVersion = new Version(depItem.CompatibleVersionItem.DesignedForVersion); }
        catch { }
        if (compatibleVersion != null && depItem.CurrentVersion != null)
        {
          e.CellStyle.ForeColor = depItem.CurrentVersion > compatibleVersion ? Color.Red : Color.Green;
        }
      }
    }

    private void SetColumnsGeneral()
    {
      dataGridView1.ColumnCount = 4;
      dataGridView1.Columns[0].Name = "Name";
      dataGridView1.Columns[0].DataPropertyName = "Name";
      dataGridView1.Columns[1].Name = "Type";
      dataGridView1.Columns[1].DataPropertyName = "Type";
      dataGridView1.Columns[2].Name = "Min";
      dataGridView1.Columns[2].DataPropertyName = "MinVersion";
      dataGridView1.Columns[3].Name = "Max";
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

    internal MpeCore.Classes.CompatibleVersionItem CompatibleVersionItem
    {
      get { return baseItem.CompatibleVersion.Items.FirstOrDefault(); }
    }
    
    public string Name
    {
      get { return baseItem.AssemblyName; }
    }

    [DisplayName("Subsystem")]
    public string SubSystem { get; set; }

    [DisplayName("Min")]
    public string MinVersion
    {
      get
      {
        if (baseItem.CompatibleVersion.Items.Count > 0)
        {
          return baseItem.CompatibleVersion.Items[0].MinRequiredVersion;
        }
        return "No version specified!";
      }
    }

    [DisplayName("Max")]
    public string MaxVersion
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

    internal Version CurrentVersion { get; set; }

    public string Installed
    {
      get 
      {
        return CurrentVersion == null ? "" : CurrentVersion.ToString();
      }
    }

    public SimplePluginDependency(MpeCore.Classes.PluginDependencyItem item)
    {
      baseItem = item;
    }    
  }
}
