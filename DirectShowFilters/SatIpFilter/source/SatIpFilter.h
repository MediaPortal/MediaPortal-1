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
#include "PacketReceiver.h"
#include "TsInputPin.h"
#include "ringbuffer.h"
#include "PidFilter.h"
#include "RtpStreamInterface.h"
#include <thread>
#include <process.h>
#include "config.h"

using namespace std;

class CTsMuxerTsOutputPin;
class CTsMuxer;
class CTsMuxerFilter;

#pragma region Interfaces

// {9D9FBAFE-3E8C-4104-A279-D9EEEC072BA2}
DEFINE_GUID(CLSID_TsMuxer, 0x9d9fbafe, 0x3e8c, 0x4104, 0xa2, 0x79, 0xd9, 0xee, 0xec, 0x7, 0x2b, 0xa2);



// {22D98D0E-6956-4EA0-9D18-4F55DEA8F5EC}
DEFINE_GUID(IID_ITsMuxer, 0x22d98d0e, 0x6956, 0x4ea0, 0x9d, 0x18, 0x4f, 0x55, 0xde, 0xa8, 0xf5, 0xec);


DECLARE_INTERFACE_(ITsMuxer, IUnknown)
{
	STDMETHOD(FilterCreateNamedPipe)(THIS_ char* devicePath)PURE;
};

#pragma endregion

#pragma region Struct

struct namedPipeReadThreadStruct{
	HANDLE handler;
	CTsMuxer* TsMuxer;
};

#pragma endregion

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
	typedef void*(*pvFunctv)();
	
	friend class CTsMuxerFilter;
	CTsMuxerFilter*	m_pFilter;							// Methods for filter interfaces
	CTsMuxerTsInputPin*	m_pTsInputPin;					// A TS rendered input pin
	CCritSec 		m_Lock;								// Main renderer critical section
	CCritSec 		m_ReceiveLock;						// Sublock for received samples

	RingBuffer*		ringbuffer;
	FILE* stream;
	bool hasSync;
	UINT_PTR timer;
	HANDLE pipeHandle;
	namedPipeReadThreadStruct parameters;

	HANDLE createPipe(const char*);
public:
	DECLARE_IUNKNOWN

	CTsMuxer(LPUNKNOWN pUnk, HRESULT *phr);
	~CTsMuxer();

	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

	HRESULT		 Write(PBYTE pbData, LONG lDataLength);
	STDMETHODIMP GetPMTPid(int pmtPid);
	STDMETHODIMP GetServiceId(int serviceId);
	STDMETHODIMP FilterCreateNamedPipe(char* devicePath);

	STDMETHODIMP IsReceiving(BOOL* yesNo);
	STDMETHODIMP Reset();

	PidFilter*		pidfilter;

private:

	// Overriden to say what interfaces we support where
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

	// Streaming Dll Import
	HINSTANCE LoadMe;
	pvFunctv  MPrtpStreamEntryPoint;
	IMPrtpStream* MPrtpStream;

	// Streaming instance thread
	//thread streamingThread;
	char* test1;
	char* test2;
	thread streamingThread;
	bool streamRunning;
	int bytesWritten;

	void			FindSync();
	void			processPackages();
	static void CALLBACK	checkConfigFile(HWND hwnd, UINT message, UINT idTimer, DWORD dwTime);
};



unsigned int __stdcall namedPipeReadThread(/*HANDLE&*//*LPVOID*/void*);



