#region Copyright (c) MediaArea.net SARL.
/*  Copyright (c) MediaArea.net SARL. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license that can
 *  be found in the License.html file in the root of the source tree.
 */

//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
// Microsoft Visual C# wrapper for MediaInfo Library
// See MediaInfo.h for help
//
// To make it working, you must put MediaInfo.Dll
// in the executable folder
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion

using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591 // Disable XML documentation warnings

namespace MediaInfoWrapper
{
  public enum StreamKind
  {
    General,
    Video,
    Audio,
    Text,
    Other,
    Image,
    Menu,
  }

  public enum InfoKind
  {
    Name,
    Text,
    Measure,
    Options,
    NameText,
    MeasureText,
    Info,
    HowTo
  }

  public enum InfoOptions
  {
    ShowInInform,
    Support,
    ShowInSupported,
    TypeOfValue
  }

  public enum InfoFileOptions
  {
    FileOption_Nothing      = 0x00,
    FileOption_NoRecursive  = 0x01,
    FileOption_CloseAll     = 0x02,
    FileOption_Max          = 0x04
  };

  public enum Status
  {
    None        =       0x00,
    Accepted    =       0x01,
    Filled      =       0x02,
    Updated     =       0x04,
    Finalized   =       0x08,
  }

  public class MediaInfo : IDisposable
  {
    //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_New();
    [DllImport("MediaInfo.dll")]
    private static extern void   MediaInfo_Delete(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open(IntPtr Handle, IntPtr FileName);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open_Buffer_Init(IntPtr Handle, Int64 File_Size, Int64 File_Offset);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open(IntPtr Handle, Int64 File_Size, Int64 File_Offset);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open_Buffer_Continue(IntPtr Handle, IntPtr Buffer, IntPtr Buffer_Size);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open_Buffer_Continue(IntPtr Handle, Int64 File_Size, byte[] Buffer, IntPtr Buffer_Size);
    [DllImport("MediaInfo.dll")]
    private static extern Int64  MediaInfo_Open_Buffer_Continue_GoTo_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern Int64  MediaInfoA_Open_Buffer_Continue_GoTo_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open_Buffer_Finalize(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open_Buffer_Finalize(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern void   MediaInfo_Close(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Inform(IntPtr Handle, IntPtr Reserved);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Option(IntPtr Handle, IntPtr Option,  IntPtr Value);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_State_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);

    private IntPtr handle;
    private readonly bool mustUseAnsi;


    //MediaInfo class
    public MediaInfo()
    {
      try
      {
        handle = MediaInfo_New();
      }
      catch
      {
        handle = IntPtr.Zero;
      }

      mustUseAnsi = Environment.OSVersion.ToString().IndexOf("Windows") == -1;
    }

    ~MediaInfo()
    {
      Dispose(false);
    }

    public IntPtr Open(string fileName)
    {
      if (handle == IntPtr.Zero) return IntPtr.Zero;
      if (mustUseAnsi)
      {
        IntPtr result;
        var fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
        try
        {
          result = MediaInfoA_Open(handle, fileNamePtr);
        }
        finally
        {
          Marshal.FreeHGlobal(fileNamePtr);
        }

        return result;
      }
      
      return MediaInfo_Open(handle, fileName);
    }

    public IntPtr Open_Buffer_Init(Int64 fileSize, Int64 fileOffset)
    {
      return handle == IntPtr.Zero ? IntPtr.Zero : MediaInfo_Open_Buffer_Init(handle, fileSize, fileOffset);
    }

    public IntPtr Open_Buffer_Continue(IntPtr buffer, IntPtr bufferSize)
    {
      return handle == IntPtr.Zero ? IntPtr.Zero : MediaInfo_Open_Buffer_Continue(handle, buffer, bufferSize);
    }

    public Int64 Open_Buffer_Continue_GoTo_Get()
    {
      return handle == IntPtr.Zero ? 0 : MediaInfo_Open_Buffer_Continue_GoTo_Get(handle);
    }

    public IntPtr Open_Buffer_Finalize()
    {
      return handle == IntPtr.Zero ? IntPtr.Zero : MediaInfo_Open_Buffer_Finalize(handle);
    }

    public void Close()
    {
      if (handle != IntPtr.Zero)
      {
        MediaInfo_Delete(handle);
        handle = IntPtr.Zero;
      }
    }

    public string Inform()
    {
      if (handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      return mustUseAnsi ? Marshal.PtrToStringAnsi(MediaInfoA_Inform(handle, (IntPtr)0)) : Marshal.PtrToStringUni(MediaInfo_Inform(handle, (IntPtr)0));
    }

    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
    {
      if (handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      if (mustUseAnsi)
      {
        string result;
        var parameterPtr = Marshal.StringToHGlobalAnsi(parameter);
        try
        {
          result = Marshal.PtrToStringAnsi(MediaInfoA_Get(handle, (IntPtr)streamKind, (IntPtr)streamNumber, parameterPtr, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
        }
        finally 
        {
          Marshal.FreeHGlobal(parameterPtr);
        }

        return result;
      }
      
      return Marshal.PtrToStringUni(MediaInfo_Get(handle, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
    }

    public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
    {
      if (handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      return mustUseAnsi ? 
        Marshal.PtrToStringAnsi(MediaInfoA_GetI(handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo)) : 
        Marshal.PtrToStringUni(MediaInfo_GetI(handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
    }

    public string Option(string option, string value)
    {
      if (handle == IntPtr.Zero) return "Unable to load MediaInfo library";
      if (mustUseAnsi)
      {
        var optionPtr = IntPtr.Zero;
        var valuePtr = IntPtr.Zero;
        string result;
        try
        {
          optionPtr = Marshal.StringToHGlobalAnsi(option);
          valuePtr = Marshal.StringToHGlobalAnsi(value);
          result = Marshal.PtrToStringAnsi(MediaInfoA_Option(handle, optionPtr, valuePtr));
        }
        finally
        {
          Marshal.FreeHGlobal(optionPtr);
          Marshal.FreeHGlobal(valuePtr);
        }

        return result;
      }
      
      return Marshal.PtrToStringUni(MediaInfo_Option(handle, option, value));
    }

    public IntPtr State_Get()
    {
      return handle == IntPtr.Zero ? IntPtr.Zero : MediaInfo_State_Get(handle);
    }

    public int Count_Get(StreamKind streamKind, int streamNumber)
    {
      return handle == IntPtr.Zero ? 0 : (int)MediaInfo_Count_Get(handle, (IntPtr)streamKind, (IntPtr)streamNumber);
    }

    //Default values, if you know how to set default values in C#, say me
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo)
    {
      return Get(streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);
    }

    public string Get(StreamKind streamKind, int streamNumber, string parameter)
    {
      return Get(streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);
    }

    public string Get(StreamKind streamKind, int streamNumber, int parameter)
    {
      return Get(streamKind, streamNumber, parameter, InfoKind.Text);
    }

    public string Option(string option)
    {
      return Option(option, "");
    }

    public int Count_Get(StreamKind streamKind)
    {
      return Count_Get(streamKind, -1);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      Close();
    }
  }

  public class MediaInfoList
  {
    //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_New();
    [DllImport("MediaInfo.dll")]
    private static extern void MediaInfoList_Delete(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName, IntPtr Options);
    [DllImport("MediaInfo.dll")]
    private static extern void MediaInfoList_Close(IntPtr Handle, IntPtr FilePos);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_Inform(IntPtr Handle, IntPtr FilePos, IntPtr Reserved);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_GetI(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_Get(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_State_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoList_Count_Get(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind, IntPtr StreamNumber);

    private readonly IntPtr handle;

    //MediaInfo class
    public MediaInfoList()
    {
      handle = MediaInfoList_New();
    }

    ~MediaInfoList()
    {
      MediaInfoList_Delete(handle);
    }

    public int Open(string fileName, InfoFileOptions options)
    {
      return (int)MediaInfoList_Open(handle, fileName, (IntPtr)options);
    }

    public void Close(int filePos)
    {
      MediaInfoList_Close(handle, (IntPtr)filePos);
    }

    public string Inform(int filePos)
    {
      return Marshal.PtrToStringUni(MediaInfoList_Inform(handle, (IntPtr)filePos, (IntPtr)0));
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
    {
      return Marshal.PtrToStringUni(MediaInfoList_Get(handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
    {
      return Marshal.PtrToStringUni(MediaInfoList_GetI(handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
    }

    public string Option(string option, string value)
    {
      return Marshal.PtrToStringUni(MediaInfoList_Option(handle, option, value));
    }

    public int State_Get()
    {
      return (int)MediaInfoList_State_Get(handle);
    }

    public int Count_Get(int filePos, StreamKind streamKind, int streamNumber)
    {
      return (int)MediaInfoList_Count_Get(handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber);
    }

    //Default values, if you know how to set default values in C#, say me
    public void Open(string fileName)
    {
      Open(fileName, 0);
    }

    public void Close()
    {
      Close(-1);
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo)
    {
      return Get(filePos, streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter)
    {
      return Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter)
    {
      return Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text);
    }

    public string Option(string option)
    {
      return Option(option, "");
    }

    public int Count_Get(int filePos, StreamKind streamKind)
    {
      return Count_Get(filePos, streamKind, -1);
    }
  }

} //NameSpace