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
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeInstaller.Dialogs
{
  public partial class DownloadInfo : Form
  {
    public WebClient Client = new WebClient();
    public bool silent = false;
    private int counter = 0;
    private string tempFile = "";
    private List<string> onlineFiles = new List<string>();

    public DownloadInfo()
    {
      InitializeComponent();
      Client.DownloadProgressChanged += Client_DownloadProgressChanged;
      Client.DownloadFileCompleted += Client_DownloadFileCompleted;
    }

    private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        try
        {
          MpeCore.MpeInstaller.KnownExtensions.Add(ExtensionCollection.Load(tempFile));
          File.Delete(tempFile);
        }
        catch (Exception)
        {
          listBox1.Items.Add("Error to download");
        }
      }
      counter++;

      if (counter >= onlineFiles.Count)
      {
        MpeCore.MpeInstaller.Save();
        Close();
        return;
      }
      NextItem();
    }

    private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar2.Value = e.ProgressPercentage;
      label1.Text = string.Format("{0} kb/{1} kb", e.BytesReceived / 1024, e.TotalBytesToReceive / 1024);
    }

    private void DownloadInfo_Shown(object sender, EventArgs e)
    {
      onlineFiles = MpeCore.MpeInstaller.InstalledExtensions.GetUpdateUrls(new List<string>());
      onlineFiles = MpeCore.MpeInstaller.KnownExtensions.GetUpdateUrls(onlineFiles);
      onlineFiles = MpeCore.MpeInstaller.GetInitialUrlIndex(onlineFiles);

      if (onlineFiles.Count < 1)
      {
        if (!silent)
          MessageBox.Show("No online update was found !");
        Close();
        return;
      }
      progressBar1.Maximum = onlineFiles.Count + 2;
      NextItem();
    }

    private void NextItem()
    {
      try
      {
        string onlineFile = onlineFiles[counter];
        tempFile = Path.GetTempFileName();
        listBox1.Items.Add(onlineFile);
        progressBar1.Value++;
        progressBar1.Update();
        listBox1.Update();
        Update();
        Client.DownloadFileAsync(new Uri(onlineFile), tempFile);
      }
      catch (Exception ex)
      {
        if (!silent)
          MessageBox.Show("Error :" + ex.Message);
        listBox1.Items.Add("Error download");
      }
    }
  }
}