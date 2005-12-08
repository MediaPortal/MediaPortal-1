#include <stdio.h>

#include "autostring.h"
CAutoString::CAutoString(int len)
{
	m_pBuffer = new char[len];
}
CAutoString::~CAutoString()
{
	delete [] m_pBuffer;
	m_pBuffer=NULL;
}

char* CAutoString::GetBuffer() 
{
	return m_pBuffer;
}
