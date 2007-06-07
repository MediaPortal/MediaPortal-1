/**
*  Demux.h
*  Copyright (C) 2004-2006 bear
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
*  bear can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef DEMUX_H
#define DEMUX_H

// Define a typedef for a list of filters.
typedef CGenericList<IBaseFilter> CFilterList;

#include "PidParser.h"
#include "Control.h"
#include "FilterGraphTools.h"
#include "mpeg2data.h"

class Demux
{
public:

	Demux(PidParser *pPidParser, IBaseFilter *pFilter, CFilterList *pFList);

	virtual ~Demux();


	STDMETHODIMP AOnConnect();
	HRESULT SetTIFState(IFilterGraph *pGraph, REFERENCE_TIME tStart);

	BOOL get_Auto();
	BOOL get_NPControl();
	BOOL get_NPSlave();
	BOOL get_AC3Mode();
	BOOL get_FixedAspectRatio();
	BOOL get_CreateTSPinOnDemux();
	BOOL get_CreateTxtPinOnDemux();
	BOOL get_CreateSubPinOnDemux();
	BOOL get_MPEG2AudioMediaType();
	BOOL get_MPEG2Audio2Mode();
	void set_MPEG2AudioMediaType(BOOL bMPEG2AudioMediaType);
	void set_FixedAspectRatio(BOOL bFixedAR);
	void set_CreateTSPinOnDemux(BOOL bCreateTSPinOnDemux);
	void set_CreateTxtPinOnDemux(BOOL bCreateTxtPinOnDemux);
	void set_CreateSubPinOnDemux(BOOL bCreateSubPinOnDemux);
	void set_AC3Mode(BOOL bAC3Mode);
	void set_NPSlave(BOOL bNPSlave);
	void set_NPControl(BOOL bNPControl);
	void set_Auto(BOOL bAuto);
	void set_MPEG2Audio2Mode(BOOL bMPEG2Audio2Mode);
	void set_ClockMode(int clockMode);
	void SetRefClock();
	int  get_MP2AudioPid();
	int  get_AAC_AudioPid();
	int  get_DTS_AudioPid();
	int  get_AC3_AudioPid();
	int get_ClockMode();
	HRESULT	GetAC3Media(AM_MEDIA_TYPE *pintype);
	HRESULT	GetMP2Media(AM_MEDIA_TYPE *pintype);
	HRESULT	GetMP1Media(AM_MEDIA_TYPE *pintype);
	HRESULT	GetAACMedia(AM_MEDIA_TYPE *pintype);
	HRESULT	GetDTSMedia(AM_MEDIA_TYPE *pintype);
	HRESULT	GetVideoMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetH264Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetMpeg4Media(AM_MEDIA_TYPE *pintype);
	HRESULT	GetTelexMedia(AM_MEDIA_TYPE *pintype);
	HRESULT GetSubtitleMedia(AM_MEDIA_TYPE *pintype);
	HRESULT	GetTSMedia(AM_MEDIA_TYPE *pintype);
	HRESULT	GetParserMedia(AM_MEDIA_TYPE *pintype);
	static	HRESULT GetPeerFilters(IBaseFilter *pFilter, PIN_DIRECTION Dir, CFilterList &FilterList);  
	static  HRESULT GetNextFilter(IBaseFilter *pFilter, PIN_DIRECTION Dir, IBaseFilter **ppNext);
	static  void AddFilterUnique(CFilterList &FilterList, IBaseFilter *pNew);
	static  HRESULT GetReferenceClock(IBaseFilter *pFilter, IReferenceClock **ppClock);
	HRESULT CheckDemuxPids(PidParser *pPidParser);
	HRESULT	Sleeps(ULONG Duration, long TimeOut[]);
	HRESULT	IsStopped();
	HRESULT	IsPlaying();
	HRESULT	IsPaused();
	HRESULT	DoStop();
	HRESULT	DoStart();
	HRESULT	DoPause();
	HRESULT GetParserFilter(CComPtr<IBaseFilter>&pMpegSections);

protected:
	HRESULT UpdateDemuxPins(IBaseFilter* pDemux);
	HRESULT CheckDemuxPin(IBaseFilter* pDemux, AM_MEDIA_TYPE pintype, IPin** pIPin);
	HRESULT CheckVideoPin(IBaseFilter* pDemux);
	HRESULT CheckAudioPin(IBaseFilter* pDemux);
	HRESULT CheckAACPin(IBaseFilter* pDemux);
	HRESULT CheckDTSPin(IBaseFilter* pDemux);
	HRESULT CheckAC3Pin(IBaseFilter* pDemux);
	HRESULT CheckTelexPin(IBaseFilter* pDemux);
	HRESULT CheckSubtitlePin(IBaseFilter* pDemux);
	HRESULT CheckTsPin(IBaseFilter* pDemux);
	HRESULT CheckParserPin(IBaseFilter* pDemux);
	HRESULT	NewParserPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewTsPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewVideoPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewAudioPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewAACPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewDTSPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewAC3Pin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewTelexPin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	NewSubtitlePin(IMpeg2Demultiplexer* muxInterface, LPWSTR pinName);
	HRESULT	LoadParserPin(IPin* pIPin);
	HRESULT	LoadTsPin(IPin* pIPin);
	HRESULT	LoadAudioPin(IPin* pIPin, ULONG pid);
	HRESULT	LoadVideoPin(IPin* pIPin, ULONG pid);
	HRESULT	LoadTelexPin(IPin* pIPin, ULONG pid);
	HRESULT	LoadSubtitlePin(IPin* pIPin, ULONG pid);
	HRESULT VetDemuxPin(IPin* pIPin, ULONG pid);
	HRESULT	ClearDemuxPin(IPin* pIPin);
	HRESULT	ChangeDemuxPin(IBaseFilter* pDemux, LPWSTR* pPinName, BOOL* pConnect);
	HRESULT UpdateNetworkProvider(IBaseFilter* pNetworkProvider);
	HRESULT CheckTIFPin(IBaseFilter* pDemux);
	HRESULT GetTIFMedia(AM_MEDIA_TYPE *pintype);
	HRESULT RemoveFilterChain(IBaseFilter *pStartFilter, IBaseFilter *pEndFilter);
	HRESULT RenderFilterPin(IPin *pIPin);
	HRESULT ReconnectFilterPin(IPin *pIPin);
	HRESULT SetReferenceClock(IBaseFilter *pFilter);

	IBaseFilter *m_pTSFileSourceFilter;
	PidParser *m_pPidParser;
	CFilterList *m_pFilterRefList;
	CCritSec m_DemuxLock;
	FilterGraphTools graphTools;

	BOOL m_bAuto;
	BOOL m_bNPControl;
	BOOL m_bNPSlave;
	BOOL m_bAC3Mode;
	BOOL m_bFixedAR;
	BOOL m_bCreateTSPinOnDemux;
	BOOL m_bCreateTxtPinOnDemux;
	BOOL m_bCreateSubPinOnDemux;
	BOOL m_bMPEG2AudioMediaType;
	BOOL m_bMPEG2Audio2Mode;
	BOOL m_WasPlaying;
	BOOL m_WasPaused;
	int  m_ClockMode;
	LONG m_TimeOut[2];

public:

	BOOL m_bConnectBusyFlag;
	BOOL m_StreamH264;
	BOOL m_StreamMpeg4;
	BOOL m_StreamVid;
	BOOL m_StreamAC3;
	BOOL m_StreamMP2;
	BOOL m_StreamAud2;
	BOOL m_StreamAAC;
	BOOL m_StreamDTS;
	int  m_SelAudioPid;
	int  m_SelVideoPid;
	int  m_SelTelexPid;
	int  m_SelSubtitlePid;
	AM_MEDIA_TYPE m_SelAudioType;
	AM_MEDIA_TYPE m_SelVideoType;
};

#endif
