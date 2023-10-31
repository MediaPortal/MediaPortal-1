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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using Microsoft.Win32;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONDisplay
  {
    private static readonly bool _DoDebug = (Assembly.GetEntryAssembly().FullName.Contains("Configuration") |
                                            Settings.Instance.ExtensiveLogging);

    private static bool _A_DLL;
    private static bool _S_DLL;
    private static bool _SU_DLL;
    private static bool _UseV3DLL;

    private static ModuleBuilder _s_mb;
    private Type _iMONDLL;
    private string _imonRC_DLLFile;

    private static bool _Initialized = false;

    internal iMONDisplay()
    {
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay constructor: called");
      }
      if (_iMONDLL == null)
      {
        CreateImonDLLWrapper();
      }
      _UseV3DLL = iMONLCDg.AdvancedSettings.Load().VFD_UseV3DLL;
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay constructor: UseV3DLL option set '{0}' for display", new object[] { _UseV3DLL });
      }
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay constructor: completed");
      }
    }

    public bool VFD_UseV3DLL
    {
      get { return _UseV3DLL; }
      set { _UseV3DLL = value; }
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


    private static bool _S_iMONLCD_SendData(ref ulong bitMap)
    {
      if (IntPtr.Size == 8)
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonSendData, bitMap) == 1;
      else if (_SU_DLL)
        return _S_SG_VFDU_iMONLCD_SendData(ref bitMap);
      else
        return _S_SG_VFD_iMONLCD_SendData(ref bitMap);
    }

    private static bool _S_iMONVFD_Init(int vfdType, int resevered)
    {
      if (IntPtr.Size == 8)
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonInit, vfdType, resevered) == 1;
      else if (_SU_DLL)
        return _S_SG_VFDU_iMONVFD_Init(vfdType, resevered);
      else
        return _S_SG_VFD_iMONVFD_Init(vfdType, resevered);
    }

    private static bool _S_iMONVFD_IsInited()
    {
      if (IntPtr.Size == 8)
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonIsInited) == 1;
      else if (_SU_DLL)
        return _S_SG_VFDU_iMONVFD_IsInited();
      else
        return _S_SG_VFD_iMONVFD_IsInited();
    }

    private static bool _S_iMONVFD_SetEQ(int[] arEQValue)
    {
      if (IntPtr.Size == 8)
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonSetEQ, arEQValue) == 1;
      else if (_SU_DLL)
        return _S_SG_VFDU_iMONVFD_SetEQ(arEQValue);
      else
        return _S_SG_VFD_iMONVFD_SetEQ(arEQValue);
    }

    private static bool _S_iMONVFD_SetText(string firstLine, string secondLine)
    {
      if (IntPtr.Size == 8)
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute("iMONDisplay", "_S_iMONVFD_SetText",
          firstLine == null ? string.Empty : firstLine,
          secondLine == null ? string.Empty : secondLine
          ) == 1;
      else if (_SU_DLL)
        return _S_SG_VFDU_iMONVFD_SetText(firstLine, secondLine);
      else
        return _S_SG_VFD_iMONVFD_SetText(firstLine, secondLine);
    }

    private static void _S_iMONVFD_Uninit()
    {
      if (IntPtr.Size == 8)
        MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonUninit);
      else if (_SU_DLL)
        _S_SG_VFDU_iMONVFD_Uninit();
      else
        _S_SG_VFD_iMONVFD_Uninit();
    }

    private static bool _S_LCD3R_SetLCDData2(ref ulong bitMap)
    {
      if (IntPtr.Size == 8)
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonSetLCDData2, bitMap) == 1;
      else if (_SU_DLL)
        return _S_SG_VFDU_LCD3R_SetLCDData2(ref bitMap);
      else
        return _S_SG_VFD_LCD3R_SetLCDData2(ref bitMap);
    }

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONLCD_SendData", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFDU_iMONLCD_SendData(ref ulong bitMap);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_Init", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFDU_iMONVFD_Init(int vfdType, int resevered);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_IsInited", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFDU_iMONVFD_IsInited();

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_SetEQ", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFDU_iMONVFD_SetEQ(int[] arEQValue);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_SetText", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFDU_iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_Uninit", CallingConvention = CallingConvention.Cdecl)]
    private static extern void _S_SG_VFDU_iMONVFD_Uninit();

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "LCD3R_SetLCDData2", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFDU_LCD3R_SetLCDData2(ref ulong bitMap);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONLCD_SendData", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFD_iMONLCD_SendData(ref ulong bitMap);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_Init", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFD_iMONVFD_Init(int vfdType, int resevered);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFD_iMONVFD_IsInited();

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFD_iMONVFD_SetEQ(int[] arEQValue);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_SetText", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFD_iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit", CallingConvention = CallingConvention.Cdecl)]
    private static extern void _S_SG_VFD_iMONVFD_Uninit();

    [DllImport(@"..\..\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "LCD3R_SetLCDData2", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool _S_SG_VFD_LCD3R_SetLCDData2(ref ulong bitMap);

    private void CreateImonDLLWrapper()
    {
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): called");
      }
      new FileInfo(Assembly.GetEntryAssembly().Location);
      _imonRC_DLLFile = FindImonRCdll();
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): using RC DLL {1}", new object[] { _imonRC_DLLFile });
      }
      if (_imonRC_DLLFile == string.Empty)
      {
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): FAILED - SG_RC.dll not found");
        }
        return;
      }

      if (IntPtr.Size == 8)
        return;

      try
      {
        if (_s_mb == null)
        {
          AssemblyName name = new AssemblyName
          {
            Name = ("iMONDLLWrapper" + Guid.NewGuid().ToString("N"))
          };
          _s_mb =
            AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule(
              "iMON_DLL_wrapper");
        }
        TypeBuilder builder2 = _s_mb.DefineType("iMONDLLWrapper" + Guid.NewGuid().ToString("N"));
        MethodBuilder builder3 = builder2.DefinePInvokeMethod("iMONRC_Init", _imonRC_DLLFile,
                                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                              MethodAttributes.Static | MethodAttributes.Public,
                                                              CallingConventions.Standard, typeof(bool),
                                                              new[] { typeof(int), typeof(int), typeof(int) },
                                                              CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_Uninit", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(void), null,
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_IsInited", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(bool), null,
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetHWType", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(int), null,
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetFirmwareVer", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(int), null,
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_CheckDriverVersion", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(int), null,
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_ChangeiMONRCSet", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(bool), new[] { typeof(int) },
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_ChangeRC6", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(bool), new[] { typeof(int) },
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetLastRFMode", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(int), null,
                                                CallingConvention.StdCall, CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        builder3 = builder2.DefinePInvokeMethod("iMONRC_GetPacket", _imonRC_DLLFile,
                                                MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                MethodAttributes.Static | MethodAttributes.Public,
                                                CallingConventions.Standard, typeof(bool),
                                                new[] { typeof(byte[]), typeof(int) }, CallingConvention.StdCall,
                                                CharSet.Auto);
        builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
        _iMONDLL = builder2.CreateType();
      }
      catch (Exception exception)
      {
        Log.Error("iMONLCDg.iMONDisplay.CreateImonDLLwrapper(): caught exception: {0}", new object[] { exception });
        return;
      }
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.CreateImonDLLWrapper(): Completed - RC DLL wrapper created.");
      }
      return;
    }

    public string FindImonRCdll()
    {
      RegistryKey key;
      string strPathDll;
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.FindImonRCdll(): called.");
      }
      bool bAntec = false;
      bool bSoundgraph = false;
      string strResult = string.Empty;
      string strFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string strPathAntec = string.Empty;
      string strPathSoundgraph = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false) != null)
      {
        bAntec = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VFD.exe", false);
        if (key != null)
        {
          strPathAntec = (string)key.GetValue("Path", string.Empty);
        }
        else
        {
          strPathAntec = strFolderPath + @"\ANTEC\VFD";
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", false) != null)
      {
        bSoundgraph = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iMON.exe", false);
        if (key != null)
        {
          strPathSoundgraph = (string)key.GetValue("Path", string.Empty);
        }
        else
        {
          strPathSoundgraph = strFolderPath + @"\SoundGraph\iMON";
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (bAntec & !bSoundgraph)
      {
        strPathDll = strPathAntec + @"\sg_rc.dll";
        if (File.Exists(strPathDll))
        {
          strResult = strPathDll;
        }
      }
      else if (!bAntec & bSoundgraph)
      {
        strPathDll = strPathSoundgraph + @"\sg_rcu.dll";
        if (File.Exists(strPathDll))
        {
          strResult = strPathDll;
        }
        else
        {
          strPathDll = strPathSoundgraph + @"\sg_rc.dll";
          if (File.Exists(strPathDll))
          {
            strResult = strPathDll;
          }
        }
      }
      else
      {
        strPathDll = strPathAntec + @"\sg_rc.dll";
        if (File.Exists(strPathDll))
        {
          strResult = strPathDll;
        }
        else
        {
          strPathDll = strPathSoundgraph + @"\sg_rc.dll";
          if (File.Exists(strPathDll))
          {
            strResult = strPathDll;
          }
        }
      }
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.FindImonRCdll(): selected file \"{0}\".", new object[] { strResult });
      }
      return strResult;
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

    public bool iMONLCD_SendData(ulong[] data)
    {
      if (_UseV3DLL)
      {
        return false;
      }
      if (_A_DLL)
      {
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonSendDataBuffer, data) == 1;
      }
      if (_S_DLL)
      {
        return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonSendDataBuffer, data) == 1;
      }
      return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonSendDataBuffer, data) == 1;
    }

    public bool iMONRC_ChangeRC6(int iRcMode)
    {
      if (_iMONDLL == null)
      {
        return false;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
            MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCChangeRC6, iRcMode) == 1;
        else
          return (bool)_iMONDLL.InvokeMember("iMONRC_ChangeRC6",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                new object[] { iRcMode });
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_ChangeRC6(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public bool iMONRC_ChangeRCSet(int iRcSet)
    {
      if (_iMONDLL == null)
      {
        return false;
      }
      try
      {
        if (IntPtr.Size == 8)
            return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
            MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCChangeiMONRCSet, iRcSet) == 1;
      else
         return (bool)_iMONDLL.InvokeMember("iMONRC_ChangeiMONRCSet",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                new object[] { iRcSet });
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_ChangeiMONRCSet(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public int iMONRC_CheckDriverVersion()
    {
      if (_iMONDLL == null)
      {
        return -1;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCCheckDriverVersion);
        else
          return (int)_iMONDLL.InvokeMember("iMONRC_CheckDriverVersion",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                null);
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_CheckDriverVersion(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public int iMONRC_GetFirmwareVer()
    {
      if (_iMONDLL == null)
      {
        return -1;
      }
      try
      {
        if (IntPtr.Size == 8)
            return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
            MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCGetFirmwareVer);
        else
          return (int)_iMONDLL.InvokeMember("iMONRC_GetFirmwareVer",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                null);
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetFirmwareVer(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public int iMONRC_GetHWType()
    {
      if (_iMONDLL == null)
      {
        return -1;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCGetHWType);
        else
          return (int)_iMONDLL.InvokeMember("iMONRC_GetHWType",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                null);
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetHWType(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public int iMONRC_GetLastRFMode()
    {
      if (_iMONDLL == null)
      {
        return -1;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCGetLastRFMode);
      else
        return (int)_iMONDLL.InvokeMember("iMONRC_GetLastRFMode",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                null);
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetLastRFMode(): Caught exception: {0}", new object[] { exception });
        return -1;
      }
    }

    public bool iMONRC_GetPacket(ref byte[] buffer, ref int iSize)
    {
      if (_iMONDLL == null)
      {
        return false;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCGetPacket, ref buffer, iSize) == 1;

        var flag = (bool)_iMONDLL.InvokeMember("iMONRC_GetPacket",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                new object[] { buffer, iSize });
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.iMONRC_GetPacket(): Returning: {0}", new object[] { flag });
        }
        return flag;
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_GetPacket(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public bool iMONRC_Init(int iRcSet, int iRcType, int iRcReserved)
    {
      if (_iMONDLL == null)
      {
        return false;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCInit, iRcSet, iRcType, iRcReserved) == 1;

        var flag = (bool)_iMONDLL.InvokeMember("iMONRC_Init",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                new object[] { iRcSet, iRcType, iRcReserved });
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.iMONRC_Init(): Returning: {0}", new object[] { flag });
        }
        return flag;
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_Init(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public bool iMONRC_IsInited()
    {
      if (_iMONDLL == null)
      {
        return false;
      }
      try
      {
        if (IntPtr.Size == 8)
          return MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
          MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCIsInited) == 1;
        else
          return (bool)_iMONDLL.InvokeMember("iMONRC_IsInited",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                null);
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONRC_IsInited(): Caught exception: {0}", new object[] { exception });
        return false;
      }
    }

    public void iMONRC_Uninit()
    {
      if (_iMONDLL != null)
      {
        try
        {
          if (IntPtr.Size == 8)
            MiniDisplayPlugin.Drivers.MPx86ProxyHandler.Instance.Execute(
            MiniDisplayPlugin.Drivers.MPx86ProxyHandler.CommandEnum.ImonRCUninit);
          else
            _iMONDLL.InvokeMember("iMONRC_Uninit",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                null);
        }
        catch (Exception exception)
        {
          Log.Debug("iMONLCDg.iMONDisplay.iMONRC_Uninit(): Caught exception: {0}", new object[] { exception });
        }
      }
    }

    public bool iMONVFD_Init(int iVfdType, int iVfdReserved)
    {
      int num = 0;
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with iMONVFD_Init({0},{1})",
                 new object[] { iVfdType.ToString("x00"), iVfdReserved.ToString("x00") });
      }
      bool flag = false;
      while ((num < 10) && !flag)
      {
        if (num > 0)
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
          iVfdReserved = iVfdReserved == 0 ? 0x8888 : 0;
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Open failed - retrying...");
          }
        }
        if (_A_DLL)
        {
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with Installed Antec DLL");
          }
          flag = _Initialized = _A_iMONVFD_Init(iVfdType, iVfdReserved);
        }
        else if (_S_DLL)
        {
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with Installed SoundGraph DLL");
          }
          flag = _Initialized = _S_iMONVFD_Init(iVfdType, iVfdReserved);
        }
        else
        {
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with default DLL");
          }
          flag = _Initialized = _iMONVFD_Init(iVfdType, iVfdReserved);
        }
        num++;
      }
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.iMONVFD_Init(): Display Open Returned: {0}", new object[] { flag });
      }
      return flag;
    }

    public bool iMONVFD_Init_OLD(int iVdfType, int iVfdReserved)
    {
      if (_A_DLL || _S_DLL)
      {
        if (_DoDebug)
        {
          Log.Debug("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with Installed DLL");
        }
      }
      else if (_DoDebug)
      {
        Log.Debug("iMONLCDg.iMONDisplay.iMONVFD_Init(): Opening Display with default DLL");
      }
      if (_A_DLL)
      {
        return _A_iMONVFD_Init(iVdfType, iVfdReserved);
      }
      if (_S_DLL)
      {
        return _S_iMONVFD_Init(iVdfType, iVfdReserved);
      }
      return _iMONVFD_Init(iVdfType, iVfdReserved);
    }

    public bool iMONVFD_IsInited()
    {
      if (!_Initialized)
        return false;

      if (_A_DLL)
      {
        return _Initialized = _A_iMONVFD_IsInited();
      }
      else if (_S_DLL)
      {
        return _Initialized = _S_iMONVFD_IsInited();
      }
      else
        return _Initialized = _iMONVFD_IsInited();
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
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.iMONVFD_SetText(): Calling SetText() from Antec DLL");
        }
        return _A_iMONVFD_SetText(firstLine, secondLine);
      }
      if (_S_DLL)
      {
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.iMONVFD_SetText(): Calling SetText() from SoundGraph DLL");
        }
        return _S_iMONVFD_SetText(firstLine, secondLine);
      }
      if (_DoDebug)
      {
        Log.Info("iMONLCDg.iMONDisplay.iMONVFD_SetText(): Calling SetText() from Default (V3) DLL");
      }
      return _iMONVFD_SetText(firstLine, secondLine);
    }

    public void iMONVFD_Uninit()
    {
      _Initialized = false;

      if (_A_DLL)
      {
        _A_iMONVFD_Uninit();
      }
      else if (_S_DLL)
      {
        _S_iMONVFD_Uninit();
      }
      else
        _iMONVFD_Uninit();
    }

    public bool Initialize(string strDLLFullPath)
    {
      try
      {
        string strDirectoryName;
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.Initialize(): called");
        }
        bool bResult;
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.Initialize(): Attempting to determine DLL source");
        }
        if (_UseV3DLL | (strDLLFullPath == string.Empty))
        {
          _A_DLL = false;
          _S_DLL = false;
          if (_UseV3DLL && _DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.Initialize(): Advanced options forces V3 DLL");
          }
          if ((strDLLFullPath == string.Empty) && _DoDebug)
          {
            Log.Info(
              "iMONLCDg.iMONDisplay.Initialize(): Installed DLL not found - using DLL supplied with MediaPortal",
              new object[0]);
          }
        }
        else if (strDLLFullPath.IndexOf("antec", StringComparison.CurrentCultureIgnoreCase) >= 0)
        {
          _A_DLL = true;
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.Initialize(): Found Antec installed DLL - Version {0}",
                     new object[] { FileVersionInfo.GetVersionInfo(strDLLFullPath).FileVersion });
          }
        }
        else if (strDLLFullPath.IndexOf("soundgraph", StringComparison.CurrentCultureIgnoreCase) >= 0)
        {
          _S_DLL = true;
          _SU_DLL = strDLLFullPath.EndsWith("sg_vfdu.dll", StringComparison.CurrentCultureIgnoreCase);

          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.Initialize(): Found SoundGraph installed DLL - Version {0}",
                     new object[] { FileVersionInfo.GetVersionInfo(strDLLFullPath).FileVersion });
          }
        }
        string strCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string strEnvironmentVariable = Environment.GetEnvironmentVariable("Path");
        if (_UseV3DLL | (strDLLFullPath == string.Empty))
        {
          strDirectoryName = Config.GetFolder(Config.Dir.Base) + @"\sg_vfd.dll";
        }
        else
        {
          strDirectoryName = new FileInfo(strDLLFullPath).DirectoryName;
        }
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.Initialize(): Forcing OS Search Path to: {0}", new object[] { strDirectoryName });
        }
        Environment.SetEnvironmentVariable("Path", strDirectoryName);
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.Initialize(): Attempting to link SG_VFD DLL");
        }
        try
        {
          iMONVFD_IsInited();
          bResult = true;
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.Initialize(): DLL linking completed");
          }
        }
        catch
        {
          if (_DoDebug)
          {
            Log.Info("iMONLCDg.iMONDisplay.Initialize(): DLL linking failed");
          }
          bResult = false;
        }
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.Initialize(): Restoring OS Search Path to: {0}",
                   new object[] { strEnvironmentVariable });
        }
        Environment.SetEnvironmentVariable("Path", strEnvironmentVariable);
        Environment.CurrentDirectory = strCurrentDirectory;
        if (_DoDebug)
        {
          Log.Info("iMONLCDg.iMONDisplay.Initialize(): completed");
        }
        return bResult;
      }
      catch (Exception exception)
      {
        Log.Info("iMONLCDg.iMONDisplay.Initialize(): CAUGHT EXCEPTION: {0}", new object[] { exception });
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
      if (_iMONDLL == null)
      {
        return false;
      }
      return true;
    }
  }
}