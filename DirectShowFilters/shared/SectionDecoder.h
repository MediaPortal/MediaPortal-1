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
#include "ISectionCallback.h"
#include "Section.h"
#include "TsHeader.h"


#define MAX_SECTIONS 256


class CSectionDecoder
{
  public:
    CSectionDecoder();
    virtual ~CSectionDecoder();

    void Reset();
    void SetCallBack(ISectionCallback* callBack);
    void SetPid(int pid);
    int GetPid() const;
    void EnableCrcCheck(bool enable);

    virtual void OnTsPacket(const unsigned char* tsPacket);
    virtual void OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);
    virtual void OnNewSection(const CSection& section);

    // TODO Kept only for compatibility with TsReader PAT and PMT parsers. Remove if/when possible.
    virtual void OnNewSection(CSection& section);

  private:
    int m_pid;
    CSection m_section;
    CTsHeader m_header;
    unsigned char m_continuityCounter;
    bool m_isCrcCheckEnabled;
    ISectionCallback* m_callback;
};
