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
#include <vector>
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "CriticalSection.h"
#include "ICallBackPat.h"
#include "IRecord.h"
#include "RecordStore.h"

using namespace MediaPortal;
using namespace std;


#define PID_PAT 0
#define TABLE_ID_PAT 0


extern void LogDebug(const wchar_t* fmt, ...);

class CParserPat : public CSectionDecoder
{
  public:
    CParserPat();
    virtual ~CParserPat();

    void Reset();
    void SetCallBack(ICallBackPat* callBack);
    void OnNewSection(CSection& section);
    bool IsReady() const;

    void GetTransportStreamDetail(unsigned short& transportStreamId,
                                  unsigned short& networkPid,
                                  unsigned short& programCount) const;
    bool GetProgram(unsigned short index,
                    unsigned short& programNumber,
                    unsigned short& pmtPid) const;
    bool GetPmtPid(unsigned short programNumber, unsigned short& pmtPid) const;

  private:
    class CRecordPat : public IRecord
    {
      public:
        CRecordPat()
        {
          ProgramNumber = 0;
          Pid = 0;
        }

        ~CRecordPat()
        {
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordPat* recordPat = dynamic_cast<const CRecordPat*>(record);
          if (
            recordPat == NULL ||
            ProgramNumber != recordPat->ProgramNumber ||
            Pid != recordPat->Pid
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ProgramNumber;
        }

        unsigned long long GetExpiryKey() const
        {
          return 0;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"PAT: program %s, program number = %hu, PID = %hu",
                    situation, ProgramNumber, Pid);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackPat* callBackPat = static_cast<ICallBackPat*>(callBack);
          if (callBackPat != NULL)
          {
            callBackPat->OnPatProgramReceived(ProgramNumber, Pid);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackPat* callBackPat = static_cast<ICallBackPat*>(callBack);
          if (callBackPat != NULL)
          {
            callBackPat->OnPatProgramChanged(ProgramNumber, Pid);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackPat* callBackPat = static_cast<ICallBackPat*>(callBack);
          if (callBackPat != NULL)
          {
            callBackPat->OnPatProgramRemoved(ProgramNumber, Pid);
          }
        }

        unsigned short ProgramNumber;
        unsigned short Pid;
    };

    CCriticalSection m_section;
    bool m_isReady;
    unsigned char m_version;
    vector<unsigned char> m_unseenSections;
    ICallBackPat* m_callBack;
    unsigned short m_transportStreamId;
    unsigned short m_networkPid;
    CRecordStore m_records;
};