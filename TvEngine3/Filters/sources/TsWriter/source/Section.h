#pragma once

#define MAX_SECTION_LENGTH 4300
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
  byte   Data[MAX_SECTION_LENGTH];
};
