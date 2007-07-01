#pragma once
#include "packetSync.h"
#include "FileReader.h"
#include "tsheader.h"
#include "Pcr.h"

class CTsDuration: public CPacketSync
{
public:
  CTsDuration();
  virtual ~CTsDuration(void);
  void SetFileReader(FileReader* reader);
	void OnTsPacket(byte* tsPacket);
  void UpdateDuration();
  CRefTime Duration();
  CPcr     StartPcr();
  CPcr     EndPcr();
  void     Set(CPcr& startPcr, CPcr& endPcr);
private:
  int m_pid;
	FileReader* m_reader;
  CPcr     m_startPcr;
  CPcr     m_endPcr;
  bool m_bSearchStart;
  bool m_bSearchEnd;
};
