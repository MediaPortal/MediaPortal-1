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
 */

#include <atlbase.h>
#include "SyncClockInterface.h"

// Possible messages to the PowerStrip API
#define UM_SETCUSTOMTIMING (WM_USER+200) 
#define UM_SETREFRESHRATE (WM_USER+201) 
#define UM_SETPOLARITY (WM_USER+202) 
#define UM_REMOTECONTROL (WM_USER+210) 
#define UM_SETGAMMARAMP (WM_USER+203) 
#define UM_CREATERESOLUTION (WM_USER+204)
#define UM_GETTIMING (WM_USER+205)
#define UM_SETCUSTOMTIMINGFAST (WM_USER+211) // Without writing the res to file

#define PositiveHorizontalPolarity 0x00 
#define PositiveVerticalPolarity 0x00 
#define NegativeHorizontalPolarity 0x02 
#define NegativeVerticalPolarity 0x04 
#define HideTrayIcon 0x00 
#define ShowTrayIcon 0x01 
#define ClosePowerStrip 0x63

#define HACTIVE 0
#define HFRONTPORCH 1
#define HSYNCWIDTH 2
#define HBACKPORCH 3
#define VACTIVE 4
#define VFRONTPORCH 5
#define VSYNCWIDTH 6
#define VBACKPORCH 7
#define PIXELCLOCK 8
#define UNKNOWN 9

#define MAX_FIFO_SIZE 1024

class CGenlock
{
public:
	class MovingAverage
	{
	public:
		MovingAverage(INT size):
		fifoSize(size),
		oldestSample(0),
		sum(0)
		  {
			  if (fifoSize > MAX_FIFO_SIZE)
			  {
				  fifoSize = MAX_FIFO_SIZE;
			  }
			  for (INT i = 0; i < MAX_FIFO_SIZE; i++)
				  fifo[i] = 0;
		  }

		  ~MovingAverage()
		  {
		  }

		  double Average(double sample)
		  {
			  sum = sum + sample - fifo[oldestSample];
			  fifo[oldestSample] = sample;
			  oldestSample++;
			  if (oldestSample == fifoSize)
				  oldestSample = 0;
			  return sum / fifoSize;
		  }

	private:
		INT fifoSize;
		double fifo[MAX_FIFO_SIZE];
		INT oldestSample;
		double sum;
	};

	CGenlock(DOUBLE target, DOUBLE limit, INT rowD, INT colD, DOUBLE clockD, UINT mon);
	~CGenlock();

	BOOL PowerstripRunning(); // TRUE if PowerStrip is running
	HRESULT GetTiming(); // Get the string representing the display's current timing parameters
	HRESULT ResetTiming(); // Reset timing to what was last registered by GetTiming()
	HRESULT ResetClock(); // Reset reference clock speed to nominal
	HRESULT SetTargetSyncOffset(DOUBLE targetD);
	HRESULT GetTargetSyncOffset(DOUBLE *targetD);
	HRESULT SetControlLimit(DOUBLE cL);
	HRESULT GetControlLimit(DOUBLE *cL);
	HRESULT SetDisplayResolution(UINT columns, UINT lines);
	HRESULT AdviseSyncClock(CComPtr<ISyncClock> sC);
	HRESULT SetMonitor(UINT mon); // Set the number of the monitor to synchronize
	HRESULT ResetStats(); // Reset statistics

	HRESULT ControlDisplay(double syncOffset); // Adjust the frequency of the display if needed
	HRESULT ControlClock(double syncOffset); // Adjust the frequency of the clock if needed

	BOOL powerstripTimingExists; // TRUE if display timing has been got through Powerstrip
	BOOL liveSource; // TRUE if live source -> display sync is the only option
	INT adjDelta; // -1 for display slower in relation to video, 0 for keep, 1 for faster 
	INT lineDelta; // The number of rows added or subtracted when adjusting display fps
	INT columnDelta; // The number of colums added or subtracted when adjusting display fps
	DOUBLE cycleDelta; // Adjustment factor for cycle time as fraction of nominal value
	UINT displayAdjustmentsMade; // The number of adjustments made to display refresh rate
	UINT clockAdjustmentsMade; // The number of adjustments made to clock frequency

	UINT totalLines, totalColumns; // Including the porches and sync widths
	UINT visibleLines, visibleColumns; // The nominal resolution
	MovingAverage *syncOffsetFifo;
	DOUBLE syncOffset, minSyncOffset, maxSyncOffset; // 
	DOUBLE syncOffsetAvg, syncOffsetAvg2; // Average of the above

	UINT pixelClock; // In pixels/s
	DOUBLE displayFreqCruise;  // Nominal display frequency in frames/s
	DOUBLE displayFreqSlower;
	DOUBLE displayFreqFaster;
	DOUBLE curDisplayFreq; // Current (adjusted) display frequency
	DOUBLE controlLimit; // How much the sync offset is allowed to drift from target sync offset
	WPARAM monitor; // The monitor to be controlled. 0-based.
	CComPtr<ISyncClock> syncClock; // Interface to an adjustable reference clock

private:
	HWND psWnd; // PowerStrip window
	const static INT TIMING_PARAM_CNT = 10;
	const static INT MAX_LOADSTRING = 100;
	UINT displayTiming[TIMING_PARAM_CNT]; // Display timing parameters
	UINT displayTimingSave[TIMING_PARAM_CNT]; // So that we can reset the display at exit
	TCHAR faster[MAX_LOADSTRING]; // String corresponding to faster display frequency
	TCHAR cruise[MAX_LOADSTRING]; // String corresponding to nominal display frequency
	TCHAR slower[MAX_LOADSTRING]; // String corresponding to slower display frequency
	TCHAR savedTiming[MAX_LOADSTRING]; // String version of saved timing (to be restored upon exit)
	DOUBLE lowSyncOffset; // The closest we want to let the scheduled render time to get to the next vsync. In % of the frame time
	DOUBLE targetSyncOffset; // Where we want the scheduled render time to be in relation to the next vsync
	DOUBLE highSyncOffset; // The furthers we want to let the scheduled render time to get to the next vsync
};

