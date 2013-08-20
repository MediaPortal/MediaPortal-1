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

#include "formatUrl.h"

#include <WinInet.h>
#include <stdio.h>
#include <wchar.h>

extern void ZeroURL(URL_COMPONENTS *url);

// gets base URL without last '/'
// @param url : URL to get base url
// @return : base URL or NULL if error
wchar_t *GetBaseUrl(const wchar_t *url)
{
  wchar_t *result = NULL;

  ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
  if ((urlComponents != NULL) && (url != NULL))
  {
    ZeroURL(urlComponents);
    urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

    if (InternetCrackUrl(url, 0, 0, urlComponents))
    {
      // if URL path is not specified, than whole URL is base URL
      if (urlComponents->dwUrlPathLength != 0)
      {
        wchar_t *temp = NULL;
        unsigned int tempLength = wcslen(url) + 1;

        if (urlComponents->dwExtraInfoLength != 0)
        {
          // there is some extra information, ignore it
          tempLength -= urlComponents->dwExtraInfoLength;
        }

        temp = ALLOC_MEM_SET(temp, wchar_t, tempLength, 0);
        if ((temp != NULL) && (tempLength != 0))
        {
          wmemcpy(temp, url, tempLength - 1);

          // find last '/'
          // before it is base URL
          const wchar_t *last = wcsrchr(temp, L'/');
          unsigned int length = (last - temp);

          result = ALLOC_MEM_SET(result, wchar_t, (length + 1), 0);
          if (result != NULL)
          {
            wmemcpy(result, temp, length);
          }
        }
        FREE_MEM(temp);
      }
    }
  }
  FREE_MEM(urlComponents);

  return result;
}

wchar_t *GetAdditionalParameters(const wchar_t *url)
{
  wchar_t *result = NULL;
  int index = IndexOf(url, L"?");
  if (index != (-1))
  {
    result = Substring(url, index + 1);
  }
  return result;
}

wchar_t *GetHost(const wchar_t *url)
{
  wchar_t *result = NULL;
  int index = IndexOf(url, L"://");
  if (index != (-1))
  {
    wchar_t *substring = Substring(url, index + 3);
    if (substring != NULL)
    {
      int index2 = IndexOf(substring, L"/");
      if (index2 == (-1))
      {
        // no additional path, whole url is host
        result = Duplicate(url);
      }
      else
      {
        // index2 is relative to index
        index += index2 + 3;
        result = Substring(url, 0, index);
      }
    }
    FREE_MEM(substring);
  }
  return result;
}

// tests if URL is absolute
// @param url : URL to test
// @return : true if URL is absolute, false otherwise or if error
bool IsAbsoluteUrl(const wchar_t *url)
{
  bool result = false;

  ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
  if ((urlComponents != NULL) && (url != NULL))
  {
    ZeroURL(urlComponents);
    urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

    if (InternetCrackUrl(url, 0, 0, urlComponents))
    {
      result = true;
    }
  }
  FREE_MEM(urlComponents);

  return result;
}

// gets absolute URL combined from base URL and relative URL
// if relative URL is absolute, then duplicate of relative URL is returned
// @param baseUrl : base URL for combining, URL have to be without last '/'
// @param relativeUrl : relative URL for combinig
// @return : absolute URL or NULL if error
wchar_t *FormatAbsoluteUrl(const wchar_t *baseUrl, const wchar_t *relativeUrl)
{
  wchar_t *result = NULL;

  if ((baseUrl != NULL) && (relativeUrl != NULL))
  {
    if (IsNullOrEmptyOrWhitespaceW(relativeUrl))
    {
      result = DuplicateW(baseUrl);
    }
    else if (IsAbsoluteUrl(relativeUrl))
    {
      result = DuplicateW(relativeUrl);
    }
    else
    {
      // URL is concatenation of base URL and relative URL
      unsigned int baseUrlLength = wcslen(baseUrl);
      unsigned int relativeUrlLength = wcslen(relativeUrl);
      // we need one extra character for '/' between base URL and relative URL

      if (EndsWith(baseUrl, L'/'))
      {
        baseUrlLength--;
      }
      if (wcsncmp(relativeUrl, L"/", 1) == 0)
      {
        // the first character is '/'
        relativeUrlLength--;
        relativeUrl++;
      }

      unsigned int length = baseUrlLength + relativeUrlLength + 1;

      result = ALLOC_MEM_SET(result, wchar_t, (length + 1), 0);
      if (result != NULL)
      {
        wcsncpy_s(result, length + 1, baseUrl, baseUrlLength);
        wcscat_s(result, length + 1, L"/");
        wcscat_s(result, length + 1, relativeUrl);
      }
    }
  }

  return result;
}

// gets absolute base URL combined from base URL and relative URL
// @param baseUrl : base URL for combining, URL have to be without last '/'
// @param relativeUrl : relative URL for combinig, URL have to be without start '/'
// @return : absolute base URL or NULL if error
wchar_t *FormatAbsoluteBaseUrl(const wchar_t *baseUrl, const wchar_t *relativeUrl)
{
  wchar_t *result = NULL;
  wchar_t *absoluteUrl = FormatAbsoluteUrl(baseUrl, relativeUrl);

  result = GetBaseUrl(absoluteUrl);
  FREE_MEM(absoluteUrl);

  return result;
}