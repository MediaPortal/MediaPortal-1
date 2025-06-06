﻿#region Copyright (C) 2005-2011 Team MediaPortal

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

#endregion

// Sorce code origin http://www.codeproject.com/KB/IP/networkshares.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using  System.DirectoryServices;
using System.Security;

namespace MediaPortal.Util
{
  #region Share Type

  /// <summary>
  /// Type of share
  /// </summary>
  [Flags]
  public enum ShareType
  {
    /// <summary>Disk share</summary>
    Disk = 0,
    /// <summary>Printer share</summary>
    Printer = 1,
    /// <summary>Device share</summary>
    Device = 2,
    /// <summary>IPC share</summary>
    IPC = 3,
    /// <summary>Special share</summary>
    Special = -2147483648, // 0x80000000,
  }

  #endregion

  #region Share

  /// <summary>
  /// Information about a local share
  /// </summary>
  public class NetShare
  {
    #region Private data

    private string _server;
    private string _netName;
    private string _path;
    private ShareType _shareType;
    private string _remark;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Server"></param>
    /// <param name="shi"></param>
    public NetShare(string server, string netName, string path, ShareType shareType, string remark)
    {
      if (ShareType.Special == shareType && "IPC$" == netName)
      {
        shareType |= ShareType.IPC;
      }

      _server = server;
      _netName = netName;
      _path = path;
      _shareType = shareType;
      _remark = remark;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The name of the computer that this share belongs to
    /// </summary>
    public string Server
    {
      get { return _server; }
    }

    /// <summary>
    /// Share name
    /// </summary>
    public string NetName
    {
      get { return _netName; }
    }

    /// <summary>
    /// Local path
    /// </summary>
    public string Path
    {
      get { return _path; }
    }

    /// <summary>
    /// Share type
    /// </summary>
    public ShareType ShareType
    {
      get { return _shareType; }
    }

    /// <summary>
    /// Comment
    /// </summary>
    public string Remark
    {
      get { return _remark; }
    }

    /// <summary>
    /// Returns true if this is a file system share
    /// </summary>
    public bool IsFileSystem
    {
      get
      {
        // Shared device
        if (0 != (_shareType & ShareType.Device)) return false;
        // IPC share
        if (0 != (_shareType & ShareType.IPC)) return false;
        // Shared printer
        if (0 != (_shareType & ShareType.Printer)) return false;

        // Standard disk share
        if (0 == (_shareType & ShareType.Special)) return true;

        // Special disk share (e.g. C$)
        if (ShareType.Special == _shareType && null != _netName && 0 != _netName.Length)
          return true;
        else
          return false;
      }
    }

    /// <summary>
    /// Get the root of a disk-based share
    /// </summary>
    public DirectoryInfo Root
    {
      get
      {
        if (IsFileSystem)
        {
          if (null == _server || 0 == _server.Length)
            if (null == _path || 0 == _path.Length)
              return new DirectoryInfo(ToString());
            else
              return new DirectoryInfo(_path);
          else
            return new DirectoryInfo(ToString());
        }
        else
          return null;
      }
    }

    #endregion

    /// <summary>
    /// Returns the path to this share
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (null == _server || 0 == _server.Length)
      {
        return string.Format(@"\\{0}\{1}", Environment.MachineName, _netName);
      }
      else
        return string.Format(@"\\{0}\{1}", _server, _netName);
    }

    /// <summary>
    /// Returns true if this share matches the local path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool MatchesPath(string path)
    {
      if (!IsFileSystem) return false;
      if (null == path || 0 == path.Length) return true;

      return path.ToLowerInvariant().StartsWith(_path.ToLowerInvariant());
    }
  }

  #endregion

  #region Share Collection

  /// <summary>
  /// A collection of shares
  /// </summary>
  public class NetShareCollection : ReadOnlyCollectionBase
  {
    #region Platform

    /// <summary>
    /// Is this an NT platform?
    /// </summary>
    protected static bool IsNT
    {
      get { return (PlatformID.Win32NT == Environment.OSVersion.Platform); }
    }

    /// <summary>
    /// Returns true if this is Windows 2000 or higher
    /// </summary>
    protected static bool IsW2KUp
    {
      get
      {
        OperatingSystem os = Environment.OSVersion;
        if (PlatformID.Win32NT == os.Platform && os.Version.Major >= 5)
          return true;
        else
          return false;
      }
    }

    #endregion

    #region Interop

    #region Constants

    /// <summary>Maximum path length</summary>
    protected const int MAX_PATH = 260;
    /// <summary>No error</summary>
    protected const int NO_ERROR = 0;
    /// <summary>Access denied</summary>
    protected const int ERROR_ACCESS_DENIED = 5;
    /// <summary>Access denied</summary>
    protected const int ERROR_WRONG_LEVEL = 124;
    /// <summary>More data available</summary>
    protected const int ERROR_MORE_DATA = 234;
    /// <summary>Not connected</summary>
    protected const int ERROR_NOT_CONNECTED = 2250;
    /// <summary>Level 1</summary>
    protected const int UNIVERSAL_NAME_INFO_LEVEL = 1;
    /// <summary>Max extries (9x)</summary>
    protected const int MAX_SI50_ENTRIES = 20;

    #endregion

    #region Structures

    /// <summary>Unc name</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    protected struct UNIVERSAL_NAME_INFO
    {
      [MarshalAs(UnmanagedType.LPTStr)]
      public string lpUniversalName;
    }

    /// <summary>Share information, NT, level 2</summary>
    /// <remarks>
    /// Requires admin rights to work. 
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    protected struct SHARE_INFO_2
    {
      [MarshalAs(UnmanagedType.LPWStr)]
      public string NetName;
      public ShareType ShareType;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string Remark;
      public int Permissions;
      public int MaxUsers;
      public int CurrentUsers;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string Path;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string Password;
    }

    /// <summary>Share information, NT, level 1</summary>
    /// <remarks>
    /// Fallback when no admin rights.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    protected struct SHARE_INFO_1
    {
      [MarshalAs(UnmanagedType.LPWStr)]
      public string NetName;
      public ShareType ShareType;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string Remark;
    }

    /// <summary>Share information, Win9x</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    protected struct SHARE_INFO_50
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
      public string NetName;

      public byte bShareType;
      public ushort Flags;

      [MarshalAs(UnmanagedType.LPTStr)]
      public string Remark;
      [MarshalAs(UnmanagedType.LPTStr)]
      public string Path;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
      public string PasswordRW;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
      public string PasswordRO;

      public ShareType ShareType
      {
        get { return (ShareType)((int)bShareType & 0x7F); }
      }
    }

    /// <summary>Share information level 1, Win9x</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    protected struct SHARE_INFO_1_9x
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
      public string NetName;
      public byte Padding;

      public ushort bShareType;

      [MarshalAs(UnmanagedType.LPTStr)]
      public string Remark;

      public ShareType ShareType
      {
        get { return (ShareType)((int)bShareType & 0x7FFF); }
      }
    }

    #endregion

    #region Functions

    /// <summary>Get a UNC name</summary>
    [DllImport("mpr", CharSet = CharSet.Auto)]
    protected static extern int WNetGetUniversalName(string lpLocalPath,
      int dwInfoLevel, ref UNIVERSAL_NAME_INFO lpBuffer, ref int lpBufferSize);

    /// <summary>Get a UNC name</summary>
    [DllImport("mpr", CharSet = CharSet.Auto)]
    protected static extern int WNetGetUniversalName(string lpLocalPath,
      int dwInfoLevel, IntPtr lpBuffer, ref int lpBufferSize);

    /// <summary>Enumerate shares (NT)</summary>
    [DllImport("netapi32", CharSet = CharSet.Unicode)]
    protected static extern int NetShareEnum(string lpServerName, int dwLevel,
      out IntPtr lpBuffer, int dwPrefMaxLen, out int entriesRead,
      out int totalEntries, ref int hResume);

    /// <summary>Enumerate shares (9x)</summary>
    [DllImport("svrapi", CharSet = CharSet.Ansi)]
    protected static extern int NetShareEnum(
      [MarshalAs(UnmanagedType.LPTStr)] string lpServerName, int dwLevel,
      IntPtr lpBuffer, ushort cbBuffer, out ushort entriesRead,
      out ushort totalEntries);

    /// <summary>Free the buffer (NT)</summary>
    [DllImport("netapi32")]
    protected static extern int NetApiBufferFree(IntPtr lpBuffer);

    #endregion

    #region Enumerate shares

    /// <summary>
    /// Enumerates the shares on Windows NT
    /// </summary>
    /// <param name="server">The server name</param>
    /// <param name="netShares">The ShareCollection</param>
    protected static void EnumerateSharesNT(string server, NetShareCollection netShares)
    {
      int level = 2;
      int entriesRead, totalEntries, nRet, hResume = 0;
      IntPtr pBuffer = IntPtr.Zero;

      try
      {
        nRet = NetShareEnum(server, level, out pBuffer, -1,
          out entriesRead, out totalEntries, ref hResume);

        if (ERROR_ACCESS_DENIED == nRet)
        {
          //Need admin for level 2, drop to level 1
          level = 1;
          nRet = NetShareEnum(server, level, out pBuffer, -1,
            out entriesRead, out totalEntries, ref hResume);
        }

        if (NO_ERROR == nRet && entriesRead > 0)
        {
          int offset = (level == 2) ? Marshal.SizeOf(typeof(SHARE_INFO_2)) : Marshal.SizeOf(typeof(SHARE_INFO_1));
          var lpItem = pBuffer;
          for (int i = 0; i < entriesRead; i++, lpItem = IntPtr.Add(lpItem, offset))
          {
            if (1 == level)
            {
              SHARE_INFO_1 si = Marshal.PtrToStructure<SHARE_INFO_1>(lpItem);
              netShares.Add(si.NetName, string.Empty, si.ShareType, si.Remark);
            }
            else
            {
              SHARE_INFO_2 si = Marshal.PtrToStructure<SHARE_INFO_2>(lpItem);
              netShares.Add(si.NetName, si.Path, si.ShareType, si.Remark);
            }
          }
        }

      }
      finally
      {
        // Clean up buffer allocated by system
        if (IntPtr.Zero != pBuffer)
          NetApiBufferFree(pBuffer);
      }
    }

    /// <summary>
    /// Enumerates the shares on Windows 9x
    /// </summary>
    /// <param name="server">The server name</param>
    /// <param name="netShares">The ShareCollection</param>
    protected static void EnumerateShares9x(string server, NetShareCollection netShares)
    {
      int level = 50;
      int nRet = 0;
      ushort entriesRead, totalEntries;

      int size = Marshal.SizeOf(typeof(SHARE_INFO_50));
      ushort cbBuffer = (ushort)(MAX_SI50_ENTRIES * size);
      //On Win9x, must allocate buffer before calling API
      IntPtr pBuffer = Marshal.AllocHGlobal(cbBuffer);

      try
      {
        nRet = NetShareEnum(server, level, pBuffer, cbBuffer,
          out entriesRead, out totalEntries);

        if (ERROR_WRONG_LEVEL == nRet)
        {
          level = 1;
          size = Marshal.SizeOf(typeof(SHARE_INFO_1_9x));
          nRet = NetShareEnum(server, level, pBuffer, cbBuffer,
            out entriesRead, out totalEntries);
        }

        if (NO_ERROR == nRet || ERROR_MORE_DATA == nRet)
        {
          IntPtr pItem = pBuffer;
          for (int i = 0; i < entriesRead; i++, pItem = IntPtr.Add(pItem, size))
          {
            if (1 == level)
            {
              SHARE_INFO_1_9x si = Marshal.PtrToStructure<SHARE_INFO_1_9x>(pItem);
              netShares.Add(si.NetName, string.Empty, si.ShareType, si.Remark);
            }
            else
            {
              SHARE_INFO_50 si = Marshal.PtrToStructure<SHARE_INFO_50>(pItem);
              netShares.Add(si.NetName, si.Path, si.ShareType, si.Remark);
            }
          }
        }
        else
          Console.WriteLine(nRet);

      }
      finally
      {
        //Clean up buffer
        Marshal.FreeHGlobal(pBuffer);
      }
    }

    /// <summary>
    /// Enumerates the shares
    /// </summary>
    /// <param name="server">The server name</param>
    /// <param name="netShares">The ShareCollection</param>
    protected static void EnumerateShares(string server, NetShareCollection netShares)
    {
      if (null != server && 0 != server.Length && !IsW2KUp)
      {
        server = server.ToUpperInvariant();

        // On NT4, 9x and Me, server has to start with "\\"
        if (!('\\' == server[0] && '\\' == server[1]))
          server = @"\\" + server;
      }

      if (IsNT)
        EnumerateSharesNT(server, netShares);
      else
        EnumerateShares9x(server, netShares);
    }

    #endregion

    #endregion

    #region Static methods

    /// <summary>
    /// Returns true if fileName is a valid local file-name of the form:
    /// X:\, where X is a drive letter from A-Z
    /// </summary>
    /// <param name="fileName">The filename to check</param>
    /// <returns></returns>
    public static bool IsValidFilePath(string fileName)
    {
      if (null == fileName || 0 == fileName.Length) return false;

      char drive = char.ToUpper(fileName[0]);
      if ('A' > drive || drive > 'Z')
        return false;

      else if (Path.VolumeSeparatorChar != fileName[1])
        return false;
      else if (Path.DirectorySeparatorChar != fileName[2])
        return false;
      else
        return true;
    }

    /// <summary>
    /// Returns the UNC path for a mapped drive or local share.
    /// </summary>
    /// <param name="fileName">The path to map</param>
    /// <returns>The UNC path (if available)</returns>
    public static string PathToUnc(string fileName)
    {
      if (null == fileName || 0 == fileName.Length) return string.Empty;

      fileName = Path.GetFullPath(fileName);
      if (!IsValidFilePath(fileName)) return fileName;

      int nRet = 0;
      UNIVERSAL_NAME_INFO rni = new UNIVERSAL_NAME_INFO();
      int bufferSize = Marshal.SizeOf(rni);

      nRet = WNetGetUniversalName(
        fileName, UNIVERSAL_NAME_INFO_LEVEL,
        ref rni, ref bufferSize);

      if (ERROR_MORE_DATA == nRet)
      {
        IntPtr pBuffer = Marshal.AllocHGlobal(bufferSize); ;
        try
        {
          nRet = WNetGetUniversalName(
            fileName, UNIVERSAL_NAME_INFO_LEVEL,
            pBuffer, ref bufferSize);

          if (NO_ERROR == nRet)
          {
            rni = Marshal.PtrToStructure<UNIVERSAL_NAME_INFO>(pBuffer);
          }
        }
        finally
        {
          Marshal.FreeHGlobal(pBuffer);
        }
      }

      switch (nRet)
      {
        case NO_ERROR:
          return rni.lpUniversalName;

        case ERROR_NOT_CONNECTED:
          //Local file-name
          NetShareCollection shi = LocalNetShares;
          if (null != shi)
          {
            NetShare share = shi[fileName];
            if (null != share)
            {
              string path = share.Path;
              if (null != path && 0 != path.Length)
              {
                int index = path.Length;
                if (Path.DirectorySeparatorChar != path[path.Length - 1])
                  index++;

                if (index < fileName.Length)
                  fileName = fileName.Substring(index);
                else
                  fileName = string.Empty;

                fileName = Path.Combine(share.ToString(), fileName);
              }
            }
          }

          return fileName;

        default:
          Console.WriteLine("Unknown return value: {0}", nRet);
          return string.Empty;
      }
    }

    /// <summary>
    /// Returns the local <see cref="Share"/> object with the best match
    /// to the specified path.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static NetShare PathToShare(string fileName)
    {
      if (null == fileName || 0 == fileName.Length) return null;

      fileName = Path.GetFullPath(fileName);
      if (!IsValidFilePath(fileName)) return null;

      NetShareCollection shi = LocalNetShares;
      if (null == shi)
        return null;
      else
        return shi[fileName];
    }

    #endregion

    #region Local shares

    /// <summary>The local shares</summary>
    private static NetShareCollection _local = null;

    /// <summary>
    /// Return the local shares
    /// </summary>
    public static NetShareCollection LocalNetShares
    {
      get
      {
        if (null == _local)
          _local = new NetShareCollection();

        return _local;
      }
    }

    /// <summary>
    /// Return the shares for a specified machine
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public static NetShareCollection GetShares(string server)
    {
      return new NetShareCollection(server);
    }

    #endregion

    #region Private Data

    /// <summary>The name of the server this collection represents</summary>
    private string _server;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor - local machine
    /// </summary>
    public NetShareCollection()
    {
      _server = string.Empty;
      EnumerateShares(_server, this);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Server"></param>
    public NetShareCollection(string server)
    {
      _server = server;
      EnumerateShares(_server, this);
    }

    #endregion

    #region Add

    protected void Add(NetShare share)
    {
      InnerList.Add(share);
    }

    protected void Add(string netName, string path, ShareType shareType, string remark)
    {
      InnerList.Add(new NetShare(_server, netName, path, shareType, remark));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns the name of the server this collection represents
    /// </summary>
    public string Server
    {
      get { return _server; }
    }

    /// <summary>
    /// Returns the <see cref="Share"/> at the specified index.
    /// </summary>
    public NetShare this[int index]
    {
      get { return (NetShare)InnerList[index]; }
    }

    /// <summary>
    /// Returns the <see cref="Share"/> which matches a given local path
    /// </summary>
    /// <param name="path">The path to match</param>
    public NetShare this[string path]
    {
      get
      {
        if (null == path || 0 == path.Length) return null;

        path = Path.GetFullPath(path);
        if (!IsValidFilePath(path)) return null;

        NetShare match = null;

        for (int i = 0; i < InnerList.Count; i++)
        {
          NetShare s = (NetShare)InnerList[i];

          if (s.IsFileSystem && s.MatchesPath(path))
          {
            //Store first match
            if (null == match)
              match = s;

            // If this has a longer path,
            // and this is a disk share or match is a special share, 
            // then this is a better match
            else if (match.Path.Length < s.Path.Length)
            {
              if (ShareType.Disk == s.ShareType || ShareType.Disk != match.ShareType)
                match = s;
            }
          }
        }

        return match;
      }
    }

    #endregion

    #region Implementation of ICollection

    /// <summary>
    /// Copy this collection to an array
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    public void CopyTo(NetShare[] array, int index)
    {
      InnerList.CopyTo(array, index);
    }

    #endregion

    public static ArrayList GetComputersOnNetwork()
    {
      ArrayList list = new ArrayList();
      using (DirectoryEntry root = new DirectoryEntry("WinNT:"))
      {
        foreach (DirectoryEntry computers in root.Children)
        {
          foreach (DirectoryEntry computer in computers.Children)
          {
            if ((computer.Name != "Schema"))
            {
              list.Add(@"\\" + computer.Name);
            }
          }
        }
      }
      return list;
    }
  }
  #endregion
}