#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace MpeInstaller.Dialogs
{
  public partial class SplashScreen : Form
  {
    private string text = "";
    private int _counter = 0;

    public SplashScreen()
    {
      InitializeComponent();
    }

    public void SetImg(string screenfile)
    {
      img.Image = new Bitmap(screenfile);
    }

    private void SplashScreen_Load(object sender, EventArgs e)
    {
      this.Location = new Point(0, 0);
      lbl_text.Parent = img;
      this.Size = new Size(Screen.FromHandle(this.Handle).Bounds.Width + 1,
                           Screen.FromHandle(this.Handle).Bounds.Height + 1);
      timer1.Enabled = true;
      lbl_text.Text = "";
    }

    public void SetProgress(string txt, int per)
    {
      timer1.Enabled = false;
      lbl_text.Text = string.Format("{0} ({1} {2}%)", text, txt, per.ToString());
      Update();
    }

    public void ResetProgress()
    {
      timer1.Enabled = true;
    }

    public void SetInfo(string txt)
    {
      text = txt;
      lbl_text.Text = txt;
      Update();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(text))
      {
        _counter++;
        if (_counter > 4)
        {
          lbl_text.Text = text;
          _counter = 0;
        }
        lbl_text.Text += ".";
        Update();
      }
    }
  }
}