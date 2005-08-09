/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Agree
 *	http://mediaportal.sourceforge.net
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
#include <bdaiface.h>
#include "SplitterSetup.h"
#include <commctrl.h>
extern void Log(const char *fmt, ...) ;

SplitterSetup::SplitterSetup(Sections *pSections) :
m_demuxSetupComplete(FALSE),m_pSectionsPin(NULL)
{
	m_bUseATSC=false;
	m_pEPGPin=NULL;
	m_pMHW1Pin=NULL;
	m_pMHW2Pin=NULL;
	m_pSections = pSections;
}

SplitterSetup::~SplitterSetup()
{
	if(m_pSectionsPin!=NULL)
		m_pSectionsPin->Release();
	m_pSectionsPin=NULL;

	if(m_pMHW1Pin!=NULL)
		m_pMHW1Pin->Release();
	m_pMHW1Pin=NULL;

	if(m_pMHW2Pin!=NULL)
		m_pMHW2Pin->Release();
	m_pMHW2Pin=NULL;


	if(m_pEPGPin!=NULL)
		m_pEPGPin->Release();
	m_pEPGPin=NULL;
}

void SplitterSetup::UseATSC(bool yesNo)
{
	m_bUseATSC=yesNo;
}

HRESULT SplitterSetup::SetDemuxPins(IFilterGraph *pGraph)
{
	if(m_demuxSetupComplete==TRUE)
		return S_FALSE;

	if(pGraph==NULL)
		return S_FALSE;

//	Log("SetDemuxPins");
	HRESULT hr;
	IGraphBuilder *pGB=NULL;

	if(FAILED(pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGB)))
	{
		Log("SetDemuxPins failed 1");
		return S_FALSE;
	}

	IBaseFilter *pDemuxer;
	hr=pGB->FindFilterByName(L"MPEG-2 Demultiplexer",&pDemuxer);
	if(FAILED(hr))
	{
		Log("SetDemuxPins failed 2");
		pGB->Release();
		return hr;
	}

	hr=SetupDemuxer(pDemuxer);
	pGB->Release();
	pDemuxer->Release();

//	Log("SetDemuxPinsDone");
	return NOERROR;
}
HRESULT SplitterSetup::SetupDemuxer(IBaseFilter *demuxFilter)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	HRESULT hr=0;

//	Log("SetupDemuxer()");
	if(demuxFilter==NULL)
		return S_FALSE;

	IMpeg2Demultiplexer *demuxer=NULL;

	hr=demuxFilter->QueryInterface(IID_IMpeg2Demultiplexer,(void**)&demuxer);
	if(FAILED(hr))
		return hr;

	//m_pSectionsPin=NULL;
	m_demuxSetupComplete=true;
	demuxer->Release();
//	Log("SetupDemuxer() done");
	return S_OK;
}

HRESULT SplitterSetup::SetupDefaultMapping()
{
//	Log("SetupDefaultMapping()");

	SetSectionMapping();
	SetMHW1Mapping();
	SetMHW2Mapping();
	SetEPGMapping();
//	Log("SetupDefaultMapping() done");
	return 0;
}
HRESULT SplitterSetup::SetMHW1Mapping()
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	
	HRESULT hr=0;
			
//	Log("Setup MHW1");

	// video

	if(m_pMHW1Pin==NULL)
		return S_FALSE;

	hr=m_pMHW1Pin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr) || pPidEnum==NULL)
		return 7;
		
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		if (count!=1) break;
			
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
		{	
			Log("failed to unmap pids");
			return 8;
		}
	}
	pPidEnum->Release();

	if (!m_bUseATSC) 
	{
//		Log("map pid 0xd2");
		pid = (ULONG)0xd2;
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			Log("failed to map pid 0xd2");
			return 4;
		}
	}	
	pMap->Release();
	return S_OK;

}
HRESULT SplitterSetup::SetMHW2Mapping()
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	
	HRESULT hr=0;
			
//	Log("Setup MHW2");

	// video

	if(m_pMHW2Pin==NULL)
		return S_FALSE;

	hr=m_pMHW2Pin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr) || pPidEnum==NULL)
		return 7;
		
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		if (count!=1) break;
			
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
		{	
			Log("failed to unmap pids");
			return 8;
		}
	}
	pPidEnum->Release();
	
	if (!m_bUseATSC) 
	{
//		Log("map pid 0xd3");
		pid = (ULONG)0xd3;
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			Log("failed to map pid 0xd3");
			return 4;
		}
	}
	pMap->Release();
	return S_OK;
}

HRESULT SplitterSetup::SetSectionMapping()
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	
	HRESULT hr=0;
			
//	Log("Setup DVBSections");

	// video

	if(m_pSectionsPin==NULL)
	{
		Log("Setup DVBSections:pin=NULL");
		return S_FALSE;
	}
//	Log("Setup DVBSections:get map");
	hr=m_pSectionsPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
	{
		Log("Setup DVBSections:cannot get map:0x%x",hr);
		return 3;
	}	// 
//	Log("Setup DVBSections:get enummap");
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr) || pPidEnum==NULL)
	{
		Log("Setup DVBSections:cannot get ienummap:0x%x",hr);
		return 7;
	}	
//	Log("Setup DVBSections:unmap pins");
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		if (count!=1) break;
//		Log("Setup DVBSections: ummap pid:0x%x",pm.ulPID);	
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
		{	
			Log("failed to unmap pids");
			return 8;
		}
	}
	
//	Log("Setup DVBSections:pids unmapped");
	pPidEnum->Release();


	if (!m_bUseATSC)
	{
//		Log("map pid 0x0");
		pid = (ULONG)0;// pat
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			Log("failed to map pid 0x0");
			return 4;
		}
//		Log("map pid 0x10");
		pid = (ULONG)0x10;// NIT
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			Log("failed to map pid 0x10");
			return 4;
		}
//		Log("map pid 0x11");
		pid = (ULONG)0x11;// sdt
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			Log("failed to map pid 0x11");
			return 4;
		}
	}
	else
	{
//		Log("map pid 0x1ffb");
		pid = (ULONG)0x1ffb;// ATSC
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); 
		if(FAILED(hr))
		{	
			Log("failed to map pid 0x1ffb");
			return 4;
		}
	}
	pMap->Release();
	return S_OK;
}
HRESULT SplitterSetup::MapAdditionalPID(ULONG pid)
{
	IMPEG2PIDMap	*pMap=NULL;
	
//	Log ("MapAdditionalPID:%x", pid);
	if (pid>=0x1fff) 
		return S_FALSE;
	HRESULT hr=0;

	if(m_pSectionsPin==NULL)
		return S_FALSE;

	hr=m_pSectionsPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
	if(FAILED(hr))
		return 4;

	pMap->Release();

//	Log ("MapAdditionalPID:%x done", pid);
	return S_OK;
}
HRESULT SplitterSetup::MapAdditionalPayloadPID(ULONG pid)
{
	IMPEG2PIDMap	*pMap=NULL;
	
	HRESULT hr=0;

	if (m_bUseATSC) return S_OK;
	if(m_pSectionsPin==NULL)
		return S_FALSE;

	hr=m_pSectionsPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->MapPID(1,&pid,MEDIA_TRANSPORT_PAYLOAD); // tv
	if(FAILED(hr))
		return 4;

	pMap->Release();
	return S_OK;
}
bool SplitterSetup::PinIsNULL()
{
	return (m_pSectionsPin==NULL);
}
HRESULT SplitterSetup::SetSectionsPin(IPin *ppin)
{
	if(m_pSectionsPin==NULL)
		m_pSectionsPin=ppin;
	return S_OK;
}

HRESULT SplitterSetup::SetMHW1Pin(IPin *ppin)
{
	if(m_pMHW1Pin==NULL)
		m_pMHW1Pin=ppin;
	return S_OK;
}
HRESULT SplitterSetup::SetMHW2Pin(IPin *ppin)
{
	if(m_pMHW2Pin==NULL)
		m_pMHW2Pin=ppin;
	return S_OK;
}
HRESULT SplitterSetup::SetEPGPin(IPin *ppin)
{
	if(m_pEPGPin==NULL)
		m_pEPGPin=ppin;
	return S_OK;
}


HRESULT SplitterSetup::SetEPGMapping()
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	
	HRESULT hr=0;
			
//	Log("Setup EPG mapping:%x",m_pEPGPin);

	// video

	if(m_pEPGPin==NULL)
		return S_FALSE;

	hr=m_pEPGPin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr) || pMap==NULL)
		return 3;
		// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr) || pPidEnum==NULL)
		return 7;
		
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		if (count!=1) break;
			
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
		{	
			Log("failed to unmap pids");
			return 8;
		}
	}
	pPidEnum->Release();

	if (!m_bUseATSC)
	{
//		Log("map pid 0x12");
		pid = (ULONG)0x12;// EIT
		hr=pMap->MapPID(1,&pid,MEDIA_MPEG2_PSI); // tv
		if(FAILED(hr))
		{
			Log("failed to map pid 0x12");
			return 4;
		}
	}
	pMap->Release();

	return S_OK;
}

HRESULT SplitterSetup::UnMapSectionPIDs()
{
//	Log("UnMapSectionPIDs()");
	SetSectionMapping();
//	Log("UnMapSectionPIDs() done");
	return S_OK;

}
HRESULT SplitterSetup::UnMapAllPIDs()
{
//	Log("UnMapAllPIDs()");
	SetupDefaultMapping();
//	Log("UnMapAllPIDs() done");
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