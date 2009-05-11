//////////////////////////////////////////////////////////////////////////////
//
//                          (C) TechnoTrend AG 2005
//  All rights are reserved. Reproduction in whole or in part is prohibited
//  without the written consent of the copyright owner.
//
//  TechnoTrend reserves the right to make changes without notice at any time.
//  TechnoTrend makes no warranty, expressed, implied or statutory, including
//  but not limited to any implied warranty of merchantability or fitness for
//  any particular purpose, or that the use will not infringe any third party
//  patent, copyright or trademark. TechnoTrend must not be liable for any
//  loss or damage arising from its use.
//
//////////////////////////////////////////////////////////////////////////////

#ifndef DRVINOUTSTRUCTS_H
#define DRVINOUTSTRUCTS_H

#pragma warning(disable:4200) // zero-sized array
#pragma pack(1)

// I2C read sequence
typedef struct
{
    BYTE Slave;
    BYTE pStartAddr[2];
    BYTE ReadAddrBytes;
    BYTE Length;
} I2CReadIn, *pI2CReadIn;

typedef struct
{
    int  iResult;
    BYTE pData[0];
} I2CReadOut, *pI2CReadOut;

// I2C write sequenze
typedef struct
{
    BYTE Slave;
    BYTE Length;
    BYTE pData[0];
} I2CWriteIn, *pI2CWriteIn;

typedef struct
{
    int  iResult;
} I2CWriteOut, *pI2CWriteOut;

// I2C access combined
typedef struct
{
    BYTE Slave;
    BYTE ReadLength;
    BYTE WriteLength;
    BYTE pWriteData[0];
} I2CCombIn, *pI2CCombIn;

typedef struct
{
    int  iResult;
    BYTE pData[0];
} I2CCombOut, *pI2CCombOut;

typedef struct
{
    DWORD          dwMCEFreq;
    DWORD          dwDvbSFreq;
    DWORD          dwDvbSSymbolrate;
    BOOL           DiSEqC;
    BOOL           Position;
    BOOL           Option;
    DWORD          dwLOFHigh;
    DWORD          dwLOFLow;
    DWORD          dwLOFSwitch;
    Polarisation   ePolarisation;
	ModulationType eModulation;
} DvbTFakeTransponder, *pDvbTFakeTransponder;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     DVB-C tune request structure
/////////////////////////////////////////////////////////////////////////////
typedef struct structDVBC_TunReq
{
/// \brief Specifies the target frequency in kHz.
    ULONG               dwFrequency;
/// \brief Specifies the modulation for the target
///        transponder. Values are specified in the
///        ModulationType enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     QAM16     1
/// \n     QAM32     2
/// \n     QAM64     3
/// \n     QAM128    7
/// \n     QAM256   11
    ULONG               dwModulationType;
/// \brief currently not in use
    ULONG               dwFECType;
/// \brief currently not in use
    ULONG               dwFECRate;
/// \brief currently not in use
    ULONG               dwOuterFECType;
/// \brief currently not in use
    ULONG               dwOuterFECRate;
/// \brief Specifies the symbol rate for the target
///        transponder in kSym/s.
    ULONG               dwSymbolRate;
/// \brief Specifies the spectral inversion for the
///        target transponder. Values are specified in the
///        SpectralInversion enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     not defined  0
/// \n     automatic    1
/// \n     normal       2
/// \n     inverted     3
    SpectralInversion   dwSpectralInversion;
} structDVBC_TunReq, *pstructDVBC_TunReq;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     DVB-S tune request structure
/////////////////////////////////////////////////////////////////////////////
typedef struct structDVBS_TunReq
{
/// \brief Specifies the target frequency in kHz.
    ULONG               dwFrequency;
/// \brief Specifies the frequency multiplier.
    ULONG               dwFrequencyMultiplier;
/// \brief Specifies the polarisation for the target
///        transponder. Values are specified in the
///        Polarisation enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     not defined         0
/// \n     linear horizontal   1
/// \n     linear vertical     2
/// \n     circular left       3
/// \n     circular right      4
    Polarisation        tPolarity;
/// \brief currently not in use
    ULONG               dwBandwidth;
/// \brief Determines if DiSEqC is being used and
///        contains information about the selected
///        LNB source. Bytes are counted LSB first.
/// \n     not active:    0xFFFFFFFF
/// \n     Position A:    Byte 1 has value 0
/// \n     Position B:    Byte 1 has value 1
/// \n     Option A:      Byte 3 has value 0
/// \n     Option B:      Byte 3 has value 1
    ULONG               dwRange;
/// \brief currently not in use
    ULONG               dwTransponder;
/// \brief Specifies the modulation for the target
///        transponder. Values are specified in the
///        ModulationType enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     QPSK    20  (used to indicate DVB-S)
/// \n     8VSB    23  (used to indicate DVB-S2)
    ULONG               dwModulationType;
/// \brief currently not in use
    ULONG               dwFECType;
/// \brief currently not in use
    ULONG               dwFECRate;
/// \brief currently not in use
    ULONG               dwOuterFECType;
/// \brief currently not in use
    ULONG               dwOuterFECRate;
/// \brief Specifies the symbol rate for the target
///        transponder in kSym/s.
    ULONG               dwSymbolRate;
/// \brief Specifies the spectral inversion for the
///        target transponder. Values are specified in the
///        SpectralInversion enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     not defined  0
/// \n     automatic    1
/// \n     normal       2
/// \n     inverted     3
    SpectralInversion   dwSpectralInversion;
/// \brief Specifies the high band LOF in kHz.
    ULONG               dwHighBandLOF;
/// \brief Specifies the low band LOF in kHz.
    ULONG               dwLowBandLOF;
/// \brief Specifies the LNB switch freqency in kHz.
    ULONG               dwSwitchFrequency;
/// \brief Specifies whether to use toneburst or not.
///        LNB source is determined by the dwRange
///        parameter.
/// \n     not active: Byte 1 has value 0
/// \n     active:     Byte 1 has value 1

    ULONG               dwMode; 
/// \brief currently not in use
    ULONG               dwCommand;
/// \brief currently not in use
    ULONG               dwCmdCount;
/// \brief reserved for use in Windows Fiji-MCE
    ULONG               dwLNBSource;
/// \brief reserved for use in Windows Fiji-MCE
    BYTE                bpDiSEqCData[16];
} structDVBS_TunReq, *pstructDVBS_TunReq;

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     DVB-T tune request structure
/////////////////////////////////////////////////////////////////////////////
typedef struct structDVBT_TunReq
{
/// \brief Specifies the target frequency in kHz.
    ULONG               dwFrequency;
/// \brief Specifies the frequency multiplier.
    ULONG               dwFrequencyMultiplier;
/// \brief Specifies the bandwidth in kHz.
    ULONG               dwBandwidth;
/// \brief Specifies the modulation for the target
///        transponder. Values are specified in the
///        ModulationType enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     QAM16     1
/// \n     QAM64     3
/// \n     QAM256   11
/// \n     QPSK     20
    ULONG               dwModulationType;
///  \brief currently not in use
    ULONG               dwFECType;
///  \brief currently not in use
    ULONG               dwFECRate;
///  \brief currently not in use
    ULONG               dwOuterFECType;
///  \brief currently not in use
    ULONG               dwOuterFECRate;
/// \brief Specifies the spectral inversion for the
///        target transponder. Values are specified in the
///        SpectralInversion enum in bdatypes.h in the
///        Windows Driver Kit.
/// \n     Example values:
/// \n     not defined  0
/// \n     automatic    1
/// \n     normal       2
/// \n     inverted     3
    SpectralInversion   dwSpectralInversion;
///  \brief currently not in use
    ULONG               dwGuardInterval;
///  \brief currently not in use
    ULONG               dwTransmissionMode;
} structDVBT_TunReq, *pstructDVBT_TunReq;

//////////////////////////////////////////////////////////////////////////////
/// \brief
///     Enumeration for all supported types of DVB
//////////////////////////////////////////////////////////////////////////////
typedef enum
{
    DVB_C = 0,
    DVB_T,
    DVB_S
} DVB_TYPE;

//////////////////////////////////////////////////////////////////////////////
/// \brief
///     DVB tune request structure to be used in all tune requests.
///     Set dvbType to the correct value for either DVB-C/T/S and fill the
///     corresponding tunerequest structure in the data union.
//////////////////////////////////////////////////////////////////////////////
typedef struct structDVB_TunReq
{
/// \brief Specifies whether to use information for DVB-C / DVB-T / DVB-S
    DVB_TYPE    dvbType;
/// \brief Contains the tune request for the DVB standard specified by dvbType.
    union DVB_TUNREQS
    {
        structDVBC_TunReq   DVBC_TunReq;
        structDVBT_TunReq   DVBT_TunReq;
        structDVBS_TunReq   DVBS_TunReq;
    } data;
} structDVB_TunReq, *pstructDVB_TunReq;

// for transport stream analysis
typedef struct
{	
	DWORD	PidCount;
	DWORD	ContErr;
	BYTE	ContCount;
} structPID, *PstructPID;

typedef enum
{
    /// the input structure did not have the expected size
    ERR_INCORRECT_SIZE = 16,
    /// the tuner interface was not available
    ERR_TUNER_IF_UNAVAILABLE,
    /// an unknown DVB type has been specified for the tune request
    ERR_UNKNOWN_DVB_TYPE
} DRIVER_ERR_CODE;

#pragma warning(default:4200) // zero-sized array
#pragma pack()

#endif // #ifndef DRVINOUTSTRUCTS_H

// eof
