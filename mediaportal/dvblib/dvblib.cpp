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
B2C2MPEG2Adapter *m_b2c2Adapter=new B2C2MPEG2Adapter("");
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
BOOL AddTSPid(int pidnum);
HRESULT LoadGraphFile(IGraphBuilder *pGraph, const WCHAR* wszName);
IPin *GetPin(IBaseFilter *pFilter, PIN_DIRECTION PinDir);
HRESULT SaveGraphFile(IGraphBuilder *pGraph, WCHAR *wszPath);
BOOL TuneCard(IB2C2MPEG2TunerCtrl2	*pB2C2FilterTunerCtrl,IB2C2MPEG2AVCtrl2	*pB2C2FilterAvCtrl,IB2C2MPEG2DataCtrl2	*pB2C2FilterDataCtrl,long Frequency,long SymbolRate,long FEC,long POL,long LNB,long Diseq,long AudioPID,long VideoPID,long LNBNumber);
// callbacks
UINT __stdcall VideoCB(MPEG2_VIDEO_INFO *pInfo);

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

void ResetDevice(void)
{
	if(m_b2c2Adapter && m_b2c2Adapter->IsInitialized())
		m_b2c2Adapter->Release();
}

BOOL DVBInit(void)
{
	if(m_b2c2Adapter==NULL)
		m_b2c2Adapter=new B2C2MPEG2Adapter("");
	if( m_b2c2Adapter)
	{
		HRESULT hr;
		if(m_b2c2Adapter->IsInitialized()==(BOOL)true)
			return true;
		hr=m_b2c2Adapter->Initialize();
		if(FAILED(hr))
			return false;
		else
		return true;
	}
	return false;	
}
//

// setup a callback
void SetVideoCB(LONGLONG *pointer)
{
	IB2C2MPEG2AVCtrl2		*pB2C2FilterAvCtrl= NULL;

	if(DVBInit()==false)
	{
		return;
	}
	pB2C2FilterAvCtrl=m_b2c2Adapter->GetAvControl();
	if(pB2C2FilterAvCtrl)
		pB2C2FilterAvCtrl->SetCallbackForVideoMode(&VideoCB);
}
UINT __stdcall VideoCB(MPEG2_VIDEO_INFO *pInfo)
{
	return 0;
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
bool GetSectionDataI(PID pid,TID tid,WORD *sectionCount,int tableSection,int timeout)
{
	
	IMpeg2Data		*pMPEG=m_mpegSIInterface;
	HRESULT			hr;


	if(AddTSPid(pid)==false)
		return false;
	if(timeout<1 || timeout>8000) // max. timeout 5 sec
		timeout=1;
	// grab table (tableSection=0) or section (tableSection!=0)
	if(tableSection==0)
		hr = pMPEG->GetTable(pid, tid, NULL, (DWORD)timeout, &m_sectionList);
	else
		hr = pMPEG->GetSection(pid, tid, NULL, (DWORD)timeout, &m_sectionList);
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
BOOL SetCallBack(LONGLONG *pointer)
{
	m_callBack=(DeliverSectionData)pointer;
	return true;
}


// 
BOOL TuneCard(IB2C2MPEG2TunerCtrl2 *pB2C2FilterTunerCtrl,IB2C2MPEG2AVCtrl2 *pB2C2FilterAvCtrl,IB2C2MPEG2DataCtrl2 *pB2C2FilterDataCtrl,long Frequency,long SymbolRate,long FEC,long POL,long LNBKhz,long Diseq,long AudioPID,long VideoPID,long LNBFreq)
{
	
	HRESULT hr;

	if(!pB2C2FilterTunerCtrl)
		return false;
	
	// Defaults.

	long lSysIpPIDSize = 34;
	long lSysTsPIDSize = 39;
	long lSleepLength = 10;
	//
        
	hr = pB2C2FilterTunerCtrl->SetFrequencyKHz(Frequency);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
        

	hr = pB2C2FilterTunerCtrl->SetSymbolRate(SymbolRate);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
        
	hr = pB2C2FilterTunerCtrl->SetLnbFrequency(LNBFreq);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
        
	hr = pB2C2FilterTunerCtrl->SetFec(6L);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
        
	hr = pB2C2FilterTunerCtrl->SetPolarity(POL);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
        
	hr = pB2C2FilterTunerCtrl->SetLnbKHz(LNBKhz);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
        
	hr = pB2C2FilterTunerCtrl->SetDiseqc(Diseq);
	if (FAILED (hr))
	{
		return false;	// *** FUNCTION EXIT POINT
	}
			
	hr = pB2C2FilterTunerCtrl->SetTunerStatusEx(500L);
	if (FAILED(hr))	
	    return false;	// *** FUNCTION EXIT POINT
	//else
	//	CallTunedOk(); // callback that the tuner is locked

	if(AudioPID!=-1 && VideoPID!=-1)
	{
		hr = pB2C2FilterAvCtrl->SetAudioVideoPIDs (AudioPID, VideoPID);
		if (FAILED (hr))
		{
			return false;	// *** FUNCTION EXIT POINT
		}
	}
	return true;
} // main tuning...

BOOL Tune(long Frequency,long SymbolRate,long FEC,long POL,long LNB,long Diseq,long LNBNumber)
{
	IB2C2MPEG2DataCtrl3 	*pB2C2FilterDataCtrl = NULL;
	IB2C2MPEG2TunerCtrl2	*pB2C2FilterTunerCtrl= NULL;
	IB2C2MPEG2AVCtrl2		*pB2C2FilterAvCtrl= NULL;
	BOOL					flag=false;
	// init adapter

	if(DVBInit()==false)
	{
		return false;
	}
	pB2C2FilterDataCtrl = m_b2c2Adapter->GetDataControl();
	pB2C2FilterTunerCtrl= m_b2c2Adapter->GetTunerControl();
	pB2C2FilterAvCtrl= m_b2c2Adapter->GetAvControl();

	// Diseq = 0-6
	// POL = 0- h / 1 -v
	if(pB2C2FilterDataCtrl!=NULL && pB2C2FilterTunerCtrl!=NULL && pB2C2FilterAvCtrl!=NULL)
		flag=TuneCard(pB2C2FilterTunerCtrl,pB2C2FilterAvCtrl,pB2C2FilterDataCtrl,Frequency,SymbolRate,FEC,POL,LNB,Diseq,-1L,-1L,LNBNumber);

	return true;
}
BOOL StopSITable(void)
{
	IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl = m_b2c2Adapter->GetDataControl();
	IMediaControl		*mediaControl=NULL;
    HRESULT hr;

	if(m_b2c2Adapter->IsInitialized()==false)
		return false;

	hr=m_b2c2Adapter->GetMediaControl(&mediaControl);
	mediaControl->Stop();
	
	return true;
}
BOOL AddTSPid(int pidnum)
{
	IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl = m_b2c2Adapter->GetDataControl();
    HRESULT				hr;
	long				pidCount=1;
	long				pids[1];

	pids[0]=pidnum;

	if(m_b2c2Adapter->IsInitialized()==false)
		return false;
	if(pB2C2FilterDataCtrl)
	{
		hr=pB2C2FilterDataCtrl->AddPIDsToPin(&pidCount,pids,1);// we use always pin 1 for mpeg2data
		if(FAILED(hr))
			return false;
	}
	
	return true;
}

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
BOOL DeleteAllPIDsI(void)
{
	IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl = m_b2c2Adapter->GetDataControl();
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
				hr=pB2C2FilterDataCtrl->DeletePIDsFromPin(pidCount,pids,1);
			} else
				return false;

		}while(pidCount>0);
	}
	
	return true;
}
//
// get tab without tune request
//

BOOL IsTunerLocked(void)
{
	IB2C2MPEG2TunerCtrl2	*pB2C2FilterTunerCtrl= NULL;
	HRESULT					hr;
	// init adapter

	if(DVBInit()==false)
	{
		return false;
	}
	pB2C2FilterTunerCtrl= m_b2c2Adapter->GetTunerControl();
	if(pB2C2FilterTunerCtrl)
	{
		hr=pB2C2FilterTunerCtrl->CheckLock();
		if(SUCCEEDED(hr))
			return true;
	}
	return false;	
}
BOOL PauseSITable(void)
{
	IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl = m_b2c2Adapter->GetDataControl();
	IMediaControl		*mediaControl=NULL;
    HRESULT hr;

	if(m_b2c2Adapter->IsInitialized()==false)
		return false;

	hr=m_b2c2Adapter->GetMediaControl(&mediaControl);
	hr=mediaControl->Pause();
	if(SUCCEEDED(hr))
		return true;
	//ResetDevice();
	
	return false;
}
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

BOOL RunSITable(void)
{
	IB2C2MPEG2DataCtrl3 *pB2C2FilterDataCtrl = m_b2c2Adapter->GetDataControl();
	IMediaControl		*mediaControl=NULL;
    HRESULT hr;

	if(m_b2c2Adapter->IsInitialized()==false)
		return false;

	hr=m_b2c2Adapter->GetMediaControl(&mediaControl);
	if(FAILED(hr))
		return false;
	hr=mediaControl->Run();
	if(SUCCEEDED(hr))
		return true;
	//ResetDevice();
	
	return false;
}
BOOL ClearAllUp(void)
{
	IMediaControl		*mediaControl=NULL;
    HRESULT hr;

	if(m_b2c2Adapter->IsInitialized()==false)
		return false;

	hr=m_b2c2Adapter->GetMediaControl(&mediaControl);
	if(FAILED(hr))
		return false;

	hr=mediaControl->Stop();
	if(FAILED(hr))
		return false;

	// should be released and NULL but: who knows ? :)
	if(m_sectionList!=NULL)
	{
		m_sectionList->Release();
		m_sectionList=NULL;
	}


	if(m_demux!=NULL)
	{
		m_demux->Release();
		m_demux=NULL;
	}
	if(m_mpeg2Data!=NULL)
	{
		m_mpeg2Data->Release();
		m_mpeg2Data=NULL;
	}

	ResetDevice();
	if(m_mpegSIInterface!=NULL)
		m_mpegSIInterface->Release();

	delete (void*)m_callBack;

	return true;
}
//


//
void ClearAllTsPids(void)
{
	m_pidArrayCount=0;
	for(int i=0;i<39;i++)
		m_pidArray[i]=0;
}
BOOL AddPidToTS(long pid)
{
	m_pidArray[m_pidArrayCount]=pid;
	m_pidArrayCount++;
	return m_pidArrayCount;
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
HRESULT LoadGraphFile(IGraphBuilder *pGraph, const WCHAR* wszName)
{
	MessageBox(NULL,(LPCTSTR)wszName,NULL,0);
    IStorage *pStorage = 0;
    if (S_OK != StgIsStorageFile(wszName))
    {
        return E_FAIL;
    }
    HRESULT hr = StgOpenStorage(wszName, 0, 
        STGM_TRANSACTED | STGM_READ | STGM_SHARE_DENY_WRITE, 
        0, 0, &pStorage);
    if (FAILED(hr))
    {
        return hr;
    }
    IPersistStream *pPersistStream = 0;
    hr = pGraph->QueryInterface(IID_IPersistStream,
             reinterpret_cast<void**>(&pPersistStream));
    if (SUCCEEDED(hr))
    {
        IStream *pStream = 0;
        hr = pStorage->OpenStream(L"ActiveMovieGraph", 0, 
            STGM_READ | STGM_SHARE_EXCLUSIVE, 0, &pStream);
        if(SUCCEEDED(hr))
        {
            hr = pPersistStream->Load(pStream);
            pStream->Release();
        }
        pPersistStream->Release();
    }
    pStorage->Release();
    return hr;
}
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
HRESULT SaveGraphFile(IGraphBuilder *pGraph, WCHAR *wszPath) 
{
    const WCHAR wszStreamName[] = L"ActiveMovieGraph"; 
    HRESULT hr;
    
    IStorage *pStorage = NULL;
    hr = StgCreateDocfile(
        wszPath,
        STGM_CREATE | STGM_TRANSACTED | STGM_READWRITE | STGM_SHARE_EXCLUSIVE,
        0, &pStorage);
    if(FAILED(hr)) 
    {
        return hr;
    }

    IStream *pStream;
    hr = pStorage->CreateStream(
        wszStreamName,
        STGM_WRITE | STGM_CREATE | STGM_SHARE_EXCLUSIVE,
        0, 0, &pStream);
    if (FAILED(hr)) 
    {
        pStorage->Release();    
        return hr;
    }

    IPersistStream *pPersist = NULL;
    pGraph->QueryInterface(IID_IPersistStream, (void**)&pPersist);
    hr = pPersist->Save(pStream, TRUE);
    pStream->Release();
    pPersist->Release();
    if (SUCCEEDED(hr)) 
    {
        hr = pStorage->Commit(STGC_DEFAULT);
    }
    pStorage->Release();
    return hr;
}
bool SetAVPids(long vPid,long aPid)
{
	IB2C2MPEG2AVCtrl2		*pB2C2FilterAvCtrl= NULL;
	HRESULT					hr;

	if(DVBInit()==false)
	{
		return false;
	}
	// get interface
	pB2C2FilterAvCtrl= m_b2c2Adapter->GetAvControl();	
	if(pB2C2FilterAvCtrl==NULL)
		return false; // serious error
	// set pids
	hr=pB2C2FilterAvCtrl->SetAudioVideoPIDs(aPid,vPid);
	if(FAILED(hr))
		return false;

	return true;
}

//
//
//
bool SetupB2C2Graph(void)
{
	IMpeg2Data			*pMPEG = NULL; // Pointer to the Sections and Tables filter.
	IGraphBuilder		*pGraph=NULL;
	IMpeg2Demultiplexer *pDemux=NULL;
	IBaseFilter			*pFilterDemux=NULL;
	IBaseFilter			*pFilterMPEG=NULL;
	HRESULT				hr;

	// do some init here (graph,basefilters,pins...)
	if(DVBInit()==false)
	{
		return false;
	}

	pGraph=m_b2c2Adapter->GetFilterGraph();
	if(pGraph==NULL)
		return false;

	hr=CoCreateInstance(CLSID_MPEG2Demultiplexer,NULL,CLSCTX_INPROC_SERVER,IID_IBaseFilter,(LPVOID*)&pFilterDemux);
	if(FAILED(hr))
		return false;

	m_demux=pFilterDemux;
	
	hr=CoCreateInstance(CLSID_MPSECTAB,NULL,CLSCTX_INPROC_SERVER,IID_IBaseFilter,(LPVOID*)&pFilterMPEG);
	if(FAILED(hr))
		return false;

	m_mpeg2Data=pFilterMPEG;
	
	hr=pGraph->AddFilter(pFilterDemux,L"filter_demux");
	if(FAILED(hr))
		return false;

	hr=pGraph->AddFilter(pFilterMPEG,L"filter_sections");
	if(FAILED(hr))
		return false;

	hr=pFilterDemux->QueryInterface(IID_IMpeg2Demultiplexer,(void**)&pDemux);
	if(FAILED(hr))
		return false;
	hr=pFilterMPEG->QueryInterface(IID_IMpeg2Data,(void**)&pMPEG);
	if(FAILED(hr))
		return false;
	// save for global access the pointer
	m_mpegSIInterface=pMPEG;

	IPin *pPin;
	AM_MEDIA_TYPE mt;
	ZeroMemory(&mt, sizeof(AM_MEDIA_TYPE));
	mt.majortype = MEDIATYPE_MPEG2_SECTIONS;
	mt.subtype = MEDIASUBTYPE_MPEG2DATA;
	// setup a pin for ts
	hr=pDemux->CreateOutputPin(&mt,L"TS-Out",&pPin);
	if(FAILED(hr))
		return false;
	// define pins
	IPin *pinMPEG2DATAIn=GetPin(pFilterMPEG,PINDIR_INPUT);
	IPin *pinB2C2Out=NULL;
	IPin *pinDemuxIn=GetPin(pFilterDemux,PINDIR_INPUT);
	hr=m_b2c2Adapter->GetTsOutPin(1,&pinB2C2Out); // always we use pin 1 of filter
	if(FAILED(hr))
		return false;
//	hr=pGraph->Connect(pinB2C2Out,pinDemuxIn);
	hr=pGraph->Render(pinB2C2Out);
	if(FAILED(hr))
		return false;
//	hr=pGraph->Connect(pPin,pinMPEG2DATAIn);
	hr=pGraph->Render(pPin);
	if(FAILED(hr))
		return false;
	
	
	pinMPEG2DATAIn->Release();
	pinDemuxIn->Release();
	pPin->Release();



	return true;
}

