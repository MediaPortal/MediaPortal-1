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
#include <map>
#include <vector>
#include "ICallBackTableParser.h"

using namespace std;


class ICallBackLvct : public ICallBackTableParser
{
  public:
    virtual ~ICallBackLvct() {}

    virtual void OnLvctReceived(unsigned char tableId,
                                unsigned short sectionTransportStreamId,
                                unsigned short mapId,
                                const char* shortName,
                                const map<unsigned long, char*>& longNames,
                                unsigned short majorChannelNumber,
                                unsigned short minorChannelNumber,
                                unsigned char modulationMode,
                                unsigned long carrierFrequency,
                                unsigned short transportStreamId,
                                unsigned short programNumber,
                                unsigned char etmLocation,
                                bool accessControlled,
                                bool hidden,
                                bool pathSelect,
                                bool outOfBand,
                                bool hideGuide,
                                unsigned char serviceType,
                                unsigned short sourceId,
                                unsigned char streamCountVideo,
                                unsigned char streamCountAudio,
                                bool isThreeDimensional,
                                const vector<unsigned long>& audioLanguages,
                                const vector<unsigned long>& captionsLanguages) {}
    virtual void OnLvctChanged(unsigned char tableId,
                                unsigned short sectionTransportStreamId,
                                unsigned short mapId,
                                const char* shortName,
                                const map<unsigned long, char*>& longNames,
                                unsigned short majorChannelNumber,
                                unsigned short minorChannelNumber,
                                unsigned char modulationMode,
                                unsigned long carrierFrequency,
                                unsigned short transportStreamId,
                                unsigned short programNumber,
                                unsigned char etmLocation,
                                bool accessControlled,
                                bool hidden,
                                bool pathSelect,
                                bool outOfBand,
                                bool hideGuide,
                                unsigned char serviceType,
                                unsigned short sourceId,
                                unsigned char streamCountVideo,
                                unsigned char streamCountAudio,
                                bool isThreeDimensional,
                                const vector<unsigned long>& audioLanguages,
                                const vector<unsigned long>& captionsLanguages) {}
    virtual void OnLvctRemoved(unsigned char tableId,
                                unsigned short sectionTransportStreamId,
                                unsigned short mapId,
                                const char* shortName,
                                const map<unsigned long, char*>& longNames,
                                unsigned short majorChannelNumber,
                                unsigned short minorChannelNumber,
                                unsigned char modulationMode,
                                unsigned long carrierFrequency,
                                unsigned short transportStreamId,
                                unsigned short programNumber,
                                unsigned char etmLocation,
                                bool accessControlled,
                                bool hidden,
                                bool pathSelect,
                                bool outOfBand,
                                bool hideGuide,
                                unsigned char serviceType,
                                unsigned short sourceId,
                                unsigned char streamCountVideo,
                                unsigned char streamCountAudio,
                                bool isThreeDimensional,
                                const vector<unsigned long>& audioLanguages,
                                const vector<unsigned long>& captionsLanguages) {}
};