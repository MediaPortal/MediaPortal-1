/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
#ifndef __MPIPTVGUIDS_DEFINED
#define __MPIPTVGUIDS_DEFINED

#define _CRT_SECURE_NO_WARNINGS

#include <streams.h>
#pragma comment(lib, "wininet.lib")

#include <wininet.h>

// {D3DD4C59-D3A7-4b82-9727-7B9203EB67C0}
DEFINE_GUID(CLSID_MPIptvSource, 
0xd3dd4c59, 0xd3a7, 0x4b82, 0x97, 0x27, 0x7b, 0x92, 0x3, 0xeb, 0x67, 0xc0);

#define QI(i) (riid == __uuidof(i)) ? GetInterface((i*)this, ppv) :
#define IN_MULTICAST(i)            (((long)(i) & 0xf0000000) == 0xe0000000)

// url format: udp://[interface]@ip:port, example: udp://192.168.1.44@233.2.3.4:1000, rtp://@233.2.3.4:1000

#define EMPTY_STRING ""
#define UDP_PROTOCOL "udp"
#define RTP_PROTOCOL "rtp"

class CMPIptvSourceStream : public CSourceStream
{

protected:

	TCHAR* protocol; // udp/rtp
	TCHAR* ip;
	WORD port;
	TCHAR* localip;
	SOCKET m_socket;

	DWORD m_seqNumber;
	char m_buffer[65536*2];
	int m_buffsize;

	HRESULT FillBuffer(IMediaSample *pSamp);
	HRESULT GetMediaType(__inout CMediaType *pMediaType);
	HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest);
    HRESULT DoBufferProcessingLoop(void);

public:

    CMPIptvSourceStream(HRESULT *phr, CSource *pFilter);
    ~CMPIptvSourceStream();

	bool Load(const TCHAR* fn);
	void Clear();

};

// This class is exported from the mpiptvsource.dll
class CMPIptvSource : public CSource, public IFileSourceFilter
{

private:
    // Constructor is private because you have to use CreateInstance
    CMPIptvSource(IUnknown *pUnk, HRESULT *phr);
    ~CMPIptvSource();

    CMPIptvSourceStream *m_stream;
	TCHAR* m_fn;

public:
	// IFileSourceFilter
	DECLARE_IUNKNOWN
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
	STDMETHODIMP Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE* pmt);
	STDMETHODIMP GetCurFile(LPOLESTR* ppszFileName, AM_MEDIA_TYPE* pmt);
	static CUnknown * WINAPI CreateInstance(IUnknown *pUnk, HRESULT *phr);  
};

#endif
