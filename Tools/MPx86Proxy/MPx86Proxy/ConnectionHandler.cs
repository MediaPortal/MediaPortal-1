using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Diagnostics;


namespace MPx86Proxy
{
  public class ConnectionHandler
  {
    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

    public bool ExtensiveLogging = false;

    class Client
    {
      public byte[] Buffer = new byte[256];
      public Socket Socket;
      public StringBuilder Sb = new StringBuilder(256);
      public byte[] Response = new byte[256];

      public List<object> Args = new List<object>();
      public volatile int Received = 0;
      public volatile int Result = -1;
      public volatile int ResultLength = -1;
      public volatile RegisterEventEnum RegisteredEvents = RegisterEventEnum.None;
      public ManualResetEvent FlagDone = new ManualResetEvent(false);
    }

    private const int _COMMAND_PROCESS_TIMEOUT = 5000; //ms

    private List<Client> _ClientList = new List<Client>();

    private Dictionary<string, Type> _CacheType = new Dictionary<string, Type>();
    private Dictionary<string, MethodInfo> _CacheMethod = new Dictionary<string, MethodInfo>();

    private TcpListener _Server = null;

    private AsyncCallback _AcceptSocketCallback = null;
    private AsyncCallback _ReceiveCallback = null;

    private MemoryMappedFile _GlobalFile = null;

    private Client _ClientRequest = null;

    private byte[] _BufferEvent = new byte[256];

    private object _Padlock = new object();
    private volatile bool _Abort = false;
    private Thread _ThreadProcess = null;
    private System.Timers.Timer _WatchdogTimer = null;

    public int Port
    {
      get
      {
        if (this._Server != null)
          return ((IPEndPoint)this._Server.LocalEndpoint).Port;

        return -1;
      }
    }

    public bool IsRunning
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        return this._Server != null;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Start(int port)
    {
      if (this._Server != null)
        return;

      if (this._AcceptSocketCallback == null)
        this._AcceptSocketCallback = new AsyncCallback(acceptSocket);

      if (this._ReceiveCallback == null)
        this._ReceiveCallback = new AsyncCallback(receive);

      //Watchdog for command execution
      this._WatchdogTimer = new System.Timers.Timer();
      this._WatchdogTimer.Interval = _COMMAND_PROCESS_TIMEOUT;
      this._WatchdogTimer.Elapsed += this.cbWatchdogTimer;
      this._WatchdogTimer.AutoReset = false;

      //Main commnad processing thread
      //All commands must be executed on the same thread otherwise it will lead to application crash when calling soundgraph native lib
      this._ThreadProcess = new Thread(this.process);
      this._Abort = false;
      this._ThreadProcess.Start();

      try
      {
        //Start main server
        this._ClientList.Clear();
        this._Server = new TcpListener(IPAddress.Any, port);
        this._Server.Start();
        this._Server.BeginAcceptSocket(this._AcceptSocketCallback, null);

        //Create global server decription file
        string strDescription = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MPx86ProxyDescription>" +
            "<Port>" + ((IPEndPoint)this._Server.LocalEndpoint).Port + "</Port>" +
            "<PID>" + Process.GetCurrentProcess().Id + "</PID>" +
            "</MPx86ProxyDescription>";


        byte[] data = Encoding.UTF8.GetBytes(strDescription);
        this._GlobalFile = MemoryMappedFile.CreateNew("MPx86ProxyDescription", 512);

        MemoryMappedViewAccessor ac = this._GlobalFile.CreateViewAccessor();
        ac.WriteArray<byte>(0, data, 0, data.Length);
        ac.Flush();
        ac.Dispose();
        ac = null;

        Logging.Log.Debug("[Run] Server started on port: {0}", this.Port);

        MainForm.Instance.logWindow("Server started on port: " + this.Port);

      }
      catch (Exception ex)
      {
        Logging.Log.Error("[Run] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Stop()
    {
      if (this._Server == null)
        return;

      try
      {
        this._Abort = true;

        TcpListener srv = this._Server;
        this._Server = null;
        srv.Stop();

        lock (this._ClientList)
        {
          for (int i = 0; i < this._ClientList.Count; i++)
            this._ClientList[i].Socket.Close();

          this._ClientList.Clear();
        }

        lock (this._Padlock)
        {
          Monitor.PulseAll(this._Padlock);
        }
        this._ThreadProcess.Join();
        this._ThreadProcess = null;

        this._WatchdogTimer.Enabled = false;
        this._WatchdogTimer.Dispose();
        this._WatchdogTimer = null;

        this._GlobalFile.Dispose();
        this._GlobalFile = null;

        Logging.Log.Debug("[Stop] Server stopped.");
        MainForm.Instance.logWindow("Server stopped.");

      }
      catch (Exception ex)
      {
        Logging.Log.Error("[Stop] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    //[MethodImpl(MethodImplOptions.Synchronized)]
    public void OnEvent(RegisterEventEnum eventType, int iValue1, int iValue2)
    {
      lock (this._ClientList)
      {
        this._BufferEvent[0] = 13;
        this._BufferEvent[1] = 0;
        this._BufferEvent[2] = 0;
        this._BufferEvent[3] = 0;
        this._BufferEvent[4] = (byte)eventType;
        this._BufferEvent[5] = (byte)iValue1;
        this._BufferEvent[6] = (byte)(iValue1 >> 8);
        this._BufferEvent[7] = (byte)(iValue1 >> 16);
        this._BufferEvent[8] = (byte)(iValue1 >> 24);
        this._BufferEvent[9] = (byte)iValue2;
        this._BufferEvent[10] = (byte)(iValue2 >> 8);
        this._BufferEvent[11] = (byte)(iValue2 >> 16);
        this._BufferEvent[12] = (byte)(iValue2 >> 24);

        for (int i = 0; i < this._ClientList.Count; i++)
        {
          Client client = this._ClientList[i];

          if ((client.RegisteredEvents & eventType) != RegisterEventEnum.None)
          {
            try
            {
              client.Socket.Send(this._BufferEvent, 0, 13, SocketFlags.None);
            }
            catch
            {
            }
          }

        }
      }
    }


    private void receive(IAsyncResult ar)
    {
      Client client = (Client)ar.AsyncState;
      string strClient = null;
      try
      {
        strClient = client.Socket.RemoteEndPoint.ToString();
        int iResult;
        int iLength = client.Socket.EndReceive(ar);
        byte[] response = client.Response;

        while (!this._Abort)
        {
          if (iLength > 0)
          {
            //Create request
            lock (this._Padlock)
            {
              client.FlagDone.Reset();
              client.Received = iLength;
              this._ClientRequest = client;
              Monitor.Pulse(this._Padlock);
            }

            //Wait for result
            if (client.FlagDone.WaitOne(5000))
              iResult = client.Result;
            else
              iResult = -2;

            //Response
            response[0] = (byte)iResult;
            response[1] = (byte)(iResult >> 8);
            response[2] = (byte)(iResult >> 16);
            response[3] = (byte)(iResult >> 24);
            client.Socket.Send(response, client.ResultLength, SocketFlags.None);

            //Next receive
            iLength = client.Socket.Receive(client.Buffer);
          }
          else
            break;
        }
      }
      catch
      {
      }

      //Connection closed

      Logging.Log.Debug("[Receive] Remote client termination: '{0}'", strClient);
      MainForm.Instance.logWindow("Remote client termination: " + strClient);

      lock (this._ClientList)
      {
        try
        {
          this._ClientList.Remove(client);

          client.Socket.Close();
        }
        catch { }
      }

    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void acceptSocket(IAsyncResult result)
    {
      try
      {
        if (this._Server != null)
        {
          Socket socket = this._Server.EndAcceptSocket(result);
          if (socket != null)
          {
            lock (this._ClientList)
            {
              Client client = new Client() { Socket = socket };
              socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this._ReceiveCallback, client);

              this._ClientList.Add(client);
            }

            string strClient = socket.RemoteEndPoint.ToString();

            Logging.Log.Debug("[AcceptSocket] New socket connection accepted: '{0}'", strClient);

            MainForm.Instance.logWindow("New socket connection accepted: " + strClient);
          }
        }
      }
      catch (Exception ex)
      {
        Logging.Log.Error("[AcceptSocket] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }

      if (this._Server != null)
        this._Server.BeginAcceptSocket(this._AcceptSocketCallback, null);
    }

    private void process()
    {
      int iResult, iLength, i, iResultLength;
      ulong dw;
      string str1, str2;

      lock (this._Padlock)
      {
        while (!this._Abort)
        {
          try
          {
            if (this._ClientRequest == null)
              Monitor.Wait(this._Padlock);
            else
            {
              this._WatchdogTimer.Start();

              iResult = -1;
              iResultLength = 4;
              iLength = this._ClientRequest.Received;
              if (iLength > 0)
              {
                #region Handle request
                byte[] data = this._ClientRequest.Buffer;

                if (iLength >= 5)
                {
                  //Offset
                  //0: Length of message (int: size 4)
                  //4: Command (size 1)

                  int iSize = BitConverter.ToInt32(data, 0);

                  if (iLength == iSize)
                  {
                    int iIdx = 4;

                    //Command
                    CommandEnum cmd = (CommandEnum)data[iIdx++];
                    if (this.ExtensiveLogging)
                      Logging.Log.Debug("[process] Command: " + cmd.ToString() + "  Length: " + iLength);

                    switch (cmd)
                    {
                      case CommandEnum.ImonInit:
                        if (iLength == 13)
                          iResult = Drivers.iMONDisplay.iMONDisplay_Init(BitConverter.ToInt32(data, iIdx), BitConverter.ToInt32(data, iIdx + 4)) ? 1 : 0;
                        goto resp;

                      case CommandEnum.ImonUninit:
                        Drivers.iMONDisplay.iMONDisplay_Uninit();
                        iResult = 1;
                        goto resp;

                      case CommandEnum.ImonIsInited:
                        iResult = Drivers.iMONDisplay.iMONDisplay_IsInited() ? 1 : 0;
                        goto resp;

                      case CommandEnum.ImonSendData:
                        if (iLength == 13)
                        {
                          dw = BitConverter.ToUInt64(data, iIdx);
                          iResult = Drivers.iMONDisplay.iMONDisplay_SendData(ref dw) ? 1 : 0;
                        }
                        goto resp;

                      case CommandEnum.ImonSetLCDData2:
                        if (iLength == 13)
                        {
                          dw = BitConverter.ToUInt64(data, iIdx);
                          iResult = Drivers.iMONDisplay.iMONDisplay_LCD3R_SetLCDData2(ref dw) ? 1 : 0;
                        }
                        goto resp;

                      case CommandEnum.ImonSetText:
                        if (iLength >= 13)
                        {
                          i = BitConverter.ToInt32(data, iIdx);
                          iIdx += 4;
                          str1 = Encoding.UTF8.GetString(data, iIdx, i);
                          iIdx += i;

                          i = BitConverter.ToInt32(data, iIdx);
                          iIdx += 4;
                          str2 = Encoding.UTF8.GetString(data, iIdx, i);
                          iIdx += i;

                          iResult = Drivers.iMONDisplay.iMONDisplay_SetText(str1, str2) ? 1 : 0;
                        }
                        break;

                      case CommandEnum.ImonSetEQ:
                        if (iLength >= 9)
                        {
                          int[] d = new int[(iLength - 5) >> 2];
                          unsafe
                          {
                            fixed (void* pDst = d)
                            {
                              fixed (byte* pSrc = data)
                              {
                                memcpy((IntPtr)pDst, (IntPtr)(pSrc + iIdx), new UIntPtr((uint)(iLength - 5)));
                              }
                            }
                          }

                          iResult = Drivers.iMONDisplay.iMONDisplay_SetEQ(d) ? 1 : 0;
                        }
                        goto resp;

                      //Extra method for performance purpose
                      case CommandEnum.ImonSendDataBuffer:
                        if (iLength >= 13)
                          iResult = Drivers.iMONDisplay.iMONDisplay_SendDataBuffer(data, iIdx, (iLength - 5) >> 3) ? 1 : 0;
                        goto resp;

                      case CommandEnum.DriverMethod:
                        break;




                      case CommandEnum.ImonRCInit:
                        if (iLength == 17)
                          iResult = Drivers.iMONDisplay.iMONRC_Init(
                              BitConverter.ToInt32(data, iIdx), BitConverter.ToInt32(data, iIdx + 4), BitConverter.ToInt32(data, iIdx + 8)) ? 1 : 0;
                        goto resp;

                      case CommandEnum.ImonRCUninit:
                        Drivers.iMONDisplay.iMONRC_Uninit();
                        iResult = 1;
                        goto resp;

                      case CommandEnum.ImonRCIsInited:
                        iResult = Drivers.iMONDisplay.iMONRC_IsInited() ? 1 : 0;
                        goto resp;

                      case CommandEnum.ImonRCGetHWType:
                        iResult = Drivers.iMONDisplay.iMONRC_GetHWType();
                        goto resp;

                      case CommandEnum.ImonRCGetFirmwareVer:
                        iResult = Drivers.iMONDisplay.iMONRC_GetFirmwareVer();
                        goto resp;

                      case CommandEnum.ImonRCCheckDriverVersion:
                        iResult = Drivers.iMONDisplay.iMONRC_CheckDriverVersion();
                        goto resp;

                      case CommandEnum.ImonRCChangeiMONRCSet:
                        if (iLength == 9)
                          iResult = Drivers.iMONDisplay.iMONRC_ChangeiMONRCSet(BitConverter.ToInt32(data, iIdx)) ? 1 : 0;
                        goto resp;

                      case CommandEnum.ImonRCChangeRC6:
                        if (iLength == 9)
                          iResult = Drivers.iMONDisplay.iMONRC_ChangeRC6(BitConverter.ToInt32(data, iIdx)) ? 1 : 0;
                        goto resp;

                      case CommandEnum.ImonRCGetLastRFMode:
                        iResult = Drivers.iMONDisplay.iMONRC_GetLastRFMode();
                        goto resp;

                      case CommandEnum.ImonRCGetPacket:
                        if (iLength > 5)
                        {
                          i = iLength - 5;

                          unsafe
                          {
                            fixed (byte* p = data)
                            {
                              iResult = Drivers.iMONDisplay.iMONRC_GetPacket(p + 5, i) ? 1 : 0;
                            }
                          }

                          Buffer.BlockCopy(data, 5, this._ClientRequest.Response, 4, i);
                          iResultLength = 4 + i;
                        }

                        goto resp;



                      case CommandEnum.ImonRCRegisterForEvents:
                        this._ClientRequest.RegisteredEvents |= RegisterEventEnum.ImonRcButtonsEvents;
                        iResult = 1;
                        goto resp;

                      case CommandEnum.ImonRCUnregisterFromEvents:
                        this._ClientRequest.RegisteredEvents &= ~RegisterEventEnum.ImonRcButtonsEvents;
                        iResult = 1;
                        goto resp;


                      default:
                        goto resp;
                    }

                    StringBuilder sb = this._ClientRequest.Sb;

                    //Offset
                    //5: DriverName (null terminated ANSI string)
                    //   MethodName (null terminated ANSI string)


                    #region Driver
                    sb.Clear();
                    while (iIdx < iSize)
                    {
                      char c = (char)data[iIdx++];
                      if (c == '\0')
                        break;

                      sb.Append(c);
                    }

                    if (sb.Length == 0)
                      goto resp;


                    string strDriver = sb.ToString();
                    #endregion

                    #region Method
                    sb.Clear();
                    while (iIdx < iSize)
                    {
                      char c = (char)data[iIdx++];
                      if (c == '\0')
                        break;

                      sb.Append(c);
                    }

                    if (sb.Length == 0)
                      goto resp;

                    string strMethod = sb.ToString();
                    #endregion

                    //Get driver

                    Type tDriver = null;
                    string strType = "MPx86Proxy.Drivers." + strDriver;
                    lock (this._CacheType)
                    {
                      if (!this._CacheType.TryGetValue(strType, out tDriver))
                      {
                        tDriver = Type.GetType(strType);
                        if (tDriver != null)
                          this._CacheType.Add(strType, tDriver);
                      }
                    }

                    if (tDriver != null)
                    {
                      //Get method
                      MethodInfo mi = null;
                      lock (this._CacheMethod)
                      {
                        if (!this._CacheMethod.TryGetValue(strMethod, out mi))
                        {
                          mi = tDriver.GetMethod(strMethod, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                          if (mi != null)
                            this._CacheMethod.Add(strMethod, mi);
                        }
                      }

                      if (mi != null)
                      {
                        //Arguments
                        // TypeCode (size 1)
                        // Data

                        this._ClientRequest.Args.Clear();

                        while ((iIdx + 1) < iSize)
                        {
                          ObjectTypeEnum code = (ObjectTypeEnum)data[iIdx++];

                          switch (code)
                          {
                            #region Byte
                            case ObjectTypeEnum.Byte:
                              if ((iIdx + 1) <= iSize)
                              {
                                this._ClientRequest.Args.Add((byte)data[iIdx]);
                                iIdx++;
                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            #region Short
                            case ObjectTypeEnum.Short:
                              if ((iIdx + 2) <= iSize)
                              {
                                this._ClientRequest.Args.Add(BitConverter.ToInt16(data, iIdx));
                                iIdx += 2;
                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            #region Integer
                            case ObjectTypeEnum.Integer:
                              if ((iIdx + 4) <= iSize)
                              {
                                this._ClientRequest.Args.Add(BitConverter.ToInt32(data, iIdx));
                                iIdx += 4;
                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            #region Long
                            case ObjectTypeEnum.Long:
                              if ((iIdx + 8) <= iSize)
                              {
                                this._ClientRequest.Args.Add(BitConverter.ToInt64(data, iIdx));
                                iIdx += 8;
                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            #region Ulong
                            case ObjectTypeEnum.ULong:
                              if ((iIdx + 8) <= iSize)
                              {
                                this._ClientRequest.Args.Add(BitConverter.ToUInt64(data, iIdx));
                                iIdx += 8;
                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            #region String
                            case ObjectTypeEnum.String:
                              if ((iIdx + 4) <= iSize)
                              {
                                int iLng = BitConverter.ToInt32(data, iIdx);
                                iIdx += 4;

                                if ((iIdx + iLng) <= iSize)
                                {
                                  this._ClientRequest.Args.Add(Encoding.UTF8.GetString(data, iIdx, iLng));
                                  iIdx += iLng;
                                }
                                else
                                  goto resp;
                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            #region Array
                            case ObjectTypeEnum.Array:
                              if ((iIdx + 5) <= iSize)
                              {
                                ObjectTypeEnum t = (ObjectTypeEnum)data[iIdx++];

                                int iLng = BitConverter.ToInt32(data, iIdx);
                                iIdx += 4;

                                object arg = null;

                                switch (t)
                                {
                                  case ObjectTypeEnum.Byte:
                                    if ((iIdx + iLng) <= iSize)
                                    {
                                      byte[] d = new byte[iLng];
                                      Buffer.BlockCopy(data, iIdx, d, 0, iLng);
                                      arg = d;
                                    }
                                    else
                                      goto resp;

                                    break;

                                  case ObjectTypeEnum.Integer:
                                    if ((iIdx + (iLng * 4)) <= iSize)
                                    {
                                      int[] d = new int[iLng];
                                      for (i = 0; i < iLng; i++)
                                      {
                                        d[i] = BitConverter.ToInt32(data, iIdx);
                                        iIdx += 4;
                                      }
                                      arg = d;
                                    }
                                    else
                                      goto resp;

                                    break;
                                }

                                this._ClientRequest.Args.Add(arg);

                              }
                              else
                                goto resp;
                              break;
                            #endregion

                            default:
                              goto resp;

                          }
                        }

                        //Invoke the native method
                        if (mi.GetParameters().Length == this._ClientRequest.Args.Count)
                        {
                          try
                          {
                            object r = mi.Invoke(null, this._ClientRequest.Args.ToArray());
                            if (r == null)
                              iResult = 1;
                            else if (r is bool)
                              iResult = (bool)r ? 1 : 0;
                            else
                              iResult = (int)r;
                          }
                          catch (Exception ex)
                          {
                            iResult = -1;
                          }
                        }
                      }

                    }
                  }
                }

                #endregion
              }

            resp:
              this._WatchdogTimer.Stop();

              this._ClientRequest.Result = iResult;
              this._ClientRequest.ResultLength = iResultLength;
              this._ClientRequest.FlagDone.Set();
              this._ClientRequest = null;

              if (this.ExtensiveLogging)
                Logging.Log.Debug("[process] Command completed:  " + iResult);
            }
          }
          catch (Exception ex)
          {
            //Command has failed
            this._ClientRequest.Result = 0;
            this._ClientRequest.ResultLength = 4;
            this._ClientRequest.FlagDone.Set();
            this._ClientRequest = null;

            if (ex is ThreadAbortException)
              break;
            else
              Logging.Log.Error("[process] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
          }
        }
      }
    }

    private void cbWatchdogTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
      Logging.Log.Error("[cbWatchdogTimer] Error: process timeout");

      //Kill the main process
      this._ThreadProcess.Abort();
      this._ThreadProcess.Join();

      //Restart the main process
      if (!this._Abort)
      {
        this._ThreadProcess = new Thread(this.process);
        this._ThreadProcess.Start();

        Logging.Log.Debug("[cbWatchdogTimer] Process restarted");
      }
    }
  }
}
