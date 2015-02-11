/*
 *  Copyright (C) 2015 Team MediaPortal
 *  http://www.team-mediaportal.com
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
 
//  Derived from 'ccextractor' code, credits below:
//  ===============================================
//  ccextractor, 0.75
//  -----------------
//  Authors: Carlos Fernández (cfsmp3), Volker Quetschke.
//  Maintainer: cfsmp3
//  
//  Lots of credit goes to other people, though:
//  McPoodle (author of the original SCC_RIP), Neuron2, 
//  and others (see source code).
//  
//  Home: http://www.ccextractor.org
//  
//  Google Summer of Code 2014 students
//  - Willem van iseghem
//  - Ruslan KuchumoV
//  - Anshul Maheshwari
//  -----------------------------------------------

#include "StdAfx.h"

#include "CcParseH264.h"

// uncomment the //LogDebug to enable extra logging
#define LOG_DETAIL //LogDebug

extern void LogDebug(const char *fmt, ...) ;

CcParseH264::CcParseH264()
{
  cc_data = (unsigned char*)malloc(1024);
  cc_databufsize = 1024;
  ccblocks_in_avc_total=0;
  ccblocks_in_avc_lost=0;
  num_unexpected_sei_length=0;
  cc_count = 0;
  cc_bytes_in_buffer = 0;
}

CcParseH264::~CcParseH264()
{
  if (cc_data)
  {
    free(cc_data);
    cc_data = NULL;
  }
}

// Functions to parse a AVC/H.264 data stream, see ISO/IEC 14496-10

DWORD CcParseH264::parseAVC1sample (const BYTE* pData, DWORD sampleLength, DWORD dwFlags)
{
  DWORD dwNalLength;
  DWORD startLength = sampleLength;
  unsigned char *pSample = (unsigned char *) pData;

  //reset output buffer
  cc_count = 0;
  cc_bytes_in_buffer = 0;
  
  while (sampleLength >= dwFlags)
  {  
    switch (dwFlags)
    {
      case 1:
        dwNalLength = (DWORD)*pSample;
        break;
      case 2:
        dwNalLength = (DWORD)(_byteswap_ushort(*(WORD *)pSample));  //The length field is big-endian format
        break;
      case 4:
        dwNalLength = _byteswap_ulong(*(DWORD *)pSample);  //The length field is big-endian format
        break;
  		default:
  			LogDebug ("CcParseH264: FATAL - parseAVC1sample() - invalid dwFlags: %d", dwFlags);
  			return cc_bytes_in_buffer;
    }

    pSample += dwFlags;
    sampleLength -= dwFlags;
        
    if (sampleLength < dwNalLength)
    {
			LogDebug ("CcParseH264: FATAL - parseAVC1sample() - dwNalLength error: %d", sampleLength - dwNalLength);
  	  return cc_bytes_in_buffer;
    }
      
    if ( (*pSample & 0x1F) == 0x06 )  //SEI data nal_unit_type
    {
      // Found SEI (used for subtitles) - lets process it
			LOG_DETAIL ("CcParseH264: parseAVC1sample() - found SEI, sampleLength %d, dwNalLength %d, startLength %d", sampleLength, dwNalLength, startLength);

			//Need to copy buffer before processing because EBSPtoRBSP() can modify the contents.... 
      unsigned char *pSampleBuffer = new unsigned char[dwNalLength-1];      
      memcpy(pSampleBuffer,pSample+1,dwNalLength-1);
      sei_rbsp(pSampleBuffer, dwNalLength-1);
      delete[] pSampleBuffer;
    }
    
    sampleLength -= dwNalLength;
    pSample += dwNalLength;
    
  }
  
  return cc_bytes_in_buffer;
}

void CcParseH264::do_NAL (unsigned char *NALstart, int Length)
{
	unsigned nal_unit_type = *NALstart & 0x1F;

  if ( nal_unit_type == 0x06 )  //SEI data
  {
    // Found SEI (used for subtitles)
    sei_rbsp(NALstart+1, Length-1);
  }
}


#define ZEROBYTES_SHORTSTARTCODE 2

// Remove 'emulation_prevention_three_byte' extra bytes from NAL
int CcParseH264::EBSPtoRBSP(unsigned char *streamBuffer, int end_bytepos, int begin_bytepos)
{
  int i, j, count;
  count = 0;

  if(end_bytepos < begin_bytepos)
    return end_bytepos;

  j = begin_bytepos;

  for(i = begin_bytepos; i < end_bytepos; ++i)
  { //starting from begin_bytepos to avoid header information
    //in NAL unit, 0x000000, 0x000001 or 0x000002 shall not occur at any byte-aligned position
    if(count == ZEROBYTES_SHORTSTARTCODE && *(streamBuffer+i) < 0x03)
    {
	    LogDebug ("CcParseH264: EBSPtoRBSP() fail -1, i %d, streamBuffer[i] %d", i, *(streamBuffer+i));
      return -1;
    }
    if(count == ZEROBYTES_SHORTSTARTCODE && *(streamBuffer+i) == 0x03)
    {
	    //LogDebug ("CcParseH264: EBSPtoRBSP() 0x03 found, i %d", i);
      //check the 4th byte after 0x000003, except when cabac_zero_word is used, in which case the last three bytes of this NAL unit must be 0x000003
      if((i < end_bytepos-1) && (*(streamBuffer+i+1) > 0x03))
      {
  	    LogDebug ("CcParseH264: EBSPtoRBSP() fail -2, i %d, streamBuffer[i+1] %d, end_bytepos-1 %d", i, streamBuffer[i+1], end_bytepos-1);
        return -2;
      }
      //if cabac_zero_word is used, the final byte of this NAL unit(0x03) is discarded, and the last two bytes of RBSP must be 0x0000
      if(i == end_bytepos-1)
        return j;

      ++i;
      count = 0;
    }
    *(streamBuffer+j) =  *(streamBuffer+i);
    if(*(streamBuffer+i) == 0x00)
      ++count;
    else
      count = 0;
    ++j;
  }

  return j;
}


// Process SEI payload in AVC data
void CcParseH264::sei_rbsp (unsigned char *seibuf, int Length)
{
  unsigned char *tbuf = seibuf;
  
  // Remove 'emulation_prevention_three_byte' added bytes	and calculate new 'end' pointer
  int newsize = EBSPtoRBSP (tbuf, Length, 0);
	if (newsize < 0) // broken NAL....
	{
		LogDebug ("CcParseH264: sei_rbsp() - EBSPtoRBSP fail, discarding NAL, retval: %d", newsize);
		return;
  }	
  unsigned char *seiend = seibuf + newsize;

  while(tbuf < seiend - 1) // Use -1 because of trailing marker
  {
    tbuf = sei_message(tbuf, seiend - 1);
  }
  
  if(tbuf == seiend - 1 )
  {
    if(*tbuf != 0x80)
        LogDebug("CcParseH264: sei_rbsp() - Strange rbsp_trailing_bits value: %02X",*tbuf);
  }
  else
	{
		// This really really looks bad
		LogDebug ("CcParseH264: sei_rbsp() - WARNING: Unexpected SEI unit length...trying to continue. seibuf: %d, seiend: %d, Length: %d", seibuf, seiend, Length);

		num_unexpected_sei_length++;
	}
}


// This combines sei_message() and sei_payload().
unsigned char *CcParseH264::sei_message (unsigned char *seibuf, unsigned char *seiend)
{
  int payloadType = 0;
	while (*seibuf==0xff)
	{
		payloadType+=255;
		seibuf++;
	}
  payloadType += *seibuf;
	seibuf++;

  int payloadSize = 0;
	while (*seibuf==0xff)
	{
		payloadSize+=255;
		seibuf++;
	}
  payloadSize += *seibuf;
	seibuf++;

	int broken=0;
  unsigned char *paystart = seibuf;
  seibuf+=payloadSize;

	if(seibuf > seiend )
	{
		// TODO: What do we do here?
		broken=1;
		if (payloadSize == 0)  //Probably due to leading zero of (next) four byte NAL start code, following 0x80 trailing byte of this SEI
		{
		  *(seibuf-1) = 0x80; //trailing zero byte is spurious - replace with 0x80 and adjust pointer to keep caller happy...
      return seibuf-1;
		}
		else if (payloadType==4)
		{
			LogDebug ("CcParseH264: sei_message() - Warning: Subtitle payload seems incorrect (too long)- Payload type: %d size: %d - seibuf: %d, seiend: %d", payloadType, payloadSize, seibuf, seiend);
		}
		else
		{
			LogDebug ("CcParseH264: sei_message() - Warning: Non-subtitle payload seems incorrect (too long)- Payload type: %d size: %d - seibuf: %d, seiend: %d", payloadType, payloadSize, seibuf, seiend);
		}
	}
	
  // Ignore all except user_data_registered_itu_t_t35() payload
  if(!broken && payloadType == 4)
  {
    user_data_registered_itu_t_t35(paystart, paystart+payloadSize);
  }

  return seibuf;
}

void CcParseH264::copy_ccdata_to_buffer (unsigned char *source, int new_cc_count)
{
	ccblocks_in_avc_total++;
	//Check space and increase buffer size if possible
	DWORD newbuffsize = cc_bytes_in_buffer+sizeof(DWORD)+sizeof(DWORD)+3+(new_cc_count*3)+1;
  if (newbuffsize > cc_databufsize)
	{
	  if (newbuffsize > 65536)
		{
			LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Max buffer size exceeded");
			ccblocks_in_avc_lost++;
			return;
	  }						  
		cc_data = (unsigned char*)realloc(cc_data, (size_t) newbuffsize);
		if (!cc_data)
		{
			LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Out of memory");
			ccblocks_in_avc_lost++;
			return;
	  }
		cc_databufsize = (long) newbuffsize;
	}
	
	if (new_cc_count > 31)
	{
		LogDebug ("CcParseH264: copy_ccdata_to_buffer() - cc_count overflow: %d", new_cc_count);
		ccblocks_in_avc_lost++;
		return;
	}
	
	//Add header data
	*(UNALIGNED DWORD*)(cc_data+cc_bytes_in_buffer) = (new_cc_count*3)+8; //length of CC packet
	cc_bytes_in_buffer += sizeof(DWORD);	
	*(UNALIGNED DWORD*)(cc_data+cc_bytes_in_buffer) = 0x34394147; //user_identifier "GA94"
	cc_bytes_in_buffer += sizeof(DWORD);
	*(cc_data+(cc_bytes_in_buffer++)) = 0x03; //user_data_type_code
	*(cc_data+(cc_bytes_in_buffer++)) = 0x40 | (new_cc_count & 0x1F); //flags and cc_count
	*(cc_data+(cc_bytes_in_buffer++)) = 0; //em_data
	
	//Copy in CC data
	memcpy(cc_data+cc_bytes_in_buffer, source, (new_cc_count*3)+1); //cc_data_pkt's + marker_bits
	
  //	for (int i = 0; i < (new_cc_count*3); i+=3)
  //	{
  //		LogDebug ("Dump CC - Triplet %d, Hex: 0x%x 0x%x 0x%x, ASCII: %c %c", i/3,  
  //		          *(unsigned char*)(source+i+0), *(unsigned char*)(source+i+1), *(unsigned char*)(source+i+2), 
  //		          (0x7F & *(unsigned char*)(source+i+1)), (0x7F & *(unsigned char*)(source+i+2)));
  //	}
	
	cc_count+=new_cc_count;
	cc_bytes_in_buffer += (new_cc_count*3)+1;
}

BYTE* CcParseH264::get_cc_buffer_pointer()
{
  return (BYTE*)cc_data;
}



void CcParseH264::user_data_registered_itu_t_t35 (unsigned char *userbuf, unsigned char *userend)
{
  unsigned char *tbuf = userbuf;
	unsigned char *cc_tmpdata;
	unsigned char process_cc_data_flag;
	int user_data_type_code;
	int user_data_len;
	int local_cc_count=0;
	// int cc_count;
  int itu_t_t35_country_code = *((byte*)tbuf);
  tbuf++;
  int itu_t_35_provider_code = *tbuf * 256 + *(tbuf+1);
  tbuf+=2;

  // ANSI/SCTE 128 2008:
  // itu_t_t35_country_code == 0xB5
  // itu_t_35_provider_code == 0x0031
  // see spec for details - no example -> no support

  // Example files (sample.ts, ...):
  // itu_t_t35_country_code == 0xB5
  // itu_t_35_provider_code == 0x002F
  // user_data_type_code == 0x03 (cc_data)
  // user_data_len == next byte (length after this byte up to (incl) marker.)
  // cc_data struct (CEA-708)
  // marker == 0xFF

  if(itu_t_t35_country_code != 0xB5)
  {
    LogDebug ("CcParseH264: udr_itu_t_t35() - Not a supported user data SEI, itu_t_35_country_code: %02x", itu_t_t35_country_code);
    return;
  }

	switch (itu_t_35_provider_code)
	{
		case 0x0031: // ANSI/SCTE 128
			LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - Caption block in ANSI/SCTE 128 - provider_code = 0x0031");
			if (*tbuf==0x47 && *(tbuf+1)==0x41 && *(tbuf+2)==0x39 && *(tbuf+3)==0x34) // ATSC1_data() - GA94
			{
				LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - ATSC1_data()...");
				tbuf+=4;
				unsigned char user_data_type_code=*tbuf;
				tbuf++;
				switch (user_data_type_code)
				{
					case 0x03:
						LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - cc_data (finally)!");

						local_cc_count= *tbuf & 0x1F;
						process_cc_data_flag = (*tbuf & 0x40) >> 6;
						
						tbuf++;
												
						// OK, all checks passed!
						tbuf++;
						cc_tmpdata = tbuf;

						// Enough room for CC captions?
						if (cc_tmpdata+local_cc_count*3 >= userend)
						{
							LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Syntax problem: Too many caption blocks.");
							return;
						}
						if (cc_tmpdata[local_cc_count*3]!=0xFF)
						{
							LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Syntax problem: Final 0xFF marker missing.");
							return;
						}

						// Save the data and process once we know the sequence number
						// Copy new cc data into cc_data
			      LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - copy new 'code 49' CC data into cc_data, length: %d", local_cc_count);
						copy_ccdata_to_buffer (cc_tmpdata, local_cc_count);
						break;
					case 0x06:
						LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - bar_data (unsupported for now)");
						break;
					default:
						break;
						LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - SCTE/ATSC reserved.");
				}

			}
			else if (*tbuf==0x44 && *(tbuf+1)==0x54 && *(tbuf+2)==0x47 && *(tbuf+3)==0x31) // afd_data() - DTG1
			{
				;
				// Active Format Description Data. Actually unrelated to captions. Left
				// here in case we want to do some reporting eventually. From specs:
				// "Active Format Description (AFD) should be included in video user
				// data whenever the rectangular picture area containing useful
				// information does not extend to the full height or width of the coded
				// frame. AFD data may also be included in user data when the
				// rectangular picture area containing
				// useful information extends to the fullheight and width of the
				// coded frame."
			}
			else
			{
				LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - SCTE/ATSC reserved.");
			}
			break;
		case 0x002F:
      LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - ATSC1_data() - provider_code = 0x002F");

			user_data_type_code = *((byte*)tbuf);
			if(user_data_type_code != 0x03)
			{
				LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - Not supported  user_data_type_code: %02x",
				   user_data_type_code);
				return;
			}
			tbuf++;
			user_data_len = *((byte*)tbuf);
			tbuf++;

			local_cc_count = *tbuf & 0x1F;
			process_cc_data_flag = (*tbuf & 0x40) >> 6;
			if (!process_cc_data_flag)
      {
          LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - process_cc_data_flag == 0, skipping this caption block.");
          break;
      }
			cc_tmpdata = tbuf+2;

			if (local_cc_count*3+3 != user_data_len)
			{
				LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Syntax problem: user_data_len != cc_count*3+3.");
				return;
			}

			// Enough room for CC captions?
			if (cc_tmpdata+local_cc_count*3 >= userend)
			{
				LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Syntax problem: Too many caption blocks.");
				return;
			}
			if (cc_tmpdata[local_cc_count*3]!=0xFF)
			{
				LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Syntax problem: Final 0xFF marker missing.");
				return;
		  }

			// Save the data and process once we know the sequence number
      
      // Copy new cc data into cc_data - replace command below.
			LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - copy new 'code 47' CC data into cc_data, length: %d", local_cc_count);
      copy_ccdata_to_buffer (cc_tmpdata, local_cc_count);

			//dump(tbuf,user_data_len-1,0);
			break;
		default:
			LOG_DETAIL ("CcParseH264: udr_itu_t_t35() - Not a supported user data SEI, itu_t_35_provider_code: %04x", itu_t_35_provider_code);
			break;
	}
}

