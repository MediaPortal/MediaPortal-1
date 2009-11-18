using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Ionic.Zip;
using MediaPortal.Configuration;
using MpeCore;
using MpeCore.Classes;
using MpeInstaller.Dialogs;
using MpeInstaller.Classes;

namespace MpeInstaller
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Init();
        }

        public MainForm(ProgramArguments args)
        {
            Init();
            if(File.Exists(args.PackageFile))
            {
                InstallFile(args.PackageFile, args.Silent);
                this.Close();
                return;
            }
        }

        private void Init()
        {
            InitializeComponent();
            extensionListControl.UnInstallExtension += extensionListControl_UnInstallExtension;
            extensionListControl.UpdateExtension += extensionListControl_UpdateExtension;
            extensionListControl.ConfigureExtension += extensionListControl_ConfigureExtension;
            extensionListControl.InstallExtension += extensionListControl_InstallExtension;
            extensionListContro_all.UnInstallExtension += extensionListControl_UnInstallExtension;
            extensionListContro_all.UpdateExtension += extensionListControl_UpdateExtension;
            extensionListContro_all.ConfigureExtension += extensionListControl_ConfigureExtension;
            extensionListContro_all.InstallExtension += extensionListControl_InstallExtension;
        }

        void extensionListControl_InstallExtension(object sender, PackageClass packageClass)
        {
            string newPackageLoacation = GetPackageLocation(packageClass);
            if (!File.Exists(newPackageLoacation))
            {
                MessageBox.Show("Can't locate the installer package. Install aborted");
                return;
            }
            PackageClass pak = new PackageClass();
            pak = pak.ZipProvider.Load(newPackageLoacation);
            if (!pak.CheckDependency(false))
            {
                MessageBox.Show("Dependency check error ! Install aborted!");
                return;
            }

            if (MessageBox.Show("This operation will install extension " + packageClass.GeneralInfo.Name + " version " + pak.GeneralInfo.Version + " \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                return;
            this.Hide();
            packageClass = MpeCore.MpeInstaller.InstalledExtensions.Get(packageClass.GeneralInfo.Id);
            if (packageClass != null)
            {
                UnInstall dlg = new UnInstall();
                dlg.Execute(packageClass, true);
                pak.CopyGroupCheck(packageClass);
            }
            pak.StartInstallWizard();
            RefreshLists();
            this.Show();
        }

        void extensionListControl_ConfigureExtension(object sender, PackageClass packageClass)
        {
            string conf_str = packageClass.GeneralInfo.Params[ParamNamesConst.CONFIG].GetValueAsPath();
            if (string.IsNullOrEmpty(conf_str))
                return;
            try
            {
                if (Path.GetExtension(conf_str).ToUpper() == ".DLL")
                {
                    string assemblyFileName = conf_str;
                    AppDomainSetup setup = new AppDomainSetup();
                    setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                    setup.PrivateBinPath = Path.GetDirectoryName(assemblyFileName); 
                    setup.ApplicationName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                    setup.ShadowCopyFiles = "true";
                    setup.ShadowCopyDirectories = Path.GetDirectoryName(assemblyFileName);
                    AppDomain appDomain = AppDomain.CreateDomain("pluginDomain", null, setup);

                    PluginLoader remoteExecutor = (PluginLoader)appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(PluginLoader).ToString());
                    remoteExecutor.Load(conf_str);
                    remoteExecutor.Dispose();
                    
                    AppDomain.Unload(appDomain);
                }
                else
                {
                    Process.Start(conf_str);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private string GetPackageLocation(PackageClass packageClass)
        {
            string newPackageLoacation = packageClass.GeneralInfo.Location;
            if (!File.Exists(newPackageLoacation))
            {
                newPackageLoacation = packageClass.LocationFolder + packageClass.GeneralInfo.Id + ".mpe2";
                if (!File.Exists(newPackageLoacation))
                {
                    if (!string.IsNullOrEmpty(packageClass.GeneralInfo.OnlineLocation))
                    {
                        newPackageLoacation = Path.GetTempFileName();
                        new DownloadFile(packageClass.GeneralInfo.OnlineLocation, newPackageLoacation);
                    }
                }
            }
            return newPackageLoacation;
        }

        void extensionListControl_UpdateExtension(object sender, PackageClass packageClass, PackageClass newpackageClass)
        {
            string newPackageLoacation = GetPackageLocation(newpackageClass);
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
            RefreshLists();
            this.Show();

        }

        void RefreshLists()
        {
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
            extensionListContro_all.Set(MpeCore.MpeInstaller.KnownExtensions.GetUniqueList());
        }

        void extensionListControl_UnInstallExtension(object sender, PackageClass packageClass)
        {
            UnInstall dlg = new UnInstall();
            dlg.Execute(packageClass, false);
            RefreshLists();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MpeCore.MpeInstaller.Init();
            RefreshLists();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "Mpe package file(*.mpe1)|*.mpe1|All files|*.*"
                                        };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                InstallFile(dialog.FileName, false);
            }
        }

        private bool IsOldFormat(string zipfile)
        {
            try
            {
                ZipFile _zipPackageFile = ZipFile.Read(zipfile);
                if (_zipPackageFile.EntryFileNames.Contains("instaler.xmp"))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void InstallFile(string file, bool silent)
        {
            if (IsOldFormat(file))
            {
                if (!silent)
                    MessageBox.Show("This is a old format file. MpiInstaller will be used to install it! ");
                Process.Start("MpInstaller.exe", file);
                return;
            }
            MpeCore.MpeInstaller.Init();
            PackageClass pak = new PackageClass();
            pak = pak.ZipProvider.Load(file);
            if (pak == null)
            {
                if (!silent)
                    MessageBox.Show("Wrong file format !");
                return;
            }
            PackageClass installedPak = MpeCore.MpeInstaller.InstalledExtensions.Get(pak.GeneralInfo.Id);
            if (pak.CheckDependency(false))
            {
                if (installedPak != null)
                {
                    if (!silent)
                        if (MessageBox.Show("This extension already have a installed version. \n This will be uninstalled first. \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                            return;
                    UnInstall dlg = new UnInstall();
                    dlg.Execute(installedPak, true);
                    pak.CopyGroupCheck(installedPak);
                }
                this.Hide();
                pak.Silent = silent;
                pak.StartInstallWizard();
                RefreshLists();
                this.Show();
            }
            else
            {
                if (!silent)
                    MessageBox.Show("Installation aborted, some of the dependency not found !");
            }            
        }

        private void btn_online_update_Click(object sender, EventArgs e)
        {
            List<string> onlineFiles = MpeCore.MpeInstaller.InstalledExtensions.GetUpdateUrls(new List<string>());
            onlineFiles = MpeCore.MpeInstaller.KnownExtensions.GetUpdateUrls(onlineFiles);
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
            RefreshLists();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            string file = string.Format("{0}\\V2\\KnownExtensions.xml", Config.GetFolder(Config.Dir.Installer));
            DateTime d = File.GetLastWriteTime(file);
            int i = File.GetLastWriteTime(file).Subtract(DateTime.Now).Days;
            if (File.Exists(file))
            {
                if (File.GetLastWriteTime(file).Subtract(DateTime.Now).Days > 7 &&
                    MpeCore.MpeInstaller.InstalledExtensions.Items.Count > 0 &&
                    MessageBox.Show("Do you want to update the extension list ?", "Update", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    btn_online_update_Click(sender, e);
                }
            }
        }



        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.All;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
                InstallFile(files[0], false);
        }
    }
}
