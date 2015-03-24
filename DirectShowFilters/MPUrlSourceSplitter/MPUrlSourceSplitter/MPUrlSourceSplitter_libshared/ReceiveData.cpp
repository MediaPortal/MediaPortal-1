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

#include "ReceiveData.h"

CReceiveData::CReceiveData(void)
{
  this->streams = new CStreamReceiveDataColletion();
  this->flags = RECEIVE_DATA_FLAG_NONE;
}

CReceiveData::~CReceiveData(void)
{
  FREE_MEM_CLASS(this->streams);
}

/* get methods */

CStreamReceiveDataColletion *CReceiveData::GetStreams(void)
{
  return this->streams;
}

/* set methods */

bool CReceiveData::SetStreamCount(unsigned int streamCount)
{
  bool result = true;

  if (this->streams->Count() != streamCount)
  {
    this->streams->Clear();

    for (unsigned int i = 0; (result && (i < streamCount)); i++)
    {
      CStreamReceiveData *stream = new CStreamReceiveData();
      result &= (stream != NULL);

      CHECK_CONDITION_EXECUTE(result, result = this->streams->Add(stream));
      CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(stream));
    }
  }

  this->flags |= result ? RECEIVE_DATA_FLAG_SET_STREAM_COUNT : RECEIVE_DATA_FLAG_NONE;
  return result;
}

void CReceiveData::SetLiveStream(bool liveStream)
{
  this->flags &= ~RECEIVE_DATA_FLAG_LIVE_STREAM;
  this->flags |= liveStream ? RECEIVE_DATA_FLAG_LIVE_STREAM : RECEIVE_DATA_FLAG_NONE;
}

/* other methods */

bool CReceiveData::IsSetStreamCount(void)
{
  return this->IsSetFlags(RECEIVE_DATA_FLAG_SET_STREAM_COUNT);
}

bool CReceiveData::IsLiveStream(void)
{
  return this->IsSetFlags(RECEIVE_DATA_FLAG_LIVE_STREAM);
}

bool CReceiveData::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

void CReceiveData::Clear(void)
{
  for (unsigned int i = 0; i < this->streams->Count(); i++)
  {
    this->streams->GetItem(i)->Clear();
  }
  this->flags = RECEIVE_DATA_FLAG_NONE;
}