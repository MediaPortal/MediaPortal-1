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

#ifndef __REPORT_BLOCK_RTCP_PACKET_DEFINED
#define __REPORT_BLOCK_RTCP_PACKET_DEFINED

class CReportBlock
{
public:
  // initializes a new instance of CReportBlock class
  CReportBlock(void);
  virtual ~CReportBlock(void);

  /* get methods */

  virtual unsigned int GetSynchronizationSourceIdentifier(void);
  virtual unsigned int GetFractionLost(void);
  virtual unsigned int GetCumulativeNumberOfPacketsLost(void);
  virtual unsigned int GetExtendedHighestSequenceNumberReceived(void);
  virtual unsigned int GetInterarrivalJitter(void);
  virtual unsigned int GetLastSenderReport(void);
  virtual unsigned int GetDelaySinceLastSenderReport(void);

  /* set methods */

  virtual void SetSynchronizationSourceIdentifier(unsigned int synchronizationSourceIdentifier);
  virtual void SetFractionLost(unsigned int fractionLost);
  virtual void SetCumulativeNumberOfPacketsLost(unsigned int cumulativeNumberOfPacketsLost);
  virtual void SetExtendedHighestSequenceNumberReceived(unsigned int extendedHighestSequenceNumberReceived);
  virtual void SetInterarrivalJitter(unsigned int interarrivalJitter);
  virtual void SetLastSenderReport(unsigned int lastSenderReport);
  virtual void SetDelaySinceLastSenderReport(unsigned int delaySinceLastSenderReport);

  /* other methods */

protected:

  unsigned int synchronizationSourceIdentifier;
  unsigned int fractionLost;
  unsigned int cumulativeNumberOfPacketsLost;
  unsigned int extendedHighestSequenceNumberReceived;
  unsigned int interarrivalJitter;
  unsigned int lastSenderReport;
  unsigned int delaySinceLastSenderReport;
};

#endif