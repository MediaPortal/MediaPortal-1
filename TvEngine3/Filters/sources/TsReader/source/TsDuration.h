#pragma once
#include "packetSync.h"
#include "MultiFileReader.h"
#include "tsheader.h"
#include "Pcr.h"

class CTsDuration: public CPacketSync
{
public:
  CTsDuration(MultiFileReader& reader);
  virtual ~CTsDuration(void);
	void OnTsPacket(byte* tsPacket);
  void UpdateDuration();
  CRefTime Duration();
  CPcr     StartPcr();
  CPcr     EndPcr();
private:
	MultiFileReader& m_reader;
  CPcr     m_startPcr;
  CPcr     m_endPcr;
  bool m_bSearchStart;
  bool m_bSearchEnd;
};
