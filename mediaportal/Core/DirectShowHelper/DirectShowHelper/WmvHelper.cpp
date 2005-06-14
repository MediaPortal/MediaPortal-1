// WmvHelper.cpp : Implementation of CWmvHelper

#include "stdafx.h"
#include "WmvHelper.h"
#include "wmsdk.h"
#include <dshowasf.h>
#include <comdef.h>
// CWmvHelper
void Log(const char *fmt, ...) ;

STDMETHODIMP CWmvHelper::SetProfile(IBaseFilter* asfWriter, ULONG bitrate, ULONG fps, ULONG screenX, ULONG screenY)
{

	//Log("SetProfile (%d, %d)",screenX,screenY);
	CComQIPtr<IConfigAsfWriter>	m_pAsfWriter = asfWriter;
	if (m_pAsfWriter==NULL)
	{
		Log("could not get asfWriter");
		return S_OK;
	}
	//Log("Get IWMProfile");
	IWMProfile* profile;
	m_pAsfWriter->GetCurrentProfile(&profile);
	if(!profile)
	{
		Log("could not Get IWMProfile");
		return S_OK;
	}
	DWORD NoOfStreams;
	HRESULT hr;
	hr=profile->GetStreamCount(&NoOfStreams);
	DWORD dwLen=256;
	WCHAR pwszName[256];
	profile->GetName(pwszName,&dwLen);
	Log("profile:%s",(char*)(_bstr_t(pwszName)));
	
	

	Log("streams:%d",NoOfStreams);
	int overhead=9000;
	int audioBitRate=44100;
	int videoBitRate=bitrate*1000;
	videoBitRate-=audioBitRate;
	videoBitRate-=overhead;

	for (int i=0; i < NoOfStreams;++i)
	{
		Log("getStream:%d",i);
		IWMStreamConfig* streamConfig;
		hr=profile->GetStream(i, &streamConfig);
		if (hr!=0) Log("  getStream returned:0x%x",hr);

		GUID streamType;
		hr=streamConfig->GetStreamType(&streamType);
		if (hr!=0) Log("  GetStreamType returned:0x%x",hr);

		CComQIPtr<IWMMediaProps>  mediaProps = streamConfig;
		WM_MEDIA_TYPE* mediaType;
		DWORD          mediaTypeLen;
		hr=mediaProps->GetMediaType(NULL,&mediaTypeLen);
		if (hr!=0) Log("  GetMediaType len:%d",mediaTypeLen);

		char* buffer = new char[mediaTypeLen];
		hr=mediaProps->GetMediaType((WM_MEDIA_TYPE*)buffer,&mediaTypeLen);
		mediaType=(WM_MEDIA_TYPE*)buffer;
		//Log("  GetMediaType ok");

		DWORD currentBitrate;
		hr=streamConfig->GetBitrate(&currentBitrate);
		if (streamType==WMMEDIATYPE_Video)
		{
			Log("  stream %d is video bitrate:%d kbps->%d", i,currentBitrate, videoBitRate);
			//hr=streamConfig->SetBitrate(videoBitRate);
			if (hr!=0) Log("  SetBitrate returned 0x%x",hr);
			if (mediaType->formattype==WMFORMAT_VideoInfo)
			{
				WMVIDEOINFOHEADER *pHeader = (WMVIDEOINFOHEADER *)mediaType->pbFormat;
				Log("  GetMediaType (%d,%d) fps:%d ", pHeader->bmiHeader.biWidth,pHeader->bmiHeader.biHeight,pHeader->AvgTimePerFrame/10000000LL);
				pHeader->rcSource.left=0;
				pHeader->rcSource.top=0;
				pHeader->rcSource.right=screenX;
				pHeader->rcSource.bottom=screenY;
				
				pHeader->rcTarget.left=0;
				pHeader->rcTarget.top=0;
				pHeader->rcTarget.right=screenX;
				pHeader->rcTarget.bottom=screenY;
				pHeader->AvgTimePerFrame=fps*10000000LL;
				pHeader->bmiHeader.biWidth=screenX;
				pHeader->bmiHeader.biHeight=screenY;
				hr=mediaProps->SetMediaType(mediaType);
				Log("  SetMediaType (%d,%d) fps:%d returned 0x%x", screenX,screenY,fps,hr);

			}
		}
		else if (streamType==WMMEDIATYPE_Audio)
		{
			Log("  stream %d is audio bitrate:%d kbps->%d", i,currentBitrate, audioBitRate);
			//hr=streamConfig->SetBitrate(audioBitRate);
			if (hr!=0) Log("  SetBitrate returned 0x%x",hr);
			
		}
		hr=profile->ReconfigStream(streamConfig);
		Log("  ReconfigStream returned 0x%x",hr);

	}
	hr=m_pAsfWriter->ConfigureFilterUsingProfile(profile);
	Log("ConfigureFilterUsingProfile returned:0x%x",hr);
	return S_OK;
}