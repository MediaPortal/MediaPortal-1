using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

using TvLibrary.Log; //Logging

namespace TVEngine.Devices
{
  #region Delegates

  public delegate void RemoteEventHandler(RemoteButton button);
  public delegate void LearntEventHandler(LearnState state, byte[] data);
  public delegate void LearnCallback(byte[] data);
  public delegate void DeviceEventHandler();
  delegate void SettingsChanged();

  #endregion Delegates

  #region Enums

  public enum RemoteButton
  {
    None = 0,
    Power1 = 165,
    Power2 = 12,
    Record = 23,
    Stop = 25,
    Pause = 24,
    Rewind = 21,
    Play = 22,
    Forward = 20,
    Replay = 27,
    Skip = 26,
    Back = 35,
    Up = 30,
    Info = 15,
    Left = 32,
    Ok = 34,
    Right = 33,
    Down = 31,
    VolumeUp = 16,
    VolumeDown = 17,
    Start = 13,
    ChannelUp = 18,
    ChannelDown = 19,
    Mute = 14,
    RecordedTV = 72,
    Guide = 38,
    LiveTV = 37,
    DVDMenu = 36,
    NumPad1 = 1,
    NumPad2 = 2,
    NumPad3 = 3,
    NumPad4 = 4,
    NumPad5 = 5,
    NumPad6 = 6,
    NumPad7 = 7,
    NumPad8 = 8,
    NumPad9 = 9,
    NumPad0 = 0,
    Oem8 = 29,
    OemGate = 28,
    Clear = 10,
    Enter = 11,
    Teletext = 90,
    Red = 91,
    Green = 92,
    Yellow = 93,
    Blue = 94,
  }

  public enum LearnState
  {
    Idle,
    Learning,
    Learned,
    Finalized,
    Failed,
  }

  #endregion Enums

  #region Remote

  /// <summary>
  /// Summary description for Remote.
  /// </summary>
  sealed class Remote : Device
  {
    #region Constructor

    static Remote()
    {
      _deviceSingleton = new Remote();
      _deviceSingleton.Init();
    }

    #endregion Constructor

    #region Implementation

    void Init()
    {
      try
      {
        // ask the OS for the class (GUID) that represents human input devices
        HidD_GetHidGuid(ref _deviceClass);

        _doubleClickTime = GetDoubleClickTime();
        _deviceBuffer = new byte[256];
        _notifyWindow = new NotifyWindow();
        _notifyWindow.Create();
        _notifyWindow.Class = _deviceClass;
        _notifyWindow.DeviceArrival += new DeviceEventHandler(OnDeviceArrival);
        _notifyWindow.DeviceRemoval += new DeviceEventHandler(OnDeviceRemoval);
        _notifyWindow.SettingsChanged += new SettingsChanged(OnSettingsChanged);
        _notifyWindow.RegisterDeviceArrival();

        Open();
      }
      catch (Exception e)
      {
        Log.Write("Remote.Init: {0}", e.Message);
      }
    }

    protected override void Open()
    {
      string devicePath = FindDevice(_deviceClass);

      if (devicePath == null) return;

      IntPtr deviceHandle = CreateFile(devicePath, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, FileFlag.Overlapped, 0);

      if (deviceHandle.ToInt32() == -1) throw new Exception(string.Format("Failed to open remote ({0})", GetLastError()));

      _notifyWindow.RegisterDeviceRemoval(deviceHandle);

      // open a stream from the device and begin an asynchronous read
      _deviceStream = new FileStream(deviceHandle, FileAccess.Read, true, 128, true);
      _deviceStream.BeginRead(_deviceBuffer, 0, _deviceBuffer.Length, new AsyncCallback(OnReadComplete), null);
    }

    void OnReadComplete(IAsyncResult asyncResult)
    {
      try
      {
        if (_deviceStream.EndRead(asyncResult) == 13)
        {
          if (_deviceBuffer[5] == (int)_doubleClickButton && Environment.TickCount - _doubleClickTick <= _doubleClickTime)
          {
            if (DoubleClick != null) DoubleClick(_doubleClickButton);
          }
          else
          {
            _doubleClickButton = (RemoteButton)_deviceBuffer[5];
            _doubleClickTick = Environment.TickCount;

            if (Click != null) Click(_doubleClickButton);
          }
        }

        // begin another asynchronous read from the device
        _deviceStream.BeginRead(_deviceBuffer, 0, _deviceBuffer.Length, new AsyncCallback(OnReadComplete), null);
      }
      catch (Exception)
      {
      }
    }

    void OnSettingsChanged()
    {
      _doubleClickTime = GetDoubleClickTime();
    }

    #endregion Implementation

    #region Interop

    [DllImport("user32")]
    static extern int GetDoubleClickTime();

    #endregion Interop

    #region Events

    public static RemoteEventHandler Click = null;
    public static RemoteEventHandler DoubleClick = null;

    #endregion Events

    #region Members

    static Remote _deviceSingleton;
    int _doubleClickTime = -1;
    int _doubleClickTick = 0;
    RemoteButton _doubleClickButton;

    #endregion Members
  }

  #endregion Remote

  #region Blaster

  /// <summary>
  /// Summary description for Blaster.
  /// </summary>
  sealed class Blaster : Device
  {
    #region Constructor

    static Blaster()
    {
      _deviceSingleton = new Blaster();
      _deviceSingleton.Init();
    }

    #endregion Constructors

    #region Methods

    public static void Send(int blasterPort, byte[] packet, int deviceType, int deviceSpeed, bool debug)
    {
      if (debug) Log.Write("Blaster.Send: Checking if  Singleton is null.");
      if (_deviceSingleton == null) return;
      if (debug) Log.Write("Blaster.Send: Checking if Device Singleton Stream is null.");
      if (_deviceSingleton._deviceStream == null) return;
      if (debug) Log.Write("Blaster.Send: Done Initial Checking. Will check arguments.");
      if (blasterPort < 0 || blasterPort > 2) throw new ArgumentException("blasterPort must be 1, 2 or 0 (both)");
      if (debug) Log.Write("Blaster.Send: BlasterPort Done.");
      if (deviceType < 0 || deviceType > 1) throw new ArgumentException("blasterType must be 1, or 0 (0 - MS, 1- SMK)");
      if (debug) Log.Write("Blaster.Send: BlasterType Done.");
      if (deviceSpeed < 0 || deviceSpeed > 2) throw new ArgumentException("blasterSpeed must be between 0 and 2 (0 - Fast, 2 - Slow)"); else _currentSpeed = deviceSpeed;
      if (debug) Log.Write("Blaster.Send: BlasterSpeed Done.");


      byte[][] packetSpeed = new byte[][]
			{
				new byte[] { 0x9F, 0x06, 0x01, 0x44 },	// fast
				new byte[] { 0x9F, 0x06, 0x01, 0x4A },	// medium
				new byte[] { 0x9F, 0x06, 0x01, 0x50 },	// slow???
			};

      byte[][] MSpacketPorts = new byte[][] //MS Device
			{
				new byte[] { 0x9F, 0x08, 0x06 },		// 0
				new byte[] { 0x9F, 0x08, 0x04 },		// 1
				new byte[] { 0x9F, 0x08, 0x02 },		// 2
			};

      byte[][] SMKpacketPorts = new byte[][] //SMK Device
			{
				new byte[] { 0x9F, 0x08, 0x00 },		// both
				new byte[] { 0x9F, 0x08, 0x01 },		// 1
				new byte[] { 0x9F, 0x08, 0x02 },        // 2
			};

      byte[][] packetPorts = new byte[][] { };



      int s = Math.Max(0, Math.Min(2, _currentSpeed));
      int p = blasterPort;

      switch (deviceType)
      {
        case 0: packetPorts = MSpacketPorts; break;
        case 1: packetPorts = SMKpacketPorts; break;
      }


      if (debug) Log.Write("Blaster.Send: Type {0}, Speed {1}", deviceType, s);
      if (debug) Log.Write("Blaster.Send: Port {0}", blasterPort == 0 ? "1 & 2" : blasterPort.ToString());
      if (debug) Log.Write("Seding packets");
      _deviceSingleton._deviceStream.Write(packetSpeed[s], 0, packetSpeed[s].Length);
      if (debug) Log.Write("Blaster.Send: Wrote speed packets");
      _deviceSingleton._deviceStream.Write(packetPorts[p], 0, packetPorts[p].Length);
      if (debug) Log.Write("Blaster.Send: Wrote port packets");
      _deviceSingleton._deviceStream.Write(packet, 0, packet.Length);
      if (debug) Log.Write("Blaster.Send: Wrote channel data  packets");
      _deviceSingleton._deviceStream.Flush();
      if (debug) Log.Write("Blaster.Send: Flushed");
    }

    public static void BeginLearn(LearnCallback learnCallback)
    {
      try
      {
        byte[] packet1 = new byte[] { 0x9F, 0x0C, 0x0F, 0xA0 };
        byte[] packet2 = new byte[] { 0x9F, 0x14, 0x01 };

        lock (_deviceSingleton) _deviceSingleton._packetArray = new ArrayList();

        _deviceSingleton._deviceStream.Write(packet1, 0, packet1.Length);
        _deviceSingleton._deviceStream.Write(packet2, 0, packet2.Length);
        _deviceSingleton._learnStartTick = Environment.TickCount;
        _deviceSingleton._deviceStream.BeginRead(_deviceSingleton._deviceBuffer, 0, _deviceSingleton._deviceBuffer.Length, new AsyncCallback(_deviceSingleton.OnReadComplete), learnCallback);
      }
      catch
      {
        if (learnCallback != null) learnCallback(null);
      }
    }

    #endregion Methods

    #region Implementation

    void Init()
    {
      _deviceClass = new Guid(0x7951772d, 0xcd50, 0x49b7, 0xb1, 0x03, 0x2b, 0xaa, 0xc4, 0x94, 0xfc, 0x57);
      _deviceBuffer = new byte[4096];

      _notifyWindow = new NotifyWindow();
      _notifyWindow.Create();
      _notifyWindow.Class = _deviceClass;
      _notifyWindow.DeviceArrival += new DeviceEventHandler(OnDeviceArrival);
      _notifyWindow.DeviceRemoval += new DeviceEventHandler(OnDeviceRemoval);
      _notifyWindow.RegisterDeviceArrival();

      // we need somewhere to store the smaller packets as they arrive
      _packetArray = new ArrayList();

      Open();
    }

    protected override void Open()
    {
      string devicePath = FindDevice(_deviceClass);

      if (devicePath == null) return;

      IntPtr deviceHandle = CreateFile(devicePath, FileAccess.ReadWrite, FileShare.ReadWrite, 0, FileMode.Open, FileFlag.Overlapped, 0);

      if (deviceHandle.ToInt32() == -1) throw new Exception(string.Format("Failed to open blaster ({0})", GetLastError()));

      _notifyWindow.RegisterDeviceRemoval(deviceHandle);

      // open a stream from the device and begin an asynchronous read
      _deviceStream = new FileStream(deviceHandle, FileAccess.ReadWrite, true, _deviceBuffer.Length, true);
    }

    void OnReadComplete(IAsyncResult asyncResult)
    {
      try
      {
        int bytesRead = _deviceStream.EndRead(asyncResult);

        // dealing with garbage
        if (asyncResult.AsyncState == null) return;

        if (bytesRead == 0)
        {
          _deviceStream.Close();
          _deviceStream = null;
          return;
        }

        if (_deviceBuffer[0] == 0x9F || (_deviceBuffer[0] & 0x80) != 0x80)
        {
          // ignore garbage - begin another asynchronous read from the device
          _deviceStream.BeginRead(_deviceBuffer, 0, _deviceBuffer.Length, new AsyncCallback(OnReadComplete), asyncResult.AsyncState);
          return;
        }

        byte[] packetBuffer = new byte[bytesRead];

        Array.Copy(_deviceBuffer, packetBuffer, bytesRead);

        lock (_deviceSingleton) _packetArray.Add(packetBuffer);

        if (Array.IndexOf(packetBuffer, (byte)0x80) != -1)
        {
          ((LearnCallback)asyncResult.AsyncState)(FinalizePacket());
          return;
        }

        // begin another asynchronous read from the device
        _deviceStream.BeginRead(_deviceBuffer, 0, _deviceBuffer.Length, new AsyncCallback(OnReadComplete), asyncResult.AsyncState);
      }
      catch (Exception e)
      {
        Log.Write("Blaster.OnReadComplete: {0}", e.Message);
      }
    }

    byte[] FinalizePacket()
    {
      return FinalizePacket3();
    }

    byte[] FinalizePacket1()
    {
      int packetLength = 0;
      int packetOffset = 0;

      foreach (byte[] packetBytes in _packetArray) packetLength += packetBytes.Length;

      byte[] packetFinal = new byte[packetLength];

      foreach (byte[] packetBytes in _packetArray)
      {
        foreach (byte packetByte in packetBytes) packetFinal[packetOffset++] = packetByte;
      }

      Log.Write("Blaster.FinalizePacket: {0} ({1} bytes)", BitConverter.ToString(packetFinal).Replace("-", ""), packetFinal.Length);

      lock (this) _packetArray = new ArrayList();

      return packetFinal;
    }

    byte[] FinalizePacket2()
    {
      int packetLength = 1;
      int packetOffset = 0;

      foreach (byte[] packetBytes in _packetArray) packetLength += packetBytes.Length;

      packetLength -= _packetArray.Count;
      packetLength += packetLength / 32;

      byte[] packetFinal = new byte[packetLength];

      foreach (byte[] packetBytes in _packetArray)
      {
        for (int byteIndex = 1; byteIndex < packetBytes.Length; byteIndex++)
        {
          if (packetOffset == 0 || packetOffset == 31 || packetOffset % 31 == 0)
          {
            packetFinal[packetOffset++] = (byte)((packetOffset + 31 <= packetLength) ? 0x9E : 0x9B);
          }

          packetFinal[packetOffset++] = packetBytes[byteIndex];
        }
      }

      Log.Write("Blaster.FinalizePacket: {0}", BitConverter.ToString(packetFinal).Replace("-", ""));

      _packetArray = new ArrayList();

      return packetFinal;
    }

    byte[] FinalizePacket3()
    {
      int packetLength = 0;
      int packetOffset = 0;

      foreach (byte[] packetBytes in _packetArray)
      {
        int indexOf9F = Array.IndexOf(packetBytes, (byte)0x9F);
        packetLength += indexOf9F == -1 ? packetBytes.Length : indexOf9F + 1;
      }

      byte[] packetFinal = new byte[packetLength];

      foreach (byte[] packetBytes in _packetArray)
      {
        foreach (byte packetByte in packetBytes)
        {
          packetFinal[packetOffset++] = (packetByte == 0x9F) ? (byte)0x80 : packetByte;

          if (packetByte == 0x9F) break;
        }
      }

      Log.Write("Blaster.FinalizePacket: {0} ({1} bytes)", BitConverter.ToString(packetFinal).Replace("-", ""), packetFinal.Length);

      _packetArray = new ArrayList();

      return packetFinal;
    }

    #endregion Implementation

    #region Properties

    public static int Speed { set { _currentSpeed = value; } get { return _currentSpeed; } }
    public static bool Present { get { return _deviceSingleton._deviceStream != null; } }

    #endregion Properties

    #region Members

    static Blaster _deviceSingleton;
    ArrayList _packetArray;
    int _learnStartTick = 0;
    static int _currentSpeed = 0;

    #endregion Members
  }

  #endregion Blaster

  #region Device

  /// <summary>
  /// Summary description for Device.
  /// </summary>
  internal abstract class Device
  {
    #region Implementation

    protected void OnDeviceArrival()
    {
      if (_deviceStream != null) return;

      Open();

      if (DeviceArrival != null) DeviceArrival();
    }

    protected abstract void Open();

    protected void OnDeviceRemoval()
    {
      if (_deviceStream == null) return;

      try
      {
        _deviceStream.Close();
        _deviceStream = null;
      }
      catch (IOException)
      {
        // we are closing the stream so ignore this
      }

      if (DeviceRemoval != null) DeviceRemoval();
    }

    protected string FindDevice(Guid classGuid)
    {
      IntPtr handle = SetupDiGetClassDevs(ref classGuid, 0, 0, 0x12);
      string devicePath = null;

      if (handle.ToInt32() == -1)
      {
        throw new Exception(string.Format("Failed in call to SetupDiGetClassDevs ({0})", GetLastError()));
      }

      for (int deviceIndex = 0; ; deviceIndex++)
      {
        DeviceInfoData deviceInfoData = new DeviceInfoData();
        deviceInfoData.Size = Marshal.SizeOf(deviceInfoData);

        if (SetupDiEnumDeviceInfo(handle, deviceIndex, ref deviceInfoData) == false)
        {
          // out of devices or do we have an error?
          if (GetLastError() != 0x103 && GetLastError() != 0x7E)
          {
            SetupDiDestroyDeviceInfoList(handle);
            throw new Exception(string.Format("Failed in call to SetupDiEnumDeviceInfo ({0})", GetLastError()));
          }

          SetupDiDestroyDeviceInfoList(handle);
          break;
        }

        DeviceInterfaceData deviceInterfaceData = new DeviceInterfaceData();
        deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);

        if (SetupDiEnumDeviceInterfaces(handle, ref deviceInfoData, ref classGuid, 0, ref deviceInterfaceData) == false)
        {
          SetupDiDestroyDeviceInfoList(handle);
          throw new Exception(string.Format("Failed in call to SetupDiEnumDeviceInterfaces ({0})", GetLastError()));
        }

        uint cbData = 0;

        if (SetupDiGetDeviceInterfaceDetail(handle, ref deviceInterfaceData, 0, 0, ref cbData, 0) == false && cbData == 0)
        {
          SetupDiDestroyDeviceInfoList(handle);
          throw new Exception(string.Format("Failed in call to SetupDiGetDeviceInterfaceDetail ({0})", GetLastError()));
        }

        DeviceInterfaceDetailData deviceInterfaceDetailData = new DeviceInterfaceDetailData();
        deviceInterfaceDetailData.Size = 5;

        if (SetupDiGetDeviceInterfaceDetail(handle, ref deviceInterfaceData, ref deviceInterfaceDetailData, cbData, 0, 0) == false)
        {
          SetupDiDestroyDeviceInfoList(handle);
          throw new Exception(string.Format("Failed in call to SetupDiGetDeviceInterfaceDetail ({0})", GetLastError()));
        }

        if (deviceInterfaceDetailData.DevicePath.IndexOf("#vid_0471&pid_0815") != -1)
        {
          SetupDiDestroyDeviceInfoList(handle);
          devicePath = deviceInterfaceDetailData.DevicePath;
          break;
        }

        if (deviceInterfaceDetailData.DevicePath.IndexOf("#vid_045e&pid_006d") != -1)
        {
          SetupDiDestroyDeviceInfoList(handle);
          devicePath = deviceInterfaceDetailData.DevicePath;
          break;
        }

        if (deviceInterfaceDetailData.DevicePath.IndexOf("#vid_1460&pid_9150") != -1)
        {
          SetupDiDestroyDeviceInfoList(handle);
          devicePath = deviceInterfaceDetailData.DevicePath;
          break;
        }

        if (deviceInterfaceDetailData.DevicePath.IndexOf("#vid_0609&pid_031d") != -1)
        {
          SetupDiDestroyDeviceInfoList(handle);
          devicePath = deviceInterfaceDetailData.DevicePath;
          break;
        }
        if (deviceInterfaceDetailData.DevicePath.IndexOf("#vid_03ee&pid_2501") != -1)
        {
          SetupDiDestroyDeviceInfoList(handle);
          devicePath = deviceInterfaceDetailData.DevicePath;
          break;
        }
      }

      return devicePath;
    }

    #endregion Implementation

    #region Interop

    [DllImport("kernel32", SetLastError = true)]
    protected static extern IntPtr CreateFile(string FileName, [MarshalAs(UnmanagedType.U4)] FileAccess DesiredAccess, [MarshalAs(UnmanagedType.U4)] FileShare ShareMode, uint SecurityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode CreationDisposition, FileFlag FlagsAndAttributes, int hTemplateFile);

    [DllImport("kernel32", SetLastError = true)]
    protected static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32", SetLastError = true)]
    protected static extern int GetLastError();

    protected enum FileFlag { Overlapped = 0x40000000, }

    [StructLayout(LayoutKind.Sequential)]
    protected struct DeviceInfoData
    {
      public int Size;
      public Guid Class;
      public uint DevInst;
      public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    protected struct DeviceInterfaceData
    {
      public int Size;
      public Guid Class;
      public uint Flags;
      public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    protected struct DeviceInterfaceDetailData
    {
      public int Size;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public string DevicePath;
    }

    [DllImport("hid")]
    protected static extern void HidD_GetHidGuid(ref Guid guid);

    [DllImport("setupapi", SetLastError = true)]
    protected static extern IntPtr SetupDiGetClassDevs(ref Guid guid, int Enumerator, int hwndParent, int Flags);

    [DllImport("setupapi", SetLastError = true)]
    protected static extern bool SetupDiEnumDeviceInfo(IntPtr handle, int Index, ref DeviceInfoData deviceInfoData);

    [DllImport("setupapi", SetLastError = true)]
    protected static extern bool SetupDiEnumDeviceInterfaces(IntPtr handle, ref DeviceInfoData deviceInfoData, ref Guid guidClass, int MemberIndex, ref DeviceInterfaceData deviceInterfaceData);

    [DllImport("setupapi", SetLastError = true)]
    protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr handle, ref DeviceInterfaceData deviceInterfaceData, int unused1, int unused2, ref uint requiredSize, int unused3);

    [DllImport("setupapi", SetLastError = true)]
    protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr handle, ref DeviceInterfaceData deviceInterfaceData, ref DeviceInterfaceDetailData deviceInterfaceDetailData, uint detailSize, int unused1, int unused2);

    [DllImport("setupapi")]
    protected static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

    #endregion Interop

    #region Events

    public static DeviceEventHandler DeviceArrival = null;
    public static DeviceEventHandler DeviceRemoval = null;

    #endregion Events

    #region Members

    protected Guid _deviceClass;
    protected FileStream _deviceStream;
    protected byte[] _deviceBuffer;
    internal NotifyWindow _notifyWindow;

    #endregion Members
  }

  #endregion Device

  #region NotifyWindow

  internal class NotifyWindow : NativeWindow
  {
    #region Interop

    const int WM_DEVICECHANGE = 0x0219;
    const int WM_SETTINGSCHANGE = 0x001A;
    const int DBT_DEVICEARRIVAL = 0x8000;
    const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

    [StructLayout(LayoutKind.Sequential)]
    struct DeviceBroadcastHeader
    {
      public int Size;
      public int DeviceType;
      public int Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DeviceBroadcastInterface
    {
      public int Size;
      public int DeviceType;
      public int Reserved;
      public Guid ClassGuid;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public string Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DeviceBroadcastHandle
    {
      public int Size;
      public int DeviceType;
      public int Reserved;
      public IntPtr Handle;
      public IntPtr HandleNotify;
      public Guid EventGuid;
      public int NameOffset;
      public byte Data;
    }

    [DllImport("user32", SetLastError = true)]
    static extern IntPtr RegisterDeviceNotification(IntPtr handle, ref DeviceBroadcastHandle filter, int flags);

    [DllImport("user32", SetLastError = true)]
    static extern IntPtr RegisterDeviceNotification(IntPtr handle, ref DeviceBroadcastInterface filter, int flags);

    [DllImport("user32")]
    static extern IntPtr UnregisterDeviceNotification(IntPtr handle);

    [DllImport("kernel32")]
    static extern int GetLastError();

    [DllImport("kernel32", SetLastError = true)]
    static extern bool CancelIo(IntPtr handle);

    #endregion Interop

    #region Methods

    internal void Create()
    {
      if (Handle != IntPtr.Zero) return;

      CreateParams Params = new CreateParams();
      Params.ExStyle = 0x80;
      Params.Style = unchecked((int)0x80000000);
      CreateHandle(Params);
    }

    #endregion Methods

    #region Properties

    internal Guid Class { get { return _deviceClass; } set { _deviceClass = value; } }

    #endregion Properties

    #region Overrides

    protected override void WndProc(ref Message m)
    {
      if (m.Msg == WM_DEVICECHANGE)
      {
        switch (m.WParam.ToInt32())
        {
          case DBT_DEVICEARRIVAL:
            OnDeviceArrival((DeviceBroadcastHeader)Marshal.PtrToStructure(m.LParam, typeof(DeviceBroadcastHeader)), m.LParam);
            break;
          case DBT_DEVICEREMOVECOMPLETE:
            OnDeviceRemoval((DeviceBroadcastHeader)Marshal.PtrToStructure(m.LParam, typeof(DeviceBroadcastHeader)), m.LParam);
            break;
        }
      }
      else if (m.Msg == WM_SETTINGSCHANGE)
      {
        if (SettingsChanged != null) SettingsChanged();
      }

      base.WndProc(ref m);
    }

    #endregion Overrides

    #region Implementation

    internal void RegisterDeviceArrival()
    {
      DeviceBroadcastInterface dbi = new DeviceBroadcastInterface();

      dbi.Size = Marshal.SizeOf(dbi);
      dbi.DeviceType = 0x5;
      dbi.ClassGuid = _deviceClass;

      _handleDeviceArrival = RegisterDeviceNotification(Handle, ref dbi, 0);

      if (_handleDeviceArrival == IntPtr.Zero) throw new Exception(string.Format("Failed in call to RegisterDeviceNotification ({0})", GetLastError()));
    }

    internal void RegisterDeviceRemoval(IntPtr deviceHandle)
    {
      DeviceBroadcastHandle dbh = new DeviceBroadcastHandle();

      dbh.Size = Marshal.SizeOf(dbh);
      dbh.DeviceType = 0x6;
      dbh.Handle = deviceHandle;

      _deviceHandle = deviceHandle;
      _handleDeviceRemoval = RegisterDeviceNotification(Handle, ref dbh, 0);

      if (_handleDeviceRemoval == IntPtr.Zero) throw new Exception(string.Format("Failed in call to RegisterDeviceNotification ({0})", GetLastError()));
    }

    internal void UnregisterDeviceArrival()
    {
      if (_handleDeviceArrival == IntPtr.Zero) return;

      UnregisterDeviceNotification(_handleDeviceArrival);
      _handleDeviceArrival = IntPtr.Zero;
    }

    internal void UnregisterDeviceRemoval()
    {
      if (_handleDeviceRemoval == IntPtr.Zero) return;

      UnregisterDeviceNotification(_handleDeviceRemoval);
      _handleDeviceRemoval = IntPtr.Zero;
      _deviceHandle = IntPtr.Zero;
    }

    void OnDeviceArrival(DeviceBroadcastHeader dbh, IntPtr ptr)
    {
      if (dbh.DeviceType == 0x05)
      {
        DeviceBroadcastInterface dbi = (DeviceBroadcastInterface)Marshal.PtrToStructure(ptr, typeof(DeviceBroadcastInterface));

        if (dbi.ClassGuid == _deviceClass && DeviceArrival != null) DeviceArrival();
      }
    }

    void OnDeviceRemoval(DeviceBroadcastHeader header, IntPtr ptr)
    {
      if (header.DeviceType == 0x06)
      {
        DeviceBroadcastHandle dbh = (DeviceBroadcastHandle)Marshal.PtrToStructure(ptr, typeof(DeviceBroadcastHandle));

        if (dbh.Handle != _deviceHandle) return;

        CancelIo(_deviceHandle);
        UnregisterDeviceRemoval();

        if (DeviceRemoval != null) DeviceRemoval();
      }
    }

    #endregion Implementation

    #region Delegates

    internal DeviceEventHandler DeviceArrival;
    internal DeviceEventHandler DeviceRemoval;
    internal SettingsChanged SettingsChanged;

    #endregion Delegates

    #region Members

    IntPtr _handleDeviceArrival;
    IntPtr _handleDeviceRemoval;
    IntPtr _deviceHandle;
    Guid _deviceClass;

    #endregion Members
  }

  #endregion NotifyWindow
}