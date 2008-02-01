#pragma once
#include "..\..\shared\packetSync.h"
#include "FileReader.h"
#include "..\..\shared\tsheader.h"
#include "..\..\shared\Pcr.h"

class CTsDuration: public CPacketSync
{
public:
  CTsDuration();
  virtual ~CTsDuration(void);
  void SetFileReader(FileReader* reader);
	void OnTsPacket(byte* tsPacket);
  void UpdateDuration();
  void SetVideoPid(int pid);
  int  GetPid();
  CRefTime Duration();      
  CPcr     StartPcr();
  CPcr     EndPcr();
  CPcr     MaxPcr();
  CPcr     FirstStartPcr();
  CRefTime TotalDuration();
  void     Set(CPcr& startPcr, CPcr& endPcr, CPcr& maxPcr);
private:
  int          m_pid;
  int          m_videoPid;
	FileReader*  m_reader;
  
  //earliest pcr currently available in the file/timeshifting files
  CPcr         m_startPcr;

  //contains the latest pcr 
  CPcr         m_endPcr;

  // in case of a PCR rollover, maxPCR contains the last pcr timestamp before the
  // rollover occured
  CPcr         m_maxPcr;

  //earliest pcr ever seen. Needed for timeshifting files since when
  //timeshifting files are wrapped and being re-used, we 'loose' the first pcr
  CPcr         m_firstStartPcr;
  bool         m_bSearchStart;
  bool         m_bSearchEnd;
  bool         m_bSearchMax;
};
