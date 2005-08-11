/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Agree
 *	http://mediaportal.sourceforge.net
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


#include "stdafx.h"
#include "dshowhelper.h"
#include "DX9AllocatorPresenter.h"
#include <map>
#include <comutil.h>
using namespace std;

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

// This is an example of an exported variable
DSHOWHELPER_API int ndshowhelper=0;

// This is an example of an exported function.
DSHOWHELPER_API int fndshowhelper(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see dshowhelper.h for the class definition
Cdshowhelper::Cdshowhelper()
{ 
	return; 
}

LPDIRECT3DDEVICE9			m_pDevice=NULL;
CVMR9AllocatorPresenter*	vmr9Presenter=NULL;
IBaseFilter*				m_pVMR9Filter=NULL;
IVMRSurfaceAllocator9*		g_allocator=NULL;
LONG						m_iRecordingId=0;

map<int,IStreamBufferRecordControl*> m_mapRecordControl;
typedef map<int,IStreamBufferRecordControl*>::iterator imapRecordControl;
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


BOOL Vmr9Init(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter* vmr9Filter,DWORD monitor)
{
	HRESULT hr;
	m_pDevice = (LPDIRECT3DDEVICE9)(dwD3DDevice);
	m_pVMR9Filter=vmr9Filter;
	
	CComQIPtr<IVMRFilterConfig9> pConfig = m_pVMR9Filter;
	if(!pConfig)
		return FALSE;

	if(FAILED(hr = pConfig->SetRenderingMode(VMR9Mode_Renderless)))
	{
		Log("Vmr9:Init() SetRenderingMode() failed 0x:%x",hr);
		return FALSE;
	}
	if(FAILED(hr = pConfig->SetNumberOfStreams(1)))
	{
		Log("Vmr9:Init() SetNumberOfStreams() failed 0x:%x",hr);
		return FALSE;
	}

	CComQIPtr<IVMRSurfaceAllocatorNotify9> pSAN = m_pVMR9Filter;
	if(!pSAN)
		return FALSE;

	vmr9Presenter = new CVMR9AllocatorPresenter( m_pDevice, callback,(HMONITOR)monitor) ;
	vmr9Presenter->QueryInterface(IID_IVMRSurfaceAllocator9,(void**)&g_allocator);

	if(FAILED(hr = pSAN->AdviseSurfaceAllocator(MY_USER_ID, g_allocator)))
	{
		Log("Vmr9:Init() AdviseSurfaceAllocator() failed 0x:%x",hr);
		return FALSE;
	}
	if (FAILED(hr = g_allocator->AdviseNotify(pSAN)))
	{
		Log("Vmr9:Init() AdviseNotify() failed 0x:%x",hr);
		return FALSE;
	}
	return TRUE;
}
void Vmr9Deinit()
{

	int hr;
	if (g_allocator!=NULL)
	{
		hr=g_allocator->Release();
		g_allocator=NULL;
		Log("Vmr9Deinit:allocator release:%d", hr);
	}
	if (vmr9Presenter!=NULL)
	{
		delete vmr9Presenter;
		Log("Vmr9Deinit:vmr9Presenter release:%d", hr);
	}
	vmr9Presenter=NULL;

	m_pDevice=NULL;
	m_pVMR9Filter=NULL;

}
void Vmr9SetDeinterlaceMode(int mode)
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

		return ;
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

		return ;
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
		
		return ;
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
		return ;
	}
	if ((pmt.formattype != FORMAT_VideoInfo2) || (pmt.cbFormat< sizeof(VIDEOINFOHEADER2)))
	{
		Log("vmr9:SetDeinterlace() not using VIDEOINFOHEADER2");
		
		return ;
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
}
void Vmr9SetDeinterlacePrefs(DWORD dwMethod)
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

}


bool DvrMsCreate(LONG *id, IBaseFilter* streamBufferSink, LPCWSTR strPath, DWORD dwRecordingType)
{
	*id=-1;
	try
	{

		Log("CStreamBufferRecorder::Create() Record");
		int hr;
		IUnknown* pRecUnk;
		CComQIPtr<IStreamBufferSink> sink=streamBufferSink;
		if (!sink)
		{
			Log("cannot get IStreamBufferSink:%x");
			return false;
		}
		hr=sink->CreateRecorder( strPath,dwRecordingType,&pRecUnk);
		if (!SUCCEEDED(hr))
		{
			Log("CStreamBufferRecorder::Create() create recorder failed:%x", hr);
			return false;
		}

		if (pRecUnk==NULL)
		{
			Log("CStreamBufferRecorder::Create() cannot get recorder failed");
			return false;
		}
		IStreamBufferRecordControl* pRecord;
		pRecUnk->QueryInterface(IID_IStreamBufferRecordControl,(void**)&pRecord);
		if (pRecord==NULL)
		{
			Log("CStreamBufferRecorder::Create() cannot get IStreamBufferRecordControl");
			return false;
		}
		*id=m_iRecordingId;
		Log("CStreamBufferRecorder::Create() recorder created id:%d", *id);
		m_mapRecordControl[m_iRecordingId]=pRecord;
		m_iRecordingId++;

		Log("CStreamBufferRecorder::Create() done %x",pRecord);	
	}
	catch(...)
	{
		Log("CStreamBufferRecorder::Create() done exception");	
	}
	return TRUE;
}
void DvrMsStart(LONG id, LONG startTime)
{
	imapRecordControl it = m_mapRecordControl.find(id);
	if (it==m_mapRecordControl.end()) return;

	try
	{
		Log("CStreamBufferRecorder::Start():%d id:%d", startTime,id);
		if (it->second==NULL)
		{
			Log("CStreamBufferRecorder::Start() recorded=null");
			return ;
		}
		Log("CStreamBufferRecorder::Start(1) %x",m_mapRecordControl[id]);
		REFERENCE_TIME timeStart=startTime;
		int hr=it->second->Start(&timeStart);
		if (!SUCCEEDED(hr))
		{
			Log("CStreamBufferRecorder::Start() start failed:%X", hr);
			if (startTime!=0)
			{
				timeStart=0;
				int hr=it->second->Start(&timeStart);
				if (!SUCCEEDED(hr))
				{
					Log("CStreamBufferRecorder::Start() start failed:%x", hr);
					return ;
				}
			}
		}
		
		Log("CStreamBufferRecorder::Start(2)");
		HRESULT hrOut;
		BOOL started,stopped;
		it->second->GetRecordingStatus(&hrOut,&started,&stopped);
		Log("CStreamBufferRecorder::Start() start status:%x started:%d stopped:%d", hrOut,started,stopped);
		Log("CStreamBufferRecorder::Start() done");
	}
	catch(...)
	{
		Log("CStreamBufferRecorder::Start()  exception");
	}
}

void DvrMsStop(LONG id)
{
	imapRecordControl it = m_mapRecordControl.find(id);
	if (it==m_mapRecordControl.end()) return;
	try
	{
		Log("CStreamBufferRecorder::Stop()");
		if (it->second!=NULL)
		{
			int hr=it->second->Stop(0);
			if (!SUCCEEDED(hr))
			{
				Log("CStreamBufferRecorder::Stop() failed:%x", hr);
				return ;
			}
			for (int x=0; x < 10;++x)
			{
				HRESULT hrOut;
				BOOL started,stopped;
				it->second->GetRecordingStatus(&hrOut,&started,&stopped);
				Log("CStreamBufferRecorder::Stop() status:%d %x started:%d stopped:%d",x, hrOut,started,stopped);
				if (stopped!=0) break;
				Sleep(100);
			}
			while (it->second->Release() >0);
			m_mapRecordControl.erase(it);

		}
		Log("CStreamBufferRecorder::Stop() done");
	}
	catch(...)
	{
		Log("CStreamBufferRecorder::Stop() exception");
	}

}