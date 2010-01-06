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

namespace MediaPortal.Player
{
  public enum eAudioDualMonoMode
  {
    STEREO = 0,
    LEFT_MONO = 1,
    RIGHT_MONO = 2,
    MIX_MONO = 3,
    UNSUPPORTED = 4
  } ;

  [ComVisible(true), ComImport,
   Guid("A575A6D8-6F52-4598-9507-6542EBB67677"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPAudioSwitcherFilter
  {
    [PreserveSig]
    int GetAudioDualMonoMode([Out] out uint mode);

    [PreserveSig]
    int SetAudioDualMonoMode([In] uint mode);
  }
}