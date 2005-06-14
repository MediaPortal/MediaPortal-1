// WmvHelper.cpp : Implementation of CWmvHelper

#include "stdafx.h"
#include "WmvHelper.h"
#include "wmsdk.h"
#include <dshowasf.h>
#include <comdef.h>
// CWmvHelper
void Log(const char *fmt, ...) ;

STDMETHODIMP CWmvHelper::SetProfile(IConfigAsfWriter* asfWriter, ULONG bitrate, ULONG fps, ULONG screenX, ULONG screenY)
{

	Log("WMV:SetProfile (%d, %d %x)",screenX,screenY,asfWriter);
	CComPtr<IConfigAsfWriter>	m_pAsfWriter;
	m_pAsfWriter.Attach( asfWriter);
	if (m_pAsfWriter==NULL)
	{
		Log("WMV:could not get IConfigAsfWriter");
		return S_OK;
	}
	//Log("WMV:Get IWMProfile");
	IWMProfile* profile;
	m_pAsfWriter->GetCurrentProfile(&profile);
	if(!profile)
	{
		Log("WMV:could not Get IWMProfile");
		return S_OK;
	}
	DWORD NoOfStreams;
	HRESULT hr;
	hr=profile->GetStreamCount(&NoOfStreams);
	DWORD dwLen=256;
	WCHAR pwszName[256];
	profile->GetName(pwszName,&dwLen);
	Log("WMV:profile:%s",(char*)(_bstr_t(pwszName)));
	
	

	Log("WMV:streams:%d",NoOfStreams);
	int overhead=9000;
	int audioBitRate=44100;
	int videoBitRate=bitrate*1000;
	videoBitRate-=audioBitRate;
	videoBitRate-=overhead;

	for (int i=0; i < NoOfStreams;++i)
	{
		Log("WMV:getStream:%d",i);
		IWMStreamConfig* streamConfig;
		hr=profile->GetStream(i, &streamConfig);
		if (hr!=0) Log("WMV:  getStream returned:0x%x",hr);

		GUID streamType;
		hr=streamConfig->GetStreamType(&streamType);
		if (hr!=0) Log("WMV:  GetStreamType returned:0x%x",hr);

		CComQIPtr<IWMMediaProps>  mediaProps = streamConfig;
		WM_MEDIA_TYPE* mediaType;
		DWORD          mediaTypeLen;
		hr=mediaProps->GetMediaType(NULL,&mediaTypeLen);
		if (hr!=0) Log("WMV:  GetMediaType len:%d",mediaTypeLen);

		char* buffer = new char[mediaTypeLen];
		hr=mediaProps->GetMediaType((WM_MEDIA_TYPE*)buffer,&mediaTypeLen);
		mediaType=(WM_MEDIA_TYPE*)buffer;
		//Log("WMV:  GetMediaType ok");

		DWORD currentBitrate;
		hr=streamConfig->GetBitrate(&currentBitrate);
		if (streamType==WMMEDIATYPE_Video)
		{
			Log("WMV:  stream %d is video bitrate:%d kbps->%d", i,currentBitrate, videoBitRate);
			//hr=streamConfig->SetBitrate(videoBitRate);
			if (hr!=0) Log("WMV:  SetBitrate returned 0x%x",hr);
			if (mediaType->formattype==WMFORMAT_VideoInfo)
			{
				WMVIDEOINFOHEADER *pHeader = (WMVIDEOINFOHEADER *)mediaType->pbFormat;
				Log("WMV:  GetMediaType (%d,%d) fps:%d ", pHeader->bmiHeader.biWidth,pHeader->bmiHeader.biHeight,pHeader->AvgTimePerFrame/10000000LL);
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
				Log("WMV:  SetMediaType (%d,%d) fps:%d returned 0x%x", screenX,screenY,fps,hr);

			}
		}
		else if (streamType==WMMEDIATYPE_Audio)
		{
			Log("WMV:  stream %d is audio bitrate:%d kbps->%d", i,currentBitrate, audioBitRate);
			//hr=streamConfig->SetBitrate(audioBitRate);
			if (hr!=0) Log("WMV:  SetBitrate returned 0x%x",hr);
			
		}
		hr=profile->ReconfigStream(streamConfig);
		Log("WMV:  ReconfigStream returned 0x%x",hr);

	}
	Log("WMV:Set Deinterlace");
	CComPtr<IServiceProvider> pProvider;
	CComPtr<IWMWriterAdvanced2> pWMWA2;
	hr = m_pAsfWriter->QueryInterface( __uuidof(IServiceProvider),(void**)&pProvider);
	if (SUCCEEDED(hr))
	{
		hr = pProvider->QueryService(IID_IWMWriterAdvanced2,IID_IWMWriterAdvanced2,(void**)&pWMWA2);
		if (SUCCEEDED(hr))
		{
			DWORD pValue = WM_DM_DEINTERLACE_NORMAL;
			// Set the first parameter to your actual input number.
			hr = pWMWA2->SetInputSetting(0, g_wszDeinterlaceMode,WMT_TYPE_DWORD, (BYTE*) &pValue, sizeof(WMT_TYPE_BOOL));
			if (!SUCCEEDED(hr))
				Log("WMV:Could not get set deinterlace mode 0x%x",hr);
		}
		else Log("WMV:Could not get IWMWriterAdvanced2");
	}
	else Log("WMV:Could not get IServiceProvider");
	hr=m_pAsfWriter->ConfigureFilterUsingProfile(profile);
	if (!SUCCEEDED(hr))
		Log("WMV:ConfigureFilterUsingProfile returned:0x%x",hr);
	return S_OK;
}