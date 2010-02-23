// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#define _WIN32_WINNT 0x0601 

#include "Win7RefreshRateHelper.h"

// Get current refresh rate on Win7
double W7GetRefreshRate()
{
	UINT32 uNumPathArrayElements = 0;
  UINT32 uNumModeInfoArrayElements = 0;
  DISPLAYCONFIG_PATH_INFO* pPathInfoArray = NULL;
  DISPLAYCONFIG_MODE_INFO* pModeInfoArray = NULL;
  DISPLAYCONFIG_TOPOLOGY_ID* pCurrentTopologyId = NULL;
  LONG result;
  double refreshRate = -1;

  // Get size of buffers for QueryDisplayConfig
  result = GetDisplayConfigBufferSizes(QDC_ALL_PATHS, &uNumPathArrayElements, &uNumModeInfoArrayElements);
	if (result != 0)
	{
		return(refreshRate);
	}

  // allocate memory for QueryDisplayConfig buffers
  pPathInfoArray = (DISPLAYCONFIG_PATH_INFO*)calloc(uNumPathArrayElements, sizeof(DISPLAYCONFIG_PATH_INFO));
  if (pPathInfoArray == NULL )
	{
    return(refreshRate);
  }

  pModeInfoArray = (DISPLAYCONFIG_MODE_INFO*)calloc(uNumModeInfoArrayElements, sizeof(DISPLAYCONFIG_MODE_INFO));
  if (pPathInfoArray == NULL )
	{
    // freeing memory
    free(pPathInfoArray);
    free(pModeInfoArray);
    return(refreshRate);
  }

  // get display configuration
  result = QueryDisplayConfig(QDC_ALL_PATHS, 
                              &uNumPathArrayElements, pPathInfoArray, 
                              &uNumModeInfoArrayElements, pModeInfoArray,
                              pCurrentTopologyId);
	if (result == 0)
 	{
 		// Get information from first active target path
  	// TODO: add support for multiple displays (presenter doesn't know active display)
  	for(int i=0; i < (int)uNumPathArrayElements; i++)
  	{
      if (pPathInfoArray[i].flags == DISPLAYCONFIG_PATH_ACTIVE)
      {
        DISPLAYCONFIG_PATH_TARGET_INFO target;
        target = pPathInfoArray[i].targetInfo;
        LONG numerator = target.refreshRate.Numerator;
        LONG denominator = target.refreshRate.Denominator;
        refreshRate = (double)numerator/(double)denominator;
        break;
	    }
    }
  }
  // freeing memory
  free(pPathInfoArray);
  free(pModeInfoArray);
  
  return(refreshRate);
}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD nReason, LPVOID lpReserved)
{
	return TRUE;
}