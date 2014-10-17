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

#include "OutputPinDumpBox.h"

COutputPinDumpBox::COutputPinDumpBox(HRESULT *result)
  : CDumpBox(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(OUTPUT_PIN_DUMP_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

COutputPinDumpBox::~COutputPinDumpBox(void)
{
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

bool COutputPinDumpBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType)
{
  FREE_MEM(this->payload);
  this->payloadSize = 0;

  if (__super::ParseInternal(buffer, length, false, false))
  {
    this->flags &= BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, OUTPUT_PIN_DUMP_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}
