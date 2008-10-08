#region Copyright (C) 2005-2008 Team MediaPortal
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class HTTPDownload : Form
  {
    private WebClient client = null;
    private string _target;

    private void UpdateUI()
    {
      this.Text = Utils.GetBestTranslation("HTTPDownload_Title");
      labelSourceURL.Text = Utils.GetBestTranslation("HTTPDownload_labelSourceURL");
      labelTargetFile.Text = Utils.GetBestTranslation("HTTPDownload_labelTargetFile");
      buttonCancel.Text = Utils.GetBestTranslation("HTTPDownload_buttonCancel");
    }

    public HTTPDownload()
    {
      InitializeComponent();
    }

    public DialogResult ShowDialog(string url, string targetFile, string userAgentOs)
    {
      _target = targetFile;
      DownloadFile(url, targetFile, userAgentOs);
      return base.ShowDialog();
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
        MessageBox.Show("Proxy is " + proxyUri.ToString(), url, MessageBoxButtons.OK,MessageBoxIcon.Warning);
      }
#endif

      client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      client.Headers.Add("user-agent", @"Mozilla/4.0 (compatible; MSIE 7.0;" + userAgentOs);
      client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
      client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
      try
      {
        client.DownloadFileAsync(new Uri(url), targetFile);
      }
      catch
      {
        Utils.ErrorDlg(Utils.GetBestTranslation("HTTPDownload_errDownloadFailed"));
        try
        {
          File.Delete(targetFile);
        }
        catch (Exception) { }
        DialogResult = DialogResult.Cancel;
      }
    }

    private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      client.CancelAsync();
      Utils.ErrorDlg(Utils.GetBestTranslation("HTTPDownload_msgCanceledByUser"));
      try
      {
        File.Delete(_target);
      }
      catch (Exception) { }
      DialogResult = DialogResult.Cancel;
    }

      private void HTTPDownload_Load(object sender, EventArgs e)
      {

      }
  }
}