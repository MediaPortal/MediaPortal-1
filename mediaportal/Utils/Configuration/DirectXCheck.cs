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
