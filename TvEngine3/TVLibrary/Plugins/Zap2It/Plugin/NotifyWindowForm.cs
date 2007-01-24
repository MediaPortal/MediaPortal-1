using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProcessPlugins.EpgGrabber
{
   public partial class NotifyWindowForm : Form
   {
      Timer _closetimer = new Timer();

      string _Title     = String.Empty;
      string _Text      = String.Empty;
      int    _iTimeOut  = 20000;

      public NotifyWindowForm(string strTitle, string strText, int iTimeOutMS)
      {
         InitializeComponent();
         _closetimer.Enabled = false;
         _Title    = strTitle;
         _Text     = strText;
         _iTimeOut = iTimeOutMS;

         this.Text            = _Title;
         this.labelText.Text  = _Text;
         _closetimer.Interval = _iTimeOut;

      }

      private void NotifyWindowForm_Load(object sender, EventArgs e)
      {
         _closetimer.Tick += new EventHandler(_closetimer_Tick);
         _closetimer.Enabled = true;
      }

      void _closetimer_Tick(object sender, EventArgs e)
      {
         //throw new Exception("The method or operation is not implemented.");
         _closetimer.Enabled = false;
         _closetimer.Tick -= _closetimer_Tick;
         this.Close();
      }

      private void buttonOK_Click(object sender, EventArgs e)
      {
         _closetimer.Tick -= _closetimer_Tick;
         _closetimer.Enabled = false;
         this.Close();
      }
   }
}