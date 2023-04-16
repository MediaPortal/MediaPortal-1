using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.MemoryMappedFiles;
using MediaPortal.GUI.Library;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplayPlugin.Drivers
{
  public class MPx86ProxyHandler : IDisposable
  {
    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll")]
    private static extern int RegisterWindowMessage(string message); //Defines a new window message that is guaranteed to be unique throughout the system. The message value can be used when sending or posting messages.

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern ushort GlobalAddAtom(string lpString);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern ushort GlobalDeleteAtom(ushort nAtom);

    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

    private const int HWND_BROADCAST = 0xFFFF; //The message is posted to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows. The message is not posted to child windows.


    private enum ObjectTypeEnum
    {
      Unknown = 0,
      Byte,
      Short,
      Integer,
      Long,
      ULong,
      String,
      Array
    }

    public enum CommandEnum
    {
      Unknown = 0,
      DriverMethod,

      ImonInit = 120,
      ImonUninit,
      ImonIsInited,
      ImonSetText,
      ImonSetEQ,
      ImonSetLCDData2,
      ImonSendData,
      ImonSendDataBuffer,

      ImonRCInit = 130,
      ImonRCUninit,
      ImonRCIsInited,
      ImonRCGetHWType,
      ImonRCGetFirmwareVer,
      ImonRCCheckDriverVersion,
      ImonRCChangeiMONRCSet,
      ImonRCChangeRC6,
      ImonRCGetLastRFMode,
      ImonRCGetPacket
    }

    private TcpClient _Connection = null;
    private byte[] _Response = new byte[8];

    private object _Padlock = new object();
    private MemoryMappedFile _File = null;
    private MemoryMappedViewAccessor _FileAccessor = null;
    private int _ProxyProccessId = -1;
    private int _ProxyPort = -1;
    private byte[] _Data = null;
    private int _MessageId;
    private ushort _Atom;

    public static MPx86ProxyHandler Instance
    {
      get
      {
        if (_Instance == null)
          _Instance = new MPx86ProxyHandler();

        return _Instance;
      }

    }private static MPx86ProxyHandler _Instance = null;


    public int Execute(string strDriver, string strMethod, params object[] args)
    {
      try
      {
        lock (this._Padlock)
        {
          //long lStart = Stopwatch.GetTimestamp();

          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = this.getData(strDriver, strMethod, args);

          //Send
          iResult = this.send(iSize);

          //Log.Debug("[MPx86ProxyHandler][Execute] Driver:{0} Method:{1} Data:{2:X16} Result:{3} Duration:{4}",
          //  strDriver,
          //  strMethod,
          //  args != null && args.Length > 0 ? args[0] : null,
          //  iResult,
          //  Stopwatch.GetTimestamp() - lStart
          //  );

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Driver:{0} Method:{1} Error: {2}", strDriver, strMethod, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = 5;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          //Send
          iResult = this.send(iSize);

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd, int iArg)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = 5 + 4;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          this._Data[5] = (byte)iArg;
          this._Data[6] = (byte)(iArg >> 8);
          this._Data[7] = (byte)(iArg >> 16);
          this._Data[8] = (byte)(iArg >> 24);

          //Send
          iResult = this.send(iSize);

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd, int iArg1, int iArg2)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = 5 + 4 + 4;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          this._Data[5] = (byte)iArg1;
          this._Data[6] = (byte)(iArg1 >> 8);
          this._Data[7] = (byte)(iArg1 >> 16);
          this._Data[8] = (byte)(iArg1 >> 24);

          this._Data[9] = (byte)iArg2;
          this._Data[10] = (byte)(iArg2 >> 8);
          this._Data[11] = (byte)(iArg2 >> 16);
          this._Data[12] = (byte)(iArg2 >> 24);

          //Send
          iResult = this.send(iSize);

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd, int iArg1, int iArg2, int iArg3)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = 5 + 4 + 4 + 4;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          this._Data[5] = (byte)iArg1;
          this._Data[6] = (byte)(iArg1 >> 8);
          this._Data[7] = (byte)(iArg1 >> 16);
          this._Data[8] = (byte)(iArg1 >> 24);

          this._Data[9] = (byte)iArg2;
          this._Data[10] = (byte)(iArg2 >> 8);
          this._Data[11] = (byte)(iArg2 >> 16);
          this._Data[12] = (byte)(iArg2 >> 24);

          this._Data[13] = (byte)iArg3;
          this._Data[14] = (byte)(iArg3 >> 8);
          this._Data[15] = (byte)(iArg3 >> 16);
          this._Data[16] = (byte)(iArg3 >> 24);

          //Send
          iResult = this.send(iSize);

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd, ulong lArg)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = 5 + 8;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          this._Data[5] = (byte)lArg;
          this._Data[6] = (byte)(lArg >> 8);
          this._Data[7] = (byte)(lArg >> 16);
          this._Data[8] = (byte)(lArg >> 24);
          this._Data[9] = (byte)(lArg >> 32);
          this._Data[10] = (byte)(lArg >> 40);
          this._Data[11] = (byte)(lArg >> 48);
          this._Data[12] = (byte)(lArg >> 56);

          //Send
          iResult = this.send(iSize);

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd, int[] data)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = (data.Length * 4) + 5;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          unsafe
          {
            fixed (void* pSrc = data)
            {
              fixed (byte* pDst = this._Data)
              {
                memcpy((IntPtr)(pDst + 5), (IntPtr)pSrc, new UIntPtr((uint)data.Length * 4));
              }
            }
          }

          //Send
          iResult = this.send(iSize);

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public int Execute(CommandEnum cmd, ulong[] data)
    {
      //long lStart = Stopwatch.GetTimestamp();

      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = (data.Length * 8) + 5;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          unsafe
          {
            fixed (void* pSrc = data)
            {
              fixed (byte* pDst = this._Data)
              {
                memcpy((IntPtr)(pDst + 5), (IntPtr)pSrc, new UIntPtr((uint)data.Length * 8));
              }
            }
          }

          //Send
          iResult = this.send(iSize);

          //Log.Debug("[MPx86ProxyHandler][Execute] Command:{0} Result:{1} Duration:{2}",
          //  cmd,
          //  iResult,
          //  Stopwatch.GetTimestamp() - lStart
          //  );

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
  }

    public int Execute(CommandEnum cmd, ref byte[] data, int iLength)
    {
      try
      {
        lock (this._Padlock)
        {
          //Connect
          int iResult = this.connect();

          if (iResult != 0)
            return iResult;

          //Get data
          int iSize = iLength + 5;
          this._Data[0] = (byte)iSize;
          this._Data[1] = (byte)(iSize >> 8);
          this._Data[2] = (byte)(iSize >> 16);
          this._Data[3] = (byte)(iSize >> 24);
          this._Data[4] = (byte)cmd;

          unsafe
          {
            fixed (void* pSrc = data)
            {
              fixed (byte* pDst = this._Data)
              {
                memcpy((IntPtr)(pDst + 5), (IntPtr)pSrc, new UIntPtr((uint)iLength));
              }
            }
          }

          //Send
          iResult = this.send(iSize, iLength, data);
          

          //Log.Debug("[MPx86ProxyHandler][Execute] Command:{0} Result:{1} Duration:{2}",
          //  cmd,
          //  iResult,
          //  Stopwatch.GetTimestamp() - lStart
          //  );

          return iResult;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MPx86ProxyHandler][Execute] Command:{0} Error: {1}", cmd, ex.Message);
        return -1;
      }
    }

    public void Dispose()
    {
      if (this._Connection != null)
      {
        try { this._Connection.Close(); }
        catch { }

        this._Connection = null;
      }

      if (this._File != null)
      {
        this._FileAccessor.Dispose();
        this._File.Dispose();

        GlobalDeleteAtom(this._Atom);

        this._FileAccessor = null;
        this._File = null;
      }
    }


    private int connect()
    {
      if (this._Data == null)
        this._Data = new byte[256];

      if (this._FileAccessor == null && this._Connection == null)
      {
        //Read port from memory file
        MemoryMappedFile f = MemoryMappedFile.OpenExisting("MPx86ProxyDescription");
        byte[] data = new byte[512];
        MemoryMappedViewAccessor ac = f.CreateViewAccessor();
        ac.ReadArray<byte>(0, data, 0, data.Length);
        ac.Dispose();
        f.Dispose();
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(Encoding.UTF8.GetString(data).Trim());
        this._ProxyProccessId = int.Parse(xml.SelectSingleNode("//MPx86ProxyDescription/PID/text()").Value);
        this._ProxyPort = int.Parse(xml.SelectSingleNode("//MPx86ProxyDescription/Port/text()").Value);
        xml = null;

        if (this._ProxyPort > 0)
        {
          //Socket mode

          this._Connection = new TcpClient();
          this._Connection.Connect("127.0.0.1", this._ProxyPort);

          if (!this._Connection.Connected)
          {
            Log.Error("[MPx86ProxyHandler][Execute] Failed to connect.");
            this._Connection = null;
            return -1;
          }

          Log.Debug("[MPx86ProxyHandler][connect] Connected to proxy server in socket mode.");
        }
        else if (this._ProxyProccessId > 0)
        {
          //MemoryFile mode

          //Our filename
          string strFile = "MediaPortalx86ProxyClient:" + Process.GetCurrentProcess().Id;

          //Create mapped file
          this._File = MemoryMappedFile.CreateNew(strFile, 256);
          this._FileAccessor = this._File.CreateViewAccessor();

          //Get win message id
          this._MessageId = RegisterWindowMessage("WM_MP_X86_PROXY_REQUEST");

          //Register global atom
          this._Atom = GlobalAddAtom(strFile);

          Log.Debug("[MPx86ProxyHandler][connect] Connected to proxy server in memory file mode.");
        }
        else
        {
          Log.Error("[MPx86ProxyHandler][connect] Unknown connection to the proxy.");
          return -1;
        }
      }

      return 0;
    }

    private int send(int iSize, int iDataLength = 0, byte[] data = null)
    {
      if (this._ProxyPort > 0)
      {
        //Send request
        this._Connection.Client.Send(this._Data, 0, iSize, SocketFlags.None);

        //Get response
        int iRec = this._Connection.Client.Receive(this._Response);
        if (iRec !=  4 + iDataLength)
          return -1;

        if (iDataLength > 0)
          Buffer.BlockCopy(this._Data, 4, data, 0, iDataLength);

        return BitConverter.ToInt32(this._Response, 0);
      }
      else
      {
        this._FileAccessor.WriteArray<byte>(0, this._Data, 0, iSize);

        //Send request
        SendMessage((IntPtr)HWND_BROADCAST, this._MessageId, (IntPtr)this._Atom, IntPtr.Zero);

        if (iDataLength > 0)
          this._FileAccessor.ReadArray<byte>(0, data, 4, iDataLength);

        //Get response
        return this._FileAccessor.ReadInt32(0);
      }
    }

    private int getData(string strDriver, string strMethod, params object[] args)
    {
      int iIdx = 4;

      //Command
      this._Data[iIdx++] = (byte)CommandEnum.DriverMethod;

      //Driver
      for (int i = 0; i < strDriver.Length; i++)
        this._Data[iIdx++] = (byte)strDriver[i];

      this._Data[iIdx++] = 0;

      //Method
      for (int i = 0; i < strMethod.Length; i++)
        this._Data[iIdx++] = (byte)strMethod[i];

      this._Data[iIdx++] = 0;

      //Arguments
      for (int i = 0; i < args.Length; i++)
      {
        object o = args[i];

        if (o is byte)
        {
          this._Data[iIdx++] = (byte)ObjectTypeEnum.Byte;
          this._Data[iIdx++] = (byte)o;
        }
        else if (o is short)
        {
          this._Data[iIdx++] = (byte)ObjectTypeEnum.Short;
          short s = (short)o;
          this._Data[iIdx++] = (byte)(s);
          this._Data[iIdx++] = (byte)(s >> 8);
        }
        else if (o is int)
        {
          this._Data[iIdx++] = (byte)ObjectTypeEnum.Integer;
          int iInt = (int)o;
          this._Data[iIdx++] = (byte)(iInt);
          this._Data[iIdx++] = (byte)(iInt >> 8);
          this._Data[iIdx++] = (byte)(iInt >> 16);
          this._Data[iIdx++] = (byte)(iInt >> 24);
        }
        else if (o is long || o is ulong)
        {
          this._Data[iIdx++] = (byte)(o is long ? ObjectTypeEnum.Long : ObjectTypeEnum.ULong);
          ulong l = (ulong)o;
          this._Data[iIdx++] = (byte)(l);
          this._Data[iIdx++] = (byte)(l >> 8);
          this._Data[iIdx++] = (byte)(l >> 16);
          this._Data[iIdx++] = (byte)(l >> 24);
          this._Data[iIdx++] = (byte)(l >> 32);
          this._Data[iIdx++] = (byte)(l >> 40);
          this._Data[iIdx++] = (byte)(l >> 48);
          this._Data[iIdx++] = (byte)(l >> 56);
        }
        else if (o is string)
        {
          this._Data[iIdx++] = (byte)ObjectTypeEnum.String;
          byte[] d = Encoding.UTF8.GetBytes((string)o);

          this._Data[iIdx++] = (byte)d.Length;
          this._Data[iIdx++] = (byte)(d.Length >> 8);
          this._Data[iIdx++] = (byte)(d.Length >> 16);
          this._Data[iIdx++] = (byte)(d.Length >> 24);

          Buffer.BlockCopy(d, 0, this._Data, iIdx, d.Length);

          iIdx += d.Length;
        }
        else if (o is int[])
        {
          this._Data[iIdx++] = (byte)ObjectTypeEnum.Array;
          this._Data[iIdx++] = (byte)ObjectTypeEnum.Integer;

          int[] d = (int[])o;
          this._Data[iIdx++] = (byte)d.Length;
          this._Data[iIdx++] = (byte)(d.Length >> 8);
          this._Data[iIdx++] = (byte)(d.Length >> 16);
          this._Data[iIdx++] = (byte)(d.Length >> 24);

          for (int iA = 0; iA < d.Length; iA++)
          {
            int iValue = d[iA];

            this._Data[iIdx++] = (byte)iValue;
            this._Data[iIdx++] = (byte)(iValue >> 8);
            this._Data[iIdx++] = (byte)(iValue >> 16);
            this._Data[iIdx++] = (byte)(iValue >> 24);
          }
        }
        else
          throw new Exception("Unknown argument type: " +  o.GetType().ToString());
      }

      //Size at position 0
      this._Data[0] = (byte)iIdx;
      this._Data[1] = (byte)(iIdx >> 8);
      this._Data[2] = (byte)(iIdx >> 16);
      this._Data[3] = (byte)(iIdx >> 24);

      //Total size
      return iIdx;
    }


  }
}
