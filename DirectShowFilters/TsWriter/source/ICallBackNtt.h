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


class ICallBackNtt : public ICallBackTableParser
{
  public:
    virtual ~ICallBackNtt() {}

    virtual void OnNttTntReceived(unsigned char transmissionMedium,
                                  unsigned char index,
                                  unsigned char satelliteId,
                                  unsigned char transponderNumber,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttTntChanged(unsigned char transmissionMedium,
                                  unsigned char index,
                                  unsigned char satelliteId,
                                  unsigned char transponderNumber,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttTntRemoved(unsigned char transmissionMedium,
                                  unsigned char index,
                                  unsigned char satelliteId,
                                  unsigned char transponderNumber,
                                  unsigned long language,
                                  const char* name) {}

    virtual void OnNttSttReceived(unsigned char transmissionMedium,
                                  unsigned char index,
                                  unsigned char satelliteId,
                                  unsigned long language,
                                  const char* referenceName,
                                  const char* fullName) {}
    virtual void OnNttSttChanged(unsigned char transmissionMedium,
                                  unsigned char index,
                                  unsigned char satelliteId,
                                  unsigned long language,
                                  const char* referenceName,
                                  const char* fullName) {}
    virtual void OnNttSttRemoved(unsigned char transmissionMedium,
                                  unsigned char index,
                                  unsigned char satelliteId,
                                  unsigned long language,
                                  const char* referenceName,
                                  const char* fullName) {}

    virtual void OnNttRttReceived(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long language,
                                  unsigned char dimensionIndex,
                                  const char* dimensionName,
                                  unsigned char levelIndex,
                                  const char* levelName) {}
    virtual void OnNttRttChanged(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long language,
                                  unsigned char dimensionIndex,
                                  const char* dimensionName,
                                  unsigned char levelIndex,
                                  const char* levelName) {}
    virtual void OnNttRttRemoved(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long language,
                                  unsigned char dimensionIndex,
                                  const char* dimensionName,
                                  unsigned char levelIndex,
                                  const char* levelName) {}

    virtual void OnNttRstReceived(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttRstChanged(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttRstRemoved(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long language,
                                  const char* name) {}

    virtual void OnNttSntReceived(unsigned char transmissionMedium,
                                  bool applicationType,
                                  unsigned short sourceId,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttSntChanged(unsigned char transmissionMedium,
                                  bool applicationType,
                                  unsigned short sourceId,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttSntRemoved(unsigned char transmissionMedium,
                                  bool applicationType,
                                  unsigned short sourceId,
                                  unsigned long language,
                                  const char* name) {}

    virtual void OnNttMntReceived(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttMntChanged(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttMntRemoved(unsigned char transmissionMedium,
                                  unsigned short vctId,
                                  unsigned long language,
                                  const char* name) {}

    virtual void OnNttCstReceived(unsigned char transmissionMedium,
                                  unsigned char currencyRegion,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttCstChanged(unsigned char transmissionMedium,
                                  unsigned char currencyRegion,
                                  unsigned long language,
                                  const char* name) {}
    virtual void OnNttCstRemoved(unsigned char transmissionMedium,
                                  unsigned char currencyRegion,
                                  unsigned long language,
                                  const char* name) {}
};