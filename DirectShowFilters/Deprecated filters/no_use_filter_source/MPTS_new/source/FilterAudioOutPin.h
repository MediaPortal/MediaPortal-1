/*
	MediaPortal TS-SourceFilter by Agree
	
*/
#ifndef __FilterAudioOutPin
#define __FilterAudioOutPin

class CFilterAudioPin : public CBaseOutputPin
{
public:
	CFilterAudioPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, Sections *pSections, HRESULT *phr);
	~CFilterAudioPin();

	DECLARE_IUNKNOWN

	HRESULT CheckMediaType(const CMediaType *cmt);
	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );
	//CSourceStream
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT	GetMediaType(int iPosition, CMediaType *pMediaType);
	STDMETHODIMP ConnectionMediaType(AM_MEDIA_TYPE *pmt);

	//
	void Process(BYTE *audioBuffer,int audioSampleLen,REFERENCE_TIME& start,REFERENCE_TIME& stop);
	HRESULT Deliver(IMediaSample *ms);
	
protected:


protected:
	CCritSec		m_cSharedState;
	BOOL			m_bDiscontinuity;
public:
	Sections	*m_pSections;
	
};

#endif
