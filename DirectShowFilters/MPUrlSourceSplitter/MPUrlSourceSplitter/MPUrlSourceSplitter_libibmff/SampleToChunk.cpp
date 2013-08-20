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

#include "SampleToChunk.h"

CSampleToChunk::CSampleToChunk(void)
{
  this->firstChunk = 0;
  this->samplesPerChunk = 0;
  this->sampleDescriptionIndex = 0;
}

CSampleToChunk::~CSampleToChunk(void)
{
}

/* get methods */

uint32_t CSampleToChunk::GetFirstChunk(void)
{
  return this->firstChunk;
}

uint32_t CSampleToChunk::GetSamplesPerChunk(void)
{
  return this->samplesPerChunk;
}

uint32_t CSampleToChunk::GetSampleDescriptionIndex(void)
{
  return this->sampleDescriptionIndex;
}

/* set methods */

void CSampleToChunk::SetFirstChunk(uint32_t firstChunk)
{
  this->firstChunk = firstChunk;
}

void CSampleToChunk::SetSamplesPerChunk(uint32_t samplesPerChunk)
{
  this->samplesPerChunk = samplesPerChunk;
}

void CSampleToChunk::SetSampleDescriptionIndex(uint32_t sampleDescriptionIndex)
{
  this->sampleDescriptionIndex = sampleDescriptionIndex;
}

/* other methods */
