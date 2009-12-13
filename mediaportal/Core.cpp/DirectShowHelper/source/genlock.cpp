/* 
 *      Copyright (C) 2005-2009 Team MediaPortal
 *      http://www.team-mediaportal.com
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
 *  Based on Arto Järvinen's work - http://www.ostrogothia.com/
 *
 */

#include "genlock.h"
#include <strsafe.h> // Required in CGenlock

CGenlock::CGenlock(DOUBLE target, DOUBLE limit, INT lineD, INT colD, DOUBLE clockD, UINT mon):
	targetSyncOffset(target), // Target sync offset, typically around 10 ms
	controlLimit(limit),      // How much sync offset is allowed to drift from target sync offset before control kicks in
	lineDelta(lineD),         // Number of rows used in display frequency adjustment, typically 1 (one)
	columnDelta(colD),        // Number of columns used in display frequency adjustment, typically 1 - 2
	cycleDelta(clockD),       // Delta used in clock speed adjustment. In fractions of 1.0. Typically around 0.001
	monitor(mon)              // The monitor to be adjusted if the display refresh rate is the controlled parameter
{
	lowSyncOffset = targetSyncOffset - controlLimit;
	highSyncOffset = targetSyncOffset + controlLimit;
	adjDelta = 0;
	displayAdjustmentsMade = 0;
	clockAdjustmentsMade = 0;
	displayFreqCruise = 0;
	displayFreqFaster = 0;
	displayFreqSlower = 0;
	curDisplayFreq = 0;
	psWnd = NULL;
	liveSource = FALSE;
	powerstripTimingExists = FALSE;
  syncOffsetFifo = new MovingAverage(64);
}


CGenlock::~CGenlock()
{
	ResetTiming();
	if (syncOffsetFifo != NULL)
	{
		delete syncOffsetFifo;
		syncOffsetFifo = NULL;
	}
	syncClock = NULL;
};


BOOL CGenlock::PowerstripRunning()
{
	psWnd = FindWindow(_T("TPShidden"), NULL); 
	if (!psWnd)
  {
    return FALSE; // Powerstrip is not running
  }
  else
  {
    return TRUE;
  }
}


// Get the display timing parameters through PowerStrip (if running).
HRESULT CGenlock::GetTiming()
{
	ATOM getTiming; 
	LPARAM lParam = NULL; 
	WPARAM wParam = monitor;
	INT i = 0;
	INT j = 0;
	INT params = 0;
	BOOL done = FALSE;
	TCHAR tmpStr[MAX_LOADSTRING];

	if (!PowerstripRunning())
  {
    return E_FAIL;
  }

	getTiming = static_cast<ATOM>(SendMessage(psWnd, UM_GETTIMING, wParam, lParam));
	GlobalGetAtomName(getTiming, savedTiming, MAX_LOADSTRING);

	while (params < TIMING_PARAM_CNT)
	{
		while (savedTiming[i] != ',' && savedTiming[i] != '\0')
		{
			tmpStr[j++] = savedTiming[i];
			tmpStr[j] = '\0';
			i++;
		}
		i++; // Skip trailing comma
		j = 0;
		displayTiming[params] = _ttoi(tmpStr);
		displayTimingSave[params] = displayTiming[params];
		params++;
	}

	// The display update frequency is controlled by adding and subtracting pixels form the
	// image. This is done by either subtracting columns or rows or both. Some displays like
	// row adjustments and some column adjustments. One should probably not do both.
	StringCchPrintf(faster, MAX_LOADSTRING, TEXT("%d,%d,%d,%d,%d,%d,%d,%d,%d,%d\0"),
		displayTiming[0],
		displayTiming[HFRONTPORCH] - columnDelta,
		displayTiming[2],
		displayTiming[3],
		displayTiming[4],
		displayTiming[VFRONTPORCH] - lineDelta,
		displayTiming[6],
		displayTiming[7],
		displayTiming[8],
		displayTiming[9]	
		);

	// Nominal update frequency
	StringCchPrintf(cruise, MAX_LOADSTRING, TEXT("%d,%d,%d,%d,%d,%d,%d,%d,%d,%d\0"),
		displayTiming[0],
		displayTiming[HFRONTPORCH],
		displayTiming[2],
		displayTiming[3],
		displayTiming[4],
		displayTiming[VFRONTPORCH],
		displayTiming[6],
		displayTiming[7],
		displayTiming[8],
		displayTiming[9]	
		);

	// Lower than nominal update frequency
	StringCchPrintf(slower, MAX_LOADSTRING, TEXT("%d,%d,%d,%d,%d,%d,%d,%d,%d,%d\0"),
		displayTiming[0],
		displayTiming[HFRONTPORCH] + columnDelta,
		displayTiming[2],
		displayTiming[3],
		displayTiming[4],
		displayTiming[VFRONTPORCH] + lineDelta,
		displayTiming[6],
		displayTiming[7],
		displayTiming[8],
		displayTiming[9]	
		);

	totalColumns = displayTiming[HACTIVE] + displayTiming[HFRONTPORCH] + displayTiming[HSYNCWIDTH] + displayTiming[HBACKPORCH];
	totalLines = displayTiming[VACTIVE] + displayTiming[VFRONTPORCH] + displayTiming[VSYNCWIDTH] + displayTiming[VBACKPORCH];
	pixelClock = 1000 * displayTiming[PIXELCLOCK]; // Pixels/s
	displayFreqCruise = (DOUBLE)pixelClock / (totalLines * totalColumns); // Frames/s
	displayFreqSlower = (DOUBLE)pixelClock / ((totalLines + lineDelta) * (totalColumns + columnDelta));
	displayFreqFaster = (DOUBLE)pixelClock / ((totalLines - lineDelta) * (totalColumns - columnDelta));
	curDisplayFreq = displayFreqCruise;
	GlobalDeleteAtom(getTiming);
	adjDelta = 0;
	powerstripTimingExists = TRUE;
	return S_OK;
}


// Reset display timing parameters to nominal.
HRESULT CGenlock::ResetTiming()
{
	LPARAM lParam = NULL; 
	WPARAM wParam = monitor; 
	ATOM setTiming; 
	LRESULT ret;

	if (!PowerstripRunning())
  {
    return E_FAIL;
  }

	if (displayAdjustmentsMade > 0)
	{
		setTiming = GlobalAddAtom(cruise); 
		lParam = setTiming;
		ret = SendMessage(psWnd, UM_SETCUSTOMTIMINGFAST, wParam, lParam);
		GlobalDeleteAtom(setTiming);
		curDisplayFreq = displayFreqCruise;
	}
	adjDelta = 0;
	return S_OK;
}


// Reset reference clock speed to nominal.
HRESULT CGenlock::ResetClock()
{
	adjDelta = 0;
	if (syncClock == NULL)
  {
    return E_FAIL;
  }
  else
  {
    return syncClock->AdjustClock(1.0);
  }
  return S_OK;
}


HRESULT CGenlock::SetTargetSyncOffset(DOUBLE targetD)
{
	targetSyncOffset = targetD;
	lowSyncOffset = targetD - controlLimit;
	highSyncOffset = targetD + controlLimit;
	return S_OK;
}


HRESULT CGenlock::GetTargetSyncOffset(DOUBLE *targetD)
{
	*targetD = targetSyncOffset;
	return S_OK;
}


HRESULT CGenlock::SetControlLimit(DOUBLE cL)
{
	controlLimit = cL;
	return S_OK;
}


HRESULT CGenlock::GetControlLimit(DOUBLE *cL)
{
	*cL = controlLimit;
	return S_OK;
}


HRESULT CGenlock::SetDisplayResolution(UINT columns, UINT lines)
{
	visibleColumns = columns;
	visibleLines = lines;
	return S_OK;
}


HRESULT CGenlock::AdviseSyncClock(CComPtr<ISyncClock> sC)
{
	if (!sC)
  {
    return E_FAIL;
  }
  if (syncClock)
  {
    syncClock = NULL; // Release any outstanding references if this is called repeatedly
  }
  syncClock = sC;
	return S_OK;
}


// Set the monitor to control. This is best done manually as not all monitors can be controlled
// so automatic detection of monitor to control might have unintended effects.
// The PowerStrip API uses zero-based monitor numbers, i.e. the default monitor is 0.
HRESULT CGenlock::SetMonitor(UINT mon)
{
	monitor = mon;
	return S_OK;
}


HRESULT CGenlock::ResetStats()
{
	minSyncOffset = 1000000.0;
	maxSyncOffset = -1000000.0;
	displayAdjustmentsMade = 0;
	clockAdjustmentsMade = 0;

	return S_OK;
}


// Synchronize by adjusting display refresh rate
HRESULT CGenlock::ControlDisplay(double syncOffset)
{
	LPARAM lParam = NULL; 
	WPARAM wParam = monitor; 
	ATOM setTiming;

	syncOffsetAvg = syncOffsetFifo->Average(syncOffset);
	minSyncOffset = min(minSyncOffset, syncOffset);
	maxSyncOffset = max(maxSyncOffset, syncOffset);

	if (!PowerstripRunning() || !powerstripTimingExists)
  {
    return E_FAIL;
  }
  // Adjust as seldom as possible by checking the current controlState before changing it.
	if ((syncOffsetAvg > highSyncOffset) && (adjDelta != 1))
  // Speed up display refresh rate by subtracting pixels from the image.
	{
		adjDelta = 1; // Increase refresh rate
		curDisplayFreq = displayFreqFaster;
		setTiming = GlobalAddAtom(faster);
		lParam = setTiming;
		SendMessage(psWnd, UM_SETCUSTOMTIMINGFAST, wParam, lParam);
		GlobalDeleteAtom(setTiming);
		displayAdjustmentsMade++;
	}
	// Slow down display refresh rate by adding pixels to the image.
  else if ((syncOffsetAvg < lowSyncOffset) && (adjDelta != -1))
	{
		adjDelta = -1;
		curDisplayFreq = displayFreqSlower;
		setTiming = GlobalAddAtom(slower);
		lParam = setTiming;
		SendMessage(psWnd, UM_SETCUSTOMTIMINGFAST, wParam, lParam);
		GlobalDeleteAtom(setTiming);
		displayAdjustmentsMade++;
	}
	// Cruise.
  else if ((syncOffsetAvg < targetSyncOffset) && (adjDelta == 1))
	{
		adjDelta = 0;
		curDisplayFreq = displayFreqCruise;
		setTiming = GlobalAddAtom(cruise);
		lParam = setTiming;
		SendMessage(psWnd, UM_SETCUSTOMTIMINGFAST, wParam, lParam);
		GlobalDeleteAtom(setTiming);
		displayAdjustmentsMade++;
	}
	else if ((syncOffsetAvg > targetSyncOffset) && (adjDelta == -1))
	{
		adjDelta = 0;
		curDisplayFreq = displayFreqCruise;
		setTiming = GlobalAddAtom(cruise);
		lParam = setTiming;
		SendMessage(psWnd, UM_SETCUSTOMTIMINGFAST, wParam, lParam);
		GlobalDeleteAtom(setTiming);
		displayAdjustmentsMade++;
	}
  return S_OK;
}


// Synchronize by adjusting reference clock rate (and therefore video FPS).
// Todo: check so that we don't have a live source
HRESULT CGenlock::ControlClock(double syncOffset)
{
	syncOffsetAvg = syncOffsetFifo->Average(syncOffset);
	minSyncOffset = min(minSyncOffset, syncOffset);
	maxSyncOffset = max(maxSyncOffset, syncOffset);

	if (!syncClock)
  {
    return E_FAIL;
  }
  // Adjust as seldom as possible by checking the current controlState before changing it.
	if ((syncOffsetAvg > highSyncOffset) && (adjDelta != 1))
	// Slow down video stream.
	{
		adjDelta = 1;
		syncClock->AdjustClock(1.0 - cycleDelta); // Makes the clock move slower by providing smaller increments
		clockAdjustmentsMade++;
	}
	// Speed up video stream.
  else if ((syncOffsetAvg < lowSyncOffset) && (adjDelta != -1))
	{
		adjDelta = -1;
		syncClock->AdjustClock(1.0 + cycleDelta);
		clockAdjustmentsMade++;
	}
	// Cruise.
  else if ((syncOffsetAvg < targetSyncOffset) && (adjDelta == 1))
	{
		adjDelta = 0;
		syncClock->AdjustClock(1.0);
		clockAdjustmentsMade++;
	}
	else if ((syncOffsetAvg > targetSyncOffset) && (adjDelta == -1))
	{
		adjDelta = 0;
		syncClock->AdjustClock(1.0);
		clockAdjustmentsMade++;
	}
	return S_OK;
}

