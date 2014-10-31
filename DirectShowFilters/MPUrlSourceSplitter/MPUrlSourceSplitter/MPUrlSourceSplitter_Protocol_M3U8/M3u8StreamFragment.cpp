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

#include "M3u8StreamFragment.h"
#include "FastSearchItemCollection.h"

//CM3u8StreamFragment::CM3u8StreamFragment(HRESULT *result, const wchar_t *uri, unsigned int fragment)
//  : CStreamFragment(result)
//{
//  this->uri = NULL;
//  this->fragmentTimestamp = 0;
//  this->fragment = fragment;
//
//  if ((result != NULL) && (SUCCEEDED(*result)))
//  {
//    this->uri = Duplicate(uri);
//
//    CHECK_POINTER_HRESULT(*result, this->uri, *result, E_OUTOFMEMORY);
//  }
//}

CM3u8StreamFragment::CM3u8StreamFragment(HRESULT *result, const wchar_t *uri, unsigned int fragment, int64_t fragmentTimestamp, unsigned int duration)
  : CStreamFragment(result)
{
  this->uri = NULL;
  this->fragmentTimestamp = fragmentTimestamp;
  this->fragment = fragment;
  this->duration = duration;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->uri = Duplicate(uri);

    CHECK_POINTER_HRESULT(*result, this->uri, *result, E_OUTOFMEMORY);
  }
}

CM3u8StreamFragment::~CM3u8StreamFragment(void)
{
  FREE_MEM(this->uri);
}

/* get methods */

unsigned int CM3u8StreamFragment::GetFragment(void)
{
  return this->fragment;
}

int64_t CM3u8StreamFragment::GetFragmentTimestamp(void)
{
  return this->fragmentTimestamp;
}

const wchar_t *CM3u8StreamFragment::GetUri(void)
{
  return this->uri;
}

unsigned int CM3u8StreamFragment::GetDuration(void)
{
  return this->duration;
}

/* set methods */

void CM3u8StreamFragment::SetEncrypted(bool ecnrypted)
{
  this->flags &= ~M3U8_STREAM_FRAGMENT_FLAG_ENCRYPTED;
  this->flags |= ecnrypted ? M3U8_STREAM_FRAGMENT_FLAG_ENCRYPTED : M3U8_STREAM_FRAGMENT_FLAG_NONE;
}

void CM3u8StreamFragment::SetEndOfStream(bool endOfStream)
{
  this->flags &= ~M3U8_STREAM_FRAGMENT_FLAG_END_OF_STREAM;
  this->flags |= endOfStream ? M3U8_STREAM_FRAGMENT_FLAG_END_OF_STREAM : M3U8_STREAM_FRAGMENT_FLAG_NONE;
}

/* other methods */

bool CM3u8StreamFragment::IsEncrypted(void)
{
  return this->IsSetFlags(M3U8_STREAM_FRAGMENT_FLAG_ENCRYPTED);
}

bool CM3u8StreamFragment::IsEndOfStream(void)
{
  return this->IsSetFlags(M3U8_STREAM_FRAGMENT_FLAG_END_OF_STREAM);
}

/* protected methods */

CFastSearchItem *CM3u8StreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CM3u8StreamFragment *fragment = new CM3u8StreamFragment(&result, this->uri, this->fragment, this->fragmentTimestamp, this->duration);
  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
  return fragment;
}

bool CM3u8StreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CM3u8StreamFragment *fragment = dynamic_cast<CM3u8StreamFragment *>(item);
    result &= (fragment != NULL);

    /*if (result)
    {
    }*/
  }

  return result;
}