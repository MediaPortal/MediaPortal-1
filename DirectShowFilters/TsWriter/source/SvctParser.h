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

class ISvctCallBack
{
  public:
    virtual void OnSvctReceived(int transmissionMedium, int vctId, int virtualChannelNumber, bool applicationVirtualChannel,
                                int bitstreamSelect, int pathSelect, int channelType, int sourceId, byte cdsReference,
                                int programNumber, byte mmsReference) = 0;
};

class CSvctParser : public CSectionDecoder
{
  public:
    CSvctParser(void);
    virtual ~CSvctParser(void);

    void Reset();
    void SetCallBack(ISvctCallBack* callBack);
    void OnNewSection(CSection& sections);
    bool IsReady();

  private:
    bool DecodeVirtualChannelMap(byte* section, int& pointer, int endOfSection, int transmissionMedium, int vctId);
    bool DecodeDefinedChannelMap(byte* section, int& pointer, int endOfSection);
    bool DecodeInverseChannelMap(byte* section, int& pointer, int endOfSection);
    void DecodeRevisionDetectionDescriptor(byte* b, int length, int tableSubtype);

    ISvctCallBack* m_pCallBack;
    map<int, int> m_mCurrentVersions;
    map<int, vector<int>*> m_mUnseenSections;
    bool m_bIsReady;
    map<int, bool> m_mSeenVirtualChannels;
    map<int, bool> m_mDefinedChannelMap;  // this map only holds the defined channels
};