/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "recorder.h"


extern void LogDebug(const char *fmt, ...) ;

CRecorder::CRecorder(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsRecorder"), pUnk)
{
	m_bRecording=false;
	m_pRecordFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);
}
CRecorder::~CRecorder(void)
{
}

void CRecorder::OnTsPacket(byte* tsPacket)
{
	if (m_bRecording)
	{
		m_multiPlexer.OnTsPacket(tsPacket);
	}
}


STDMETHODIMP CRecorder::SetPcrPid(int pcrPid)
{
	LogDebug("Recorder:pcr pid:%x",pcrPid);
	m_multiPlexer.SetPcrPid(pcrPid);
	return S_OK;
}
STDMETHODIMP CRecorder::AddPesStream(int pid)
{
	LogDebug("Recorder:add pes stream pid:%x",pid);
	m_multiPlexer.AddPesStream(pid);
	return S_OK;
}
STDMETHODIMP CRecorder::SetRecordingFileName(char* pszFileName)
{
	m_multiPlexer.Reset();
	strcpy(m_szFileName,pszFileName);
	return S_OK;
}
STDMETHODIMP CRecorder::StartRecord()
{
	if (strlen(m_szFileName)==0) return E_FAIL;
	::DeleteFile((LPCTSTR) m_szFileName);
	WCHAR wstrFileName[2048];
	MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

	m_pRecordFile = new FileWriter();
	m_pRecordFile->SetFileName( wstrFileName);

	LogDebug("Recorder:Start Recording:'%s'",m_szFileName);
	m_bRecording=true;
	return S_OK;
}
STDMETHODIMP CRecorder::StopRecord()
{
	LogDebug("Recorder:Stop Recording:'%s'",m_szFileName);
	m_bRecording=false;
	m_multiPlexer.Reset();
	if (m_pRecordFile!=NULL)
	{
		m_pRecordFile->CloseFile();
		delete m_pRecordFile;
		m_pRecordFile=NULL;
	}
	return S_OK;
}


void CRecorder::Write(byte* buffer, int len)
{
	if (!m_bRecording) return;
	if (m_pRecordFile!=NULL)
	{
		m_pRecordFile->Write(buffer,len);
	}
}