/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Demux
#define __Demux
#pragma warning(disable: 4511 4512 4995)

#include "Section.h"
#include "Control.h"

// media types

class SplitterSetup
{
public:

	SplitterSetup(Sections *pSections);
	virtual ~SplitterSetup();
	STDMETHODIMP SetDemuxPins(IFilterGraph *pGraph);
	HRESULT SetupDefaultMapping();
	HRESULT MapAdditionalPID(ULONG pid);
	HRESULT MapAdditionalPayloadPID(ULONG pid);
	HRESULT UnMapAllPIDs();
	HRESULT SetSectionsPin(IPin *ppin);
	HRESULT SetMHW1Pin(IPin *ppin);
	HRESULT SetMHW2Pin(IPin *ppin);
	bool PinIsNULL();
	HRESULT SetEPGMapping();
protected:
	HRESULT SetupDemuxer(IBaseFilter *p);
	HRESULT GetPSIMedia(AM_MEDIA_TYPE *pintype);
	HRESULT SetSectionMapping();
	HRESULT SetMHW1Mapping();
	HRESULT SetMHW2Mapping();

protected:
	Sections *m_pSections;
	BOOL m_demuxSetupComplete;
	IPin *m_pSectionsPin;
	IPin *m_pMHW1Pin;
	IPin *m_pMHW2Pin;
	

};

#endif
