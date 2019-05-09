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
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class HTTPDownload : Form
  {
    private WebClient client;
    private string _target;

    public HTTPDownload()
    {
      // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
      ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;
      InitializeComponent();
      Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
    }

    public DialogResult ShowDialog(string url, string targetFile, string userAgentOs)
    {
      _target = targetFile;
      DownloadFile(url, targetFile, userAgentOs);
      return ShowDialog();
    }

    private void DownloadFile(string url, string targetFile, string userAgentOs)
    {
      labelURL.Text = url;
      labelTarget.Text = Path.GetFileName(targetFile);
      client = new WebClient();

#if DEBUG
      Uri fileuri = new Uri(url);
      Uri proxyUri = client.Proxy.GetProxy(fileuri);
      if (proxyUri == fileuri)
      {
        MessageBox.Show("No proxy detected.", url, MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else
      {
        MessageBox.Show("Proxy is " + proxyUri, url, MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
#endif

      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      client.Headers.Add("user-agent", @"Mozilla/4.0 (compatible; MSIE 7.0;" + userAgentOs);
      client.DownloadProgressChanged += client_DownloadProgressChanged;
      client.DownloadFileCompleted += client_DownloadFileCompleted;
      client.DownloadFileAsync(new Uri(url), targetFile);
    }

    private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error is WebException)
      {
        Utils.ErrorDlg(Localizer.GetBestTranslation("HTTPDownload_errDownloadFailed"));
        try
        {
          File.Delete(_target);
        }
        catch {}
        DialogResult = DialogResult.Abort;
      }
      else
        DialogResult = DialogResult.OK;
    }

    private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      client.CancelAsync();
      Utils.ErrorDlg(Localizer.GetBestTranslation("HTTPDownload_msgCanceledByUser"));
      try
      {
        File.Delete(_target);
      }
      catch {}
      DialogResult = DialogResult.Cancel;
    }

    private void HTTPDownload_Load(object sender, EventArgs e) {}
  }
}