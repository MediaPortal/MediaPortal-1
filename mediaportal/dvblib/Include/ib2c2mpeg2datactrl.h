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
// File: ib2c2mpeg2datactrl.h
//

#ifndef _IB2C2MPEG2DataCtrl_H_
#define _IB2C2MPEG2DataCtrl_H_

#if defined __linux__

#include "linux_windefs.h"

#else // Windows

#ifdef __cplusplus
extern "C" {
#endif

#endif //defined __linux__

#if defined __linux__	// Class implementation for Linux

class CAVSrcFilter;

class IB2C2MPEG2DataCtrl
{
protected: // Data

	CAVSrcFilter * m_pFilter;

public:	// Constructor

	IB2C2MPEG2DataCtrl (CAVSrcFilter *);

	STDMETHOD (Initialize) (THIS_
				VOID
			  ) PURE;

#else 						// COM implementation for Windows

DECLARE_INTERFACE_(IB2C2MPEG2DataCtrl, IUnknown) {

#endif // defined __linux__

	// Note: Add new methods ***only*** at the end, after
	//       existing methods.  Do ***not*** remove methods.
	//       These restrictions are necessary due to the need to
	//       maintain compatibility in COM with past implementations.

public:
	// Transport Stream methods

	STDMETHOD (GetMaxPIDCount) (THIS_
				long *
			  ) PURE;

	//this function is obselete, please use IB2C2MPEG2DataCtrl2's AddPIDsToPin function
	STDMETHOD (AddPIDs) (THIS_
				long, long *
			  ) PURE;

	//this function is obselete, please use IB2C2MPEG2DataCtrl2's DeletePIDsFromPin function
	STDMETHOD (DeletePIDs) (THIS_
				long, long *
			  ) PURE;

	// IP methods

	STDMETHOD (GetMaxIpPIDCount) (THIS_
				long *
			  ) PURE;

	STDMETHOD (AddIpPIDs) (THIS_
				long, long *
			  ) PURE;

	STDMETHOD (DeleteIpPIDs) (THIS_
				long, long *
			  ) PURE;

	STDMETHOD (GetIpPIDs) (THIS_
				long *, long *
			  ) PURE;

	// All protocols

	STDMETHOD (PurgeGlobalPIDs) (THIS_
				VOID
			  ) PURE;

	STDMETHOD (GetMaxGlobalPIDCount) (THIS_
				long *
			  ) PURE;

	STDMETHOD (GetGlobalPIDs) (THIS_
				long *, long *
			  ) PURE;

#if defined WIN32

	STDMETHOD (ResetDataReceptionStats) (THIS_
			  ) PURE;

	STDMETHOD (GetDataReceptionStats) (THIS_
				long *, long *
			  ) PURE;

#endif //defined WIN32

// Add new methods to IB2C2MPEG2DataCtrl2

};

#if defined __linux__	// Class implementation for Linux

class IB2C2MPEG2DataCtrl2 : public IB2C2MPEG2DataCtrl
{
public:	// Constructor
	IB2C2MPEG2DataCtrl2 (CAVSrcFilter *);

#else 						// COM implementation for Windows

DECLARE_INTERFACE_(IB2C2MPEG2DataCtrl2, IB2C2MPEG2DataCtrl) {

#endif // defined __linux__

public:
#if defined WIN32

	STDMETHOD (AddPIDsToPin) (THIS_
				long *, long *, long
			  ) PURE;

	STDMETHOD (DeletePIDsFromPin) (THIS_
				long, long *, long
			  ) PURE;

#endif //defined WIN32

#if defined __linux__

	STDMETHOD (GetBufferPos) (THIS_
				long unsigned int *
			  ) PURE;

#endif //defined __linux__
};

#if defined __linux__	// Class implementation for Linux

class IB2C2MPEG2DataCtrl3 : public IB2C2MPEG2DataCtrl2
{
public:	// Constructor
	IB2C2MPEG2DataCtrl3 (CAVSrcFilter *);

#else 						// COM implementation for Windows

DECLARE_INTERFACE_(IB2C2MPEG2DataCtrl3, IB2C2MPEG2DataCtrl2) {

#endif // defined __linux__

public:
	STDMETHOD (AddTsPIDs) (THIS_
				long, long *
			  ) PURE;

	STDMETHOD (DeleteTsPIDs) (THIS_
				long, long *
			  ) PURE;

	STDMETHOD (GetTsState) (THIS_
				long * plOpen,
				long * plRunning,
				long * plCount,
				long * plPIDArray
			  ) PURE;

	STDMETHOD (GetIpState) (THIS_
				long * plOpen,
				long * plRunning,
				long * plCount,
				long * plPIDArray
			  ) PURE;

	STDMETHOD (GetReceivedDataIp) (THIS_
				__int64 *, __int64 *
			  ) PURE;

	STDMETHOD (AddMulticastMacAddress) (THIS_
				tMacAddressList * pMacAddrList
			  ) PURE;

	STDMETHOD (GetMulticastMacAddressList) (THIS_
				tMacAddressList * pMacAddrList
			  ) PURE;

	STDMETHOD (DeleteMulticastMacAddress) (THIS_
				tMacAddressList * pMacAddrList
			  ) PURE;

	STDMETHOD (SetUnicastMacAddress) (THIS_
				unsigned char * pMacAddr
			  ) PURE;

	STDMETHOD (GetUnicastMacAddress) (THIS_
				unsigned char * pMacAddr
			  ) PURE;

	STDMETHOD (RestoreUnicastMacAddress) (THIS_
			  ) PURE;
};

#if defined WIN32
#ifdef __cplusplus
}
#endif
#endif //defined WIN32

#endif // ! _IB2C2MPEG2DataCtrl_H_
