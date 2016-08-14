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
#include "CriticalSection.h"
#include "ICallBackMgt.h"
#include "IRecord.h"
#include "RecordStore.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_MGT 0xc7


extern void LogDebug(const wchar_t* fmt, ...);

class CParserMgt
{
  public:
    CParserMgt(unsigned short pid);
    virtual ~CParserMgt();

    void Reset();
    void SetCallBack(ICallBackMgt* callBack);
    void OnNewSection(CSection& section);
    bool IsSeen() const;
    bool IsReady() const;

    unsigned short GetTableCount() const;
    bool GetTable(unsigned short index,
                  unsigned short& tableType,
                  unsigned short& pid,
                  unsigned char& versionNumber,
                  unsigned long& numberBytes) const;

  private:
    class CRecordMgt : public IRecord
    {
      public:
        CRecordMgt()
        {
          TableType = 0;
          Pid = 0;
          VersionNumber = 0;
          NumberBytes = 0;
        }

        ~CRecordMgt()
        {
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMgt* recordMgt = dynamic_cast<const CRecordMgt*>(record);
          if (
            recordMgt == NULL ||
            TableType != recordMgt->TableType ||
            Pid != recordMgt->Pid ||
            VersionNumber != recordMgt->VersionNumber ||
            NumberBytes != recordMgt->NumberBytes
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return TableType;
        }

        unsigned long long GetExpiryKey() const
        {
          return 0;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MGT: table %s, table type = %hu, PID = %hu, version number = %hhu, number bytes = %lu",
                    situation, TableType, Pid, VersionNumber, NumberBytes);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackMgt* callBackMgt = static_cast<ICallBackMgt*>(callBack);
          if (callBackMgt != NULL)
          {
            callBackMgt->OnMgtReceived(TableType, Pid, VersionNumber, NumberBytes);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackMgt* callBackMgt = static_cast<ICallBackMgt*>(callBack);
          if (callBackMgt != NULL)
          {
            callBackMgt->OnMgtChanged(TableType, Pid, VersionNumber, NumberBytes);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackMgt* callBackMgt = static_cast<ICallBackMgt*>(callBack);
          if (callBackMgt != NULL)
          {
            callBackMgt->OnMgtRemoved(TableType, Pid, VersionNumber, NumberBytes);
          }
        }

        unsigned short TableType;
        unsigned short Pid;
        unsigned char VersionNumber;
        unsigned long NumberBytes;
    };

    static bool DecodeTableRecord(unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordMgt& record);

    CCriticalSection m_section;
    unsigned short m_pid;
    vector<unsigned short> m_seenSections;
    vector<unsigned short> m_unseenSections;
    ICallBackMgt* m_callBack;
    CRecordStore m_records;
};