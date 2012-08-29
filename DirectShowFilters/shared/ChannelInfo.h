/* 
 *	Copyright (C) 2006-2010 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#define CHANNEL_INFO_MAX_STRING_LENGTH 4095
#define DESCRIPTOR_MAX_STRING_LENGTH 255

class CChannelInfo
{
  public:
    CChannelInfo(void);
    CChannelInfo(const CChannelInfo& info);
    virtual ~CChannelInfo(void);
    CChannelInfo& operator = (const CChannelInfo &info);
    void Copy(const CChannelInfo &info);
    void Reset();

    int NetworkId;
    int TransportId;
    int ServiceId;
    char ServiceName[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
    char ProviderName[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
    char NetworkNames[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
    char LogicalChannelNumber[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
    int ServiceType;
    int HasVideo;
    int HasAudio;
    bool IsEncrypted;
    bool IsRunning;
    bool IsOtherMux;
    int PmtPid;
    bool IsPmtReceived;
    bool IsServiceInfoReceived;
    int PatVersion;
};
