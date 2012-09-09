#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using MpeCore;
using MpeCore.Classes;
using MpeInstaller.Dialogs;
using MpeCore.Dialogs;

namespace MpeInstaller
{
  public partial class MainForm : Form
  {
    private SplashScreen splashScreen = new SplashScreen();
    private bool _loading = true;
    private ScreenShotNavigator _screenShotNavigator = new ScreenShotNavigator();

    public MainForm()
    {
      Init();
    }

    public MainForm(ProgramArguments args) : this()
    {
      try
      {
        if (File.Exists(args.PackageFile))
        {
          ApplicationSettings.Instance.DoUpdateInStartUp = false;
          InstallFile(args.PackageFile, args.Silent, false);
        }
        else if (args.Update)
        {
          ApplicationSettings.Instance.DoUpdateInStartUp = false;
          RefreshListControls();
          DoUpdateAll(false);
        }
        else if (args.MpQueue)
        {
          ApplicationSettings.Instance.DoUpdateInStartUp = false;
          if (args.Splash)
          {
            splashScreen.SetImg(args.BackGround);
            splashScreen.Show();
            splashScreen.Update();
          }
          ExecuteMpQueue();
          if (splashScreen.Visible)
            splashScreen.Close();
        }
        else if (args.UninstallPackage)
        {
          if (string.IsNullOrEmpty(args.PackageID)) return;
          PackageClass pc = MpeCore.MpeInstaller.InstalledExtensions.Get(args.PackageID);
          if (pc == null) return;

          UnInstall dlg = new UnInstall();
          dlg.Execute(pc, args.Silent);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
      finally
      {
        this.Close();
      }
    }

    public void ExecuteMpQueue()
    {
      ExecuteQueue();
      string mpExe = Path.Combine(Application.StartupPath, "MediaPortal.exe");
      if (File.Exists(mpExe))
      {
        Process.Start(mpExe);
        Thread.Sleep(3000);
      }
    }

    public void ExecuteQueue()
    {
      //UpdateList(true);
      QueueCommandCollection collection = QueueCommandCollection.Load();
      foreach (QueueCommand item in collection.Items)
      {
        switch (item.CommandEnum)
        {
          case CommandEnum.Install:
            {
              PackageClass packageClass = MpeCore.MpeInstaller.KnownExtensions.Get(item.TargetId,
                                                                                   item.TargetVersion.
                                                                                     ToString());
              if (packageClass == null)
                continue;
              splashScreen.SetInfo("Installing " + packageClass.GeneralInfo.Name);
              string newPackageLoacation = ExtensionUpdateDownloader.GetPackageLocation(packageClass, Client_DownloadProgressChanged, Client_DownloadFileCompleted);
              InstallFile(newPackageLoacation, true, false);
            }
            break;
          case CommandEnum.Uninstall:
            {
              PackageClass packageClass = MpeCore.MpeInstaller.InstalledExtensions.Get(item.TargetId);
              if (packageClass == null)
                continue;
              splashScreen.SetInfo("UnInstalling " + packageClass.GeneralInfo.Name);
              UnInstall dlg = new UnInstall();
              dlg.Execute(packageClass, true);
            }
            break;
          default:
            break;
        }
      }
      collection.Items.Clear();
      collection.Save();
    }

    private void SetFilterForKnownExtensionsList()
    {
      MpeCore.MpeInstaller.KnownExtensions.Hide(ApplicationSettings.Instance.ShowOnlyStable, ApplicationSettings.Instance.ShowOnlyCompatible);
      if (MpeCore.MpeInstaller.KnownExtensions.GetHiddenExtensionCount() > 0)
      {
        toolStripLabelWarn.Visible = true;
        string infoText = "";
        if (ApplicationSettings.Instance.ShowOnlyStable) infoText += "unstable ";
        if (ApplicationSettings.Instance.ShowOnlyCompatible) infoText += (ApplicationSettings.Instance.ShowOnlyStable ? "and " : "") + "incompatible ";
        infoText += "extensions are hidden";
        toolStripLabelWarn.Text = infoText;
      }
      else
        toolStripLabelWarn.Visible = false;
    }

    public void Init()
    {
      InitializeComponent();
      MpeCore.MpeInstaller.Init();
      MpeCore.MpeInstaller.InstalledExtensions.IgnoredUpdates = ApplicationSettings.Instance.IgnoredUpdates;
      MpeCore.MpeInstaller.KnownExtensions.IgnoredUpdates = ApplicationSettings.Instance.IgnoredUpdates;
      _loading = true;
      onlyStableToolStripMenuItem.Checked = ApplicationSettings.Instance.ShowOnlyStable;
      onlyCompatibleToolStripMenuItem.Checked = ApplicationSettings.Instance.ShowOnlyCompatible;
      _loading = false;
      extensionListControlInstalled.UnInstallExtension += extensionListControl_UnInstallExtension;
      extensionListControlInstalled.UpdateExtension += extensionListControl_UpdateExtension;
      extensionListControlInstalled.ConfigureExtension += extensionListControl_ConfigureExtension;
      extensionListControlInstalled.InstallExtension += extensionListControl_InstallExtension;
      extensionListControlInstalled.ShowScreenShot += extensionListControl_ShowScreenShot;
      extensionListControlKnown.UnInstallExtension += extensionListControl_UnInstallExtension;
      extensionListControlKnown.UpdateExtension += extensionListControl_UpdateExtension;
      extensionListControlKnown.ConfigureExtension += extensionListControl_ConfigureExtension;
      extensionListControlKnown.InstallExtension += extensionListControl_InstallExtension;
      extensionListControlKnown.ShowScreenShot += extensionListControl_ShowScreenShot;
    }

    private void extensionListControl_ShowScreenShot(object sender, PackageClass packageClass)
    {
      _screenShotNavigator.Set(packageClass);
      if (!_screenShotNavigator.Visible)
        _screenShotNavigator.Show();
    }

    private void extensionListControl_InstallExtension(object sender, PackageClass packageClass)
    {
      string newPackageLoacation = ExtensionUpdateDownloader.GetPackageLocation(packageClass, Client_DownloadProgressChanged, Client_DownloadFileCompleted);
      if (!File.Exists(newPackageLoacation))
      {
        MessageBox.Show("Can't locate the installer package. Install aborted");
        return;
      }
      PackageClass pak = new PackageClass();
      pak = pak.ZipProvider.Load(newPackageLoacation);
      if (pak == null)
      {
        MessageBox.Show("Package loading error ! Install aborted!");
        try
        {
          if (newPackageLoacation != packageClass.GeneralInfo.Location)
            File.Delete(newPackageLoacation);
        }
        catch { }
        return;
      }
      if (!pak.CheckDependency(false))
      {
        if (MessageBox.Show("Dependency check error! Install aborted!\nWould you like to view more details?", pak.GeneralInfo.Name,
          MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
        {
          DependencyForm frm = new DependencyForm(pak);
          frm.ShowDialog();
        }
        pak.ZipProvider.Dispose();
        try
        {
          if (newPackageLoacation != packageClass.GeneralInfo.Location)
            File.Delete(newPackageLoacation);
        }
        catch { }
        return;
      }

      if (packageClass.GeneralInfo.Version.CompareTo(pak.GeneralInfo.Version) != 0)
      {
        if (MessageBox.Show(
          string.Format(@"Downloaded version of {0} is {1} and differs from your selected version: {2}!
Do you want to continue ?",packageClass.GeneralInfo.Name, pak.GeneralInfo.Version,packageClass.GeneralInfo.Version), "Install extension", MessageBoxButtons.YesNo,
          MessageBoxIcon.Error) != DialogResult.Yes)
        return;
      }

      if (
        MessageBox.Show(
          "This operation will install " + packageClass.GeneralInfo.Name + " version " +
          pak.GeneralInfo.Version + "\n Do you want to continue ?", "Install extension", MessageBoxButtons.YesNo,
          MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      this.Hide();
      packageClass = MpeCore.MpeInstaller.InstalledExtensions.Get(packageClass.GeneralInfo.Id);
      if (packageClass != null)
      {
        if (pak.GeneralInfo.Params[ParamNamesConst.FORCE_TO_UNINSTALL_ON_UPDATE].GetValueAsBool())
        {
          if (
            MessageBox.Show(
              "Another version of this extension is installed\nand needs to be uninstalled first.\nDo you want to continue?\n" +
              "Old extension version: " + packageClass.GeneralInfo.Version + "\n" +
              "New extension version: " + pak.GeneralInfo.Version,
              "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
          {
            this.Show();
            return;
          }
          UnInstall dlg = new UnInstall();
          dlg.Execute(packageClass, false);
        }
        else
        {
          MpeCore.MpeInstaller.InstalledExtensions.Remove(packageClass);
        }
        pak.CopyGroupCheck(packageClass);
      }
      pak.StartInstallWizard();
      RefreshListControls();
      pak.ZipProvider.Dispose();
      try
      {
        if (newPackageLoacation != packageClass.GeneralInfo.Location)
          File.Delete(newPackageLoacation);
      }
      catch { }
      this.Show();
    }

    private void extensionListControl_ConfigureExtension(object sender, PackageClass packageClass)
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

          PluginLoader remoteExecutor =
            (PluginLoader)
            appDomain.CreateInstanceFromAndUnwrap(
              Assembly.GetAssembly(typeof (MpeCore.MpeInstaller)).Location,
              typeof (PluginLoader).ToString());
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

    private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (splashScreen.Visible)
      {
        splashScreen.ResetProgress();
      }
    }

    private void Client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
    {
      if (splashScreen.Visible)
      {
        splashScreen.SetProgress("Downloading", e.ProgressPercentage);
      }
    }

    private void extensionListControl_UpdateExtension(object sender, PackageClass packageClass,
                                                      PackageClass newpackageClass)
    {
      this.Hide();
      if (DoUpdate(packageClass, newpackageClass, false))
      {
        RefreshListControls();
      }
      this.Show();
    }

    private void DoUpdateAll(bool askForInfoUpdate)
    {
      var updatelist = new Dictionary<PackageClass, PackageClass>();
      foreach (PackageClass packageClass in MpeCore.MpeInstaller.InstalledExtensions.Items)
      {
        PackageClass update = MpeCore.MpeInstaller.KnownExtensions.GetUpdate(packageClass);
        if (update == null)
          continue;
        updatelist.Add(packageClass, update);
      }
      if (updatelist.Count > 0)
      {
        this.Hide();
        foreach (KeyValuePair<PackageClass, PackageClass> valuePair in updatelist)
        {
          if (valuePair.Value == null)
            continue;
          DoUpdate(valuePair.Key, valuePair.Value, true);
        }
        RefreshListControls();
        this.Show();
      }
      else
      {
        if (askForInfoUpdate && MessageBox.Show("All installed extensions seem up to date.\nRefresh update info and try again?", "No updates found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
          ExtensionUpdateDownloader.UpdateList(false, true, null, null);
          SetFilterForKnownExtensionsList();
          RefreshListControls();
          DoUpdateAll(false);
        }
      }
    }

    private bool DoUpdate(PackageClass packageClass, PackageClass newpackageClass, bool silent)
    {
      string newPackageLoacation = ExtensionUpdateDownloader.GetPackageLocation(newpackageClass, Client_DownloadProgressChanged, Client_DownloadFileCompleted);
      if (!File.Exists(newPackageLoacation))
      {
        if (!silent)
          MessageBox.Show("Can't locate the installer package. Update aborted");
        return false;
      }
      PackageClass pak = new PackageClass();
      pak = pak.ZipProvider.Load(newPackageLoacation);
      if (pak == null)
      {
        if (!silent)
          MessageBox.Show("Invalid package format ! Update aborted !");
        return false;
      }
      if (pak.GeneralInfo.Id != newpackageClass.GeneralInfo.Id ||
          pak.GeneralInfo.Version.CompareTo(newpackageClass.GeneralInfo.Version) < 0)
      {
        if (!silent)
          MessageBox.Show("Invalid update information ! Update aborted!");
        return false;
      }
      if (!pak.CheckDependency(false))
      {
        if (!silent)
        {
          if (MessageBox.Show("Dependency check error! Update aborted!\nWould you like to view more details?", pak.GeneralInfo.Name,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
          {
            DependencyForm frm = new DependencyForm(pak);
            frm.ShowDialog();
          }
        }
        return false;
      }
      if (!silent)
        if (
          MessageBox.Show(
            "This operation will update the extension " + packageClass.GeneralInfo.Name + " to the version " +
            pak.GeneralInfo.Version + " \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo,
            MessageBoxIcon.Exclamation) != DialogResult.Yes)
          return false;
      // only uninstall previous version, if the new package has the setting to force uninstall of previous version on update
      if (pak.GeneralInfo.Params[ParamNamesConst.FORCE_TO_UNINSTALL_ON_UPDATE].GetValueAsBool())
      {
        UnInstall dlg = new UnInstall();
        dlg.Execute(packageClass, true);
      }
      else
      {
        MpeCore.MpeInstaller.InstalledExtensions.Remove(packageClass);
      }
      pak.CopyGroupCheck(packageClass);
      pak.Silent = true;
      pak.StartInstallWizard();
      return true;
    }

    private void RefreshListControls()
    {
      toolStripLastUpdate.Text = "Last update: " + (ApplicationSettings.Instance.LastUpdate == DateTime.MinValue ? "Never" : ApplicationSettings.Instance.LastUpdate.ToString("g"));
      extensionListControlInstalled.Set(MpeCore.MpeInstaller.InstalledExtensions, true);
      extensionListControlKnown.Set(MpeCore.MpeInstaller.KnownExtensions.GetUniqueList(MpeCore.MpeInstaller.InstalledExtensions), false);
    }

    private void extensionListControl_UnInstallExtension(object sender, PackageClass packageClass)
    {
      UnInstall dlg = new UnInstall();
      if (dlg.Execute(packageClass, false))
      {
        RefreshListControls();
      }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      // The default connection limit is 2 in .Net on most platforms! This means downloading two files will block all other WebRequests.
      System.Net.ServicePointManager.DefaultConnectionLimit = 100;

      // the first time a WebClient is used to download data, it will do a proxy discovery which can be slow (10 seconds)
      // do this on startup in a background thread to have the discovery ready when before the UI might block
      new Thread(() => 
      { 
        System.Net.WebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.google.com")); 
      }) 
      { 
        IsBackground = true, 
        Name = "ProxyPrecacheDiscovery" 
      }.Start();

      SetFilterForKnownExtensionsList();
      RefreshListControls();
    }

    private void FileOpen_Click(object sender, EventArgs e)
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
        using (ZipFile zipPackageFile = ZipFile.Read(zipfile))
        {
          if (zipPackageFile.EntryFileNames.Contains("instaler.xmp"))
            return true;
        }
        return false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public void InstallFile(string file, bool silent)
    {
      InstallFile(file, silent, true);
    }

    public void InstallFile(string file, bool silent, bool gui)
    {
      if (!File.Exists(file))
      {
        if (!silent)
          MessageBox.Show("File not found !");
        return;
      }

      if (IsOldFormat(file))
      {
        if (!silent)
          MessageBox.Show("This is an old format file (mpi).  The extension will NOT be installed. ");
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
      if (pak.CheckDependency(false))
      {
        PackageClass installedPak = MpeCore.MpeInstaller.InstalledExtensions.Get(pak.GeneralInfo.Id);
        if (installedPak != null)
        {
          if (pak.GeneralInfo.Params[ParamNamesConst.FORCE_TO_UNINSTALL_ON_UPDATE].GetValueAsBool())
          {
            if (!silent)
              if (
                MessageBox.Show(
                  "Another version of this extension is installed\nand needs to be uninstalled first.\nDo you want to continue?\n" +
                  "Old extension version: " + installedPak.GeneralInfo.Version + "\n" +
                  "New extension version: " + pak.GeneralInfo.Version,
                  "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                return;
            UnInstall dlg = new UnInstall();
            dlg.Execute(installedPak, true);
            pak.CopyGroupCheck(installedPak);
          }
          else
          {
            MpeCore.MpeInstaller.InstalledExtensions.Remove(pak);
          }
        }
        if (gui)
          this.Hide();
        pak.Silent = silent;
        pak.StartInstallWizard();
        if (gui)
        {
          SetFilterForKnownExtensionsList();
          RefreshListControls();
          this.Show();
        }
      }
      else
      {
        if (!silent)
        {
          if (MessageBox.Show("Dependency check error! Install aborted!\nWould you like to view more details?", pak.GeneralInfo.Name, 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
          {
            DependencyForm frm = new DependencyForm(pak);
            frm.ShowDialog();
          }
        }
      }
    }

    private void RefreshUpdateInfo_Click(object sender, EventArgs e)
    {
      ExtensionUpdateDownloader.UpdateList(false, false, Client_DownloadProgressChanged, Client_DownloadFileCompleted);
      SetFilterForKnownExtensionsList();
      RefreshListControls();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
      DateTime d = ApplicationSettings.Instance.LastUpdate;
      int i = DateTime.Now.Subtract(d).Days;
      if (((ApplicationSettings.Instance.DoUpdateInStartUp && i > ApplicationSettings.Instance.UpdateDays) ||
           MpeCore.MpeInstaller.KnownExtensions.Items.Count == 0) &&
          MessageBox.Show("Do you want to update the extension list ?", "Update", MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question) == DialogResult.Yes)
      {
        RefreshUpdateInfo_Click(sender, e);
        if (ApplicationSettings.Instance.UpdateAll)
          DoUpdateAll(false);
      }
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        e.Effect = DragDropEffects.All;
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files.Length > 0)
        InstallFile(files[0], false);
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      ApplicationSettings.Instance.Save();
    }

    private void UpdateAll_Click(object sender, EventArgs e)
    {
      DoUpdateAll(true);
    }

    private void chk_stable_CheckedChanged(object sender, EventArgs e)
    {
      if (!_loading)
      {
        ApplicationSettings.Instance.ShowOnlyStable = onlyStableToolStripMenuItem.Checked;
        SetFilterForKnownExtensionsList();
        RefreshListControls();
      }
    }

    private void CleanCache_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("Do you want to clear all unused data?\nYou need to redownload update info.", "Cleanup",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question) == DialogResult.Yes)
      {
        ExtensionCollection collection = new ExtensionCollection();
        foreach (PackageClass item in MpeCore.MpeInstaller.KnownExtensions.Items)
        {
          if (MpeCore.MpeInstaller.InstalledExtensions.Get(item) == null)
          {
            collection.Items.Add(item);
          }
        }
        foreach (PackageClass packageClass in collection.Items)
        {
          try
          {
            if (Directory.Exists(packageClass.LocationFolder))
              Directory.Delete(packageClass.LocationFolder, true);
            string baseExtensionPath = Path.GetDirectoryName(packageClass.LocationFolder.Trim('\\'));
            if (Directory.Exists(baseExtensionPath) && Directory.GetFileSystemEntries(baseExtensionPath).Length == 0)
            {
              Directory.Delete(baseExtensionPath);
            }
            MpeCore.MpeInstaller.KnownExtensions.Remove(packageClass);
          }
          catch (Exception ex)
          {
            MessageBox.Show("Error : " + ex.Message);
          }
        }
        ApplicationSettings.Instance.LastUpdate = DateTime.MinValue;
        MpeCore.MpeInstaller.Save();
        SetFilterForKnownExtensionsList();
        RefreshListControls();
      }
    }

    private void chk_dependency_CheckedChanged(object sender, EventArgs e)
    {
      if (!_loading)
      {
        ApplicationSettings.Instance.ShowOnlyCompatible = onlyCompatibleToolStripMenuItem.Checked;
        SetFilterForKnownExtensionsList();
        RefreshListControls();
      }
    }

    private void wikiToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        Process.Start("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/17_Extensions/1_Installer_(MPEI)");
      }
      catch (Exception) { }
    }

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SettingsForm dlg = new SettingsForm();
      dlg.chk_update.Checked = ApplicationSettings.Instance.DoUpdateInStartUp;
      dlg.chk_updateExtension.Checked = ApplicationSettings.Instance.UpdateAll;
      dlg.numeric_Days.Value = ApplicationSettings.Instance.UpdateDays;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        ApplicationSettings.Instance.DoUpdateInStartUp = dlg.chk_update.Checked;
        ApplicationSettings.Instance.UpdateAll = dlg.chk_updateExtension.Checked;
        ApplicationSettings.Instance.UpdateDays = (int)dlg.numeric_Days.Value;
      }
    }

  }
}