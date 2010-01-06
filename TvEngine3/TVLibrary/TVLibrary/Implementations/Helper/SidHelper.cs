/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;

namespace TvLibrary.Helper
{
  /// <summary>
  /// SID Helper methods
  /// </summary>
  public class SidHelper
  {
    #region imports

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool LookupAccountSid(
      [In, MarshalAs(UnmanagedType.LPTStr)] string systemName,
      IntPtr sid,
      [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder name,
      ref int cbName,
      StringBuilder referencedDomainName,
      ref int cbReferencedDomainName,
      out int use);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool LookupAccountName(
      [In, MarshalAs(UnmanagedType.LPTStr)] string systemName,
      [In, MarshalAs(UnmanagedType.LPTStr)] string accountName,
      IntPtr sid,
      ref int cbSid,
      StringBuilder referencedDomainName,
      ref int cbReferencedDomainName,
      out int use);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool ConvertSidToStringSid(
      IntPtr sid,
      [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool ConvertStringSidToSid(
      [In, MarshalAs(UnmanagedType.LPTStr)] string pStringSid,
      ref IntPtr sid);

    #endregion

    ///<summary>
    /// Gets the SID pointer
    ///</summary>
    ///<param name="name">Name</param>
    ///<returns>Pointer to the SID</returns>
    public static IntPtr GetSidPtr(string name)
    {
      IntPtr _sid = IntPtr.Zero; //pointer to binary form of SID string.
      int _sidLength = 0; //size of SID buffer.
      int _domainLength = 0; //size of domain name buffer.
      int _use; //type of object.
      //stringBuilder for domain name.
      StringBuilder _domain = new StringBuilder();

      //first call of the function only returns the size 
      //of buffers (SID, domain name)
      LookupAccountName(null, name, _sid, ref _sidLength, _domain,
                        ref _domainLength, out _use);
      int _error = Marshal.GetLastWin32Error();

      //error 122 (The data area passed to a system call is too small) 
      // normal behaviour.
      if (_error != 122)
      {
        return IntPtr.Zero;
      }
      //allocates memory for domain name
      _domain = new StringBuilder(_domainLength);
      //allocates memory for SID
      _sid = Marshal.AllocHGlobal(_sidLength);
      bool _rc = LookupAccountName(null, name, _sid, ref _sidLength, _domain,
                                   ref _domainLength, out _use);

      if (_rc == false)
      {
        Marshal.GetLastWin32Error();
        Marshal.FreeHGlobal(_sid);
        return IntPtr.Zero;
      }
      return _sid;
    }

    /// <summary>
    /// Gets the SID
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>SID</returns>
    public static string GetSid(string name)
    {
      IntPtr _sid = IntPtr.Zero; //pointer to binary form of SID string.
      int _sidLength = 0; //size of SID buffer.
      int _domainLength = 0; //size of domain name buffer.
      int _use; //type of object.
      //stringBuilder for domain name.
      StringBuilder _domain = new StringBuilder();
      string _sidString = "";

      //first call of the function only returns the size 
      //of buffers (SID, domain name)
      LookupAccountName(null, name, _sid, ref _sidLength, _domain,
                        ref _domainLength, out _use);
      int _error = Marshal.GetLastWin32Error();

      //error 122 (The data area passed to a system call is too small) 
      // normal behaviour.
      if (_error != 122)
      {
        throw (new Exception(new Win32Exception(_error).Message));
      }
      //allocates memory for domain name
      _domain = new StringBuilder(_domainLength);
      //allocates memory for SID
      _sid = Marshal.AllocHGlobal(_sidLength);
      bool _rc = LookupAccountName(null, name, _sid, ref _sidLength, _domain,
                                   ref _domainLength, out _use);

      if (_rc == false)
      {
        _error = Marshal.GetLastWin32Error();
        Marshal.FreeHGlobal(_sid);
        throw (new Exception(new Win32Exception(_error).Message));
      }
      // converts binary SID into string
      _rc = ConvertSidToStringSid(_sid, ref _sidString);

      if (_rc == false)
      {
        _error = Marshal.GetLastWin32Error();
        Marshal.FreeHGlobal(_sid);
        throw (new Exception(new Win32Exception(_error).Message));
      }
      Marshal.FreeHGlobal(_sid);
      return _sidString;
    }

    /// <summary>
    /// Gets the name to the given sid
    /// </summary>
    /// <param name="sid">The SID</param>
    /// <returns>Name to the SID</returns>
    public static string GetName(string sid)
    {
      IntPtr _sid = IntPtr.Zero; //pointer to binary form of SID string.
      int _nameLength = 0; //size of object name buffer
      int _domainLength = 0; //size of domain name buffer
      int _use; //type of object
      StringBuilder _domain = new StringBuilder(); //domain name variable
      StringBuilder _name = new StringBuilder(); //object name variable

      //converts SID string into the binary form
      bool _rc0 = ConvertStringSidToSid(sid, ref _sid);

      if (_rc0 == false)
      {
        Marshal.GetLastWin32Error();
        Marshal.FreeHGlobal(_sid);
        return String.Empty;
      }

      //first call of method returns the size of domain name 
      //and object name buffers
      LookupAccountSid(null, _sid, _name, ref _nameLength, _domain,
                       ref _domainLength, out _use);
      _domain = new StringBuilder(_domainLength); //allocates memory for domain name
      _name = new StringBuilder(_nameLength); //allocates memory for object name
      bool _rc = LookupAccountSid(null, _sid, _name, ref _nameLength, _domain,
                                  ref _domainLength, out _use);

      if (_rc == false)
      {
        Marshal.GetLastWin32Error();
        Marshal.FreeHGlobal(_sid);
        return String.Empty;
      }
      Marshal.FreeHGlobal(_sid);
      return _domain + "\\" + _name;
    }
  }
}