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

#pragma once

#ifndef __MSHS_MANIFEST_TRACK_BOX_DEFINED
#define __MSHS_MANIFEST_TRACK_BOX_DEFINED

#include "MshsManifestCustomAttributeBoxCollection.h"
#include "Box.h"

#include <stdint.h>

#define MSHS_MANIFEST_TRACK_BOX_TYPE                                  L"mstr"

#define MSHS_MANIFEST_TRACK_BOX_FLAG_NONE                             BOX_FLAG_NONE

#define MSHS_MANIFEST_TRACK_BOX_FLAG_LAST                             (BOX_FLAG_LAST + 0)

#define MSHS_NAL_UNIT_LENGTH_DEFAULT                                  4

#define MSHS_AUDIO_TAG_LINEAR_PCM                                     0x00000001
#define MSHS_AUDIO_TAG_WMA_V7_V8_V9                                   0x00000161
#define MSHS_AUDIO_TAG_WMA_V10                                        0x00000161
#define MSHS_AUDIO_TAG_ISO_MPEG_1_LAYER_3                             0x00000055
#define MSHS_AUDIO_TAG_ISO_AAC                                        0x000000FF
#define MSHS_AUDIO_TAG_VENDOR_EXTENSIBLE                              0x0000FFFE

#define MSHS_FOURCC_H264                                              L"H264"
#define MSHS_FOURCC_WVC1                                              L"WVC1"
#define MSHS_FOURCC_AACL                                              L"AACL"
#define MSHS_FOURCC_WMAP                                              L"WMAP"

class CMshsManifestTrackBox : public CBox
{
public:
  // creats new instance of CMshsManifestTrackBox class
  CMshsManifestTrackBox(HRESULT *result);
  // desctructor
  virtual ~CMshsManifestTrackBox(void);

  /* get methods */

  // ordinal that identifies the Track and MUST be unique for each Track in the Stream
  // the Index SHOULD start at 0 and increment by 1 for each subsequent Track in the Stream
  uint32_t GetIndex(void);

  // average bandwidth consumed by the track, in bits-per-second (bps)
  // the value 0 MAY be used for Tracks whose Bit Rate is negligible relative to other Tracks in the Presentation
  uint32_t GetBitrate(void);

  // maximum width of a video Sample, in pixels
  uint32_t GetMaxWidth(void);

  // maximum height of a video Sample, in pixels
  uint32_t GetMaxHeight(void);

  // data that specifies parameters specific to the Media Format and common to all Samples in the Track,
  // represented as a string of hex-coded bytes
  // the format and semantic meaning of byte sequence varies with the value of the FourCC field as follows:
  // FourCC field equals "H264": The CodecPrivateData field contains a hex-coded string representation of the following byte sequence:
  //  %x00 %x00 %x00 %x01 SPSField %x00 %x00 %x00 %x01 PPSField
  //  SPSField contains the Sequence Parameter Set (SPS)
  //  PPSField contains the Slice Parameter Set (PPS)
  // FourCC field equals "WVC1": The CodecPrivateData field contains a hex-coded string representation of the VIDEOINFOHEADER structure
  // FourCC field equals "AACL": The CodecPrivateData field SHOULD be empty
  // FourCC field equals "WMAP": The CodecPrivateData field contains the WAVEFORMATEX structure, if the AudioTag field equals "65534" equals, and SHOULD be empty otherwise
  // FourCC is a vendor extension value: The format of the CodecPrivateData field is also vendor-extensible
  //  registration of the FourCC field value with MPEG4-RA, to can be used to avoid collision between extensions
  const wchar_t *GetCodecPrivateData(void);

  // sampling Rate of an audio Track
  uint32_t GetSamplingRate(void);

  // channel Count of an audio Track
  uint16_t GetChannels(void);

  // sample Size of an audio Track
  uint16_t GetBitsPerSample(void);

  // size of each audio Packet, in bytes
  uint32_t GetPacketSize(void);

  // numeric code that identifies which Media Format and variant of the Media Format is used for each Sample in an audio Track
  // the following range of values is reserved with the following semantic meanings:
  // "1": The sample Media Format is Linear 8 or 16 bit Pulse Code Modulation
  // "353": Microsoft Windows Media Audio v7, v8 and v9.x Standard (WMA Standard)
  // "353": Microsoft Windows Media Audio v9.x and v10 Professional (WMA Professional)
  // "85": ISO MPEG-1 Layer III (MP3)
  // "255": ISO Advanced Audio Coding (AAC)
  // "65534": Vendor-extensible format, if specified, the codec private data field SHOULD contain a hex-encoded version of
  //    the WAVE_FORMAT_EXTENSIBLE structure
  uint32_t GetAudioTag(void);

  // four-character code that identifies which Media Format is used for each Sample
  // the following range of values is reserved with the following semantic meanings:
  // "H264": Video Samples for this Track use Advanced Video Coding, as specified in [AVCFF]
  // "WVC1": Video Samples for this Track use VC-1, as specified in [VC-1]
  // "AACL": Audio Samples for this Track use AAC (Low Complexity), as specified in [AAC]
  // "WMAP": Audio Samples for this Track use WMA Professional
  // other : a vendor extension value containing a registered with MPEG4-RA
  const wchar_t *GetFourCC(void);

  // number of bytes that specify the length of each Network Abstraction Layer (NAL) unit
  // this field SHOULD be omitted unless the value of the FourCC field is "H264"
  // the default value is NAL_UNIT_LENGTH_DEFAULT
  uint16_t GetNalUnitLengthField(void);

  // gets custom attributes associated with track
  // @return : custom attributes associated with track
  CMshsManifestCustomAttributeBoxCollection *GetCustomAttributes(void);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  /* set methods */

  // ordinal that identifies the Track and MUST be unique for each Track in the Stream
  // the Index SHOULD start at 0 and increment by 1 for each subsequent Track in the Stream
  void SetIndex(uint32_t index);

  // average bandwidth consumed by the track, in bits-per-second (bps)
  // the value 0 MAY be used for Tracks whose Bit Rate is negligible relative to other Tracks in the Presentation
  void SetBitrate(uint32_t bitrate);

  // maximum width of a video Sample, in pixels
  void SetMaxWidth(uint32_t maxWidth);

  // maximum height of a video Sample, in pixels
  void SetMaxHeight(uint32_t maxHeight);

  // data that specifies parameters specific to the Media Format and common to all Samples in the Track,
  // represented as a string of hex-coded bytes
  // the format and semantic meaning of byte sequence varies with the value of the FourCC field as follows:
  // FourCC field equals "H264": The CodecPrivateData field contains a hex-coded string representation of the following byte sequence:
  //  %x00 %x00 %x00 %x01 SPSField %x00 %x00 %x00 %x01 PPSField
  //  SPSField contains the Sequence Parameter Set (SPS)
  //  PPSField contains the Slice Parameter Set (PPS)
  // FourCC field equals "WVC1": The CodecPrivateData field contains a hex-coded string representation of the VIDEOINFOHEADER structure
  // FourCC field equals "AACL": The CodecPrivateData field SHOULD be empty
  // FourCC field equals "WMAP": The CodecPrivateData field contains the WAVEFORMATEX structure, if the AudioTag field equals "65534" equals, and SHOULD be empty otherwise
  // FourCC is a vendor extension value: The format of the CodecPrivateData field is also vendor-extensible
  //  registration of the FourCC field value with MPEG4-RA, to can be used to avoid collision between extensions
  bool SetCodecPrivateData(const wchar_t *codecPrivateData);

  // sampling Rate of an audio Track
  void SetSamplingRate(uint32_t samplingRate);

  // channel Count of an audio Track
  void SetChannels(uint16_t channels);

  // sample Size of an audio Track
  void SetBitsPerSample(uint16_t bitsPerSample);

  // size of each audio Packet, in bytes
  void SetPacketSize(uint32_t packetSize);

  // numeric code that identifies which Media Format and variant of the Media Format is used for each Sample in an audio Track
  // the following range of values is reserved with the following semantic meanings:
  // "1": The sample Media Format is Linear 8 or 16 bit Pulse Code Modulation
  // "353": Microsoft Windows Media Audio v7, v8 and v9.x Standard (WMA Standard)
  // "353": Microsoft Windows Media Audio v9.x and v10 Professional (WMA Professional)
  // "85": ISO MPEG-1 Layer III (MP3)
  // "255": ISO Advanced Audio Coding (AAC)
  // "65534": Vendor-extensible format, if specified, the codec private data field SHOULD contain a hex-encoded version of
  //    the WAVE_FORMAT_EXTENSIBLE structure
  void SetAudioTag(uint32_t audioTag);

  // four-character code that identifies which Media Format is used for each Sample
  // the following range of values is reserved with the following semantic meanings:
  // "H264": Video Samples for this Track use Advanced Video Coding, as specified in [AVCFF]
  // "WVC1": Video Samples for this Track use VC-1, as specified in [VC-1]
  // "AACL": Audio Samples for this Track use AAC (Low Complexity), as specified in [AAC]
  // "WMAP": Audio Samples for this Track use WMA Professional
  // other : a vendor extension value containing a registered with MPEG4-RA
  bool SetFourCC(const wchar_t *fourCC);

  // number of bytes that specify the length of each Network Abstraction Layer (NAL) unit
  // this field SHOULD be omitted unless the value of the FourCC field is "H264"
  // the default value is NAL_UNIT_LENGTH_DEFAULT
  void SetNalUnitLengthField(uint16_t nalUnitLengthField);

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const uint8_t *buffer, uint32_t length);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

private:

  // ordinal that identifies the Track and MUST be unique for each Track in the Stream
  // the Index SHOULD start at 0 and increment by 1 for each subsequent Track in the Stream
  uint32_t index;

  // average bandwidth consumed by the track, in bits-per-second (bps)
  // the value 0 MAY be used for Tracks whose Bit Rate is negligible relative to other Tracks in the Presentation
  uint32_t bitrate;

  // maximum width of a video Sample, in pixels
  uint32_t maxWidth;

  // maximum height of a video Sample, in pixels
  uint32_t maxHeight;

  // data that specifies parameters specific to the Media Format and common to all Samples in the Track,
  // represented as a string of hex-coded bytes
  // the format and semantic meaning of byte sequence varies with the value of the FourCC field as follows:
  // FourCC field equals "H264": The CodecPrivateData field contains a hex-coded string representation of the following byte sequence:
  //  %x00 %x00 %x00 %x01 SPSField %x00 %x00 %x00 %x01 PPSField
  //  SPSField contains the Sequence Parameter Set (SPS)
  //  PPSField contains the Slice Parameter Set (PPS)
  // FourCC field equals "WVC1": The CodecPrivateData field contains a hex-coded string representation of the VIDEOINFOHEADER structure
  // FourCC field equals "AACL": The CodecPrivateData field SHOULD be empty
  // FourCC field equals "WMAP": The CodecPrivateData field contains the WAVEFORMATEX structure, if the AudioTag field equals "65534" equals, and SHOULD be empty otherwise
  // FourCC is a vendor extension value: The format of the CodecPrivateData field is also vendor-extensible
  //  registration of the FourCC field value with MPEG4-RA, to can be used to avoid collision between extensions
  wchar_t *codecPrivateData;

  // sampling Rate of an audio Track
  uint32_t samplingRate;

  // channel Count of an audio Track
  uint16_t channels;

  // sample Size of an audio Track
  uint16_t bitsPerSample;

  // size of each audio Packet, in bytes
  uint32_t packetSize;

  // numeric code that identifies which Media Format and variant of the Media Format is used for each Sample in an audio Track
  // the following range of values is reserved with the following semantic meanings:
  // "1": The sample Media Format is Linear 8 or 16 bit Pulse Code Modulation
  // "353": Microsoft Windows Media Audio v7, v8 and v9.x Standard (WMA Standard)
  // "353": Microsoft Windows Media Audio v9.x and v10 Professional (WMA Professional)
  // "85": ISO MPEG-1 Layer III (MP3)
  // "255": ISO Advanced Audio Coding (AAC)
  // "65534": Vendor-extensible format, if specified, the codec private data field SHOULD contain a hex-encoded version of
  //    the WAVE_FORMAT_EXTENSIBLE structure
  uint32_t audioTag;

  // four-character code that identifies which Media Format is used for each Sample
  // the following range of values is reserved with the following semantic meanings:
  // "H264": Video Samples for this Track use Advanced Video Coding, as specified in [AVCFF]
  // "WVC1": Video Samples for this Track use VC-1, as specified in [VC-1]
  // "AACL": Audio Samples for this Track use AAC (Low Complexity), as specified in [AAC]
  // "WMAP": Audio Samples for this Track use WMA Professional
  // other : a vendor extension value containing a registered with MPEG4-RA
  wchar_t *fourCC;

  // number of bytes that specify the length of each Network Abstraction Layer (NAL) unit
  // this field SHOULD be omitted unless the value of the FourCC field is "H264"
  // the default value is NAL_UNIT_LENGTH_DEFAULT
  uint16_t nalUnitLengthField;

  CMshsManifestCustomAttributeBoxCollection *customAttributes;

  /* methods */

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed (added to buffer)
  // @return : number of bytes stored into buffer, 0 if error
  virtual uint32_t GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif