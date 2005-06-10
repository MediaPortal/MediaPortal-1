using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using System.Threading;
using Microsoft.Win32;
using System.Text;

namespace ProcessPlugins.CallerId
{
  public class ISDNWatch
  {
    [StructLayout(LayoutKind.Sequential)]
      struct capiRequest
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
      struct capiMessageHeader
    {
      public ushort Length;
      public ushort ApplicationId;
      public byte Command;
      public byte SubCommand;
      public ushort MessageNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
      struct capiConnectInd 
    {
      public uint PLCI;
      public ushort CIP;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst=100)]
      public string buffer;
    }

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_REGISTER(
      int MessageBufferSize,
      int MaxLogicalConnection,
      int MaxBDataBlocks,
      int MaxBDataLen,
      ref int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_RELEASE(
      int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    static extern void CAPI_WAIT_FOR_SIGNAL(
      int ApplicationId);

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_PUT_MESSAGE(
      int ApplicationID,
      [MarshalAs(UnmanagedType.AsAny)] object CAPIMessage );

    [DllImport("CAPI2032.DLL")]
    static extern int CAPI_GET_MESSAGE(
      int ApplicationID,
      ref IntPtr CapiBufferPointer);

    [DllImport("kernel32")]
    static extern void RtlMoveMemory(
      ref capiMessageHeader Destination,
      IntPtr Source,
      int Length);

    [DllImport("kernel32")]
    static extern void RtlMoveMemory(
      ref capiConnectInd Destination,
      IntPtr Source,
      int Length);

    [DllImport("tapi32.dll")]
    static extern int tapiGetLocationInfoW(
      [MarshalAs(UnmanagedType.LPTStr)]
      StringBuilder CountryCode,
      [MarshalAs(UnmanagedType.LPTStr)]
      StringBuilder AereaCode);

    public delegate void EventHandler(string CallerId);
    static public event EventHandler CidReceiver = null;
    
    bool stopThread = false;
    const int HeaderLength = 8;
    const int CAPI_CONNECT = 0x02;
    const int CAPI_IND = 0x82;

    public void Start()
    {
      Thread watchThread = new Thread(new ThreadStart(WatchThread));
      watchThread.Name = "CAPI Monitoring";
      watchThread.Start();
    }

    public void Stop()
    {
      stopThread = true;
    }

    void WatchThread()
    {
      try
      {
        int applicationId = 0;

        // Registering with CAPI
        int capiResult = CAPI_REGISTER(3072, 2, 7, 2048, ref applicationId);
        if (capiResult != 0)
          Log.Write("ISDN: Application cannot register with CAPI");
        else
        {
          Log.Write("ISDN: Application registered with CAPI ({0})", applicationId);

          capiRequest capiRequest = new capiRequest();
          capiRequest.Length = 26;
          capiRequest.ApplicationId = (short)applicationId;
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
            Log.Write("ISDN: CAPI signaling cannot be activated");
          else
          {
            Log.Write("ISDN: CAPI signaling activated");
          }
        }

        // Waiting for signal and signal-processing
        string CallerId = null;
        capiMessageHeader MessageHeader = new capiMessageHeader();
        IntPtr capiBufferPointer = new IntPtr();
        while (!stopThread)
        {
          //        CAPI_WAIT_FOR_SIGNAL(applicationId);
          if (CAPI_GET_MESSAGE(applicationId, ref capiBufferPointer) == 0)
          {
            RtlMoveMemory(ref MessageHeader, capiBufferPointer, HeaderLength);
            if ((MessageHeader.Command == CAPI_CONNECT) && (MessageHeader.SubCommand == CAPI_IND))
            {
              capiConnectInd ConnectInd = new capiConnectInd();
              RtlMoveMemory (ref ConnectInd, (IntPtr)(capiBufferPointer.ToInt32() + HeaderLength), (MessageHeader.Length - HeaderLength));
              int lengthCalledId = ConnectInd.buffer[0];
              string CalledId = ConnectInd.buffer.Substring(2, (lengthCalledId - 1));
              int lengthCallerId = ConnectInd.buffer[lengthCalledId + 1];
              CallerId = ConnectInd.buffer.Substring((lengthCalledId + 4), (lengthCallerId - 2));

              if (ConnectInd.buffer[lengthCalledId+2] != 33)
                CallerId = "+" + CallerId;

              Log.Write("ISDN: CalledID: {0}", CalledId);
              Log.Write("ISDN: CallerID: {0}", CallerId);

              CidReceiver(CallerId);
            }
          }
          Thread.Sleep(200);
        }

        // Release CAPI
        if (CAPI_RELEASE(applicationId) == 0)
        {
          stopThread = false;
          Log.Write("ISDN: CAPI released ({0})", applicationId);
        }
        else
          Log.Write("ISDN: CAPI cannot be released");
      }
      catch (System.DllNotFoundException)
      {
        stopThread = false;
        Log.Write("ISDN: Can't find CAPI2032.dll");
        return;
      }
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

    public static LocationInfo GetLocationInfo()
    {
      StringBuilder countryCode = new StringBuilder(8);
      StringBuilder areaCode = new StringBuilder(8);
      LocationInfo locationInfo = new LocationInfo();
      if (tapiGetLocationInfoW(countryCode, areaCode) == 0)
      {
        locationInfo.CountryCode = countryCode.ToString();
        locationInfo.AreaCode = areaCode.ToString();
      }
      else
        Log.Write("ISDN: Can't get TAPI location info!!!");

      return locationInfo;
    }
  }
}
