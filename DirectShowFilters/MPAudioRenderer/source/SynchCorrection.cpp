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

#include "stdafx.h"

#include "SynchCorrection.h"
#include "AudioClockTracker.h"

#include "alloctracing.h"

extern void Log(const char *fmt, ...);

SynchCorrection::SynchCorrection(void) :
  m_dAudioDelay(0.0) // audio delay is not reset on seek / pause
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
  m_iBiasDir = 0;
  m_dAVmult = 1.0;
  m_ullTotalTime = 0;
  m_dlastAdjustment = 1.0;
  m_dBiasAdjustmentDelay=0;
  m_bQualityMode = false;
  m_iQualityDir = 0;
  m_bQualityCorrectionOn = false;
  m_bDriftCorrectionEnabled = true;
  m_bBiasCorrectionEnabled = true;
  m_bAdjustmentCorrectionEnabled = true;

  m_Bias.Reset();
  m_Adjustment.Reset();
  m_AVTracker.Reset();
}

void SynchCorrection::Reset(double dBias)
{
  Log("SynchCorrection::Reset");
  Reset();
  m_Bias.SetAdjuster(dBias);
}
double SynchCorrection::SuggestedAudioMultiplier(double sampleLength, double bias, double adjustment)
{
  return GetRequiredAdjustment(sampleLength,  m_dAVmult, bias, adjustment);
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

double SynchCorrection::GetAVMult() const
{
  return m_dAVmult;
}

// used for the adjustment - it also corrects bias
void SynchCorrection::SetAdjustment(double adjustment)
{
  if (adjustment != m_dlastAdjustment)
  {
	  m_dlastAdjustment=adjustment;
    if (m_iBiasDir != 0) // there has been an adjustment already
    {
      // if the direction is different we have overshot so half the correction
      if ((m_iBiasDir == DIRUP) && (adjustment < 1))
        m_dBiasCorrection /= 2;
      else if ((m_iBiasDir == DIRDOWN) && (adjustment > 1)) 
        m_dBiasCorrection /= 2;
    }

    if (adjustment > 1)
      m_iBiasDir = DIRUP;
    else if (adjustment < 1)
      m_iBiasDir = DIRDOWN;

    m_Bias.SetAdjuster(m_Bias.GetAdjustment() + m_dBiasCorrection * (double)m_iBiasDir);
  }
  m_Adjustment.SetAdjuster(adjustment);
}

double SynchCorrection::GetAdjustment()
{
  return m_Adjustment.GetAdjustment();
}

void SynchCorrection::SetBias(double bias)
{
  // handle intrastream bias change
  double currentDrift = TotalAudioDrift(m_dAVmult);
  Reset();
  m_Bias.SetAdjuster(bias);
  m_dBiasAdjustmentDelay=currentDrift;
}

double SynchCorrection::GetBias()
{
  return m_Bias.GetAdjustment();
}

void SynchCorrection::SetAudioDelay(INT64 delay)
{
  m_dAudioDelay = (double)delay;
}
INT64 SynchCorrection::GetAudioDelay() const
{
  return (INT64)m_dAudioDelay;
}

//EVR presenter requests a delay
void SynchCorrection::SetPresenterInducedAudioDelay(INT64 delay)
{
  m_dEVRAudioDelay = (double)delay;
}

INT64 SynchCorrection::GetPresenterInducedAudioDelay() const
{
  return (INT64)m_dEVRAudioDelay;
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
double SynchCorrection::GetRequiredAdjustment(double sampleTime, double AVMult, double bias, double adjustment)
{
  double ret = AVMult * bias * adjustment;
  double totalAudioDrift = TotalAudioDrift(AVMult) + m_dAudioDelay + m_dBiasAdjustmentDelay +m_dEVRAudioDelay;

  if (ret > 1.0 - QUALITY_BIAS_LIMIT &&  ret < 1.0 + QUALITY_BIAS_LIMIT) 
	m_bQualityMode = true;
  else
    m_bQualityMode = false;

  if (totalAudioDrift > ALLOWED_DRIFT)
  { // we've stretched too much shift down for a while
    ret = ret * (1.0 / CORRECTION_RATE);
  }
  if (totalAudioDrift < ALLOWED_DRIFT * -1.0)
  { // haven't streched enough
    ret = ret * CORRECTION_RATE;
  } 
  if (m_bQualityMode)
  {
    ret = 1.0; // 1 to 1 playback unless proved otherwise
    if (m_bQualityCorrectionOn) // we are correcting drift
    {
      if (((m_iQualityDir == DIRUP) && (totalAudioDrift > QUALITY_CORRECTION_LIMIT)) ||
         ((m_iQualityDir == DIRDOWN) && (totalAudioDrift < QUALITY_CORRECTION_LIMIT * -1.0)))
      {
        //we've corrected enough
        m_bQualityCorrectionOn=false;
        m_iQualityDir=0;
      }
      if (m_iQualityDir==DIRUP) //behind so stretch
      {
        ret = QUALITY_CORRECTION_MULTIPLIER;
      }
      else if (m_iQualityDir==DIRDOWN) // in front so slow
      {
        ret = 1.0 / QUALITY_CORRECTION_MULTIPLIER;
      }
    }
    else // not correcting now so check for breach
    {
      if (totalAudioDrift > QUALITY_DRIFT_LIMIT)
      {
        m_bQualityCorrectionOn = true;
        m_iQualityDir = DIRDOWN;
      }
      else if (totalAudioDrift < QUALITY_DRIFT_LIMIT * -1.0)
      {
        m_bQualityCorrectionOn = true;
        m_iQualityDir = DIRUP;
      }
    }
  }
  return ret;
}


