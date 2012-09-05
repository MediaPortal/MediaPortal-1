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
#include "..\..\shared\sectiondecoder.h"
#include "..\..\shared\channelinfo.h"
#include "..\..\shared\pidtable.h"
#include "..\..\shared\tsHeader.h"
#include <map>
#include <vector>
using namespace std;

class IVctCallBack
{
  public:
    virtual void OnVctReceived(const CChannelInfo& info) = 0;
};

#define PID_VCT 0x1ffb

class CVctParser : public CSectionDecoder
{
  public:
    CVctParser(void);
    virtual ~CVctParser(void);
    void Reset();
    void SetCallBack(IVctCallBack* callBack);
    void OnNewSection(CSection& section);
    bool IsReady();

  private:
    void DecodeServiceLocationDescriptor(byte* b, int length, int* hasVideo, int* hasAudio);
    void DecodeMultipleStrings(byte* b, int length, vector<char*>* strings);
    void DecodeString(byte* b, int compression_type, int mode, int number_bytes, char** string);

    IVctCallBack* m_pCallBack;
    map<unsigned int, bool> m_mSeenSections;
    bool m_bIsReady;
};
