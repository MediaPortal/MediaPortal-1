/*
 *	Copyright (C) 2006-2007 Team MediaPortal
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
#include <vector>

class CSubtitleInputPin;
class CPcrInputPin;
class CPMTInputPin;
class CDVBFilterPin;

class CDVBSubDecoder;

typedef __int64 int64_t;

// {1CF3606B-6F89-4813-9D05-F9CA324CF2EA}
DEFINE_GUID(CLSID_DVBSub2, 
  0x1cf3606b, 0x6f89, 0x4813, 0x9d, 0x5, 0xf9, 0xca, 0x32, 0x4c, 0xf2, 0xea);

// {901C9084-246A-47c9-BBCD-F8F398D30AB0}
DEFINE_GUID(IID_IDVBSubtitle2, 
  0x901c9084, 0x246a, 0x47c9, 0xbb, 0xcd, 0xf8, 0xf3, 0x98, 0xd3, 0xa, 0xb0);


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

class CDVBSub : public CBaseFilter, public MSubdecoderObserver, IDVBSubtitle
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

private:

  void Reset();
  void LogDebugMediaPosition( const char *text );

public:

  CSubtitleInputPin*  m_pSubtitlePin;

private: // data

  int m_VideoPid;

  CDVBSubDecoder*     m_pSubDecoder;      // Subtitle decoder
	IMediaFilter*       m_pMediaFilter;     
  IMediaSeeking*      m_pIMediaSeeking;   // Media seeking interface
  IReferenceClock*    m_pReferenceClock;
  CCritSec            m_Lock;				      // Main renderer critical section
  CCritSec            m_ReceiveLock;		  // Sublock for received samples

  REFERENCE_TIME      m_startTimestamp;

  int                 (CALLBACK *m_pSubtitleObserver) (SUBTITLE* sub);
  int                 (CALLBACK *m_pTimestampResetObserver) ();

  bool                m_bSeekingDone;
};