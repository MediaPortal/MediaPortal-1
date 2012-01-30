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

#pragma once 

#include "stdafx.h"
#include "Settings.h"
#include "queuedaudiosink.h"
#include "ITimeStretch.h"

#include "../SoundTouch/Include/SoundTouch.h"
#include "SoundTouchEx.h"
//#include "SyncClock.h"

typedef struct tagSpeakerPair {
  DWORD dwLeft, dwRight;
  __inline DWORD PairMask()  { return dwLeft | dwRight; };
} SpeakerPair;

// List of speakers that should be handled as a pair
static SpeakerPair PairedSpeakers[] = {
  {SPEAKER_FRONT_LEFT, SPEAKER_FRONT_RIGHT},
  {SPEAKER_BACK_LEFT, SPEAKER_BACK_RIGHT},
  {SPEAKER_FRONT_LEFT_OF_CENTER, SPEAKER_FRONT_RIGHT_OF_CENTER},
  {SPEAKER_FRONT_CENTER, SPEAKER_BACK_CENTER}, // not sure about this one
  {SPEAKER_SIDE_LEFT, SPEAKER_SIDE_RIGHT},
  {SPEAKER_TOP_FRONT_LEFT, SPEAKER_TOP_FRONT_RIGHT},
  {SPEAKER_TOP_BACK_LEFT, SPEAKER_TOP_BACK_RIGHT},
  {SPEAKER_TOP_FRONT_CENTER, SPEAKER_TOP_BACK_CENTER},  // not sure about this one
  {NULL, NULL} // end marker
};

static DWORD gdwDefaultChannelMask[] = {
  0, // no channels - invalid
  KSAUDIO_SPEAKER_MONO,
  KSAUDIO_SPEAKER_STEREO,
  KSAUDIO_SPEAKER_STEREO | KSAUDIO_SPEAKER_GROUND_FRONT_CENTER,
  KSAUDIO_SPEAKER_QUAD,
  0, // 5 channels?
  KSAUDIO_SPEAKER_5POINT1,
  0, // 7 channels?
  KSAUDIO_SPEAKER_7POINT1_SURROUND
};

static DWORD gdwAC3SpeakerOrder[] = {
  SPEAKER_FRONT_LEFT, 
  SPEAKER_FRONT_CENTER,
  SPEAKER_FRONT_RIGHT,
  SPEAKER_BACK_LEFT, 
  SPEAKER_BACK_RIGHT,
  SPEAKER_LOW_FREQUENCY,
};
#define cAC3SpeakerOrder  (sizeof(gdwAC3SpeakerOrder)/sizeof(DWORD))
#define SPEAKER_AC3_VALID_POSITIONS ( \
  SPEAKER_FRONT_LEFT | \
  SPEAKER_FRONT_CENTER | \
  SPEAKER_FRONT_RIGHT | \
  SPEAKER_BACK_LEFT | \
  SPEAKER_BACK_RIGHT | \
  SPEAKER_LOW_FREQUENCY )

#define OUT_BUFFER_SIZE   65536
#define OUT_BUFFER_COUNT  20

using namespace std;

class CTimeStretchFilter : public CQueuedAudioSink, public ITimeStretch
{
public:
  CTimeStretchFilter(AudioRendererSettings *pSettings);
  ~CTimeStretchFilter();

  // IAudioSink implementation
  HRESULT Init();
  HRESULT Cleanup();
  HRESULT NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth);
  HRESULT EndOfStream();

public: 

  /// Sets new rate control value. Normal rate = 1.0, smaller values
  /// represent slower rate, larger faster rates.
  void setRate(double newRate);

  /// Sets new tempo control value. Normal tempo = 1.0, smaller values
  /// represent slower tempo, larger faster tempo.
  void setTempo(double newTempo, double newAdjustment);

  /// Sets new rate control value as a difference in percents compared
  /// to the original rate (-50 .. +100 %)
  void setRateChange(double newRate);

  /// Sets new tempo control value as a difference in percents compared
  /// to the original tempo (-50 .. +100 %)
  void setTempoChange(double newTempo);

  /// Sets pitch change in octaves compared to the original pitch  
  /// (-1.00 .. +1.00)
  void setPitchOctaves(double newPitch);

  /// Sets pitch change in semi-tones compared to the original pitch
  /// (-12 .. +12)
  void setPitchSemiTones(int newPitch);
  void setPitchSemiTones(double newPitch);

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

  //HRESULT CheckFormat(WAVEFORMATEX *pwf);
  //HRESULT CheckFormat(WAVEFORMATEXTENSIBLE *pwfe);
  //HRESULT SetFormat(WAVEFORMATEX *pwf);
  HRESULT SetFormat(WAVEFORMATEXTENSIBLE *pwfe);
  HRESULT ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE *pwfe, const WAVEFORMATEX *pwf);

  bool putSamples(const short *inBuffer, long inSamples);
  uint receiveSamples(short **outBuffer, uint maxSamples);

  //bool ProcessSamples(const short *inBuffer, long inSamples, short *outBuffer, long *outSamples, long maxOutSamples);
  //bool processSample(IMediaSample *pMediaSample);

protected:

  // Initialization
  //HRESULT InitAllocator();
  HRESULT OnInitAllocatorProperties(ALLOCATOR_PROPERTIES *properties);

  // Processing
  virtual DWORD ThreadProc();


  bool putSamplesInternal(const short *inBuffer, long inSamples);
  uint receiveSamplesInternal(short *outBuffer, uint maxSamples);


  void setTempoInternal(double newTempo, double newAdjustment);

// Internal implementation
private:

  AudioRendererSettings* m_pSettings;

  vector<HANDLE> m_hSampleEvents;
  vector<DWORD>  m_dwSampleWaitObjects;


  static const uint SAMPLE_LEN = 0x40000;
  std::vector<CSoundTouchEx *> *m_Streams;
  WAVEFORMATEXTENSIBLE *m_pWaveFormat;
  double m_fCurrentTempo;
  double m_fCurrentAdjustment;
  double m_fNewTempo;
  double m_fNewAdjustment;

  CCritSec m_allocatorLock;
};
