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

SynchCorrection::SynchCorrection(void) :
  m_dBiasCorrection(0.0001),
  m_dBiasDir(0),
  m_dAVmult(1.0),
  m_ullTotalTime(0),
  m_ullClockError(0),
  m_dlastAdjustment(1.0)
{
  debugLine = new char[1023];
}

SynchCorrection::~SynchCorrection(void)
{
  delete debugLine;
}

double SynchCorrection::SuggestedAudioMultiplier(INT64 sampleLength)
{
  return GetRequiredAdjustment(sampleLength, m_Bias.getAdjustment(), m_dAVmult, 
    m_Bias.getAdjustments(), m_Adjustment.getAdjustments(), AVTracker.getAudioProcessed(), 
    AVTracker.getAudioResampled(), m_Bias.GetTotalBaseTime());
}

// call after resampling to indicate what was resampled
void SynchCorrection::AudioResampled(double sourceLength, double resampleLength, double driftFactor)
{
  AVTracker.ResampleComplete(sourceLength, resampleLength, driftFactor);
  if (AVTracker.getAudioProcessed() != 0 && m_ullClockError == 0)
  {
    m_ullClockError = m_Bias.GetTotalBaseTime() - m_Bias.getAdjustments() - m_Adjustment.getAdjustments();
  }
}

INT64 SynchCorrection::getAudioTime()
{
  return AVTracker.getAudioProcessed();
}

INT64 SynchCorrection::getResampledAudioTime()
{
  return AVTracker.getAudioResampled();
}
double SynchCorrection::getCurrentDrift()
{
  return TotalAudioDrift(m_dAVmult, m_Bias.getAdjustment(), 
    m_Bias.getAdjustments(), m_Adjustment.getAdjustments(), AVTracker.getAudioProcessed(), 
    AVTracker.getAudioResampled(),m_Bias.GetTotalBaseTime());
}

char* SynchCorrection::DebugData()
{
  sprintf(debugLine,"Base Clock : %I64d clock error : %I64d adjAdjustments : %I64d biasAdjustments : %I64d Audio Processed : %I64d Audio Out : %I64d bias : %f Drift Multiplier : %f Drift : %f",
    m_Bias.GetTotalBaseTime(),
    m_ullClockError,
    m_Adjustment.getAdjustments(),
    m_Bias.getAdjustments(),
    AVTracker.getAudioProcessed(),
    AVTracker.getAudioResampled(),
    m_Bias.getAdjustment(),
    m_dAVmult,
    TotalAudioDrift(m_dAVmult,m_Bias.getAdjustment(), m_Bias.getAdjustments(),m_Adjustment.getAdjustments(), AVTracker.getAudioProcessed(),AVTracker.getAudioResampled(),m_Bias.GetTotalBaseTime()));

  return debugLine;
}

void SynchCorrection::setAVMult(double mult)
{
  m_dAVmult = 1.0 / mult;
}

double SynchCorrection::getAVMult()
{
  return m_dAVmult;
}

// used for the adjustment - it also corrects bias
void SynchCorrection::setAdjustment(double adjustment)
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

    m_Bias.SetAdjuster(m_Bias.getAdjustment() + m_dBiasCorrection * (double)m_dBiasDir);
  }
  m_Adjustment.SetAdjuster(adjustment);
}

void SynchCorrection::setBias(double bias)
{
  m_Bias.SetAdjuster(bias);
}

// recalculation of the delta value for the reference clock
INT64 SynchCorrection::GetCorrectedTimeDelta(INT64 time)
{
  m_ullTotalTime += time;
  return time + m_Bias.Adjustment(time, m_Adjustment.getAdjustment()) + m_Adjustment.Adjustment(time);
}

double SynchCorrection::GetAdjustment()
{
  return m_Adjustment.getAdjustment();
}

double SynchCorrection::GetBias()
{
  return m_Bias.getAdjustment();
}

double SynchCorrection::TotalAudioDrift(double AVMult,double biasMultiplier, INT64 biasAdjustment, INT64 adjustmentAdjustment, INT64 totalAudioProcessed, INT64 totalAudioAfterSampling, INT64 totalBaseTime)
{
  //We expect that the expected 
//  UINT64 referenceTime = totalBaseTime+biasAdjustment+adjustmentAdjustment;
  //(referenceTime-m_ullClockError)*AVMult = stream time on the reference clock adjusted for drift (this should be already output)
  // (double)(totalAudioProcessed-totalBaseTime+m_ullClockError) = remaining audio in buffer
  //  (double)(totalAudioProcessed-totalBaseTime+m_ullClockError) * biasMultiplier * AVMult = what the resampling should be
  // m_ullClockError needs to be within a second or two or the stream start (ideally should be after)
//  double expectedProcessedAudio = (double) (referenceTime-m_ullClockError)*AVMult + (double)(totalAudioProcessed-totalBaseTime+m_ullClockError) * biasMultiplier * AVMult;

//  return totalAudioAfterSampling - expectedProcessedAudio;
	return AVTracker.getCurrentDrift(AVMult);
}

double SynchCorrection::GetRequiredAdjustment(long sampleTime, double biasMultiplier, double AVMult, INT64 biasAdjustment, INT64 adjustmentAdjustment, INT64 totalAudioProcessed, INT64 totalAudioAfterSampling, INT64 totalBaseTime)
{
  double ret = AVMult;
  double allowableDrift = 10000.0; // 1 ms
  double adjustmentFactor = 0.005; // 0.5%
  double totalAudioDrift = TotalAudioDrift(AVMult,biasMultiplier,biasAdjustment,adjustmentAdjustment,totalAudioProcessed,totalAudioAfterSampling,totalBaseTime);
  if (totalAudioDrift > allowableDrift)
  { // we've stretched too much shift down for a while
    ret = AVMult - adjustmentFactor;
  }
  if (totalAudioDrift < allowableDrift * -1.0)
  { // haven't streched enough
    ret = AVMult + adjustmentFactor;
  } 
  return ret;
}
