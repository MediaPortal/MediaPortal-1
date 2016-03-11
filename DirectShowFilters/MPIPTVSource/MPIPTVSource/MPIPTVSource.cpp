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

// MPIPTVSource.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "MPIPTVSource.h"
#include "Utilities.h"

#define MODULE_NAME                                               _T("MPIPTVSource")

#define METHOD_SET_CONNECT_INFO_NAME                              _T("SetConnectInfo()")

CMPIPTVSource::CMPIPTVSource(IUnknown *pUnk, HRESULT *phr)
  : CSource(NAME(_T("MediaPortal IPTV Source")), pUnk, CLSID_MPIPTVSource)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);

  // The pin magically adds itself to our pin array.
  this->m_parameters = new CParameterCollection();
  this->m_configuration = GetConfiguration(&this->logger, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, CONFIGURATION_SECTION_MPIPTVSOURCE);
  this->m_stream = new CMPIPTVSourceStream(phr, this, this->m_configuration);

  if (phr)
  {
    if (this->m_stream == NULL)
    {
      *phr = E_OUTOFMEMORY;
    }
    else
    {
      TCHAR *guid = ConvertGuidToString(this->m_stream->GetInstanceId());
      this->logger.Log(LOGGER_INFO, _T("%s: %s: created new instance of IPTV source stream, id: %s"), MODULE_NAME, METHOD_CONSTRUCTOR_NAME, guid);
      FREE_MEM(guid);
      *phr = S_OK;
    }
  }

  this->m_url = NULL;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTVSource::~CMPIPTVSource()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  TCHAR *guid = ConvertGuidToString(this->m_stream->GetInstanceId());
  this->logger.Log(LOGGER_INFO, _T("%s: %s: destroying instance of IPTV source stream, id: %s"), MODULE_NAME, METHOD_DESTRUCTOR_NAME, guid);
  FREE_MEM(guid);
  delete this->m_stream;
  delete this->m_parameters;
  delete this->m_configuration;

  FREE_MEM(this->m_url);

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);
}

STDMETHODIMP CMPIPTVSource::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  if (riid == _uuidof(IFileSourceFilter))
  {
    return GetInterface((IFileSourceFilter *)this, ppv);
  }
  else if (riid == _uuidof(IMPIPTVConnectInfo))
  {
    return GetInterface((IMPIPTVConnectInfo *)this, ppv);
  }
  else
  {
    return __super::NonDelegatingQueryInterface(riid, ppv);
  }
}

STDMETHODIMP CMPIPTVSource::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE* pmt) 
{
#ifdef _MBCS
  this->m_url = ConvertToMultiByteW(pszFileName);
#else
  this->m_url = ConvertToUnicodeW(pszFileName);
#endif

  if (this->m_url == NULL)
  {
    return E_FAIL;
  }

  // temporary section for specifying network interface
  if (this->SetConnectInfo(pszFileName))
  {
    return E_FAIL;
  }

  if (this->m_parameters != NULL)
  {
    // we have set some parameters
    // get url parameter
    PCParameter urlParameter = this->m_parameters->GetParameter(URL_PARAMETER_NAME, true);
    if (urlParameter != NULL)
    {
      // free current url
      FREE_MEM(this->m_url);
      // make duplicate of parameter url
      this->m_url = Duplicate(urlParameter->GetValue());
    }
  }

  if(!m_stream->Load(this->m_url, this->m_parameters))
  {
    return E_FAIL;
  }

  return S_OK;
}

STDMETHODIMP CMPIPTVSource::GetCurFile(LPOLESTR* ppszFileName, AM_MEDIA_TYPE* pmt)
{
  if (!ppszFileName)
  {
    return E_POINTER;
  }

  *ppszFileName = ConvertToUnicode(this->m_url);
  if ((*ppszFileName) == NULL)
  {
    return E_FAIL;
  }

  return S_OK;
}

CUnknown * WINAPI CMPIPTVSource::CreateInstance(IUnknown *pUnk, HRESULT *phr)
{
  CMPIPTVSource *pNewFilter = new CMPIPTVSource(pUnk, phr);

  if (phr)
  {
    if (pNewFilter == NULL) 
    {
      *phr = E_OUTOFMEMORY;
    }
    else
    {
      *phr = S_OK;
    }
  }

  return pNewFilter;
}

// split parameters string by separator
// @param parameters : null-terminated string containing parameters
// @param separator : null-terminated separator string
// @param length : length of first token (without separator)
// @param restOfParameters : reference to rest of parameter string without first token and separator, if NULL then there is no rest of parameters and whole parameters string was processed
// @param separatorMustBeFound : specifies if separator must be found
// @return : true if successful, false otherwise
bool SplitBySeparator(const TCHAR *parameters, const TCHAR *separator, unsigned int *length, TCHAR **restOfParameters, bool separatorMustBeFound)
{
  bool result = false;

  if ((parameters != NULL) && (separator != NULL) && (length != NULL) && (restOfParameters))
  {
    unsigned int parameterLength = _tcslen(parameters);

    TCHAR *tempSeparator = NULL;
    TCHAR *tempParameters = (TCHAR *)parameters;
    while(true)
    {
      tempSeparator = (TCHAR *)_tcsstr(tempParameters, separator);
      if (tempSeparator == NULL)
      {
        // possible separator not found
        *length = _tcslen(parameters);
        *restOfParameters = NULL;
        result = !separatorMustBeFound;
        break;
      }
      else
      {
        // possible separator found - if after first separator is second separator, it's not separator (double separator represents separator character)
        // check next character if is it separator character - if yes, continue in cycle, if not than separator found

        if (_tcslen(tempSeparator) > 1)
        {
          // we are not on the last character
          // check next character if is it separator character
          tempParameters = tempSeparator + _tcslen(separator);
          if (_tcsncmp(tempParameters, separator, _tcslen(separator)) != 0)
          {
            // next character is not separator character
            // we found separator
            break;
          }
          else
          {
            // next character is separator character, skip
            tempParameters += _tcslen(separator);
          }
        }
        else
        {
          // we found separator
          break;
        }
      }
    }

    if (tempSeparator != NULL)
    {
      // we found separator
      // everything before separator is token, everything after separator is rest
      *length = parameterLength - _tcslen(tempSeparator);
      *restOfParameters = tempSeparator + _tcslen(separator);
      result = true;
    }
  }

  return result;
}

// replaces double separator character with one separator character
// returns size of memory block to fit result value, 0 if error
// [in] lpszValue - value with double separator character
// [in] lpszSeparator - separator string
// [out] lpszReplacedValue - pointer to buffer where result will be stored, if NULL result is ignored
// [in] replacedValueLength - size of lpszReplacedValue

// replace double separator character with one separator character
// @param value : value with double separator character
// @param separator : separator string
// @param replacedValue : reference to buffer where result will be stored, if NULL result is ignored
// @param replacedValueLength : the length of replaced value buffer
// @return : size of memory block to fit result value, UINT_MAX if error
unsigned int ReplaceDoubleSeparator(const TCHAR *value, const TCHAR *separator, TCHAR *replacedValue, unsigned int replacedValueLength)
{
  unsigned int requiredLength = UINT_MAX;
  // first count of replaced value length

  if ((value != NULL) && (separator != NULL))
  {
    requiredLength = 0;

    TCHAR *tempSeparator = NULL;
    TCHAR *tempValue = (TCHAR *)value;
    while(true)
    {
      unsigned int valueLength = _tcslen(tempValue);
      tempSeparator = (TCHAR *)_tcsstr(tempValue, separator);

      if (tempSeparator != NULL)
      {
        // possible separator found - if after first separator is second separator, it's not separator (double separator represents separator character)
        // check next character if is it separator character - if yes, skip, if not than separator found

        requiredLength += valueLength - _tcslen(tempSeparator);
        if (replacedValue != NULL)
        {
          _tcsncat_s(replacedValue, replacedValueLength, tempValue, valueLength - _tcslen(tempSeparator));
        }

        if (_tcslen(tempSeparator) > 1)
        {
          // we are not on the last character
          // check next character if is it separator character
          tempValue = tempSeparator + _tcslen(separator);
          if (_tcsncmp(tempValue, separator, _tcslen(separator)) == 0)
          {
            // next character is separator character, skip
            tempValue += _tcslen(separator);
            requiredLength++;
            if (replacedValue != NULL)
            {
              _tcsncat_s(replacedValue, replacedValueLength, separator, _tcslen(separator));
            }
          }
        }
      }
      else
      {
        requiredLength += valueLength;
        if (replacedValue != NULL)
        {
          _tcsncat_s(replacedValue, replacedValueLength, tempValue, valueLength);
        }
        break;
      }
    }
  }

  return requiredLength;
}

STDMETHODIMP CMPIPTVSource::SetConnectInfo(LPCOLESTR pszConnectInfo)
{
  HRESULT result = S_OK;

  logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME);
#ifdef _MBCS
  TCHAR *sParameters = ConvertToMultiByteW(pszConnectInfo);
#else
  TCHAR *sParameters = ConvertToUnicodeW(pszConnectInfo);
#endif

  result = (sParameters == NULL) ? E_FAIL : S_OK;

  if (result == S_OK)
  {
    logger.Log(LOGGER_INFO, _T("%s: %s: additional data: %s"), MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME, sParameters);

    // now we have unified string
    // let's parse

    this->m_parameters->Clear();

    bool splitted = false;
    unsigned int tokenLength = 0;
    TCHAR *rest = NULL;
    do
    {
      splitted = SplitBySeparator(sParameters, _T("|"), &tokenLength, &rest, false);
      if (splitted)
      {
        // token length is without terminating null character
        tokenLength++;
        ALLOC_MEM_DEFINE_SET(token, TCHAR, tokenLength, 0);
        if (token == NULL)
        {
          logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME, _T("not enough memory for token"));
          result = E_OUTOFMEMORY;
        }

        if (result == S_OK)
        {
          // copy token from parameters string
          _tcsncpy_s(token, tokenLength, sParameters, tokenLength - 1);
          sParameters = rest;

          unsigned int nameLength = 0;
          TCHAR *value = NULL;
          bool splittedNameAndValue = SplitBySeparator(token, _T("="), &nameLength, &value, true);

          if ((splittedNameAndValue) && (nameLength != 0))
          {
            // if correctly splitted parameter name and value
            nameLength++;
            ALLOC_MEM_DEFINE_SET(name, TCHAR, nameLength, 0);
            if (name == NULL)
            {
              logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME, _T("not enough memory for parameter name"));
              result = E_OUTOFMEMORY;
            }

            if (result == S_OK)
            {
              // copy name from token
              _tcsncpy_s(name, nameLength, token, nameLength - 1);

              // get length of value with replaced double separator
              unsigned int replacedLength = ReplaceDoubleSeparator(value, _T("|"), NULL, 0) + 1;

              ALLOC_MEM_DEFINE_SET(replacedValue, TCHAR, replacedLength, 0);
              if (replacedValue == NULL)
              {
                logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME, _T("not enough memory for replaced value"));
                result = E_OUTOFMEMORY;
              }

              if (result == S_OK)
              {
                ReplaceDoubleSeparator(value, _T("|"), replacedValue, replacedLength);

                CParameter *parameter = new CParameter(name, replacedValue);
                this->m_parameters->Add(parameter);
              }

              FREE_MEM(replacedValue);
            }

            FREE_MEM(name);
          }
        }

        FREE_MEM(token);
      }
    } while ((splitted) && (rest != NULL) && (result == S_OK));

    if (result == S_OK)
    {
      logger.Log(LOGGER_INFO, _T("%s: %s: count of parameters: %u"), MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME, this->m_parameters->Count());
      for(unsigned int i = 0; i < this->m_parameters->Count(); i++)
      {
        PCParameter parameter = this->m_parameters->GetParameter(i);
        logger.Log(LOGGER_INFO, _T("%s: %s: parameter name: %s, value: %s"), MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME, parameter->GetName(), parameter->GetValue());
      }
    }
  }

  FREE_MEM(sParameters);

  logger.Log(LOGGER_INFO, (result == S_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_SET_CONNECT_INFO_NAME);
  
  return S_OK;
}
