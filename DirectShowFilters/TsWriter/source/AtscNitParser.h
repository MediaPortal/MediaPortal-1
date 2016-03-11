/*
* Copyright (C) 2006-2008 Team MediaPortal
* http://www.team-mediaportal.com
*
* This Program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2, or (at your option)
* any later version.
*
* This Program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with GNU Make; see the file COPYING. If not, write to
* the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
* http://www.gnu.org/copyleft/gpl.html
*
*/
#pragma once
#include <Windows.h>
#include <map>
#include <vector>
#include "..\..\shared\SectionDecoder.h"
using namespace std;

class CAtscNitParser : public CSectionDecoder
{
  public:
    CAtscNitParser();
    virtual ~CAtscNitParser(void);
    void Reset();
    void OnNewSection(CSection& section);
    bool IsReady();
    int GetCarrierDefinition(int reference);
    int GetModulationMode(int reference);

  private:
    bool DecodeCarrierDefinition(byte* section, int& pointer, int endOfSection, byte& firstIndex, int transmissionMedium);
    bool DecodeModulationMode(byte* section, int& pointer, int endOfSection, byte& firstIndex, int transmissionMedium);
    bool DecodeSatelliteInformation(byte* section, int& pointer, int endOfSection);
    bool DecodeTransponderData(byte* section, int& pointer, int endOfSection);
    void DecodeRevisionDetectionDescriptor(byte* b, int length, int tableSubtype);

    map<int, int> m_mCurrentVersions;
    map<int, vector<int>*> m_mUnseenSections;
    bool m_bIsReady;
    map<int, int> m_mCarrierDefinitions;
    map<int, int> m_mModulationModes;
};