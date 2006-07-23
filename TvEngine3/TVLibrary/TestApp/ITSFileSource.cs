using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DShowNET.TsFileSink
{

  [ComVisible(true), ComImport,
Guid("559E6E81-FAC4-4EBC-9530-662DAA27EDC2"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITSFileSource
  {
    [PreserveSig]
    int GetVideoPid(ref ushort vpid);
    [PreserveSig]
    int GetAudioPid(ref ushort apid);
    [PreserveSig]
    int GetAudio2Pid(ref ushort a2pid);
    [PreserveSig]
    int GetAC3Pid(ref ushort ac3pid);
    [PreserveSig]
    int GetTelexPid(ref ushort telexpid);
    [PreserveSig]
    int GetPMTPid(ref ushort pmtpid);
    [PreserveSig]
    int GetSIDPid(ref ushort sidpid);
    [PreserveSig]
    int GetPCRPid(ref ushort pcrpid);
    [PreserveSig]
    int GetDuration(ref long dur);
    [PreserveSig]
    int GetShortDescr([Out, MarshalAs(UnmanagedType.AnsiBStr)] string shortDesc);
    [PreserveSig]
    int GetExtendedDescr([Out, MarshalAs(UnmanagedType.AnsiBStr)] string extdesc);
    [PreserveSig]
    int GetPgmNumb(ref ushort pPgmNumb);
    [PreserveSig]
    int GetPgmCount(ref ushort pPgmCount);
    [PreserveSig]
    int SetPgmNumb(ushort pPgmNumb);
    [PreserveSig]
    int NextPgmNumb();
    [PreserveSig]
    int GetTsArray([Out, MarshalAs(UnmanagedType.LPArray)] uint[] pPidArray);

    [PreserveSig]
    int GetAC3Mode(ref ushort pAC3Mode);
    [PreserveSig]
    int SetAC3Mode(ushort AC3Mode);

    [PreserveSig]
    int GetMP2Mode(ref ushort pMP2Mode);
    [PreserveSig]
    int SetMP2Mode(ushort MP2Mode);

    [PreserveSig]
    int GetAutoMode(ref ushort pAutoMode);
    [PreserveSig]
    int SetAutoMode(ushort AutoMode);

    [PreserveSig]
    int GetDelayMode(ref ushort pDelayMode);
    [PreserveSig]
    int SetDelayMode(ushort DelayMode);

    [PreserveSig]
    int GetRateControlMode(ref ushort pRateControl);
    [PreserveSig]
    int SetRateControlMode(ushort RateControl);

    [PreserveSig]
    int GetCreateTSPinOnDemux(ref ushort pbCreatePin);
    [PreserveSig]
    int SetCreateTSPinOnDemux(ushort bCreatePin);

    [PreserveSig]
    int GetReadOnly(ref ushort pFileMode);

    [PreserveSig]
    int GetBitRate(ref long pRate);
    [PreserveSig]
    int SetBitRate(long Rate);

    //New interfaces added after 2.0.1.7 official release
    [PreserveSig]
    int GetAC3_2Pid(ref ushort ac3_2pid);

    [PreserveSig]
    int GetNIDPid(ref ushort nidpid);
    [PreserveSig]
    int GetChannelNumber([Out, MarshalAs(UnmanagedType.LPArray)] byte[] pointer);
    [PreserveSig]
    int GetNetworkName([Out, MarshalAs(UnmanagedType.AnsiBStr)] string networkName);

    [PreserveSig]
    int GetONIDPid(ref ushort onidpid);
    [PreserveSig]
    int GetONetworkName([Out, MarshalAs(UnmanagedType.AnsiBStr)] string pointer);
    [PreserveSig]
    int GetChannelName([Out, MarshalAs(UnmanagedType.AnsiBStr)] string pointer);

    [PreserveSig]
    int GetTSIDPid(ref ushort tsidpid);

    [PreserveSig]
    int GetEPGFromFile();
    [PreserveSig]
    int GetShortNextDescr([Out, MarshalAs(UnmanagedType.AnsiBStr)] string shortnextdesc);
    [PreserveSig]
    int GetExtendedNextDescr([Out, MarshalAs(UnmanagedType.AnsiBStr)] string extnextdesc);

    [PreserveSig]
    int PrevPgmNumb();

    [PreserveSig]
    int GetAudio2Mode(ref ushort pAudio2Mode);
    [PreserveSig]
    int SetAudio2Mode(ushort Audio2Mode);

    [PreserveSig]
    int GetNPControl(ref ushort pNPControl);
    [PreserveSig]
    int SetNPControl(ushort pNPControl);

    [PreserveSig]
    int GetNPSlave(ref ushort pNPSlave);
    [PreserveSig]
    int SetNPSlave(ushort pNPSlave);

    [PreserveSig]
    int SetTunerEvent();

    [PreserveSig]
    int SetRegSettings();
    [PreserveSig]
    int GetRegSettings();
    [PreserveSig]
    int SetRegProgram();

    [PreserveSig]
    int ShowFilterProperties();
    [PreserveSig]
    int Refresh();

    [PreserveSig]
    int GetROTMode(ref ushort ROTMode);
    [PreserveSig]
    int SetROTMode(ushort ROTMode);
    [PreserveSig]
    int GetClockMode(ref ushort ClockMode);
    [PreserveSig]
    int SetClockMode(ushort ClockMode);

    //New method added after 2.0.1.8
    [PreserveSig]
    int GetVideoPidType([Out, MarshalAs(UnmanagedType.LPArray)] byte[] pointer);
    [PreserveSig]
    int ShowEPGInfo();
    [PreserveSig]
    int GetAACPid(ref ushort pAacPid);
    [PreserveSig]
    int GetAAC2Pid(ref ushort pAac2Pid);
    [PreserveSig]
    int GetCreateTxtPinOnDemux(ref ushort pbCreatePin);
    [PreserveSig]
    int SetCreateTxtPinOnDemux(ushort bCreatePin);
    [PreserveSig]
    int Load(string pszFileName, ref DirectShowLib.MediaType pmt);
    [PreserveSig]
    int GetPCRPosition(ref long pos);
    [PreserveSig]
    int ShowStreamMenu(IntPtr hwnd);
  }
}
