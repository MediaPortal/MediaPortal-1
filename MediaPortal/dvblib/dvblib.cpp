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

#include <stdio.h>
#include "stdafx.h"
#include <initguid.h>
#include <streams.h>
//#include <dshowasf.h>
#include <comdef.h>
//#include <Dshow.h>
#include <bdaiface.h>
#include "Include\b2c2mpeg2adapter.h"
#include "mpeg2data.h"
#include "dvblib.h"
#include "Include\b2c2_defs.h"
#include "Include\B2C2_Guids.h"
#include "Include\b2c2mpeg2adapter.h"
#include "Include\ib2c2mpeg2tunerctrl.h"
#include "Include\ib2c2mpeg2datactrl.h"
#include "Include\ib2c2mpeg2avctrl.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define MAX_IP_PIDS			34
#define MAX_TS_PIDS			39

#define PID_MAX 			0x1fff

// copied from "../sky2pcavsrc/constants.h"; better move b2c2_defs.h
#define SKY2PC_E_OUTOFLOCK						0x90010115
#define ALPHA_VALUE    0.6f     // Alpha value for bitmap (0.0 to 1.0)
#define BMP_SIZE_X     0.3f     // Width of bitmap in comp. space
#define BMP_SIZE_Y     0.3f     // Height of bitmap in comp. space

DEFINE_GUID(CLSID_Demux, 0xAFB6C280, 0x2C41, 0x11D3, 0x8A, 0x60, 0x00, 0x00, 0xF8, 0x1E, 0x0E, 0x4A);
DEFINE_GUID(CLSID_MPSECTAB, 0xC666E115, 0xBB62, 0x4027, 0xA1, 0x13, 0x82, 0xD6, 0x43, 0xFE, 0x2D, 0x99);
const IID IID_IMpeg2Data = {0x9B396D40, 0xF380, 0x4e3c, 0xA5, 0x14, 0x1A,0x82, 0xBF, 0x6E, 0xBF, 0xE6};
DEFINE_GUID(MPEG_SEC_TYPE,0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
DEFINE_GUID(CLSID_GrabberSample, 
0x2fa4f053, 0x6d60, 0x4cb0, 0x95, 0x3, 0x8e, 0x89, 0x23, 0x4f, 0x3f, 0x73);

DEFINE_GUID(IID_IGrabberSample, 
0x6b652fff, 0x11fe, 0x4fce, 0x92, 0xad, 0x02, 0x66, 0xb5, 0xd7, 0xc7, 0x8f);

//
DeliverSectionData m_callBack;
#define MAX_DATA_PIDS_PER_PIN	39
// pid array
long m_pidArray[39];
long m_pidArrayCount;
//
//
IMpeg2Data			*m_mpegSIInterface=NULL;
IBaseFilter			*m_demux=NULL;
IBaseFilter			*m_mpeg2Data=NULL;
ISectionList		*m_sectionList=NULL;

typedef enum 
{
	DATA_PIN_0,
	DATA_PIN_1,
	DATA_PIN_2,
	DATA_PIN_3
} tDataPinIndex;
//
// callbacks

#if defined WIN32
	#define MAX_ADDR_FORMAT_STR		"%02X-%02X-%02X-%02X-%02X-%02X"
#else // __linux__
	#define MAX_ADDR_FORMAT_STR		"%02X:%02X:%02X:%02X:%02X:%02X"
#endif //defined WIN32




BEGIN_MESSAGE_MAP(CdvblibApp, CWinApp)
END_MESSAGE_MAP()

static char* logbuffer=NULL;

void Log(const char *fmt, ...) 
{
	if (logbuffer==NULL)
	{
		logbuffer=new char[100000];
	}
	va_list ap;
	va_start(ap,fmt);

	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("log/dvb.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			logbuffer);
		fclose(fp);
	}
};

// CdvblibApp-Erstellung

CdvblibApp::CdvblibApp()
{
	// TODO: Hier Code zur Konstruktion einfügen
	// Alle wichtigen Initialisierungen in InitInstance positionieren

}


// Das einzige CdvblibApp-Objekt

CdvblibApp theApp;


// CdvblibApp Initialisierung

BOOL CdvblibApp::InitInstance()
{
	CWinApp::InitInstance();
	return TRUE;
}
HRESULT GetSNR(IB2C2MPEG2TunerCtrl2 *pTunerCtrl,long *sigStrength,long *sigQuality)
{
	pTunerCtrl->GetSignalStrength(sigStrength);
	pTunerCtrl->GetSignalQuality(sigQuality);
	return S_OK;
}
bool GetSectionData(IBaseFilter *filter,PID pid,TID tid,WORD *sectionCount,int tableSection,int timeout)
{
	
	IMpeg2Data		*pMPEG=NULL;
	HRESULT			hr;
	IMpeg2Stream	*pStream = NULL;

	if(filter==NULL)
	{
		filter=m_mpeg2Data;
		if(m_mpeg2Data==NULL)
			return false;
	}

	hr=filter->QueryInterface(IID_IMpeg2Data,(void**)&pMPEG);
	if(FAILED(hr))
		return false;
	
	// s
	if(timeout<1 || timeout>20000) // max. timeout 20 sec
		timeout=1;
	// grab table or section
	if(tableSection==0)
		hr = pMPEG->GetTable(pid, tid, NULL, (DWORD)timeout, &m_sectionList);
	else
		hr = pMPEG->GetSection(pid, tid, NULL, (DWORD)timeout, &m_sectionList);

	// ok?
	if (SUCCEEDED(hr))
	{
		
		m_sectionList->GetNumberOfSections(sectionCount);
		return true;
	}
 
	return false;
}
//
//

bool GetSectionPtr(int section,long *dataPointer,int *len,int *header,int *tableExtId,int *version,int *secNum,int *lastSecNum)
{
	if(m_sectionList!=NULL)
	{
		SECTION *sec=NULL;		
		DWORD size=0;
		m_sectionList->GetSectionData(section,&size,&sec);
		LONG_SECTION *pLongSection = (LONG_SECTION*) sec;
		*dataPointer=(long)pLongSection->RemainingData;
		*len=size;
		*header=sec->Header.W;
		*tableExtId=pLongSection->TableIdExtension;
		*secNum=pLongSection->SectionNumber;
		*lastSecNum=pLongSection->LastSectionNumber;
		*version=pLongSection->Version.B;
		return true;
	}
	return false;
}
bool ReleaseSectionsBuffer(void)
{
	if(m_sectionList!=NULL)
		m_sectionList->Release();
	m_sectionList=NULL;
	return true;
}
//
//
//

BOOL GetSectionCount(IBaseFilter *filter,PID pid,TID tid,int sectionNumber,WORD *sectionCount,long *dataLen)
{
	IMpeg2Data		*pMPEG=NULL;
	ISectionList	*pSectionList=NULL;
	long			len=0;
	HRESULT			hr;

	hr=filter->QueryInterface(IID_IMpeg2Data,(void**)&pMPEG);
	if(FAILED(hr))
		return false;

	hr = pMPEG->GetTable(pid, tid, NULL, 5000L, &pSectionList);
	if (SUCCEEDED(hr))
	{
		SECTION *pSection;

        DWORD cbSize;

		hr=pSectionList->GetNumberOfSections(sectionCount);
		for(WORD i=0;i<*sectionCount;i++)
		{
			hr=pSectionList->GetSectionData(i,&cbSize,&pSection);

			len+=cbSize;
		}
		*dataLen=len;
		pSectionList->Release();
		pMPEG->Release();
		delete pSection;
	}else
		return false;

	return true;
}
// 

BOOL DeleteAllPIDs(IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl,long pin)
{
    HRESULT				hr;
	long				pidCount=39;
	long				pids[39];


	if(pB2C2FilterDataCtrl)
	{
		do
		{
			hr=pB2C2FilterDataCtrl->GetTsState(NULL,NULL,&pidCount,pids);
			if(SUCCEEDED(hr))
			{
				hr=pB2C2FilterDataCtrl->DeletePIDsFromPin(pidCount,pids,pin);
			} else
				return false;

		}while(pidCount>0);
	}
	
	return true;
}

//
// get tab without tune request
//


long SetPidToPin(IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl,long pin,long pid)
{
	long	count=1;
	long	pids[2];
	HRESULT hr;

	pids[0]=pid;

	if(pB2C2FilterDataCtrl)
	{
		hr=pB2C2FilterDataCtrl->AddPIDsToPin(&count,pids,pin);
		if(SUCCEEDED(hr))
			return count;
	}
	return 0;
}
HRESULT GetPidMap(IPin* pin, unsigned long* pid, unsigned long* mediasampletype)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;

	int hr=pin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
    if(FAILED(hr))
		return 1;
	// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr))
		return 5;
	// enum and unmap the pids
	PID_MAP			pm;
	ULONG			count;
	if(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		*pid=pm.ulPID;
		*mediasampletype=pm.MediaSampleContent;
	}
	
	pPidEnum->Release();
	pMap->Release();
	return 0;
}

HRESULT DeliverMediaSample(IPin *inputPin,IMediaSample *mediaSample)
{
	IMemInputPin *pMIP=NULL;
	HRESULT hr;

	hr=inputPin->QueryInterface(IID_IMemInputPin,(void**)&pMIP);
	if(FAILED(hr))
		return hr;

	hr=pMIP->Receive(mediaSample);
	if(FAILED(hr))
		return hr;

	return S_OK;
}

HRESULT SetupDemuxer(IPin *pVideo,int videoPID,IPin *pAudio,int audioPID,IPin *pAudioAC3,int AC3PID)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	int				maxCounter;
	HRESULT hr=0;

	// video
	if (pVideo!=NULL)
	{
		hr=pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
		{
			Log("unable to get IMPEG2PIDMap :0x%x",hr);
			return 1;
		}
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
		{
			Log("unable to get IEnumPIDMap :0x%x",hr);
			return 5;
		}
		// enum and unmap the pids
		maxCounter=30;
		while(pPidEnum->Next(1,&pm,&count)== S_OK)
		{
			maxCounter--;
			if (maxCounter<0) break;
			if (count !=1) break;
			
			Log("unable unmap pid :0x%x",pm.ulPID);
			umPid=pm.ulPID;
			hr=pMap->UnmapPID(1,&umPid);
			if(FAILED(hr))
			{
				Log("unable to unmap pid :0x%x",hr);
				return 6;
			}
		}
		pPidEnum->Release();
		if (videoPID>0 && videoPID<0x1fff)
		{
			// map new pid
			pid = (ULONG)videoPID;
			hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
			if(FAILED(hr))
			{
				Log("unable to map pid :0x%x",hr);
				return 2;
			}
		}
		pMap->Release();
	}
	
	// audio 
	if (pAudio!=NULL)
	{
		hr=pAudio->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
			return 3;
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
			return 7;
		// enum and unmap the pids
		maxCounter=20;
		while(pPidEnum->Next(1,&pm,&count)== S_OK)
		{
			if (count!=1) break;
			
			maxCounter--;
			if (maxCounter<0) break;
			umPid=pm.ulPID;
			hr=pMap->UnmapPID(1,&umPid);
			if(FAILED(hr))
				return 8;
		}
		pPidEnum->Release();
		if (audioPID>0 && audioPID <0x2000)
		{
			pid = (ULONG)audioPID;
			hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
			if(FAILED(hr))
				return 4;
		}
		pMap->Release();
	}

	// AC3
	if (pAudioAC3!=NULL)
	{
		hr=pAudioAC3->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
			return 3;
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
			return 7;
		// enum and unmap the pids
		maxCounter=20;
		while(pPidEnum->Next(1,&pm,&count)== S_OK)
		{
			maxCounter--;
			if (maxCounter<0) break;
			if (count!=1) break;
			umPid=pm.ulPID;
			hr=pMap->UnmapPID(1,&umPid);
			if(FAILED(hr))
				return 8;
		}
		pPidEnum->Release();
		if (AC3PID>0 && AC3PID<0x1fff)
		{
			pid = (ULONG)AC3PID;
			hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
			if(FAILED(hr))
			return 4;
		}

		pMap->Release();
	}
	return S_OK;

}

HRESULT GetPidMapping(IPin *pVideo,int* pids, int* elementary_stream, int* count)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP		*pm = new PID_MAP[30];
	ULONG			pidCount;
	ULONG			umPid;
	HRESULT hr=0;
	//Log("GetPidMapping() %d",sizeof(int));
	try
	{
		if (pVideo==NULL)
		{
			Log("video pin=0");
			delete[] pm;
			*count=0;
			return S_OK;
		}
		hr=pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
		{
			Log("unable to get IMPEG2PIDMap :0x%x",hr);
			delete[] pm;
			*count=0;
			return S_OK;
		}
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
		{
			Log("unable to get IEnumPIDMap :0x%x",hr);
			delete[] pm;
			*count=0;
			return S_OK;
		}
		// enum and unmap the pids
		hr=pPidEnum->Next(30,&pm[0],&pidCount);
		if(FAILED(hr) || pidCount<=0)
		{
			Log("unable to get pids:0x%x count:%d",hr,pidCount);
			delete[] pm;
			*count=0;
			return S_OK;
		}

		//Log("GetPidMapping() 1");
		int maxCount=*count;
		if (maxCount>pidCount) maxCount=pidCount;

		//Log("GetPidMapping() count:%d",maxCount);
		for (int i=0; i < maxCount;++i)
		{
			//Log("GetPidMapping() i:%d pid:%x",i,pm[i].ulPID);
			pids[i]=(int)pm[i].ulPID;
			elementary_stream[i]=(int)pm[i].MediaSampleContent;
		}
		//Log("GetPidMapping() d");
		*count=maxCount;
		pPidEnum->Release();
		pMap->Release();
		//Log("GetPidMapping() done");
	}
	catch(...)
	{
		Log("exception GetPidMapping");
	}
	delete[] pm;
	//Log("GetPidMapping() donreturn");
	return S_OK;
}

HRESULT SetupDemuxerPin(IPin *pVideo,int videoPID, int elementary_stream, bool unmapOtherPids)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP		*pm = new PID_MAP[30];
	ULONG			count;
	ULONG			umPid;
	int				maxCounter;
	HRESULT hr=0;
	try
	{
		Log("map pid:%x", videoPID);
		// video
		if (pVideo!=NULL)
		{
			hr=pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
			if(FAILED(hr) || pMap==NULL)
			{
				Log("unable to get IMPEG2PIDMap :0x%x",hr);
				delete[] pm;
				return 1;
			}
		}
		if (videoPID>=0 && videoPID <0x1fff)
		{
			// map new pid
			Log("  map pid:%x", videoPID);
			pid = (ULONG)videoPID;
			hr=pMap->MapPID(1,&pid,(MEDIA_SAMPLE_CONTENT)elementary_stream);
			if(FAILED(hr))
			{
				Log("unable to map pid:%x %x", pid,hr);
				delete[] pm;
				return 2;
			}
		}
		if (unmapOtherPids)
		{
			hr=pMap->EnumPIDMap(&pPidEnum);
			if(FAILED(hr) || pPidEnum==NULL)
			{
				Log("unable to get IEnumPIDMap :0x%x",hr);
				delete[] pm;
				return 5;
			}
			// enum and unmap the pids
			pPidEnum->Next(30,&pm[0],&count);
			for (int i=0; i < count;++i)
			{
				umPid=pm[i].ulPID;
				if (umPid != videoPID)
				{
					Log("  unmap pid:%x", umPid);
					hr=pMap->UnmapPID(1,&umPid);
					if (FAILED(hr))
					{
						Log("unable to unmap pid:%x %x", umPid,hr);
					}
				}
			}
			pPidEnum->Release();
		}
		pMap->Release();
	}
	catch(...)
	{
		Log("exception SetupDemuxerPin pid%x", videoPID);
	}
	delete[] pm;
	return S_OK;
}

HRESULT SetupDemuxerPids(IPin *pVideo,int *videoPID, int pidCount,int elementary_stream, bool unmapOtherPids)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP*  pm = new PID_MAP[30];
	ULONG			count;
	ULONG			umPid;
	int				maxCounter;
	HRESULT hr=0;
	try
	{

		Log("setup pids: count %d", pidCount);
		// video
		if (pVideo!=NULL)
		{
			hr=pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
			if(FAILED(hr) || pMap==NULL)
			{
				Log("unable get IMPEG2PIDMap  %x",hr);
				delete[] pm;
				return 1;
			}
			for (int i=0; i <pidCount;++i)
			{
				if (videoPID[i]>=0 && videoPID[i] < 0x1fff)
				{
					// map new pid
					Log("  map pid:%x", videoPID[i]);
					pid = (ULONG)videoPID[i];
					hr=pMap->MapPID(1,&pid,(MEDIA_SAMPLE_CONTENT)elementary_stream);
					if(FAILED(hr))
					{
						Log("unable map pid:%x %x", pid,hr);
						delete[] pm;
						return 2;
					}
				}
			}

			if (unmapOtherPids)
			{
				hr=pMap->EnumPIDMap(&pPidEnum);
				if(FAILED(hr) || pPidEnum==NULL)
				{
					Log("unable get IEnumPIDMap  %x",hr);
					delete[] pm;
					return 5;
				}
				// enum and unmap the pids
				
				pPidEnum->Next(30,&pm[0],&count);
				for (int i=0; i < count;++i)
				{
					BOOL mapped=false;
					umPid=pm[i].ulPID;
					for (int x=0; x < pidCount;++x)
					{
						if (umPid == videoPID[x]) 
						{
							mapped=TRUE;
						}
					}

					if (!mapped)
					{
						Log("  unmap pid:%x", umPid);
						hr=pMap->UnmapPID(1,&umPid);
						if (FAILED(hr))
						{
							Log("unable unmap pid:%x %x", umPid,hr);
						}
					}
				}
				pPidEnum->Release();
			}

			pMap->Release();
		}
	}
	catch(...)
	{
		Log("exception SetupDemuxerPids:%d", pidCount);
	}
	delete[] pm;
	return S_OK;
}
HRESULT DumpMpeg2DemuxerMappings(IBaseFilter* mpeg2Demuxer)
{
	Log("Mpeg2DemuxerMappings");
  IEnumPins* enumPins;
  if ( FAILED(mpeg2Demuxer->EnumPins(&enumPins)))
  {
    return S_OK;
  }
  enumPins->Reset();
  IPin* pins[2];
  ULONG fetched;
  while (enumPins->Next(1,pins,&fetched)==S_OK)
  {
    if (fetched<1) break;
    if (pins[0]==NULL) break;
    PIN_DIRECTION direction;
    pins[0]->QueryDirection(&direction);
    if (direction==PINDIR_INPUT) continue;
    PIN_INFO pinInfo;
    pins[0]->QueryPinInfo(&pinInfo);

    
	  IMPEG2PIDMap	*pMap=NULL;
	  IEnumPIDMap		*pPidEnum=NULL;
    pins[0]->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
    if (pMap!=NULL)
    {
      Log("  pin:%s", (char*)  _bstr_t(pinInfo.achName));
      if (SUCCEEDED( pMap->EnumPIDMap(&pPidEnum) ))
      {
	      PID_MAP			pm;
        ULONG count;
        while(pPidEnum->Next(1,&pm,&count)== S_OK)
		    {
          if (count<1) break;
          Log("    pid:0x%x type:%d", pm.ulPID,pm.MediaSampleContent);
        }
		    pPidEnum->Release();
      }
		  pMap->Release();
    }
  }
  enumPins->Release();
  return S_OK;
}
//

IPin *GetPin(IBaseFilter *pFilter, PIN_DIRECTION PinDir)
{
    BOOL       bFound = FALSE;
    IEnumPins  *pEnum;
    IPin       *pPin;

    HRESULT hr = pFilter->EnumPins(&pEnum);
    if (FAILED(hr))
    {
        return NULL;
    }
    while(pEnum->Next(1, &pPin, 0) == S_OK)
    {
        PIN_DIRECTION PinDirThis;
        pPin->QueryDirection(&PinDirThis);
        if (bFound = (PinDir == PinDirThis))
            break;
        pPin->Release();
    }
    pEnum->Release();
    return (bFound ? pPin : NULL);  
}


HRESULT SetWmvProfile(IBaseFilter* baseFilter, ULONG bitrate, ULONG fps, ULONG screenX, ULONG screenY)
{
/*
	HRESULT hr;
	Log("WMV:SetProfile (%d, %d %x) ",screenX,screenY,baseFilter);

	CComPtr<IBaseFilter>	m_pBaseFilter;
	m_pBaseFilter.Attach( baseFilter);
	if (m_pBaseFilter==NULL)
	{
		Log("WMV:could not get IBaseFilter");
		return S_OK;
	}

	CComQIPtr<IFileSinkFilter>	testje=m_pBaseFilter;
	if (testje==NULL)
	{
		Log("WMV:could not get IFileSinkFilter");
		return S_OK;
	}
	else Log("WMV:got IFileSinkFilter");

	CComQIPtr<IConfigAsfWriter>	m_pAsfWriter=m_pBaseFilter;
	if (m_pAsfWriter==NULL)
	{
		Log("WMV:could not get IConfigAsfWriter");
		m_pAsfWriter=testje;
		if (m_pAsfWriter==NULL)
		{
			Log("WMV:2could not get IConfigAsfWriter");
		}
		else
			Log("WMV:got IConfigAsfWriter");

		return S_OK;
	}
	//Log("WMV:Get IWMProfile");
	IWMProfile* profile;
	m_pAsfWriter->GetCurrentProfile(&profile);
	if(!profile)
	{
		Log("WMV:could not Get IWMProfile");
		return S_OK;
	}
	DWORD NoOfStreams;
	hr=profile->GetStreamCount(&NoOfStreams);
	DWORD dwLen=256;
	WCHAR pwszName[256];
	profile->GetName(pwszName,&dwLen);
	Log("WMV:profile:%s",(char*)(_bstr_t(pwszName)));

	if (bitrate>0 && fps>0 && screenX>0 && screenY>0)
	{
		Log("WMV:streams:%d",NoOfStreams);
		int overhead=9000;
		int audioBitRate=44100;
		int videoBitRate=bitrate*1000;
		videoBitRate-=audioBitRate;
		videoBitRate-=overhead;

		for (int i=0; i < NoOfStreams;++i)
		{
			Log("WMV:getStream:%d",i);
			IWMStreamConfig* streamConfig;
			hr=profile->GetStream(i, &streamConfig);
			if (hr!=0) Log("WMV:  getStream returned:0x%x",hr);

			GUID streamType;
			hr=streamConfig->GetStreamType(&streamType);
			if (hr!=0) Log("WMV:  GetStreamType returned:0x%x",hr);

			CComQIPtr<IWMMediaProps>  mediaProps = streamConfig;
			WM_MEDIA_TYPE* mediaType;
			DWORD          mediaTypeLen;
			hr=mediaProps->GetMediaType(NULL,&mediaTypeLen);
			if (hr!=0) Log("WMV:  GetMediaType len:%d",mediaTypeLen);

			char* buffer = new char[mediaTypeLen];
			hr=mediaProps->GetMediaType((WM_MEDIA_TYPE*)buffer,&mediaTypeLen);
			mediaType=(WM_MEDIA_TYPE*)buffer;
			//Log("WMV:  GetMediaType ok");

			DWORD currentBitrate;
			hr=streamConfig->GetBitrate(&currentBitrate);
			if (streamType==WMMEDIATYPE_Video)
			{
				Log("WMV:  stream %d is video bitrate:%d kbps->%d", i,currentBitrate, videoBitRate);
				//hr=streamConfig->SetBitrate(videoBitRate);
				if (hr!=0) Log("WMV:  SetBitrate returned 0x%x",hr);
				if (mediaType->formattype==WMFORMAT_VideoInfo)
				{
					WMVIDEOINFOHEADER *pHeader = (WMVIDEOINFOHEADER *)mediaType->pbFormat;
					Log("WMV:  GetMediaType (%d,%d) fps:%d ", pHeader->bmiHeader.biWidth,pHeader->bmiHeader.biHeight,pHeader->AvgTimePerFrame/10000000LL);
					pHeader->rcSource.left=0;
					pHeader->rcSource.top=0;
					pHeader->rcSource.right=screenX;
					pHeader->rcSource.bottom=screenY;
					
					pHeader->rcTarget.left=0;
					pHeader->rcTarget.top=0;
					pHeader->rcTarget.right=screenX;
					pHeader->rcTarget.bottom=screenY;
					pHeader->AvgTimePerFrame=fps*10000000LL;
					pHeader->bmiHeader.biWidth=screenX;
					pHeader->bmiHeader.biHeight=screenY;
					hr=mediaProps->SetMediaType(mediaType);
					Log("WMV:  SetMediaType (%d,%d) fps:%d returned 0x%x", screenX,screenY,fps,hr);

				}
			}
			else if (streamType==WMMEDIATYPE_Audio)
			{
				Log("WMV:  stream %d is audio bitrate:%d kbps->%d", i,currentBitrate, audioBitRate);
				//hr=streamConfig->SetBitrate(audioBitRate);
				if (hr!=0) Log("WMV:  SetBitrate returned 0x%x",hr);
				
			}
			hr=profile->ReconfigStream(streamConfig);
			Log("WMV:  ReconfigStream returned 0x%x",hr);

		}
		hr=m_pAsfWriter->ConfigureFilterUsingProfile(profile);
		if (!SUCCEEDED(hr))
			Log("WMV:ConfigureFilterUsingProfile returned:0x%x",hr);
	}
	Log("WMV:Set Deinterlace");
	CComPtr<IServiceProvider> pProvider;
	CComPtr<IWMWriterAdvanced2> pWMWA2;
	hr = m_pAsfWriter->QueryInterface( __uuidof(IServiceProvider),(void**)&pProvider);
	if (SUCCEEDED(hr))
	{
		hr = pProvider->QueryService(IID_IWMWriterAdvanced2,IID_IWMWriterAdvanced2,(void**)&pWMWA2);
		if (SUCCEEDED(hr))
		{
			WMT_ATTR_DATATYPE wmType;
			DWORD pValue = WM_DM_DEINTERLACE_NORMAL;
			WORD len=sizeof(DWORD);
			// Set the first parameter to your actual input number.
			hr = pWMWA2->GetInputSetting(1, g_wszDeinterlaceMode,&wmType, (BYTE*) &pValue, &len );
			if (!SUCCEEDED(hr))
				Log("WMV:Could not get deinterlace mode pin1 0x%x",hr);
			pValue = WM_DM_DEINTERLACE_NORMAL;
			hr = pWMWA2->SetInputSetting(1, g_wszDeinterlaceMode,wmType, (BYTE*) &pValue, len );
			if (!SUCCEEDED(hr))
				Log("WMV:Could not set deinterlace mode pin1 0x%x (%d, %d, %d)",hr,wmType,pValue,len);
			else
				Log("WMV:Set deinterlace mode (%d, %d, %d)",wmType,pValue,len);
		}
		else Log("WMV:Could not get IWMWriterAdvanced2");
	}
	else Log("WMV:Could not get IServiceProvider");
	*/
	return S_OK;
}
