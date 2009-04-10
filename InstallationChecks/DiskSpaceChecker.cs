#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.IO;
using System.Windows.Forms;
namespace MediaPortal.DeployTool.InstallationChecks
{
  class DiskSpaceChecker
  {
    //
    // returns the remaining hard disk capacity of the given drive 
    //
    public static double GetRemainingHardDiskCapacity(string driveName)
    {
      long capacityInB;
      double capacityInGB;

      DriveInfo driveInfo = new DriveInfo(driveName);
      try
      {
        capacityInB = driveInfo.TotalFreeSpace;
      }
      catch (IOException)
      {
        capacityInB = 0;
      }
      capacityInGB = (double) capacityInB/(1024*1024*1024);
      capacityInGB = Math.Round(capacityInGB, 3);
      return capacityInGB;
    }
  }
}
