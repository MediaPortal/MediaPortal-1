#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  /// <summary>
  /// Logitech diNovo MediaPad LCD driver
  /// </summary>
  /// <author>JoeDalton</author>
  public class MediaPad : BaseDisplay, IDisplay
  {
    private readonly double mediaPadId; //Unique ID of the Mediapad
    private readonly double mediaPadLcdId; //Unique ID of the Mediapad LCD display
    private readonly bool isDisabled;
    private readonly string errorMessage;

    public MediaPad()
    {
      try
      {
        mediaPadId = GetMediapadUID(out mediaPadLcdId);
        if (mediaPadId == 0)
        {
          isDisabled = true;
          errorMessage = "Could not find a Logitech diNovo Mediapad";
        }
      }
      catch (Exception ex)
      {
        isDisabled = true;
        errorMessage = ex.Message;
      }
    }

    #region IDisplay Members

    public bool IsDisabled
    {
      get { return isDisabled; }
    }

    public string ErrorMessage
    {
      get { return errorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters) {}

    public void DrawImage(Bitmap bitmap) {}


    /// <summary>
    /// Sends a line of text to the display
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      if (mediaPadId == 0)
      {
        return;
      }
      Status res = SetLine(mediaPadLcdId, (short)line, ref message, DisplayMode.Static);
      if (res != Status.Success)
      {
        Log.Warn("MiniDisplay: Could not send text to Mediapad LCD ({0})", res.ToString());
      }
    }

    /// <summary>
    /// Gets the driver name
    /// </summary>
    public string Name
    {
      get { return "MediaPad"; }
    }

    /// <summary>
    /// Gets the driver description
    /// </summary>
    public string Description
    {
      get { return "Logitech diNovo MediaPad v1.0"; }
    }

    /// <summary>
    /// Returns whether this driver supports text mode
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Returns whether this driver supports graphic mode
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public void Configure() {}

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
    /// <param name="_backLightLevel">Backlight level</param>
    /// <param name="_contrast">Contrast on?</param>
    /// <param name="_contrastLevel">Contrast level</param>
    /// <param name="_blankOnExit">Blank on exit?</param>
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
    {
      Log.Info("MiniDisplay: Found Logitech diNovo Mediapad with ID {0} and LCD ID {1}", mediaPadId, mediaPadLcdId);
      SetDisplayMode(ScreenMode.Normal);
      Beep();
      return;
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      Clear();
    }

    public void CleanUp()
    {
      Clear();
    }

    private void Clear()
    {
      if (mediaPadId == 0)
      {
        return;
      }
      Status res = ClearScreen(mediaPadLcdId);
      if (res != Status.Success)
      {
        Log.Warn("MiniDisplay: Could not clear Mediapad LCD ({0}", res.ToString());
      }
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
      if (mediaPadId == 0)
      {
        return;
      }
      SetDisplayMode(ScreenMode.Time);
    }

    #endregion

    /// <summary>
    /// Sets the display mode
    /// </summary>
    /// <param name="mode">The <see cref="ScreenMode"/> to set the display to.</param>
    private void SetDisplayMode(ScreenMode mode)
    {
      Status res = SetMode(mediaPadLcdId, mode);
      if (res != Status.Success)
      {
        Log.Warn("MiniDisplay: Could not switch Mediapad to time mode ({0})", res.ToString());
      }
    }

    /// <summary>
    /// Makes the display beep
    /// </summary>
    private void Beep()
    {
      Status res = MakeBeep(mediaPadLcdId);
      if (res != Status.Success)
      {
        Log.Warn("MiniDisplay: Could not make Mediapad beep ({0}", res.ToString());
      }
      return;
    }

    #region Interop code

    /// <summary>
    /// Clears the mediapad's screen identified by the lcd uid retrieved by getMediapadUID()
    /// </summary>
    /// <param name="lcdUid">The id of the LCD to clear</param>
    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
    [
      DllImport("MediaPadLayer.dll", EntryPoint = "clearScreen", CharSet = CharSet.Ansi, SetLastError = true,
        ExactSpelling = true)]
    private static extern Status ClearScreen(double lcdUid);

//
//    /// <summary>
//    /// Sets the blue led state
//    /// </summary>
//    /// <param name="lcdUid"></param>
//    /// <param name="state">1=on, 0=off</param>
//    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
//    [DllImport("MediaPadLayer.dll", EntryPoint="controlLed")]
//    private static extern Status ControlLed(double lcdUid, short state);
//
    /// <summary>
    /// Gets the mediapad unique ID, if there are many mediapad, only the first uid is returned
    /// don't call this function many times, please call it only once and store the result into a variable
    /// </summary>
    /// <param name="lcdUid">The uid of the lcd linked to the mediapad</param>
    /// <returns>The uid of the mediapad</returns>
    [
      DllImport("MediaPadLayer.dll", EntryPoint = "getMediapadUID", CharSet = CharSet.Ansi, SetLastError = true,
        ExactSpelling = true)]
    private static extern double GetMediapadUID(out double lcdUid);

//
//    /// <summary>
//    /// Gets the current screen mode of indicated Mediapad LCD.
//    /// </summary>
//    /// <param name="lcdUid">The uid of the lcd to get the current screenmode for</param>
//    /// <param name="state">The current <see cref="ScreenMode"/> value.</param>
//    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
//    [DllImport("MediaPadLayer.dll", EntryPoint="GetMode", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
//    private static extern Status GetMode(double lcdUid, out ScreenMode state);

    /// <summary>
    ///Makes a beep, DUID is the unique ID of the mediapad retrived by getMediapadUID()
    /// </summary>
    /// <param name="DUID"></param>
    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
    [DllImport("MediaPadLayer.dll", EntryPoint = "makeBeep", CharSet = CharSet.Ansi, SetLastError = true,
      ExactSpelling = true)]
    private static extern Status MakeBeep(double DUID);

//    /// <summary>
//    /// Registers a window to receive the Initialize Screen key event, 
//    /// so when this key is pressed on the Mediapad, 
//    /// a message WM_APP+40 is sent to that window.
//    /// </summary>
//    /// <param name="uid">The uid of the Mediapad to register the key events for</param>
//    /// <param name="hWnd">The window handle of the window to send the message to</param>
//    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
//    [DllImport("MediaPadLayer.dll", EntryPoint="registerclearEvent", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
//    private static extern Status RegisterClearEvent(double uid, IntPtr hWnd);
//
//    /// <summary>
//    /// Registers a window to receive all key events, so when a key is pressed, 
//    /// a message WM_APP+20 is sent to that window.
//    /// </summary>
//    /// <param name="uid">The uid of the Mediapad to register the key events for</param>
//    /// <param name="hWnd">The window handle of the window to send the message to</param>
//    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
//    [DllImport("MediaPadLayer.dll", EntryPoint="registerkeyEvent", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
//    private static extern Status RegisterkeyEvent(double uid, IntPtr hWnd);
//
//    /// <summary>
//    /// Sets the mediapad's icon state
//    /// </summary>
//    /// <param name="lcdUid">The uid of the Mediapad LCD to set the icon states for</param>
//    /// <param name="icon"></param>
//    /// <param name="state"></param>
//    /// <returns></returns>
//    [DllImport("MediaPadLayer.dll", EntryPoint="SetIconState", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
//    private static extern Status SetIconState(double lcdUid, IconConst icon, IconState state);

    [DllImport("MediaPadLayer.dll", EntryPoint = "setLine", CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern Status SetLine(double lcdUid, short line,
                                         [MarshalAs(UnmanagedType.VBByRefStr)] ref string text, DisplayMode mode);

    /// <summary>
    /// Sets the desired screen mode of indicated Mediapad LCD.
    /// </summary>
    /// <param name="lcdUid">The uid of the lcd to set the current screenmode for</param>
    /// <param name="state">The desired <see cref="ScreenMode"/> value.</param>
    /// <returns><see cref="Status.Success"/> or another <see cref="Status"/> value in case of an error</returns>
    [DllImport("MediaPadLayer.dll", EntryPoint = "SetMode", CharSet = CharSet.Ansi, SetLastError = true,
      ExactSpelling = true)]
    private static extern Status SetMode(double lcdUid, ScreenMode state);


    /// <summary>
    /// Describes how text is displayed on the LCD.
    /// </summary>
    private enum DisplayMode
    {
      /// <summary>
      /// ??The line is cleared??
      /// </summary>
      Clear = 1,
      /// <summary>
      /// The line is shown as fixed text
      /// </summary>
      Static = 2,
      /// <summary>
      /// The line scrolls if the text is too long
      /// </summary>
      Scroll = 3,
      /// <summary>
      /// The text is wrapped to the next line
      /// </summary>
      Wrapped = 4
    }

    /// <summary>
    /// Describes the screen mode of the LCD
    /// </summary>
    private enum ScreenMode
    {
      /// <summary>
      /// Display is in 3 lines mode
      /// </summary>
      Normal = 0,
      /// <summary>
      /// Display is in 2 lines mode.  Top line has double height.
      /// </summary>
      DoubleUpper = 1,
      /// <summary>
      /// Display is in 2 lines mode.  Bottom line has double height.
      /// </summary>
      DoubleLower = 2,
      /// <summary>
      /// Display shows current time and ignores text sent to it.
      /// </summary>
      Time = 3
    }

    /// <summary>
    /// Status codes
    /// </summary>
    private enum Status : uint
    {
      /// <summary>
      /// No error occured
      /// </summary>
      Success = 0x0,
      /// <summary>
      /// The target device returned an error status code to the
      /// request. This status should never be returned to the client.
      /// The KHAL internal caller should analyze the actual error code
      /// from the data in the hardware response, try possible recovery
      /// and return Unsuccessfull or IOControlError to the client
      /// if the recovery scheme failed.
      /// </summary>
      HardwareError = 0x3,
      /// <summary>
      /// This is an internal error which should never get to the
      /// calling client since it was unloaded while the I/O request
      /// was still occuring.
      /// </summary>
      ClientUnloaded = 0x4,
      /// <summary>
      /// The value of a parameter is invalid for the target function.
      /// This includes buffer with too small size for the data.
      /// </summary>
      InvalidParameter = 0x80000001,
      /// <summary>
      /// The ulSize param of one of the structures used for this function
      /// does no match its expected size. Check initilization of ulSize
      /// with the structure size. If this still fails, check DLL version.
      /// </summary>
      InvalidSize = 0x80000002,
      /// <summary>
      /// The pointer is referencing an invalid memory area for the request.
      /// Check that size of the memory allocated is correct. Some functions need
      /// both Read/Write access. Check that the memory was not deallocated after
      /// the function call. Also check for NULL pointers.
      /// </summary>
      InvalidPointer = 0x80000003,
      /// <summary>
      /// System resources needed to complete the request could not be allocated.
      /// Look for for memory leaks, unclosed HANDLEs (events, files etc...)
      /// This usually indicates that the system is in bad shape.
      /// </summary>
      LowResources = 0x80000004,
      /// <summary>
      /// The main process could not be started. Ensure that main process EXE file
      /// is in a accessible path for the current process. This indicate that KHAL
      /// may need to be re-installed.
      /// </summary>
      MainProcessError = 0x80000005,
      /// <summary>
      /// The DUID does not match any currently listed device. Check that the device
      /// was not removed. The client can be notify by registering to the event
      /// DL_EVENT_DEVICE_REMOVAL. After receiving the removal event, any request
      /// to the device will fail.
      /// </summary>
      InvalidId = 0x80000006,
      /// <summary>
      /// The requested function value is invalid for the specified device. Check
      /// for device type mismatch such as calling a mouse function for a keyboard.
      /// </summary>
      InvalidFunction = 0x80000007,
      /// <summary>
      /// The requested event value is invalid for the specified device. Check for
      /// out of range or device type mismatch.
      /// </summary>
      InvalidEvent = 0x80000008,
      /// <summary>
      /// The device is in the process of beeing removed. Expect a device removal
      /// event to be posted sooner or later.
      /// </summary>
      DeviceRemoval = 0x8000000A,
      /// <summary>
      /// Allocation of internal data allocation failed because one of the lists
      /// maximum size was exceeded. Check for spurious or never canceled event
      /// requests. Run the DLL debug version to find which is the "undersized"
      /// list.
      /// </summary>
      ListOverflow = 0x8000000B,
      /// <summary>
      /// An unexpected bug error occured. Use DLL debug version for more
      /// information on the actual error. This should not happen and needs
      /// debugging of the DLL internal code. Debug version should throw an
      /// assertion exception for this one.
      /// </summary>
      InternalError = 0x8000000C,
      /// <summary>
      /// The Callback function pointer is not in executable code section.
      /// </summary>
      InvalidCallback = 0x8000000D,
      /// <summary>
      /// The LD_HANDLE is not valid.
      /// </summary>
      InvalidHandle = 0x8000000E,
      /// <summary>
      /// An other client has already registered the specified event with
      /// exclusive flag set.
      /// </summary>
      ExclusiveConflict = 0x8000000F,
      /// <summary>
      /// IO request to the driver failed. May happen if an invalid
      /// request is send to the driver. The DLL should have catched
      /// the problem before sending it down to the drivers !
      /// </summary>
      IOControlError = 0x80000010,
      /// <summary>
      /// Generic error code... Use DLL debug version for more info in the trace.
      /// </summary>
      Unsuccessful = 0x80000011,
      /// <summary>
      /// The maximum completion time was excided before the function could
      /// be completed.
      /// </summary>
      Timeout = 0x80000012,
      /// <summary>
      /// The requested function is not implemented.
      /// </summary>
      NotImplemented = 0x80000013,
      /// <summary>
      /// The requested function can not be executed because of missing steps
      /// in the sequence. E.g. The FLock status for some corded keyboard can
      /// not be read if it wasn//t previously set.
      /// </summary>
      SequenceError = 0x80000014,
      /// <summary>
      /// The target device doesn//t have the capability to perform the requested
      /// operation or to generate the event. Check if the device description
      /// file for a missing capability flag.
      /// </summary>
      DeviceNotCapable = 0x80000015,
      /// <summary>
      /// The requested status event does not exists or is invalid for this type
      /// of device.
      /// </summary>
      DeviceInvalidStatus = 0x80000016,
      /// <summary>
      /// The device wireless connection hasn//t been established yet, this
      /// request can not be completeted at this time. This is a BT specific
      /// error code.
      /// </summary>
      DeviceNotConnected = 0x80000017,
      /// <summary>
      /// The current user session is locked. No action can be taken now
      /// because it might interfere with the other users device settings.
      /// NOTE : THIS IS NOT USED ANYMORE.
      /// </summary>
      [Obsolete] SessionLocked = 0x80000018,
      /// <summary>
      /// The exact same event has already been registered by the same client.
      /// Note that event must also have same notification method to be
      /// considered as a duplicate.
      /// </summary>
      EventDuplication = 0x80000019,
      /// <summary>
      /// No KhalApi.dll found. Check that the path to the Khalapi.dll is correctly
      /// stored in HKEY_LOCAL_MACHINE/Software/Logitech/Khal registry key. Only
      /// happens when using CDrvApi proxy.
      /// </summary>
      DllOrFunctionNotFound = 0x8000001A,
      /// <summary>
      /// A call to a system function has returned an error
      /// </summary>
      SystemError = 0x8000001B,
      /// <summary>
      /// An access to the system Registry has failed
      /// </summary>
      RegistryError = 0x8000001C
    }

//    private enum IconConst
//    {
//      IconMail = 0,
//      IconMessenger = 1,
//      IconBell = 3,
//      IconMute = 2
//    }

//    private enum IconState
//    {
//      Blink = 3,
//      On = 2,
//      Off = 1
//    }

    #endregion
  }
}