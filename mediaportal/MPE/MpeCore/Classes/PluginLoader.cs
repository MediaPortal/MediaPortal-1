using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using csscript;
using MediaPortal.Profile;
using MpeCore.Classes;
//using MpeInstaller.Classes.CSScriptLibrary;
using MpeCore.Classes.CSScriptLibrary;
using ComInterfaceType = System.Runtime.InteropServices.ComInterfaceType;
using LayoutKind = System.Runtime.InteropServices.LayoutKind;
using UnmanagedType = System.Runtime.InteropServices.UnmanagedType;
using System.Runtime.InteropServices;


namespace MpeCore.Classes
{
  public class PluginLoader : MarshalByRefObject, IDisposable
  {
    public PluginLoader() {}

    public void Load(string plugin)
    {
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
      Util.LoadPlugins(plugin);
    }

    private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      Assembly retval = null;
      List<string> searchDirs = new List<string>();
      searchDirs.Add(AppDomain.CurrentDomain.BaseDirectory);
      searchDirs.Add(AppDomain.CurrentDomain.RelativeSearchPath);
      foreach (string dir in searchDirs)
      {
        if ((retval = AssemblyResolver.ResolveAssembly(args.Name, dir)) != null)
          break;
      }

      return retval;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      MediaPortal.Profile.Settings.SaveCache();
    }
  }

  #region Licence...

//-----------------------------------------------------------------------------
// Date:	17/10/04	Time: 2:33p 
// Module:	AssemblyResolver.cs
// Classes:	AssemblyResolver
//
// This module contains the definition of the AssemblyResolver class. Which implements 
// some mothods for simplified Assembly navigation
//
// Written by Oleg Shilo (oshilo@gmail.com)
// Copyright (c) 2004-2009. All rights reserved.
//
// Redistribution and use of this code WITHOUT MODIFICATIONS are permitted provided that 
// the following conditions are met:
// 1. Redistributions must retain the above copyright notice, this list of conditions 
//  and the following disclaimer. 
// 2. Neither the name of an author nor the names of the contributors may be used 
//	to endorse or promote products derived from this software without specific 
//	prior written permission.
//
// Redistribution and use of this code WITH MODIFICATIONS are permitted provided that all 
// above conditions are met and software is not used or sold for profit.
//
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//	Caution: Bugs are expected!
//----------------------------------------------

  #endregion

  namespace CSScriptLibrary
  {
    /// <summary>
    /// Class for resolving assembly name to assembly file
    /// </summary>
    public class AssemblyResolver
    {
      #region Class public data...

      /// <summary>
      /// File to be excluded from assembly search
      /// </summary>
      public static string ignoreFileName = "";

      #endregion

      #region Class public methods...

      /// <summary>
      /// Resolves assembly name to assembly file. Loads assembly file to the current AppDomain.
      /// </summary>
      /// <param name="assemblyName">The name of assembly</param>
      /// <param name="dir">The name of directory where local assemblies are expected to be</param>
      /// <returns>loaded assembly</returns>
      public static Assembly ResolveAssembly(string assemblyName, string dir)
      {
        if (Directory.Exists(dir))
        {
          //try file with name AssemblyDisplayName + .dll 
          string[] asmFileNameTokens = assemblyName.Split(", ".ToCharArray(), 5);

          string asmFile = Path.Combine(dir, asmFileNameTokens[0]) + ".dll";
          if (ignoreFileName != Path.GetFileName(asmFile) && File.Exists(asmFile))
          {
            try
            {
              AssemblyName asmName = AssemblyName.GetAssemblyName(asmFile);
              if (asmName != null && asmName.Name == asmFileNameTokens[0])
                return Assembly.LoadFrom(asmFile);
              else if (assemblyName.IndexOf(",") == -1 && asmName.FullName.StartsWith(assemblyName))
                //short name requested 
                return Assembly.LoadFrom(asmFile);
            }
            catch {}
          }

          foreach (string file in Directory.GetFiles(dir, asmFileNameTokens[0] + ".*"))
          {
            try
            {
              AssemblyName asmName = AssemblyName.GetAssemblyName(file);
              if (asmName != null && asmName.FullName == assemblyName)
                return Assembly.LoadFrom(file);
              else if (assemblyName.IndexOf(",") == -1 && asmName.FullName.StartsWith(assemblyName)) //short name requested
                return Assembly.LoadFrom(file);
            }
            catch {}
          }
        }
        return null;
      }

      /// <summary>
      /// Resolves namespace/assembly(file) name into array of assembly locations (local and GAC ones).
      /// </summary>
      /// <param name="name">'namespace'/assembly(file) name</param>
      /// <param name="searchDirs">Assembly seartch directories</param>
      /// <returns>collection of assembly file names wher namespace is impelemented</returns>
      public static string[] FindAssembly(string name, string[] searchDirs)
      {
#if net1
			ArrayList retval = new ArrayList();
#else
        List<string> retval = new List<string>();
#endif

        foreach (string dir in searchDirs)
        {
          foreach (string asmLocation in FindLocalAssembly(name, dir)) //local assemblies allternative locations
            retval.Add(asmLocation);

          if (retval.Count != 0)
            break;
        }

        if (retval.Count == 0)
        {
          foreach (string asmGACLocation in FindGlobalAssembly(name))
          {
            retval.Add(asmGACLocation);
          }
        }
#if net1
			return (string[])retval.ToArray(typeof(string));
#else
        return retval.ToArray();
#endif
      }

      /// <summary>
      /// Resolves namespace into array of local assembly locations.
      /// (Currently it returns only one assembly location but in future 
      /// it can be extended to collect all assemblies with the same namespace)
      /// </summary>
      /// <param name="name">namespace/assembly name</param>
      /// <param name="dir">directory</param>
      /// <returns>collection of assembly file names wher namespace is impelemented</returns>
      public static string[] FindLocalAssembly(string name, string dir)
      {
        //We are returning and array because name may represent assembly name or namespace 
        //and as such can consist of more than one assembly file (multiple assembly file is not supported at this stage).
        if (Directory.Exists(dir))
        {
          string asmFile = Path.Combine(dir, name);
          if (asmFile != Path.GetFileName(asmFile) && File.Exists(asmFile))
            return new string[] {asmFile};

          foreach (string ext in new string[] {".dll", ".exe", ".csc"})
          {
            string file = asmFile + ext; //just in case if user did not specify the extension
            if (ignoreFileName != Path.GetFileName(file) && File.Exists(file))
              return new string[] {file};
          }
        }
        return new string[0];
      }

      /// <summary>
      /// Resolves namespace into array of global assembly (GAC) locations.
      /// </summary>
      /// <param name="namespaceStr">'namespace' name</param>
      /// <returns>collection of assembly file names wher namespace is impelemented</returns>
      public static string[] FindGlobalAssembly(String namespaceStr)
      {
#if net1
			ArrayList retval = new ArrayList();
#else
        List<string> retval = new List<string>();
#endif
        try
        {
          AssemblyEnum asmEnum = new AssemblyEnum(namespaceStr);
          String asmName;
          while ((asmName = asmEnum.GetNextAssembly()) != null)
          {
            string asmLocation = AssemblyCache.QueryAssemblyInfo(asmName);
            retval.Add(asmLocation);
          }
        }
        catch
        {
          //If exception is thrown it is very likely it is because where fusion.dll does not exist/unavailable/broken.
          //We might be running under the MONO run-time. 
        }

        if (retval.Count == 0 && namespaceStr.ToLower().EndsWith(".dll"))
          retval.Add(namespaceStr); //in case of if the namespaceStr is a dll name
#if net1
			return (string[])retval.ToArray(typeof(string));
#else
        return retval.ToArray();
#endif
      }

      #endregion

      /// <summary>
      /// Search for namespace into local assembly file.
      /// </summary>
      private static bool IsNamespaceDefinedInAssembly(string asmFileName, string namespaceStr)
      {
        if (File.Exists(asmFileName))
        {
          try
          {
            Assembly assembly = Assembly.LoadFrom(asmFileName);
            if (assembly != null)
            {
              foreach (Module m in assembly.GetModules())
              {
                foreach (Type t in m.GetTypes())
                {
                  if (namespaceStr == t.Namespace)
                  {
                    return true;
                  }
                }
              }
            }
          }
          catch {}
        }
        return false;
      }
    }
  }
}

namespace csscript
{
  /// <summary>
  /// COM HR checker: just to make code more compact;
  /// </summary>
  internal class COM
  {
    public static void CheckHR(int hr)
    {
      if (hr < 0)
        Marshal.ThrowExceptionForHR(hr);
    }
  }

  /// <summary>
  /// IAssemblyCache; COM import
  /// </summary>
  [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
  internal interface IAssemblyCache
  {
    //PreserveSig() Indicates that the HRESULT or retval signature transformation that takes place during COM interop calls should be suppressed
    [PreserveSig()]
    int UninstallAssembly(int flags,
                          [MarshalAs(UnmanagedType.LPWStr)] string assemblyName,
                          InstallReference refData,
                          out AssemblyCacheUninstallDisposition disposition);

    [PreserveSig()]
    int QueryAssemblyInfo(int flags,
                          [MarshalAs(UnmanagedType.LPWStr)] string assemblyName,
                          ref AssemblyInfo assemblyInfo);

    [PreserveSig()]
    int Reserved(int flags,
                 IntPtr pvReserved,
                 out Object ppAsmItem,
                 [MarshalAs(UnmanagedType.LPWStr)] string assemblyName);

    [PreserveSig()]
    int Reserved(out Object ppAsmScavenger);

    [PreserveSig()]
    int InstallAssembly(int flags,
                        [MarshalAs(UnmanagedType.LPWStr)] string assemblyFilePath,
                        InstallReference refData);
  }

  /// <summary>
  /// IAssemblyName; COM import
  /// </summary>
  [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
  internal interface IAssemblyName
  {
    [PreserveSig()]
    int SetProperty(int PropertyId,
                    IntPtr pvProperty,
                    int cbProperty);

    [PreserveSig()]
    int GetProperty(int PropertyId,
                    IntPtr pvProperty,
                    ref int pcbProperty);

    [PreserveSig()]
    int Finalize();

    [PreserveSig()]
    int GetDisplayName(StringBuilder pDisplayName,
                       ref int pccDisplayName,
                       int displayFlags);

    [PreserveSig()]
    int Reserved(ref Guid guid,
                 Object o1,
                 Object o2,
                 string string1,
                 Int64 llFlags,
                 IntPtr pvReserved,
                 int cbReserved,
                 out IntPtr ppv);

    [PreserveSig()]
    int GetName(ref int pccBuffer,
                StringBuilder pwzName);

    [PreserveSig()]
    int GetVersion(out int versionHi,
                   out int versionLow);

    [PreserveSig()]
    int IsEqual(IAssemblyName pAsmName,
                int cmpFlags);

    [PreserveSig()]
    int Clone(out IAssemblyName pAsmName);
  }

  /// <summary>
  /// IAssemblyEnum; COM import
  /// </summary>
  [System.Runtime.InteropServices.ComImport,
   System.Runtime.InteropServices.InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
   Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
  internal interface IAssemblyEnum
  {
    [System.Runtime.InteropServices.PreserveSig()]
    int GetNextAssembly(IntPtr pvReserved,
                        out IAssemblyName ppName,
                        int flags);

    [PreserveSig()]
    int Reset();

    [PreserveSig()]
    int Clone(out IAssemblyEnum ppEnum);
  }

  /// <summary>
  /// AssemblyCommitFlags; Used by COM imported calls 
  /// </summary>
  internal enum AssemblyCommitFlags
  {
    Default,
    Force
  }

  /// <summary>
  /// AssemblyCacheFlags; Used by COM imported calls
  /// </summary>
  [Flags]
  internal enum AssemblyCacheFlags
  {
    GAC = 2
  }

  /// <summary>
  /// AssemblyCacheUninstallDisposition; Used by COM imported calls
  /// </summary>
  internal enum AssemblyCacheUninstallDisposition
  {
    Unknown,
    Uninstalled,
    StillInUse,
    AlreadyUninstalled,
    DeletePending,
    HasInstallReference,
    ReferenceNotFound,
  }

  /// <summary>
  /// CreateAssemblyNameObjectFlags; Used by COM imported calls
  /// </summary>
  internal enum CreateAssemblyNameObjectFlags
  {
    CANOF_DEFAULT,
    CANOF_PARSE_DISPLAY_NAME,
    CANOF_SET_DEFAULT_VALUES
  }

  /// <summary>
  /// AssemblyNameDisplayFlags; Used by COM imported calls
  /// </summary>
  [Flags]
  internal enum AssemblyNameDisplayFlags
  {
    VERSION = 0x01,
    CULTURE = 0x02,
    PUBLIC_KEY_TOKEN = 0x04,
    PROCESSORARCHITECTURE = 0x20,
    RETARGETABLE = 0x80,
    ALL = VERSION
          | CULTURE
          | PROCESSORARCHITECTURE
          | PUBLIC_KEY_TOKEN
          | RETARGETABLE
  }

  /// <summary>
  /// InstallReference + struct initialization; Used by COM imported calls
  /// </summary>
  [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
  internal class InstallReference
  {
    private int cbSize;
    private int flags;
    private Guid guidScheme;
    [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)] private string identifier;
    [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)] private string nonCannonicalData;

    public InstallReference(Guid guid, string id, string data)
    {
      cbSize = (int)(2 * IntPtr.Size + 16 + (id.Length + data.Length) * 2);
      flags = 0;
      guidScheme = guid;
      identifier = id;
      nonCannonicalData = data;
    }

    public Guid GuidScheme
    {
      get { return guidScheme; }
    }
  }

  /// <summary>
  /// AssemblyInfo; Used by COM imported calls
  /// </summary>
  [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
  internal struct AssemblyInfo
  {
    public int cbAssemblyInfo;
    public int assemblyFlags;
    public long assemblySizeInKB;
    [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)] public string currentAssemblyPath;
    public int cchBuf;
  }

  /// <summary>
  /// InstallReferenceGuid; Used by COM imported calls
  /// </summary>
  [System.Runtime.InteropServices.ComVisible(false)]
  internal class InstallReferenceGuid
  {
    public static bool IsValidGuidScheme(Guid guid)
    {
      return (guid.Equals(UninstallSubkeyGuid) ||
              guid.Equals(FilePathGuid) ||
              guid.Equals(OpaqueStringGuid) ||
              guid.Equals(Guid.Empty));
    }

    public static readonly Guid UninstallSubkeyGuid = new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
    public static readonly Guid FilePathGuid = new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
    public static readonly Guid OpaqueStringGuid = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
  }

  /// <summary>
  ///  Helper calss for IAssemblyCache
  /// </summary>
  [System.Runtime.InteropServices.ComVisible(false)]
  internal class AssemblyCache
  {
    // If you use this, fusion will do the streaming & commit
    public static void InstallAssembly(string assemblyPath, InstallReference reference, AssemblyCommitFlags flags)
    {
      if (reference != null)
      {
        if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
          throw new ArgumentException("Invalid argument( reference guid).");
      }

      IAssemblyCache asmCache = null;

      COM.CheckHR(CreateAssemblyCache(out asmCache, 0));
      COM.CheckHR(asmCache.InstallAssembly((int)flags, assemblyPath, reference));
    }

    public static void UninstallAssembly(string assemblyName, InstallReference reference,
                                         out AssemblyCacheUninstallDisposition disp)
    {
      AssemblyCacheUninstallDisposition dispResult = AssemblyCacheUninstallDisposition.Uninstalled;
      if (reference != null)
      {
        if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
          throw new ArgumentException("Invalid argument (reference guid).");
      }

      IAssemblyCache asmCache = null;

      COM.CheckHR(CreateAssemblyCache(out asmCache, 0));
      COM.CheckHR(asmCache.UninstallAssembly(0, assemblyName, reference, out dispResult));

      disp = dispResult;
    }

    public static string QueryAssemblyInfo(string assemblyName)
    {
      if (assemblyName == null)
      {
        throw new ArgumentException("Invalid argument (assemblyName)");
      }

      AssemblyInfo aInfo = new AssemblyInfo();
      aInfo.cchBuf = 1024;
      aInfo.currentAssemblyPath = "Path".PadLeft(aInfo.cchBuf);

      IAssemblyCache ac = null;
      COM.CheckHR(CreateAssemblyCache(out ac, 0));
      COM.CheckHR(ac.QueryAssemblyInfo(0, assemblyName, ref aInfo));

      return aInfo.currentAssemblyPath;
    }

    [System.Runtime.InteropServices.DllImport("fusion.dll")]
    internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
  }

  /// <summary>
  /// Helper calss for IAssemblyEnum
  /// </summary>
  [System.Runtime.InteropServices.ComVisible(false)]
  internal class AssemblyEnum
  {
    public AssemblyEnum(string sAsmName)
    {
      IAssemblyName asmName = null;
      if (sAsmName != null) //if no name specified all ssemblies will be returned
      {
        COM.CheckHR(CreateAssemblyNameObject(out asmName, sAsmName,
                                             CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero));
      }
      COM.CheckHR(CreateAssemblyEnum(out m_assemblyEnum, IntPtr.Zero, asmName, AssemblyCacheFlags.GAC, IntPtr.Zero));
    }

    public string GetNextAssembly()
    {
      string retval = null;
      if (!m_done)
      {
        IAssemblyName asmName = null;
        COM.CheckHR(m_assemblyEnum.GetNextAssembly((IntPtr)0, out asmName, 0));

        if (asmName != null)
          retval = GetFullName(asmName);

        m_done = (retval != null);
      }
      return retval;
    }

    private string GetFullName(IAssemblyName asmName)
    {
      StringBuilder fullName = new StringBuilder(1024);
      int iLen = fullName.Capacity;
      COM.CheckHR(asmName.GetDisplayName(fullName, ref iLen, (int)AssemblyNameDisplayFlags.ALL));

      return fullName.ToString();
    }

    [System.Runtime.InteropServices.DllImport("fusion.dll")]
    internal static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum,
                                                  IntPtr pUnkReserved,
                                                  IAssemblyName pName,
                                                  AssemblyCacheFlags flags,
                                                  IntPtr pvReserved);

    [System.Runtime.InteropServices.DllImport("fusion.dll")]
    internal static extern int CreateAssemblyNameObject(out IAssemblyName ppAssemblyNameObj,
                                                        [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName,
                                                        CreateAssemblyNameObjectFlags flags,
                                                        IntPtr pvReserved);

    private bool m_done;
    private IAssemblyEnum m_assemblyEnum = null;
  }
}