/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
#include <streams.h>
#include <bdaiface.h>
#include "SplitterSetup.h"
#include <commctrl.h>
#include <atlbase.h>
#include <streams.h>
#include <dvdmedia.h>

extern void LogDebug(const char *fmt, ...) ;

SplitterSetup::SplitterSetup(Sections *pSections) :
m_demuxSetupComplete(FALSE)
{
	m_pSections = pSections;
	m_pAudio=NULL;
	m_pVideo=NULL;

	memset(&m_videoHdr,0,sizeof(MPEG2VIDEOINFO));
	m_videoHdr.dwProfile=2;
	m_videoHdr.dwLevel=2;

	memset(&m_ac3Hdr,0,sizeof(WAVEFORMATEX));
	m_ac3Hdr.wFormatTag=0;
	m_ac3Hdr.nChannels=2;
	m_ac3Hdr.nSamplesPerSec=48000;
	m_ac3Hdr.nBlockAlign=768;
	m_ac3Hdr.wBitsPerSample=16;
	m_ac3Hdr.cbSize=0;
	
}

SplitterSetup::~SplitterSetup()
{
}

HRESULT SplitterSetup::SetDemuxPins(IFilterGraph *pGraph)
{

	LogDebug("demux:SetDemuxPins()");
	if(m_demuxSetupComplete==true)
	{
		LogDebug("demux already setup");
		return S_FALSE;
	}
	if(pGraph==NULL)
	{
		LogDebug("IFilterGraph==NULL");
		return S_FALSE;
	}

	HRESULT hr;
	IGraphBuilder *pGB=NULL;

	if(FAILED(pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGB)))
	{
		LogDebug("IID_IGraphBuilder not found");
		return S_FALSE;
	}

	IEnumFilters* pEnum;
	hr=pGB->EnumFilters(&pEnum);
	if (SUCCEEDED(hr))
	{
		IBaseFilter* pFilter;
		ULONG        fetched=0;
		pEnum->Reset();
		while (SUCCEEDED(pEnum->Next(1,&pFilter,&fetched)))
		{
			if (fetched==1 && pFilter!=NULL)
			{
				//LogDebug("got filter");
				IMpeg2Demultiplexer *demuxer=NULL;
				hr=pFilter->QueryInterface(IID_IMpeg2Demultiplexer,(void**)&demuxer);
				if ( SUCCEEDED(hr) && demuxer!=NULL)
				{
					demuxer->Release();
					//LogDebug("demux: found IMpeg2Demultiplexer");
					SetupDemuxer(pFilter);
				}/*
				else
				{
					IEnumPins *pinEnum;
					if (SUCCEEDED(pFilter->EnumPins(&pinEnum)))
					{
						if (pinEnum!=NULL)
						{
							pinEnum->Reset();
							ULONG fetched;
							IPin* pin;
							while (SUCCEEDED(pinEnum->Next(1,&pin,&fetched)))
							{
								if (fetched==1 && pin!=NULL)
								{
									IMPEG2PIDMap	*pMap=NULL;
									if (SUCCEEDED(pin->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap)))
									{
										pMap->Release();
									}
									pin->Release();
								}
								else break;
							}
							pinEnum->Release();
						}
					}
				}*/
				pFilter->Release();
			}
			else break;
		}
		pEnum->Release();
	}
	else
	{
		LogDebug("failed to enum filters");
	}
	pGB->Release();
	return NOERROR;
}
HRESULT SplitterSetup::SetupDemuxer(IBaseFilter *demuxFilter)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	HRESULT hr=0;

	LogDebug("mpeg2 demux:SetupDemuxer()");
	if(demuxFilter==NULL)
		return S_FALSE;

	IMpeg2Demultiplexer *demuxer=NULL;

	hr=demuxFilter->QueryInterface(IID_IMpeg2Demultiplexer,(void**)&demuxer);
	if(FAILED(hr))
	{
		LogDebug("mpeg2 demux:FAILED to get IMpeg2Demultiplexer");
		return hr;
	}
	// create pins on demuxer
	// audio
	AM_MEDIA_TYPE type;

	// video 
	if (m_pSections->pids.VideoPid>0)
	{
		if (m_pVideo==NULL)
		{
			LogDebug("mpeg2 demux:create video pin");
			ZeroMemory(&type, sizeof(AM_MEDIA_TYPE));
			GetVideoMedia(&type);
			hr=demuxer->CreateOutputPin(&type, L"Video",&m_pVideo);
			if (hr==VFW_E_DUPLICATE_NAME)
			{
				//pin already exists!
				IEnumPins* enumPins;
				demuxFilter->EnumPins(&enumPins);
				enumPins->Reset();
				IPin* pins[2];
				ULONG fetched;
				while (enumPins->Next(1, &pins[0],&fetched)==0)
				{
					if (fetched==1)
					{
						PIN_INFO pinInfo;
						pinInfo.pFilter=NULL;
						hr=pins[0]->QueryPinInfo(&pinInfo);
						if (SUCCEEDED(hr))
						{
							if (wcscmp(pinInfo.achName,L"Video")==0)
							{
								m_pVideo=pins[0];
							}
							if (pinInfo.pFilter!=NULL)
								pinInfo.pFilter->Release();
						}
						pins[0]->Release();
					}
					else break;
				}
				enumPins->Release();
			}
		}
	}
	if (m_pAudio==NULL)
	{
		LogDebug("mpeg2 demux:create audio pin");
		ZeroMemory(&type, sizeof(AM_MEDIA_TYPE));
		if(m_pSections->pids.VideoPid>0)
		{
			GetAC3Media(&type);
		}
		else
		{
			if(m_pSections->pids.PCRPid==0 || m_pSections->pids.PCRPid>=0x1FFF)
			{
				GetAudioPayload(&type);// radio
			}
			else
			{
				GetMP2Media(&type); // radio
			}	
		}
		hr=demuxer->CreateOutputPin(&type,L"Audio" ,&m_pAudio);
		if (hr==VFW_E_DUPLICATE_NAME)
		{
			//pin already exists!
			IEnumPins* enumPins;
			demuxFilter->EnumPins(&enumPins);
			enumPins->Reset();
			IPin* pins[2];
			ULONG fetched;
			while (enumPins->Next(1, &pins[0],&fetched)==0)
			{
				if (fetched==1)
				{
					PIN_INFO pinInfo;
					pinInfo.pFilter=NULL;
					hr=pins[0]->QueryPinInfo(&pinInfo);
					if (SUCCEEDED(hr))
					{
						if (wcscmp(pinInfo.achName,L"Audio")==0)
						{
							m_pAudio=pins[0];
						}
						
						if (pinInfo.pFilter!=NULL)
							pinInfo.pFilter->Release();
					}
					pins[0]->Release();
				}
				else break;
			}
			enumPins->Release();
		}
	}

	SetupPids();

	m_demuxSetupComplete=true;
	demuxer->Release();
	return S_OK;
}

HRESULT SplitterSetup::SetupPids()
{
	// video
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	ULONG			pid;
	PID_MAP			pm;
	ULONG			count;
	ULONG			umPid;
	int				maxCounter;
	HRESULT hr=0;
	LogDebug("mpeg2 demux:SetupPids() audio:%x video:%x (%x %x)",m_pSections->pids.VideoPid,m_pSections->pids.CurrentAudioPid, m_pVideo,m_pAudio);
	if (m_pVideo!=NULL && m_pSections->pids.VideoPid>0)
	{
		LogDebug("mpeg2 demux:SetupPids() setup video pid");
		hr=m_pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
		{	
			LogDebug("mpeg2 demux:SetupPids() failed to get IMPEG2PIDMap");
			return 1;
		}
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
		{
			LogDebug("mpeg2 demux:SetupPids() failed to enum pids");
			return 5;
		}
		// enum and unmap the pids
		maxCounter=20;
		while(pPidEnum->Next(1,&pm,&count)== S_OK)
		{
			maxCounter--;
			if (maxCounter<0) break;
			if (count !=1) break;
			umPid=pm.ulPID;
			hr=pMap->UnmapPID(1,&umPid);
			if(FAILED(hr))
			{
				LogDebug("mpeg2 demux:SetupPids() failed unmap pid:%x",umPid);
				return 6;
			}
		}
		pPidEnum->Release();
		// map new pid
		pid = (ULONG)m_pSections->pids.VideoPid;
		LogDebug("demuxer:map video pid:0x%x", pid);
		
		//if(m_pSections->pids.MPEG4==false)// if the mpeg2 stream contains mpeg4 we map transport payload
			hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
		//else
		//	hr=pMap->MapPID(1,&pid,MEDIA_TRANSPORT_PAYLOAD);

		if(FAILED(hr))
		{
			LogDebug("mpeg2 demux:SetupPids() failed map pid:%x",pid);
			return 2;
		}
		pMap->Release();
	}
	
	// audio 
	if (m_pAudio!=NULL)
	{

		LogDebug("mpeg2 demux:SetupPids() setup audio pid");
		hr=m_pAudio->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
		{
			LogDebug("mpeg2 demux:SetupPids() failed to get IMPEG2PIDMap");
			return 3;
		}
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
		{
			LogDebug("mpeg2 demux:SetupPids() failed to enumpids");
			return 7;
		}
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
			{
				LogDebug("mpeg2 demux:SetupPids() failed unmap pid:%x",umPid);
				return 8;
			}
		}
		pPidEnum->Release();
		pid = (ULONG)m_pSections->pids.CurrentAudioPid;
		if(m_pSections->pids.VideoPid>0)
		{
			hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM); // tv
			LogDebug("demuxer:map audio pid:0x%x MEDIA_ELEMENTARY_STREAM", pid);
		}
		else
		{
			if(m_pSections->pids.PCRPid==0 || m_pSections->pids.PCRPid>=0x1FFF)
			{
				hr=pMap->MapPID(1,&pid,MEDIA_TRANSPORT_PAYLOAD); // radio
				LogDebug("demuxer:map audio pid:0x%x MEDIA_TRANSPORT_PAYLOAD", pid);
			}
			else
			{
				hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM); // radio
				LogDebug("demuxer:map audio pid:0x%x MEDIA_ELEMENTARY_STREAM", pid);
			}
		}

		if(FAILED(hr))
		{
			LogDebug("mpeg2 demux:SetupPids() failed to map pid:%x",pid);
			return 4;
		}

		pMap->Release();
	}
	LogDebug("mpeg2 demux:SetupPids() done");
	return S_OK;

}
void SplitterSetup::ShowDemuxPins(IFilterGraph *pGraph)
{
	DbgSetModuleLevel(LOG_TRACE,10);
	/*
	IBaseFilter* pFilter;
	pGraph->FindFilterByName(L"NVIDIA Transport Demux",&pFilter);
	IEnumPins *pinEnum;
	if (SUCCEEDED(pFilter->EnumPins(&pinEnum)))
	{
		if (pinEnum!=NULL)
		{
			pinEnum->Reset();
			ULONG fetched;
			IPin* pin;
			while (SUCCEEDED(pinEnum->Next(1,&pin,&fetched)))
			{
				if (fetched==1 && pin!=NULL)
				{
					
					PIN_INFO info;
					PIN_DIRECTION dir;
					pin->QueryDirection(&dir);
					if (dir==PINDIR_OUTPUT)
					{
						pin->QueryPinInfo(&info);
						AM_MEDIA_TYPE pmt;
						pin->ConnectionMediaType(&pmt);
						DisplayType((LPTSTR)info.achName,&pmt);
						pin->Release();
						MPEG2VIDEOINFO* mp=(MPEG2VIDEOINFO *)pmt.pbFormat;
						WAVEFORMATEX* mpa=(WAVEFORMATEX*)pmt.pbFormat;
						int xx=1;
					}
				}
				else break;
			}
			pinEnum->Release();
		}
	}*/
	//DbgOutString("demuxer:");
	//DumpGraph(pGraph,5);
	if (m_pVideo!=NULL)
	{
		AM_MEDIA_TYPE pmt;
		m_pVideo->ConnectionMediaType(&pmt);
	//	DisplayType("video media:",&pmt);
	}
	if (m_pAudio!=NULL)
	{
		AM_MEDIA_TYPE pmt;
		m_pAudio->ConnectionMediaType(&pmt);
	//	DisplayType("audio media:",&pmt);
	}
}

HRESULT SplitterSetup::GetAC3Media(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	LogDebug("audio media type: audio / ac3");
	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_DOLBY_AC3;
	pintype->cbFormat = sizeof(WAVEFORMATEX);
	pintype->pbFormat = (BYTE*)&m_ac3Hdr;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->formattype = FORMAT_WaveFormatEx;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT SplitterSetup::GetMP2Media(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	LogDebug("audio media type: audio / MPEG2 audio");
	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_MPEG2_AUDIO; //MEDIASUBTYPE_MPEG1Payload;
	pintype->formattype = FORMAT_WaveFormatEx; //FORMAT_None; //
	pintype->cbFormat = sizeof(g_MPEG1AudioFormat); //Mpeg2ProgramVideo
	pintype->pbFormat = g_MPEG1AudioFormat; //;Mpeg2ProgramVideo
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}
HRESULT SplitterSetup::GetAudioPayload(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	LogDebug("audio media type: audio / mpeg1 payload");
	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_MPEG1Payload; //MEDIASUBTYPE_MPEG1Payload;
	pintype->formattype = FORMAT_WaveFormatEx; //FORMAT_None; //
	pintype->cbFormat = sizeof(g_MPEG1AudioFormat); //Mpeg2ProgramVideo
	pintype->pbFormat = g_MPEG1AudioFormat; //;Mpeg2ProgramVideo
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT SplitterSetup::GetMP1Media(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	LogDebug("audio media type: audio / mpeg1 payload");
	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_MPEG1Payload;
	pintype->formattype = FORMAT_WaveFormatEx; //FORMAT_None; //
	pintype->cbFormat = sizeof(MPEG1AudioFormat);
	pintype->pbFormat = MPEG1AudioFormat;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 1;
	pintype->pUnk = NULL;

	return S_OK;
}

HRESULT SplitterSetup::GetVideoMedia(AM_MEDIA_TYPE *pintype)

{
	HRESULT hr = E_INVALIDARG;
	if(pintype == NULL){return hr;}
	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	if(m_pSections->pids.MPEG4==false)
	{
		LogDebug("video media type: video / mpeg2 video");
		pintype->majortype = MEDIATYPE_Video;
		pintype->subtype = MEDIASUBTYPE_MPEG2_VIDEO;
		pintype->bFixedSizeSamples = TRUE;
		pintype->bTemporalCompression = 0;
		pintype->lSampleSize = 1;
		pintype->formattype = FORMAT_MPEG2Video;//MPEG2VIDEOINFO 
		pintype->pUnk = NULL;
		pintype->cbFormat = sizeof(MPEG2VIDEOINFO);
		pintype->pbFormat = (BYTE*)&m_videoHdr;//Mpeg2ProgramVideo;
	}
	else
	{
		LogDebug("video media type: video / mpeg4 video");
		pintype->majortype = MEDIATYPE_Video;
		pintype->subtype = m_pSections->pids.idMPEG4;
		pintype->bFixedSizeSamples = TRUE;
		pintype->bTemporalCompression = 0;
		pintype->lSampleSize = 0;
		pintype->formattype = FORMAT_MPEG2Video;
		pintype->pUnk = NULL;
		pintype->cbFormat = sizeof(MPEG2VIDEOINFO);
		pintype->pbFormat = (BYTE*)&m_videoHdr;//Mpeg2ProgramVideo;
	}
	return S_OK;
}
