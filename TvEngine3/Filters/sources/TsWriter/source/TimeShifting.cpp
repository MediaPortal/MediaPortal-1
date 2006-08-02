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

#include "timeshifting.h"


extern void LogDebug(const char *fmt, ...) ;

CTimeShifting::CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsTimeshifting"), pUnk)
{
	m_bTimeShifting=false;
	m_pTimeShiftFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);
}
CTimeShifting::~CTimeShifting(void)
{
}

void CTimeShifting::OnTsPacket(byte* tsPacket)
{
	if (m_bTimeShifting)
	{
		m_multiPlexer.OnTsPacket(tsPacket);
	}
}


STDMETHODIMP CTimeShifting::SetPcrPid(int pcrPid)
{
	LogDebug("Timeshifter:pcr pid:%x",pcrPid);
	m_multiPlexer.SetPcrPid(pcrPid);
	return S_OK;
}
STDMETHODIMP CTimeShifting::AddPesStream(int pid)
{
	LogDebug("Timeshifter:add pes stream pid:%x",pid);
	m_multiPlexer.AddPesStream(pid);
	return S_OK;
}
STDMETHODIMP CTimeShifting::RemovePesStream(int pid)
{
	LogDebug("Recorder:remove pes stream pid:%x",pid);
	m_multiPlexer.RemovePesStream(pid);
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetTimeShiftingFileName(char* pszFileName)
{
	m_multiPlexer.Reset();
	strcpy(m_szFileName,pszFileName);
	strcat(m_szFileName,".tsbuffer");
	LogDebug("Timeshifter:set filename:%s",m_szFileName);
	return S_OK;
}
STDMETHODIMP CTimeShifting::Start()
{
	if (strlen(m_szFileName)==0) return E_FAIL;
	::DeleteFile((LPCTSTR) m_szFileName);
	WCHAR wstrFileName[2048];
	MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

	m_pTimeShiftFile = new MultiFileWriter();
	if (FAILED(m_pTimeShiftFile->OpenFile(wstrFileName))) 
	{
		LogDebug("Timeshifter:failed to open filename:%s",m_szFileName);
		m_pTimeShiftFile->CloseFile();
		delete m_pTimeShiftFile;
		m_pTimeShiftFile=NULL;
		return E_FAIL;
	}

	LogDebug("Timeshifter:Start timeshifting:'%s'",m_szFileName);
	m_bTimeShifting=true;
	return S_OK;
}
STDMETHODIMP CTimeShifting::Stop()
{
	LogDebug("Timeshifter:Stop timeshifting:'%s'",m_szFileName);
	m_bTimeShifting=false;
	m_multiPlexer.Reset();
	if (m_pTimeShiftFile!=NULL)
	{
		m_pTimeShiftFile->CloseFile();
		delete m_pTimeShiftFile;
		m_pTimeShiftFile=NULL;
	}
	return S_OK;
}


void CTimeShifting::Write(byte* buffer, int len)
{
	if (!m_bTimeShifting) return;
	if (m_pTimeShiftFile!=NULL)
	{
		m_pTimeShiftFile->Write(buffer,len);
	}
}