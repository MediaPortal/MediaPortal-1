using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Windows;
using Microsoft.Win32;

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

    /// <summary>
    /// Tries to load settings of the splashscreen (color, background image...)
    /// </summary>
    public bool RetrieveSplashBackground()
    {
      ReadSplashScreenXML();
      if (pbBackground.Image == null) ReadReferenceXML();

      TryToFindMPScreenSelectorSettings();

      return pbBackground.Image != null;
    }

    /// <summary>
    /// Tries to read the splashscreen.xml file of the currentskin
    /// </summary>
    private void ReadSplashScreenXML()
    {
      string SkinFilePath = string.Empty;

      // try to find the splashscreen.xml ín the curent skin folder
      string SkinDir = GetCurrentMpSkinFolder();
      SkinFilePath = SkinDir + "\\splashscreen.xml";

      if (!System.IO.File.Exists(SkinFilePath))
      {
        return; // if not found... leave
      }


      XmlDocument doc = new XmlDocument();
      doc.Load(SkinFilePath);
      XmlNodeList ControlsList = doc.DocumentElement.SelectNodes("/window/controls/control");

      foreach (XmlNode Control in ControlsList)
      {
        if (Control.SelectSingleNode("type/text()").Value.ToLower() == "image"
          && Control.SelectSingleNode("id/text()").Value == "1") // if the background image control is found
        {
          string BackgoundImageName = Control.SelectSingleNode("texture/text()").Value;
          string BackgroundImagePath = SkinDir + "\\media\\" + BackgoundImageName;
          if (System.IO.File.Exists(BackgroundImagePath))
          {
            pbBackground.Image = new Bitmap(BackgroundImagePath); // load the image as background
          }
          continue;
        }
        if (Control.SelectSingleNode("type/text()").Value.ToLower() == "label"
          && Control.SelectSingleNode("id/text()").Value == "2") // if the center label control is found
        {
          if (Control.SelectSingleNode("textsize") != null) // textsize info found?
          {
            float TextSize = float.Parse(Control.SelectSingleNode("textsize/text()").Value);
            lblMainLable.Font = new Font(lblMainLable.Font.FontFamily, TextSize, lblMainLable.Font.Style);
          }
          if (Control.SelectSingleNode("textcolor") != null) // textcolor info found?
          {
            Color TextColor = ColorTranslator.FromHtml(Control.SelectSingleNode("textcolor/text()").Value);
            lblMainLable.ForeColor = TextColor;
          }
        }
      }
    }

    /// <summary>
    /// Tries to find the background of the current skin by analysing the reference.xml
    /// </summary>
    private void ReadReferenceXML()
    {
      string SkinReferenceFilePath = string.Empty;
      string SkinDir = GetCurrentMpSkinFolder();

      SkinReferenceFilePath = SkinDir + "\\references.xml";

      XmlDocument doc = new XmlDocument();
      doc.Load(SkinReferenceFilePath);
      XmlNodeList ControlsList = doc.DocumentElement.SelectNodes("/controls/control");

      foreach (XmlNode Control in ControlsList)
      {
        if (Control.SelectSingleNode("type/text()").Value.ToLower() == "image")
        {
          string BackgoundImageName = Control.SelectSingleNode("texture/text()").Value;
          string BackgroundImagePath = SkinDir + "\\media\\" + BackgoundImageName;
          if (System.IO.File.Exists(BackgroundImagePath))
          {
            pbBackground.Image = new Bitmap(BackgroundImagePath); // load the image as background
          }
        }
      }
    }


    private string GetMpInstallationPath()
    {
      string AppDir = string.Empty;
      try
      {
        RegistryKey MpRegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Team MediaPortal\MediaPortal\");
        AppDir = (string)MpRegKey.GetValue("ApplicationDir", string.Empty);
      }
      catch
      { }
      return AppDir;
    }

    private string GetMpConfigPath()
    {
      string ConfigDir = string.Empty;
      try
      {
        RegistryKey MpRegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Team MediaPortal\MediaPortal\");
        ConfigDir = (string)MpRegKey.GetValue("ConfigDir", string.Empty);
      }
      catch
      { }
      return ConfigDir;
    }

    private string GetCurrentMpSkinFolder()
    {
      string MpSkinPath = string.Empty;
      try
      {
        string mpPath = GetMpInstallationPath();
        string mpConfigPath = GetMpConfigPath();

        XmlDocument doc = new XmlDocument();
        doc.Load(mpConfigPath + "\\mediaportal.xml");
        XmlNodeList SectionList = doc.DocumentElement.SelectNodes("/profile/section");

        foreach (XmlNode Section in SectionList)
        {
          try
          {
            if (Section.Attributes["name"].Value == "skin")
            {
              string MpSkinName = string.Empty;
              foreach (XmlNode Element in Section.ChildNodes)
              {
                if (Element.Attributes["name"].Value == "name")
                {
                  MpSkinName = Element.InnerText;
                  if(MpSkinName != string.Empty) MpSkinPath = mpPath + "\\Skin\\" + MpSkinName;
                  break;
                }
              }
              break;
            }
          }
          catch (NullReferenceException err)
          {
          }
        }

      }
      catch
      { }
      return MpSkinPath;
    }

    private void frmFullScreen_Load(object sender, EventArgs e)
    {
      Cursor.Hide();

      this.Location = new Point(0, 0);
      this.Size = new Size(Screen.FromHandle(this.Handle).Bounds.Width + 1, Screen.FromHandle(this.Handle).Bounds.Height + 1);
    }

    private void frmFullScreen_FormClosing(object sender, FormClosingEventArgs e)
    {
      Cursor.Show();
    }

    private void TryToFindMPScreenSelectorSettings()
    {
      string mpConfigPath = GetMpConfigPath();

      if (System.IO.File.Exists(mpConfigPath + "\\mediaportal.xml"))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(mpConfigPath + "\\mediaportal.xml");

        XmlNodeList SectionList = doc.DocumentElement.SelectNodes("/profile/section[@name='screenselector']");
        if (SectionList.Count > 0)
        {
          if (SectionList[0].SelectSingleNode("entry[@name='usescreenselector'][text()]").InnerText == "yes")
          {
            int ScreenNumber = Convert.ToInt16(SectionList[0].SelectSingleNode("entry[@name='screennumber'][text()]").InnerText); // read the screen number
            ScreenNumber++; // increase the number by 1 to respect the DisplayDevice naming convention

            foreach (Screen tmpscreen in Screen.AllScreens)
            {
              if (tmpscreen.DeviceName.Contains("DISPLAY" + ScreenNumber)) // if the selected Display is found
              {
                this.Location = new Point(tmpscreen.Bounds.X, tmpscreen.Bounds.Y); // set the form position into this screen
                this.Size = new Size(tmpscreen.Bounds.Width + 1, tmpscreen.Bounds.Height + 1);
              }
            }
          }
          else
          {
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.FromHandle(this.Handle).Bounds.Width + 1, Screen.FromHandle(this.Handle).Bounds.Height + 1);
          }
        }
      }
    }
  }
}