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


class ICallBackNitAtsc : public ICallBackTableParser
{
  public:
    virtual ~ICallBackNitAtsc() {}

    virtual void OnNitCdtReceived(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned long frequency) {}
    virtual void OnNitCdtChanged(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned long frequency) {}
    virtual void OnNitCdtRemoved(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned long frequency) {}

    virtual void OnNitMmtReceived(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char transmissionSystem,
                                  unsigned char innerCodingMode,
                                  bool splitBitstreamMode,
                                  unsigned char modulationFormat,
                                  unsigned long symbolRate) {}
    virtual void OnNitMmtChanged(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char transmissionSystem,
                                  unsigned char innerCodingMode,
                                  bool splitBitstreamMode,
                                  unsigned char modulationFormat,
                                  unsigned long symbolRate) {}
    virtual void OnNitMmtRemoved(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char transmissionSystem,
                                  unsigned char innerCodingMode,
                                  bool splitBitstreamMode,
                                  unsigned char modulationFormat,
                                  unsigned long symbolRate) {}

    virtual void OnNitSitReceived(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  bool youAreHere,
                                  unsigned char frequencyBand,
                                  bool outOfService,
                                  unsigned char hemisphere,
                                  unsigned short orbitalPosition,
                                  unsigned char polarisationType,
                                  unsigned char numberOfTransponders) {}
    virtual void OnNitSitChanged(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  bool youAreHere,
                                  unsigned char frequencyBand,
                                  bool outOfService,
                                  unsigned char hemisphere,
                                  unsigned short orbitalPosition,
                                  unsigned char polarisationType,
                                  unsigned char numberOfTransponders) {}
    virtual void OnNitSitRemoved(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  bool youAreHere,
                                  unsigned char frequencyBand,
                                  bool outOfService,
                                  unsigned char hemisphere,
                                  unsigned short orbitalPosition,
                                  unsigned char polarisationType,
                                  unsigned char numberOfTransponders) {}

    virtual void OnNitTdtReceived(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  unsigned char transponderNumber,
                                  unsigned char transportType,
                                  unsigned char polarisation,
                                  unsigned char cdtReference,
                                  unsigned char mmtReference,
                                  unsigned short vctId,
                                  bool rootTransponder,
                                  bool wideBandwidthVideo,
                                  unsigned char waveformStandard,
                                  bool wideBandwidthAudio,
                                  bool compandedAudio,
                                  unsigned char matrixMode,
                                  unsigned short subcarrier2Offset,
                                  unsigned short subcarrier1Offset,
                                  unsigned long carrierFrequencyOverride) {}
    virtual void OnNitTdtChanged(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  unsigned char transponderNumber,
                                  unsigned char transportType,
                                  unsigned char polarisation,
                                  unsigned char cdtReference,
                                  unsigned char mmtReference,
                                  unsigned short vctId,
                                  bool rootTransponder,
                                  bool wideBandwidthVideo,
                                  unsigned char waveformStandard,
                                  bool wideBandwidthAudio,
                                  bool compandedAudio,
                                  unsigned char matrixMode,
                                  unsigned short subcarrier2Offset,
                                  unsigned short subcarrier1Offset,
                                  unsigned long carrierFrequencyOverride) {}
    virtual void OnNitTdtRemoved(unsigned char index,
                                  unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  unsigned char transponderNumber,
                                  unsigned char transportType,
                                  unsigned char polarisation,
                                  unsigned char cdtReference,
                                  unsigned char mmtReference,
                                  unsigned short vctId,
                                  bool rootTransponder,
                                  bool wideBandwidthVideo,
                                  unsigned char waveformStandard,
                                  bool wideBandwidthAudio,
                                  bool compandedAudio,
                                  unsigned char matrixMode,
                                  unsigned short subcarrier2Offset,
                                  unsigned short subcarrier1Offset,
                                  unsigned long carrierFrequencyOverride) {}
};