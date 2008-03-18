using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Windows;

namespace FullscreenMsg
{
  public partial class frmFullScreen : Form
  {
    IntPtr CurrentSplashScreenHandle = IntPtr.Zero;
    static IntPtr CurrentSpashScreenInfoHandle = IntPtr.Zero;
    string CurrentSplashScreenInfo = string.Empty;

    public MainForm MainFormObj;
    public int CloseTimeOut = 1000;
    string _CloseOnWindowName = string.Empty;
    string _CloseOnForegroundWindowName = string.Empty;
    public bool ForceForeground = true;
    public bool OberservateMPStartup = false;

    public string CloseOnWindowName
    {
      get { return _CloseOnWindowName; }
      set
      { 
        _CloseOnWindowName = value;
        _CloseOnForegroundWindowName = string.Empty;
      }
    }

    public string CloseOnForegroundWindowName
    {
      get { return _CloseOnForegroundWindowName; }
      set 
      {
        _CloseOnWindowName = string.Empty; 
        _CloseOnForegroundWindowName = value;
      }
    }


    public delegate int Callback(int hWnd, int lParam);
    Callback myCallBack = new Callback(EnumChildGetValue);

    public frmFullScreen()
    {
      InitializeComponent();
    }

    private void timerFullscreen_Tick(object sender, EventArgs e)
    {
      timerFullscreen.Enabled = false;

      if (this.Visible && ForceForeground)
      {
        this.Activate();
        WindowManagement.SetForegroundWindow(this.Handle); // && WindowManagement.GetForegroundWindow() != this.Handle
        this.Refresh();
      }

      if (OberservateMPStartup) CaptureMPsplashScreenInfo();

      if (_CloseOnWindowName != string.Empty) CloseOnWindowExistence();
      if (_CloseOnForegroundWindowName != string.Empty) CloseOnForegroundWindowExistence();

      Application.DoEvents();

      timerFullscreen.Enabled = true;
    }

    public static int EnumChildGetValue(int hWnd, int lParam)
    {
      if (WindowManagement.GetClassName(hWnd) == "WindowsForms10.STATIC.app.0.3ce0bb8" && CurrentSpashScreenInfoHandle == IntPtr.Zero) CurrentSpashScreenInfoHandle = (IntPtr)hWnd;
      return 1;
    }

    private void CaptureMPsplashScreenInfo()
    {
      if (CurrentSplashScreenHandle != IntPtr.Zero)
      {
        if (CurrentSpashScreenInfoHandle == IntPtr.Zero)
        {
          WindowManagement.EnumChildWindows((int)CurrentSplashScreenHandle, myCallBack, 0);
        }
        else
        {
          StringBuilder formDetails = new StringBuilder(256);
          int txtValue;
          txtValue = WindowManagement.GetWindowText((int)CurrentSpashScreenInfoHandle, formDetails, 256);
          string tmpString = formDetails.ToString().Trim();
          if (CurrentSplashScreenInfo != tmpString)
          {
            CurrentSplashScreenInfo = tmpString;
            System.Diagnostics.Debug.WriteLine(CurrentSplashScreenInfo);
            lblMainLable.Text = CurrentSplashScreenInfo;
          }
        }
      }
      else
      {
        CurrentSplashScreenHandle = WindowManagement.FindWindow(null, "SplashScreen");
      }
    }

    private void CloseOnForegroundWindowExistence()
    {
      IntPtr tmpFoundWindowHandle = IntPtr.Zero;

      WindowManagement.GetHandleFromPartialCaption(ref tmpFoundWindowHandle, _CloseOnForegroundWindowName);
      if (tmpFoundWindowHandle != IntPtr.Zero)
      {
        ForceForeground = false;
        if (tmpFoundWindowHandle == WindowManagement.GetForegroundWindow())
        {
          timerFullscreen.Enabled = false;
          System.Threading.Thread.Sleep(CloseTimeOut);
          this.Close();
        }
      }
    }

    private void CloseOnWindowExistence()
    {
      IntPtr tmpFoundWindowHandle = IntPtr.Zero;

      WindowManagement.GetHandleFromPartialCaption(ref tmpFoundWindowHandle, CloseOnWindowName);
      if (tmpFoundWindowHandle != IntPtr.Zero)
      {
        timerFullscreen.Enabled = false;
        System.Threading.Thread.Sleep(CloseTimeOut);
        this.Close();
      }
    }

    private void lblMainLable_DoubleClick(object sender, EventArgs e)
    {
      this.WindowState = FormWindowState.Minimized;
      this.ShowInTaskbar = true;

      this.MainFormObj.FormBorderStyle = FormBorderStyle.None;
      this.MainFormObj.Left = 0;
      this.MainFormObj.Top = 0;

      this.MainFormObj.ShowInTaskbar = true;
      this.MainFormObj.Show();
      this.MainFormObj.Activate();
    }


  }
}