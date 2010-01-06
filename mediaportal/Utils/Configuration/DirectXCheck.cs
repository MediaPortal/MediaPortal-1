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

using System;
using System.IO;

namespace MediaPortal.Configuration
{
  public class DirectXCheck
  {
    public static bool IsInstalled()
    {
      string[] DllList = {
                           @"\System32\D3DX9_30.dll",
                           @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.Direct3D.dll",
                           @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.DirectDraw.dll",
                           @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.DirectInput.dll",
                           @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.dll",
                           @"\microsoft.net\DirectX for Managed Code\1.0.2911.0\Microsoft.DirectX.Direct3DX.dll"
                         };
      string WinDir = Environment.GetEnvironmentVariable("WINDIR");
      foreach (string DllFile in DllList)
      {
        if (!File.Exists(WinDir + "\\" + DllFile))
        {
          return false;
        }
      }
      return true;
    }
  }
}