/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __RTSP_TASK_SCHEDULER_DEFINED
#define __RTSP_TASK_SCHEDULER_DEFINED

#include "BasicUsageEnvironment.hh"

class RtspTaskScheduler : public BasicTaskScheduler
{
public:
	static RtspTaskScheduler* createNew();
	virtual ~RtspTaskScheduler();

protected:
	RtspTaskScheduler();
	// called only by "createNew()"

protected:
	virtual void doEventLoop(char* watchVariable);
};

#endif
