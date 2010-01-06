using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Drawing;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MpeInstaller.Dialogs
{
  public partial class DownloadFile : Form
  {
    private string Source;
    private string Dest;
    public WebClient Client = new WebClient();

    public DownloadFile()
    {
      InitializeComponent();
    }

    private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        if (File.Exists(Dest))
        {
          WaitForNoBusy();
          if (!Client.IsBusy)
          {
            try
            {
              File.Delete(Dest);
            }
            catch (Exception) {}
          }
        }
        MessageBox.Show(e.Error.Message + "\n" + e.Error.InnerException);
      }
      button1.Enabled = false;
      Close();
    }

    private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
      label1.Text = string.Format("{0} kb/{1} kb", e.BytesReceived / 1024, e.TotalBytesToReceive / 1024);
    }

    public DownloadFile(string source, string dest)
    {
      InitializeComponent();
      StartDownload(source, dest);
    }

    public void StartDownload(string source, string dest)
    {
      Source = source;
      Dest = dest;
      Client.DownloadProgressChanged += client_DownloadProgressChanged;
      Client.DownloadFileCompleted += client_DownloadFileCompleted;
      Client.UseDefaultCredentials = true;
      Client.Proxy.Credentials = CredentialCache.DefaultCredentials;
      //Client.CachePolicy = new RequestCachePolicy();

      progressBar1.Minimum = 0;
      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      ShowDialog();
    }

    private void DownloadFile_Shown(object sender, EventArgs e)
    {
      Client.DownloadFileAsync(new Uri(Source), Dest);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Client.CancelAsync();
      WaitForNoBusy();
      this.Close();
    }

    private void WaitForNoBusy()
    {
      int counter = 0;
      while (Client.IsBusy || counter < 10)
      {
        counter++;
        Thread.Sleep(100);
      }
    }
  }
}