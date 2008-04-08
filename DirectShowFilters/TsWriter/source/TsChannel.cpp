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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "TsChannel.h"

extern void LogDebug(const char *fmt, ...) ;

CTsChannel::CTsChannel(LPUNKNOWN pUnk, HRESULT *phr,int id) 
{
	m_id=id;
	m_pVideoAnalyzer = new CVideoAnalyzer(pUnk,phr);
	m_pPmtGrabber = new CPmtGrabber(pUnk,phr);
	m_pRecorder = new CDiskRecorder(RecordingMode::Recording);
	m_pTimeShifting= new CDiskRecorder(RecordingMode::TimeShift);
	m_pTeletextGrabber= new CTeletextGrabber(pUnk,phr);
  m_pCaGrabber= new CCaGrabber(pUnk,phr);
}

CTsChannel::~CTsChannel(void)
{
	if (m_pVideoAnalyzer!=NULL)
	{
		LogDebug("del m_pVideoAnalyzer");
		delete m_pVideoAnalyzer;
		m_pVideoAnalyzer=NULL;
	}
	if (m_pPmtGrabber!=NULL)
	{
		LogDebug("del m_pPmtGrabber");
		delete m_pPmtGrabber;
		m_pPmtGrabber=NULL;
	}
	if (m_pRecorder!=NULL)
	{
		LogDebug("del m_pRecorder");
		delete m_pRecorder;
		m_pRecorder=NULL;
	}
	if (m_pTimeShifting!=NULL)
	{
		LogDebug("del m_pTimeShifting");
		delete m_pTimeShifting;
		m_pTimeShifting=NULL;
	}
	if (m_pTeletextGrabber!=NULL)
	{
		LogDebug("del m_pTeletextGrabber");
		delete m_pTeletextGrabber;
		m_pTeletextGrabber=NULL;
	}
	if (m_pCaGrabber!=NULL)
	{
		LogDebug("del m_pCaGrabber");
		delete m_pCaGrabber;
		m_pCaGrabber=NULL;
	}
	LogDebug("del done...");
}

void CTsChannel::OnTsPacket(byte* tsPacket)
{
	try
	{
		m_pVideoAnalyzer->OnTsPacket(tsPacket);
		m_pPmtGrabber->OnTsPacket(tsPacket);
		m_pRecorder->OnTsPacket(tsPacket);
		m_pTimeShifting->OnTsPacket(tsPacket);
		m_pTeletextGrabber->OnTsPacket(tsPacket);
    m_pCaGrabber->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in AnalyzeTsPacket");
	}
}