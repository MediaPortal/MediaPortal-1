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
#include <streams.h>
#include "TsDuration.h"
#include "AdaptionField.h"
extern void LogDebug(const char *fmt, ...) ;

CTsDuration::CTsDuration()
{
  m_videoPid=-1;
}

CTsDuration::~CTsDuration(void)
{
}


void CTsDuration::SetFileReader(FileReader* reader)
{
  m_reader=reader;
}

void CTsDuration::Set(CPcr& startPcr, CPcr& endPcr, CPcr& maxPcr)
{
  m_startPcr=startPcr;
  m_endPcr=endPcr;
  m_maxPcr=maxPcr;
}
  
void CTsDuration::SetVideoPid(int pid)
{
  m_videoPid=pid;
}

int CTsDuration::GetPid()
{
  if (m_videoPid>0) return m_videoPid;
  return m_pid;
}
void CTsDuration::UpdateDuration()
{
  m_bSearchStart=true;
  m_bSearchEnd=false;
  m_bSearchMax=false;
  m_startPcr.Reset();
  m_maxPcr.Reset();
  m_reader->SetFilePointer(0,FILE_BEGIN);
  byte buffer[32712];
  while (!m_startPcr.IsValid)
  {
    DWORD dwBytesRead;
    if (!SUCCEEDED(m_reader->Read(buffer,sizeof(buffer),&dwBytesRead)))
    {
      return;
    }
    if (dwBytesRead==0) 
    {
      return;
    }
    OnRawData(buffer,dwBytesRead);
  }
  m_bSearchEnd=true;
  m_bSearchStart=false;
  m_endPcr.Reset();
  __int64 offset=sizeof(buffer);
  __int64 fileSize=m_reader->GetFileSize();
  
  while (!m_endPcr.IsValid)
  {
   // LogDebug("fileSize:%x off:%x",(DWORD)fileSize, (DWORD)fileSize-offset);
    DWORD dwBytesRead;
    m_reader->SetFilePointer(fileSize-offset,FILE_BEGIN);
    if (!SUCCEEDED(m_reader->Read(buffer,sizeof(buffer),&dwBytesRead)))
    {
      break;
    }
    if (dwBytesRead==0) 
    {
      break;
    }
    OnRawData(buffer,dwBytesRead);
    offset+=sizeof(buffer);
  }
  if (m_endPcr.PcrReferenceBase < m_startPcr.PcrReferenceBase)
  {
    //PCR rollover
    m_bSearchMax=true;
    m_bSearchEnd=false;
    offset=sizeof(buffer);
    while (!m_maxPcr.IsValid)
    {
      DWORD dwBytesRead;
      m_reader->SetFilePointer(fileSize-offset,FILE_BEGIN);
      if (!SUCCEEDED(m_reader->Read(buffer,sizeof(buffer),&dwBytesRead)))
      {
        break;
      }
      if (dwBytesRead==0) 
      {
        break;
      }
      OnRawData(buffer,dwBytesRead);
      offset+=sizeof(buffer);
    }
  }
}

void CTsDuration::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);
  CAdaptionField field;
  field.Decode(header,tsPacket);
  if (field.Pcr.IsValid)
  {
    if (m_bSearchStart)
    {
      if ( (m_videoPid>0 && header.Pid==m_videoPid) || m_videoPid<0)
      {
        m_pid=header.Pid;
        m_startPcr=field.Pcr;
        m_bSearchStart=false;
      }
    }
    if (m_bSearchEnd && m_pid==header.Pid)
    {
      m_endPcr=field.Pcr;
    }
    if (m_bSearchMax && m_pid==header.Pid)
    {
      if (field.Pcr.ToClock() >  m_startPcr.ToClock())
      {
        m_maxPcr=field.Pcr;
      }
    }
  }
}

CRefTime CTsDuration::Duration()
{
  if (m_maxPcr.IsValid)
  {
    double duration= m_endPcr.ToClock() + (m_maxPcr.ToClock()- m_startPcr.ToClock());
    CPcr pcr;
    pcr.FromClock(duration);
    //LogDebug("Duration:%f %s", duration, pcr.ToString());
    CRefTime refTime((LONG)(duration*1000.0f));
    return refTime;
  }
  else
  {
    double duration= m_endPcr.ToClock() - m_startPcr.ToClock();
    CPcr pcr;
    pcr.FromClock(duration);
    //LogDebug("Duration:%f %s", duration, pcr.ToString());
    CRefTime refTime((LONG)(duration*1000.0f));
    return refTime;
  }
}

CPcr CTsDuration::StartPcr()
{
  return m_startPcr;
}

CPcr CTsDuration::EndPcr()
{
  return m_endPcr;
}
CPcr CTsDuration::MaxPcr()
{
  return m_maxPcr;
}