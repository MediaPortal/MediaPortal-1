#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
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
    private bool stopRequested = false;
    private bool AllowWindowOverlayRequested = false;
    private SplashForm frm;
    private FullScreenSplashScreen frmFull;
    private Form OutDatedSkinForm = null;
    private string info;

    public SplashScreen()
    {
    }


    /// <summary>
    /// Starts the splash screen.
    /// </summary>
    public void Run()
    {
      Thread thread = new Thread(new ThreadStart(DoRun));
      thread.Name = "SplashScreen";
      thread.Start();
    }

    /// <summary>
    /// Stops the splash screen.
    /// </summary>
    public void Stop()
    {
      stopRequested = true;
    }

    /// <summary>
    /// Allows other windows to overlay the splashscreen
    /// </summary>
    public void AllowWindowOverlay(Form FrmOutdatedSkin)
    {
      AllowWindowOverlayRequested = true;
      OutDatedSkinForm = FrmOutdatedSkin;
    }

    /// <summary>
    /// Determine if the Splash has been closed
    /// </summary>
    public bool isStopped()
    {
      // do only return the state of the normal splashscreen to allow mp to work during the delay stop phase of the fullscreen splash
      return (frm == null);
    }

    /// <summary>
    /// Set the contents of the information label of the splash screen
    /// </summary>
    /// <param name="information">the information to set</param>
    public void SetInformation(string information)
    {
      info = information;
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
        bool useFullScreenSplash = true;
        bool startFullScreen = true;

        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          useFullScreenSplash = xmlreader.GetValueAsBool("general", "usefullscreensplash", true);
          startFullScreen = xmlreader.GetValueAsBool("general", "startfullscreen", true);
        }

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
    /// handles the normal splash screen
    /// </summary>
    private void ShowNormalSplash()
    {
      frm = new SplashForm();
      frm.SetVersion(Version);
      frm.Show();
      frm.Update();
      frm.FadeIn();
      string oldInfo = null;
      while (!stopRequested && frm.Focused) //run until stop of splashscreen is requested
      {
        if (oldInfo != info)
        {
          frm.SetInformation(info);
          oldInfo = info;
        }
        Thread.Sleep(25);
        //Application.DoEvents();
      }
      frm.FadeOut();
      frm.Close(); //closes, and disposes the form
      frm = null;
    }

    /// <summary>
    /// handles the fullscreen splash
    /// </summary>
    private void ShowFullScreenSplashScreen()
    {
      frmFull = new FullScreenSplashScreen();
      Cursor.Hide();

      //frmFull.pbBackground.Image = new System.Drawing.Bitmap(GetBackgroundImagePath());
      frmFull.RetrieveSplashScreenInfo();

      frmFull.Left = (Screen.PrimaryScreen.Bounds.Width/2 - frmFull.Width/2) + 1;
      frmFull.Top = (Screen.PrimaryScreen.Bounds.Height/2 - frmFull.Height/2) + 1;

      frmFull.lblMain.Parent = frmFull.pbBackground;
      frmFull.lblVersion.Parent = frmFull.lblMain;
      frmFull.lblCVS.Parent = frmFull.lblMain;

      frmFull.SetVersion(Version);
      frmFull.Show();

      frmFull.Update();
      //frmFull.FadeIn(); // remarked because without it the start looks faster (more powerful and responding)
      frmFull.Opacity = 100;

      string oldInfo = null;
      bool delayedStopAllowed = false;
      int stopRequestTime = 0;
      while (!delayedStopAllowed && (frmFull.Focused || OutDatedSkinForm != null))
        //run until stop of splashscreen is requested
      {
        if (stopRequested && stopRequestTime == 0) // store the current time when stop of the splashscreen is requested
        {
          stopRequestTime = Environment.TickCount;
          frmFull.TopMost = false; // allow the splashscreen to be overlayed by other windows (like the mp main screen)
        }
        if (AllowWindowOverlayRequested == true && OutDatedSkinForm != null)
          // Allow other Windows to Overlay the splashscreen
        {
          if (OutDatedSkinForm.Visible) // prepare everything to let the Outdated skin message appear
          {
            if (frmFull.Focused)
            {
              frmFull.TopMost = false;
              OutDatedSkinForm.TopMost = true;
              OutDatedSkinForm.BringToFront();
              Cursor.Show();
            }
          }
          else
          {
            AllowWindowOverlayRequested = false;
            frmFull.TopMost = true;
            frmFull.BringToFront();
            Cursor.Hide();
          }
        }
        if ((stopRequestTime != 0) && ((Environment.TickCount - 5000) > stopRequestTime))
        {
          delayedStopAllowed = true; // if stop is requested for more than 5sec ... leave the loop
        }

        if (oldInfo != info)
        {
          frmFull.SetInformation(info);
          oldInfo = info;
        }
        Thread.Sleep(25);
      }

      Cursor.Show();
      frmFull.Close();
      frmFull = null;
    }


    /// <summary>
    /// Summary description for SplashScreen.
    /// </summary>
    private class SplashForm : MPForm
    {
      private Panel panel1;
      private Label versionLabel;
      private Label cvsLabel;
      private Label informationLabel;
      private PictureBox pictureBox1;

      /// <summary>
      /// Required designer variable.
      /// </summary>
      private Container components = null;

      public SplashForm()
      {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
      }

      public void SetInformation(string information)
      {
        informationLabel.Text = information;
        Update();
      }

      public void SetVersion(string version)
      {
        string[] strVersion = version.Split('-');
        versionLabel.Text = strVersion[0];
        Log.Info("Version: Application {0}", strVersion[0]);
        if (strVersion.Length > 1)
        {
          string day = strVersion[2].Substring(0, 2);
          string month = strVersion[2].Substring(3, 2);
          string year = strVersion[2].Substring(6, 4);
          string time = strVersion[3].Substring(0, 5);
          string build = strVersion[4].Substring(0, 13).Trim();
          cvsLabel.Text = string.Format("{0} {1} ({2}-{3}-{4} / {5} CET)", strVersion[1], build, year, month, day, time);
          Log.Info("Version: {0}", cvsLabel.Text);
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
          if (components != null)
          {
            components.Dispose();
          }
        }
        base.Dispose(disposing);
      }

      public void FadeIn()
      {
        while (Opacity <= 0.9)
        {
          Opacity += 0.02;
          Thread.Sleep(15);
        }
      }

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
        this.panel1 = new System.Windows.Forms.Panel();
        this.informationLabel = new System.Windows.Forms.Label();
        this.versionLabel = new System.Windows.Forms.Label();
        this.cvsLabel = new System.Windows.Forms.Label();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // panel1
        // 
        this.panel1.BackColor = System.Drawing.Color.White;
        this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.panel1.Controls.Add(this.informationLabel);
        this.panel1.Controls.Add(this.versionLabel);
        this.panel1.Controls.Add(this.cvsLabel);
        this.panel1.Controls.Add(this.pictureBox1);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(0, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(390, 126);
        this.panel1.TabIndex = 0;
        // 
        // informationLabel
        // 
        this.informationLabel.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this.informationLabel.BackColor = System.Drawing.Color.Transparent;
        this.informationLabel.Font =
          new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point,
                                  ((byte) (0)));
        this.informationLabel.Location = new System.Drawing.Point(15, 99);
        this.informationLabel.Name = "informationLabel";
        this.informationLabel.Size = new System.Drawing.Size(358, 16);
        this.informationLabel.TabIndex = 4;
        this.informationLabel.Text = "Information";
        this.informationLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // versionLabel
        // 
        this.versionLabel.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this.versionLabel.BackColor = System.Drawing.Color.Transparent;
        this.versionLabel.Font =
          new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point,
                                  ((byte) (0)));
        this.versionLabel.Location = new System.Drawing.Point(267, 79);
        this.versionLabel.Name = "versionLabel";
        this.versionLabel.Size = new System.Drawing.Size(100, 16);
        this.versionLabel.TabIndex = 5;
        this.versionLabel.Text = "";
        this.versionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // cvsLabel
        // 
        this.cvsLabel.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this.cvsLabel.BackColor = System.Drawing.Color.Transparent;
        this.cvsLabel.Font =
          new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point,
                                  ((byte) (0)));
        this.cvsLabel.Location = new System.Drawing.Point(24, 79);
        this.cvsLabel.Name = "cvsLabel";
        this.cvsLabel.Size = new System.Drawing.Size(200, 16);
        this.cvsLabel.TabIndex = 5;
        this.cvsLabel.Text = "";
        this.cvsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        // 
        // pictureBox1
        // 
        this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
        this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        this.pictureBox1.Image = global::MediaPortal.Properties.Resources.mplogo_new;
        this.pictureBox1.Location = new System.Drawing.Point(16, 12);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(356, 65);
        this.pictureBox1.TabIndex = 3;
        this.pictureBox1.TabStop = false;
        // 
        // SplashScreen
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(390, 126);
        this.Controls.Add(this.panel1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.MaximizeBox = false;
        this.Name = "SplashScreen";
        this.Opacity = 0;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "SplashScreen";
        this.TopMost = true;
        this.panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
      }

      #endregion
    }
  }
}