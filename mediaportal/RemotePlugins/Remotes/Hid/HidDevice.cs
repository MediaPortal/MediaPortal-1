using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Win32;

namespace Hid
{
  /// <summary>
  ///   Represent a HID device.
  /// </summary>
  public class HidDevice
  {
    /// <summary>
    ///   Class constructor will fetch this object properties from HID sub system.
    /// </summary>
    /// <param name="hRawInputDevice">
    ///   Device Handle as provided by RAWINPUTHEADER.hDevice, typically accessed as
    ///   rawinput.header.hDevice
    /// </param>
    public HidDevice(IntPtr hRawInputDevice)
    {
      //Fetch various information defining the given HID device
      Name = RawInput.GetDeviceName(hRawInputDevice);

      //Open our device from the device name/path
      var handle = Function.CreateFile(Name,
        FileAccess.NONE,
        FileShare.FILE_SHARE_READ | FileShare.FILE_SHARE_WRITE,
        IntPtr.Zero,
        CreationDisposition.OPEN_EXISTING,
        FileFlagsAttributes.FILE_FLAG_OVERLAPPED,
        IntPtr.Zero
        );

      if (handle.IsInvalid)
      {
        Debug.WriteLine("Failed to CreateFile from device name " + Marshal.GetLastWin32Error());
      }
      else
      {
        //Get manufacturer string
        var manufacturerString = new StringBuilder(256);
        if (Function.HidD_GetManufacturerString(handle, manufacturerString, manufacturerString.Capacity))
        {
          Manufacturer = manufacturerString.ToString();
        }

        //Get product string
        var productString = new StringBuilder(256);
        if (Function.HidD_GetProductString(handle, productString, productString.Capacity))
        {
          Product = productString.ToString();
        }

        //Get attributes
        var attributes = new HIDD_ATTRIBUTES();
        if (Function.HidD_GetAttributes(handle, ref attributes))
        {
          VendorId = attributes.VendorID;
          ProductId = attributes.ProductID;
          Version = attributes.VersionNumber;
        }

        handle.Close();
      }
    }

    public string Name { get; private set; }
    public string Manufacturer { get; private set; }
    public string Product { get; private set; }
    public ushort VendorId { get; private set; }
    public ushort ProductId { get; private set; }
    public ushort Version { get; private set; }

    /// <summary>
    ///   Print information about this device to our debug output.
    /// </summary>
    public void DebugWrite()
    {
      Debug.WriteLine(
        "================ HID =========================================================================================");
      Debug.WriteLine("==== Name: " + Name);
      Debug.WriteLine("==== Manufacturer: " + Manufacturer);
      Debug.WriteLine("==== Product: " + Product);
      Debug.WriteLine("==== VendorID: 0x" + VendorId.ToString("X4"));
      Debug.WriteLine("==== ProductID: 0x" + ProductId.ToString("X4"));
      Debug.WriteLine("==== Version: " + Version);
      Debug.WriteLine(
        "==============================================================================================================");
    }

    /// <summary>
    /// Create a human readable string out of this object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string res = "====== HidDevice ====\n";
      res += "== Name: " + Name + "\n";
      res += "== Manufacturer: " + Manufacturer + "\n";
      res += "== Product: " + Product + "\n";
      res += "== VendorID: 0x" + VendorId.ToString("X4") + "\n";
      res += "== ProductID: 0x" + ProductId.ToString("X4") + "\n";
      res += "== Version: " + Version + "\n";
      res += "=====================\n";
      return res;
    }

  }
}