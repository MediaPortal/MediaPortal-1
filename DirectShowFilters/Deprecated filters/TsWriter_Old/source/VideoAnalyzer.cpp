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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "videoanalyzer.h"


extern void LogDebug(const char *fmt, ...) ;

CVideoAnalyzer::CVideoAnalyzer(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsVideoAnalyzer"), pUnk)
{
}
CVideoAnalyzer::~CVideoAnalyzer(void)
{
		m_videoAudioAnalyzer.Reset();
}
  
STDMETHODIMP CVideoAnalyzer::SetVideoPid( int videoPid)
{
	try
	{
		m_videoAudioAnalyzer.SetVideoPid(videoPid);
		m_videoAudioAnalyzer.Reset();
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::SetVideoPid exception");
	}
	return S_OK;
}
STDMETHODIMP CVideoAnalyzer::GetVideoPid( int* videoPid)
{
	try
	{
		*videoPid=m_videoAudioAnalyzer.GetVideoPid();
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::GetVideoPid exception");
	}
	return S_OK;
}

STDMETHODIMP CVideoAnalyzer::SetAudioPid( int audioPid)
{
	try
	{
		m_videoAudioAnalyzer.SetAudioPid(audioPid);
		m_videoAudioAnalyzer.Reset();
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::SetAudioPid exception");
	}
	return S_OK;
}
STDMETHODIMP CVideoAnalyzer::GetAudioPid( int* audioPid)
{
	try
	{
		*audioPid=m_videoAudioAnalyzer.GetAudioPid();
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::GetAudioPid exception");
	}
	return S_OK;
}

STDMETHODIMP CVideoAnalyzer::IsVideoEncrypted( int* yesNo)
{
	try
	{
		*yesNo = (m_videoAudioAnalyzer.IsVideoScrambled()?1:0);
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::IsVideoEncrypted exception");
	}
	return S_OK;
}
STDMETHODIMP CVideoAnalyzer::IsAudioEncrypted( int* yesNo)
{
	try
	{
		*yesNo = (m_videoAudioAnalyzer.IsAudioScrambled()?1:0);
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::IsAudioEncrypted exception");
	}
	return S_OK;
}

STDMETHODIMP CVideoAnalyzer::Reset()
{
	try
	{
		m_videoAudioAnalyzer.Reset();
	}
	catch(...)
	{
		LogDebug("analyzer CVideoAnalyzer::Reset exception");
	}
	return S_OK;
}

void CVideoAnalyzer::OnTsPacket(byte* tsPacket)
{
	m_videoAudioAnalyzer.OnTsPacket(tsPacket);
}
