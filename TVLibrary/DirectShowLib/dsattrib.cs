#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2007
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DirectShowLib.BDA
{

  #region Declarations

  #endregion

  #region Interfaces

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("583ec3cc-4960-4857-982b-41a33ea0a006"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAttributeSet
  {
    [PreserveSig]
    int SetAttrib(
      [In] Guid guidAttribute,
      [In] IntPtr pbAttribute,
      [In] int dwAttributeLength
      );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("52dbd1ec-e48f-4528-9232-f442a68f0ae1"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAttributeGet
  {
    [PreserveSig]
    int GetCount([Out] out int plCount);

    [PreserveSig]
    int GetAttribIndexed(
      [In] int lIndex,
      [Out] out Guid guidAttribute,
      [In, Out] IntPtr pbAttribute,
      [In, Out] ref int dwAttributeLength
      );

    [PreserveSig]
    int GetAttrib(
      [In] Guid guidAttribute,
      [In, Out] IntPtr pbAttribute,
      [In, Out] ref int dwAttributeLength
      );
  }

  #endregion
}