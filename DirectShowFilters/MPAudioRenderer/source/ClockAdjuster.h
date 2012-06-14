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

#pragma once

#include "stdafx.h"

class ClockAdjuster
{
public:
  ClockAdjuster(void);
  ~ClockAdjuster(void);
  void Reset();
  // get / set pair for the multiplier
  void SetAdjuster(double val);
  double GetAdjustment() const;
  // returns the total adjustments made
  INT64 GetAdjustments() const;
  // returns the time over which the adjustments are made
  INT64 GetTotalBaseTime() const;
  // reference clock delta in relation to this adjuster
  virtual INT64 Adjustment(INT64 time);
  // gets the delta relative to another adjustment value
  // i.e. if sample is x ms adjustment is x*y ms 
  // if another adjustment is present then x*y*z
  INT64 ClockAdjuster::Adjustment(UINT64 time, double otherMultiplier);

protected:
  // current error
  double m_dError;
  // current adjustment rate
  double m_dMultVal;
  // adjustments made
  INT64 m_llAdjustmentsMade;
  // time adjustments are over
  INT64 m_ullTotalTime;
};
