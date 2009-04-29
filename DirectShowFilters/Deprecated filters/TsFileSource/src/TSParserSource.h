/**
*  TSParserSource.h
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSParserSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSParserSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSParserSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSParserSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef TSPARSERSOURCE_H
#define TSPARSERSOURCE_H

// Define a typedef for a list of filters.
typedef CGenericList<IBaseFilter> CFilterList;

#include <objbase.h>
#include "ITSParserSource.h"

class CTSParserSourceFilter;
#include "TSParserSourcePin.h"
#include "TSParserInputPin.h"
#include "TSFileSourceClock.h"

#include "RegStore.h"
#include "SettingsStore.h"
#include "TunerEvent.h"
#include "PidInfo.h"
#include "PidParser.h"
#include "MemReader.h"
#include "MultiMemReader.h"
#include "Demux.h"
#include "StreamInfo.h"
#include "StreamParser.h"
#include "NetRender.h"
#include "NetInfo.h"
#include "DVBMpeg2DataParser.h"
#include <comdef.h>
#include "BDAiface.h"
#include "SharedMemory.h"


/**********************************************
 *
 *  CTSParserSourceFilter Class
 *
 **********************************************/

class CTSParserSourceFilter : public CSource,
							public IFileSourceFilter,
							public ITSParserSource,
							public IAMStreamSelect,
							public IAMFilterMiscFlags,
							protected CAMThread,
							public IAsyncReader,
							public ISpecifyPropertyPages, public IBDA_DeviceControl, public IBDA_Topology, public UpdateThread
{
	friend class CTSParserSourcePin;
	friend class CTSParserInputPin;
public:
	DECLARE_IUNKNOWN
	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:
	//private contructor because CreateInstance creates this object
	CTSParserSourceFilter(IUnknown *pUnk, HRESULT *phr);
	~CTSParserSourceFilter();

	// Overriden to say what interfaces we support where
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
//	STDMETHODIMP_(ULONG) NonDelegatingRelease();
	HRESULT AddGraphToRot(IUnknown *pUnkGraph, DWORD *pdwRegister); 
	void RemoveGraphFromRot(DWORD pdwRegister);
	void set_ROTMode();
	HRESULT GetObjectFromROT(WCHAR* wsFullName, IUnknown **ppUnk);

public:
	// Pin enumeration
	CBasePin * GetPin(int n);
	int GetPinCount();
	STDMETHODIMP FindPin(LPCWSTR Id, IPin ** ppPin);

	void ResetStreamTime(void);
	BOOL is_Active(void);
	STDMETHODIMP Run(REFERENCE_TIME tStart);
	STDMETHODIMP Pause();
	STDMETHODIMP Stop();


	HRESULT FileSeek(REFERENCE_TIME seektime);

	HRESULT LoadPgmReg(void);
	HRESULT GetFileSize(__int64 *pStartPosition, __int64 *pEndPosition);
	HRESULT OnConnect();
	HRESULT RefreshPids();
	HRESULT RefreshDuration();
	STDMETHODIMP ShowFilterProperties();
	STDMETHODIMP Refresh();
	HRESULT UpdatePidParser(FileReader *pFileReader);
	BOOL get_AutoMode();
	BOOL get_PinMode();
	NetInfoArray netArray;

protected:

	//IBDA_DeviceControl
	STDMETHODIMP StartChanges(void);
	STDMETHODIMP CheckChanges(void);
	STDMETHODIMP CommitChanges(void);
	STDMETHODIMP GetChangeState(ULONG *pState);

	STDMETHODIMP GetNodeTypes(ULONG *pulcNodeTypes, ULONG ulcNodeTypesMax, ULONG rgulNodeTypes[  ]);
	STDMETHODIMP GetNodeDescriptors(ULONG *ulcNodeDescriptors, ULONG ulcNodeDescriptorsMax, BDANODE_DESCRIPTOR rgNodeDescriptors[  ]);
	STDMETHODIMP GetNodeInterfaces(ULONG ulNodeType, ULONG *pulcInterfaces, ULONG ulcInterfacesMax, GUID rgguidInterfaces[  ]);
	STDMETHODIMP GetPinTypes(ULONG *pulcPinTypes, ULONG ulcPinTypesMax, ULONG rgulPinTypes[  ]);
	STDMETHODIMP GetTemplateConnections(ULONG *pulcConnections, ULONG ulcConnectionsMax, BDA_TEMPLATE_CONNECTION rgConnections[  ]);
	STDMETHODIMP CreatePin(ULONG ulPinType, ULONG *pulPinId);
	STDMETHODIMP DeletePin(ULONG ulPinId);
	STDMETHODIMP SetMediaType(ULONG ulPinId, AM_MEDIA_TYPE *pMediaType);
	STDMETHODIMP SetMedium(ULONG ulPinId, REGPINMEDIUM *pMedium);
	STDMETHODIMP CreateTopology(ULONG ulInputPinId, ULONG ulOutputPinId);
	STDMETHODIMP GetControlNode(ULONG ulInputPinId, ULONG ulOutputPinId, ULONG ulNodeType, IUnknown **ppControlNode);
        
	//IAMFilterMiscFlags
	virtual ULONG STDMETHODCALLTYPE  GetMiscFlags(void);

	// IAMStreamSelect
	STDMETHODIMP Count(DWORD *pcStreams);
	STDMETHODIMP Info( long lIndex,
					AM_MEDIA_TYPE **ppmt,
					DWORD *pdwFlags,
					LCID *plcid,
					DWORD *pdwGroup,
					WCHAR **ppszName,
					IUnknown **ppObject,
					IUnknown **ppUnk);
	STDMETHODIMP  Enable(long lIndex, DWORD dwFlags);

	// ISpecifyPropertyPages
	STDMETHODIMP GetPages(CAUUID *pPages);

	// IFileSourceFilter
	STDMETHODIMP Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
//	STDMETHODIMP GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);

	// ITSParserSource
	STDMETHODIMP GetVideoPid(WORD *pVPid);
	STDMETHODIMP GetAudioPid(WORD *pAPid);
	STDMETHODIMP GetAudio2Pid(WORD *pA2Pid);
	STDMETHODIMP GetAC3Pid(WORD *pAC3Pid);
	STDMETHODIMP GetAC3_2Pid(WORD *pAC3_2Pid);
	STDMETHODIMP GetTelexPid(WORD *pTelexPid);
	STDMETHODIMP GetNIDPid(WORD *pNIDPid);
	STDMETHODIMP GetNetworkName(BYTE *pointer);
	STDMETHODIMP GetChannelNumber (BYTE *pointer);
	STDMETHODIMP GetONIDPid(WORD *pONIDPid);
	STDMETHODIMP GetONetworkName(BYTE *pointer);
	STDMETHODIMP GetChannelName(BYTE *pointer);
	STDMETHODIMP GetTSIDPid(WORD *pTSIDPid);
	STDMETHODIMP GetEPGFromFile(void);
	STDMETHODIMP GetShortNextDescr(BYTE * POINTER);
	STDMETHODIMP GetExtendedNextDescr(BYTE * POINTER);
	STDMETHODIMP GetPMTPid(WORD *pPMTPid);
	STDMETHODIMP GetSIDPid(WORD *pSIDPid);
	STDMETHODIMP GetPCRPid(WORD * pcrpid);
	STDMETHODIMP GetDuration(REFERENCE_TIME * dur);
	STDMETHODIMP GetShortDescr(BYTE * POINTER);
	STDMETHODIMP GetExtendedDescr(BYTE * POINTER);
	STDMETHODIMP GetPgmNumb(WORD * pPgmNumb);
	STDMETHODIMP GetPgmCount(WORD * pPgmCount);
	STDMETHODIMP SetPgmNumb(WORD pPgmNumb);
	STDMETHODIMP NextPgmNumb(void);
	STDMETHODIMP PrevPgmNumb(void);
	STDMETHODIMP GetTsArray(ULONG * pPidArray);
	STDMETHODIMP GetAC3Mode(WORD* pAC3Mode);
	STDMETHODIMP SetAC3Mode(WORD AC3Mode);
	STDMETHODIMP GetMP2Mode(WORD* pMP2Mode);
	STDMETHODIMP SetMP2Mode(WORD MP2Mode);
	STDMETHODIMP GetAudio2Mode(WORD* pAudio2Mode);
	STDMETHODIMP SetAudio2Mode(WORD Audio2Mode);
	STDMETHODIMP GetAutoMode(WORD* pAutoMode);
	STDMETHODIMP SetAutoMode(WORD AutoMode);
	STDMETHODIMP GetNPControl(WORD* pNPControl);
	STDMETHODIMP SetNPControl(WORD NPControl);
	STDMETHODIMP GetNPSlave(WORD* pNPSlave);
	STDMETHODIMP SetNPSlave(WORD NPSlave);
	STDMETHODIMP GetDelayMode(WORD* pDelayMode);
	STDMETHODIMP SetDelayMode(WORD DelayMode);
	STDMETHODIMP GetRateControlMode(WORD* pRateControl);
	STDMETHODIMP SetRateControlMode(WORD RateControl);
	STDMETHODIMP GetCreateTSPinOnDemux(WORD *pbCreatePin);
	STDMETHODIMP SetCreateTSPinOnDemux(WORD bCreatePin);
	STDMETHODIMP GetReadOnly(WORD* pFileMode);
	STDMETHODIMP GetBitRate(long* pRate);
	STDMETHODIMP SetBitRate(long Rate);
	STDMETHODIMP SetRegStore(LPTSTR nameReg);
	STDMETHODIMP GetRegStore(LPTSTR nameReg);
	STDMETHODIMP SetTunerEvent(void);
	STDMETHODIMP SetRegSettings();
	STDMETHODIMP GetRegSettings();
	STDMETHODIMP SetRegProgram();
	STDMETHODIMP GetROTMode(WORD *ROTMode);
	STDMETHODIMP SetROTMode(WORD ROTMode);
	STDMETHODIMP GetClockMode(WORD *ClockMode);
	STDMETHODIMP SetClockMode(WORD ClockMode);

//New method added after 2.0.1.8
	STDMETHODIMP GetVideoPidType(BYTE * POINTER);
	STDMETHODIMP ShowEPGInfo();
	STDMETHODIMP GetAACPid(WORD *pAacPid);
	STDMETHODIMP GetAAC2Pid(WORD *pAac2Pid);
	STDMETHODIMP GetCreateTxtPinOnDemux(WORD *pbCreatePin);
	STDMETHODIMP SetCreateTxtPinOnDemux(WORD bCreatePin);
	STDMETHODIMP ReLoad(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetPCRPosition(REFERENCE_TIME *pos);
	STDMETHODIMP ShowStreamMenu(HWND hwnd);

//New method added after 2.2.0.3
	STDMETHODIMP GetFixedAspectRatio(WORD *pbFixedAR);
	STDMETHODIMP SetFixedAspectRatio(WORD pbFixedAR);

//New method added after 2.2.0.6
	STDMETHODIMP GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetDTSPid(WORD *pAacPid);
	STDMETHODIMP GetDTS2Pid(WORD *pAac2Pid);
	STDMETHODIMP GetSubtitlePid(WORD *pSubPid);
	STDMETHODIMP GetCreateSubPinOnDemux(WORD *pbCreatePin);
	STDMETHODIMP SetCreateSubPinOnDemux(WORD bCreatePin);

//New method added after 2.2.0.8
	STDMETHODIMP GetSharedMode(WORD* pSharedMode);
	STDMETHODIMP SetSharedMode(WORD SharedMode);
	STDMETHODIMP GetInjectMode(WORD* pInjectMode);
	STDMETHODIMP SetInjectMode(WORD InjectMode);

	CFilterList m_FilterRefList;	// List to hold the Removed filters.string

protected:

	HRESULT load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt);
	HRESULT set_PgmNumb(WORD PgmNumb);
	HRESULT set_TunerEvent(void);
	HRESULT set_RegProgram();
	HRESULT showEPGInfo();


	SharedMemory* m_pSharedMemory;
	DVBMpeg2DataParser *m_pMpeg2DataParser;
	CTSParserSourcePin *m_pPin;          // A simple rendered output pin
	CTSParserInputPin *m_pInpPin;         // A simple rendered Input pin
	CTSFileSourceClock *m_pClock;
	CRegStore *m_pRegStore;
	CSettingsStore *m_pSettingsStore;
	TunerEvent *m_pTunerEvent;
	PidParser *m_pPidParser;
	StreamParser *m_pStreamParser;
	FileReader *m_pFileReader;
	Demux *m_pDemux;
	BOOL m_bRotEnable;
	BOOL m_bSharedMode;
	BOOL m_bColdStart;
	CCritSec m_Lock;                // Main renderer critical section
	CCritSec m_SelectLock;                // Main renderer critical section
    
    DWORD m_dwGraphRegister;		//registration number for the RunningObjectTable

	FileReader *m_pFileDuration;
//	REFERENCE_TIME m_rtLastCurrentTime;
	BOOL m_bThreadRunning;
	BOOL m_bReload;
	__int64 m_llLastMultiFileStart;
	__int64 m_llLastMultiFileLength;
    enum Command {CMD_INIT, CMD_PAUSE, CMD_RUN, CMD_STOP, CMD_EXIT};
	void UpdateThreadProc(void);
	BOOL m_WriteThreadActive;
    DWORD ThreadProc();
	HRESULT DoProcessingLoop(void);
	BOOL ThreadRunning(void);
    Command GetRequest(void) { return (Command) CAMThread::GetRequest(); }
    BOOL    CheckRequest(Command *pCom) { return CAMThread::CheckRequest( (DWORD *) pCom); }

	ParserFunctions parserFunctions;
	DVBTChannels *m_pDVBTChannels;


//*****************************************************************************************
//ASync Additions


    // --- IPin methods ---
//	STDMETHODIMP Connect(IPin * pReceivePin, const AM_MEDIA_TYPE *pmt);

//    CAsyncIo *m_pIo;
    BOOL  m_bQueriedForAsyncReader;

//	virtual HRESULT InitAllocator(IMemAllocator **ppAlloc);
//	HRESULT	DecideAllocator(IMemInputPin *pPin, IMemAllocator **ppAlloc);

    // --- IAsyncReader methods ---
    // pass in your preferred allocator and your preferred properties.
    // method returns the actual allocator to be used. Call GetProperties
    // on returned allocator to learn alignment and prefix etc chosen.
    // this allocator will be not be committed and decommitted by
    // the async reader, only by the consumer.
    STDMETHODIMP RequestAllocator(
                      IMemAllocator* pPreferred,
                      ALLOCATOR_PROPERTIES* pProps,
                      IMemAllocator ** ppActual);

    // queue a request for data.
    // media sample start and stop times contain the requested absolute
    // byte position (start inclusive, stop exclusive).
    // may fail if sample not obtained from agreed allocator.
    // may fail if start/stop position does not match agreed alignment.
    // samples allocated from source pin's allocator may fail
    // GetPointer until after returning from WaitForNext.
    STDMETHODIMP Request(
                     IMediaSample* pSample,
                     DWORD_PTR dwUser);         // user context

    // block until the next sample is completed or the timeout occurs.
    // timeout (millisecs) may be 0 or INFINITE. Samples may not
    // be delivered in order. If there is a read error of any sort, a
    // notification will already have been sent by the source filter,
    // and STDMETHODIMP will be an error.
    STDMETHODIMP WaitForNext(
                      DWORD dwTimeout,
                      IMediaSample** ppSample,  // completed sample
                      DWORD_PTR * pdwUser);     // user context

    // sync read of data. Sample passed in must have been acquired from
    // the agreed allocator. Start and stop position must be aligned.
    // equivalent to a Request/WaitForNext pair, but may avoid the
    // need for a thread on the source filter.
    STDMETHODIMP SyncReadAligned(
                      IMediaSample* pSample);


    // sync read. works in stopped state as well as run state.
    // need not be aligned. Will fail if read is beyond actual total
    // length.
    STDMETHODIMP SyncRead(
                      LONGLONG llPosition,  // absolute file position
                      LONG lLength,         // nr bytes required
                      BYTE* pBuffer);       // write data here

    // return total length of stream, and currently available length.
    // reads for beyond the available length but within the total length will
    // normally succeed but may block for a long period.
    STDMETHODIMP Length(
                      LONGLONG* pTotal,
                      LONGLONG* pAvailable);

    // cause all outstanding reads to return, possibly with a failure code
    // (VFW_E_TIMEOUT) indicating they were cancelled.
    // these are defined on IAsyncReader and IPin
    STDMETHODIMP BeginFlush(void);
    STDMETHODIMP EndFlush(void);

//*****************************************************************************************
};

#endif
