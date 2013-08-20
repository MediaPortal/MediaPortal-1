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

#include "Dns.h"

CDns::CDns(void)
{
}

CDns::~CDns(void)
{
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CDns::GetIpAddresses(const wchar_t *hostName, WORD port, int family, int type, int protocol, unsigned int flags, CIpAddressCollection *collection)
{
  HRESULT result = S_OK;

  CHECK_POINTER_DEFAULT_HRESULT(result, hostName);
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  if (SUCCEEDED(result))
  {
    ADDRINFOW hints;
    ADDRINFOW *addresses = NULL;

    wchar_t *portStr = FormatString(L"%d", port);
    CHECK_POINTER_HRESULT(result, portStr, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // setup the hints address info structure
      // which is passed to the getaddrinfo() function
      ZeroMemory(&hints, sizeof(ADDRINFOW));

      const int safe_flags =
        AI_PASSIVE |
        AI_CANONNAME |
        AI_NUMERICHOST |
        AI_NUMERICSERV |
#ifdef AI_ALL
        AI_ALL |
#endif
#ifdef AI_ADDRCONFIG
        AI_ADDRCONFIG |
#endif
#ifdef AI_V4MAPPED
        AI_V4MAPPED |
#endif
        0;

      hints.ai_family = family;
      hints.ai_socktype = type;
      hints.ai_protocol = protocol;

      // unfortunately, some flags change the layout of struct addrinfo, so
      // they cannot be copied blindly from p_hints to &hints. Therefore, we
      // only copy flags that we know for sure are "safe".
      hints.ai_flags = flags & safe_flags;

      // we only ever use port *numbers*
      hints.ai_flags |= AI_NUMERICSERV;

      hints.ai_flags |= AI_NUMERICHOST;
      result = HRESULT_FROM_WIN32(GetAddrInfo(hostName, portStr, &hints, &addresses));

      if (FAILED(result))
      {
        hints.ai_flags &= ~AI_NUMERICHOST;
        result = HRESULT_FROM_WIN32(GetAddrInfo(hostName, portStr, &hints, &addresses));
      }

      if (SUCCEEDED(result))
      {
        const wchar_t *canonicalName = (addresses != NULL) ? addresses->ai_canonname : NULL;
        for (ADDRINFOW *address = addresses; (SUCCEEDED(result) && (address != NULL)); address = address->ai_next)
        {
          CIpAddress *ipAddress = new CIpAddress(address, canonicalName);
          CHECK_POINTER_HRESULT(result, ipAddress, result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = collection->Add(ipAddress) ? result : E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(ipAddress));
        }
      }

      FreeAddrInfo(addresses);
      addresses = NULL;
    }
    FREE_MEM(portStr);
  }

  return result;
}