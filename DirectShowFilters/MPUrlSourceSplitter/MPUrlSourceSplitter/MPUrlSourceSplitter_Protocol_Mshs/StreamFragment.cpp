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

//#include "StreamFragment.h"
//
//CStreamFragment::CStreamFragment(const wchar_t *url, uint64_t fragmentDuration, uint64_t fragmentTime, unsigned int fragmentType)
//{
//  this->url = Duplicate(url);
//  this->fragmentDuration = fragmentDuration;
//  this->fragmentTime = fragmentTime;
//  this->downloaded = false;
//  this->fragmentType = fragmentType;
//  this->storeFilePosition = -1;
//
//  this->buffer = new CLinearBuffer();
//  this->length = 0;
//}
//
//CStreamFragment::~CStreamFragment(void)
//{
//  FREE_MEM(this->url);
//  FREE_MEM_CLASS(this->buffer);
//}
//
///* get methods */
//
//uint64_t CStreamFragment::GetFragmentDuration(void)
//{
//  return this->fragmentDuration;
//}
//
//uint64_t CStreamFragment::GetFragmentTime(void)
//{
//  return this->fragmentTime;
//}
//
//const wchar_t *CStreamFragment::GetUrl(void)
//{
//  return this->url;
//}
//
//bool CStreamFragment::GetDownloaded(void)
//{
//  return this->downloaded;
//}
//
//unsigned int CStreamFragment::GetFragmentType(void)
//{
//  return this->fragmentType;
//}
//
//int64_t CStreamFragment::GetStoreFilePosition(void)
//{
//  return this->storeFilePosition;
//}
//
//CLinearBuffer *CStreamFragment::GetBuffer()
//{
//  return this->buffer;
//}
//
//unsigned int CStreamFragment::GetLength(void)
//{
//  return (this->buffer != NULL) ? this->buffer->GetBufferOccupiedSpace() : this->length;
//}
//
///* set methods */
//
//void CStreamFragment::SetDownloaded(bool downloaded)
//{
//  this->downloaded = downloaded;
//}
//
//void CStreamFragment::SetStoredToFile(int64_t position)
//{
//  this->storeFilePosition = position;
//  if (this->storeFilePosition != (-1))
//  {
//    if (this->buffer != NULL)
//    {
//      this->length = this->buffer->GetBufferOccupiedSpace();
//    }
//
//    FREE_MEM_CLASS(this->buffer);
//  }
//  else
//  {
//    if (this->buffer == NULL)
//    {
//      this->buffer = new CLinearBuffer();
//    }
//  }
//}
//
///* other methods */
//
//bool CStreamFragment::IsStoredToFile(void)
//{
//  return (this->storeFilePosition != (-1));
//}