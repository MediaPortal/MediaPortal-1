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

#ifndef __GENERAL_TAG_DEFINED
#define __GENERAL_TAG_DEFINED

#include "Item.h"

#define GENERAL_TAG_FLAG_NONE                                         ITEM_FLAG_NONE

#define GENERAL_TAG_FLAG_LAST                                         (ITEM_FLAG_LAST + 0)

#define TAG_DIRECTIVE_SIZE                                            1
#define TAG_DIRECTIVE_PREFIX                                          L"#"      // with '#' starts directive (comment or tag)

#define TAG_SEPARATOR                                                 L":"      // tag separator is optional, but if present it is ':'
#define TAG_SEPARATOR_SIZE                                            1

class CGeneralTag : public CItem
{
public:
  CGeneralTag(HRESULT *result);
  virtual ~CGeneralTag(void);

  /* get methods */

  // gets tag
  // @return : tag
  virtual const wchar_t *GetTag(void);

  // gets tag content
  // @return: tag content (can be NULL if no tag content)
  virtual const wchar_t *GetTagContent(void);

  /* set methods */

  /* other methods */

  // tests if item can be part of media playlist
  // @param : the playlist version
  // @return : true if item can be part of media playlist, false otherwise
  virtual bool IsMediaPlaylistItem(unsigned int version);

  // tests if item can be part of master playlist
  // @param : the playlist version
  // @return : true if item can be part of master playlist, false otherwise
  virtual bool IsMasterPlaylistItem(unsigned int version);

  // clears current instance
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with item data for parsing
  // @param length : the length of data in buffer
  // @param : the playlist version
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length, unsigned int version);

  // parses item data
  // @param item : the item to parse
  // @return : true if successful, false otherwise
  virtual bool ParseItem(CItem *item);

  // parses general tag data
  // @param tag : the general tag to parse
  // @param : the playlist version
  // @return : true if successful, false otherwise
  virtual bool ParseGeneralTag(CGeneralTag *tag, unsigned int version);

protected:

  // holds tag
  wchar_t *tag;

  // holds tag content
  wchar_t *tagContent;

  /* methods */

  // parses current tag
  // @param : the playlist version
  // @return : true if successful, false otherwise
  virtual bool ParseTag(unsigned int version);

  // creates item
  // @return : item or NULL if error
  virtual CItem *CreateItem(void);

  // deeply clones current instance to specified item
  // @param item : the item to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CItem *item);
};

#endif