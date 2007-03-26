#pragma once
#include "packetSync.h"
#include "MultiFileReader.h"
#include "tsheader.h"
#include "TsDuration.h"
#include "Pcr.h"

class CTsFileSeek: public CPacketSync
{
public:
  CTsFileSeek(MultiFileReader& reader, CTsDuration& duration );
public:
  virtual ~CTsFileSeek(void);
	void OnTsPacket(byte* tsPacket);
  void Seek(CRefTime refTime);
private:
  MultiFileReader& m_reader;
  CTsDuration& m_duration;
  CPcr m_pcrFound;
};
