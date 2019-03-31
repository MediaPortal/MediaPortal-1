/*
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#include "ParserBat.h"
#include <cstddef>    // NULL
#include <cwchar>     // wcsncpy()
#include <sstream>
#include <string>
#include "..\..\shared\EnterCriticalSection.h"

using namespace MediaPortal;
using namespace std;


CParserBat::CParserBat(ISectionDispatcher* sectionDispatcher)
  : CParserNitDvb(sectionDispatcher)
{
  CParserBat::SetPid(PID_BAT);
  m_tableIds.clear();
  m_tableIds.push_back(TABLE_ID_BAT);
}

CParserBat::~CParserBat()
{
  CEnterCriticalSection lock(m_section);
  CleanUp();
}

void CParserBat::SetPid(unsigned short pid)
{
  CEnterCriticalSection lock(m_section);
  wstringstream ss;
  ss << L"BAT " << pid;
  wstring s = ss.str();
  size_t characterCount = sizeof(m_name) / sizeof(m_name[0]);
  wcsncpy(m_name, s.c_str(), characterCount);
  m_name[characterCount - 1] = NULL;
  CSectionDecoder::SetPid(pid);
  CSectionDecoder::Reset();
}

bool CParserBat::IsSeen() const
{
  return IsSeenOther();
}

bool CParserBat::IsReady() const
{
  return IsReadyOther();
}

unsigned char CParserBat::GetBouquetNameCount(unsigned short bouquetId) const
{
  return GetNameCount(Bouquet, TABLE_ID_BAT, bouquetId, bouquetId);
}

bool CParserBat::GetBouquetNameByIndex(unsigned short bouquetId,
                                        unsigned char index,
                                        unsigned long& language,
                                        char* name,
                                        unsigned short& nameBufferSize) const
{
  return GetNameByIndex(Bouquet,
                        TABLE_ID_BAT,
                        bouquetId,
                        bouquetId,
                        index,
                        language,
                        name,
                        nameBufferSize);
}

bool CParserBat::GetBouquetNameByLanguage(unsigned short bouquetId,
                                          unsigned long language,
                                          char* name,
                                          unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(Bouquet,
                            TABLE_ID_BAT,
                            bouquetId,
                            bouquetId,
                            language,
                            name,
                            nameBufferSize);
}

unsigned char CParserBat::GetFreesatRegionNameCount(unsigned long regionId) const
{
  return GetNameCount(FreesatRegion, TABLE_ID_BAT, regionId >> 16, regionId & 0xffff);
}

bool CParserBat::GetFreesatRegionNameByIndex(unsigned long regionId,
                                                unsigned char index,
                                                unsigned long& language,
                                                char* name,
                                                unsigned short& nameBufferSize) const
{
  return GetNameByIndex(FreesatRegion,
                        TABLE_ID_BAT,
                        regionId >> 16,
                        regionId & 0xffff,
                        index,
                        language,
                        name,
                        nameBufferSize);
}

bool CParserBat::GetFreesatRegionNameByLanguage(unsigned long regionId,
                                                    unsigned long language,
                                                    char* name,
                                                    unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(FreesatRegion,
                            TABLE_ID_BAT,
                            regionId >> 16,
                            regionId & 0xffff,
                            language,
                            name,
                            nameBufferSize);
}

unsigned char CParserBat::GetFreesatChannelCategoryNameCount(unsigned long categoryId) const
{
  return GetNameCount(FreesatChannelCategory, TABLE_ID_BAT, categoryId >> 16, categoryId & 0xffff);
}

bool CParserBat::GetFreesatChannelCategoryNameByIndex(unsigned long categoryId,
                                                          unsigned char index,
                                                          unsigned long& language,
                                                          char* name,
                                                          unsigned short& nameBufferSize) const
{
  return GetNameByIndex(FreesatChannelCategory,
                        TABLE_ID_BAT,
                        categoryId >> 16,
                        categoryId & 0xffff,
                        index,
                        language,
                        name,
                        nameBufferSize);
}

bool CParserBat::GetFreesatChannelCategoryNameByLanguage(unsigned long categoryId,
                                                            unsigned long language,
                                                            char* name,
                                                            unsigned short& nameBufferSize) const
{
  return GetNameByLanguage(FreesatChannelCategory,
                            TABLE_ID_BAT,
                            categoryId >> 16,
                            categoryId & 0xffff,
                            language,
                            name,
                            nameBufferSize);
}
