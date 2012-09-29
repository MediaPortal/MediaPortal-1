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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeCore.Dialogs
{
  public partial class DownloadInfo : Form
  {
    bool cancel = false;
    public bool silent = false;
    public bool installedOnly = false;
    private int counter = -1;
    private List<string> onlineFiles = new List<string>();
    int runningThreads = 0;

    public DownloadInfo()
    {
      InitializeComponent();
    }

    private void DownloadInfo_Shown(object sender, EventArgs e)
    {
      onlineFiles = MpeCore.MpeInstaller.InstalledExtensions.GetUpdateUrls(new List<string>());
      if (!installedOnly)
      {
        onlineFiles = MpeCore.MpeInstaller.KnownExtensions.GetUpdateUrls(onlineFiles);
        onlineFiles = MpeCore.MpeInstaller.GetInitialUrlIndex(onlineFiles);
      }

      if (onlineFiles.Count < 1)
      {
        if (!silent)
          MessageBox.Show("No online update was found !");
        Close();
        return;
      }
      progressBar1.Maximum = onlineFiles.Count;
      runningThreads = 0;
      for (int i = 1; i <= 5; i++)
      {
        new Thread(DownloadThread).Start();
      }
    }

    void DownloadThread()
    {
      lock (this) { runningThreads++; }
      try
      {
        string tempFile = Path.GetTempFileName();
        CompressionWebClient client = new CompressionWebClient();
        int index = -1;
        while (index < onlineFiles.Count && !cancel)
        {
          lock (this)
          {
            counter++;
            index = counter;
          }
          if (index >= onlineFiles.Count)
            return;
          string onlineFile = onlineFiles[index];
          bool success = false;
          try
          {
            client.DownloadFile(onlineFile, tempFile);
            var extCol = ExtensionCollection.Load(tempFile);
            lock (this)
            {
              MpeCore.MpeInstaller.KnownExtensions.Add(extCol);
            }
            success = true;
          }
          catch (Exception ex)
          {
            System.Diagnostics.Debug.WriteLine(string.Format("Error downloading '{0}': {1}", onlineFile, ex.Message));
          }
          Invoke((Action)(() =>
          {
            progressBar1.Value++;
            listBox1.Items.Add(string.Format("{0}{1}", success ? "+" : "-", onlineFile));
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
          }));
          if (File.Exists(tempFile)) File.Delete(tempFile);
        }
      }
      catch { }
      finally
      {
        lock (this) 
        { 
          runningThreads--;
          if (runningThreads <= 0)
          {
            MpeCore.MpeInstaller.Save();
            Invoke((Action)(() =>
            {
              DialogResult = cancel ? DialogResult.Cancel : DialogResult.OK;
              Close();
            }));
          }
        }
      }
    }

    private void CancelClick(object sender, EventArgs e)
    {
      cancel = true;
      btnCancel.Enabled = false;
    }
  }
}