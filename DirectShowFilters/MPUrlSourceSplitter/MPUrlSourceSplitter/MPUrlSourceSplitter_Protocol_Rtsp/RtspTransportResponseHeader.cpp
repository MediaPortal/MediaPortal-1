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

#include "StdAfx.h"

#include "RtspTransportResponseHeader.h"
#include "conversions.h"
#include "hex.h"
#include "BufferHelper.h"

CRtspTransportResponseHeader::CRtspTransportResponseHeader(HRESULT *result)
  : CRtspResponseHeader(result)
{
  this->destination = NULL;
  this->source = NULL;
  this->layers = 0;
  this->lowerTransport = NULL;
  this->maxClientPort = 0;
  this->maxInterleaved = 0;
  this->maxPort = 0;
  this->maxServerPort = 0;
  this->minClientPort = 0;
  this->minInterleaved = 0;
  this->minPort = 0;
  this->minServerPort = 0;
  this->mode = NULL;
  this->profile = NULL;
  this->synchronizationSourceIdentifier = 0;
  this->timeToLive = 0;
  this->transportProtocol = NULL;
}

CRtspTransportResponseHeader::~CRtspTransportResponseHeader(void)
{
  FREE_MEM(this->destination);
  FREE_MEM(this->source);
  FREE_MEM(this->lowerTransport);
  FREE_MEM(this->mode);
  FREE_MEM(this->profile);
  FREE_MEM(this->transportProtocol);
}

/* get methods */

const wchar_t *CRtspTransportResponseHeader::GetTransportProtocol(void)
{
  return this->transportProtocol;
}

const wchar_t *CRtspTransportResponseHeader::GetProfile(void)
{
  return this->profile;
}

const wchar_t *CRtspTransportResponseHeader::GetLowerTransport(void)
{
  return this->lowerTransport;
}

const wchar_t *CRtspTransportResponseHeader::GetDestination(void)
{
  return this->destination;
}

unsigned int CRtspTransportResponseHeader::GetMinInterleavedChannel(void)
{
  return this->minInterleaved;
}

unsigned int CRtspTransportResponseHeader::GetMaxInterleavedChannel(void)
{
  return this->maxInterleaved;
}

unsigned int CRtspTransportResponseHeader::GetTimeToLive(void)
{
  return this->timeToLive;
}

unsigned int CRtspTransportResponseHeader::GetLayers(void)
{
  return this->layers;
}

unsigned int CRtspTransportResponseHeader::GetMinPort(void)
{
  return this->minPort;
}

unsigned int CRtspTransportResponseHeader::GetMaxPort(void)
{
  return this->maxPort;
}

unsigned int CRtspTransportResponseHeader::GetMinClientPort(void)
{
  return this->minClientPort;
}

unsigned int CRtspTransportResponseHeader::GetMaxClientPort(void)
{
  return this->maxClientPort;
}

unsigned int CRtspTransportResponseHeader::GetMinServerPort(void)
{
  return this->minServerPort;
}

unsigned int CRtspTransportResponseHeader::GetMaxServerPort(void)
{
  return this->maxServerPort;
}

const wchar_t *CRtspTransportResponseHeader::GetMode(void)
{
  return this->mode;
}

unsigned int CRtspTransportResponseHeader::GetSynchronizationSourceIdentifier(void)
{
  return this->synchronizationSourceIdentifier;
}

const wchar_t *CRtspTransportResponseHeader::GetSource(void)
{
  return this->source;
}

/* set methods */

/* other methods */

bool CRtspTransportResponseHeader::IsUnicast(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_UNICAST);
}

bool CRtspTransportResponseHeader::IsMulticast(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_MULTICAST);
}

bool CRtspTransportResponseHeader::IsInterleaved(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_INTERLEAVED);
}

bool CRtspTransportResponseHeader::IsTransportProtocolRTP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_TRANSPORT_PROTOCOL_RTP);
}

bool CRtspTransportResponseHeader::IsProfileAVP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PROFILE_AVP);
}

bool CRtspTransportResponseHeader::IsLowerTransportTCP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LOWER_TRANSPORT_TCP);
}

bool CRtspTransportResponseHeader::IsLowerTransportUDP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LOWER_TRANSPORT_UDP);
}

bool CRtspTransportResponseHeader::IsAppend(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_APPEND);
}

bool CRtspTransportResponseHeader::IsTimeToLive(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_TIME_TO_LIVE);
}

bool CRtspTransportResponseHeader::IsLayers(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LAYERS);
}

bool CRtspTransportResponseHeader::IsPort(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PORT);
}

bool CRtspTransportResponseHeader::IsClientPort(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_CLIENT_PORT);
}

bool CRtspTransportResponseHeader::IsServerPort(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SERVER_PORT);
}

bool CRtspTransportResponseHeader::IsSynchronizationSourceIdentifier(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SSRC);
}

bool CRtspTransportResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_TRANSPORT_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      // find first separator
      // transport protocol, profile and optionally lower transport are before first separator

      unsigned int position = 0;
      unsigned int valueLength = this->GetValueLength();
      int index = IndexOf(this->value, valueLength, RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR_LENGTH);
      result &= (index > 0);

      if (result)
      {
        int index2 = IndexOf(this->value, index, RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH);
        result &= (index2 > 0);

        if (result)
        {
          // first part is transport protocol
          // second part is profile
          // third part (optional) is lower transport
          int index3 = IndexOf(this->value + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH, index - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH, RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH);

          this->transportProtocol = Substring(this->value, 0, index2);
          result &= (this->transportProtocol != NULL);

          if (index3 == (-1))
          {
            // without optional part
            this->profile = Substring(this->value, index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH, index - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH);
            result &= (this->profile != NULL);
          }
          else
          {
            // with optional part
            this->profile = Substring(this->value, index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH, index - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH - index3 - RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH);
            index3 += index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH;
            this->lowerTransport = Substring(this->value, index3 + RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH, index - index3 - RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH);

            result &= (this->profile != NULL);
            result &= (this->lowerTransport != NULL);
          }

          if (result)
          {
            // set flags
            if (wcscmp(this->transportProtocol, RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_RTP) == 0)
            {
              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_TRANSPORT_PROTOCOL_RTP;
            }

            if (wcscmp(this->profile, RTSP_TRANSPORT_RESPONSE_HEADER_PROFILE_AVP) == 0)
            {
              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PROFILE_AVP;
            }

            if (this->lowerTransport != NULL)
            {
              if (wcscmp(this->lowerTransport, RTSP_TRANSPORT_RESPONSE_HEADER_LOWER_TRANSPORT_TCP) == 0)
              {
                this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LOWER_TRANSPORT_TCP;
              }

              if (wcscmp(this->lowerTransport, RTSP_TRANSPORT_RESPONSE_HEADER_LOWER_TRANSPORT_UDP) == 0)
              {
                this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LOWER_TRANSPORT_UDP;
              }
            }
          }

          position = index + 1;
        }
      }

      while (result && (position < valueLength))
      {
        // try to find separator
        index = IndexOf(this->value + position, valueLength - position, RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR_LENGTH);
        unsigned int tempLength = (index == (-1)) ? (valueLength - position) : (index);

        if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_UNICAST, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_UNICAST_LENGTH) == 0)
        {
          this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_UNICAST;
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MULTICAST, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MULTICAST_LENGTH) == 0)
        {
          this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_MULTICAST;
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_DESTINATION, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_DESTINATION_LENGTH) == 0)
        {
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SOURCE, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SOURCE_LENGTH) == 0)
        {
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_INTERLEAVED, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_INTERLEAVED_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            // optionally is specified range
            int index3 = IndexOf(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH);

            if (index3 == (-1))
            {
              // without range, set min and max interleaved values to same value
              this->minInterleaved = this->maxInterleaved = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_INTERLEAVED;
            }
            else
            {
              // range specified
              this->minInterleaved = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);
              this->maxInterleaved = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH + index3 + RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_INTERLEAVED;
            }
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_APPEND, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_APPEND_LENGTH) == 0)
        {
          this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_APPEND;
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_TIME_TO_LIVE, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_TIME_TO_LIVE_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            this->timeToLive = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

            this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_TIME_TO_LIVE;
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_LAYERS, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_LAYERS_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            this->layers = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

            this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LAYERS;
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_PORT, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_PORT_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            // optionally is specified range
            int index3 = IndexOf(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH);

            if (index3 == (-1))
            {
              // without range, set min and max port values to same value
              this->minPort = this->maxPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PORT;
            }
            else
            {
              // range specified
              this->minPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);
              this->maxPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH + index3 + RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PORT;
            }
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_CLIENT_PORT, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_CLIENT_PORT_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            // optionally is specified range
            int index3 = IndexOf(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH);

            if (index3 == (-1))
            {
              // without range, set min and max client port values to same value
              this->minClientPort = this->maxClientPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_CLIENT_PORT;
            }
            else
            {
              // range specified
              this->minClientPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);
              this->maxClientPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH + index3 + RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_CLIENT_PORT;
            }
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SERVER_PORT, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SERVER_PORT_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            // optionally is specified range
            int index3 = IndexOf(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH);

            if (index3 == (-1))
            {
              // without range, set min and max server port values to same value
              this->minServerPort = this->maxServerPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SERVER_PORT;
            }
            else
            {
              // range specified
              this->minServerPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);
              this->maxServerPort = GetValueUnsignedInt(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH + index3 + RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH, 0);

              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SERVER_PORT;
            }
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SSRC, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SSRC_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            unsigned char *output = NULL;
            unsigned int outputLength = 0;

            result &= SUCCEEDED(hex_decode(this->value + position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, &output, &outputLength));
            result &= (outputLength == 4);

            if (result)
            {
              this->synchronizationSourceIdentifier = RBE32(output, 0);
              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SSRC;
            }

            FREE_MEM(output);
          }
        }
        else if (wcsncmp(this->value + position, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MODE, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MODE_LENGTH) == 0)
        {
          int index2 = IndexOf(this->value + position, tempLength, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (index2 > 0);

          if (result)
          {
            this->mode = Substring(this->value, position + index2 + RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
            result &= (this->mode != NULL);

            if (result)
            {
              this->flags |= RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_MODE;
            }
          }
        }
        else
        {
          // unknown parameter, ignore
        }

        position += tempLength + RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR_LENGTH;
      }
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_TRANSPORT_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}

/* protected methods */

bool CRtspTransportResponseHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspTransportResponseHeader *header = dynamic_cast<CRtspTransportResponseHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(header->destination, this->destination, result);
    SET_STRING_AND_RESULT_WITH_NULL(header->source, this->source, result);
    SET_STRING_AND_RESULT_WITH_NULL(header->lowerTransport, this->lowerTransport, result);
    SET_STRING_AND_RESULT_WITH_NULL(header->mode, this->mode, result);
    SET_STRING_AND_RESULT_WITH_NULL(header->profile, this->profile, result);
    SET_STRING_AND_RESULT_WITH_NULL(header->transportProtocol, this->transportProtocol, result);

    header->layers = this->layers;
    header->maxClientPort = this->maxClientPort;
    header->maxInterleaved = this->maxInterleaved;
    header->maxPort = this->maxPort;
    header->maxServerPort = this->maxServerPort;
    header->minClientPort = this->minClientPort;
    header->minInterleaved = this->minInterleaved;
    header->minPort = this->minPort;
    header->minServerPort = this->minServerPort;
    header->synchronizationSourceIdentifier = this->synchronizationSourceIdentifier;
    header->timeToLive = this->timeToLive;
  }

  return result;
}

CHttpHeader *CRtspTransportResponseHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspTransportResponseHeader *header = new CRtspTransportResponseHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}