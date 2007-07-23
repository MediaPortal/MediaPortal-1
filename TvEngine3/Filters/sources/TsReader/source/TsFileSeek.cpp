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
#include "TsFileSeek.h"
#include "adaptionfield.h"

extern void LogDebug(const char *fmt, ...) ;
CTsFileSeek::CTsFileSeek( CTsDuration& duration)
:m_duration(duration)
{
}

CTsFileSeek::~CTsFileSeek(void)
{
}

void CTsFileSeek::SetFileReader(FileReader* reader)
{
  m_reader=reader;
}

//*********************************************************
// Seeks in the file to the specific timestamp
// refTime : timestamp. Should be 0 < timestamp < duration
//
// The method will make a guess where the timestamp is located in the file
// and do a PCR seek from there until it finds the correct timestamp
//
void CTsFileSeek::Seek(CRefTime refTime)
{
  float fileDuration=(float)m_duration.Duration().Millisecs();
  float seekTimeStamp=(float)refTime.Millisecs();
  
  //sanity checks...
  if (seekTimeStamp < 0) seekTimeStamp=0;
  if (seekTimeStamp > fileDuration) seekTimeStamp=fileDuration;

  //make a guess where should start looking in the file
  float percent=seekTimeStamp/fileDuration;
  __int64 filePos=m_reader->GetFileSize()*percent;

  seekTimeStamp /= 1000.0f; // convert to seconds.

  m_seekPid=m_duration.GetPid();
  LogDebug("seek to %f filepos:%x pid:%x", seekTimeStamp,(DWORD)filePos, m_seekPid);
  
  byte buffer[188*10];
  if (filePos<=0)
  {
    //no need to seek for timestamp 0, 
    //simply set the pointer at the beginning of the file
    m_reader->SetFilePointer(0,FILE_BEGIN);
    return;
  }
  if (filePos+sizeof(buffer) > m_reader->GetFileSize())
  {
    //no need to seek when we want to seek to end of file
    //simply set the pointer at the end of the file
    m_reader->SetFilePointer(0,FILE_END);
    return;
  }
  
  SeekState state=FindPcr;
  while (true)
  {
    //sanity checks
    if (filePos<0) return;
    if (filePos+sizeof(buffer) > m_reader->GetFileSize()) return;

    //set filepointer to filePos
    m_reader->SetFilePointer(filePos,FILE_BEGIN);

    //read buffer from file at position filePos
    DWORD dwBytesRead;
    if (!SUCCEEDED(m_reader->Read(buffer, sizeof(buffer),&dwBytesRead)))
    {
      return;
    }
    if (dwBytesRead==0) 
    {
      return;
    }
    //process data
    m_pcrFound.Reset();
    OnRawData(buffer,dwBytesRead);

    //did we find a pcr?
    if (m_pcrFound.IsValid)
    {
      //yes. pcr found
      //Check state
      double clockFound=m_pcrFound.ToClock();
      if (state==FindPcr)
      {
        //we found the pcr.
        //compare it with the timestamp we want to seek to
        if (clockFound < seekTimeStamp)
        {
          // pcr found is too low, move forward in file and seek next pcr
          state=FindNextPcr;
          filePos += sizeof(buffer);
        }
        else if (clockFound > seekTimeStamp)
        {
          // pcr found is too high, move backward in file and seek previous pcr
          state=FindPreviousPcr;
          filePos -= sizeof(buffer);
        }
        else
        {
          //pcr is correct, just return
          LogDebug(" got %f", clockFound);
          return;
        }
      }
      else
      {
        //pcr found, check state
        if (state==FindNextPcr)
        {
          //looking for a pcr > seektime
          if (clockFound >= seekTimeStamp)
          {
            //found it..
            LogDebug(" got %f", clockFound);
            return;
          }
          filePos+=sizeof(buffer);
        }
        else if (state==FindPreviousPcr)
        {
          //looking for a pcr < seektime
          if (clockFound < seekTimeStamp)
          {
            //found it...
            LogDebug(" got %f", clockFound);
            return;
          }
          filePos-=sizeof(buffer);
        }
      }
    }
    else
    {
      //no pcr found.
      if (state == FindPreviousPcr)
      {
        //move filepointer back and continue searching for a PCR
        filePos -=sizeof(buffer);
      }
      else
      {
        //move filepointer forward and continue searching for a PCR
        filePos += sizeof(buffer);
      }
    }
  }
}


//*********************************************************
// Callback method. This method gets called via
// CTsFileSeek::Seek()->OnRawData(buffer,dwBytesRead)
// tsPacket : pointer to 188 byte Transport Stream packet
//
// This method checks if the ts packet contains a PCR timestamp
// and ifso sets the PCR timestamp in m_pcrFound;
void CTsFileSeek::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);
  CAdaptionField field;
  field.Decode(header,tsPacket);
  if (field.Pcr.IsValid)
  {
    //got a pcr, is it the correct pid?
    if ( (header.Pid==m_seekPid) || (m_seekPid<0) )
    {
      // pid is valid
      // did we have a pcr rollover ??
      if (m_duration.MaxPcr().IsValid)
      {
        //pcr rollover occured.
        //next we need to convert the pcr into filestamp
        //since we are seeking from 0-duration
        //but the file can start with any pcr timestamp
        if (field.Pcr.ToClock() <=m_duration.EndPcr().ToClock())
        {
          // pcr < endpcr
          //   pcrFound= (pcr-startpcr)
          m_pcrFound=field.Pcr;
          double d1=m_pcrFound.ToClock();
          double start=m_duration.StartPcr().ToClock();//earliest pcr available in the file
          d1-=start;
          m_pcrFound.FromClock(d1);
        }
        else
        {
          //PCR > endpcr
          //   pcrFound- pcr+(maxPcr-startpcr)
          // StartPcr------>MaxPcr--->0--------->EndPcr
          m_pcrFound=field.Pcr;
          double d1=m_pcrFound.ToClock();
          double start=m_duration.MaxPcr().ToClock()- m_duration.StartPcr().ToClock();
          d1+=start;
          m_pcrFound.FromClock(d1);
        }
      }
      else
      {
        //no pcr rollover occured.
        //next we need to convert the pcr into filestamp
        //since we are seeking from 0-duration
        //but the file can start with any pcr timestamp
        // formula: pcrfound = pcr-startpcr;
        m_pcrFound=field.Pcr;
        double d1=m_pcrFound.ToClock();
        double start=m_duration.StartPcr().ToClock(); //earliest pcr available in the file
        d1-=start;
        m_pcrFound.FromClock(d1);
      }
    }
  }
}