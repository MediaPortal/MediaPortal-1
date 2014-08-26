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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "AfhsDecryptionContext.h"

#pragma warning(pop)

CAfhsDecryptionContext::CAfhsDecryptionContext(HRESULT *result)
{
  this->curlInstance = NULL;
  this->segmentFragments = NULL;
  this->segmentFragmentDownloading = UINT_MAX;
  this->segmentFragmentProcessing = UINT_MAX;
  this->segmentFragmentToDownload = UINT_MAX;
  this->segmentFragmentDecrypting = UINT_MAX;
}

CAfhsDecryptionContext::~CAfhsDecryptionContext(void)
{
}

/* get methods */

CAfhsCurlInstance *CAfhsDecryptionContext::GetCurlInstance(void)
{
  return this->curlInstance;
}

CAfhsSegmentFragmentCollection *CAfhsDecryptionContext::GetSegmentsFragments(void)
{
  return this->segmentFragments;
}

unsigned int CAfhsDecryptionContext::GetSegmentFragmentDownloading(void)
{
  return this->segmentFragmentDownloading;
}

unsigned int CAfhsDecryptionContext::GetSegmentFragmentProcessing(void)
{
  return this->segmentFragmentProcessing;
}

unsigned int CAfhsDecryptionContext::GetSegmentFragmentToDownload(void)
{
  return this->segmentFragmentToDownload;
}

unsigned int CAfhsDecryptionContext::GetSegmentFragmentDecrypting(void)
{
  return this->segmentFragmentDecrypting;
}

/* set methods */

void CAfhsDecryptionContext::SetCurlInstance(CAfhsCurlInstance *curlInstance)
{
  this->curlInstance = curlInstance;
}

void CAfhsDecryptionContext::SetSegmentsFragments(CAfhsSegmentFragmentCollection *segmentFragments)
{
  this->segmentFragments = segmentFragments;
}

void CAfhsDecryptionContext::SetSegmentFragmentDownloading(unsigned int segmentFragmentDownloading)
{
  this->segmentFragmentDownloading = segmentFragmentDownloading;
}

void CAfhsDecryptionContext::SetSegmentFragmentProcessing(unsigned int segmentFragmentProcessing)
{
  this->segmentFragmentProcessing = segmentFragmentProcessing;
}

void CAfhsDecryptionContext::SetSegmentFragmentToDownload(unsigned int segmentFragmentToDownload)
{
  this->segmentFragmentToDownload = segmentFragmentToDownload;
}

void CAfhsDecryptionContext::SetSegmentFragmentDecrypting(unsigned int segmentFragmentDecrypting)
{
  this->segmentFragmentDecrypting = segmentFragmentDecrypting;
}

/* other methods */



///* get methods */
//
//// gets segments and fragments collection
//CSegmentFragmentCollection *CAfhsDecryptionContext::GetSegmentsFragments(void)
//{
//  return this->segmentsFragments;
//}
//
//unsigned int CAfhsDecryptionContext::GetSegmentFragmentDownloading(void)
//{
//  return this->segmentFragmentDownloading;
//}
//
//unsigned int CAfhsDecryptionContext::GetSegmentFragmentProcessing(void)
//{
//  return this->segmentFragmentProcessing;
//}
//
//unsigned int CAfhsDecryptionContext::GetSegmentFragmentToDownload(void)
//{
//  return this->segmentFragmentToDownload;
//}
//
//const wchar_t *CAfhsDecryptionContext::GetManifestUrl(void)
//{
//  return this->manifestUrl;
//}
//
//const wchar_t *CAfhsDecryptionContext::GetManifestContent(void)
//{
//  return this->manifestContent;
//}
//
//bool CAfhsDecryptionContext::GetForceDownload(void)
//{
//  return this->forceDownload;
//}
//
///* set methods */
//
//void CAfhsDecryptionContext::SetSegmentsFragments(CSegmentFragmentCollection *segmentsFragments)
//{
//  this->segmentsFragments = segmentsFragments;
//}
//
//void CAfhsDecryptionContext::SetSegmentFragmentDownloading(unsigned int segmentFragmentDownloading)
//{
//  this->segmentFragmentDownloading = segmentFragmentDownloading;
//}
//
//void CAfhsDecryptionContext::SetSegmentFragmentProcessing(unsigned int segmentFragmentProcessing)
//{
//  this->segmentFragmentProcessing = segmentFragmentProcessing;
//}
//
//void CAfhsDecryptionContext::SetSegmentFragmentToDownload(unsigned int segmentFragmentToDownload)
//{
//  this->segmentFragmentToDownload = segmentFragmentToDownload;
//}
//
//void CAfhsDecryptionContext::SetManifestUrl(const wchar_t *manifestUrl)
//{
//  this->manifestUrl = manifestUrl;
//}
//
//void CAfhsDecryptionContext::SetManifestContent(const wchar_t *manifestContent)
//{
//  this->manifestContent = manifestContent;
//}
//
//void CAfhsDecryptionContext::SetForceDownload(bool forceDownload)
//{
//  this->forceDownload = forceDownload;
//}
//
///* other methods */
