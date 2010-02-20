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
using System.Security;
using System.Text;

namespace MediaPortal.Configuration
{
  public class Reg
  {
    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx", SetLastError = true)]
    public static extern int RegOpenKeyEx(
      UIntPtr hKey,
      string subKey,
      uint options,
      int sam,
      out UIntPtr phkResult);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
    public static extern int RegQueryValueEx(
      UIntPtr hKey,
      string lpValueName,
      int lpReserved,
      out uint lpType,
      StringBuilder lpData,
      ref uint lpcbData);

    #region RegistyrFlags

    [Flags]
    public enum RegWow64Options
    {
      None = 0,
      KEY_WOW64_64KEY = 0x100,
      KEY_WOW64_32KEY = 0x200
    }

    [Flags]
    public enum RegistryRights
    {
      ReadKey = 131097,
      WriteKey = 131078
    }

    [Flags]
    public enum RegistryRoot : uint
    {
      HKCU = 0x80000001,
      HKLM = 0x80000002
    }

    #endregion
  }
}