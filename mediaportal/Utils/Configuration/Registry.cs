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