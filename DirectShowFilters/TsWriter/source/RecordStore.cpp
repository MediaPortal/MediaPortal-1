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
#include "RecordStore.h"
#include <ctime>
#include "..\..\shared\TimeUtils.h"


CRecordStore::CRecordStore(unsigned long expiryTimeout)
{
  m_expireNaturally = true;
  m_expiryTimeout = expiryTimeout;
}

CRecordStore::~CRecordStore()
{
  RemoveAllRecords();
}

bool CRecordStore::AddOrUpdateRecord(IRecord** record, void* callBack)
{
  IRecord* newRecord = *record;
  if (newRecord == NULL)
  {
    return false;
  }

  unsigned long long key = (*record)->GetKey();
  map<unsigned long long, IRecord*>::const_iterator existingRecordIt = m_records.find(key);
  if (existingRecordIt == m_records.end() || existingRecordIt->second == NULL)
  {
    //newRecord->Debug(L"received");
    m_records[key] = newRecord;
    newRecord->OnReceived(callBack);
    return true;
  }

  IRecord* existingRecord = existingRecordIt->second;
  if (
    existingRecord->LastSeen != 0 &&
    CTimeUtils::ElapsedMillis(existingRecord->LastSeen) <= m_expiryTimeout
  )
  {
    existingRecord->Debug(L"key duplicate 1 [old]");
    newRecord->Debug(L"key duplicate 2 [new]");
    delete newRecord;
    *record = NULL;
    return false;
  }

  if (!existingRecord->Equals(newRecord))
  {
    newRecord->Debug(L"changed");
    delete existingRecord;
    m_records[key] = newRecord;
    newRecord->OnChanged(callBack);
    return true;
  }

  // No change.
  existingRecord->LastSeen = clock();
  delete newRecord;
  *record = NULL;
  return false;
}

void CRecordStore::MarkExpiredRecords(unsigned long long key)
{
  m_expireNaturally = false;
  map<unsigned long long, IRecord*>::const_iterator recordIt = m_records.begin();
  while (recordIt != m_records.end())
  {
    IRecord* record = recordIt->second;
    if (record == NULL)
    {
      m_records.erase(recordIt++);
    }
    else
    {
      if (record->GetExpiryKey() == key)
      {
        record->LastSeen = 0;
      }
      recordIt++;
    }
  }
}

unsigned long CRecordStore::RemoveExpiredRecords(void* callBack)
{
  return InternalRemoveExpiredRecords(callBack, false, 0);
}

unsigned long CRecordStore::RemoveExpiredRecords(void* callBack,
                                                  unsigned long long subsetKey)
{
  return InternalRemoveExpiredRecords(callBack, true, subsetKey);
}

void CRecordStore::RemoveAllRecords()
{
  map<unsigned long long, IRecord*>::iterator recordIt = m_records.begin();
  for ( ; recordIt != m_records.end(); recordIt++)
  {
    IRecord* record = recordIt->second;
    if (record != NULL)
    {
      delete record;
      recordIt->second = NULL;
    }
  }
  m_records.clear();
}

unsigned long CRecordStore::GetRecordCount() const
{
  return m_records.size();
}

bool CRecordStore::GetRecordByIndex(unsigned long index, IRecord** record) const
{
  if (index >= m_records.size())
  {
    *record = NULL;
    return false;
  }

  map<unsigned long long, IRecord*>::const_iterator recordIt = m_records.begin();
  for ( ; recordIt != m_records.end(); recordIt++)
  {
    if (index == 0)
    {
      *record = recordIt->second;
      return true;
    }
    index--;
  }

  *record = NULL;
  return false;
}

bool CRecordStore::GetRecordByKey(unsigned long long key, IRecord** record) const
{
  map<unsigned long long, IRecord*>::const_iterator recordIt = m_records.find(key);
  if (recordIt != m_records.end())
  {
    *record = recordIt->second;
    return true;
  }

  *record = NULL;
  return false;
}

unsigned long CRecordStore::InternalRemoveExpiredRecords(void* callBack,
                                                          bool isSubsetKeyValid,
                                                          unsigned long long subsetKey)
{
  unsigned long expiredRecordCount = 0;
  map<unsigned long long, IRecord*>::iterator recordIt = m_records.begin();
  while (recordIt != m_records.end())
  {
    IRecord* record = recordIt->second;
    if (record == NULL)
    {
      m_records.erase(recordIt++);
    }
    else if (
      (m_expireNaturally && CTimeUtils::ElapsedMillis(record->LastSeen) >= m_expiryTimeout) ||
      (
        !m_expireNaturally &&
        record->LastSeen == 0 &&
        (
          !isSubsetKeyValid ||
          subsetKey == record->GetExpiryKey()
        )
      )
    )
    {
      record->Debug(L"removed");
      record->OnRemoved(callBack);
      delete record;
      recordIt->second = NULL;
      m_records.erase(recordIt++);
      expiredRecordCount++;
    }
    else
    {
      recordIt++;
    }
  }
  return expiredRecordCount;
}