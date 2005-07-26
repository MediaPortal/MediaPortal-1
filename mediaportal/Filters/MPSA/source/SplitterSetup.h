/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Demux
#define __Demux

#include "Section.h"
#include "Control.h"

// media types

class SplitterSetup
{
public:

	SplitterSetup(Sections *pSections);
	virtual ~SplitterSetup();
	STDMETHODIMP SetDemuxPins(IFilterGraph *pGraph);
	HRESULT SetupPins(IPin *);
	HRESULT MapAdditionalPID(ULONG pid);
	HRESULT MapAdditionalPayloadPID(ULONG pid);
	HRESULT UnMapAllPIDs();
	HRESULT SetPin(IPin *ppin);
	bool PinIsNULL();

protected:
	HRESULT SetupDemuxer(IBaseFilter *p);
	HRESULT GetPSIMedia(AM_MEDIA_TYPE *pintype);
protected:
	Sections *m_pSections;
	BOOL m_demuxSetupComplete;
	IPin *m_pSectionsPin;
	

};

#endif
