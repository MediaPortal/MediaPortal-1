// Copyright by Agree
//
// TSMuxSplitter filter is using some elements from SampleParser!!
//
// Copyright (c) GDCL 2004. All Rights Reserved. 
// You are free to re-use this as the basis for your own filter development,
// provided you retain this copyright notice in the source.
// http://www.gdcl.co.uk
//
//////////////////////////////////////////////////////////////////////

#ifndef __TSMUXSPLITTER_
#define __TSMUXSPLITTER_

#include "pullpin2.h"
#include "Demuxer.h"
#include "proppage.h"

// media type format blocks
// media types
	static BYTE	Mpeg2ProgramVideo [] = 
	{
					0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00
	};
	
	static	BYTE	MPEG1AudioFormat [] = 
	{
		0x50, 0x00,
		0x02, 0x00,
		0x80, 0xBB,	0x00, 0x00, 
		0x00, 0x7D,	0x00, 0x00, 
		0x00, 0x03,				
		0x00, 0x00,				
		0x16, 0x00,				
		0x02, 0x00,				
		0x00, 0xE8,				
		0x03, 0x00,				
		0x01, 0x00,	0x01,0x00,  
		0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00
	};
	//
		static BYTE g_Mpeg2ProgramVideo [] = 
		{
			0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
			0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
		0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
		0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
		0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
		0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
		0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
		0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

		//0x051736=333667-> 10000000/333667 = 29.97fps
		//0x061A80=400000-> 10000000/400000 = 25fps
		0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
		0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
		0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
		0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
		0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
		0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
		0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
		0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
		0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
		0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
		0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
		0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
		0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
		0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
		0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
		0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
				//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
		0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
		0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
		0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
				//  .dwSequenceHeader [1]
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00
	};


	static BYTE g_MPEG1AudioFormat [] = 
	{
		0x50, 0x00,				//wFormatTag
		0x02, 0x00,				//nChannels
		0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
		0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
		0x01, 0x00,				//nBlockAlign
		0x00, 0x00,				//wBitsPerSample
		0x16, 0x00,				//cbSize
		0x02, 0x00,				//wValidBitsPerSample
		0x00, 0xe8,				//wSamplesPerBlock
		0x03, 0x00,				//wReserved
		0x01, 0x00, 0x01, 0x00, //dwChannelMask
		0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
	};
// DShow interfaces used in smart pointers
_COM_SMARTPTR_TYPEDEF(IMemAllocator, IID_IMemAllocator);
_COM_SMARTPTR_TYPEDEF(IMediaSample, IID_IMediaSample);
_COM_SMARTPTR_TYPEDEF(IAsyncReader, IID_IAsyncReader);

// classes declared here
class StreamInputPin;
class SplitterOutputPin;
class TSMuxSplitter;

class StreamInputPin : public CBasePin
{
public:
    StreamInputPin(TSMuxSplitter* pFilter, CCritSec* pLock, HRESULT* phr);
    ~StreamInputPin();

    // base pin overrides
    HRESULT CheckMediaType(const CMediaType* pmt);
    HRESULT GetMediaType(int iPosition, CMediaType* pmt);
    STDMETHODIMP BeginFlush();
    STDMETHODIMP EndFlush();
    HRESULT CompleteConnect(IPin* pPeer);
    HRESULT Inactive();
    STDMETHODIMP Receive(IMediaSample* pSample);
    STDMETHODIMP EndOfStream();

    // called from filter
    HRESULT Seek(REFERENCE_TIME tStart);
    bool Suspend();
    void Resume();

private:
    class implPullPin : public CPullPin2
    {
    public:
        implPullPin(StreamInputPin* pPin);

        HRESULT BeginFlush();
        HRESULT Receive(IMediaSample* pSample);
        HRESULT EndOfStream();
        void OnError(HRESULT hr);
        HRESULT EndFlush();
    private:
        StreamInputPin* m_pPin;
    };

    implPullPin* m_pPull;
    TSMuxSplitter* m_pParser;
    bool m_bFlushing;
};

class SplitterOutputPin 
: public CBaseOutputPin,
  public IMediaSeeking
{
public:
    SplitterOutputPin(TSMuxSplitter* pParser, CCritSec* pLock, HRESULT* phr, LPCWSTR pName);

	DECLARE_IUNKNOWN
	STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    // base class overrides for connection establishment
    HRESULT CheckMediaType(const CMediaType* pmt);
    HRESULT GetMediaType(int iPosition, CMediaType* pmt);
    HRESULT SetMediaType(const CMediaType* pmt);
    HRESULT DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pprop);
	HRESULT CompleteConnect(IPin *pPin);

    // this group of methods deal with the COutputQueue
    HRESULT Active();
    HRESULT Inactive();
    HRESULT DeliverEndOfStream();
    HRESULT DeliverBeginFlush();
    HRESULT DeliverEndFlush();
	void SendAnyway()
	{ 
		if (m_pOutputQ)
		{
			m_pOutputQ->SendAnyway();
		}
	}

    // called from filter.
    // We can offer two media types -- typically one packetised, and one payload-only.
    // We report to the filter which has been selected so that it can prepare the data
    // appropriately.
    void OfferMediaType( const CMediaType* pmtPayload);
    bool IsPacketised()
    {
        return m_bPacketised;
    }
    HRESULT DeliverData(const BYTE* pData, long cBytes, const REFERENCE_TIME* ptStart);

// IMediaSeeking
public:
    STDMETHODIMP GetCapabilities(DWORD * pCapabilities );
    STDMETHODIMP CheckCapabilities(DWORD * pCapabilities );
    STDMETHODIMP IsFormatSupported(const GUID * pFormat);
    STDMETHODIMP QueryPreferredFormat(GUID * pFormat);
    STDMETHODIMP GetTimeFormat(GUID *pFormat);
    STDMETHODIMP IsUsingTimeFormat(const GUID * pFormat);
    STDMETHODIMP SetTimeFormat(const GUID * pFormat);
    STDMETHODIMP GetDuration(LONGLONG *pDuration);
    STDMETHODIMP GetStopPosition(LONGLONG *pStop);
    STDMETHODIMP GetCurrentPosition(LONGLONG *pCurrent);
    STDMETHODIMP ConvertTimeFormat(LONGLONG * pTarget, const GUID * pTargetFormat,
                              LONGLONG    Source, const GUID * pSourceFormat );
    STDMETHODIMP SetPositions(LONGLONG * pCurrent, DWORD dwCurrentFlags
			, LONGLONG * pStop, DWORD dwStopFlags );
    STDMETHODIMP GetPositions(LONGLONG * pCurrent,
                              LONGLONG * pStop );
    STDMETHODIMP GetAvailable(LONGLONG * pEarliest, LONGLONG * pLatest );
    STDMETHODIMP SetRate(double dRate);
    STDMETHODIMP GetRate(double * pdRate);
    STDMETHODIMP GetPreroll(LONGLONG * pllPreroll);

private:
    CMediaType m_mtPacket;
    CMediaType m_mtPayload;
    bool m_bPacketised;
    bool m_bDiscont;
    TSMuxSplitter* m_pParser;
    COutputQueue* m_pOutputQ;
};


class DECLSPEC_UUID("AAFFB56F-8EB1-4c19-BCA9-1227FF294A2D")
TSMuxSplitter : public CBaseFilter,public ISpecifyPropertyPages
{
public:
    DECLARE_IUNKNOWN
	// constructor method used by class factory
    static CUnknown* WINAPI CreateInstance(LPUNKNOWN pUnk, HRESULT* phr);


    // filter registration tables
    static const AMOVIESETUP_MEDIATYPE m_sudType[];
    static const AMOVIESETUP_PIN m_sudPin[];
    static const AMOVIESETUP_FILTER m_sudFilter;

    // CBaseFilter methods
    int GetPinCount();
    CBasePin *GetPin(int n);
	STDMETHODIMP Pause();

    // called from input pin
    HRESULT BeginFlush();
    HRESULT EndFlush();
    HRESULT EndOfStream();
    HRESULT Receive(IMediaSample* pSample);
    HRESULT CompleteConnect(IPin* pPeer);

	// called from output pin
	bool SelectSeekingPin(SplitterOutputPin* pPin);
	void DeselectSeekingPin(SplitterOutputPin* pPin);
	REFERENCE_TIME GetDuration();
	void GetSeekingParams(REFERENCE_TIME* ptStart, REFERENCE_TIME* ptStop, double* pdRate);
	HRESULT Seek(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);
	HRESULT SetRate(double dRate);
	HRESULT SetStopTime(REFERENCE_TIME tStop);

	//
	HRESULT ProcessPacket(BYTE *data,DWORD len,LONGLONG pts,bool audioVideo);
	DWORD GetAudioPID(void);
	DWORD GetVideoPID(void);
	STDMETHODIMP GetPages(CAUUID *pPages);
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid,void **ppv);

	// 
	void GetVideoHeader(Demux::VideoHeader * vh)
	{
		*vh=m_demuxer.m_VideoHeader;
	}
	void GetAudioHeader(Demux::AudioHeader * ah)
	{
		*ah=m_demuxer.m_AudioHeader;
	}
	bool IsAC3Audio()
	{
		return m_demuxer.m_isAC3Audio;
	}
private:
    // construct only via class factory
    TSMuxSplitter(LPUNKNOWN pUnk, HRESULT* phr);
    ~TSMuxSplitter();

    bool CreateVideoType();
    bool CreateAudioType();

private:
    CCritSec m_csFilter;
    SplitterOutputPin* m_pVideo;
    SplitterOutputPin* m_pAudio;
    StreamInputPin* m_pInput;
    Demux m_demuxer;

   enum
    {
        ParseBufferSize = 81780,
    };

    BYTE m_ParseBuffer[ParseBufferSize];
	BYTE *m_AudioStreamBuffer;
	BYTE *m_VideoStreamBuffer;
    // for timing baseline and duration
    LONGLONG m_firstPTS;
    LONGLONG m_lastPTS;
    LONGLONG m_llFileSize;

    // for seeking
    CCritSec m_csSeeking;
    REFERENCE_TIME m_tStart;
    REFERENCE_TIME m_tStop;
    double m_dRate;
    SplitterOutputPin* m_pSeekingPin;

    // VBR seeking -- multiple tries, refining bitrate
    bool m_bLocating;
    int m_nTries;
    // when locating, last known position and time
    LONGLONG m_posLast;
    REFERENCE_TIME m_tsLast;
    // current target offset (file location of this seek)
    LONGLONG m_posThis;

};

#endif