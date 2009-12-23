/*
 *	Copyright (C) 2006-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */


#include "StdAfx.h"

#include <xprtdefs.h>
#include <initguid.h>

#pragma once

// {558D9EA6-B177-4c30-9ED5-BF2D714BCBCA}
DEFINE_GUID(IID_IAudioStream, 
  0x558d9ea6, 0xb177, 0x4c30, 0x9e, 0xd5, 0xbf, 0x2d, 0x71, 0x4b, 0xcb, 0xca);



DECLARE_INTERFACE_( IAudioStream, IUnknown )
{    
  STDMETHOD(GetAudioStream)( __int32 &stream ) PURE;
};
