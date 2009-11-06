using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeInstaller.Dialogs;

namespace MpeInstaller
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            extensionListControl.UnInstallExtension += extensionListControl_UnInstallExtension;
            extensionListControl.UpdateExtension += extensionListControl_UpdateExtension;
        }

        void extensionListControl_UpdateExtension(object sender, PackageClass packageClass, PackageClass newpackageClass)
        {
            string newPackageLoacation = newpackageClass.GeneralInfo.Location;
            if(!File.Exists(newPackageLoacation))
            {
                newPackageLoacation = newpackageClass.LocationFolder + newpackageClass.GeneralInfo.Id + ".mpe2";
                if(!File.Exists(newPackageLoacation))
                {
                    if (!string.IsNullOrEmpty(newpackageClass.GeneralInfo.OnlineLocation))
                    {
                        newPackageLoacation = Path.GetTempFileName();
                        new DownloadFile(newpackageClass.GeneralInfo.OnlineLocation, newPackageLoacation);
                    }
                }
            }
            if (!File.Exists(newPackageLoacation))
            {
                MessageBox.Show("Can't locate the installer package. Update aborted");
                return;
            }
            PackageClass pak = new PackageClass();
            pak = pak.ZipProvider.Load(newPackageLoacation);
            if (pak.GeneralInfo.Id != newpackageClass.GeneralInfo.Id || pak.GeneralInfo.Version.CompareTo(newpackageClass.GeneralInfo.Version) < 0)
            {
                MessageBox.Show("Invalid update information ! Update aborted!");
                return;
            }
            if (!pak.CheckDependency(false))
            {
                MessageBox.Show("Dependency check error ! Update aborted!");
                return;
            }

            if (MessageBox.Show("This operation update extension " + packageClass.GeneralInfo.Name + " to the version " + pak.GeneralInfo.Version + " \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                return;
            this.Hide();
            UnInstall dlg = new UnInstall();
            dlg.Execute(packageClass, true);
            pak.CopyGroupCheck(packageClass);
            pak.Silent = true;
            pak.StartInstallWizard();
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
            this.Show();

        }

        void extensionListControl_UnInstallExtension(object sender, PackageClass packageClass)
        {
            UnInstall dlg = new UnInstall();
            dlg.Execute(packageClass, false);
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MpeCore.MpeInstaller.Init();
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "Mpe package file(*.mpe2)|*.mpe2|All files|*.*"
                                        };
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                MpeCore.MpeInstaller.Init();
                PackageClass pak = new PackageClass();
                pak = pak.ZipProvider.Load(dialog.FileName);
                if (pak == null)
                {
                    MessageBox.Show("Wrong file format !");
                    return;
                }
                PackageClass installedPak = MpeCore.MpeInstaller.InstalledExtensions.Get(pak.GeneralInfo.Id);
                if (pak.CheckDependency(false))
                {
                    if (installedPak != null)
                    {
                        if (MessageBox.Show("This extension already have a installed version. \n This will be uninstalled first. \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo,MessageBoxIcon.Exclamation) != DialogResult.Yes)
                            return;
                        UnInstall dlg = new UnInstall();
                        dlg.Execute(installedPak, true);
                        pak.CopyGroupCheck(installedPak);
                    }
                    this.Hide();
                    pak.StartInstallWizard();
                    extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
                    this.Show();
                }
            }
        }

        private void btn_online_update_Click(object sender, EventArgs e)
        {
            List<string> onlineFiles = MpeCore.MpeInstaller.InstalledExtensions.GetUpdateUrls();
            if(onlineFiles.Count<1)
            {
                MessageBox.Show("No online update was found !");
                return;
            }

            foreach (string onlineFile in onlineFiles)
            {
                try
                {
                    string file = Path.GetTempFileName();
                    new DownloadFile(onlineFile, file);
                    MpeCore.MpeInstaller.KnownExtensions.Add(ExtensionCollection.Load(file));
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error :" + ex.Message);
                }
            }
            MpeCore.MpeInstaller.Save();
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }
    }
}
