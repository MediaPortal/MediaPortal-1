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
// File: ib2c2mpeg2tunerctrl.h
//

#ifndef _IB2C2MPEG2TunerCtrl_H_
#define _IB2C2MPEG2TunerCtrl_H_

#if defined __linux__

#include "linux_windefs.h"

#endif //defined __linux__

#include "b2c2_defs.h"

#if defined __linux__	// Class implementation for Linux

class CAVSrcFilter;

///////////////////////////////////////////////////////////////////////////////
//
// IB2C2MPEG2TunerCtrl
//

class IB2C2MPEG2TunerCtrl
{
protected: // Data

	CAVSrcFilter * m_pFilter;

public:	// Constructor
	IB2C2MPEG2TunerCtrl (CAVSrcFilter *);

#else 					// COM implementation for Windows

#ifdef __cplusplus
extern "C" {
#endif

DECLARE_INTERFACE_(IB2C2MPEG2TunerCtrl, IUnknown)
{

#endif // defined __linux__

	// Note: Add new methods ***only*** at the end, after
	//       existing methods.  Do ***not*** remove methods.
	//       These restrictions are necessary due to the need to
	//       maintain compatibility in COM with past implementations.

	// Satellite, Cable, Terrestrial (ATSC and DVB)

	STDMETHOD(SetFrequency) (THIS_
				long	
			 ) PURE;

	// Satellite, Cable

	STDMETHOD(SetSymbolRate) (THIS_
				long
			 ) PURE;

	// Satellite only

	STDMETHOD(SetLnbFrequency) (THIS_
				long
			 ) PURE;

	STDMETHOD(SetFec) (THIS_
				long
			 ) PURE;

	STDMETHOD(SetPolarity) (THIS_
				long
			 ) PURE;

	STDMETHOD(SetLnbKHz) (THIS_
				long
			 ) PURE;
	
	STDMETHOD(SetDiseqc) (THIS_
				long
			 ) PURE;

	// Cable only

	STDMETHOD(SetModulation) (THIS_
				long
			 ) PURE;
	
	// All tuners

	STDMETHOD(Initialize) (THIS_
				VOID
			 ) PURE;

	STDMETHOD(SetTunerStatus) (THIS_
				VOID
			 ) PURE;

	STDMETHOD(CheckLock) (THIS_
				VOID
			 ) PURE;

	STDMETHOD(GetTunerCapabilities) (THIS_
				tTunerCapabilities *, long *
			 ) PURE;

	// Terrestrial (ATSC)

	STDMETHOD(GetFrequency) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetSymbolRate) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetModulation) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetSignalStrength) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetSignalLevel) (THIS_
				float *
			 ) PURE;

	STDMETHOD(GetSNR) (THIS_
				float *
			 ) PURE;

	STDMETHOD(GetPreErrorCorrectionBER) (THIS_
				float *, bool
			 ) PURE;

	STDMETHOD(GetUncorrectedBlocks) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetTotalBlocks) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetChannel) (THIS_
				long *
			 ) PURE;

	STDMETHOD(SetChannel) (THIS_
				long
			 ) PURE;

// Add new methods to IB2C2MPEG2TunerCtrl2

};	// DECLARE_INTERFACE_(IB2C2MPEG2TunerCtrl, IUnknown) - END


///////////////////////////////////////////////////////////////////////////////
//
// IB2C2MPEG2TunerCtrl2
//

#if defined __linux__	// Class implementation for Linux

class IB2C2MPEG2TunerCtrl2 : public IB2C2MPEG2TunerCtrl
{
public:	// Constructor
	IB2C2MPEG2TunerCtrl2 (CAVSrcFilter *);

#else 					// COM implementation for Windows

DECLARE_INTERFACE_(IB2C2MPEG2TunerCtrl2, IB2C2MPEG2TunerCtrl)
{

#endif // defined __linux__

	STDMETHOD(SetTunerStatusEx) (THIS_
				long
			 ) PURE;

	STDMETHOD(SetFrequencyKHz) (THIS_
				long	
			 ) PURE;

	// Terrestrial DVB only

	STDMETHOD(SetGuardInterval) (THIS_
				long
			 ) PURE;

	STDMETHOD(GetGuardInterval) (THIS_
				long *
			 ) PURE;

	STDMETHOD(GetFec) (THIS_
				long * plFec
			 ) PURE;

	STDMETHOD(GetPolarity) (THIS_
				long * plPolarity
			 ) PURE;

	STDMETHOD(GetDiseqc) (THIS_
				long * plDiseqc
			 ) PURE;

	STDMETHOD(GetLnbKHz) (THIS_
				long * plLnbKHz

			 ) PURE;

	STDMETHOD(GetLnbFrequency) (THIS_
				long * plFrequencyMHz
			 ) PURE;

	STDMETHOD(GetCorrectedBlocks) (THIS_
				long * plCorrectedBlocks
			 ) PURE;

	STDMETHOD(GetSignalQuality) (THIS_
				long * pdwSignalQuality
			 ) PURE;

}; // DECLARE_INTERFACE_(IB2C2MPEG2TunerCtrl2, IB2C2MPEG2TunerCtrl) - END

#if !defined __linux__
#ifdef __cplusplus
}
#endif
#endif //!defined __linux__

#endif // ! _IB2C2MPEG2TunerCtrl_H_
