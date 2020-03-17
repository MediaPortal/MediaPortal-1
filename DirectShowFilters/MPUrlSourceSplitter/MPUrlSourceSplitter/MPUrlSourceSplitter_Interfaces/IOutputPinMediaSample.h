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

#ifndef __IOUTPUT_PIN_MEDIA_SAMPLE_DEFINED
#define __IOUTPUT_PIN_MEDIA_SAMPLE_DEFINED

#include "OutputPinPacket.h"

#include <streams.h>

MIDL_INTERFACE("32377C8E-8898-4F74-872E-E507FEB72FAA")
IOutputPinMediaSample : public IUnknown
{
  // sets output packet to media sample
  // @param packet : the packet to set to media sample
  // @return : S_OK if successful, E_INVALIDARG if packet is NULL, error code otherwise
  virtual STDMETHODIMP SetPacket(COutputPinPacket *packet) = 0;
};

#endif