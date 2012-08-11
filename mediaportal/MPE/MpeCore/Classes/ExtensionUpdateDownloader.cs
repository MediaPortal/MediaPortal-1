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
using System.Linq;
using System.Text;
using MpeCore.Dialogs;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;

namespace MpeCore.Classes
{
  public static class ExtensionUpdateDownloader
  {
    public const string UpdateIndexUrl = "http://install.team-mediaportal.com/MPEI/extensions.txt";

    static string tempUpdateIndex;

    public static void UpdateList(bool silent, bool installedOnly, DownloadProgressChangedEventHandler downloadProgressChanged, AsyncCompletedEventHandler downloadFileCompleted)
    {
      if (!installedOnly)
      {
        DownloadExtensionIndex(downloadProgressChanged, downloadFileCompleted);
      }
      DownloadInfo dlg = new DownloadInfo();
      dlg.silent = silent;
      dlg.installedOnly = installedOnly;
      if (dlg.ShowDialog() == DialogResult.OK && !installedOnly)
      {
        ApplicationSettings.Instance.LastUpdate = DateTime.Now;
        ApplicationSettings.Instance.Save();
      }
    }

    static void DownloadExtensionIndex(DownloadProgressChangedEventHandler downloadProgressChanged, AsyncCompletedEventHandler downloadFileCompleted)
    {
      DownloadFile dlg = null;
      try
      {
        tempUpdateIndex = Path.GetTempFileName();
        dlg = new DownloadFile();
        if (downloadProgressChanged != null) dlg.Client.DownloadProgressChanged += downloadProgressChanged;
        if (downloadFileCompleted != null) dlg.Client.DownloadFileCompleted += downloadFileCompleted;
        dlg.Client.DownloadFileCompleted += UpdateIndex_DownloadFileCompleted;
        dlg.StartDownload(UpdateIndexUrl, tempUpdateIndex);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        if (downloadProgressChanged != null) dlg.Client.DownloadProgressChanged -= downloadProgressChanged;
        if (downloadFileCompleted != null) dlg.Client.DownloadFileCompleted -=downloadFileCompleted;
        dlg.Client.DownloadFileCompleted -= UpdateIndex_DownloadFileCompleted;
        File.Delete(tempUpdateIndex);
      }
    }

    static void UpdateIndex_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (!File.Exists(tempUpdateIndex)) return;

      var indexUrls = new List<string>();
      string[] lines = File.ReadAllLines(tempUpdateIndex);
      foreach (string line in lines)
      {
        if (string.IsNullOrEmpty(line)) continue;
        if (line.StartsWith("#")) continue;

        indexUrls.Add(line.Split(';')[0]);
      }
      
      MpeCore.MpeInstaller.SetInitialUrlIndex(indexUrls);
    }

    public static string GetPackageLocation(PackageClass packageClass, DownloadProgressChangedEventHandler downloadProgressChanged, AsyncCompletedEventHandler downloadFileCompleted)
    {
      string newPackageLoacation = packageClass.GeneralInfo.Location;
      if (!File.Exists(newPackageLoacation))
      {
        newPackageLoacation = packageClass.LocationFolder + packageClass.GeneralInfo.Id + ".mpe2";
        if (!File.Exists(newPackageLoacation))
        {
          if (!string.IsNullOrEmpty(packageClass.GeneralInfo.OnlineLocation))
          {
            DownloadFile dlg = null;
            try
            {
              newPackageLoacation = Path.GetTempFileName();
              dlg = new DownloadFile();
              if (downloadProgressChanged != null) dlg.Client.DownloadProgressChanged += downloadProgressChanged;
              if (downloadFileCompleted != null) dlg.Client.DownloadFileCompleted += downloadFileCompleted;
              dlg.StartDownload(packageClass.GeneralInfo.OnlineLocation, newPackageLoacation);
            }
            catch (Exception ex)
            {
              MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
              if (downloadProgressChanged != null) dlg.Client.DownloadProgressChanged -= downloadProgressChanged;
              if (downloadFileCompleted != null) dlg.Client.DownloadFileCompleted -= downloadFileCompleted;
            }
          }
        }
      }
      return newPackageLoacation;
    }
  }
}
