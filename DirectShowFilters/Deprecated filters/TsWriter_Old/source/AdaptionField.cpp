/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#pragma warning(disable : 4995)
#include <windows.h>
#include "adaptionField.h" 


#define ADAPTION_FIELD_LENGTH_OFFSET        0x4
#define PCR_FLAG_OFFSET                     0x5

#define DISCONTINUITY_FLAG_BIT              0x80
#define RANDOM_ACCESS_FLAG_BIT              0x40
#define ES_PRIORITY_FLAG_BIT                0x20
#define PCR_FLAG_BIT                        0x10
#define OPCR_FLAG_BIT                       0x8
#define SPLICING_FLAG_BIT                   0x4
#define TRANSPORT_PRIVATE_DATA_FLAG_BIT     0x2
#define ADAPTION_FIELD_EXTENSION_FLAG_BIT   0x1

extern void LogDebug(const char *fmt, ...) ;



CAdaptionField::CAdaptionField()
{
}
CAdaptionField::CAdaptionField(CTsHeader& header, byte* tsPacket)
{
	Decode(header,tsPacket);
}

CAdaptionField::~CAdaptionField(void)
{
}

void CAdaptionField::Decode(CTsHeader& header,byte* tsPacket)
{
  if (header.PayLoadOnly() || tsPacket[ADAPTION_FIELD_LENGTH_OFFSET] < 7) 
  {
     DiscontinuityIndicator=false;
     RandomAccessInidicator=false;
     ElementaryStreamPriorityIndicator=false;
     PcrFlag=false;
     OPcrFlag=false;
     SplicingPointFlag=false;
     TransportPrivateDataFlag=false;
     AdaptionFieldExtensionFlag=false;
     Pcr.Reset();
     OPcr.Reset();
     return;
  }

  DiscontinuityIndicator             = ((tsPacket[PCR_FLAG_OFFSET] & DISCONTINUITY_FLAG_BIT)!=0);
  RandomAccessInidicator             = ((tsPacket[PCR_FLAG_OFFSET] & RANDOM_ACCESS_FLAG_BIT)!=0);
  ElementaryStreamPriorityIndicator  = ((tsPacket[PCR_FLAG_OFFSET] & ES_PRIORITY_FLAG_BIT)!=0);
  PcrFlag                            = ((tsPacket[PCR_FLAG_OFFSET] & PCR_FLAG_BIT)!=0);
  OPcrFlag                           = ((tsPacket[PCR_FLAG_OFFSET] & OPCR_FLAG_BIT)!=0);
  SplicingPointFlag                  = ((tsPacket[PCR_FLAG_OFFSET] & SPLICING_FLAG_BIT)!=0);
  TransportPrivateDataFlag           = ((tsPacket[PCR_FLAG_OFFSET] & TRANSPORT_PRIVATE_DATA_FLAG_BIT)!=0);
  AdaptionFieldExtensionFlag         = ((tsPacket[PCR_FLAG_OFFSET] & ADAPTION_FIELD_EXTENSION_FLAG_BIT)!=0);
  
  int offset=6;
  if (PcrFlag)
  {
    Pcr.Decode(&tsPacket[offset]);
    offset+=6;
  }
  if (OPcrFlag)
  {
    OPcr.Decode(&tsPacket[offset]);
    offset+=6;
  }
  if (SplicingPointFlag)
  {
    SpliceCountDown=tsPacket[offset];
    offset++;
  }

  if (TransportPrivateDataFlag)
  {
    // transport private data length      : 8 bits
    // for (int i=0; i < transport private data length; ++i)
    // {
    //   private data                     : 8 bits
    // }
  }

  if (AdaptionFieldExtensionFlag)
  {
    // adaption field extension length : 8 bits
    // ltw_flag                        : 1 bit
    // PieceWiseRate_flag              : 1 bit
    // SeamlessSplice_flag             : 1 bit
    // reserved                        : 5 bits
    // if (ltw_flag)
    // {
    //   ltw_valid_flag                : 1 bit
    //   ltw_offset                    : 15 bits
    // }
    // if (PieceWiseRate_flag)
    // {
    //   reserved                      : 2 bit
    //   piecewaterate                 : 22 bits
    // }
    // if (SeamlessSplice_flag)
    // {
    //   splicetype                    : 4 bit
    //   DTS_next_AU[32..0]            : 3 bits
    //   marker_bit                    : 1 bit
    //   DTS_next_AU[29..15]           : 15 bits
    //   marker_bit                    : 1 bit
    //   DTS_next_AU[14..0]            : 15 bits
    //   marker_bit                    : 1 bit
    // }
    // for (int i=0; i < N; ++i)
    // {
    //   reserved                      : 8 bits
    // }
  }
}
