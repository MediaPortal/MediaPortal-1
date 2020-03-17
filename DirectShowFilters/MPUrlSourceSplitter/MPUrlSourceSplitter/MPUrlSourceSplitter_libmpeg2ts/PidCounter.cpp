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

#include "PidCounter.h"

CPidCounter::CPidCounter(HRESULT *result)
{
  this->currentCounter = PID_COUNTER_NOT_SPECIFIED;
  this->previousCounter = PID_COUNTER_NOT_SPECIFIED;
}

CPidCounter::~CPidCounter(void)
{
}

/* get methods */

uint8_t CPidCounter::GetCurrentCounter(void)
{
  return this->currentCounter;
}

uint8_t CPidCounter::GetPreviousCounter(void)
{
  return this->previousCounter;
}

uint8_t CPidCounter::GetExpectedCurrentCounter(void)
{
  uint8_t result = PID_COUNTER_NOT_SPECIFIED;

  if (this->GetCurrentCounter() != PID_COUNTER_NOT_SPECIFIED)
  {
    result = (this->GetCurrentCounter() + 1) & 0x0F;
  }

  return result;
}

uint8_t CPidCounter::GetExpectedPreviousCounter(void)
{
  uint8_t result = PID_COUNTER_NOT_SPECIFIED;

  if (this->GetPreviousCounter() != PID_COUNTER_NOT_SPECIFIED)
  {
    result = (this->GetPreviousCounter() + 1) & 0x0F;
  }

  return result;
}

/* set methods */

void CPidCounter::SetCurrentCounter(uint8_t currentCounter)
{
  this->currentCounter = currentCounter;
}

void CPidCounter::SetPreviousCounter(uint8_t previousCounter)
{
  this->previousCounter = previousCounter;
}

/* other methods */

bool CPidCounter::IsSetCurrentCounter(void)
{
  return (this->currentCounter != PID_COUNTER_NOT_SPECIFIED);
}

bool CPidCounter::IsSetPreviousCounter(void)
{
  return (this->previousCounter != PID_COUNTER_NOT_SPECIFIED);
}

void CPidCounter::Clear(void)
{
  this->currentCounter = PID_COUNTER_NOT_SPECIFIED;
  this->previousCounter = PID_COUNTER_NOT_SPECIFIED;
}

/* protected methods */