using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace MPx86Proxy.Drivers
{
  public class iMONDisplay
  {
    private enum ModeEnum
    {
      Unknown = 0,
      Default,
      Antec,
      SoundGraphVFD,
      SoundGraphVFDU
    }

    private static ModeEnum _Mode = ModeEnum.Default;

    #region Antec: C:\Program Files (x86)\VFD\SG_VFD.dll
    [DllImport(@"C:\Program Files (x86)\VFD\SG_VFD.dll", EntryPoint = "iMONLCD_SendData", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool A_iMONLCD_SendData(ref ulong bitMap);

    [DllImport(@"C:\Program Files (x86)\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool A_iMONVFD_Init(int vfdType, int resevered);

    [DllImport(@"C:\Program Files (x86)\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool A_iMONVFD_IsInited();

    [DllImport(@"C:\Program Files (x86)\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool A_iMONVFD_SetEQ(int[] arEQValue);

    [DllImport(@"C:\Program Files (x86)\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_SetText", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool A_iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport(@"C:\Program Files (x86)\VFD\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void A_iMONVFD_Uninit();
    #endregion

    #region .\SG_VFD.dll
    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_SendData", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool iMONVFD_SendData(ref ulong bitMap);

    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool iMONVFD_Init(int vfdType, int resevered);

    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool iMONVFD_IsInited();

    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool iMONVFD_SetEQ(int[] arEQValue);

    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_SetText", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport(@".\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void iMONVFD_Uninit();
    #endregion

    #region SoundGraph: C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll
    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONLCD_SendData", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONLCD_SendData(ref ulong bitMap);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONVFD_Init(int vfdType, int resevered);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_IsInited", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONVFD_IsInited();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_SetEQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONVFD_SetEQ(int[] arEQValue);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_SetText", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "iMONVFD_Uninit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void S_iMONVFD_Uninit();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFD.dll", EntryPoint = "LCD3R_SetLCDData2", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_LCD3R_SetLCDData2(ref ulong bitMap);
    #endregion

    #region SoundGraph: C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll
    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONLCD_SendData", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONLCD_SendData(ref ulong bitMap);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONVFD_Init(int vfdType, int resevered);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_IsInited", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONVFD_IsInited();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_SetEQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONVFD_SetEQ(int[] arEQValue);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_SetText", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "iMONVFD_Uninit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SU_iMONVFD_Uninit();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_VFDU.dll", EntryPoint = "LCD3R_SetLCDData2", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_LCD3R_SetLCDData2(ref ulong bitMap);
    #endregion

    #region SoundGraph: C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll
    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONRC_Init(int set, int type, int reserverd);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_Uninit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void S_iMONRC_Uninit();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_IsInited", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONRC_IsInited();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_GetHWType", CallingConvention = CallingConvention.Cdecl)]
    public static extern int S_iMONRC_GetHWType();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_GetFirmwareVer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int S_iMONRC_GetFirmwareVer();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_CheckDriverVersion", CallingConvention = CallingConvention.Cdecl)]
    public static extern int S_iMONRC_CheckDriverVersion();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_ChangeiMONRCSet", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONRC_ChangeiMONRCSet(int set);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_ChangeRC6", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool S_iMONRC_ChangeRC6(int mode);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_GetLastRFMode", CallingConvention = CallingConvention.Cdecl)]
    public static extern int S_iMONRC_GetLastRFMode();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RC.dll", EntryPoint = "iMONRC_GetPacket", CallingConvention = CallingConvention.Cdecl)]
    public unsafe static extern bool S_iMONRC_GetPacket(byte* p, int size);
    #endregion

    #region SoundGraph: C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll
    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONRC_Init(int set, int type, int reserverd);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_Uninit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SU_iMONRC_Uninit();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_IsInited", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONRC_IsInited();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_GetHWType", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SU_iMONRC_GetHWType();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_GetFirmwareVer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SU_iMONRC_GetFirmwareVer();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_CheckDriverVersion", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SU_iMONRC_CheckDriverVersion();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_ChangeiMONRCSet", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONRC_ChangeiMONRCSet(int set);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_ChangeRC6", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SU_iMONRC_ChangeRC6(int mode);

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_GetLastRFMode", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SU_iMONRC_GetLastRFMode();

    [DllImport(@"C:\Program Files (x86)\SoundGraph\iMON\SG_RCU.dll", EntryPoint = "iMONRC_GetPacket", CallingConvention = CallingConvention.Cdecl)]
    public unsafe static extern bool SU_iMONRC_GetPacket(byte* p, int size);
    #endregion


    #region Display methods
    public static bool iMONDisplay_SendData(ref ulong bitMap)
    {
      switch (_Mode)
      {
        case ModeEnum.Default:
          return iMONVFD_SendData(ref bitMap);

        case ModeEnum.Antec:
          return A_iMONLCD_SendData(ref bitMap);

        case ModeEnum.SoundGraphVFD:
          return S_iMONLCD_SendData(ref bitMap);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONLCD_SendData(ref bitMap);

        default:
          return false;
      }
    }

    public static bool iMONDisplay_Init(int vfdType, int resevered)
    {
      Logging.Log.Debug("[iMONDisplay_Init]");

      switch (_Mode)
      {
        case ModeEnum.Default:
          return iMONVFD_Init(vfdType, resevered);

        case ModeEnum.Antec:
          return A_iMONVFD_Init(vfdType, resevered);

        case ModeEnum.SoundGraphVFD:
          return S_iMONVFD_Init(vfdType, resevered);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONVFD_Init(vfdType, resevered);

        default:
          return false;
      }
    }

    public static bool iMONDisplay_IsInited()
    {
      switch (_Mode)
      {
        case ModeEnum.Default:
          return iMONVFD_IsInited();

        case ModeEnum.Antec:
          return A_iMONVFD_IsInited();

        case ModeEnum.SoundGraphVFD:
          return S_iMONVFD_IsInited();

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONVFD_IsInited();

        default:
          return false;
      }
    }

    public static bool iMONDisplay_SetEQ(int[] arEQValue)
    {
      switch (_Mode)
      {
        case ModeEnum.Default:
          return iMONVFD_SetEQ(arEQValue);

        case ModeEnum.Antec:
          return A_iMONVFD_SetEQ(arEQValue);

        case ModeEnum.SoundGraphVFD:
          return S_iMONVFD_SetEQ(arEQValue);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONVFD_SetEQ(arEQValue);

        default:
          return false;
      }
    }

    public static bool iMONDisplay_SetText(string firstLine, string secondLine)
    {
      switch (_Mode)
      {
        case ModeEnum.Default:
          return iMONVFD_SetText(firstLine, secondLine);

        case ModeEnum.Antec:
          return A_iMONVFD_SetText(firstLine, secondLine);

        case ModeEnum.SoundGraphVFD:
          return S_iMONVFD_SetText(firstLine, secondLine);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONVFD_SetText(firstLine, secondLine);

        default:
          return false;
      }
    }

    public static void iMONDisplay_Uninit()
    {
      Logging.Log.Debug("[iMONDisplay_Uninit]");

      switch (_Mode)
      {
        case ModeEnum.Default:
          iMONVFD_Uninit();
          return;

        case ModeEnum.Antec:
          A_iMONVFD_Uninit();
          return;

        case ModeEnum.SoundGraphVFD:
          S_iMONVFD_Uninit();
          return;

        case ModeEnum.SoundGraphVFDU:
          SU_iMONVFD_Uninit();
          return;

        default:
          return;
      }
    }

    public static bool iMONDisplay_LCD3R_SetLCDData2(ref ulong bitMap)
    {
      switch (_Mode)
      {
        case ModeEnum.Default:
          return false;

        case ModeEnum.Antec:
          return false;

        case ModeEnum.SoundGraphVFD:
          return S_LCD3R_SetLCDData2(ref bitMap);

        case ModeEnum.SoundGraphVFDU:
          return SU_LCD3R_SetLCDData2(ref bitMap);

        default:
          return false;
      }
    }

    public static bool iMONDisplay_SendDataBuffer(byte[] data, int iOffset, int iLength)
    {
      ulong l;

      while (iLength-- > 0)
      {
        l = BitConverter.ToUInt64(data, iOffset);

        switch(_Mode)
        {
          case ModeEnum.Default:
            if (!iMONVFD_SendData(ref l))
              return false;
            break;

          case ModeEnum.Antec:
            if (!A_iMONLCD_SendData(ref l))
              return false;
            break;

          case ModeEnum.SoundGraphVFD:
            if (!S_iMONLCD_SendData(ref l))
              return false;
            break;

          case ModeEnum.SoundGraphVFDU:
            if (!SU_iMONLCD_SendData(ref l))
              return false;
            break;

          default:
            return false;
        }

        iOffset += 8;

        //Put some delay after each second call
        //if ((iLength & 1) > 0)
        //    System.Threading.Thread.Sleep(1);
      }
      return true;
    }

    #endregion

    #region RC Methods
    public static bool iMONRC_Init(int set, int type, int reserverd)
    {
      Logging.Log.Debug("[iMONRC_Init]");

      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_Init(set, type, reserverd);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_Init(set, type, reserverd);

        default:
          return false;
      }
    }

    public static void iMONRC_Uninit()
    {
      Logging.Log.Debug("[iMONRC_Uninit]");

      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          S_iMONRC_Uninit();
          return;

        case ModeEnum.SoundGraphVFDU:
          SU_iMONRC_Uninit();
          return;
      }
    }

    public static bool iMONRC_IsInited()
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_IsInited();

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_IsInited();

        default:
          return false;
      }
    }

    public static int iMONRC_GetHWType()
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_GetHWType();

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_GetHWType();

        default:
          return -1;
      }
    }

    public static int iMONRC_GetFirmwareVer()
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_GetFirmwareVer();

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_GetFirmwareVer();

        default:
          return -1;
      }
    }

    public static int iMONRC_CheckDriverVersion()
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_CheckDriverVersion();

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_CheckDriverVersion();

        default:
          return -1;
      }
    }

    public static bool iMONRC_ChangeiMONRCSet(int set)
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_ChangeiMONRCSet(set);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_ChangeiMONRCSet(set);

        default:
          return false;
      }
    }

    public static bool iMONRC_ChangeRC6(int mode)
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_ChangeRC6(mode);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_ChangeRC6(mode);

        default:
          return false;
      }
    }

    public static int iMONRC_GetLastRFMode()
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_GetLastRFMode();

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_GetLastRFMode();

        default:
          return -1;
      }
    }

    public unsafe static bool iMONRC_GetPacket(byte* p, int size)
    {
      switch (_Mode)
      {
        case ModeEnum.SoundGraphVFD:
          return S_iMONRC_GetPacket(p, size);

        case ModeEnum.SoundGraphVFDU:
          return SU_iMONRC_GetPacket(p, size);

        default:
          return false;
      }
    }
    #endregion


    public static void Detect()
    {
      Logging.Log.Debug("[iMONDisplay][Detect] called.");

      RegistryKey key;
      string strPathDll;
      ModeEnum mode = ModeEnum.Default;
      string strFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string strPathAntec = string.Empty;
      string strPathSoundgraph = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false) != null)
      {
        Logging.Log.Debug("[iMONDisplay][Detect] found Antec registry keys.");
        mode = ModeEnum.Antec;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VFD.exe", false);
        if (key != null)
        {
          strPathAntec = (string)key.GetValue("Path", string.Empty);
          if (strPathAntec == string.Empty)
          {
            Logging.Log.Debug("[iMONDisplay][Detect] Antec file Path registry key not found. trying default path");
            strPathAntec = strFolderPath + @"\Antec\VFD";
          }
          else
            Logging.Log.Debug("[iMONDisplay][Detect] found Antec file Path registry key.");
        }
        else
        {
          Logging.Log.Debug("[iMONDisplay][Detect] Antec file Path registry key not found. trying default path");
          strPathAntec = strFolderPath + @"\Antec\VFD";
        }

        Registry.LocalMachine.Close();
      }
      else
        Registry.CurrentUser.Close();

      if (Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", false) != null)
      {
        Logging.Log.Debug("[iMONDisplay][Detect] found SoundGraph registry keys.");

        mode = ModeEnum.SoundGraphVFDU;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iMON.exe", false);
        if (key != null)
        {
          strPathSoundgraph = (string)key.GetValue("Path", string.Empty);
          if (strPathSoundgraph == string.Empty)
          {
            Logging.Log.Debug("[iMONDisplay][Detect] SoundGraph file Path registry key not found. trying default path");
            strPathSoundgraph = strFolderPath + @"\SoundGraph\iMON";
          }
          else
            Logging.Log.Debug("[iMONDisplay][Detect] found SoundGraph file Path registry key.");

        }
        else
        {
          Logging.Log.Debug("[iMONDisplay][Detect] SoundGraph file Path registry key not found. trying default path");
          strPathSoundgraph = strFolderPath + @"\Antec\VFD";
        }
        Registry.LocalMachine.Close();
      }
      else
        Registry.CurrentUser.Close();

      if (mode == ModeEnum.Antec)
      {
        strPathDll = strPathAntec + @"\sg_vfd.dll";
        if (File.Exists(strPathDll))
        {
          Logging.Log.Debug("[iMONDisplay][Detect] Selected Antec DLL.");
          _Mode = mode;
        }
      }
      else if (mode == ModeEnum.SoundGraphVFDU)
      {
        strPathDll = strPathSoundgraph + @"\sg_vfdu.dll";
        if (File.Exists(strPathDll))
        {
          Logging.Log.Debug("[iMONDisplay][Detect] Selected SoundGraph DLL: " + strPathDll);
          _Mode = mode;
        }
        else
        {
          strPathDll = strPathSoundgraph + @"\sg_vfd.dll";
          if (File.Exists(strPathDll))
          {
            //Older iMon SW
            Logging.Log.Debug("[iMONDisplay][Detect] Selected SoundGraph DLL: " + strPathDll);
            _Mode = ModeEnum.SoundGraphVFD;
          }
          else
            _Mode = ModeEnum.Unknown;
        }
      }
      else
      {
        strPathDll = strPathAntec + @"\sg_vfd.dll";
        if (File.Exists(strPathDll))
        {
          Logging.Log.Debug("[iMONDisplay][Detect] Picked Antec DLL: " + strPathDll);
          _Mode = mode;
        }
        else
        {
          strPathDll = strPathSoundgraph + @"\sg_vfd.dll";
          if (File.Exists(strPathDll))
          {
            Logging.Log.Debug("[iMONDisplay][Detect] Picked Soundgraph DLL: " + strPathDll);
            _Mode = ModeEnum.Unknown;
          }
        }
      }

      Logging.Log.Debug("[iMONDisplay][Detect] completed - selected mode: {0}", _Mode);
    }
  }
}
