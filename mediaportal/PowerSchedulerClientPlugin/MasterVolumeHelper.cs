#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace MediaPortal.Plugins.Process
{
  /// <summary>
  /// This class allows to get and set the master volume and mute (only for Vista/Win7)
  /// </summary>
  public class MasterVolumeHelper : IDisposable
  {
    #region Variables

    private MMDeviceEnumerator _devEnum = null;
    private MMDevice _defaultDevice = null;
    private AudioEndpointVolume _endpointVolume = null;

    #endregion

    #region Public methods

    /// <summary>
    /// Constructor
    /// </summary>
    public MasterVolumeHelper()
    {
      if (Environment.OSVersion.Version.Major < 6)
        return;

      // Get device enumerator
      _devEnum = new MMDeviceEnumerator();
      if (_devEnum == null)
      {
        throw new Exception("MasterVolumeHelper(): MMDeviceEnumerator() returns null");
      }

      // Get default audio device
      _defaultDevice = _devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
      if (_defaultDevice == null)
      {
        throw new Exception("MasterVolumeHelper(): GetDefaultAudioEndpoint() returns null");
      }

      // Get endpoint volume interface for default audio device
      _endpointVolume = _defaultDevice.Activate();
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
      if (Environment.OSVersion.Version.Major < 6)
        return;

      _endpointVolume = null;
      if (_defaultDevice != null)
      {
        _defaultDevice.Dispose();
        _defaultDevice = null;
      }
      if (_devEnum != null)
      {
        _devEnum.Dispose();
        _devEnum = null;
      }
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Get/set the master volume mute.
    /// </summary>
    public bool Muted
    {
      get
      {
        if (_endpointVolume != null)
          return (_endpointVolume.GetMute());
        return false;
      }
      set
      {
        if (_endpointVolume != null)
          _endpointVolume.SetMute(value);
      }
    }

    /// <summary>
    /// Get the default device's friendly name
    /// </summary>
    public string FriendlyName
    {
      get
      {
        if (_defaultDevice != null)
          return (_defaultDevice.FriendlyName);
        return string.Empty;
      }
    }

    /// <summary>
    /// Get the default device's state
    /// </summary>
    public string State
    {
      get
      {
        if (_defaultDevice != null)
          return (_defaultDevice.GetState);
        return string.Empty;
      }
    }

    #endregion
  }

  #region Core Audio SDK interface wrapper classes

  /// <summary>
  /// The MMDeviceEnumerator class encapsulates the IMMDeviceEnumerator interface,
  /// which provides methods for enumerating multimedia device resources. 
  /// </summary>
  internal class MMDeviceEnumerator : IDisposable
  {
    private IMMDeviceEnumerator _deviceEnumerator = null;
    private IMMDevice _defaultAudioEndpoint = null;

    /// <summary>
    /// Creator
    /// </summary>
    internal MMDeviceEnumerator()
    {
      _deviceEnumerator = new _MMDeviceEnumerator() as IMMDeviceEnumerator;
      if (_deviceEnumerator == null)
        throw new Exception("MMDeviceEnumerator(): _MMDeviceEnumerator() returns null");
    }

    public void Dispose()
    {
      _defaultAudioEndpoint = null;
    }

    internal MMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role)
    {
      if (_defaultAudioEndpoint == null && _deviceEnumerator != null)
      {
        _deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out _defaultAudioEndpoint);
      }
      return new MMDevice(_defaultAudioEndpoint);
    }
  }

  /// <summary>
  /// The MMDevice class encapsulates the IMMDevice interface, which represents an audio device.
  /// </summary>
  internal class MMDevice : IDisposable
  {
    private IMMDevice _device;
    private IAudioEndpointVolume _audioEndpointVolume;

    internal MMDevice(IMMDevice device)
    {
      if (device == null)
        throw new ArgumentNullException("MMDevice(device): device is null");
      _device = device;
    }

    public void Dispose()
    {
      _audioEndpointVolume = null;
    }

    internal AudioEndpointVolume Activate()
    {
      Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;

      if (_audioEndpointVolume == null && _device != null)
      {
        object ppInterface;
        _device.Activate (ref IID_IAudioEndpointVolume, CLSCTX.CLSCTX_ALL, IntPtr.Zero, out ppInterface);
        _audioEndpointVolume = ppInterface as IAudioEndpointVolume;
      }
      return new AudioEndpointVolume(_audioEndpointVolume);
    }

    internal string FriendlyName
    {
      get 
      {
        if (_device != null)
        {
          // The PROPERTYKEY struct for the friendly name of the audio device
          PROPERTYKEY PKEY_Device_FriendlyName = new PROPERTYKEY();
          PKEY_Device_FriendlyName.fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0");    // GUID for PKEY_Device_FriendlyName
          PKEY_Device_FriendlyName.pid = 14;                                                    // PID for PKEY_Device_FriendlyName

          // The PROPVARIANT struct for getting the friendly name
          PROPVARIANT varName = new PROPVARIANT();

          // Open devices's property store
          IPropertyStore propertyStore;
          _device.OpenPropertyStore((int)STGM.STGM_READ, out propertyStore);
          propertyStore.GetValue(ref PKEY_Device_FriendlyName, out varName);
          return Marshal.PtrToStringUni(varName.pwszVal);
        }
        return String.Empty;
      }
    }

    internal string GetState
    {
      get
      {
        if (_device != null)
        {
          // Get device's state
          DEVICE_STATE state;
          _device.GetState(out state);
          return state.ToString();
        }
        return String.Empty;
      }
    }
  }

  /// <summary>
  /// The AudioEndpointVolume class encapsulates the IAudioEndpointVolume interface,
  /// which represents the volume controls on the audio stream to or from an audio endpoint device. 
  /// </summary>
  internal class AudioEndpointVolume
  {
    private IAudioEndpointVolume _audioEndPointVolume;

    internal AudioEndpointVolume(IAudioEndpointVolume audioEndpointVolume)
    {
      if (audioEndpointVolume == null)
        throw new ArgumentNullException("AudioEndpointVolume(audioEndpointVolume): audioEndpointVolume is null");
      _audioEndPointVolume = audioEndpointVolume;
    }

    internal bool GetMute()
    {
      bool result = false;
      if (_audioEndPointVolume != null)
        _audioEndPointVolume.GetMute(out result);
      return result;
    }

    internal void SetMute(bool bMute)
    {
      if (_audioEndPointVolume != null)
        _audioEndPointVolume.SetMute(bMute, Guid.Empty);
    }
  }

  #endregion

  #region Core Audio SDK enums and interfaces

  [Flags]
  enum CLSCTX : uint
  {
    CLSCTX_INPROC_SERVER = 0x1,
    CLSCTX_INPROC_HANDLER = 0x2,
    CLSCTX_LOCAL_SERVER = 0x4,
    CLSCTX_INPROC_SERVER16 = 0x8,
    CLSCTX_REMOTE_SERVER = 0x10,
    CLSCTX_INPROC_HANDLER16 = 0x20,
    CLSCTX_RESERVED1 = 0x40,
    CLSCTX_RESERVED2 = 0x80,
    CLSCTX_RESERVED3 = 0x100,
    CLSCTX_RESERVED4 = 0x200,
    CLSCTX_NO_CODE_DOWNLOAD = 0x400,
    CLSCTX_RESERVED5 = 0x800,
    CLSCTX_NO_CUSTOM_MARSHAL = 0x1000,
    CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
    CLSCTX_NO_FAILURE_LOG = 0x4000,
    CLSCTX_DISABLE_AAA = 0x8000,
    CLSCTX_ENABLE_AAA = 0x10000,
    CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
    CLSCTX_INPROC = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,
    CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
    CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER
  }

  [Flags]
  enum DEVICE_STATE : uint
  {
    DEVICE_STATE_ACTIVE = 0x00000001,       // The audio endpoint device is active
    DEVICE_STATE_DISABLED = 0x00000002,     // The audio endpoint device is disabled
    DEVICE_STATE_NOTPRESENT = 0x00000004,   // The audio endpoint device is not present because the audio adapter that connects to the endpoint device
                                            // has been removed from the system, or the user has disabled the adapter device in Device Manager.
    DEVICE_STATE_UNPLUGGED = 0x00000008,    // The audio endpoint device is unplugged
    DEVICE_STATEMASK_ALL = 0x0000000F       // Includes audio endpoint devices in all states—active, disabled, not present, and unplugged
  }

  enum EDataFlow
  {
    eRender,
    eCapture,
    eAll,
    EDataFlow_enum_count
  }

  enum ERole
  {
    eConsole,
    eMultimedia,
    eCommunications,
    ERole_enum_count
  }

  [Flags]
  enum STGM
  {
    STGM_READ = 0x0,
    STGM_WRITE = 0x1,
    STGM_READWRITE = 0x2,
    STGM_SHARE_DENY_NONE = 0x40,
    STGM_SHARE_DENY_READ = 0x30,
    STGM_SHARE_DENY_WRITE = 0x20,
    STGM_SHARE_EXCLUSIVE = 0x10,
    STGM_PRIORITY = 0x40000,
    STGM_CREATE = 0x1000,
    STGM_CONVERT = 0x20000,
    STGM_FAILIFTHERE = 0x0,
    STGM_DIRECT = 0x0,
    STGM_TRANSACTED = 0x10000,
    STGM_NOSCRATCH = 0x100000,
    STGM_NOSNAPSHOT = 0x200000,
    STGM_SIMPLE = 0x8000000,
    STGM_DIRECT_SWMR = 0x400000,
    STGM_DELETEONRELEASE = 0x4000000
  }

  [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMMDeviceEnumerator
  {
    [PreserveSig]
    int EnumAudioEndpoints(EDataFlow dataFlow, UInt32 StateMask, out IntPtr device);
    [PreserveSig]
    int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
    [PreserveSig]
    int GetDevice(string pwstrId, out IMMDevice ppDevice);
    [PreserveSig]
    int RegisterEndpointNotificationCallback(IntPtr pClient);
    [PreserveSig]
    int UnregisterEndpointNotificationCallback(IntPtr pClient);
  }

  [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
  internal class _MMDeviceEnumerator
  {
  }

  [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMMDevice
  {
    [PreserveSig]
    int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    [PreserveSig]
    int OpenPropertyStore(Int32 stgmAccess, out IPropertyStore ppProperties);
    [PreserveSig]
    int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
    [PreserveSig]
    int GetState(out DEVICE_STATE pdwState);
  }

  [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IAudioEndpointVolume
  {
    [PreserveSig]
    int RegisterControlChangeNotify(IntPtr pNotify);
    [PreserveSig]
    int UnregisterControlChangeNotify(IntPtr pNotify);
    [PreserveSig]
    int GetChannelCount(out int pnChannelCount);
    [PreserveSig]
    int SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);
    [PreserveSig]
    int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);
    [PreserveSig]
    int GetMasterVolumeLevel(out float pfLevelDB);
    [PreserveSig]
    int GetMasterVolumeLevelScalar(out float pfLevel);
    [PreserveSig]
    int SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);
    [PreserveSig]
    int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);
    [PreserveSig]
    int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
    [PreserveSig]
    int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
    [PreserveSig]
    int SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, Guid pguidEventContext);
    [PreserveSig]
    int GetMute(out bool pbMute);
    [PreserveSig]
    int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
    [PreserveSig]
    int VolumeStepUp(Guid pguidEventContext);
    [PreserveSig]
    int VolumeStepDown(Guid pguidEventContext);
    [PreserveSig]
    int QueryHardwareSupport(out uint pdwHardwareSupportMask);
    [PreserveSig]
    int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  internal struct PROPERTYKEY
  {
    public Guid fmtid;
    public uint pid;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct PROPVARIANT
  {
    public ushort vt;
    public ushort wReserved1, wReserved2, wReserved3;
    public IntPtr pwszVal;
    public int p2;
  }

  [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IPropertyStore
  {
    [PreserveSig]
    int GetCount(out int propCount);
    [PreserveSig]
    int GetAt(int property, out PROPERTYKEY key);
    [PreserveSig]
    int GetValue(ref PROPERTYKEY key, out PROPVARIANT value);
    [PreserveSig]
    int SetValue(ref PROPERTYKEY key, ref object value);
    [PreserveSig]
    int Commit();
  }

  #endregion

}