/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#include <streams.h>
#include <bdaiface.h>
#include "SplitterSetup.h"
#include <commctrl.h>
#include <atlbase.h>

SplitterSetup::SplitterSetup(Sections *pSections) :
m_demuxSetupComplete(FALSE)
{
	m_pSections = pSections;
	m_pAudio=NULL;
	m_pVideo=NULL;
}

SplitterSetup::~SplitterSetup()
{
}

HRESULT SplitterSetup::SetDemuxPins(IFilterGraph *pGraph)
{

	if(m_demuxSetupComplete==true)
		return S_FALSE;

	if(pGraph==NULL)
		return S_FALSE;

	HRESULT hr;
	IGraphBuilder *pGB=NULL;

	if(FAILED(pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGB)))
	{
		return S_FALSE;
	}

	IBaseFilter *pDemuxer;
	hr=pGB->FindFilterByName(L"MPEG-2 Demultiplexer",&pDemuxer);
	if(FAILED(hr))
	{
		pGB->Release();
		return hr;
	}

	hr=SetupDemuxer(pDemuxer);
	pGB->Release();
	return NOERROR;
}
HRESULT SplitterSetup::SetupDemuxer(IBaseFilter *demuxFilter)
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	HRESULT hr=0;

	if(demuxFilter==NULL)
		return S_FALSE;

	IMpeg2Demultiplexer *demuxer=NULL;

	hr=demuxFilter->QueryInterface(IID_IMpeg2Demultiplexer,(void**)&demuxer);
	if(FAILED(hr))
		return hr;
	// create pins on demuxer
	// audio
	AM_MEDIA_TYPE type;

	// video 
	if (m_pSections->pids.VideoPid>0)
	{
		ZeroMemory(&type, sizeof(AM_MEDIA_TYPE));
		GetVideoMedia(&type);
		hr=demuxer->CreateOutputPin(&type, L"Video",&m_pVideo);
	}
	int audioToUse=(m_pSections->pids.AudioPid>0?m_pSections->pids.AudioPid:m_pSections->pids.AC3);
	if(audioToUse>0)
	{
		ZeroMemory(&type, sizeof(AM_MEDIA_TYPE));
		if(m_pSections->pids.VideoPid>0)
		{
			if(audioToUse==m_pSections->pids.AudioPid)
				GetMP2Media(&type);// tv
			else 
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
	if (m_pVideo!=NULL && m_pSections->pids.VideoPid>0)
	{

		hr=m_pVideo->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
			return 1;
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
			return 5;
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
				return 6;
		}
		pPidEnum->Release();
		// map new pid
		pid = (ULONG)m_pSections->pids.VideoPid;
		hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM);
		if(FAILED(hr))
			return 2;
		pMap->Release();
	}
	
	// audio 
	int audioToUse=(m_pSections->pids.AudioPid>0?m_pSections->pids.AudioPid:m_pSections->pids.AC3);
	if (m_pAudio!=NULL)
	{

		hr=m_pAudio->QueryInterface(IID_IMPEG2PIDMap,(void**)&pMap);
		if(FAILED(hr) || pMap==NULL)
			return 3;
		// 
		hr=pMap->EnumPIDMap(&pPidEnum);
		if(FAILED(hr) || pPidEnum==NULL)
			return 7;
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
				return 8;
		}
		pPidEnum->Release();
		pid = (ULONG)audioToUse;
		if(m_pSections->pids.VideoPid>0)
			hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM); // tv
		else
		{
			if(m_pSections->pids.PCRPid==0 || m_pSections->pids.PCRPid>=0x1FFF)
			{
				hr=pMap->MapPID(1,&pid,MEDIA_TRANSPORT_PAYLOAD); // radio
			}
			else
			{
				hr=pMap->MapPID(1,&pid,MEDIA_ELEMENTARY_STREAM); // radio
			}
		}

		if(FAILED(hr))
			return 4;

		pMap->Release();
	}
	return S_OK;

}

HRESULT SplitterSetup::GetAC3Media(AM_MEDIA_TYPE *pintype)
{
	HRESULT hr = E_INVALIDARG;

	if(pintype == NULL){return hr;}

	ZeroMemory(pintype, sizeof(AM_MEDIA_TYPE));
	pintype->majortype = MEDIATYPE_Audio;
	pintype->subtype = MEDIASUBTYPE_DOLBY_AC3;
	pintype->cbFormat = sizeof(MPEG1AudioFormat);
	pintype->pbFormat = MPEG1AudioFormat;
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
	pintype->majortype = MEDIATYPE_Video;
	pintype->subtype = MEDIASUBTYPE_MPEG2_VIDEO;
	pintype->bFixedSizeSamples = TRUE;
	pintype->bTemporalCompression = 0;
	pintype->lSampleSize = 0;
	pintype->formattype = FORMAT_MPEG2Video;
	pintype->pUnk = NULL;
	pintype->cbFormat = sizeof(Mpeg2ProgramVideo);
	pintype->pbFormat = Mpeg2ProgramVideo;

	return S_OK;
}