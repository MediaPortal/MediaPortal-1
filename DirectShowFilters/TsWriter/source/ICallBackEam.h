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


class ICallBackEam : public ICallBackTableParser
{
  public:
    virtual ~ICallBackEam() {}

    virtual void OnEamReceived(unsigned short id,
                                unsigned long originatorCode,
                                const char* eventCode,
                                const map<unsigned long, char*>& NatureOfActivationTexts,
                                unsigned char alertMessageTimeRemaining,
                                unsigned long eventStartTime,
                                unsigned short eventDuration,
                                unsigned char alertPriority,
                                unsigned short detailsOobSourceId,
                                unsigned short detailsMajorChannelNumber,
                                unsigned short detailsMinorChannelNumber,
                                unsigned char detailsRfChannel,
                                unsigned short detailsProgramNumber,
                                unsigned short audioOobSourceId,
                                const map<unsigned long, char*>& alertTexts,
                                const vector<unsigned long>& locationCodes,
                                const vector<unsigned long>& exceptions,
                                const vector<unsigned long>& alternativeExceptions) = 0;
    virtual void OnEamChanged(unsigned short id,
                                unsigned long originatorCode,
                                const char* eventCode,
                                const map<unsigned long, char*>& NatureOfActivationTexts,
                                unsigned char alertMessageTimeRemaining,
                                unsigned long eventStartTime,
                                unsigned short eventDuration,
                                unsigned char alertPriority,
                                unsigned short detailsOobSourceId,
                                unsigned short detailsMajorChannelNumber,
                                unsigned short detailsMinorChannelNumber,
                                unsigned char detailsRfChannel,
                                unsigned short detailsProgramNumber,
                                unsigned short audioOobSourceId,
                                const map<unsigned long, char*>& alertTexts,
                                const vector<unsigned long>& locationCodes,
                                const vector<unsigned long>& exceptions,
                                const vector<unsigned long>& alternativeExceptions) = 0;
    virtual void OnEamRemoved(unsigned short id,
                                unsigned long originatorCode,
                                const char* eventCode,
                                const map<unsigned long, char*>& NatureOfActivationTexts,
                                unsigned char alertMessageTimeRemaining,
                                unsigned long eventStartTime,
                                unsigned short eventDuration,
                                unsigned char alertPriority,
                                unsigned short detailsOobSourceId,
                                unsigned short detailsMajorChannelNumber,
                                unsigned short detailsMinorChannelNumber,
                                unsigned char detailsRfChannel,
                                unsigned short detailsProgramNumber,
                                unsigned short audioOobSourceId,
                                const map<unsigned long, char*>& alertTexts,
                                const vector<unsigned long>& locationCodes,
                                const vector<unsigned long>& exceptions,
                                const vector<unsigned long>& alternativeExceptions) = 0;
};