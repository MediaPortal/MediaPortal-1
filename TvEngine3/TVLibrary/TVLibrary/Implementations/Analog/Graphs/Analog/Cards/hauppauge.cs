#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal - diehard2
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
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;


namespace TvLibrary.Implementations.Analog
{

  ///<summary>
  /// Hauppauge quality control
  ///</summary>
  public class Hauppauge : IDisposable
  {
    private bool disposed;

    [DllImport("kernel32.dll")]
    internal static extern IntPtr LoadLibrary(String dllname);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, String procname);

    [DllImport("kernel32.dll")]
    internal static extern bool FreeLibrary(IntPtr hModule);

    IntPtr hauppaugelib = IntPtr.Zero;

    delegate int Init(IBaseFilter capture, [MarshalAs(UnmanagedType.LPStr)] string tuner);
    delegate int DeInit();
    delegate bool IsHauppauge();
    delegate int SetVidBitRate(int maxkbps, int minkbps, bool isVBR);
    delegate int GetVidBitRate(out int maxkbps, out int minkbps, out bool isVBR);
    delegate int SetAudBitRate(int bitrate);
    delegate int GetAudBitRate(out int bitrate);
    delegate int SetStreamType(int stream);
    delegate int GetStreamType(out int stream);
    delegate int SetDNRFilter(bool onoff);

    readonly Init _Init;
    readonly DeInit _DeInit;
    readonly IsHauppauge _IsHauppauge;
    readonly SetVidBitRate _SetVidBitRate;
    readonly GetVidBitRate _GetVidBitRate;
    readonly SetAudBitRate _SetAudBitRate;
    readonly GetAudBitRate _GetAudBitRate;
    readonly SetStreamType _SetStreamType;
    readonly GetStreamType _GetStreamType;
    readonly SetDNRFilter _SetDNRFilter;

    readonly HResult hr;

    //Initializes the Hauppauge interfaces

    /// <summary>
    /// Constructor: Require the Hauppauge capture filter, and the deviceid for the card to be passed in
    /// </summary>
    public Hauppauge(IBaseFilter filter, string tuner)
    {
      try
      {
        //Don't create the class if we don't have any filter;

        if (filter == null)
        {
          return;
        }

        //Load Library
        hauppaugelib = LoadLibrary("hauppauge.dll");

        //Get Proc addresses, and set the delegates for each function
        IntPtr procaddr = GetProcAddress(hauppaugelib, "Init");
        _Init = (Init)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(Init));

        procaddr = GetProcAddress(hauppaugelib, "DeInit");
        _DeInit = (DeInit)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(DeInit));

        procaddr = GetProcAddress(hauppaugelib, "IsHauppauge");
        _IsHauppauge = (IsHauppauge)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(IsHauppauge));

        procaddr = GetProcAddress(hauppaugelib, "SetVidBitRate");
        _SetVidBitRate = (SetVidBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetVidBitRate));

        procaddr = GetProcAddress(hauppaugelib, "GetVidBitRate");
        _GetVidBitRate = (GetVidBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(GetVidBitRate));

        procaddr = GetProcAddress(hauppaugelib, "SetAudBitRate");
        _SetAudBitRate = (SetAudBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetAudBitRate));

        procaddr = GetProcAddress(hauppaugelib, "GetAudBitRate");
        _GetAudBitRate = (GetAudBitRate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(GetAudBitRate));

        procaddr = GetProcAddress(hauppaugelib, "SetStreamType");
        _SetStreamType = (SetStreamType)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetStreamType));

        procaddr = GetProcAddress(hauppaugelib, "GetStreamType");
        _GetStreamType = (GetStreamType)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(GetStreamType));

        procaddr = GetProcAddress(hauppaugelib, "SetDNRFilter");
        _SetDNRFilter = (SetDNRFilter)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(SetDNRFilter));

        //Hack
        //The following is strangely necessary when using delegates instead of P/Invoke - linked to MP using utf-8
        //Hack

        byte[] encodedstring = Encoding.UTF32.GetBytes(tuner);
        string card = Encoding.Unicode.GetString(encodedstring);

        hr = new HResult(_Init(filter, card));
        Log.Log.WriteFile("Hauppauge Quality Control Initializing " + hr.ToDXString());
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Init failed " + ex.Message);
      }
    }

    /// <summary>
    /// Toggles Dynamic Noise Reduction on/off
    /// </summary>
    public bool SetDNR(bool onoff)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _SetDNRFilter(onoff);
            return true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge SetDNR failed " + ex.Message);
      }
      return false;

    }

    /// <summary>
    /// Get the video bit rate
    /// </summary>
    public bool GetVideoBitRate(out int minKbps, out int maxKbps, out bool isVBR)
    {
      maxKbps = minKbps = -1;
      isVBR = false;
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _GetVidBitRate(out maxKbps, out minKbps, out isVBR);
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Error GetBitrate " + ex.Message);
      }

      return true;
    }

    /// <summary>
    /// Sets the video bit rate
    /// </summary>
    public bool SetVideoBitRate(int minKbps, int maxKbps, bool isVBR)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            hr.Set(_SetVidBitRate(maxKbps, minKbps, isVBR));
            Log.Log.WriteFile("Hauppauge Set Bit Rate " + hr.ToDXString());
            return true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Set Vid Rate " + ex.Message);
      }
      return false;
    }

    /// <summary>
    /// Get the audio bit rate
    /// </summary>
    public bool GetAudioBitRate(out int Kbps)
    {
      Kbps = -1;
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _GetAudBitRate(out Kbps);
            return true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Get Audio Bitrate " + ex.Message);
      }
      return false;
    }

    /// <summary>
    /// Set the audio bit rate
    /// </summary>
    public bool SetAudioBitRate(int Kbps)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _SetAudBitRate(Kbps);
            return true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Set Audio Bit Rate " + ex.Message);
      }
      return false;
    }

    /// <summary>
    /// Get the stream type
    /// </summary>
    public bool GetStream(out int stream)
    {
      stream = -1;
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _GetStreamType(out stream);
            return true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Get Stream " + ex.Message);
      }
      return false;
    }

    /// <summary>
    /// Set the stream type
    /// </summary>
    public bool SetStream(int stream)
    {
      try
      {
        if (hauppaugelib != IntPtr.Zero)
        {
          if (_IsHauppauge())
          {
            _SetStreamType(103);
            return true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Hauppauge Set Stream Type " + ex.Message);
      }
      return false;
    }

    #region IDisposable Members

    /// <summary>
    /// Deallocate Hauppauge class
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Deallocate Hauppauge class
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          // Dispose managed resources if any
        }
        try
        {
          if (hauppaugelib != IntPtr.Zero)
          {
            if (_IsHauppauge())
              _DeInit();

            FreeLibrary(hauppaugelib);
            hauppaugelib = IntPtr.Zero;
          }
        } catch (Exception ex)
        {
          Log.Log.WriteFile("Hauppauge exception " + ex.Message);
          Log.Log.WriteFile("Hauppauge Disposed hcw.txt");
        }
      }
      disposed = true;
    }
    /// <summary>
    /// Destructor
    /// </summary>
    ~Hauppauge()
    {
      Dispose(false);
    }
    #endregion
  }
}
