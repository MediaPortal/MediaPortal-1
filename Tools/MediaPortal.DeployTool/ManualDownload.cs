using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class ManualDownload : Form
  {
    private string _url;

    private void UpdateUI()
    {
      this.Text = Localizer.Instance.GetString("ManualDownload_Title");
      labelHeading.Text = Localizer.Instance.GetString("ManualDownload_labelHeading");
      labelTargetFile.Text = Localizer.Instance.GetString("ManualDownload_labelTargetFile");
      linkURL.Text = labelTargetFile.Text = Localizer.Instance.GetString("ManualDownload_linkURL");
      labelTargetDir.Text = Localizer.Instance.GetString("ManualDownload_labelTargetDir");
      linkDir.Text = Localizer.Instance.GetString("ManualDownload_linkDir");
      labelDesc.Text = Localizer.Instance.GetString("ManualDownload_labelDesc");
      buttonContinue.Text = Localizer.Instance.GetString("ManualDownload_buttonContinue");
    }

    public ManualDownload()
    {
      InitializeComponent();
      UpdateUI();
    }

    public DialogResult ShowDialog(string url, string targetFile, string targetDir)
    {
      _url = url;
      labelTargetFile.Text = targetFile;
      labelTargetDir.Text = targetDir;
      return base.ShowDialog();
    }

    private void linkURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(_url);
    }

    private void linkDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(labelTargetDir.Text);
    }

    private void buttonContinue_Click(object sender, EventArgs e)
    {
      if (!File.Exists(labelTargetDir.Text + "\\" + labelTargetFile.Text))
        Utils.ErrorDlg(Localizer.Instance.GetString("ManualDownload_errFileNotFound"));
      else
        DialogResult = DialogResult.OK;
    }
  }
}