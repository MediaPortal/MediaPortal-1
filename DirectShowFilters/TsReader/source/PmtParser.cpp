/* 
*	Copyright (C) 2006-2010 Team MediaPortal
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
#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include "StdAfx.h"

#include "PmtParser.h"
#include "channelinfo.h"
#include <cassert>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

void LogDebug(const char *fmt, ...); 

//TsReader specific PMT parser class.
CPmtParser::CPmtParser()
{
  CBasePmtParser::CBasePmtParser();
  m_pmtCallback=NULL;
}

CPmtParser::~CPmtParser(void)
{
}

void CPmtParser::SetPmtCallBack(IPmtCallBack* callback)
{
  m_pmtCallback=callback;
}

void CPmtParser::PmtFoundCallback()
{
  if (m_pmtCallback!=NULL)
  {
    m_pmtCallback->OnPmtReceived(GetPid());
  }
}

void CPmtParser::PidsFoundCallback()
{
  if (m_pmtCallback!=NULL)
  {
    //LogDebug("DecodePMT pid:0x%x pcrpid:0x%x videopid:0x%x audiopid:0x%x ac3pid:0x%x sid:%x",
    //  m_pidInfo.PmtPid, m_pidInfo.PcrPid,m_pidInfo.VideoPid,m_pidInfo.AudioPid1,m_pidInfo.AC3Pid,m_pidInfo.ServiceId);
    m_pmtCallback->OnPidsReceived(m_pidInfo);
  }
}

void CPmtParser::OnNewSection(CSection& section)
{   
  DecodePmtPidTable(section);
}
