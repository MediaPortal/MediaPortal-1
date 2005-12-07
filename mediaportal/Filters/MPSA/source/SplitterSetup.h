/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Demux
#define __Demux
#pragma warning(disable: 4511 4512 4995)

#include "Section.h"
#include "Control.h"
#include "..\..\..\dvblib\Include\b2c2mpeg2adapter.h"
#include "..\..\..\dvblib\Include\b2c2_defs.h"
#include "..\..\..\dvblib\Include\B2C2_Guids.h"
#include "..\..\..\dvblib\Include\b2c2mpeg2adapter.h"
#include "..\..\..\dvblib\Include\ib2c2mpeg2tunerctrl.h"
#include "..\..\..\dvblib\Include\ib2c2mpeg2datactrl.h"
#include "..\..\..\dvblib\Include\ib2c2mpeg2avctrl.h"

// media types

class SplitterSetup
{
public:

	SplitterSetup(Sections *pSections);
	virtual ~SplitterSetup();
	HRESULT 		SetSectionMapping();
	STDMETHODIMP	SetDemuxPins(IFilterGraph *pGraph);
	HRESULT 		SetupDefaultMapping();
	HRESULT 		MapAdditionalPID(ULONG pid);
	HRESULT 		MapAdditionalPayloadPID(ULONG pid);
	HRESULT 		UnMapAllPIDs();
	HRESULT 		UnMapSectionPIDs();
	HRESULT 		SetSectionsPin(IPin *ppin);
	HRESULT 		SetMHW1Pin(IPin *ppin);
	HRESULT 		SetMHW2Pin(IPin *ppin);
	HRESULT 		SetEPGPin(IPin *ppin);
	bool			PinIsNULL();
	HRESULT 		SetEPGMapping();
	void			UseATSC(bool yesNo);
    long 			SS2SetPidToPin(long pin,long pid);
	BOOL 			SS2DeleteAllPIDs(long pin);
protected:
	HRESULT 		GetPSIMedia(AM_MEDIA_TYPE *pintype);
	HRESULT 		SetMHW1Mapping();
	HRESULT 		SetMHW2Mapping();
protected:
	IB2C2MPEG2DataCtrl3*	m_dataCtrl;
	bool					m_bUseATSC;
	Sections*				m_pSections;
	IPin*					m_pSectionsPin;
	IPin*					m_pMHW1Pin;
	IPin*					m_pMHW2Pin;
	IPin*					m_pEPGPin;	

};

#endif
