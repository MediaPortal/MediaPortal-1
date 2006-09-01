using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MediaPortal.ControlDevices.USBUIRT
{
  public class USBUIRTAPI
  {
    #region delegates
    public delegate void StartLearningEventHandler(object sender, LearningEventArgs e);
    public delegate void EventLearnedHandler(object sender, LearningEventArgs e);
    public delegate void EndLearnedHandler(object sender, EventArgs e);

    public delegate void RemoteCommandFeedbackHandler(object command, string irCode);
    public delegate void UUIRTReceiveCallbackDelegate(string val, IntPtr reserved);

    public delegate void IRLearnCallbackDelegate(uint val, uint val2, ulong val3);
    public delegate void OnRemoteCommand(object command);

    public delegate void ThreadSafeSendMessageDelegate(int wmMsg, int wparam, int lparam);

    #endregion

    public static IntPtr UUPTR_EMPTY = new IntPtr(-1);
    public static IntPtr UUPTR_NULL = IntPtr.Zero;

    [StructLayout(LayoutKind.Sequential)]
    public struct UUINFO
    {
      public int fwVersion;
      public int protVersion;
      public char fwDateDay;
      public char fwDateMonth;
      public char fwDateYear;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UUGPIO
    {
      byte[] irCode;
      byte action;
      byte duration;
    }

    [DllImport("uuirtdrv.dll")]
    static public extern IntPtr UUIRTOpen();

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTClose(IntPtr hHandle);

    [DllImport("uuirtdrv.dll")]
    static extern bool UUIRTGetDrvInfo(ref int puDrvVersion);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTGetUUIRTInfo(IntPtr hHandle, ref UUINFO puuInfo);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTGetUUIRTConfig(IntPtr hHandle, ref uint puConfig);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTSetUUIRTConfig(IntPtr hHandle, uint uConfig);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTTransmitIR(IntPtr hHandle, string IRCode, int codeFormat, int repeatCount, int inactivityWaitTime, IntPtr hEvent, int res1, int res2);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTLearnIR(IntPtr hHandle, int codeFormat, [MarshalAs(UnmanagedType.LPStr)] StringBuilder ircode, IRLearnCallbackDelegate progressProc, int userData, ref int pAbort, int param1, [MarshalAs(UnmanagedType.AsAny)] Object o, [MarshalAs(UnmanagedType.AsAny)] Object oo);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTSetReceiveCallback(IntPtr hHandle, UUIRTReceiveCallbackDelegate receiveProc, int none);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTSetUUIRTGPIOCfg(IntPtr hHandle, int index, ref UUGPIO GpioSt);
    //HUUHANDLE	  hHandle, int index, PUUGPIO pGpioSt);

    [DllImport("uuirtdrv.dll")]
    static public extern bool UUIRTGetUUIRTGPIOCfg(IntPtr hHandle, ref int numSlots, ref uint dwPortPins, ref UUGPIO GpioSt);
    //(HUUHANDLE hHandle, int *pNumSlots, UINT32 *pdwPortPins, PUUGPIO pGPIOStruct);
  }
}
