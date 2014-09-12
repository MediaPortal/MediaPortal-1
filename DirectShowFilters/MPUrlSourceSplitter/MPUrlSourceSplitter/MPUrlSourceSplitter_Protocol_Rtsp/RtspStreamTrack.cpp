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

#include "RtspStreamTrack.h"

CRtspStreamTrack::CRtspStreamTrack(HRESULT *result)
  : CFlags()
{
  this->streamFragments = NULL;
  this->lastCumulatedRtpTimestamp = 0;
  this->lastRtpPacketTimestamp = 0;
  this->firstRtpPacketTimestamp = 0;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->streamLength = 0;
  this->clockFrequency = 0;
  this->dshowTimeBaseNumerator = 0;
  this->dshowTimeBaseDenominator = 1;
  this->rtpTimestampCorrection = 0;
  this->lastReceiveDataTime = 0;

  this->cacheFile = NULL;

  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->streamFragments = new CRtspStreamFragmentCollection(result);
    this->cacheFile = new CCacheFile(result);

    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
  }
}

CRtspStreamTrack::~CRtspStreamTrack(void)
{
  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM_CLASS(this->cacheFile);
}

/* get methods */

CRtspStreamFragmentCollection *CRtspStreamTrack::GetStreamFragments(void)
{
  return this->streamFragments;
}

unsigned int CRtspStreamTrack::GetStreamFragmentDownloading(void)
{
  return this->streamFragmentDownloading;
}

unsigned int CRtspStreamTrack::GetStreamFragmentProcessing(void)
{
  return this->streamFragmentProcessing;
}

unsigned int CRtspStreamTrack::GetStreamFragmentToDownload(void)
{
  return this->streamFragmentToDownload;
}

int64_t CRtspStreamTrack::GetStreamLength(void)
{
  return this->streamLength;
}

int64_t CRtspStreamTrack::GetBytePosition(void)
{
  int64_t result = 0;

  unsigned int first = this->GetStreamFragments()->GetStartSearchingIndex();
  unsigned int count = this->GetStreamFragments()->GetSearchCount();

  if (count != 0)
  {
    CRtspStreamFragment *firstFragment = this->GetStreamFragments()->GetItem(first);
    CRtspStreamFragment *lastFragment = this->GetStreamFragments()->GetItem(first + count - 1);

    result = (unsigned int)(lastFragment->GetFragmentStartPosition() + lastFragment->GetLength() - firstFragment->GetFragmentStartPosition());
  }

  return result;
}

unsigned int CRtspStreamTrack::GetFirstRtpPacketTimestamp(void)
{
  return this->firstRtpPacketTimestamp;
}

unsigned int CRtspStreamTrack::GetFirstRtpPacketTicks(void)
{
  return this->firstRtpPacketTicks;
}

int64_t CRtspStreamTrack::GetRtpPacketTimestamp(unsigned int currentRtpPacketTimestamp)
{
  int64_t difference = ((currentRtpPacketTimestamp < this->lastRtpPacketTimestamp) ? 0x0000000100000000 : 0);
  difference += currentRtpPacketTimestamp;
  difference -= this->lastRtpPacketTimestamp;

  if (currentRtpPacketTimestamp < this->lastRtpPacketTimestamp)
  {
    // try to identify if overflow occured or RTP timestamp is only slightly decreased
    uint64_t diff = this->lastRtpPacketTimestamp - currentRtpPacketTimestamp;

    // on this place is difference always greater than or equal to zero, we can safely cast it to uint64_t
    if (diff < (uint64_t)difference)
    {
      // RTP timestamp decrease is more probable than overflow
      difference -= 0x0000000100000000;
    }
  }

  int64_t result = this->lastCumulatedRtpTimestamp + difference;

  this->lastCumulatedRtpTimestamp += difference;
  this->lastRtpPacketTimestamp = currentRtpPacketTimestamp;

  return result;
}

int64_t CRtspStreamTrack::GetRtpPacketTimestampInDshowTimeBaseUnits(int64_t rtpPacketTimestamp)
{
  return (rtpPacketTimestamp * this->dshowTimeBaseNumerator / this->dshowTimeBaseDenominator);
}

int64_t CRtspStreamTrack::GetRtpTimestampCorrection(void)
{
  return this->rtpTimestampCorrection;
}

unsigned int CRtspStreamTrack::GetClockFrequency(void)
{
  return this->clockFrequency;
}

CCacheFile *CRtspStreamTrack::GetCacheFile(void)
{
  return this->cacheFile;
}

unsigned int CRtspStreamTrack::GetLastReceiveDataTime(void)
{
  return this->lastReceiveDataTime;
}

unsigned int CRtspStreamTrack::GetLastProcessedSize(void)
{
  return this->lastProcessedSize;
}

unsigned int CRtspStreamTrack::GetCurrentProcessedSize(void)
{
  return this->currentProcessedSize;
}

/* set methods */

void CRtspStreamTrack::SetStreamFragmentDownloading(unsigned int streamFragmentDownloading)
{
  this->streamFragmentDownloading = streamFragmentDownloading;
}

void CRtspStreamTrack::SetStreamFragmentProcessing(unsigned int streamFragmentProcessing)
{
  this->streamFragmentProcessing = streamFragmentProcessing;
}

void CRtspStreamTrack::SetStreamFragmentToDownload(unsigned int streamFragmentToDownload)
{
  this->streamFragmentToDownload = streamFragmentToDownload;
}

void CRtspStreamTrack::SetStreamLength(int64_t streamLength)
{
  this->streamLength = streamLength;
}

void CRtspStreamTrack::SetStreamLengthFlag(bool setStreamLengthFlag)
{
  this->flags &= ~RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH;
  this->flags |= setStreamLengthFlag ? RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH : RTSP_STREAM_TRACK_FLAG_NONE;
}

void CRtspStreamTrack::SetEndOfStreamFlag(bool endOfStreamFlag)
{
  this->flags &= ~RTSP_STREAM_TRACK_FLAG_END_OF_STREAM;
  this->flags |= endOfStreamFlag ? RTSP_STREAM_TRACK_FLAG_END_OF_STREAM : RTSP_STREAM_TRACK_FLAG_NONE;
}

void CRtspStreamTrack::SetReceivedAllDataFlag(bool receivedAllDataFlag)
{
  this->flags &= ~RTSP_STREAM_TRACK_FLAG_RECEIVED_ALL_DATA;
  this->flags |= receivedAllDataFlag ? RTSP_STREAM_TRACK_FLAG_RECEIVED_ALL_DATA : RTSP_STREAM_TRACK_FLAG_NONE;
}

void CRtspStreamTrack::SetFirstRtpPacketTimestamp(unsigned int rtpPacketTimestamp, bool firstRtpPacketTimestampFlag, unsigned int firstRtpPacketTicks)
{
  this->firstRtpPacketTimestamp = rtpPacketTimestamp;
  this->lastRtpPacketTimestamp = 0;
  this->lastCumulatedRtpTimestamp = 0;

  this->flags &= ~RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP;
  this->flags |= firstRtpPacketTimestampFlag ? RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP : RTSP_STREAM_TRACK_FLAG_NONE;

  if (firstRtpPacketTimestampFlag && (!this->IsSetFlags(RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TICKS)))
  {
    this->firstRtpPacketTicks = firstRtpPacketTicks;
    this->flags |= RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TICKS;
  }
}

void CRtspStreamTrack::SetClockFrequency(unsigned int clockFrequency)
{
  this->clockFrequency = clockFrequency;

  unsigned int gcd = GreatestCommonDivisor(DSHOW_TIME_BASE, this->clockFrequency);
  this->dshowTimeBaseNumerator = (int64_t)(DSHOW_TIME_BASE / gcd);
  this->dshowTimeBaseDenominator = (int64_t)(this->clockFrequency / gcd);
}

void CRtspStreamTrack::SetRtpTimestampCorrection(int64_t rtpTimestampCorrection)
{
  this->rtpTimestampCorrection = rtpTimestampCorrection;
}

void CRtspStreamTrack::SetLastReceiveDataTime(unsigned int lastReceiveDataTime)
{
  this->lastReceiveDataTime = lastReceiveDataTime;
}

void CRtspStreamTrack::SetLastProcessedSize(unsigned lastProcessedSize)
{
  this->lastProcessedSize = lastProcessedSize;
}

void CRtspStreamTrack::SetCurrentProcessedSize(unsigned int currentProcessedSize)
{
  this->currentProcessedSize = currentProcessedSize;
}

/* other methods */

bool CRtspStreamTrack::IsSetFirstRtpPacketTimestamp(void)
{
  return this->IsSetFlags(RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP);
}

bool CRtspStreamTrack::IsSetStreamLength(void)
{
  return this->IsSetFlags(RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH);
}

bool CRtspStreamTrack::IsSetEndOfStream(void)
{
  return this->IsSetFlags(RTSP_STREAM_TRACK_FLAG_END_OF_STREAM);
}

bool CRtspStreamTrack::IsReceivedAllData(void)
{
  return this->IsSetFlags(RTSP_STREAM_TRACK_FLAG_RECEIVED_ALL_DATA);
}

/* protected methods */
