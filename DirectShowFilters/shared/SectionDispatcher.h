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
#include <cstddef>    // NULL
#include <vector>
#include "CriticalSection.h"
#include "EnterCriticalSection.h"
#include "ISectionCallback.h"
#include "ISectionDispatcher.h"
#include "Section.h"
#include "Thread.h"

using namespace std;
using namespace MediaPortal;


#define PROCESSOR_COUNT 4
#define SECTION_QUEUE_LENGTH_LIMIT 100


extern void LogDebug(const wchar_t* fmt, ...);

class CSectionDispatcher : public ISectionDispatcher
{
  public:
    CSectionDispatcher();
    ~CSectionDispatcher();

    void DequeueSections(ISectionCallback& sectionDelegate);
    void EnqueueSection(unsigned short pid,
                        unsigned char tableId,
                        const CSection& section,
                        ISectionCallback& sectionDelegate);

  private:
    class CQueuedSection
    {
      public:
        CQueuedSection(unsigned short pid, unsigned char tableId, CSection* section, ISectionCallback& sectionDelegate)
          : m_delegate(sectionDelegate)
        {
          m_pid = pid;
          m_tableId = tableId;
          m_section = section;
        }

        ~CQueuedSection()
        {
          if (m_section != NULL)
          {
            delete m_section;
            m_section = NULL;
          }
        }

        unsigned short Pid() const
        {
          return m_pid;
        }

        unsigned char TableId() const
        {
          return m_tableId;
        }

        CSection* Section() const
        {
          return m_section;
        }

        ISectionCallback& Delegate() const
        {
          return m_delegate;
        }

      private:
        unsigned short m_pid;
        unsigned char m_tableId;
        CSection* m_section;
        ISectionCallback& m_delegate;
    };

    class CSectionProcessor
    {
      public:
        CSectionProcessor()
        {
          m_queueLengthMaximum = 0;
          m_isQueueFull = false;
          m_thread.Start(INFINITE, &SectionProcessorThreadFunction, this);
        }

        ~CSectionProcessor()
        {
          m_thread.Stop();
          ClearSectionQueue();
        }

        void DequeueSections(ISectionCallback& sectionDelegate)
        {
          CEnterCriticalSection lock(m_queueSection);
          vector<const CQueuedSection*>::iterator it = m_queue.begin();
          while (it != m_queue.end())
          {
            const CQueuedSection* section = *it;
            if (&(section->Delegate()) == &sectionDelegate)
            {
              delete section;
              *it = NULL;
              it = m_queue.erase(it);
            }
            else
            {
              it++;
            }
          }
        }

        void EnqueueSection(const CQueuedSection* section)
        {
          if (section == NULL)
          {
            return;
          }

          size_t queueSize = 0;
          {
            CEnterCriticalSection lock(m_queueSection);
            if (m_isQueueFull || m_queue.size() >= SECTION_QUEUE_LENGTH_LIMIT)
            {
              m_isQueueFull = true;
              LogDebug(L"section processor: failed to enqueue section, section queue is full");
              delete section;
              return;
            }

            m_queue.push_back(section);
            queueSize = m_queue.size();
            if (m_queue.size() > m_queueLengthMaximum)
            {
              m_queueLengthMaximum = m_queue.size();
            }
          }

          if (queueSize == 1)
          {
            m_thread.Wake();
          }
        }

        static bool __cdecl SectionProcessorThreadFunction(void* arg)
        {
          CSectionProcessor* processor = (CSectionProcessor*)arg;
          if (processor == NULL)
          {
            LogDebug(L"section processor: section processor thread processor not provided");
            return false;
          }

          processor->ProcessNextSection();
          return true;
        }

        void ProcessNextSection()
        {
          const CQueuedSection* section;
          {
            CEnterCriticalSection lock(m_queueSection);
            while (true)
            {
              if (m_queue.size() == 0)
              {
                m_isQueueFull = false;
                return;
              }
              section = m_queue[0];
              if (section != NULL)
              {
                break;
              }
              m_queue.erase(m_queue.begin());
            }
          }

          section->Delegate().OnNewSection(section->Pid(), section->TableId(), *(section->Section()));

          CEnterCriticalSection lock(m_queueSection);
          delete section;
          m_queue.erase(m_queue.begin());
          if (m_queue.size() == 0)
          {
            m_isQueueFull = false;
          }
          else
          {
            m_thread.Wake();
          }
        }

        void ClearSectionQueue()
        {
          CEnterCriticalSection lock(m_queueSection);
          for (vector<const CQueuedSection*>::iterator it = m_queue.begin(); it != m_queue.end(); it++)
          {
            const CQueuedSection* section = *it;
            if (section != NULL)
            {
              delete section;
              *it = NULL;
            }
          }
          m_queue.clear();
        }

        unsigned long MaximumQueueLength() const
        {
          return m_queueLengthMaximum;
        }

      private:
        CThread m_thread;
        vector<const CQueuedSection*> m_queue;
        CCriticalSection m_queueSection;
        unsigned long m_queueLengthMaximum;
        bool m_isQueueFull;
    };

    CSectionProcessor m_processors[PROCESSOR_COUNT];
    unsigned char m_nextProcessor;
    CCriticalSection m_section;
};