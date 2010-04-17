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
#include <map>
#include "TeletextGrabber.h"
#include "ChannelScan.h"
#include "ProgramToTransportStream.h"
#include "ElementaryToTransportStream.h"
#include "PacketReceiver.h"
#include "MPEGInputPin.h"
#include "VideoInputPin.h"
#include "AudioInputPin.h"
#include "TeletextInputPin.h"
#include "TsInputPin.h"

using namespace std;

class CTsMuxerTsOutputPin;
class CTsMuxer;
class CTsMuxerFilter;

// {511D13F0-8A56-42fa-B151-B72A325CF71A}
DEFINE_GUID(CLSID_TsMuxer, 0x511d13f0, 0x8a56, 0x42fa, 0xb1, 0x51, 0xb7, 0x2a, 0x32, 0x5c, 0xf7, 0x1a);

// {8533D2D1-1BE1-4262-B70A-432DF592B903}
DEFINE_GUID(IID_ITsMuxer,0x8533d2d1, 0x1be1, 0x4262, 0xb7, 0xa, 0x43, 0x2d, 0xf5, 0x92, 0xb9, 0x3);

DECLARE_INTERFACE_(ITsMuxer, IUnknown)
{
	STDMETHOD(GetPMTPid)(THIS_ int pmtPid)PURE;
	STDMETHOD(GetServiceId)(THIS_ int serviceId)PURE;
	STDMETHOD(TTxSetCallBack)(THIS_ IAnalogTeletextCallBack* callback)PURE;
};
// Main filter object

class CTsMuxerFilter : public CBaseFilter
{
	CTsMuxer * const m_pTsMuxer;

public:

	// Constructor
	CTsMuxerFilter(CTsMuxer *m_pTsMuxer,
		LPUNKNOWN pUnk,
		CCritSec *pLock,
		HRESULT *phr);

	// Pin enumeration
	CBasePin * GetPin(int n);
	int GetPinCount();

	// Open and close the file as necessary
	STDMETHODIMP Run(REFERENCE_TIME tStart);
	STDMETHODIMP Pause();
	STDMETHODIMP Stop();
};


//  CTsMuxer object which has filter and pin members

class CTsMuxer : public CUnknown, public ITsMuxer, public IPacketReceiver
{
	friend class CTsMuxerFilter;
	friend class CTsMuxerTsOutputPin;
	CTsMuxerFilter*	m_pFilter;							// Methods for filter interfaces
	CTsMuxerMPEGInputPin*	m_pMPEGInputPin;			// A MPEG rendered input pin
	CTsMuxerVideoInputPin*	m_pVideoInputPin;			// A Video rendered input pin
	CTsMuxerAudioInputPin*	m_pAudioInputPin;			// A Audio rendered input pin
	CTsMuxerTeletextInputPin*	m_pTeletextInputPin;	// A Teletext rendered input pin
	CTsMuxerTsInputPin*	m_pTsInputPin;					// A TS rendered input pin
	CTsMuxerTsOutputPin*	m_pTsOutputPin;				// A TS output pin
	CCritSec 		m_Lock;								// Main renderer critical section
	CCritSec 		m_ReceiveLock;						// Sublock for received samples
public:
	DECLARE_IUNKNOWN

	CTsMuxer(LPUNKNOWN pUnk, HRESULT *phr);
	~CTsMuxer();

	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

	HRESULT		 WriteProgram(PBYTE pbData, LONG lDataLength);
	HRESULT		 WriteAudio(PBYTE pbData, LONG lDataLength);
	HRESULT		 WriteVideo(PBYTE pbData, LONG lDataLength);
	HRESULT		 WriteTeletext(PBYTE pbData, LONG lDataLength);
	STDMETHODIMP GetPMTPid(int pmtPid);
	STDMETHODIMP GetServiceId(int serviceId);

	STDMETHODIMP IsReceiving(BOOL* yesNo);
	STDMETHODIMP Reset();

	STDMETHODIMP TTxSetCallBack(IAnalogTeletextCallBack* callback);
private:

	// Overriden to say what interfaces we support where
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
	CChannelScan* m_pChannelScan;
	CTeletextGrabber* m_pTeletextGrabber;
	CProgramToTransportStream* m_pProgramTsWriter;
	CElementaryToTransportStream* m_pElementaryTsWriter;
};





