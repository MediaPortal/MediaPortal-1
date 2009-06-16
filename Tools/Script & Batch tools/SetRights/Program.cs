using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SetRights
{
  class Program
  {
    static int Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Usage:\n\n");
        Console.WriteLine("      SetRights <DirPath>");
        return 1;
      }
      GrantFullControlFolder(args[0]);
      return 0;
    }

    /// <summary>
    /// Grants full control on the given file for everyone
    /// </summary>
    /// <param name="folderName">FolderName</param>
    public static void GrantFullControlFolder(string folderName)
    {
      if (!System.IO.Directory.Exists(folderName)) return;
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
            PropagationFlags.NoPropagateInherit,
            AccessControlType.Allow);
        security.AddAccessRule(newRule);
        System.IO.Directory.SetAccessControl(folderName, security);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error while setting full write access to everyone for file: {0} : {1}", folderName, ex);
      }
    }
  }
}
