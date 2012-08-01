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
#include "Settings.h"
#include "queuedaudiosink.h"
#include "ITimeStretch.h"

#include "../SoundTouch/Include/SoundTouch.h"
#include "SoundTouchEx.h"
#include "Globals.h"
#include "SyncClock.h"

using namespace std;

class CTimeStretchFilter : public CQueuedAudioSink, public ITimeStretch
{
public:
  CTimeStretchFilter(AudioRendererSettings *pSettings, CSyncClock* pClock);
  ~CTimeStretchFilter();

  // IAudioSink implementation
  HRESULT Init();
  HRESULT PutSample(IMediaSample* pSample);
  HRESULT NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder);
  HRESULT EndOfStream();

public: 

  /// Sets new rate control value. Normal rate = 1.0, smaller values
  /// represent slower rate, larger faster rates.
  void setRate(float newRate);

  /// Sets new tempo control value. Normal tempo = 1.0, smaller values
  /// represent slower tempo, larger faster tempo.
  void setTempo(float newTempo, float newAdjustment);

  /// Sets new rate control value as a difference in percents compared
  /// to the original rate (-50 .. +100 %)
  void setRateChange(float newRate);

  /// Sets new tempo control value as a difference in percents compared
  /// to the original tempo (-50 .. +100 %)
  void setTempoChange(float newTempo);

  /// Sets pitch change in octaves compared to the original pitch  
  /// (-1.00 .. +1.00)
  void setPitchOctaves(float newPitch);

  /// Sets pitch change in semi-tones compared to the original pitch
  /// (-12 .. +12)
  void setPitchSemiTones(int newPitch);
  void setPitchSemiTones(float newPitch);

  /// Sets sample rate.
  void setSampleRate(uint srate);

  /// Flushes the last samples from the processing pipeline to the output.
  /// Clears also the internal processing buffers.
  //
  /// Note: This function is meant for extracting the last samples of a sound
  /// stream. This function may introduce additional blank samples in the end
  /// of the sound stream, and thus it's not recommended to call this function
  /// in the middle of a sound stream.
  void flush();

  uint flushEx();

  /// Clears all the samples.
  void clear();

  /// Returns number of samples currently unprocessed.
  uint numUnprocessedSamples() const;

  /// Returns number of samples currently available.
  uint numSamples() const;

  /// Returns nonzero if there aren't any samples available for outputting.
  int isEmpty() const;

  // set the number of channels to process
  // internally enough SoundTouch processors will be 
  // created to process the requested number of channels
  // Any samples already in que will be lost!
  //void setChannels(int channels);

  // Changes a setting controlling the processing system behaviour. See the
  // 'SETTING_...' defines for available setting ID's.
  // 
  // \return 'TRUE' if the setting was succesfully changed
  BOOL setSetting(int settingId, int value);

  HRESULT SetFormat(const WAVEFORMATEXTENSIBLE *pwfe);
  HRESULT CheckSample(IMediaSample* pSample);

  bool putSamples(const short *inBuffer, long inSamples);
  uint receiveSamples(short **outBuffer, uint maxSamples);

protected:
  // Processing
  virtual DWORD ThreadProc();

  bool putSamplesInternal(const short *inBuffer, long inSamples);
  uint receiveSamplesInternal(short *outBuffer, uint maxSamples);

  void setTempoInternal(float newTempo, float newAdjustment);

  void CheckStreamContinuity(IMediaSample* pSample);
  void CreateOutput(UINT32 nInFrames, UINT32 nOutFrames, double dBias, double dAdjustment, double dAVMult, bool bFlushPartialSample);
  void OutputSample(bool bForce);

// Internal implementation
private:

  AudioRendererSettings* m_pSettings;

  vector<HANDLE> m_hSampleEvents;
  vector<DWORD>  m_dwSampleWaitObjects;

  AM_MEDIA_TYPE* m_pMediaType;
  REFERENCE_TIME m_rtInSampleTime;
  REFERENCE_TIME m_rtNextIncomingSampleTime;

  static const uint SAMPLE_LEN = 0x40000;
  std::vector<CSoundTouchEx *> *m_Streams;
  WAVEFORMATEXTENSIBLE *m_pWaveFormat;
  double m_fCurrentTempo;
  double m_fCurrentAdjustment;
  double m_fNewTempo;
  double m_fNewAdjustment;

  CCritSec m_allocatorLock;

  CSyncClock* m_pClock;
};
