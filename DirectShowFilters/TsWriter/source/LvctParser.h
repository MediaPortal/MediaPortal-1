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
#include <algorithm>
#include <vector>
#include "..\..\shared\SectionDecoder.h"
using namespace std;

class ILvctCallBack
{
  public:
    virtual void OnLvctReceived(int tableId, char* name, int majorChannelNumber, int minorChannelNumber, int modulationMode, 
                                unsigned int carrierFrequency, int channelTsid, int programNumber, int etmLocation,
                                bool accessControlled, bool hidden, int pathSelect, bool outOfBand, bool hideGuide,
                                int serviceType, int sourceId, int videoStreamCount, int audioStreamCount,
                                vector<unsigned int>& languages) = 0;
};

#define PID_ATSC_BASE_PID 0x1ffb
#define PID_SCTE_BASE_PID 0x1ffc

class CLvctParser : public CSectionDecoder
{
  public:
    CLvctParser(int pid);
    virtual ~CLvctParser(void);
    void Reset();
    void SetCallBack(ILvctCallBack* callBack);
    void OnNewSection(CSection& section);
    bool IsReady();

  private:
    CLvctParser();
    void DecodeServiceLocationDescriptor(byte* b, int length, int* videoStreamCount, int* audioStreamCount, vector<unsigned int>* languages);
    void DecodeMultipleStrings(byte* b, int length, vector<char*>* strings);
    void DecodeString(byte* b, int compression_type, int mode, int number_bytes, char** string);

    ILvctCallBack* m_pCallBack;
    int m_iCurrentVersion;
    vector<int> m_mUnseenSections;
    bool m_bIsReady;
};