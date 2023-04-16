using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPx86Proxy
{
    public enum CommandEnum
    {
        Unknown = 0,
        DriverMethod,

        ImonInit = 120,
        ImonUninit,
        ImonIsInited,
        ImonSetText,
        ImonSetEQ,
        ImonSetLCDData2,
        ImonSendData,
        ImonSendDataBuffer,

        ImonRCInit = 130,
        ImonRCUninit,
        ImonRCIsInited,
        ImonRCGetHWType,
        ImonRCGetFirmwareVer,
        ImonRCCheckDriverVersion,
        ImonRCChangeiMONRCSet,
        ImonRCChangeRC6,
        ImonRCGetLastRFMode,
        ImonRCGetPacket,
        ImonRCRegisterForEvents,
        ImonRCUnregisterFromEvents
    }
}
