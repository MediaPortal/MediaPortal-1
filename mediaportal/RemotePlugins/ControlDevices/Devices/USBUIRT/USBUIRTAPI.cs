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
using System.Runtime.InteropServices;
using System.Text;

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
      private byte[] irCode;
      private byte action;
      private byte duration;
    }

    [DllImport("uuirtdrv.dll")]
    public static extern IntPtr UUIRTOpen();

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTClose(IntPtr hHandle);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTGetDrvInfo(ref int puDrvVersion);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTGetUUIRTInfo(IntPtr hHandle, ref UUINFO puuInfo);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTGetUUIRTConfig(IntPtr hHandle, ref uint puConfig);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTSetUUIRTConfig(IntPtr hHandle, uint uConfig);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTTransmitIR(IntPtr hHandle, string IRCode, int codeFormat, int repeatCount,
                                              int inactivityWaitTime, IntPtr hEvent, int res1, int res2);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTLearnIR(IntPtr hHandle, int codeFormat,
                                           [MarshalAs(UnmanagedType.LPStr)] StringBuilder ircode,
                                           IRLearnCallbackDelegate progressProc, int userData, ref int pAbort,
                                           int param1, [MarshalAs(UnmanagedType.AsAny)] Object o,
                                           [MarshalAs(UnmanagedType.AsAny)] Object oo);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTSetReceiveCallback(IntPtr hHandle, UUIRTReceiveCallbackDelegate receiveProc, int none);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTSetUUIRTGPIOCfg(IntPtr hHandle, int index, ref UUGPIO GpioSt);

    //HUUHANDLE	  hHandle, int index, PUUGPIO pGpioSt);

    [DllImport("uuirtdrv.dll")]
    public static extern bool UUIRTGetUUIRTGPIOCfg(IntPtr hHandle, ref int numSlots, ref uint dwPortPins,
                                                   ref UUGPIO GpioSt);

    //(HUUHANDLE hHandle, int *pNumSlots, UINT32 *pdwPortPins, PUUGPIO pGPIOStruct);
  }
}