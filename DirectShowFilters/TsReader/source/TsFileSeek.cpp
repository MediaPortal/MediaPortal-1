/* 
 *  Copyright (C) 2005 Team MediaPortal
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
#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <math.h>
#include "TsFileSeek.h"
#include "..\..\shared\adaptionfield.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

const float SEEKING_ACCURACY = (float)0.08; // 1/25 *2 (2 frames in PAL)
const int MAX_SEEKING_ITERATIONS = 50;

extern void LogDebug(const char *fmt, ...) ;
CTsFileSeek::CTsFileSeek( CTsDuration& duration)
:m_duration(duration)
{
  m_useBinarySearch = true;
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
  double fileDuration=(double)m_duration.Duration().Millisecs();
  double seekTimeStamp=(double)refTime.Millisecs();
  
  //sanity checks...
  if (seekTimeStamp < 0) seekTimeStamp=0;
  //if (seekTimeStamp > fileDuration) seekTimeStamp=fileDuration;

  //make a guess where should start looking in the file
  double percent=seekTimeStamp/fileDuration;
  __int64 filePos=(__int64)(m_reader->GetFileSize()*percent);

  filePos/=188;
  filePos*=188;

  if( m_duration.FirstStartPcr() > m_duration.EndPcr() )
  {
    // no PCR rollover is allowed when using binary search with seeking
    m_useBinarySearch = false;
  }
  else
  {
    m_useBinarySearch = true;
  }

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
  __int64 prevfilePos=filePos;
  __int64 binaryMax=m_reader->GetFileSize();
  __int64 binaryMin=0;
  __int64 lastFilePos=0;
  int seekingIteration=0;

  SeekState state=FindPcr;
  Reset() ;   // Reset "PacketSync"
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
      double clockFound=m_pcrFound.ToClock();
      if( m_useBinarySearch )
      {
        double diff = fabs( seekTimeStamp - clockFound );
        //LogDebug(" got %f at filepos %x diff %f ( %I64x, %I64x )", clockFound, (DWORD)filePos, diff, binaryMin, binaryMax);
          
        if( diff < SEEKING_ACCURACY )
        {
          LogDebug(" stop seek: %f at %x - target: %f, diff: %f", 
            clockFound, (DWORD)filePos, seekTimeStamp, diff);
          m_reader->SetFilePointer(filePos,FILE_BEGIN);
          return;
        }

        seekingIteration++;
        if( seekingIteration > MAX_SEEKING_ITERATIONS )
        {
          LogDebug(" stop seek max iterations reached (%d): %f at %x - target: %f, diff: %f", 
            MAX_SEEKING_ITERATIONS, clockFound, (DWORD)filePos, seekTimeStamp, diff);
          m_reader->SetFilePointer(filePos,FILE_BEGIN);
          return;
        }

        // lower bound becomes valid
        if( clockFound > seekTimeStamp ) 
        {
          if (filePos < binaryMax) binaryMax = filePos-1;
        }
        else
        {
          if (filePos > binaryMin) binaryMin = filePos+1;
        }

        if (lastFilePos==filePos)
        {
          LogDebug(" stop seek closer target found : %f at %x - target: %f, diff: %f", 
            clockFound, (DWORD)filePos, seekTimeStamp, diff);
          m_reader->SetFilePointer(filePos,FILE_BEGIN);
          return;
        }

        lastFilePos=filePos;
        filePos = binaryMin + ( binaryMax - binaryMin ) / 2;
        Reset() ;   // Random jump, Reset "PacketSync"
      }
      else
      {
        if (state==FindPcr)
        {
          prevfilePos=filePos;
          //we found the pcr.
          //compare it with the timestamp we want to seek to
          if (clockFound < seekTimeStamp)
          {
            // pcr found is too low, move forward in file and seek next pcr
            state=FindNextPcr;
          
            //LogDebug(" got %f at filepos %x ->find next", clockFound, (DWORD)filePos);
            filePos += sizeof(buffer);
          }
          else if (clockFound > seekTimeStamp)
          {
            // pcr found is too high, move backward in file and seek previous pcr
            //LogDebug(" got %f at filepos %x ->find prev", clockFound, (DWORD)filePos);
            state=FindPreviousPcr;
            filePos -= sizeof(buffer);
            Reset() ;   // Backward jump, Reset "PacketSync"
          }
          else
          {
            //pcr is correct, just return
            //LogDebug(" got %f", clockFound);
            m_reader->SetFilePointer(filePos,FILE_BEGIN);
            return;
          }
        }
        else
        {
          //pcr found, check state
          if (state==FindNextPcr)
          {
            //LogDebug(" got %f at filepos %x", clockFound, (DWORD)filePos);
            //looking for a pcr > seektime
            if (clockFound > seekTimeStamp)
            {
              //found it..
              //LogDebug(" stop seek too big: %f at %x", clockFound, (DWORD)filePos);
              m_reader->SetFilePointer(prevfilePos,FILE_BEGIN);
              return;
            }
            prevfilePos=filePos;
            filePos+=sizeof(buffer);
          }
          else if (state==FindPreviousPcr)
          {
            //LogDebug(" got %f at filepos %x", clockFound, (DWORD)filePos);
            //looking for a pcr < seektime
            if (clockFound < seekTimeStamp)
            {
              //found it...
              //LogDebug(" stop seek too small: %f at %x", clockFound, (DWORD)filePos);
              m_reader->SetFilePointer(filePos,FILE_BEGIN);
              return;
            }
            prevfilePos=filePos;
            filePos-=sizeof(buffer);
            Reset() ;   // Backward jump, Reset "PacketSync"
          }
        }
      }
    }
    else // no first PCR
    {
      //no pcr found.
      if (state == FindPreviousPcr)
      {
        //move filepointer back and continue searching for a PCR
        filePos -=sizeof(buffer);
        Reset() ;   // Backward jump, Reset "PacketSync"
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
  if (m_pcrFound.IsValid) return ;

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