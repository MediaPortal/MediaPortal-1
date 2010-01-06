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