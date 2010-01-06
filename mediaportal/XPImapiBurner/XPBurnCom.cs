#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace XPBurn.COM
{
  [ComImport]
  [Guid("520CCA63-51A5-11D3-9144-00104BA11C5E")]
  internal class MSDiscMasterObj {}

  // TODO: This should be a static class
  internal class GUIDS
  {
    public static readonly Guid IID_IJolietDiscMaster = new Guid("E3BC42CE-4E5C-11D3-9144-00104BA11C5E");
    public static readonly Guid IID_IRedbookDiscMaster = new Guid("E3BC42CD-4E5C-11D3-9144-00104BA11C5E");
  }

  [ComImport]
  [Guid("EC9E51C1-4E5D-11D3-9144-00104BA11C5E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IDiscMasterProgressEvents
  {
    void QueryCancel([MarshalAs(UnmanagedType.I1)] out bool pbCancel);
    void NotifyPnPActivity();
    void NotifyAddProgress(int nCompletedSteps, int nTotalSteps);
    void NotifyBlockProgress(int nCompleted, int nTotal);
    void NotifyTrackProgress(int nCurrentTrack, int nTotalTrack);
    void NotifyPreparingBurn(int nEstimatedSeconds);
    void NotifyClosingDisc(int nEstimatedSeconds);
    void NotifyBurnComplete(uint statusHR);
    void NotifyEraseComplete(uint statusHR);
  }

  [ComImport]
  [Guid("DDF445E1-54BA-11d3-9144-00104BA11C5E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal unsafe interface IEnumDiscMasterFormats
  {
    void Next(uint cFormats, out Guid lpiidFormatID, out uint pcFetched);
    void Skip(uint cFormats);
    void Reset();
    void Clone(void** ppEnum);
  }

  [ComImport]
  [Guid("85AC9776-CA88-4cf2-894E-09598C078A41")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IDiscRecorder
  {
    void Init(ref byte pbyUniqueID, uint nulIDSize, uint nulDriveNumber);
    void GetRecorderGUID(ref byte pbyUniqueID, uint ulBufferSize, out uint pulReturnSizeRequired);
    void GetRecorderType(out int fTypeCode);

    void GetDisplayNames([MarshalAs(UnmanagedType.BStr)] out string pbstrVendorID,
                         [MarshalAs(UnmanagedType.BStr)] out string pbstrProductID,
                         [MarshalAs(UnmanagedType.BStr)] out string pbstrRevision);

    void GetBasePnPID([MarshalAs(UnmanagedType.LPWStr)] string pbstrBasePnPID);
    void GetPath([MarshalAs(UnmanagedType.BStr)] out string pbstrPath);
    void GetRecorderProperties(out IPropertyStorage ppPropStg);
    void SetRecorderProperties(IPropertyStorage pPropStg);
    void GetRecorderState(out uint pulDevStateFlags);
    void OpenExclusive();
    void QueryMediaType(out int fMediaType, out int fMediaFlags);

    void QueryMediaInfo(out byte pbSessions, out byte pbLastTrack, out uint ulStartAddresss, out uint ulNextWritable,
                        out uint ulFreeBlocks);

    void Eject();
    void Erase(bool bFullErase);
    void Close();
  }

  [ComImport]
  [Guid("9B1921E1-54AC-11d3-9144-00104BA11C5E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal unsafe interface IEnumDiscRecorders
  {
    void Next(uint cRecorders, out IDiscRecorder ppRecorder, out uint pcFetched);
    void Skip(uint cRecorders);
    void Reset();
    void Clone(void** ppEnum);
  }

  [ComImport]
  [Guid("00000000-0000-0000-c000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IUnknown {}

  [ComImport]
  [Guid("520CCA62-51A5-11D3-9144-00104BA11C5E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IDiscMaster
  {
    void Open();
    void EnumDiscMasterFormats(out IEnumDiscMasterFormats ppEnum);
    void GetActiveDiscMasterFormat(out Guid lpiid);
    void SetActiveDiscMasterFormat(ref Guid riid, out IUnknown ppUnk);
    void EnumDiscRecorders(out IEnumDiscRecorders ppRecorder);
    void GetActiveDiscRecorder(out IDiscRecorder pRecorder);
    void SetActiveDiscRecorder(IDiscRecorder pRecorder);
    void ClearFormatContent();
    unsafe void ProgressAdvise(IDiscMasterProgressEvents pEvents, uint** pvCookie);
    unsafe void ProgressUnadvise(uint* vCookie);
    void RecordDisc(bool bSimulate, bool bEjectAfterBurn);
    void Close();
  }

  [ComImport]
  [Guid("E3BC42CE-4E5C-11D3-9144-00104BA11C5E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IJolietDiscMaster
  {
    void GetTotalDataBlocks(out int pnBlocks);
    void GetUsedDataBlocks(out int pnBlocks);
    void GetDataBlockSize(out int pnBlockBytes);
    void AddData(IStorage pStorage, int lFileOverwrite);
    void GetJolietProperties(out IPropertyStorage ppPropStg);
    void SetJolietProperties(IPropertyStorage ppPropStg);
  }

  [ComImport]
  [Guid("E3BC42CD-4E5C-11D3-9144-00104BA11C5E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IRedbookMaster
  {
    void GetTotalAudioTracks(out int pnTracks);
    void GetTotalAudioBlocks(out int pnBlocks);
    void GetUsedAudioBlocks(out int pnBlocks);
    void GetAvailableAudioTrackBlocks(out int pnBlocks);
    void GetAudioBlockSize(out int pnBlockBytes);
    void CreateAudioTrack(int nBlocks);
    void AddAudioTrackBlocks(ref byte pby, int cb);
    void CloseAudioTrack();
  }

  [ComImport]
  [Guid("00000138-0000-0000-c000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IPropertyStorage
  {
    unsafe void ReadMultiple(uint p1, ref PROPSPEC p2, ref PROPVARIANT p3);
    unsafe void WriteMultiple(uint p1, PROPSPEC* p2, PROPVARIANT* p3, uint p4);

    [PreserveSig]
    unsafe int DeleteMultiple(uint p1, PROPSPEC* p2);

    [PreserveSig]
    unsafe int ReadPropertyNames(uint p1, uint* p2, char** p3);

    [PreserveSig]
    unsafe int WritePropertyNames(uint p1, uint* p2, char** p3);

    [PreserveSig]
    unsafe int DeletePropertyNames(uint p1, uint* p2);

    [PreserveSig]
    int Commit(uint p1);

    [PreserveSig]
    int Revert();

    [PreserveSig]
    unsafe int Enum(void** p1);

    [PreserveSig]
    unsafe int SetTimes(FILETIME* p1, FILETIME* p2, FILETIME* p3);

    [PreserveSig]
    unsafe int SetClass(Guid* p1);

    [PreserveSig]
    unsafe int Stat(STATPROPSETSTG* p1);
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct PROPSPEC
  {
    [FieldOffset(0)] public uint ulKind;
    [FieldOffset(4)] public PROPKIND __unnamed;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct PROPKIND
  {
    [FieldOffset(0)] public uint propid;
    [FieldOffset(0)] public char* lpwstr;
  }

/*  struct tag_inner_PROPVARIANT
  {
    VARTYPE vt;
    PROPVAR_PAD1   wReserved1;
    PROPVAR_PAD2   wReserved2;
    PROPVAR_PAD3   wReserved3;
    [switch_is((unsigned short) vt)] union*/

  [StructLayout(LayoutKind.Explicit)]
  internal struct PROPVARIANT
  {
    [FieldOffset(0)] public VARIANTKIND __unnamed;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct VARIANTKIND
  {
    [FieldOffset(0)] public VARIANTHEADER __unnamed;
    [FieldOffset(0)] public tagDEC decVal;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct VARIANTHEADER
  {
    [FieldOffset(0)] public ushort vt;
    [FieldOffset(2)] public ushort wReserved1;
    [FieldOffset(4)] public ushort wReserved2;
    [FieldOffset(6)] public ushort wReserved3;
    [FieldOffset(8)] public VARIANTUNION __unnamed;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct VARIANTUNION
  {
    [FieldOffset(0)] public sbyte cVal;
    [FieldOffset(0)] public byte bVal;
    [FieldOffset(0)] public short iVal;
    [FieldOffset(0)] public ushort uiVal;
    [FieldOffset(0)] public int lVal;
    [FieldOffset(0)] public uint ulVal;
    [FieldOffset(0)] public int intVal;
    [FieldOffset(0)] public uint uintVal;
    [FieldOffset(0)] public long hVal;
    [FieldOffset(0)] public ulong uhVal;
    [FieldOffset(0)] public float fltVal;
    [FieldOffset(0)] public double dblVal;
    [FieldOffset(0)] public short boolVal;
    [FieldOffset(0)] public int scode;
    [FieldOffset(0)] public CY cyVal;
    [FieldOffset(0)] public double date;
    [FieldOffset(0)] public FILETIME filetime;
    [FieldOffset(0)] public Guid* puuid;
    [FieldOffset(0)] public CLIPDATA* pclipdata;
    [FieldOffset(0)] public char* bstrVal;
    [FieldOffset(0)] public BSTRBLOB bstrblobVal;
    [FieldOffset(0)] public BLOB blob;
    [FieldOffset(0)] public sbyte* pszVal;
    [FieldOffset(0)] public char* pwszVal;
    [FieldOffset(0)] public void* punkVal;
    [FieldOffset(0)] public void* pdispVal;
    [FieldOffset(0)] public void* pStream;
    [FieldOffset(0)] public void* pStorage;
    [FieldOffset(0)] public tagVersionedStream* pVersionedStream;
    [FieldOffset(0)] public SAFEARRAY* parray;
    [FieldOffset(0)] public CAC cac;
    [FieldOffset(0)] public CAUB caub;
    [FieldOffset(0)] public CAI cai;
    [FieldOffset(0)] public CAUI caui;
    [FieldOffset(0)] public CAL cal;
    [FieldOffset(0)] public CAUL caul;
    [FieldOffset(0)] public CAH cah;
    [FieldOffset(0)] public CAUH cauh;
    [FieldOffset(0)] public CAFLT caflt;
    [FieldOffset(0)] public CADBL cadbl;
    [FieldOffset(0)] public CABOOL cabool;
    [FieldOffset(0)] public CASCODE cascode;
    [FieldOffset(0)] public CACY cacy;
    [FieldOffset(0)] public CADATE cadate;
    [FieldOffset(0)] public CAFILETIME cafiletime;
    [FieldOffset(0)] public CACLSID cauuid;
    [FieldOffset(0)] public CACLIPDATA caclipdata;
    [FieldOffset(0)] public CABSTR cabstr;
    [FieldOffset(0)] public CABSTRBLOB cabstrblob;
    [FieldOffset(0)] public CALPSTR calpstr;
    [FieldOffset(0)] public CALPWSTR calpwstr;
    [FieldOffset(0)] public CAPROPVARIANT capropvar;
    [FieldOffset(0)] public sbyte* pcVal;
    [FieldOffset(0)] public byte* pbVal;
    [FieldOffset(0)] public short* piVal;
    [FieldOffset(0)] public ushort* puiVal;
    [FieldOffset(0)] public int* plVal;
    [FieldOffset(0)] public uint* pulVal;
    [FieldOffset(0)] public int* pintVal;
    [FieldOffset(0)] public uint* puintVal;
    [FieldOffset(0)] public float* pfltVal;
    [FieldOffset(0)] public double* pdblVal;
    [FieldOffset(0)] public short* pboolVal;
    [FieldOffset(0)] public tagDEC* pdecVal;
    [FieldOffset(0)] public int* pscode;
    [FieldOffset(0)] public CY* pcyVal;
    [FieldOffset(0)] public double* pdate;
    [FieldOffset(0)] public char** pbstrVal;
    [FieldOffset(0)] public void** ppunkVal;
    [FieldOffset(0)] public void** ppdispVal;
    [FieldOffset(0)] public SAFEARRAY** pparray;
    [FieldOffset(0)] public PROPVARIANT* pvarVal;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct CY
  {
    [FieldOffset(0)] public LOHI __unnamed;
    [FieldOffset(0)] public long int64;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct LOHI
  {
    [FieldOffset(0)] public uint Lo;
    [FieldOffset(4)] public int Hi;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct FILETIME
  {
    [FieldOffset(0)] public uint dwLowDateTime;
    [FieldOffset(4)] public uint dwHighDateTime;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CLIPDATA
  {
    [FieldOffset(0)] public uint cbSize;
    [FieldOffset(4)] public int ulClipFmt;
    [FieldOffset(8)] public byte* pClipData;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct BSTRBLOB
  {
    [FieldOffset(0)] public uint cbSize;
    [FieldOffset(4)] public byte* pData;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct BLOB
  {
    [FieldOffset(0)] public uint cbSize;
    [FieldOffset(4)] public byte* pBlobData;
  }

  [ComImport]
  [Guid("0000000c-0000-0000-c000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IStream
  {
    [PreserveSig]
    unsafe int Read(void* p1, uint p2, uint* p3);

    [PreserveSig]
    unsafe int Write(void* p1, uint p2, uint* p3);

    [PreserveSig]
    unsafe int Seek(long p1, uint p2, ulong* p3);

    [PreserveSig]
    int SetSize(ulong p1);

    [PreserveSig]
    unsafe int CopyTo(IStream p1, ulong p2, ulong* p3, ulong* p4);

    [PreserveSig]
    int Commit(uint p1);

    [PreserveSig]
    int Revert();

    [PreserveSig]
    int LockRegion(ulong p1, ulong p2, uint p3);

    [PreserveSig]
    int UnlockRegion(ulong p1, ulong p2, uint p3);

    [PreserveSig]
    unsafe int Stat(STATSTG* p1, uint p2);

    [PreserveSig]
    unsafe int Clone(void** p1);
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct STATSTG
  {
    [FieldOffset(0)] public char* pwcsName;
    [FieldOffset(4)] public uint type;
    [FieldOffset(8)] public ulong cbSize;
    [FieldOffset(16)] public FILETIME mtime;
    [FieldOffset(24)] public FILETIME ctime;
    [FieldOffset(32)] public FILETIME atime;
    [FieldOffset(40)] public uint grfMode;
    [FieldOffset(44)] public uint grfLocksSupported;
    [FieldOffset(48)] public Guid clsid;
    [FieldOffset(64)] public uint grfStateBits;
    [FieldOffset(68)] public uint reserved;
  }

  [ComImport]
  [Guid("0000000b-0000-0000-c000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IStorage
  {
    [PreserveSig]
    unsafe int CreateStream([MarshalAs(UnmanagedType.LPWStr)] string p1, uint p2, uint p3, uint p4, void** p5);

    [PreserveSig]
    unsafe int OpenStream([MarshalAs(UnmanagedType.LPWStr)] string p1, void* p2, uint p3, uint p4, out IStream p5);

    [PreserveSig]
    unsafe int CreateStorage([MarshalAs(UnmanagedType.LPWStr)] string p1, uint p2, uint p3, uint p4, void** p5);

    [PreserveSig]
    unsafe int OpenStorage([MarshalAs(UnmanagedType.LPWStr)] string p1, IStorage p2, uint p3, char** p4, uint p5,
                           out IStorage p6);

    [PreserveSig]
    unsafe int CopyTo(uint p1, Guid* p2, char** p3, IStorage p4);

    [PreserveSig]
    int MoveElementTo([MarshalAs(UnmanagedType.LPWStr)] string p1, IStorage p2,
                      [MarshalAs(UnmanagedType.LPWStr)] string p3, uint p4);

    [PreserveSig]
    int Commit(uint p1);

    [PreserveSig]
    int Revert();

    [PreserveSig]
    unsafe int EnumElements(uint p1, void* p2, uint p3, out IEnumSTATSTG enm);

    [PreserveSig]
    int DestroyElement([MarshalAs(UnmanagedType.LPWStr)] string p1);

    [PreserveSig]
    int RenameElement([MarshalAs(UnmanagedType.LPWStr)] string p1, [MarshalAs(UnmanagedType.LPWStr)] string p2);

    [PreserveSig]
    unsafe int SetElementTimes([MarshalAs(UnmanagedType.LPWStr)] string p1, FILETIME* p2, FILETIME* p3, FILETIME* p4);

    [PreserveSig]
    unsafe int SetClass(Guid* p1);

    [PreserveSig]
    int SetStateBits(uint p1, uint p2);

    [PreserveSig]
    unsafe int Stat(STATSTG* p1, uint p2);
  }

  [ComImport]
  [Guid("0000000d-0000-0000-c000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IEnumSTATSTG
  {
    [PreserveSig]
    unsafe int Next(uint p1, STATSTG* p2, uint* p3);

    [PreserveSig]
    int Skip(uint p1);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    unsafe int Clone(void** p1);
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct tagVersionedStream
  {
    [FieldOffset(0)] public Guid guidVersion;
    [FieldOffset(16)] public void* pStream;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct SAFEARRAY
  {
    [FieldOffset(0)] public ushort cDims;
    [FieldOffset(2)] public ushort fFeatures;
    [FieldOffset(4)] public uint cbElements;
    [FieldOffset(8)] public uint cLocks;
    [FieldOffset(12)] public void* pvData;
    [FieldOffset(16)] public SAFEARRAYBOUND_Buffer_1 rgsabound;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct SAFEARRAYBOUND
  {
    [FieldOffset(0)] public uint cElements;
    [FieldOffset(4)] public int lLbound;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAC
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public sbyte* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAUB
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public byte* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAI
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public short* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAUI
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public ushort* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAL
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public int* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAUL
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public uint* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAH
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public long* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAUH
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public ulong* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAFLT
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public float* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CADBL
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public double* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CABOOL
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public short* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CASCODE
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public int* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CACY
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public CY* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CADATE
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public double* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAFILETIME
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public FILETIME* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CACLSID
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public Guid* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CACLIPDATA
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public CLIPDATA* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CABSTR
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public char** pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CABSTRBLOB
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public BSTRBLOB* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CALPSTR
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public sbyte** pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CALPWSTR
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public char** pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct CAPROPVARIANT
  {
    [FieldOffset(0)] public uint cElems;
    [FieldOffset(4)] public PROPVARIANT* pElems;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct tagDEC
  {
    [FieldOffset(0)] public ushort wReserved;
    [FieldOffset(0)] public LODECTYPE __unnamedLO;
    [FieldOffset(4)] public uint Hi32;
    [FieldOffset(4)] public HIDECTYPE __unnamedHI;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct LODECTYPE
  {
    [FieldOffset(0)] public SCALESIGN __unnamed;
    [FieldOffset(0)] public ushort signscale;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct SCALESIGN
  {
    [FieldOffset(0)] public byte scale;
    [FieldOffset(1)] public byte sign;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct HIDECTYPE
  {
    [FieldOffset(0)] public LOMIDTYPE __unnamed;
    [FieldOffset(0)] public ulong Lo64;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct LOMIDTYPE
  {
    [FieldOffset(0)] public uint Lo32;
    [FieldOffset(4)] public uint Mid32;
  }

  [ComImport]
  [Guid("00000139-0000-0000-c000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IEnumSTATPROPSTG
  {
    [PreserveSig]
    unsafe int Next(uint p1, STATPROPSTG* p2, uint* p3);

    [PreserveSig]
    int Skip(uint p1);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    unsafe int Clone(void** p1);
  }

  [StructLayout(LayoutKind.Explicit)]
  internal unsafe struct STATPROPSTG
  {
    [FieldOffset(0)] public char* lpwstrName;
    [FieldOffset(4)] public uint propid;
    [FieldOffset(8)] public ushort vt;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct STATPROPSETSTG
  {
    [FieldOffset(0)] public Guid fmtid;
    [FieldOffset(16)] public Guid clsid;
    [FieldOffset(32)] public uint grfFlags;
    [FieldOffset(36)] public FILETIME mtime;
    [FieldOffset(44)] public FILETIME ctime;
    [FieldOffset(52)] public FILETIME atime;
    [FieldOffset(60)] public uint dwOSVersion;
  }

  internal unsafe struct SAFEARRAYBOUND_Buffer_1
  {
    public SAFEARRAYBOUND firstElement;

    public SAFEARRAYBOUND this[int index]
    {
      get
      {
        fixed (SAFEARRAYBOUND* f = &firstElement)
        {
          return f[index];
        }
      }
      set
      {
        fixed (SAFEARRAYBOUND* f = &firstElement)
        {
          f[index] = value;
        }
      }
    }
  }
}