// PCM test signals and media types for the audio filter tests.
#pragma once

#include <streams.h>
#include <ks.h>
#include <ksmedia.h>
#include <mmdeviceapi.h>
#include <cmath>
#include <cstdint>
#include <vector>

struct PcmFormat
{
  int rate = 48000;
  int channels = 2;
  int bits = 16;
  bool isFloat = false;

  int BlockAlign() const { return channels * bits / 8; }
  int BytesPerSecond() const { return rate * BlockAlign(); }

  static DWORD ChannelMask(int channels)
  {
    switch (channels)
    {
      case 1: return SPEAKER_FRONT_CENTER;
      case 2: return SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT;
      case 4: return SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT | SPEAKER_BACK_LEFT | SPEAKER_BACK_RIGHT;
      case 6: return SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT | SPEAKER_FRONT_CENTER | SPEAKER_LOW_FREQUENCY | SPEAKER_BACK_LEFT | SPEAKER_BACK_RIGHT;
      case 8: return SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT | SPEAKER_FRONT_CENTER | SPEAKER_LOW_FREQUENCY | SPEAKER_BACK_LEFT | SPEAKER_BACK_RIGHT | SPEAKER_SIDE_LEFT | SPEAKER_SIDE_RIGHT;
      default: return 0;
    }
  }

  // WAVEFORMATEXTENSIBLE, the format that every WASAPI based graph uses
  CMediaType MediaType() const
  {
    WAVEFORMATEXTENSIBLE w = {};
    w.Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
    w.Format.nChannels = static_cast<WORD>(channels);
    w.Format.nSamplesPerSec = rate;
    w.Format.wBitsPerSample = static_cast<WORD>(bits);
    w.Format.nBlockAlign = static_cast<WORD>(BlockAlign());
    w.Format.nAvgBytesPerSec = BytesPerSecond();
    w.Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);
    w.Samples.wValidBitsPerSample = static_cast<WORD>(bits);
    w.dwChannelMask = ChannelMask(channels);
    w.SubFormat = isFloat ? MEDIASUBTYPE_IEEE_FLOAT : MEDIASUBTYPE_PCM;   // same GUIDs as KSDATAFORMAT_SUBTYPE_*

    CMediaType mt;
    mt.SetType(&MEDIATYPE_Audio);
    mt.SetSubtype(isFloat ? &MEDIASUBTYPE_IEEE_FLOAT : &MEDIASUBTYPE_PCM);
    mt.SetFormatType(&FORMAT_WaveFormatEx);
    mt.SetTemporalCompression(FALSE);
    mt.SetSampleSize(BlockAlign());
    // The block is padded: MPAudioRenderer's CopyWaveFormatEx copies sizeof(WAVEFORMATEXTENSIBLE) + cbSize bytes (62 for
    // an extensible format of 40 bytes) from the format of the pin. See the disabled test about it in MpAudioRendererTests.cpp.
    BYTE block[sizeof(WAVEFORMATEXTENSIBLE) + 64] = {};
    memcpy(block, &w, sizeof(w));
    mt.SetFormat(block, sizeof(block));
    return mt;
  }

  // 'frames' frames of a sine wave in every channel (amplitude 0..1 of full scale); 0 amplitude is silence
  std::vector<uint8_t> Sine(double frequency, double amplitude, size_t frames, size_t firstFrame = 0) const
  {
    std::vector<uint8_t> out(frames * BlockAlign());
    const double pi = 3.14159265358979323846;
    for (size_t f = 0; f < frames; f++)
    {
      const double v = amplitude * std::sin(2.0 * pi * frequency * static_cast<double>(firstFrame + f) / rate);
      for (int c = 0; c < channels; c++)
      {
        uint8_t* p = &out[(f * channels + c) * (bits / 8)];
        if (isFloat)
        {
          const float x = static_cast<float>(v);
          memcpy(p, &x, 4);
        }
        else if (bits == 16)
        {
          const int16_t x = static_cast<int16_t>(std::lround(v * 32767.0));
          memcpy(p, &x, 2);
        }
        else if (bits == 24)
        {
          const int32_t x = static_cast<int32_t>(std::lround(v * 8388607.0));
          p[0] = static_cast<uint8_t>(x);
          p[1] = static_cast<uint8_t>(x >> 8);
          p[2] = static_cast<uint8_t>(x >> 16);
        }
        else if (bits == 32)
        {
          const int32_t x = static_cast<int32_t>(std::llround(v * 2147483647.0));
          memcpy(p, &x, 4);
        }
      }
    }
    return out;
  }
};

// true when Windows has a default render endpoint (the renderer needs one to connect)
inline bool HasDefaultAudioRenderDevice()
{
  IMMDeviceEnumerator* enumerator = nullptr;
  if (FAILED(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_ALL, __uuidof(IMMDeviceEnumerator), reinterpret_cast<void**>(&enumerator))))
    return false;
  IMMDevice* device = nullptr;
  const bool ok = SUCCEEDED(enumerator->GetDefaultAudioEndpoint(eRender, eConsole, &device)) && device;
  if (device)
    device->Release();
  enumerator->Release();
  return ok;
}
