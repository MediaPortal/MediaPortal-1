#include <windows.h>
#include "Section.h"
#pragma warning(disable : 4995)

CSection::CSection(void)
{
	Data=new byte[MAX_SECTION_LENGTH*5];
  Reset();
}

CSection::~CSection(void)
{
	delete[] Data;
}

void CSection::Reset()
{
  Version=-1;
  SectionNumber=-1;
  SectionLength=-1;
  SectionPos=0;
  BufferPos=0;
  SectionLength=0;
  NetworkId=-1;
  TransportId=0;
  LastSectionNumber=0;
	Length=0;
}