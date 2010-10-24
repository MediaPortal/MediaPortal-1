// Copyright (C) 2005-2010 Team MediaPortal
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

#include "SynchCorrection.h"
#include "AudioClockTracker.h"
#include <stddef.h>
#include <stdarg.h>
#include "alloctracing.h"

extern void Log(const char *fmt, ...);

SynchCorrection::SynchCorrection(void) 
{
  m_pDebugLine = new char[1023];
  Reset();
}

SynchCorrection::~SynchCorrection(void)
{
  delete[] m_pDebugLine;
}

void SynchCorrection::Reset()
{
  Log("SynchCorrection::Reset");
  m_dBiasCorrection = 0.0001;
  m_dBiasDir = 0;
  m_dAVmult = 1.0;
  m_ullTotalTime = 0;
  m_dlastAdjustment = 1.0;

  m_Bias.Reset();
  m_Adjustment.Reset();
  m_AVTracker.Reset();
}

double SynchCorrection::SuggestedAudioMultiplier(double sampleLength)
{
  return GetRequiredAdjustment(sampleLength,  m_dAVmult);
}

// call after resampling to indicate what was resampled
void SynchCorrection::AudioResampled(double sourceLength, double resampleLength,double bias, double adjustment, double driftFactor)
{
  m_AVTracker.ResampleComplete(sourceLength, resampleLength, bias, adjustment, driftFactor);
}


double SynchCorrection::GetCurrentDrift()
{
  return TotalAudioDrift(m_dAVmult);
}

char* SynchCorrection::DebugData()
{
  sprintf(m_pDebugLine,"Base Clock : %I64d adjAdjustments : %I64d biasAdjustments : %I64d Audio Processed : %I64d Audio Out : %I64d bias : %f adjustment : %f Drift Multiplier : %f Drift : %f",
    m_Bias.GetTotalBaseTime(),
    m_Adjustment.GetAdjustments(),
    m_Bias.GetAdjustments(),
    m_AVTracker.GetAudioProcessed(),
    m_AVTracker.GetAudioResampled(),
    m_Bias.GetAdjustment(),
    m_Adjustment.GetAdjustment(),
    m_dAVmult,
    TotalAudioDrift(m_dAVmult));

  return m_pDebugLine;
}

void SynchCorrection::SetAVMult(double mult)
{
  m_dAVmult = 1.0 / mult;
}

double SynchCorrection::GetAVMult()
{
  return m_dAVmult;
}

// used for the adjustment - it also corrects bias
void SynchCorrection::SetAdjustment(double adjustment)
{
  if (adjustment != m_dlastAdjustment)
  {
	  m_dlastAdjustment=adjustment;
    if (m_dBiasDir != 0) // there has been an adjustment already
    {
      // if the direction is different we have overshot so half the correction
      if ((m_dBiasDir == DIRUP) && (adjustment < 1))
        m_dBiasCorrection /= 2;
      else if ((m_dBiasDir == DIRDOWN) && (adjustment > 1)) 
        m_dBiasCorrection /= 2;
    }

    if (adjustment > 1)
      m_dBiasDir = DIRUP;
    else if (adjustment < 1)
      m_dBiasDir = DIRDOWN;

    m_Bias.SetAdjuster(m_Bias.GetAdjustment() + m_dBiasCorrection * (double)m_dBiasDir);
  }
  m_Adjustment.SetAdjuster(adjustment);
}

double SynchCorrection::GetAdjustment()
{
  return m_Adjustment.GetAdjustment();
}

void SynchCorrection::SetBias(double bias)
{
  m_Bias.SetAdjuster(bias);
}

double SynchCorrection::GetBias()
{
  return m_Bias.GetAdjustment();
}

// recalculation of the delta value for the reference clock
INT64 SynchCorrection::GetCorrectedTimeDelta(INT64 time)
{
  m_ullTotalTime += time;
  return time + m_Bias.Adjustment(time, m_Adjustment.GetAdjustment()) + m_Adjustment.Adjustment(time);
}

// get the current drift
double SynchCorrection::TotalAudioDrift(double AVMult)
{
  return m_AVTracker.GetCurrentDrift(AVMult);
}

// get the adjustment required to match the hardware clocks
double SynchCorrection::GetRequiredAdjustment(double sampleTime, double AVMult)
{
  double ret = AVMult;
  double totalAudioDrift = TotalAudioDrift(AVMult);
  if (totalAudioDrift > ALLOWED_DRIFT)
  { // we've stretched too much shift down for a while
    ret = AVMult - CORRECTION_RATE;
  }
  if (totalAudioDrift < ALLOWED_DRIFT * -1.0)
  { // haven't streched enough
    ret = AVMult + CORRECTION_RATE;
  } 
  return ret;
}
