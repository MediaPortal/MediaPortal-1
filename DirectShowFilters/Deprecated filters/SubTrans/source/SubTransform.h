/*
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: tourettes
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

#include "dvbsubs\dvbsubs.h"
#include "SubdecoderObserver.h"
#include "PidObserver.h"

class CSubtitleInputPin;
class CPcrInputPin;
class CPMTInputPin;
class CDVBSubDecoder;

typedef __int64 int64_t;
//typedef unsigned __int16 uint16_t;
//typedef unsigned __int8 uint8_t;

// {591AB987-9689-4c07-846D-0006D5DD2BFD}
DEFINE_GUID(CLSID_SubTransform,
	0x591ab987, 0x9689, 0x4c07, 0x84, 0x6d, 0x0, 0x6, 0xd5, 0xdd, 0x2b, 0xfd);

DECLARE_INTERFACE_( IStreamAnalyzer, IUnknown )
{
   STDMETHOD( SetSubtitlePID ) ( THIS_ ULONG pPID ) PURE;
   STDMETHOD( SetPcrPID ) ( THIS_ ULONG pPID ) PURE;
};

struct PTSTime
{
	int h;
	int m;
	int s;
	int u;
};

class CSubTransform : public CTransformFilter, public MSubdecoderObserver, public MPidObserver
{
public:
  // Constructor & destructor
  CSubTransform( LPUNKNOWN pUnk, HRESULT *phr );
  ~CSubTransform();

  // Overridden CTransformFilter methods
  HRESULT CheckInputType( const CMediaType *mtIn );
  HRESULT CheckTransform( const CMediaType *mtIn, const CMediaType *mtOut );
  HRESULT DecideBufferSize( IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pProp );
  HRESULT GetMediaType( int iPosition, CMediaType *pMediaType );
  HRESULT Transform( IMediaSample *pIn, IMediaSample *pOut );
  HRESULT SetMediaType( PIN_DIRECTION direction, const CMediaType *pmt );
  //HRESULT CompleteConnect( PIN_DIRECTION direction, IPin *pReceivePin );
  HRESULT CheckConnect(PIN_DIRECTION dir,IPin *pPin);

  STDMETHODIMP Run( REFERENCE_TIME tStart );
  STDMETHODIMP Pause();
  STDMETHODIMP Stop();

  HRESULT BeginFlush( void );
  HRESULT EndFlush( void );

  CBasePin * GetPin( int n );
  int GetPinCount();

  static CUnknown * WINAPI CreateInstance( LPUNKNOWN pUnk, HRESULT *pHr );

	void CSubTransform::GetVideoInfoParameters(
        const VIDEOINFOHEADER *pvih,    // Pointer to the format header
        BYTE  * const pbData,			      // Pointer to first address in buffer
        DWORD *pdwWidth,				        // Returns the width in pixels
        DWORD *pdwHeight,				        // Returns the height in pixels
        LONG  *plStrideInBytes,			    // Add this to a row to get the new row down
        BYTE **ppbTop,					        // Pointer to first byte in top row of pixels
        bool bYuv );

	bool IsValidYUY2( const CMediaType *pmt );
	bool IsValidUYVY( const CMediaType *pmt );

	void Reset();

	// Interface
	STDMETHOD(SetSubtitlePid) ( THIS_ ULONG pPid );
  STDMETHOD(SetPcrPid)      ( THIS_ ULONG pPid );

	// From MSubdecoderObserver
	void Notify();

  // From MPidObserver
  void SetPcrPid( LONG pid );
	void SetSubtitlePid( LONG pid );
  void PTSToPTSTime( ULONGLONG pts, PTSTime* ptsTime );

private:

  HRESULT ProcessFrameUYVY( BYTE *pbInput, BYTE *pbOutput, long *pcbByte );
  HRESULT ProcessFrameYUY2( BYTE *pbInput, BYTE *pbOutput, long *pcbByte );

	void StretchSubtitle();

  VIDEOINFOHEADER		m_VihIn;   // Current video format (input)
  VIDEOINFOHEADER		m_VihOut;  // Current video format (output)

	CSubtitleInputPin*	m_pSubtitlePin;
	CPcrInputPin*		    m_pPcrPin;
  CPMTInputPin*       m_pPMTPin;

	CDVBSubDecoder*		  m_pSubDecoder;

	CCritSec			m_Lock;				    // Main renderer critical section
  CCritSec			m_ReceiveLock;		// Sublock for received samples

	unsigned char*  m_curSubtitleData;  //[720*576*3];
	CSubtitle*      m_pSubtitle;

  ULONGLONG m_NextSubtitlePTS;
	ULONGLONG m_curSubtitlePTS;
	ULONGLONG m_ShowDuration;
	ULONGLONG m_curPTS;
	ULONGLONG m_firstPTS;
	ULONGLONG m_PTSdiff;

  bool      m_firstPTSDone;

  HBITMAP				m_DibsSub;  // dibsection for to-be-scaled subtitle bitmap
  HDC					  m_DC;
  void *				m_pDibBits;
  HGDIOBJ				m_OldObject;

	bool m_bRenderCurrentSubtitle;
	bool m_bSubtitleDiscarded;

  int m_VideoPid;
};
