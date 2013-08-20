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

#include "F4MMedia.h"

CF4MMedia::CF4MMedia(void)
{
  this->bitrate = UINT_MAX;
  this->width = UINT_MAX;
  this->height = UINT_MAX;

  this->bootstrapInfoId = NULL;
  this->drmAdditionalHeaderId = NULL;
  this->dvrInfoId = NULL;
  this->groupSpecifier = NULL;
  this->metadata = NULL;
  this->multicastStreamName = NULL;
  this->url = NULL;
}

CF4MMedia::~CF4MMedia(void)
{
  FREE_MEM(this->bootstrapInfoId);
  FREE_MEM(this->drmAdditionalHeaderId);
  FREE_MEM(this->dvrInfoId);
  FREE_MEM(this->groupSpecifier);
  FREE_MEM(this->metadata);
  FREE_MEM(this->multicastStreamName);
  FREE_MEM(this->url);
}

unsigned int CF4MMedia::GetBitrate(void)
{
  return this->bitrate;
}

unsigned int CF4MMedia::GetWidth(void)
{
  return this->width;
}

unsigned int CF4MMedia::GetHeight(void)
{
  return this->height;
}

const wchar_t *CF4MMedia::GetDrmAdditionalHeaderId(void)
{
  return this->drmAdditionalHeaderId;
}

const wchar_t *CF4MMedia::GetBootstrapInfoId(void)
{
  return this->bootstrapInfoId;
}

const wchar_t *CF4MMedia::GetDvrInfoId(void)
{
  return this->dvrInfoId;
}

const wchar_t *CF4MMedia::GetGroupSpecifier(void)
{
  return this->groupSpecifier;
}

const wchar_t *CF4MMedia::GetMulticastStreamName(void)
{
  return this->multicastStreamName;
}

const wchar_t *CF4MMedia::GetMetadata(void)
{
  return this->metadata;
}

const wchar_t *CF4MMedia::GetUrl(void)
{
  return this->url;
}

  /* set methods */

 bool CF4MMedia::SetBitrate(unsigned int bitrate)
 {
   this->bitrate = bitrate;
   return true;
 }

bool CF4MMedia::SetWidth(unsigned int width)
{
  this->width = width;
  return true;
}

bool CF4MMedia::SetHeight(unsigned int height)
{
  this->height = height;
  return true;
}

bool CF4MMedia::SetDrmAdditionalHeaderId(const wchar_t *drmAdditionalHeaderId)
{
  SET_STRING_RETURN_WITH_NULL(this->drmAdditionalHeaderId, drmAdditionalHeaderId);
}

bool CF4MMedia::SetBootstrapInfoId(const wchar_t *bootstrapInfoId)
{
  SET_STRING_RETURN_WITH_NULL(this->bootstrapInfoId, bootstrapInfoId);
}

bool CF4MMedia::SetDvrInfoId(const wchar_t *dvrInfoId)
{
  SET_STRING_RETURN_WITH_NULL(this->dvrInfoId, dvrInfoId);
}

bool CF4MMedia::SetGroupSpecifier(const wchar_t *groupSpecifier)
{
  SET_STRING_RETURN_WITH_NULL(this->groupSpecifier, groupSpecifier);
}

bool CF4MMedia::SetMulticastStreamName(const wchar_t *multicastStreamName)
{
  SET_STRING_RETURN_WITH_NULL(this->multicastStreamName, multicastStreamName);
}

bool CF4MMedia::SetMetadata(const wchar_t *metadata)
{
  SET_STRING_RETURN_WITH_NULL(this->metadata, metadata);
}

bool CF4MMedia::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}