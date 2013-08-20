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

#include "SegmentFragment.h"

CSegmentFragment::CSegmentFragment(unsigned int segment, unsigned int fragment, uint64_t fragmentTimestamp)
{
  this->segment = segment;
  this->fragment = fragment;
  this->fragmentTimestamp = fragmentTimestamp;
  this->downloaded = false;
  this->processed = false;
  this->storeFilePosition = -1;
  this->length = 0;

  this->httpDownloadRequest = NULL;
  this->httpDownloadResponse = NULL;
}

CSegmentFragment::~CSegmentFragment(void)
{
  FREE_MEM_CLASS(this->httpDownloadRequest);
  FREE_MEM_CLASS(this->httpDownloadResponse);
}

/* get methods */

unsigned int CSegmentFragment::GetSegment(void)
{
  return this->segment;
}

unsigned int CSegmentFragment::GetFragment(void)
{
  return this->fragment;
}

uint64_t CSegmentFragment::GetFragmentTimestamp(void)
{
  return this->fragmentTimestamp;
}

int64_t CSegmentFragment::GetStoreFilePosition(void)
{
  return this->storeFilePosition;
}

unsigned int CSegmentFragment::GetLength(void)
{
  return (this->length == 0) ? this->httpDownloadResponse->GetReceivedData()->GetBufferOccupiedSpace() : this->length;
}

CHttpDownloadRequest *CSegmentFragment::GetHttpDownloadRequest(void)
{
  return this->httpDownloadRequest;
}

CHttpDownloadResponse *CSegmentFragment::GetHttpDownloadResponse(void)
{
  return this->httpDownloadResponse;
}

/* set methods */

void CSegmentFragment::SetDownloaded(bool downloaded)
{
  this->downloaded = downloaded;
}

void CSegmentFragment::SetProcessed(bool processed)
{
  this->processed = processed;
}

void CSegmentFragment::SetStoredToFile(int64_t position)
{
  this->storeFilePosition = position;
  if (this->storeFilePosition != (-1))
  {
    this->length = this->httpDownloadResponse->GetReceivedData()->GetBufferOccupiedSpace();
    this->httpDownloadResponse->GetReceivedData()->DeleteBuffer();
  }
}

bool CSegmentFragment::SetHttpDownloadRequest(CHttpDownloadRequest *downloadRequest)
{
  CHttpDownloadRequest *oldRequest = this->httpDownloadRequest;
  this->httpDownloadRequest = dynamic_cast<CHttpDownloadRequest *>(downloadRequest->Clone());
  bool result = (this->httpDownloadRequest != NULL);

  if (result)
  {
    FREE_MEM_CLASS(oldRequest);
  }
  else
  {
    this->httpDownloadRequest = oldRequest;
  }

  return result;
}

bool CSegmentFragment::SetHttpDownloadResponse(CHttpDownloadResponse *downloadResponse)
{
  CHttpDownloadResponse *oldResponse = this->httpDownloadResponse;
  this->httpDownloadResponse = dynamic_cast<CHttpDownloadResponse *>(downloadResponse->Clone());
  bool result = (this->httpDownloadResponse != NULL);

  if (result)
  {
    FREE_MEM_CLASS(oldResponse);
  }
  else
  {
    this->httpDownloadResponse = oldResponse;
  }

  return result;
}

/* other methods */

bool CSegmentFragment::IsStoredToFile(void)
{
  return (this->storeFilePosition != (-1));
}

bool CSegmentFragment::IsDownloaded(void)
{
  return this->downloaded;
}

bool CSegmentFragment::IsProcessed(void)
{
  return this->processed;
}

CSegmentFragment *CSegmentFragment::Clone(void)
{
  CSegmentFragment *result = new CSegmentFragment(this->segment, this->fragment, this->fragmentTimestamp);
  if (result != NULL)
  {
    FREE_MEM_CLASS(result->httpDownloadRequest);
    FREE_MEM_CLASS(result->httpDownloadResponse);
    result->downloaded = this->downloaded;
    result->httpDownloadRequest = dynamic_cast<CHttpDownloadRequest *>(this->httpDownloadRequest->Clone());
    result->httpDownloadResponse = dynamic_cast<CHttpDownloadResponse *>(this->httpDownloadResponse->Clone());
    result->length = this->length;
    result->processed = this->processed;
    result->storeFilePosition = this->storeFilePosition;
  }
  return result;
}

bool CSegmentFragment::CreateHttpDownloadRequest(void)
{
  FREE_MEM_CLASS(this->httpDownloadRequest);
  this->httpDownloadRequest = new CHttpDownloadRequest();

  return (this->httpDownloadRequest != NULL);
}

bool CSegmentFragment::CreateHttpDownloadResponse(void)
{
  FREE_MEM_CLASS(this->httpDownloadResponse);
  this->httpDownloadResponse = new CHttpDownloadResponse();

  return (this->httpDownloadResponse != NULL);
}

void CSegmentFragment::FreeHttpDownloadRequest(void)
{
  FREE_MEM_CLASS(this->httpDownloadRequest);
}

void CSegmentFragment::FreeHttpDownloadResponse(void)
{
  FREE_MEM_CLASS(this->httpDownloadResponse);
}