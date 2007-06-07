/**
 *	DVBTChannels.h
 *	Copyright (C) 2004 Nate
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


#ifndef DVBTCHANNELS_H
#define DVBTCHANNELS_H

#include "StdAfx.h"
#include "LogMessage.h"
//#include "XMLDocument.h"
//#include "IDWOSDDataList.h"
#include <vector>

class DVBTChannels;
class DVBTChannels_Network;

enum DVBTChannels_Service_PID_Types
{
	unknown,
	video,
	h264,
	mpeg4,
	mp1,
	mp2,
	ac3,
	aac,
	dts,
	teletext,
	subtitle,
	pmt,
	pcr,
	DVBTChannels_Service_PID_Types_Count
};

static const LPWSTR DVBTChannels_Service_PID_Types_String[] =
{
	L"Unknown",
	L"MPEG2 Video",
	L"H264 Video",
	L"MPEG4 Video",
	L"MPEG Audio",
	L"MPEG2 Audio",
	L"AC3 Audio",
	L"AAC Audio",
	L"DTS Audio",
	L"Teletext",
	L"Subtitle",
	L"PMT",
	L"PCR"
};

//Stream
class DVBTChannels_Stream : public LogMessageCaller
{
public:
	DVBTChannels_Stream();
	virtual ~DVBTChannels_Stream();

	virtual void SetLogCallback(LogMessageCallback *callback);

	void UpdateStream(DVBTChannels_Stream *pNewStream);
	void PrintStreamDetails();

	long PID;
	DVBTChannels_Service_PID_Types Type;
	//char Lang[4];
	LPWSTR Language;
	BOOL bActive;
	BOOL bDetected;
};

//Service
class DVBTChannels_Service : public LogMessageCaller
{
	friend DVBTChannels;
public:
	DVBTChannels_Service();
	virtual ~DVBTChannels_Service();

	virtual void SetLogCallback(LogMessageCallback *callback);

	void AddStream(DVBTChannels_Stream* pStream);
//	HRESULT LoadFromXML(XMLElement *pElement);
//	HRESULT SaveToXML(XMLElement *pElement);

	DVBTChannels_Service_PID_Types GetStreamType(int index);
	long GetStreamPID(int index);
	long GetStreamPID(DVBTChannels_Service_PID_Types streamtype, int index);

	long GetStreamCount();
	long GetStreamCount(DVBTChannels_Service_PID_Types streamtype);

	BOOL UpdateService(DVBTChannels_Service *pNewService);
	BOOL UpdateStreams(DVBTChannels_Service *pNewService);
	void PrintServiceDetails();

	DVBTChannels_Stream *FindStreamByPID(long PID);

public:
	long serviceId;
	long logicalChannelNumber;
	LPWSTR serviceName;

protected:
	std::vector<DVBTChannels_Stream *> m_streams;
	CCritSec m_streamsLock;
	long favoriteID;
	BOOL bManualUpdate;
};

//Network
class DVBTChannels_Network : public LogMessageCaller//, public IDWOSDDataList
{
	friend DVBTChannels;
public:
	DVBTChannels_Network(DVBTChannels *pChannels);
	virtual ~DVBTChannels_Network();

	virtual void SetLogCallback(LogMessageCallback *callback);

//	HRESULT LoadFromXML(XMLElement *pElement);
//	HRESULT SaveToXML(XMLElement *pElement);

	DVBTChannels_Service *FindDefaultService();
	DVBTChannels_Service *FindServiceByServiceId(long serviceId);
	DVBTChannels_Service *FindNextServiceByServiceId(long serviceId);
	DVBTChannels_Service *FindPrevServiceByServiceId(long serviceId);

	BOOL UpdateNetwork(DVBTChannels_Network *pNewNetwork);
	void PrintNetworkDetails(DVBTChannels_Network *pNetwork);

	//IDWOSDDataList Methods
	virtual LPWSTR GetListName();
	virtual LPWSTR GetListItem(LPWSTR name, long nIndex = 0);
	virtual long GetListSize();
	virtual HRESULT FindListItem(LPWSTR name, int *pIndex);

public:
	long originalNetworkId;
	long transportStreamId;
	long networkId;
	long frequency;
	long frequencyInStream;
	long bandwidth;
	unsigned char otherFrequencyFlag : 1;
	std::vector<long> otherFrequencys;

	LPWSTR networkName;

protected:
	DVBTChannels *m_pChannels;

	std::vector<DVBTChannels_Service *> m_services;
	CCritSec m_servicesLock;
	LPWSTR m_dataListName;
	LPWSTR m_dataListString;
};

//NetworkList
class DVBTChannels_NetworkList : public LogMessageCaller
{
	friend DVBTChannels;
public:
	DVBTChannels_NetworkList();
	virtual ~DVBTChannels_NetworkList();

	virtual void SetLogCallback(LogMessageCallback *callback);
	virtual HRESULT Clear();

	virtual DVBTChannels_Network *CreateNetwork(long originalNetworkId, long transportStreamId, long networkId);
	virtual DVBTChannels_Network *FindNetwork(long originalNetworkId, long transportStreamId, long networkId);
	virtual DVBTChannels_Network *FindNetworkByONID(long originalNetworkId);
	virtual DVBTChannels_Network *FindNetworkByTSID(long transportStreamId);
	virtual DVBTChannels_Network *FindNetworkByFrequency(long frequency);
	virtual DVBTChannels_Network *FindNextNetworkByOriginalNetworkId(long oldOriginalNetworkId);
	virtual DVBTChannels_Network *FindPrevNetworkByOriginalNetworkId(long oldOriginalNetworkId);
	virtual DVBTChannels_Network *FindNextNetworkByFrequency(long oldFrequency);
	virtual DVBTChannels_Network *FindPrevNetworkByFrequency(long oldFrequency);

protected:
	std::vector<DVBTChannels_Network *> m_networks;
	CCritSec m_networksLock;
};

//Channels
class DVBTChannels : public DVBTChannels_NetworkList//, public IDWOSDDataList
{
public:
	DVBTChannels();
	virtual ~DVBTChannels();

	virtual void SetLogCallback(LogMessageCallback *callback);
	HRESULT Destroy();

//	HRESULT LoadChannels(LPWSTR filename);
//	HRESULT SaveChannels(LPWSTR filename = NULL);

	long GetDefaultBandwidth();

	DVBTChannels_Network *FindDefaultNetwork();

	//Update Methods
	BOOL UpdateNetwork(DVBTChannels_Network *pNewNetwork);

	HRESULT MoveNetworkUp(long transportStreamId);
	HRESULT MoveNetworkDown(long transportStreamId);

	//IDWOSDDataList Methods
	virtual LPWSTR GetListName();
	virtual LPWSTR GetListItem(LPWSTR name, long nIndex = 0);
	virtual long GetListSize();
	virtual HRESULT FindListItem(LPWSTR name, int *pIndex);

protected:
	long m_bandwidth;
	LPWSTR m_filename;
	LPWSTR m_dataListName;
	LPWSTR m_dataListString;
};

#endif
