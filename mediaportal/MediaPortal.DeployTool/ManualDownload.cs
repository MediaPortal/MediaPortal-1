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

    public ManualDownload()
    {
      InitializeComponent();
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
        Utils.ErrorDlg("The requested file still does not exist.");
      else
        DialogResult = DialogResult.OK;
    }
  }
}