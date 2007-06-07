/**
*  TSParserSource.cpp
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSParserSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSParserSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSParserSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSParserSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/
#include "stdafx.h"

#include "bdaiface.h"
#include "ks.h"
#include "ksmedia.h"
#include "bdamedia.h"
#include "mediaformats.h"

#include "TSParserSource.h"
#include "TSParserSourceGuids.h"
#include "TunerEvent.h"
#include "global.h"


CUnknown * WINAPI CTSParserSourceFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);
	CTSParserSourceFilter *pNewObject = new CTSParserSourceFilter(punk, phr);

	if (pNewObject == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
	}

	return pNewObject;
}

// Constructor
CTSParserSourceFilter::CTSParserSourceFilter(IUnknown *pUnk, HRESULT *phr) :
	CSource(NAME("CTSParserSourceFilter"), pUnk, CLSID_TSParserSource),
	m_bRotEnable(FALSE),
	m_bSharedMode(FALSE),
	m_pInpPin(NULL),
	m_pPin(NULL),
	m_pDVBTChannels(NULL),
	m_WriteThreadActive(FALSE),
	m_FilterRefList(NAME("MyFilterRefList")),
	m_pSettingsStore(NULL),
	m_pRegStore(NULL),
	m_pTunerEvent(NULL),
	m_pStreamParser(NULL),
	m_pDemux(NULL),
	m_pPidParser(NULL),
	m_pFileDuration(NULL),
	m_pFileReader(NULL),
	m_pSharedMemory(NULL),
	m_pClock(NULL)
{
	ASSERT(phr);

	m_pClock = new CTSFileSourceClock( NAME(""), GetOwner(), phr );
	if (m_pClock == NULL)
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pSharedMemory = new SharedMemory(64000000);
	m_pFileReader = new MemReader(m_pSharedMemory);
	m_pFileDuration = new MemReader(m_pSharedMemory);//Get Live File Duration Thread
	m_pPidParser = new PidParser(m_pFileReader);
	m_pDemux = new Demux(m_pPidParser, this, &m_FilterRefList);
	m_pStreamParser = new StreamParser(m_pPidParser, m_pDemux, &netArray);

	m_pMpeg2DataParser = NULL;
//	m_pMpeg2DataParser = new DVBMpeg2DataParser();
	m_pDVBTChannels = NULL;
	if (m_pMpeg2DataParser)
	{
		m_pDVBTChannels = new DVBTChannels();
		m_pMpeg2DataParser->SetDVBTChannels(m_pDVBTChannels);
	}

	m_pInpPin = new CTSParserInputPin(this, GetOwner(), &m_Lock, phr);
	if (m_pInpPin == NULL)
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pPin = new CTSParserSourcePin(GetOwner(), this, phr);
	if (m_pPin == NULL)
	{
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	m_pTunerEvent = new TunerEvent((CTSFileSourceFilter*)this);
	m_pRegStore = new CRegStore("SOFTWARE\\TSParserSource");
	m_pSettingsStore = new CSettingsStore();

	// Load Registry Settings data
	GetRegStore("default");

	CMediaType cmt;
	cmt.InitMediaType();
	cmt.SetType(&MEDIATYPE_Stream);
	cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
	cmt.SetSubtype(&MEDIASUBTYPE_NULL);
	m_pPin->SetMediaType(&cmt);

	m_bThreadRunning = FALSE;
	m_bReload = FALSE;
	m_llLastMultiFileStart = 0;
	m_llLastMultiFileLength = 0;
	m_bColdStart = FALSE;

}

CTSParserSourceFilter::~CTSParserSourceFilter()
{
	//Make sure the worker thread is stopped before we exit.
	//Also closes the files.m_hThread
	if (CAMThread::ThreadExists())
	{
		CAMThread::CallWorker(CMD_STOP);
		int count = 200;
		while (m_bThreadRunning && count--)
			Sleep(10);

		CAMThread::CallWorker(CMD_EXIT);
		CAMThread::Close();
	}

	//Clear the filter list;
	POSITION pos = m_FilterRefList.GetHeadPosition();
	while (pos){

		if (m_FilterRefList.Get(pos) != NULL)
				m_FilterRefList.Get(pos)->Release();

		m_FilterRefList.Remove(pos);
		pos = m_FilterRefList.GetHeadPosition();
	}

	if (m_pMpeg2DataParser)
	{
		m_pMpeg2DataParser->ReleaseFilter();
		delete m_pMpeg2DataParser;
		m_pMpeg2DataParser = NULL;
	}

    if (m_dwGraphRegister)
    {
        RemoveGraphFromRot(m_dwGraphRegister);
        m_dwGraphRegister = 0;
    }

	if (m_pTunerEvent)
	{
		m_pTunerEvent->UnRegisterForTunerEvents();
		m_pTunerEvent->Release();
	}

	if (m_pClock) delete  m_pClock;
	if (m_pDemux) delete	m_pDemux;
	if (m_pRegStore) delete m_pRegStore;
	if (m_pSettingsStore) delete  m_pSettingsStore;
	if (m_pPidParser) delete  m_pPidParser;
	if (m_pStreamParser) delete	m_pStreamParser;
	if (m_pPin) delete	m_pPin;
	if (m_pInpPin) delete	m_pInpPin;
	if (m_pSharedMemory) delete m_pSharedMemory;
	if (m_pFileReader) delete	m_pFileReader;
	if (m_pFileDuration) delete  m_pFileDuration;
}

void CTSParserSourceFilter::UpdateThreadProc(void)
{
	m_WriteThreadActive = TRUE;
	REFERENCE_TIME rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);

	int count = 1;

	while (!ThreadIsStopping(100))
	{
		HRESULT hr = S_OK;// if an error occurs.

		REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);

		//Reparse the file for service change	
		if ((REFERENCE_TIME)(rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime)
		{
			if(m_State != State_Stopped	&& TRUE)
			{
				//check pids every 5sec or quicker if no pids parsed
				if (((count & 1) & m_bColdStart) || !m_pPidParser->pidArray.Count())
//				if (!m_pPidParser->pidArray.Count())
				{
					UpdatePidParser(m_pFileReader);
				}

				//Change back to normal Auto operation if not already
				if (count == 6 && m_pPidParser->pidArray.Count() && m_bColdStart)
				{
					//Change back to normal Auto operation
					m_pDemux->set_Auto(m_bColdStart);
					m_bColdStart = FALSE; 
				}
			}

			count++;
			if (count > 6)
				count = 0;

			rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
		}
		
		if (!m_bColdStart && m_pPin->checkUpdateParser(m_pPidParser->m_PATVersion))
		{
			BOOL isMulticasting = FALSE;
			for (int pos = 0; pos < netArray.Count(); pos++)
			{
				if (netArray[pos].playing == TRUE)
				{
					NetInfo *netAddr = new NetInfo();
					netAddr->rotEnable = m_bRotEnable;
					netAddr->bParserSink = m_bSharedMode;
					wcscpy(netAddr->fileName, netArray[pos].fileName);
					wcscpy(netAddr->pathName, netArray[pos].pathName);
					wcscpy(netAddr->strIP, netArray[pos].strIP);
					wcscpy(netAddr->strNic, netArray[pos].strNic);
					wcscpy(netAddr->strPort, netArray[pos].strPort);
					netAddr->userIP = netArray[pos].userIP;
					netAddr->userNic = netArray[pos].userNic;
					netAddr->userPort = netArray[pos].userPort;

					//
					// Create the Network Filtergraph 
					//
					hr = CNetRender::CreateNetworkGraph(netAddr);
					if(FAILED(hr)  || (hr > 31))
					{
						delete netAddr;
		//				MessageBoxW(NULL, netAddr->fileName, L"Graph Builder Failed", NULL);
						break;
					}

					LPOLESTR wFileName = new WCHAR[1+wcslen(netAddr->fileName)];
					wcscpy(wFileName, netAddr->fileName);
					if SUCCEEDED(load(wFileName, NULL))
					{
						CNetRender::DeleteNetworkGraph(&netArray[pos]);
						netArray.RemoveAt(pos);
						//Add the new filtergraph settings to the local array
						netArray.Add(netAddr);
						isMulticasting = TRUE;
					}
					else
						delete netAddr;

					if (wFileName)
						delete[] wFileName;

					break;
				}
			};

			if (!isMulticasting)
				UpdatePidParser(m_pFileReader);

//			Sleep (1000);
		}

		Sleep(100);
	}
	m_WriteThreadActive = FALSE;
}

DWORD CTSParserSourceFilter::ThreadProc(void)
{
    HRESULT hr;  // the return code from calls
    Command com;

    do
    {
        com = GetRequest();
        if(com != CMD_INIT)
        {
			m_bThreadRunning = FALSE;
            DbgLog((LOG_ERROR, 1, TEXT("Thread expected init command")));
            Reply((DWORD) E_UNEXPECTED);
        }
		Sleep(10);

    } while(com != CMD_INIT);

    DbgLog((LOG_TRACE, 1, TEXT("Worker thread initializing")));

	LPOLESTR fileName;
	m_pFileReader->GetFileName(&fileName);

	hr = m_pFileDuration->SetFileName(fileName);
	if (FAILED(hr))
    {
		m_bThreadRunning = FALSE;
		DbgLog((LOG_ERROR, 1, TEXT("ThreadCreate failed. Aborting thread.")));

        Reply(hr);  // send failed return code from ThreadCreate
        return 1;
    }

	hr = m_pFileDuration->OpenFile();
    if(FAILED(hr))
    {
		m_bThreadRunning = FALSE;
        DbgLog((LOG_ERROR, 1, TEXT("ThreadCreate failed. Aborting thread.")));

		hr = m_pFileDuration->CloseFile();
        Reply(hr);  // send failed return code from ThreadCreate
        return 1;
    }

	hr = m_pFileDuration->CloseFile();

    // Initialisation suceeded
    Reply(NOERROR);

    Command cmd;
    do
    {
        cmd = GetRequest();

        switch(cmd)
        {
            case CMD_EXIT:
				m_bThreadRunning = FALSE;
                Reply(NOERROR);
                break;

            case CMD_RUN:
                DbgLog((LOG_ERROR, 1, TEXT("CMD_RUN received before a CMD_PAUSE???")));
                // !!! fall through

            case CMD_PAUSE:
				if (SUCCEEDED(m_pFileDuration->OpenFile()))
				{
					m_bThreadRunning = TRUE;
					Reply(NOERROR);
					DoProcessingLoop();
					m_bThreadRunning = FALSE;
				}
                break;

            case CMD_STOP:
				m_pFileDuration->CloseFile();
				m_bThreadRunning = FALSE;
                Reply(NOERROR);
                break;

            default:
                DbgLog((LOG_ERROR, 1, TEXT("Unknown command %d received!"), cmd));
                Reply((DWORD) E_NOTIMPL);
                break;
        }
		Sleep(10);

    } while(cmd != CMD_EXIT);

	m_pFileDuration->CloseFile();
	m_bThreadRunning = FALSE;
    DbgLog((LOG_TRACE, 1, TEXT("Worker thread exiting")));
    return 0;
}

//
// DoProcessingLoop
//
HRESULT CTSParserSourceFilter::DoProcessingLoop(void)
{
    Command com;

	m_pFileDuration->GetFileSize(&m_llLastMultiFileStart, &m_llLastMultiFileLength);
	REFERENCE_TIME rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);

	int count = 1;

	BoostThread Boost;
    do
    {
        while(!CheckRequest(&com))
        {
			HRESULT hr = S_OK;// if an error occurs.

			REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);

			WORD bReadOnly = FALSE;
			m_pFileDuration->get_ReadOnly(&bReadOnly);
			//Reparse the file for service change	
			if ((REFERENCE_TIME)(rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime && bReadOnly)
			{
				CNetRender::UpdateNetFlow(&netArray);
				if(m_State != State_Stopped	&& TRUE)
				{
					__int64 fileStart, filelength;
					m_pFileDuration->GetFileSize(&fileStart, &filelength);

					//Get the FileReader Type
					WORD bMultiMode;
					m_pFileDuration->get_ReaderMode(&bMultiMode);
					//Do MultiFile timeshifting mode
					if((bMultiMode & ((__int64)(fileStart + (__int64)5000000) < m_llLastMultiFileStart))
						|| (bMultiMode & (fileStart == 0) & ((__int64)(filelength + (__int64)5000000) < m_llLastMultiFileLength))
						|| (!bMultiMode & ((__int64)(filelength + (__int64)5000000) < m_llLastMultiFileLength))
						&& TRUE)
					{
						LPOLESTR pszFileName;
						if (m_pFileDuration->GetFileName(&pszFileName) == S_OK)
						{
							LPOLESTR pFileName = new WCHAR[1+lstrlenW(pszFileName)];
							if (pFileName != NULL)
							{
								wcscpy(pFileName, pszFileName);
								load(pFileName, NULL);
								if (pFileName)
									delete[] pFileName;
							}
						}
					}
					
					m_llLastMultiFileStart = fileStart;
					m_llLastMultiFileLength = filelength;
				}


				rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
			}

			if (m_State != State_Stopped && bReadOnly)
			{
				if(!m_pPidParser->m_ParsingLock	&& m_pPidParser->pidArray.Count()
					&& TRUE) //cold start
				{
					hr = m_pPin->UpdateDuration(m_pFileDuration);
					if (hr == S_OK)
					{
						if (!m_bColdStart)
							if (m_pDemux->CheckDemuxPids(m_pPidParser) == S_FALSE)
							{
								m_pDemux->AOnConnect();
							}
					}
					else
						count = 0;
				}
			}
			
			if(m_State != State_Stopped && !m_pPidParser->get_ProgPinMode())
			{
				CComPtr<IBaseFilter>pMpegSections;
				if (m_pMpeg2DataParser && SUCCEEDED(m_pDemux->GetParserFilter(pMpegSections)))
				{
					m_pMpeg2DataParser->SetFilter(pMpegSections);
					m_pMpeg2DataParser->SetDVBTChannels(m_pDVBTChannels);
					m_pMpeg2DataParser->StartScan();
				}
			}

			
			//randomly park the file pointer to help minimise HDD clogging
//			if (rtCurrentTime&1)
				m_pFileDuration->SetFilePointer(0, FILE_END);
//			else
//				m_pFileDuration->SetFilePointer(0, FILE_BEGIN);
			
			//kill the netrender graphs if were released

			if (netArray.Count() && CUnknown::m_cRef == 0)
				netArray.Clear();

			Sleep(100);
		}

        // For all commands sent to us there must be a Reply call!
        if(com == CMD_RUN || com == CMD_PAUSE)
        {
			m_bThreadRunning = TRUE;
            Reply(NOERROR);
        }
        else if(com != CMD_STOP)
        {
            Reply((DWORD) E_UNEXPECTED);
            DbgLog((LOG_ERROR, 1, TEXT("Unexpected command!!!")));
        }
		Sleep(10);

    } while(com != CMD_STOP);

	if (m_WriteThreadActive)
	{
		UpdateThread::StopThread(100);
		m_WriteThreadActive = FALSE;
	}

	if (m_pMpeg2DataParser)
		m_pMpeg2DataParser->ReleaseFilter();

	m_bThreadRunning = FALSE;

    return S_FALSE;
}

BOOL CTSParserSourceFilter::ThreadRunning(void)
{ 
	return m_bThreadRunning;
}

STDMETHODIMP CTSParserSourceFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	CheckPointer(ppv,E_POINTER);

	CAutoLock lock(&m_Lock);
	// Do we have this interface
    if (riid == IID_IFileSinkFilter)
	{
        return m_pInpPin->NonDelegatingQueryInterface(riid, ppv);
    } 
	if (riid == IID_ITSParserSource)
	{
		return GetInterface((ITSParserSource*)this, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)this, ppv);
	}
	if (riid == IID_ISpecifyPropertyPages)
	{
		return GetInterface((ISpecifyPropertyPages*)this, ppv);
	}
	if (riid == IID_IMediaPosition || riid == IID_IMediaSeeking)
	{
		return m_pPin->NonDelegatingQueryInterface(riid, ppv);
	}
    if (riid == IID_IAMFilterMiscFlags)
    {
		return GetInterface((IAMFilterMiscFlags*)this, ppv);
    }
	if (riid == IID_IAMStreamSelect && (m_pDemux->get_Auto() | m_bColdStart))
	{
		return GetInterface((IAMStreamSelect*)this, ppv);
	}
	if (riid == IID_IReferenceClock)
	{
		return GetInterface((IReferenceClock*)m_pClock, ppv);
	}
	if (riid == IID_IAsyncReader)
	{
		if ((!m_pPidParser->pids.pcr
			&& !get_AutoMode()
			&& m_pPidParser->get_ProgPinMode())
			&& m_pPidParser->get_AsyncMode())
		{
			return GetInterface((IAsyncReader*)this, ppv);
		}
	}
	if (riid == IID_IBDA_DeviceControl)
    {
		return GetInterface((IBDA_DeviceControl*)this, ppv);
    }
	if (riid == IID_IBDA_Topology)
    {
		return GetInterface((IBDA_Topology*)this, ppv);
    }
	return CSource::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

//The following code snippet shows examples of arrays of template connections and joints:
	//
	// BDA Template Topology Connections //
	// Lists the possible connections between pin types and
	// node types. This structure along with the BDA_FILTER_TEMPLATE, 
	// KSFILTER_DESCRIPTOR, and BDA_PIN_PAIRING structures 
	// describe how topologies are created in the filter.
	//
	const KSTOPOLOGY_CONNECTION TemplateTunerConnections[] =
	{
		{
			-1, 0, 0, 0
		}, 
		// from upstream filter to 0 pin of 0 node  ? ?
		{
			0, 1, 1, 0
		}, 
		// from 1 pin of 0 node to 0 pin of 1 node  ? ?
		{
			1, 1, -1, 1
		}, 
		// from 1 pin of 1 node to downstream filter 
	};
	//
	// lists the template joints between antenna (input) and transport 
	// (output) pin types. Values given to joints correspond to indexes 
	// of elements in the preceding KSTOPOLOGY_CONNECTION array.//
	// For this template topology, the RF node (0) belongs to the antenna 
	// pin and the 8VSB demodulator node (1) belongs to the transport pin//
	const ULONG AntennaTransportJoints[] =
	{
		1
	// Second element in the preceding KSTOPOLOGY_CONNECTION array.
	};

/*
	const KSPIN_DESCRIPTOR KsPin_Descriptor_Table[] =
	{
		{
			1,              //InterfacesCount;
			NULL,			//Interfaces;
			0,              //MediumsCount;
			NULL,			//Mediums;
			0,              //DataRangesCount;
			NULL,		    //DataRanges;
			KSPIN_DATAFLOW_IN,          //DataFlow;
			KSPIN_COMMUNICATION_NONE,     //Communication;
			NULL,             //Category;
			NULL,             //Name;
			union
			{
				0,            //Reserved;
				struct
				{
					0,           //ConstrainedDataRangesCount;
					NULL,		 //ConstrainedDataRanges;
				},
			},
	};
*/
STDMETHODIMP CTSParserSourceFilter::StartChanges(void)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::CheckChanges(void)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::CommitChanges(void)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetChangeState(ULONG *pState)
{
	CheckPointer(pState, E_POINTER);
//BDA_CHANGE_STATE 
	*pState = BDA_CHANGES_COMPLETE; //BDA_CHANGES_PENDING

	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetNodeTypes(ULONG *pulcNodeTypes, ULONG ulcNodeTypesMax, ULONG rgulNodeTypes[  ])
{
	CheckPointer(pulcNodeTypes, E_POINTER);
	*pulcNodeTypes = 1;

	if (!ulcNodeTypesMax) 
		return S_OK;

	CheckPointer(rgulNodeTypes, E_POINTER);
	ULONG i = 0;
	memcpy(rgulNodeTypes, &i, sizeof(i));

	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetNodeDescriptors(ULONG *ulcNodeDescriptors, ULONG ulcNodeDescriptorsMax, BDANODE_DESCRIPTOR rgNodeDescriptors[  ])
{
	CheckPointer(ulcNodeDescriptors, E_POINTER);
	*ulcNodeDescriptors = 1;

	if (!ulcNodeDescriptorsMax) 
		return S_OK;

	BDANODE_DESCRIPTOR NodeDescriptor;
	NodeDescriptor.ulBdaNodeType = 1;					// The node type as it is used in 
														//the BDA template topology
	NodeDescriptor.guidFunction = KSNODE_BDA_RF_TUNER;// GUID from BdaMedia.h describing
														// the node's function (e.g.
														// KSNODE_BDA_RF_TUNER)
	NodeDescriptor.guidName = GUID_NULL;				// GUID that can be use to look up
														// a displayable name for the node.

	CheckPointer(rgNodeDescriptors, E_POINTER);
	memcpy(rgNodeDescriptors, &NodeDescriptor, sizeof(NodeDescriptor));

	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetNodeInterfaces(ULONG ulNodeType, ULONG *pulcInterfaces, ULONG ulcInterfacesMax, GUID rgguidInterfaces[  ])
{
	CheckPointer(pulcInterfaces, E_POINTER);
	*pulcInterfaces = 0;

	if (!ulcInterfacesMax) 
		return S_OK;

	CheckPointer(rgguidInterfaces, E_POINTER);
	ULONG i = 0;
	memcpy(rgguidInterfaces,  &i, sizeof(i));

	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetPinTypes(ULONG *pulcPinTypes, ULONG ulcPinTypesMax, ULONG rgulPinTypes[  ])
{
	CheckPointer(pulcPinTypes, E_POINTER);
	*pulcPinTypes = 1;

	if (!ulcPinTypesMax) 
		return S_OK;

	CheckPointer(rgulPinTypes, E_POINTER);
	ULONG i = 0;
	memcpy(rgulPinTypes, &i, sizeof(i));

	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetTemplateConnections(ULONG *pulcConnections, ULONG ulcConnectionsMax, BDA_TEMPLATE_CONNECTION rgConnections[  ])
{
	CheckPointer(pulcConnections, E_POINTER);
	*pulcConnections = 1;

	if (!ulcConnectionsMax)
		return S_OK;

	CheckPointer(rgConnections, E_POINTER);
	memcpy(rgConnections, TemplateTunerConnections, sizeof(TemplateTunerConnections));
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::CreatePin(ULONG ulPinType, ULONG *pulPinId)
{
	return E_NOTIMPL;// S_OK;
}

STDMETHODIMP CTSParserSourceFilter::DeletePin(ULONG ulPinId)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::SetMediaType(ULONG ulPinId, AM_MEDIA_TYPE *pMediaType)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::SetMedium(ULONG ulPinId, REGPINMEDIUM *pMedium)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::CreateTopology(ULONG ulInputPinId, ULONG ulOutputPinId)
{
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::GetControlNode(ULONG ulInputPinId, ULONG ulOutputPinId, ULONG ulNodeType, IUnknown **ppControlNode)
{
	return S_OK;
}


//STDMETHODIMP_(ULONG) CTSParserSourceFilter::NonDelegatingRelease()
//{
//	if (CUnknown::m_cRef == 1)
//		netArray.Clear();

//	return CBaseFilter::NonDelegatingRelease();
//}

//IAMFilterMiscFlags
ULONG STDMETHODCALLTYPE CTSParserSourceFilter::GetMiscFlags(void)
{
//	return (ULONG)AM_FILTER_MISC_FLAGS_IS_SOURCE; 
//	return (ULONG)AM_FILTER_MISC_FLAGS_IS_RENDERER; 
	return (ULONG)(AM_FILTER_MISC_FLAGS_IS_SOURCE | AM_FILTER_MISC_FLAGS_IS_RENDERER); 
}//IAMFilterMiscFlags

STDMETHODIMP  CTSParserSourceFilter::Count(DWORD *pcStreams) //IAMStreamSelect
{
	if(!pcStreams)
		return E_INVALIDARG;

	CAutoLock SelectLock(&m_SelectLock);

	*pcStreams = 0;

	if (!m_pStreamParser->StreamArray.Count() ||
		!m_pPidParser->pidArray.Count() ||
		m_pPidParser->m_ParsingLock) //cold start
		return VFW_E_NOT_CONNECTED;

	*pcStreams = m_pStreamParser->StreamArray.Count();

	return S_OK;
} //IAMStreamSelect

STDMETHODIMP  CTSParserSourceFilter::Info( 
						long lIndex,
						AM_MEDIA_TYPE **ppmt,
						DWORD *pdwFlags,
						LCID *plcid,
						DWORD *pdwGroup,
						WCHAR **ppszName,
						IUnknown **ppObject,
						IUnknown **ppUnk) //IAMStreamSelect
{
	CAutoLock SelectLock(&m_SelectLock);

	//Check if file has been parsed
	if (!m_pPidParser->pidArray.Count() || m_pPidParser->m_ParsingLock)
		return E_FAIL;

	m_pStreamParser->ParsePidArray();
	m_pStreamParser->SetStreamActive(m_pPidParser->get_ProgramNumber());

	//Check if file has been parsed
	if (!m_pStreamParser->StreamArray.Count())
		return E_FAIL;
	
	//Check if in the bounds of index
	if(lIndex >= m_pStreamParser->StreamArray.Count() || lIndex < 0)
		return S_FALSE;

	if(ppmt) {

		AM_MEDIA_TYPE*	pmt = &m_pStreamParser->StreamArray[lIndex].media;
        *ppmt = (AM_MEDIA_TYPE *)CoTaskMemAlloc(sizeof(**ppmt));
        if (*ppmt == NULL)
            return E_OUTOFMEMORY;

		memcpy(*ppmt, pmt, sizeof(*pmt));

		if (pmt->cbFormat)
		{
			(*ppmt)->pbFormat = (BYTE*)CoTaskMemAlloc(pmt->cbFormat);
			memcpy((*ppmt)->pbFormat, pmt->pbFormat, pmt->cbFormat);
		}
	};

	if(pdwGroup)
		*pdwGroup = m_pStreamParser->StreamArray[lIndex].group;

	if(pdwFlags)
		*pdwFlags = m_pStreamParser->StreamArray[lIndex].flags;

	if(plcid)
		*plcid = m_pStreamParser->StreamArray[lIndex].lcid;

	if(ppszName) {

        *ppszName = (WCHAR *)CoTaskMemAlloc(sizeof(m_pStreamParser->StreamArray[lIndex].name));
        if (*ppszName == NULL)
            return E_OUTOFMEMORY;

		ZeroMemory(*ppszName, sizeof(m_pStreamParser->StreamArray[lIndex].name));
		wcscpy(*ppszName, m_pStreamParser->StreamArray[lIndex].name);
	}

	if(ppObject)
		*ppObject = (IUnknown *)m_pStreamParser->StreamArray[lIndex].object;

	if(ppUnk)
		*ppUnk = (IUnknown *)m_pStreamParser->StreamArray[lIndex].unk;

	return NOERROR;
} //IAMStreamSelect

STDMETHODIMP  CTSParserSourceFilter::Enable(long lIndex, DWORD dwFlags) //IAMStreamSelect
{
	CAutoLock SelectLock(&m_SelectLock);

	//Test if ready
	if (!m_pStreamParser->StreamArray.Count() ||
		!m_pPidParser->pidArray.Count() ||
		m_pPidParser->m_ParsingLock)
		return VFW_E_NOT_CONNECTED;

	//Test if out of bounds
	if (lIndex >= m_pStreamParser->StreamArray.Count() || lIndex < 0)
		return E_INVALIDARG;

	int indexOffset = netArray.Count() + (int)(netArray.Count() != 0);

	if (!lIndex)
		showEPGInfo();
	else if (lIndex && lIndex < m_pStreamParser->StreamArray.Count() - indexOffset - 2){

		m_pDemux->m_StreamVid = m_pStreamParser->StreamArray[lIndex].Vid;
		m_pDemux->m_StreamH264 = m_pStreamParser->StreamArray[lIndex].H264;
		m_pDemux->m_StreamMpeg4 = m_pStreamParser->StreamArray[lIndex].Mpeg4;
		m_pDemux->m_StreamAC3 = m_pStreamParser->StreamArray[lIndex].AC3;
		m_pDemux->m_StreamMP2 = m_pStreamParser->StreamArray[lIndex].Aud;
		m_pDemux->m_StreamAAC = m_pStreamParser->StreamArray[lIndex].AAC;
		m_pDemux->m_StreamDTS = m_pStreamParser->StreamArray[lIndex].DTS;
		m_pDemux->m_StreamAud2 = m_pStreamParser->StreamArray[lIndex].Aud2;
		set_PgmNumb((WORD)m_pStreamParser->StreamArray[lIndex].group + 1);
		BoostThread Boost;
		m_pStreamParser->SetStreamActive(m_pStreamParser->StreamArray[lIndex].group);
		m_pDemux->m_StreamVid = 0;
		m_pDemux->m_StreamH264 = 0;
		m_pDemux->m_StreamAC3 = 0;
		m_pDemux->m_StreamMP2 = 0;
		m_pDemux->m_StreamAud2 = 0;
		m_pDemux->m_StreamAAC = 0;
		m_pDemux->m_StreamDTS = 0;
		set_RegProgram();
	}
	else if (lIndex == m_pStreamParser->StreamArray.Count() - indexOffset - 2) //File Menu title
	{}
	else if (lIndex == m_pStreamParser->StreamArray.Count() - indexOffset - 1) //Load file Browser
	{
		load(L"", NULL);
	}
	else if (lIndex == m_pStreamParser->StreamArray.Count() - indexOffset) //Multicasting title
	{}
	else if (lIndex > m_pStreamParser->StreamArray.Count() - indexOffset) //Select multicast streams
	{
		WCHAR wfilename[MAX_PATH];
		wcscpy(wfilename, netArray[lIndex - (m_pStreamParser->StreamArray.Count() - netArray.Count())].fileName);
		if (SUCCEEDED(load(wfilename, NULL)))
		{
//			m_pFileReader->set_DelayMode(TRUE);
//			m_pFileDuration->set_DelayMode(TRUE);
			m_pFileReader->set_DelayMode(FALSE); //Cold Start
			m_pFileDuration->set_DelayMode(FALSE); //Cold Start
			REFERENCE_TIME stop, start = (__int64)max(0,(__int64)(m_pPidParser->pids.dur - RT_2_SECOND));
			IMediaSeeking *pMediaSeeking;
			if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
			{
				pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
				pMediaSeeking->Release();
			}
		}
	}

	//Change back to normal Auto operation
	if (m_bColdStart)
	{
		//Change back to normal Auto operation
		m_pDemux->set_Auto(m_bColdStart);
		m_bColdStart = FALSE; //
	}

	return S_OK;

} //IAMStreamSelect


CBasePin * CTSParserSourceFilter::GetPin(int n)
{
	if (n == 0) {
		ASSERT(m_pInpPin);
		return m_pInpPin;
	}
	else if (n == 1) {
		ASSERT(m_pPin);
		return m_pPin;
	} else {
		return NULL;
	}
}

int CTSParserSourceFilter::GetPinCount()
{
	return 2;
}

STDMETHODIMP CTSParserSourceFilter::FindPin(LPCWSTR Id, IPin ** ppPin)
{
    CheckPointer(ppPin,E_POINTER);
    ValidateReadWritePtr(ppPin,sizeof(IPin *));

	CAutoLock lock(&m_Lock);
	if (!wcscmp(Id, m_pPin->CBasePin::Name())) {

		*ppPin = m_pPin;
		if (*ppPin!=NULL){
			(*ppPin)->AddRef();
			return NOERROR;
		}
	}
	else if (!wcscmp(Id, m_pInpPin->CBasePin::Name())) {

		*ppPin = m_pPin;
		if (*ppPin!=NULL){
			(*ppPin)->AddRef();
			return NOERROR;
		}
	}

	return CSource::FindPin(Id, ppPin);
}

void CTSParserSourceFilter::ResetStreamTime(void)
{
	CRefTime cTime;
	StreamTime(cTime);
	m_tStart = REFERENCE_TIME(m_tStart) + REFERENCE_TIME(cTime);
}

BOOL CTSParserSourceFilter::is_Active(void)
{
	return ((m_State == State_Paused) || (m_State == State_Running));
}

STDMETHODIMP CTSParserSourceFilter::Run(REFERENCE_TIME tStart)
{
	CAutoLock cObjectLock(m_pLock);

	if(!is_Active())
	{
		if (m_pInpPin->IsConnected())
			m_pInpPin->Load();

		if (m_pFileReader->IsFileInvalid())
		{
			HRESULT hr = m_pFileReader->OpenFile();
			if (FAILED(hr))
				return hr;
		}

		if (m_pFileDuration->IsFileInvalid())
		{
			HRESULT hr = m_pFileDuration->OpenFile();
			if (FAILED(hr))
				return hr;
		}

		//Set our StreamTime Reference offset to zero
		m_tStart = tStart;

		REFERENCE_TIME start, stop;
		m_pPin->GetPositions(&start, &stop);

		//Start at least 100ms into file to skip header
		if (start == 0 && m_pPidParser->pidArray.Count())
			start += m_pPidParser->get_StartTimeOffset();

//***********************************************************************************************
//Old Capture format Additions
		if (m_pPidParser->pids.pcr){ 
//***********************************************************************************************
			m_pPin->m_DemuxLock = TRUE;
			m_pPin->setPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
//m_pPin->PrintTime(TEXT("Run"), (__int64) start, 10000);
			m_pPin->m_DemuxLock = FALSE;
//			m_pPin->m_IntBaseTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntCurrentTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;
		}

		// Check if Enabled
		if (m_pDemux->get_NPControl() || m_pDemux->get_NPSlave())
			set_TunerEvent();

		if (!m_bThreadRunning && CAMThread::ThreadExists())
			CAMThread::CallWorker(CMD_RUN);
	}

	return CBaseFilter::Run(tStart);
//	return CSource::Run(tStart);
}

HRESULT CTSParserSourceFilter::Pause()
{
//::OutputDebugString(TEXT("Pause In \n"));
	CAutoLock cObjectLock(m_pLock);

	if(!is_Active())
	{
		if (m_pInpPin->IsConnected())
			m_pInpPin->Load();

		if (m_pFileReader->IsFileInvalid())
		{
			HRESULT hr = m_pFileReader->OpenFile();
			if (FAILED(hr))
				return hr;
		}

		if (m_pFileDuration->IsFileInvalid())
		{
			HRESULT hr = m_pFileDuration->OpenFile();
			if (FAILED(hr))
				return hr;
		}

		REFERENCE_TIME start, stop;
		m_pPin->GetPositions(&start, &stop);
//m_pPin->PrintTime(TEXT("Pause"), (__int64) start, 10000);

		//Start at least 100ms into file to skip header
		if (start == 0 && m_pPidParser->pidArray.Count())
			start += m_pPidParser->get_StartTimeOffset();

//***********************************************************************************************
//Old Capture format Additions
		if (m_pPidParser->pids.pcr){ 
//***********************************************************************************************
			m_pPin->m_DemuxLock = TRUE;
			m_pPin->setPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
			m_pPin->m_DemuxLock = FALSE;
//			m_pPin->m_IntBaseTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntCurrentTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;

		}
		
		//MSDemux fix
		if (start >= m_pPidParser->pids.dur)
		{
			IMediaSeeking *pMediaSeeking;
			if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
			{
				REFERENCE_TIME stop, start = m_pPidParser->get_StartTimeOffset();
				HRESULT hr = pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
				pMediaSeeking->Release();
			}
		}

		// Check if Enabled
		if (m_pDemux->get_NPControl() || m_pDemux->get_NPSlave())
			set_TunerEvent();

		if (!m_bThreadRunning && CAMThread::ThreadExists())
			CAMThread::CallWorker(CMD_PAUSE);

	}

	return CBaseFilter::Pause();
//	return CSource::Pause();
}

STDMETHODIMP CTSParserSourceFilter::Stop()
{
	CAutoLock lock(&m_Lock);
	CAutoLock cObjectLock(m_pLock);

//	if (m_bThreadRunning && CAMThread::ThreadExists())
//		CAMThread::CallWorker(CMD_STOP);

//	HRESULT hr = CSource::Stop();
	HRESULT hr = CBaseFilter::Stop();

	if (m_pTunerEvent)
		m_pTunerEvent->UnRegisterForTunerEvents();

	m_pFileReader->CloseFile();
	m_pFileDuration->CloseFile();

	return hr;
}

HRESULT CTSParserSourceFilter::FileSeek(REFERENCE_TIME seektime)
{
	if (m_pFileReader->IsFileInvalid())
	{
		return S_FALSE;
	}

	if (seektime > m_pPidParser->pids.dur)
	{
		return S_FALSE;
	}

	if (m_pPidParser->pids.dur > 10)
	{
		__int64 fileStart;
		__int64 filelength = 0;
		m_pFileReader->GetFileSize(&fileStart, &filelength);

		// shifting right by 14 rounds the seek and duration time down to the
		// nearest multiple 16.384 ms. More than accurate enough for our seeks.
		__int64 nFileIndex = 0;

		if (m_pPidParser->pids.dur>>14)
			nFileIndex = filelength * (__int64)(seektime>>14) / (__int64)(m_pPidParser->pids.dur>>14);

		nFileIndex = min(filelength, nFileIndex);
		nFileIndex = max(m_pPidParser->get_StartOffset(), nFileIndex);
		m_pFileReader->setFilePointer(nFileIndex, FILE_BEGIN);
	}
	return S_OK;
}

HRESULT CTSParserSourceFilter::OnConnect()
{
	BOOL wasThreadRunning = FALSE;
	if (m_bThreadRunning && CAMThread::ThreadExists()) {

		CAMThread::CallWorker(CMD_STOP);
		int count = 200;
		while (m_bThreadRunning && count--){Sleep(10);};
		wasThreadRunning = TRUE;
	}

	HRESULT hr = m_pDemux->AOnConnect();

	m_pStreamParser->SetStreamActive(m_pPidParser->get_ProgramNumber());

	if (wasThreadRunning)
		CAMThread::CallWorker(CMD_RUN);

	return hr;
}

STDMETHODIMP CTSParserSourceFilter::GetPages(CAUUID *pPages)
{
	if (pPages == NULL) return E_POINTER;
	pPages->cElems = 1;
	pPages->pElems = (GUID*)CoTaskMemAlloc(sizeof(GUID));
	if (pPages->pElems == NULL)
	{
		return E_OUTOFMEMORY;
	}
	pPages->pElems[0] = CLSID_TSParserSourceProp;
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt)
{
	CAutoLock SelectLock(&m_SelectLock);
	return load(pszFileName, pmt);
}

HRESULT CTSParserSourceFilter::load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt)
{
	// Is this a valid filename supplied
	CheckPointer(pszFileName,E_POINTER);

	LPOLESTR wFileName = new WCHAR[lstrlenW(pszFileName)+1];
	wcscpy(wFileName, pszFileName);

	if (_wcsicmp(wFileName, L"") == 0)
	{
		TCHAR tmpFile[MAX_PATH];
		LPTSTR ptFilename = (LPTSTR)&tmpFile;
		ptFilename[0] = '\0';

		// Setup the OPENFILENAME structure
		OPENFILENAME ofn = { sizeof(OPENFILENAME), NULL, NULL,
							 TEXT("Transport Stream Files (*.mpg, *.ts, *.tsbuffer, *.vob)\0*.mpg;*.ts;*.tsbuffer;*.vob\0All Files\0*.*\0\0"), NULL,
							 0, 1,
							 ptFilename, MAX_PATH,
							 NULL, 0,
							 NULL,
							 TEXT("Open Files (TS Parser Source Filter)"),
							 OFN_FILEMUSTEXIST|OFN_HIDEREADONLY, 0, 0,
							 NULL, 0, NULL, NULL };

		// Display the SaveFileName dialog.
		if( GetOpenFileName( &ofn ) != FALSE )
		{
			USES_CONVERSION;
			if(wFileName)
				delete[] wFileName;

			wFileName = new WCHAR[1+lstrlenW(T2W(ptFilename))];
			wcscpy(wFileName, T2W(ptFilename));
		}
		else
		{
			if(wFileName)
				delete[] wFileName;

			return NO_ERROR;
		}
	}

	HRESULT hr;

	//
	// Check & create a NetSource Filtergraph and play the file 
	//
	NetInfo *netAddr = new NetInfo();
	netAddr->rotEnable = m_bRotEnable;
	netAddr->bParserSink = m_bSharedMode;

	//
	// Check if the FileName is a Network address 
	//
	if (CNetRender::IsMulticastAddress(wFileName, netAddr))
	{
		//
		// Check in the local array if the Network address already active 
		//
		int pos = 0;
		if (!CNetRender::IsMulticastActive(netAddr, &netArray, &pos))
		{
//BoostThread Boost;
			//
			// Create the Network Filtergraph 
			//
			hr = CNetRender::CreateNetworkGraph(netAddr);
			if(FAILED(hr)  || (hr > 31))
			{
				delete netAddr;
				if(wFileName)
					delete[] wFileName;
//				MessageBoxW(NULL, netAddr->fileName, L"Graph Builder Failed", NULL);
				return hr;
			}
			//Add the new filtergraph settings to the local array
			netArray.Add(netAddr);
			if(wFileName)
				delete[] wFileName;

			wFileName = new WCHAR[1+lstrlenW(netAddr->fileName)];
			wcscpy(wFileName, netAddr->fileName);
//			m_pFileReader->set_DelayMode(TRUE);
//			m_pFileDuration->set_DelayMode(TRUE);
			m_pFileReader->set_DelayMode(FALSE);
			m_pFileDuration->set_DelayMode(FALSE);

		}
		else // If already running
		{
			if(wFileName)
				delete[] wFileName;

			wFileName = new WCHAR[1+lstrlenW(netArray[pos].fileName)];
			wcscpy(wFileName, netArray[pos].fileName);
			delete netAddr;
		}
	}
	else
		delete netAddr;

	for (int pos = 0; pos < netArray.Count(); pos++)
	{
		if (!lstrcmpiW(wFileName, netArray[pos].fileName))
			netArray[pos].playing = TRUE;
		else
			netArray[pos].playing = FALSE;
	}

	//Jump to a different Load method if already been set.
	if (m_bThreadRunning || is_Active() || m_pPin->CBasePin::IsConnected())
	{
		hr = ReLoad(wFileName, pmt);
		if(wFileName)
			delete[] wFileName;

		return hr;
	}

	BoostThread Boost;

	//Get delay Mode
	USHORT bDelay;
	m_pFileReader->get_DelayMode(&bDelay);

	//Get Pin Mode 
	BOOL pinModeSave = m_pPidParser->get_ProgPinMode();

	//Get ROT Mode 
	BOOL bRotEnable = m_bRotEnable;

	//Get Auto Mode 
	BOOL bAutoEnable = m_pDemux->get_Auto();

	//Get clock type
	int clock = m_pDemux->get_ClockMode();

	//Get Inject Mode 
	BOOL bInjectMode = m_pPin->get_InjectMode();

	//Get Rate Mode 
	BOOL bRateControl = m_pPin->get_RateControl();

	//Get NP Control Mode 
	BOOL bNPControl = m_pDemux->get_NPControl();

	//Get NP Slave Mode 
	BOOL bNPSlave = m_pDemux->get_NPSlave();

	//Get AC3 Mode 
	BOOL bAC3Mode = m_pDemux->get_AC3Mode();

	//Get Aspect Ratio Mode 
	BOOL bFixedAspectRatio = m_pDemux->get_FixedAspectRatio();

	//Get Create TS Pin Mode 
	BOOL bCreateTSPin = m_pDemux->get_CreateTSPinOnDemux();

	//Get Create Txt Pin Mode 
	BOOL bCreateTxtPin = m_pDemux->get_CreateTxtPinOnDemux();

	//Get Create Subtitle Pin Mode 
	BOOL bCreateSubPin = m_pDemux->get_CreateSubPinOnDemux();

	//Get MPEG2 Audio Media Type Mode 
	BOOL bMPEG2AudioMediaType = m_pDemux->get_MPEG2AudioMediaType();

	//Get Audio 2 Mode Mode 
	BOOL bAudio2Mode = m_pDemux->get_MPEG2Audio2Mode();

	delete m_pStreamParser;
	delete m_pDemux;
	delete m_pPidParser;
	delete m_pFileReader;
	delete m_pFileDuration;

	long length = lstrlenW(wFileName);
	if ((length < 9) || (_wcsicmp(wFileName+length-9, L".tsbuffer") != 0))
	{
		m_pFileReader = new MemReader(m_pSharedMemory);
		m_pFileDuration = new MemReader(m_pSharedMemory);//Get Live File Duration Thread
	}
	else
	{
		m_pFileReader = new MultiMemReader(m_pSharedMemory);
		m_pFileDuration = new MultiMemReader(m_pSharedMemory);
	}
	//m_pFileReader->SetDebugOutput(TRUE);
	//m_pFileDuration->SetDebugOutput(TRUE);

	m_pPidParser = new PidParser(m_pFileReader);
	m_pDemux = new Demux(m_pPidParser, this, &m_FilterRefList);
	m_pStreamParser = new StreamParser(m_pPidParser, m_pDemux, &netArray);

	// Load Registry Settings data
	GetRegStore("default");
	
	//Check for forced pin mode
	if (pmt)
	{
		//Set Auto Mode if we had been told to previously
		m_pDemux->set_Auto(bAutoEnable); 

		//Set delay if we had been told to previously
		if (bDelay)
		{
			m_pFileReader->set_DelayMode(bDelay);
			m_pFileDuration->set_DelayMode(bDelay);
		}

		//Set ROT Mode if we had been told to previously
		m_bRotEnable = bRotEnable;

		//Set clock if we had been told to previously
		if (clock)
			m_pDemux->set_ClockMode(clock);

		m_pDemux->set_MPEG2AudioMediaType(bMPEG2AudioMediaType);
		m_pDemux->set_FixedAspectRatio(bFixedAspectRatio);
		m_pDemux->set_CreateTSPinOnDemux(bCreateTSPin);
		m_pDemux->set_CreateTxtPinOnDemux(bCreateTxtPin);
		m_pDemux->set_CreateSubPinOnDemux(bCreateSubPin);
		m_pDemux->set_AC3Mode(bAC3Mode);
		m_pDemux->set_NPSlave(bNPSlave);
		m_pDemux->set_NPControl(bNPControl);
		m_pDemux->set_MPEG2Audio2Mode(bAudio2Mode);
	}

	hr = m_pFileReader->SetFileName(wFileName);
	if (FAILED(hr))
	{
		if(wFileName)
			delete[] wFileName;

		return hr;
	}

	hr = m_pFileReader->OpenFile();
	if (FAILED(hr))
	{
		if (!m_pSharedMemory->GetShareMode())
			m_pSharedMemory->SetShareMode(TRUE);
		else
			m_pSharedMemory->SetShareMode(FALSE);

		hr = m_pFileReader->OpenFile();
		if (FAILED(hr))
		{
			if(wFileName)
				delete[] wFileName;

			return VFW_E_INVALIDMEDIATYPE;
		}
	}

	CAMThread::Create();			 //Create our GetDuration thread
	if (CAMThread::ThreadExists())
		CAMThread::CallWorker(CMD_INIT); //Initalize our GetDuration thread

	set_ROTMode();

	__int64 fileStart;
	__int64	fileSize = 0;
	m_pFileReader->GetFileSize(&fileStart, &fileSize);
	//If this a file start then return null.
	if (fileSize < MIN_FILE_SIZE)
	{
//		m_pFileReader->setFilePointer(0, FILE_BEGIN);
//		m_pPidParser->ParsePinMode();
		m_pPidParser->ParseFromFile(0);
		if (!m_pPidParser->pids.dur)
			m_pPidParser->pids.Clear();

		//Check for forced pin mode
		if (pmt)
		{
			//Set for cold start
			m_bColdStart = m_pDemux->get_Auto();
			m_pDemux->set_Auto(FALSE);
//			m_pClock->SetClockRate(0.99);

			if(MEDIATYPE_Stream == pmt->majortype)
			{
				//Are we in Transport mode
				if (MEDIASUBTYPE_MPEG2_TRANSPORT == pmt->subtype)
					m_pPidParser->set_ProgPinMode(FALSE);

				//Are we in Program mode
				else if (MEDIASUBTYPE_MPEG2_PROGRAM == pmt->subtype)
					m_pPidParser->set_ProgPinMode(TRUE);

				m_pPidParser->set_AsyncMode(FALSE);
			}
		}
		else
		{
//			m_pPidParser->ParsePinMode();
		}

		CMediaType cmt;
		cmt.InitMediaType();
		cmt.SetType(&MEDIATYPE_Stream);
		cmt.SetSubtype(&MEDIASUBTYPE_NULL);

		//Are we in Transport mode
		if (!m_pPidParser->get_ProgPinMode())
			cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
		//Are we in Program mode
		else 
			cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_PROGRAM);

		m_pPin->SetMediaType(&cmt);

		{
			CAutoLock cObjectLock(m_pLock);
			m_pFileReader->CloseFile();
		}

		if(wFileName)
			delete[] wFileName;

		m_pPin->m_IntBaseTimePCR = 0;
		m_pPin->m_IntStartTimePCR = 0;
		m_pPin->m_IntCurrentTimePCR = 0;
		m_pPin->m_IntEndTimePCR = 0;
		m_pPin->SetDuration(0);

		IMediaSeeking *pMediaSeeking;
		if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
		{
			CAutoLock cObjectLock(m_pLock);
			REFERENCE_TIME stop, start = 0;
			stop = 0;
			hr = pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
			pMediaSeeking->Release();
			NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
		}

		return S_OK;
	}

	m_pFileReader->setFilePointer(m_pPidParser->get_StartOffset(), FILE_BEGIN);

	RefreshPids();

	LoadPgmReg();
	RefreshDuration();

	//Check for forced pin mode
	if (pmt)
	{
		if(MEDIATYPE_Stream == pmt->majortype)
		{
			//Are we in Transport mode
			if (MEDIASUBTYPE_MPEG2_TRANSPORT == pmt->subtype)
				m_pPidParser->set_ProgPinMode(FALSE);

			//Are we in Program mode
			else if (MEDIASUBTYPE_MPEG2_PROGRAM == pmt->subtype)
				m_pPidParser->set_ProgPinMode(TRUE);

			m_pPidParser->set_AsyncMode(FALSE);
		}
	}

	CMediaType cmt;
	cmt.InitMediaType();
	cmt.SetType(&MEDIATYPE_Stream);
	cmt.SetSubtype(&MEDIASUBTYPE_NULL);

	//Are we in Transport mode
	if (!m_pPidParser->get_ProgPinMode())
		cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
	//Are we in Program mode
	else 
		cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_PROGRAM);

	m_pPin->SetMediaType(&cmt);

	{
		CAutoLock cObjectLock(m_pLock);
		m_pFileReader->CloseFile();
	}
	
	if(wFileName)
		delete[] wFileName;

	m_pPin->m_IntBaseTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntCurrentTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;

	{
//		CAutoLock cObjectLock(m_pLock);
		IMediaSeeking *pMediaSeeking;
		if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
		{
			CAutoLock cObjectLock(m_pLock);
			REFERENCE_TIME stop, start = m_pPidParser->get_StartTimeOffset();
			stop = m_pPidParser->pids.dur;
			hr = pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
			pMediaSeeking->Release();
			NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
		}
	}
	return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::ReLoad(LPCOLESTR pszFileName, const AM_MEDIA_TYPE *pmt)
{
	HRESULT hr;

	BoostThread Boost;
	{
		//Test the file incase it doesn't exist,
		//also loads it into the File buffer for a smoother change over.
		FileReader *pFileReader = NULL;
		SharedMemory *pSharedMemory = NULL;
		long length = lstrlenW(pszFileName);
		if ((length < 9) || (_wcsicmp(pszFileName+length-9, L".tsbuffer") != 0))
		{
			pSharedMemory = new SharedMemory(64000000);
			pFileReader = new MemReader(pSharedMemory);
		}
		else
		{
			pSharedMemory = new SharedMemory(64000000);
			pFileReader = new MultiMemReader(pSharedMemory);
		}

		hr = pFileReader->SetFileName(pszFileName);
		if (FAILED(hr))
		{
			delete pFileReader;
			return hr;
		}

		hr = pFileReader->OpenFile();
		if (FAILED(hr))
		{
			if (!pSharedMemory->GetShareMode())
				pSharedMemory->SetShareMode(TRUE);
			else
				pSharedMemory->SetShareMode(FALSE);

			hr = pFileReader->OpenFile();
			if (FAILED(hr))
			{
				delete pFileReader;
				delete pSharedMemory;
				return VFW_E_INVALIDMEDIATYPE;
			}
		}

//		hr = pFileReader->OpenFile();
//		if (FAILED(hr))
//		{
//			delete pFileReader;
//			return VFW_E_INVALIDMEDIATYPE;
//		}

		pFileReader->CloseFile();
		delete pFileReader;
		delete pSharedMemory;
	}

	BOOL wasThreadRunning = FALSE;
	if (m_bThreadRunning && CAMThread::ThreadExists()) {

		CAMThread::CallWorker(CMD_STOP);
		while (m_bThreadRunning){Sleep(100);};
		wasThreadRunning = TRUE;
	}

	BOOL bState_Running = FALSE;
	BOOL bState_Paused = FALSE;

	if (m_State == State_Running)
		bState_Running = TRUE;
	else if (m_State == State_Paused)
		bState_Paused = TRUE;

	if (bState_Paused || bState_Running)
	{
		CAutoLock lock(&m_Lock);
		m_pDemux->DoStop();
	}

	//Get delay Mode
	USHORT bDelay;
	m_pFileReader->get_DelayMode(&bDelay);

	//Get Pin Mode 
	BOOL pinModeSave = m_pPidParser->get_ProgPinMode();

	//Get ROT Mode 
	BOOL bRotEnable = m_bRotEnable;

	//Get Auto Mode 
	BOOL bAutoEnable = m_pDemux->get_Auto();

	//Get clock type
	int clock = m_pDemux->get_ClockMode();

	//Get Inject Mode 
	BOOL bInjectMode = m_pPin->get_InjectMode();

	//Get Rate Mode 
	BOOL bRateControl = m_pPin->get_RateControl();

	//Get NP Control Mode 
	BOOL bNPControl = m_pDemux->get_NPControl();

	//Get NP Slave Mode 
	BOOL bNPSlave = m_pDemux->get_NPSlave();

	//Get AC3 Mode 
	BOOL bAC3Mode = m_pDemux->get_AC3Mode();

	//Get Aspect Ratio Mode 
	BOOL bFixedAspectRatio = m_pDemux->get_FixedAspectRatio();

	//Get Create TS Pin Mode 
	BOOL bCreateTSPin = m_pDemux->get_CreateTSPinOnDemux();

	//Get Create Txt Pin Mode 
	BOOL bCreateTxtPin = m_pDemux->get_CreateTxtPinOnDemux();

	//Get Create Subtitle Pin Mode 
	BOOL bCreateSubPin = m_pDemux->get_CreateSubPinOnDemux();

	//Get MPEG2 Audio Media Type Mode 
	BOOL bMPEG2AudioMediaType = m_pDemux->get_MPEG2AudioMediaType();

	//Get Audio 2 Mode Mode 
	BOOL bAudio2Mode = m_pDemux->get_MPEG2Audio2Mode();


	delete m_pStreamParser;
	delete m_pDemux;
	delete m_pPidParser;
	delete m_pFileReader;
	delete m_pFileDuration;

	long length = lstrlenW(pszFileName);
	if ((length < 9) || (_wcsicmp(pszFileName+length-9, L".tsbuffer") != 0))
	{
		m_pFileReader = new MemReader(m_pSharedMemory);
		m_pFileDuration = new MemReader(m_pSharedMemory);//Get Live File Duration Thread
	}
	else
	{
		m_pFileReader = new MultiMemReader(m_pSharedMemory);
		m_pFileDuration = new MultiMemReader(m_pSharedMemory);
	}
	//m_pFileReader->SetDebugOutput(TRUE);
	//m_pFileDuration->SetDebugOutput(TRUE);

	m_pPidParser = new PidParser(m_pFileReader);
	m_pDemux = new Demux(m_pPidParser, this, &m_FilterRefList);
	m_pStreamParser = new StreamParser(m_pPidParser, m_pDemux, &netArray);

	//here we reset the shared memory buffers
	m_pSharedMemory->SetShareMode(FALSE);
	m_pSharedMemory->SetShareMode(TRUE);

	// Load Registry Settings data
	GetRegStore("default");

	//Check for forced pin mode
	if (TRUE)
	{
		//Set Auto Mode if we had been told to previously
		m_pDemux->set_Auto(bAutoEnable); 

		//Set delay if we had been told to previously
		if (bDelay)
		{
			m_pFileReader->set_DelayMode(bDelay);
			m_pFileDuration->set_DelayMode(bDelay);
		}

		//Get Rate Mode 
		m_pPin->set_RateControl(bRateControl);

		//Set ROT Mode if we had been told to previously
		m_bRotEnable = bRotEnable;

		//Set clock if we had been told to previously
		if (clock)
			m_pDemux->set_ClockMode(clock);

		m_pDemux->set_MPEG2AudioMediaType(bMPEG2AudioMediaType);
		m_pDemux->set_FixedAspectRatio(bFixedAspectRatio);
		m_pDemux->set_CreateTSPinOnDemux(bCreateTSPin);
		m_pDemux->set_CreateTxtPinOnDemux(bCreateTxtPin);
		m_pDemux->set_CreateSubPinOnDemux(bCreateSubPin);
		m_pDemux->set_AC3Mode(bAC3Mode);
		m_pDemux->set_NPSlave(bNPSlave);
		m_pDemux->set_NPControl(bNPControl);
		m_pDemux->set_MPEG2Audio2Mode(bAudio2Mode);
	}

	hr = m_pFileReader->SetFileName(pszFileName);
	if (FAILED(hr))
		return hr;

	hr = m_pFileReader->OpenFile();
	if (FAILED(hr))
	{
		if (!m_pSharedMemory->GetShareMode())
			m_pSharedMemory->SetShareMode(TRUE);
		else
			m_pSharedMemory->SetShareMode(FALSE);

		hr = m_pFileReader->OpenFile();
		if (FAILED(hr))
			return VFW_E_INVALIDMEDIATYPE;
	}

//	hr = m_pFileReader->OpenFile();
//	if (FAILED(hr))
//		return VFW_E_INVALIDMEDIATYPE;

	hr = m_pFileDuration->SetFileName(pszFileName);
	if (FAILED(hr))
		return hr;

	hr = m_pFileDuration->OpenFile();
	if (FAILED(hr))
		return VFW_E_INVALIDMEDIATYPE;

	set_ROTMode();

	__int64 fileStart;
	__int64	fileSize = 0;
	m_pFileReader->GetFileSize(&fileStart, &fileSize);
	m_llLastMultiFileStart = fileStart;
	m_llLastMultiFileLength = fileSize;

	int count = 0;
	__int64 fileSizeSave = fileSize;
/*	while(fileSize < 5000000 && count < 10)
	{
		count++;
		Sleep(500);
		m_pFileReader->GetFileSize(&fileStart, &fileSize);
		if (fileSize <= fileSizeSave)
		{
			NotifyEvent(EC_NEED_RESTART, NULL, NULL);
			Sleep(1000);
			break;
		}

		fileSizeSave = fileSize;
	};
*/
	while (fileSize < MIN_FILE_SIZE)
	{
//		m_pFileReader->setFilePointer(0, FILE_BEGIN);
//		m_pPidParser->ParsePinMode();
		m_pPidParser->ParseFromFile(0);
		if (m_pPidParser->pids.dur > 0)
		{
			m_pPidParser->pids.Clear();
			break;
		}
		else
			m_pPidParser->pids.Clear();

		//Check for forced pin mode
		if (pmt)
		{
			//Set for cold start
			m_bColdStart = m_pDemux->get_Auto();
			m_pDemux->set_Auto(FALSE);
//			m_pClock->SetClockRate(0.99);

			if(MEDIATYPE_Stream == pmt->majortype)
			{
				//Are we in Transport mode
				if (MEDIASUBTYPE_MPEG2_TRANSPORT == pmt->subtype)
					m_pPidParser->set_ProgPinMode(FALSE);

				//Are we in Program mode
				else if (MEDIASUBTYPE_MPEG2_PROGRAM == pmt->subtype)
					m_pPidParser->set_ProgPinMode(TRUE);

				m_pPidParser->set_AsyncMode(FALSE);
			}
		}
		else
		{
//			m_pPidParser->ParsePinMode();
		}

		CMediaType cmt;
		cmt.InitMediaType();
		cmt.SetType(&MEDIATYPE_Stream);
		cmt.SetSubtype(&MEDIASUBTYPE_NULL);

		//Are we in Transport mode
		if (!m_pPidParser->get_ProgPinMode())
			cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
		//Are we in Program mode
		else 
			cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_PROGRAM);
		m_pPin->SetMediaType(&cmt);

		{
			CAutoLock cObjectLock(m_pLock);
			m_pFileReader->CloseFile();
		}

		IMediaSeeking *pMediaSeeking;
		if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
		{
			CAutoLock cObjectLock(m_pLock);
			m_pPin->m_IntBaseTimePCR = 0;
			m_pPin->m_IntStartTimePCR = 0;
			m_pPin->m_IntCurrentTimePCR = 0;
			m_pPin->m_IntEndTimePCR = 0;
			m_pPin->SetDuration(0);
			REFERENCE_TIME stop, start = 0;
			stop = 0;
			hr = pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
			pMediaSeeking->Release();
			NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
		}
		return S_OK;
	};

	m_pFileReader->setFilePointer(m_pPidParser->get_StartOffset(), FILE_BEGIN);

	m_pPidParser->RefreshPids();
	LoadPgmReg();
	m_pStreamParser->ParsePidArray();
	RefreshDuration();
	m_pPin->m_IntBaseTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntCurrentTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;

	//Check for forced pin mode
	if (pmt)
	{
		if(MEDIATYPE_Stream == pmt->majortype)
		{
			//Are we in Transport mode
			if (MEDIASUBTYPE_MPEG2_TRANSPORT == pmt->subtype)
				m_pPidParser->set_ProgPinMode(FALSE);

			//Are we in Program mode
			else if (MEDIASUBTYPE_MPEG2_PROGRAM == pmt->subtype)
				m_pPidParser->set_ProgPinMode(TRUE);

			m_pPidParser->set_AsyncMode(FALSE);
		}
	}

	CMediaType cmt;
	cmt.InitMediaType();
	cmt.SetType(&MEDIATYPE_Stream);
	cmt.SetSubtype(&MEDIASUBTYPE_NULL);

	//Are we in Transport mode
	if (!m_pPidParser->get_ProgPinMode())
		cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
	//Are we in Program mode
	else 
		cmt.SetSubtype(&MEDIASUBTYPE_MPEG2_PROGRAM);

	m_pPin->SetMediaType(&cmt);

	// Reconnect Demux if pin mode has changed and Source is connected
	if (m_pPidParser->get_ProgPinMode() != pinModeSave && (IPin*)m_pPin->IsConnected())
	{
		m_pPin->ReNewDemux();
	}

	{
		CAutoLock cObjecLock(m_pLock);
		m_pDemux->AOnConnect();
	}
	m_pStreamParser->SetStreamActive(m_pPidParser->get_ProgramNumber());
//	m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);


	if (bState_Paused || bState_Running)
	{
		CAutoLock cObjectLock(m_pLock);
		m_pDemux->DoStart();
	}
				
	if (bState_Paused)
	{
		CAutoLock cObjectLock(m_pLock);
		m_pDemux->DoPause();
	}

	{
//		CAutoLock cObjectLock(m_pLock);
		IMediaSeeking *pMediaSeeking;
		if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
		{
			CAutoLock cObjectLock(m_pLock);
			REFERENCE_TIME stop, start = m_pPidParser->get_StartTimeOffset();
			stop = m_pPidParser->pids.dur;
			hr = pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
			pMediaSeeking->Release();
			NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);	
		}
	}

	if (wasThreadRunning)
		CAMThread::CallWorker(CMD_RUN);


	return S_OK;
}

HRESULT CTSParserSourceFilter::LoadPgmReg(void)
{

	HRESULT hr = S_OK;

	if (m_pPidParser->m_TStreamID && m_pPidParser->pidArray.Count() >= 2)
	{
		std::string saveName = m_pSettingsStore->getName();

		TCHAR cNID_TSID_ID[20];
		sprintf(cNID_TSID_ID, "%i:%i", m_pPidParser->m_NetworkID, m_pPidParser->m_TStreamID);

		// Load Registry Settings data
		GetRegStore(cNID_TSID_ID);

		if (m_pPidParser->set_ProgramSID() == S_OK)
		{
		}
		m_pSettingsStore->setName(saveName);
	}
	return hr;
}

HRESULT CTSParserSourceFilter::Refresh()
{
	CAutoLock lock(&m_Lock);

	if (m_pFileReader)
		return UpdatePidParser(m_pFileReader);

	return E_FAIL;
}

HRESULT CTSParserSourceFilter::UpdatePidParser(FileReader *pFileReader)
{
	HRESULT hr = S_FALSE;// if an error occurs.

	REFERENCE_TIME start, stop;
	m_pPin->GetPositions(&start, &stop);

	PidParser *pPidParser = new PidParser(pFileReader);
	
	if (m_pPidParser->pidArray.Count())
	{
		int ver;
		pPidParser->ParsePinMode();
		ver = pPidParser->m_PATVersion;
		if (pPidParser->pidArray.Count() || ver != m_pPidParser->m_PATVersion)
		{
			int sid = m_pPidParser->pids.sid;
			int sidsave = m_pPidParser->m_ProgramSID;

			if (pPidParser->m_TStreamID) 
			{
				if (sid)
					pPidParser->set_SIDPid(sid); //Setup for search
				else
					pPidParser->set_SIDPid(pPidParser->pids.sid); //Setup for search

				pPidParser->set_ProgramSID(); //set to same sid as before

				if (sidsave)
					pPidParser->m_ProgramSID = sidsave; // restore old sid reg setting.
				else
					pPidParser->m_ProgramSID = pPidParser->pids.sid; // restore old sid reg setting.
			}
							
			if (m_pDemux->CheckDemuxPids(pPidParser) == S_OK)
			{
				delete pPidParser;
				return S_OK;
			}
		}
	}

	int sid = m_pPidParser->pids.sid;
	int sidsave = m_pPidParser->m_ProgramSID;

	if (pPidParser->RefreshPids() == S_OK)
	{
		int count = 200;
		while(m_pDemux->m_bConnectBusyFlag && count--)
			Sleep(10);

		m_pDemux->m_bConnectBusyFlag = TRUE;

	//	__int64 intBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPin->m_IntStartTimePCR - m_pPin->m_IntBaseTimePCR));
		__int64 intBaseTimePCR = parserFunctions.SubtractPCR(m_pPin->m_IntStartTimePCR, m_pPin->m_IntBaseTimePCR);
		intBaseTimePCR = (__int64)max(0, intBaseTimePCR);

		if (pPidParser->pidArray.Count())
		{
			hr = S_OK;

			//Check if we are locked out
			int count = 0;
			while (m_pPidParser->m_ParsingLock)
			{
				Sleep(10);
				count++;

				if (count > 100)
				{
					delete  pPidParser;
					m_pDemux->m_bConnectBusyFlag = FALSE;
					return S_FALSE;
				}
			}
			//Lock the parser
			m_pPidParser->m_ParsingLock = TRUE;

			m_pPidParser->m_PATVersion = pPidParser->m_PATVersion;
			m_pPidParser->m_TStreamID = pPidParser->m_TStreamID;
			m_pPidParser->m_NetworkID = pPidParser->m_NetworkID;
			m_pPidParser->m_ONetworkID = pPidParser->m_ONetworkID;
//			m_pPidParser->m_ProgramSID = pPidParser->m_ProgramSID;
			m_pPidParser->m_ProgPinMode = pPidParser->m_ProgPinMode;
			m_pPidParser->m_AsyncMode = pPidParser->m_AsyncMode;
			m_pPidParser->m_PacketSize = pPidParser->m_PacketSize;
			m_pPidParser->m_ATSCFlag = pPidParser->m_ATSCFlag;
			memcpy(m_pPidParser->m_NetworkName, pPidParser->m_NetworkName, 128);
			memcpy(m_pPidParser->m_NetworkName + 127, "\0", 1);
			m_pPidParser->pids.CopyFrom(&pPidParser->pids);
			m_pPidParser->pidArray.Clear();

			m_pPin->WaitPinLock();
			for (int i = 0; i < pPidParser->pidArray.Count(); i++){
				PidInfo *pPids = new PidInfo;
				pPids->CopyFrom(&pPidParser->pidArray[i]);
				m_pPidParser->pidArray.Add(pPids);
			}
			//UnLock the parser
			m_pPidParser->m_ParsingLock	= FALSE;
		}

		if (m_pPidParser->m_TStreamID) 
		{
			if (sid)
				m_pPidParser->set_SIDPid(sid); //Setup for search
			else
				m_pPidParser->set_SIDPid(m_pPidParser->pids.sid); //Setup for search

			m_pPidParser->set_ProgramSID(); //set to same sid as before

			if (sidsave)
				m_pPidParser->m_ProgramSID = sidsave; // restore old sid reg setting.
			else
				m_pPidParser->m_ProgramSID = m_pPidParser->pids.sid; // restore old sid reg setting.
		}
						
		m_pStreamParser->ParsePidArray();
		m_pDemux->m_bConnectBusyFlag = FALSE;

		if (m_pDemux->CheckDemuxPids(m_pPidParser) == S_FALSE)
		{
			m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPidParser->pids.start, intBaseTimePCR);
			m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));
			m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
			m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;

			m_pDemux->AOnConnect();
			m_pStreamParser->SetStreamActive(m_pPidParser->get_ProgramNumber());
			WORD bDelay = 0;
			m_pFileReader->get_DelayMode(&bDelay);
			if (!bDelay)
			{
				IMediaSeeking *pMediaSeeking;
				if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
				{
					start += RT_SECOND;
					hr = pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
					pMediaSeeking->Release();
				}
			}
		}
		else
			m_pStreamParser->SetStreamActive(m_pPidParser->get_ProgramNumber());

		m_pDemux->m_bConnectBusyFlag = FALSE;

	}
	delete  pPidParser;
	return hr;
}

HRESULT CTSParserSourceFilter::RefreshPids()
{
	HRESULT hr = m_pPidParser->ParseFromFile(m_pFileReader->getFilePointer());
	m_pStreamParser->ParsePidArray();
	return hr;
}

HRESULT CTSParserSourceFilter::RefreshDuration()
{
	return m_pPin->SetDuration(m_pPidParser->pids.dur);
}

STDMETHODIMP CTSParserSourceFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)
{

	CheckPointer(ppszFileName, E_POINTER);
	*ppszFileName = NULL;

	LPOLESTR pFileName = NULL;
	HRESULT hr = m_pFileReader->GetFileName(&pFileName);
	if (FAILED(hr))
		return hr;

	if (pFileName != NULL)
	{
		*ppszFileName = (LPOLESTR)
		QzTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(pFileName)));

		if (*ppszFileName != NULL)
		{
			wcscpy(*ppszFileName, pFileName);
		}
	}

	if(pmt)
	{
		ZeroMemory(pmt, sizeof(*pmt));
//		pmt->majortype = MEDIATYPE_Video;
		pmt->majortype = MEDIATYPE_Stream;

		//Are we in Program mode
		if (m_pPidParser && m_pPidParser->get_ProgPinMode())
			pmt->subtype = MEDIASUBTYPE_MPEG2_PROGRAM;
		else //Are we in Transport mode
			pmt->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;

		pmt->subtype = MEDIASUBTYPE_NULL;
	}

	return S_OK;
} 

STDMETHODIMP CTSParserSourceFilter::GetVideoPid(WORD *pVPid)
{
	if(!pVPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	if (m_pPidParser->pids.vid)
		*pVPid = m_pPidParser->pids.vid;
	else if (m_pPidParser->pids.h264)
		*pVPid = m_pPidParser->pids.h264;
	else
		*pVPid = 0;

	return NOERROR;
}


STDMETHODIMP CTSParserSourceFilter::GetVideoPidType(BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	if (m_pPidParser->pids.vid)
		sprintf((char *)pointer, "MPEG 2");
	else if (m_pPidParser->pids.h264)
		sprintf((char *)pointer, "H.264");
	else if (m_pPidParser->pids.mpeg4)
		sprintf((char *)pointer, "MPEG 4");
	else
		sprintf((char *)pointer, "None");

	return NOERROR;
}


STDMETHODIMP CTSParserSourceFilter::GetAudioPid(WORD *pAPid)
{
	if(!pAPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pAPid = m_pPidParser->pids.aud;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetAudio2Pid(WORD *pA2Pid)
{
	if(!pA2Pid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pA2Pid = m_pPidParser->pids.aud2;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetAACPid(WORD *pAacPid)
{
	if(!pAacPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pAacPid = m_pPidParser->pids.aac;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetAAC2Pid(WORD *pAac2Pid)
{
	if(!pAac2Pid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pAac2Pid = m_pPidParser->pids.aac2;

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetDTSPid(WORD *pDtsPid)
{
	if(!pDtsPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pDtsPid = m_pPidParser->pids.dts;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetDTS2Pid(WORD *pDts2Pid)
{
	if(!pDts2Pid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pDts2Pid = m_pPidParser->pids.dts2;

	return NOERROR;

}
STDMETHODIMP CTSParserSourceFilter::GetAC3Pid(WORD *pAC3Pid)
{
	if(!pAC3Pid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pAC3Pid = m_pPidParser->pids.ac3;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetAC3_2Pid(WORD *pAC3_2Pid)
{
	if(!pAC3_2Pid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	*pAC3_2Pid = m_pPidParser->pids.ac3_2;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetTelexPid(WORD *pTelexPid)
{
	if(!pTelexPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	*pTelexPid = m_pPidParser->pids.txt;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetSubtitlePid(WORD *pSubPid)
{
	if(!pSubPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	*pSubPid = m_pPidParser->pids.sub;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetNIDPid(WORD *pNIDPid)
{
	if(!pNIDPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	*pNIDPid = m_pPidParser->m_NetworkID;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetONIDPid(WORD *pONIDPid)
{
	if(!pONIDPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pONIDPid = m_pPidParser->m_ONetworkID;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetTSIDPid(WORD *pTSIDPid)
{
	if(!pTSIDPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pTSIDPid = m_pPidParser->m_TStreamID;

	return NOERROR;

}
	
STDMETHODIMP CTSParserSourceFilter::GetPMTPid(WORD *pPMTPid)
{
	if(!pPMTPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	*pPMTPid = m_pPidParser->pids.pmt;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetSIDPid(WORD *pSIDPid)
{
	if(!pSIDPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pSIDPid = m_pPidParser->pids.sid;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetPCRPid(WORD *pPCRPid)
{
	if(!pPCRPid)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pPCRPid = m_pPidParser->pids.pcr - m_pPidParser->pids.opcr;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetDuration(REFERENCE_TIME *dur)
{
	if(!dur)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*dur = m_pPidParser->pids.dur;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetChannelNumber(BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ChannelNumber(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetNetworkName(BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_NetworkName(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetONetworkName (BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ONetworkName(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetChannelName(BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ChannelName(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetEPGFromFile(void)
{
	CAutoLock lock(&m_Lock);
	return m_pPidParser->get_EPGFromFile();
}

STDMETHODIMP CTSParserSourceFilter::GetShortNextDescr (BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ShortNextDescr(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetExtendedNextDescr (BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ExtendedNextDescr(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetShortDescr (BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ShortDescr(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetExtendedDescr (BYTE *pointer)
{
	if (!pointer)
		  return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pPidParser->get_ExtendedDescr(pointer);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetPgmNumb(WORD *pPgmNumb)
{
	if(!pPgmNumb)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pPgmNumb = m_pPidParser->get_ProgramNumber() + 1;

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetPgmCount(WORD *pPgmCount)
{
	if(!pPgmCount)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pPgmCount = m_pPidParser->pidArray.Count();

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::SetPgmNumb(WORD PgmNumb)
{
	CAutoLock lock(&m_Lock);
	return set_PgmNumb(PgmNumb);
}

HRESULT CTSParserSourceFilter::set_PgmNumb(WORD PgmNumb)
{
	//If only one program don't change it
	if (m_pPidParser->pidArray.Count() < 1)
		return NOERROR;

	REFERENCE_TIME start, stop;
	m_pPin->GetPositions(&start, &stop);

	int PgmNumber = PgmNumb;
	PgmNumber --;
	if (PgmNumber >= m_pPidParser->pidArray.Count())
	{
		PgmNumber = m_pPidParser->pidArray.Count() - 1;
	}
	else if (PgmNumber <= -1)
	{
		PgmNumber = 0;
	}
	
	BOOL wasThreadRunning = FALSE;
	if (m_bThreadRunning && CAMThread::ThreadExists()) {

		CAMThread::CallWorker(CMD_STOP);
		while (m_bThreadRunning){Sleep(100);};
		wasThreadRunning = TRUE;
	}


//	m_pPin->m_IntBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPin->m_IntStartTimePCR - m_pPin->m_IntBaseTimePCR));
	m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPin->m_IntStartTimePCR, m_pPin->m_IntBaseTimePCR);
	m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));

//	m_pPin->m_DemuxLock = TRUE;
	m_pPidParser->set_ProgramNumber((WORD)PgmNumber);
	m_pPin->SetDuration(m_pPidParser->pids.dur);

//	m_pPin->m_IntBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPidParser->pids.start - m_pPin->m_IntBaseTimePCR));
	m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPidParser->pids.start, m_pPin->m_IntBaseTimePCR);
	m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));

	m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;
	OnConnect();

	ResetStreamTime();

	IMediaSeeking *pMediaSeeking;
	if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
	{
		pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
		pMediaSeeking->Release();
	}

//Sleep(5000);

	if (wasThreadRunning)
		CAMThread::CallWorker(CMD_RUN);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::NextPgmNumb(void)
{
	CAutoLock lock(&m_Lock);

	//If only one program don't change it
	if (m_pPidParser->pidArray.Count() < 2)
		return NOERROR;

	REFERENCE_TIME start, stop;
	m_pPin->GetPositions(&start, &stop);

	WORD PgmNumb = m_pPidParser->get_ProgramNumber();
	PgmNumb++;
	if (PgmNumb >= m_pPidParser->pidArray.Count())
	{
		PgmNumb = 0;
	}

	BOOL wasThreadRunning = FALSE;
	if (m_bThreadRunning && CAMThread::ThreadExists()) {

		CAMThread::CallWorker(CMD_STOP);
		while (m_bThreadRunning){Sleep(100);};
		wasThreadRunning = TRUE;
	}

//	m_pPin->m_IntBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPin->m_IntStartTimePCR - m_pPin->m_IntBaseTimePCR));
	m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPin->m_IntStartTimePCR, m_pPin->m_IntBaseTimePCR);
	m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));

//	m_pPin->m_DemuxLock = TRUE;
	m_pPidParser->set_ProgramNumber(PgmNumb);
	m_pPin->SetDuration(m_pPidParser->pids.dur);

//	m_pPin->m_IntBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPidParser->pids.start - m_pPin->m_IntBaseTimePCR));
	m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPidParser->pids.start, m_pPin->m_IntBaseTimePCR);
	m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));

	m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;
	OnConnect();
//	Sleep(200);
	ResetStreamTime();

	IMediaSeeking *pMediaSeeking;
	if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
	{
		pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
		pMediaSeeking->Release();
	}

//	m_pPin->setPositions(&start, AM_SEEKING_AbsolutePositioning, NULL, NULL);

//	m_pPin->m_DemuxLock = FALSE;

	if (wasThreadRunning)
		CAMThread::CallWorker(CMD_RUN);

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::PrevPgmNumb(void)
{
	CAutoLock lock(&m_Lock);

	//If only one program don't change it
	if (m_pPidParser->pidArray.Count() < 2)
		return NOERROR;

	REFERENCE_TIME start, stop;
	m_pPin->GetPositions(&start, &stop);

	int PgmNumb = m_pPidParser->get_ProgramNumber();
	PgmNumb--;
	if (PgmNumb < 0)
	{
		PgmNumb = m_pPidParser->pidArray.Count() - 1;
	}

	BOOL wasThreadRunning = FALSE;
	if (m_bThreadRunning && CAMThread::ThreadExists()) {

		CAMThread::CallWorker(CMD_STOP);
		while (m_bThreadRunning){Sleep(100);};
		wasThreadRunning = TRUE;
	}

//	m_pPin->m_IntBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPin->m_IntStartTimePCR - m_pPin->m_IntBaseTimePCR));
	m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPin->m_IntStartTimePCR, m_pPin->m_IntBaseTimePCR);
	m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));

//	m_pPin->m_DemuxLock = TRUE;
	m_pPidParser->set_ProgramNumber((WORD)PgmNumb);
	m_pPin->SetDuration(m_pPidParser->pids.dur);

//	m_pPin->m_IntBaseTimePCR = (__int64)min(m_pPidParser->pids.end, (__int64)(m_pPidParser->pids.start - m_pPin->m_IntBaseTimePCR));
	m_pPin->m_IntBaseTimePCR = parserFunctions.SubtractPCR(m_pPidParser->pids.start, m_pPin->m_IntBaseTimePCR);
	m_pPin->m_IntBaseTimePCR = (__int64)max(0, (__int64)(m_pPin->m_IntBaseTimePCR));

	m_pPin->m_IntStartTimePCR = m_pPidParser->pids.start;
	m_pPin->m_IntEndTimePCR = m_pPidParser->pids.end;
	OnConnect();
//	Sleep(200);

	ResetStreamTime();

	IMediaSeeking *pMediaSeeking;
	if(GetFilterGraph() && SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
	{
		pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
		pMediaSeeking->Release();
	}

//	m_pPin->setPositions(&start, AM_SEEKING_AbsolutePositioning, NULL, NULL);

//	m_pPin->m_DemuxLock = FALSE;

	if (wasThreadRunning)
		CAMThread::CallWorker(CMD_RUN);

	return NOERROR;
}

HRESULT CTSParserSourceFilter::GetFileSize(__int64 *pStartPosition, __int64 *pEndPosition)
{
	CAutoLock lock(&m_Lock);
	return m_pFileReader->GetFileSize(pStartPosition, pEndPosition);
}

STDMETHODIMP CTSParserSourceFilter::GetTsArray(ULONG *pPidArray)
{
	if(!pPidArray)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);

	m_pPidParser->get_CurrentTSArray(pPidArray);
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetAC3Mode(WORD *pAC3Mode)
{
	if(!pAC3Mode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pAC3Mode = m_pDemux->get_AC3Mode();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetAC3Mode(WORD AC3Mode)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_AC3Mode(AC3Mode);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetMP2Mode(WORD *pMP2Mode)
{
	if(!pMP2Mode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pMP2Mode = m_pDemux->get_MPEG2AudioMediaType();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetMP2Mode(WORD MP2Mode)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_MPEG2AudioMediaType(MP2Mode);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetAudio2Mode(WORD *pAudio2Mode)
{
	if(!pAudio2Mode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pAudio2Mode = m_pDemux->get_MPEG2Audio2Mode();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetAudio2Mode(WORD Audio2Mode)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_MPEG2Audio2Mode(Audio2Mode);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetAutoMode(WORD *AutoMode)
{
	if(!AutoMode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*AutoMode = m_pDemux->get_Auto();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetAutoMode(WORD AutoMode)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_Auto(AutoMode);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetNPControl(WORD *NPControl)
{
	if(!NPControl)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*NPControl = m_pDemux->get_NPControl();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetNPControl(WORD NPControl)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_NPControl(NPControl);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetNPSlave(WORD *NPSlave)
{
	if(!NPSlave)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*NPSlave = m_pDemux->get_NPSlave();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetNPSlave(WORD NPSlave)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_NPSlave(NPSlave);
	OnConnect();
	return NOERROR;
}

HRESULT CTSParserSourceFilter::set_TunerEvent(void)
{
	if (m_pTunerEvent)
		if (GetFilterGraph() && SUCCEEDED(m_pTunerEvent->HookupGraphEventService(GetFilterGraph())))
		{
			m_pTunerEvent->RegisterForTunerEvents();
		}

	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetTunerEvent(void)
{
	CAutoLock lock(&m_Lock);
	return set_TunerEvent();
}

STDMETHODIMP CTSParserSourceFilter::GetDelayMode(WORD *DelayMode)
{
	if(!DelayMode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pFileReader->get_DelayMode(DelayMode);
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetDelayMode(WORD DelayMode)
{
	CAutoLock lock(&m_Lock);
	m_pFileReader->set_DelayMode(DelayMode);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetSharedMode(WORD* pSharedMode)
{
	if (!pSharedMode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pSharedMode = m_bSharedMode;
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetSharedMode(WORD SharedMode)
{
	CAutoLock lock(&m_Lock);
	m_bSharedMode = SharedMode;
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetInjectMode(WORD* pInjectMode)
{
	if (!pInjectMode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pInjectMode = m_pPin->get_InjectMode();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetInjectMode(WORD InjectMode)
{
	CAutoLock lock(&m_Lock);
	m_pPin->set_InjectMode(InjectMode);
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetRateControlMode(WORD* pRateControl)
{
	if (!pRateControl)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pRateControl = m_pPin->get_RateControl();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetRateControlMode(WORD RateControl)
{
	CAutoLock lock(&m_Lock);
	m_pPin->set_RateControl(RateControl);
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetFixedAspectRatio(WORD *pbFixedAR)
{
	if(!pbFixedAR)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pbFixedAR = m_pDemux->get_FixedAspectRatio();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetFixedAspectRatio(WORD bFixedAR)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_FixedAspectRatio(bFixedAR);
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetCreateTSPinOnDemux(WORD *pbCreatePin)
{
	if(!pbCreatePin)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pbCreatePin = m_pDemux->get_CreateTSPinOnDemux();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetCreateTSPinOnDemux(WORD bCreatePin)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_CreateTSPinOnDemux(bCreatePin);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetCreateTxtPinOnDemux(WORD *pbCreatePin)
{
	if(!pbCreatePin)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pbCreatePin = m_pDemux->get_CreateTxtPinOnDemux();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetCreateTxtPinOnDemux(WORD bCreatePin)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_CreateTxtPinOnDemux(bCreatePin);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetCreateSubPinOnDemux(WORD *pbCreatePin)
{
	if(!pbCreatePin)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pbCreatePin = m_pDemux->get_CreateSubPinOnDemux();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetCreateSubPinOnDemux(WORD bCreatePin)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_CreateSubPinOnDemux(bCreatePin);
	OnConnect();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetReadOnly(WORD *ReadOnly)
{
	if(!ReadOnly)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	m_pFileReader->get_ReadOnly(ReadOnly);
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetBitRate(long *pRate)
{
    if(!pRate)
        return E_INVALIDARG;

    CAutoLock lock(&m_Lock);
    *pRate = m_pPin->get_BitRate();

    return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetBitRate(long Rate)
{
    CAutoLock lock(&m_Lock);
    m_pPin->set_BitRate(Rate);

    return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::GetROTMode(WORD *ROTMode)
{
	if(!ROTMode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*ROTMode = m_bRotEnable;
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetROTMode(WORD ROTMode)
{
	CAutoLock lock(&m_Lock);
	m_bRotEnable = ROTMode;
	set_ROTMode();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetClockMode(WORD *ClockMode)
{
	if(!ClockMode)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*ClockMode = m_pDemux->get_ClockMode();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetClockMode(WORD ClockMode)
{
	CAutoLock lock(&m_Lock);
	m_pDemux->set_ClockMode(ClockMode);
	m_pDemux->SetRefClock();
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetRegStore(LPTSTR nameReg)
{

	char name[128] = "";
	sprintf(name, "%s", nameReg);

	if ((strcmp(name, "user")!=0) && (strcmp(name, "default")!=0))
	{
		std::string saveName = m_pSettingsStore->getName();
		m_pSettingsStore->setName(nameReg);
		m_pSettingsStore->setProgramSIDReg((int)m_pPidParser->pids.sid);
		m_pSettingsStore->setAudio2ModeReg((BOOL)m_pDemux->get_MPEG2Audio2Mode());
		m_pSettingsStore->setAC3ModeReg((BOOL)m_pDemux->get_AC3Mode());
		m_pRegStore->setSettingsInfo(m_pSettingsStore);
		m_pSettingsStore->setName(saveName);
	}
	else
	{
		WORD delay;
		m_pFileReader->get_DelayMode(&delay);
		m_pSettingsStore->setDelayModeReg((BOOL)delay);
		m_pSettingsStore->setSharedModeReg((BOOL)m_bSharedMode);
		m_pSettingsStore->setInjectModeReg((BOOL)m_pPin->get_InjectMode());
		m_pSettingsStore->setRateControlModeReg((BOOL)m_pPin->get_RateControl());
		m_pSettingsStore->setAutoModeReg((BOOL)m_pDemux->get_Auto());
		m_pSettingsStore->setNPControlReg((BOOL)m_pDemux->get_NPControl());
		m_pSettingsStore->setNPSlaveReg((BOOL)m_pDemux->get_NPSlave());
		m_pSettingsStore->setMP2ModeReg((BOOL)m_pDemux->get_MPEG2AudioMediaType());
		m_pSettingsStore->setAudio2ModeReg((BOOL)m_pDemux->get_MPEG2Audio2Mode());
		m_pSettingsStore->setAC3ModeReg((BOOL)m_pDemux->get_AC3Mode());
		m_pSettingsStore->setFixedAspectRatioReg((BOOL)m_pDemux->get_FixedAspectRatio());
		m_pSettingsStore->setCreateTSPinOnDemuxReg((BOOL)m_pDemux->get_CreateTSPinOnDemux());
		m_pSettingsStore->setROTModeReg((int)m_bRotEnable);
		m_pSettingsStore->setClockModeReg((BOOL)m_pDemux->get_ClockMode());
		m_pSettingsStore->setCreateTxtPinOnDemuxReg((BOOL)m_pDemux->get_CreateTxtPinOnDemux());

		m_pRegStore->setSettingsInfo(m_pSettingsStore);
	}
    return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::GetRegStore(LPTSTR nameReg)
{
	char name[128] = "";
	sprintf(name, "%s", nameReg);

	std::string saveName = m_pSettingsStore->getName();

	// Load Registry Settings data
	m_pSettingsStore->setName(nameReg);

	if(m_pRegStore->getSettingsInfo(m_pSettingsStore))
	{
		if ((strcmp(name, "user")!=0) && (strcmp(name, "default")!=0))
		{
			m_pPidParser->set_SIDPid(m_pSettingsStore->getProgramSIDReg());
			m_pDemux->set_AC3Mode(m_pSettingsStore->getAC3ModeReg());
			m_pDemux->set_MPEG2Audio2Mode(m_pSettingsStore->getAudio2ModeReg());
			m_pSettingsStore->setName(saveName);
		}
		else
		{	
			m_pFileReader->set_DelayMode(m_pSettingsStore->getDelayModeReg());
			m_pFileDuration->set_DelayMode(m_pSettingsStore->getDelayModeReg());
			m_pDemux->set_Auto(m_pSettingsStore->getAutoModeReg());
			m_pDemux->set_NPControl(m_pSettingsStore->getNPControlReg());
			m_pDemux->set_NPSlave(m_pSettingsStore->getNPSlaveReg());
			m_pDemux->set_MPEG2AudioMediaType(m_pSettingsStore->getMP2ModeReg());
			m_pDemux->set_MPEG2Audio2Mode(m_pSettingsStore->getAudio2ModeReg());
			m_pDemux->set_AC3Mode(m_pSettingsStore->getAC3ModeReg());
			m_pDemux->set_FixedAspectRatio(m_pSettingsStore->getFixedAspectRatioReg());
			m_pDemux->set_CreateTSPinOnDemux(m_pSettingsStore->getCreateTSPinOnDemuxReg());
			m_bSharedMode = m_pSettingsStore->getSharedModeReg();
			m_pPin->set_InjectMode(m_pSettingsStore->getInjectModeReg());
			m_pPin->set_RateControl(m_pSettingsStore->getRateControlModeReg());
			m_bRotEnable = m_pSettingsStore->getROTModeReg();
			m_pDemux->set_ClockMode(m_pSettingsStore->getClockModeReg());
			m_pDemux->set_CreateTxtPinOnDemux(m_pSettingsStore->getCreateTxtPinOnDemuxReg());
		}
	}

    return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetRegSettings()
{
	CAutoLock lock(&m_Lock);
	SetRegStore("user");
    return NOERROR;
}


STDMETHODIMP CTSParserSourceFilter::GetRegSettings()
{
	CAutoLock lock(&m_Lock);
	GetRegStore("user");
    return NOERROR;
}

HRESULT CTSParserSourceFilter::set_RegProgram()
{
	if (m_pPidParser->pids.sid && m_pPidParser->m_TStreamID)
	{
		TCHAR cNID_TSID_ID[32];
		sprintf(cNID_TSID_ID, "%i:%i", m_pPidParser->m_NetworkID, m_pPidParser->m_TStreamID);
		SetRegStore(cNID_TSID_ID);
	}
    return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::SetRegProgram()
{
	CAutoLock lock(&m_Lock);
	return set_RegProgram();
}

STDMETHODIMP CTSParserSourceFilter::ShowFilterProperties()
{
	CAutoLock cObjectLock(m_pLock);

//    HWND    phWnd = (HWND)CreateEvent(NULL, FALSE, FALSE, NULL);

	ULONG refCount;
	IEnumFilters * piEnumFilters = NULL;
	if (GetFilterGraph() && SUCCEEDED(GetFilterGraph()->EnumFilters(&piEnumFilters)))
	{
		IBaseFilter * pFilter;
		while (piEnumFilters->Next(1, &pFilter, 0) == NOERROR )
		{
			ISpecifyPropertyPages* piProp = NULL;
			if ((pFilter->QueryInterface(IID_ISpecifyPropertyPages, (void **)&piProp) == S_OK) && (piProp != NULL))
			{
				FILTER_INFO filterInfo;
				if (pFilter->QueryFilterInfo(&filterInfo) == S_OK)
				{
					LPOLESTR fileName = NULL;
					m_pFileReader->GetFileName(&fileName);
			
					if (fileName && !wcsicmp(fileName, filterInfo.achName))
					{
						CAUUID caGUID;
						piProp->GetPages(&caGUID);
						if(caGUID.cElems)
						{
							IUnknown *piFilterUnk = NULL;
							if (pFilter->QueryInterface(IID_IUnknown, (void **)&piFilterUnk) == S_OK)
							{
								OleCreatePropertyFrame(0, 0, 0, filterInfo.achName, 1, &piFilterUnk, caGUID.cElems, caGUID.pElems, 0, 0, NULL);
								piFilterUnk->Release();
							}
							CoTaskMemFree(caGUID.pElems);
						}
					}
					filterInfo.pGraph->Release(); 
				}
				piProp->Release();
			}
			refCount = pFilter->Release();
			pFilter = NULL;
		}
		refCount = piEnumFilters->Release();
	}
//	CloseHandle(phWnd);
	return NOERROR;
}

BOOL CTSParserSourceFilter::get_AutoMode()
{
	return m_pDemux->get_Auto();
}

BOOL CTSParserSourceFilter::get_PinMode()
{
	return m_pPidParser->get_ProgPinMode();;
}

// Adds a DirectShow filter graph to the Running Object Table,
// allowing GraphEdit to "spy" on a remote filter graph.
HRESULT CTSParserSourceFilter::AddGraphToRot(
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
/*	
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
*/
    hr = CreateItemMoniker(L"!", wsz, &pMoniker);
    if (SUCCEEDED(hr))
	{
        hr = pROT->Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, pUnkGraph, 
                            pMoniker, pdwRegister);
	}
    return hr;
}
        
// Removes a filter graph from the Running Object Table
void CTSParserSourceFilter::RemoveGraphFromRot(DWORD pdwRegister)
{
    CComPtr <IRunningObjectTable> pROT;

    if (SUCCEEDED(GetRunningObjectTable(0, &pROT))) 
        pROT->Revoke(pdwRegister);
}

void CTSParserSourceFilter::set_ROTMode()
{
	if (m_bRotEnable)
	{
		if (GetFilterGraph() && !m_dwGraphRegister && FAILED(AddGraphToRot (GetFilterGraph(), &m_dwGraphRegister)))
			m_dwGraphRegister = 0;
	}
	else if (m_dwGraphRegister)
	{
			RemoveGraphFromRot(m_dwGraphRegister);
			m_dwGraphRegister = 0;
	}
}

HRESULT CTSParserSourceFilter::GetObjectFromROT(WCHAR* wsFullName, IUnknown **ppUnk)
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

HRESULT CTSParserSourceFilter::ShowEPGInfo()
{
	CAutoLock lock(&m_Lock);
	return showEPGInfo();
}

HRESULT CTSParserSourceFilter::showEPGInfo()
{
	HRESULT hr = m_pPidParser->get_EPGFromFile();
	if (hr == S_OK)
	{
		unsigned char netname[128] = "";
		unsigned char onetname[128] ="";
		unsigned char chname[128] ="";
		unsigned char chnumb[128] ="";
		unsigned char shortdescripor[128] ="";
		unsigned char Extendeddescripor[600] ="";
		unsigned char shortnextdescripor[128] ="";
		unsigned char Extendednextdescripor[600] ="";
		m_pPidParser->get_NetworkName((unsigned char*)&netname);
		m_pPidParser->get_ONetworkName((unsigned char*)&onetname);
		m_pPidParser->get_ChannelName((unsigned char*)&chname);
		m_pPidParser->get_ChannelNumber((unsigned char*)&chnumb);
		m_pPidParser->get_ShortDescr((unsigned char*)&shortdescripor);
		m_pPidParser->get_ExtendedDescr((unsigned char*)&Extendeddescripor);
		m_pPidParser->get_ShortNextDescr((unsigned char*)&shortnextdescripor);
		m_pPidParser->get_ExtendedNextDescr((unsigned char*)&Extendednextdescripor);
		TCHAR szBuffer[(6*128)+ (2*600)];
		sprintf(szBuffer, "Network Name:- %s\n"
		"ONetwork Name:- %s\n"
		"Channel Number:- %s\n"
		"Channel Name:- %s\n\n"
		"Program Name: - %s\n"
		"Program Description:- %s\n\n"
		"Next Program Name: - %s\n"
		"Next Program Description:- %s\n"
			,netname,
			onetname,
			chnumb,
			chname,
			shortdescripor,
			Extendeddescripor,
			shortnextdescripor,
			Extendednextdescripor
			);
			MessageBox(NULL, szBuffer, TEXT("Program Infomation"), MB_OK);
	}
	return hr;
}

STDMETHODIMP CTSParserSourceFilter::GetPCRPosition(REFERENCE_TIME *pos)
{
	if(!pos)
		return E_INVALIDARG;

	CAutoLock lock(&m_Lock);
	*pos = m_pPin->getPCRPosition();

	return NOERROR;

}

STDMETHODIMP CTSParserSourceFilter::ShowStreamMenu(HWND hwnd)
{
	CAutoLock lock(&m_Lock);
	HRESULT hr;

	POINT mouse;
	GetCursorPos(&mouse);

	HMENU hMenu = CreatePopupMenu();
	if (hMenu)
	{
		IAMStreamSelect *pIAMStreamSelect;
		hr = this->QueryInterface(IID_IAMStreamSelect, (void**)&pIAMStreamSelect);
		if (SUCCEEDED(hr))
		{
			ULONG count;
			pIAMStreamSelect->Count(&count);

			ULONG flags, group, lastgroup = -1;
				
			for(UINT i = 0; i < count; i++)
			{
				WCHAR* pStreamName = NULL;

				if(S_OK == pIAMStreamSelect->Info(i, 0, &flags, 0, &group, &pStreamName, 0, 0))
				{
					if(lastgroup != group && i) 
						::AppendMenu(hMenu, MF_SEPARATOR, NULL, NULL);

					lastgroup = group;

					if(pStreamName)
					{
//						UINT uFlags = MF_STRING | MF_ENABLED;
//						if (flags & AMSTREAMSELECTINFO_EXCLUSIVE)
//							uFlags |= MF_CHECKED; //MFT_RADIOCHECK;
//						else if (flags & AMSTREAMSELECTINFO_ENABLED)
//							uFlags |= MF_CHECKED;
						
						UINT uFlags = (flags?MF_CHECKED:MF_UNCHECKED) | MF_STRING | MF_ENABLED;
						::AppendMenuW(hMenu, uFlags, (i + 0x100), LPCWSTR(pStreamName));
						CoTaskMemFree(pStreamName);
					}
				}
			}

			SetForegroundWindow(hwnd);
			UINT index = ::TrackPopupMenu(hMenu, TPM_LEFTBUTTON|TPM_RETURNCMD, mouse.x, mouse.y, 0, hwnd, 0);
			PostMessage(hwnd, NULL, 0, 0);

			if(index & 0x100) 
				pIAMStreamSelect->Enable((index & 0xff), AMSTREAMSELECTENABLE_ENABLE);

			pIAMStreamSelect->Release();
		}
		DestroyMenu(hMenu);
	}
	return hr;
}























//*****************************************************************************************
//ASync Additions

STDMETHODIMP CTSParserSourceFilter::RequestAllocator(
                      IMemAllocator* pPreferred,
                      ALLOCATOR_PROPERTIES* pProps,
                      IMemAllocator ** ppActual)
{
	CAutoLock cObjectLock(m_pLock);
	Pause();
	Stop();

    return S_OK;
}

STDMETHODIMP CTSParserSourceFilter::Request(
                     IMediaSample* pSample,
                     DWORD_PTR dwUser)
{
	return E_NOTIMPL;
}

STDMETHODIMP CTSParserSourceFilter::WaitForNext(
                      DWORD dwTimeout,
                      IMediaSample** ppSample,  
                      DWORD_PTR * pdwUser)
{
	return E_NOTIMPL;
}

STDMETHODIMP CTSParserSourceFilter::SyncReadAligned(
                      IMediaSample* pSample)
{
	return E_NOTIMPL;
}

STDMETHODIMP CTSParserSourceFilter::SyncRead(
                      LONGLONG llPosition,  // absolute file position
                      LONG lLength,         // nr bytes required
                      BYTE* pBuffer)
{
    CheckPointer(pBuffer, E_POINTER);

	HRESULT hr;
	LONG dwBytesToRead = lLength;
    CAutoLock lck(&m_Lock);
    DWORD dwReadLength;

	if (m_pFileReader->IsFileInvalid())
	{
		hr = m_pFileReader->OpenFile();
		if (FAILED(hr))
			return E_FAIL;

		int count = 0;
		__int64 fileStart, fileSize = 0;
		m_pFileReader->GetFileSize(&fileStart, &fileSize);

		//If this a file start then return null.
		while(fileSize < 500000 && count < 10)
		{
			Sleep(100);
			m_pFileReader->GetFileSize(&fileStart, &fileSize);
			count++;
		}
	}

	__int64 fileStart, fileSize = 0;
	m_pFileReader->GetFileSize(&fileStart, &fileSize);
	// Read the data from the file
	llPosition = min(fileSize, llPosition);
	llPosition = max(0, llPosition);
//	hr = m_pFileReader->Read(pBuffer, dwBytesToRead, &dwReadLength, (__int64)(llPosition - fileSize), FILE_END);
	hr = m_pFileReader->Read(pBuffer, dwBytesToRead, &dwReadLength, llPosition, FILE_BEGIN);
	if (FAILED(hr))
		return hr;

	if (dwReadLength < (DWORD)dwBytesToRead) 
	{
		WORD wReadOnly = 0;
		m_pFileReader->get_ReadOnly(&wReadOnly);
		if (wReadOnly)
		{
			while (dwReadLength < (DWORD)dwBytesToRead) 
			{
				WORD bDelay = 0;
				m_pFileReader->get_DelayMode(&bDelay);

				if (bDelay > 0)
					Sleep(2000);
				else
					Sleep(100);

				__int64 fileStart, filelength;
				m_pFileReader->GetFileSize(&fileStart, &filelength);
				ULONG ulNextBytesRead = 0;				
				llPosition = min(filelength, llPosition);
				llPosition = max(0, llPosition);
//				HRESULT hr = m_pFileReader->Read(pBuffer, dwBytesToRead, &dwReadLength, (__int64)(llPosition - filelength), FILE_END);
				HRESULT hr = m_pFileReader->Read(pBuffer, dwBytesToRead, &dwReadLength, llPosition, FILE_BEGIN);
				if (FAILED(hr))
					return hr;

				if ((ulNextBytesRead == 0) || (ulNextBytesRead == dwReadLength))
					return E_FAIL;

				dwReadLength = ulNextBytesRead;
			}
		}
		else
		{
			m_pFileReader->CloseFile();
			return E_FAIL;
		}
	}

	return NOERROR;
}

    // return total length of stream, and currently available length.
    // reads for beyond the available length but within the total length will
    // normally succeed but may block for a long period.
STDMETHODIMP CTSParserSourceFilter::Length(
                      LONGLONG* pTotal,
                      LONGLONG* pAvailable)
{
    CAutoLock lck(&m_Lock);

    CheckPointer(pTotal, E_POINTER);
    CheckPointer(pAvailable, E_POINTER);


	HRESULT hr;

	__int64 fileStart;
	__int64	fileSize = 0;

	if (m_pFileReader->IsFileInvalid())
	{
		hr = m_pFileReader->OpenFile();
		if (FAILED(hr))
			return E_FAIL;

		int count = 0;
		m_pFileReader->GetFileSize(&fileStart, &fileSize);

		//If this a file start then return null.
		while(fileSize < 500000 && count < 10)
		{
			Sleep(100);
			m_pFileReader->GetFileSize(&fileStart, &fileSize);
			count++;
		}
	}

	m_pFileReader->GetFileSize(&fileStart, &fileSize);

	*pTotal = fileSize;		
	*pAvailable = fileSize;		
	return NOERROR;
}

STDMETHODIMP CTSParserSourceFilter::BeginFlush(void)
{
	return E_NOTIMPL;
}

STDMETHODIMP CTSParserSourceFilter::EndFlush(void)
{
	return E_NOTIMPL;
}
//m_pPin->PrintTime(TEXT("Run"), (__int64) tStart, 10000);

//*****************************************************************************************


//////////////////////////////////////////////////////////////////////////
// End of interface implementations
//////////////////////////////////////////////////////////////////////////

