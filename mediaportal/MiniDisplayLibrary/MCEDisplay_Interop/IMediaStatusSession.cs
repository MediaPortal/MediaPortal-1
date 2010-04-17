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

using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.MCEDisplay_Interop
{
  [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("A70D81F2-C9D2-4053-AF0E-CDEA39BDD1AD")]
  public interface IMediaStatusSession
  {
    [DispId(1)]
    void MediaStatusChange(
      [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] MediaStatusPropertyTag[] tags,
      [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] object[] values);

    [DispId(2)]
    void Close();
  }
}