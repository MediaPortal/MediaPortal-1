/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Demux
#define __Demux
#pragma warning(disable: 4511 4512 4995)

#include "Section.h"
#include "Control.h"


class SplitterSetup
{
public:

	SplitterSetup(Sections *pSections);
	virtual ~SplitterSetup();
	HRESULT 		SetSectionMapping(IPin* pPin);
	STDMETHODIMP	SetDemuxPins(IFilterGraph *pGraph);
	HRESULT 		MapAdditionalPID(IPin* pPin,ULONG pid);
	HRESULT 		MapAdditionalPayloadPID(IPin* pPin,ULONG pid);
	void			UseATSC(bool yesNo);
protected:
	HRESULT 		GetPSIMedia(AM_MEDIA_TYPE *pintype);

protected:
	bool					m_bUseATSC;
	Sections*				m_pSections;
};

#endif
