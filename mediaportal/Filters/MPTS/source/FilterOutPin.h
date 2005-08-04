/*
	MediaPortal TS-SourceFilter by Agree

	
*/


#ifndef __FilterOutPin
#define __FilterOutPin
#include "MPTSFilter.h"
#include "Sections.h"
#include "Buffers.h"


class CFilterOutPin : public CSourceStream,public CSourceSeeking
{
public:
	CFilterOutPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr);
	~CFilterOutPin();

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );
	STDMETHODIMP SetPositions(LONGLONG *pCurrent,DWORD CurrentFlags,LONGLONG *pStop,DWORD StopFlags);
	//STDMETHODIMP GetPositions(LONGLONG *pCurrent,LONGLONG *pStop);
	STDMETHODIMP GetDuration(LONGLONG *pDuration);

	//CSourceStream
	HRESULT GetMediaType(CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT FillBuffer(IMediaSample *pSample);
	virtual HRESULT OnThreadStartPlay();
	virtual HRESULT OnThreadCreate();
	HRESULT Run(REFERENCE_TIME tStart);
	// CSourceSeeking
	HRESULT ChangeStart();
	HRESULT ChangeStop();
	HRESULT ChangeRate();
	void UpdateFromSeek();
	//STDMETHODIMP GetCurrentPosition(LONGLONG *pCurrent);
	HRESULT SetDuration(REFERENCE_TIME duration);

protected:
	HRESULT GetReferenceClock(IReferenceClock **pClock);

protected:
	CMPTSFilter * const m_pMPTSFilter;
	FileReader * const m_pFileReader;
	Sections * const m_pSections;

	CBuffers *m_pBuffers;
	
	CCritSec m_cSharedState;

	REFERENCE_TIME m_AvgTimePerFrame;
	REFERENCE_TIME m_rtStreamTime, m_rtSourceTime;
	REFERENCE_TIME m_rtFrameLength;
	REFERENCE_TIME m_timeStart;
	double m_dTimeInc;
	ULONGLONG m_StartTime;
	BOOL m_bDiscontinuity, m_bFlushing;

	long m_lTSPacketDeliverySize;
	DWORD m_dwStartTime;
	bool m_positionToSet;

};

#endif
