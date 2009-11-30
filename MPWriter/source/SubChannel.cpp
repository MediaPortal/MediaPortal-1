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


CSubChannel::CSubChannel(LPUNKNOWN pUnk, HRESULT *phr, int id)
:CUnknown( NAME ("MPFileWriterSubChannel"), pUnk)
{
	m_id = id;
	LogDebug("CSubChannel::ctor() - (%d)",m_id);

	m_pTsWriter = new CProgramToTransportStream();
	m_pTsRecorder = new CProgramToTransportStreamRecorder();

	m_pTeletextGrabber = new CTeletextGrabber();

	strcpy(m_strRecordingFileName,"");
	strcpy(m_strTimeShiftFileName,"");
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

STDMETHODIMP CSubChannel::SetTimeShiftFileName(char* pszFileName)
{
	strcpy(m_strTimeShiftFileName,pszFileName);
	strcat(m_strTimeShiftFileName,".tsbuffer");
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
		LogDebug("CSubChannel::StartTimeShifting - (%d) - Stopping first - Filename:'%s'",m_id,m_strTimeShiftFileName);
		m_pTsWriter->Close();
		m_bIsTimeShifting=false;
	}

	if (strlen(m_strTimeShiftFileName)==0) return E_FAIL;

	::DeleteFile((LPCTSTR) m_strTimeShiftFileName);
	LogDebug("CSubChannel::StartTimeShifting() - (%d) - Filename:'%s'",m_id,m_strTimeShiftFileName);

	m_pTsWriter->Initialize(m_strTimeShiftFileName);
	m_bIsTimeShifting=true;
	m_bPaused=false;
	WCHAR wstrFileName[2048];
	MultiByteToWideChar(CP_ACP,0,m_strTimeShiftFileName,-1,wstrFileName,1+strlen(m_strTimeShiftFileName));
	m_pTeletextGrabber->Start();
	return S_OK;

}	
STDMETHODIMP CSubChannel::StopTimeShifting()
{
	CAutoLock lock(&m_Lock);

	if (m_bIsTimeShifting)
	{
		LogDebug("CSubChannel::StopTimeShifting() - (%d) - Filename:'%s'",m_id,m_strTimeShiftFileName);
		m_pTeletextGrabber->Stop();
		m_pTsWriter->Close();
		strcpy(m_strTimeShiftFileName,"");
		m_bIsTimeShifting=false;
		::DeleteFile((LPCTSTR) m_strTimeShiftFileName);
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

STDMETHODIMP CSubChannel::SetRecordingFileName(char* pszFileName)
{
	if(m_bIsRecording){
		return S_OK;
	}
	strcpy(m_strRecordingFileName,pszFileName);
	return S_OK;
}


STDMETHODIMP CSubChannel::StartRecord()
{
	CAutoLock lock(&m_Lock);
	StopRecord();
	if (strlen(m_strRecordingFileName)==0) return E_FAIL;

	::DeleteFile((LPCTSTR) m_strRecordingFileName);
	LogDebug("CSubChannel::StartRecord() - (%d) - Filename:'%s'",m_id,m_strRecordingFileName);
	m_pTsRecorder->Initialize(m_strRecordingFileName);
	m_bIsRecording = true;
	return S_OK;

}	
STDMETHODIMP CSubChannel::StopRecord()
{
	CAutoLock lock(&m_Lock);
	if(!m_bIsRecording){
		return S_OK;
	}

	LogDebug("CSubChannel::StopRecord() - (%d) - Filename:'%s'",m_id,m_strRecordingFileName);
	m_pTsRecorder->Close();
	strcpy(m_strRecordingFileName,"");
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
