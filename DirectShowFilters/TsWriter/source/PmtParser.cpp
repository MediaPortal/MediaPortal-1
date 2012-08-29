/* 
 *  Copyright (C) 2006-2010 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include <windows.h>
#include "PmtParser.h"

void LogDebug(const char *fmt, ...) ; 

CPmtParser::CPmtParser()
{
  CBasePmtParser::CBasePmtParser();
  m_pCallBack = NULL;
}

CPmtParser::~CPmtParser(void)
{
}

void CPmtParser::Reset()
{
  LogDebug("PmtParser: reset");
  CBasePmtParser::Reset();
  LogDebug("PmtParser: reset done");
}

void CPmtParser::SetCallBack(IPmtCallBack2* callBack)
{
  m_pCallBack = callBack;
}

void CPmtParser::OnNewSection(CSection& sections)
{ 
  if (m_bIsFound)
  {
    return;
  }

  if (!DecodePmtSection(sections))
  {
    return;
  }

  if (m_pCallBack != NULL)
  {
    m_pCallBack->OnPmtReceived(m_pidInfo);
    m_bIsFound = true;
    m_pCallBack = NULL;
  }
}
