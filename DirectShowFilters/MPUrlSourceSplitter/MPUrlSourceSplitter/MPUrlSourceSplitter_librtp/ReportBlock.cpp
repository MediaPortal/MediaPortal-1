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

#include "ReportBlock.h"

CReportBlock::CReportBlock(void)
{
  this->synchronizationSourceIdentifier = 0;
  this->fractionLost = 0;
  this->cumulativeNumberOfPacketsLost = 0;
  this->extendedHighestSequenceNumberReceived = 0;
  this->interarrivalJitter = 0;
  this->lastSenderReport = 0;
  this->delaySinceLastSenderReport = 0;
}

CReportBlock::~CReportBlock(void)
{
}

/* get methods */

unsigned int CReportBlock::GetSynchronizationSourceIdentifier(void)
{
  return this->synchronizationSourceIdentifier;
}

unsigned int CReportBlock::GetFractionLost(void)
{
  return this->fractionLost;
}

unsigned int CReportBlock::GetCumulativeNumberOfPacketsLost(void)
{
  return this->cumulativeNumberOfPacketsLost;
}

unsigned int CReportBlock::GetExtendedHighestSequenceNumberReceived(void)
{
  return this->extendedHighestSequenceNumberReceived;
}

unsigned int CReportBlock::GetInterarrivalJitter(void)
{
  return this->interarrivalJitter;
}

unsigned int CReportBlock::GetLastSenderReport(void)
{
  return this->lastSenderReport;
}

unsigned int CReportBlock::GetDelaySinceLastSenderReport(void)
{
  return this->delaySinceLastSenderReport;
}

/* set methods */

void CReportBlock::SetSynchronizationSourceIdentifier(unsigned int synchronizationSourceIdentifier)
{
  this->synchronizationSourceIdentifier = synchronizationSourceIdentifier;
}

void CReportBlock::SetFractionLost(unsigned int fractionLost)
{
  this->fractionLost = fractionLost;
}

void CReportBlock::SetCumulativeNumberOfPacketsLost(unsigned int cumulativeNumberOfPacketsLost)
{
  this->cumulativeNumberOfPacketsLost = cumulativeNumberOfPacketsLost;
}

void CReportBlock::SetExtendedHighestSequenceNumberReceived(unsigned int extendedHighestSequenceNumberReceived)
{
  this->extendedHighestSequenceNumberReceived = extendedHighestSequenceNumberReceived;
}

void CReportBlock::SetInterarrivalJitter(unsigned int interarrivalJitter)
{
  this->interarrivalJitter = interarrivalJitter;
}

void CReportBlock::SetLastSenderReport(unsigned int lastSenderReport)
{
  this->lastSenderReport = lastSenderReport;
}

void CReportBlock::SetDelaySinceLastSenderReport(unsigned int delaySinceLastSenderReport)
{
  this->delaySinceLastSenderReport = delaySinceLastSenderReport;
}

/* other methods */