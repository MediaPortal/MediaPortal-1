#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal
{
  /// <summary>
  /// SplashScreen controller class
  /// </summary>
  /// <remarks>
  /// This class is a thread-safe wrapper around the real splash screen.
  /// The real splash screen runs in a different thread.  All communications with it should
  /// go through this wrapper class to avoid cross-thread updating of controls in the 
  /// splash screen.
  /// </remarks>
  public class SplashScreen
  {
    public string Version;
    private bool _stopRequested;
    private bool _alwaysOnTop;
    private SplashForm _frm;
    private FullScreenSplashScreen _frmFull;
    private string _info;
    internal static Screen CurrentDisplay;

    /// <summary>
    /// Starts the splash screen.
    /// </summary>
    public void Run()
    {
      var thread = new Thread(DoRun) {Name = "SplashScreen"};
      thread.Start();
    }

    /// <summary>
    /// Stops the splash screen.
    /// </summary>
    public void Stop()
    {
      _stopRequested = true;
    }

    /// <summary>
    /// Determine if the splash screen has been closed
    /// </summary>
    public bool IsStopped()
    {
      return _frm == null && _frmFull == null;
    }

    /// <summary>
    /// Set the contents of the information label of the splash screen
    /// </summary>
    /// <param name="information">the information to set</param>
    public void SetInformation(string information)
    {
      _info = information;
    }

    /// <summary>
    /// Starts the actual splash screen.
    /// </summary>
    /// <remarks>
    /// This method is started in a background thread by the <see cref="Run"/> method.</remarks>
    private void DoRun()
    {
      try
      {
        bool useFullScreenSplash;
        bool startFullScreen;
        int screenNumber;

        using (Settings xmlreader = new MPSettings())
        {
          useFullScreenSplash = xmlreader.GetValueAsBool("general", "usefullscreensplash", true);
          startFullScreen = !D3D.WindowedOverride && (D3D.FullscreenOverride || xmlreader.GetValueAsBool("general", "startfullscreen", true));
          screenNumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);
          _alwaysOnTop = xmlreader.GetValueAsBool("general", "alwaysontop", false);
        }
        
        if (D3D.ScreenNumberOverride >= 0)
        {
          screenNumber = D3D.ScreenNumberOverride;
        }

        if (screenNumber < 0 || screenNumber >= Screen.AllScreens.Length)
        {
          screenNumber = 0;
        }

        CurrentDisplay = Screen.AllScreens[screenNumber];
        Log.Info("SplashScreen: Splash screen is using screen: {0} (Position: {1},{2} Dimensions: {3}x{4})",
          GetCleanDisplayName(CurrentDisplay), CurrentDisplay.Bounds.Location.X, CurrentDisplay.Bounds.Location.Y, CurrentDisplay.Bounds.Width, CurrentDisplay.Bounds.Height);

        if (useFullScreenSplash && startFullScreen)
        {
          ShowFullScreenSplashScreen();
        }
        else
        {
          ShowNormalSplash();
        }
      }
      catch (Exception e)
      {
        Log.Error("Error during splashscreen handling: {0}", e.Message);
      }
    }

    /// <summary>
    /// XP returns random garbage in a name of a display in .NET
    /// </summary>
    /// <param name="screen"></param>
    /// <returns></returns>
    private static string GetCleanDisplayName(Screen screen)
    {
      if (OSInfo.OSInfo.VistaOrLater())
      {
        return screen.DeviceName;
      }

      int length = screen.DeviceName.IndexOf("\0", StringComparison.Ordinal);
      string deviceName = length == -1 ? screen.DeviceName : screen.DeviceName.Substring(0, length);
      return deviceName;
    }

    /// <summary>
    /// handles the normal splash screen
    /// </summary>
    private void ShowNormalSplash()
    {
      _frm = new SplashForm {TopMost = _alwaysOnTop};
      _frm.Location = new Point(CurrentDisplay.Bounds.X + CurrentDisplay.Bounds.Width/2 - _frm.Size.Width/2,
                                CurrentDisplay.Bounds.Y + CurrentDisplay.Bounds.Height/2 - _frm.Size.Height/2);
      _frm.SetVersion(Version);
      _frm.Show();
      _frm.Update();
      _frm.FadeIn();
      string oldInfo = null;

      // run until stop of splash screen is requested
      while (!_stopRequested) 
      {
        if (oldInfo != _info)
        {
          _frm.SetInformation(_info);
          oldInfo = _info;
        }
        Thread.Sleep(10);
      }

      _frm.FadeOut();
      _frm.Close();
      _frm = null;
    }

    /// <summary>
    /// handles the full screen splash
    /// </summary>
    private void ShowFullScreenSplashScreen()
    {
      Cursor.Hide();
      _frmFull = new FullScreenSplashScreen();
      _frmFull.RetrieveSplashScreenInfo();
      _frmFull.TopMost = _alwaysOnTop;
      _frmFull.Bounds = CurrentDisplay.Bounds;
      _frmFull.lblMain.Parent = _frmFull.pbBackground;
      _frmFull.lblVersion.Parent = _frmFull.lblMain;
      _frmFull.lblCVS.Parent = _frmFull.lblMain;
      _frmFull.SetVersion(Version);
      _frmFull.Show();
      _frmFull.Update();
      _frmFull.Opacity = 100;

      string oldInfo = null;

      // run until stop of splash screen is requested
      while (!_stopRequested)
      {
        if (oldInfo != _info)
        {
          _frmFull.SetInformation(_info);
          oldInfo = _info;
        }
        Thread.Sleep(10);
      }
      _frmFull.Close();
      _frmFull = null;
    }

    /// <summary>
    /// Summary description for SplashScreen.
    /// </summary>
    private class SplashForm : MPForm
    {
      private Panel _panel1;
      private Label _versionLabel;
      private Label _cvsLabel;
      private Label _informationLabel;

      /// <summary>
      /// Required designer variable.
      /// </summary>
      private readonly Container _components = null;

      /// <summary>
      /// 
      /// </summary>
      public SplashForm()
      {
        // Required for Windows Form Designer support
        InitializeComponent();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="information"></param>
      public void SetInformation(string information)
      {
        _informationLabel.Text = information;
        Update();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="version"></param>
      public void SetVersion(string version)
      {
        string[] strVersion = version.Split('-');
        _versionLabel.Text = strVersion[0];
        Log.Info("Version: Application {0}", strVersion[0]);
        if (strVersion.Length > 1)
        {
          string day   = strVersion[2].Substring(0, 2);
          string month = strVersion[2].Substring(3, 2);
          string year  = strVersion[2].Substring(6, 4);
          string time  = strVersion[3].Substring(0, 5);
          string build = strVersion[4].Substring(0, 13).Trim();
          _cvsLabel.Text = string.Format("{0} {1} ({2}-{3}-{4} / {5} CET)", strVersion[1], build, year, month, day, time);
          Log.Info("Version: {0}", _cvsLabel.Text);
        }
        Update();
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose(bool disposing)
      {
        if (disposing)
        {
          if (_components != null)
          {
            _components.Dispose();
          }
        }
        base.Dispose(disposing);
      }

      /// <summary>
      /// 
      /// </summary>
      public void FadeIn()
      {
        while (Opacity <= 0.9)
        {
          Opacity += 0.02;
          Thread.Sleep(15);
        }
      }

      /// <summary>
      /// 
      /// </summary>
      public void FadeOut()
      {
        while (Opacity >= 0.02)
        {
          Opacity -= 0.02;
          Thread.Sleep(15);
        }
        SendToBack();
        Hide();
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
        this._panel1 = new System.Windows.Forms.Panel();
        this._informationLabel = new System.Windows.Forms.Label();
        this._versionLabel = new System.Windows.Forms.Label();
        this._cvsLabel = new System.Windows.Forms.Label();
        this._panel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // panel1
        // 
        this._panel1.BackColor = System.Drawing.Color.Transparent;
        this._panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        this._panel1.Controls.Add(this._informationLabel);
        this._panel1.Controls.Add(this._versionLabel);
        this._panel1.Controls.Add(this._cvsLabel);
        this._panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this._panel1.Location = new System.Drawing.Point(0, 0);
        this._panel1.Name = "_panel1";
        this._panel1.Size = new System.Drawing.Size(399, 175);
        this._panel1.TabIndex = 0;
        // 
        // informationLabel
        // 
        this._informationLabel.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this._informationLabel.BackColor = System.Drawing.Color.Transparent;
        this._informationLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold,
                                                             System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this._informationLabel.ForeColor = System.Drawing.Color.White;
        this._informationLabel.Location = new System.Drawing.Point(11, 138);
        this._informationLabel.Name = "_informationLabel";
        this._informationLabel.Size = new System.Drawing.Size(377, 16);
        this._informationLabel.TabIndex = 4;
        this._informationLabel.Text = "Information";
        this._informationLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // versionLabel
        // 
        this._versionLabel.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this._versionLabel.BackColor = System.Drawing.Color.Transparent;
        this._versionLabel.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular,
                                                         System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this._versionLabel.ForeColor = System.Drawing.Color.White;
        this._versionLabel.Location = new System.Drawing.Point(277, 113);
        this._versionLabel.Name = "_versionLabel";
        this._versionLabel.Size = new System.Drawing.Size(111, 16);
        this._versionLabel.TabIndex = 5;
        this._versionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // cvsLabel
        // 
        this._cvsLabel.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this._cvsLabel.BackColor = System.Drawing.Color.Transparent;
        this._cvsLabel.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular,
                                                     System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this._cvsLabel.Location = new System.Drawing.Point(24, 113);
        this._cvsLabel.Name = "_cvsLabel";
        this._cvsLabel.Size = new System.Drawing.Size(211, 16);
        this._cvsLabel.TabIndex = 5;
        // 
        // SplashForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackgroundImage = global::MediaPortal.Properties.Resources.mplogo;
        this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        this.ClientSize = new System.Drawing.Size(400, 172);
        this.Controls.Add(this._panel1);
        this.DoubleBuffered = true;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.MaximizeBox = false;
        this.Name = "SplashForm";
        this.Opacity = 0;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "SplashScreen";
        this.TopMost = false;
        this.TransparencyKey = System.Drawing.Color.FromArgb(0, 0, 0, 0);
        this._panel1.ResumeLayout(false);
        this.ResumeLayout(false);
      }

      #endregion
    }
  }
}