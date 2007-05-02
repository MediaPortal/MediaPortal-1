// adapted for Scaleo EV VFD by David Kesl

// Usbhidio is brought to you by
// Jan Axelson
// Lakeview Research
// 2209 Winnebago St.
// Madison, WI 53704
// Phone: 608-241-5824
// Fax: 608-241-5848
// http://www.lvr.com
// jan@lvr.com

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.VFD_Control
{
  public class control
  {
    /// <summary>
    /// Enumeration of VFD symbols:
    /// V,
    /// CD,
    /// Play,
    /// Rewind,
    /// Pause,
    /// Forward,
    /// Mute,
    /// REC,
    /// Antenna,
    /// DVD
    /// </summary>
    public enum VFDSymbols
    {
      V = 0x8000, // for VCD
      CD = 0x4000,
      Play = 0x2000,
      Rewind = 0x1000,
      Pause = 0x0800,
      Forward = 0x0400,
      Mute = 0x0200,
      REC = 0x0100,
      Antenna = 0x0002,
      DVD = 0x0001
    }

    #region Readonly Fields

    private readonly Hid _MyHID = new Hid();

    #endregion

    #region Fields

    private int _HIDHandle;
    private bool _MyDeviceDetected;
    private int _Volume = -1;
    private BitVector32 _SymbolsVisible = new BitVector32(0);

    #endregion

    /// <summary>
    /// Uses a series of API calls to locate a HID-class device
    /// by its Vendor ID and Product ID.
    /// </summary>
    /// <returns>True if the device is detected, False if not detected.</returns>
    private bool FindTheHid()
    {
      string[] DevicePathName = new string[128];
      FileIOApiDeclarations.SECURITY_ATTRIBUTES Security = new FileIOApiDeclarations.SECURITY_ATTRIBUTES();

      try
      {
        Guid HidGuid = Guid.Empty;
        _MyDeviceDetected = false;

        // Values for the SECURITY_ATTRIBUTES structure:
        Security.lpSecurityDescriptor = 0;
        Security.bInheritHandle = Convert.ToInt32(true);
        Security.nLength = Marshal.SizeOf(Security);

        // IDs vom VFD des Scaleo E
        short MyVendorID = short.Parse("1509", NumberStyles.HexNumber);
        short MyProductID = short.Parse("925d", NumberStyles.HexNumber);

        /*
                  API function: 'HidD_GetHidGuid
                  Purpose: Retrieves the interface class GUID for the HID class.
                  Accepts: 'A System.Guid object for storing the GUID.
                  */

        HidApiDeclarations.HidD_GetHidGuid(ref HidGuid);
        Debug.WriteLine(Debugging.ResultOfAPICall("GetHidGuid"));

        // Display the GUID.
        string GUIDString = HidGuid.ToString();
        Debug.WriteLine("  GUID for system HIDs: " + GUIDString);

        // Fill an array with the device path names of all attached HIDs.
        bool DeviceFound = DeviceManagement.FindDeviceFromGuid(HidGuid, ref DevicePathName);

        // If there is at least one HID, attempt to read the Vendor ID and Product ID
        // of each device until there is a match or all devices have been examined.

        if (DeviceFound)
        {
          int MemberIndex = 0;
          do
          {
            // ***
            // API function:
            // CreateFile
            // Purpose:
            // Retrieves a handle to a device.
            // Accepts:
            // A device path name returned by SetupDiGetDeviceInterfaceDetail
            // The type of access requested (read/write).
            // FILE_SHARE attributes to allow other processes to access the device while this handle is open.
            // A Security structure. Using Null for this may cause problems under Windows XP.
            // A creation disposition value. Use OPEN_EXISTING for devices.
            // Flags and attributes for files. Not used for devices.
            // Handle to a template file. Not used.
            // Returns: a handle that enables reading and writing to the device.
            // ***

            _HIDHandle = FileIOApiDeclarations.CreateFile
              (DevicePathName[MemberIndex],
               FileIOApiDeclarations.GENERIC_READ | FileIOApiDeclarations.GENERIC_WRITE,
               FileIOApiDeclarations.FILE_SHARE_READ | FileIOApiDeclarations.FILE_SHARE_WRITE,
               ref Security,
               FileIOApiDeclarations.OPEN_EXISTING, 0, 0);

            Debug.WriteLine(Debugging.ResultOfAPICall("CreateFile"));
            Debug.WriteLine("  Returned handle: " + _HIDHandle.ToString("x") + "h");

            if (_HIDHandle != FileIOApiDeclarations.INVALID_HANDLE_VALUE)
            {
              // The returned handle is valid,
              // so find out if this is the device we're looking for.

              // Set the Size property of DeviceAttributes to the number of bytes in the structure.
              _MyHID.DeviceAttributes.Size = Marshal.SizeOf(_MyHID.DeviceAttributes);

              // ***
              // API function:
              // HidD_GetAttributes
              // Purpose:
              // Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID,
              // Product ID, and Product Version Number for a device.
              // Accepts:
              // A handle returned by CreateFile.
              // A pointer to receive a HIDD_ATTRIBUTES structure.
              // Returns:
              // True on success, False on failure.
              // ***

              int Result = HidApiDeclarations.HidD_GetAttributes(_HIDHandle, ref _MyHID.DeviceAttributes);


              Debug.WriteLine(Debugging.ResultOfAPICall("HidD_GetAttributes"));

              if (Result != 0)
              {
                Debug.WriteLine("  HIDD_ATTRIBUTES structure filled without error.");

                //Debug.WriteLine("  Structure size: " + MyHID.DeviceAttributes.Size);
                Debug.WriteLine("  Vendor ID: " + _MyHID.DeviceAttributes.VendorID.ToString("x"));
                Debug.WriteLine("  Product ID: " + _MyHID.DeviceAttributes.ProductID.ToString("x"));
                Debug.WriteLine("  Version Number: " + _MyHID.DeviceAttributes.VersionNumber.ToString("x"));

                // Find out if the device matches the one we're looking for.
                if ((_MyHID.DeviceAttributes.VendorID == MyVendorID) &
                    (_MyHID.DeviceAttributes.ProductID == MyProductID))
                {
                  // It's the desired device.
                  Debug.WriteLine("  My device detected");

                  // Display the information in form's list box.
                  /*
									lstResults.Items.Add("Device detected:");
									lstResults.Items.Add("  Vendor ID = " + _MyHID.DeviceAttributes.VendorID.ToString("x"));
									lstResults.Items.Add("  Product ID = " + _MyHID.DeviceAttributes.ProductID.ToString("x"));
									
									ScrollToBottomOfListBox();
									*/
                  _MyDeviceDetected = true;

                  // Save the DevicePathName so OnDeviceChange() knows which name is my device.
                }
                else
                {
                  // It's not a match, so close the handle.
                  _MyDeviceDetected = false;

                  FileIOApiDeclarations.CloseHandle(_HIDHandle);

                  Debug.WriteLine(Debugging.ResultOfAPICall("CloseHandle"));
                }
              }
              else
              {
                // There was a problem in retrieving the information.
                Debug.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                _MyDeviceDetected = false;
                FileIOApiDeclarations.CloseHandle(_HIDHandle);
              }
            }

            // Keep looking until we find the device or there are no more left to examine.
            MemberIndex = MemberIndex + 1;
          } while (!(_MyDeviceDetected | (MemberIndex == DevicePathName.Length + 1)));
        }

        if (_MyDeviceDetected)
        {
          // The device was detected.
          // Learn the capabilities of the device.
          _MyHID.Capabilities = _MyHID.GetDeviceCapabilities(_HIDHandle);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        //HandleException(this.Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(), ex);
      }
      return _MyDeviceDetected;
    }

    /// <summary>
    /// Initializes the VFD screen, including opening of USB
    /// </summary>
    public void initScreen()
    {
      if (!FindTheHid())
      {
        throw new NotSupportedException("ScaleoEV display not found");
      }
      if (Process.GetProcessesByName("ehshell").Length > 0)
      {
        return; //exit if xpMCE is running
      }
      Hid.OutputReport myOutputReport = new Hid.OutputReport();
      myOutputReport.Write(new byte[] {0x00, 0x10, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x10, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x10, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x10, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
    }


    /// <summary>
    /// Displays a symbol on the VFD
    /// </summary>
    /// <param name="vfdsymbol"></param>
    /// <param name="visible"></param>
    public void updateSymbol(VFDSymbols vfdsymbol, bool visible)
    {
      _SymbolsVisible[(int) vfdsymbol] = visible;
      _updateSymbols();
    }

    private void _updateSymbols()
    {
      byte[] WriteBuffer = new byte[9];
      int tmp = _SymbolsVisible.Data;

      WriteBuffer[2] = (byte) (tmp >> 8);
      WriteBuffer[3] = (byte) (tmp & 0xff);

      if (_Volume >= 0) // for Volume<0 don't show Volume symbols
      {
        WriteBuffer[3] |= 0x04;
        for (int j = 3; j < 9; j++)
        {
          if ((float) _Volume/4 > j - 3)
          {
            WriteBuffer[j] = (byte) (WriteBuffer[j] | 0x08);
          }
          if ((float) _Volume/4 - 0.5 > j - 3)
          {
            WriteBuffer[j] = (byte) (WriteBuffer[j] | 0x80);
          }
        }
      }
      if (Process.GetProcessesByName("ehshell").Length > 0)
      {
        return; //exit if xpMCE is running
      }
      Hid.OutputReport myOutputReport = new Hid.OutputReport();
      myOutputReport.Write(new byte[] {0x00, 0x10, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(WriteBuffer, _HIDHandle);
    }

    /// <summary>
    /// sets the Volume symbols on the VFD
    /// </summary>
    /// <param name="volume"></param>
    public void setVolume(int volume) // 12 Balken
    {
      _Volume = volume;
      _updateSymbols();
    }

    /// <summary>
    /// Writes with big letter to VFD
    /// </summary>
    /// <param name="text"></param>
    public void writeMainScreen(string text)
    {
      byte[] WriteBuffer = new byte[18];
      byte[] WB1 = new byte[9];
      byte[] WB2 = new byte[9];
      if (Process.GetProcessesByName("ehshell").Length > 0)
      {
        return; //exit if xpMCE is running
      }
      for (int i = 0; i < text.Length; i++)
      {
        WriteBuffer[i + 1] = (byte) (text[i]);
      }
      for (int i = 1; i < 9; i++)
      {
        WB1[i] = WriteBuffer[i];
        WB2[i] = WriteBuffer[i + 8];
      }

      Hid.OutputReport myOutputReport = new Hid.OutputReport();
      myOutputReport.Write(new byte[] {0x00, 0x06, 0x05, 0x02, 0x01, 0x05, 0x00, 0x08, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x06, 0x03, 0x01, 0x0A, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(WB1, _HIDHandle); // Text 1-8
      myOutputReport.Write(WB2, _HIDHandle); // Text 9-14
    }

    /// <summary>
    /// Writes with small letter in 2 lines;
    /// Second line scrolls to the left if longer than 18 chars
    /// </summary>
    /// <param name="LineNo"></param>
    /// <param name="text"></param>
    public void writeLine(int LineNo, string text)
    {
      byte[] WriteBuffer = new byte[50];
      byte[] WB1 = new byte[9];
      byte[] WB2 = new byte[9];
      byte[] WB3 = new byte[9];
      byte[] WB4 = new byte[9];
      byte[] WB5 = new byte[9];
      byte[] WB6 = new byte[9];
      if (Process.GetProcessesByName("ehshell").Length > 0)
      {
        return; //exit if xpMCE is running
      }
      for (int i = 0; i < text.Length; i++)
      {
        WriteBuffer[i + 1] = (byte) (text[i]);
      }
      for (int i = 1; i < 9; i++)
      {
        WB1[i] = WriteBuffer[i];
        WB2[i] = WriteBuffer[i + 8];
        WB3[i] = WriteBuffer[i + 16];
        WB4[i] = WriteBuffer[i + 24];
        WB5[i] = WriteBuffer[i + 32];
        WB6[i] = WriteBuffer[i + 40];
      }

      Hid.OutputReport myOutputReport = new Hid.OutputReport();
      //7 0x06
      //6 0x03
      //5 1=obli 3=unli 2=obre 4=unre scroll  hängt auch von der Vergangenheit ab???
      //4 text.Length
      //3 
      //2 0x00
      //1 0x00
      //0 0x00
      if (LineNo == 1)
      {
        // erase line 1               
        myOutputReport.Write(new byte[] {0x00, 0x04, 0x04, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
        // init line 1
        myOutputReport.Write(new byte[] {0x00, 0x06, 0x03, 0x01, (byte) text.Length, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      }
      else
      {
        // erase line 2               
        myOutputReport.Write(new byte[] {0x00, 0x04, 0x04, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
        // init line 2                
        myOutputReport.Write(new byte[] {0x00, 0x06, 0x03, 0x03, (byte) text.Length, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      }
      // write text
      myOutputReport.Write(WB1, _HIDHandle); // Text 1-8
      if (text.Length > 8)
      {
        myOutputReport.Write(WB2, _HIDHandle); // Text 9-14
      }
      if (text.Length > 16)
      {
        myOutputReport.Write(WB3, _HIDHandle); // Text 9-16
      }
      if (text.Length > 24)
      {
        myOutputReport.Write(WB4, _HIDHandle); // Text 9-16
      }
      if (text.Length > 32)
      {
        myOutputReport.Write(WB5, _HIDHandle); // Text 9-16
      }
      if (text.Length > 40)
      {
        myOutputReport.Write(WB6, _HIDHandle); // Text 9-16
      }
      // write trailer
      myOutputReport.Write(new byte[] {0x00, 0x06, 0x05, 0x02, 0x02, 0x05, 0x00, 0x80, 0x00}, _HIDHandle);
      if (text.Length > 18)
      {
        myOutputReport.Write(new byte[] {0x00, 0x06, 0x05, 0x02, 0x02, 0x05, 0x00, 0x84, 0x00}, _HIDHandle);
      }
    }

    /// <summary>
    /// Clears the text lines
    /// </summary>
    public void clearLines()
    {
      writeLine(1, "");
      writeLine(2, "");
    }

    /// <summary>
    /// Clears the VFD
    /// </summary>
    public void clearScreen()
    {
      if (Process.GetProcessesByName("ehshell").Length > 0)
      {
        return; //exit if xpMCE is running
      }
      Hid.OutputReport myOutputReport = new Hid.OutputReport();
      myOutputReport.Write(new byte[] {0x00, 0x04, 0x04, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      //clear screen
      myOutputReport.Write(new byte[] {0x00, 0x04, 0x04, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x10, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
      myOutputReport.Write(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, _HIDHandle);
    }

    /// <summary>
    /// Perform actions that must execute when the program ends.
    /// Disconnects the USB
    /// </summary>
    public void Shutdown()
    {
      try
      {
        // Close open handles to the device.
        if (_HIDHandle != 0)
        {
          FileIOApiDeclarations.CloseHandle(_HIDHandle);
          Debug.WriteLine(Debugging.ResultOfAPICall("CloseHandle (HIDHandle)"));
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
  }
}