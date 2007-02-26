#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal - diehard2
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
using MediaPortal.GUI.Library;
using DirectShowLib;
using MediaPortal.Util;

namespace DShowNET
{

	public class Hauppauge:IDisposable
	{
    private bool disposed = false;
    [DllImport("hauppauge.dll", EntryPoint = "Init", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int Init(IBaseFilter tuner);
    [DllImport("hauppauge.dll", EntryPoint = "IsHauppauge", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern bool IsHauppauge();
    [DllImport("hauppauge.dll", EntryPoint = "SetVidBitRate", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int SetVidBitRate(int maxkbps, int minkbps, bool isVBR);
    [DllImport("hauppauge.dll", EntryPoint = "DeInit", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int DeInit();
    [DllImport("hauppauge.dll", EntryPoint = "GetVidBitRate", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int GetVidBitRate(out int maxkbps, out int minkbps, out bool isVBR);
    [DllImport("hauppauge.dll", EntryPoint = "SetAudBitRate", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int SetAudBitRate(int bitrate);
    [DllImport("hauppauge.dll", EntryPoint = "GetAudBitRate", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int GetAudBitrate(out int bitrate);
    [DllImport("hauppauge.dll", EntryPoint = "SetStreamType", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int SetStreamType(int stream);
    [DllImport("hauppauge.dll", EntryPoint = "GetStreamType", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int GetStreamType(out int stream);
    [DllImport("hauppauge.dll", EntryPoint = "SetDNRFilter", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int SetDNRFilter(bool onoff);

    HResult hr; 
    
    //Initializes the Hauppauge interfaces

		public Hauppauge(IBaseFilter filter)
		{
      try
      {
        hr = new HResult(Init(filter));
        Log.Info("Hauppauge Quality Control Initializing " + hr.ToDXString());
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Init failed "+ ex.Message);
      }
		}

    public bool SetDNR(bool onoff)
    {
      try
      {
        if (IsHauppauge())
        {
          SetDNRFilter(onoff);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge SetDNR failed " + ex.Message);
      }
      return false;

    }
   
		public bool GetVideoBitRate(out int minKbps, out int maxKbps,out bool isVBR)
		{
      maxKbps = minKbps = -1;
      isVBR = false;
      try
      {
        if (IsHauppauge())
        {
          GetVidBitRate(out maxKbps, out minKbps, out isVBR);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Error GetBitrate " + ex.Message);
      }

			return true;
		}

		public bool SetVideoBitRate(int minKbps, int maxKbps,bool isVBR)
		{
      try
      {
        if (IsHauppauge())
        {
          hr.Set(SetVidBitRate(maxKbps, minKbps, isVBR));
          Log.Info("Hauppauge Set Bit Rate " + hr.ToDXString());
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Set Vid Rate " + ex.Message);
      }
      return false;
		}

    public bool GetAudioBitRate(out int Kbps)
    {
      Kbps = -1;
      try
      {
        if (IsHauppauge())
        {
          GetAudBitrate(out Kbps);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Get Audio Bitrate " + ex.Message);
      }
      return false;
    }

    public bool SetAudioBitRate(int Kbps)
    {
      try
      {
        if (IsHauppauge())
        {
          SetAudBitRate(Kbps);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Set Audio Bit Rate " + ex.Message);
      }
      return false;
    }

    public bool GetStream(out int stream)
    {
      stream = -1;
      try
      {
        if (IsHauppauge())
        {
          GetStreamType(out stream);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Hauppauge Get Stream " + ex.Message);
      }
      return false;
    }

    public bool SetStream(int stream)
    {
      try
      {
        if (IsHauppauge())
        {
          SetStreamType(103);
          return true;
        }
      }
      catch(Exception ex)
      {
        Log.Error("Hauppauge Set Stream Type " + ex.Message);
      }
      return false;
    }
   
    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
        }
        DeInit();
        Log.Info("Hauppauge Disposed hcw.txt");
      }
      disposed = true;
    }

    ~Hauppauge()      
   {
      Dispose(false);
   }
    #endregion
  }
}