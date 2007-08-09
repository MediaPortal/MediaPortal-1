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

    public HTTPDownload()
    {
      InitializeComponent();
    }
    public DialogResult ShowDialog(string url, string targetFile)
    {
      _target = targetFile;
      DownloadFile(url, targetFile);
      return base.ShowDialog();
    }
    private void DownloadFile(string url, string targetFile)
    {
      labelURL.Text = url;
      labelTarget.Text = Path.GetFileName(targetFile);
      client = new WebClient();
      client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
      client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
      try
      {
        client.DownloadFileAsync(new Uri(url), targetFile);
      }
      catch
      {
        Utils.ErrorDlg("There was an error downloading the file.");
        File.Delete(targetFile);
        DialogResult = DialogResult.Cancel;
        Close();
      }
    }

    void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar.Value = e.ProgressPercentage;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      client.CancelAsync();
      Utils.ErrorDlg("Download canceled by user.");
      File.Delete(_target);
      DialogResult = DialogResult.Cancel;
    }
  }
}