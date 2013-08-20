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

#include "SampleEntryBoxFactory.h"
#include "HandlerBox.h"
#include "AudioSampleEntryBox.h"
#include "VisualSampleEntryBox.h"

CSampleEntryBoxFactory::CSampleEntryBoxFactory(void)
{
}

CSampleEntryBoxFactory::~CSampleEntryBoxFactory(void)
{
}

CBox *CSampleEntryBoxFactory::CreateBox(const uint8_t *buffer, uint32_t length, uint32_t handlerType)
{
  CBox *result = NULL;
  bool continueParsing = ((buffer != NULL) && (length > 0));

  if (continueParsing)
  {
    CBox *box = new CBox();
    continueParsing &= (box != NULL);

    if (continueParsing)
    {
      continueParsing &= box->Parse(buffer, length);
      if (continueParsing)
      {
        switch (handlerType)
        {
        case HANDLER_TYPE_AUDIO:
          CREATE_SPECIFIC_BOX_WITHOUT_HEADER_TYPE(box, CAudioSampleEntryBox, buffer, length, continueParsing, result);
          break;
        case HANDLER_TYPE_VIDEO:
          CREATE_SPECIFIC_BOX_WITHOUT_HEADER_TYPE(box, CVisualSampleEntryBox, buffer, length, continueParsing, result);
          break;
        default:
          break;
        }

        if (continueParsing && (result == NULL))
        {
          result = __super::CreateBox(buffer, length);
        }
      }
    }

    if (continueParsing && (result == NULL))
    {
      result = box;
    }

    if (!continueParsing)
    {
      FREE_MEM_CLASS(box);
    }
  }

  return result;
}

CBox *CSampleEntryBoxFactory::CreateBox(const uint8_t *buffer, uint32_t length)
{
  return NULL;
}