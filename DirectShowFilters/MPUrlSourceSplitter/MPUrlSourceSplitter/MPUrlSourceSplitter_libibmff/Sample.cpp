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

#include "Sample.h"

CSample::CSample(void)
{
  this->sampleCompositionTimeOffset = 0;
  this->sampleDuration = 0;
  this->sampleFlags = 0;
  this->sampleSize = 0;
}

CSample::~CSample(void)
{
}

/* get methods */

uint32_t CSample::GetSampleDuration(void)
{
  return this->sampleDuration;
}

uint32_t CSample::GetSampleSize(void)
{
  return this->sampleSize;
}

uint32_t CSample::GetSampleFlags(void)
{
  return this->sampleFlags;
}

uint32_t CSample::GetSampleCompositionTimeOffset(void)
{
  return this->sampleCompositionTimeOffset;
}

/* set methods */

void CSample::SetSampleDuration(uint32_t sampleDuration)
{
  this->sampleDuration = sampleDuration;
}

void CSample::SetSampleSize(uint32_t sampleSize)
{
  this->sampleSize = sampleSize;
}

void CSample::SetSampleFlags(uint32_t sampleFlags)
{
  this->sampleFlags = sampleFlags;
}

void CSample::SetSampleCompositionTimeOffset(uint32_t sampleCompositionTimeOffset)
{
  this->sampleCompositionTimeOffset = sampleCompositionTimeOffset;
}

/* other methods */