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

#include "HttpDumpBox.h"

CHttpDumpBox::CHttpDumpBox(HRESULT *result)
  : CDumpBox(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(HTTP_DUMP_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CHttpDumpBox::~CHttpDumpBox(void)
{
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

unsigned int CHttpDumpBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType)
{
  uint32_t position = __super::ParseInternal(buffer, length, false, false);

  if (position != 0)
  {
    HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

    if (checkType)
    {
      this->flags &= BOX_FLAG_PARSED;
      this->flags |= (wcscmp(this->type, HTTP_DUMP_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }

    if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
    {
      this->ProcessAdditionalBoxes(buffer, length, position);
    }

    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  }

  return this->IsSetFlags(BOX_FLAG_PARSED) ? position : 0;
}
