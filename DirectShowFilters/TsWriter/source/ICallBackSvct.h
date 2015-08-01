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
#include "ICallBackTableParser.h"


class ICallBackSvct : public ICallBackTableParser
{
  public:
    virtual ~ICallBackSvct() {}

    virtual void OnSvctDcReceived(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned short virtualChannelNumber) {}
    virtual void OnSvctDcChanged(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned short virtualChannelNumber) {}
    virtual void OnSvctDcRemoved(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned short virtualChannelNumber) {}

    virtual void OnSvctVcReceived(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  bool splice,
                                  unsigned long activationTime,
                                  bool hdtvChannel,
                                  bool preferredSource,
                                  unsigned short virtualChannelNumber,
                                  bool applicationVirtualChannel,
                                  unsigned char bitstreamSelect,
                                  unsigned char pathSelect,
                                  unsigned char toneSelect,
                                  unsigned char transportType,
                                  unsigned char channelType,
                                  unsigned short sourceId,
                                  unsigned short nvodChannelBase,
                                  unsigned char cdtReference,
                                  unsigned short programNumber,
                                  unsigned char mmtReference,
                                  bool scrambled,
                                  unsigned char videoStandard,
                                  bool wideBandwidthAudio,
                                  bool compandedAudio,
                                  unsigned char matrixMode,
                                  unsigned short subcarrier2Offset,
                                  unsigned short subcarrier1Offset,
                                  unsigned char satelliteId,
                                  unsigned char transponder,
                                  bool suppressVideo,
                                  unsigned char audioSelection,
                                  unsigned long carrierFrequencyOverride,
                                  unsigned long symbolRateOverride,
                                  unsigned short majorChannelNumber,
                                  unsigned short minorChannelNumber,
                                  unsigned short transportStreamId,
                                  bool outOfBand,
                                  bool hideGuide,
                                  unsigned char serviceType) {}
    virtual void OnSvctVcChanged(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  bool splice,
                                  unsigned long activationTime,
                                  bool hdtvChannel,
                                  bool preferredSource,
                                  unsigned short virtualChannelNumber,
                                  bool applicationVirtualChannel,
                                  unsigned char bitstreamSelect,
                                  unsigned char pathSelect,
                                  unsigned char toneSelect,
                                  unsigned char transportType,
                                  unsigned char channelType,
                                  unsigned short sourceId,
                                  unsigned short nvodChannelBase,
                                  unsigned char cdtReference,
                                  unsigned short programNumber,
                                  unsigned char mmtReference,
                                  bool scrambled,
                                  unsigned char videoStandard,
                                  bool wideBandwidthAudio,
                                  bool compandedAudio,
                                  unsigned char matrixMode,
                                  unsigned short subcarrier2Offset,
                                  unsigned short subcarrier1Offset,
                                  unsigned char satelliteId,
                                  unsigned char transponder,
                                  bool suppressVideo,
                                  unsigned char audioSelection,
                                  unsigned long carrierFrequencyOverride,
                                  unsigned long symbolRateOverride,
                                  unsigned short majorChannelNumber,
                                  unsigned short minorChannelNumber,
                                  unsigned short transportStreamId,
                                  bool outOfBand,
                                  bool hideGuide,
                                  unsigned char serviceType) {}
    virtual void OnSvctVcRemoved(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  bool splice,
                                  unsigned long activationTime,
                                  bool hdtvChannel,
                                  bool preferredSource,
                                  unsigned short virtualChannelNumber,
                                  bool applicationVirtualChannel,
                                  unsigned char bitstreamSelect,
                                  unsigned char pathSelect,
                                  unsigned char toneSelect,
                                  unsigned char transportType,
                                  unsigned char channelType,
                                  unsigned short sourceId,
                                  unsigned short nvodChannelBase,
                                  unsigned char cdtReference,
                                  unsigned short programNumber,
                                  unsigned char mmtReference,
                                  bool scrambled,
                                  unsigned char videoStandard,
                                  bool wideBandwidthAudio,
                                  bool compandedAudio,
                                  unsigned char matrixMode,
                                  unsigned short subcarrier2Offset,
                                  unsigned short subcarrier1Offset,
                                  unsigned char satelliteId,
                                  unsigned char transponder,
                                  bool suppressVideo,
                                  unsigned char audioSelection,
                                  unsigned long carrierFrequencyOverride,
                                  unsigned long symbolRateOverride,
                                  unsigned short majorChannelNumber,
                                  unsigned short minorChannelNumber,
                                  unsigned short transportStreamId,
                                  bool outOfBand,
                                  bool hideGuide,
                                  unsigned char serviceType) {}

    virtual void OnSvctIcReceived(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned short sourceId,
                                  unsigned short virtualChannelNumber) {}
    virtual void OnSvctIcChanged(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned short sourceId,
                                  unsigned short virtualChannelNumber) {}
    virtual void OnSvctIcRemoved(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned short sourceId,
                                  unsigned short virtualChannelNumber) {}
};