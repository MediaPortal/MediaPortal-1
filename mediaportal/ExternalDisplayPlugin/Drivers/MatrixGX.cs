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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Ripper;
using MediaPortal.TV.Recording;
using MediaPortal.UserInterface.Controls;
using LibDriverCoreClient;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// Matrix Orbital GX Typhoon Graphic LCD Driver
  /// </summary>
  /// <author>CybrMage</author>
  public class MatrixGX : BaseDisplay, IDisplay
  {
    private readonly SHA256Managed sha256 = new SHA256Managed(); //instance of crypto engine used to calculate hashes
    private byte[] lastHash; //hash of the last bitmap that was sent to the display
    private byte[] ReceivedBitmapData;
    private readonly string[] _Lines = new string[2];
    private bool _isDisabled = false;
    private string _errorMessage = "";
    private readonly MOGXDisplay MOD = new MOGXDisplay();
    private bool _IsOpen = false;
    private int _Gcols;
    private int _Grows;
    private static bool _useVolumeDisplay = false;
    private static bool _useProgressDisplay = false;
    private static bool _useInvertedDisplay = false;
    private static bool _useIcons = false;
    private static bool _useDiskIconForAllMedia = false;
    private int _BacklightRED;
    private int _BacklightGREEN;
    private int _BacklightBLUE;
    public static bool _mpIsIdle;

    public MatrixGX()
    {
      try {}
      catch (Exception ex)
      {
        _isDisabled = true;
        _errorMessage = ex.Message;
      }
    }

    #region IDisplay Members

    public bool IsDisabled
    {
      get
      {
        // ensure that the required library files exist
        _isDisabled = false;
        int fMissing = 0;
        if (!File.Exists("LibDriverCoreClient.dll"))
        {
          fMissing += 1;
        }
        if (!File.Exists("log4net.dll"))
        {
          fMissing += 2;
        }
        if (!File.Exists("fastbitmap.dll"))
        {
          fMissing += 4;
        }
        if (fMissing > 0)
        {
          if ((fMissing & 1) > 0)
          {
            _errorMessage = "Required file \"LibDriverCoreClient.dll\" is not installed!\n";
            _isDisabled = true;
            Log.Info("IDisplay: MatrixGX.Setup() - Required file \"LibDriverCoreClient.dll\" is not installed!");
          }
          if ((fMissing & 2) > 0)
          {
            _errorMessage += "Required file \"log4net.dll\" is not installed!\n";
            _isDisabled = true;
            Log.Info("IDisplay: MatrixGX.Setup() - Required file \"log4net.dll\" is not installed!");
          }
          if ((fMissing & 4) > 0)
          {
            _errorMessage += "Required file \"fastbitmap.dll\" is not installed!\n";
            _isDisabled = true;
            Log.Info("IDisplay: MatrixGX.Setup() - Required file \"fastbitmap.dll\" is not installed!");
          }
        }
        return _isDisabled;
      }
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters) {}

    public void DrawImage(Bitmap bitmap)
    {
      if (!MOD.IsOpen)
      {
        return;
      }
      Log.Debug("IDisplay(API) MatrixGX.DrawImage() - called");
      if (bitmap == null)
      {
        Log.Debug("IDisplay(API) MatrixGX.DrawImage():  bitmap null");
        return;
      }
      BitmapData data =
        bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
      try
      {
        if (ReceivedBitmapData == null)
        {
          ReceivedBitmapData = new byte[data.Stride*_Grows];
        }
        Marshal.Copy(data.Scan0, ReceivedBitmapData, 0, ReceivedBitmapData.Length);
      }
      catch (Exception ex)
      {
        Log.Debug("IDisplay(API) MatrixGX.DrawImage(): caught exception - {0}", ex.ToString());
      }
      finally
      {
        bitmap.UnlockBits(data);
      }
      //Calculate its hash so we can compare it to the previous bitmap more efficiently
      byte[] hash = sha256.ComputeHash(ReceivedBitmapData);
      //Compare the new hash with the previous one to determine whether the new image is
      //equal to the one that is already shown.  If they are equal, then we are done
      if (ByteArray.AreEqual(hash, lastHash))
      {
        Log.Debug("IDisplay(API) MatrixGX.DrawImage() - completed - bitmap not changed");
        return;
      }
      lastHash = hash;
      MOD.SendImage(bitmap);
      Log.Debug("IDisplay(API) MatrixGX.DrawImage() - completed");
    }


    /// <summary>
    /// Displays the message on the indicated line
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      if (!MOD.IsOpen)
      {
        return;
      }
      Log.Debug("IDisplay(API) MatrixGX.SetLine() called for Line {0} msg: '{1}'", line.ToString(), message);
      _Lines[line] = message;
      if (_Lines[0].Trim() == "MediaPortal")
      {
        _mpIsIdle = true;
      }
      else
      {
        _mpIsIdle = false;
      }
      if (line == 1)
      {
        MOD.SendText(_Lines[0], _Lines[1]);
      }
      Log.Debug("IDisplay(API) iMONLCDg.SetLine() completed");
    }

    /// <summary>
    /// Short name of this display driver
    /// </summary>
    public string Name
    {
      get { return "MatrixGX"; }
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "Matrix Orbital GX Series LCD driver V1.0"; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return true; }
    }

    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      Form AdvSettings = new AdvancedSetupForm();
      AdvSettings.ShowDialog();
    }

    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="_port">The port the display is connected to</param>
    /// <param name="_lines">The number of lines in text mode</param>
    /// <param name="_cols">The number of columns in text mode</param>
    /// <param name="_delay">Communication delay in text mode</param>
    /// <param name="_linesG">The height in pixels in graphic mode</param>
    /// <param name="_colsG">The width in pixels in graphic mode</param>
    /// <param name="_delayG">Communication delay in graphic mode</param>
    /// <param name="_backLight">Backlight on?</param>
    /// <param name="_contrast">Contrast value</param>
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _contrast)
    {
      Log.SetLogLevel(MediaPortal.Services.Level.Debug);
      Log.Debug("IDisplay: MatrixGX.Setup() - called");
      // load the advanced settings
      try
      {
        AdvancedSettings AdvSettings = AdvancedSettings.Load();
        _useVolumeDisplay = AdvSettings.MOGX_VolumeDisplay;
        _useProgressDisplay = AdvSettings.MOGX_ProgressDisplay;
        _useInvertedDisplay = AdvSettings.MOGX_UseInvertedDisplay;
        _BacklightRED = AdvSettings.MOGX_BacklightRED;
        _BacklightGREEN = AdvSettings.MOGX_BacklightGREEN;
        _BacklightBLUE = AdvSettings.MOGX_BacklightBLUE;
        _useIcons = AdvSettings.MOGX_UseIcons;
        _useDiskIconForAllMedia = AdvSettings.MOGX_UseDiskIconForAllMedia;
        Log.Debug(
          "IDisplay: MatrixGX.Setup() - Advanced Options - Vol: {0}, Prog: {1}, Invert: {2}, Backlight: R{3} - G{4} - B{5}, Dicon: {6}, DFAM: {7}",
          _useVolumeDisplay.ToString(), _useProgressDisplay.ToString(), _useInvertedDisplay.ToString(),
          _BacklightRED.ToString(), _BacklightGREEN.ToString(), _BacklightBLUE.ToString(), _useIcons,
          _useDiskIconForAllMedia);
        // GX series display
        _Grows = _linesG;
        _Gcols = _colsG;
        if (_Gcols > 240)
        {
          Log.Info("IDisplay: MatrixGX.Setup() - Invalid Graphics Columns value");
          _Grows = 240;
        }
        if (_Grows > 64)
        {
          Log.Info("IDisplay: MatrixGX.Setup() - Invalid Graphics Lines value");
          _Grows = 64;
        }
        _IsOpen = MOD.OpenDisplay(_BacklightRED, _BacklightGREEN, _BacklightBLUE, _useInvertedDisplay);
        if (_IsOpen)
        {
          Log.Debug("IDisplay: MatrixGX.Setup() - Display opened.");
          _isDisabled = false;
        }
        else
        {
          Log.Debug("IDisplay: MatrixGX.Setup() - Unable to open device - display disabled");
          _isDisabled = true;
          _errorMessage = "MatrixGX.setup() failed... No Matrix GX display found";
        }
      }
      catch (Exception ex)
      {
        Log.Debug("IDisplay: MatrixGX.Setup() - threw an exception: {0}", ex.ToString());
        _isDisabled = true;
        _errorMessage = "MatrixGX.setup() failed... Did you copy the required files to the MediaPortal directory?";
      }
      Log.Debug("IDisplay: MatrixGX.Setup() - completed");
    }

    public void Initialize()
    {
      if (!MOD.IsOpen)
      {
        return;
      }
      Log.Debug("IDisplay: MatrixGX.Initialize() - called");
      Clear();
    }

    public void CleanUp()
    {
      if (!MOD.IsOpen)
      {
        return;
      }
      Log.Debug("IDisplay: MatrixGX.CleanUp() - called");
      MOD.ClearDisplay();
      MOD.CloseDisplay();
    }

    private void Clear()
    {
      if (!MOD.IsOpen)
      {
        return;
      }
      Log.Debug("IDisplay: MatrixGX.Clear() - called");
      MOD.ClearDisplay();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      Log.Debug("IDisplay: MatrixGX.Dispose() - called");
      //            MOD.CloseDisplay(_BackLightControl);
    }

    #endregion

    #region MOGXDisplay

    private class MOGXDisplay
    {
      private bool _isOpen = false;
      private bool _isClosing = false;
      // GX specific functions
      private DCClient GX_Client = null;
      private DCCClientDeviceList GX_Devices = null;
      private DCCSession GX_Session = null;
      private Bitmap GX_Surface = null;
      private Graphics GX_Graphics = null;
      private int _BacklightR;
      private int _BacklightG;
      private int _BacklightB;
      private bool stopDisplayUpdateThread = false;
      private Thread _displayThread;
      private static readonly object DWriteMutex = new object();

      public bool IsOpen
      {
        get { return _isOpen; }
      }

      public bool OpenDisplay(int _backlightR, int _backlightG, int _backlightB, bool _InvertDisplay)
      {
        Log.Debug("MatrixGX.MOGXDisplay.OpenDisplay() - called");
        try
        {
          // set the advanced configuration parameters
          _BacklightR = _backlightR;
          _BacklightG = _backlightG;
          _BacklightB = _backlightB;
          // connect to DriverCore
          GX_Client = new DCClient();
          GX_Client.Connect();
          GX_Devices = GX_Client.Devices;
          if (GX_Devices.Count() > 0)
          {
            Log.Debug("MatrixGX.MOGXDisplay.OpenDisplay() - Found a GX series device");
            // there is a device available
            GX_Session = GX_Devices[0].CreateSession("MediaPortal");
            GX_Session.CreateGraphics(out GX_Surface);
            GX_Graphics = Graphics.FromImage(GX_Surface);
            _isOpen = true;
            _isClosing = false;
            _BacklightR = _backlightR;
            _BacklightG = _backlightG;
            _BacklightB = _backlightB;
            Log.Debug("MatrixGX.MOGXDisplay.OpenDisplay() - Display Opened");
            //set the backlight
            BacklightOn();
            //invert the display if needed
            GX_Session.SetOptions(true, false);
            if (_InvertDisplay)
            {
              GX_Session.SetOptions(true, true);
            }
            _displayThread = new Thread(this.DisplayUpdate);
            _displayThread.IsBackground = true;
            _displayThread.Priority = ThreadPriority.BelowNormal;
            _displayThread.Name = "DisplayUpdateThread";
            _displayThread.Start();
            if (_displayThread.IsAlive)
            {
              Log.Debug("MatrixGX.DisplayUpdate() Thread Started");
            }
            else
            {
              Log.Debug("MatrixGX.DisplayUpdate() FAILED TO START");
              CloseDisplay();
            }
          }
          else
          {
            Log.Debug("MatrixGX.MOGXDisplay.OpenDisplay() - No GX Series Display found");
            _isOpen = false;
          }
        }
        catch (Exception ex)
        {
          Log.Debug("MatrixGX.MOGXDisplay.OpenDisplay() - Display not opened - caught exception {0}", ex.ToString());
          Log.Error(ex);
          _isOpen = false;
        }
        Log.Debug("MatrixGX.MOGXDisplay.OpenDisplay() - Completed");
        return _isOpen;
      }

      public void CloseDisplay()
      {
        Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() - called");
        try
        {
          _isClosing = true;
          lock (DWriteMutex)
          {
            GX_Graphics.Dispose();
            GX_Session.End();
            GX_Session = null;
            GX_Client.Disconnect();
            GX_Client = null;
            Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() Stopping DisplayUpdate() Thread");
            stopDisplayUpdateThread = true;
            Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() DisplayUpdate() Thread Stopped");
            while (_displayThread.IsAlive)
            {
              Thread.Sleep(100);
            }
            _isOpen = false;
          }
          Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() - Display closed.");
        }
        catch (Exception ex)
        {
          Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() - caught exception on display close: {0}", ex.ToString());
          Log.Error(ex);
          _isOpen = false;
        }
      }

      public void SendImage(Bitmap _Bitmap)
      {
        //                RectangleF gx_bounds = GetTextBounds();
        //                GX_Graphics.DrawImageUnscaledAndClipped(_Bitmap,gx_bounds);
        if (_isClosing)
        {
          return;
        }
        lock (DWriteMutex)
        {
          Log.Debug("MatrixGX.MOGXDisplay.SendImage() - called");
          // draw the bitmap onto the display surface
          if ((_Bitmap.Height == GX_Surface.Height) & (_Bitmap.Width == GX_Surface.Width))
            //                    if ((_Bitmap.Height == gx_bounds.Height) & (_Bitmap.Width == gx_bounds.Width))
          {
            GX_Surface = (Bitmap) _Bitmap.Clone();
          }
          else
          {
            int bHeight = Math.Min(_Bitmap.Height, GX_Surface.Height);
            int bWidth = Math.Min(_Bitmap.Width, GX_Surface.Width);
            GX_Surface = _Bitmap.Clone(new Rectangle(0, 0, bWidth, bHeight), PixelFormat.Format1bppIndexed);
            //send the surface to the display
          }
          // we are done... the image is sent to the device by the DisplayUpdate thread
          //                Log.Debug("MatrixGX.MOGXDisplay.SendImage() - Sending Image to device");
          //                GX_Session.SendAsyncFrame(GX_Surface);
        }
        Log.Debug("MatrixGX.MOGXDisplay.SendImage() - completed");
      }

      public void ClearDisplay()
      {
        if (!_isOpen || _isClosing)
        {
          return;
        }
        lock (DWriteMutex)
        {
          Log.Debug("MatrixGX.MOGXDisplay.ClearDisplay() - called");
          GX_Graphics = Graphics.FromImage(GX_Surface);
          GX_Graphics.Clear(Color.White);
          Log.Debug("MatrixGX.MOGXDisplay.ClearDisplay() - Sending blank image to device");
          GX_Session.SendAsyncFrame(GX_Surface);
        }
        Log.Debug("MatrixGX.MOGXDisplay.ClearDisplay() - completed");
      }

      private RectangleF GetTextBounds() // must be called with DWriteMutex LOCKED
      {
        GraphicsUnit gx_format = GraphicsUnit.Pixel;
        RectangleF gx_bounds = GX_Surface.GetBounds(ref gx_format);
        if (_useVolumeDisplay)
        {
          gx_bounds.Offset(0, 8);
          gx_bounds.Height -= 8;
        }
        if (_useProgressDisplay)
        {
          gx_bounds.Height -= 8;
        }
        if (_useIcons)
        {
          gx_bounds.Width -= 32;
        }
        return gx_bounds;
      }

      public void SendText(string _line1, string _line2)
      {
        // GX series display
        // setup the graphics surface
        //constrain the text display
        RectangleF gx_bounds = GetTextBounds();

        if (_isClosing)
        {
          return;
        }
        lock (DWriteMutex)
        {
          Log.Debug("MatrixGX.MOGXDisplay.SendText() - called");
          GX_Graphics = Graphics.FromImage(GX_Surface);
          GX_Graphics.SmoothingMode = SmoothingMode.None;
          GX_Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
          GX_Graphics.Clear(Color.White);
          //              GX_Graphics.DrawLine(Pens.Black, 0, 0, GX_Surface.Width, GX_Surface.Height); 
          Font useFont = new Font(Settings.Instance.Font, Settings.Instance.FontSize);
          // draw the text into the bounded image
          int sLength = _line1.Length;
          while (GX_Graphics.MeasureString(_line1.Substring(0, sLength), useFont).Width > gx_bounds.Width)
          {
            sLength--;
          }
          GX_Graphics.DrawString(_line1.Substring(0, sLength), useFont, Brushes.Black, gx_bounds);
          //                GX_Graphics.DrawString(_line1, useFont, Brushes.Black, 0, (float)offset);
          gx_bounds.Offset(0, (useFont.GetHeight() + 1));
          gx_bounds.Height -= (useFont.GetHeight() + 1);
          //                int startY = (int)useFont.GetHeight() + 1;
          //                    GX_Graphics.DrawString(_line2, useFont, Brushes.Black, 0, (float)startY + (float)offset);
          sLength = _line2.Length;
          while (GX_Graphics.MeasureString(_line2.Substring(0, sLength), useFont).Width > gx_bounds.Width)
          {
            sLength--;
          }
          GX_Graphics.DrawString(_line2.Substring(0, sLength), useFont, Brushes.Black, gx_bounds);
        }
        // we are done - display is updated by the DisplayUpdate thread
        // draw the icons if needed
        //                DrawProgressBars();
        //                DrawDiskIcon();
        //                DrawIcons();
        //                DrawLargeIcons();

        // send the graphics to the display
        //                Log.Debug("MatrixGX.MOGXDisplay.SendText() - sending image to display");
        //                GX_Session.SendAsyncFrame(GX_Surface);
        Log.Debug("MatrixGX.MOGXDisplay.SendText() - completed");
      }

      private void ClearIconArea() // must be called with DWriteMutex LOCKED
      {
        if (!_useIcons || _isClosing)
        {
          return;
        }
        Log.Debug("MatrixGX.MOGXDisplay.ClearIconArea() - called");
        RectangleF gx_bounds = GetTextBounds();
        GX_Graphics.FillRectangle(Brushes.White, gx_bounds.Width, 0, GX_Surface.Width - gx_bounds.Width,
                                  GX_Surface.Height);
        Log.Debug("MatrixGX.MOGXDisplay.ClearIconArea() - completed");
      }

      private void DrawLargeIcons() // must be called with DWriteMutex LOCKED
      {
        if (!_useIcons || _isClosing)
        {
          return;
        }
        Log.Debug("MatrixGX.MOGXDisplay.DrawLargeIcons() - called");
        Log.Debug("MatrixGX.MOGXDisplay.DrawLargeIcons() - completed");
      }

      private void DrawIcons(uint ICON_STATUS) // must be called with DWriteMutex LOCKED
      {
        if (!_useIcons || _isClosing)
        {
          return;
        }
        Log.Debug("MatrixGX.MOGXDisplay.DrawIcons() - called");
        while (ICON_STATUS > 0)
        {
          switch (ICON_STATUS)
          {
            case (uint) IconType.ICON_Time:
              ICON_STATUS -= (uint) IconType.ICON_Time;
              break;
            case (uint) IconType.ICON_Rec:
              ICON_STATUS -= (uint) IconType.ICON_Rec;
              break;
            case (uint) IconType.ICON_CD:
              ICON_STATUS -= (uint) IconType.ICON_CD;
              break;
            case (uint) IconType.ICON_DVD:
              ICON_STATUS -= (uint) IconType.ICON_DVD;
              break;
            case (uint) IconType.ICON_TV:
              ICON_STATUS -= (uint) IconType.ICON_TV;
              break;
            case (uint) IconType.ICON_Music:
              ICON_STATUS -= (uint) IconType.ICON_Music;
              break;
            case (uint) IconType.ICON_WMA:
              ICON_STATUS -= (uint) IconType.ICON_WMA;
              break;
            case (uint) IconType.ICON_WAV:
              ICON_STATUS -= (uint) IconType.ICON_WAV;
              break;
            case (uint) IconType.ICON_OGG:
              ICON_STATUS -= (uint) IconType.ICON_OGG;
              break;
            case (uint) IconType.ICON_MP3:
              ICON_STATUS -= (uint) IconType.ICON_MP3;
              break;
            case (uint) IconType.ICON_Movie:
              ICON_STATUS -= (uint) IconType.ICON_Movie;
              break;
            case (uint) IconType.ICON_xVid:
              ICON_STATUS -= (uint) IconType.ICON_xVid;
              break;
            case (uint) IconType.ICON_WMV:
              ICON_STATUS -= (uint) IconType.ICON_WMV;
              break;
            case (uint) IconType.ICON_MPG:
              ICON_STATUS -= (uint) IconType.ICON_MPG;
              break;
            case (uint) IconType.ICON_DivX:
              ICON_STATUS -= (uint) IconType.ICON_DivX;
              break;
          }
          // draw the icon
        }
        Log.Debug("MatrixGX.MOGXDisplay.DrawIcons() - completed");
      }

      private void DrawDiskIcon(uint segments) // must be called with DWriteMutex LOCKED
      {
        if (!_useIcons || _isClosing)
        {
          return;
        }
        Log.Debug("MatrixGX.MOGXDisplay.DrawDiskIcon() - called");
        try
        {
          for (int i = 0; i < 8; i++)
          {
            if ((segments & (1 << i)) > 0)
            {
              GX_Graphics.FillPie(Brushes.Black, 223, 0, 16, 16, i*45, 45);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Debug("MatrixGX.MOGXDisplay.DrawDiskIcon() - caught exception {0}", ex.ToString());
        }

        Log.Debug("MatrixGX.MOGXDisplay.DrawDiskIcon() - completed");
      }

      private void DrawProgressBars() // must be called with DWriteMutex LOCKED
      {
        Log.Debug("MatrixGX.MOGXDisplay.DrawProgressBars() - called");
        if ((!_useProgressDisplay & !_useVolumeDisplay) || _isClosing)
        {
          return;
        }
        // simulate icons 

        RectangleF gx_bounds = GetTextBounds();
        int barWidth = (int) gx_bounds.Width - 1;
        if (g_Player.Playing & _useVolumeDisplay)
        {
          if (!VolumeHandler.Instance.IsMuted)
          {
            float factor = 65535/barWidth - 6;
            int volLevel = (int) ((DShowNET.AudioMixer.AudioMixerHelper.GetVolume()/factor) - 0.01) + 1;
            //                        volLevel = (DShowNET.AudioMixer.AudioMixerHelper.GetVolume() / 324);
            // draw the Volume bars onto the graphics
            GX_Graphics.FillRectangle(Brushes.Black, 0, 0, 3, 5);
            GX_Graphics.FillRectangle(Brushes.Black, barWidth - 2, 0, 3, 5);
            GX_Graphics.DrawLine(Pens.Black, 0, 2, barWidth, 2);
            GX_Graphics.FillRectangle(Brushes.Black, 3, 0, volLevel, 5);
          }
        }
        if (g_Player.Playing & _useProgressDisplay)
        {
          int progLevel = (int) ((((float) g_Player.CurrentPosition/(float) g_Player.Duration) - 0.01)*(barWidth - 6)) +
                          1;
          // draw the progress bars onto the graphics
          GX_Graphics.FillRectangle(Brushes.Black, 0, 59, 3, 5);
          GX_Graphics.FillRectangle(Brushes.Black, barWidth - 2, 59, 3, 5);
          GX_Graphics.DrawLine(Pens.Black, 0, 61, barWidth, 61);
          GX_Graphics.FillRectangle(Brushes.Black, 3, 59, progLevel, 5);
        }
        Log.Debug("MatrixGX.MOGXDisplay.DrawProgressBars() - completed");
      }

      private enum LargeIconType
      {
        IDLE = 0,
        TV = 1,
        MOVIE = 2,
        MUSIC = 3,
        VIDEO = 4,
        RECORDING = 5,
        PAUSED = 6
        //zSPARE2 = 7,
        //zSPARE3 = 8,
        //zSPARE4 = 9
      }

      public class DiskIcon
      {
        private readonly uint[] _DiskMask = {0xFE, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x7F};
        private readonly uint[] _DiskMaskInv = {0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80};
        private readonly uint _diskSolidOnMask = 0xFF;
        private readonly uint _diskSolidOffMask = 0x00;
        private bool _diskOn = false;
        private bool _diskFlash = false;
        private bool _diskRotate = false;
        private int _flashState = 1;
        private int _diskSegment = 0;
        private bool _diskRotateClockwise = true;
        private bool _diskInverted = false;
        private bool _diskSRWFlash = true;

        public uint Mask
        {
          get
          {
            //                Log.Debug("flashing: {0} FLASH : {1} Rotate: {2}",_diskFlash, _flashState.ToString(),_diskRotate );
            if (!_diskOn)
            {
              // disk is off
              return _diskSolidOffMask;
            }
            else
            {
              // disk is on
              if (!_diskRotate)
              {
                // disk is on and not rotating
                if (!_diskFlash)
                {
                  // disk is on and not flashing
                  return _diskSolidOnMask;
                }
                else
                {
                  // disk is on solid and flashing
                  if (_flashState == 1)
                  {
                    // disk flash state is on
                    return _diskSolidOnMask;
                  }
                  else
                  {
                    // disk flash state if off
                    return _diskSolidOffMask;
                  }
                }
              }
              else
              {
                if (!_diskFlash)
                {
                  // disk is on and rotating and not flashing
                  if (!_diskInverted)
                  {
                    return _DiskMask[_diskSegment];
                  }
                  else
                  {
                    return _DiskMaskInv[_diskSegment];
                  }
                }
                else
                {
                  // disk is on and rotating and flashing
                  if (_flashState > 0)
                  {
                    // disk flash state is on
                    if (!_diskInverted)
                    {
                      return _DiskMask[_diskSegment];
                    }
                    else
                    {
                      return _DiskMaskInv[_diskSegment];
                    }
                  }
                  else
                  {
                    // disk flash state is off
                    return _diskSolidOffMask;
                  }
                }
              }
            }
          }
        }

        public void On()
        {
          _diskOn = true;
        }

        public void Off()
        {
          _diskOn = false;
        }

        //
        public void InvertOn()
        {
          _diskInverted = true;
        }

        public void InvertOff()
        {
          _diskInverted = false;
        }

        //
        public void FlashOn()
        {
          _diskFlash = true;
        }

        public void FlashOff()
        {
          _diskFlash = false;
          _flashState = 1;
        }

        //
        public void RotateCW()
        {
          _diskRotateClockwise = true;
          _diskRotate = true;
        }

        public void RotateCCW()
        {
          _diskRotateClockwise = false;
          _diskRotate = true;
        }

        //
        public void Reset()
        {
          _diskFlash = false;
          _diskRotate = false;
          _diskSegment = 0;
          _diskRotateClockwise = true;
          _diskOn = false;
          _flashState = 1;
          _diskInverted = false;
          _diskSRWFlash = true;
        }

        //
        //
        public void Animate()
        {
          if ((_diskRotate & !_diskFlash) || (_diskRotate & (_diskFlash & !_diskSRWFlash)))
          {
            if (_diskRotateClockwise)
            {
              _diskSegment++;
              if (_diskSegment > 7)
              {
                _diskSegment = 0;
              }
            }
            else
            {
              _diskSegment--;
              if (_diskSegment < 0)
              {
                _diskSegment = 7;
              }
            }
          }
          if (_diskFlash)
          {
            if (_flashState == 1)
            {
              _flashState = 0;
            }
            else
            {
              _flashState = 1;
            }
          }
        }

        #region Unused Methods

        //public void RotateOff()
        //{
        //  _diskRotateClockwise = false;
        //  _diskRotate = false;
        //}

        //public bool IsOn
        //{
        //  get { return _diskOn; }
        //}

        //public bool IsFlashing
        //{
        //  get { return _diskFlash; }
        //}

        //public bool IsInverted
        //{
        //  get { return _diskInverted; }
        //}

        //public bool IsRotating
        //{
        //  get { return _diskFlash; }
        //}
        ////

        //public void SRWFlashOn()
        //{
        //  _diskSRWFlash = true;
        //}

        //public void SRWFlashOff()
        //{
        //  _diskSRWFlash = false;
        //}

        #endregion

        //
      }

      // this method runs in a seperate thread, collects system statistics and updates the LCD
      // icons appropriately
      private void DisplayUpdate()
      {
        string currentItem;
        string[] temp;
        uint DISK_ICON_STATUS = 0;
        ;
        bool flashStatus = false;
        DiskIcon Disk = new DiskIcon();
        Log.Debug("iMONLCDg.DisplayUpdate() Starting Icon Update Thread");
        CDDrive CD = new CDDrive();
        bool TOC_Valid = false;
        char[] DriveLetter;
        int CurrentLargeIcon = 0;

        DriveLetter = CDDrive.GetCDDriveLetters();
        Log.Debug("MatrixGX.DisplayUpdate() Found {0} CD/DVD Drives.", DriveLetter.Length.ToString());
        if (DriveLetter.Length > 0)
        {
          CD.Open(DriveLetter[0]);
        }
        Disk.Reset();
        while (true)
        {
          if (stopDisplayUpdateThread)
          {
            Log.Debug("MatrixGX.DisplayUpdate() Icon Update Thread terminating");
            stopDisplayUpdateThread = false;
            return;
          }

          #region DisplayUpdate work section

          flashStatus = !flashStatus;
          int LastCurrentLargeIcon = CurrentLargeIcon;
          int NewCurrentLargeIcon = 0;
          uint ICON_STATUS = 0x0;
          Disk.Off();
          Disk.Animate();
          // check to see if the recording icon nedds to be turned on
          if (Recorder.IsAnyCardRecording())
          {
            ICON_STATUS |= (uint) IconType.ICON_Rec;
            NewCurrentLargeIcon = (int) LargeIconType.RECORDING;
          }
          if (g_Player.Playing)
          {
            // determine the type of file that is playing
            if ((g_Player.IsTV || g_Player.IsTVRecording) & !(g_Player.IsDVD || g_Player.IsCDA))
            {
              ICON_STATUS |= (uint) IconType.ICON_TV;
              NewCurrentLargeIcon = (int) LargeIconType.TV;
            }

            if (g_Player.IsDVD || g_Player.IsCDA)
            {
              if (g_Player.IsDVD & g_Player.IsVideo)
              {
                ICON_STATUS |= (uint) IconType.ICON_Movie;
                ICON_STATUS |= (uint) IconType.ICON_DVD;
                NewCurrentLargeIcon = (int) LargeIconType.MOVIE;
              }
              else if (g_Player.IsCDA & !g_Player.IsVideo)
              {
                ICON_STATUS |= (uint) IconType.ICON_Music;
                ICON_STATUS |= (uint) IconType.ICON_CD;
                NewCurrentLargeIcon = (int) LargeIconType.MUSIC;
              }
              Disk.On();
              Disk.InvertOff();
              if (g_Player.Playing & !g_Player.Paused)
              {
                if (g_Player.Speed > 0)
                {
                  Disk.RotateCW();
                }
                else if (g_Player.Speed < 0)
                {
                  Disk.RotateCCW();
                }
                Disk.FlashOff();
              }
              else
              {
                Disk.FlashOn();
                NewCurrentLargeIcon = (int) LargeIconType.PAUSED;
              }
            }

            if (g_Player.IsMusic)
            {
              ICON_STATUS |= (uint) IconType.ICON_Music;
              NewCurrentLargeIcon = (int) LargeIconType.MUSIC;
              Disk.On();
              Disk.InvertOn();
              if (g_Player.Playing & !g_Player.Paused)
              {
                if (g_Player.Speed > 0)
                {
                  Disk.RotateCW();
                }
                else if (g_Player.Speed < 0)
                {
                  Disk.RotateCCW();
                }
                Disk.FlashOff();
              }
              else
              {
                Disk.FlashOn();
                NewCurrentLargeIcon = (int) LargeIconType.PAUSED;
              }
              if (!_useDiskIconForAllMedia)
              {
                Disk.Off();
              }
              // determine the media type
              currentItem = GUIPropertyManager.GetProperty("#Play.Current.File");
              if (currentItem.Length > 0)
              {
                // there is a currently playing file - set appropriate icons
                temp = currentItem.Split('.');
                if (temp.Length > 1)
                {
                  switch (temp[1])
                  {
                    case "mp3":
                      ICON_STATUS |= (uint) IconType.ICON_MP3;
                      break;
                    case "ogg":
                      ICON_STATUS |= (uint) IconType.ICON_OGG;
                      break;
                    case "wma":
                      ICON_STATUS |= (uint) IconType.ICON_WMA;
                      break;
                    case "wav":
                      ICON_STATUS |= (uint) IconType.ICON_WAV;
                      break;
                  }
                }
              }
            }

            if (g_Player.IsVideo & !g_Player.IsDVD)
            {
              ICON_STATUS |= (uint) IconType.ICON_Movie;
              ICON_STATUS |= (uint) IconType.ICON_Music;
              NewCurrentLargeIcon = (int) LargeIconType.VIDEO;
              Disk.On();
              Disk.InvertOn();
              if (g_Player.Playing & !g_Player.Paused)
              {
                if (g_Player.Speed > 0)
                {
                  Disk.RotateCW();
                }
                else if (g_Player.Speed < 0)
                {
                  Disk.RotateCCW();
                }
                Disk.FlashOff();
              }
              else
              {
                Disk.FlashOn();
                NewCurrentLargeIcon = (int) LargeIconType.PAUSED;
              }
              if (!_useDiskIconForAllMedia)
              {
                Disk.Off();
              }
              // determine the media type
              currentItem = GUIPropertyManager.GetProperty("#Play.Current.File");
              Log.Debug("current file: {0}", currentItem);
              if (currentItem.Length > 0)
              {
                ICON_STATUS |= (uint) IconType.ICON_Music;
                // there is a currently playing file - set appropriate icons
                temp = currentItem.Split('.');
                if (temp.Length > 1)
                {
                  switch (temp[1].ToLower())
                  {
                    case "ifo":
                    case "vob":
                    case "mpg":
                      ICON_STATUS |= (uint) IconType.ICON_MPG;
                      break;
                    case "wmv":
                      ICON_STATUS |= (uint) IconType.ICON_WMV;
                      break;
                    case "divx":
                      ICON_STATUS |= (uint) IconType.ICON_DivX;
                      break;
                    case "xvid":
                      ICON_STATUS |= (uint) IconType.ICON_xVid;
                      break;
                  }
                }
              }
              //                      Log.Debug("ExternalDisplay.iMONLCDg.UpdateIcons: Setting MOVIE icon");
            }
          }
          if (!g_Player.Playing || !_useDiskIconForAllMedia)
          {
            DISK_ICON_STATUS = 0;
            if (CD.IsOpened)
            {
              if (CD.IsCDReady())
              {
                if (!TOC_Valid)
                {
                  TOC_Valid = CD.Refresh();
                }
                // there is a CD or DVD in the drive
                if (CD.GetNumAudioTracks() > 0)
                {
                  DISK_ICON_STATUS = ICON_CDIn;
                }
                else
                {
                  DISK_ICON_STATUS = ICON_DVDIn;
                }
              }
              else
              {
                TOC_Valid = false;
              }
            }
          }
          if (g_Player.Player == null)
          {
            if (_mpIsIdle)
            {
              // MediaPortal is in an idle state
              ICON_STATUS |= (uint) IconType.ICON_Time;
              NewCurrentLargeIcon = (int) LargeIconType.IDLE;
            }
          }
          //              if (flashStatus) ICON_STATUS |= ICON_SCR1;

          if (NewCurrentLargeIcon != LastCurrentLargeIcon)
          {
            CurrentLargeIcon = NewCurrentLargeIcon;
          }

          #region DisplayUpdate - Display data section

          // update the display
          lock (DWriteMutex)
          {
            Log.Debug("MatrixGX.DisplayUpdate() Building display image");
            if (_useProgressDisplay || _useVolumeDisplay || _useIcons)
            {
              ClearIconArea();
            }
            if (_useProgressDisplay || _useVolumeDisplay)
            {
              DrawProgressBars();
            }
            if (_useIcons)
            {
              if (!g_Player.Playing || !_useDiskIconForAllMedia)
              {
                DrawDiskIcon(DISK_ICON_STATUS);
              }
              else
              {
                DrawDiskIcon(Disk.Mask);
              }
              DrawIcons(ICON_STATUS);
              DrawLargeIcons();
            }
            Log.Debug("MatrixGX.DisplayUpdate() image built - sending to display");
            GX_Session.SendAsyncFrame(GX_Surface);
          }

          #endregion

          #endregion

          Thread.Sleep(500);
        }
      }

      #region Display Icons

      /// <summary>
      /// Display Icons bitmaps
      /// </summary>
      /// <returns>Int 64 with the icon bit set</returns>	
      //    public class Icons
      //    {
      private readonly uint ICON_CDIn = 0x6B;

      private readonly uint ICON_DVDIn = 0x55;

      private enum IconType : uint
      {
        ICON_WAV = 0x4000,
        ICON_Rec = 0x2000,
        ICON_Time = 0x1000,
        ICON_xVid = 0x0800,
        ICON_WMV = 0x0400,
        ICON_WMA = 0x0200,
        ICON_MP3 = 0x0100,
        ICON_OGG = 0x0080,
        ICON_MPG = 0x0040,
        ICON_DivX = 0x0020,
        ICON_Music = 0x0010,
        ICON_Movie = 0x0008,
        ICON_CD = 0x0004,
        ICON_DVD = 0x0002,
        ICON_TV = 0x0001
      }

      #endregion

      #region Unused Methods

      public void BacklightOn()
      {
        if (!_isOpen)
        {
          return;
        }
        lock (DWriteMutex)
        {
          GX_Session.SetRGBBacklight((byte) _BacklightR, (byte) _BacklightB, (byte) _BacklightG);
        }
      }

      #endregion
    }

    #endregion

    #region Advanced Settings class

    [Serializable]
    public class AdvancedSettings
    {
      #region Static Fields

      private static AdvancedSettings m_Instance; //Reference to the single instance of this task

      #endregion

      #region Fields

      /// <summary>
      /// String representation of the selected display type.  Used for (de)serializing
      /// </summary>
      private bool m_VolumeDisplay = false;

      private bool m_ProgressDisplay = false;
      private bool m_UseInvertedDisplay = false;
      private bool m_UseIcons = false;
      private bool m_UseDiskIconForAllMedia = false;
      private int m_backlightR;
      private int m_backlightG;
      private int m_backlightB;

      #endregion

      #region Properties

      /// <summary>
      /// Gets the single instance
      /// </summary>
      /// <value>The single instance of this class</value>
      public static AdvancedSettings Instance
      {
        get
        {
          if (m_Instance == null)
          {
            m_Instance = Load();
          }
          return m_Instance;
        }
      }

      [XmlAttribute]
      public bool MOGX_VolumeDisplay
      {
        get { return m_VolumeDisplay; }
        set { m_VolumeDisplay = value; }
      }

      [XmlAttribute]
      public bool MOGX_ProgressDisplay
      {
        get { return m_ProgressDisplay; }
        set { m_ProgressDisplay = value; }
      }

      [XmlAttribute]
      public bool MOGX_UseInvertedDisplay
      {
        get { return m_UseInvertedDisplay; }
        set { m_UseInvertedDisplay = value; }
      }

      [XmlAttribute]
      public int MOGX_BacklightRED
      {
        get { return m_backlightR; }
        set { m_backlightR = value; }
      }

      [XmlAttribute]
      public int MOGX_BacklightGREEN
      {
        get { return m_backlightG; }
        set { m_backlightG = value; }
      }

      [XmlAttribute]
      public int MOGX_BacklightBLUE
      {
        get { return m_backlightB; }
        set { m_backlightB = value; }
      }

      [XmlAttribute]
      public bool MOGX_UseIcons
      {
        get { return m_UseIcons; }
        set { m_UseIcons = value; }
      }

      [XmlAttribute]
      public bool MOGX_UseDiskIconForAllMedia
      {
        get { return m_UseDiskIconForAllMedia; }
        set { m_UseDiskIconForAllMedia = value; }
      }

      #endregion

      #region Functions

      /// <summary>
      /// Loads the settings from XML
      /// </summary>
      /// <returns>The loaded settings</returns>
      public static AdvancedSettings Load()
      {
        Log.Debug("IDisplay MatrixGX.AdvancedSettings.Load() started");
        AdvancedSettings Settings;
        if (File.Exists(Config.GetFile(Config.Dir.Config, "ExternalDisplay_MatrixGX.xml")))
        {
          Log.Debug("IDisplay MatrixGX.AdvancedSettings.Load() Loading settings from XML file");
          XmlSerializer ser = new XmlSerializer(typeof (AdvancedSettings));
          XmlTextReader rdr = new XmlTextReader(Config.GetFile(Config.Dir.Config, "ExternalDisplay_MatrixGX.xml"));
          Settings = (AdvancedSettings) ser.Deserialize(rdr);
          rdr.Close();
        }
        else
        {
          Log.Debug("IDisplay MatrixGX.AdvancedSettings.Load() Loading settings from defaults");
          Settings = new AdvancedSettings();
          Default(Settings);
        }
        Log.Debug("IDisplay MatrixGX.AdvancedSettings.Load() completed");
        return Settings;
      }

      /// <summary>
      /// Saves the settings to XML
      /// </summary>
      public static void Save()
      {
        Log.Debug("IDisplay MatrixGX.AdvancedSettings.Save() Saving settings to XML file");
        XmlSerializer ser = new XmlSerializer(typeof (AdvancedSettings));
        XmlTextWriter w =
          new XmlTextWriter(Config.GetFile(Config.Dir.Config, "ExternalDisplay_MatrixGX.xml"), Encoding.UTF8);
        w.Formatting = Formatting.Indented;
        w.Indentation = 2;
        ser.Serialize(w, Instance);
        w.Close();
        Log.Debug("IDisplay MatrixGX.AdvancedSettings.Save() completed");
      }

      /// <summary>
      /// Creates the default settings when config file cannot be found
      /// </summary>
      /// <param name="_settings"></param>
      private static void Default(AdvancedSettings _settings)
      {
        _settings.MOGX_ProgressDisplay = false;
        _settings.MOGX_VolumeDisplay = false;
        _settings.MOGX_UseInvertedDisplay = false;
        _settings.MOGX_UseIcons = false;
        _settings.MOGX_UseDiskIconForAllMedia = false;
        _settings.MOGX_BacklightRED = 255;
        _settings.MOGX_BacklightGREEN = 255;
        _settings.MOGX_BacklightBLUE = 255;
      }

      #endregion
    }

    #endregion

    #region AdvancedSetup form

    public class AdvancedSetupForm : Form
    {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private readonly IContainer components = null;

      private MPGroupBox groupBox1;
      private MPCheckBox mpVolumeDisplay;
      private MPCheckBox mpProgressBar;
      private MPButton btnOK;
      private MPCheckBox mpUseInvertedDisplay;
      private MPCheckBox mpUseIcons;
      private MPCheckBox mpUseDiskIconForAllMedia;
      private MPGroupBox groupBox2;
      private TrackBar tbBacklightRED;
      private TrackBar tbBacklightGREEN;
      private TrackBar tbBacklightBLUE;
      private MPLabel mpLabel1;
      private MPLabel mpLabel2;
      private MPLabel mpLabel3;

      public AdvancedSetupForm()
      {
        InitializeComponent();

        mpVolumeDisplay.DataBindings.Add("Checked", AdvancedSettings.Instance, "MOGX_VolumeDisplay");
        mpProgressBar.DataBindings.Add("Checked", AdvancedSettings.Instance, "MOGX_ProgressDisplay");
        mpUseInvertedDisplay.DataBindings.Add("Checked", AdvancedSettings.Instance, "MOGX_UseInvertedDisplay");
        mpUseIcons.DataBindings.Add("Checked", AdvancedSettings.Instance, "MOGX_UseIcons");
        mpUseDiskIconForAllMedia.DataBindings.Add("Checked", AdvancedSettings.Instance, "MOGX_UseDiskIconForAllMedia");
        tbBacklightRED.DataBindings.Add("Value", AdvancedSettings.Instance, "MOGX_BacklightRED");
        tbBacklightGREEN.DataBindings.Add("Value", AdvancedSettings.Instance, "MOGX_BacklightGREEN");
        tbBacklightBLUE.DataBindings.Add("Value", AdvancedSettings.Instance, "MOGX_BacklightBLUE");
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
        if (disposing && (components != null))
        {
          components.Dispose();
        }
        base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
        this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.mpVolumeDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpProgressBar = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseInvertedDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseDiskIconForAllMedia = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.tbBacklightRED = new System.Windows.Forms.TrackBar();
        this.tbBacklightGREEN = new System.Windows.Forms.TrackBar();
        this.tbBacklightBLUE = new System.Windows.Forms.TrackBar();
        this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
        this.groupBox1.SuspendLayout();
        this.groupBox2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize) (this.tbBacklightRED)).BeginInit();
        ((System.ComponentModel.ISupportInitialize) (this.tbBacklightGREEN)).BeginInit();
        ((System.ComponentModel.ISupportInitialize) (this.tbBacklightBLUE)).BeginInit();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
              | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox1.Controls.Add(this.groupBox2);
        this.groupBox1.Controls.Add(this.mpVolumeDisplay);
        this.groupBox1.Controls.Add(this.mpProgressBar);
        this.groupBox1.Controls.Add(this.mpUseInvertedDisplay);
        this.groupBox1.Controls.Add(this.mpUseIcons);
        this.groupBox1.Controls.Add(this.mpUseDiskIconForAllMedia);
        this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox1.Location = new System.Drawing.Point(9, 6);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(236, 283);
        this.groupBox1.TabIndex = 4;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Configuration";
        // 
        // mpVolumeDisplay
        // 
        this.mpVolumeDisplay.AutoSize = true;
        this.mpVolumeDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpVolumeDisplay.Location = new System.Drawing.Point(16, 25);
        this.mpVolumeDisplay.Name = "mpVolumeDisplay";
        this.mpVolumeDisplay.Size = new System.Drawing.Size(171, 17);
        this.mpVolumeDisplay.TabIndex = 6;
        this.mpVolumeDisplay.Text = "Show Volume display";
        this.mpVolumeDisplay.UseVisualStyleBackColor = true;
        // 
        // mpProgressBar
        // 
        this.mpProgressBar.AutoSize = true;
        this.mpProgressBar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpProgressBar.Location = new System.Drawing.Point(16, 48);
        this.mpProgressBar.Name = "mpProgressBar";
        this.mpProgressBar.Size = new System.Drawing.Size(193, 17);
        this.mpProgressBar.TabIndex = 7;
        this.mpProgressBar.Text = "Show Progress Display";
        this.mpProgressBar.UseVisualStyleBackColor = true;
        // 
        // mpUseInvertedDisplay
        // 
        this.mpUseInvertedDisplay.AutoSize = true;
        this.mpUseInvertedDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseInvertedDisplay.Location = new System.Drawing.Point(16, 71);
        this.mpUseInvertedDisplay.Name = "mpUseInvertedDisplay";
        this.mpUseInvertedDisplay.Size = new System.Drawing.Size(193, 17);
        this.mpUseInvertedDisplay.TabIndex = 8;
        this.mpUseInvertedDisplay.Text = "Use Inverted Display";
        this.mpUseInvertedDisplay.UseVisualStyleBackColor = true;
        // 
        // mpUseIcons
        // 
        this.mpUseIcons.AutoSize = true;
        this.mpUseIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseIcons.Location = new System.Drawing.Point(16, 94);
        this.mpUseIcons.Name = "mpUseInvertedDisplay";
        this.mpUseIcons.Size = new System.Drawing.Size(193, 17);
        this.mpUseIcons.TabIndex = 8;
        this.mpUseIcons.Text = "Use Icons";
        this.mpUseIcons.UseVisualStyleBackColor = true;
        // 
        // mpUseDiskIconForAllMedia
        // 
        this.mpUseDiskIconForAllMedia.AutoSize = true;
        this.mpUseDiskIconForAllMedia.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseDiskIconForAllMedia.Location = new System.Drawing.Point(16, 117);
        this.mpUseDiskIconForAllMedia.Name = "mpUseInvertedDisplay";
        this.mpUseDiskIconForAllMedia.Size = new System.Drawing.Size(193, 17);
        this.mpUseDiskIconForAllMedia.TabIndex = 8;
        this.mpUseDiskIconForAllMedia.Text = "Use Disk Icon For All Media";
        this.mpUseDiskIconForAllMedia.UseVisualStyleBackColor = true;
        // 
        // btnOK
        // 
        this.btnOK.Anchor =
          ((System.Windows.Forms.AnchorStyles)
           (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
        this.btnOK.Location = new System.Drawing.Point(167, 274);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(78, 23);
        this.btnOK.TabIndex = 12;
        this.btnOK.Text = "&OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // groupBox2
        // 
        this.groupBox2.Controls.Add(this.mpLabel1);
        this.groupBox2.Controls.Add(this.tbBacklightBLUE);
        this.groupBox2.Controls.Add(this.mpLabel2);
        this.groupBox2.Controls.Add(this.tbBacklightGREEN);
        this.groupBox2.Controls.Add(this.mpLabel3);
        this.groupBox2.Controls.Add(this.tbBacklightRED);
        this.groupBox2.Location = new System.Drawing.Point(10, 140);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(221, 125);
        this.groupBox2.TabIndex = 75;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "Backlight";
        // 
        // mpLabel1
        // 
        this.mpLabel1.Location = new System.Drawing.Point(5, 20);
        this.mpLabel1.Name = "mpLabel1";
        this.mpLabel1.Size = new System.Drawing.Size(40, 23);
        this.mpLabel1.TabIndex = 51;
        this.mpLabel1.Text = "Red";
        this.mpLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // tbBacklightRED
        // 
        this.tbBacklightRED.AutoSize = false;
        this.tbBacklightRED.Location = new System.Drawing.Point(50, 20);
        this.tbBacklightRED.Maximum = 255;
        this.tbBacklightRED.Name = "tbBacklightRED";
        this.tbBacklightRED.Size = new System.Drawing.Size(160, 23);
        this.tbBacklightRED.TabIndex = 73;
        this.tbBacklightRED.TickFrequency = 9;
        this.tbBacklightRED.TickStyle = System.Windows.Forms.TickStyle.None;
        this.tbBacklightRED.Value = 255;
        // 
        // mpLabel2
        // 
        this.mpLabel2.Location = new System.Drawing.Point(5, 45);
        this.mpLabel2.Name = "mpLabel1";
        this.mpLabel2.Size = new System.Drawing.Size(40, 23);
        this.mpLabel2.TabIndex = 51;
        this.mpLabel2.Text = "Green";
        this.mpLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // tbBacklightGREEN
        // 
        this.tbBacklightGREEN.AutoSize = false;
        this.tbBacklightGREEN.Location = new System.Drawing.Point(50, 45);
        this.tbBacklightGREEN.Maximum = 255;
        this.tbBacklightGREEN.Name = "tbBacklightGREEN";
        this.tbBacklightGREEN.Size = new System.Drawing.Size(160, 23);
        this.tbBacklightGREEN.TabIndex = 10;
        this.tbBacklightGREEN.TickFrequency = 8;
        this.tbBacklightGREEN.TickStyle = System.Windows.Forms.TickStyle.None;
        this.tbBacklightGREEN.Value = 255;
        // 
        // mpLabel3
        // 
        this.mpLabel3.Location = new System.Drawing.Point(5, 70);
        this.mpLabel3.Name = "mpLabel1";
        this.mpLabel3.Size = new System.Drawing.Size(40, 23);
        this.mpLabel3.TabIndex = 51;
        this.mpLabel3.Text = "Blue";
        this.mpLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // tbBacklightBLUE
        // 
        this.AutoSize = false;
        this.tbBacklightBLUE.Location = new System.Drawing.Point(50, 70);
        this.tbBacklightBLUE.Maximum = 255;
        this.tbBacklightBLUE.Name = "tbBacklightBLUE";
        this.tbBacklightBLUE.Size = new System.Drawing.Size(160, 23);
        this.tbBacklightBLUE.TabIndex = 11;
        this.tbBacklightBLUE.TickFrequency = 8;
        this.tbBacklightBLUE.TickStyle = System.Windows.Forms.TickStyle.None;
        this.tbBacklightBLUE.Value = 255;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(257, 308);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.groupBox1);
        this.Name = "Form1";
        this.Text = "Advanced Settings";
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.groupBox2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize) (this.tbBacklightRED)).EndInit();
        ((System.ComponentModel.ISupportInitialize) (this.tbBacklightGREEN)).EndInit();
        ((System.ComponentModel.ISupportInitialize) (this.tbBacklightBLUE)).EndInit();
        this.ResumeLayout(false);
      }

      #endregion

      #region UI Buttons

      private void btnOK_Click(object sender, EventArgs e)
      {
        Log.Debug("IDisplay MatrixGX.AdvancedSetupForm.btnOK_Click() started");
        AdvancedSettings.Save();
        Hide();
        Close();
        Log.Debug("IDisplay MatrixGX.AdvancedSetupForm.btnOK_Click() Completed");
      }

      #endregion
    }

    #endregion
  }
}