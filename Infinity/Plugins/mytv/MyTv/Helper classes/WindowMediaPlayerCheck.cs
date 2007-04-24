using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace MyTv
{
  class WindowMediaPlayerCheck
  {
    public bool IsInstalled
    {
      get
      {
        using (
            RegistryKey subkey =
                Registry.LocalMachine.OpenSubKey(
                    @"Software\Microsoft\Active Setup\Installed Components\{22d6f312-b0f6-11d0-94ab-0080c74c7e95}")
            )
        {
          if (subkey != null)
          {
            if (((int)subkey.GetValue("IsInstalled")) == 1)
            {
              //10.0.0.3802
              //11,0,5721,5145
              string wmpversion = (string)subkey.GetValue("Version");
              if (wmpversion.Length > 0)
              {
                string strTmp = "";
                for (int i = 0; i < wmpversion.Length; ++i)
                {
                  if (Char.IsDigit(wmpversion[i]))
                  {
                    strTmp += wmpversion[i];
                  }
                }
                long lVersion = Convert.ToInt64(strTmp);
                if (lVersion < 10003802) return false;
                return true;
              }
            }
          }
        }
        return false;
      }
    }
  }
}
