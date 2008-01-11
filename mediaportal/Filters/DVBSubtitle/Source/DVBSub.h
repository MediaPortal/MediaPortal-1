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

#pragma once

#define ULONG_PTR DWORD
#include <windows.h>
#include <xprtdefs.h>
#include <streams.h>
#include <bdaiface.h>
#include <initguid.h>
#include <atlcomcli.h>

#include "dvbsubs\dvbsubdecoder.h"
#include "SubdecoderObserver.h"
#include "PidObserver.h"
#include "ITSFileSource.h"
#include "TSThread.h"
#include <vector>

class CSubtitleInputPin;
class CPcrInputPin;
class CPMTInputPin;
class CDVBFilterPin;

class CDVBSubDecoder;

typedef __int64 int64_t;

// {591AB987-9689-4c07-846D-0006D5DD2BFD}
DEFINE_GUID(CLSID_DVBSub,
	0x591ab987, 0x9689, 0x4c07, 0x84, 0x6d, 0x0, 0x6, 0xd5, 0xdd, 0x2b, 0xfd);

// C19647D5-A861-4845-97A6-EBD0A135D0BF
DEFINE_GUID(IID_IDVBSubtitle,
0xc19647d5, 0xa861, 0x4845, 0x97, 0xa6, 0xeb, 0xd0, 0xa1, 0x35, 0xd0, 0xbf);

struct __declspec(uuid("559E6E81-FAC4-4EBC-9530-662DAA27EDC2")) ITSFileSource;

// structure used to communicate subtitles to MediaPortal's managed code
struct SUBTITLE
{
  // Subtitle bitmap
  LONG        bmType;
  LONG        bmWidth;
  LONG        bmHeight;
  LONG        bmWidthBytes;
  WORD        bmPlanes;
  WORD        bmBitsPixel;
  LPVOID      bmBits;

  unsigned    __int64 timestamp;
  unsigned    __int64 timeOut;
  int         firstScanLine;
};

DECLARE_INTERFACE_( IDVBSubtitle, IUnknown )
{
  //STDMETHOD(GetSubtitle) ( int place, SUBTITLE* pSubtitle ) PURE;
  //STDMETHOD(GetSubtitleCount) ( int* count ) PURE;
  STDMETHOD(SetCallback) ( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub) ) PURE;
  STDMETHOD(SetTimestampResetCallback)( int (CALLBACK *pSubtitleObserver)() ) PURE;
  //STDMETHOD(DiscardOldestSubtitle) () PURE;
  STDMETHOD(Test)(int status) PURE;
};

enum PinMappingState
{
  PidNotAvailable = 0,
  PidAvailable,
  PidMapped
};

struct PinMappingInfo
{
  PinMappingState mappingState;
  ULONG pid;
  MEDIA_SAMPLE_CONTENT sampleContent;
};


extern void LogDebug( const char *fmt, ... );

class CDVBSub : public CBaseFilter, public MSubdecoderObserver, IDVBSubtitle, TSThread
{
public:
  // Constructor & destructor
  CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock );
  ~CDVBSub();

  // Methods from directshow base classes
  STDMETHODIMP Run( REFERENCE_TIME tStart );
	STDMETHODIMP Pause();
	STDMETHODIMP Stop();
  CBasePin * GetPin( int n );
  int GetPinCount();

  virtual HRESULT STDMETHODCALLTYPE GetSubtitle( int place, SUBTITLE* pSubtitle );
  virtual HRESULT STDMETHODCALLTYPE DiscardOldestSubtitle();
  virtual HRESULT STDMETHODCALLTYPE GetSubtitleCount( int* count );

  // IDVBSubtitle
  virtual HRESULT STDMETHODCALLTYPE SetCallback( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub) );
  virtual HRESULT STDMETHODCALLTYPE SetTimestampResetCallback( int (CALLBACK *pTimestampResetObserver)() );
  virtual HRESULT STDMETHODCALLTYPE Test(int status);

  // IUnknown
  DECLARE_IUNKNOWN;

  STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

  // From MSubdecoderObserver
	void NotifySubtitle();
  void NotifyFirstPTS( ULONGLONG firstPTS );

  void SetSubtitlePid( LONG pid );

  static CUnknown * WINAPI CreateInstance( LPUNKNOWN pUnk, HRESULT *pHr );

  void SetPcr( ULONGLONG pcr );
  void NotifySeeking();

  void Event();

  // Worker thread related functions
  void ThreadProc();
  HRESULT SetPid( CBaseInputPin* pin, LONG pid, MEDIA_SAMPLE_CONTENT sampleContent );
  HRESULT MapPidToDemuxer( CBaseInputPin* pPin, LONG pid, MEDIA_SAMPLE_CONTENT sampleContent );

private:

  void Reset();
  void LogDebugMediaPosition( const char *text );
  HRESULT ConnectToTSFileSource();
  HRESULT FindVideoPID();

public:

  CSubtitleInputPin*  m_pSubtitlePin;
	CPcrInputPin*		    m_pPCRPin;
  CPMTInputPin*       m_pPMTPin;

private: // data

  int m_VideoPid;

  CDVBSubDecoder*     m_pSubDecoder;      // Subtitle decoder
	IMediaFilter*       m_pMediaFilter;     
  IMediaSeeking*      m_pIMediaSeeking;   // Media seeking interface
  IReferenceClock*    m_pReferenceClock;
  CAMEvent            m_TimerEvent;       // Used to signal timer events
  CCritSec            m_Lock;				      // Main renderer critical section
  CCritSec            m_ReceiveLock;		  // Sublock for received samples

  LONGLONG            m_basePCR;          // Base PCR from TSFileSource
  LONGLONG            m_firstPCR;
  LONGLONG            m_curPCR;           // Current PCR from PCR InputPin
  LONGLONG            m_fixPCR;           // diff between TSFileSouce first PCR and PCRInputPin PCR
  LONGLONG            m_seekDifPCR;       
  REFERENCE_TIME      m_startTimestamp;

  int                 (CALLBACK *m_pSubtitleObserver) (SUBTITLE* sub);
  int                 (CALLBACK *m_pTimestampResetObserver) ();

  ITSFileSource*      m_pTSFileSource;

  bool                m_bSeekingDone;

  std::vector<PinMappingInfo> m_SubtitlePinMapping;
  std::vector<PinMappingInfo> m_PCRPinMapping;
  std::vector<PinMappingInfo> m_PMTPinMapping;
};
