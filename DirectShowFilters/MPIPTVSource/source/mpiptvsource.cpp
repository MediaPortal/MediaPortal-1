// mpiptvsource.cpp : Defines the exported functions for the DLL application.
//
#include "mpiptvsource.h"

CMPIptvSourceStream::CMPIptvSourceStream(HRESULT *phr, CSource *pFilter)
: CSourceStream(NAME("MediaPortal IPTV Stream"), phr, pFilter, L"Out"),
ip(NULL),
localip(NULL),
protocol(NULL),
port(0) ,
m_socket(-1),
m_seqNumber(0),
m_buffsize(0)
{
}

CMPIptvSourceStream::~CMPIptvSourceStream()
{
	Clear();
}

void CMPIptvSourceStream::Clear() 
{
	if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
	if(CAMThread::ThreadExists())
	{
		CAMThread::CallWorker(CMD_EXIT);
		CAMThread::Close();
	}
	if (ip) {
		CoTaskMemFree(ip);
		ip = NULL;
	}
	if (localip) {
		CoTaskMemFree(localip);
		localip = NULL;
	}
	if (protocol) {
		CoTaskMemFree(protocol);
		protocol = NULL;
	}
}

HRESULT CMPIptvSourceStream::FillBuffer(IMediaSample *pSamp)
{
	BYTE *pData;
    long cbData;

    CheckPointer(pSamp, E_POINTER);

	CAutoLock cAutoLock(m_pFilter->pStateLock());

    // Access the sample's data buffer
    pSamp->GetPointer(&pData);
    cbData = pSamp->GetSize();

	memcpy(pData, m_buffer, min(m_buffsize, cbData));

	REFERENCE_TIME rtStart = m_seqNumber;
    REFERENCE_TIME rtStop  = rtStart + 1;

    pSamp->SetTime(&rtStart, &rtStop);
	pSamp->SetActualDataLength(min(m_buffsize, cbData));

	m_seqNumber++;
	pSamp->SetSyncPoint(TRUE);

	return S_OK;
}

HRESULT CMPIptvSourceStream::GetMediaType(__inout CMediaType *pMediaType) 
{
	pMediaType->majortype = MEDIATYPE_Stream;
	pMediaType->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;

	return S_OK;
}

HRESULT CMPIptvSourceStream::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest) 
{
	HRESULT hr;
    CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(pRequest, E_POINTER);

    // If the bitmap file was not loaded, just fail here.
    
    // Ensure a minimum number of buffers
    if (pRequest->cBuffers == 0)
    {
        pRequest->cBuffers = 1;
    }
	pRequest->cbBuffer = 2 * 65536;

    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(pRequest, &Actual);
    if (FAILED(hr)) 
    {
        return hr;
    }

    // Is this allocator unsuitable?
    if (Actual.cbBuffer < pRequest->cbBuffer) 
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CMPIptvSourceStream::DoBufferProcessingLoop(void) 
{
	Command com;

    OnThreadStartPlay();

	WSADATA wsaData;
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	sockaddr_in addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
	if (localip) {
		addr.sin_addr.s_addr = inet_addr(localip);
	} else {
		addr.sin_addr.s_addr = htonl(INADDR_ANY);
	}
	addr.sin_port = htons((u_short)port);

	ip_mreq imr; 
	imr.imr_multiaddr.s_addr = inet_addr(ip);
	if (localip) {
		imr.imr_interface.s_addr = inet_addr(localip);
	} else {
		imr.imr_interface.s_addr = INADDR_ANY;
	}
    unsigned long nonblocking = 1;

	if((m_socket = socket(AF_INET, SOCK_DGRAM, 0)) >= 0)
	{
/*		u_long argp = 1;
		ioctlsocket(m_socket, FIONBIO, &argp);
*/
		DWORD dw = TRUE;
		if(setsockopt(m_socket, SOL_SOCKET, SO_REUSEADDR, (const char*)&dw, sizeof(dw)) < 0)
		{
			closesocket(m_socket);
			m_socket = -1;
		}

		if (ioctlsocket(m_socket, FIONBIO, &nonblocking) != 0) {
			closesocket(m_socket);
			m_socket = -1;
		}

		if(bind(m_socket, (struct sockaddr*)&addr, sizeof(addr)) < 0)
		{
			closesocket(m_socket);
			m_socket = -1;
		}

		if(IN_MULTICAST(htonl(imr.imr_multiaddr.s_addr)))
		{
			int ret = setsockopt(m_socket, IPPROTO_IP, IP_ADD_MEMBERSHIP, (const char*)&imr, sizeof(imr));
			if(ret < 0) ret = ::WSAGetLastError();
			ret = ret;
		}
	}

	SetThreadPriority(m_hThread, THREAD_PRIORITY_TIME_CRITICAL);

	int fromlen = sizeof(addr);

    do {
		BOOL requestAvail;
	while ((requestAvail = CheckRequest(&com)) == FALSE) {

		m_buffsize = 0;
	    do {

		    int len = recvfrom(m_socket, &m_buffer[m_buffsize], 65536, 0, (SOCKADDR*)&addr, &fromlen);

		    if(len <= 0) {
				Sleep(1);
				continue;
			}
			m_buffsize += len;
	    } while ((requestAvail = CheckRequest(&com)) == FALSE && m_buffsize <= 65536);
		if (requestAvail) break;

	    IMediaSample *pSample;

	    HRESULT hr = GetDeliveryBuffer(&pSample,NULL,NULL,0);
	    if (FAILED(hr)) {
                Sleep(1);
		continue;	// go round again. Perhaps the error will go away
			    // or the allocator is decommited & we will be asked to
			    // exit soon.
	    }

		

	    // fill buffer
		hr = FillBuffer(pSample);

	    if (hr == S_OK) {
		hr = Deliver(pSample);
                pSample->Release();

                // downstream filter returns S_FALSE if it wants us to
                // stop or an error if it's reporting an error.
                if(hr != S_OK)
                {
                  DbgLog((LOG_TRACE, 2, TEXT("Deliver() returned %08x; stopping"), hr));
				  if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
			      WSACleanup();
                  return S_OK;
                }

	    } else if (hr == S_FALSE) {
                // derived class wants us to stop pushing data
		pSample->Release();
		DeliverEndOfStream();
		if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
	    WSACleanup();
		return S_OK;
	    } else {
                // derived class encountered an error
                pSample->Release();
		DbgLog((LOG_ERROR, 1, TEXT("Error %08lX from FillBuffer!!!"), hr));
                DeliverEndOfStream();
                m_pFilter->NotifyEvent(EC_ERRORABORT, hr, 0);
				if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
			    WSACleanup();
                return hr;
	    }

            // all paths release the sample
	}

        // For all commands sent to us there must be a Reply call!

	if (com == CMD_RUN || com == CMD_PAUSE) {
	    Reply(NOERROR);
	} else if (com != CMD_STOP) {
	    Reply((DWORD) E_UNEXPECTED);
	    DbgLog((LOG_ERROR, 1, TEXT("Unexpected command!!!")));
	}
    } while (com != CMD_STOP);
    if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
	WSACleanup();
    return S_FALSE;
}

bool CMPIptvSourceStream::Load(const TCHAR* fn) 
{
	Clear();
	URL_COMPONENTS url;
	url.dwStructSize = sizeof(URL_COMPONENTS);
	url.lpszScheme = NULL;
	url.lpszExtraInfo = NULL;
	url.lpszHostName = NULL;
	url.lpszPassword = NULL;
	url.lpszUrlPath = NULL;
	url.lpszUserName = NULL;
	TCHAR *srcurl = "udp://192.168.2.197@233.1.1.1:1234";
	if (!InternetCrackUrl(srcurl, 0, 0, &url)) {
		return false;
	}
	if (!(ip = (TCHAR*) CoTaskMemAlloc((url.dwHostNameLength + 1) * sizeof(TCHAR)))) {
		return false;
	}
	memset(ip, 0, (url.dwHostNameLength + 1) * sizeof(TCHAR));
	strncat(ip, url.lpszHostName, url.dwHostNameLength);
	if (url.dwUserNameLength > 0) {
		if (!(localip = (TCHAR*) CoTaskMemAlloc((url.dwUserNameLength + 1) * sizeof(TCHAR)))) {
			return false;
		}
		memset(localip, 0, (url.dwUserNameLength + 1) * sizeof(TCHAR));
		strncat(localip, url.lpszUserName, url.dwUserNameLength);
	}
	if (!(protocol = (TCHAR*) CoTaskMemAlloc((url.dwSchemeLength + 1) * sizeof(TCHAR)))) {
		return false;
	}
	memset(protocol, 0, (url.dwSchemeLength + 1) * sizeof(TCHAR));
	strncat(protocol, url.lpszScheme, url.dwSchemeLength);
	port = url.nPort;
	
	return true;
}

// This is the constructor of a class that has been exported.
// see mpiptvsource.h for the class definition
CMPIptvSource::CMPIptvSource(IUnknown *pUnk, HRESULT *phr)
: CSource(NAME("MediaPortal IPTV Source"), pUnk, CLSID_MPIptvSource)
{
    // The pin magically adds itself to our pin array.
    m_stream = new CMPIptvSourceStream(phr, this);

    if (phr)
    {
        if (m_stream == NULL)
            *phr = E_OUTOFMEMORY;
        else
            *phr = S_OK;
    } 

	m_fn = EMPTY_STRING;
}


CMPIptvSource::~CMPIptvSource()
{
    delete m_stream;
	if (m_fn != EMPTY_STRING) {
		CoTaskMemFree(m_fn);
		m_fn = EMPTY_STRING;
	}
}

STDMETHODIMP CMPIptvSource::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

	return 
		QI(IFileSourceFilter)
		__super::NonDelegatingQueryInterface(riid, ppv);
}

STDMETHODIMP CMPIptvSource::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE* pmt) 
{
	size_t length = wcstombs(NULL, pszFileName, 0);
	if(!(m_fn = (TCHAR*)CoTaskMemAlloc((length+1)*sizeof(TCHAR))))
		return E_OUTOFMEMORY;
    wcstombs(m_fn, pszFileName, length + 1);
	if(!m_stream->Load(m_fn))
		return E_FAIL;

	return S_OK;
}

STDMETHODIMP CMPIptvSource::GetCurFile(LPOLESTR* ppszFileName, AM_MEDIA_TYPE* pmt)
{
	if(!ppszFileName) return E_POINTER;
	
	if(!(*ppszFileName = (LPOLESTR)CoTaskMemAlloc((strlen(m_fn)+1)*sizeof(WCHAR))))
		return E_OUTOFMEMORY;

	mbstowcs(*ppszFileName, m_fn, strlen(m_fn) + 1);

	return S_OK;
}

CUnknown * WINAPI CMPIptvSource::CreateInstance(IUnknown *pUnk, HRESULT *phr)
{
    CMPIptvSource *pNewFilter = new CMPIptvSource(pUnk, phr );

    if (phr)
    {
        if (pNewFilter == NULL) 
            *phr = E_OUTOFMEMORY;
        else
            *phr = S_OK;
    }

    return pNewFilter;
}
