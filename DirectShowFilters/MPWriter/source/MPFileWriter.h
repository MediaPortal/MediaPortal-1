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
#include <map>
#include "filewriter.h"
#include "TeletextGrabber.h"
#include "AnalogVideoAudioObserver.h"
#include "ChannelScan.h"
#include "SubChannel.h"
using namespace std;

class CMPFileWriterMPEG2InputPin;
class CMPFileWriterTeletextInputPin;
class CMPFileWriter;
class CMPFileWriterFilter;

DEFINE_GUID(CLSID_MPFileWriter, 0xdb35f5ed, 0x26b2, 0x4a2a,  0x92, 0xd3, 0x85, 0x2e, 0x14, 0x5b, 0xf3, 0x2d );

DEFINE_GUID(IID_IMPFileRecord,0xd5ff805e, 0xa98b, 0x4d56,  0xbe, 0xde, 0x3f, 0x1b, 0x8e, 0xf7, 0x25, 0x33);

// interface
//	//STDMETHOD(SetRecordingMode)(THIS_ int mode)PURE;

DECLARE_INTERFACE_(IMPFileRecord, IUnknown)
{
	STDMETHOD(SetRecordingMode)(THIS_ int subChannelId, int mode)PURE;
	STDMETHOD(SetRecordingFileName)(THIS_ int subChannelId, char* pszFileName)PURE;
	STDMETHOD(StartRecord)(THIS_ int subChannelId)PURE;
	STDMETHOD(StopRecord)(THIS_ int subChannelId)PURE;
	STDMETHOD(IsReceiving)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(Reset)(THIS_)PURE;
	STDMETHOD(SetTimeShiftFileName)(THIS_ int subChannelId, char* pszFileName)PURE;
	STDMETHOD(StartTimeShifting)(THIS_ int subChannelId)PURE;
	STDMETHOD(StopTimeShifting)(THIS_ int subChannelId)PURE;
	STDMETHOD(PauseTimeShifting)(THIS_ int subChannelId, int onOff)PURE;
	STDMETHOD(SetTimeShiftParams)(THIS_ int subChannelId, int minFiles, int maxFiles, ULONG maxFileSize)PURE;
	STDMETHOD(TTxSetCallBack)(THIS_ int subChannelId, IAnalogTeletextCallBack* callback)PURE;
	STDMETHOD(SetVideoAudioObserver)(THIS_ int subChannelId, IAnalogVideoAudioObserver* callback)PURE;
	STDMETHOD(SetRecorderVideoAudioObserver)(THIS_ int subChannelId, IAnalogVideoAudioObserver* callback)PURE;
	STDMETHOD(AddChannel)(THIS_ int* subChannelId)PURE;
	STDMETHOD(DeleteChannel)(THIS_ int subChannelId)PURE;
	STDMETHOD(DeleteAllChannels)(THIS_)PURE;
};
// Main filter object

class CMPFileWriterFilter : public CBaseFilter
{
	CMPFileWriter * const m_pMPFileWriter;

public:

	// Constructor
	CMPFileWriterFilter(CMPFileWriter *m_pMPFileWriter,
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


//  Pin object

class CMPFileWriterMPEG2InputPin : public CRenderedInputPin
{
	CMPFileWriter*  const m_pMPFileWriter;	// Main renderer object
	CCritSec* const	m_pReceiveLock;		    // Sample critical section
public:

	CMPFileWriterMPEG2InputPin(CMPFileWriter *m_pMPFileWriter,
		LPUNKNOWN pUnk,
		CBaseFilter *pFilter,
		CCritSec *pLock,
		CCritSec *pReceiveLock,
		HRESULT *phr);

	// Do something with this media sample
	STDMETHODIMP Receive(IMediaSample *pSample);
	STDMETHODIMP EndOfStream(void);
	STDMETHODIMP ReceiveCanBlock();

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


//  CMPFileWriter object which has filter and pin members

class CMPFileWriter : public CUnknown, public IMPFileRecord
{
	friend class CMPFileWriterFilter;
	friend class CMPFileWriterMPEG2InputPin;
	friend class CMPFileWriterTeletextInputPin;
	CMPFileWriterFilter*	m_pFilter;						// Methods for filter interfaces
	CMPFileWriterMPEG2InputPin*	m_pMPEG2InputPin;			// A MPEG2 rendered input pin
	CMPFileWriterTeletextInputPin*	m_pTeletextInputPin;	// A Teletext rendered input pin
	CCritSec 		m_Lock;									// Main renderer critical section
	CCritSec 		m_ReceiveLock;							// Sublock for received samples
public:
	DECLARE_IUNKNOWN

	CMPFileWriter(LPUNKNOWN pUnk, HRESULT *phr);
	~CMPFileWriter();

	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

	HRESULT		 Write(PBYTE pbData, LONG lDataLength);
	HRESULT		 WriteTeletext(PBYTE pbData, LONG lDataLength);
	STDMETHODIMP SetRecordingMode(int subChannelId, int mode);
	STDMETHODIMP SetRecordingFileName(int subChannelId, char* pszFileName);
	STDMETHODIMP StartRecord(int subChannelId);
	STDMETHODIMP StopRecord(int subChannelId);

	STDMETHODIMP IsReceiving(BOOL* yesNo);
	STDMETHODIMP Reset();

	STDMETHODIMP SetTimeShiftFileName(int subChannelId, char* pszFileName);
	STDMETHODIMP StartTimeShifting(int subChannelId);
	STDMETHODIMP StopTimeShifting(int subChannelId);
	STDMETHODIMP PauseTimeShifting(int subChannelId, int onOff);
	STDMETHODIMP SetTimeShiftParams(int subChannelId,  int minFiles, int maxFiles, ULONG maxFileSize);
	STDMETHODIMP TTxSetCallBack(int subChannelId, IAnalogTeletextCallBack* callback);
	STDMETHODIMP SetVideoAudioObserver(int subChannelId, IAnalogVideoAudioObserver* callback);
	STDMETHODIMP SetRecorderVideoAudioObserver(int subChannelId, IAnalogVideoAudioObserver* callback);
	STDMETHODIMP AddChannel(int* subChannelId);
	STDMETHODIMP DeleteChannel(int subChannelId);
	STDMETHODIMP DeleteAllChannels();
private:

	CSubChannel* GetSubChannel(int handle);	
	// Overriden to say what interfaces we support where
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
	CChannelScan* m_pChannelScan;
	vector<CSubChannel*> m_vecChannels;
	typedef vector<CSubChannel*>::iterator ivecChannels;
	int m_id;
};





class CMPFileWriterTeletextInputPin : public CRenderedInputPin
{
	CMPFileWriter*  const m_pMPFileWriter;	// Main renderer object
	CCritSec* const	m_pReceiveLock;		    // Sample critical section
public:

	CMPFileWriterTeletextInputPin(CMPFileWriter *m_pMPFileWriter,
		LPUNKNOWN pUnk,
		CBaseFilter *pFilter,
		CCritSec *pLock,
		CCritSec *pReceiveLock,
		HRESULT *phr);

	// Do something with this media sample
	STDMETHODIMP Receive(IMediaSample *pSample);
	STDMETHODIMP EndOfStream(void);
	STDMETHODIMP ReceiveCanBlock();

	// Check if the pin can support this specific proposed type and format
	HRESULT			CheckMediaType(const CMediaType *);
	// Break connection
	HRESULT			BreakConnect();
	BOOL			IsReceiving();
	void			Reset();
	// Track NewSegment
	STDMETHODIMP NewSegment(REFERENCE_TIME tStart,REFERENCE_TIME tStop,double dRate);
private:
	BOOL			m_bIsReceiving;
	DWORD			m_lTickCount;
	CCritSec		m_section;
};


