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

bool CDiscontinuityParser::IsDiscontinuity(void)
{
  return this->IsSetFlags(DISCONTINUITY_PARSER_FLAG_DISCONTINUITY);
}

HRESULT CDiscontinuityParser::Parse(CTsPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    this->flags &= ~DISCONTINUITY_PARSER_FLAG_DISCONTINUITY;

    // check for right values
    // skip null packets (pid 0x1fff)
    // pid counters are incremented only if payload is present (adaptation field control is ts_packet_adaptation_field_control_only_payload or ts_packet_adaptation_field_control_adaptation_field_with_payload)

    if ((packet->GetPID() != TS_PACKET_PID_NULL) && ((packet->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD) || (packet->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD)))
    {
      // check if PID counter is set
      CPidCounter *pidCounter = this->pidCounters->GetItem(packet->GetPID());

      if (pidCounter->IsSetCurrentCounter())
      {
        // PID counter for 'pid' is already set

        this->flags |= (pidCounter->GetExpectedCurrentCounter() != packet->GetContinuityCounter()) ? DISCONTINUITY_PARSER_FLAG_DISCONTINUITY : DISCONTINUITY_PARSER_FLAG_NONE;

        if (this->IsDiscontinuity())
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
