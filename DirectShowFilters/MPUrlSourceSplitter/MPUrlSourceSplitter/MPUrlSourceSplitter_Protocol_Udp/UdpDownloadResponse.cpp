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

#include "UdpDownloadResponse.h"

CUdpDownloadResponse::CUdpDownloadResponse(void)
  : CDownloadResponse()
{
}

CUdpDownloadResponse::~CUdpDownloadResponse(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CUdpDownloadResponse *CUdpDownloadResponse::Clone(void)
{
  CUdpDownloadResponse *result = new CUdpDownloadResponse();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CUdpDownloadResponse::CloneInternal(CUdpDownloadResponse *clonedRequest)
{
  bool result = __super::CloneInternal(clonedRequest);

  if (result)
  {
  }

  return result;
}