// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "ClockAdjuster.h"

#include "alloctracing.h"

ClockAdjuster::ClockAdjuster(void)
{
  Reset();
}

ClockAdjuster::~ClockAdjuster()
{
}

void ClockAdjuster::Reset()
{
  m_dError = 0.0;
  m_dMultVal = 1.0;  
  m_llAdjustmentsMade = 0;
  m_ullTotalTime = 0;
}

void ClockAdjuster::SetAdjuster(double val)
{
  m_dMultVal = val;
}

double ClockAdjuster::GetAdjustment() const
{
  return m_dMultVal;
}

INT64 ClockAdjuster::GetAdjustments() const
{
  return m_llAdjustmentsMade;
}

INT64 ClockAdjuster::GetTotalBaseTime() const
{
  return m_ullTotalTime;
}

// reference clock delta in relation to this adjuster
INT64 ClockAdjuster::Adjustment(INT64 time)
{
    INT64 ret = 0;
    double fullAdjustment = ((double)time) * m_dMultVal + m_dError;
    ret = (INT64)fullAdjustment - time;
    m_llAdjustmentsMade += ret;
    m_ullTotalTime += time;
    m_dError = fullAdjustment-(double)time - (double)ret;
    return ret;
}
// reference clock delta in relation to this adjuster corrected for a second adjuster
INT64 ClockAdjuster::Adjustment(UINT64 time, double otherMultiplier)
{
  INT64 ret = 0;
  double otherMultAdjustment = ((double)time) * otherMultiplier -(double)time;
  double fullAdjustment = ((double)time) * otherMultiplier * m_dMultVal - otherMultAdjustment;
  fullAdjustment += m_dError;
  ret = (INT64)fullAdjustment - time;
  m_llAdjustmentsMade += ret;
  m_ullTotalTime += time;
  m_dError = fullAdjustment-(double)time - (double)ret;
  return ret;
}
