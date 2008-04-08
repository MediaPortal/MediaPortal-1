#include <windows.h>
#include "DvbUtil.h"

CDvbUtil::CDvbUtil(void)
{
}

CDvbUtil::~CDvbUtil(void)
{
}

void CDvbUtil::getString468A(BYTE *b, int l1,char *text)
{
	int i = 0;
	int num=0;
	unsigned char c;
	char em_ON = (char)0x86;
	char em_OFF = (char)0x87;
	
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
			text[num] = '\r';
			text[num+1]=0;
			num++;
			goto cont;
		}
				
		if (((BYTE)c) < 0x20)
		{
			goto cont;
		}
				
		text[num] = c;
		text[num+1]=0;
		num++;
cont:
		l1 -= 1;
		i += 1;
	}while (!(l1 <= 0));

}