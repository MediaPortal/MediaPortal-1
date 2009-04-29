/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: Agree
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
#pragma warning(disable: 4786)

#include <windows.h>

#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include "SplitterSetup.h"
#include <commctrl.h>
extern void Log(const char *fmt, ...) ;

SplitterSetup::SplitterSetup(Sections *pSections) 
{
	m_bUseATSC=false;
	m_pSections = pSections;
}

SplitterSetup::~SplitterSetup()
{
}

void SplitterSetup::UseATSC(bool yesNo)
{
	m_bUseATSC=yesNo;
}



HRESULT SplitterSetup::SetSectionMapping(IPin* pPin)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	
	IPin* pConnectedTo=NULL;
	HRESULT hr=pPin->ConnectedTo(&pConnectedTo);
	if (!SUCCEEDED(hr))
	{
		Log("Setup DVBSections:pConnectedTo=NULL");
		return hr;
	}
			
	hr=pConnectedTo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
	{
		pConnectedTo->Release();
		pConnectedTo=NULL;
		return 3;
	}	

	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr) || pPidEnum==NULL)
	{
		pMap->Release();
		pMap=NULL;
		pConnectedTo->Release();
		pConnectedTo=NULL;
		return 7;
	}	
//	Log("Setup DVBSections:unmap pins");
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		if (count!=1) break;
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
		{	
			pPidEnum->Release();
			pPidEnum=NULL;
			pMap->Release();
			pMap=NULL;
			pConnectedTo->Release();
			pConnectedTo=NULL;
			return 8;
		}
	}
	
	pPidEnum->Release();
	pPidEnum=NULL;


	if (!m_bUseATSC)
	{
		Log("map pid 0x0");
		pid = (ULONG)0;// pat
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			pMap->Release();
			pMap=NULL;
			pConnectedTo->Release();
			pConnectedTo=NULL;
			return 4;
		}

		pid = (ULONG)0x10;// NIT
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			pMap->Release();
			pMap=NULL;
			pConnectedTo->Release();
			pConnectedTo=NULL;
			return 4;
		}

		pid = (ULONG)0x11;// sdt
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			pMap->Release();
			pMap=NULL;
			pConnectedTo->Release();
			pConnectedTo=NULL;
			return 4;
		}
	}
	else
	{
		pid = (ULONG)0x1ffb;// ATSC
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); 
		if(FAILED(hr))
		{	
			pMap->Release();
			pMap=NULL;
			pConnectedTo->Release();
			pConnectedTo=NULL;
			return 4;
		}
	}
	pMap->Release();
	pMap=NULL;
	pConnectedTo->Release();
	pConnectedTo=NULL;
	return S_OK;
}

HRESULT SplitterSetup::MapAdditionalPID(IPin* pPin,ULONG pid)
{
	if (pid>=0x1fff) 
		return S_FALSE;

	IPin* pConnectedTo=NULL;
	HRESULT hr=pPin->ConnectedTo(&pConnectedTo);
	if (!SUCCEEDED(hr))
	{
		Log("Setup DVBSections:pConnectedTo=NULL");
		return hr;
	}
	IMPEG2PIDMap	*pMap=NULL;
	
	Log ("MapAdditionalPID:%x", pid);

	hr=pConnectedTo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
	{
		pConnectedTo->Release();
		pConnectedTo=NULL;
		return 3;
	}	// 
	hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
	if(FAILED(hr))
	{
		pMap->Release();
		pMap=NULL;
		pConnectedTo->Release();
		pConnectedTo=NULL;
		return 4;
	}
	pMap->Release();
	pMap=NULL;
	pConnectedTo->Release();
	pConnectedTo=NULL;
	return S_OK;
}

HRESULT SplitterSetup::MapAdditionalPayloadPID(IPin* pPin,ULONG pid)
{
	if (m_bUseATSC) return S_OK;
	IPin* pConnectedTo=NULL;
	HRESULT hr=pPin->ConnectedTo(&pConnectedTo);
	if (!SUCCEEDED(hr))
	{
		Log("Setup DVBSections:pConnectedTo=NULL");
		return hr;
	}
	IMPEG2PIDMap	*pMap=NULL;
	

	hr=pConnectedTo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
	{
		pConnectedTo->Release();
		pConnectedTo=NULL;
		return 3;
	}	// 
	hr=pMap->MapPID(1,&pid,MEDIA_TRANSPORT_PAYLOAD); // tv
	if(FAILED(hr))
	{
		pConnectedTo->Release();
		pConnectedTo=NULL;
		pMap->Release();
		pMap=NULL;
		return 4;
	}

	pConnectedTo->Release();
	pConnectedTo=NULL;
	pMap->Release();
	pMap=NULL;
		
	return S_OK;
}


HRESULT SplitterSetup::GetPSIMedia(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_MPEG2_SECTIONS;
	pintype->subtype = MEDIASUBTYPE_DVB_SI;
	return S_OK;
}

