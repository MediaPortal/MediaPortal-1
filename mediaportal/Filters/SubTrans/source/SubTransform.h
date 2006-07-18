/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#include "ITSFileSource.h"
#include "dvbsubs\dvbsubs.h"
#include "SubdecoderObserver.h"

class CSubtitleInputPin;
class CAudioInputPin;
class CDVBSubDecoder;

typedef __int64 int64_t;
//typedef unsigned __int16 uint16_t;
//typedef unsigned __int8 uint8_t;

// {591AB987-9689-4c07-846D-0006D5DD2BFD}
DEFINE_GUID(CLSID_SubTransform, 
	0x591ab987, 0x9689, 0x4c07, 0x84, 0x6d, 0x0, 0x6, 0xd5, 0xdd, 0x2b, 0xfd);

struct __declspec(uuid("559E6E81-FAC4-4EBC-9530-662DAA27EDC2")) ITSFileSource;

DECLARE_INTERFACE_( IStreamAnalyzer, IUnknown )
{
   STDMETHOD(SetSubtitlePID) (THIS_ ULONG pPID ) PURE;
};

class CSubTransform : public CTransformFilter, public MSubdecoderObserver
{
public:
  // Constructor & destructor
  CSubTransform( LPUNKNOWN pUnk, HRESULT *phr );
  ~CSubTransform();

  // Overridden CTransformFilter methods
  HRESULT CheckInputType( const CMediaType *mtIn);
  HRESULT CheckTransform( const CMediaType *mtIn, const CMediaType *mtOut );
  HRESULT DecideBufferSize( IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pProp );
  HRESULT GetMediaType( int iPosition, CMediaType *pMediaType );
  HRESULT Transform( IMediaSample *pIn, IMediaSample *pOut );

	STDMETHODIMP Run( REFERENCE_TIME tStart );
	STDMETHODIMP Pause();
	STDMETHODIMP Stop();

  HRESULT BeginFlush( void );
  HRESULT EndFlush( void );

  CBasePin * GetPin( int n );
  int GetPinCount();

  HRESULT SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt);

  static CUnknown * WINAPI CreateInstance(LPUNKNOWN pUnk, HRESULT *pHr);

	void CSubTransform::GetVideoInfoParameters(
  const VIDEOINFOHEADER *pvih,	// Pointer to the format header
  BYTE  * const pbData,			// Pointer to first address in buffer
  DWORD *pdwWidth,				// Returns the width in pixels
  DWORD *pdwHeight,				// Returns the height in pixels
  LONG  *plStrideInBytes,			// Add this to a row to get the new row down
  BYTE **ppbTop,					// Pointer to first byte in top row of pixels
  bool bYuv );

	
	HRESULT ConnectToTSFileSource();

	bool IsValidYUY2( const CMediaType *pmt );
	bool IsValidUYVY( const CMediaType *pmt );

	void Reset();

	// Interface
	STDMETHOD(SetSubtitlePID)( THIS_ ULONG pPID );

	// From MSubdecoderObserver
	void Notify();

private:
  HRESULT ProcessFrameUYVY( BYTE *pbInput, BYTE *pbOutput, long *pcbByte );
  HRESULT ProcessFrameYUY2( BYTE *pbInput, BYTE *pbOutput, long *pcbByte );
	
	void StretchSubtitle();

  VIDEOINFOHEADER		m_VihIn;   // Current video format (input)
  VIDEOINFOHEADER		m_VihOut;  // Current video format (output)

	CSubtitleInputPin*	m_pSubtitlePin;
	CAudioInputPin*		  m_pAudioPin;
	
	CDVBSubDecoder*		m_pSubDecoder;

	CCritSec			m_Lock;				// Main renderer critical section
  CCritSec			m_ReceiveLock;		// Sublock for received samples

	unsigned char*		m_curSubtitleData;//[720*576*3];
	ULONGLONG			m_NextSubtitlePTS;
	ULONGLONG			m_curSubtitlePTS;
	ULONGLONG			m_ShowDuration;

	ULONGLONG			m_curPTS; 
	ULONGLONG			m_firstPTS;
	ULONGLONG			m_PTSdiff;

	CSubtitle*			m_pSubtitle;

  HBITMAP				m_DibsSub; // dibsection for to-be-scaled subtitle bitmap
  HDC					  m_DC;
  void *				m_pDibBits;
  HGDIOBJ				m_OldObject;

	bool				m_bRenderCurrentSubtitle;
	bool				m_bSubtitleDiscarded;

	CComQIPtr<ITSFileSource> m_pTSFileSource;
};