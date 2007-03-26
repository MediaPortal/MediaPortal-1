#pragma once
#include "packetSync.h"
#include "FileReader.h"
#include "tsheader.h"
#include "TsDuration.h"
#include "Pcr.h"

class CTsFileSeek: public CPacketSync
{
public:
  CTsFileSeek( CTsDuration& duration );
public:
  virtual ~CTsFileSeek(void);
	void OnTsPacket(byte* tsPacket);
  void Seek(CRefTime refTime);
  void SetFileReader(FileReader* reader);
private:
  FileReader* m_reader;
  CTsDuration& m_duration;
  CPcr m_pcrFound;
};
