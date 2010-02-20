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

#ifndef _MP_TASK_SCHEDULER_H
#define _MP_TASK_SCHEDULER_H

#include "BasicUsageEnvironment.hh"


class MPTaskScheduler: public BasicTaskScheduler {
public:
	static MPTaskScheduler* createNew();
	virtual ~MPTaskScheduler();

protected:
	MPTaskScheduler();
	// called only by "createNew()"

protected:
	virtual void doEventLoop(char* watchVariable);

};

#endif