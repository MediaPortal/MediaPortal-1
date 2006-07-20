using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.TsFileSink
{

  [ComImport, Guid("5CDD5C68-80DC-43E1-9E44-C849CA8026E7")]
  public class TsFileSink { };

  [ComVisible(true), ComImport,
  Guid("0d2620cd-a57a-4458-b96f-76442b70e9c7"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsFileSink
  {
    [PreserveSig]
    int GetBufferSize(ref int size);
    [PreserveSig]
    int SetRegSettings();
    [PreserveSig]
    int GetRegSettings();
    [PreserveSig]
    int GetRegFileName([In, Out, MarshalAs(UnmanagedType.AnsiBStr)] ref string fileName);
    [PreserveSig]
    int SetRegFileName([In, MarshalAs(UnmanagedType.AnsiBStr)] ref string fileName);
    [PreserveSig]
    int GetBufferFileName([In, Out, MarshalAs(UnmanagedType.LPWStr)] ref StringBuilder fileName);
    [PreserveSig]
    int SetBufferFileName([In, MarshalAs(UnmanagedType.AnsiBStr)] string fileName);
    //	int GetCurrentTSFile(FileWriter* fileWriter) ;
    [PreserveSig]
    int GetNumbFilesAdded(ref ushort numbAdd);
    [PreserveSig]
    int GetNumbFilesRemoved(ref ushort numbRem);
    [PreserveSig]
    int GetCurrentFileId(ref ushort fileID);
    [PreserveSig]
    int GetMinTSFiles(ref ushort minFiles);
    [PreserveSig]
    int SetMinTSFiles(ushort minFiles);
    [PreserveSig]
    int GetMaxTSFiles(ref ushort maxFiles);
    [PreserveSig]
    int SetMaxTSFiles(ushort maxFiles);
    [PreserveSig]
    int GetMaxTSFileSize(ref long maxSize);
    [PreserveSig]
    int SetMaxTSFileSize(long maxSize);
    [PreserveSig]
    int GetChunkReserve(ref long chunkSize);
    [PreserveSig]
    int SetChunkReserve(long chunkSize);
    [PreserveSig]
    int GetFileBufferSize(ref long lpllsize);
  }
}
