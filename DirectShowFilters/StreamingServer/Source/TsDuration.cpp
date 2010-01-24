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
//#include <afx.h>
//#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "TsDuration.h"
#include "..\..\shared\AdaptionField.h"
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
  if (!m_firstStartPcr.IsValid)
  {
    m_firstStartPcr=m_startPcr;
  }
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

//*********************************************************
// Determines the total duration of the file (or timeshifting files)
// 
// 
void CTsDuration::UpdateDuration()
{

  byte buffer[32712];
  DWORD dwBytesRead;
  int Loop=5 ;

  do
  {
    m_bSearchStart=true;
    m_bSearchEnd=false;
    m_bSearchMax=false;
    m_startPcr.Reset();
    m_maxPcr.Reset();
    m_reader->SetFilePointer(0,FILE_BEGIN);

    //find the first pcr in the file
    while (!m_startPcr.IsValid)
    {
      if (!SUCCEEDED(m_reader->Read(buffer,sizeof(buffer),&dwBytesRead)))
      {
        //park filepointer at end of file
        m_reader->SetFilePointer(-1,FILE_END);
        m_reader->Read(buffer,1,&dwBytesRead);
        return;
      }
      if (dwBytesRead==0) 
      {
        //park filepointer at end of file
        m_reader->SetFilePointer(-1,FILE_END);
        m_reader->Read(buffer,1,&dwBytesRead);
        return;
      }
    OnRawData(buffer,dwBytesRead);
    }

    //find the last pcr in the file
    m_bSearchEnd=true;
    m_bSearchStart=false;
    m_endPcr.Reset();
    __int64 offset=sizeof(buffer);
    __int64 fileSize=m_reader->GetFileSize();
  
    while (!m_endPcr.IsValid)
    {
      DWORD dwBytesRead;
      m_reader->SetFilePointer(-offset,FILE_END);
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

    Loop-- ;
    if(m_endPcr.PcrReferenceBase < m_startPcr.PcrReferenceBase)
		{
			if (Loop < 4)		// Show log on 2nd wrong detection.
				LogDebug("Abnormal start PCR, endPcr %I64d, startPcr %I64d",m_endPcr.PcrReferenceBase, m_startPcr.PcrReferenceBase);
			Sleep(20) ;
		}
  }
  while ((m_endPcr.PcrReferenceBase < m_startPcr.PcrReferenceBase) && Loop) ;
    // When startPcr > endPcr, it could be a result of wrong file used to find "startPcr".
    // If this file is just reused by TsWriter, and list buffer not updated yet, the search will operate
    // in the latest ts packets received causing the start higher the end ( readed in the previous buffer )
    // The only thing to do to discriminate an error and a real rollover is to re-search startPcr. 
    // Entering the following code with erroneous startPcr > endPcr makes an endless loop !!
    // The startPcr read can also failed when it occurs between deleting and reusing the ts buffer.
    // This abort the method. Duration will be updated on next call.
  if (Loop==0)
    LogDebug("PCR rollover normally found ! endPcr %I64d, startPcr %I64d",m_endPcr.PcrReferenceBase, m_startPcr.PcrReferenceBase);
  else
  {
    if(Loop<3)  // 1 failed + 1 succeded is quasi-normal, more is a bit suspicious ( disk drive too slow or problem ? )
      LogDebug("Recovered wrong start PCR, seek to 'begin' on reused file ! ( Retried %d times )",4-Loop) ;
  }

  //When the last pcr < first pcr then a pcr roll over occured
  //find where in the file this rollover happened
  //and fill maxPcr
  if (m_endPcr.PcrReferenceBase < m_startPcr.PcrReferenceBase)
  {
    //PCR rollover
    m_bSearchMax=true;
    m_bSearchEnd=false;
    __int64 offset=sizeof(buffer);
    while (!m_maxPcr.IsValid)
    {
      DWORD dwBytesRead;
      m_reader->SetFilePointer(-offset,FILE_END);
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

  //park filepointer at end of file
  m_reader->SetFilePointer(-1,FILE_END);
  m_reader->Read(buffer,1,&dwBytesRead);
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
        if (!m_firstStartPcr.IsValid)
        {
          m_firstStartPcr=m_startPcr;
        }
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


//*********************************************************
// returns the duration in REFERENCE_TIME
// of the file (or timeshifting files)
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

//*********************************************************
// returns the total duration in REFERENCE_TIME
// of the file (or timeshifting files) since start of playback
// The TotalDuration() >= Duration() since the timeshifting files may have been
// wrapped and reused.
CRefTime CTsDuration::TotalDuration()
{
  if (m_maxPcr.IsValid)
  {
    double duration= m_endPcr.ToClock() + (m_maxPcr.ToClock()- m_firstStartPcr.ToClock());
    CPcr pcr;
    pcr.FromClock(duration);
    //LogDebug("Duration:%f %s", duration, pcr.ToString());
    CRefTime refTime((LONG)(duration*1000.0f));
    return refTime;
  }
  else
  {
    double duration= m_endPcr.ToClock() - m_firstStartPcr.ToClock();
    CPcr pcr;
    pcr.FromClock(duration);
    //LogDebug("Duration:%f %s", duration, pcr.ToString());
    CRefTime refTime((LONG)(duration*1000.0f));
    return refTime;
  }
}

///*********************************************************
///returns the earliest pcr we've encountered since we started playback
///Needed for timeshifting files since when
///timeshifting files are wrapped and being re-used, we 'loose' the first pcr
CPcr CTsDuration::FirstStartPcr()
{
  return m_firstStartPcr;
}

///*********************************************************
///returns the earliest pcr currently available in the file/timeshifting files
CPcr CTsDuration::StartPcr()
{
  return m_startPcr;
}

///*********************************************************
///returns the latest pcr 
CPcr CTsDuration::EndPcr()
{
  return m_endPcr;
}
///*********************************************************
///returns the pcr after which the pcr roll over happened
CPcr CTsDuration::MaxPcr()
{
  return m_maxPcr;
}
