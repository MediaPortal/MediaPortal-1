#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplayPlugin.VFD_Control;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  /// <summary>
  /// Shuttle PF27 VFD driver
  /// </summary>
  /// <author>pridehaveit</author>
  public class ShuttlePF27 : BaseDisplay, IDisplay
  {
    #region Fields

    private control vfd;

    #endregion

    #region Readonly Fields

    private const int Lines = 1;
    private const byte VolumeMaximum = 12;
    private const int TextLengthMaximum = 20;
    private const int MessageLengthMaximum = 7;

    private readonly bool isDisabled;
    private readonly string errorMessage = "";

    #endregion

    #region Constructor

    public ShuttlePF27()
    {
      try
      {
        vfd = new control(0x051c, 0x0005);
        vfd.initScreen();
      }
      catch (NotSupportedException ex)
      {
        isDisabled = true;
        errorMessage = ex.Message;
      }
    }

    #endregion

    #region IDisplay Members

    /// <summary>
    /// On clean up the clock is activated so the time is shown
    /// when MediaPortal is closed or put to standby.
    /// </summary>
    public void CleanUp()
    {
      SetClock();
    }

    /// <summary>
    /// Shows the advanced configuration screen.
    /// </summary>
    public void Configure()
    {
      MessageBox.Show("No advanced configuration", "Information",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Description of this display driver.
    /// </summary>
    public string Description
    {
      get { return "Shuttle PF27 VFD driver for Shuttle SG33G5M Barebones"; }
    }

    public void Dispose()
    {
      try
      {
        vfd.Shutdown();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void DrawImage(Bitmap bitmap)
    {
    }

    public string ErrorMessage
    {
      get { return errorMessage; }
    }

    /// <summary>
    /// Clears screen.
    /// </summary>
    public void Initialize()
    {
      ClearScreen();
    }

    public bool IsDisabled
    {
      get { return isDisabled; }
    }

    /// <summary>
    /// Short name of this display driver.
    /// </summary>
    public string Name
    {
      get { return "Shuttle PF27"; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    /// <summary>
    /// Displays the message on the indicated line.
    /// Shuttle PF27 display only supports 1 line of 20 characters.
    /// Additionally it supports volume 0 till 12 and various icons
    /// defined by enum Icon.
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      if (line >= Lines)
      {
        Log.Error("ShuttlePF27.SetLine: error bad line number" + line);
        return;
      }
      try
      {
        // Write message
        SetText(message);

        uint icons = 0;

        // Show radio icon if on radio screen
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_RADIO)
        {
          icons += (uint)Icon.Radio;
        }

        // Show music icon if on any music screen
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_MUSIC ||
           (GUIWindowManager.ActiveWindow >= (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST &&
            GUIWindowManager.ActiveWindow <= (int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC))
        {
          icons += (uint)Icon.Music;
        }

        // Show CD / DVD icon if a CD or DVD is played
        if (g_Player.IsCDA || g_Player.IsDVD)
        {
          icons += (uint)Icon.CdDvd;
        }

        // Show television icon if on any tv screen
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
           (GUIWindowManager.ActiveWindow >= (int)GUIWindow.Window.WINDOW_TVGUIDE &&
            GUIWindowManager.ActiveWindow <= (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS))
        {
          icons += (uint)Icon.Television;
        }

        // Show camera if on pictures screen
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_PICTURES)
        {
          icons += (uint)Icon.Camera;
        }

        // Show rewind icon if speed is negative
        if (g_Player.Speed < 0)
        {
          icons += (uint)Icon.Rewind;
        }

        // Show record icon if a recording is in progress or a recording in played
        if (g_Player.IsTVRecording)
        {
          icons += (uint)Icon.Record;
        }

        // Show play icon if playing with speed 1 and not paused and not in DVD menu
        if (g_Player.Playing & !g_Player.Paused & (g_Player.Speed == 1) & !g_Player.IsDVDMenu)
        {
          icons += (uint)Icon.Play;
        }

        // Show pause icon if paused
        if (g_Player.Paused)
        {
          icons += (uint)Icon.Pause;
        }

        // Show stop icon if not playing
        if (!g_Player.Playing)
        {
          icons += (uint)Icon.Stop;
        }

        // Show forward icon if speed is greater 1
        if (g_Player.Speed > 1)
        {
          icons += (uint)Icon.Forward;
        }

        // Reverse icon not used

        // Repeat icon not used

        // Show mute icon if volume is muted
        if (VolumeHandler.Instance.IsMuted)
        {
          icons += (uint)Icon.Mute;
        }

        // Volume is converted into value between 0 and 12
        // TODO: Volume does not seem to be linear
        //       How is the volume calculated for the bar show in GUI?
        byte volume = (byte)(VolumeHandler.Instance.Volume * VolumeMaximum
          / VolumeHandler.Instance.Maximum);

        SetIconsAndVolume(icons, volume);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Clears screen.
    /// </summary>
    /// <param name="_port">The port the display is connected to</param>
    /// <param name="_lines">The number of lines in text mode</param>
    /// <param name="_cols">The number of columns in text mode</param>
    /// <param name="_delay">Communication delay in text mode</param>
    /// <param name="_linesG">The height in pixels in graphic mode</param>
    /// <param name="_colsG">The width in pixels in graphic mode</param>
    /// <param name="_delayG">Communication delay in graphic mode</param>
    /// <param name="_backLight">Backlight on?</param>
    /// <param name="_backLightLevel">Backlight level</param>
    /// <param name="_contrast">Contrast on?</param>
    /// <param name="_contrastLevel">Contrast level</param>
    /// <param name="_blankOnExit">Blank on exit?</param>
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
    {
    }

    /// <summary>
    /// Shuttle PF27 display does not support graphics.
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Shuttle PF27 display only support text.
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    #endregion

    #region IDisposable Members

    #endregion

    #region ShuttlePF27 Members

    private void LogDebug(string message)
    {
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Debug("ShuttlePF27: " + message);
      }
    }

    private void ClearScreen()
    {
      LogDebug("Clearing screen");
      SendMessage(MessageType.ClearScreen, new byte[] { (byte)MessageType.ClearScreen });
      LogDebug("Clearing screen finished");
    }

    private void ResetCursor()
    {
      LogDebug("Resetting cursor");
      SendMessage(MessageType.ClearScreen, new byte[] { (byte)MessageType.ResetCursor });
      LogDebug("Resetting cursor finished");
    }

    private void SetText(string text)
    {
      LogDebug("Setting text");
      // Set cursor to first position
      ResetCursor();
      int totalLen = Math.Min(TextLengthMaximum, text.Length);
      // Set text in chunks of 7 bytes
      for (int i = 0; i < totalLen; i += 7)
      {
        int partLen = Math.Min(MessageLengthMaximum, totalLen - i);
        byte[] textBuffer = new byte[partLen];
        for (int j = 0; j < partLen; j++)
        {
          textBuffer[j] = (byte)text[i + j];
        }
        SendMessage(MessageType.SetText, textBuffer);
      }
      LogDebug("Setting text finished");
    }

    private void SetIconsAndVolume(uint icons, byte volume)
    {
      LogDebug("Setting icons and volume");
      // put volume into 1. byte
      icons += volume;
      byte[] buffer = new byte[4];
      for (int i = 0; i < 4; i++)
      {
        buffer[i] = (byte)icons;
        icons = icons >> 8;
      }
      SendMessage(MessageType.SetIconsAndVolume, buffer);
      LogDebug("Setting icons and volume finished");
    }

    /// <summary>
    /// Set clock to current time.
    /// </summary>
    private void SetClock()
    {
      LogDebug("Setting and activating clock");
      DateTime now = DateTime.Now;
      SendMessage(MessageType.SetClock, new byte[] {
        CodeValue(now.Second),
        CodeValue(now.Minute),
        CodeValue(now.Hour),
        CodeValue((int)now.DayOfWeek),
        CodeValue(now.Day),
        CodeValue(now.Month),
        CodeValue(now.Year % 100)
      });
      // Give display time to set clock
      Thread.Sleep(50);
      SendMessage(MessageType.ActivateClock, new byte[] { (byte)MessageType.ActivateClock });
      LogDebug("Setting and activating clock finished");
    }

    /// <summary>
    /// Codes a decimal number in a hex number.
    /// 33 decimal get 0x33 hex.
    /// </summary>
    /// <param name="value">Value to code</param>
    /// <returns>Value coded</returns>
    private byte CodeValue(int value)
    {
      return (byte)((value / 10 * 16) + (value % 10));
    }

    /// <summary>
    /// Sends a message to the Shuttle PF27 display.
    /// After each message a pause of at least 50 milliseconds is necessary to
    /// leave the display time to process the message otherwise the following
    /// messages could be swallowed.
    /// </summary>
    /// <param name="messageType">
    /// Type of the message described in comment of enum MessageType.
    /// </param>
    /// <param name="messageData">
    /// Up to 7 bytes of data of the message.
    /// </param>
    private void SendMessage(MessageType messageType, byte[] messageData)
    {
      byte[] reportBuffer = new byte[9];
      int len = Math.Min(7, messageData.Length);
      // 1. byte takes up the report id
      reportBuffer[0] = 0x00;
      // The upper 4 bits of the 2. byte take up the message type
      // The lower 4 bits contain the message length (up to 7)
      reportBuffer[1] = (byte)((((byte)messageType) << 4) + len);
      // Bytes 3 till 9 contain the message data
      for (int i = 0; i < len; i++)
      {
        reportBuffer[i + 2] = messageData[i];
      }
      vfd.WriteHidOutputReport(reportBuffer);
      // Give display time to process message
      Thread.Sleep(50);
    }

    /// <summary>
    /// The Shuttle PF27 display support the following message types:
    /// 1:  Clear screen or reset cursor. Data is 1 byte long.
    ///     If data is 1 the screen is cleared
    ///     If data is 2 the cursor is resetted to the first position
    /// 3:  Activates the clock display. Data is 1 byte long and need to have
    ///     the value 3.
    /// 7:  Set icons and volume. Data is 4 bytes long. The first byte contains
    ///     the volume. A value between 0 and 12 is allowed. Byte 2 till 4 take
    ///     up the icons defined by enum Icon.
    /// 9:  Set text message. Data contains the test message in ASCII.
    ///     The message must be send in chunks of 7 bytes. So if all
    ///     20 character should be filled 3 messages of 7, 7 and 6 bytes
    ///     must be send.
    /// 13: Sets the clock. Data is 7 byte long containing the following values:
    ///     1. byte: Seconds
    ///     2. byte: Minutes
    ///     3. byte: Hour
    ///     4. byte: DayOfWeek (0: Sunday till 6: Saturday)
    ///     5. byte: Day
    ///     6. byte: Month
    ///     7. byte: Year (two-digit)
    ///     Strangly the decimal values are coded to hex value. So if you want
    ///     to set 33 seconds the value of byte 1 is 0x33 hex. Between setting
    ///     the clock and activating the clock display an pause of at least
    ///     50 milliseconds is necessary.
    /// </summary>
    private enum MessageType
    {
      ClearScreen = 1,
      ResetCursor = 2,
      ActivateClock = 3,
      SetIconsAndVolume = 7,
      SetText = 9,
      SetClock = 13
    }

    /// <summary>
    /// Values of the different icons.
    /// These are exactly the values used in
    /// bytes 2 till 4 of message with type 7.
    /// Byte 1 is left zero to take up the volume.
    /// </summary>
    private enum Icon
    {
      Clock = 0x10000000,
      Radio = 0x08000000,
      Music = 0x04000000,
      CdDvd = 0x02000000,
      Television = 0x01000000,

      Camera = 0x00100000,
      Rewind = 0x00080000,
      Record = 0x00040000,
      Play = 0x00020000,
      Pause = 0x00010000,

      Stop = 0x00001000,
      Forward = 0x00000800,
      Reverse = 0x00000400,
      Repeat = 0x00000200,
      Mute = 0x00000100
    }

    #endregion
  }
}
