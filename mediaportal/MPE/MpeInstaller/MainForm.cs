#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
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
    private ApplicationSettings _settings = new ApplicationSettings();
    private SplashScreen splashScreen = new SplashScreen();
    private bool _loading = true;

    public MainForm()
    {
      Init();
    }

    public MainForm(ProgramArguments args)
    {
      Init();
      try
      {
        if (File.Exists(args.PackageFile))
        {
          _settings.DoUpdateInStartUp = false;
          InstallFile(args.PackageFile, args.Silent, false);
          this.Close();
          return;
        }
        if (args.Update)
        {
          _settings.DoUpdateInStartUp = false;
          RefreshLists();
          DoUpdateAll();
          this.Close();
          return;
        }
        if (args.MpQueue)
        {
          _settings.DoUpdateInStartUp = false;
          if (args.Splash)
          {
            splashScreen.SetImg(args.BackGround);
            splashScreen.Show();
            splashScreen.Update();
          }
          ExecuteMpQueue();
          this.Close();
          if (splashScreen.Visible)
            splashScreen.Close();
          return;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        MessageBox.Show(ex.StackTrace);
      }
    }

    public void ExecuteMpQueue()
    {
      Process[] prs = Process.GetProcesses();
      foreach (Process pr in prs)
      {
        if (pr.ProcessName.Equals("MediaPortal", StringComparison.InvariantCultureIgnoreCase))
        {
          pr.CloseMainWindow();
          pr.Close();
          Thread.Sleep(500);
        }
      }
      prs = Process.GetProcesses();
      foreach (Process pr in prs)
      {
        if (pr.ProcessName.Equals("MediaPortal", StringComparison.InvariantCultureIgnoreCase))
        {
          try
          {
            Thread.Sleep(5000);
            pr.Kill();
          }
          catch (Exception) {}
        }
      }
      ExecuteQueue();
      Process.Start(Config.GetFile(Config.Dir.Base, "MediaPortal.exe"));
      Thread.Sleep(3000);
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
              string newPackageLoacation = GetPackageLocation(packageClass);
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

    public void Init()
    {
      InitializeComponent();
      MpeCore.MpeInstaller.Init();
      _settings = ApplicationSettings.Load();
      _loading = true;
      chk_update.Checked = _settings.DoUpdateInStartUp;
      chk_updateExtension.Checked = _settings.UpdateAll;
      numeric_Days.Value = _settings.UpdateDays;
      chk_update_CheckedChanged(null, null);
      _loading = false;
      extensionListControl.UnInstallExtension += extensionListControl_UnInstallExtension;
      extensionListControl.UpdateExtension += extensionListControl_UpdateExtension;
      extensionListControl.ConfigureExtension += extensionListControl_ConfigureExtension;
      extensionListControl.InstallExtension += extensionListControl_InstallExtension;
      extensionListContro_all.UnInstallExtension += extensionListControl_UnInstallExtension;
      extensionListContro_all.UpdateExtension += extensionListControl_UpdateExtension;
      extensionListContro_all.ConfigureExtension += extensionListControl_ConfigureExtension;
      extensionListContro_all.InstallExtension += extensionListControl_InstallExtension;
    }

    private void extensionListControl_InstallExtension(object sender, PackageClass packageClass)
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

      if (
        MessageBox.Show(
          "This operation will install extension " + packageClass.GeneralInfo.Name + " version " +
          pak.GeneralInfo.Version + " \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo,
          MessageBoxIcon.Exclamation) != DialogResult.Yes)
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
            DownloadFile dlg = new DownloadFile();
            dlg.Client.DownloadProgressChanged += Client_DownloadProgressChanged;
            dlg.Client.DownloadFileCompleted += Client_DownloadFileCompleted;
            dlg.StartDownload(packageClass.GeneralInfo.OnlineLocation, newPackageLoacation);
          }
        }
      }
      return newPackageLoacation;
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
      DoUpdate(packageClass, newpackageClass, false);
      RefreshLists();
      this.Show();
    }

    private void DoUpdateAll()
    {
      this.Hide();
      var updatelist = new Dictionary<PackageClass, PackageClass>();
      foreach (PackageClass packageClass in MpeCore.MpeInstaller.InstalledExtensions.Items)
      {
        PackageClass update = MpeCore.MpeInstaller.KnownExtensions.GetUpdate(packageClass);
        if (update == null)
          continue;
        updatelist.Add(packageClass, update);
      }
      foreach (KeyValuePair<PackageClass, PackageClass> valuePair in updatelist)
      {
        if (valuePair.Value == null)
          continue;
        DoUpdate(valuePair.Key, valuePair.Value, true);
      }
      RefreshLists();
      this.Show();
    }

    private bool DoUpdate(PackageClass packageClass, PackageClass newpackageClass, bool silent)
    {
      string newPackageLoacation = GetPackageLocation(newpackageClass);
      if (!File.Exists(newPackageLoacation))
      {
        if (!silent)
          MessageBox.Show("Can't locate the installer package. Update aborted");
        return false;
      }
      PackageClass pak = new PackageClass();
      pak = pak.ZipProvider.Load(newPackageLoacation);
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
          MessageBox.Show("Dependency check error ! Update aborted!");
        return false;
      }
      if (!silent)
        if (
          MessageBox.Show(
            "This operation update extension " + packageClass.GeneralInfo.Name + " to the version " +
            pak.GeneralInfo.Version + " \n Do you want to continue ? ", "Install extension", MessageBoxButtons.YesNo,
            MessageBoxIcon.Exclamation) != DialogResult.Yes)
          return false;
      UnInstall dlg = new UnInstall();
      dlg.Execute(packageClass, true);
      pak.CopyGroupCheck(packageClass);
      pak.Silent = true;
      pak.StartInstallWizard();
      return true;
    }

    private void RefreshLists()
    {
      extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
      extensionListContro_all.Set(MpeCore.MpeInstaller.KnownExtensions.GetUniqueList());
    }

    private void extensionListControl_UnInstallExtension(object sender, PackageClass packageClass)
    {
      UnInstall dlg = new UnInstall();
      dlg.Execute(packageClass, false);
      RefreshLists();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
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
          MessageBox.Show("This is a old format file. MpiInstaller will be used to install it! ");
        Process.Start(MpeCore.MpeInstaller.TransformInRealPath("%Base%")+@"\MpInstaller.exe", file);
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
            if (
              MessageBox.Show(
                "This extension already have a installed version. \n This will be uninstalled first. \n Do you want to continue ?  \n"+
                 "Old extension version: "+ installedPak.GeneralInfo.Version.ToString()+" \n" +
                 "New extension version: "+ pak.GeneralInfo.Version.ToString(),
                "Install extension", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
              return;
          UnInstall dlg = new UnInstall();
          dlg.Execute(installedPak, true);
          pak.CopyGroupCheck(installedPak);
        }
        if (gui)
          this.Hide();
        pak.Silent = silent;
        pak.StartInstallWizard();
        if (gui)
        {
          RefreshLists();
          this.Show();
        }
      }
      else
      {
        if (!silent)
          MessageBox.Show("Installation aborted, some of the dependency not found !");
      }
    }

    private void btn_online_update_Click(object sender, EventArgs e)
    {
      UpdateList(false);
      RefreshLists();
    }

    private void UpdateList(bool silent)
    {
      DownloadInfo dlg = new DownloadInfo();
      dlg.silent = silent;
      dlg.ShowDialog();
      _settings.LastUpdate = DateTime.Now;
      _settings.Save();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
      DateTime d = _settings.LastUpdate;
      int i = DateTime.Now.Subtract(d).Days;
      if (_settings.DoUpdateInStartUp && i > _settings.UpdateDays &&
          MpeCore.MpeInstaller.InstalledExtensions.Items.Count > 0 &&
          MessageBox.Show("Do you want to update the extension list ?", "Update", MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question) == DialogResult.Yes)
      {
        btn_online_update_Click(sender, e);
        if (_settings.UpdateAll)
          DoUpdateAll();
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
      _settings.Save();
    }

    private void chk_update_CheckedChanged(object sender, EventArgs e)
    {
      if (!_loading)
      {
        _settings.DoUpdateInStartUp = chk_update.Checked;
        _settings.UpdateAll = chk_updateExtension.Checked;
        _settings.UpdateDays = (int)numeric_Days.Value;
      }
      if (!_settings.DoUpdateInStartUp)
      {
        numeric_Days.Enabled = false;
        chk_updateExtension.Enabled = false;
      }
      else
      {
        numeric_Days.Enabled = true;
        chk_updateExtension.Enabled = true;
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      DoUpdateAll();
    }
  }
}