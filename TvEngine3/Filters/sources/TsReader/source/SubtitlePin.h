#pragma once
#include "tsreader.h"

class CSubtitlePin: public CSourceStream, public CSourceSeeking
{
public:
	CSubtitlePin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section);
	~CSubtitlePin();

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

	//CSourceStream
	HRESULT GetMediaType(CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT CheckConnect(IPin *pReceivePin);
	HRESULT FillBuffer(IMediaSample *pSample);
  HRESULT BreakConnect();
  STDMETHODIMP SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags);
	

	// CSourceSeeking
	HRESULT ChangeStart();
	HRESULT ChangeStop();
	HRESULT ChangeRate();

	HRESULT OnThreadStartPlay();
	void SetStart(CRefTime rtStartTime);
	void FlushStart();
	void FlushStop();
  bool IsConnected();

protected:
  void UpdateFromSeek();
  bool m_bConnected;
	BOOL m_bDiscontinuity;
	CTsReaderFilter *	const m_pTsReaderFilter;
	CCritSec* m_section;
};
