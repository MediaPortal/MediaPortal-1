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

#ifndef __MP_URL_SOURCE_SPLITTER_AFHS_DECRYPTION_AKAMAI_DEFINED
#define __MP_URL_SOURCE_SPLITTER_AFHS_DECRYPTION_AKAMAI_DEFINED

#include "IAfhsDecryptionPlugin.h"
#include "AkamaiFlashInstance.h"
#include "AkamaiFlvPacket.h"
#include "HttpCurlInstance.h"
#include "BoxCollection.h"
#include "ParsedMediaDataBox.h"

#define DECRYPTION_NAME                                                       L"AFHS_AKAMAI"

#define AKAMAI_GUID_LENGTH                                                    12
#define AKAMAI_GUID_URL_PART                                                  L"g="
#define AKAMAI_GUID_URL_PART_LENGTH                                           2

class CMPUrlSourceSplitter_Afhs_Decryption_Akamai : public IAfhsDecryptionPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Afhs_Decryption_Akamai class
  CMPUrlSourceSplitter_Afhs_Decryption_Akamai(CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Afhs_Decryption_Akamai(void);

  // IAfhsDecryptionPlugin interface

  // check if decryption plugin supports decrypting segments and fragments
  // @param context : decryption context of AFHS protocol
  // @result : one of DecryptionResult values
  DecryptionResult Supported(CAfhsDecryptionContext *context);

  // IAfhsSimpleDecryptionPlugin interface

  // clears decryption plugin session
  // @return : S_OK if successfull
  HRESULT ClearSession(void);

  // process segments and fragments
  // @param context : decryption context of AFHS protocol
  // @result : S_OK if successful, error code otherwise
  HRESULT ProcessSegmentsAndFragments(CAfhsDecryptionContext *context);

  // IPlugin interface

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull
  HRESULT Initialize(PluginConfiguration *configuration);

protected:
  CLogger *logger;

  // holds various parameters supplied by caller
  CParameterCollection *configurationParameters;

  // specifies if received data were analysed for akamai pattern
  bool receivedDataAnalysed;
  // specifies if received data can be decrypted by akamai decryptor
  bool receivedDataCanBeDecrypted;
  // specifies if key was requested and still don't received
  bool keyRequestPending;

  // holds flash instance initialize result
  // default E_NOT_VALID_STATE (not initialized)
  HRESULT initializeAkamaiFlashInstanceResult;

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

  // holds akamai swf file
  wchar_t *akamaiSwfFile;
  // holds last timestamp
  unsigned int lastTimestamp;

  /* methods */

  // gets all boxes in segment and fragment
  // @param segmentFragment : segment and fragment to get boxes
  // @return : boxes collection or NULL if error
  CBoxCollection *GetBoxes(CSegmentFragment *segmentFragment);

  // gets media data box from specified segment and fragment
  // @param segmentFragment : segment and fragment to get media data box
  // @result : buffer with media data box data or NULL if error
  CLinearBuffer *GetMediaDataBox(CSegmentFragment *segmentFragment);

  // gets FLV packet from specified buffer
  // @param buffer : buffer to get FLV packet
  // @return : FLV packet or NULL if error
  CAkamaiFlvPacket *GetAkamaiFlvPacket(CLinearBuffer *buffer);

  // gets akamai GUID (12 random characters from ASCII(65) to ASCII(89))
  // @return : akamai GUID
  wchar_t *GetAkamaiGuid(void);

  // gets url for key from url from akamai FLV packet
  // @param segmentFragmentUrl : url from segment and fragment
  // @param packetUrl : url from akamai FLV packet
  // @param akamaiGuid : akamai GUID
  // @return : url for key or NULL if error
  wchar_t *GetKeyUrlFromUrl(const wchar_t *segmentFragmentUrl, const wchar_t *packetUrl, const wchar_t *akamaiGuid);

  // gets resource from module
  // @param name : name of the resource
  // @param type : the resource type
  // @return : buffer with filled data from resource or NULL if error
  CLinearBuffer *GetResource(const wchar_t *name, const wchar_t *type);

  // gets random akamai swf file name
  // @return : random akamai swf file name or NULL if error
  wchar_t *CMPUrlSourceSplitter_Afhs_Decryption_Akamai::GetAkamaiSwfFile(void);

  // gets parsed media data box
  // @param context : decryption context of AFHS protocol
  // @param segmentFragment : segment and fragment to get media data box
  // @return : parsed media data box or NULL if error
  CParsedMediaDataBox *ParseMediaDataBox(CAfhsDecryptionContext *context, CSegmentFragment *segmentFragment);

  // gets decryption key from segment and fragment
  // @param segmentFragment : segment and fragment to get decryption key
  // @param key : reference to key variable
  // @param keyLength : reference to key length variable
  void GetDecryptionKeyFromSegmentFragment(CSegmentFragment *segmentFragment, uint8_t **key, unsigned int *keyLength);
};

#endif