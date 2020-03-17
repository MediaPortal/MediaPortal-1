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

#include "RtspTransportRequestHeader.h"

CRtspTransportRequestHeader::CRtspTransportRequestHeader(HRESULT *result)
  : CRtspRequestHeader(result)
{
  this->destination = NULL;
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

CRtspTransportRequestHeader::~CRtspTransportRequestHeader(void)
{
  FREE_MEM(this->destination);
  FREE_MEM(this->lowerTransport);
  FREE_MEM(this->mode);
  FREE_MEM(this->profile);
  FREE_MEM(this->transportProtocol);
}

/* get methods */

const wchar_t *CRtspTransportRequestHeader::GetName(void)
{
  return RTSP_TRANSPORT_REQUEST_HEADER_NAME;
}

const wchar_t *CRtspTransportRequestHeader::GetValue(void)
{
  FREE_MEM(this->value);

  // create value from flags and specified values

  if ((this->GetTransportProtocol() != NULL) && (this->GetProfile() != NULL))
  {
    // create string in format "transport protocol/profile[/lower transport]"
    this->value = FormatString(L"%s/%s%s%s", this->GetTransportProtocol(), this->GetProfile(), (this->GetLowerTransport() != NULL) ? L"/" : L"", (this->GetLowerTransport() != NULL) ? this->GetLowerTransport() : L"");
  }

  if ((this->value != NULL) && (this->IsUnicast()))
  {
    wchar_t *temp = FormatString(L"%s;unicast", this->value);
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsMulticast()))
  {
    wchar_t *temp = FormatString(L"%s;multicast", this->value);
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->GetDestination() != NULL))
  {
    wchar_t *temp = FormatString(L"%s;destination=%s", this->value, this->GetDestination());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsInterleaved()))
  {
    wchar_t *temp = FormatString(L"%s;interleaved=%u-%u", this->value, this->GetMinInterleavedChannel(), this->GetMaxInterleavedChannel());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsAppend()))
  {
    wchar_t *temp = FormatString(L"%s;append", this->value);
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsTimeToLive()))
  {
    wchar_t *temp = FormatString(L"%s;ttl=%u", this->value, this->GetTimeToLive());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsLayers()))
  {
    wchar_t *temp = FormatString(L"%s;layers=%u", this->value, this->GetLayers());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsPort()))
  {
    wchar_t *temp = FormatString(L"%s;port=%u-%u", this->value, this->GetMinPort(), this->GetMaxPort());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsClientPort()))
  {
    wchar_t *temp = FormatString(L"%s;client_port=%u-%u", this->value, this->GetMinClientPort(), this->GetMaxClientPort());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsServerPort()))
  {
    wchar_t *temp = FormatString(L"%s;server_port=%u-%u", this->value, this->GetMinServerPort(), this->GetMaxServerPort());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->IsSynchronizationSourceIdentifier()))
  {
    wchar_t *temp = FormatString(L"%s;ssrc=%08X", this->value, this->GetSynchronizationSourceIdentifier());
    FREE_MEM(this->value);
    this->value = temp;
  }

  if ((this->value != NULL) && (this->GetMode() != NULL))
  {
    wchar_t *temp = FormatString(L"%s;mode=%s", this->value, this->GetMode());
    FREE_MEM(this->value);
    this->value = temp;
  }

  return this->value;
}

const wchar_t *CRtspTransportRequestHeader::GetTransportProtocol(void)
{
  return this->transportProtocol;
}

const wchar_t *CRtspTransportRequestHeader::GetProfile(void)
{
  return this->profile;
}

const wchar_t *CRtspTransportRequestHeader::GetLowerTransport(void)
{
  return this->lowerTransport;
}

const wchar_t *CRtspTransportRequestHeader::GetDestination(void)
{
  return this->destination;
}

unsigned int CRtspTransportRequestHeader::GetMinInterleavedChannel(void)
{
  return this->minInterleaved;
}

unsigned int CRtspTransportRequestHeader::GetMaxInterleavedChannel(void)
{
  return this->maxInterleaved;
}

unsigned int CRtspTransportRequestHeader::GetTimeToLive(void)
{
  return this->timeToLive;
}

unsigned int CRtspTransportRequestHeader::GetLayers(void)
{
  return this->layers;
}

unsigned int CRtspTransportRequestHeader::GetMinPort(void)
{
  return this->minPort;
}

unsigned int CRtspTransportRequestHeader::GetMaxPort(void)
{
  return this->maxPort;
}

unsigned int CRtspTransportRequestHeader::GetMinClientPort(void)
{
  return this->minClientPort;
}

unsigned int CRtspTransportRequestHeader::GetMaxClientPort(void)
{
  return this->maxClientPort;
}

unsigned int CRtspTransportRequestHeader::GetMinServerPort(void)
{
  return this->minServerPort;
}

unsigned int CRtspTransportRequestHeader::GetMaxServerPort(void)
{
  return this->maxServerPort;
}

const wchar_t *CRtspTransportRequestHeader::GetMode(void)
{
  return this->mode;
}

unsigned int CRtspTransportRequestHeader::GetSynchronizationSourceIdentifier(void)
{
  return this->synchronizationSourceIdentifier;
}

/* set methods */

bool CRtspTransportRequestHeader::SetName(const wchar_t *name)
{
  // we never set name
  return false;
}

bool CRtspTransportRequestHeader::SetValue(const wchar_t *value)
{
  // we never set value
  return false;
}

bool CRtspTransportRequestHeader::SetTransportProtocol(const wchar_t *transportProtocol)
{
  SET_STRING_RETURN_WITH_NULL(this->transportProtocol, transportProtocol);
}

bool CRtspTransportRequestHeader::SetProfile(const wchar_t *profile)
{
  SET_STRING_RETURN_WITH_NULL(this->profile, profile);
}

bool CRtspTransportRequestHeader::SetLowerTransport(const wchar_t *lowerTransport)
{
  SET_STRING_RETURN_WITH_NULL(this->lowerTransport, lowerTransport);
}

bool CRtspTransportRequestHeader::SetDestination(const wchar_t *destination)
{
  SET_STRING_RETURN_WITH_NULL(this->destination, destination);
}

void CRtspTransportRequestHeader::SetMinInterleavedChannel(unsigned int minInterleavedChannel)
{
  this->minInterleaved = minInterleavedChannel;
}

void CRtspTransportRequestHeader::SetMaxInterleavedChannel(unsigned int maxInterleavedChannel)
{
  this->maxInterleaved = maxInterleavedChannel;
}

void CRtspTransportRequestHeader::SetTimeToLive(unsigned int timeToLive)
{
  this->timeToLive = timeToLive;
}

void CRtspTransportRequestHeader::SetLayers(unsigned int layers)
{
  this->layers = layers;
}

void CRtspTransportRequestHeader::SetMinPort(unsigned int minPort)
{
  this->minPort = minPort;
}

void CRtspTransportRequestHeader::SetMaxPort(unsigned int maxPort)
{
  this->maxPort = maxPort;
}

void CRtspTransportRequestHeader::SetMinClientPort(unsigned int minClientPort)
{
  this->minClientPort = minClientPort;
}

void CRtspTransportRequestHeader::SetMaxClientPort(unsigned int maxClientPort)
{
  this->maxClientPort = maxClientPort;
}

void CRtspTransportRequestHeader::SetMinServerPort(unsigned int minServerPort)
{
  this->minServerPort = minServerPort;
}

void CRtspTransportRequestHeader::SetMaxServerPort(unsigned int maxServerPort)
{
  this->maxServerPort = maxServerPort;
}

bool CRtspTransportRequestHeader::SetMode(const wchar_t *mode)
{
  SET_STRING_RETURN_WITH_NULL(this->mode, mode);
}

void CRtspTransportRequestHeader::SetSynchronizationSourceIdentifier(unsigned int synchronizationSourceIdentifier)
{
  this->synchronizationSourceIdentifier = synchronizationSourceIdentifier;
}

/* other methods */

bool CRtspTransportRequestHeader::IsUnicast(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_UNICAST);
}

bool CRtspTransportRequestHeader::IsMulticast(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_MULTICAST);
}

bool CRtspTransportRequestHeader::IsInterleaved(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_INTERLEAVED);
}

bool CRtspTransportRequestHeader::IsTransportProtocolRTP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_TRANSPORT_PROTOCOL_RTP);
}

bool CRtspTransportRequestHeader::IsProfileAVP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_PROFILE_AVP);
}

bool CRtspTransportRequestHeader::IsLowerTransportTCP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_LOWER_TRANSPORT_TCP);
}

bool CRtspTransportRequestHeader::IsLowerTransportUDP(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_LOWER_TRANSPORT_UDP);
}

bool CRtspTransportRequestHeader::IsAppend(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_APPEND);
}

bool CRtspTransportRequestHeader::IsTimeToLive(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_TIME_TO_LIVE);
}

bool CRtspTransportRequestHeader::IsLayers(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_LAYERS);
}

bool CRtspTransportRequestHeader::IsPort(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_PORT);
}

bool CRtspTransportRequestHeader::IsClientPort(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_CLIENT_PORT);
}

bool CRtspTransportRequestHeader::IsServerPort(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_SERVER_PORT);
}

bool CRtspTransportRequestHeader::IsSynchronizationSourceIdentifier(void)
{
  return this->IsSetFlags(RTSP_TRANSPORT_REQUEST_HEADER_FLAG_SSRC);
}

/* protected methods */

bool CRtspTransportRequestHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspTransportRequestHeader *header = dynamic_cast<CRtspTransportRequestHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(header->destination, this->destination, result);
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

CHttpHeader *CRtspTransportRequestHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspTransportRequestHeader *header = new CRtspTransportRequestHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}