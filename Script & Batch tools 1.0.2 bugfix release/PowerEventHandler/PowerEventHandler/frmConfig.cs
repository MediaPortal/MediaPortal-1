using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
//using Microsoft.Win32;

namespace PowerEventHandler
{
  [Flags]
  public enum EXECUTION_STATE : uint
  {
    ES_SYSTEM_REQUIRED = 0x00000001,
    ES_DISPLAY_REQUIRED = 0x00000002,
    ES_CONTINUOUS = 0x80000000
  }

  public partial class frmConfig : Form
  {
    //private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSCOMMAND = 0x0112;
    //private const int WM_CLOSE = 0x0010;
    private const int WM_POWERBROADCAST = 0x0218;
    private const int WM_ENDSESSION = 0x0016;
    private const int WM_QUERYENDSESSION = 0x0011;

    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMSTANDBY = 0x0005;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    //private const int PBTF_APMRESUMEFROMFAILURE = 0x00000001;
    //private const int PBT_APMBATTERYLOW = 0x0009;
    //private const int PBT_APMPOWERSTATUSCHANGE = 0x000A;
    //private const int PBT_APMOEMEVENT = 0x000B;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    private const int BROADCAST_QUERY_DENY = 0x424D5144;

    private const int SC_SCREENSAVE = 0xF140;

    bool _onResumeRunning = false;
    bool _suspended = false;

    static object syncObj = new object();
    //called when windows wakes up again
    static object syncResume = new object();

    [DllImport("Kernel32.DLL")]
    private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);

    public frmConfig()
    {
      InitializeComponent();
    }

    protected override void WndProc(ref Message msg)
    {
      try
      {
        if (msg.Msg == WM_POWERBROADCAST)
        {
          txbLog.Text += String.Format("PowerEventHandler: WM_POWERBROADCAST: {0}", msg.WParam.ToInt32()) + Environment.NewLine;
          switch (msg.WParam.ToInt32())
          {
            //The PBT_APMQUERYSUSPEND message is sent to request permission to suspend the computer.
            //An application that grants permission should carry out preparations for the suspension before returning.
            //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
            case PBT_APMQUERYSUSPEND:
              txbLog.Text += "PowerEventHandler: Windows is requesting hibernate mode" + Environment.NewLine;
              if (!OnQuerySuspend(ref msg)) return;
              break;

            //The PBT_APMQUERYSTANDBY message is sent to request permission to suspend the computer.
            //An application that grants permission should carry out preparations for the suspension before returning.
            //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
            case PBT_APMQUERYSTANDBY:
              // Stop all media before suspending or hibernating
              txbLog.Text += "PowerEventHandler: Windows is requesting standby mode" + Environment.NewLine;
              if (!OnQuerySuspend(ref msg)) return;
              break;

            //The PBT_APMQUERYSUSPENDFAILED message is sent to notify the application that suspension was denied
            //by some other application. However, this message is only sent when we receive PBT_APMQUERY* before.
            case PBT_APMQUERYSUSPENDFAILED:
              txbLog.Text += "PowerEventHandler: Windows is denied to go to suspended mode" + Environment.NewLine;
              // dero: IT IS NOT SAFE to rely on this message being sent! Sometimes it is not sent even if we
              // processed PBT_AMQUERYSUSPEND/PBT_APMQUERYSTANDBY
              // I observed this using TVService.PowerScheduler
              break;

            //The PBT_APMQUERYSTANDBYFAILED message is sent to notify the application that suspension was denied
            //by some other application. However, this message is only sent when we receive PBT_APMQUERY* before.
            case PBT_APMQUERYSTANDBYFAILED:
              txbLog.Text += "PowerEventHandler: Windows is denied to go to standby mode" + Environment.NewLine;
              // dero: IT IS NOT SAFE to rely on this message being sent! Sometimes it is not sent even if we
              // processed PBT_AMQUERYSUSPEND/PBT_APMQUERYSTANDBY
              // I observed this using TVService.PowerScheduler
              break;

            case PBT_APMSTANDBY:
              txbLog.Text += "PowerEventHandler: Windows is standbying" + Environment.NewLine;
              OnSuspend(ref msg);
              break;

            case PBT_APMSUSPEND:
              txbLog.Text += "PowerEventHandler: Windows is hibernating" + Environment.NewLine;
              OnSuspend(ref msg);
              break;

            //The PBT_APMRESUMECRITICAL event is broadcast as a notification that the system has resumed operation. 
            //this event can indicate that some or all applications did not receive a PBT_APMSUSPEND event. 
            //For example, this event can be broadcast after a critical suspension caused by a failing battery.
            case PBT_APMRESUMECRITICAL:
              txbLog.Text += "PowerEventHandler: Windows has resumed from critical hibernate mode" + Environment.NewLine;
              OnResume();
              break;

            //The PBT_APMRESUMESUSPEND event is broadcast as a notification that the system has resumed operation after being suspended.
            case PBT_APMRESUMESUSPEND:
              txbLog.Text += "PowerEventHandler: Windows has resumed from hibernate mode" + Environment.NewLine;
              OnResume();
              break;

            //The PBT_APMRESUMESTANDBY event is broadcast as a notification that the system has resumed operation after being standby.
            case PBT_APMRESUMESTANDBY:
              txbLog.Text += "PowerEventHandler: Windows has resumed from standby mode" + Environment.NewLine;
              OnResume();
              break;

            //The PBT_APMRESUMEAUTOMATIC event is broadcast when the computer wakes up automatically to
            //handle an event. An application will not generally respond unless it is handling the event, because the user is not present.
            case PBT_APMRESUMEAUTOMATIC:
              txbLog.Text += "PowerEventHandler: Windows has resumed from standby or hibernate mode to handle a requested event" + Environment.NewLine;
              OnResume();
              break;
          }
        }
        else if (msg.Msg == WM_QUERYENDSESSION)
        {
          txbLog.Text += "PowerEventHandler: Windows is requesting shutdown mode" + Environment.NewLine;
          base.WndProc(ref msg);
          if (!OnQueryShutDown(ref msg))
          {
            txbLog.Text += "PowerEventHandler: shutdown mode denied" + Environment.NewLine;
            return;
          }
          else
          {
            txbLog.Text += "PowerEventHandler: shutdown mode granted" + Environment.NewLine;
            //base._shuttingDown = true;
            msg.Result = (IntPtr)1; //tell windows we are ready to shutdown          
          }
        }
        // gibman - http://mantis.team-mediaportal.com/view.php?id=1073     
        else if (msg.Msg == WM_ENDSESSION) // && msg.WParam == ((IntPtr)1))
        {
          base.WndProc(ref msg);
          txbLog.Text += "PowerEventHandler: shutdown mode executed" + Environment.NewLine;
          msg.Result = IntPtr.Zero; // tell windows it's ok to shutdown        

          //tMouseClickTimer.Stop();
          //tMouseClickTimer.Dispose();
          Application.ExitThread();
          Application.Exit();
        }
        base.WndProc(ref msg);
      }
      catch (Exception ex)
      {
        txbLog.Text += ex.Message;
      }
    }

    private void OnResume()
    {
      if (_onResumeRunning == true)
      {
        txbLog.Text += "PowerEventHandler: OnResume - already running -> return without further action" + Environment.NewLine;
        return;
      }
      lock (syncResume)
      {
        if (!_suspended)
        {
          txbLog.Text += "PowerEventHandler: OnResume - OnResume called but !_suspended" + Environment.NewLine;
          return;
        }
        EXECUTION_STATE oldState = EXECUTION_STATE.ES_CONTINUOUS;
        if (cbTurnTVOn.Checked)
        {
          txbLog.Text += "PowerEventHandler: OnResume - Trying to wake up the monitor / tv" + Environment.NewLine;
          EXECUTION_STATE state = EXECUTION_STATE.ES_CONTINUOUS |
                                  EXECUTION_STATE.ES_DISPLAY_REQUIRED;
          oldState = SetThreadExecutionState(state);
        }


        txbLog.Text += "PowerEventHandler: OnResume" + Environment.NewLine;

        _onResumeRunning = true;

        if (txbOnResume.Text != String.Empty) RunExternalProgram(txbOnResume.Text);

        _suspended = false;
        _onResumeRunning = false;
      }
    }

    //called when windows hibernates or goes into standbye mode
    private void OnSuspend(ref Message msg)
    {
      lock (syncObj)
      {

        if (_suspended)
        {
          return;
        }
        _suspended = true;

        if (txbOnSuspend.Text != String.Empty) RunExternalProgram(txbOnSuspend.Text);

        txbLog.Text += "PowerEventHandler: OnSuspend - Done" + Environment.NewLine;
      }
    }

    //called when windows asks permission to hibernate or standby
    private bool OnQuerySuspend(ref Message msg)
    {
      lock (syncObj)
      {
        txbLog.Text += "PowerEventHandler: OnQuerySuspend" + Environment.NewLine;
        if (_suspended)
        {
          return true;
        }

        //if (Recorder.IsAnyCardRecording()) // if we are recording then deny request
        //{
        //  msg.Result = new IntPtr(BROADCAST_QUERY_DENY);
        //  Log.Info("PowerEventHandler: TVRecording running -> Suspend stopped");
        //  return false;
        //}

        return true;
      }
    }

    //called when windows asks permission to restart, shutdown or logout
    private bool OnQueryShutDown(ref Message msg)
    {
      lock (syncObj)
      {
        txbLog.Text += "PowerEventHandler: OnQueryShutDown" + Environment.NewLine;
        //if (Recorder.IsAnyCardRecording()) // if we are recording then deny request
        //{
        //  msg.Result = new IntPtr(BROADCAST_QUERY_DENY);
        //  Log.Info("PowerEventHandler: TVRecording running -> Shutdown stopped");
        //  return false;
        //}

        return true;
      }
    }

    private void RunExternalProgram(string programPath)
    {
      try
      {
        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(programPath); //new System.Diagnostics.ProcessStartInfo(@"C:\listfiles.bat");
        psi.RedirectStandardOutput = false;
        psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        psi.UseShellExecute = true;
        System.Diagnostics.Process listFiles;
        txbLog.Text += "PowerEventHandler: Start program: " + programPath + Environment.NewLine;
        int LauchTimeOut = Convert.ToInt16(txbLaunchTimeout.Text) * 1000;
        txbLog.Text += "PowerEventHandler: LaunchTimeOut is set to: " + LauchTimeOut.ToString() + Environment.NewLine;
        listFiles = System.Diagnostics.Process.Start(psi);
        //System.IO.StreamReader myOutput = listFiles.StandardOutput;
        listFiles.WaitForExit(LauchTimeOut);
        if (listFiles.HasExited)
        {
          //string output = myOutput.ReadToEnd();
          //this.processResults.Text = output;
          txbLog.Text += "PowerEventHandler: TimeOut while starting program: " + programPath + Environment.NewLine;
        }
      }
      catch (Exception ex)
      {
        txbLog.Text += "PowerEventHandler: Problem while starting program: " + programPath + Environment.NewLine;
        txbLog.Text += ex.Message + Environment.NewLine;
      }
    }

    private void cbTurnTVOn_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      this.Visible = !this.Visible;
      this.WindowState = FormWindowState.Normal;
    }

    private void frmConfig_FormClosed(object sender, FormClosedEventArgs e)
    {
      Application.Exit();
    }

    private void frmConfig_FormClosing(object sender, FormClosingEventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void btnBrowseOnSuspend_Click(object sender, EventArgs e)
    {
      if (System.IO.File.Exists(txbOnSuspend.Text)) openFileDialog.FileName = txbOnSuspend.Text;

      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        txbOnSuspend.Text = openFileDialog.FileName;
      }
    }

    private void btnBrowseOnResume_Click(object sender, EventArgs e)
    {
      if (System.IO.File.Exists(txbOnResume.Text)) openFileDialog.FileName = txbOnResume.Text;

      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        txbOnResume.Text = openFileDialog.FileName;
      }
    }

    private void StartTimer_Tick(object sender, EventArgs e)
    {
      this.Visible = false;
      StartTimer.Enabled = false;
    }

    private void frmConfig_Load(object sender, EventArgs e)
    {
      if(cbMinimizeAtStartup.Checked) StartTimer.Enabled = true;
    }

    private void frmConfig_SizeChanged(object sender, EventArgs e)
    {
      if (this.WindowState == FormWindowState.Minimized)
      {
        this.Visible = false;
      }
    }

      private void txbLaunchTimeout_KeyDown(object sender, KeyEventArgs e)
      {
          if ( (e.KeyValue < 48 || e.KeyValue > 57) && e.KeyCode != Keys.Back)
          {
              System.Diagnostics.Debug.WriteLine(e.KeyCode);
              e.SuppressKeyPress = true;
          }
      }

    private void btnTestOnResume_Click(object sender, EventArgs e)
    {
      if (txbOnResume.Text != String.Empty) RunExternalProgram(txbOnResume.Text);
    }

    private void btnTestOnSuspend_Click(object sender, EventArgs e)
    {
      if (txbOnSuspend.Text != String.Empty) RunExternalProgram(txbOnSuspend.Text);
    }

  }
}