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
#include <streams.h>
#include "PacketReceiver.h"

const AMOVIESETUP_MEDIATYPE audioInputPinTypes[] =
{
	{&MEDIATYPE_Audio,&MEDIASUBTYPE_MPEG2_AUDIO},
	{&MEDIATYPE_Audio,&MEDIASUBTYPE_MPEG1Payload},
	{&MEDIATYPE_Audio,&MEDIASUBTYPE_MPEG1AudioPayload},
	{&MEDIATYPE_Audio,&MEDIASUBTYPE_DVD_LPCM_AUDIO},
	{&MEDIATYPE_Audio,&MEDIASUBTYPE_DOLBY_AC3},
	{&MEDIATYPE_Stream,&MEDIASUBTYPE_MPEG1Audio}
};

//  Pin object

class CTsMuxerAudioInputPin : public CRenderedInputPin
{
	IPacketReceiver*  const m_pTsMuxer;	// Main renderer object
	CCritSec* const	m_pReceiveLock;		    // Sample critical section
public:

	CTsMuxerAudioInputPin(IPacketReceiver *m_pTsMuxer,
		LPUNKNOWN pUnk,
		CBaseFilter *pFilter,
		CCritSec *pLock,
		CCritSec *pReceiveLock,
		HRESULT *phr);

	// Do something with this media sample
	STDMETHODIMP Receive(IMediaSample *pSample);
	STDMETHODIMP EndOfStream(void);
	STDMETHODIMP ReceiveCanBlock();
	HRESULT GetMediaType(int iPosition,CMediaType *pMediaType);

	// Check if the pin can support this specific proposed type and format
	HRESULT		CheckMediaType(const CMediaType *);
	// Break connection
	HRESULT		BreakConnect();
	BOOL			IsReceiving();
	void			Reset();
	// Track NewSegment
	STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);
private:
	BOOL				m_bIsReceiving;
	DWORD				m_lTickCount;
	CCritSec		m_section;
};


