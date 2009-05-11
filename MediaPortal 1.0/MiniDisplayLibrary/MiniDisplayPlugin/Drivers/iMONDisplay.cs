using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONDisplay
  {
    private static bool _A_DLL = false;
    private System.Type _iMONDLL;
    private static bool _S_DLL = false;
    private static bool _UseV3DLL;
    private const BindingFlags BINDING_FLAGS = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
    private static bool DoDebug = (Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging);
    private string imonRC_DLLFile;
    private const MethodAttributes METHOD_ATTRIBUTES = (MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public);
    private static ModuleBuilder s_mb;

    internal iMONDisplay()
    {
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay constructor: called", new object[0]);
      }
      if (this._iMONDLL == null)
      {
        this.CreateImonDLLWrapper();
      }
      _UseV3DLL = iMONLCDg.AdvancedSettings.Load().VFD_UseV3DLL;
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay constructor: UseV3DLL option set '{0}' for display", new object[] { _UseV3DLL });
      }
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay constructor: completed", new object[0]);
      }
    }

    [DllImport(@"..\VFD\SG_VFD.dll", EntryPoint = "iMONLCD_SendData")]
    private static extern bool _A_iMONLCD_SendData(ref ulong bitMap);
    [DllImport(@"..\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_Init")]
    private static extern bool _A_iMONVFD_Init(int vfdType, int resevered);
    [DllImport(@"..\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited")]
    private static extern bool _A_iMONVFD_IsInited();
    [DllImport(@"..\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ")]
    private static extern bool _A_iMONVFD_SetEQ(int[] arEQValue);
    [DllImport(@"..\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_SetText")]
    private static extern bool _A_iMONVFD_SetText(string firstLine, string secondLine);
    [DllImport(@"..\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit")]
    private static extern void _A_iMONVFD_Uninit();
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONLCD_SendData")]
    private static extern bool _iMONLCD_SendData(ref ulong bitMap);
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_Init")]
    private static extern bool _iMONVFD_Init(int vfdType, int resevered);
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited")]
    private static extern bool _iMONVFD_IsInited();
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ")]
    private static extern bool _iMONVFD_SetEQ(int[] arEQValue);
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_SetText")]
    private static extern bool _iMONVFD_SetText(string firstLine, string secondLine);
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit")]
    private static extern void _iMONVFD_Uninit();
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "iMONLCD_SendData")]
    private static extern bool _S_iMONLCD_SendData(ref ulong bitMap);
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_Init")]
    private static extern bool _S_iMONVFD_Init(int vfdType, int resevered);
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited")]
    private static extern bool _S_iMONVFD_IsInited();
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ")]
    private static extern bool _S_iMONVFD_SetEQ(int[] arEQValue);
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_SetText")]
    private static extern bool _S_iMONVFD_SetText(string firstLine, string secondLine);
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit")]
    private static extern void _S_iMONVFD_Uninit();
    [DllImport(@"..\iMON\SG_VFD.dll", EntryPoint = "LCD3R_SetLCDData2")]
    private static extern bool _S_LCD3R_SetLCDData2(ref ulong bitMap);
    private bool CreateImonDLLWrapper()
    {
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): called", new object[0]);
      }
      new FileInfo(Assembly.GetEntryAssembly().Location);
      this.imonRC_DLLFile = this.FindImonRCdll();
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): using RC DLL {1}", new object[] { this.imonRC_DLLFile });
      }
      if (this.imonRC_DLLFile == string.Empty)
      {
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): FAILED - SG_RC.dll not found", new object[0]);
        }
        return false;
      }
      try
      {
        if (s_mb == null)
        {
          AssemblyName name = new AssemblyName();
          name.Name = "iMONDLLWrapper" + Guid.NewGuid().ToString("N");
          s_mb = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("iMON_DLL_wrapper");
        }
        TypeBuilder builder2 = s_mb.DefineType("iMONDLLWrapper" + Guid.NewGuid().ToString("N"));
        MethodBuilder builder3 = builder2.DefinePInvokeMethod("iMONRC_Init", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(bool), new System.Type[] { typeof(int), typeof(int), typeof(int) }, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_Uninit", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(void), null, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_IsInited", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(bool), null, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetHWType", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), null, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetFirmwareVer", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), null, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_CheckDriverVersion", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), null, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_ChangeiMONRCSet", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(bool), new System.Type[] { typeof(int) }, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_ChangeRC6", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(bool), new System.Type[] { typeof(int) }, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetLastRFMode", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), null, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetPacket", this.imonRC_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(bool), new System.Type[] { typeof(byte[]), typeof(int) }, CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        this._iMONDLL = builder2.CreateType();
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Error("iMONLCDg.iMONDisplay.CreateImonDLLwrapper(): caught exception: {0}", new object[] { exception });
        return false;
      }
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): Completed - RC DLL wrapper created.", new object[0]);
      }
      return true;
    }

    public string FindImonRCdll()
    {
      RegistryKey key;
      string str;
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.FindImonRCdll(): called.", new object[0]);
      }
      bool flag = false;
      bool flag2 = false;
      string str2 = string.Empty;
      string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string str4 = string.Empty;
      string str5 = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false) != null)
      {
        flag = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VFD.exe", false);
        if (key != null)
        {
          str4 = (string)key.GetValue("Path", string.Empty);
        }
        else
        {
          str4 = folderPath + @"\ANTEC\VFD";
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", false) != null)
      {
        flag2 = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iMON.exe", false);
        if (key != null)
        {
          str5 = (string)key.GetValue("Path", string.Empty);
        }
        else
        {
          str5 = folderPath + @"\SoundGraph\iMON";
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (flag & !flag2)
      {
        str = str4 + @"\sg_rc.dll";
        if (File.Exists(str))
        {
          str2 = str;
        }
      }
      else if (!flag & flag2)
      {
        str = str5 + @"\sg_rc.dll";
        if (File.Exists(str))
        {
          str2 = str;
        }
      }
      else
      {
        str = str4 + @"\sg_rc.dll";
        if (File.Exists(str))
        {
          str2 = str;
        }
        else
        {
          str = str5 + @"\sg_rc.dll";
          if (File.Exists(str))
          {
            str2 = str;
          }
        }
      }
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.FindImonRCdll(): selected file \"{0}\".", new object[] { str2 });
      }
      return str2;
    }

    public bool iMONLCD_SendData(ref ulong bitMap)
    {
      if (_UseV3DLL)
      {
        return false;
      }
      if (_A_DLL)
      {
        return _A_iMONLCD_SendData(ref bitMap);
      }
      if (_S_DLL)
      {
        return _S_iMONLCD_SendData(ref bitMap);
      }
      return _iMONLCD_SendData(ref bitMap);
    }

    public bool iMONRC_ChangeRC6(int RC_MODE)
    {
      if (this._iMONDLL == null)
      {
        return false;
      }
      try
      {
        return (bool)this._iMONDLL.InvokeMember("iMONRC_ChangeRC6", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { RC_MODE });
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_ChangeRC6(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public bool iMONRC_ChangeRCSet(int RC_SET)
    {
      if (this._iMONDLL == null)
      {
        return false;
      }
      try
      {
        return (bool)this._iMONDLL.InvokeMember("iMONRC_ChangeiMONRCSet", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { RC_SET });
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_ChangeiMONRCSet(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public int iMONRC_CheckDriverVersion()
    {
      if (this._iMONDLL == null)
      {
        return -1;
      }
      try
      {
        return (int)this._iMONDLL.InvokeMember("iMONRC_CheckDriverVersion", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_CheckDriverVersion(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public int iMONRC_GetFirmwareVer()
    {
      if (this._iMONDLL == null)
      {
        return -1;
      }
      try
      {
        return (int)this._iMONDLL.InvokeMember("iMONRC_GetFirmwareVer", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetFirmwareVer(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public int iMONRC_GetHWType()
    {
      if (this._iMONDLL == null)
      {
        return -1;
      }
      try
      {
        return (int)this._iMONDLL.InvokeMember("iMONRC_GetHWType", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetHWType(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public int iMONRC_GetLastRFMode()
    {
      if (this._iMONDLL == null)
      {
        return -1;
      }
      try
      {
        return (int)this._iMONDLL.InvokeMember("iMONRC_GetLastRFMode", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetLastRFMode(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public bool iMONRC_GetPacket(ref byte[] Buffer, ref int BufferSize)
    {
      if (this._iMONDLL == null)
      {
        return false;
      }
      try
      {
        bool flag = (bool)this._iMONDLL.InvokeMember("iMONRC_GetPacket", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { Buffer, (int)BufferSize });
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONRC_GetPacket(): Returning: {0}", new object[] { flag });
        }
        return flag;
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetPacket(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public bool iMONRC_Init(int RC_SET, int RC_TYPE, int RC_RESERVED)
    {
      if (this._iMONDLL == null)
      {
        return false;
      }
      try
      {
        bool flag = (bool)this._iMONDLL.InvokeMember("iMONRC_Init", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { RC_SET, RC_TYPE, RC_RESERVED });
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONRC_Init(): Returning: {0}", new object[] { flag });
        }
        return flag;
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_Init(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public bool iMONRC_IsInited()
    {
      if (this._iMONDLL == null)
      {
        return false;
      }
      try
      {
        return (bool)this._iMONDLL.InvokeMember("iMONRC_IsInited", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_IsInited(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public void iMONRC_Uninit()
    {
      if (this._iMONDLL != null)
      {
        try
        {
          this._iMONDLL.InvokeMember("iMONRC_Uninit", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
        } catch (Exception exception)
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONRC_Uninit(): Caught exception: {0}", new object[] { exception });
        }
      }
    }

    public bool iMONVFD_Init(int VFD_Type, int VFD_Reserved)
    {
      int num = 0;
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with iMONVFD_Init({0},{1})", new object[] { VFD_Type.ToString("x00"), VFD_Reserved.ToString("x00") });
      }
      bool flag = false;
      while ((num < 10) && !flag)
      {
        if (!flag && (num > 0))
        {
          if (_A_DLL)
          {
            _A_iMONVFD_Uninit();
          }
          else if (_S_DLL)
          {
            _S_iMONVFD_Uninit();
          }
          else
          {
            _iMONVFD_Uninit();
          }
          Thread.Sleep(150);
          if (VFD_Reserved == 0)
          {
            VFD_Reserved = 0x8888;
          }
          else
          {
            VFD_Reserved = 0;
          }
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Open failed - retrying...", new object[0]);
          }
        }
        if (_A_DLL)
        {
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with Installed Antec DLL", new object[0]);
          }
          flag = _A_iMONVFD_Init(VFD_Type, VFD_Reserved);
        }
        else if (_S_DLL)
        {
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with Installed SoundGraph DLL", new object[0]);
          }
          flag = _S_iMONVFD_Init(VFD_Type, VFD_Reserved);
        }
        else
        {
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with default DLL", new object[0]);
          }
          flag = _iMONVFD_Init(VFD_Type, VFD_Reserved);
        }
        num++;
      }
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Display Open Returned: {0}", new object[] { flag });
      }
      return flag;
    }

    public bool iMONVFD_Init_OLD(int VFD_Type, int VFD_Reserved)
    {
      if (_A_DLL || _S_DLL)
      {
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with Installed DLL", new object[0]);
        }
      }
      else if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with default DLL", new object[0]);
      }
      if (_A_DLL)
      {
        return _A_iMONVFD_Init(VFD_Type, VFD_Reserved);
      }
      if (_S_DLL)
      {
        return _S_iMONVFD_Init(VFD_Type, VFD_Reserved);
      }
      return _iMONVFD_Init(VFD_Type, VFD_Reserved);
    }

    public bool iMONVFD_IsInited()
    {
      if (_A_DLL)
      {
        return _A_iMONVFD_IsInited();
      }
      if (_S_DLL)
      {
        return _S_iMONVFD_IsInited();
      }
      return _iMONVFD_IsInited();
    }

    public bool iMONVFD_SetEQ(int[] arEQValue)
    {
      if (_A_DLL)
      {
        return _A_iMONVFD_SetEQ(arEQValue);
      }
      if (_S_DLL)
      {
        return _S_iMONVFD_SetEQ(arEQValue);
      }
      return _iMONVFD_SetEQ(arEQValue);
    }

    public bool iMONVFD_SetText(string firstLine, string secondLine)
    {
      if (_A_DLL)
      {
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_SetText(): Calling SetText() from Antec DLL", new object[0]);
        }
        return _A_iMONVFD_SetText(firstLine, secondLine);
      }
      if (_S_DLL)
      {
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_SetText(): Calling SetText() from SoundGraph DLL", new object[0]);
        }
        return _S_iMONVFD_SetText(firstLine, secondLine);
      }
      if (DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.iMONVFD_SetText(): Calling SetText() from Default (V3) DLL", new object[0]);
      }
      return _iMONVFD_SetText(firstLine, secondLine);
    }

    public void iMONVFD_Uninit()
    {
      if (_A_DLL)
      {
        _A_iMONVFD_Uninit();
      }
      if (_S_DLL)
      {
        _S_iMONVFD_Uninit();
      }
      _iMONVFD_Uninit();
    }

    public bool Initialize(string DLLFullPath)
    {
      try
      {
        string directoryName;
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): called", new object[0]);
        }
        bool flag = false;
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Attempting to determine DLL source", new object[0]);
        }
        if (_UseV3DLL | (DLLFullPath == string.Empty))
        {
          _A_DLL = false;
          _S_DLL = false;
          if (_UseV3DLL && DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Advanced options forces V3 DLL", new object[0]);
          }
          if ((DLLFullPath == string.Empty) && DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Installed DLL not found - using DLL supplied with MediaPortal", new object[0]);
          }
        }
        else if (DLLFullPath.ToLower().Contains("antec"))
        {
          _A_DLL = true;
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Found Antec installed DLL - Version {0}", new object[] { FileVersionInfo.GetVersionInfo(DLLFullPath).FileVersion });
          }
        }
        else if (DLLFullPath.ToLower().Contains("soundgraph"))
        {
          _S_DLL = true;
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Found SoundGraph installed DLL - Version {0}", new object[] { FileVersionInfo.GetVersionInfo(DLLFullPath).FileVersion });
          }
        }
        string currentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string environmentVariable = Environment.GetEnvironmentVariable("Path");
        if (_UseV3DLL | (DLLFullPath == string.Empty))
        {
          directoryName = Config.GetFolder(Config.Dir.Base) + @"\sg_vfd.dll";
        }
        else
        {
          directoryName = new FileInfo(DLLFullPath).DirectoryName;
        }
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Forcing OS Search Path to: {0}", new object[] { directoryName });
        }
        Environment.SetEnvironmentVariable("Path", directoryName);
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Attempting to link SG_VFD DLL", new object[0]);
        }
        try
        {
          this.iMONVFD_IsInited();
          flag = true;
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): DLL linking completed", new object[0]);
          }
        } catch
        {
          if (DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): DLL linking failed", new object[0]);
          }
          flag = false;
        }
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): Restoring OS Search Path to: {0}", new object[] { environmentVariable });
        }
        Environment.SetEnvironmentVariable("Path", environmentVariable);
        Environment.CurrentDirectory = currentDirectory;
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): completed", new object[0]);
        }
        return flag;
      } catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.iMONDisplay.Initialize(): CAUGHT EXCEPTION: {0}", new object[] { exception });
        return false;
      }
    }

    public bool LCD3R_SetLCDData2(ref ulong bitMap)
    {
      if (_UseV3DLL)
      {
        return false;
      }
      if (_A_DLL)
      {
        return false;
      }
      return (_S_DLL && _S_LCD3R_SetLCDData2(ref bitMap));
    }

    public bool RC_Available()
    {
      if (this._iMONDLL == null)
      {
        return false;
      }
      return true;
    }

    public bool VFD_UseV3DLL
    {
      get
      {
        return _UseV3DLL;
      }
      set
      {
        _UseV3DLL = value;
      }
    }

    public enum iMON_Remote_Type
    {
      AnyCall = 0x68,
      AutoCAN = 0x76,
      AutoCAN_II = 120,
      DIGN_iMON = 0x6a,
      Enlight = 0x75,
      iMON_2_4G = 0x74,
      iMON_EX = 0x70,
      iMON_MM = 0x6b,
      iMON_PAD = 0x73,
      iMON_RC = 0x65,
      iMON_RSC = 0x66,
      iMON_WT = 0x6f,
      KLOSS_iMON = 110,
      laser_pointer = 0x67,
      LG = 0x6c,
      LLUON_iMON = 0x6d,
      MCE_Remote = 0x77,
      Philips = 0x72,
      TG_Advent = 0x71,
      TG_iMON = 0x69
    }
  }
}

