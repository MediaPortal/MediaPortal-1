// VMR9Helper.cpp : Implementation of CVMR9Helper

#include "stdafx.h"
#include "VMR9Helper.h"
#include ".\vmr9helper.h"
#include <dvdmedia.h>


// CVMR9Helper
#define MY_USER_ID 0x6ABE51
#define IsInterlaced(x) ((x) & AMINTERLACE_IsInterlaced)
#define IsSingleField(x) ((x) & AMINTERLACE_1FieldPerSample)
#define IsField1First(x) ((x) & AMINTERLACE_Field1First)

#define INIT_GUID(name, l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8) \
		const GUID name \
				= { l, w1, w2, { b1, b2,  b3,  b4,  b5,  b6,  b7,  b8 } }


INIT_GUID(bobDxvaGuid,0x335aa36e,0x7884,0x43a4,0x9c,0x91,0x7f,0x87,0xfa,0xf3,0xe3,0x7e);

void Log(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("log/vmr9.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetSystemTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
};
VMR9_SampleFormat ConvertInterlaceFlags(DWORD dwInterlaceFlags)
{
    if (IsInterlaced(dwInterlaceFlags)) {
        if (IsSingleField(dwInterlaceFlags)) {
            if (IsField1First(dwInterlaceFlags)) {
                return VMR9_SampleFieldSingleEven;
            }
            else {
                return VMR9_SampleFieldSingleOdd;
            }
        }
        else {
            if (IsField1First(dwInterlaceFlags)) {
                return VMR9_SampleFieldInterleavedEvenFirst;
             }
            else {
                return VMR9_SampleFieldInterleavedOddFirst;
            }
        }
    }
    else {
        return VMR9_SampleProgressiveFrame;  // Not interlaced.
    }
}

STDMETHODIMP CVMR9Helper::Init(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter* vmr9Filter,DWORD monitor)
{	
	Log("Vmr9:Init()");
	HRESULT hr;
	m_pDevice = (LPDIRECT3DDEVICE9)(dwD3DDevice);
	m_pVMR9Filter.Attach(vmr9Filter);
	
	CComQIPtr<IVMRFilterConfig9> pConfig = m_pVMR9Filter;
	if(!pConfig)
		return E_FAIL;

	if(FAILED(hr = pConfig->SetRenderingMode(VMR9Mode_Renderless)))
	{
		Log("Vmr9:Init() SetRenderingMode() failed 0x:%x",hr);
		return E_FAIL;
	}
	if(FAILED(hr = pConfig->SetNumberOfStreams(1)))
	{
		Log("Vmr9:Init() SetNumberOfStreams() failed 0x:%x",hr);
		return E_FAIL;
	}

	CComQIPtr<IVMRSurfaceAllocatorNotify9> pSAN = m_pVMR9Filter;
	if(!pSAN)
		return E_FAIL;

	 vmr9AllocPresenter = new CVMR9AllocatorPresenter( m_pDevice, callback,(HMONITOR)monitor) ;
    g_allocator.Attach(vmr9AllocPresenter);

	if(FAILED(hr = pSAN->AdviseSurfaceAllocator(MY_USER_ID, g_allocator)))
	{
		Log("Vmr9:Init() AdviseSurfaceAllocator() failed 0x:%x",hr);
		return E_FAIL;
	}
	if (FAILED(hr = g_allocator->AdviseNotify(pSAN)))
	{
		Log("Vmr9:Init() AdviseNotify() failed 0x:%x",hr);
		return E_FAIL;
	}

/*	
	Log("Vmr9:Init() set YUV mixing mode");
	CComQIPtr<IVMRMixerControl9> pMixControl = m_pVMR9Filter;
	DWORD dwPrefs;
	pMixControl->GetMixingPrefs(&dwPrefs); 
	Log("Vmr9:Init() current mixing preferences:%x",dwPrefs); 

	// Remove the current render target flag.
	dwPrefs &= ~MixerPref_RenderTargetMask; 

	// Add the render target flag that we want.
	dwPrefs |= MixerPref_RenderTargetYUV;

	// Set the new flags.
	if (FAILED(hr = pMixControl->SetMixingPrefs(dwPrefs)))
	{
		Log("Vmr9:Init() cannot use YUV mixing mode 0x:%x",hr);
	}
*/
	return S_OK;
}
	
STDMETHODIMP CVMR9Helper::Deinit(void)
{
	if (vmr9AllocPresenter!=NULL)
		vmr9AllocPresenter->ReleaseCallBack();
	m_pDevice=NULL;
	vmr9AllocPresenter=NULL;
	return S_OK;
}
STDMETHODIMP CVMR9Helper::SetDeinterlacePrefs(DWORD dwMethod)
{
	int hr;
	CComQIPtr<IVMRDeinterlaceControl9> pDeint=m_pVMR9Filter;
	switch (dwMethod)
	{
		case 1:
			Log("vmr9:SetDeinterlace() preference to BOB");
			hr=pDeint->SetDeinterlacePrefs(DeinterlacePref9_BOB);
		break;
		case 2:
			Log("vmr9:SetDeinterlace() preference to Weave");
			hr=pDeint->SetDeinterlacePrefs(DeinterlacePref9_Weave);
		break;
		case 3:
			Log("vmr9:SetDeinterlace() preference to NextBest");
			hr=pDeint->SetDeinterlacePrefs(DeinterlacePref9_NextBest );
		break;
	}
	return S_OK;
}

STDMETHODIMP CVMR9Helper::SetDeinterlaceMode(int mode)
{
	//0=None
	//1=Bob
	//2=Weave
	//3=Best
	Log("vmr9:SetDeinterlace() SetDeinterlaceMode(%d)",mode);
	CComQIPtr<IVMRDeinterlaceControl9> pDeint=m_pVMR9Filter;
	VMR9VideoDesc VideoDesc; 
	DWORD dwNumModes = 0;
	GUID deintMode;
	int hr;
	if (mode==0)
	{
		//off
		hr=pDeint->SetDeinterlaceMode(0xFFFFFFFF,(LPGUID)&GUID_NULL);
		if (!SUCCEEDED(hr)) Log("vmr9:SetDeinterlace() failed hr:0x%x",hr);
		hr=pDeint->GetDeinterlaceMode(0,&deintMode);
		if (!SUCCEEDED(hr)) Log("vmr9:GetDeinterlaceMode() failed hr:0x%x",hr);
		Log("vmr9:SetDeinterlace() deinterlace mode OFF: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
				deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

		return S_OK;
	}
	if (mode==1)
	{
		//BOB

		hr=pDeint->SetDeinterlaceMode(0xFFFFFFFF,(LPGUID)&bobDxvaGuid);
		if (!SUCCEEDED(hr)) Log("vmr9:SetDeinterlace() failed hr:0x%x",hr);
		hr=pDeint->GetDeinterlaceMode(0,&deintMode);
		if (!SUCCEEDED(hr)) Log("vmr9:GetDeinterlaceMode() failed hr:0x%x",hr);
		Log("vmr9:SetDeinterlace() deinterlace mode BOB: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
				deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

		return S_OK;
	}
	if (mode==2)
	{
		//WEAVE
		hr=pDeint->SetDeinterlaceMode(0xFFFFFFFF,(LPGUID)&GUID_NULL);
		if (!SUCCEEDED(hr)) Log("vmr9:SetDeinterlace() failed hr:0x%x",hr);
		hr=pDeint->GetDeinterlaceMode(0,&deintMode);
		if (!SUCCEEDED(hr)) Log("vmr9:GetDeinterlaceMode() failed hr:0x%x",hr);
		Log("vmr9:SetDeinterlace() deinterlace mode WEAVE: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
				deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);
		return S_OK;
	}

	AM_MEDIA_TYPE pmt;
	ULONG fetched;
	IPin* pins[10];
	CComPtr<IEnumPins> pinEnum;
	hr=m_pVMR9Filter->EnumPins(&pinEnum);
	pinEnum->Reset();
	pinEnum->Next(1,&pins[0],&fetched);
	hr=pins[0]->ConnectionMediaType(&pmt);
	pins[0]->Release();

	VIDEOINFOHEADER2* vidInfo2 =(VIDEOINFOHEADER2*)pmt.pbFormat;
	if (vidInfo2==NULL)
	{
		Log("vmr9:SetDeinterlace() VMR9 not connected");
		return S_OK;
	}
	if ((pmt.formattype != FORMAT_VideoInfo2) || (pmt.cbFormat< sizeof(VIDEOINFOHEADER2)))
	{
		Log("vmr9:SetDeinterlace() not using VIDEOINFOHEADER2");
		return S_OK;
	}

	Log("vmr9:SetDeinterlace() resolution:%dx%d planes:%d bitcount:%d fmt:%d %c%c%c%c",
		vidInfo2->bmiHeader.biWidth,vidInfo2->bmiHeader.biHeight,
		vidInfo2->bmiHeader.biPlanes,
		vidInfo2->bmiHeader.biBitCount,
		vidInfo2->bmiHeader.biCompression,
		(char)(vidInfo2->bmiHeader.biCompression&0xff),
		(char)((vidInfo2->bmiHeader.biCompression>>8)&0xff),
		(char)((vidInfo2->bmiHeader.biCompression>>16)&0xff),
		(char)((vidInfo2->bmiHeader.biCompression>>24)&0xff)
		);
	char major[128];
	char subtype[128];
	strcpy(major,"unknown");
	sprintf(subtype,"unknown (0x%x-0x%x-0x%x-0x%x)",pmt.subtype.Data1,pmt.subtype.Data2,pmt.subtype.Data3,pmt.subtype.Data4);
	if (pmt.majortype==MEDIATYPE_AnalogVideo)
		strcpy(major,"Analog video");
	if (pmt.majortype==MEDIATYPE_Video)
		strcpy(major,"video");
	if (pmt.majortype==MEDIATYPE_Stream)
		strcpy(major,"stream");

	if (pmt.subtype==MEDIASUBTYPE_MPEG2_VIDEO)
		strcpy(subtype,"mpeg2 video");
	if (pmt.subtype==MEDIASUBTYPE_MPEG1System)
		strcpy(subtype,"mpeg1 system");
	if (pmt.subtype==MEDIASUBTYPE_MPEG1VideoCD)
		strcpy(subtype,"mpeg1 videocd");

	if (pmt.subtype==MEDIASUBTYPE_MPEG1Packet)
		strcpy(subtype,"mpeg1 packet");
	if (pmt.subtype==MEDIASUBTYPE_MPEG1Payload )
		strcpy(subtype,"mpeg1 payload");
//	if (pmt.subtype==MEDIASUBTYPE_ATSC_SI)
//		strcpy(subtype,"ATSC SI");
//	if (pmt.subtype==MEDIASUBTYPE_DVB_SI)
//		strcpy(subtype,"DVB SI");
//	if (pmt.subtype==MEDIASUBTYPE_MPEG2DATA)
//		strcpy(subtype,"MPEG2 Data");
	if (pmt.subtype==MEDIASUBTYPE_MPEG2_TRANSPORT)
		strcpy(subtype,"MPEG2 Transport");
	if (pmt.subtype==MEDIASUBTYPE_MPEG2_PROGRAM)
		strcpy(subtype,"MPEG2 Program");
	
	if (pmt.subtype==MEDIASUBTYPE_CLPL)
		strcpy(subtype,"MEDIASUBTYPE_CLPL");
	if (pmt.subtype==MEDIASUBTYPE_YUYV)
		strcpy(subtype,"MEDIASUBTYPE_YUYV");
	if (pmt.subtype==MEDIASUBTYPE_IYUV)
		strcpy(subtype,"MEDIASUBTYPE_IYUV");
	if (pmt.subtype==MEDIASUBTYPE_YVU9)
		strcpy(subtype,"MEDIASUBTYPE_YVU9");
	if (pmt.subtype==MEDIASUBTYPE_Y411)
		strcpy(subtype,"MEDIASUBTYPE_Y411");
	if (pmt.subtype==MEDIASUBTYPE_Y41P)
		strcpy(subtype,"MEDIASUBTYPE_Y41P");
	if (pmt.subtype==MEDIASUBTYPE_YUY2)
		strcpy(subtype,"MEDIASUBTYPE_YUY2");
	if (pmt.subtype==MEDIASUBTYPE_YVYU)
		strcpy(subtype,"MEDIASUBTYPE_YVYU");
	if (pmt.subtype==MEDIASUBTYPE_UYVY)
		strcpy(subtype,"MEDIASUBTYPE_UYVY");
	if (pmt.subtype==MEDIASUBTYPE_Y211)
		strcpy(subtype,"MEDIASUBTYPE_Y211");
	if (pmt.subtype==MEDIASUBTYPE_RGB565)
		strcpy(subtype,"MEDIASUBTYPE_RGB565");
	if (pmt.subtype==MEDIASUBTYPE_RGB32)
		strcpy(subtype,"MEDIASUBTYPE_RGB32");
	if (pmt.subtype==MEDIASUBTYPE_ARGB32)
		strcpy(subtype,"MEDIASUBTYPE_ARGB32");
	if (pmt.subtype==MEDIASUBTYPE_RGB555)
		strcpy(subtype,"MEDIASUBTYPE_RGB555");
	if (pmt.subtype==MEDIASUBTYPE_RGB24)
		strcpy(subtype,"MEDIASUBTYPE_RGB24");
	if (pmt.subtype==MEDIASUBTYPE_AYUV)
		strcpy(subtype,"MEDIASUBTYPE_AYUV");
	if (pmt.subtype==MEDIASUBTYPE_YV12)
		strcpy(subtype,"MEDIASUBTYPE_YV12");
//	if (pmt.subtype==MEDIASUBTYPE_NV12)
//		strcpy(subtype,"MEDIASUBTYPE_NV12");
	Log("vmr9:SetDeinterlace() major:%s subtype:%s", major,subtype);
	VideoDesc.dwSize = sizeof(VMR9VideoDesc);
	VideoDesc.dwFourCC=vidInfo2->bmiHeader.biCompression;
	VideoDesc.dwSampleWidth=vidInfo2->bmiHeader.biWidth;
	VideoDesc.dwSampleHeight=vidInfo2->bmiHeader.biHeight;
	VideoDesc.SampleFormat=ConvertInterlaceFlags(vidInfo2->dwInterlaceFlags);
	VideoDesc.InputSampleFreq.dwDenominator=(DWORD)vidInfo2->AvgTimePerFrame;
	VideoDesc.InputSampleFreq.dwNumerator=10000000;
	VideoDesc.OutputFrameFreq.dwDenominator=(DWORD)vidInfo2->AvgTimePerFrame;
	VideoDesc.OutputFrameFreq.dwNumerator=VideoDesc.InputSampleFreq.dwNumerator;
	if (VideoDesc.SampleFormat != VMR9_SampleProgressiveFrame)
	{
		VideoDesc.OutputFrameFreq.dwNumerator=2*VideoDesc.InputSampleFreq.dwNumerator;
	}

	// Fill in the VideoDesc structure (not shown).
	hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, NULL);
	if (SUCCEEDED(hr) && (dwNumModes != 0))
	{
		// Allocate an array for the GUIDs that identify the modes.
		GUID *pModes = new GUID[dwNumModes];
		if (pModes)
		{
			Log("vmr9:SetDeinterlace() found %d deinterlacing modes", dwNumModes);
			// Fill the array.
			hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, pModes);
			if (SUCCEEDED(hr))
			{
				// Loop through each item and get the capabilities.
				for (int i = 0; i < dwNumModes; i++)
				{
					hr=pDeint->SetDeinterlaceMode(0xFFFFFFFF,&pModes[0]);
					if (SUCCEEDED(hr)) 
					{
						Log("vmr9:SetDeinterlace() set deinterlace mode:%d",i);

						
						
						pDeint->GetDeinterlaceMode(0,&deintMode);
						if (deintMode.Data1==pModes[0].Data1 &&
							deintMode.Data2==pModes[0].Data2 &&
							deintMode.Data3==pModes[0].Data3 &&
							deintMode.Data4==pModes[0].Data4)
						{
							Log("vmr9:SetDeinterlace() succeeded");
						}
						else
							Log("vmr9:SetDeinterlace() deinterlace mode set to: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
									deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);
						break;
					}
					else
						Log("vmr9:SetDeinterlace() deinterlace mode:%d failed 0x:%x",i,hr);

				}
			}
			delete [] pModes;
		}
	}



	return S_OK;
}