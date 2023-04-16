using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MPx86Proxy
{
    public partial class MainForm : Form
    {
        //[DllImport("user32.dll")]
        //private static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        //[DllImport("user32.dll")]
        //private static extern int RegisterWindowMessage(string message); //Defines a new window message that is guaranteed to be unique throughout the system. The message value can be used when sending or posting messages.

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern ushort GlobalAddAtom(string lpString);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern ushort GlobalFindAtom(string lpString);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern ushort GlobalDeleteAtom(ushort nAtom);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern uint GlobalGetAtomName(ushort nAtom, System.Text.StringBuilder lpBuffer, int nSize);


        //private int _MessageId = -1;

        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string message); //Defines a new window message that is guaranteed to be unique throughout the system. The message value can be used when sending or posting messages.
        
        private uint _WM_RC_PluginNotify;

        private ConnectionHandler _ConnectionHandler = new ConnectionHandler();

        private bool _Hide = false;
        private bool _AppClose = false;

        internal static MainForm Instance;

        public MainForm(string[] args)
        {
            Instance = this;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-?":
                    case "?":
                    case "help":
                    case "-help":
                        break;

                    case "-h":
                        this._Hide = true; // -h
                        break;

                    default:
                        break;
                }
            }

            this.InitializeComponent();

            //Version
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text += version;

            //Detect iMON dll
            Drivers.iMONDisplay.Detect();

            //Start server
            this._ConnectionHandler.Start(0);

            //Register WM RC Notify mesage
            this._WM_RC_PluginNotify = (uint)RegisterWindowMessage("WM_RC_PLUGIN_NOTIFY");

            //Init iMON RC API
            Drivers.iMONApi.RcResult result = Drivers.iMONApi.IMON_RcApi_Init(this.Handle, this._WM_RC_PluginNotify);
            this.logWindow("iMON API RC INIT: " + result.ToString());
            //result = Drivers.iMONApi.IMON_RcApi_IsInited();
            //result = Drivers.iMONApi.IMON_RcApi_IsPluginModeEnabled();

            this.logWindow("Ready.");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == Program.WM_ACTIVATEAPP && (int)m.WParam == Process.GetCurrentProcess().Id)
                this.cbMainFormUnhide(); //someone (another process) said that we should show the window (WM_ACTIVATEAPP)
            else if (m.Msg == this._WM_RC_PluginNotify)
            {
                switch ((Drivers.iMONApi.RCNotifyCode)m.WParam)
                {
                    case Drivers.iMONApi.RCNotifyCode.RCNM_HW_CONNECTED:
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_HW_DISCONNECTED:
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_IMON_CLOSED:
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_IMON_RESTARTED:
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_PLUGIN_FAILED:
                        Drivers.iMONApi.RCNInitResult errCode = (Drivers.iMONApi.RCNInitResult)m.LParam;
                        Log.Log.Error("[iMONReceiver][WndProc][RCNM_PLUGIN_FAILED] " + errCode);

                        //Try again if no reply
                        if (errCode == Drivers.iMONApi.RCNInitResult.RCN_ERR_IMON_NO_REPLY)
                        {
                            Drivers.iMONApi.IMON_RcApi_Uninit();
                            System.Threading.Thread.Sleep(200);
                            Drivers.iMONApi.IMON_RcApi_Init(this.Handle, this._WM_RC_PluginNotify);
                        }
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_PLUGIN_SUCCEED:
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_KNOB_ACTION:
                    case Drivers.iMONApi.RCNotifyCode.RCNM_RC_BUTTON_DOWN:
                    case Drivers.iMONApi.RCNotifyCode.RCNM_RC_BUTTON_UP:
                        this._ConnectionHandler.OnEvent(RegisterEventEnum.ImonRcButtonsEvents, (int)m.WParam, (int)m.LParam);
                        break;

                    case Drivers.iMONApi.RCNotifyCode.RCNM_RC_REMOTE:
                        break;

                    default:
                        break;
                }
            }
        }

        #region Log Window
        internal void logWindow(string strText)
        {
            this.eventTableControl.AppendEvent(strText);
        }
        internal void logWindow(string strText, MPx86Proxy.Controls.EventTableMessageTypeEnum type)
        {
            this.eventTableControl.AppendEvent(strText, type);
        }
        internal void logWindow(string strText, Icon icon)
        {
            MPx86Proxy.Controls.EventTableMessageTypeEnum type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Info;
            if (icon == System.Drawing.SystemIcons.Warning)
                type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Warning;
            else if (icon == System.Drawing.SystemIcons.Error)
                type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Error;

            this.eventTableControl.AppendEvent(strText, type, icon);
        }
        internal void logWindow(string strText, Icon icon, bool bUpdateLast)
        {
            MPx86Proxy.Controls.EventTableMessageTypeEnum type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Info;
            if (icon == System.Drawing.SystemIcons.Warning)
                type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Warning;
            else if (icon == System.Drawing.SystemIcons.Error)
                type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Error;

            this.eventTableControl.AppendEvent(strText, type, icon, bUpdateLast);
        }
        internal void logWindow(string strText, Icon icon, bool bUpdateLast, bool bSystemMessage)
        {
            MPx86Proxy.Controls.EventTableMessageTypeEnum type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Info;
            if (icon == System.Drawing.SystemIcons.Warning)
                type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Warning;
            else if (icon == System.Drawing.SystemIcons.Error)
                type = MPx86Proxy.Controls.EventTableMessageTypeEnum.Error;

            if (bSystemMessage)
                type |= MPx86Proxy.Controls.EventTableMessageTypeEnum.System;

            this.eventTableControl.AppendEvent(strText, type, icon, bUpdateLast);
        }
        #endregion

        private void cbMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this._AppClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.notifyIcon.Visible = true;
                this.Hide();
            }
            else
            {
                this._ConnectionHandler.Stop();

                Drivers.iMONApi.IMON_RcApi_Uninit();
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                this.cbMainFormUnhide();
        }

        private void cbMainFormUnhide()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;

            this.notifyIcon.Visible = false;
            this.notifyIcon.Icon = MPx86Proxy.Properties.Resources.Logo;

            this.WindowState = FormWindowState.Normal;
        }

        private void cbMainFormShown(object sender, EventArgs e)
        {
            if (this._Hide)
            {
                this.notifyIcon.Visible = true;
                this.Hide();
            }
        }

        private void cbMainFormResize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                //notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(500);
                if (Application.OpenForms.Count == 1)
                    this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState || FormWindowState.Maximized == this.WindowState)
            {
                //notifyIcon1.Visible = false;

                //this.toolStripStatusLabelLastMessage.Size = new Size(this.Width - 400, this.toolStripStatusLabelLastMessage.Size.Height);
            }


            this.notifyIcon.Visible = !this.Visible;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this._AppClose = true;
            this.Close();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this._AppClose = true;
            this.Close();
        }
    }
}
