/* 
*	Copyright (C) 2006-2008 Team MediaPortal
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

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "liveMedia.hh"
#include "SubChannel.h"

extern void LogDebug(const char *fmt, ...) ;
extern void LogDebug(const wchar_t *fmt, ...);

CSubChannel::CSubChannel(LPUNKNOWN pUnk, HRESULT *phr, int id)
:CUnknown( NAME ("MPFileWriterSubChannel"), pUnk)
{
	m_id = id;
	LogDebug("CSubChannel::ctor() - (%d)",m_id);

	m_pTsWriter = new CProgramToTransportStream();
	m_pTsRecorder = new CProgramToTransportStreamRecorder();

	m_pTeletextGrabber = new CTeletextGrabber();

	wcscpy(m_wstrRecordingFileName, L"");
	wcscpy(m_wstrTimeShiftFileName, L"");
	m_bIsTimeShifting=false;
	m_bIsRecording=false;
}




// Destructor

CSubChannel::~CSubChannel()
{
	LogDebug("CSubChannel::dtor() - (%d)",m_id);

	if(m_bIsRecording){
		StopRecord();
		LogDebug("%CSubChannel::  - (%d) - Stopping Recording",m_id);
	}


	if(m_bIsTimeShifting){
		StopTimeShifting();
		LogDebug("CSubChannel::  - (%d) - Stopping Timeshifting",m_id);
	}
	
	delete m_pTsRecorder;
	m_pTsRecorder = NULL;

	LogDebug("CProgramToTransportStreamRecorder::dtor() completed - (%d)",m_id);

	delete m_pTsWriter;
	m_pTsWriter = NULL;

	LogDebug("CProgramToTransportStream::dtor() completed - (%d)",m_id);

	delete m_pTeletextGrabber;
	m_pTeletextGrabber = NULL;

	LogDebug("CTeletextGrabber::dtor() completed - (%d)",m_id);

}

STDMETHODIMP CSubChannel::SetTimeShiftFileNameW(wchar_t* pwszFileName)
{
	wcscpy(m_wstrTimeShiftFileName, pwszFileName);
	wcscat(m_wstrTimeShiftFileName, L".tsbuffer");
	return S_OK;
}

STDMETHODIMP CSubChannel::SetChannelType(int channelType)
{
	m_pTsWriter->SetProgramType(channelType);
	m_pTsRecorder->SetProgramType(channelType);
	return S_OK;
}

STDMETHODIMP CSubChannel::SetTimeShiftParams(int minFiles, int maxFiles, ULONG maxFileSize)
{
	m_pTsWriter->SetTimeShiftParams(  minFiles,  maxFiles,  maxFileSize);
	return S_OK;
}
STDMETHODIMP CSubChannel::StartTimeShifting()
{
	CAutoLock lock(&m_Lock);

	if (m_bIsTimeShifting)
	{
		LogDebug(L"CSubChannel::StartTimeShifting - (%d) - Stopping first - Filename:'%s'", m_id, m_wstrTimeShiftFileName);
		m_pTsWriter->Close();
		m_bIsTimeShifting=false;
	}

	if (wcslen(m_wstrTimeShiftFileName)==0) return E_FAIL;

	::DeleteFileW((LPCWSTR) m_wstrTimeShiftFileName);
	LogDebug(L"CSubChannel::StartTimeShifting() - (%d) - Filename:'%s'",m_id,m_wstrTimeShiftFileName);

	m_pTsWriter->Initialize(m_wstrTimeShiftFileName);
	m_bIsTimeShifting=true;
	m_bPaused=false;
	m_pTeletextGrabber->Start();
	return S_OK;

}	
STDMETHODIMP CSubChannel::StopTimeShifting()
{
	CAutoLock lock(&m_Lock);

	if (m_bIsTimeShifting)
	{
		LogDebug(L"CSubChannel::StopTimeShifting() - (%d) - Filename:'%s'",m_id,m_wstrTimeShiftFileName);
		m_pTeletextGrabber->Stop();
		m_pTsWriter->Close();
		wcscpy(m_wstrTimeShiftFileName,L"");
		m_bIsTimeShifting=false;
		::DeleteFileW((LPCWSTR) m_wstrTimeShiftFileName);
	}
	return S_OK;
}

STDMETHODIMP CSubChannel::PauseTimeShifting(int onOff)
{
	CAutoLock lock(&m_Lock);

	LogDebug("CSubChannel::PauseTimeShifting() - (%d) - Status: %d",m_id,onOff);
	m_bPaused=(onOff!=0);
	if(m_bPaused){
		m_pTsWriter->Flush();
		if(m_pTeletextGrabber!=NULL){
			m_pTeletextGrabber->Stop();
		}
	}else{
		if(m_pTeletextGrabber!=NULL){
			m_pTeletextGrabber->Start();
		}
	}
	return S_OK;
}

STDMETHODIMP CSubChannel::SetRecordingFileNameW(wchar_t* pwszFileName)
{
	if(m_bIsRecording){
		return S_OK;
	}
	wcscpy(m_wstrRecordingFileName, pwszFileName);
	return S_OK;
}


STDMETHODIMP CSubChannel::StartRecord()
{
	CAutoLock lock(&m_Lock);
	StopRecord();
	if (wcslen(m_wstrRecordingFileName)==0) return E_FAIL;

	::DeleteFileW((LPCWSTR) m_wstrRecordingFileName);
	LogDebug(L"CSubChannel::StartRecord() - (%d) - Filename:'%s'",m_id,m_wstrRecordingFileName);
	m_pTsRecorder->Initialize(m_wstrRecordingFileName);
	m_bIsRecording = true;
	return S_OK;

}	
STDMETHODIMP CSubChannel::StopRecord()
{
	CAutoLock lock(&m_Lock);
	if(!m_bIsRecording){
		return S_OK;
	}

	LogDebug(L"CSubChannel::StopRecord() - (%d) - Filename:'%s'",m_id,m_wstrRecordingFileName);
	m_pTsRecorder->Close();
	wcscpy(m_wstrRecordingFileName, L"");
	m_bIsRecording = false;
	return S_OK;
}

HRESULT CSubChannel::Write(PBYTE pbData, LONG lDataLength)
{
	CAutoLock lock(&m_Lock);
	if(m_bIsRecording){
			m_pTsRecorder->Write(pbData,lDataLength);
	}
	if (m_bIsTimeShifting)
	{
		if (!m_bPaused)
		{
			m_pTsWriter->Write(pbData,lDataLength);
		}
	}
	return S_OK;
}

HRESULT CSubChannel::WriteTeletext(PBYTE pbData, LONG lDataLength){
	if(m_bIsTimeShifting){
		if(m_pTeletextGrabber!=NULL){
			m_pTeletextGrabber->OnSampleReceived(pbData,lDataLength);
		}
	}
	return S_OK;
}


STDMETHODIMP CSubChannel::TTxSetCallBack(IAnalogTeletextCallBack* callback){
	LogDebug("CSubChannel::TTxSetCallBack() - (%d)",m_id);
	m_pTeletextGrabber->SetCallBack(callback);
	return S_OK;
}

STDMETHODIMP CSubChannel::SetVideoAudioObserver(IAnalogVideoAudioObserver* callback){
	LogDebug("CSubChannel::SetVideoAudioObserver() - (%d)",m_id);
	m_pTsWriter->SetVideoAudioObserver(callback);
	return S_OK;
}


STDMETHODIMP CSubChannel::SetRecorderVideoAudioObserver(IAnalogVideoAudioObserver* callback){
	LogDebug("CSubChannel::SetRecorderVideoAudioObserver() - (%d)",m_id);
	m_pTsRecorder->SetVideoAudioObserver(callback);
	return S_OK;
}
