/* 
*	Copyright (C) 2005-2008 Team MediaPortal
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

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "SatIpFilter.h"
#include "liveMedia.hh"
#include <fstream>


const AMOVIESETUP_MEDIATYPE tsPinType =
{
	&MEDIATYPE_Stream,				// Major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT	// Minor type
};

const AMOVIESETUP_PIN tsMuxerPins[] =
{
	{
		L"TS Input",				// Pin string name
			FALSE,                  // Is it rendered
			FALSE,                  // Is it an output
			FALSE,                  // Allowed none
			FALSE,                  // Likewise many
			&CLSID_NULL,            // Connects to filter
			L"Output",              // Connects to pin
			1,                      // Number of types
			&tsPinType				// Pin information
	}
};

const AMOVIESETUP_FILTER tsMuxer =
{
	&CLSID_TsMuxer,					// Filter CLSID
	L"MediaPortal Sat>IP Filter",	// String name
	MERIT_DO_NOT_USE,				// Filter merit
	1,								// Number pins
	tsMuxerPins						// Pin details
};

static char logbuffer[2000]; 

void LogDebug(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	TCHAR folder[MAX_PATH];
	TCHAR fileName[MAX_PATH];
	::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
	sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\TsMuxer.Log",folder);

	FILE* fp = fopen(fileName,"a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,systemTime.wMilliseconds,
			logbuffer);
		fclose(fp);
	}
};

//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
	L"MediaPortal Sat>IP Filter", &CLSID_TsMuxer, CTsMuxer::CreateInstance, NULL, &tsMuxer
};
int g_cTemplates = 1;

#pragma region TsMuxerFilter

// Constructor

CTsMuxerFilter::CTsMuxerFilter(CTsMuxer *pTsMuxer,
							   LPUNKNOWN pUnk,
							   CCritSec *pLock,
							   HRESULT *phr) :
CBaseFilter(NAME("Ts Muxer"), pUnk, pLock, CLSID_TsMuxer),
m_pTsMuxer(pTsMuxer)
{
}


//
// GetPin
//
CBasePin * CTsMuxerFilter::GetPin(int n)
{
	if (n == 0) {
		return m_pTsMuxer->m_pTsInputPin;
	}else {
		return NULL;
	}
}


//
// GetPinCount
//
int CTsMuxerFilter::GetPinCount()
{
	return 1;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CTsMuxerFilter::Stop()
{
	CAutoLock cObjectLock(m_pLock);

	LogDebug("CTsMuxerFilter::Stop()");

	HRESULT result =  CBaseFilter::Stop();
	LogDebug("CTsMuxerFilter::Stop() completed");
	return result;
}


//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CTsMuxerFilter::Pause()
{
	LogDebug("CTsMuxerFilter::Pause()");
	CAutoLock cObjectLock(m_pLock);

	if (m_pTsMuxer != NULL)
	{
		// GraphEdit calls Pause() before calling Stop() for this filter.
		// If we have encountered a write error (such as disk full),
		// then stopping the graph could cause our log to be deleted
		// (because the current log file handle would be invalid).
		// 
		// To preserve the log, don't open/create the log file on pause
		// if we have previously encountered an error.  The write error
		// flag gets cleared when setting a new log file name or
		// when restarting the graph with Run().
		//m_pTsMuxer->m_pProgramTsWriter->Close();
		//m_pTsMuxer->m_pElementaryTsWriter->Close();
	}
	LogDebug("CTsMuxerFilter::Pause() finished");
	return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CTsMuxerFilter::Run(REFERENCE_TIME tStart)
{
	LogDebug("CTsMuxerFilter::Run()");
	CAutoLock cObjectLock(m_pLock);

	return CBaseFilter::Run(tStart);
}

#pragma endregion

//
//  CTsMuxer class
//
CTsMuxer::CTsMuxer(LPUNKNOWN pUnk, HRESULT *phr) :
CUnknown(NAME("CTsMuxer"), pUnk),
m_pFilter(NULL),
m_pTsInputPin(NULL)
{
	LogDebug("CTsMuxer::ctor()");

	TCHAR folder[MAX_PATH];
	TCHAR fileName[MAX_PATH];
	::SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);
	sprintf(fileName, "%s\\Team MediaPortal\\MediaPortal TV Server\\log\\output.ts", folder);

	stream = fopen(fileName, "ab");

	DeleteFile("TsMuxer.log");
	m_pFilter = new CTsMuxerFilter(this, GetOwner(), &m_Lock, phr);
	if (m_pFilter == NULL) 
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}


	m_pTsInputPin = new CTsMuxerTsInputPin(this,GetOwner(),
		m_pFilter,
		&m_Lock,
		&m_ReceiveLock,
		phr);
	if (m_pTsInputPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	// ringbuffer
	ringbuffer = new RingBuffer(TS_PACKET_LEN * 20000);

	hasSync = false; // we have no sync at the beginning

	pidfilter = new PidFilter();
	// for testing only
	pidfilter->Add(0);	// 0x0000
	pidfilter->Add(200); // 0x00c8
	pidfilter->Add(201); // 0x00c9
	pidfilter->Add(202); // 0x00ca
	pidfilter->Add(203); // x00cb

	// starting timer to check for a config file
	timer = SetTimer(NULL, NULL, 1000, (TIMERPROC)checkConfigFile);


	// configure streaming
	LoadMe = LoadLibrary("RtpStreamer.dll");
	if (LoadMe != 0)
		LogDebug("LoadMe library loaded!\n");
	else
		LogDebug("LoadMe library failed to load!\n");
	MPrtpStreamEntryPoint = (pvFunctv)GetProcAddress(LoadMe, "CreateClassInstance");
	if (!MPrtpStreamEntryPoint) LogDebug("shit!!");
	MPrtpStream = (IMPrtpStream*)(MPrtpStreamEntryPoint());
	streamRunning = false;
	
	test1 = "192.168.178.26";
	test2 = "test.ts";
	bytesWritten = 0;
	//thread th(&IMPrtpStream::MPrtpStreamCreate, this->MPrtpStream, test1, 8888, test2);
	//streamingThread = thread(&IMPrtpStream::MPrtpStreamCreate, this->MPrtpStream, test1, 8888, test2);
	//streamingThread.detach(); // fire & forget, maybe not the best option so have a look here later: http://stackoverflow.com/questions/16296284/workaround-for-blocking-async
	//streamingThread (MPrtpStream->MPrtpStreamCreate, ("192.168.178.26", 8888, "test.ts"));



	// create the named pipe
	//unsigned int id = 0;
	
	//pipeHandle = createPipe();
	//parameters.handler = pipeHandle;
	//parameters.TsMuxer = this;
	//::CloseHandle((HANDLE)::_beginthreadex(0, 0, &namedPipeReadThread, /*(void*)*//*(LPVOID)pipeHandle*/&parameters, 0, &id));
	FilterCreateNamedPipe(PIPE_NAME);
}




// Destructor

CTsMuxer::~CTsMuxer()
{
	LogDebug("CMPFilerWriter::dtor()");


	delete m_pTsInputPin;
	m_pTsInputPin = NULL;

	LogDebug("CTsMuxerTsInputPin::dtor() completed");


	delete m_pFilter;
	m_pFilter = NULL;

	fclose(stream);

	LogDebug("CTsMuxerFilter::dtor() completed");
}


//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CTsMuxer::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);

	CTsMuxer *pNewObject = new CTsMuxer(punk, phr);
	if (pNewObject == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
	}

	return pNewObject;

} // CreateInstance


//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CTsMuxer::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	CheckPointer(ppv,E_POINTER);
	CAutoLock lock(&m_Lock);

	// Do we have this interface
	if (riid == IID_ITsMuxer)
	{
		return GetInterface((ITsMuxer*)this, ppv);
	}else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) {
		return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
	} 

	return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration 
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

//
// DllRegisterSever
//
// Handle the registration of this filter
//
STDAPI DllRegisterServer()
{
	return AMovieDllRegisterServer2( TRUE );

} // DllRegisterServer


//
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
	return AMovieDllRegisterServer2( FALSE );

} // DllUnregisterServer


//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
					  DWORD  dwReason, 
					  LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

STDMETHODIMP CTsMuxer::GetPMTPid(int pmtPid)
{
	pmtPid = 0x20;
	return S_OK;
}

STDMETHODIMP CTsMuxer::GetServiceId(int serviceId)
{
	serviceId = 1;
	return S_OK;
}


STDMETHODIMP CTsMuxer::FilterCreateNamedPipe(char* devicePath)
{
	unsigned int id = 0;

	pipeHandle = createPipe(devicePath);
	parameters.handler = pipeHandle;
	parameters.TsMuxer = this;
	::CloseHandle((HANDLE)::_beginthreadex(0, 0, &namedPipeReadThread, /*(void*)*//*(LPVOID)pipeHandle*/&parameters, 0, &id));

	return S_OK;
}


HRESULT CTsMuxer::Write(PBYTE pbData, LONG lDataLength)
{
	CAutoLock lock(&m_Lock);

	ringbuffer->Write(pbData, lDataLength);

	processPackages();

	return S_OK;
}

void CTsMuxer::processPackages()
{
	FindSync();

	if (ringbuffer->GetReadAvail() >= TS_PACKET_LEN && hasSync) {
		// process each package
		while (ringbuffer->GetReadAvail() >= TS_PACKET_LEN) {
			unsigned char* out = new unsigned char[TS_PACKET_LEN];
			ringbuffer->Read(out, TS_PACKET_LEN);

			// check if we are still in sync
			if (out[0] != TS_PACKET_SYNC) {
				LogDebug("Lost Sync >_>");
				hasSync = false;

				delete out;
				return; // exit while
			}

			// PID filter
			unsigned short Pid;
			Pid = ((out[1] & 0x1F) << 8) + out[2];
			//LogDebug("Pid 0x%04x", Pid);
			if (pidfilter->PidRequested(Pid)) {
				//fwrite(out, 1, TS_PACKET_LEN, stream);
				MPrtpStream->write(out, TS_PACKET_LEN);
				if (!streamRunning)
					bytesWritten += TS_PACKET_LEN;
			}
			delete out;
		}
		if (!streamRunning && hasSync && bytesWritten > (TS_PACKET_LEN * 900)) {
			streamRunning = true;
			LogDebug("startStreaming");
			streamingThread = thread(&IMPrtpStream::MPrtpStreamCreate, this->MPrtpStream, test1, 8888, test2);
			streamingThread.detach(); // fire & forget, maybe not the best option so have a look here later: http://stackoverflow.com/questions/16296284/workaround-for-blocking-async
		}
	}
}

void CTsMuxer::FindSync()
{
	bool run = true;
	if (ringbuffer->GetReadAvail() >= 940 && !hasSync) {
		while (run)
		{
			if ((ringbuffer->ReadOneByte() == TS_PACKET_SYNC) &&
				(ringbuffer->MovePointerAndReadOneByte(TS_PACKET_LEN) == TS_PACKET_SYNC))
			{
				// at th ebeginning of a ts packet!
				//OnTsPacket(&pData[syncOffset]);
				ringbuffer->MovePointerAndReadOneByte(TS_PACKET_LEN - 1); // move the pointer to the beginning of the next package
				LogDebug("!!!!!!!!!!!!SYNC BYTE!!!!!!!!!!!");
				hasSync = true; // we have sync now!
				run = false;
			}
		}
	}
}

HANDLE CTsMuxer::createPipe(const char* pipeName) {
	LogDebug("Named pipe: create pipe - %s", pipeName);


	HANDLE hPipe;

	hPipe = CreateNamedPipe(
		pipeName,				  // pipe name 
		PIPE_ACCESS_DUPLEX,       // read/write access 
		PIPE_TYPE_MESSAGE |       // message type pipe 
		PIPE_READMODE_MESSAGE |   // message-read mode 
		PIPE_WAIT,                // blocking mode 
		PIPE_UNLIMITED_INSTANCES, // max. instances  
		PIPE_BUFFER_SIZE,         // output buffer size 
		PIPE_BUFFER_SIZE,         // input buffer size 
		NMPWAIT_USE_DEFAULT_WAIT, // client time-out 
		NULL);                    // default security attribute 

	if (INVALID_HANDLE_VALUE == hPipe)
	{
		LogDebug("Named pipe: Error occurred while creating the pipe: %d", GetLastError());
		//return 1;  //Error
	}
	else
	{
		LogDebug("Named pipe: CreateNamedPipe() was successful.");
	}

	return hPipe;
}

unsigned int __stdcall namedPipeReadThread(/*HANDLE&*//*LPVOID hPipe_tmp*/void* p)
{
	LogDebug("Named pipe: Thread created");

	namedPipeReadThreadStruct parameters = *reinterpret_cast<namedPipeReadThreadStruct*>(p);
	//namedPipeReadThreadStruct parameters = *parameters_tmp;
	if (/*hPipe_tmp*/parameters.handler == NULL)
	{
		LogDebug("Named pipe: Pipe Server Failure:");
		LogDebug("   namedPipeReadThread got an unexpected NULL value in hPipe_tmp.");
		LogDebug("   namedPipeReadThread exitting.");

		return 1; //error
	}

	HANDLE hPipe = (HANDLE)/*hPipe_tmp*/parameters.handler;

	while (true)
	{
		LogDebug("Named pipe: Wait for connection");
		BOOL  bResult = ::ConnectNamedPipe(hPipe, 0);
		DWORD dwError = GetLastError();

		if (bResult || dwError == ERROR_PIPE_CONNECTED)
		{
			BYTE  buffer[PIPE_BUFFER_SIZE] = { 0 };
			char szBuffer[PIPE_BUFFER_SIZE];
			DWORD read = 0;

			UINT   uMessage = 0;

			if (!(::ReadFile(hPipe, &buffer, PIPE_BUFFER_SIZE, &read, 0)))
			{
				LogDebug("Named pipe: Error reading pipe - %d", GetLastError());
			}
			else
			{
				LogDebug("Named pipe: received a message");
				//uMessage = *((UINT*)&buffer[0]);
				// The processing of the received data
				LogDebug("Message: %s", buffer);
				
				char tokens[] = ":,";
				char* subStr;
				char* command = NULL;
				subStr = strtok((char*)buffer, tokens);
				while (subStr != NULL)
				{
					//LogDebug("Tokenized string using *  is:: %s", subStr);

					if (command == NULL)
						command = subStr;

					if (subStr != command) {
						if (strcmp(command, "AddPids") == 0) {
							LogDebug("Neamed pipe: add pid %d", atoi(subStr));
							parameters.TsMuxer->pidfilter->Add(atoi(subStr));
						}
						else if (strcmp(command, "DelPids") == 0) {
							LogDebug("Neamed pipe: del pid %d", atoi(subStr));
							parameters.TsMuxer->pidfilter->Del(atoi(subStr));
						}
					}

					subStr = strtok(NULL, tokens);
				}

				//delete command, subStr;
				//delete[] tokens;

				//parameters.TsMuxer->pidfilter->Add();
				// Reply to client
				strcpy(szBuffer, ACK_MESG_RECV);

				bResult = WriteFile(
					hPipe,                  // handle to pipe 
					szBuffer,               // buffer to write from 
					strlen(szBuffer) + 1,   // number of bytes to write, include the NULL 
					&read,					// number of bytes written 
					NULL);					// not overlapped I/O 
			}
			::DisconnectNamedPipe(hPipe);
		}
		else
		{
			LogDebug("Named pipe: Error while waiting for client - %d", GetLastError());
		}

		::Sleep(0);
	}
}


STDMETHODIMP  CTsMuxer::IsReceiving(BOOL* yesNo)
{
	return S_OK;
}

STDMETHODIMP  CTsMuxer::Reset()
{
	return S_OK;
}

void CALLBACK CTsMuxer::checkConfigFile(
	HWND hwnd,        // handle to window for timer messages
	UINT message,     // WM_TIMER message
	UINT idTimer,     // timer identifier
	DWORD dwTime)     // current system time
{
	LogDebug("Checking config...");
}

