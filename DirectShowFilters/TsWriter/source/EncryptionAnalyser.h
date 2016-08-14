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
#include "..\..\shared\TsHeader.h"
#include "CriticalSection.h"
#include "EncryptionState.h"
#include "ICallBackEncryptionAnalyser.h"
#include "IEncryptionAnalyser.h"

using namespace MediaPortal;
using namespace std;


class CEncryptionAnalyser : public IEncryptionAnalyser
{
  public:
    CEncryptionAnalyser();
    virtual ~CEncryptionAnalyser();

    void Reset();
    void SetCallBack(ICallBackEncryptionAnalyser* callBack);
    bool OnTsPacket(CTsHeader& header, unsigned char* tsPacket);

    EncryptionState GetPidState(unsigned short pid);

  private:
    typedef struct PidState
    {
      unsigned short Pid;
      EncryptionState State;
      unsigned short PacketCount;        // The number of consecutive packets which do not match the current state.
    } PidState;

    void CleanUp();

    CCriticalSection m_section;
    map<unsigned short, PidState*> m_pids;
    ICallBackEncryptionAnalyser* m_callBack;
};