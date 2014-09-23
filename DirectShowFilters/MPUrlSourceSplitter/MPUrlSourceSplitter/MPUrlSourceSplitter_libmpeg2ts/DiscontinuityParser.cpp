/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "DiscontinuityParser.h"
#include "TsPacket.h"
#include "TsPacketConstants.h"
#include "ErrorCodes.h"

CDiscontinuityParser::CDiscontinuityParser(HRESULT *result)
  : CParser(result)
{
  this->pidCounters = NULL;
  this->lastDiscontinuityPid = DISCONTINUITY_PID_NOT_SPECIFIED;
  this->lastDiscontinuityCounter = PID_COUNTER_NOT_SPECIFIED;
  this->lastExpectedCounter = PID_COUNTER_NOT_SPECIFIED;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->pidCounters = new CPidCounterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->pidCounters, *result, E_OUTOFMEMORY);
  }
}

CDiscontinuityParser::~CDiscontinuityParser(void)
{
  FREE_MEM_CLASS(this->pidCounters);
}

/* get methods */

unsigned int CDiscontinuityParser::GetLastDiscontinuityPid(void)
{
  return this->lastDiscontinuityPid;
}

uint8_t CDiscontinuityParser::GetLastDiscontinuityCounter(void)
{
  return this->lastDiscontinuityCounter;
}

uint8_t CDiscontinuityParser::GetLastExpectedCounter(void)
{
  return this->lastExpectedCounter;
}

/* set methods */

/* other methods */

HRESULT CDiscontinuityParser::Parse(unsigned char *buffer, unsigned int length)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_CONDITION_HRESULT(result, (length % TS_PACKET_SIZE) == 0, result, E_MPEG2TS_NOT_ALIGNED_BUFFER_SIZE);

  if (SUCCEEDED(result))
  {
    // analyze discontinuity only if we have aligned MPEG2 TS packets
    unsigned int processed = 0;
    bool discontinuityDetected = false;

    while (SUCCEEDED(result) && (processed < length) && (!discontinuityDetected))
    {
      CTsPacket *packet = new CTsPacket(&result);
      CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, packet->Parse(buffer + processed, length - processed), result, E_MPEG2TS_CANNOT_PARSE_PACKET);

      if (SUCCEEDED(result))
      {
        // check for right values
        // skip NULL packets (PID 0x1FFF)
        // PID counters are incremented only if payload is present (adaptation field control is 1 or 3)

        if ((packet->GetPID() != TS_PACKET_PID_NULL) && ((packet->GetAdaptationFieldControl() == 1) || (packet->GetAdaptationFieldControl() == 3)))
        {
          // check if PID counter is set
          CPidCounter *pidCounter = this->pidCounters->GetItem(packet->GetPID());

          if (pidCounter->IsSetCurrentCounter())
          {
            // PID counter for 'pid' is already set

            discontinuityDetected |= (pidCounter->GetExpectedCurrentCounter() != packet->GetContinuityCounter());

            if (discontinuityDetected)
            {
              this->lastDiscontinuityPid = packet->GetPID();
              this->lastDiscontinuityCounter = packet->GetContinuityCounter();
              this->lastExpectedCounter = pidCounter->GetExpectedCurrentCounter();
            }
          }

          // in all cases set PID counter
          pidCounter->SetPreviousCounter(pidCounter->GetCurrentCounter());
          pidCounter->SetCurrentCounter(packet->GetContinuityCounter());
        }
      }

      processed += TS_PACKET_SIZE;
      FREE_MEM_CLASS(packet);
    }

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = processed);
  }

  return result;
}

void CDiscontinuityParser::Clear(void)
{
  __super::Clear();

  this->pidCounters->Clear();
  this->lastDiscontinuityPid = DISCONTINUITY_PID_NOT_SPECIFIED;
  this->lastDiscontinuityCounter = PID_COUNTER_NOT_SPECIFIED;
  this->lastExpectedCounter = PID_COUNTER_NOT_SPECIFIED;
}

/* protected methods */
