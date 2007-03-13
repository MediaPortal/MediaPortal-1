/**
*  Global.h
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/
#ifndef GLOBAL_H
#define GLOBAL_H

#ifndef ABOVE_NORMAL_PRIORITY_CLASS
#define ABOVE_NORMAL_PRIORITY_CLASS 0x00008000
#endif

#ifndef BELOW_NORMAL_PRIORITY_CLASS
#define BELOW_NORMAL_PRIORITY_CLASS 0x00004000
#endif

#ifndef FILE_START_POS_TS
#define FILE_START_POS_TS 500000 //Minimum file position to begin parsing for TS Mode
#endif

#ifndef RT_FILE_START_TS
#define RT_FILE_START_TS 1000000 //0.1sec stream position to begin playing in TS Mode
#endif

#ifndef FILE_START_POS_PS
#define FILE_START_POS_PS 1 //Minimum file position to begin parsing for PS Mode
#endif

#ifndef RT_FILE_START_PS
#define RT_FILE_START_PS 1 //0sec stream position to begin playing in PS Mode
#endif

#ifndef MIN_FILE_SIZE
#define MIN_FILE_SIZE 2000000 //Minimum filesize to parse
#endif

#ifndef RT_SECOND
#define RT_SECOND 10000000 //1sec
#endif

#ifndef RT_2_SECOND
#define RT_2_SECOND 20000000 //2 sec
#endif

/**********************************************
 *
 *  CBufferInfo Class
 *
 **********************************************/

class CBufferInfo
{
public:
	CBufferInfo(){};
	virtual ~CBufferInfo(){};

	BYTE *sample;
	long size;
};

class BoostThread
{
public:
	BoostThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				m_nPriority = GetThreadPriority(GetCurrentThread());
				SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL - 1);
		   }
		#endif
   }

   ~BoostThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				SetThreadPriority(GetCurrentThread(), m_nPriority);
		   }
		#endif
   }
     
   int m_nPriority;
};

class LowBoostThread
{
public:
   LowBoostThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				m_nPriority = GetThreadPriority(GetCurrentThread());
				SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL - 2);
		   }
		#endif
   }

   ~LowBoostThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				SetThreadPriority(GetCurrentThread(), m_nPriority);
		   }
		#endif
   }
     
   int m_nPriority;
};

class HighestThread
{
public:
   HighestThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				m_nPriority = GetThreadPriority(GetCurrentThread());
				SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_HIGHEST);
		   }
		#endif
   }

   ~HighestThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				SetThreadPriority(GetCurrentThread(), m_nPriority);
		   }
		#endif
   }
      
   int m_nPriority;
};

class AbnormalThread
{
public:
   AbnormalThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				m_nPriority = GetThreadPriority(GetCurrentThread());
				SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_ABOVE_NORMAL);
		   }
		#endif
   }

   ~AbnormalThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				SetThreadPriority(GetCurrentThread(), m_nPriority);
		   }
		#endif
   }
      
   int m_nPriority;
};

class NormalThread
{
public:
   NormalThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				m_nPriority = GetThreadPriority(GetCurrentThread());
				SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_NORMAL);
		   }
		#endif
   }

   ~NormalThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				SetThreadPriority(GetCurrentThread(), m_nPriority);
		   }
		#endif
   }
      
   int m_nPriority;
};

class BrakeThread
{
public:
   BrakeThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				m_nPriority = GetThreadPriority(GetCurrentThread());
				SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_LOWEST);
		   }
		#endif
   }

   ~BrakeThread()
   {
		#ifndef DEBUG
		   if ((int)GetPriorityClass(GetCurrentProcess()) != IDLE_PRIORITY_CLASS)
		   {
				SetThreadPriority(GetCurrentThread(), m_nPriority);
		   }
		#endif
   }
     
   int m_nPriority;
};

class BoostProcess
{
public:
   BoostProcess()
   {
		#ifndef DEBUG
			m_nPriority = GetPriorityClass(GetCurrentProcess());
			SetPriorityClass(GetCurrentProcess(), REALTIME_PRIORITY_CLASS);
		#endif
   }

   ~BoostProcess()
   {
		#ifndef DEBUG
			SetPriorityClass(GetCurrentProcess(), m_nPriority);
		#endif
   }
     
   DWORD m_nPriority;
};

class HighProcess
{
public:
   HighProcess()
   {
		#ifndef DEBUG
			m_nPriority = GetPriorityClass(GetCurrentProcess());
			SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);
		#endif
   }

   ~HighProcess()
   {
		#ifndef DEBUG
			SetPriorityClass(GetCurrentProcess(), m_nPriority);
		#endif
   }
      
   DWORD m_nPriority;
};

class AbnormalProcess
{
public:
   AbnormalProcess()
   {
		#ifndef DEBUG
			m_nPriority = GetPriorityClass(GetCurrentProcess());
			SetPriorityClass(GetCurrentProcess(), ABOVE_NORMAL_PRIORITY_CLASS);
		#endif
   }

   ~AbnormalProcess()
   {
		#ifndef DEBUG
			SetThreadPriority(GetCurrentThread(), m_nPriority);
		#endif
   }
      
   DWORD m_nPriority;
};

class NormalProcess
{
public:
   NormalProcess()
   {
		#ifndef DEBUG
			m_nPriority = GetPriorityClass(GetCurrentProcess());
			SetPriorityClass(GetCurrentProcess(), NORMAL_PRIORITY_CLASS);
		#endif
   }

   ~NormalProcess()
   {
		#ifndef DEBUG
			SetPriorityClass(GetCurrentProcess(), m_nPriority);
		#endif
   }
      
   DWORD m_nPriority;
};

class BelowNormalProcess
{
public:
   BelowNormalProcess()
   {
		#ifndef DEBUG
			m_nPriority = GetPriorityClass(GetCurrentProcess());
			SetPriorityClass(GetCurrentProcess(), BELOW_NORMAL_PRIORITY_CLASS);
		#endif
   }

   ~BelowNormalProcess()
   {
		#ifndef DEBUG
			SetPriorityClass(GetCurrentProcess(), m_nPriority);
		#endif
   }
      
   DWORD m_nPriority;
};

class IdleProcess
{
public:
   IdleProcess()
   {
		#ifndef DEBUG
			m_nPriority = GetPriorityClass(GetCurrentProcess());
			SetPriorityClass(GetCurrentProcess(), IDLE_PRIORITY_CLASS);
		#endif
   }

   ~IdleProcess()
   {
		#ifndef DEBUG
			SetPriorityClass(GetCurrentProcess(), m_nPriority);
		#endif
   }
     
   DWORD m_nPriority;
};

class FirstAffinity
{
public:
   FirstAffinity()
   {
		#ifndef DEBUG
			m_nAffinity = 0;
			DWORD SystemAffinityMask = 0;
			if (GetProcessAffinityMask(GetCurrentProcess(), &m_nAffinity, &SystemAffinityMask)
				&& SystemAffinityMask > 1)
				SetThreadAffinityMask(GetCurrentThread(), 0x01&m_nAffinity);
		#endif
   }

   ~FirstAffinity()
   {
		#ifndef DEBUG
			if (m_nAffinity)
				SetThreadAffinityMask(GetCurrentThread(), m_nAffinity);
		#endif
   }
     
   DWORD m_nAffinity;
};

class SecondAffinity
{
public:
   SecondAffinity()
   {
		#ifndef DEBUG
			m_nAffinity = 0;
			DWORD SystemAffinityMask = 0;
			if (GetProcessAffinityMask(GetCurrentProcess(), &m_nAffinity, &SystemAffinityMask)
				&& SystemAffinityMask > 1)
				SetThreadAffinityMask(GetCurrentThread(), 0x10&m_nAffinity);
		#endif
   }

   ~SecondAffinity()
   {
		#ifndef DEBUG
			if (m_nAffinity)
				SetThreadAffinityMask(GetCurrentThread(), m_nAffinity);
		#endif
   }
     
   DWORD m_nAffinity;
};

#endif


