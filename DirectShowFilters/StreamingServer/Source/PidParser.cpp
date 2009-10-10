/**
*  PidParser.cpp
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
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
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "PidParser.h"
#include "Global.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////


PidParser::PidParser(CSampleBuffer *pSampleBuffer, FileReader *pFileReader)
{
	m_pSampleBuffer = pSampleBuffer;
	m_pFileReader = pFileReader;
	m_PacketSize = 188; //Start with Transport packet size 
	m_ATSCFlag = false;
	m_NitPid = 0x10;
	m_NetworkID = 0; //NID store
	ZeroMemory(m_NetworkName, 128);
	m_ONetworkID = 0; //ONID store
	m_TStreamID = 0; //TSID store
	m_ProgramSID = 0; //SID store for prog search
	m_ProgPinMode = FALSE; //Set to Transport Stream Mode
	m_AsyncMode = FALSE; //Set for control by filter
	pids.Clear();
	m_buflen = 0;
	ZeroMemory(m_pDummy, 0x4000);
	ZeroMemory(m_shortdescr, 128);
	ZeroMemory(m_extenddescr, 600);
	m_pgmnumb = 0;
	filepos = 0;
	m_fileLenOffset = 0;
	m_fileEndOffset = 0;
	m_fileStartOffset = 0;
	m_FileStartPointer = 0;
	m_ParsingLock = FALSE;
}

PidParser::~PidParser()
{
	pidArray.Clear();
}

HRESULT PidParser::ParsePinMode(__int64 fileStartPointer)
{
	HRESULT hr = S_OK;

	if (m_pFileReader->IsFileInvalid())
	{
		return NOERROR;
	}

	//Store file pointer so we can reset it before leaving this method
	FileReader *pFileReader = m_pFileReader->CreateFileReader(); //new FileReader();
	char fileName[MAX_PATH];
	m_pFileReader->GetFileName((char*)&fileName);
	pFileReader->SetFileName(fileName);

	hr = pFileReader->OpenFile();
	if (FAILED(hr)){
		delete pFileReader;
		return VFW_E_INVALIDMEDIATYPE;
	}

	__int64 fileStart, filelength;
	pFileReader->GetFileSize(&fileStart, &filelength);

	//Check if we are locked out
	int count = 0;
	while (m_ParsingLock)
	{
		Sleep(100);
		count++;
		if (count > 10)
		{
			pFileReader->CloseFile();
			delete pFileReader;
			return S_FALSE;
		}
	}
	//Lock the parser
	m_ParsingLock = TRUE;

	// Access the sample's data buffer
	ULONG a = 0;
	ULONG ulDataLength = min((ULONG)filelength, MIN_FILE_SIZE*2);
	ULONG ulDataRead = 0;
	PBYTE pData = new BYTE[ulDataLength];
	pFileReader->setFilePointer(0, FILE_BEGIN);
	pFileReader->Read(pData, ulDataLength, &ulDataRead);
	ulDataLength = ulDataRead;

	if (ulDataLength > 2048*4)
	{
		m_PacketSize = 0x800;
		m_ProgPinMode = TRUE;
		int count = 0;
		a = ulDataLength - m_PacketSize;
		hr = S_OK;
		while (hr == S_OK && count < 2)
		{
			//search at the end of the file for sync bytes
			hr = FindSyncByte(pData, ulDataLength, &a, -1);
			if (hr == S_OK)
			{
				count++;
				a -= m_PacketSize;
			}
		}
		if (count < 2)
		{
			m_PacketSize = 188;
			m_ProgPinMode = FALSE;
			count = 0;
			a = ulDataLength - m_PacketSize;
			hr = S_OK;
			while (hr == S_OK && count < 10)
			{
				//search at the end of the file for sync bytes
				hr = FindSyncByte(pData, ulDataLength, &a, -1);
				if (hr == S_OK)
				{
					count++;
					a -= m_PacketSize;
				}

			}
			if (count < 10)
			{
				m_PacketSize = 188;
				m_ProgPinMode = FALSE;
			}
		}
	}

	//UnLock the parser
	m_ParsingLock = FALSE;
	delete[] pData;
	pFileReader->CloseFile();
	delete pFileReader;
	return hr;
}

HRESULT PidParser::ParseFromFile(__int64 fileStartPointer)
{
	HRESULT hr = S_OK;

	if (m_pFileReader->IsFileInvalid() && !m_pSampleBuffer->Count())
	{
		return NOERROR;
	}


	//Store file pointer so we can reset it before leaving this method
	FileReader *pFileReader = m_pFileReader->CreateFileReader(); //new FileReader();
	char fileName[MAX_PATH];
	m_pFileReader->GetFileName((char*)&fileName);
	pFileReader->SetFileName(fileName);

	hr = pFileReader->OpenFile();
	if (FAILED(hr)){
		delete pFileReader;
		return VFW_E_INVALIDMEDIATYPE;
	}


	//Check if we are locked out
	int count = 0;
	while (m_ParsingLock)
	{
		Sleep(100);
		count++;
		if (count > 10)
		{
			pFileReader->CloseFile();
			delete pFileReader;
			return S_FALSE;
		}
	}
	//Lock the parser
	m_ParsingLock = TRUE;

	// Access the sample's data buffer

	pids.Clear();
	
	m_ATSCFlag = false;
	m_NitPid = 0x10;
	m_NetworkID = 0; //NID store
	ZeroMemory(m_NetworkName, 128);
	m_ONetworkID = 0; //ONID store
	m_TStreamID = 0; //TSID store
	m_ProgramSID = 0; //SID store for prog search
	m_ProgPinMode = FALSE; //Set to Transport Stream Mode
	m_AsyncMode = FALSE; //Set for control by filter

	__int64 fileStart, filelength;
	pFileReader->GetFileSize(&fileStart, &filelength);
	ULONG ulDataLength = (ULONG)min((ULONG)MIN_FILE_SIZE*2, filelength);
	fileStartPointer = min((__int64)(filelength - (__int64)ulDataLength), fileStartPointer);
	m_FileStartPointer = max(get_StartOffset(), fileStartPointer);
	if (filelength < MIN_FILE_SIZE*2)
		pFileReader->setFilePointer(0, FILE_BEGIN);
	else
		pFileReader->setFilePointer(m_FileStartPointer, FILE_BEGIN);

	PBYTE pData = new BYTE[MIN_FILE_SIZE*2];
	ULONG ulDataRead = 0;
	if FAILED(m_pSampleBuffer->ReadSampleBuffer(pData, ulDataLength, &ulDataRead))
		pFileReader->Read(pData, ulDataLength, &ulDataRead);

	if (ulDataRead < 1)
	{
		//UnLock the parser and return false
		m_ParsingLock = FALSE;
		delete[] pData;
		pFileReader->CloseFile();
		delete pFileReader;
		return S_FALSE;
	}
	
	ulDataLength = ulDataRead;

	//check if we have something to give us the media format
	ULONG a = 0;
	m_PacketSize = 0x800;
	m_ProgPinMode = TRUE;
	count = 0;
	a = ulDataRead - m_PacketSize;
	hr = S_OK;
	while (hr == S_OK && count < 2)
	{
		//search back from the end of the file for sync bytes
		hr = FindSyncByte(pData, ulDataRead, &a, -1);
		if (hr == S_OK)
		{
			count++;
			a -= m_PacketSize;
		}
	}
	if (count < 2)
	{
		m_PacketSize = 188;
		m_ProgPinMode = FALSE;
		count = 0;
		a = ulDataRead - m_PacketSize;
		hr = S_OK;
		while (hr == S_OK && count < 10)
		{
			//search back from the end of the file for sync bytes
			hr = FindSyncByte(pData, ulDataRead, &a, -1);
			if (hr == S_OK)
			{
				count++;
				a -= m_PacketSize;
			}
		}
		if (count < 10)
		{
			m_PacketSize = 188;
			m_ProgPinMode = FALSE;
		}
	}

	a = 0;
	if (ulDataLength > 0)
	{
		//Clear all Pid arrays
		pidArray.Clear();
		BOOL patfound = false;
		//skip if we are in program mode
		if (!m_ProgPinMode)
		{
/*
			a = 0;
			hr = S_OK;
			while (hr == S_OK)
			{
				//search at the head of the file
				hr = FindSyncByte(pData, ulDataLength, &a, 1);
				if (hr == S_OK)
				{
					//parse next packet for the PAT
					if (ParsePAT(pData, ulDataLength, a) == S_OK)
					{
						// If second pass
						if (patfound)
						{
							break;
						}
						else
						{
							//Set to exit after next pat & Erase first occuracnce
							pidArray.Clear();
							m_TStreamID = 0; //TSID store
							patfound = true;
						}
					};
					a += m_PacketSize;
				}
			}

			//if no PAT found Scan for PMTs
			if (pidArray.Count() == 0)
			{
				a = 0;
				pids.sid = 0;
				pids.pmt = 0;
				hr = S_OK;

				//Scan buffer for any PMTs
				while (pids.pmt == 0x00 && hr == S_OK)
				{
					//search at the head of the file
					hr = FindSyncByte(pData, ulDataLength, &a, 1);
					if (hr == S_OK)
					{
						//parse next packet for the PMT
						if (ParsePMT(pData, ulDataLength, a) == S_OK)
						{
							//Check if PMT was already found
							BOOL pmtfound = false;
							for (int i = 0; i < pidArray.Count(); i++)
							{
								//Search PMT Array for the PMT & SID also
								if (pidArray[i].sid == pids.sid)
								{
									pmtfound = true;
									break;
								}
							}
							if (!pmtfound)
								AddPidArray();

							pids.sid = 0x00;
							pids.pmt = 0x00;
						}
						a += m_PacketSize;
					}
				}
			}

			//Loop through Programs found
			int i = 0;
			while (i < pidArray.Count())
			{
				//filepos = 0;
				a = 0;
				pids.Clear();
				hr = S_OK;

				int curr_pmt = pidArray[i].pmt;
				int pmtfound = 0;
				int curr_sid = pidArray[i].sid;

				while (pids.pmt != curr_pmt && hr == S_OK)
				{
					//search at the head of the file
					hr = FindSyncByte(pData, ulDataLength, &a, 1);
					if (hr != S_OK)
						break;
					//parse next packet for the PMT
					pids.Clear();
					if (ParsePMT(pData, ulDataLength, a) == S_OK)
					{
						//Check PMT & SID matches program
						if (pids.pmt == curr_pmt && pids.sid == curr_sid && pmtfound > 0)
						{
							//Search for valid A/V pids
							if (IsValidPMT(pData, ulDataLength) == S_OK)
							{
								//Set pcr & Store pids from PMT
								RefreshDuration(FALSE, pFileReader);
								SetPidArray(i);
								i++;
								break;
							}
							pids.pmt = 0;
							pids.sid = 0;
							break;
						}

						if (pids.pmt == curr_pmt && pids.sid == curr_sid)
						{
							pmtfound++;
							pids.pmt = 0;
							pids.sid = 0;
						}
					}
					a += m_PacketSize;
				};

				if (pids.pmt != curr_pmt || pids.sid != curr_sid || hr != S_OK) //Make sure we have a correct packet
					pidArray.RemoveAt(i);
			}
*/
			a = ulDataLength - m_PacketSize;
			hr = S_OK;
			while (hr == S_OK)
			{
				//search at the head of the file
				hr = FindSyncByte(pData, ulDataLength, &a, -1);
				if (hr == S_OK)
				{
					//parse next packet for the PAT
					if (ParsePAT(pData, ulDataLength, a) == S_OK)
					{
						break;
					}
					a -= m_PacketSize;
				}
			};

			//if no PAT found Scan for PMTs
			if (pidArray.Count() == 0)
			{
				a = ulDataLength - m_PacketSize;
				pids.sid = 0;
				pids.pmt = 0;
				hr = S_OK;

				//Scan buffer for any PMTs
				while (pids.pmt == 0x00 && hr == S_OK)
				{
					//search at the head of the file
					hr = FindSyncByte(pData, ulDataLength, &a, -1);
					if (hr == S_OK)
					{
						//parse next packet for the PMT
						if (ParsePMT(pData, ulDataLength, a) == S_OK)
						{
							//Check if PMT was already found
							BOOL pmtfound = false;
							for (int i = 0; i < pidArray.Count(); i++)
							{
								//Search PMT Array for the PMT & SID also
								if (pidArray[i].sid == pids.sid)
								{
									pmtfound = true;
									break;
								}
							}
							if (!pmtfound)
								AddPidArray();

							pids.sid = 0x00;
							pids.pmt = 0x00;
						}
						a -= m_PacketSize;
					}
				}
			}

			//Loop through Programs found
			int i = 0;
			while (i < pidArray.Count())
			{
				//filepos = 0;
				pids.Clear();
				hr = S_OK;

				int curr_pmt = pidArray[i].pmt;
				int pmtfound = 0;
				int curr_sid = pidArray[i].sid;

				a = ulDataLength - m_PacketSize;
				while (pids.pmt != curr_pmt && hr == S_OK)
				{
					//search at the head of the file
					hr = FindSyncByte(pData, ulDataLength, &a, -1);
					if (hr != S_OK)
						break;
					//parse next packet for the PMT
					pids.Clear();
					if (ParsePMT(pData, ulDataLength, a) == S_OK)
					{
						//Check PMT & SID matches program
						if (pids.pmt == curr_pmt && pids.sid == curr_sid && pmtfound > 0)
						{
							//Search for valid A/V pids
							if (IsValidPMT(pData, ulDataLength) == S_OK)
							{
								//Set pcr & Store pids from PMT
								RefreshDuration(FALSE, pFileReader);
								SetPidArray(i);
								i++;
								break;
							}
							pids.pmt = 0;
							pids.sid = 0;
							break;
						}

						if (pids.pmt == curr_pmt && pids.sid == curr_sid)
						{
							pmtfound++;
							pids.pmt = 0;
							pids.sid = 0;
						}
					}
					a -= m_PacketSize;
				};

				if (pids.pmt != curr_pmt || pids.sid != curr_sid || hr != S_OK) //Make sure we have a correct packet
					pidArray.RemoveAt(i);
			}
		}

		//Search for A/V pids if no NIT or valid PMT found.
		if (pidArray.Count() == 0)
		{
			//check if transport mode only
			if (!m_ProgPinMode)
			{
				//Search for any A/V Pids
				if (ACheckVAPids(pData, ulDataLength) == S_OK)
				{
					//Set the pcr to video or audio incase its included in packet
					USHORT pcrsave = pids.pcr;

					if (!pids.dur && pids.pcr) {
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.vid) {
						pids.pcr = pids.vid;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.h264) {
						pids.pcr = pids.h264;
						RefreshDuration(FALSE, pFileReader);
					}
						
					if (!pids.dur && pids.mpeg4) {
						pids.pcr = pids.mpeg4;
						RefreshDuration(FALSE, pFileReader);
					}
						
					if (!pids.dur && pids.aud) {
						pids.pcr = pids.aud;
						RefreshDuration(FALSE, pFileReader);
					}
							
					if (!pids.dur && pids.aud2) {
						pids.pcr = pids.aud2;
						RefreshDuration(FALSE, pFileReader);
					}
					
					if (!pids.dur && pids.ac3) {
						pids.pcr = pids.ac3;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.ac3_2) {
						pids.pcr = pids.ac3_2;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.aac) {
						pids.pcr = pids.aac;
						RefreshDuration(FALSE, pFileReader);
					}
							
					if (!pids.dur && pids.aac2) {
						pids.pcr = pids.aac2;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.dts) {
						pids.pcr = pids.dts;
						RefreshDuration(FALSE, pFileReader);
					}
							
					if (!pids.dur && pids.dts2) {
						pids.pcr = pids.dts2;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.txt) {
						pids.pcr = pids.txt;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.sub) {
						pids.pcr = pids.sub;
						RefreshDuration(FALSE, pFileReader);
					}

					if (!pids.dur && pids.vid) {
						pids.pcr = pids.vid;
						pids.opcr = pids.pcr;
						RefreshDuration(FALSE, pFileReader);
					}

					// restore pcr pid if no matches with A/V pids
					if (!pids.dur) {
						//set fake duration if needed
						pids.pcr = pcrsave;
						RefreshDuration(FALSE, pFileReader);
					}
					else
						AddPidArray();
				}
			}

			if (!pidArray.Count())
				pids.Clear();

			// If nothing then check for Program Pin Mode
			if ((pids.vid | pids.h264 | pids.mpeg4 | pids.sub 
				| pids.aud | pids.txt | pids.ac3 | pids.aac 
				| pids.dts | pids.pcr | pids.opcr) == 0)
			{
				m_PacketSize = 0x800; //Set for 2048 block
				m_pFileReader->set_DelayMode(FALSE);//cold start
				pFileReader->set_DelayMode(FALSE);//cold start
//				m_pFileReader->set_DelayMode(TRUE);
				m_ProgPinMode = TRUE;

				//Search for any A/V Pids
				if (CheckVAStreams(pData, ulDataLength) == S_OK)
				{
					if (!(pids.vid | pids.h264 | pids.mpeg4 | pids.sub 
						 | pids.aud  | pids.txt  | pids.ac3 
						 | pids.aac | pids.dts)){

						m_PacketSize = 0x930; //Set for 2352 block
						if (CheckVAStreams(pData, ulDataLength) == S_OK)
						{
							if (!(pids.vid | pids.h264 | pids.mpeg4 | pids.sub 
								| pids.aud  | pids.txt  | pids.ac3 
								| pids.aac | pids.dts)) {
									m_PacketSize = 188;
									pFileReader->set_DelayMode(FALSE);
									m_pFileReader->set_DelayMode(FALSE);
									m_ProgPinMode = FALSE;
							}
						}
						else
							RefreshDuration(FALSE, pFileReader);
					}
					else
						RefreshDuration(FALSE, pFileReader);

					// Check if we found mpg header times 
					if (!pids.dur) {
						//set fake duration if needed
						RefreshDuration(FALSE, pFileReader);
					}
					else
						AddPidArray();
				}
			}
		}

		//Scan for missing durations & Fix
		for (int n = 0; n < pidArray.Count(); n++){
			//Search Duration Array for the first duration
			if (pidArray[n].dur > 1){
			//Search Duration Array for empty durations
				for (int x = 0; x < pidArray.Count(); x++){
					//If empty then fill with first found duration 
					if (pidArray[x].dur < 1)
						pidArray[x].dur = pidArray[n].dur;
				}
			}
		}
/*
		//Check for a ONID in file
		if (!m_ATSCFlag && !m_ProgPinMode)
			if (CheckONIDInFile(pFileReader) == S_OK)
			{
			}

		//Check for a NID in file
		if (!m_ATSCFlag && !m_ProgPinMode)
			if(CheckNIDInFile(pFileReader) != S_OK)
			{
			}
*/
		//Set the Program Number to beginning & load back pids
		m_pgmnumb = 0;
		if (pidArray.Count()) {

			pids.Clear();
			pids.CopyFrom(&pidArray[m_pgmnumb]);
		}

		if (pids.vid != 0 || pids.h264 != 0 || pids.mpeg4 != 0 
			|| pids.aud != 0 || pids.txt != 0 || pids.sub
			|| pids.ac3 != 0 || pids.aac != 0 || pids.pcr != 0
			|| pids.pcr != 0 || pids.dts)
		{
			hr = S_OK;
		}
		else
		{
			m_AsyncMode = TRUE; //Set IAsyncReader interface Active
			hr = S_FALSE;
		}
	}
	else
	{
		hr = E_FAIL;
	}

	//UnLock the parser
	m_ParsingLock = FALSE;
	delete[] pData;
	pFileReader->CloseFile();
	delete pFileReader;
	return hr;
}

HRESULT PidParser::RefreshPids()
{
	if (m_pFileReader->IsFileInvalid() && !m_pSampleBuffer->Count())
	{
		return NOERROR;
	}

//	CAutoLock parserlock(&m_ParserLock);
	__int64 fileStart, fileSize = 0;
	m_pFileReader->GetFileSize(&fileStart, &fileSize);
	__int64 filestartpointer = min((__int64)(fileSize - (__int64)5000000), m_pFileReader->getFilePointer());
	filestartpointer = max(get_StartOffset(), filestartpointer);

	WORD readonly = 0;;
	m_pFileReader->get_ReadOnly(&readonly);
	//Check if file is being recorded
	if(fileSize < MIN_FILE_SIZE && readonly)
	{
		int count = 0;
		__int64 fileSizeSave = fileSize;
		while (fileSize < MIN_FILE_SIZE*10 && count < 20)
		{
			m_pFileReader->GetFileSize(&fileStart, &fileSize);
			while (!m_pSampleBuffer->Count() && (fileSize < fileSizeSave + MIN_FILE_SIZE) && (count < 20))
			{
				Sleep(100);
				count++;
				m_pFileReader->GetFileSize(&fileStart, &fileSize);
			}
			count++;
			m_pFileReader->GetFileSize(&fileStart, &fileSize);
			fileSizeSave = fileSize;
			ParseFromFile(filestartpointer); 
			if (m_ONetworkID > 0 && m_NetworkID > 0 || m_ProgPinMode) //cold start
				return S_OK;
		}
	}
	else
	{
		ParseFromFile(filestartpointer);
		if (m_ONetworkID > 0 && m_NetworkID > 0 && m_NetworkID > 0 || m_ProgPinMode) //cold start
			return S_OK;
	}
	return S_FALSE;
}

HRESULT PidParser::RefreshDuration(BOOL bStoreInArray, FileReader *pFileReader)
{
	__int64 fileStart, filelength;
	pFileReader->GetFileSize(&fileStart, &filelength);

//*********************************************************************************************
//Old Capture format Additions

	if (!pids.pcr && !m_ProgPinMode) {
		//Set our fake duration
		__int64 calcDuration = 1000000;
		if((__int64)((__int64)pids.bitrate / (__int64)8000))
			__int64 calcDuration = (REFERENCE_TIME)(filelength / (__int64)((__int64)pids.bitrate / (__int64)8000));

		pids.dur = (REFERENCE_TIME)(calcDuration * (__int64)10000);
		
		//Refresh all the sub program durations in the pid array
		if (bStoreInArray) {
			for (int i = 0; i < pidArray.Count(); i++)
			{
				pidArray[i].dur = pids.dur;
			}
		}
//PrintTime(TEXT("RefreshDuration1"), pids.dur, 10000);
		return S_OK;
	}
//*********************************************************************************************

	__int64 originalFilePointer = pFileReader->getFilePointer();

	//check file duration the easy way
	if (filelength < MIN_FILE_SIZE)
	{
		pids.start = GetPCRFromFile(pFileReader, 1);
		pFileReader->setFilePointer(0, FILE_BEGIN);
		pids.end = GetPCRFromFile(pFileReader, -1);
		pids.dur = (REFERENCE_TIME)((pids.end - pids.start)/9) * 1000;
	}
	else
		pids.dur = GetFileDuration(&pids, pFileReader);

	//Refresh all the sub program durations in the pid array
	if (bStoreInArray)
		for (int i = 0; i < pidArray.Count(); i++)
		{
			pidArray[i].dur = pids.dur;
		}

	pFileReader->setFilePointer(originalFilePointer, FILE_BEGIN);
//PrintTime(TEXT("RefreshDuration2"), pids.dur, 10000);

	return S_OK;
}


HRESULT PidParser::FindSyncByte(PBYTE pbData, ULONG ulDataLength, ULONG* a, int step)
{
	//look for Program Pin Mode
	if (m_ProgPinMode)
	{
		//Look for Mpeg Program Pack headers
		while ((*a >= 0) && (*a < ulDataLength))
		{
			if ((ULONG)((0xFF&pbData[*a])<<24
					| (0xFF&pbData[*a+1])<<16
					| (0xFF&pbData[*a+2])<<8
					| (0xFF&pbData[*a+3])) == 0x1BA)
			{
				if (*a+m_PacketSize < ulDataLength)
				{
					if ((ULONG)((0xFF&pbData[*a + m_PacketSize])<<24
						| (0xFF&pbData[*a+1 + m_PacketSize])<<16
						| (0xFF&pbData[*a+2 + m_PacketSize])<<8
						| (0xFF&pbData[*a+3 + m_PacketSize])) == 0x1BA)
						return S_OK;
				}
				else
				{
					if (step > 0)
						return E_FAIL;

					if (*a-m_PacketSize > 0)
					{
						if ((ULONG)((0xFF&pbData[*a - m_PacketSize])<<24
						| (0xFF&pbData[*a+1 - m_PacketSize])<<16
						| (0xFF&pbData[*a+2 - m_PacketSize])<<8
						| (0xFF&pbData[*a+3 - m_PacketSize])) == 0x1BA)
							return S_OK;
					}
				}
			}
			*a += step;
		}
		return E_FAIL;
	}

	//Set for Transport Pin Mode
	while ((*a >= 0) && (*a < ulDataLength))
	{
		if (pbData[*a] == 0x47)
		{
			if (*a+m_PacketSize < ulDataLength)
			{
				if (pbData[*a+m_PacketSize] == 0x47)
					return S_OK;
			}
			else
			{
				if (step > 0)
					return E_FAIL;

				if (*a-m_PacketSize > 0)
				{
					if (pbData[*a-m_PacketSize] == 0x47)
						return S_OK;
				}
			}
		}
		*a += step;
	}
	return E_FAIL;
}

HRESULT PidParser::ParsePAT(PBYTE pData, ULONG lDataLength, long pos)
{
	HRESULT hr = S_FALSE;

	int sectionLen;

	if ( (0x30 & pData[pos + 3]) == 0x10 && pData[pos + 4] == 0 &&
		  pData[pos + 5] == 0 && (0xf0 & pData[pos + 6]) == 0xB0 ) {

		if ((((0x1F & pData[pos + 1]) << 8)|(0xFF & pData[pos + 2])) == 0x00){

			sectionLen = (WORD)(((0x0F & pData[pos + 6]) << 8) | (0xFF & pData[pos + 7]));
			m_TStreamID = ((0xFF & pData[pos + 8]) << 8) | (0xFF & pData[pos + 9]); //Get TSID Pid

			for (long b = pos + 7 + 5 ; b < pos + sectionLen + 7 - 4 ; b = b + 4) //less 4 crc bytes
			{
				//Get Program value and skip if nit pid
				if ((((0xFF & pData[b + 1]) << 8) | (0xFF & pData[b + 2])) == 0)
				{
					m_NitPid = (WORD)(0x1F & pData[b + 3]) << 8 | (0xFF & pData[b + 4]);
				}
				else if (((0xe0 & pData[b + 3]) == 0xe0) && ((pData[b + 3]) != 0xff))
				{
					PidInfo *newPidInfo = new PidInfo();

					newPidInfo->sid =
						(WORD)(0xFF & pData[b + 1]) << 8 | (0xFF & pData[b + 2]);

					newPidInfo->pmt =
						(WORD)(0x1F & pData[b + 3]) << 8 | (0xFF & pData[b + 4]);

					pidArray.Add(newPidInfo);
				}
			}

			//If no Program PMT Info as with an ATSC
			if (pidArray.Count() != 0)
			{
				//Set flag to enable searching for NID
				m_ATSCFlag = false;
				return S_OK;
			}
			//Set flag to disable searching for NID
			m_ATSCFlag = true;
		}
	}
	return hr;
}

HRESULT PidParser::ParsePMT(PBYTE pData, ULONG ulDataLength, long pos)
{
	WORD pid;
	WORD channeloffset;
	WORD EsDescLen;
	WORD StreamType;
	WORD privatepid = 0;
	WORD sectionLen = 0;

	if ((0x30&pData[pos+3])==0x10 && pData[pos+4] == 0 && pData[pos+5] == 2 && (0xf0&pData[pos+6])==0xb0)
	{
		pids.pmt      = (WORD)(0x1F & pData[pos+1 ])<<8 | (0xFF & pData[pos+2 ]);
		pids.pcr      = (WORD)(0x1F & pData[pos+13])<<8 | (0xFF & pData[pos+14]);
		pids.sid      = (WORD)(0xFF & pData[pos+8 ])<<8 | (0xFF & pData[pos+9 ]);
		pids.chnumb    = pids.sid; //We do this to make up a channel number incase we don't parse the correct one later
		sectionLen	  = (WORD)min(182, (WORD)((0x0F & pData[pos+6])<<8 | (0xFF & pData[pos+7])));
		channeloffset = (WORD)(0x0F & pData[pos+15])<<8 | (0xFF & pData[pos+16]);

		for (long b=17+channeloffset+pos; b<pos+sectionLen; b=b+5)
		{
			if ( (0xe0&pData[b+1]) == 0xe0 )
			{
				pid = (WORD)(0x1F & pData[b+1])<<8 | (0xFF & pData[b+2]);
				EsDescLen = (WORD)(0x0F&pData[b+3]<<8 | 0xFF&pData[b+4]);

				StreamType = (WORD)(0xFF&pData[b]);

				if (StreamType == 0x02)
				{
					pids.vid = pid;
				}

				if (StreamType == 0x1b)
				{
					pids.h264 = pid;
				}

				if (StreamType == 0x10)
				{
					pids.mpeg4 = pid;
				}

				if ((StreamType == 0x03) || (StreamType == 0x04))
				{
					if (pids.aud != 0)
						pids.aud2 = pid;
					else
						pids.aud = pid;
				}

				if (StreamType == 0x06)
				{
					if (CheckEsDescriptorForDTS(pData, ulDataLength, b + 5, b + 5 + EsDescLen))
					{
						if (pids.dts == 0)
							pids.dts = pid;
						else
							pids.dts2 = pid;// If already have DTS then get next.
					}
					else if (CheckEsDescriptorForSubtitle(pData, ulDataLength, b + 5, b + 5 + EsDescLen))
						pids.sub = pid;
					else if (CheckEsDescriptorForAC3(pData, ulDataLength, b + 5, b + 5 + EsDescLen))
					{
						if (pids.ac3 == 0)
							pids.ac3 = pid;
						else
							pids.ac3_2 = pid;// If already have AC3 then get next.
					}
					else if (CheckEsDescriptorForTeletext(pData, ulDataLength, b + 5, b + 5 + EsDescLen))
						pids.txt = pid;
					else
					{
						//This could be a bid dodgy. What if there is an ac3 or txt in a future loop?
						if (pids.ac3 == 0 && pids.txt != 0)
						{
							pids.ac3 = pid;
						}
						else if (pids.ac3 != 0 && pids.txt == 0)
						{
							pids.txt = pid;
						}
					}
				}

				if (StreamType == 0x81 || StreamType == 0x83 || StreamType == 0x85 || StreamType == 0x8a)
				{
					if (pids.ac3 == 0)
						pids.ac3 = pid;
					else
						pids.ac3_2 = pid;// If already have AC3 then get next.
				}

//				if (StreamType == 0x0b)
//					if (pids.txt == 0)
//						pids.txt = pid;

				if (StreamType == 0x0b) //Subtitle
					if (pids.sub == 0)
						pids.sub = pid;

				if (StreamType == 0x0f) // AAC
					if (pids.aac == 0)
						pids.aac = pid;
					else
						pids.aac2 = pid;
/*
				if (StreamType >= 0x88 && StreamType <= 0x8a) // DTS
					if (pids.dts == 0)
						pids.dts = pid;
					else
						pids.dts2 = pid;
*/
				b+=EsDescLen;
			}
		}
		return S_OK;
	}
	else
	{
		return S_FALSE;
	}
}

HRESULT PidParser::CheckForPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, int pos, REFERENCE_TIME* pcrtime)
{
	if (m_ProgPinMode)
	{
		// Get PTS
		if((0xC0&pData[pos+4]) == 0x40) {

			*pcrtime =	((REFERENCE_TIME)(0x38 & pData[pos+4])<<27) |
						((REFERENCE_TIME)(0x03 & pData[pos+4])<<28) |
						((REFERENCE_TIME)(0xFF & pData[pos+5])<<20) |
						((REFERENCE_TIME)(0xF8 & pData[pos+6])<<12) |
						((REFERENCE_TIME)(0x03 & pData[pos+6])<<13) |
						((REFERENCE_TIME)(0xFF & pData[pos+7])<<5)  |
						((REFERENCE_TIME)(0xF8 & pData[pos+8])>>3);
			return S_OK;
		}
		return S_FALSE;
	}

	if ((WORD)((0x1F & pData[pos+1])<<8 | (0xFF & pData[pos+2])) == pPids->pcr)
	{
		WORD pcrmask = 0x10;
		if ((pData[pos+3] & 0x30) == 30)
			pcrmask = 0xff;

		if (((pData[pos+3] & 0x30) >= 0x20)
			&& ((pData[pos+4] & 0x11) != 0x00)
			&& ((pData[pos+5] & pcrmask) == 0x10))
		{
				*pcrtime =	((REFERENCE_TIME)(0xFF & pData[pos+6])<<25) |
							((REFERENCE_TIME)(0xFF & pData[pos+7])<<17) |
							((REFERENCE_TIME)(0xFF & pData[pos+8])<<9)  |
							((REFERENCE_TIME)(0xFF & pData[pos+9])<<1)  |
							((REFERENCE_TIME)(0x80 & pData[pos+10])>>7);

				//A true PCR has been found so drop the other OPCR search
				pPids->opcr = 0;

				return S_OK;
		};
	}

//*********************************************************************************************
//Old Capture format Additions


	if ((WORD)((0x1F & pData[pos+1])<<8 | (0xFF & pData[pos+2])) == pPids->opcr &&
		(WORD)(pData[pos+1]&0xF0) == 0x40)
	{
		if (((pData[pos+3] & 0x10) == 0x10)
			&& ((pData[pos+4]) == 0x00)
			&& ((pData[pos+5]) == 0x00)
			&& ((pData[pos+6]) == 0x01)
			&& ((pData[pos+7]) == 0xEA)
			&& ((pData[pos+8] | pData[pos+9]) == 0x00)
			&& ((pData[pos+10] & 0xC0) == 0x80)
			&& ((pData[pos+11] & 0xC0) == 0x80 || (pData[pos+11] & 0xC0) == 0xC0)
			&& (pData[pos+12] >= 0x05)
			)
		{
			// Get PTS
			if((0xF0 & pData[pos+13]) == 0x10 || (0xF0 & pData[pos+13]) == 0x30) {

				*pcrtime =	((REFERENCE_TIME)(0x0C & pData[pos+13])<<29) |
							((REFERENCE_TIME)(0xFF & pData[pos+14])<<22) |
							((REFERENCE_TIME)(0xFE & pData[pos+15])<<14) |
							((REFERENCE_TIME)(0xFF & pData[pos+16])<<7)  |
							((REFERENCE_TIME)(0xFE & pData[pos+17])>>1);
				return S_OK;
			}
			// Get DTS
			if((0xF0 & pData[pos+18]) == 0x10) {

				*pcrtime =	((REFERENCE_TIME)(0x0C & pData[pos+18])<<29) |
							((REFERENCE_TIME)(0xFF & pData[pos+19])<<22) |
							((REFERENCE_TIME)(0xFE & pData[pos+20])<<14) |
							((REFERENCE_TIME)(0xFF & pData[pos+21])<<7)  |
							((REFERENCE_TIME)(0xFE & pData[pos+22])>>1);
				return S_OK;
			}
		};
	}

//*********************************************************************************************

	return S_FALSE;
}

//*********************************************************************************************
//Old Capture format Additions

HRESULT PidParser::CheckForOPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, int pos, REFERENCE_TIME* pcrtime)
{
	if (((WORD)((0x1F&pData[pos+1])<<8)|(0xFF&pData[pos+2])) == pPids->opcr
		&& (pData[pos+1]&0xF0) == 0x40)
	{
//		if (((pData[pos+3] & 0x1F) == 0x10)
		if (((pData[pos+3] & 0x10) == 0x10)
			&& ((pData[pos+4]) == 0x00)
			&& ((pData[pos+5]) == 0x00)
			&& ((pData[pos+6]) == 0x01)
			&& ((pData[pos+7]) == 0xEA)
			&& ((pData[pos+8] | pData[pos+9]) == 0x00)
			&& ((pData[pos+10] & 0xC0) == 0x80)
			&& ((pData[pos+11] & 0xC0))// == 0x80 || (pData[pos+11] & 0xC0) == 0xC0)
			&& (pData[pos+12] >= 0x05)
			)
		{
			// Get PTS
			if((0xF0 & pData[pos+13]) == 0x10 || (0xF0 & pData[pos+13]) == 0x30) {

				*pcrtime =	((REFERENCE_TIME)(0x0C & pData[pos+13])<<29) |
							((REFERENCE_TIME)(0xFF & pData[pos+14])<<22) |
							((REFERENCE_TIME)(0xFE & pData[pos+15])<<14) |
							((REFERENCE_TIME)(0xFF & pData[pos+16])<<7)  |
							((REFERENCE_TIME)(0xFE & pData[pos+17])>>1);
				return S_OK;
			}
			// Get DTS
			if((0xF0 & pData[pos+18]) == 0x10) {


				*pcrtime =	((REFERENCE_TIME)(0x0C & pData[pos+18])<<29) |
							((REFERENCE_TIME)(0xFF & pData[pos+19])<<22) |
							((REFERENCE_TIME)(0xFE & pData[pos+20])<<14) |
							((REFERENCE_TIME)(0xFF & pData[pos+21])<<7)  |
							((REFERENCE_TIME)(0xFE & pData[pos+22])>>1);
				return S_OK;
			}
		};
	}
	return S_FALSE;
}

HRESULT PidParser::FindNextOPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos, int step)
{
	HRESULT hr = S_OK;

	*pcrtime = 0;

	while( *pcrtime == 0 && hr == S_OK)
	{
		hr = FindSyncByte(pData, ulDataLength, pulPos, step);
		if (FAILED(hr))
			return hr;

		if (S_FALSE == CheckForOPCR(pData, ulDataLength, pPids, *pulPos, pcrtime))
		{
			*pulPos = *pulPos + (step * m_PacketSize);
		}
		else
		{
			return S_OK;
		}
	}
	return E_FAIL;
}

//*********************************************************************************************

BOOL PidParser::CheckEsDescriptorForAC3(PBYTE pData, ULONG ulDataLength, int pos, int lastpos)
{
	WORD DescTag;
	while (pos < lastpos)
	{
		DescTag = (0xFF & pData[pos]);
		if (DescTag == 0x6a) return TRUE;
		pos += (int)(0xFF & pData[pos+1]) + 2;
	}
	return FALSE;
}

BOOL PidParser::CheckEsDescriptorForDTS(PBYTE pData, ULONG ulDataLength, int pos, int lastpos)
{
	WORD DescTag;
	while (pos < lastpos)
	{
		DescTag = (0xFF & pData[pos]);
		if (DescTag == 0x73) return TRUE;
		pos += (int)(0xFF & pData[pos+1]) + 2;
	}
	return FALSE;
}

BOOL PidParser::CheckEsDescriptorForTeletext(PBYTE pData, ULONG ulDataLength, int pos, int lastpos)
{
	WORD DescTag;
	while (pos < lastpos)
	{
		DescTag = (0xFF & pData[pos]);
		if (DescTag == 0x56) return TRUE;

		pos += (int)(0xFF & pData[pos+1]) + 2;
	}
	return FALSE;
}

BOOL PidParser::CheckEsDescriptorForSubtitle(PBYTE pData, ULONG ulDataLength, int pos, int lastpos)
{
	WORD DescTag;
	while (pos < lastpos)
	{
		DescTag = (0xFF & pData[pos]);
		if (DescTag == 0x59) return TRUE;

		pos += (int)(0xFF & pData[pos+1]) + 2;
	}
	return FALSE;
}

HRESULT PidParser::IsValidPMT(PBYTE pData, ULONG ulDataLength)
{
	HRESULT hr = S_FALSE;

	//exit if no a/v pids to find
	if (pids.aud + pids.vid + pids.h264 + pids.mpeg4
		+ pids.ac3 + pids.aac + pids.dts + pids.txt + pids.sub == 0)
		return hr;

	ULONG a, b;
	WORD pid;
	int addlength, addfield;
	WORD error, start;
	DWORD psiID, pesID;
	a = 0;
	hr = S_OK;

	while (hr == S_OK)
	{
		hr = FindSyncByte(pData, ulDataLength, &a, 1);
		if (hr == S_OK)
		{
			b = a;
			error = 0x80&pData[b+1];
			start = 0x40&pData[b+1];
			addfield = (0x30 & pData[b+3])>>4;
			if (addfield > 1) {

				addlength = (0xff & pData[b+4]) + 1;
			} else {

				addlength = 0;
			}

			if (start == 0x40) {
				pesID = ( (255&pData[b+4+addlength])<<24
						| (255&pData[b+5+addlength])<<16
						| (255&pData[b+6+addlength])<<8
						| (255&pData[b+7+addlength]) );
				psiID = pesID>>16;

				pid = ((0x1F & pData[b+1])<<8 | (0xFF & pData[b+2]));

				if (pid && ((pid == pids.vid) || (pid == pids.h264)))
				{
					//if ((0xFF0 & pesID) == 0x1e0)		// I'm not sure that is is necessary. H264 streams don't adhere to this
					{
						return S_OK;
					}
				}

				if (pid && ((pid == pids.aud) || (pid == pids.ac3) || (pid == pids.sub)))
				{
					//if ((0xFF0 & pesID) == 0x1c0)
					{
						return S_OK;
					}
				}

				if (((0xFF0&pesID) == 0x1b0) && (pids.ac3 == pid) && pids.ac3) {
					return S_OK;
				};
			}
			a += m_PacketSize;
		}
	}
	return hr;
}

HRESULT PidParser::FindFirstPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos)
{
	*pulPos = 0;
	return FindNextPCR(pData, ulDataLength, pPids, pcrtime, pulPos, 1);
}

HRESULT PidParser::FindLastPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos)
{
	*pulPos = ulDataLength - m_PacketSize;
	return FindNextPCR(pData, ulDataLength, pPids, pcrtime, pulPos, -1);
}

HRESULT PidParser::FindNextPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos, int step)
{
	HRESULT hr = S_OK;

	*pcrtime = 0;

	while( *pcrtime == 0 && hr == S_OK)
	{
		hr = FindSyncByte(pData, ulDataLength, pulPos, step);
		if (FAILED(hr))
			return hr;

		if (S_FALSE == CheckForPCR(pData, ulDataLength, pPids, *pulPos, pcrtime))
		{
			*pulPos = *pulPos + (step * m_PacketSize);
		}
		else
		{
			return S_OK;
		}
	}
	return E_FAIL;
}

REFERENCE_TIME PidParser::GetPCRFromFile(FileReader *pFileReader, int step)
{
	if (step == 0)
		return 0;

	REFERENCE_TIME pcrtime = 0;

	ULONG lDataLength = MIN_FILE_SIZE;
	PBYTE pData = new BYTE[lDataLength];
	ULONG lDataRead = 0;

	pFileReader->Read(pData, lDataLength, &lDataRead, 0, (step > 0) ? FILE_BEGIN : FILE_END);
	if (lDataRead <= m_PacketSize){

		delete[] pData;
		return 0;
	}
	
	ULONG a = (step > 0) ? 0 : lDataRead-m_PacketSize;

	if (step > 0)
		FindFirstPCR(pData, lDataRead, &pids, &pcrtime, &a);
	else
		FindLastPCR(pData, lDataRead, &pids, &pcrtime, &a);

	delete[] pData;

	return pcrtime;
}

HRESULT PidParser::ACheckVAPids(PBYTE pData, ULONG ulDataLength)
{
	ULONG a, b;
	WORD pid;
	HRESULT hr;
	int addlength, addfield;
	WORD error, start;
	DWORD psiID, pesID;
	a = 0;

	hr = S_OK;
	while (hr == S_OK)
	{
		hr = FindSyncByte(pData, ulDataLength, &a, 1);
		if (hr == S_OK)
		{
			b = a;
			error = 0x80&pData[b+1];
			start = 0x40&pData[b+1];
			addfield = (0x30 & pData[b+3])>>4;
			if (addfield > 1) {

				addlength = (0xff & pData[b+4]) + 1;
			} else {

				addlength = 0;
			}

			if (start == 0x40 && pData[b+6+addlength] == 0x01) {

				pesID = ( (255&pData[b+4+addlength])<<24
						| (255&pData[b+5+addlength])<<16
						| (255&pData[b+6+addlength])<<8
						| (255&pData[b+7+addlength]) );

				psiID = pesID>>16;
				if (!psiID) {

					pid = ((0x1F & pData[b+1])<<8 | (0xFF & pData[b+2]));

					if (((0xFFFFFFF0&pesID) == 0x1e0) && (pids.vid == 0)) {
						pids.vid = pid;

					};

					if ((0xFFFFFFF0&pesID) == 0x1c0) {
						if (pids.aud == 0) {
							pids.aud = pid;
						} else {
							if (pids.aud != pid) {
								pids.aud2 = pid;
							}
						}
					};

					if ((0xFFFFFFF0&pesID) == 0x1b0 && (0xFFF&pesID) < 0x1BA) {
						if (pids.ac3 == 0) {
							pids.ac3 = pid;
						} else {
							if (pids.ac3 != pid) {
								pids.ac3_2 = pid;
							}
						}
					};
				}
			}
		}
		a += m_PacketSize;
	}
	return S_OK;
}

HRESULT PidParser::CheckVAStreams(PBYTE pData, ULONG ulDataLength)
{
	ULONG a, b;
	WORD pid;
	HRESULT hr;
	int addlength;
	WORD error, start;
	DWORD psiID, pesID;
	a = 0;

	hr = S_OK;
	while (hr == S_OK)
	{
		hr = FindSyncByte(pData, ulDataLength, &a, 1);
		if (hr == S_OK)
		{
			b = a;
			error = 0x80&pData[b+4];
			start = 0x40&pData[b+4];
			addlength = 14 + (0x07&pData[b+13]);

			if (start == 0x40 && pData[b+2+addlength] == 0x01) {

				pesID = ( (255&pData[b+addlength])<<24
						| (255&pData[b+1+addlength])<<16
						| (255&pData[b+2+addlength])<<8
						| (255&pData[b+3+addlength]));

				psiID = pesID>>16;
				if (!psiID) {

					pid = 0xFF&(WORD)pesID;

					if (((0xFF0&pesID) == 0x1e0) && (pids.vid == 0)) {
						pids.vid = pid;

					};

					if ((0xFFFFFFF0&pesID) == 0x1c0) {
						if (pids.aud == 0) {
							pids.aud = pid;
						} else {
							if (pids.aud != pid) {
								pids.aud2 = pid;
							}
						}
					};

					if ((0xFFFFFFF0&pesID) == 0x1b0
						&& (0xFFF&pesID) != 0x1BB
						&& (0xFFF&pesID) != 0x1BA) {
						if (pids.ac3 == 0) {
							pids.ac3 = pid;
						} else {
							if (pids.ac3 != pid) {
								pids.ac3_2 = pid;
							}
						}
					};
				}
			}
		}
		a += m_PacketSize;
	}
	return S_OK;
}



HRESULT PidParser::CheckNIDInFile(FileReader *pFileReader)
{

	HRESULT hr = S_FALSE;

	if (m_NetworkID == 0 && m_TStreamID !=0)
	{
		bool extPacket = false; //Start at first packet
		int sectLen = 0;
		m_buflen = 0;
		ULONG pos = 0;
		int iterations = 0;
		bool nitfound = false;

		ULONG ulDataLength = MIN_FILE_SIZE;
		ULONG ulDataRead = 0;
		PBYTE pData = new BYTE[ulDataLength];

		__int64 fileStart, filelength;
		pFileReader->GetFileSize(&fileStart, &filelength);
		pFileReader->setFilePointer(min((__int64)(filelength - (__int64)ulDataLength), m_FileStartPointer), FILE_BEGIN);
///////////////		pFileReader->setFilePointer(m_FileStartPointer, FILE_BEGIN);
		pFileReader->Read(pData, ulDataLength, &ulDataRead);

		hr = FindSyncByte(pData, ulDataLength, &pos, 1);

		while ((pos < ulDataLength) && (hr == S_OK) && (nitfound == false))
		{
			nitfound = CheckForNID(pData, pos, &extPacket, &sectLen);

			if ((iterations > 64) && (nitfound == false))
			{
				hr = S_FALSE;
			}
			else
			{
				if (nitfound == false)
				{
					pos = pos + m_PacketSize;
					if (pos >= MIN_FILE_SIZE)
					{
						ULONG ulBytesRead = 0;
						hr = pFileReader->Read(pData, MIN_FILE_SIZE, &ulBytesRead);

						iterations++;
						pos = 0;
						if (hr == S_OK)
						{
							hr = FindSyncByte(pData, MIN_FILE_SIZE, &pos, 1);
						}
					}
				}
			}
		}

		if (nitfound == true && m_buflen > 0)
		{
			//get network ID Number
			m_NetworkID = (0xFF & m_pDummy[8]) << 8 | (0xFF & m_pDummy[9]);
		
			//get network name
			int a = 17;
			int b = min(m_pDummy[16], 128);
			memcpy(m_NetworkName, m_pDummy + a, b);
			//get channel numbers
			for (int n = 0; n < pidArray.Count(); n++)
			{
			//find channel from sid's
				for (int i = 0 ; i < m_buflen; i++)
				{
					if (((m_pDummy[i]<<8)|m_pDummy[i+1]) == pidArray[n].sid
						&& ((0xFC & m_pDummy[i+2]) == 0xFC))
					{
						pidArray[n].chnumb = (int)((0x03 & m_pDummy[i+2])| m_pDummy[i+3]);
						break;
					}
				};
			}
		}
		delete[] pData;
	}
	if (m_NetworkID == 0)
		return S_FALSE;
	else
		return S_OK;
}

bool PidParser::CheckForNID(PBYTE pData, int pos, bool *extpacket, int *sectlen)
{
	//Return ok if we already found an NID or if we don't have a TSID since we don't have a PAT
	if (m_TStreamID ==0)
		return true;

	int b = pos; //Set pos for Start Packet ID
	int endpos = pos + m_PacketSize; //Set to exclude crc bytes

	//Test if we have an NID descriptor
	if (pData[pos + 4] == 0
		&& (*extpacket == false) // parse the first packet only 
		&& pData[pos + 5] == 0x40 
		&& (0xF0 & pData[pos + 6]) == 0xF0
		&& (((0x1F & pData[pos + 1]) << 8)|(0xFF & pData[pos + 2])) == m_NitPid //0x10
		&& (((0x1F & pData[pos + 11]) << 8)|(0xFF & pData[pos + 12])) == 0	//yes if we have the NID flag
		)
	{
		*sectlen =((0x0F & pData[pos + 6]) << 8)|(0xFF & pData[pos + 7]) + 4; //include CRC bytes

		 // test if next packet required 
		if (*sectlen > 176)
		{
			*extpacket = true; //set search for extended packet
		}
		else
		{
			*extpacket = false; // set for next packet

			//if no descriptor info
			if (*sectlen <= 0x0F + 4)
				*sectlen = 0;
		};

	}
	else if ((*extpacket == true) // second time past this test
			&& (0xF0 & pData[pos + 3]) == 0x10 
			&& (0xF0 & pData[pos + 1]) == 0x00 
			&& (((0x1F & pData[pos + 1]) << 8)|(0xFF & pData[pos + 2])) == m_NitPid //0x10
			)
	{
		b += 4; //set for extended packet ID
		endpos += 4; //set not to exclude crc bytes
	}
	else
		return false; //No Descriptor Found

	while(b < endpos) 
	{
		//Bufer overflow
		if (m_buflen > 0x3FFF || *sectlen < 1) 
			return true; // buffer over flow or end parsing
	 
		*sectlen = *sectlen - 1;
		m_pDummy[m_buflen] = pData[b];
		m_buflen++;
		b++;
	};
	return false;
}

HRESULT PidParser::CheckONIDInFile(FileReader *pFileReader)
{

	HRESULT hr = S_FALSE;

	if (m_ONetworkID == 0 && m_TStreamID !=0)
	{
		bool extPacket = false; //Start at first packet
		m_buflen = 0;
		int sectLen = 0;
		ULONG pos = 0;
		int iterations = 0;
		bool onitfound = false;

		ULONG ulDataLength = MIN_FILE_SIZE;
		ULONG ulDataRead = 0;
		PBYTE pData = new BYTE[ulDataLength];

		__int64 fileStart, filelength;
		pFileReader->GetFileSize(&fileStart, &filelength);
		pFileReader->setFilePointer(min((__int64)(filelength - (__int64)ulDataLength), m_FileStartPointer), FILE_BEGIN);
/////////////		pFileReader->setFilePointer(m_FileStartPointer, FILE_BEGIN);
		pFileReader->Read(pData, ulDataLength, &ulDataRead);

		hr = FindSyncByte(pData, ulDataLength, &pos, 1);

		while ((pos < ulDataLength) && (hr == S_OK) && (onitfound == false)) 
		{
			if (!onitfound)
			{
				onitfound = CheckForONID(pData, pos, &extPacket, &sectLen);
			}


			if ((iterations > 64) && (onitfound == false)) 
			{
				hr = S_FALSE;
			}
			else
			{
				if (onitfound == false)
				{
					pos = pos + m_PacketSize;
					if (pos >= MIN_FILE_SIZE)
					{
						ULONG ulBytesRead = 0;
						hr = pFileReader->Read(pData, MIN_FILE_SIZE, &ulBytesRead);

						iterations++;
						pos = 0;
						if (hr == S_OK)
						{
							hr = FindSyncByte(pData, MIN_FILE_SIZE, &pos, 1);
						}
					}
				}
			}
		}

		if (onitfound == true && m_buflen > 0)
		{
			//get Onetwork ID Number
			m_ONetworkID = (0xFF & m_pDummy[13]) << 8 | (0xFF & m_pDummy[14]);
		
			for (int n = 0; n < pidArray.Count(); n++)
			{
			//find channel from sid's
				for (int i = 0; i < m_buflen; i++)
				{
					//Search ID Array for the SID Value
					if (pidArray[n].sid == (((0xFF & m_pDummy[i]) << 8) | (0xFF & m_pDummy[i + 1]))
						&& 0xFD00 == (((0xFD & m_pDummy[i + 2]) << 8) | (0x7F & m_pDummy[i + 3])))
//						&& 0xFD80 == (((0xFF & m_pDummy[i + 2]) << 8) | (0x80 & m_pDummy[i + 3])))
//						&& 0xFD80 == (((0xFF & m_pDummy[i + 2]) << 8) | (0xFF & m_pDummy[i + 3])))
					{
						int a = i + 9;
						int b = min(m_pDummy[i + 8], 128);
						memcpy(pidArray[n].onetname, m_pDummy + a, b);
						a += b; 
						b = min(m_pDummy[a], 128); a++;
						memcpy(pidArray[n].chname, m_pDummy + a, b);
						break;
					}
				};
			}
		}
		delete[] pData;
	}
	if (m_ONetworkID == 0)
		return S_FALSE;
	else
		return S_OK;
}
	
bool PidParser::CheckForONID(PBYTE pData, int pos, bool *extpacket, int *sectlen)
{
	//Return ok if we already found an ONID or if we don't have a TSID since we don't have a PAT
	if (m_TStreamID ==0)
		return true;

	int b = pos; //Set pos for Start Packet ID
	int endpos = pos + m_PacketSize; //Set to exclude crc bytes

	//Test if we have an ONID discriptor
	if ((*extpacket == false)
		&&  (pData[pos + 4] == 0)
		&& (pData[pos + 5] == 0x42) 
		&& ((0xF0 & pData[pos + 6]) == 0xF0)
		&& ((0xBF & pData[pos + 1]) == 0x00) //first packet of ID's
//		&& ((0xF0 & pData[pos + 1]) == 0x40) //first packet of ID's
		&& ((0x01 & pData[pos+11]) == 0x00)    // should be start of parsing
		&& ((((0x1F & pData[pos + 1]) << 8)|(0xFF & pData[pos + 2])) == 0x11) 
		&& ((((0x1F & pData[pos + 11]) << 8)|(0xFF & pData[pos + 12])) == 0)	//yes if we have the ONID flag
		)
	{
		*sectlen =((0x0F & pData[pos + 6]) << 8)|(0xFF & pData[pos + 7]) + 8; //include CRC bytes

		 // test if next packet required 
		if (*sectlen > 176)
			*extpacket = true; //set search for extended packet
		else
		{
			*extpacket = false; // set for next packet

			//if no descriptor info
			if (*sectlen <= 0x0F + 8)
				*sectlen = 0;
		};
	}
	else if ((*extpacket == true)	//Test if we have an ext ID discriptor
			&& (0xF0 & pData[pos + 3]) == 0x10 
			&& (0xF0 & pData[pos + 1]) == 0x00 //NOT first packet of ID's
			&& (((0x1F & pData[pos + 1]) << 8)|(0xFF & pData[pos + 2])) == 0x11
			)
	{
		b += 4; //set for extended packet ID
		endpos += 4; //set not to exclude crc bytes
		*sectlen = *sectlen + 4;
	}
	else
		return false; //No Descriptor Found

	while(b < endpos) 
	{
		//Bufer overflow
		if (m_buflen > 0x3FFF || *sectlen < 1) 
			return true; // buffer over flow or end parsing
	 
		*sectlen = *sectlen - 1;
		m_pDummy[m_buflen] = pData[b];
		m_buflen++;
		b++;
	};
	return false;
}

HRESULT PidParser::ParseEISection (ULONG ulDataLength)
{
	int pos = 0;
	WORD DescTag;
	int DescLen;

	while ((ULONG)pos < ulDataLength)
	{
		DescTag = (0xFF & m_pDummy[pos]);
		DescLen = (int)(0xFF & m_pDummy[pos+1]);

		if (DescTag == 0x4d) //0x42) {
		{
			// short event descriptor
			ParseShortEvent(pos, DescLen+2);
		}
		else
		{
			if (DescTag == 0x4e)
			{
				// extended event descriptor
				ParseExtendedEvent(pos, DescLen+2);
			}
		}

		pos = pos + DescLen + 2;
	}
	return S_OK;
}

HRESULT PidParser::ParseShortEvent(int start, ULONG ulDataLength)
{
	memcpy(m_shortdescr, m_pDummy + start, ulDataLength);
	return S_OK;
}

HRESULT PidParser::ParseExtendedEvent(int start, ULONG ulDataLength)
{
	memcpy(m_extenddescr, m_pDummy + start, ulDataLength);
	return S_OK;
}

REFERENCE_TIME PidParser::GetFileDuration(PidInfo *pPids, FileReader *pFileReader)
{

	HRESULT hr = S_OK;
	__int64 fileStart;
	__int64 filelength;
	REFERENCE_TIME totalduration = 0;
	REFERENCE_TIME startPCRSave = 0;
	REFERENCE_TIME endPCRtotal = 0;
	pPids->start = 0;
	pPids->end = 0;
	__int64 tolerence = 100000UL;


	__int64 startFilePos = 0; 

	pFileReader->GetFileSize(&fileStart, &filelength);
	filelength -= 100000;
	__int64 endFilePos = filelength;
	m_fileLenOffset = filelength;
	m_fileStartOffset = get_StartOffset();// skip faulty header 
	m_fileEndOffset = 0;
	BOOL bBitRateOK = FALSE;
	long lDataLength = MIN_FILE_SIZE;
	PBYTE pData = new BYTE[lDataLength];



//***********************************************************************************************
	int count = 0;
	__int64 kByteRate = 0;
	REFERENCE_TIME calcDuration = 0;

	// find first Duration
	while (m_fileStartOffset < filelength){

		hr = GetPCRduration(pData, lDataLength, pPids, filelength, &startFilePos, &endFilePos, pFileReader);
		if (hr == S_OK){
			//Save the start PCR only
			if (startPCRSave == 0 && pPids->start > 0)
				startPCRSave = pPids->start;

			//Add the PCR time difference
			totalduration = totalduration + (__int64)(pPids->end - pPids->start); // add duration to total.

			if (totalduration > 0 && calcDuration == 0) {
				//Only do this once even if we have a partial duration.
				kByteRate = (__int64) ((endFilePos*(__int64)100) / totalduration);
				if (kByteRate)
					calcDuration = 	(REFERENCE_TIME)((__int64)((__int64)filelength / (__int64)kByteRate)*(__int64)100);
//				kByteRate = (__int64) ((endFilePos *(__int64)10000) / totalduration);
//				calcDuration = 	(REFERENCE_TIME)((__int64)((__int64)filelength / (__int64)kByteRate)*(__int64)10000);
			}

			WORD readonly = 0;
			m_pFileReader->get_ReadOnly(&readonly);
			//If we have a calculated duration.
			if (calcDuration > 0) {
				//if we could not get a total file duration.
				if((endFilePos + tolerence) < filelength) {
					//If we are live or can't parse after 4 attempts.
					if(readonly || count > 2) {
						//Make up our end time from the calculated duration.
						totalduration = calcDuration;
						pPids->end = startPCRSave + totalduration;
						break;
					}
				}
			}
			else
				count++;

			//Test if at end of file & return
			if (endFilePos >= (__int64)(filelength - tolerence)){

				REFERENCE_TIME PeriodOfPCR = (REFERENCE_TIME)(((__int64)(pPids->end - pPids->start)/9)*1000); 

				//8bits per byte and convert to sec divide by pcr duration then average it
				if (PeriodOfPCR > 0)
				{
					pids.bitrate = long (((endFilePos - startFilePos)*80000000) / PeriodOfPCR);
					bBitRateOK = TRUE;
				}

				break;
			}

			m_fileStartOffset = endFilePos;
		}
		else
			m_fileStartOffset = m_fileStartOffset + 100000;

		//If unable to find any pcr's so don't go again
		if (pPids->start == 0 || pPids->end == 0){

			//Check if we have a pre-calculated length to use. 
			if(calcDuration > 0) {
				totalduration = calcDuration;
				pPids->end = startPCRSave + totalduration;
//PrintTime(TEXT("GetFileDuration get start or end pcr Failed"), pPids->end, 90);
				break;
			}
			else
			{
				if (pPids->start != 0 && pPids->end == 0){

					pPids->start = 0;
					pPids->end = 0;
					m_fileLenOffset = m_fileLenOffset/2;
					endFilePos = m_fileLenOffset;
					m_fileEndOffset = m_fileLenOffset;
					m_fileStartOffset = get_StartOffset();// skip faulty header 
					continue;
				}
				else
					break;

			}
		}

		pPids->start = 0;
		pPids->end = 0;
		m_fileLenOffset = filelength - m_fileStartOffset;
		endFilePos = m_fileLenOffset;
		m_fileEndOffset = 0;
	};

	if (startPCRSave != 0){

		pPids->start = startPCRSave; //Restore to first PCR

//PrintTime(TEXT("GetFileDuration got start pcr "), pPids->start, 90);
//PrintTime(TEXT("GetFileDuration got end pcr "), pPids->end, 90);

		if (pPids->end == 0)
			pPids->end = startPCRSave + totalduration; //Set the end pcr if not set.

		__int64 duration = (__int64)(((__int64)totalduration/(__int64)9) * (__int64)1000);

		if (totalduration > 0 && !bBitRateOK)
		{
			__int64 bitRate = (__int64)((__int64)filelength * (__int64)100);
			pids.bitrate = (long)(bitRate/(__int64)((__int64)totalduration/(__int64)9000));
		}
//PrintTime(TEXT("GetFileDuration ok"), (REFERENCE_TIME)(__int64)(((__int64)totalduration/(__int64)9) * (__int64)1000), 10000);
		delete[] pData;
		return (REFERENCE_TIME)(__int64)(((__int64)totalduration/(__int64)9) * (__int64)1000);
	}

	delete[] pData;
//PrintTime(TEXT("GetFileDuration Failed"), 0, 10000);
	return 0; //Duration not found
}

HRESULT PidParser::GetPCRduration(PBYTE pData,
								  long lDataLength,
								  PidInfo *pPids,
								  __int64 filelength,
								  __int64* pStartFilePos,
								  __int64* pEndFilePos,
								  FileReader *pFileReader)
{
	HRESULT hr;
	ULONG pos;
	REFERENCE_TIME midPCR;
	__int64 filetolerence = 100000UL;
	ULONG ulBytesRead = 0;
	pos = 0; 

	pFileReader->setFilePointer(m_fileStartOffset, FILE_BEGIN);
	pFileReader->Read(pData, lDataLength, &ulBytesRead);

	hr = FindNextPCR(pData, lDataLength, pPids, &pPids->start, &pos, 1); //Get the PCR
	if (hr == S_OK){
		//In case the PCR starts with zero
		if (!pPids->start)
			pPids->start = 1;

		m_fileLenOffset = m_fileLenOffset - (((__int64)pos) - 1);
		*pStartFilePos = m_fileStartOffset + (((__int64)pos) - 1);
//		m_fileLenOffset = m_fileLenOffset - (((__int64)pos) - 1);
//		*pStartFilePos = m_fileStartOffset + (((__int64)pos) - 1);
//		m_fileLenOffset = m_fileLenOffset - (__int64)(pos - 1);
//		*pStartFilePos = m_fileStartOffset + (__int64)(pos - 1); 
	}

	pos = lDataLength - m_PacketSize;
	hr = FindNextPCR(pData, lDataLength, pPids, &pPids->end, &pos, -1); //Get the PCR
	if (hr != S_OK)
		return S_FALSE; // Unable to get PCR time in first block

	__int64 SaveEndPCR = pPids->end;

	while(m_fileLenOffset > (__int64)m_PacketSize)//(10 * m_PacketSize))
	{
		if (pPids->start == 0)
			break; //exit if no PCR found

		pos = lDataLength - m_PacketSize;

		pFileReader->setFilePointer(-(__int64)(m_fileEndOffset + (__int64)lDataLength), FILE_END);
		pFileReader->Read(pData, lDataLength, &ulBytesRead);

		hr = FindNextPCR(pData, lDataLength, pPids, &pPids->end, &pos, -1); //Get the PCR
		if (hr != S_OK)
			break; //exit if no PCR found
		
		*pEndFilePos = filelength - m_fileEndOffset - (__int64)lDataLength + (__int64)pos - 1;// 

		if (*pEndFilePos <= *pStartFilePos)
			break; //exit if past start time
		
		m_fileEndOffset = m_fileEndOffset + (__int64)(m_fileLenOffset / 2); //Set file mid search pos

		//exit if bad PCR timming found
		if (pPids->end > pPids->start)
		{
			pos = lDataLength - m_PacketSize;

			pFileReader->setFilePointer(-(__int64)(m_fileEndOffset + (__int64)lDataLength), FILE_END);
			pFileReader->Read(pData, lDataLength, &ulBytesRead);

			hr = FindNextPCR(pData, lDataLength, pPids, &midPCR, &pos, -1); //Get the PCR
			if (hr != S_OK){

				m_fileLenOffset = (__int64)(m_fileLenOffset / 2); //Set file length offset for next search  
				return S_OK; // File length matchs PCR time
				break; //exit if no PCR found
			}

			//Test if mid file pos is the mid PCR time.
			if ((__int64)((__int64)midPCR - (__int64)pPids->start) <= (__int64)((__int64)(pPids->end - pPids->start) / 2) + filetolerence
				&& (__int64)((__int64)midPCR - (__int64)pPids->start) > (__int64)((__int64)(pPids->end - pPids->start) / 2) - filetolerence){

				m_fileLenOffset = (__int64)(m_fileLenOffset / 2); //Set file length offset for next search  

				return S_OK; // File length matchs PCR time
			}

			//force match if program pin mode
//			if (m_ProgPinMode && pPids->end && pPids->start) {
			if (pPids->end && pPids->start
				&& (__int64)midPCR > (__int64)pPids->start
				&& (__int64)midPCR < (__int64)pPids->end){

				m_fileLenOffset = (__int64)(m_fileLenOffset / 2); //Set file length offset for next search  
				return S_OK; // File length matchs PCR time
			}

		}
		m_fileLenOffset = (__int64)(m_fileLenOffset / 2); //Set file length offset for next search 
	}
	return S_FALSE; // File length does not match PCR time
}

void PidParser::AddPidArray()
{
	PidInfo *newPidInfo = new PidInfo();
	pidArray.Add(newPidInfo);
	SetPidArray(pidArray.Count() - 1);
}


void PidParser::SetPidArray(int n)
{
	if ((n < 0) || (n >= pidArray.Count()))
		return;
	PidInfo *pidInfo = &pidArray[n];

	pidInfo->CopyFrom(&pids);

	pidInfo->TsArray[0] = 0;
	AddTsPid(pidInfo, pids.pmt);	AddTsPid(pidInfo, pids.pcr);
	AddTsPid(pidInfo, pids.vid);
	AddTsPid(pidInfo, pids.h264);	AddTsPid(pidInfo, pids.mpeg4);
	AddTsPid(pidInfo, pids.txt);	AddTsPid(pidInfo, pids.sub);
	AddTsPid(pidInfo, pids.aud);	AddTsPid(pidInfo, pids.ac3);
	AddTsPid(pidInfo, pids.aac);	AddTsPid(pidInfo, pids.dts);
	AddTsPid(pidInfo, 0x00);			AddTsPid(pidInfo, 0x10);
	AddTsPid(pidInfo, 0x11);			AddTsPid(pidInfo, 0x12);
	AddTsPid(pidInfo, 0x13);			AddTsPid(pidInfo, 0x14);

	if (pids.aud2 != 0) AddTsPid(pidInfo, pids.aud2);
	if (pids.ac3_2 != 0) AddTsPid(pidInfo, pids.ac3_2);
	if (pids.aac2 != 0) AddTsPid(pidInfo, pids.aac2);
	if (pids.dts2 != 0) AddTsPid(pidInfo, pids.dts2);
}

void PidParser::AddTsPid(PidInfo *pidInfo, WORD pid)
{
	for (int i = 1; i < (int) pidInfo->TsArray[0]; i++)
	{
		if (pidInfo->TsArray[i] == pid)
			return;
	}

	pidInfo->TsArray[0]++;
	pidInfo->TsArray[pidInfo->TsArray[0]] = pid;
	return;
}

void PidParser::get_ChannelNumber(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	TCHAR sz[128];
	sprintf(sz, "%i",pids.chnumb);
	memcpy(pointer, sz, 128);
}

void PidParser::get_NetworkName(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, m_NetworkName, 128);
	memcpy(pointer + 127, "\0", 1);
}

void PidParser::get_ONetworkName(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, pids.onetname, 128);
	memcpy(pointer + 127, "\0", 1);
}

void PidParser::get_ChannelName(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, pids.chname, 128);
	memcpy(pointer + 127, "\0", 1);
}

void PidParser::get_ShortDescr(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, pids.sdesc, 128);
	memcpy(pointer + 127, "\0", 1);
}

void PidParser::get_ExtendedDescr(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, pids.edesc, 600);
	memcpy(pointer + 599, "\0", 1);
}

void PidParser::get_ShortNextDescr(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, pids.sndesc, 128);
	memcpy(pointer + 127, "\0", 1);
}

void PidParser::get_ExtendedNextDescr(BYTE *pointer)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pointer, pids.endesc, 600);
	memcpy(pointer + 599, "\0", 1);
}

void PidParser::get_CurrentTSArray(ULONG *pPidArray)
{
//	CAutoLock parserlock(&m_ParserLock);
	memcpy(pPidArray, pidArray[m_pgmnumb].TsArray, 16*sizeof(ULONG));
}

WORD PidParser::get_ProgramNumber()
{
//	CAutoLock parserlock(&m_ParserLock);
	return m_pgmnumb;
}

BOOL PidParser::get_ProgPinMode()
{
//	CAutoLock parserlock(&m_ParserLock);
	return m_ProgPinMode;
}

BOOL PidParser::get_AsyncMode()
{
//	CAutoLock parserlock(&m_ParserLock);
	return m_AsyncMode;
}

void PidParser::set_ProgPinMode(BOOL mode)
{
//	CAutoLock parserlock(&m_ParserLock);
	if (mode){

		m_PacketSize = 0x800; //Set for 2048 block
		m_pFileReader->set_DelayMode(FALSE);//cold start
//		m_pFileReader->set_DelayMode(TRUE);
		m_ProgPinMode = TRUE;
	}
	else{

		m_PacketSize = 188;
		m_pFileReader->set_DelayMode(FALSE);
		m_ProgPinMode = FALSE;
	}
}

__int64 PidParser::get_StartOffset(void)
{
	if (m_ProgPinMode)
		return FILE_START_POS_PS;
	else
		return FILE_START_POS_TS; 
}

REFERENCE_TIME PidParser::get_StartTimeOffset(void)
{
	if (m_ProgPinMode)
		return RT_FILE_START_PS;
	else
		return RT_FILE_START_TS; 
}

void PidParser::set_AsyncMode(BOOL mode)
{
//	CAutoLock parserlock(&m_ParserLock);
	if (mode)
		m_AsyncMode = TRUE;
	else
		m_AsyncMode = FALSE;
}

ULONG PidParser::get_PacketSize()
{
//	CAutoLock parserlock(&m_ParserLock);
	return m_PacketSize;
}

void PidParser::set_ProgramNumber(WORD programNumber)
{
//	CAutoLock parserlock(&m_ParserLock);
	m_pgmnumb = programNumber;
	pids.Clear();
	pids.CopyFrom(&pidArray[m_pgmnumb]);
}

void PidParser::set_SIDPid(int bProgramSID)
{
//	CAutoLock parserlock(&m_ParserLock);
	m_ProgramSID = bProgramSID;
	return;
}

HRESULT PidParser::set_ProgramSID()
{
//	CAutoLock parserlock(&m_ParserLock);
	HRESULT hr = S_FALSE;
	m_pgmnumb = 0;
	
	// fail if there is only one program in file
	if (!pidArray.Count())// cold start <= 1)
		return S_FALSE;

	//loop through SID's in list
	for (int c = 0; c < pidArray.Count(); c++)
	{
		if (m_ProgramSID == pidArray[c].sid && m_ProgramSID != 0)
		{
			//now copy the pids from the SID program found
			m_pgmnumb = c;
			pids.Clear();
			pids.CopyFrom(&pidArray[m_pgmnumb]);
			return S_OK;
		}
	}
	return S_FALSE;
}

void PidParser::PrintTime(LPCTSTR lstring, __int64 value, __int64 divider)
{

	TCHAR sz[100];
	long ms = (long)(value / divider);
	long secs = ms / 1000;
	long mins = secs / 60;
	long hours = mins / 60;
	ms = ms % 1000;
	secs = secs % 60;
	mins = mins % 60;
//	wsprintf(sz, TEXT("%05i - %s %02i:%02i:%02i.%03i\n"), debugcount, lstring, hours, mins, secs, ms);
	wsprintf(sz, TEXT("%05i - %s %02i:%02i:%02i.%03i\n"), 0, lstring, hours, mins, secs, ms);
	::OutputDebugString(sz);
//	debugcount++;
//MessageBox(NULL, sz,lstring, NULL);
}

