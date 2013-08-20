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

#include "SessionTag.h"

CSessionTag::CSessionTag(void)
{
  this->originalTag = NULL;
  this->instanceTag = NULL;
  this->tagContent = NULL;
}

CSessionTag::~CSessionTag(void)
{
  FREE_MEM(this->originalTag);
  FREE_MEM(this->instanceTag);
  FREE_MEM(this->tagContent);
}

/* get methods */

const wchar_t *CSessionTag::GetOriginalTag(void)
{
  return this->originalTag;
}

const wchar_t *CSessionTag::GetInstanceTag(void)
{
  return this->instanceTag;
}

const wchar_t *CSessionTag::GetTagContent(void)
{
  return this->tagContent;
}

/* set methods */

/* other methods */

bool CSessionTag::IsOriginalTag(const wchar_t *tag)
{
  return ((this->GetOriginalTag() != NULL) && (tag != NULL) && (wcscmp(this->GetOriginalTag(), tag) == 0));
}

bool CSessionTag::IsInstanceTag(const wchar_t *tag)
{
  return ((this->GetInstanceTag() != NULL) && (tag != NULL) && (wcscmp(this->GetInstanceTag(), tag) == 0));
}

void CSessionTag::Clear(void)
{
  FREE_MEM(this->originalTag);
  FREE_MEM(this->instanceTag);
  FREE_MEM(this->tagContent);
}

unsigned int CSessionTag::Parse(const wchar_t *buffer, unsigned int length)
{
  this->Clear();
  unsigned int result = 0;

  if (length >= SESSION_TAG_SIZE)
  {
    if (buffer[1] == SESSION_TAG_SEPARATOR)
    {
      // first character is session tag
      this->originalTag = Substring(buffer, 0, SESSION_TAG_SIZE - 1);
      if (this->originalTag != NULL)
      {
        LineEnding endOfLine = GetEndOfLine(buffer, length);
        unsigned int contentSize = (endOfLine.position == (-1)) ? length : endOfLine.position;

        if (contentSize > SESSION_TAG_SIZE)
        {
          contentSize -= SESSION_TAG_SIZE;
          this->tagContent = Substring(buffer, SESSION_TAG_SIZE, contentSize);
          result = (this->tagContent != NULL) ? (endOfLine.position + endOfLine.size) : 0;
        }
        else
        {
          result = SESSION_TAG_SIZE;
        }
      }
    }
  }

  return result;
}