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

void CTsFileSeek::Seek(CRefTime refTime)
{
  float duration=(float)m_duration.Duration().Millisecs();
  float seekPos=(float)refTime.Millisecs();
  //if (seekPos>1000) seekPos-=1000;
  //else seekPos=1000;
  float percent=seekPos/duration;
  __int64 filePos=m_reader->GetFileSize()*percent;
  seekPos/=1000.0f;

  m_seekPid=m_duration.GetPid();
  LogDebug("seek to %f filepos:%x pid:%x", seekPos,(DWORD)filePos, m_seekPid);
  byte buffer[188*10];
  int state=0;
  while (true)
  {
    if (filePos<0) return;
    if (filePos+sizeof(buffer) > m_reader->GetFileSize()) return;
    m_reader->SetFilePointer(filePos,FILE_BEGIN);
    DWORD dwBytesRead;
    if (!SUCCEEDED(m_reader->Read(buffer,sizeof(buffer),&dwBytesRead)))
    {
      return;
    }
    if (dwBytesRead==0) 
    {
      return;
    }
    m_pcrFound.Reset();
    OnRawData(buffer,dwBytesRead);
    if (m_pcrFound.IsValid)
    {
      double clock=m_pcrFound.ToClock();
      if (state==0)
      {
        if (clock < seekPos)
        {
          state=1;
          filePos+=sizeof(buffer);
        }
        else if (clock > seekPos)
        {
          state=-1;
          filePos-=sizeof(buffer);
        }
        else
        {
          LogDebug(" got %f", clock);
          return;
        }
      }
      else
      {
        if (state==1)
        {
          if (clock >= seekPos)
          {
            LogDebug(" got %f", clock);
            return;
          }
          filePos+=sizeof(buffer);
        }
        else if (state==-1)
        {
          if (clock < seekPos)
          {
            LogDebug(" got %f", clock);
            return;
          }
          filePos-=sizeof(buffer);
        }
      }
    }
    else
    {
      if (state<0)
      {
        filePos-=sizeof(buffer);
      }
      else
      {
        filePos+=sizeof(buffer);
      }
    }
  }
}


void CTsFileSeek::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);
  CAdaptionField field;
  field.Decode(header,tsPacket);
  if (field.Pcr.IsValid)
  {
    if ( (header.Pid==m_seekPid) || (m_seekPid<0) )
    {
      if (m_duration.MaxPcr().IsValid)
      {
        if (field.Pcr.ToClock() <=m_duration.EndPcr().ToClock())
        {
          m_pcrFound=field.Pcr;
          double d1=m_pcrFound.ToClock();
          double start=m_duration.StartPcr().ToClock();
          d1-=start;
          m_pcrFound.FromClock(d1);
        }
        else
        {
          m_pcrFound=field.Pcr;
          double d1=m_pcrFound.ToClock();
          double start=m_duration.MaxPcr().ToClock()- m_duration.StartPcr().ToClock();
          d1+=start;
          m_pcrFound.FromClock(d1);
        }
      }
      else
      {
        m_pcrFound=field.Pcr;
        double d1=m_pcrFound.ToClock();
        double start=m_duration.StartPcr().ToClock();
        d1-=start;
        m_pcrFound.FromClock(d1);
      }
    }
  }
}