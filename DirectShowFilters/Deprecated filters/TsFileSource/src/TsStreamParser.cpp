/**
*  TsStreamParser.cpp
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

#include <streams.h>
#include "TsStreamParser.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////


TsStreamParser::TsStreamParser(PidParser *pPidParser, Demux * pDemux, NetInfoArray *pNetArray)
{
	m_pPidParser = pPidParser;
	m_pDemux = pDemux;
	m_pNetArray = pNetArray;
}

TsStreamParser::~TsStreamParser()
{
	StreamArray.Clear();
}

HRESULT TsStreamParser::ParsePidArray()
{
	HRESULT hr = S_FALSE;

	CAutoLock StreamLock(&m_StreamLock);

	StreamArray.Clear();
	if (!m_pPidParser->pidArray.Count())
		return hr;

//	if (m_pPidParser->m_ParsingLock)
//		return hr;

	//Check if we are locked out
	int cnt = 0;
	while (m_pPidParser->m_ParsingLock)
	{
		Sleep(100);
		cnt++;
		if (cnt > 10)
		{
			return S_FALSE;
		}
	}
	//Lock the parser
	m_pPidParser->m_ParsingLock = TRUE;

	int index = 0;

	//Setup Network Name
	streams.Clear();
	LoadStreamArray(0);
	AddStreamArray();
	TCHAR szBuffer[256];
	if (m_pPidParser->m_NetworkName[0] == 0)
		sprintf(szBuffer,"  Program Info Unavailable", m_pPidParser->m_NetworkName);
	else
		sprintf(szBuffer,"  %s Program Information", m_pPidParser->m_NetworkName);

	mbstowcs(StreamArray[index].name, (const char*)&szBuffer, 256);
	ZeroMemory(szBuffer, sizeof(szBuffer));
//	m_pDemux->GetVideoMedia(&StreamArray[index].media);
	index++;

	int count = 0;
	while (count < m_pPidParser->pidArray.Count())
	{
		streams.Clear();
		LoadStreamArray(count);

		//Setup Video Track
		AddStreamArray();
		TCHAR szBuffer[400];
		if (m_pPidParser->pidArray[count].chnumb == 0)
			sprintf(szBuffer,"   ");
		else
			sprintf(szBuffer,"   %i. ", m_pPidParser->pidArray[count].chnumb);

		if (m_pPidParser->pidArray[count].chname[0] == 0) {

			strcat(szBuffer,"Program ");
			TCHAR sz[32];
			sprintf(sz, "%i", count + 1);
			strcat(szBuffer, sz);
		}
		else
			strcat(szBuffer, (const char *)&m_pPidParser->pidArray[count].chname);


		if (m_pPidParser->pidArray[count].onetname[0] == 0)
			strcat(szBuffer," [Network Unknown]");
		else {

			strcat(szBuffer," [");
			strcat(szBuffer, (const char *)&m_pPidParser->pidArray[count].onetname);
			strcat(szBuffer,"]");
		}
			
		mbstowcs(StreamArray[index].name, (const char*)&szBuffer, sizeof(szBuffer));
		StreamArray[index].lcid = 0;
		m_pDemux->GetVideoMedia(&StreamArray[index].media);

		if (m_pPidParser->pidArray[count].h264)
		{
			StreamArray[index].Pid = m_pPidParser->pidArray[count].h264;
			StreamArray[index].H264 = true;
			m_pDemux->GetH264Media(&StreamArray[index].media);
		}
		else if (m_pPidParser->pidArray[count].mpeg4)
		{
			StreamArray[index].Pid = m_pPidParser->pidArray[count].mpeg4;
			StreamArray[index].Mpeg4 = true;
			m_pDemux->GetMpeg4Media(&StreamArray[index].media);
		}
		else if (m_pPidParser->pidArray[count].vid)
		{
			StreamArray[index].Pid = m_pPidParser->pidArray[count].vid;
			StreamArray[index].Vid = true;
			m_pDemux->GetVideoMedia(&StreamArray[index].media);
		}
		else
			StreamArray[index].Vid = true;

		index++;

		//Setup Audio Tracks
		if (m_pPidParser->pidArray[count].aud)
		{
			AddStreamArray();
			StreamArray[index].Aud = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].aud;
			m_pDemux->GetMP2Media(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        Mpeg Audio Track");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].aud2)
		{
			AddStreamArray();
			StreamArray[index].Aud = true;
			StreamArray[index].Aud2 = true;
			m_pDemux->GetMP2Media(&StreamArray[index].media);
			StreamArray[index].Pid = m_pPidParser->pidArray[count].aud2;
			wcscat(StreamArray[index].name, L"        Mpeg Audio Track 2");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].ac3)
		{
			AddStreamArray();
			StreamArray[index].AC3 = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].ac3;
			m_pDemux->GetAC3Media(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        AC3 Audio Track");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].ac3_2)
		{
			AddStreamArray();
			StreamArray[index].AC3 = true;
			StreamArray[index].Aud2 = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].ac3_2;
			m_pDemux->GetAC3Media(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        AC3 Audio Track 2");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].aac)
		{
			AddStreamArray();
			StreamArray[index].AAC = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].aac;
			m_pDemux->GetAACMedia(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        AAC Audio Track");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].aac2)
		{
			AddStreamArray();
			StreamArray[index].AAC = true;
			StreamArray[index].Aud2 = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].aac2;
			m_pDemux->GetAACMedia(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        AAC Audio Track 2");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].dts)
		{
			AddStreamArray();
			StreamArray[index].DTS = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].dts;
			m_pDemux->GetDTSMedia(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        DTS Audio Track");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		if (m_pPidParser->pidArray[count].dts2)
		{
			AddStreamArray();
			StreamArray[index].DTS = true;
			StreamArray[index].Aud2 = true;
			StreamArray[index].Pid = m_pPidParser->pidArray[count].dts2;
			m_pDemux->GetDTSMedia(&StreamArray[index].media);
			wcscat(StreamArray[index].name, L"        DTS Audio Track 2");
			StreamArray[index].lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_NEUTRAL), SORT_DEFAULT);
			index++;
		}
		count++;
	}

	streams.Clear();
	LoadStreamArray(count);
	AddStreamArray();
	swprintf(StreamArray[index].name, L"  File Menu:");
	index++;
	streams.Clear();
	LoadStreamArray(count);
	AddStreamArray();
	swprintf(StreamArray[index].name, L"  Open Browse Window");
	index++;
	count++;

	if (m_pNetArray->Count())
	{
		streams.Clear();
		LoadStreamArray(count);
		AddStreamArray();
		swprintf(StreamArray[index].name, L"  Active Multicast Streams:");
		index++;

		int offset = count;
		int i = 0;
		while (i < m_pNetArray->Count())
		{
			streams.Clear();
			LoadStreamArray(count);
			AddStreamArray();
			swprintf(StreamArray[index].name, L"UDP@ %S : %S : %S [%lukb/s]",
												(*m_pNetArray)[i].strIP,
												(*m_pNetArray)[i].strPort,
												(*m_pNetArray)[i].strNic,
												(__int64)((*m_pNetArray)[i].flowRate * 80)/10000);
			if ((*m_pNetArray)[i].playing)
				StreamArray[index].flags = AMSTREAMSELECTINFO_ENABLED;
			else
				StreamArray[index].flags = 0;

			i++;
			index++;
		}
	}
	m_pPidParser->m_ParsingLock = FALSE;

	return S_OK;
}
	
void TsStreamParser::LoadStreamArray(int cnt)
{
		streams.flags = 0; 
		streams.group = cnt;
		streams.object = 0;
		streams.unk = 0;
}
	
void TsStreamParser::SetStreamActive(int group)
{
	CAutoLock StreamLock(&m_StreamLock);

	int count = 0;
	while (count < StreamArray.Count())
	{
		if (m_pNetArray->Count())
		{
			if (count <= count - m_pNetArray->Count())
				StreamArray[count].flags = 0;
		}

		if (StreamArray.Count() && StreamArray[count].group == group)
		{
			if (StreamArray[count].Vid | StreamArray[count].H264 | StreamArray[count].Mpeg4)
			{
				StreamArray[count].flags = AMSTREAMSELECTINFO_ENABLED;
			}
			else if (StreamArray[count].Pid == m_pDemux->m_SelAudioPid && m_pDemux->m_SelAudioPid)
			{
				StreamArray[count].flags = AMSTREAMSELECTINFO_EXCLUSIVE | AMSTREAMSELECTINFO_ENABLED; //AMSTREAMSELECTINFO_ENABLED;
				m_StreamIndex = count;
			}
		}
		count++;
	}

}

bool TsStreamParser::IsStreamActive(int index)
{
	CAutoLock StreamLock(&m_StreamLock);

	if (StreamArray[index].flags == AMSTREAMSELECTINFO_EXCLUSIVE)
		return true;

	return false;
}

void TsStreamParser::AddStreamArray()
{
	StreamInfo *newStreamInfo = new StreamInfo();
	StreamArray.Add(newStreamInfo);
	SetStreamArray(StreamArray.Count() - 1);
}


void TsStreamParser::SetStreamArray(int n)
{
	if ((n < 0) || (n >= StreamArray.Count()))
		return;

	StreamInfo *StreamInfo = &StreamArray[n];

	StreamInfo->CopyFrom(&streams);
}

void TsStreamParser::set_StreamArray(int streamIndex)
{
	m_StreamIndex = streamIndex;
	streams.Clear();
	streams.CopyFrom(&StreamArray[streamIndex]);
}

WORD TsStreamParser::get_StreamIndex()
{
	return m_StreamIndex;
}

void TsStreamParser::set_StreamIndex(WORD streamIndex)
{
	m_StreamIndex = streamIndex;
}

//				WCHAR test(0x00B3);
//				memcpy(StreamArray[count].name, &test, 2);

