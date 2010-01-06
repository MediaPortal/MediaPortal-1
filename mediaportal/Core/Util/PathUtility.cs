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
using System.IO;
using System.Security.Permissions;
using System.Security.Principal;
using System.Security.AccessControl;

namespace MediaPortal.Util
{
  public static class PathUtility
  {
    private const int defaultBufferSize = 0x1000; // 4KB

    #region GetSecureDeleteOnCloseTempFileStream

    /// <summary>
    /// Creates a unique, randomly named, secure, zero-byte temporary file on disk, which is automatically deleted when it is no longer in use. Returns the opened file stream.
    /// </summary>
    /// <remarks>
    /// <para>The generated file name is a cryptographically strong, random string. The file name is guaranteed to be unique to the system's temporary folder.</para>
    /// <para>The <see cref="GetSecureDeleteOnCloseTempFileStream"/> method will raise an <see cref="IOException"/> if no unique temporary file name is available. Although this is possible, it is highly improbable. To resolve this error, delete all uneeded temporary files.</para>
    /// <para>The file is created as a zero-byte file in the system's temporary folder.</para>
    /// <para>The file owner is set to the current user. The file security permissions grant full control to the current user only.</para>
    /// <para>The file sharing is set to none.</para>
    /// <para>The file is marked as a temporary file. File systems avoid writing data back to mass storage if sufficient cache memory is available, because an application deletes a temporary file after a handle is closed. In that case, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.</para>
    /// <para>The system deletes the file immediately after it is closed or the <see cref="FileStream"/> is finalized.</para>
    /// </remarks>
    /// <returns>The opened <see cref="FileStream"/> object.</returns>
    public static FileStream GetSecureDeleteOnCloseTempFileStream()
    {
      return GetSecureDeleteOnCloseTempFileStream(defaultBufferSize, FileOptions.DeleteOnClose);
    }

    /// <summary>
    /// Creates a unique, randomly named, secure, zero-byte temporary file on disk, which is automatically deleted when it is no longer in use. Returns the opened file stream with the specified buffer size.
    /// </summary>
    /// <remarks>
    /// <para>The generated file name is a cryptographically strong, random string. The file name is guaranteed to be unique to the system's temporary folder.</para>
    /// <para>The <see cref="GetSecureDeleteOnCloseTempFileStream"/> method will raise an <see cref="IOException"/> if no unique temporary file name is available. Although this is possible, it is highly improbable. To resolve this error, delete all uneeded temporary files.</para>
    /// <para>The file is created as a zero-byte file in the system's temporary folder.</para>
    /// <para>The file owner is set to the current user. The file security permissions grant full control to the current user only.</para>
    /// <para>The file sharing is set to none.</para>
    /// <para>The file is marked as a temporary file. File systems avoid writing data back to mass storage if sufficient cache memory is available, because an application deletes a temporary file after a handle is closed. In that case, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.</para>
    /// <para>The system deletes the file immediately after it is closed or the <see cref="FileStream"/> is finalized.</para>
    /// </remarks>
    /// <param name="bufferSize">A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.</param>
    /// <returns>The opened <see cref="FileStream"/> object.</returns>
    public static FileStream GetSecureDeleteOnCloseTempFileStream(int bufferSize)
    {
      return GetSecureDeleteOnCloseTempFileStream(bufferSize, FileOptions.DeleteOnClose);
    }

    /// <summary>
    /// Creates a unique, randomly named, secure, zero-byte temporary file on disk, which is automatically deleted when it is no longer in use. Returns the opened file stream with the specified buffer size and file options.
    /// </summary>
    /// <remarks>
    /// <para>The generated file name is a cryptographically strong, random string. The file name is guaranteed to be unique to the system's temporary folder.</para>
    /// <para>The <see cref="GetSecureDeleteOnCloseTempFileStream"/> method will raise an <see cref="IOException"/> if no unique temporary file name is available. Although this is possible, it is highly improbable. To resolve this error, delete all uneeded temporary files.</para>
    /// <para>The file is created as a zero-byte file in the system's temporary folder.</para>
    /// <para>The file owner is set to the current user. The file security permissions grant full control to the current user only.</para>
    /// <para>The file sharing is set to none.</para>
    /// <para>The file is marked as a temporary file. File systems avoid writing data back to mass storage if sufficient cache memory is available, because an application deletes a temporary file after a handle is closed. In that case, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.</para>
    /// <para>The system deletes the file immediately after it is closed or the <see cref="FileStream"/> is finalized.</para>
    /// <para>Use the <paramref name="options"/> parameter to specify additional file options. You can specify <see cref="FileOptions.Encrypted"/> to encrypt the file contents using the current user account. Specify <see cref="FileOptions.Asynchronous"/> to enable overlapped I/O when using asynchronous reads and writes.</para>
    /// </remarks>
    /// <param name="bufferSize">A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.</param>
    /// <param name="options">A <see cref="FileOptions"/> value that specifies additional file options.</param>
    /// <returns>The opened <see cref="FileStream"/> object.</returns>
    public static FileStream GetSecureDeleteOnCloseTempFileStream(int bufferSize, FileOptions options)
    {
      FileStream fs = GetSecureFileStream(Path.GetTempPath(), bufferSize, options | FileOptions.DeleteOnClose);
      File.SetAttributes(fs.Name, File.GetAttributes(fs.Name) | FileAttributes.Temporary);

      return fs;
    }

    #endregion

    #region GetSecureTempFileStream

    public static FileStream GetSecureTempFileStream()
    {
      return GetSecureTempFileStream(defaultBufferSize, FileOptions.None);
    }

    public static FileStream GetSecureTempFileStream(int bufferSize)
    {
      return GetSecureTempFileStream(bufferSize, FileOptions.None);
    }

    public static FileStream GetSecureTempFileStream(int bufferSize, FileOptions options)
    {
      FileStream fs = GetSecureFileStream(Path.GetTempPath(), bufferSize, options);
      File.SetAttributes(fs.Name,
                         File.GetAttributes(fs.Name) | FileAttributes.NotContentIndexed | FileAttributes.Temporary);

      return fs;
    }

    #endregion

    #region GetSecureTempFileName

    public static string GetSecureTempFileName()
    {
      return GetSecureTempFileName(false);
    }

    public static string GetSecureTempFileName(bool encrypted)
    {
      using (
        FileStream fs = GetSecureFileStream(Path.GetTempPath(), defaultBufferSize,
                                            encrypted ? FileOptions.Encrypted : FileOptions.None))
      {
        File.SetAttributes(fs.Name,
                           File.GetAttributes(fs.Name) | FileAttributes.NotContentIndexed | FileAttributes.Temporary);
        return fs.Name;
      }
    }

    #endregion

    #region GetSecureFileName

    public static string GetSecureFileName(string path)
    {
      return GetSecureFileName(path, false);
    }

    public static string GetSecureFileName(string path, bool encrypted)
    {
      using (
        FileStream fs = GetSecureFileStream(path, defaultBufferSize,
                                            encrypted ? FileOptions.Encrypted : FileOptions.None))
      {
        return fs.Name;
      }
    }

    #endregion

    #region GetSecureFileStream

    public static FileStream GetSecureFileStream(string path)
    {
      return GetSecureFileStream(path, defaultBufferSize, FileOptions.None);
    }

    public static FileStream GetSecureFileStream(string path, int bufferSize)
    {
      return GetSecureFileStream(path, bufferSize, FileOptions.None);
    }

    public static FileStream GetSecureFileStream(string path, int bufferSize, FileOptions options)
    {
      if (path == null)
        throw new ArgumentNullException("path");

      if (bufferSize <= 0)
        throw new ArgumentOutOfRangeException("bufferSize");

      if ((options &
           ~(FileOptions.Asynchronous | FileOptions.DeleteOnClose | FileOptions.Encrypted | FileOptions.RandomAccess |
             FileOptions.SequentialScan | FileOptions.WriteThrough)) != FileOptions.None)
        throw new ArgumentOutOfRangeException("options");

      new FileIOPermission(FileIOPermissionAccess.Write, path).Demand();

      SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
      FileSecurity fileSecurity = new FileSecurity();
      fileSecurity.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, AccessControlType.Allow));
      fileSecurity.SetAccessRuleProtection(true, false);

      fileSecurity.SetOwner(user);

      // Attempt to create a unique file three times before giving up.
      // It is highly improbable that there will ever be a name clash,
      // therefore we do not check to see if the file first exists.
      for (int attempt = 0; attempt < 3; attempt++)
      {
        try
        {
          return new FileStream(Path.Combine(path, Path.GetRandomFileName()), FileMode.CreateNew,
                                FileSystemRights.FullControl, FileShare.None, bufferSize, options, fileSecurity);
        }
        catch (IOException)
        {
          if (attempt == 2)
            throw;
        }
      }
      // This code can never be reached.
      // The compiler thinks otherwise.
      throw new IOException();
    }

    #endregion
  }
}