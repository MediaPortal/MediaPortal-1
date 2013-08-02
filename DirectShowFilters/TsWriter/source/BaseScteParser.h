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
#include "..\..\shared\ChannelInfo.h"
#include "..\..\shared\SectionDecoder.h"
#include "AtscNitParser.h"
#include "LvctParser.h"
#include "NttParser.h"
#include "SvctParser.h"

class CBaseScteParser : public CSectionDecoder, ILvctCallBack, INttCallBack, ISvctCallBack
{
  public:
    CBaseScteParser(void);
    virtual ~CBaseScteParser(void);
    void Reset();
    void OnNewSection(CSection& section);
    bool IsReady();
    int GetChannelCount();
    bool GetChannel(int index, CChannelInfo** info);

    void OnLvctReceived(int tableId, char* name, int majorChannelNumber, int minorChannelNumber, int modulationMode, 
                        unsigned int carrierFrequency, int channelTsid, int programNumber, int etmLocation,
                        bool accessControlled, bool hidden, int pathSelect, bool outOfBand, bool hideGuide,
                        int serviceType, int sourceId, int videoStreamCount, int audioStreamCount,
                        vector<unsigned int>& languages);
    void OnNttReceived(int sourceId, bool applicationType, char* name, unsigned int lang);
    void OnSvctReceived(int transmissionMedium, int vctId, int virtualChannelNumber, bool applicationVirtualChannel,
                        int bitstreamSelect, int pathSelect, int channelType, int sourceId, byte cdsReference,
                        int programNumber, byte mmsReference);

  private:
    void CleanUp();

    bool m_bIsReady;
    CAtscNitParser m_nitParser;
    CLvctParser m_clvctParser;
    CLvctParser m_tlvctParser;
    CNttParser m_nttParser;
    CSvctParser m_svctParser;
    map<int, CChannelInfo*> m_mServices;
    map<int, bool> m_mServicesWithoutNames;
};