using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace MyTv
{
  class TsReaderCheck
  {
    public bool IsInstalled
    {
      get
      {
        using (RegistryKey subkey = Registry.ClassesRoot.OpenSubKey(@"CLSID\{B9559486-E1BB-45D3-A2A2-9A7AFE49B23F}"))
        {
          if (subkey != null)
          {
            SetExtension(".ts", "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}");
            SetExtension(".tp", "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}");
            SetExtension(".tsbuffer", "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}");
            SetExtension(".tsp", "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}");
            SetPermission(".ts");
            SetPermission(".tp");
            SetPermission(".tsbuffer");
            SetPermission(".tsp");
            return true;
          }
        }
        return false;
      }
    }
    void SetExtension(string extension, string clsid)
    {
      using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Media Type\Extensions",true))
      {
        RegistryKey subkey=key.OpenSubKey(extension,true);
        if (subkey==null)
        {
          subkey=key.CreateSubKey(extension);
        }
        subkey.SetValue("Source Filter", clsid);
        subkey.Close();
      }
    }
    void SetPermission(string extension)
    {
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\MultiMedia\WMPlayer\Extensions", true))
      {
        RegistryKey subkey = key.OpenSubKey(extension, true);
        if (subkey == null)
        {
          subkey = key.CreateSubKey(extension);
        }
        UInt32 permission = 0xf;
        UInt32 runtime = 0x7;
        subkey.SetValue("Permission", permission);
        subkey.SetValue("Runtime", runtime);
        subkey.Close();
      }
    }
  }
}

