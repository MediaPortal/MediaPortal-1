/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#define MAX_BUFFER_SIZE 0x10000
class CBuffer
{
public:
	CBuffer(void);
	~CBuffer(void);
	int		 Length();
	byte*  Data();
	void   Add(CBuffer* pBuffer);    
	double Pcr();
	double Pts();
	double Dts();
	void	 Set(double pcrTime, double ptsTime, double dtsTime,int length);
private:
	double m_pcrTime;
	double m_dtsTime;
	double m_ptsTime;
	byte* m_pBuffer;
	int   m_iLength;
};
