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
#pragma once

#define MAX_PCR (UINT64)0x1FFFFFFFF 

class CPcr
{
public:
	CPcr();
  CPcr(const CPcr& pcr);
	virtual ~CPcr(void);
  void   Reset();
  void   Decode(byte* data);
  static bool DecodeFromPesHeader(byte* pesHeader,int payloadStart,CPcr& pts, CPcr& dts);
  void   FromClock(double clock);
  double ToClock() const;
  void   Time(int& day, int& hour, int &minutes, int& seconds, int & millsecs);
  char*  ToString();
  
  CPcr& operator+=(const CPcr &rhs);
  CPcr& operator-=(const CPcr &rhs); 
  CPcr operator+(const CPcr &rhs); 
  CPcr operator-(const CPcr &rhs);
  CPcr& operator=(const CPcr &rhs);
  bool operator==(const CPcr &other) const ;
  bool operator>(const CPcr &other) const ;
  bool operator!=(const CPcr &other) const ;

  UINT64 PcrReferenceBase;
  UINT64 PcrReferenceExtension;
	bool   IsValid;
private:
  char m_buffer[100];
};
