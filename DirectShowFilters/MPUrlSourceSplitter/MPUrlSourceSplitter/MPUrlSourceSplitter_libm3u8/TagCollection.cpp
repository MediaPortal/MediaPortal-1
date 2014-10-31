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

#include "TagCollection.h"

CTagCollection::CTagCollection(HRESULT *result)
  : CCollection(result)
{
}

CTagCollection::~CTagCollection(void)
{
}

/* get methods */

CEndListTag *CTagCollection::GetEndList(void)
{
  for (unsigned int i = 0; i < this->Count(); i++)
  {
    CEndListTag *tag = dynamic_cast<CEndListTag *>(this->GetItem(i));
    
    if (tag != NULL)
    {
      return tag;
    }
  }

  return NULL;
}

CMediaSequenceTag *CTagCollection::GetMediaSequence(void)
{
  for (unsigned int i = 0; i < this->Count(); i++)
  {
    CMediaSequenceTag *tag = dynamic_cast<CMediaSequenceTag *>(this->GetItem(i));
    
    if (tag != NULL)
    {
      return tag;
    }
  }

  return NULL;
}

/* set methods */

/* other methods */

/* protected methods */

CTag *CTagCollection::Clone(CTag *tag)
{
  return (CTag *)tag->Clone();
}
