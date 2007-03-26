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
  float percent=seekPos/duration;
  __int64 filePos=m_reader->GetFileSize()*percent;
  seekPos/=1000.0f;
  LogDebug("seek to %f", seekPos);

  byte buffer[188*10];
  int state=0;
  while (true)
  {
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
    OnRawData(buffer,dwBytesRead);
    if (m_pcrFound.IsValid)
    {
      if (state==0)
      {
        if (m_pcrFound.ToClock() < seekPos)
        {
          state=1;
          filePos+=sizeof(buffer);
        }
        else if (m_pcrFound.ToClock() > seekPos)
        {
          state=-1;
          filePos-=sizeof(buffer);
        }
        else
        {
            LogDebug(" got %f", m_pcrFound.ToClock());
          return;
        }
      }
      else
      {
        if (state==1)
        {
          if (m_pcrFound.ToClock() >= seekPos)
          {
            LogDebug(" got %f", m_pcrFound.ToClock());
            return;
          }
          filePos+=sizeof(buffer);
        }
        else if (state==-1)
        {
          if (m_pcrFound.ToClock() < seekPos)
          {
            LogDebug(" got %f", m_pcrFound.ToClock());
            return;
          }
          filePos-=sizeof(buffer);
        }
      }
    }
    else
    {
      filePos+=sizeof(buffer);
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
    m_pcrFound=field.Pcr;
    double d1=m_pcrFound.ToClock();
    double start=m_duration.StartPcr().ToClock();
    d1-=start;
    m_pcrFound.FromClock(d1);
  }
}