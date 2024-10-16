#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Runtime.InteropServices;

namespace MediaPortal.Mixer
{
  internal class MixerNativeMethods
  {
    #region Constructors

    private MixerNativeMethods() {}

    #endregion Constructors

    #region Methods

    [DllImport("winmm.dll")]
    public static extern MixerError mixerClose(IntPtr handle);

    [DllImport("winmm.dll")]
    public static extern MixerError mixerGetControlDetailsA(IntPtr handle, MixerControlDetails mixerControlDetails,
                                                            int fdwDetails);

//		[DllImport("winmm.dll")] 
//		public static extern int mixerGetDevCapsA(int uMxId, MIXERCAPS pmxcaps, int cbmxcaps); 

//		[DllImport("winmm.dll")] 
//		public static extern int mixerGetID(IntPtr handle, int pumxID, int fdwId);

    [DllImport("winmm.dll")]
    public static extern MixerError mixerGetLineControlsA(IntPtr handle, MixerLineControls mixerLineControls,
                                                          MixerLineControlFlags flags);

    [DllImport("winmm.dll")]
    public static extern MixerError mixerGetLineInfoA(IntPtr handle, ref MixerLine mixerLine, MixerLineFlags flags);

//		[DllImport("winmm.dll")]
//		public static extern int mixerGetNumDevs();

    [DllImport("winmm.dll")]
    public static extern MixerError mixerMessage(int hmx, int uMsg, int dwParam1, int dwParam2);

    [DllImport("winmm.dll")]
    public static extern MixerError mixerOpen(ref IntPtr handle, int index, IntPtr callbackWindowHandle, int dwInstance,
                                              MixerFlags flags);

    [DllImport("winmm.dll")]
    public static extern MixerError mixerOpen(ref IntPtr handle, int index, MixerCallback callback, int dwInstance,
                                              MixerFlags flags);

    [DllImport("winmm.dll")]
    public static extern MixerError mixerSetControlDetails(IntPtr handle, MixerControlDetails mixerControlDetails,
                                                           int fdwDetails);

    #endregion Methods

    #region Structures

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct MixerControl
    {
      #region Fields

      public int Size;
      public int ControlId;
      public MixerControlType ControlType;
      public int fdwControl;
      public int MultipleItems;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
      public string ShortName;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string Name;

      //  union {
      //  struct {
      //    LONG lMinimum;
      //    LONG lMaximum;
      //  } DUMMYSTRUCTNAME;
      //  struct {
      //    DWORD dwMinimum;
      //    DWORD dwMaximum;
      //  } DUMMYSTRUCTNAME2;
      //  DWORD dwReserved[6];
      //} Bounds;
      public int Minimum;
      public int Maximum;
      public int BoundsReserved2;
      public int BoundsReserved3;
      public int BoundsReserved4;
      public int BoundsReserved5;


      //  union {
      //    DWORD cSteps;
      //    DWORD cbCustomData;
      //    DWORD dwReserved[6];
      //} Metrics;
      public int MetricsReserved0;
      public int MetricsReserved1;
      public int MetricsReserved2;
      public int MetricsReserved3;
      public int MetricsReserved4;
      public int MetricsReserved5;

      #endregion Fields
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public class MixerControlDetails : IDisposable
    {
      #region Constructors

      public MixerControlDetails(int controlId)
      {
        this.Size = Marshal.SizeOf(typeof(MixerControlDetails));
        this.ControlId = controlId;
        this.Data = Marshal.AllocCoTaskMem(4);
        this.Channels = 1;
        this.Item = IntPtr.Zero;
        this.DataSize = Marshal.SizeOf(4);
      }

      #endregion Constructors

      #region Methods

      public void Dispose()
      {
        if (this.Data != IntPtr.Zero)
          Marshal.FreeCoTaskMem(this.Data);
      }

      #endregion Methods

      #region Fields

      public int Size;
      public int ControlId;
      public int Channels;

      //union {
      //    HWND  hwndOwner;
      //    DWORD cMultipleItems;
      //  } DUMMYUNIONNAME;
      public IntPtr Item;

      public int DataSize;
      public IntPtr Data;

      #endregion Fields
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct MixerLine
    {
      #region Constructors

      public MixerLine(MixerComponentType componentType)
      {
        this.Size = Marshal.SizeOf(typeof(MixerLine));
        this.Destination = 0;
        this.Source = 0;
        this.LineId = 0;
        this.Status = MixerLineStatusFlags.Disconnected;
        this.dwUser = IntPtr.Zero;
        this.ComponentType = componentType;
        this.Channels = 0;
        this.Connections = 0;
        this.Controls = 0;
        this.ShortName = string.Empty;
        this.Name = string.Empty;
        this.Type = MixerLineTargetType.None;
        this.DeviceId = 0;
        this.ManufacturerId = 0;
        this.ProductId = 0;
        this.DriverVersion = 0;
        this.ProductName = string.Empty;
      }

      #endregion Constructors

      #region Fields

      public int Size;
      public int Destination;
      public int Source;
      public int LineId;
      public MixerLineStatusFlags Status;
      public IntPtr dwUser;
      public MixerComponentType ComponentType;
      public int Channels;
      public int Connections;
      public int Controls;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
      public string ShortName;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string Name;

      public MixerLineTargetType Type;
      public int DeviceId;
      public short ManufacturerId;
      public short ProductId;
      public int DriverVersion;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string ProductName;

      #endregion Fields
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public class MixerLineControls : IDisposable
    {
      #region Constructors

      public MixerLineControls(int lineId, MixerControlType controlType)
      {
        this.Size = Marshal.SizeOf(typeof(MixerLineControls));
        this.LineId = lineId;
        this.ControlType = Convert.ToUInt32(controlType);
        this.Controls = 1;
        this.DataSize = Marshal.SizeOf(typeof(MixerControl));
        this.Data = Marshal.AllocCoTaskMem(this.DataSize);
      }

      #endregion Constructors

      #region Methods

      public void Dispose()
      {
        if (this.Data != IntPtr.Zero)
          Marshal.FreeCoTaskMem(this.Data);
      }

      #endregion Methods

      #region Fields

      public int Size;
      public int LineId;
      public uint ControlType;
      public int Controls;
      public int DataSize;
      public IntPtr Data;

      #endregion Fields
    }

    #endregion Structures
  }
}