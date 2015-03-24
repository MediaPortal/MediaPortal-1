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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_DEFINED

#include "Logger.h"
#include "AfhsDecryptionPlugin.h"
#include "AfhsCurlInstance.h"
#include "AfhsSegmentFragmentCollection.h"
#include "AfhsDecryptionHoster.h"
#include "AkamaiFlashInstance.h"
#include "ParsedMediaDataBox.h"
#include "MediaDataBox.h"

#define AFHS_PROTOCOL_DECRYPTION_NAME                                                     L"AFHS_DECRYPTION_AKAMAI"

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_NONE                  AFHS_DECRYPTION_PLUGIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_FLASH_INSTANCE_READY  (1 << (AFHS_DECRYPTION_PLUGIN_FLAG_LAST + 0))
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_KEY_REQUEST_PENDING   (1 << (AFHS_DECRYPTION_PLUGIN_FLAG_LAST + 1))

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_LAST                  (AFHS_DECRYPTION_PLUGIN_FLAG_LAST + 2)

#define AKAMAI_GUID_LENGTH                                                                12
#define AKAMAI_GUID_URL_PART                                                              L"g="
#define AKAMAI_GUID_URL_PART_LENGTH                                                       2

class CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai : public CAfhsDecryptionPlugin
{
public:
  // constructor
  CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai(void);

  // CPlugin implementation

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  virtual GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  // CAfhsDecryptionPlugin implementation

  // gets decryption result about current stream
  // @param decryptionContext : AFHS decryption context
  // @return : one of DECRYPTION_RESULT values
  virtual HRESULT GetDecryptionResult(CAfhsDecryptionContext *decryptionContext);

  // gets decryption score if decryptor result is DECRYPTION_RESULT_KNOWN
  // @return : decryption score (decryptor with highest score is set as active decryptor)
  virtual unsigned int GetDecryptionScore(void);

  // clears current session
  virtual void ClearSession(void);

  // decrypts encrypted segment fragments
  // @param decryptionContext : AFHS decryption context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT DecryptSegmentFragments(CAfhsDecryptionContext *decryptionContext);

protected:
  // holds akamain flash instance to decrypt received data
  CAkamaiFlashInstance *akamaiFlashInstance;
  // holds akamai GUID
  wchar_t *akamaiGuid;
  // holds last decryption key url
  wchar_t *lastKeyUrl;
  // holds session ID (part of lastKeyUrl)
  wchar_t *sessionID;
  // holds last key
  uint8_t *lastKey;
  unsigned int lastKeyLength;

  // holds akamai swf file name
  wchar_t *akamaiSwfFileName;
  // holds last timestamp
  unsigned int lastTimestamp;

  /* methods */

  // gets resource from module
  // @param name : name of the resource
  // @param type : the resource type
  // @return : buffer with filled data from resource or NULL if error
  CLinearBuffer *GetResource(const wchar_t *name, const wchar_t *type);

  // gets random akamai swf file name
  // @param context : decryption context of AFHS protocol
  // @return : random akamai swf file name or NULL if error
  wchar_t *GetAkamaiSwfFileName(CAfhsDecryptionContext *context);

  // gets parsed media data box
  // @param parsedMediaDataBox : parsed media data box
  // @param context : decryption context of AFHS protocol
  // @param segmentFragment : segment and fragment to get media data box
  // @return : S_OK if successful, error code otherwise
  HRESULT ParseMediaDataBox(CParsedMediaDataBox *parsedMediaDataBox, CAfhsDecryptionContext *context, CAfhsSegmentFragment *segmentFragment);

  // gets url for key from url from akamai FLV packet
  // @param segmentFragmentUrl : url from segment and fragment
  // @param packetUrl : url from akamai FLV packet
  // @param akamaiGuid : akamai GUID
  // @return : url for key or NULL if error
  wchar_t *GetKeyUrlFromUrl(const wchar_t *segmentFragmentUrl, const wchar_t *packetUrl, const wchar_t *akamaiGuid);

  // gets media data box from specified segment and fragment
  // @param mediaDataBox : the media data box to be filled in (it will be created, caller is responsible for freeing memory)
  // @param segmentFragment : segment and fragment to get media data box
  // @return : S_OK if successful, error code otherwise
  HRESULT GetMediaDataBox(CMediaDataBox **mediaDataBox, CAfhsSegmentFragment *segmentFragment);

  // gets FLV packet from specified buffer
  // @param akamaiFlvPacket : the akamai FLV packet to be filled in (it will be created, caller is responsible for freeing memory)
  // @param buffer : buffer to get FLV packet
  // @return : S_OK if successful, error code otherwise
  HRESULT GetAkamaiFlvPacket(CAkamaiFlvPacket **akamaiFlvPacket, CLinearBuffer *buffer);

  // gets all boxes in segment and fragment
  // @param boxes : the box collection to be filled in (it will be created, caller is responsible for freeing memory)
  // @param segmentFragment : segment and fragment to get boxes
  // @return : S_OK if successful, error code otherwise
  HRESULT GetBoxes(CBoxCollection **boxes, CAfhsSegmentFragment *segmentFragment);
};

#endif
