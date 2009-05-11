/*
 *	Copyright (C) 2006-2009 Team MediaPortal
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

#include "IDVBSub.h"

class CSubtitleInputPin;
class CDVBSubDecoder;

typedef __int64 int64_t;

extern void LogDebug( const char *fmt, ... );

class CDVBSub : public CBaseFilter, public MSubdecoderObserver, IDVBSubtitle, IDVBSubtitleSource
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

  // IDVBSubtitleSource
  virtual HRESULT STDMETHODCALLTYPE StatusTest( int status );
  virtual HRESULT STDMETHODCALLTYPE SetBitmapCallback( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub));
  virtual HRESULT STDMETHODCALLTYPE SetResetCallback( int (CALLBACK *pResetObserver)() );
  
  // IDVBSubtitle
  virtual HRESULT STDMETHODCALLTYPE SetUpdateTimeoutCallback( int (CALLBACK *pUpdateTimeoutObserver)(__int64* pTimeout) );
  virtual HRESULT STDMETHODCALLTYPE Test( int status );
  virtual HRESULT STDMETHODCALLTYPE SetSubtitlePid( LONG pPid );
  virtual HRESULT STDMETHODCALLTYPE SetFirstPcr( LONGLONG pPcr );
  virtual HRESULT STDMETHODCALLTYPE SeekDone( CRefTime& rtSeek );
  virtual HRESULT STDMETHODCALLTYPE SetTimeCompensation( CRefTime& rtCompensation );
  virtual HRESULT STDMETHODCALLTYPE NotifyChannelChange();

  // IUnknown
  DECLARE_IUNKNOWN;
  STDMETHODIMP NonDelegatingQueryInterface( REFIID riid, void ** ppv );

  // From MSubdecoderObserver
  void NotifySubtitle();
  void UpdateSubtitleTimeout( uint64_t pTimeout );
  
  void NotifySeeking();

  static CUnknown * WINAPI CreateInstance( LPUNKNOWN pUnk, HRESULT *pHr );

#ifdef _DEBUG
  STDMETHODIMP_(ULONG) NonDelegatingAddRef();
  STDMETHODIMP_(ULONG) NonDelegatingRelease();
#endif

private:

  void Reset();
  void LogDebugMediaPosition( const char *text );

public:

  CSubtitleInputPin*  m_pSubtitlePin;
  
private: // data

  int                 m_subtitlePid;

  CDVBSubDecoder*     m_pSubDecoder;      // Subtitle decoder
  IMediaSeeking*      m_pIMediaSeeking;   // Media seeking interface
  CCritSec            m_Lock;				      // Main renderer critical section
  CCritSec            m_ReceiveLock;		  // Sublock for received samples

  REFERENCE_TIME      m_startTimestamp;
  REFERENCE_TIME      m_CurrentSeekPosition;
  LONGLONG            m_basePCR;
  LONGLONG            m_prevSubtitleTimestamp;
  CRefTime            m_currentTimeCompensation;

  bool                m_bBasePcrSet;

  int                 (CALLBACK *m_pSubtitleObserver) (SUBTITLE* sub);
  int                 (CALLBACK *m_pResetObserver) ();
  int                 (CALLBACK *m_pUpdateTimeoutObserver) (__int64* pTimeout);

  bool                m_bSeekingDone;
};
