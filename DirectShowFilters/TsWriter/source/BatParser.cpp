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
#include "BatParser.h"

extern void LogDebug(const char *fmt, ...) ;
extern bool DisableCRCCheck();

CBatParser::CBatParser(void)
{
  m_sName = "BatParser";
  SetPid(PID_BAT);
  m_vTableIds.push_back(0x4a);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset();
}

CBatParser::~CBatParser(void)
{
}

void CBatParser::GetBouquetIds(int originalNetworkId, int transportStreamId, int serviceId, vector<int>* bouquetIds)
{
  GetNetworkIds(originalNetworkId, transportStreamId, serviceId, bouquetIds);
}

int CBatParser::GetBouquetNameCount(int bouquetId)
{
  return GetNetworkNameCount(bouquetId);
}

void CBatParser::GetBouquetName(int bouquetId, int index, unsigned int* language, char** name)
{
  GetNetworkName(bouquetId, index, language, name);
}