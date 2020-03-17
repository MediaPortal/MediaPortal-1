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

#ifndef __SESSION_TAG_DEFINED
#define __SESSION_TAG_DEFINED

#include "Flags.h"

#define ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, tagType, classTag, classTagType, assigned, result)       \
                                                                                                        \
if ((result != 0) && (!assigned) && (sessionTag->IsOriginalTag(tagType)) && (classTag == NULL))         \
{                                                                                                       \
  classTag = dynamic_cast<classTagType *>(sessionTag);                                                  \
  result = (classTag != NULL) ? result : 0;                                                             \
  assigned = true;                                                                                      \
}

#define ADD_SESSION_TAG_TO_COLLECTION_BY_ORIGINAL_TAG(sessionTag, tagType, collection, classTagType, assigned, result)       \
                                                                                                        \
if ((result != 0) && (!assigned) && (sessionTag->IsOriginalTag(tagType)))                               \
{                                                                                                       \
  classTagType *classTag = dynamic_cast<classTagType *>(sessionTag);                                    \
  if (classTag != NULL)                                                                                 \
  {                                                                                                     \
    result = collection->Add(classTag) ? result : 0;                                                    \
    assigned = true;                                                                                    \
  }                                                                                                     \
}

#define ASSIGN_SESSION_TAG_BY_INSTANCE_TAG(sessionTag, tagType, classTag, classTagType, assigned, result)       \
                                                                                                        \
if ((result != 0) && (!assigned) && (sessionTag->IsInstanceTag(tagType)) && (classTag == NULL))         \
{                                                                                                       \
  classTag = dynamic_cast<classTagType *>(sessionTag);                                                  \
  result = (classTag != NULL) ? result : 0;                                                             \
  assigned = true;                                                                                      \
}

#define ADD_SESSION_TAG_TO_COLLECTION_BY_INSTANCE_TAG(sessionTag, tagType, collection, classTagType, assigned, result)       \
                                                                                                        \
if ((result != 0) && (!assigned) && (sessionTag->IsInstanceTag(tagType)))                               \
{                                                                                                       \
  classTagType *classTag = dynamic_cast<classTagType *>(sessionTag);                                    \
  if (classTag != NULL)                                                                                 \
  {                                                                                                     \
    result = collection->Add(classTag) ? result : 0;                                                    \
    assigned = true;                                                                                    \
  }                                                                                                     \
}

#define SESSION_TAG_SIZE                                              2         // minimum size for session tag (<tag>= )
#define SESSION_TAG_SEPARATOR                                         L'='

#define SESSION_TAG_FLAG_NONE                                         FLAGS_NONE

#define SESSION_TAG_FLAG_LAST                                         (FLAGS_LAST + 0)

class CSessionTag : public CFlags
{
public:
  // initialize a new instance of CSessionTag class
  CSessionTag(HRESULT *result);
  virtual ~CSessionTag(void);

  /* get methods */

  // gets session original tag
  // @return : session original tag
  virtual const wchar_t *GetOriginalTag(void);

  // gets session instance tag - for better determination of specific session type
  // @return: session instance tag
  virtual const wchar_t *GetInstanceTag(void);

  // gets tag content
  // @return: tag content
  virtual const wchar_t *GetTagContent(void);

  /* set methods */

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // tests if original tag is specified tag
  // @param tag : tag to test
  // @return : true if original tag is specified tag, false otherwise
  virtual bool IsOriginalTag(const wchar_t *tag);

  // tests if instance tag is specified tag
  // @param tag : tag to test
  // @return : true if instance tag is specified tag, false otherwise
  virtual bool IsInstanceTag(const wchar_t *tag);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds session tag (only one character)
  wchar_t *originalTag;

  // holds tag content
  wchar_t *tagContent;

  // holds session instance tag (for better determination of specific session type)
  wchar_t *instanceTag;
};

#endif