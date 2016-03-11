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

    private void pictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      progressBar1.Visible = false;
      lblIndex.Text = string.Format("{0} / {1}", Index + 1, Urls.Count);
      lblIndex.Visible = true;
    }

    private void pictureBox_LoadProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
    }

    public void Set(PackageClass pak)
    {
      Index = 0;
      pictureBox.ImageLocation = "";
      progressBar1.Visible = false;
      Urls.Clear();
      string[] u = pak.GeneralInfo.Params[ParamNamesConst.ONLINE_SCREENSHOT].Value.Split(ParamNamesConst.SEPARATORS);
      foreach (var s in u)
      {
        Urls.Add(s);
      }
      Text = string.Format("{0} Screen Shots ({1})", pak.GeneralInfo.Name, Urls.Count);
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
      lblIndex.Visible = false;
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