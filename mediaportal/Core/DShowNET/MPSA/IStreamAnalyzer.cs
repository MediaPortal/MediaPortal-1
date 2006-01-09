using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.MPSA
{


  //IMPDST
  [ComVisible(true), ComImport,
  Guid("FB1EF498-2C7D-4fed-B2AA-B8F9E199F074"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IStreamAnalyzer
  {
    [PreserveSig]
    int put_MediaType(IntPtr pmt);

    [PreserveSig]
    int get_MediaType(IntPtr pmt);

    [PreserveSig]
    int get_IPin(IntPtr pPin);

    [PreserveSig]
    int get_State(IntPtr state);

    [PreserveSig]
    int GetChannelCount(ref UInt16 count);

    [PreserveSig]
    int GetChannel(UInt16 chNumber, IntPtr ptr);

    [PreserveSig]
    int GetCISize(ref UInt16 len);

    [PreserveSig]
    int ResetParser();

    [PreserveSig]
    int ResetPids();

    [PreserveSig]
    int SetPMTProgramNumber(int prg);

    [PreserveSig]
    int GetPMTData(IntPtr pmt);

    [PreserveSig]
    int IsChannelReady(int chNum);

    [PreserveSig]
    int UseATSC(byte yesNo);

    [PreserveSig]
    int IsATSCUsed(out bool yesNo);

    [PreserveSig]
    int GetLCN(Int16 channel, out Int16 networkId, out Int16 transportId, out Int16 serviceID, out Int16 LCN);
  }

}
