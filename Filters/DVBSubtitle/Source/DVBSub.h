/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#include <initguid.h>

#include "dvbsubs\dvbsubdecoder.h"
#include "SubdecoderObserver.h"
#include "SubtitleObserver.h"
#include "PidObserver.h"

class CSubtitleInputPin;
class CSubtitleOutputPin;
class CPcrInputPin;
class CPMTInputPin;

class CDVBSubDecoder;
class MSubtitleObserver; 

typedef __int64 int64_t;

// {591AB987-9689-4c07-846D-0006D5DD2BFD}
DEFINE_GUID(CLSID_DVBSub, 
	0x591ab987, 0x9689, 0x4c07, 0x84, 0x6d, 0x0, 0x6, 0xd5, 0xdd, 0x2b, 0xfd);

DECLARE_INTERFACE_( IDVBSubtitle, IUnknown )
{
  STDMETHOD(GetSubtitle) ( int place, CSubtitle *pSubtitle ) PURE;
  STDMETHOD(SetCallback) ( MSubtitleObserver *pSubtitleObserver ) PURE;
  STDMETHOD(GetSubtitleCount) ( int count ) PURE;
  STDMETHOD(DiscardOldestSubtitle) () PURE;
};

class CDVBSub : public CBaseFilter, public MSubdecoderObserver, MPidObserver
{
public:
  // Constructor & destructor
  CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock );
  ~CDVBSub();

  HRESULT CheckConnect( PIN_DIRECTION dir, IPin *pPin );
  
  STDMETHODIMP Run( REFERENCE_TIME tStart );
	STDMETHODIMP Pause();
	STDMETHODIMP Stop();

  CBasePin * GetPin( int n );
  int GetPinCount();

  static CUnknown * WINAPI CreateInstance(LPUNKNOWN pUnk, HRESULT *pHr);

	void Reset();

	// Interface
	STDMETHOD (GetSubtitle)( int place, CSubtitle *pSubtitle );
  STDMETHOD (SetCallback)( MSubtitleObserver *pSubtitle );
  STDMETHOD (GetSubtitleCount)( int count );
  STDMETHOD (DiscardOldestSubtitle)();

	// From MSubdecoderObserver
	void Notify();

  // From MPidObserver
  void SetPcrPid( LONG pid );
	void SetSubtitlePid( LONG pid );

private:

  CSubtitleInputPin*  m_pSubtitleInputPin;
  CSubtitleOutputPin* m_pSubtitleOutputPin;
	CPcrInputPin*		    m_pPcrPin;
  CPMTInputPin*       m_pPMTPin;

  CDVBSubDecoder*     m_pSubDecoder;

  CCritSec            m_Lock;				    // Main renderer critical section
  CCritSec            m_ReceiveLock;		// Sublock for received samples

  unsigned char*      m_curSubtitleData;//[720*576*3];
  ULONGLONG           m_firstPTS;
  CSubtitle*          m_pSubtitle;

  MSubtitleObserver*  m_pSubtitleObserver;

  int m_VideoPid;
};