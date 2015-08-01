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
#include <sstream>
#include "EnterCriticalSection.h"

using namespace MediaPortal;
using namespace std;


CParserBat::CParserBat(void)
{
  CParserBat::SetPid(PID_BAT);
  m_tableIds.clear();
  m_tableIds.push_back(TABLE_ID_BAT);
}

CParserBat::~CParserBat(void)
{
  CEnterCriticalSection lock(m_section);
  CleanUp();
}

void CParserBat::SetPid(unsigned short pid)
{
  CEnterCriticalSection lock(m_section);
  wstringstream s;
  s << L"BAT " << pid;
  wcsncpy(m_name, s.str().c_str(), sizeof(m_name) / sizeof(m_name[0]));
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

bool CParserBat::GetService(unsigned short originalNetworkId,
                            unsigned short transportStreamId,
                            unsigned short serviceId,
                            unsigned short preferredLogicalChannelNumberBouquetId,
                            unsigned short preferredLogicalChannelNumberRegionId,
                            unsigned short& freesatChannelId,
                            unsigned short& openTvChannelId,
                            unsigned short& logicalChannelNumber,
                            bool& visibleInGuide,
                            unsigned short* bouquetIds,
                            unsigned char& bouquetIdCount,
                            unsigned long* availableInCells,
                            unsigned char& availableInCellCount,
                            unsigned long long* targetRegionIds,
                            unsigned char& targetRegionIdCount,
                            unsigned short* freesatRegionIds,
                            unsigned char& freesatRegionIdCount,
                            unsigned short* openTvRegionIds,
                            unsigned char& openTvRegionIdCount,
                            unsigned short* freesatChannelCategoryIds,
                            unsigned char& freesatChannelCategoryIdCount,
                            unsigned char* norDigChannelListIds,
                            unsigned char& norDigChannelListIdCount,
                            unsigned long* availableInCountries,
                            unsigned char& availableInCountryCount,
                            unsigned long* unavailableInCountries,
                            unsigned char& unavailableInCountryCount) const
{
  return CParserNitDvb::GetService(originalNetworkId,
                                    transportStreamId,
                                    serviceId,
                                    preferredLogicalChannelNumberBouquetId,
                                    preferredLogicalChannelNumberRegionId,
                                    freesatChannelId,
                                    openTvChannelId,
                                    logicalChannelNumber,
                                    visibleInGuide,
                                    bouquetIds,
                                    bouquetIdCount,
                                    availableInCells,
                                    availableInCellCount,
                                    targetRegionIds,
                                    targetRegionIdCount,
                                    freesatRegionIds,
                                    freesatRegionIdCount,
                                    openTvRegionIds,
                                    openTvRegionIdCount,
                                    freesatChannelCategoryIds,
                                    freesatChannelCategoryIdCount,
                                    norDigChannelListIds,
                                    norDigChannelListIdCount,
                                    availableInCountries,
                                    availableInCountryCount,
                                    unavailableInCountries,
                                    unavailableInCountryCount);
}

unsigned char CParserBat::GetBouquetNameCount(unsigned short bouquetId) const
{
  return GetNetworkNameCount(bouquetId);
}

bool CParserBat::GetBouquetNameByIndex(unsigned short bouquetId,
                                        unsigned char index,
                                        unsigned long& language,
                                        char* name,
                                        unsigned short nameBufferCount) const
{
  return GetNetworkNameByIndex(bouquetId, index, language, name, nameBufferCount);
}

bool CParserBat::GetBouquetNameByLanguage(unsigned short bouquetId,
                                          unsigned long language,
                                          char* name,
                                          unsigned short nameBufferCount) const
{
  return GetNetworkNameByLanguage(bouquetId, language, name, nameBufferCount);
}