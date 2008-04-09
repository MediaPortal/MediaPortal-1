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
      this.Text = Localizer.Instance.GetString("HTTPDownload_Title");
      labelSourceURL.Text = Localizer.Instance.GetString("HTTPDownload_labelSourceURL");
      labelTargetFile.Text = Localizer.Instance.GetString("HTTPDownload_labelTargetFile");
      buttonCancel.Text = Localizer.Instance.GetString("HTTPDownload_buttonCancel");
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
      client.Headers.Add("user-agent", @"Mozilla/4.0 (compatible; MSIE 7.0;" + userAgentOs);
      client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
      client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
      try
      {
        client.DownloadFileAsync(new Uri(url), targetFile);
      }
      catch
      {
        Utils.ErrorDlg(Localizer.Instance.GetString("HTTPDownload_errDownloadFailed"));
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
      Utils.ErrorDlg(Localizer.Instance.GetString("HTTPDownload_msgCanceledByUser"));
      try
      {
        File.Delete(_target);
      }
      catch (Exception) { }
      DialogResult = DialogResult.Cancel;
    }
  }
}