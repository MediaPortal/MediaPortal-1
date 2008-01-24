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
#include <windows.h>
#include "DvbUtil.h"

CDvbUtil::CDvbUtil(void)
{
}

CDvbUtil::~CDvbUtil(void)
{
}

void CDvbUtil::getString468A(BYTE *b, int maxLen,char *text)
{
	int i = 0;
	int num=0;
	unsigned char c;
	char em_ON = (char)0x86;
	char em_OFF = (char)0x87;

  if (maxLen< 1) return;
  if (text==NULL) return;
  if (b==NULL) return;

  int len=maxLen;
	do
	{
		c = (char)b[i];
	/*	if(c=='Ü')
		{
			int a=0;
		}*/
		if ( (((BYTE)c) >= 0x80) && (((BYTE)c) <= 0x9F))
		{
			goto cont;
		}
		if (i==0 && ((BYTE)c) < 0x20)
		{
			goto cont;
		}
				
		if (c == em_ON)
		{
			//					
			goto cont;
		}
		if (c == em_OFF)
		{
			//					
			goto cont;
		}
				
		if ( ((BYTE)c) == 0x84)
		{
      if (num >=maxLen) return;
			text[num] = '\r';
			text[num+1]=0;
			num++;
			goto cont;
		}
				
		if (((BYTE)c) < 0x20)
		{
      //0x1-0x5 = choose character set...
      if ((BYTE)c < 0x1 || (BYTE)c>0x5)
      {
			  goto cont;
      }
		}
				
      
    if (num >=maxLen) return;
		text[num] = c;
		text[num+1]=0;
		num++;
cont:
		len -= 1;
		i += 1;
	}while (!(len <= 0));

}
