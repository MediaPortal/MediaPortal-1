/**
 *	DVBMpeg2DataParser.cpp
 *	Copyright (C) 2004, 2005 Nate
 *  Copyright (C) 2004 JoeyBloggs
 *
 *	This file is part of DigitalWatch, a free DTV watching and recording
 *	program for the VisionPlus DVB-T.
 *
 *	DigitalWatch is free software; you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation; either version 2 of the License, or
 *	(at your option) any later version.
 *
 *	DigitalWatch is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with DigitalWatch; if not, write to the Free Software
 *	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#include "stdafx.h"
#include "DVBMpeg2DataParser.h"
#include "GlobalFunctions.h"
#include "Globals.h"

#include <process.h>
#include <math.h>

void ParseMpeg2DataThread(void *pParam)
{
	DVBMpeg2DataParser *scanner;
	scanner = (DVBMpeg2DataParser *)pParam;
	scanner->StartScanThread();
}

//////////////////////////////////////////////////////////////////////
// DVBSection
//////////////////////////////////////////////////////////////////////
DVBSection::DVBSection()
{
	segmented = 0;
	fd = -1;
	pid = -1;
	tableId = -1;
	tableIdExt = -1;
	sectionVersionNumber = -1;
	sectionFilterDone = 0;

	timeout = 0;
	startTime = 0;
	runningTime = 0;
	nextSegment = NULL;

	memset (m_sectionDone, 0, 32 * sizeof(__int8));
}

DVBSection::~DVBSection()
{
	if (nextSegment)
		delete nextSegment;
}

void DVBSection::Setup(long pid, long tableId, long segmented, int timeout)
{
	this->pid = pid;
	this->tableId = tableId;
	this->segmented = segmented;
	this->timeout = timeout;
}

DVBSection *DVBSection::CreateNewSegment(long tableIdExtention, long version_number)
{
	nextSegment = new DVBSection();
	nextSegment->segmented = segmented;
	nextSegment->timeout = timeout;
	nextSegment->tableId = tableId;
	nextSegment->tableIdExt = tableIdExtention;
	nextSegment->sectionVersionNumber = version_number;
	return nextSegment;
}

DVBSection *DVBSection::FindSegment(long tableIdExtention)
{
	DVBSection *next = this;
	while (next)
	{
		if (next->tableIdExt == tableIdExtention)
			return next;
		next = next->nextSegment;
	}
	return NULL;
}

BOOL DVBSection::IsSectionDone(long sectionNumber)
{
	return (m_sectionDone[sectionNumber/8] >> (sectionNumber % 8)) & 1;
}

void DVBSection::SetSectionDone(long sectionNumber)
{
	m_sectionDone[sectionNumber/8] |= 1 << (sectionNumber % 8);
}

void DVBSection::ResetSectionDone()
{
	memset (m_sectionDone, 0, sizeof(m_sectionDone));
}

//////////////////////////////////////////////////////////////////////
// DVBStream
//////////////////////////////////////////////////////////////////////
DVBStream::DVBStream()
{
	MpegStreamType = 0;
}

DVBStream::~DVBStream()
{
}

//////////////////////////////////////////////////////////////////////
// DVBService
//////////////////////////////////////////////////////////////////////
DVBService::DVBService()
{
	transportStreamId = 0;
	providerName = NULL;
	pmtPid = 0;
	pcrPid = 0;
	type = 0;
	scrambled = 0;
	running = RM_UNDEFINED;
	section = NULL;
}

DVBService::~DVBService()
{
	if (providerName)
		delete[] providerName;

	if (section)
		delete section;
}

void DVBService::ParseSDTDescriptors(unsigned char *buf, int remainingLength)
{
	while (remainingLength > 0)
	{
		unsigned char descriptorTag = buf[0];
		unsigned char descriptorLen = buf[1];

		if (!descriptorLen)
		{
			log.showf("descriptor_tag == 0x%02x, len is 0\n", descriptorTag);
			break;
		}

		switch (descriptorTag)
		{
			case 0x48:
			{
				ParseServiceDescriptor(buf);
				break;
			}

/*			case 0x53:
			{
				parse_ca_identifier_descriptor (buf, data);
				break;
			}
*/
			default:
			{
				log.showf("skip descriptor 0x%02x\n", descriptorTag);
			}
		};

		buf += (descriptorLen + 2);
		remainingLength -= (descriptorLen + 2);
	}
}

void DVBService::ParseServiceDescriptor(unsigned char *buf)
{
	type = buf[2];

	buf += 3;
	strCopyA2W(providerName, (LPSTR)(buf+1), buf[0]);

	// TODO: remove control characters (FIXME: handle short/long name)
	// TODO: handle character set correctly (e.g. via iconv)
	// c.f. EN 300 468 annex A

	buf += 1+buf[0];
	strCopyA2W(serviceName, (LPSTR)(buf+1), buf[0]);

	// TODO: remove control characters (FIXME: handle short/long name)
	// TODO: handle character set correctly (e.g. via iconv)
	// c.f. EN 300 468 annex A

	log.showf("0x%04x 0x%04x: pmt_pid 0x%04x %S -- %S (%s%s)\n",
			  transportStreamId, serviceId, pmtPid, providerName, serviceName,
			  running == RM_NOT_RUNNING ? "not running" :
			  running == RM_STARTS_SOON ? "starts soon" :
			  running == RM_PAUSING     ? "pausing" :
			  running == RM_RUNNING     ? "running" : "???",
			  scrambled ? ", scrambled" : "");
}


//////////////////////////////////////////////////////////////////////
// DVBTransponder
//////////////////////////////////////////////////////////////////////
DVBTransponder::DVBTransponder() :
	DVBTChannels_Network(NULL)
{
}

DVBTransponder::~DVBTransponder()
{
}

void DVBTransponder::SetLogCallback(LogMessageCallback *callback)
{
	CAutoLock servicesLock(&m_servicesLock);

	LogMessageCaller::SetLogCallback(callback);

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for (; it != m_services.end(); it++ )
	{
		DVBTChannels_Service *pService = *it;
		pService->SetLogCallback(callback);
	}
}

DVBService *DVBTransponder::CreateService(long serviceId)
{
	DVBService *pService = new DVBService();
	pService->SetLogCallback(m_pLogCallback);
	pService->serviceId = serviceId;
	pService->transportStreamId = transportStreamId;
	m_services.push_back(pService);
	return pService;
}

void DVBTransponder::ParseNITDescriptors(unsigned char *buf, int remainingLength)
{
	while (remainingLength > 0)
	{
		unsigned char descriptorTag = buf[0];
		unsigned char descriptorLen = buf[1];

		if (!descriptorLen)
		{
			log.showf("descriptorTag == 0x%02x, len is 0\n", descriptorTag);
			break;
		}

		switch (descriptorTag)
		{
			case 0x40:
			{
				(log << "Found a network name descriptor\n").Write();
				strCopyA2W(networkName, (LPSTR)(buf+2), descriptorLen);
				(log << "  Network Name = " << networkName << "\n").Write();
				break;
			}

			case 0x43:
			{
				log.showf("Found a satellite delivery system descriptor\n");
				//LogMessageIndent indent(&log);
				//parse_satellite_delivery_system_descriptor(buf, tp);
				break;
			}

			case 0x44:
			{
				log.showf("Found a cable delivery system descriptor\n");
				//LogMessageIndent indent(&log);
				//parse_cable_delivery_system_descriptor(buf, tp);
				break;
			}

			case 0x5a:
			{
				log.showf("Found a terrestrial delivery system descriptor\n");
				LogMessageIndent indent(&log);
				ParseTerrestrialDeliverySystemDescriptor(buf);
				break;
			}

			case 0x62:
			{
				log.showf("Found a frequency list descriptor\n");
				LogMessageIndent indent(&log);
				ParseFrequencyListDescriptor(buf);
				break;
			}

			case 0x83:
			{
				log.showf("Found a terrestrial channel number descriptor\n");
				LogMessageIndent indent(&log);
				ParseTerrestrialChannelNumberDescriptor(buf);
				break;
			}

			default:
			{
				log.showf("Found an unknown descriptor 0x%02x\n", descriptorTag);
			}		
		};

		buf += (descriptorLen + 2);
		remainingLength -= (descriptorLen + 2);
	}
}

void DVBTransponder::ParseTerrestrialDeliverySystemDescriptor(unsigned char *buf)
{
	static const LPWSTR constellationString[] = { L"QPSK", L"QAM_16", L"QAM_64", L"QAM_AUTO" };
	static const LPWSTR coderateString[] = { L"FEC_1_2", L"FEC_2_3", L"FEC_3_4", L"FEC_5_6", L"FEC_7_8", L"FEC_AUTO" };
	static const LPWSTR transmissionModeString[] = { L"2K", L"8K" };

	frequencyInStream = (buf[2] << 24) | (buf[3] << 16);
	frequencyInStream |= (buf[4] << 8) | buf[5];
	frequencyInStream /= 100;
	log.showf("Frequency %i\n", frequencyInStream);

	bandwidth = 8 - ((buf[6] >> 5) & 0x3);
	log.showf("Bandwidth %i\n", bandwidth);

	otherFrequencyFlag = (buf[8] & 0x01);
	if (otherFrequencyFlag)
		(log << "Other frequency flags set\n").Write();

	long constellation = (buf[7] >> 6) & 0x3;
	long coderateHP = buf[7] & 0x7;
	long coderateLP = (buf[8] >> 5) & 0x7;
	long transmissionMode = (buf[8] & 0x2);

	if (constellation > 3)
		constellation = 3;
	if (coderateHP > 5)
		coderateHP = 5;
	if (coderateLP > 5)
		coderateLP = 5;
	if (transmissionMode > 1)
		transmissionMode = 1;

	(log << constellationString[constellation] << "    "
		 << "Transmission mode " << transmissionModeString[transmissionMode] << "\n").Write();
	(log << "HP - " << coderateString[coderateHP] << "    "
		 << "LP - " << coderateString[coderateLP] << "\n").Write();

}

void DVBTransponder::ParseFrequencyListDescriptor(unsigned char *buf)
{
	long otherFrequencyCount = (buf[1] - 1) / 4;
	if (otherFrequencyCount < 1 || (buf[2] & 0x03) != 3)
	{
		return;
	}

	otherFrequencys.clear();

	buf += 3;
	for (int i=0; i < otherFrequencyCount; i++)
	{
		long otherFrequency = (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
		otherFrequency /= 100;
		log.showf("Alternate Frequency %i\n", otherFrequency);
		otherFrequencys.push_back(otherFrequency);
		buf += 4;
	}
}

void DVBTransponder::ParseTerrestrialChannelNumberDescriptor(unsigned char *buf)
{
	// 32 bits per record
	long recordCount = buf[1] / 4;
	if (recordCount < 1)
		return;

	// desc id, desc len, (service id, service number)
	buf += 2;
	for (long i=0; i < recordCount; i++)
	{
		long serviceId = (buf[0]<<8)|(buf[1]&0xff);
		long channelNum = (buf[2]&0x03<<8)|(buf[3]&0xff);
		log.showf("Service ID 0x%x has channel number %d\n", serviceId, channelNum);

		//Might need to loop here for other transponders
		DVBService *pService;
		pService = (DVBService *)FindServiceByServiceId(serviceId);
		if (!pService)
			pService = CreateService(serviceId);

		if (pService)
			pService->logicalChannelNumber = channelNum;
		
		buf += 4;
	}
}

//////////////////////////////////////////////////////////////////////
// TransponderList
//////////////////////////////////////////////////////////////////////

HRESULT DVBTransponderList::Clear()
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for ( ; it != m_networks.end() ; it++ )
	{
		DVBTransponder *pTransponder = (DVBTransponder *)*it;
		delete pTransponder;
	}
	m_networks.clear();

	return S_OK;
}

DVBTChannels_Network *DVBTransponderList::CreateNetwork(long originalNetworkId, long transportStreamId, long networkId)
{
	DVBTransponder *pNetwork = new DVBTransponder();
	pNetwork->SetLogCallback(m_pLogCallback);
	pNetwork->originalNetworkId = originalNetworkId;
	pNetwork->transportStreamId = transportStreamId;
	pNetwork->networkId = networkId;

	CAutoLock lock(&m_networksLock);
	m_networks.push_back((DVBTChannels_Network *)pNetwork);
	return pNetwork;
}

//////////////////////////////////////////////////////////////////////
// DVBMpeg2DataParser
//////////////////////////////////////////////////////////////////////

DVBMpeg2DataParser::DVBMpeg2DataParser() :
	m_cBufferSize(4096)
{
	m_hScanningDoneEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	m_hScanningStopEvent[0] = CreateEvent(NULL, TRUE, FALSE, NULL);
	m_hScanningStopEvent[1] = CreateEvent(NULL, FALSE, FALSE, NULL);

	m_pDataBuffer = new unsigned char[m_cBufferSize];

	m_piMpeg2Data = NULL;
	m_pDVBTChannels = NULL;
	m_frequency = 0;
	m_bThreadStarted = FALSE;

	m_currentTransponder = NULL;

	m_logWriter.SetFilename(L"TSFileSource.log");
	log.AddCallback(&m_logWriter);
	log.ClearFile();
	(log << "-------------------------\nTSFileSource Scanning log\n").Write();
	(log << "Ver 2.2.0.9\n").Write();
//	log.LogVersionNumber();
	(log << "-------------------------\n").Write();

	m_networks.SetLogCallback(&m_logWriter);
}

DVBMpeg2DataParser::~DVBMpeg2DataParser()
{
	CloseHandle(m_hScanningDoneEvent);
	CloseHandle(m_hScanningStopEvent[0]);
	CloseHandle(m_hScanningStopEvent[1]);

	delete[] m_pDataBuffer;
}

void DVBMpeg2DataParser::SetDVBTChannels(DVBTChannels *pChannels)
{
	m_pDVBTChannels = pChannels;
}

void DVBMpeg2DataParser::SetFrequency(long frequency)
{
	m_frequency = frequency;
}

void DVBMpeg2DataParser::SetFilter(CComPtr <IBaseFilter> pBDASecTab)
{
	if (m_bThreadStarted)
		return;

	m_piMpeg2Data.Release();

	if (pBDASecTab != NULL)
	{
		pBDASecTab.QueryInterface(&m_piMpeg2Data);
	}
}

void DVBMpeg2DataParser::ReleaseFilter()
{
	if (m_bThreadStarted)
	{
		SetEvent(m_hScanningStopEvent[0]);
		WaitForThreadToFinish();
		m_piMpeg2Data.Release();
		ResetEvent(m_hScanningStopEvent[0]);
		m_bThreadStarted = FALSE;
	}
	else
		m_piMpeg2Data.Release();

	return;

}

//////////////////////////////////////////////////////////////////////
// Method just to kick off a separate thread
//////////////////////////////////////////////////////////////////////
HRESULT DVBMpeg2DataParser::StartScan()
{
	//Start a new thread to do the scanning so that it doesn't interfere with
	//the rest of DW operation.

	if (m_bThreadStarted)
		return S_FALSE;
	m_bThreadStarted = TRUE;

	unsigned long result = _beginthread(ParseMpeg2DataThread, 0, (void *) this);
	if (result == -1L)
	{
		m_bThreadStarted = FALSE;
		return E_FAIL;
	}
	return S_OK;
}

BOOL DVBMpeg2DataParser::IsScanRunning()
{
	return m_bThreadStarted;
}

HRESULT DVBMpeg2DataParser::EndScan()
{
	//Stop the thread 

	if (!m_bThreadStarted)
		return S_OK;

	SetEvent(m_hScanningStopEvent[0]);
	WaitForThreadToFinish();

	m_bThreadStarted = FALSE;
	return S_OK;
}

DWORD DVBMpeg2DataParser::WaitForThreadToFinish()
{
	DWORD result = WAIT_TIMEOUT;
	while ((result == WAIT_TIMEOUT) && (m_bThreadStarted))
	{
		result = WaitForSingleObject(m_hScanningDoneEvent, 1000);
	}
	return result;
}

//////////////////////////////////////////////////////////////////////
// The real work starts here
//////////////////////////////////////////////////////////////////////
void DVBMpeg2DataParser::StartScanThread()
{
	HRESULT hr;

//	BrakeThread Brake;

	m_logWriter.SetLogBufferLimit(100);

	try
	{
		ResetEvent(m_hScanningDoneEvent);

		(log << "\nStarting Scan Thread...\n").Write();

		long nScanCount = 0;

		while (m_piMpeg2Data)
		{
			// Make sure the filter is running
			if (!IsFilterRunning())
			{
				DWORD dwWait = WaitForSingleObject(m_hScanningStopEvent[0], 100);
				if (dwWait != WAIT_TIMEOUT)
					break;
				Sleep(100);
				continue;
			}

			nScanCount++;

			if (nScanCount == 1)
			{
//				g_pOSD->Data()->SetItem(L"Scanning", L"Scanning");
//				g_pTv->ShowOSDItem(L"Scanning");
			}
			if (nScanCount % 20 == 0)
			{
				// Clear the log file every so often so that it doesn't grow huge
				m_logWriter.Clear();
				(log << "-------------------------\nTSFileSource Scanning log\n").Write();
				(log << "Ver 2.2.0.9\n").Write();
//				log.LogVersionNumber();
				(log << "-------------------------\n").Write();
				(log << "\nStarting Scan " << nScanCount << "\n").Write();
			}

			// Initial tables to scan
			DVBSection *pPATSection;
			pPATSection = new DVBSection();
			pPATSection->Setup(0x00, 0x00, 0, 5);  // PAT

			// Scan tables
			hr = ReadSection(pPATSection);	// if PAT fails then there's no point doing NIT or SDT
			delete pPATSection;
/*
			if SUCCEEDED(hr)
			{
				DVBSection *pSection;
				pSection = new DVBSection();
				pSection->Setup(0x10, 0x40, 0, 30); // NIT
				m_waitingFilters.push_back(pSection);

				pSection = new DVBSection();
				pSection->Setup(0x11, 0x42, 0, 30);  // SDT
				m_waitingFilters.push_back(pSection);

				BOOL bKeepScanning = TRUE;
				while (m_waitingFilters.size())
				{
					pSection = m_waitingFilters.front();
					if (bKeepScanning)
					{
						hr = ReadSection(pSection);
						if FAILED(hr)
						{
							// Stop scanning, but keep looping to delete all the sections.
							bKeepScanning = FALSE;
						}
					}
					m_waitingFilters.erase(m_waitingFilters.begin());
					delete pSection;
				}
			}
*/
			if SUCCEEDED(hr)
				UpdateInformation();

			m_currentTransponder = NULL;
			m_networks.Clear();

//			if (g_pOSD)
//			{
//				if (g_pTv)
//					g_pTv->HideOSDItem(L"Scanning");
//				g_pOSD->Data()->SetItem(L"Scanning", L"");
//			}

			DWORD dwWait = WaitForSingleObject(m_hScanningStopEvent[0], 5000);
			if (dwWait != WAIT_TIMEOUT)
				break;
		}
	}
	catch (...)
	{
		(log << "Unhandled exception in DVBMpeg2DataParser::StartScanThread()\n");
	}
	m_bThreadStarted = FALSE;
	SetEvent(m_hScanningDoneEvent);

//	if (g_pOSD)
//	{
//		if (g_pTv)
//			g_pTv->HideOSDItem(L"Scanning");
//		g_pOSD->Data()->SetItem(L"Scanning", L"");
//	}
}

HRESULT DVBMpeg2DataParser::ReadSection(DVBSection *pSection)
{
	HRESULT hr;

	log.showf(_T("Looking on pid 0x%.4x for tableId 0x%.2x\n"), pSection->pid, pSection->tableId);
	LogMessageIndent indent(&log);

	CComPtr<ISectionList> piSectionList;
	
	if (!m_piMpeg2Data)
	{
		return (log << "filter dissapeared\n").Write(E_FAIL);
	}
	if (!IsFilterRunning())
	{
		return (log << "Filter was stopped\n").Write(E_FAIL);
	}

#define SECTION_STREAM
#ifdef SECTION_STREAM

	CComPtr <IMpeg2Stream> pStream;
	if FAILED(hr = m_piMpeg2Data->GetStreamOfSections((PID)pSection->pid, (TID)pSection->tableId, NULL, m_hScanningStopEvent[1], &pStream))
		return (log << "Failed to get stream of sections : " << hr << "\n").Write(hr);
	(log << "Got IMpeg2Stream\n").Write();

	MPEG_STREAM_BUFFER streamBuffer;

	ZeroMemory(&streamBuffer, sizeof(MPEG_STREAM_BUFFER));
	streamBuffer.dwDataBufferSize = m_cBufferSize;
	streamBuffer.pDataBuffer = (BYTE*)m_pDataBuffer;

	if FAILED(hr = pStream->SupplyDataBuffer(&streamBuffer))
	{
		return (log << "Failed to supply data buffer : " << hr << "\n").Write(hr);
	}
	(log << "Supplied data buffer\n").Write();

	DWORD dwWait = WaitForMultipleObjects(2, m_hScanningStopEvent, FALSE, (DWORD)(pSection->timeout*1000));
	(log << "Event or timeout triggered  " << (long)dwWait << "\n").Write();

	if (dwWait == WAIT_OBJECT_0)
	{
		return (log << "Filter is being released so we can stop waiting for sections\n").Write(E_FAIL);
	}
	if (dwWait == WAIT_OBJECT_0 + 1)
	{
		if FAILED(hr = streamBuffer.hr)
			return (log << "Section stream buffer fill failed : " << hr << "\n").Write(hr);

		ParseSection(pSection, m_pDataBuffer);
		return S_OK;
	}
	return (log << "Timeout getting table (pid=" << pSection->pid << ", tableId=" << pSection->tableId << "\n").Write(S_FALSE);

#else

	if SUCCEEDED(m_piMpeg2Data->GetTable(pSection->pid, pSection->tableId, NULL, pSection->timeout*1000, &piSectionList))
	{
		WORD cSections;
		if SUCCEEDED(hr = piSectionList->GetNumberOfSections(&cSections))
		{
			(log << "Section(s) found: " << (long)cSections << "\n").Write();
			for ( WORD i=0 ; i<cSections ; i++ )
			{
				SECTION* pstSection;
				DWORD ulSize;

				if SUCCEEDED(hr = piSectionList->GetSectionData(i, &ulSize, &pstSection))
				{
					ParseSection(pSection, (unsigned char *)pstSection);
					// Don't free pstSection. the filter does it automatically
				}
			}
			return S_OK;
		}
		else
		{
			return (log << "Error getting number of sections : " << hr << "\n").Write(S_FALSE);
		}
	}

	return (log << "Timeout getting table (pid=" << pSection->pid << ", tableId=" << pSection->tableId << "\n").Write(S_FALSE);

#endif

}

void DVBMpeg2DataParser::ParseSection(DVBSection *pSection, unsigned char *pBuffer)
{
	int tableId;
	BOOL sectionSyntaxIndicator;
	int sectionLength;
	int tableIdExt;
	int sectionVersionNumber;
	int sectionNumber;
	int lastSectionNumber;
	int i;

#ifdef SECTION_STREAM
	tableId = pBuffer[0];
#else
	SECTION *pSectionData = (SECTION *)pBuffer;
	tableId = pSectionData->TableId;
#endif

	if (tableId != pSection->tableId)
	{
		(log << "Invalid table id " << tableId << "  expecting " << pSection->tableId << "\n").Write();
		return;
	}

#ifdef SECTION_STREAM
	sectionSyntaxIndicator = ((pBuffer[1] & 0x80) != 0);
	// the section length value is the size starting immediately after the section length field, so we add 3 to get the total size of the section.
	sectionLength = (((pBuffer[1] & 0x0f) << 8) | pBuffer[2]) + 3;
#else
	MPEG_HEADER_BITS *pHeader = (MPEG_HEADER_BITS*)&pSectionData->Header.W;
	sectionSyntaxIndicator = pHeader->SectionSyntaxIndicator;
	// the section length value is the size starting immediately after the section length field, so we add 3 to get the total size of the section.
	sectionLength = pHeader->SectionLength + 3;
#endif

	// remove 4 bytes to ignore the CRC
	sectionLength -= 4;

	if (sectionSyntaxIndicator)
	{
#ifdef SECTION_STREAM
		tableIdExt = (pBuffer[3] << 8) | pBuffer[4];
		sectionVersionNumber = (pBuffer[5] >> 1) & 0x1f;
		sectionNumber = pBuffer[6];
		lastSectionNumber = pBuffer[7];
#else
		LONG_SECTION *pLong = (LONG_SECTION*)pSectionData;
		MPEG_HEADER_VERSION_BITS *pVersion = (MPEG_HEADER_VERSION_BITS*)&pLong->Version.B;

		tableIdExt = pLong->TableIdExtension;
		sectionVersionNumber = pVersion->VersionNumber;
		sectionNumber = pLong->SectionNumber;
		lastSectionNumber = pLong->LastSectionNumber;
#endif

		if (pSection->segmented && pSection->tableIdExt != -1 && pSection->tableIdExt != tableIdExt)
		{
			/* find or allocate actual section_buf matching tableIdExt */
			DVBSection *nextSegment = pSection->FindSegment(tableIdExt);
			if (nextSegment)
				pSection = nextSegment;
			else
				pSection = pSection->CreateNewSegment(tableIdExt, sectionVersionNumber);
		}

		if (pSection->sectionVersionNumber != sectionVersionNumber ||
			pSection->tableIdExt != tableIdExt)
		{
			if (pSection->sectionVersionNumber != -1 && pSection->tableIdExt != -1)
				log.showf("section version_number or tableIdExt changed "
						  "%d -> %d / %04x -> %04x\n",
						  pSection->sectionVersionNumber, sectionVersionNumber,
						  pSection->tableIdExt, tableIdExt);
			pSection->tableIdExt = tableIdExt;
			pSection->sectionVersionNumber = sectionVersionNumber;
			pSection->sectionFilterDone = 0;
			pSection->ResetSectionDone();
		}

		unsigned char *buf = pBuffer + 8;
		sectionLength -= 8;

		log.showf("pid 0x%02x tid 0x%02x tableIdExt 0x%04x, "
				  "%i/%i (version %i)\n",
				  pSection->pid, tableId, tableIdExt,
				  sectionNumber, lastSectionNumber,
				  sectionVersionNumber);
		LogMessageIndent indent(&log);

		if (!pSection->IsSectionDone(sectionNumber))
		{
			pSection->SetSectionDone(sectionNumber);

			switch (tableId)
			{
				case 0x00:
				{
					log.showf("PAT\n");
					LogMessageIndent indent(&log);
					ParsePAT(buf, sectionLength, tableIdExt);
					break;
				}
				case 0x02:
				{
					log.showf("PMT\n");
					LogMessageIndent indent(&log);
					ParsePMT(buf, sectionLength, tableIdExt);
					break;
				}
				case 0x41:
				{
					log.showf("////////////////////////////////////////////// NIT other\n");
					//LogMessageIndent indent(&log);
				}
				case 0x40:
				{
					log.showf("NIT (%s TS)\n", tableId == 0x40 ? "actual":"other");
					LogMessageIndent indent(&log);
					ParseNIT(buf, sectionLength, tableIdExt);
					break;
				}
				case 0x42:
				case 0x46:
				{
					log.showf("SDT (%s TS)\n", tableId == 0x42 ? "actual":"other");
					LogMessageIndent indent(&log);
					ParseSDT(buf, sectionLength, tableIdExt);
					break;
				}
				default:
					break;
			};

			for (i = 0; i <= lastSectionNumber; i++)
				if (!pSection->IsSectionDone(i))
					break;

			if (i > lastSectionNumber)
			{
				pSection->sectionFilterDone = 1;
				(log << "Done all sections for this table\n").Write();
			}
		}
		else
		{
			(log << "Section already done\n").Write();
		}

		if (pSection->segmented)
		{
			/* always wait for timeout; this is because we don't now how
			 * many segments there are
			 */
			return; //0;
		}
		else if (pSection->sectionFilterDone)
			return;
	}
}

void DVBMpeg2DataParser::ParsePAT(unsigned char *buf, int sectionLength, int transportStreamId)
{
	log.showf("Transport Stream Id 0x%04x\n", transportStreamId);

	m_currentTransponder = (DVBTransponder *)m_networks.FindNetworkByTSID(transportStreamId);	//i'm not expecting this to return anything, but just in case
	if (!m_currentTransponder)
		m_currentTransponder = (DVBTransponder *)m_networks.CreateNetwork(0, transportStreamId, 0);

	while (sectionLength >= 4)
	{
		DVBService *pService;
		int serviceId = (buf[0] << 8) | buf[1];
		int pmtPID = ((buf[2] & 0x1f) << 8) | buf[3];

		if (serviceId != 0)	/*  skip nit pid entry... */
		{
			log.showf("  Found service id 0x%04x with PMT 0x%04x\n", serviceId, pmtPID);

			/* SDT might have been parsed first... */
			pService = (DVBService *)m_currentTransponder->FindServiceByServiceId(serviceId);
			if (!pService)
				pService = m_currentTransponder->CreateService(serviceId);
			else
				log.showf("Existing service object found\n");

			pService->pmtPid = pmtPID;
			if (!pmtPID)
			{
				log.showf("Skipping adding filter. pmt pid is 0x00\n");
			}
			else
			{
				DVBSection *pSection = new DVBSection();
				pSection->Setup(pmtPID, 0x02, 0, 5);
				m_waitingFilters.push_back(pSection);
			}
		}
		else
		{
			log.showf("Skipping nit pid entry with serviceId 0x00\n");
		}

		buf += 4;
		sectionLength -= 4;
	};
}

void DVBMpeg2DataParser::ParsePMT(unsigned char *buf, int sectionLength, int serviceId)
{
	int programInfoLen;
	DVBService *pService;

	if (!m_currentTransponder)
	{
		(log << "No PAT found before scanning PMT for serviceId " << serviceId << "\n").Write();
		return;
	}

	pService = (DVBService *)m_currentTransponder->FindServiceByServiceId(serviceId);
	if (!pService)
	{
		log.showf("PMT for serivce_id 0x%04x was not in PAT\n", serviceId);
		return;
	}

	pService->pcrPid = ((buf[0] & 0x1f) << 8) | buf[1];

	log.showf("serviceId 0x%04x, transportStreamId=0x%04x\n", pService->serviceId, pService->transportStreamId);
	LogMessageIndent indent(&log);

	log.showf("PCR         : PID 0x%04x\n", pService->pcrPid);

	programInfoLen = ((buf[2] & 0x0f) << 8) | buf[3];

	buf += programInfoLen + 4;
	sectionLength -= programInfoLen + 4;

	while (sectionLength >= 5)
	{
		DVBStream *pStream = new DVBStream();

		pStream->Type = unknown;
		pStream->MpegStreamType = buf[0];
		pStream->PID = ((buf[1] & 0x1f) << 8) | buf[2];
		int streamInfoLen = ((buf[3] & 0x0f) << 8) | buf[4];
		buf += 5;
		sectionLength -= 5;

		switch (pStream->MpegStreamType)
		{
			case 0x01:
			case 0x02:
			{
				pStream->Type = video;
				log.showf("VIDEO       : PID 0x%04x\n", pStream->PID);
				break;
			}
			case 0x03:
			case 0x04:
			{
				pStream->Type = mp2;
				log.showf("MPEG2 AUDIO : PID 0x%04x\n", pStream->PID);
				//LogMessageIndent indent2(&log);
				//ParseLangDescriptor(buf, streamInfoLen, audio);
				break;
			}
			case 0x06:
			case 0x81:
			{
				if (FindDescriptor(0x56, buf, streamInfoLen, NULL, NULL))
				{
					pStream->Type = teletext;
					log.showf("TELETEXT    : PID 0x%04x\n", pStream->PID);
					break;
				}
				else if (FindDescriptor(0x59, buf, streamInfoLen, NULL, NULL))
				{
					/* Note: The subtitling descriptor can also signal
					 * teletext subtitling, but then the teletext descriptor
					 * will also be present; so we can be quite confident
					 * that we catch DVB subtitling streams only here, w/o
					 * parsing the descriptor. */
					pStream->Type = subtitle;
					log.showf("SUBTITLING  : PID 0x%04x\n", pStream->PID);
					break;
				}
				else if (FindDescriptor(0x6a, buf, streamInfoLen, NULL, NULL))
				{
					pStream->Type = ac3;
					log.showf("AC3 AUDIO   : PID 0x%04x\n", pStream->PID);
					//LogMessageIndent indent2(&log);
					//ParseLangDescriptor(buf, streamInfoLen, audio);
					break;
				}
				else if (FindDescriptor(0x73, buf, streamInfoLen, NULL, NULL))
				{
					pStream->Type = dts;
					log.showf("DTS AUDIO   : PID 0x%04x\n", pStream->PID);
					//LogMessageIndent indent2(&log);
					//ParseLangDescriptor(buf, streamInfoLen, audio);
					break;
				}
			}
			case 0x1B:
			{
				pStream->Type = h264;
				log.showf("H264 VIDEO       : PID 0x%04x\n", pStream->PID);
				break;
			}
			case 0x0F:
			{
				pStream->Type = aac;
				log.showf("AAC AUDIO       : PID 0x%04x\n", pStream->PID);
				break;
			}
      case 0x11:
			{
				pStream->Type = aac;
				log.showf("AAC AUDIO       : PID 0x%04x\n", pStream->PID);
				break;
			}
			case 0x10:
			{
				pStream->Type = mpeg4;
				log.showf("MPEG4 VIDEO       : PID 0x%04x\n", pStream->PID);
				break;
			}
			/* fall through */
			default:
			{
				log.showf("OTHER     : PID 0x%04x TYPE 0x%02x\n", pStream->PID, pStream->MpegStreamType);
			}
		};

		pService->m_streams.push_back(pStream);

		buf += streamInfoLen;
		sectionLength -= streamInfoLen;
	}

	if(pService->pmtPid)
	{
		DVBStream *pStream = new DVBStream();
		pStream->Type = pmt;
		pStream->MpegStreamType = 0;
		pStream->PID = pService->pmtPid;
		pService->m_streams.push_back(pStream);
	}

	if(pService->pcrPid)
	{
		DVBStream *pStream = new DVBStream();
		pStream->Type = pcr;
		pStream->MpegStreamType = 0;
		pStream->PID = pService->pcrPid;
		pService->m_streams.push_back(pStream);
	}

}

void DVBMpeg2DataParser::ParseNIT(unsigned char *buf, int sectionLength, int networkId)
{
	int descriptorsLoopLen = ((buf[0] & 0x0f) << 8) | buf[1];

	if (sectionLength < descriptorsLoopLen + 4)
	{
		log.showf("section too short: networkId == 0x%04x, "
				  "sectionLength == %i, descriptorsLoopLen == %i\n",
				  networkId, sectionLength, descriptorsLoopLen);
		return;
	}

	if ((m_currentTransponder->networkId != 0) && (m_currentTransponder->networkId != networkId))
		(log << "Warning: NIT is changing NID from " << m_currentTransponder->networkId << " to " << networkId << "\n").Write();
	m_currentTransponder->networkId = networkId;

	log.showf("TransportStreamId 0x%04x, NetworkId 0x%04x\n",
			  m_currentTransponder->transportStreamId, m_currentTransponder->networkId);

	m_currentTransponder->ParseNITDescriptors(buf + 2, descriptorsLoopLen);

	sectionLength -= descriptorsLoopLen + 4;
	buf += descriptorsLoopLen + 4;

	while (sectionLength > 6)
	{
		int transportStreamId = (buf[0] << 8) | buf[1];
		int originalNetworkId = (buf[2] << 8) | buf[3];

		DVBTransponder *pTransponder;
		pTransponder = (DVBTransponder *)m_networks.FindNetworkByTSID(transportStreamId);
		if (!pTransponder)
			pTransponder = (DVBTransponder *)m_networks.CreateNetwork(originalNetworkId, transportStreamId, networkId);

		if ((pTransponder->originalNetworkId != 0) && (pTransponder->originalNetworkId != originalNetworkId))
			(log << "Warning: NIT is changing ONID from " << pTransponder->originalNetworkId << " to " << originalNetworkId << "\n").Write();
		pTransponder->originalNetworkId = originalNetworkId;

		descriptorsLoopLen = ((buf[4] & 0x0f) << 8) | buf[5];

		if (sectionLength < descriptorsLoopLen + 4)
		{
			log.showf("section too short: transport_stream_id == 0x%04x, "
					  "sectionLength == %i, descriptorsLoopLen == %i\n",
					  pTransponder->transportStreamId, sectionLength, descriptorsLoopLen);
			break;
		}

		log.showf("TransportStreamId 0x%04x, OriginalNetworkId 0x%04x\n",
				  pTransponder->transportStreamId, pTransponder->originalNetworkId);

		pTransponder->ParseNITDescriptors(buf + 6, descriptorsLoopLen);

		sectionLength -= descriptorsLoopLen + 6;
		buf += descriptorsLoopLen + 6;
	};
}

void DVBMpeg2DataParser::ParseSDT(unsigned char *buf, int sectionLength, int transportStreamId)
{
	long originalNetworkId = (buf[0] << 8) | buf[1];

	DVBTransponder *pTransponder;
 	pTransponder = (DVBTransponder *)m_networks.FindNetworkByTSID(transportStreamId);	//i'm not expecting this to return anything, but just in case
	if (!pTransponder)
		pTransponder = (DVBTransponder *)m_networks.CreateNetwork(originalNetworkId, transportStreamId, 0);

	if ((pTransponder->originalNetworkId != 0) && (pTransponder->originalNetworkId != originalNetworkId))
		(log << "Warning: SDT is changing ONID from " << pTransponder->originalNetworkId << " to " << originalNetworkId << "\n").Write();
	pTransponder->originalNetworkId = originalNetworkId;

	log.showf("TransportStreamId 0x%04x, OriginalNetworkId 0x%04x\n",
		pTransponder->transportStreamId, pTransponder->originalNetworkId);

	buf += 3;	       /*  skip original network id + reserved field */

	while (sectionLength > 4)
	{
		int serviceId = (buf[0] << 8) | buf[1];
		int remainingLength = ((buf[3] & 0x0f) << 8) | buf[4];

		if (sectionLength < remainingLength || !remainingLength)
		{
			log.showf("section too short: service_id == 0x%02x, "
					  "section_length == %i, remainingLength == %i\n",
					  serviceId, sectionLength, remainingLength);
			return;
		}

		DVBService *pService;
		pService = (DVBService *)pTransponder->FindServiceByServiceId(serviceId);
		if (!pService)
			pService = pTransponder->CreateService(serviceId);

		pService->running = (enum running_mode)((buf[3] >> 5) & 0x7);
		pService->scrambled = (buf[3] >> 4) & 1;
		pService->transportStreamId = transportStreamId;

		pService->ParseSDTDescriptors(buf + 5, remainingLength - 5);

		sectionLength -= remainingLength + 5;
		buf += remainingLength + 5;
	};
}

void DVBMpeg2DataParser::UpdateInformation()
{
	if (m_currentTransponder)
	{
		m_currentTransponder->frequency = m_frequency;
		m_pDVBTChannels->UpdateNetwork(m_currentTransponder);
	}
}

BOOL DVBMpeg2DataParser::IsFilterRunning()
{
	HRESULT hr;

	CComQIPtr<IMediaFilter> piMediaFilter(m_piMpeg2Data);
	if (!piMediaFilter)
		return FALSE;

	FILTER_STATE filterState;
	if FAILED(hr = piMediaFilter->GetState(1000, &filterState))
		return FALSE;

	if (hr == VFW_S_STATE_INTERMEDIATE)
		return FALSE;

	return (filterState == State_Running);
}


//////////////////////////////////////////////////////////////////////
// These are functions that could be static
//////////////////////////////////////////////////////////////////////

int DVBMpeg2DataParser::FindDescriptor(__int8 tag, unsigned char *buf, int remainingLength, const unsigned char **desc, int *descLen)
{
	while (remainingLength > 0) {
		unsigned char descriptorTag = buf[0];
		unsigned char descriptorLen = buf[1];

		if (!descriptorLen)
		{
			log.showf("descriptorTag == 0x%02x, len is 0\n", descriptorTag);
			break;
		}

		if (tag == descriptorTag)
		{
			if (desc)
				*desc = buf + 2;
			if (descLen)
				*descLen = descriptorLen;
			return 1;
		}

		buf += (descriptorLen + 2);
		remainingLength -= (descriptorLen + 2);
	}
	return 0;
}

/*void DVBMpeg2DataParser::ParseLangDescriptor(unsigned char *buf, int remainingLength, DVBServiceAudio &audio)
{
	unsigned char *position = NULL;
	int length = 0;

	int success = FindDescriptor(0x0A, buf, remainingLength, (const unsigned char **)&position, &length);

	if (success)
	{
		if (length == 4)
		{
			log.showf("LANG=%.3s %d\n", position, position[3]);
			memcpy(audio.lang, position, 3);
			audio.lang[3] = 0;
			switch (position[3])
			{
			case 1:
				audio.audioType = DVBServiceAudio::AT_CLEAN_EFFECTS;
				break;
			case 2:
				audio.audioType = DVBServiceAudio::AT_HEADING_IMPARED;
				break;
			case 3:
				audio.audioType = DVBServiceAudio::AT_VISUALLY_IMPARED;
				break;
			default:
				audio.audioType = DVBServiceAudio::AT_DEFAULT;
				break;
			};
		}
		else
		{
			(log << "LANG descriptor is too " << ((length > 4) ? "long" : "short") << " (" << length << ")\n").Write();
		}
	}
	else
	{
		(log << "LANG descriptor not found\n").Write();
	}
}
*/

