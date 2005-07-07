// StreamBufferRecorder.cpp : Implementation of CStreamBufferRecorder

#include "stdafx.h"
#include "StreamBufferRecorder.h"
#include <comutil.h>
#include ".\streambufferrecorder.h"


// CStreamBufferRecorder
extern void Log(const char *fmt, ...) ;


void CStreamBufferRecorder::FinalRelease() 
{
	Log("CStreamBufferRecorder::FinalRelease()");
}
STDMETHODIMP CStreamBufferRecorder::Create(IBaseFilter* streamBufferSink,BSTR strPath, DWORD dwRecordingType)
{
	try
	{
		_bstr_t bstrPath(strPath,true);

		Log("CStreamBufferRecorder::Record %s", (char*)bstrPath);
		int hr;
		CComPtr<IUnknown> pRecUnk;
		CComQIPtr<IStreamBufferSink> sink=streamBufferSink;
		if (!sink)
		{
			Log("cannot get IStreamBufferSink:%x", hr);
			return E_FAIL;
		}
		Log("CStreamBufferRecorder::CreateRecorder");

		hr=sink->CreateRecorder( (LPCWSTR)bstrPath,dwRecordingType,&pRecUnk);
		if (!SUCCEEDED(hr))
		{
			Log("create recorded failed:%x", hr);
			return E_FAIL;
		}

		m_recordControl=pRecUnk;
		if (!m_recordControl)
		{
			Log("cannot get recorder failed:%x", hr);
			return E_FAIL;
		}
		

		Log("CStreamBufferRecorder::done");	
	}
	catch(...)
	{
		Log("CStreamBufferRecorder::done exception");	
	}
	return S_OK;
}
STDMETHODIMP CStreamBufferRecorder::Start(LONG startTime)
{
	try
	{
		Log("CStreamBufferRecorder::Start:%d", startTime);
		if (m_recordControl==NULL)
		{
			Log("cannot get recorder ");
			return E_FAIL;
		}
		REFERENCE_TIME timeStart=startTime;
		int hr=m_recordControl->Start(&timeStart);
		if (!SUCCEEDED(hr))
		{
			Log("CStreamBufferRecorder:: start1 failed:%X", hr);
			if (startTime!=0)
			{
				timeStart=0;
				int hr=m_recordControl->Start(&timeStart);
				if (!SUCCEEDED(hr))
				{
					Log("start failed:%x", hr);
					return E_FAIL;
				}
			}
		}
		
		HRESULT hrOut;
		BOOL started,stopped;
		m_recordControl->GetRecordingStatus(&hrOut,&started,&stopped);
		Log("rec status:%x started:%d stopped:%d", hrOut,started,stopped);
		Log("CStreamBufferRecorder:: started");
	}
	catch(...)
	{
		Log("CStreamBufferRecorder:: start exception");
	}
	return S_OK;
}
STDMETHODIMP CStreamBufferRecorder::Stop(void)
{
	
	try
	{
		Log("CStreamBufferRecorder:: Stop");
		if (m_recordControl!=NULL)
		{
			int hr=m_recordControl->Stop(0);
			if (!SUCCEEDED(hr))
			{
				Log("stop failed:%x", hr);
				return S_OK;
			}
			for (int x=0; x < 10;++x)
			{
				HRESULT hrOut;
				BOOL started,stopped;
				m_recordControl->GetRecordingStatus(&hrOut,&started,&stopped);
				Log("rec status:%d %x started:%d stopped:%d",x, hrOut,started,stopped);
				if (stopped!=0) break;
				Sleep(100);
			}
			m_recordControl.Release();
		}
		Log("CStreamBufferRecorder:: Stopped");
	}
	catch(...)
	{
		Log("CStreamBufferRecorder:: Stopped exception");
	}

	return S_OK;
}

STDMETHODIMP CStreamBufferRecorder::SetAttributeString(BSTR strName, BSTR strValue)
{
	if (m_recordControl==NULL) return S_OK;
	CComQIPtr<IStreamBufferRecordingAttribute> pAttrib = m_recordControl;
	if (pAttrib==NULL) return S_OK;

	_bstr_t bstrName(strName);
	_bstr_t bstrValue(strValue);

	WCHAR* wstr=(wchar_t *)bstrValue;
	WORD len=(WORD)wcslen(wstr);
	HRESULT hr=pAttrib->SetAttribute(0,(LPCWSTR)bstrName,STREAMBUFFER_TYPE_STRING ,(BYTE*)wstr,len*sizeof(WCHAR));
	if (!SUCCEEDED(hr))
	{
		Log("CStreamBufferRecorder::SetAttributeDWORD(%s %s) failed:%x",
					(char*)bstrName,(char*)bstrValue,hr);
	}
	return S_OK;
}

STDMETHODIMP CStreamBufferRecorder::SetAttributeDWORD(BSTR strName, ULONG dwValue)
{
	if (m_recordControl==NULL) return S_OK;
	CComQIPtr<IStreamBufferRecordingAttribute> pAttrib = m_recordControl;
	if (pAttrib==NULL) return S_OK;

	_bstr_t bstrName(strName);

	HRESULT hr=pAttrib->SetAttribute(0,(LPCWSTR)bstrName,STREAMBUFFER_TYPE_DWORD ,(BYTE*)&dwValue,sizeof(DWORD));
	if (!SUCCEEDED(hr))
	{
		Log("CStreamBufferRecorder::SetAttributeDWORD(%s %d) failed:%x",
					(char*)bstrName,dwValue,hr);
	}
	return S_OK;
}
