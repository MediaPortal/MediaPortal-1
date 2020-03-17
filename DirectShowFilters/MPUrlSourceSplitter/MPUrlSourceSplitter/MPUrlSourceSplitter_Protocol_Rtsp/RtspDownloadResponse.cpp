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

#include "RtspDownloadResponse.h"
#include "ErrorCodes.h"

CRtspDownloadResponse::CRtspDownloadResponse(HRESULT *result)
  : CDownloadResponse(result)
{
  this->sessionDescriptionRaw = NULL;
  this->sessionDescription = NULL;
  this->contentBaseUrl = NULL;
  this->contentLocationUrl = NULL;
  this->tracks = NULL;
  this->rtspRequest = NULL;
  this->rtspResponse = NULL;
  this->sessionTimeout = UINT_MAX;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->tracks = new CRtspTrackCollection(result);
    CHECK_POINTER_HRESULT(*result, this->tracks, *result, E_OUTOFMEMORY);
  }
}

CRtspDownloadResponse::~CRtspDownloadResponse(void)
{
  FREE_MEM(this->sessionDescriptionRaw);
  FREE_MEM_CLASS(this->sessionDescription);
  FREE_MEM(this->contentBaseUrl);
  FREE_MEM(this->contentLocationUrl);
  FREE_MEM_CLASS(this->tracks);
  FREE_MEM_CLASS(this->rtspRequest);
  FREE_MEM_CLASS(this->rtspResponse);
}

/* get methods */

CSessionDescription *CRtspDownloadResponse::GetSessionDescription(void)
{
  return this->sessionDescription;
}

const wchar_t *CRtspDownloadResponse::GetRawSessionDescription(void)
{
  return this->sessionDescriptionRaw;
}

const wchar_t *CRtspDownloadResponse::GetContentBaseUrl(void)
{
  return this->contentBaseUrl;
}

const wchar_t *CRtspDownloadResponse::GetContentLocationUrl(void)
{
  return this->contentLocationUrl;
}

CRtspTrackCollection *CRtspDownloadResponse::GetRtspTracks(void)
{
  return this->tracks;
}

CRtspRequest *CRtspDownloadResponse::GetRtspRequest(void)
{
  return this->rtspRequest;
}

CRtspResponse *CRtspDownloadResponse::GetRtspResponse(void)
{
  return this->rtspResponse;
}

unsigned int CRtspDownloadResponse::GetSessionTimeout(void)
{
  return this->sessionTimeout;
}

/* set methods */

bool CRtspDownloadResponse::SetRawSessionDescription(const wchar_t *rawSessionDescription)
{
  SET_STRING_RETURN_WITH_NULL(this->sessionDescriptionRaw, rawSessionDescription);
}

bool CRtspDownloadResponse::SetContentBaseUrl(const wchar_t *contentBaseUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->contentBaseUrl, contentBaseUrl);
}

bool CRtspDownloadResponse::SetContentLocationUrl(const wchar_t *contentLocationUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->contentLocationUrl, contentLocationUrl);
}

void CRtspDownloadResponse::SetRtspRequest(CRtspRequest *request)
{
  FREE_MEM_CLASS(this->rtspRequest);
  this->rtspRequest = (request == NULL) ? NULL : request->Clone();
}

void CRtspDownloadResponse::SetRtspResponse(CRtspResponse *response)
{
  FREE_MEM_CLASS(this->rtspResponse);
  this->rtspResponse = (response == NULL) ? NULL : response->Clone();
}

void CRtspDownloadResponse::SetSessionTimeout(unsigned int sessionTimeout)
{
  this->sessionTimeout = sessionTimeout;
}

/* other methods */

void CRtspDownloadResponse::ClearRtspRequest(void)
{
  FREE_MEM_CLASS(this->rtspRequest);
}

void CRtspDownloadResponse::ClearRtspResponse(void)
{
  FREE_MEM_CLASS(this->rtspResponse);
}

void CRtspDownloadResponse::ClearRtspRequestAndResponse(void)
{
  FREE_MEM_CLASS(this->rtspRequest);
  FREE_MEM_CLASS(this->rtspResponse);
}

bool CRtspDownloadResponse::ParseRawSessionDescription(void)
{
  FREE_MEM_CLASS(this->sessionDescription);
  HRESULT result = (this->sessionDescriptionRaw != NULL) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    this->sessionDescription = new CSessionDescription(&result);
    CHECK_POINTER_HRESULT(result, this->sessionDescription, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, this->sessionDescription->Parse(this->sessionDescriptionRaw, wcslen(this->sessionDescriptionRaw)), result, E_RTSP_SESSION_DESCRIPTION_PARSE_ERROR);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->sessionDescription));
  return SUCCEEDED(result);
}

/* protected methods */

CDownloadResponse *CRtspDownloadResponse::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CRtspDownloadResponse *response = new CRtspDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CRtspDownloadResponse::CloneInternal(CDownloadResponse *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CRtspDownloadResponse *response = dynamic_cast<CRtspDownloadResponse *>(clone);

    SET_STRING_RESULT_WITH_NULL(response->sessionDescriptionRaw, this->sessionDescriptionRaw, result);
    SET_STRING_RESULT_WITH_NULL(response->contentBaseUrl, this->contentBaseUrl, result);
    SET_STRING_RESULT_WITH_NULL(response->contentLocationUrl, this->contentLocationUrl, result);
    response->sessionTimeout = this->sessionTimeout;

    if (this->sessionDescription != NULL)
    {
      result &= response->ParseRawSessionDescription();
    }

    result &= response->tracks->Append(this->tracks);
  }

  return result;
}