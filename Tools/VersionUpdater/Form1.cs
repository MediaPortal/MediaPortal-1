using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace VersionUpdater
{
  public partial class Form1 : Form
  {
    private VersionUpdater versionUpdater;
    private VersionResult versionResult;
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void Scan_Click(object sender, EventArgs e)
    {
      folderBrowserDialog1.SelectedPath = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));

      DialogResult result = folderBrowserDialog1.ShowDialog();
      if (result == DialogResult.OK)
      {
        versionUpdater = new VersionUpdater(folderBrowserDialog1.SelectedPath);
        listView1.Items.Clear();
        listView1.Items.Add(nameof(versionUpdater.SolutionInfoAssemblyVersion)).SubItems.Add(versionUpdater.SolutionInfoAssemblyVersion);
        listView1.Items.Add(nameof(versionUpdater.SolutionInfoAssemblyFileVersion)).SubItems.Add(versionUpdater.SolutionInfoAssemblyFileVersion);
        listView1.Items.Add(nameof(versionUpdater.MPConfigAppConfig)).SubItems.Add(versionUpdater.MPConfigAppConfig);
        listView1.Items.Add(nameof(versionUpdater.MPAppConfig)).SubItems.Add(versionUpdater.MPAppConfig);
        listView1.Items.Add(nameof(versionUpdater.InstallerScript)).SubItems.Add(versionUpdater.InstallerScript);
        listView1.Items.Add(nameof(versionUpdater.DeployToolMaxPackageVersion)).SubItems.Add(versionUpdater.DeployToolMaxPackageVersion);
        listView1.Items.Add(nameof(versionUpdater.DeployToolCurrentPackageVersion)).SubItems.Add(versionUpdater.DeployToolCurrentPackageVersion);
        listView1.Items.Add(nameof(versionUpdater.DeployToolCurrentDisplayVersion)).SubItems.Add(versionUpdater.DeployToolCurrentDisplayVersion);

        versionResult = versionUpdater.Validate();
        if (versionResult.Valid)
        {
          tbCurrentVersion.Text = versionResult.Current.ToString();
          tbDescription.Text = versionResult.VersionTitle;
          tbUpgradableFrom.Text = versionResult.UpgradableFrom.ToString();
          btnSave.Enabled = true;
        }
        else
          MessageBox.Show("Current configuration is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

      }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      if (versionResult.Valid)
      {
        Version current;
        Version upgradableFrom;
        if (System.Version.TryParse(tbCurrentVersion.Text, out current) && System.Version.TryParse(tbUpgradableFrom.Text, out upgradableFrom))
        {
          versionUpdater.Save(current, upgradableFrom, tbDescription.Text);
          MessageBox.Show("Successfully saved");
        }
        else
          MessageBox.Show("Versioninfo in current version or upgradable version is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void tbCurrentVersion_Leave(object sender, EventArgs e)
    {
      if (versionResult.Current.ToString() != tbCurrentVersion.Text)
        tbUpgradableFrom.Text = versionResult.Current.ToString();
    }

    private void tbVersion_Validating(object sender, CancelEventArgs e)
    {
      Version version;
      bool valid = System.Version.TryParse(tbCurrentVersion.Text, out version);
      valid = valid && version.Major > 0 && version.Minor >= 0 && version.Build >= 0 && version.Revision == -1;
      errorProvider1.SetError(sender as Control, valid ? String.Empty : "Invalid format for version. It should be f.e. 1.39.1");
      e.Cancel = !valid;
    }
  }
}
