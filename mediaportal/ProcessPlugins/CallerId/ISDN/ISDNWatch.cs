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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.CallerId
{
  public class ISDNWatch
  {
    [StructLayout(LayoutKind.Sequential)]
    private struct capiRequest
    {
      public short Length;
      public short ApplicationId;
      public byte Command;
      public byte SubCommand;
      public short MessageNumber;
      public int Controller;
      public int InfoMask;
      public int CIPMask1;
      public int CIPMask2;
      public byte CallingParty;
      public byte CallingPartySub;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct capiMessageHeader
    {
      public ushort Length;
      public ushort ApplicationId;
      public byte Command;
      public byte SubCommand;
      public ushort MessageNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct capiConnectInd
    {
      public uint PLCI;
      public ushort CIP;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)] public byte[] buffer;
    }

    [DllImport("CAPI2032.DLL")]
    private static extern int CAPI_INSTALLED();

    [DllImport("CAPI2032.DLL")]
    private static extern int CAPI_REGISTER(
      int MessageBufferSize,
      int MaxLogicalConnection,
      int MaxBDataBlocks,
      int MaxBDataLen,
      ref int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    private static extern int CAPI_RELEASE(
      int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    private static extern void CAPI_WAIT_FOR_SIGNAL(
      int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    private static extern int CAPI_PUT_MESSAGE(
      int ApplicationID,
      [MarshalAs(UnmanagedType.AsAny)] object CAPIMessage);

    [DllImport("CAPI2032.DLL")]
    private static extern int CAPI_GET_MESSAGE(
      int ApplicationID,
      ref IntPtr CapiBufferPointer);

    [DllImport("kernel32")]
    private static extern void RtlMoveMemory(
      ref capiMessageHeader Destination,
      IntPtr Source,
      int Length);

    [DllImport("kernel32")]
    private static extern void RtlMoveMemory(
      ref capiConnectInd Destination,
      IntPtr Source,
      int Length);

    [DllImport("tapi32.dll")]
    private static extern int tapiGetLocationInfoW(
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder CountryCode,
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder AereaCode);

    public delegate void EventHandler(string CallerId);

    public static event EventHandler CidReceiver = null;

    private bool stopThread = false;
    private const int HeaderLength = 8;
    private const int CAPI_CONNECT = 0x02;
    private const int CAPI_IND = 0x82;

    public ISDNWatch()
    {
    }

    public class LocationInfo
    {
      public string CountryCode, AreaCode;

      public LocationInfo()
      {
        CountryCode = "";
        AreaCode = "";
      }
    }

    public static bool CapiInstalled
    {
      get
      {
        int result = -1;

        try
        {
          result = CAPI_INSTALLED();
        }
        catch (Exception)
        {
        }

        if (result == 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    public void Start()
    {
      Thread watchThread = new Thread(new ThreadStart(WatchThread));
      watchThread.Name = "CAPI Monitor";
      watchThread.IsBackground = true;
      watchThread.Start();
    }

    public void Stop()
    {
      stopThread = true;
    }

    private void WatchThread()
    {
      int applicationId = 0;

      // Registering with CAPI
      int capiResult = CAPI_REGISTER(3072, 2, 7, 2048, ref applicationId);
      if (capiResult != 0)
      {
        Log.Info("ISDN: Application cannot register with CAPI");
      }
      else
      {
        Log.Info("ISDN: Application registered with CAPI ({0})", applicationId);

        capiRequest capiRequest = new capiRequest();
        capiRequest.Length = 26;
        capiRequest.ApplicationId = (short) applicationId;
        capiRequest.Command = 0x0005;
        capiRequest.SubCommand = 0x0080;
        capiRequest.MessageNumber = 1;
        capiRequest.Controller = 1;
        capiRequest.InfoMask = 0x0000;
        capiRequest.CIPMask1 = 1;
        capiRequest.CIPMask2 = 0x0000;
        capiRequest.CallingParty = 0x0000;
        capiRequest.CallingPartySub = 0x0000;
        capiResult = CAPI_PUT_MESSAGE(applicationId, capiRequest);

        if (capiResult != 0)
        {
          Log.Info("ISDN: CAPI signaling cannot be activated");
        }
        else
        {
          Log.Info("ISDN: CAPI signaling activated");

          while (!stopThread) // Waiting for signal and signal-processing
          {
            string callerId = null;
            string calledId = null;
            string logBuffer = "";
            capiMessageHeader messageHeader = new capiMessageHeader();
            IntPtr capiBufferPointer = new IntPtr();

            //        CAPI_WAIT_FOR_SIGNAL(applicationId);
            if (CAPI_GET_MESSAGE(applicationId, ref capiBufferPointer) == 0)
            {
              RtlMoveMemory(ref messageHeader, capiBufferPointer, HeaderLength);

              Log.Info("ISDN: CAPI command: 0x{0} / 0x{1}", messageHeader.Command.ToString("X2"),
                       messageHeader.SubCommand.ToString("X2"));

              if ((messageHeader.Command == CAPI_CONNECT) && (messageHeader.SubCommand == CAPI_IND))
              {
                capiConnectInd ConnectInd = new capiConnectInd();
                RtlMoveMemory(ref ConnectInd, (IntPtr) (capiBufferPointer.ToInt32() + HeaderLength),
                              (messageHeader.Length - HeaderLength));

                for (int i = 99; i >= 0; i--)
                {
                  if ((logBuffer.Length != 0) || (ConnectInd.buffer[i] != 0))
                  {
                    if ((ConnectInd.buffer[i] < 48) || (ConnectInd.buffer[i] > 57))
                    {
                      logBuffer = "(" + ConnectInd.buffer[i] + ")" + logBuffer;
                    }
                    else
                    {
                      logBuffer = (char) ConnectInd.buffer[i] + logBuffer;
                    }
                  }
                }

                Log.Info("ISDN: Buffer: {0}", logBuffer);

                int lengthCalledId = ConnectInd.buffer[0];
                int lengthCallerId = ConnectInd.buffer[lengthCalledId + 1];

                for (int i = 2; i < (lengthCalledId + 1); i++)
                {
                  calledId = calledId + (char) ConnectInd.buffer[i];
                }
                for (int i = (lengthCalledId + 4); i < (lengthCallerId + lengthCalledId + 2); i++)
                {
                  callerId = callerId + (char) ConnectInd.buffer[i];
                }

                if (callerId != null)
                {
                  callerId = callerId.TrimStart('0');
                  Log.Info("ISDN: stripped {0} leading zeros", lengthCallerId - callerId.Length - 2);
                }

                if (ConnectInd.buffer[lengthCalledId + 2] == 17) // International call
                {
                  callerId = "+" + callerId;
                }

                Log.Info("ISDN: CalledID: {0}", calledId);
                Log.Info("ISDN: CallerID: {0}", callerId);

                CidReceiver(callerId);
              }
            }
            Thread.Sleep(200);
          }

          // Release CAPI
          if (CAPI_RELEASE(applicationId) == 0)
          {
            stopThread = false;
            Log.Info("ISDN: CAPI released ({0})", applicationId);
          }
          else
          {
            Log.Info("ISDN: CAPI cannot be released");
          }
        }
      }
    }

    public static LocationInfo GetLocationInfo()
    {
      StringBuilder countryCode = new StringBuilder(8);
      StringBuilder areaCode = new StringBuilder(8);
      LocationInfo locationInfo = new LocationInfo();
      if (tapiGetLocationInfoW(countryCode, areaCode) == 0)
      {
        locationInfo.CountryCode = countryCode.ToString();
        locationInfo.AreaCode = areaCode.ToString();
        if (locationInfo.AreaCode[0] == '0')
        {
          locationInfo.AreaCode = locationInfo.AreaCode.Remove(0, 1);
        }
      }
      else
      {
        Log.Info("ISDN: Can't get TAPI location info!!!");
      }

      return locationInfo;
    }
  }
}