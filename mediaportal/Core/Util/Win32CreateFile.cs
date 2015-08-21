using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Win32
{
  public static partial class Function
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern SafeFileHandle CreateFile(
      [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
      [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
      IntPtr lpSecurityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
      [MarshalAs(UnmanagedType.U4)] CreationDisposition dwCreationDisposition,
      [MarshalAs(UnmanagedType.U4)] FileFlagsAttributes dwFlagsAndAttributes,
      IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern SafeFileHandle CreateFileA(
      [MarshalAs(UnmanagedType.LPStr)] string lpFileName,
      [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
      IntPtr lpSecurityAttributes,
      [MarshalAs(UnmanagedType.U4)] CreationDisposition dwCreationDisposition,
      [MarshalAs(UnmanagedType.U4)] FileFlagsAttributes dwFlagsAndAttributes,
      IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern SafeFileHandle CreateFileW(
      [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
      [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
      IntPtr lpSecurityAttributes,
      [MarshalAs(UnmanagedType.U4)] CreationDisposition dwCreationDisposition,
      [MarshalAs(UnmanagedType.U4)] FileFlagsAttributes dwFlagsAndAttributes,
      IntPtr hTemplateFile);
  }


  public static partial class Macro
  {
  }


  public static partial class Const
  {
  }

  [Flags]
  public enum FileAccess : uint
  {
    NONE = 0,

    GENERIC_ALL = 0x10000000,
    GENERIC_EXECUTE = 0x20000000,
    GENERIC_READ = 0x80000000,
    GENERIC_WRITE = 0x40000000,

    FILE_READ_DATA = (0x0001), // file & pipe
    FILE_LIST_DIRECTORY = (0x0001), // directory

    FILE_WRITE_DATA = (0x0002), // file & pipe
    FILE_ADD_FILE = (0x0002), // directory

    FILE_APPEND_DATA = (0x0004), // file
    FILE_ADD_SUBDIRECTORY = (0x0004), // directory
    FILE_CREATE_PIPE_INSTANCE = (0x0004), // named pipe

    FILE_READ_EA = (0x0008), // file & directory

    FILE_WRITE_EA = (0x0010), // file & directory

    FILE_EXECUTE = (0x0020), // file
    FILE_TRAVERSE = (0x0020), // directory

    FILE_DELETE_CHILD = (0x0040), // directory

    FILE_READ_ATTRIBUTES = (0x0080), // all

    FILE_WRITE_ATTRIBUTES = (0x0100), // all

    FILE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF),

    FILE_GENERIC_READ = (STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE),

    FILE_GENERIC_WRITE =
      (STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE),
    FILE_GENERIC_EXECUTE = (STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE),

    DELETE = (0x00010000),
    READ_CONTROL = (0x00020000),
    WRITE_DAC = (0x00040000),
    WRITE_OWNER = (0x00080000),
    SYNCHRONIZE = (0x00100000),

    STANDARD_RIGHTS_REQUIRED = (0x000F0000),

    STANDARD_RIGHTS_READ = (READ_CONTROL),
    STANDARD_RIGHTS_WRITE = (READ_CONTROL),
    STANDARD_RIGHTS_EXECUTE = (READ_CONTROL),

    STANDARD_RIGHTS_ALL = (0x001F0000),

    SPECIFIC_RIGHTS_ALL = (0x0000FFFF),

    ACCESS_SYSTEM_SECURITY = (0x01000000),

    MAXIMUM_ALLOWED = (0x02000000)
  }


  [Flags]
  public enum FileShare : uint
  {
    /// <summary>
    ///   Prevents other processes from opening a file or device if they request delete, read, or write access.
    /// </summary>
    FILE_SHARE_NONE = 0x00000000,

    /// <summary>
    ///   Enables subsequent open operations on an object to request read access.
    ///   Otherwise, other processes cannot open the object if they request read access.
    ///   If this flag is not specified, but the object has been opened for read access, the function fails.
    /// </summary>
    FILE_SHARE_READ = 0x00000001,

    /// <summary>
    ///   Enables subsequent open operations on an object to request write access.
    ///   Otherwise, other processes cannot open the object if they request write access.
    ///   If this flag is not specified, but the object has been opened for write access, the function fails.
    /// </summary>
    FILE_SHARE_WRITE = 0x00000002,

    /// <summary>
    ///   Enables subsequent open operations on an object to request delete access.
    ///   Otherwise, other processes cannot open the object if they request delete access.
    ///   If this flag is not specified, but the object has been opened for delete access, the function fails.
    /// </summary>
    FILE_SHARE_DELETE = 0x00000004
  }

  public enum CreationDisposition : uint
  {
    /// <summary>
    ///   Creates a new file. The function fails if a specified file exists.
    /// </summary>
    CREATE_NEW = 1,

    /// <summary>
    ///   Creates a new file, always.
    ///   If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file
    ///   attributes,
    ///   and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES
    ///   structure specifies.
    /// </summary>
    CREATE_ALWAYS = 2,

    /// <summary>
    ///   Opens a file. The function fails if the file does not exist.
    /// </summary>
    OPEN_EXISTING = 3,

    /// <summary>
    ///   Opens a file, always.
    ///   If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
    /// </summary>
    OPEN_ALWAYS = 4,

    /// <summary>
    ///   Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
    ///   The calling process must open the file with the GENERIC_WRITE access right.
    /// </summary>
    TRUNCATE_EXISTING = 5
  }

  [Flags]
  public enum FileFlagsAttributes : uint
  {
    FILE_ATTRIBUTE_READONLY = 0x00000001,
    FILE_ATTRIBUTE_HIDDEN = 0x00000002,
    FILE_ATTRIBUTE_SYSTEM = 0x00000004,
    FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
    FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
    FILE_ATTRIBUTE_DEVICE = 0x00000040,
    FILE_ATTRIBUTE_NORMAL = 0x00000080,
    FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
    FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
    FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
    FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
    FILE_ATTRIBUTE_OFFLINE = 0x00001000,
    FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
    FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,
    FILE_ATTRIBUTE_INTEGRITY_STREAM = 0x00008000,
    FILE_ATTRIBUTE_VIRTUAL = 0x00010000,
    FILE_ATTRIBUTE_NO_SCRUB_DATA = 0x00020000,
    //  These are flags supported through CreateFile (W7) and CreateFile2 (W8 and beyond)
    FILE_FLAG_WRITE_THROUGH = 0x80000000,
    FILE_FLAG_OVERLAPPED = 0x40000000,
    FILE_FLAG_NO_BUFFERING = 0x20000000,
    FILE_FLAG_RANDOM_ACCESS = 0x10000000,
    FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
    FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
    FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
    FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
    FILE_FLAG_SESSION_AWARE = 0x00800000,
    FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
    FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
    FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000
  }
}