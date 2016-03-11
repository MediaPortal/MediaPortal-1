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
#pragma once
#include <Windows.h>
#include <map>
#include <vector>
#include "..\..\shared\SectionDecoder.h"
using namespace std;

class INttCallBack
{
  public:
    virtual void OnNttReceived(int sourceId, bool applicationType, char* name, unsigned int language) = 0;
};

class CNttParser : public CSectionDecoder
{
  public:
    CNttParser(void);
    virtual ~CNttParser(void);

    void Reset();
    void SetCallBack(INttCallBack* callBack);
    void OnNewSection(CSection& sections);
    bool IsReady();

  private:
    bool DecodeTransponderName(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    bool DecodeSatelliteText(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    bool DecodeRatingsText(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    bool DecodeRatingSystem(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    bool DecodeCurrencySystem(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    bool DecodeSourceName(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    bool DecodeMapName(byte* section, int& pointer, int endOfSection, unsigned int languageCode);
    void DecodeMultilingualText(byte* b, int length, char** string);
    void DecodeRevisionDetectionDescriptor(byte* b, int length, int tableSubtype);

    INttCallBack* m_pCallBack;
    map<int, int> m_mCurrentVersions;
    map<int, vector<int>*> m_mUnseenSections;
    bool m_bIsReady;
};