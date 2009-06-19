#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;

namespace SetRights
{
  class Program
  {
    static int Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Usage:\n");
        Console.WriteLine("      SetRights <Type> <DirPath>\n");
        Console.WriteLine("Samples:");
        Console.WriteLine("      SetRights FOLDER C:\\Temp");
        Console.WriteLine("      SetRights HKLM \"Software\\Team MediaPortal\"");
        Console.WriteLine("      SetRights HKCU \"Software\\Team MediaPortal\"");
        return 1;
      }
      switch (args[0].ToLowerInvariant())
      {
        case "folder": GrantFullControlFolder(args[1]); break;
        case "hklm": GrantFullControlRegKeyLM(args[1]); break;
        case "hkcu": GrantFullControlRegKeyUser(args[1]); break;
      }
      return 0;
    }

    /// <summary>
    /// Grants full control on the given file for everyone
    /// </summary>
    /// <param name="folderName">FolderName</param>
    public static void GrantFullControlFolder(string folderName)
    {
      if (!System.IO.Directory.Exists(folderName))
      {
        Console.WriteLine("Directory not found: {0}", folderName);
        return;
      }

      Console.WriteLine("Granting full control to: {0}", folderName);
      try
      {
        // Create a SecurityIdentifier object for "everyone".
        SecurityIdentifier everyoneSid =
            new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        DirectorySecurity security = System.IO.Directory.GetAccessControl(folderName);
        FileSystemAccessRule newRule =
          new FileSystemAccessRule(
            everyoneSid,
            FileSystemRights.FullControl,                                       // full control so no arm if new files are created
            InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, // all subfolders and files
            PropagationFlags.InheritOnly,
            AccessControlType.Allow);
        security.AddAccessRule(newRule);
        System.IO.Directory.SetAccessControl(folderName, security);

        Console.WriteLine("Success");
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error while setting full write access to everyone for file: {0} : {1}", folderName, ex);
      }
    }
    /// <summary>
    /// Grants full control on the given registry key under HKCU for everyone
    /// </summary>
    /// <param name="subKey">subkey name</param>
    public static void GrantFullControlRegKeyUser(string subKey)
    {
      GrantFullControlRegKey(Registry.CurrentUser, subKey);
    }
    /// <summary>
    /// Grants full control on the given registry key under HKLM for everyone
    /// </summary>
    /// <param name="subKey">subkey name</param>
    public static void GrantFullControlRegKeyLM(string subKey)
    {
      GrantFullControlRegKey(Registry.LocalMachine, subKey);
    }
    /// <summary>
    /// Grants full control on the given registry key under given root name for everyone
    /// </summary>
    /// <param name="subKey">subkey name</param>
    public static void GrantFullControlRegKey(string keyShortName, string subKey)
    {
      switch (keyShortName)
      {
        case "HKCU": GrantFullControlRegKeyUser(subKey); break;
        case "HKLM": GrantFullControlRegKeyLM(subKey); break;
        default:
          throw new Exception("Invalid registry base key");
      }
    }
    /// <summary>
    /// Grants full control on the given registry key under RegistryKey for everyone
    /// </summary>
    /// <param name="subKey">subkey name</param>
    public static void GrantFullControlRegKey(RegistryKey baseKey, string subKey)
    {
      RegistryKey rKey = baseKey.OpenSubKey(subKey, true);

      if (rKey == null) return;
      Console.WriteLine("Granting full control to: {0}\\{1}", baseKey, subKey);
      try
      {
        // Create a SecurityIdentifier object for "everyone".
        SecurityIdentifier everyoneSid =
            new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        RegistrySecurity security = rKey.GetAccessControl();
        RegistryAccessRule newRule =
          new RegistryAccessRule(
            everyoneSid,
            RegistryRights.FullControl,  // modify is enough for reading/writing/deleting
            InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, // all subfolders and files
            PropagationFlags.InheritOnly,
            AccessControlType.Allow);
        security.AddAccessRule(newRule);
        rKey.SetAccessControl(security);
        rKey.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error while setting full write access to everyone for file: {0} : {1}", subKey, ex);
      }
    }
  }
}
