// dvblib.cpp : Definiert die Initialisierungsroutinen für die DLL.
//
#include <stdio.h>
#include "stdafx.h"
#include <initguid.h>
#include <streams.h>
//#include <Dshow.h>
#include <bdaiface.h>
#include "Include\b2c2mpeg2adapter.h"
#include "mpeg2data.h"
#include "dvblib.h"
#include "b2c2_defs.h"
#include "Include\B2C2_Guids.h"
#include "b2c2mpeg2adapter.h"
#include "ib2c2mpeg2tunerctrl.h"
#include "ib2c2mpeg2datactrl.h"
#include "ib2c2mpeg2avctrl.h"
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define MAX_IP_PIDS			34
#define MAX_TS_PIDS			39

#define PID_MAX 			0x1fff

// copied from "../sky2pcavsrc/constants.h"; better move b2c2_defs.h
#define SKY2PC_E_OUTOFLOCK						0x90010115

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


HRESULT SetupDemuxer(IPin *pVideo,IPin *pAudio,int audioPID,int videoPID)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	HRESULT hr=0;

	// video
	hr=pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
    if(FAILED(hr))
		return 1;
	// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr))
		return 5;
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
			return 6;
	}
	pPidEnum->Release();
	// map new pid
	pid = (ULONG)videoPID;
	hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
	if(FAILED(hr))
		return 2;
	pMap->Release();
	// audio 
	hr=pAudio->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
	if(FAILED(hr))
		return 3;
	// 
	hr=pMap->EnumPIDMap(&pPidEnum);
	if(FAILED(hr))
		return 7;
	// enum and unmap the pids
	while(pPidEnum->Next(1,&pm,&count)== S_OK)
	{
		umPid=pm.ulPID;
		hr=pMap->UnmapPID(1,&umPid);
		if(FAILED(hr))
			return 8;
	}
	pPidEnum->Release();
	pid = (ULONG)audioPID;
	hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
	if(FAILED(hr))
		return 4;

	pMap->Release();
	
	
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


//
//
//

