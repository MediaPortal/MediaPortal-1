using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SetupTv
{
  public partial class NotifyForm : Form
  {
    public NotifyForm(string caption,string message)
    {
      InitializeComponent();
      this.Text = caption;
      label1.Text = message;
    }
    public void WaitForDisplay()
    {
      long ticks = DateTime.Now.Ticks;
      do
      {
        Application.DoEvents();
        System.Threading.Thread.Sleep(10);
      } while (DateTime.Now.Ticks < (ticks + 1500));
    }
  }
}