#pragma once
#include <Windows.h>
#include "..\..\shared\packetSync.h"
#include "FileReader.h"
#include "TsDuration.h"
#include "..\..\shared\Pcr.h"
#include <streams.h>  // CRefTime

class CTsFileSeek: public CPacketSync
{
public:
  enum SeekState
  {
    FindPreviousPcr=-1,
    FindPcr=0,
    FindNextPcr=1
  };
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
  bool          m_useBinarySearch;
};
