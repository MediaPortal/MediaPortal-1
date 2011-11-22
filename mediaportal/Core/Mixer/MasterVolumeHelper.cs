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

// Parts of the code is borrowed from the CodeProject site. 
// License of that borrowed code is Below:
/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/


#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MediaPortal.Mixer
{
  public delegate void AudioEndpointVolumeNotificationDelegate(AudioVolumeNotificationData data);

  [StructLayout(LayoutKind.Sequential)]
  public struct PropVariant
  {
    #region struct fields

    ushort vt;
    ushort wReserved1;
    ushort wReserved2;
    ushort wReserved3;
    IntPtr p;
    int p2;

    #endregion

    #region union members

    sbyte cVal // CHAR cVal;
    {
      get { return (sbyte)GetDataBytes()[0]; }
    }

    byte bVal // UCHAR bVal;
    {
      get { return GetDataBytes()[0]; }
    }

    short iVal // SHORT iVal;
    {
      get { return BitConverter.ToInt16(GetDataBytes(), 0); }
    }

    ushort uiVal // USHORT uiVal;
    {
      get { return BitConverter.ToUInt16(GetDataBytes(), 0); }
    }

    int lVal // LONG lVal;
    {
      get { return BitConverter.ToInt32(GetDataBytes(), 0); }
    }

    uint ulVal // ULONG ulVal;
    {
      get { return BitConverter.ToUInt32(GetDataBytes(), 0); }
    }

    long hVal // LARGE_INTEGER hVal;
    {
      get { return BitConverter.ToInt64(GetDataBytes(), 0); }
    }

    ulong uhVal // ULARGE_INTEGER uhVal;
    {
      get { return BitConverter.ToUInt64(GetDataBytes(), 0); }
    }

    float fltVal // FLOAT fltVal;
    {
      get { return BitConverter.ToSingle(GetDataBytes(), 0); }
    }

    double dblVal // DOUBLE dblVal;
    {
      get { return BitConverter.ToDouble(GetDataBytes(), 0); }
    }

    bool boolVal // VARIANT_BOOL boolVal;
    {
      get { return (iVal == 0 ? false : true); }
    }

    int scode // SCODE scode;
    {
      get { return lVal; }
    }

    decimal cyVal // CY cyVal;
    {
      get { return decimal.FromOACurrency(hVal); }
    }

    DateTime date // DATE date;
    {
      get { return DateTime.FromOADate(dblVal); }
    }

    #endregion // union members

    private byte[] GetDataBytes()
    {
      byte[] ret = new byte[IntPtr.Size + sizeof(int)];
      if (IntPtr.Size == 4)
        BitConverter.GetBytes(p.ToInt32()).CopyTo(ret, 0);
      else if (IntPtr.Size == 8)
        BitConverter.GetBytes(p.ToInt64()).CopyTo(ret, 0);
      BitConverter.GetBytes(p2).CopyTo(ret, IntPtr.Size);
      return ret;
    }

    [DllImport("ole32.dll")]
    private extern static int PropVariantClear(ref PropVariant pvar);

    public void Clear()
    {
      PropVariant var = this;
      PropVariantClear(ref var);

      vt = (ushort)VarEnum.VT_EMPTY;
      wReserved1 = wReserved2 = wReserved3 = 0;
      p = IntPtr.Zero;
      p2 = 0;
    }

    public VarEnum Type
    {
      get { return (VarEnum)vt; }
    }

    public object Value
    {
      get
      {
        switch ((VarEnum)vt)
        {
          case VarEnum.VT_I1:
            return cVal;
          case VarEnum.VT_UI1:
            return bVal;
          case VarEnum.VT_I2:
            return iVal;
          case VarEnum.VT_UI2:
            return uiVal;
          case VarEnum.VT_I4:
          case VarEnum.VT_INT:
            return lVal;
          case VarEnum.VT_UI4:
          case VarEnum.VT_UINT:
            return ulVal;
          case VarEnum.VT_I8:
            return hVal;
          case VarEnum.VT_UI8:
            return uhVal;
          case VarEnum.VT_R4:
            return fltVal;
          case VarEnum.VT_R8:
            return dblVal;
          case VarEnum.VT_BOOL:
            return boolVal;
          case VarEnum.VT_ERROR:
            return scode;
          case VarEnum.VT_CY:
            return cyVal;
          case VarEnum.VT_DATE:
            return date;
          case VarEnum.VT_FILETIME:
            return DateTime.FromFileTime(hVal);
          case VarEnum.VT_BSTR:
            return Marshal.PtrToStringBSTR(p);
          case VarEnum.VT_BLOB:
            byte[] blobData = new byte[lVal];
            IntPtr pBlobData;
            if (IntPtr.Size == 4)
            {
              pBlobData = new IntPtr(p2);
            }
            else if (IntPtr.Size == 8)
            {
              pBlobData = new IntPtr(BitConverter.ToInt64(GetDataBytes(), sizeof(int)));
            }
            else
              throw new NotSupportedException();
            Marshal.Copy(pBlobData, blobData, 0, lVal);
            return blobData;
          case VarEnum.VT_LPSTR:
            return Marshal.PtrToStringAnsi(p);
          case VarEnum.VT_LPWSTR:
            return Marshal.PtrToStringUni(p);
          case VarEnum.VT_UNKNOWN:
            return Marshal.GetObjectForIUnknown(p);
          case VarEnum.VT_DISPATCH:
            return p;
          default:
            throw new NotSupportedException("The type of this variable is not support ('" + vt.ToString() + "')");
        }
      }
    }
  }

  //Windows Communication Foundation Allmost ready made enumerator.
  [Flags]
  enum CTX : uint
  {
    INPROC_SERVER = 0x1,
    INPROC_HANDLER = 0x2,
    LOCAL_SERVER = 0x4,
    INPROC_SERVER16 = 0x8,
    REMOTE_SERVER = 0x10,
    INPROC_HANDLER16 = 0x20,
    RESERVED1 = 0x40,
    RESERVED2 = 0x80,
    RESERVED3 = 0x100,
    RESERVED4 = 0x200,
    NO_CODE_DOWNLOAD = 0x400,
    RESERVED5 = 0x800,
    NO_CUSTOM_MARSHAL = 0x1000,
    ENABLE_CODE_DOWNLOAD = 0x2000,
    NO_FAILURE_LOG = 0x4000,
    DISABLE_AAA = 0x8000,
    ENABLE_AAA = 0x10000,
    FROM_DEFAULT_CONTEXT = 0x20000,
    ALL = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER | INPROC_HANDLER
  }

  //MIDL_INTERFACE("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")
  //00475     IMMDeviceCollection : public IUnknown
  //00476     {
  //00477     public:
  //00478         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetCount( 
  //00479             /* [out] */ 
  //00480             __out  UINT *pcDevices) = 0;
  //00481         
  //00482         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Item( 
  //00483             /* [in] */ 
  //00484             __in  UINT nDevice,
  //00485             /* [out] */ 
  //00486             __out  IMMDevice **ppDevice) = 0;
  //00487         
  //00488     };

  [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IMMDeviceCollection
  {
    [PreserveSig]
    int GetCount(out uint pcDevices);
    [PreserveSig]
    int Item(uint nDevice, out IMMDevice ppDevice);
  }

  //    EXTERN_C const IID IID_IMMDeviceEnumerator;
  //00649 
  //00650 #if defined(__cplusplus) && !defined(CINTERFACE)
  //00651     
  //00652     MIDL_INTERFACE("A95664D2-9614-4F35-A746-DE8DB63617E6")
  //00653     IMMDeviceEnumerator : public IUnknown
  //00654     {
  //00655     public:
  //00656         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE EnumAudioEndpoints( 
  //00657             /* [in] */ 
  //00658             __in  EDataFlow dataFlow,
  //00659             /* [in] */ 
  //00660             __in  DWORD dwStateMask,
  //00661             /* [out] */ 
  //00662             __out  IMMDeviceCollection **ppDevices) = 0;
  //00663         
  //00664         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetDefaultAudioEndpoint( 
  //00665             /* [in] */ 
  //00666             __in  EDataFlow dataFlow,
  //00667             /* [in] */ 
  //00668             __in  ERole role,
  //00669             /* [out] */ 
  //00670             __out  IMMDevice **ppEndpoint) = 0;
  //00671         
  //00672         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetDevice( 
  //00673             /*  */ 
  //00674             __in  LPCWSTR pwstrId,
  //00675             /* [out] */ 
  //00676             __out  IMMDevice **ppDevice) = 0;
  //00677         
  //00678         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE RegisterEndpointNotificationCallback( 
  //00679             /* [in] */ 
  //00680             __in  IMMNotificationClient *pClient) = 0;
  //00681         
  //00682         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE UnregisterEndpointNotificationCallback( 
  //00683             /* [in] */ 
  //00684             __in  IMMNotificationClient *pClient) = 0;
  //00685         
  //00686     };

  [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IMMDeviceEnumerator
  {
    [PreserveSig]
    int EnumAudioEndpoints(uint dataFlow, devstatus StateMask, out IMMDeviceCollection device);
    [PreserveSig]
    int GetDefaultAudioEndpoint(uint dataFlow, uint role, out IMMDevice endpoint);
    [PreserveSig]
    int GetDevice(string pwstrId, out IMMDevice device);
    [PreserveSig]
    int RegisterEndpointNotificationCallback(IntPtr pclient);
    [PreserveSig]
    int UnregisterEndpointNotificationCallback(IntPtr pclient);
  }

  //stgmAccess [in]

  //The storage-access mode. This parameter specifies whether to open the property store in read mode, write mode, or read/write mode. Set this parameter to one of the following STGM constants:

  //STGM_READ

  //STGM_WRITE

  //STGM_READWRITE

  //The method permits a client running as an administrator to open a store for read-only, write-only, or read/write access. A client that is not running as an administrator is restricted to read-only access. For more information about STGM constants, see the Windows SDK documentation.

  enum Stgmacces
  {
    STGM_READ = 0x00000000,
    STGM_WRITE = 0x00000001,
    STGM_READWRITE = 0x00000002
  }

  /*typedef struct {
    GUID  fmtid;
    DWORD pid;
  } PROPERTYKEY;*/

  struct PROPERTYKEY
  {
    public Guid id;
    public int pid;
  };

  //         MIDL_INTERFACE("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")
  //00422     IPropertyStore : public IUnknown
  //00423     {
  //00424     public:
  //00425         virtual HRESULT STDMETHODCALLTYPE GetCount( 
  //00426             /* [out] */ __RPC__out DWORD *cProps) = 0;
  //00427         
  //00428         virtual HRESULT STDMETHODCALLTYPE GetAt( 
  //00429             /* [in] */ DWORD iProp,
  //00430             /* [out] */ __RPC__out PROPERTYKEY *pkey) = 0;
  //00431         
  //00432         virtual HRESULT STDMETHODCALLTYPE GetValue( 
  //00433             /* [in] */ __RPC__in REFPROPERTYKEY key,
  //00434             /* [out] */ __RPC__out PROPVARIANT *pv) = 0;
  //00435         
  //00436         virtual HRESULT STDMETHODCALLTYPE SetValue( 
  //00437             /* [in] */ __RPC__in REFPROPERTYKEY key,
  //00438             /* [in] */ __RPC__in REFPROPVARIANT propvar) = 0;
  //00439         
  //00440         virtual HRESULT STDMETHODCALLTYPE Commit( void) = 0;
  //00441         
  //00442     };

  [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IPropertyStore
  {
    [PreserveSig]
    int GetCount(out Int32 count);
    [PreserveSig]
    int GetAt(int Prop, out PROPERTYKEY pkey);
    [PreserveSig]
    int GetValue(ref PROPERTYKEY pkey, out PropVariant ppv);
    [PreserveSig]
    int SetValue(ref PROPERTYKEY pkey, ref PropVariant ppropvar);
    [PreserveSig]
    int Commit();
  };

  //    DEVICE_STATE_ACTIVE
  //0x00000001
  //The audio endpoint device is active. That is, the audio adapter that connects to the endpoint device is present and enabled. In addition, if the endpoint device plugs into a jack on the adapter, then the endpoint device is plugged in.
  //DEVICE_STATE_DISABLED
  //0x00000002
  //The audio endpoint device is disabled. The user has disabled the device in the Windows multimedia control panel, Mmsys.cpl. For more information, see Remarks.
  //DEVICE_STATE_NOTPRESENT
  //0x00000004
  //The audio endpoint device is not present because the audio adapter that connects to the endpoint device has been removed from the system, or the user has disabled the adapter device in Device Manager.
  //DEVICE_STATE_UNPLUGGED
  //0x00000008
  //The audio endpoint device is unplugged. The audio adapter that contains the jack for the endpoint device is present and enabled, but the endpoint device is not plugged into the jack. Only a device with jack-presence detection can be in this state. For more information about jack-presence detection, see Audio Endpoint Devices.
  //DEVICE_STATEMASK_ALL
  //0x0000000F 

  [Flags]
  public enum devstatus : uint
  {
    DEVICE_STATE_ACTIVE = 0x00000001,
    DEVICE_STATE_DISABLED = 0x00000002,
    DEVICE_STATE_NOTPRESENT = 0x00000004,
    DEVICE_STATE_UNPLUGGED = 0x00000008,
    DEVICE_STATEMASK_ALL = 0x0000000F
  }

  //      MIDL_INTERFACE("D666063F-1587-4E43-81F1-B948E807363F")
  //00342     IMMDevice : public IUnknown
  //00343     {
  //00344     public:
  //00345         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Activate( 
  //00346             /* [in] */ 
  //00347             __in  REFIID iid,
  //00348             /* [in] */ 
  //00349             __in  DWORD dwClsCtx,
  //00350             /* [unique][in] */ 
  //00351             __in_opt  PROPVARIANT *pActivationParams,
  //00352             /* [iid_is][out] */ 
  //00353             __out  void **ppInterface) = 0;
  //00354         
  //00355         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE OpenPropertyStore( 
  //00356             /* [in] */ 
  //00357             __in  DWORD stgmAccess,
  //00358             /* [out] */ 
  //00359             __out  IPropertyStore **ppProperties) = 0;
  //00360         
  //00361         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetId( 
  //00362             /* [out] */ 
  //00363             __deref_out  LPWSTR *ppstrId) = 0;
  //00364         
  //00365         virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetState( 
  //00366             /* [out] */ 
  //00367             __out  DWORD *pdwState) = 0;
  //00368         
  //00369     };

  [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IMMDevice
  {
    [PreserveSig]
    int Activate(ref Guid iid, CTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    [PreserveSig]
    int OpenPropertyStore(Stgmacces stgmAccess, out IPropertyStore propertyStore);
    [PreserveSig]
    int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
    [PreserveSig]
    int GetState(out devstatus pdwState);
  }

  //  MIDL_INTERFACE("657804FA-D6AD-4496-8A60-352752AF4F89")
  //00112     IAudioEndpointVolumeCallback : public IUnknown
  //00113     {
  //00114     public:
  //00115         virtual HRESULT STDMETHODCALLTYPE OnNotify( 
  //00116             PAUDIO_VOLUME_NOTIFICATION_DATA pNotify) = 0;
  //00117         
  //00118     };   

  [Guid("657804FA-D6AD-4496-8A60-352752AF4F89"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAudioEndpointVolumeCallback
  {
    [PreserveSig]
    int OnNotify(IntPtr pNotifyData);
  };

  //      MIDL_INTERFACE("5CDF2C82-841E-4546-9722-0CF74078229A")
  //00191     IAudioEndpointVolume : public IUnknown
  //00192     {
  //00193     public:
  //00194         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE RegisterControlChangeNotify( 
  //00195             /* [in] */ 
  //00196             __in  IAudioEndpointVolumeCallback *pNotify) = 0;
  //00197         
  //00198         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE UnregisterControlChangeNotify( 
  //00199             /* [in] */ 
  //00200             __in  IAudioEndpointVolumeCallback *pNotify) = 0;
  //00201         
  //00202         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetChannelCount( 
  //00203             /* [out] */ 
  //00204             __out  UINT *pnChannelCount) = 0;
  //00205         
  //00206         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetMasterVolumeLevel( 
  //00207             /* [in] */ 
  //00208             __in  float fLevelDB,
  //00209             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00210         
  //00211         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetMasterVolumeLevelScalar( 
  //00212             /* [in] */ 
  //00213             __in  float fLevel,
  //00214             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00215         
  //00216         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetMasterVolumeLevel( 
  //00217             /* [out] */ 
  //00218             __out  float *pfLevelDB) = 0;
  //00219         
  //00220         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetMasterVolumeLevelScalar( 
  //00221             /* [out] */ 
  //00222             __out  float *pfLevel) = 0;
  //00223         
  //00224         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetChannelVolumeLevel( 
  //00225             /* [in] */ 
  //00226             __in  UINT nChannel,
  //00227             float fLevelDB,
  //00228             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00229         
  //00230         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetChannelVolumeLevelScalar( 
  //00231             /* [in] */ 
  //00232             __in  UINT nChannel,
  //00233             float fLevel,
  //00234             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00235         
  //00236         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetChannelVolumeLevel( 
  //00237             /* [in] */ 
  //00238             __in  UINT nChannel,
  //00239             /* [out] */ 
  //00240             __out  float *pfLevelDB) = 0;
  //00241         
  //00242         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetChannelVolumeLevelScalar( 
  //00243             /* [in] */ 
  //00244             __in  UINT nChannel,
  //00245             /* [out] */ 
  //00246             __out  float *pfLevel) = 0;
  //00247         
  //00248         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetMute( 
  //00249             /* [in] */ 
  //00250             __in  BOOL bMute,
  //00251             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00252         
  //00253         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetMute( 
  //00254             /* [out] */ 
  //00255             __out  BOOL *pbMute) = 0;
  //00256         
  //00257         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetVolumeStepInfo( 
  //00258             /* [out] */ 
  //00259             __out  UINT *pnStep,
  //00260             /* [out] */ 
  //00261             __out  UINT *pnStepCount) = 0;
  //00262         
  //00263         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE VolumeStepUp( 
  //00264             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00265         
  //00266         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE VolumeStepDown( 
  //00267             /* [unique][in] */ LPCGUID pguidEventContext) = 0;
  //00268         
  //00269         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE QueryHardwareSupport( 
  //00270             /* [out] */ 
  //00271             __out  DWORD *pdwHardwareSupportMask) = 0;
  //00272         
  //00273         virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetVolumeRange( 
  //00274             /* [out] */ 
  //00275             __out  float *pflVolumeMindB,
  //00276             /* [out] */ 
  //00277             __out  float *pflVolumeMaxdB,
  //00278             /* [out] */ 
  //00279             __out  float *pflVolumeIncrementdB) = 0;
  //00280         
  //00281     };

  [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAudioEndpointVolume
  {
    [PreserveSig]
    int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
    [PreserveSig]
    int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
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

  //    class DECLSPEC_UUID("BCDE0395-E52F-467C-8E3D-C4579291692E")
  //00914 MMDeviceEnumerator;

  [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
  class _AEDeviceEnumerator
  {
  }

  public class AudioVolumeNotificationData
  {
    private Guid _EventContext;
    private bool _Muted;
    private float _MasterVolume;
    private int _Channels;
    private float[] _ChannelVolume;

    public Guid EventContext
    {
      get
      {
        return _EventContext;
      }
    }

    public bool Muted
    {
      get
      {
        return _Muted;
      }
    }

    public float MasterVolume
    {
      get
      {
        return _MasterVolume;
      }
    }
    public int Channels
    {
      get
      {
        return _Channels;
      }
    }

    public float[] ChannelVolume
    {
      get
      {
        return _ChannelVolume;
      }
    }
    public AudioVolumeNotificationData(Guid eventContext, bool muted, float masterVolume, float[] channelVolume)
    {
      _EventContext = eventContext;
      _Muted = muted;
      _MasterVolume = masterVolume;
      _Channels = channelVolume.Length;
      _ChannelVolume = channelVolume;
    }
  }

  internal struct AUDIO_VOLUME_NOTIFICATION_DATA
  {
    public Guid guidEventContext;
    public bool bMuted;
    public float fMasterVolume;
    public uint nChannels;
    public float ChannelVolume;

    private void FixCS0649()
    {
      guidEventContext = Guid.Empty;
      bMuted = false;
      fMasterVolume = 0;
      nChannels = 0;
      ChannelVolume = 0;
    }
  }

  internal class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
  {
    private AEDev _Parent;

    internal AudioEndpointVolumeCallback(AEDev parent)
    {
      _Parent = parent;
    }

    [PreserveSig]
    public int OnNotify(IntPtr NotifyData)
    {
      //Since AUDIO_VOLUME_NOTIFICATION_DATA is dynamic in length based on the
      //number of audio channels available we cannot just call PtrToStructure 
      //to get all data, thats why it is split up into two steps, first the static
      //data is marshalled into the data structure, then with some IntPtr math the
      //remaining floats are read from memory.
      AUDIO_VOLUME_NOTIFICATION_DATA data = (AUDIO_VOLUME_NOTIFICATION_DATA)Marshal.PtrToStructure(NotifyData, typeof(AUDIO_VOLUME_NOTIFICATION_DATA));

      //Determine offset in structure of the first float
      IntPtr Offset = Marshal.OffsetOf(typeof(AUDIO_VOLUME_NOTIFICATION_DATA), "ChannelVolume");
      //Determine offset in memory of the first float
      IntPtr FirstFloatPtr = (IntPtr)((long)NotifyData + (long)Offset);

      float[] voldata = new float[data.nChannels];

      //Read all floats from memory.
      for (int i = 0; i < data.nChannels; i++)
      {
        voldata[i] = (float)Marshal.PtrToStructure(FirstFloatPtr, typeof(float));
      }

      //Create combined structure and Fire Event in parent class.
      AudioVolumeNotificationData NotificationData = new AudioVolumeNotificationData(data.guidEventContext, data.bMuted, data.fMasterVolume, voldata);
      _Parent.FireNotification(NotificationData);
      return 0; //S_OK
    }
  }
  
  public class AEDev : IDisposable
  {
    private IMMDevice _RealDevice;
    // private AudEVol _AudioEndpointVolume;
    private static Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
    private IMMDeviceEnumerator _realEnumerator = new _AEDeviceEnumerator() as IMMDeviceEnumerator;
    private IAudioEndpointVolume _AudioEndPointVolume;
    private AudioEndpointVolumeCallback _CallBack;
    public event AudioEndpointVolumeNotificationDelegate OnVolumeNotification;

    internal AEDev()
    {
      IMMDevice _Device = null;
      Marshal.ThrowExceptionForHR(((IMMDeviceEnumerator)_realEnumerator).GetDefaultAudioEndpoint(0, 1, out _Device));
      _RealDevice = _Device;
      object result;
      Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioEndpointVolume, CTX.ALL, IntPtr.Zero, out result));
      _AudioEndPointVolume = result as IAudioEndpointVolume;
      _CallBack = new AudioEndpointVolumeCallback(this);
      Marshal.ThrowExceptionForHR(_AudioEndPointVolume.RegisterControlChangeNotify(_CallBack));
    }

    public void Dispose()
    {
      if (_CallBack != null)
      {
        Marshal.ThrowExceptionForHR(_AudioEndPointVolume.UnregisterControlChangeNotify(_CallBack));
        _CallBack = null;
      }
    }

    internal void FireNotification(AudioVolumeNotificationData NotificationData)
    {
      AudioEndpointVolumeNotificationDelegate del = OnVolumeNotification;
      if (del != null)
      {
        del(NotificationData);
      }
    }

    public float MasterVolume
    {
      get
      {
        float result = 0;
        Marshal.ThrowExceptionForHR(_AudioEndPointVolume.GetMasterVolumeLevelScalar(out result));
        return result;
      }
      set
      {
        Marshal.ThrowExceptionForHR(_AudioEndPointVolume.SetMasterVolumeLevelScalar(value, Guid.Empty));
      }
    }

    public bool Muted
    {
      get
      {
        bool result = false;
        Marshal.ThrowExceptionForHR(_AudioEndPointVolume.GetMute(out result));
        return result;
      }
      set
      {
        Marshal.ThrowExceptionForHR(_AudioEndPointVolume.SetMute(value, Guid.Empty));
      }
    }
  }
}
