//-----------------------------------------------------------------------------
// File: D3DApp.cs
//
// Desc: Application class for the Direct3D samples framework library.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal;
using MediaPortal.Util;

namespace D3D
{
  /// <summary>
  /// The base class for all the graphics (D3D) samples, it derives from windows forms
  /// </summary>
  public class D3DApp : System.Windows.Forms.Form
  {
    #region Menu Information
    // The menu items that *all* samples will need
    protected System.Windows.Forms.MainMenu mnuMain;
    protected System.Windows.Forms.MenuItem mnuFile;
    private System.Windows.Forms.MenuItem mnuBreak1;
    private System.Windows.Forms.MenuItem mnuBreak2;
    protected System.Windows.Forms.MenuItem mnuExit;
    #endregion

    // The window we will render too
    private System.Windows.Forms.Control ourRenderTarget;
    // Should we use the default windows
    protected bool isUsingMenus = true;

    // We need to keep track of our enumeration settings
    private bool isMaximized = false; // Are we maximized?
    private bool isHandlingSizeChanges = true; // Are we handling size changes?
    protected bool m_bResized=false;

    // Internal variables for the state of the app
    protected bool windowed;
    protected bool active;
    protected bool ready;
    protected bool hasFocus;
    protected bool isMultiThreaded = true;
    protected bool      m_bAutoHideMouse=true;
    protected DateTime m_MouseTimeOut;

    protected System.Drawing.Size storedSize;
    private System.Windows.Forms.MenuItem mnuSetup;
    protected System.Drawing.Point storedLocation;
    protected Rectangle   oldBounds;

    // Overridable functions for the 3D scene created by the app
    protected virtual void Initialize() { /* Do Nothing */ }
    protected virtual void InitializeDeviceObjects() { /* Do Nothing */ }
    protected virtual void OnDeviceReset(System.Object sender, System.EventArgs e) { /* Do Nothing */ }
    protected virtual void FrameMove() { /* Do Nothing */ }
    protected virtual void Render() { /* Do Nothing */ }
    protected virtual void OnDeviceLost(System.Object sender, System.EventArgs e) { /* Do Nothing */ }
    protected virtual void OnDeviceDisposing(System.Object sender, System.EventArgs e) { /* Do Nothing */ }

    protected virtual void OnStartup() {}
    protected virtual void OnExit() {}



    /// <summary>
    /// Constructor
    /// </summary>
    public D3DApp()
    {
      active = false;
      ready = false;
      hasFocus = false;
      

      ourRenderTarget = this;

      this.Text = "D3D9 Sample";
      this.ClientSize = new System.Drawing.Size(720,576);
      this.KeyPreview = true;

      
      //      startFullscreen=true;
      // When clipCursorWhenFullscreen is TRUE, the cursor is limited to
      // the device window when the app goes fullscreen.  This prevents users
      // from accidentally clicking outside the app window on a multimon system.
      // This flag is turned off by default for debug builds, since it makes 

      InitializeComponent();
      SetStyle(ControlStyles.AllPaintingInWmPaint | 
        ControlStyles.UserPaint | 
        ControlStyles.DoubleBuffer,
        true);    

    }




    /// <summary>
    /// Picks the best graphics device, and initializes it
    /// </summary>
    /// <returns>true if a good device was found, false otherwise</returns>
    public bool CreateGraphicsSample()
    {
        ready = true;
        return true;
    }



    /// <summary>
    /// Run the simulation
    /// </summary>
    /// 
    public void PreRun()
    {
      System.Windows.Forms.Control mainWindow = this;
      mainWindow.Show();	
      Initialize() ;

      storedSize=this.ClientSize;
      storedLocation=this.Location ;
      oldBounds=new Rectangle(Bounds.X,Bounds.Y,Bounds.Width,Bounds.Height);
      AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml");
      bool bStartFull=xmlreader.GetValueAsBool("general","startfullscreen",false);
      if (bStartFull)
      {
        Log.Write("start fullscreen");
        //Win32API.EnableStartBar(false);
        //Win32API.ShowStartBar(false);
        //this.SendToBack();
        //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.FormBorderStyle=FormBorderStyle.None;
        this.MaximizeBox=false;
        this.MinimizeBox=false;
        this.Menu=null;
        this.Location= new System.Drawing.Point(0,0);
        this.Bounds=Screen.PrimaryScreen.Bounds;
        this.ClientSize = new Size(Bounds.Width, Bounds.Height);
				isMaximized=true;
      }
      else
      {
        
        this.WindowState = FormWindowState.Normal;
        this.FormBorderStyle=FormBorderStyle.Sizable;
        this.MaximizeBox=true;
        this.MinimizeBox=true;          
        this.Menu=mnuMain;
        this.Location = storedLocation;
				isMaximized=false;
      }
      // The app is ready to go
      OnDeviceLost(null,null);
      InitializeDeviceObjects();
      OnDeviceReset(null, null);

      OnStartup();
    }

    public void Run()
    {
      // Now we're ready to recieve and process Windows messages.
      System.Windows.Forms.Control mainWindow = this;

      // If the render target is a form and *not* this form, use that form instead,
      // otherwise, use the main form.
      if ((ourRenderTarget is System.Windows.Forms.Form) && (ourRenderTarget != this))
        mainWindow = ourRenderTarget;

      mainWindow.Show();	
      Initialize() ;

      storedSize=this.ClientSize;
      storedLocation=this.Location ;
      oldBounds=new Rectangle(Bounds.X,Bounds.Y,Bounds.Width,Bounds.Height);
      AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml");
      bool bStartFull=xmlreader.GetValueAsBool("general","startfullscreen",false);
      if (bStartFull)
      {
        Log.Write("start fullscreen");
        //Win32API.EnableStartBar(false);
        //Win32API.ShowStartBar(false);
        //this.SendToBack();
        //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.FormBorderStyle=FormBorderStyle.None;
        this.MaximizeBox=false;
        this.MinimizeBox=false;
        this.Menu=null;
        this.Location= new System.Drawing.Point(0,0);
        this.Bounds=Screen.PrimaryScreen.Bounds;
        this.ClientSize = new Size(Bounds.Width, Bounds.Height);
      }
      else
      {
        this.WindowState = FormWindowState.Normal;
        this.FormBorderStyle=FormBorderStyle.Sizable;
        this.MaximizeBox=true;
        this.MinimizeBox=true;          
        this.Menu=mnuMain;
        this.Location = storedLocation;
      }

      OnStartup();
      
      while (mainWindow.Created)
      {
        System.Windows.Forms.Application.DoEvents();
        System.Threading.Thread.Sleep(20);
        Invalidate();
      }
      OnExit();
    }





    public void OnSetup(object sender, EventArgs e)
    {
      bool bAutoHide=m_bAutoHideMouse;
      m_bAutoHideMouse=false;
      m_MouseTimeOut=DateTime.Now;
      Cursor.Show();
      SetupForm dlg = new SetupForm();
      Cursor.Show();
      dlg.ShowDialog(this);
      Cursor.Show();
      m_MouseTimeOut=DateTime.Now;
      m_bAutoHideMouse=bAutoHide;
    }


    #region WinForms Overrides
    /// <summary>
    /// Clean up any resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      //Win32API.EnableStartBar(true);
      //Win32API.ShowStartBar(true);
      mnuMain.Dispose();
      base.Dispose(disposing);
    }




    /// <summary>
    /// Handle any key presses
    /// </summary>
    protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
    {
			
      /*
            // Check for our shortcut keys (Escape to quit)
            if ((byte)e.KeyChar == (byte)(int)System.Windows.Forms.Keys.Escape)
            {
              mnuExit.PerformClick();
              e.Handled = true;
            }*/

      // Allow the control to handle the keystroke now
      base.OnKeyPress(e);
    }




    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }


    private void D3DApp_Click(object sender, MouseEventArgs  e)
    {
      mouseclick(e);
    }

    private void D3DApp_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      mousemove(e);
    }


    /// <summary>
    /// Handle system keystrokes (ie, alt-enter)
    /// </summary>
    protected void OnKeyDown(object sender,System.Windows.Forms.KeyEventArgs e)
    {
      if ((e.Alt) && (e.KeyCode == System.Windows.Forms.Keys.Return))
      {
        isMaximized=!isMaximized;
        if (isMaximized)
        {
          Log.Write("windowed->fullscreen");
          //Win32API.EnableStartBar(false);
          //Win32API.ShowStartBar(false);
          //this.SendToBack();
          //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
          this.FormBorderStyle=FormBorderStyle.None;
          this.MaximizeBox=false;
          this.MinimizeBox=false;
          this.Menu=null;
          this.Location= new System.Drawing.Point(0,0);
          this.Bounds=new Rectangle(0,0,Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
          this.ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
          
          //this.BringToFront();

          m_bResized=true;
          isMaximized=true;
          Log.Write("windowed->fullscreen done {0}", isMaximized);
        }
        else
        {
          Log.Write("fullscreen->windowed");
          //Win32API.EnableStartBar(true);
          //Win32API.ShowStartBar(true);
          this.WindowState = FormWindowState.Normal;
          this.FormBorderStyle=FormBorderStyle.Sizable;
          this.MaximizeBox=true;
          this.MinimizeBox=true;          
          this.Menu=mnuMain;
          this.Location = storedLocation;
          Bounds=new Rectangle(oldBounds.X,oldBounds.Y,oldBounds.Width,oldBounds.Height);
          this.ClientSize = storedSize;
          m_bResized=true;
          Log.Write("fullscreen->windowed done {0}", isMaximized);
        }
        OnDeviceDisposing(null,null);
        OnDeviceReset(null,null);
        e.Handled=true;
        return;
        /*
        // Toggle the fullscreen/window mode
        if (active && ready)
        {

          try
          {
            ToggleFullscreen();                    
            return;
          }
          catch
          {
            HandleSampleException(new ResetFailedException(), ApplicationMessage.ApplicationMustExit);
          }
          finally
          {
            e.Handled = true;
          }
        }*/
      }
      else if (e.KeyCode == System.Windows.Forms.Keys.F2)
      {
        OnSetup(null,null);
      }

      if (e.Handled==false)
      {
        keydown( e);
      }
    }




    /// <summary>
    /// Winforms generated code for initializing the form
    /// </summary>
    private void InitializeComponent()
    {
      this.mnuMain = new System.Windows.Forms.MainMenu();
      this.mnuFile = new System.Windows.Forms.MenuItem();
      this.mnuSetup = new System.Windows.Forms.MenuItem();
      this.mnuBreak1 = new System.Windows.Forms.MenuItem();
      this.mnuBreak2 = new System.Windows.Forms.MenuItem();
      this.mnuExit = new System.Windows.Forms.MenuItem();
      // 
      // mnuMain
      // 
      this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                            this.mnuFile});
      // 
      // mnuFile
      // 
      this.mnuFile.Index = 0;
      this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                            this.mnuSetup,
                                                                            this.mnuBreak1,
                                                                            this.mnuBreak2,
                                                                            this.mnuExit});
      this.mnuFile.Text = "&File";
      // 
      // mnuSetup
      // 
      this.mnuSetup.Index = 0;
      this.mnuSetup.Text = "&Setup...";
      this.mnuSetup.Click += new System.EventHandler(this.OnSetup);
      // 
      // mnuBreak1
      // 
      this.mnuBreak1.Index = 1;
      this.mnuBreak1.Text = "-";
      // 
      // mnuBreak2
      // 
      this.mnuBreak2.Index = 2;
      this.mnuBreak2.Text = "-";
      
      // 
      // D3DApp
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(292, 273);
      this.MinimumSize = new System.Drawing.Size(100, 100);
      this.Name = "D3DApp";
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.D3DApp_Click);
      this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.D3DApp_MouseMove);

    }








    /// <summary>
    /// Make sure our graphics cursor (if available) moves with the cursor
    /// </summary>
    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
    {
      // Let the control handle the mouse now
      base.OnMouseMove(e);
    }




    /// <summary>
    /// Handle size changed events
    /// </summary>
    protected override void OnSizeChanged(System.EventArgs e)
    {
      this.OnResize(e);
      base.OnSizeChanged(e);
    }




    /// <summary>
    /// Handle resize events
    /// </summary>
    protected override void OnResize(System.EventArgs e)
    {
      if (isHandlingSizeChanges)
      {
        // Are we maximized?
        isMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
        if (!isMaximized)
        {
          //storedSize = this.ClientSize;
          // storedLocation = this.Location;
        }
      }
      active = !(this.WindowState == System.Windows.Forms.FormWindowState.Minimized || this.Visible == false);
      base.OnResize(e);
      m_bResized=true;
    }




    /// <summary>
    /// Once the form has focus again, we can continue to handle our resize
    /// and resets..
    /// </summary>
    protected override void OnGotFocus(System.EventArgs e)
    {
      isHandlingSizeChanges = true;
      base.OnGotFocus (e);
    }




    /// <summary>
    /// Handle move events
    /// </summary>
    protected override void OnMove(System.EventArgs e)
    {
      if (isHandlingSizeChanges)
      {
        //storedLocation = this.Location;
      }
      base.OnMove(e);
    }




    /// <summary>
    /// Handle closing event
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
    }
    #endregion
		

		
    protected virtual void keypressed(System.Windows.Forms.KeyPressEventArgs e)
    {
    }
    protected virtual void keydown( System.Windows.Forms.KeyEventArgs e)
    {
    }
    protected virtual void mousemove(System.Windows.Forms.MouseEventArgs e)
    {
		}
		protected virtual void mouseclick(MouseEventArgs e)
		{
      //this.Text=String.Format("show click");
    }

		private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			keypressed(e);
		}


	}

  #region Enums for D3D Applications
  /// <summary>
  /// Messages that can be used when displaying an error
  /// </summary>
  public enum ApplicationMessage 
  { 
    None, 
    ApplicationMustExit, 
    WarnSwitchToRef
  };
  #endregion




  #region Various SampleExceptions
  /// <summary>
  /// The default sample exception type
  /// </summary>
  public class SampleException : System.ApplicationException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message 
    { 
      get 
      { 
        string strMsg = string.Empty;

        strMsg = "Generic application error. Enable\n";
        strMsg += "debug output for detailed information.";

        return strMsg;
      } 
    }
  }




  /// <summary>
  /// Exception informing user no compatible devices were found
  /// </summary>
  public class NoCompatibleDevicesException : SampleException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        string strMsg = string.Empty;
        strMsg = "This sample cannot run in a desktop\n";
        strMsg += "window with the current display settings.\n";
        strMsg += "Please change your desktop settings to a\n";
        strMsg += "16- or 32-bit display mode and re-run this\n";
        strMsg += "sample.";

        return strMsg;
      }
    }
  }




  /// <summary>
  /// An exception for when the ReferenceDevice is null
  /// </summary>
  public class NullReferenceDeviceException : SampleException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        string strMsg = string.Empty;
        strMsg = "Warning: Nothing will be rendered.\n";
        strMsg += "The reference rendering device was selected, but your\n";
        strMsg += "computer only has a reduced-functionality reference device\n";
        strMsg += "installed.  Install the DirectX SDK to get the full\n";
        strMsg += "reference device.\n";

        return strMsg;
      }
    }
  }




  /// <summary>
  /// An exception for when reset fails
  /// </summary>
  public class ResetFailedException : SampleException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        string strMsg = string.Empty;
        strMsg = "Could not reset the Direct3D device.";

        return strMsg;
      }
    }
  }




  /// <summary>
  /// The exception thrown when media couldn't be found
  /// </summary>
  public class MediaNotFoundException : SampleException
  {
    private string mediaFile;
    public MediaNotFoundException(string filename) : base()
    {
      mediaFile = filename;
    }
    public MediaNotFoundException() : base()
    {
      mediaFile = string.Empty;
    }


    
    
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        string strMsg = string.Empty;
        strMsg = "Could not load required media.";
        if (mediaFile.Length > 0)
          strMsg += string.Format("\r\nFile: {0}", mediaFile);

        return strMsg;
      }
    }
  }
  #endregion



};