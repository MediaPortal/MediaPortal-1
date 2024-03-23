using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MPx86Proxy.Drivers
{
  public class iMONDisplayWrapper
  {
    //Possible return values from iMON Display APIs
    public enum DSPResult : int
    {
      DSP_SUCCEEDED = 0,
      DSP_E_FAIL = 1,
      DSP_E_OUTOFMEMORY = 2,
      DSP_E_INVALIDARG = 3,
      DSP_E_NOT_INITED = 4,
      DSP_E_POINTER = 5,
      DSP_S_INITED = 0x1000,
      DSP_S_NOT_INITED = 0x1001,
      DSP_S_IN_PLUGIN_MODE = 0x1002,
      DSP_S_NOT_IN_PLUGIN_MODE = 0x1003
    }

    public enum ImonDisplayWrapperCommandEnum
    {
      Init = 0,
      UnInit,
      IsInitialized,
      GetStatus,
      SetVfdText,
      SetVfdEqData,
      SetLcdEqData,
      SetLcdText,
      SetLcdAllIcons,
      SetLcdOrangeIcon,
      SetLcdMediaTypeIcon,
      SetLcdSpeakerIcon,
      SetLcdVideoCodecIcon,
      SetLcdAudioCodecIcon,
      SetLcdAspectRatioIcon,
      SetLcdEtcIcon,
      SetLcdProgress
    }



    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_Init(IntPtr result);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_Uninit();

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_IsInitialized();

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_GetStatus(IntPtr status);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetVfdText(IntPtr line1, IntPtr line2);

    //Import function to set VFD EQDATA
    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern int IDW_SetVfdEqData(IntPtr eqData);

    //Import function to set LCD EQDATA
    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern int IDW_SetLcdEqData(IntPtr eqDataLeft, IntPtr eqDataRight);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdText(IntPtr line);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdAllIcons([MarshalAs(UnmanagedType.Bool)] bool on);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdOrangeIcon(byte iconData1, byte iconData2);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdMediaTypeIcon(byte iconData);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdSpeakerIcon(byte iconData1, byte iconData2);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdVideoCodecIcon(byte iconData);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdAudioCodecIcon(byte iconData);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdAspectRatioIcon(byte iconData);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdEtcIcon(byte iconData);

    [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern DSPResult IDW_SetLcdProgress(int currentPosition, int total);


    public static int HandleRequest(ImonDisplayWrapperCommandEnum cmd, byte[] data, int iOffset, int iLength, out int iResponseLength)
    {
      iResponseLength = 0;

      unsafe
      {
        fixed (byte* p = data)
        {
          switch (cmd)
          {
            case ImonDisplayWrapperCommandEnum.Init:
              iResponseLength = iLength;
              return (int)IDW_Init((IntPtr)(p + iOffset));

            case ImonDisplayWrapperCommandEnum.UnInit:
              return (int)IDW_Uninit();

            case ImonDisplayWrapperCommandEnum.IsInitialized:
              return (int)IDW_IsInitialized();

            case ImonDisplayWrapperCommandEnum.GetStatus:
              iResponseLength = iLength;
              return (int)IDW_GetStatus((IntPtr)(p + iOffset));

            case ImonDisplayWrapperCommandEnum.SetVfdText:
              return (int)IDW_SetVfdText((IntPtr)(p + iOffset + 2), (IntPtr)(p + iOffset + BitConverter.ToUInt16(data, iOffset) + 6));

            case ImonDisplayWrapperCommandEnum.SetVfdEqData:
              return (int)IDW_SetVfdEqData((IntPtr)(p + iOffset));

            case ImonDisplayWrapperCommandEnum.SetLcdEqData:
              return (int)IDW_SetLcdEqData((IntPtr)(p + iOffset), (IntPtr)(p + iOffset + (16 * 4)));
            
            case ImonDisplayWrapperCommandEnum.SetLcdText:
              return (int)IDW_SetLcdText((IntPtr)(p + iOffset + 2));

            case ImonDisplayWrapperCommandEnum.SetLcdAllIcons:
              return (int)IDW_SetLcdAllIcons(data[iOffset] != 0);

            case ImonDisplayWrapperCommandEnum.SetLcdOrangeIcon:
              return (int)IDW_SetLcdOrangeIcon(data[iOffset], data[iOffset + 1]);

            case ImonDisplayWrapperCommandEnum.SetLcdMediaTypeIcon:
              return (int)IDW_SetLcdMediaTypeIcon(data[iOffset]);

            case ImonDisplayWrapperCommandEnum.SetLcdSpeakerIcon:
              return (int)IDW_SetLcdSpeakerIcon(data[iOffset], data[iOffset + 1]);

            case ImonDisplayWrapperCommandEnum.SetLcdVideoCodecIcon:
              return (int)IDW_SetLcdVideoCodecIcon(data[iOffset]);

            case ImonDisplayWrapperCommandEnum.SetLcdAudioCodecIcon:
              return (int)IDW_SetLcdAudioCodecIcon(data[iOffset]);

            case ImonDisplayWrapperCommandEnum.SetLcdAspectRatioIcon:
              return (int)IDW_SetLcdAspectRatioIcon(data[iOffset]);

            case ImonDisplayWrapperCommandEnum.SetLcdEtcIcon:
              return (int)IDW_SetLcdEtcIcon(data[iOffset]);

            case ImonDisplayWrapperCommandEnum.SetLcdProgress:
              return (int)IDW_SetLcdProgress(BitConverter.ToInt32(data, iOffset), BitConverter.ToInt32(data, iOffset + 4));

            default:
              return (int)DSPResult.DSP_E_FAIL;
          }

        }
      }
    }
  }
}
