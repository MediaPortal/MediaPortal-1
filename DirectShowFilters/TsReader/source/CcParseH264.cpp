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


#include "CcParseH264.h"

extern void LogDebug(const char *fmt, ...) ;

CcParseH264::CcParseH264()
{
  cc_data = (unsigned char*)malloc(1024);
  ccblocks_in_avc_total=0;
  ccblocks_in_avc_lost=0;
  num_unexpected_sei_length=0;
  cc_count = 0;
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

void CcParseH264::do_NAL (unsigned char *NALstart, int Length)
{
	unsigned nal_unit_type = *NALstart & 0x1F;

  if ( nal_unit_type == 0x06 )  //SEI data
  {
    // Found SEI (used for subtitles)
    sei_rbsp(NALstart+1, Length-1);
  }
}


// Remove 'emulation_prevention_three_byte' extra bytes from NAL
unsigned char * CcParseH264::remove_03emu(unsigned char *from, unsigned char *to)
{
	int num=to-from;
	int newsize = EBSPtoRBSP (from,num,0);
	if (newsize==-1) // broken NAL....
	{
		return NULL;
  }
  return from+newsize;
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
    if(count == ZEROBYTES_SHORTSTARTCODE && streamBuffer[i] < 0x03)
      return -1;
    if(count == ZEROBYTES_SHORTSTARTCODE && streamBuffer[i] == 0x03)
    {
      //check the 4th byte after 0x000003, except when cabac_zero_word is used, in which case the last three bytes of this NAL unit must be 0x000003
      if((i < end_bytepos-1) && (streamBuffer[i+1] > 0x03))
        return -1;
      //if cabac_zero_word is used, the final byte of this NAL unit(0x03) is discarded, and the last two bytes of RBSP must be 0x0000
      if(i == end_bytepos-1)
        return j;

      ++i;
      count = 0;
    }
    streamBuffer[j] = streamBuffer[i];
    if(streamBuffer[i] == 0x00)
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
  unsigned char *seiend = seibuf + Length;
	//LogDebug ("CcParseH264: sei_rbsp() - Start1, seibuf: %d, seiend: %d, Length: %d, endbyte: %x", seibuf, seiend, Length, *(seiend-1));
	seiend = remove_03emu(seibuf, seiend); // Remove 'emulation_prevention_three_byte' added bytes
	//LogDebug ("CcParseH264: sei_rbsp() - Start2, seibuf: %d, seiend: %d, endbyte: %x", seibuf, seiend, *(seiend-1));

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
		if (payloadType==4)
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

void CcParseH264::copy_ccdata_to_buffer (char *source, int new_cc_count)
{
	ccblocks_in_avc_total++;
	if (cc_buffer_saved==0)
	{
		LogDebug ("CcParseH264: copy_ccdata_to_buffer() - Warning: Probably loss of CC data, unsaved buffer being rewritten");
		ccblocks_in_avc_lost++;
	}
	memcpy(cc_data+cc_count*3, source, new_cc_count*3+1);
	cc_count+=new_cc_count;
	cc_buffer_saved=0;
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
			LogDebug ("CcParseH264: udr_itu_t_t35() - Caption block in ANSI/SCTE 128...");
			if (*tbuf==0x47 && *(tbuf+1)==0x41 && *(tbuf+2)==0x39 && *(tbuf+3)==0x34) // ATSC1_data() - GA94
			{
				LogDebug ("CcParseH264: udr_itu_t_t35() - ATSC1_data()...");
				tbuf+=4;
				unsigned char user_data_type_code=*tbuf;
				tbuf++;
				switch (user_data_type_code)
				{
					case 0x03:
						LogDebug ("CcParseH264: udr_itu_t_t35() - cc_data (finally)!");

//						cc_count = 2; // Forced test
//						process_cc_data_flag = (*tbuf & 2) >> 1;
//						mandatory_1 = (*tbuf & 1);
//						mandatory_0 = (*tbuf & 4) >>2;
//						if (!mandatory_1 || mandatory_0)
//						{
//							printf ("Essential tests not passed.");
//							break;
//						}

						local_cc_count= *tbuf & 0x1F;
						process_cc_data_flag = (*tbuf & 0x40) >> 6;

//						if (!process_cc_data_flag)
//						{
//							LogDebug ("CcParseH264: udr_itu_t_t35() - process_cc_data_flag == 0, skipping this caption block.");
//							break;
//						}

//						The following tests are not passed in Comcast's sample videos. *tbuf here is always 0x41.
//						if (! (*tbuf & 0x80)) // First bit must be 1
//						{
//							LogDebug ("CcParseH264: udr_itu_t_t35() - Fixed bit should be 1, but it's 0 - skipping this caption block.");
//							break;
//						}
//						if (*tbuf & 0x20) // Third bit must be 0
//						{
//							LogDebug ("CcParseH264: udr_itu_t_t35() - Fixed bit should be 0, but it's 1 - skipping this caption block.");
//							break;
//						}
						
						tbuf++;
						
//						Another test that the samples ignore. They contain 00!
//						if (*tbuf!=0xFF)
//						{
//							LogDebug ("CcParseH264: udr_itu_t_t35() - Fixed value should be 0xFF, but it's %02X - skipping this caption block.", *tbuf);
//						}
						
						// OK, all checks passed!
						tbuf++;
						cc_tmpdata = tbuf;

//						//TODO: I don't think we have user_data_len here
//						if (cc_count*3+3 != user_data_len)
//						fatal(CCX_COMMON_EXIT_BUG_BUG,
//							"Syntax problem: user_data_len != cc_count*3+3.");

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
						if (local_cc_count*3+1 > cc_databufsize)
						{
							cc_data = (unsigned char*)realloc(cc_data, (size_t) cc_count*6+1);
							if (!cc_data)
							{
								LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Out of memory");
								return;
						  }
							cc_databufsize = (long) cc_count*6+1;
						}
						// Copy new cc data into cc_data
			      LogDebug ("CcParseH264: udr_itu_t_t35() - copy new 'code 49' CC data into cc_data, length: %d", local_cc_count);
						copy_ccdata_to_buffer ((char *) cc_tmpdata, local_cc_count);
						break;
					case 0x06:
						LogDebug ("CcParseH264: udr_itu_t_t35() - bar_data (unsupported for now)");
						break;
					default:
						LogDebug ("CcParseH264: udr_itu_t_t35() - SCTE/ATSC reserved.");
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
				LogDebug ("CcParseH264: udr_itu_t_t35() - SCTE/ATSC reserved.");
			}
			break;
		case 0x002F:
      LogDebug ("CcParseH264: udr_itu_t_t35() - ATSC1_data() - provider_code = 0x002F");

			user_data_type_code = *((byte*)tbuf);
			if(user_data_type_code != 0x03)
			{
				LogDebug ("CcParseH264: udr_itu_t_t35() - Not supported  user_data_type_code: %02x\n",
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
          LogDebug ("CcParseH264: udr_itu_t_t35() - process_cc_data_flag == 0, skipping this caption block.");
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
			if (cc_count*3+1 > cc_databufsize)
			{
				cc_data = (unsigned char*)realloc(cc_data, (size_t) cc_count*6+1);
				if (!cc_data)
				{
					LogDebug ("CcParseH264: udr_itu_t_t35() - FATAL - Out of memory");
					return;
			  }
				cc_databufsize = (long) cc_count*6+1;
			}
      
      // Copy new cc data into cc_data - replace command below.
			LogDebug ("CcParseH264: udr_itu_t_t35() - copy new 'code 47' CC data into cc_data, length: %d", local_cc_count);
      copy_ccdata_to_buffer ((char *) cc_tmpdata, local_cc_count);

			//dump(tbuf,user_data_len-1,0);
			break;
		default:
			LogDebug ("CcParseH264: udr_itu_t_t35() - Not a supported user data SEI, itu_t_35_provider_code: %04x\n", itu_t_35_provider_code);
			break;
	}
}

