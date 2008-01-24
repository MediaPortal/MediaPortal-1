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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include "FireDtv.h"
static const GUID KSPROPSETID_Firesat = { 0xab132414, 0xd060, 0x11d0, { 0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba,0xf3 } };



extern void LogDebug(const char *fmt, ...) ;
/**************************************************************************************************
*   methode:		SelectPids_DVBS()
*	author:			Norbert Druml
*   description:	Select pids
*
*	created:		160904
*	last updated:	
*
*	parameters:		hDevice
*					pPids
*   return:			FIRESAT_STATUS
**************************************************************************************************/

HRESULT CFireDtv::SelectPids_DVBS(PFIRESAT_SELECT_PIDS_DVBS		pPids)
{
	HRESULT										hr;
	FIRESAT_SELECT_PIDS_DVBS	instance;

	//LogDebug("firedtv:SelectPids_DVBS() %d %d %d %d %x %x %x %x %x",
	//	sizeof(FIRESAT_SELECT_PIDS_DVBS),pPids->uNumberOfValidPids,
	//	pPids->bFullTransponder,pPids->bCurrentTransponder,
	//	pPids->uPids[0],pPids->uPids[1],pPids->uPids[2],pPids->uPids[3],pPids->uPids[4]);
	// transfer data down to the driver

	hr = m_pPropertySet -> Set(	KSPROPSETID_Firesat,
								KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S,
								&instance,
								sizeof(FIRESAT_SELECT_PIDS_DVBS),
								pPids,
								sizeof(FIRESAT_SELECT_PIDS_DVBS));


	if(hr)
	{
		LogDebug("firedtv:SelectPids_DVBS() returns %x", hr);
		return E_FAIL;
	}
	
	return S_OK;
}

/**************************************************************************************************
*   methode:		SelectPids_DVBT()
*	author:			Norbert Druml
*   description:	Select pids
*
*	created:		160904
*	last updated:	
*
*	parameters:		hDevice
*					pPids
*   return:			FIRESAT_STATUS
**************************************************************************************************/

HRESULT CFireDtv::SelectPids_DVBT(PFIRESAT_SELECT_PIDS_DVBT		pPids)
{

	HRESULT									hr;
	FIRESAT_SELECT_PIDS_DVBT				instance;

  //LogDebug("firedtv:SelectPids_DVBT()");
	// transfer data down to the driver
	hr = m_pPropertySet -> Set(	KSPROPSETID_Firesat,
								KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T,
								&instance,
								sizeof(FIRESAT_SELECT_PIDS_DVBT),
								pPids,
								sizeof(FIRESAT_SELECT_PIDS_DVBT));

	if(hr)
	{
		LogDebug("firedtv:SelectPids_DVBT() returns %x", hr);
		return E_FAIL;
	}
	
	return S_OK;
}


CFireDtv::CFireDtv(IFilterGraph *graph)
{
	m_isDvbt=false;
	CComPtr<IEnumFilters> pEnum;
	graph->EnumFilters(&pEnum);
	CComPtr<IBaseFilter> filter;
	ULONG fetched=0;
  while (filter.Release(), pEnum->Next(1,&filter,&fetched)== NOERROR)
	{
		if (fetched!=1) break;
		
		m_pPropertySet = filter;
		if (m_pPropertySet==NULL) continue;
		DWORD dwSupported=0;
		if (S_OK==m_pPropertySet->QuerySupported(KSPROPSETID_Firesat,KSPROPERTY_FIRESAT_DRIVER_VERSION,&dwSupported))
		{
			FILTER_INFO info;
			filter->QueryFilterInfo(&info);
       if (info.pGraph) info.pGraph->Release();
			char filterName[1024];
			WideCharToMultiByte(CP_ACP,0,info.achName,wcslen(info.achName),filterName,sizeof(filterName),NULL,NULL);
			filterName[wcslen(info.achName)]=0;
			LogDebug("firedtv: filtername:%s", filterName);
			if (strstr(filterName,"DVBT")!=NULL) m_isDvbt=true;
			return;
		}
		m_pPropertySet.Release();
	}
//	LogDebug("firedtv:no firedtv tuner found");
}

CFireDtv::~CFireDtv(void)
{
}

bool CFireDtv::IsFireDtv()
{
	return (m_pPropertySet!=NULL);
}

bool CFireDtv::SetPids(vector<int> pids)
{
	if (!IsFireDtv()) return false;
	if (m_isDvbt)
	{
//		LogDebug("firedtv:send pids dvbt pid count:%x", pids.size());
		FIRESAT_SELECT_PIDS_DVBT dvbt;
		memset(&dvbt,0,sizeof(dvbt));
		if (pids.size()==0)
		{
			dvbt.bCurrentTransponder=TRUE;
			dvbt.bFullTransponder=TRUE;
			dvbt.uNumberOfValidPids=0;
		}
		else
		{
			dvbt.bCurrentTransponder=TRUE;
			dvbt.bFullTransponder=FALSE;
			dvbt.uNumberOfValidPids=min(pids.size(),16);
			for (int i=0; i < dvbt.uNumberOfValidPids; ++i)
			{
				dvbt.uPids[i]=pids[i];
			}
		}
		SelectPids_DVBT(&dvbt);
	}
	else
	{
//		LogDebug("firedtv:send pids dvbs pid count:%x", pids.size());
		FIRESAT_SELECT_PIDS_DVBS dvbs;
		memset(&dvbs,0,sizeof(dvbs));
		if (pids.size()==0)
		{
			dvbs.bCurrentTransponder=TRUE;
			dvbs.bFullTransponder=TRUE;
			dvbs.uNumberOfValidPids=0;
		}
		else
		{
			dvbs.bCurrentTransponder=TRUE;
			dvbs.bFullTransponder=FALSE;
			dvbs.uNumberOfValidPids=min(pids.size(),16);
			for (int i=0; i < dvbs.uNumberOfValidPids; ++i)
			{
				dvbs.uPids[i]=pids[i];
			}
		}
		SelectPids_DVBS(&dvbs);
	}
	return true;
}

void CFireDtv::DisablePidFiltering()
{
	vector<int> pids;
	SetPids(pids);
}
