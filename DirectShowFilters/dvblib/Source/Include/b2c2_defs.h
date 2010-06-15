/*********************************************************************
*
*		Filename:	b2c2_defs.h
*
*		Description:	
*						
*		History:		
*						
*
*		Copyright (c) 2002 B2C2, Incorporated
*		
*
*********************************************************************/
/*
// Copyright (c) 1998-2002 B2C2, Incorporated.  All Rights Reserved.
//
// THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF B2C2, INCORPORATED.
// The copyright notice above does not evidence any
// actual or intended publication of such source code.
//
// This file is proprietary source code of B2C2, Incorporated. and is released pursuant to and
// subject to the restrictions of the non-disclosure agreement and license contract entered
// into by the parties.
*/

//
// File: b2c2_defs.h
//

#ifndef _B2C2_DEFS_H_
#define _B2C2_DEFS_H_

/* 
 *	FEC enumeration used by SetFEC()/GetFEC()
 */
typedef enum eFECTAG
{
	FEC_1_2 = 1,
	FEC_2_3,
	FEC_3_4,
	FEC_5_6,
	FEC_7_8,
	FEC_AUTO,
	FEC_COUNT = 6,
} eFEC;

/* 
 *	Guard interval enumeration used by SetGuardInterval()/GetGuardInterval()
 */
typedef enum eGuardIntervalTAG
{
	GUARD_INTERVAL_1_32 = 0,
	GUARD_INTERVAL_1_16,
	GUARD_INTERVAL_1_8,
	GUARD_INTERVAL_1_4,
	GUARD_INTERVAL_AUTO,
	GUARD_INTERVAL_COUNT,
} eGuardInterval;

/* 
 *	Polarity enumeration used by SetPolarity()/GetPolarity ()
 */
typedef enum ePolarityTAG
{
	POLARITY_HORIZONTAL = 0,
	POLARITY_VERTICAL,
	POLARITY_COUNT,
 	//if no LNB power is needed
	POLARITY_LNB_NO_POWER = 10,
} ePolarity;

/* 
 *	LNB enumeration used by SetLnbKHz()/GetLnbKHz()
 */
typedef enum eLNBSelectionTAG
{
	LNB_SELECTION_0 = 0,
	LNB_SELECTION_22,
	LNB_SELECTION_33,
	LNB_SELECTION_44,
	LNB_SELECTION_COUNT,
} eLNBSelection;

/* 
 *	Diseqc enumeration used by SetDiseqc()/GetDiseqc()
 */
typedef enum eDiseqcTAG
{
	DISEQC_NONE = 0,
	DISEQC_SIMPLE_A,
	DISEQC_SIMPLE_B,
	DISEQC_LEVEL_1_A_A,
	DISEQC_LEVEL_1_B_A,
	DISEQC_LEVEL_1_A_B,
	DISEQC_LEVEL_1_B_B,
	DISEQC_COUNT
} eDiseqc;

/* 
 *	Modulation enumeration used by SetModulation()/GetModulation()
 */
typedef enum eModulationTAG
{
	QAM_4 = 2,
	QAM_16,
	QAM_32,
	QAM_64,
	QAM_128,
	QAM_256,
	MODE_UNKNOWN = -1
} eModulation;

/* 
 *	Tuner Modulation enumeration used in TunerCapabilities structure to 
 *  return the modulation used by the tuner.
 */
typedef enum tTunerModulationTAG
{
	TUNER_SATELLITE = 0,
	TUNER_CABLE  = 1,
	TUNER_TERRESTRIAL = 2,
	TUNER_ATSC = 3,
	TUNER_UNKNOWN = -1,
} tTunerModulation;


/*
 *	Structure completedy by GetTunerCapabilities() to return tuner capabilities
 */
typedef struct tTunerCapabilities
{
	tTunerModulation	eModulation;
	unsigned long		dwConstellationSupported;       // Show if SetModulation() is supported
	unsigned long		dwFECSupported;                 // Show if SetFec() is suppoted
	unsigned long		dwMinTransponderFreqInKHz;
	unsigned long		dwMaxTransponderFreqInKHz;
	unsigned long		dwMinTunerFreqInKHz;
	unsigned long		dwMaxTunerFreqInKHz;
	unsigned long		dwMinSymbolRateInBaud;
	unsigned long		dwMaxSymbolRateInBaud;
	unsigned long		bAutoSymbolRate;
	unsigned long		dwPerformanceMonitoring;        // See bitmask definitions below
} tTunerCapabilities, *pTunerCapabilities;

/*
 *	Bitmasks for comparison with dwPerformanceMonitoring member of tTunerCapabilities
 *	to determine which of these various options are supported by the current tuner.
 */
															// If set in dwPerformanceMonitoring, tuner supports:
#define BER_SUPPORTED						1L				// BER reporting via GetPreErrorCorrectionBER ()
#define BLOCK_COUNT_SUPPORTED				(1L << 1)		// Block count report via GetTotalBlocks ()
#define CORRECTED_BLOCK_COUNT_SUPPORTED		(1L << 2)		// Corrected block count via GetCorrectedBlocks 
#define UNCORRECTED_BLOCK_COUNT_SUPPORTED	(1L << 3)		// Uncorrected block count via GetUncorrectedBlocks 
#define	SNR_SUPPORTED						(1L << 4)		// SNR via GetSNR ()
#define SIGNAL_STRENGTH_SUPPORTED			(1L << 5)		// Signal strength via GetSignalStrength()
#define SIGNAL_QUALITY_SUPPORTED			(1L << 6)		// Signal quality via GetSignalQuality()

/*
 *	Structure for Mac address list used by *UnicastMacAddress* functions
 */
#define B2C2_SDK_MAC_ADDR_SIZE			6
#define B2C2_SDK_MAC_ADDR_LIST_MAX		32

typedef struct tMacAddressList
{
	long lCount;				// Input : Number of MAC addresses at array
								// Output: Number of MAC addresses set
	unsigned char aabtMacAddr[B2C2_SDK_MAC_ADDR_LIST_MAX][B2C2_SDK_MAC_ADDR_SIZE];
} tMacAddressList, *ptMacAddressList;

/*
 *  Error codes, returned by B2C2 SDK functions in addition to 
 *  COM error codes.
 */

#define B2C2_SDK_E_ALREADY_EXIST			0x10011000		// The PID to add by AddPIDsToPin or AddPIDs already exists.
//#define B2C2_SDK_E_PID_ERROR				0x90011001
#define B2C2_SDK_E_ALREADY_FULL				0x90011002		// Failed to add PID by AddPIDsToPin or AddPIDs because maximum number reached.

// B2C2MPEG2Adapter error codes

#define B2C2_SDK_E_CREATE_INTERFACE 		0x90020001		// Not all interfaces could be created correctly.  
#define B2C2_SDK_E_UNSUPPORTED_DEVICE		0x90020002		// (Linux) The given network device is no B2C2 Boradband device.

#define B2C2_SDK_E_NOT_INITIALIZED 			0x90020003		// Device has not been initialized before calling this functions.
															// Call Initialize () first.

#define B2C2_SDK_E_INVALID_PIN	 			0x90020004		// (Windows) The pin number given at the first argument is invalid { 0 ... 3 }.
#define B2C2_SDK_E_NO_TS_FILTER				0x90020005		// (Windows) No custom renderer filter created. Call CreateTsFilter () first.
#define B2C2_SDK_E_PIN_ALREADY_CONNECTED	0x90020007		// (Windows) The output pin is already connected to a renderer filter input pin.
#define B2C2_SDK_E_NO_INPUT_PIN				0x90020008		// (Windows) No input pin on the custom renderer filter found, check pin name if given.


#endif	// _B2C2_DEFS_H_
