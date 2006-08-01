#pragma once

class CSection
{
public:
  CSection(void);
  virtual ~CSection(void);
  void   Reset();
  int    Version;
  int    SectionNumber;
  int    LastSectionNumber;
  unsigned int NetworkId;
  int    TransportId;
  int    SectionPos;
  int    BufferPos;
  int    SectionLength;
	int    Length;
  byte   Data[4096];
};
