#region Copyright (C) 2005-2011 Team MediaPortal

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

//
// This class is based on the source of http://www.codeproject.com/Articles/13391/Using-IFilter-in-C. 
// Many thanks to the original author!
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper
{
  /// <summary>
  /// An interface used to create instances of a specific class.
  /// </summary>
  [Guid("00000001-0000-0000-c000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IClassFactory
  {
    /// <summary>
    /// Create an instance of a class.
    /// </summary>
    /// <param name="pUnkOuter">A pointer to the IUnknown interface of an aggregate.</param>
    /// <param name="rIid">The identifier of the interface used to communicate with the instance.</param>
    /// <param name="ppvObject">The instance created.</param>
    /// <returns>an HRESULT indicating whether the instance was successfully created</returns>
    [PreserveSig]
    int CreateInstance([MarshalAs(UnmanagedType.Interface)] object pUnkOuter, ref Guid rIid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

    /// <summary>
    /// Lock an object application open in memory. This enables instances to be created more quickly.
    /// </summary>
    /// <param name="fLock"><c>True</c> to increment the lock count.</param>
    /// <returns>an HRESULT indicating whether the application was successfully locked</returns>
    [PreserveSig]
    int LockServer(bool fLock);
  }

  /// <summary>
  /// A utility class, used to create instances of classes that are implemented in external libraries.
  /// </summary>
  public static class ComHelper
  {
    private delegate int DllGetClassObject(ref Guid rClsid, ref Guid rIid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

    private class LibrarySet
    {
      private readonly List<IntPtr> _libraryHandles = new List<IntPtr>();
      public void AddLibrary(IntPtr handle)
      {
        lock (_libraryHandles)
        {
          _libraryHandles.Add(handle);
        }
      }

      ~LibrarySet()
      {
        foreach (IntPtr handle in _libraryHandles)
        {
          try
          {
            MediaPortal.Common.Utils.NativeMethods.FreeLibrary(handle);
          }
          catch
          {
          }
        }
      }
    }

    static readonly LibrarySet LIBRARIES = new LibrarySet();

    /// <summary>
    /// Load a COM object instance from a file.
    /// </summary>
    /// <param name="fileName">The name of the file containing the COM class implementation.</param>
    /// <param name="clsid">The COM object class identifier.</param>
    /// <param name="iid">The interface identifier for the COM object.</param>
    /// <param name="useAssemblyRelativeLocation">If <c>true</c>, add the assembly path to <paramref name="fileName"/>.</param>
    /// <returns>the COM object instance if successful, otherwise <c>null</c></returns>
    public static object LoadComObjectFromFile(string fileName, Guid clsid, Guid iid, bool useAssemblyRelativeLocation = true)
    {
      IClassFactory classFactory = null;
      try
      {
        classFactory = ComHelper.GetClassFactory(fileName, clsid, useAssemblyRelativeLocation);
        if (classFactory == null)
        {
          return null;
        }
        object obj;
        Guid interfaceId = iid;
        int hr = classFactory.CreateInstance(null, ref interfaceId, out obj);
        HResult.ThrowException(hr, string.Format("Failed to obtain instance from class factory for COM class {0} and interface {1}.", clsid, iid));
        return obj;
      }
      finally
      {
        Release.ComObject("COM helper class factory", ref classFactory);
      }
    }

    /// <summary>
    /// Get a class factory for a COM class.
    /// </summary>
    /// <param name="fileName">The name of the file containing the COM class implementation.</param>
    /// <param name="clsid">The class identifier.</param>
    /// <param name="useAssemblyRelativeLocation">If <c>true</c>, add the assembly path to <paramref name="fileName"/>.</param>
    /// <returns>an IClassFactory instance that can be used to create instances of a class</returns>
    public static IClassFactory GetClassFactory(string fileName, Guid clsid, bool useAssemblyRelativeLocation = false)
    {
      string filePath = useAssemblyRelativeLocation ? PathManager.BuildAssemblyRelativePath(fileName) : fileName;
      IntPtr libraryHandle = MediaPortal.Common.Utils.NativeMethods.LoadLibraryA(filePath);
      if (libraryHandle == IntPtr.Zero)
      {
        return null;
      }

      // Keep a reference to the library. This reference must be released later
      // to avoid leaking memory.
      LIBRARIES.AddLibrary(libraryHandle);

      // Obtain a delegate for the class's DllGetClassObject function.
      IntPtr functionAddress = MediaPortal.Common.Utils.NativeMethods.GetProcAddress(libraryHandle, "DllGetClassObject");
      if (functionAddress == IntPtr.Zero)
      {
        return null;
      }
      DllGetClassObject dllGetClassObject = (DllGetClassObject)Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(DllGetClassObject));

      // Retrieve the class factory.
      Guid comObjectClsid = clsid;
      Guid classFactoryClsid = typeof(IClassFactory).GUID;
      object comObjectClassFactory;
      int hr = dllGetClassObject(ref comObjectClsid, ref classFactoryClsid, out comObjectClassFactory);
      HResult.ThrowException(hr, string.Format("Failed to obtain class factory for COM class {0}.", clsid));
      return comObjectClassFactory as IClassFactory;
    }
  }
}
