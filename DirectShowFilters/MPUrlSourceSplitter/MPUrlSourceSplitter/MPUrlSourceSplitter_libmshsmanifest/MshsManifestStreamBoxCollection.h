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

#pragma once

#ifndef __MSHS_MANIFEST_STREAM_BOX_COLLECTION_DEFINED
#define __MSHS_MANIFEST_STREAM_BOX_COLLECTION_DEFINED

#include "Collection.h"
#include "MshsManifestStreamBox.h"

class CMshsManifestStreamBoxCollection : public CCollection<CMshsManifestStreamBox>
{
public:
  CMshsManifestStreamBoxCollection(HRESULT *result);
  virtual ~CMshsManifestStreamBoxCollection(void);

  //// test if stream exists in collection
  //// @param name : the stream name to find
  //// @param invariant : specifies if stream name shoud be find with invariant casing
  //// @return : true if stream exists, false otherwise
  //bool Contains(const wchar_t *name, bool invariant);

  //// gets stream from collection with specified name
  //// @param name : the name of stream to find
  //// @param invariant : specifies if stream name shoud be find with invariant casing
  //// @return : the reference to stream or NULL if not find
  //CMSHSStream *GetStream(const wchar_t *name, bool invariant);

protected:

  //// compare two item keys
  //// @param firstKey : the first item key to compare
  //// @param secondKey : the second item key to compare
  //// @param context : the reference to user defined context
  //// @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  //int CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context);

  //// gets key for item
  //// @param item : the item to get key
  //// @return : the key of item
  //const wchar_t * GetKey(CMSHSStream *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CMshsManifestStreamBox *Clone(CMshsManifestStreamBox *item);
};

#endif