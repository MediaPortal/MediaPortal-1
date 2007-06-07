/**
 *	DVBMpeg2DataParser.h
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

#ifndef DVBMPEG2DATAPARSER_H
#define DVBMPEG2DATAPARSER_H

#include "LogMessage.h"
#include "LogMessageWriter.h"
#include "DVBTChannels.h"
#include <mpeg2data.h>
#include <mpeg2bits.h>
#include <vector>

enum running_mode
{
	RM_UNDEFINED   = 0x00,
	RM_NOT_RUNNING = 0x01,
	RM_STARTS_SOON = 0x02,
	RM_PAUSING     = 0x03,
	RM_RUNNING     = 0x04
};

class DVBTransponder;
class DVBMpeg2DataParser;

class DVBSection
{
public:
	DVBSection();
	virtual ~DVBSection();
	void Setup(long pid, long tableId, long segmented, int timeout);

	DVBSection *CreateNewSegment(long tableIdExtention, long versionNumber);
	DVBSection *FindSegment(long tableIdExtention);

	BOOL IsSectionDone(long sectionNumber);
	void SetSectionDone(long sectionNumber);
	void ResetSectionDone();

	long pid;
	long tableId;

	//unsigned long run_once : 1;
	unsigned long segmented : 1;
	long fd;
	long tableIdExt;
	long sectionVersionNumber;
	long sectionFilterDone;

	time_t timeout;
	time_t startTime;
	time_t runningTime;
	DVBSection *nextSegment;

protected:
	__int8 m_sectionDone[32];

};


class DVBStream : public DVBTChannels_Stream
{
public:
	DVBStream();
	virtual ~DVBStream();

	long MpegStreamType;
/*
	enum audioTypes
	{
		AT_DEFAULT          = 0x00,
		AT_CLEAN_EFFECTS    = 0x01,
		AT_HEADING_IMPARED  = 0x02,
		AT_VISUALLY_IMPARED = 0x03
	} audioType;
*/
};

class DVBService : public DVBTChannels_Service
{
	friend DVBTransponder;
	friend DVBMpeg2DataParser;

public:
	DVBService();
	virtual ~DVBService();

	void ParseSDTDescriptors(unsigned char *buf, int remainingLength);

private:
	void ParseServiceDescriptor(unsigned char *buf);

public:
	long transportStreamId;
	LPWSTR providerName;
	long pmtPid;
	long pcrPid;
	unsigned char type;
	unsigned char scrambled : 1;
	enum running_mode running;
	DVBSection *section; //priv
};

class DVBTransponder : public DVBTChannels_Network
{
public:
	DVBTransponder();
	virtual ~DVBTransponder();

	void SetLogCallback(LogMessageCallback *callback);

	DVBService *CreateService(long serviceId);

	void ParseNITDescriptors(unsigned char *buf, int remainingLength);

private:
	void ParseTerrestrialDeliverySystemDescriptor(unsigned char *buf);
	void ParseFrequencyListDescriptor(unsigned char *buf);
	void ParseTerrestrialChannelNumberDescriptor(unsigned char *buf);
};

//TransponderList
class DVBTransponderList : public DVBTChannels_NetworkList
{
public:
	HRESULT Clear();
	DVBTChannels_Network *CreateNetwork(long originalNetworkId, long transportStreamId, long networkId);
};

class DVBMpeg2DataParser
{
public:
	DVBMpeg2DataParser();
	virtual ~DVBMpeg2DataParser();

	void SetDVBTChannels(DVBTChannels *pNetwork);
	void SetFrequency(long frequency);
	void SetFilter(CComPtr <IBaseFilter> pBDASecTab);
	void ReleaseFilter();

	HRESULT StartScan();
	BOOL IsScanRunning();
	HRESULT EndScan();
	DWORD WaitForThreadToFinish();

	void StartScanThread();

private:
	HRESULT ReadSection(DVBSection *pSection);
	void ParseSection(DVBSection *pSection, unsigned char *pBuffer);

	void ParsePAT(unsigned char *buf, int sectionLength, int transportStreamId);
	void ParsePMT(unsigned char *buf, int sectionLength, int serviceId);
	void ParseNIT(unsigned char *buf, int sectionLength, int networkId);
	void ParseSDT(unsigned char *buf, int sectionLength, int transportStreamId);

	void UpdateInformation();

	BOOL IsFilterRunning();

	// Methods that could be static
	int FindDescriptor(__int8 tag, unsigned char *buf, int remainingLength, const unsigned char **desc, int *descLen);
	//void ParseLangDescriptor(unsigned char *buf, int remainingLength, DVBServiceAudio &audio);

private:
	DVBTChannels *m_pDVBTChannels;
	long m_frequency;
	CComPtr <IMpeg2Data> m_piMpeg2Data;

	HANDLE m_hScanningStopEvent[2];
	HANDLE m_hScanningDoneEvent;

	BOOL m_bThreadStarted;

	vector<DVBSection *> m_waitingFilters;

	DVBTransponderList m_networks;
	DVBTransponder *m_currentTransponder;

	const int m_cBufferSize;
	unsigned char *m_pDataBuffer;

	LogMessage log;
	LogMessageWriter m_logWriter;
};

#endif
