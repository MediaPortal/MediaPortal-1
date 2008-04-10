//------------------------------------------------------------------------------
// File: Dump.h
//
// Desc: DirectShow sample code - definitions for dump renderer.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
#include <map>
using namespace std;

enum StreamState
{
	Started,
	NotPresent,
	Scrambled,
	Unscrambled,
};
typedef struct AdaptionHeader
	{
		bool DiscontinuityIndicator;
		bool RandomAccessIndicator;
		bool ElementaryStreamPriorityIndicator;
		bool PCRFlag;
		bool OPCRFlag;
		bool SplicingPointFlag;
		bool TransportPrivateData;
		bool AdaptationHeaderExtension;
		unsigned short Len;
		ULONGLONG PCRValue;
		WORD PCRCounter;
};


struct stTSHEADER
{
	BYTE SyncByte			;
	bool TransportError		;
	bool PayloadUnitStart	;
	bool TransportPriority	;
	unsigned short Pid		;
	BYTE TScrambling		;
	BYTE AdaptionControl	;
	BYTE ContinuityCounter	;
};
typedef stTSHEADER TSHeader;

struct stPESHeader
{
	BYTE     Reserved				;
	BYTE     ScramblingControl		;
	BYTE     Priority 				;
	BYTE     dataAlignmentIndicator	;
	BYTE     Copyright				;
	BYTE     Original				;
	BYTE     PTSFlags				;
	BYTE     ESCRFlag				;
	BYTE     ESRateFlag				;
	BYTE     DSMTrickModeFlag		;
	BYTE     AdditionalCopyInfoFlag	;
	BYTE     PESCRCFlag				;
	BYTE     PESExtensionFlag		;
	BYTE     PESHeaderDataLength	;
};
typedef stPESHeader PESHeader;

struct stPTSTime
{
	ULONGLONG h;
	ULONGLONG m;
	ULONGLONG s;
	ULONGLONG u;
};
typedef stPTSTime PTSTime;

class CDumpInputPin;
class CDump;
class CDumpFilter;

// {8943BEB7-E0BC-453b-9EA5-EB93899FA51C}
DEFINE_GUID(CLSID_MPTSWriter, 
0x8943beb7, 0xe0bc, 0x453b, 0x9e, 0xa5, 0xeb, 0x93, 0x89, 0x9f, 0xa5, 0x1c);
// {236D0A77-D105-43fd-A203-578859AB7948}
DEFINE_GUID(IID_IMPTSWriter, 
0x236d0a77, 0xd105, 0x43fd, 0xa2, 0x3, 0x57, 0x88, 0x59, 0xab, 0x79, 0x48);

// interface
DECLARE_INTERFACE_(IMPTSWriter, IUnknown)
{
	STDMETHOD(ResetPids)()PURE;
	STDMETHOD(SetVideoPid)(THIS_ int videoPid)PURE;
	STDMETHOD(SetAudioPid)(THIS_ int audioPid)PURE;
	STDMETHOD(SetAudioPid2)(THIS_ int audioPid)PURE;
	STDMETHOD(SetAC3Pid)(THIS_ int ac3Pid)PURE;
	STDMETHOD(SetTeletextPid)(THIS_ int ttxtPid)PURE;
	STDMETHOD(SetSubtitlePid)(THIS_ int subtitlePid)PURE;
	STDMETHOD(SetPMTPid)(THIS_ int pmtPid)PURE;
	STDMETHOD(SetPCRPid)(THIS_ int pcrPid)PURE;
	STDMETHOD(TimeShiftBufferDuration)(THIS_ ULONGLONG* duration)PURE;
	STDMETHOD(IsStarted)(THIS_ int* yesNo)PURE;
};

// {3E05D715-0AE2-4d6a-8EE9-51DB5FBAB72B}
DEFINE_GUID(IID_IMPTSRecord,0x3e05d715, 0xae2, 0x4d6a, 0x8e, 0xe9, 0x51, 0xdb, 0x5f, 0xba, 0xb7, 0x2b);

// interface
DECLARE_INTERFACE_(IMPTSRecord, IUnknown)
{
    STDMETHOD(SetRecordingFileName)(THIS_ char* pszFileName)PURE;
    STDMETHOD(StartRecord)(THIS_ ULONGLONG timeFromTimeshiftBuffer)PURE;
    STDMETHOD(StopRecord)(THIS_ ULONGLONG startTime)PURE;
	STDMETHOD(SetAttribute)(THIS_ int attribNo, char* attribValue)PURE;
};
// Main filter object

class CDumpFilter : public CBaseFilter
{
    CDump * const m_pDump;

public:

    // Constructor
    CDumpFilter(CDump *pDump,
                LPUNKNOWN pUnk,
                CCritSec *pLock,
                HRESULT *phr);

    // Pin enumeration
    CBasePin * GetPin(int n);
    int GetPinCount();

    // Open and close the file as necessary
    STDMETHODIMP Run(REFERENCE_TIME tStart);
    STDMETHODIMP Pause();
    STDMETHODIMP Stop();
};


//  Pin object

class CDumpInputPin : public CRenderedInputPin
{
    CDump    * const	m_pDump;           // Main renderer object
    CCritSec * const	m_pReceiveLock;    // Sample critical section
    REFERENCE_TIME		m_tLast;             // Last sample receive time
	byte				m_restBuffer[200];
	int					m_restBufferLen;
public:

    CDumpInputPin(CDump *pDump,
                  LPUNKNOWN pUnk,
                  CBaseFilter *pFilter,
                  CCritSec *pLock,
                  CCritSec *pReceiveLock,
                  HRESULT *phr);

    // Do something with this media sample
    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream(void);
    STDMETHODIMP ReceiveCanBlock();

    // Write detailed information about this sample to a file
//    HRESULT WriteStringInfo(IMediaSample *pSample);

    // Check if the pin can support this specific proposed type and format
    HRESULT		CheckMediaType(const CMediaType *);
    // Break connection
    HRESULT		BreakConnect();

    // Track NewSegment
    STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);
	HRESULT 	SetVideoPid(int videoPid);
	HRESULT 	SetAudioPid(int audioPid);
	HRESULT 	SetAudioPid2(int audioPid);
	HRESULT 	SetAC3Pid(int ac3Pid);
	HRESULT 	SetTeletextPid(int ttxtPid);
	HRESULT 	SetSubtitlePid(int subtitlePid);
	HRESULT 	SetPMTPid(int pmtPid);
	HRESULT 	SetPCRPid(int pcrPid);
	int 		GetVideoPid();
	int 		GetAudioPid();
	int 		GetAudioPid2();
	int 		GetAC3Pid();
	int 		GetTeletextPid();
	int 		GetSubtitlePid();
	int 		GetPMTPid();
	int 		GetPCRPid();
	void		ResetPids();
private:
	CCritSec				m_section;
	int 		m_videoPid;
	int 		m_audio1Pid;
	int 		m_audio2Pid;
	int 		m_ac3Pid;
	int 		m_ttxtPid;
	int 		m_subtitlePid;
	int 		m_pmtPid;
	int 		m_pcrPid;
	bool		IsPidValid(int pid);
	bool		m_bResettingPids;
	
	bool		m_bUpdatePids;
	StreamState	m_audioState;
	StreamState	m_videoState;
	DWORD       m_audioTimer;
	DWORD       m_videoTimer;
};


//  CDump object which has filter and pin members

class CDump : public CUnknown, public IFileSinkFilter, public IMPTSWriter, public IMPTSRecord
{
	enum RecState
	{
		Idle,
		Copying,
		Following
	};
    friend class CDumpFilter;
    friend class CDumpInputPin;
    CDumpFilter*	m_pFilter;       // Methods for filter interfaces
    CDumpInputPin*	m_pPin;          // A simple rendered input pin
    CCritSec 		m_Lock;                // Main renderer critical section
    CCritSec 		m_ReceiveLock;         // Sublock for received samples
    CPosPassThru*	m_pPosition;      // Renderer position controls
    HANDLE   		m_hFile;               // Handle to file for dumping
	HANDLE   		m_hInfoFile;           // Handle to file for dumping
	HANDLE	 		m_logFileHandle;
	HANDLE	 		m_recHandle;
    LPOLESTR 		m_pFileName;           // The filename where we dump
    BOOL     		m_fWriteError;
	__int64 		m_currentFilePosition;
	__int64 		logFilePos;

	__int64         m_recStartPosition;
	RecState		m_recState;
	byte*			m_pCopyBuffer;
	int				m_pesPid;
public:
    DECLARE_IUNKNOWN

    CDump(LPUNKNOWN pUnk, HRESULT *phr);
    ~CDump();

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);
	STDMETHODIMP Log(__int64 value,bool crlf);
	STDMETHODIMP Log(char* text,bool crlf);

    // Write raw data stream to a file
	HRESULT 	 UpdateInfoFile(bool pids);
	void		 Flush();
	HRESULT		Clear();


    // Implements the IFileSinkFilter interface
    STDMETHODIMP SetFileName(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
    STDMETHODIMP GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	// interface methodes
	STDMETHODIMP ResetPids();
	STDMETHODIMP SetVideoPid(int videoPid);
	STDMETHODIMP SetAudioPid(int audioPid);
	STDMETHODIMP SetAudioPid2(int audioPid);
	STDMETHODIMP SetAC3Pid(int ac3Pid);
	STDMETHODIMP SetTeletextPid(int ttxtPid);
	STDMETHODIMP SetSubtitlePid(int subtitlePid);
	STDMETHODIMP SetPMTPid(int pmtPid);
	STDMETHODIMP SetPCRPid(int pcrPid);
	STDMETHODIMP TimeShiftBufferDuration(ULONGLONG* duration);
	STDMETHODIMP IsStarted(int* yesNo);
	//record
    STDMETHODIMP SetRecordingFileName(char* pszFileName);
    STDMETHODIMP StartRecord( ULONGLONG timeFromTimeshiftBuffer);
    STDMETHODIMP StopRecord( ULONGLONG startTime);
	STDMETHODIMP SetAttribute(int attribNo, char* attribValue);

	HRESULT 	WriteTimeshiftFile(PBYTE pbData, LONG lDataLength);
	HRESULT 	WriteRecordingFile(PBYTE pbData, LONG lDataLength);
	HRESULT     CopyRecordingFile();
	bool		IsCopyingRecordingFile();
private:

    // Overriden to say what interfaces we support where
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    // Open and write to the file
    HRESULT 	OpenFile();
    HRESULT 	CloseFile();
    HRESULT 	HandleWriteFailure();
//	HRESULT CreateNewFileMapping();
	void 		GetPTS(BYTE *data,ULONGLONG *pts);
	HRESULT		GetAdaptionHeader(BYTE *data,AdaptionHeader *header);
	void 		GetPESHeader(BYTE *data,PESHeader *header);
	void 		GetTSHeader(BYTE *data,TSHeader *header);
	void 		PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	void 		WriteAttributesToRecordingFile();

	ULONGLONG				m_pesStart;
	ULONGLONG				m_pesNow;
	char					m_strRecordingFileName[1024];
	map<__int64, __int64>	m_mapPES;
	typedef map<__int64, __int64> ::iterator imapPES;
	map<int,string>         m_mapAttributes;
	typedef map<int,string>::iterator imapAttributes;         
};
