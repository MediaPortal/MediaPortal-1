/*
	MediaPortal TS-SourceFilter by Agree
	
*/
#ifndef __FilterVideoOutPin
#define __FilterVideoOutPin

#include "MPTSFilter.h"
#include "Sections.h"
#include "Buffers.h"
#include "TimeShiftSeeking.h"
#include "tsdemux.h"
#include <map>
using namespace std;

// the video struct

class CFilterVideoPin : public CSourceStream,CSourceSeeking
{
public:
	CFilterVideoPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr);
	~CFilterVideoPin();

	STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );
	//CSourceStream
	HRESULT GetMediaType(CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT FillBuffer(IMediaSample *pSample);
	virtual HRESULT OnThreadStartPlay();
	virtual HRESULT OnThreadCreate();
	
	// CSourceSeeking
	HRESULT ChangeStart();
	HRESULT ChangeStop();
	HRESULT ChangeRate();
	void	UpdateFromSeek();
	HRESULT SetDuration(REFERENCE_TIME duration);
	void	ResetBuffers(__int64 newPosition);
	void	AboutToStop();
	STDMETHODIMP SetPositions(LONGLONG *pCurrent,DWORD CurrentFlags,LONGLONG *pStop,DWORD StopFlags);

	//
	void Process(BYTE *data, REFERENCE_TIME& ptsStart,REFERENCE_TIME& ptsEnd, int& videoSampleLen, int& audioSampleLen);

	
protected:
	TsDemux m_tsDemuxer;


protected:
	void			SeekIFrame();
	HRESULT			GetData(byte* pData, int maxLen, bool allowedToWait);
	void			UpdatePositions(ULONGLONG& ptsNow);
	CMPTSFilter *	const m_pMPTSFilter;
	FileReader *	const m_pFileReader;
	Sections *		const m_pSections;
	CBuffers *		m_pBuffers;
	CCritSec		m_cSharedState;
	BOOL			m_bDiscontinuity;
	long			m_lTSPacketDeliverySize;
	bool			m_bAboutToStop;
	map<int,bool>	m_mapDiscontinuitySent;
	int				m_iPESPid;
	typedef map<int,bool>::iterator imapDiscontinuitySent;
	BYTE			m_videoBuffer[40000];
	BYTE			m_audioBuffer[40000];
	int             m_videoSampleLen;
	int             m_audioSampleLen;
};

#endif
