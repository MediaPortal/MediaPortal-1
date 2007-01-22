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
#include <initguid.h>

#include "dvbsubs\dvbsubdecoder.h"
#include "SubdecoderObserver.h"
#include "PidObserver.h"

class CSubtitleInputPin;
class CSubtitleOutputPin;
class CPcrInputPin;
class CPMTInputPin;

class CDVBSubDecoder;

typedef __int64 int64_t;

// {591AB987-9689-4c07-846D-0006D5DD2BFD}
DEFINE_GUID(CLSID_DVBSub, 
	0x591ab987, 0x9689, 0x4c07, 0x84, 0x6d, 0x0, 0x6, 0xd5, 0xdd, 0x2b, 0xfd);

// C19647D5-A861-4845-97A6-EBD0A135D0BF
DEFINE_GUID(IID_IDVBSubtitle, 
0xc19647d5, 0xa861, 0x4845, 0x97, 0xa6, 0xeb, 0xd0, 0xa1, 0x35, 0xd0, 0xbf);


// structure used to communicate subtitles to MediaPortals managed code
struct SUBTITLE{
	LONG        bmType;
    LONG        bmWidth;
    LONG        bmHeight;
    LONG        bmWidthBytes;
    WORD        bmPlanes;
    WORD        bmBitsPixel;
    LPVOID      bmBits;

	unsigned __int64 timeOut;
};

DECLARE_INTERFACE_( IDVBSubtitle, IUnknown )
{
  STDMETHOD(GetSubtitle) ( int place, SUBTITLE* pSubtitle ) PURE;
  STDMETHOD(GetSubtitleCount) ( int* count ) PURE;
  STDMETHOD(SetCallback) ( int (CALLBACK *pSubtitleObserver)() ) PURE;
  STDMETHOD(DiscardOldestSubtitle) () PURE;
  STDMETHOD(Test)(int status) PURE;
};


extern void LogDebug(const char *fmt, ...);

class CDVBSub : public CBaseFilter, public MSubdecoderObserver, MPidObserver, IDVBSubtitle
{
public:
  // Constructor & destructor
  CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock );
  ~CDVBSub();

  // Methods from directshow base classes 
  HRESULT CheckConnect( PIN_DIRECTION dir, IPin *pPin );
  STDMETHODIMP Run( REFERENCE_TIME tStart );
	STDMETHODIMP Pause();
	STDMETHODIMP Stop();
  CBasePin * GetPin( int n );
  int GetPinCount();

  // IDVBSubtitle
  virtual HRESULT STDMETHODCALLTYPE GetSubtitle( int place, SUBTITLE* pSubtitle );
  virtual HRESULT STDMETHODCALLTYPE SetCallback( int (CALLBACK *pSubtitleObserver)() );
  virtual HRESULT STDMETHODCALLTYPE GetSubtitleCount( int* count );
  virtual HRESULT STDMETHODCALLTYPE DiscardOldestSubtitle();

  virtual HRESULT STDMETHODCALLTYPE Test(int status);

  // IUnknown
  DECLARE_IUNKNOWN;

  /*
      STDMETHODIMP QueryInterface(REFIID riid, void **ppv) {
		  	if(riid == IID_IBaseFilter){
		LogDebug("riid = Trying basefilter");
	}
	else if(riid == IID_IDVBSubtitle){
		LogDebug("riid = IID_IDVBSubtitle");
	}
	else if(riid == IID_IUnknown){
		LogDebug("riid = IID_IUnknown");
	}

        return GetOwner()->QueryInterface(riid,ppv);            
    };                                                          
    STDMETHODIMP_(ULONG) AddRef() {
		//LogDebug("Before AddRef : %i", this->m_cRef);
        return GetOwner()->AddRef();                            
    };                                                          
    STDMETHODIMP_(ULONG) Release() {       
		//LogDebug("Before Release : %i", this->m_cRef);
        return GetOwner()->Release();                           
    };*/

  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
	
  // From MSubdecoderObserver
	void Notify();

  // From MPidObserver
  void SetPcrPid( LONG pid );
	void SetSubtitlePid( LONG pid );

  static CUnknown * WINAPI CreateInstance(LPUNKNOWN pUnk, HRESULT *pHr);

	void Reset();

private:
  CSubtitleInputPin*  m_pSubtitleInputPin;
  CSubtitleOutputPin* m_pSubtitleOutputPin;
	CPcrInputPin*		    m_pPcrPin;
  CPMTInputPin*       m_pPMTPin;

  int m_VideoPid;     

  CDVBSubDecoder*     m_pSubDecoder;

  CCritSec            m_Lock;				      // Main renderer critical section
  CCritSec            m_ReceiveLock;		  // Sublock for received samples

  ULONGLONG           m_firstPTS;

  int                (CALLBACK *m_pSubtitleObserver) (); 
};