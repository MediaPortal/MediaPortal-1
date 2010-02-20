/*
Copyright (c) 1998-2002 B2C2, Incorporated.  All Rights Reserved.
 
THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF B2C2, INCORPORATED.
The copyright notice above does not evidence any
actual or intended publication of such source code. 
 
This file is proprietary source code of B2C2, Incorporated. and is released pursuant to and
subject to the restrictions of the non-disclosure agreement and license contract entered
into by the parties.
	
	  IB2C2MPEGDataPin.h
	  define the interface for the data output pins
*/

#ifndef _IB2C2MPEG2DataPin_H__
#define _IB2C2MPEG2DataPin_H__

#ifdef __cplusplus
extern "C" {
#endif

DECLARE_INTERFACE_(IB2C2MPEG2DataPin, IUnknown) {

	//return the pin index
	STDMETHOD(GetDataPinIndex) (THIS_
				long *
			 ) PURE;

	//add PIDs to this pin
	//Parameter:	a PID array, a pointer to long as number of PIDs in the array
	//Return:		FAILED() -- non added, plNumPID contains the number of PIDs added
	STDMETHOD(AddDataPIDs) (THIS_
				long *plNumPID, long *plPIDs
			 ) PURE;

	//delete PIDs to this pin
	//Parameter:	a PID array, number of PIDs in the array
	//Return:		FAILED() -- non deleted
	STDMETHOD(DeleteDataPIDs) (THIS_
				long, long *
			 ) PURE;
};

#ifdef __cplusplus
}
#endif

#endif//_IB2C2MPEG2DataPin_H__