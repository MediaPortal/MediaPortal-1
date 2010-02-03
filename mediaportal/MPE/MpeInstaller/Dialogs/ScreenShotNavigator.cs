using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeInstaller.Dialogs
{
  public partial class ScreenShotNavigator : Form
  {
    public List<string> Urls;
    public int Index = 0;
    public ScreenShotNavigator()
    {
      InitializeComponent();
      Urls = new List<string>();
      pictureBox.LoadProgressChanged += pictureBox_LoadProgressChanged;
      pictureBox.LoadCompleted += pictureBox_LoadCompleted;
    }

    void pictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      progressBar1.Visible = false;
    }

    void pictureBox_LoadProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
    }

    public void Set(PackageClass pak)
    {
      Index = 0;
      Text = string.Format("{0} Screen Shots ({1})", pak.GeneralInfo.Name, Urls.Count);
      pictureBox.ImageLocation = "";
      progressBar1.Visible = false;
      Urls.Clear();
      string[] u = pak.GeneralInfo.Params[ParamNamesConst.ONLINE_SCREENSHOT].Value.Split(';');
      foreach (var s in u)
      {
        Urls.Add(s);
      }
      SetButton();
      LoadImage();
    }

    private void btn_prev_Click(object sender, EventArgs e)
    {
      Index--;
      if (Index < 0)
        Index = 0;
      SetButton();
      LoadImage();
    }

    private void btn_next_Click(object sender, EventArgs e)
    {
      Index++;
      if (Index > Urls.Count - 1)
        Index = Urls.Count - 1;
      SetButton();
      LoadImage();
    }

    private void ScreenShotNavigator_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = true;
      this.Hide();
    }

    private void LoadImage()
    {
      pictureBox.LoadAsync(Urls[Index]);
      progressBar1.Value = 0;
      progressBar1.Visible = true;
    }

    private void SetButton()
    {
      if (Index - 1 < 0)
        btn_prev.Enabled = false;
      else
        btn_prev.Enabled = true;
      if (Index + 1 > Urls.Count - 1)
        btn_next.Enabled = false;
      else
        btn_next.Enabled = true;
    }
  }
}
