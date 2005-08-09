/*
	MediaPortal TS-SourceFilter by Agree

	
*/


#ifndef __FilterOutPin
#define __FilterOutPin
#include "MPTSFilter.h"
#include "Sections.h"
#include "Buffers.h"
#include "TimeShiftSeeking.h"


class CFilterOutPin : public CSourceStream,public CTimeShiftSeeking
{
public:
	CFilterOutPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr);
	~CFilterOutPin();

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );
	STDMETHODIMP SetPositions(LONGLONG *pCurrent,DWORD CurrentFlags,LONGLONG *pStop,DWORD StopFlags);

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
	void ResetBuffers();
protected:
	HRESULT GetReferenceClock(IReferenceClock **pClock);

protected:
	void UpdatePositions(ULONGLONG& startPts, ULONGLONG& endPts);
	CMPTSFilter * const m_pMPTSFilter;
	FileReader * const m_pFileReader;
	Sections * const m_pSections;

	CBuffers *m_pBuffers;
	
	CCritSec m_cSharedState;

	double m_dTimeInc;
	ULONGLONG m_StartTime;
	BOOL m_bDiscontinuity, m_bFlushing;

	long m_lTSPacketDeliverySize;
	DWORD m_dwStartTime;
	bool m_positionToSet;
	ULONGLONG m_lastPTS;
};

#endif
