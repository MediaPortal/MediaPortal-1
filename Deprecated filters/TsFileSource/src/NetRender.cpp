/**
*  NetRender.ccp
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include <Winsock2.h>
#include "stdafx.h"
#include "NetRender.h"
#include "TSFileSinkGuids.h"
#include "TSParserSinkGuids.h"
#include "ITSFileSink.h"
#include "ITSParserSink.h"
#include "NetworkGuids.h"
#include "TSFileSourcePin.h"
#include <stdio.h>
#include <string.h>
#include <time.h>
#include <sys/types.h>
#include <sys/timeb.h>
#include "Global.h"

#ifndef ABOVE_NORMAL_PRIORITY_CLASS
#define ABOVE_NORMAL_PRIORITY_CLASS 0x00008000
#endif

#ifndef BELOW_NORMAL_PRIORITY_CLASS
#define BELOW_NORMAL_PRIORITY_CLASS 0x00004000
#endif
//////////////////////////////////////////////////////////////////////
// NetRender
//////////////////////////////////////////////////////////////////////

CNetRender::CNetRender()
{
}

CNetRender::~CNetRender()
{
}

HRESULT CNetRender::CreateNetworkGraph(NetInfo *netAddr)
{
	HRESULT hr = S_OK;
//	BoostProcess Boost;

	//
	// Re-Create the FilterGraph if already Active
	//
	if (netAddr->pNetworkGraph)
		DeleteNetworkGraph(netAddr);
	//
    // Instantiate filter graph interface
	//
	hr = CoCreateInstance(CLSID_FilterGraph,
					NULL, CLSCTX_INPROC, 
                    IID_IGraphBuilder,
					(void **)&netAddr->pNetworkGraph);
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	// Create the either types of NetSource Filters and add it to the FilterGraph
	//
    hr = CoCreateInstance(
                        CLSID_BDA_DSNetReceive, 
                        NULL, 
                        CLSCTX_INPROC_SERVER,
                        IID_IBaseFilter, 
                        reinterpret_cast<void**>(&netAddr->pNetSource)
                        );
    if (FAILED (hr))
    {
		hr = CoCreateInstance(
                        CLSID_DSNetReceive, 
                        NULL, 
                        CLSCTX_INPROC_SERVER,
                        IID_IBaseFilter, 
                        reinterpret_cast<void**>(&netAddr->pNetSource)
                        );
		if (FAILED (hr))
		{
			DeleteNetworkGraph(netAddr);
			return hr;
		}
	}

	hr = netAddr->pNetworkGraph->AddFilter(netAddr->pNetSource, L"Network Source");
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	// Setup NetSource Filter, Get the Control Interface
	//
	CComPtr <IMulticastConfig> piMulticastConfig = NULL;
	if (FAILED(hr = netAddr->pNetSource->QueryInterface(IID_IMulticastConfig, (void**)&piMulticastConfig)))
	{
		DeleteNetworkGraph(netAddr);
        return hr;
	}

	//
	// if no NIC then set default
	//
	if (!netAddr->userNic)
		netAddr->userNic = inet_addr ("127.0.0.1");

	if (FAILED(hr = piMulticastConfig->SetNetworkInterface(netAddr->userNic))) //0 /*INADDR_ANY == 0*/ )))
	{
		DeleteNetworkGraph(netAddr);
        return hr;
	}

	//
	// if no Multicast IP then set default
	//
	if (!netAddr->userIP)
		netAddr->userIP = inet_addr ("224.0.0.0");

	//
	// if no Multicast port then set default
	//
	if (!netAddr->userPort)
		netAddr->userPort = htons ((USHORT) 1234);

	if (FAILED(hr = piMulticastConfig->SetMulticastGroup(netAddr->userIP, netAddr->userPort)))
	{
		DeleteNetworkGraph(netAddr);
        return hr;
	}

	//
	// Create the Sink Filter and add it to the FilterGraph
	//
	if (netAddr->bParserSink)
	{
		hr = CoCreateInstance(CLSID_TSParserSink, 
							NULL, 
							CLSCTX_INPROC_SERVER,
							IID_IBaseFilter, 
							reinterpret_cast<void**>(&netAddr->pFileSink)
							);
	}
	else
	{
		hr = CoCreateInstance(CLSID_TSFileSink, 
								NULL, 
								CLSCTX_INPROC_SERVER,
								IID_IBaseFilter, 
								reinterpret_cast<void**>(&netAddr->pFileSink)
								);
	}

    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	if (netAddr->bParserSink)
	{
		hr = netAddr->pNetworkGraph->AddFilter(netAddr->pFileSink, L"Memory Sink");
	}
	else
	{
		hr = netAddr->pNetworkGraph->AddFilter(netAddr->pFileSink, L"File Sink");
	}

    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	// Connect the NetSource Filter to the Sink Filter 
	//
	hr = CTSFileSourcePin::RenderOutputPins(netAddr->pNetSource);
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	// Get the Sink Filter Interface 
	//
	CComPtr <IFileSinkFilter> pIFileSink;
	hr = netAddr->pFileSink->QueryInterface(&pIFileSink);
    if(FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	// Add the Date/Time Stamp to the FileName 
	//
	WCHAR wfileName[MAX_PATH] = L"";
	_tzset();
	time(&netAddr->time);
	netAddr->tmTime = localtime(&netAddr->time);
	wcsftime(wfileName, 32, L"(%Y-%m-%d %H-%M-%S)", netAddr->tmTime);

	swprintf(netAddr->fileName, L"%S%S UDP (%S-%S-%S).tsbuffer",
								netAddr->pathName,
								wfileName,
								netAddr->strIP,
								netAddr->strPort,
								netAddr->strNic);

	//
	// Set the Sink Filter File Name 
	//
	hr = pIFileSink->SetFileName(netAddr->fileName, NULL);
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }
//MessageBoxW(NULL, netAddr->fileName, L"time", NULL);

	//
	//Register the FilterGraph in the Object Running Table
	//
	if (netAddr->rotEnable)
	{
		if (!netAddr->dwGraphRegister && FAILED(AddGraphToRot (netAddr->pNetworkGraph, &netAddr->dwGraphRegister)))
			netAddr->dwGraphRegister = 0;
	}
	
	//
	// Get the IMediaControl Interface 
	//
	CComPtr <IMediaControl> pMediaControl;
	hr = netAddr->pNetworkGraph->QueryInterface(&pMediaControl);
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	//Run the FilterGraph
	//
	hr = pMediaControl->Run(); 
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	//Wait for data to build before testing data flow
	//
//	Sleep(1000);

	//
	// Get the Sink Filter Interface 
	//
//	CComPtr <ITSParserSink> pITSFileSink;
	CComPtr <ITSFileSink> pITSFileSink;

	if (netAddr->bParserSink)
	{
		hr = netAddr->pFileSink->QueryInterface(IID_ITSParserSink, (void**)&pITSFileSink);
	}
	else
	{
		hr = netAddr->pFileSink->QueryInterface(IID_ITSFileSink, (void**)&pITSFileSink);
	}

    if(FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	__int64 llDataFlow = 0;
	int count = 0;

	//
	// Loop until we have data or time out 
	//
	while(llDataFlow < 20000 && count < 25) //2000000
	{
		Sleep(200);
		hr = pITSFileSink->GetFileBufferSize(&llDataFlow);
		if(FAILED (hr))
		{
			DeleteNetworkGraph(netAddr);
			return hr;
		}

		if (llDataFlow > (__int64)(netAddr->buffSize + (__int64)1000))
		{
			netAddr->buffSize = llDataFlow; 
			count = max(0, count-1);
		}
		count++;
	}

	//
	// Check for data flow one last time incase it has stopped 
	//
	Sleep(500);
	pITSFileSink->GetFileBufferSize(&llDataFlow);

	if(llDataFlow < 20000 || (llDataFlow < (__int64)(netAddr->buffSize + (__int64)1)))//2000000
    {
		DeleteNetworkGraph(netAddr);
		return VFW_E_INVALIDMEDIATYPE;//ERROR_CANNOT_MAKE;
    }

	//
	// Get the filename from the sink filter just in case it has changed 
	//
	LPWSTR pwFileName = new WCHAR[MAX_PATH];
	hr = pITSFileSink->GetBufferFileName(pwFileName);
	if(FAILED (hr))
	{
		DeleteNetworkGraph(netAddr);
		return hr;
	}
	swprintf(netAddr->fileName, L"%S", pwFileName);
	if (pwFileName)
		delete[] pwFileName;
	
//MessageBoxW(NULL, netAddr->fileName, ptFileName, NULL);
	SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);

    return hr;
}

HRESULT CNetRender::RestartNetworkGraph(NetInfo *netAddr)
{
//	BoostThread Boost;
	HRESULT hr = S_OK;
	//
	// Get the IMediaControl Interface 
	//
	CComPtr <IMediaControl> pMediaControl;
	hr = netAddr->pNetworkGraph->QueryInterface(&pMediaControl);
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	//Run the FilterGraph
	//
	hr = pMediaControl->Stop(); 
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	//
	//Wait for data to build before testing data flow
	//
	Sleep(100);

	//
	//Run the FilterGraph
	//
	hr = pMediaControl->Run(); 
    if (FAILED (hr))
    {
		DeleteNetworkGraph(netAddr);
        return hr;
    }

	return hr;

}

void CNetRender::DeleteNetworkGraph(NetInfo *netAddr)
{
	//
	// Test if the filtergraph exists 
	//
	if(netAddr->pNetworkGraph)
	{
		//
		// Stop the graph just in case 
		//
		IMediaControl *pMediaControl;
		if (SUCCEEDED(netAddr->pNetworkGraph->QueryInterface(IID_IMediaControl, (void **) &pMediaControl)))
		{
			pMediaControl->Stop(); 
			pMediaControl->Release();
		}

		//
		// Unregister the filtergraph from the object table
		//
		if (netAddr->dwGraphRegister)
		{
			RemoveGraphFromRot(netAddr->dwGraphRegister);
			netAddr->dwGraphRegister = 0;
		}
	}

	//
	// Release the filtergraph filters & graph
	//
	if(netAddr->pNetSource) netAddr->pNetSource->Release();
	if(netAddr->pFileSink) netAddr->pFileSink->Release();
	if(netAddr->pNetworkGraph) netAddr->pNetworkGraph->Release();
	netAddr->pNetworkGraph = NULL;
	netAddr->pNetSource = NULL;
	netAddr->pFileSink = NULL;
}


BOOL CNetRender::UpdateNetFlow(NetInfoArray *netArray)
{
	//
	// return if the array is empty, no running graphs
	//
	if (!(*netArray).Count())
		return FALSE;

	HRESULT hr;
	//
	// loop through the array
	for( int i = 0; i < (int)(*netArray).Count(); i++)
	{
		//
		// Get the Sink Filter Interface 
		//
		ITSFileSink *pITSFileSink;
		hr = (*netArray)[i].pFileSink->QueryInterface(IID_ITSFileSink, (void**)&pITSFileSink);
		if(SUCCEEDED(hr))
		{
			__int64 buffSizeSave = (*netArray)[i].buffSize; 
			hr = pITSFileSink->GetFileBufferSize(&(*netArray)[i].buffSize);
			if (SUCCEEDED(hr))
			{
				(*netArray)[i].flowRate = max(0, (*netArray)[i].buffSize - buffSizeSave);
				//Save the last time value
				if ((*netArray)[i].flowRate)
				{
					(*netArray)[i].lastTime = timeGetTime();
					(*netArray)[i].retry = 0;
				}
			}

			pITSFileSink->Release();
		}
		//
		//Check if data is flowing and restart graph if not
		if ((*netArray)[i].lastTime && !(*netArray)[i].playing)
		{
			if((*netArray)[i].retry > 5)
			{
				netArray->RemoveAt(i);
				return hr;
			}

			if(((*netArray)[i].lastTime + 2000) < timeGetTime())
			{
				if (FAILED(RestartNetworkGraph(&(*netArray)[i])))
				{
					netArray->RemoveAt(i);
					return hr;
				}
				(*netArray)[i].lastTime = timeGetTime();
				(*netArray)[i].retry = (*netArray)[i].retry + 1;
			}

		}
	}
	return hr;
}

BOOL CNetRender::IsMulticastActive(NetInfo *netAddr, NetInfoArray *netArray, int *pos)
{
	//
	// return if the array is empty, no running graphs
	//
	if (!(*netArray).Count())
		return FALSE;

	//
	// loop through the array for graphs that are already using the net IP
	for((*pos) = 0; (*pos) < (int)(*netArray).Count(); (*pos)++)
	{
		if (netAddr->userIP == (*netArray)[*pos].userIP
			&& netAddr->userPort == (*netArray)[*pos].userPort
			&& netAddr->userNic == (*netArray)[*pos].userNic)
		{
			//
			// return the filename of the found graph
			//
			wcscpy(netAddr->fileName, (*netArray)[*pos].fileName);
//MessageBoxW(NULL, netAddr->fileName, L"IsMulticastActive", NULL);
			return TRUE;
		}
	}
	return FALSE;
}

BOOL CNetRender::IsMulticastAddress(LPOLESTR lpszFileName, NetInfo *netAddr)
{
	wcslwr(lpszFileName);
	LPWSTR portPosFile = NULL;
	LPWSTR portPosUrl = NULL;
	LPWSTR nicPosFile = NULL;
	LPWSTR nicPosUrl = NULL;
	LPWSTR addrPosFile = wcsstr(lpszFileName, L"udp@");
	LPWSTR addrPosUrl = wcsstr(lpszFileName, L"udp://");
	LPWSTR endString = lpszFileName + wcslen(lpszFileName);
	LPWSTR endPos = NULL;

//MessageBoxW(NULL, lpszFileName,lpszFileName, NULL);
	//Check if we have a valid Network Address
	if (addrPosFile && addrPosFile < endString)
	{
		addrPosFile = min(addrPosFile + 4, endString);
		portPosFile = wcsstr(addrPosFile, L"#");
	}
	else if (addrPosUrl && addrPosUrl < endString)
	{
		addrPosUrl = min(addrPosUrl + 6, endString);
		portPosUrl = wcsstr(addrPosUrl, L":");
	}
	else
		return FALSE;

//MessageBoxW(NULL, addrPosUrl,L"addrPosUrl", NULL);
	//Check if we have a valid Port
	if (portPosFile && portPosFile < endString)
	{
		portPosFile = min(portPosFile + 1, endString);
		nicPosFile = wcsstr(portPosFile, L"$");
	}
	else if (portPosUrl && portPosUrl < endString)
	{
		portPosUrl = min(portPosUrl + 1, endString);
		nicPosUrl = wcsstr(portPosUrl, L"@");
	}
	else
		return FALSE;

//MessageBoxW(NULL, portPosUrl,L"portPosUrl", NULL);
	//Check if we have a valid nic
	if (nicPosFile && nicPosFile < endString)
	{
		nicPosFile = min(nicPosFile + 1, endString);
		endPos = wcsrchr(nicPosFile, '.');
	}
	else if (nicPosUrl && nicPosUrl < endString)
	{
		nicPosUrl = min(nicPosUrl + 1, endString);
		endPos = endString+1;
	}
	else
		return FALSE;

//MessageBoxW(NULL, nicPosUrl,L"nicPosUrl", NULL);
	//Check if we have an end extension such as a ".ts"
	if (!endPos)
		endPos = endString+1;

	if (addrPosFile)
		lstrcpynW(netAddr->strIP, addrPosFile, min(15,max(0,(portPosFile - addrPosFile))));
	else if(addrPosUrl)
		lstrcpynW(netAddr->strIP, addrPosUrl, min(15,max(0,(portPosUrl - addrPosUrl))));
	else
		return FALSE;

//MessageBoxW(NULL, netAddr->strIP,L"netAddr->strIP", NULL);

	TCHAR temp[MAX_PATH];
	sprintf((char *)temp, "%S", netAddr->strIP);	
	netAddr->userIP = inet_addr ((const char*)&temp);
	if (!IsMulticastingIP(netAddr->userIP))
		return FALSE;

	if (portPosFile)
		lstrcpynW(netAddr->strPort, portPosFile, min(5,max(0,(nicPosFile - portPosFile))));
	else if(portPosUrl)
		lstrcpynW(netAddr->strPort, portPosUrl, min(5,max(0,(nicPosUrl - portPosUrl))));

//MessageBoxW(NULL, netAddr->strPort,L"netAddr->strPort", NULL);

	sprintf((char *)temp, "%S", netAddr->strPort);	
//MessageBox(NULL, temp,"temp", NULL);
	netAddr->userPort = htons ((USHORT) (StrToInt((const char*)&temp) & 0x0000ffff));
	if (!netAddr->userPort)
		return FALSE;

	if (nicPosFile)
		lstrcpynW(netAddr->strNic, nicPosFile, min(15,max(0,(endPos - nicPosFile + 1))));
	else if(nicPosUrl)
		lstrcpynW(netAddr->strNic, nicPosUrl, min(15,max(0,(endPos - nicPosUrl + 1))));

//MessageBoxW(NULL, netAddr->strNic,L"netAddr->strNic", NULL);

	sprintf((char *)temp, "%S", netAddr->strNic);	
	netAddr->userNic = inet_addr ((const char*)&temp);
	if (!IsUnicastingIP(netAddr->userNic))
		return FALSE;

	//copy path to new filename
	if (addrPosFile)
		lstrcpynW(netAddr->pathName, lpszFileName, min(MAX_PATH,max(0,(addrPosFile - lpszFileName - 3))));
	else if(addrPosUrl)
		lstrcpynW(netAddr->pathName, lpszFileName, min(MAX_PATH,max(0,(addrPosUrl - lpszFileName - 5))));

//MessageBoxW(NULL, netAddr->pathName,L"netAddr->pathName", NULL);
	//Addon in Multicast Address
	swprintf(netAddr->fileName, L"%SUDP (%S-%S-%S).tsbuffer",
								netAddr->pathName,
								netAddr->strIP,
								netAddr->strPort,
								netAddr->strNic);

//MessageBoxW(NULL, netAddr->fileName,netAddr->pathName, NULL);

	return TRUE;
}

static TBYTE Highest_IP[] = {239, 255, 255, 255};
static TBYTE Lowest_IP[] = {224, 0, 0, 0};

BOOL CNetRender::IsUnicastingIP(DWORD dwNetIP)
{
    return (((TBYTE *)&dwNetIP)[0] < Lowest_IP[0]) ;
}

BOOL CNetRender::IsMulticastingIP(DWORD dwNetIP)
{
    return (((TBYTE *)&dwNetIP)[0] >= Lowest_IP[0] &&
            ((TBYTE *)&dwNetIP)[0] <= Highest_IP[0]) ;
}

// Adds a DirectShow filter graph to the Running Object Table,
// allowing GraphEdit to "spy" on a remote filter graph.
HRESULT CNetRender::AddGraphToRot(
        IUnknown *pUnkGraph, 
        DWORD *pdwRegister
        ) 
{
    CComPtr <IMoniker>              pMoniker;
    CComPtr <IRunningObjectTable>   pROT;
    WCHAR wsz[128];
    HRESULT hr;

    if (FAILED(GetRunningObjectTable(0, &pROT)))
        return E_FAIL;

    swprintf(wsz, L"FilterGraph %08x pid %08x\0", (DWORD_PTR) pUnkGraph, 
              GetCurrentProcessId());
	
	//Search the ROT for the same reference
	IUnknown *pUnk = NULL;
	if (SUCCEEDED(GetObjectFromROT(wsz, &pUnk)))
	{
		//Exit out if we have an object running in ROT
		if (pUnk)
		{
			pUnk->Release();
			return S_OK;
		}
	}

    hr = CreateItemMoniker(L"!", wsz, &pMoniker);
    if (SUCCEEDED(hr))
	{
        hr = pROT->Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, pUnkGraph, 
                            pMoniker, pdwRegister);
	}
    return hr;
}
        
// Removes a filter graph from the Running Object Table
void CNetRender::RemoveGraphFromRot(DWORD pdwRegister)
{
    CComPtr <IRunningObjectTable> pROT;

    if (SUCCEEDED(GetRunningObjectTable(0, &pROT))) 
        pROT->Revoke(pdwRegister);
}

HRESULT CNetRender::GetObjectFromROT(WCHAR* wsFullName, IUnknown **ppUnk)
{
	if( *ppUnk )
		return E_FAIL;

	HRESULT	hr;

	IRunningObjectTablePtr spTable;
	IEnumMonikerPtr	spEnum = NULL;
	_bstr_t	bstrtFullName;

	bstrtFullName = wsFullName;

	// Get the IROT interface pointer
	hr = GetRunningObjectTable( 0, &spTable ); 
	if (FAILED(hr))
		return E_FAIL;

	// Get the moniker enumerator
	hr = spTable->EnumRunning( &spEnum ); 
	if (SUCCEEDED(hr))
	{
		_bstr_t	bstrtCurName; 

		// Loop thru all the interfaces in the enumerator looking for our reqd interface 
		IMonikerPtr spMoniker = NULL;
		while (SUCCEEDED(spEnum->Next(1, &spMoniker, NULL)) && (NULL != spMoniker))
		{
			// Create a bind context 
			IBindCtxPtr spContext = NULL;
			hr = CreateBindCtx(0, &spContext); 
			if (SUCCEEDED(hr))
			{
				// Get the display name
				WCHAR *wsCurName = NULL;
				hr = spMoniker->GetDisplayName(spContext, NULL, &wsCurName );
				bstrtCurName = wsCurName;

				// We have got our required interface pointer //
				if (SUCCEEDED(hr) && bstrtFullName == bstrtCurName)
				{ 
					hr = spTable->GetObject( spMoniker, ppUnk );
					return hr;
				}	
			}
			spMoniker.Release();
		}
	}
	return E_FAIL;
}

