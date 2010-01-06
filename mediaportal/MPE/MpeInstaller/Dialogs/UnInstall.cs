using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.SectionPanel;

namespace MpeInstaller.Dialogs
{
  public partial class UnInstall : Form
  {
    public PackageClass Package;

    public UnInstall()
    {
      InitializeComponent();
    }

    public void Execute(PackageClass packageClass, bool silent)
    {
      if (!silent)
        if (
          MessageBox.Show("Do you want to Unistall extension " + packageClass.GeneralInfo.Name, "Uninstall extension",
                          MessageBoxButtons.YesNo) != DialogResult.Yes)
          return;
      packageClass.UnInstallInfo = new UnInstallInfoCollection(packageClass);
      packageClass.UnInstallInfo = packageClass.UnInstallInfo.Load();
      packageClass.Silent = silent;
      if (packageClass.UnInstallInfo == null)
      {
        if (!silent)
          if (
            MessageBox.Show("No uninstall information found. Do you want to remowe from list ? ", "Uninstall extension",
                            MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;
        packageClass.UnInstallInfo = new UnInstallInfoCollection(packageClass);
      }
      packageClass.FileUnInstalled += packageClass_FileUnInstalled;
      progressBar1.Maximum = packageClass.UnInstallInfo.Items.Count + 1;
      Package = packageClass;
      ShowDialog();
    }

    private void packageClass_FileUnInstalled(object sender, MpeCore.Classes.Events.UnInstallEventArgs e)
    {
      if (progressBar1.Value < progressBar1.Maximum)
        progressBar1.Value++;
      label1.Text = e.Message;
      progressBar1.Refresh();
      label1.Refresh();
      Refresh();
    }

    private void UnInstall_Shown(object sender, EventArgs e)
    {
      Package.UnInstall();
      this.Close();
    }
  }
}