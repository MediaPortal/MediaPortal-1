/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#include "..\..\shared\packetsync.h"
#include "videoanalyzer.h"
#include "channelscan.h"
#include "epgscanner.h"
#include "pmtgrabber.h"
#include "DiskRecorder.h"
#include "teletextgrabber.h"
#include "cagrabber.h"
#include "channellinkagescanner.h"
#include "tschannel.h"
#include "videoaudioobserver.h"
#include <map>
#include <vector>
using namespace std;

class CMpTsFilterPin;
class CMpOobSiFilterPin;
class CMpTs;
class CMpTsFilter;

DEFINE_GUID(CLSID_MpTsFilter, 0xfc50bed6, 0xfe38, 0x42d3, 0xb8, 0x31, 0x77, 0x16, 0x90, 0x9, 0x1a, 0x6e);

// {5EB9F392-E7FD-4071-8E44-3590E5E767BA}
DEFINE_GUID(IID_TSFilter, 0x5eb9f392, 0xe7fd, 0x4071, 0x8e, 0x44, 0x35, 0x90, 0xe5, 0xe7, 0x67, 0xba);

DECLARE_INTERFACE_(ITSFilter, IUnknown)
{
	STDMETHOD(AddChannel)(THIS_ int* handle)PURE;
  STDMETHOD(DeleteChannel)(THIS_ int handle)PURE;
  STDMETHOD(DeleteAllChannels)()PURE;

	STDMETHOD(AnalyzerSetVideoPid)(THIS_ int handle, int videoPid)PURE;
	STDMETHOD(AnalyzerGetVideoPid)(THIS_ int handle,  int* videoPid)PURE;
	STDMETHOD(AnalyzerSetAudioPid)(THIS_ int handle,  int audioPid)PURE;
	STDMETHOD(AnalyzerGetAudioPid)(THIS_ int handle,  int* audioPid)PURE;
	STDMETHOD(AnalyzerIsVideoEncrypted)(THIS_ int handle,  int* yesNo)PURE;
	STDMETHOD(AnalyzerIsAudioEncrypted)(THIS_ int handle,  int* yesNo)PURE;
	STDMETHOD(AnalyzerReset)(THIS_ int handle )PURE;

	
	STDMETHOD(PmtSetPmtPid)(THIS_ int handle,  int pmtPid, long serviceId);
	STDMETHOD(PmtSetCallBack)(THIS_ int handle,   IPMTCallback* callback);
	STDMETHOD(PmtGetPMTData) (THIS_ int handle,   BYTE *pmtData);

	
  STDMETHOD(RecordSetRecordingFileNameW)(THIS_ int handle,wchar_t* pwszFileName)PURE;
  STDMETHOD(RecordStartRecord)(THIS_ int handle)PURE;
  STDMETHOD(RecordStopRecord)(THIS_ int handle)PURE;
	STDMETHOD(RecordSetPmtPid)(THIS_ int handle,int mtPid,int serviceId,byte* pmtData,int pmtLength)PURE;
	STDMETHOD(RecordSetVideoAudioObserver)(THIS_ int handle, IVideoAudioObserver* callback)PURE;

	STDMETHOD(TimeShiftSetTimeShiftingFileNameW)(THIS_ int handle, wchar_t* pwszFileName)PURE;
	STDMETHOD(TimeShiftStart)(THIS_ int handle )PURE;
	STDMETHOD(TimeShiftStop)(THIS_ int handle )PURE;
	STDMETHOD(TimeShiftReset)(THIS_ int handle )PURE;
	STDMETHOD(TimeShiftGetBufferSize) (THIS_ int handle, long * size) PURE;
	STDMETHOD(TimeShiftSetPmtPid) (THIS_ int handle, int pmtPid,int serviceId,byte* pmtData,int pmtLength) PURE;
	STDMETHOD(TimeShiftPause) (THIS_ int handle, BYTE onOff) PURE;
	STDMETHOD(TimeShiftSetParams) (THIS_ int handle, int minFiles, int maxFiles, ULONG chunkSize) PURE;
	STDMETHOD(TimeShiftGetCurrentFilePosition) (THIS_ int handle,__int64 * position, long * bufferId) PURE;
	STDMETHOD(SetVideoAudioObserver)(THIS_ int handle, IVideoAudioObserver* callback)PURE;

	STDMETHOD(TTxStart)(THIS_ int handle)PURE;
	STDMETHOD(TTxStop)(THIS_ int handle )PURE;
	STDMETHOD(TTxSetTeletextPid)(THIS_ int handle,int teletextPid)PURE;
	STDMETHOD(TTxSetCallBack)(THIS_ int handle,ITeletextCallBack* callback)PURE;

  STDMETHOD(CaSetCallBack)(THIS_ int handle,ICACallback* callback)PURE;
	STDMETHOD(CaGetCaData) (THIS_ int handle,BYTE *caData)PURE;
	STDMETHOD(CaReset)(THIS_ int handle)PURE;

  STDMETHOD(GetStreamQualityCounters)(THIS_ int handle, int* totalTsBytes, int* totalRecordingBytes, 
      int* TsDiscontinuity, int* recordingDiscontinuity)PURE;
  
    STDMETHOD(TimeShiftSetChannelType)(THIS_ int handle, int channelType)PURE;
};

// Main filter object

class CMpTsFilter : public CBaseFilter
{

public:

    CMpTsFilter(CMpTs* pDump, LPUNKNOWN pUnk, CCritSec* pFilterLock, CCritSec* pReceiveLock, HRESULT* pHr);

 CBasePin * GetPin(int n);
    int GetPinCount();

    STDMETHODIMP Run(REFERENCE_TIME tStart);
    STDMETHODIMP Pause();
    STDMETHODIMP Stop();

	private:
    CMpTs* const m_pWriterFilter;
    CCritSec* m_pReceiveLock;

};


//  Pin object

class CMpTsFilterPin : public CRenderedInputPin,public CPacketSync
{
public:
	CMpTsFilterPin(CMpTs* pDump, LPUNKNOWN pUnk, CBaseFilter* pFilter, CCritSec* pFilterLock, CCritSec* pReceiveLock, HRESULT* phr);

    // Do something with this media sample
    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream();
    STDMETHODIMP ReceiveCanBlock();

	HRESULT CheckMediaType(const CMediaType* pMediaType);
    HRESULT		BreakConnect();
		void			Reset();

	STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);

		//CPacketSync overrides
		void OnTsPacket(byte* tsPacket);
		void AssignRawPacketWriter(FileWriter* pRawPacketWriter);
private:
	CMpTs* const m_pWriterFilter;
    CCritSec* m_pReceiveLock;
    FileWriter* m_pRawPacketWriter;
};


// This pin is designed to receive digital cable service information, normally carried out of band.
// The format of these sections is specified in ANSI/SCTE 65.
class CMpOobSiFilterPin : public CRenderedInputPin
{
  public:
    CMpOobSiFilterPin(CMpTs* pDump, LPUNKNOWN pUnk, CBaseFilter* pFilter, CCritSec* pFilterLock, CCritSec* pReceiveLock, HRESULT* phr);

    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream();
    STDMETHODIMP ReceiveCanBlock();
	
    HRESULT CheckMediaType(const CMediaType* pMediaType);
    HRESULT BreakConnect();
    STDMETHODIMP NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);

    void AssignRawSectionWriter(FileWriter* pRawSectionWriter);

  private:
    CMpTs* const m_pWriterFilter;
    CCritSec* m_pReceiveLock;
    FileWriter* m_pRawSectionWriter;
};


//  CMpTs object which has filter and pin members

class CMpTs : public CUnknown, public ITSFilter
{

    friend class CMpTsFilter;
    friend class CMpTsFilterPin;
    friend class CMpOobSiFilterPin;
    CMpTsFilter*	m_pFilter;       // Methods for filter interfaces
    CMpTsFilterPin*	m_pPin;          // A simple rendered input pin
    CMpOobSiFilterPin*	m_pOobSiPin; // A simple rendered input pin for receiving out-of-band SI sections.
    CCritSec m_filterLock;          // Main renderer critical section
    CCritSec m_receiveLock;         // Streaming critical section
public:
    DECLARE_IUNKNOWN

		STDMETHODIMP AddChannel( int* handle);
    STDMETHODIMP DeleteChannel( int handle);
    STDMETHODIMP DeleteAllChannels();

		STDMETHODIMP AnalyzerSetVideoPid(int handle, int videoPid);
		STDMETHODIMP AnalyzerGetVideoPid(int handle,  int* videoPid);
		STDMETHODIMP AnalyzerSetAudioPid(int handle,  int audioPid);
		STDMETHODIMP AnalyzerGetAudioPid(int handle,  int* audioPid);
		STDMETHODIMP AnalyzerIsVideoEncrypted(int handle,  int* yesNo);
		STDMETHODIMP AnalyzerIsAudioEncrypted(int handle,  int* yesNo);
		STDMETHODIMP AnalyzerReset(int handle );

		STDMETHODIMP PmtSetPmtPid(int handle,int pmtPid, long serviceId);
		STDMETHODIMP PmtSetCallBack(int handle,IPMTCallback* callback);
		STDMETHODIMP PmtGetPMTData (int handle,BYTE *pmtData);


		STDMETHODIMP RecordSetRecordingFileNameW( int handle,wchar_t* pszFileName);
		STDMETHODIMP RecordStartRecord( int handle);
		STDMETHODIMP RecordStopRecord( int handle);
		STDMETHODIMP RecordSetPmtPid(int handle,int mtPid,int serviceId,byte* pmtData,int pmtLength );
		STDMETHODIMP RecordSetVideoAudioObserver(int handle, IVideoAudioObserver* callback);

		STDMETHODIMP TimeShiftSetTimeShiftingFileNameW( int handle, wchar_t* pwszFileName);
		STDMETHODIMP TimeShiftStart( int handle );
		STDMETHODIMP TimeShiftStop( int handle );
		STDMETHODIMP TimeShiftReset( int handle );
		STDMETHODIMP TimeShiftGetBufferSize( int handle, long * size) ;
		STDMETHODIMP TimeShiftSetPmtPid( int handle, int pmtPid, int serviceId,byte* pmtData,int pmtLength) ;
		STDMETHODIMP TimeShiftPause( int handle, BYTE onOff) ;
		STDMETHODIMP TimeShiftSetParams(int handle, int minFiles, int maxFiles, ULONG chunkSize) ;
		STDMETHODIMP TimeShiftGetCurrentFilePosition(int handle,__int64 * position,long * bufferId);
		STDMETHODIMP SetVideoAudioObserver(int handle, IVideoAudioObserver* callback);

		STDMETHODIMP TTxStart( int handle);
		STDMETHODIMP TTxStop( int handle );
		STDMETHODIMP TTxSetTeletextPid( int handle,int teletextPid);
		STDMETHODIMP TTxSetCallBack( int handle,ITeletextCallBack* callback);

    STDMETHODIMP CaSetCallBack(int handle,ICACallback* callback);
	  STDMETHODIMP CaGetCaData(int handle,BYTE *caData);
	  STDMETHODIMP CaReset(int handle);
    STDMETHODIMP GetStreamQualityCounters(int handle, int* totalTsBytes, int* totalRecordingBytes, 
      int* TsDiscontinuity, int* recordingDiscontinuity);

		STDMETHODIMP TimeShiftSetChannelType(int handle, int channelType);

    CMpTs(LPUNKNOWN pUnk, HRESULT *phr);
    ~CMpTs();
    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);
		void AnalyzeTsPacket(byte* tsPacket);
    void AnalyzeOobSiSection(CSection& section);

private:
    // Overriden to say what interfaces we support where
		CTsChannel* GetTsChannel(int handle);	
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
		CChannelScan*   m_pChannelScanner;
		CEpgScanner*		m_pEpgScanner;
		FileWriter* m_pRawPacketWriter;
        bool b_dumpRawPackets;
		CChannelLinkageScanner* m_pChannelLinkageScanner;
		vector<CTsChannel*> m_vecChannels;
		CCritSec m_channelLock;         // Lock for protecting access to m_vecChannels.
    typedef vector<CTsChannel*>::iterator ivecChannels;
		int m_id;
};
