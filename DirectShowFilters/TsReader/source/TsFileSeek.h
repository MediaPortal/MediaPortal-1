#pragma once
#include "..\..\shared\packetSync.h"
#include "FileReader.h"
#include "..\..\shared\tsheader.h"
#include "TsDuration.h"
#include "..\..\shared\Pcr.h"

class CTsFileSeek: public CPacketSync
{
public:
  CTsFileSeek( CTsDuration& duration );
  virtual ~CTsFileSeek(void);
	void OnTsPacket(byte* tsPacket);
  void Seek(CRefTime refTime);
  void SetFileReader(FileReader* reader);

private:
  FileReader*   m_reader;
  CTsDuration&  m_duration;
  CPcr          m_pcrFound;
  int           m_seekPid;
};
