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

#ifndef __MSHS_MANIFEST_CUSTOM_ATTRIBUTE_BOX_COLLECTION_DEFINED
#define __MSHS_MANIFEST_CUSTOM_ATTRIBUTE_BOX_COLLECTION_DEFINED

#include "Collection.h"
#include "MshsManifestCustomAttributeBox.h"

class CMshsManifestCustomAttributeBoxCollection : public CCollection<CMshsManifestCustomAttributeBox>
{
public:
  CMshsManifestCustomAttributeBoxCollection(HRESULT *result);
  virtual ~CMshsManifestCustomAttributeBoxCollection(void);

  // test if custom attribute exists in collection
  // @param name : the custom attribute name to find
  // @param invariant : specifies if custom attribute name shoud be find with invariant casing
  // @return : true if custom attribute exists, false otherwise
  bool Contains(const wchar_t *name, bool invariant);

  // gets custom attribute from collection with specified name
  // @param name : the name of custom attribute to find
  // @param invariant : specifies if custom attribute name shoud be find with invariant casing
  // @return : the reference to custom attribute or NULL if not find
  CMshsManifestCustomAttributeBox *GetCustomAttribute(const wchar_t *name, bool invariant);

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
  //const wchar_t * GetKey(CMSHSCustomAttribute *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CMshsManifestCustomAttributeBox *Clone(CMshsManifestCustomAttributeBox *item);
};

#endif