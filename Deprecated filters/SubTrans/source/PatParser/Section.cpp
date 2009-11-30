#include <windows.h>
#include "Section.h"

CSection::CSection(void)
{
  Reset();
}

CSection::~CSection(void)
{
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